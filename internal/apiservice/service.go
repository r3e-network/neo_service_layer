package api

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"net/http"
	"sync"
	"time"

	"github.com/go-chi/chi/v5"
	chimiddleware "github.com/go-chi/chi/v5/middleware"
	"github.com/go-chi/cors"
	"github.com/go-chi/jwtauth/v5"
	"github.com/r3e-network/neo_service_layer/internal/apiservice/middleware"
	gasbank "github.com/r3e-network/neo_service_layer/internal/gasbankservice"
	pricefeed "github.com/r3e-network/neo_service_layer/internal/pricefeedservice"
	trigger "github.com/r3e-network/neo_service_layer/internal/triggerservice"
	"github.com/sirupsen/logrus"
)

// Service implements the API service
type Service struct {
	config       *Config
	router       *chi.Mux
	server       *http.Server
	tokenAuth    *jwtauth.JWTAuth
	functionsSvc functions.IService
	secretsSvc   secrets.IService
	gasBankSvc   gasbank.IService
	priceFeedSvc pricefeed.IService
	triggerSvc   trigger.IService
	stats        StatsResponse
	statsMu      sync.RWMutex
	healthChecks map[string]func() HealthStatus
	healthMu     sync.RWMutex
	log          *logrus.Logger
}

// Dependencies defines the services required by the API service
type Dependencies struct {
	functionservice  functions.IService
	secretservice    secrets.IService
	GasBankService   gasbank.IService
	PriceFeedService pricefeed.IService
	TriggerService   trigger.IService
	Logger           *logrus.Logger
}

// NewService creates a new API service
func NewService(config *Config, deps *Dependencies) (*Service, error) {
	if config == nil {
		return nil, errors.New("config cannot be nil")
	}
	if deps == nil {
		return nil, errors.New("dependencies cannot be nil")
	}

	// Validate config
	if config.Port <= 0 {
		config.Port = 3000 // Default port
	}
	if config.Host == "" {
		config.Host = "0.0.0.0" // Default host
	}
	if config.ReadTimeout <= 0 {
		config.ReadTimeout = 30 * time.Second // Default read timeout
	}
	if config.WriteTimeout <= 0 {
		config.WriteTimeout = 30 * time.Second // Default write timeout
	}
	if config.IdleTimeout <= 0 {
		config.IdleTimeout = 60 * time.Second // Default idle timeout
	}
	if config.MaxRequestBodySize <= 0 {
		config.MaxRequestBodySize = 1024 * 1024 // 1MB default
	}
	if config.JWTSecret == "" {
		config.JWTSecret = "neo-service-layer-jwt-secret" // Default JWT secret
	}
	if config.JWTExpiryDuration <= 0 {
		config.JWTExpiryDuration = 24 * time.Hour // 24 hours default
	}

	// Validate dependencies
	if deps.functionservice == nil {
		return nil, errors.New("functions service cannot be nil")
	}
	if deps.secretservice == nil {
		return nil, errors.New("secrets service cannot be nil")
	}
	if deps.GasBankService == nil {
		return nil, errors.New("gas bank service cannot be nil")
	}
	if deps.PriceFeedService == nil {
		return nil, errors.New("price feed service cannot be nil")
	}
	if deps.TriggerService == nil {
		return nil, errors.New("trigger service cannot be nil")
	}
	if deps.Logger == nil {
		deps.Logger = logrus.New() // Default logger
	}

	// Create JWT auth
	tokenAuth := jwtauth.New("HS256", []byte(config.JWTSecret), nil)

	// Create service instance
	svc := &Service{
		config:       config,
		tokenAuth:    tokenAuth,
		functionsSvc: deps.functionservice,
		secretsSvc:   deps.secretservice,
		gasBankSvc:   deps.GasBankService,
		priceFeedSvc: deps.PriceFeedService,
		triggerSvc:   deps.TriggerService,
		stats: StatsResponse{
			LastUpdated: time.Now(),
		},
		healthChecks: make(map[string]func() HealthStatus),
		log:          deps.Logger,
	}

	// Initialize router
	svc.initRouter()

	// Initialize HTTP server
	svc.server = &http.Server{
		Addr:         fmt.Sprintf("%s:%d", config.Host, config.Port),
		Handler:      svc.router,
		ReadTimeout:  config.ReadTimeout,
		WriteTimeout: config.WriteTimeout,
		IdleTimeout:  config.IdleTimeout,
	}

	// Initialize health checks
	svc.initHealthChecks()

	return svc, nil
}

// Start starts the API service
func (s *Service) Start() error {
	s.log.Infof("Starting API service on %s", s.server.Addr)
	if s.config.EnableHTTPS && s.config.CertFile != "" && s.config.KeyFile != "" {
		return s.server.ListenAndServeTLS(s.config.CertFile, s.config.KeyFile)
	}
	return s.server.ListenAndServe()
}

// Stop stops the API service
func (s *Service) Stop(ctx context.Context) error {
	s.log.Info("Stopping API service")
	return s.server.Shutdown(ctx)
}

// initRouter initializes the API router
func (s *Service) initRouter() {
	r := chi.NewRouter()

	// Middleware
	r.Use(chimiddleware.RequestID)
	r.Use(chimiddleware.RealIP)
	if s.config.EnableRequestLogging {
		r.Use(chimiddleware.Logger)
	}
	r.Use(chimiddleware.Recoverer)
	r.Use(chimiddleware.Timeout(60 * time.Second))
	r.Use(chimiddleware.SetHeader("Content-Type", "application/json"))

	// CORS
	if s.config.EnableCORS {
		corsOptions := cors.Options{
			AllowedOrigins:   s.config.AllowedOrigins,
			AllowedMethods:   []string{"GET", "POST", "PUT", "DELETE", "OPTIONS"},
			AllowedHeaders:   []string{"Accept", "Authorization", "Content-Type", "X-CSRF-Token"},
			ExposedHeaders:   []string{"Link"},
			AllowCredentials: true,
			MaxAge:           300,
		}
		r.Use(cors.Handler(corsOptions))
	}

	// Rate limiting
	if s.config.EnableRateLimiting && s.config.RateLimitPerMinute > 0 {
		r.Use(middleware.RateLimiter(s.config.RateLimitPerMinute))
	}

	// Routes
	r.Get("/health", s.handleHealthCheck)
	r.Get("/stats", s.handleGetStats)

	// API routes
	r.Route("/api/v1", func(r chi.Router) {
		// Public routes
		r.Group(func(r chi.Router) {
			r.Post("/auth/verify", s.handleVerifySignature)
		})

		// Protected routes
		r.Group(func(r chi.Router) {
			r.Use(jwtauth.Verifier(s.tokenAuth))
			r.Use(middleware.JWTAuthenticator(s.tokenAuth))
			r.Use(middleware.AddressFromToken)

			// Functions
			r.Route("/functions", func(r chi.Router) {
				r.Post("/", s.handleCreateFunction)
				r.Get("/", s.handleListFunctions)
				r.Get("/{id}", s.handleGetFunction)
				r.Put("/{id}", s.handleUpdateFunction)
				r.Delete("/{id}", s.handleDeleteFunction)
				r.Post("/{id}/invoke", s.handleInvokeFunction)
				r.Get("/{id}/executions", s.handleListFunctionExecutions)
				r.Get("/{id}/permissions", s.handleGetFunctionPermissions)
				r.Put("/{id}/permissions", s.handleUpdateFunctionPermissions)
			})

			// Secrets
			r.Route("/secrets", func(r chi.Router) {
				r.Post("/", s.handleStoreSecret)
				r.Get("/", s.handleListSecrets)
				r.Get("/{key}", s.handleGetSecret)
				r.Delete("/{key}", s.handleDeleteSecret)
			})

			// GasBank
			r.Route("/gas", func(r chi.Router) {
				r.Post("/allocate", s.handleAllocateGas)
				r.Post("/release", s.handleReleaseGas)
				r.Get("/balance", s.handleGetGasBalance)
			})

			// Price Feed
			r.Route("/prices", func(r chi.Router) {
				r.Get("/", s.handleListPrices)
				r.Get("/{symbol}", s.handleGetPrice)
			})

			// Triggers
			r.Route("/triggers", func(r chi.Router) {
				r.Post("/", s.handleCreateTrigger)
				r.Get("/", s.handleListTriggers)
				r.Get("/{id}", s.handleGetTrigger)
				r.Put("/{id}", s.handleUpdateTrigger)
				r.Delete("/{id}", s.handleDeleteTrigger)
				r.Post("/{id}/execute", s.handleExecuteTrigger)
				r.Get("/{id}/executions", s.handleGetTriggerExecutions)
				r.Get("/{id}/metrics", s.handleGetTriggerMetrics)
			})

			// User Profile
			r.Get("/profile", s.handleGetUserProfile)
		})
	})

	// Set router
	s.router = r
}

// initHealthChecks initializes health check functions
func (s *Service) initHealthChecks() {
	s.healthChecks["api"] = func() HealthStatus {
		return HealthStatus{
			Service:     "api",
			Status:      "healthy",
			Version:     "1.0.0",
			Message:     "API service is running",
			LastChecked: time.Now(),
		}
	}

	s.healthChecks["functions"] = func() HealthStatus {
		return HealthStatus{
			Service:     "functions",
			Status:      "healthy",
			Version:     "1.0.0",
			Message:     "Functions service is running",
			LastChecked: time.Now(),
		}
	}

	s.healthChecks["secrets"] = func() HealthStatus {
		return HealthStatus{
			Service:     "secrets",
			Status:      "healthy",
			Version:     "1.0.0",
			Message:     "Secrets service is running",
			LastChecked: time.Now(),
		}
	}

	s.healthChecks["gasbank"] = func() HealthStatus {
		return HealthStatus{
			Service:     "gasbank",
			Status:      "healthy",
			Version:     "1.0.0",
			Message:     "Gas bank service is running",
			LastChecked: time.Now(),
		}
	}

	s.healthChecks["pricefeed"] = func() HealthStatus {
		return HealthStatus{
			Service:     "pricefeed",
			Status:      "healthy",
			Version:     "1.0.0",
			Message:     "Price feed service is running",
			LastChecked: time.Now(),
		}
	}

	s.healthChecks["trigger"] = func() HealthStatus {
		return HealthStatus{
			Service:     "trigger",
			Status:      "healthy",
			Version:     "1.0.0",
			Message:     "Trigger service is running",
			LastChecked: time.Now(),
		}
	}
}

// handleGetTriggerExecutions handles GET /api/v1/triggers/{id}/executions
func (s *Service) handleGetTriggerExecutions(w http.ResponseWriter, r *http.Request) {
	triggerID := chi.URLParam(r, "id")
	if triggerID == "" {
		http.Error(w, "trigger ID is required", http.StatusBadRequest)
		return
	}

	executions, err := s.triggerSvc.GetTriggerExecutions(r.Context(), triggerID)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	json.NewEncoder(w).Encode(executions)
}

// handleGetTriggerMetrics handles GET /api/v1/triggers/{id}/metrics
func (s *Service) handleGetTriggerMetrics(w http.ResponseWriter, r *http.Request) {
	triggerID := chi.URLParam(r, "id")
	if triggerID == "" {
		http.Error(w, "trigger ID is required", http.StatusBadRequest)
		return
	}

	metrics, err := s.triggerSvc.GetTriggerMetrics(r.Context(), triggerID)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	json.NewEncoder(w).Encode(metrics)
}
