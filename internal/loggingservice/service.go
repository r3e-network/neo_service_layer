package logging

import (
	"context"
	"fmt"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/loggingservice/models"
	log "github.com/sirupsen/logrus"
)

// Service implements the Logging service
type Service struct {
	config  *models.Config
	storage LogStorage
	logger  *log.Logger
}

// NewService creates a new Logging service
func NewService(config *models.Config) (*Service, error) {
	logger := log.New()
	logLevel, err := log.ParseLevel(config.LogLevel)
	if err != nil {
		logger.Warnf("Invalid log level '%s' in config, using default 'info'", config.LogLevel)
		logLevel = log.InfoLevel
	}
	logger.SetLevel(logLevel)

	if config.EnableJSONLogs {
		logger.SetFormatter(&log.JSONFormatter{})
	} else {
		logger.SetFormatter(&log.TextFormatter{FullTimestamp: true})
	}

	// --- Initialize Storage ---
	var storage LogStorage
	var storageErr error

	if config.LogFilePath != "" {
		logger.Infof("Initializing FileStorage at %s", config.LogFilePath)
		storage, storageErr = NewFileStorage(config)
		if storageErr != nil {
			logger.Errorf("Failed to initialize FileStorage: %v. Falling back to InMemoryStorage.", storageErr)
			maxEntries := 10000
			storage = NewInMemoryStorage(maxEntries)
			logger.Infof("Initialized logging service with InMemoryStorage (maxEntries: %d) as fallback.", maxEntries)
			storageErr = nil
		} else {
			logger.Info("Initialized logging service with FileStorage.")
		}
	} else {
		maxEntries := 10000
		storage = NewInMemoryStorage(maxEntries)
		logger.Infof("Initialized logging service with InMemoryStorage (maxEntries: %d)", maxEntries)
	}

	if storageErr != nil {
		return nil, fmt.Errorf("failed to initialize log storage: %w", storageErr)
	}

	return &Service{
		config:  config,
		storage: storage,
		logger:  logger,
	}, nil
}

// LogDebug logs a debug message
func (s *Service) LogDebug(message string, context map[string]interface{}) error {
	return s.logInternal(models.LogLevelDebug, message, context)
}

// LogInfo logs an info message
func (s *Service) LogInfo(message string, context map[string]interface{}) error {
	return s.logInternal(models.LogLevelInfo, message, context)
}

// LogWarning logs a warning message
func (s *Service) LogWarning(message string, context map[string]interface{}) error {
	return s.logInternal(models.LogLevelWarn, message, context)
}

// LogError logs an error message
func (s *Service) LogError(message string, context map[string]interface{}) error {
	return s.logInternal(models.LogLevelError, message, context)
}

// QueryLogs retrieves logs matching a query
func (s *Service) QueryLogs(ctx context.Context, query models.LogQuery) ([]models.LogEntry, error) {
	results, err := s.storage.Query(query)
	if err != nil {
		s.logger.Errorf("Error querying logs: %v", err)
		return nil, fmt.Errorf("failed to query logs")
	}
	return results, nil
}

// Start starts the Logging service
func (s *Service) Start() error {
	s.logger.Info("Starting Logging service...")
	s.logger.Info("Logging service started.")
	return nil
}

// Shutdown stops the Logging service
func (s *Service) Shutdown(ctx context.Context) error {
	s.logger.Info("Shutting down Logging service...")
	if err := s.storage.Close(); err != nil {
		s.logger.Errorf("Error closing log storage: %v", err)
	}
	s.logger.Info("Logging service shut down.")
	return nil
}

// logInternal creates and stores a log entry if level is sufficient
func (s *Service) logInternal(level models.LogLevel, message string, context map[string]interface{}) error {
	configLevel := models.ParseLogLevel(s.config.LogLevel)
	if level < configLevel {
		return nil
	}

	entry := models.LogEntry{
		ID:        fmt.Sprintf("log-%d", time.Now().UnixNano()),
		Timestamp: time.Now(),
		Level:     level.String(),
		Message:   message,
		Service:   getServiceFromContext(context),
		Context:   context,
	}

	s.logViaLogger(entry)

	if err := s.storage.Store(entry); err != nil {
		s.logger.Errorf("Failed to store log entry: %v", err)
		return fmt.Errorf("failed to store log entry: %w", err)
	}

	return nil
}

// logViaLogger logs the entry using the configured service logger (logrus)
func (s *Service) logViaLogger(entry models.LogEntry) {
	fields := log.Fields{}
	for k, v := range entry.Context {
		fields[k] = v
	}
	fields["service"] = entry.Service

	logEntry := s.logger.WithFields(fields)

	switch models.ParseLogLevel(entry.Level) {
	case models.LogLevelDebug:
		logEntry.Debug(entry.Message)
	case models.LogLevelInfo:
		logEntry.Info(entry.Message)
	case models.LogLevelWarn:
		logEntry.Warn(entry.Message)
	case models.LogLevelError:
		logEntry.Error(entry.Message)
	default:
		logEntry.Info(entry.Message)
	}
}

// getServiceFromContext extracts the service name from context
func getServiceFromContext(context map[string]interface{}) string {
	if context == nil {
		return "unknown"
	}

	service, ok := context["service"]
	if !ok {
		return "unknown"
	}

	serviceStr, ok := service.(string)
	if !ok {
		return "unknown"
	}

	return serviceStr
}

// --- LogLevel types/functions REMOVED (Assume they exist in models.go) ---
/*
type LogLevel int
const (
	LogLevelDebug LogLevel = iota
	LogLevelInfo
	LogLevelWarn
	LogLevelError
)
func (l LogLevel) String() string { ... }
func ParseLogLevel(levelStr string) LogLevel { ... }
*/
