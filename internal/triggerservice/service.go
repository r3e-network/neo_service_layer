package trigger

import (
	"context"
	"fmt"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/core/neo"
	functions_iface "github.com/r3e-network/neo_service_layer/internal/functionservice"
	"github.com/r3e-network/neo_service_layer/internal/triggerservice/internal"
	"github.com/r3e-network/neo_service_layer/internal/triggerservice/models"
	log "github.com/sirupsen/logrus"
)

// ServiceConfig represents the trigger service configuration
type ServiceConfig struct {
	MaxTriggers           int           // Maximum triggers per user
	MaxExecutions         int           // Maximum executions per trigger
	ExecutionWindow       time.Duration // Window to track executions
	MaxConcurrentTriggers int           // Max concurrent triggers running
}

// Service represents the trigger service
type Service struct {
	manager   internal.TriggerManager
	store     internal.TriggerStore
	metrics   internal.TriggerMetricsCollector
	alerts    internal.TriggerAlertManager
	scheduler internal.TriggerScheduler
	neoClient neo.NeoClient
	stopChan  chan struct{}
	wg        sync.WaitGroup
	config    *ServiceConfig
}

// NewService creates a new trigger service
func NewService(config *ServiceConfig, neoClient neo.NeoClient, funcService functions_iface.IService, walletService internal.WalletService) (*Service, error) {
	if config == nil {
		config = &ServiceConfig{
			MaxTriggers:           100,
			MaxExecutions:         1000,
			ExecutionWindow:       time.Hour * 24,
			MaxConcurrentTriggers: 10,
		}
	}

	// Create trigger policy from config
	policy := &models.TriggerPolicy{
		MaxTriggersPerUser:      config.MaxTriggers,
		MaxExecutionsPerTrigger: config.MaxExecutions,
		ExecutionWindow:         config.ExecutionWindow.Milliseconds(),
		MaxConcurrentExecutions: config.MaxConcurrentTriggers,
		MinInterval:             time.Minute.Milliseconds(),
		MaxInterval:             (time.Hour * 24).Milliseconds(),
		CooldownPeriod:          (time.Minute * 5).Milliseconds(),
	}

	// Initialize sub-components
	store := internal.NewTriggerStore()
	metrics := internal.NewTriggerMetricsCollector()
	alerts := internal.NewTriggerAlertManager()
	scheduler := internal.NewTriggerScheduler()

	// Create the trigger manager
	manager := internal.NewTriggerManager(
		store,
		metrics,
		alerts,
		scheduler,
		policy,
		neoClient,
		funcService,
		walletService,
	)

	return &Service{
		manager:   manager,
		store:     store,
		metrics:   metrics,
		alerts:    alerts,
		scheduler: scheduler,
		neoClient: neoClient,
		stopChan:  make(chan struct{}),
		config:    config,
	}, nil
}

// CreateTrigger creates a new trigger
func (s *Service) CreateTrigger(ctx context.Context, userAddress util.Uint160, trigger *models.Trigger) (*models.Trigger, error) {
	return s.manager.CreateTrigger(ctx, userAddress, trigger)
}

// UpdateTrigger updates an existing trigger
func (s *Service) UpdateTrigger(ctx context.Context, userAddress util.Uint160, triggerID string, trigger *models.Trigger) (*models.Trigger, error) {
	return s.manager.UpdateTrigger(ctx, userAddress, triggerID, trigger)
}

// DeleteTrigger deletes a trigger
func (s *Service) DeleteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) error {
	return s.manager.DeleteTrigger(ctx, userAddress, triggerID)
}

// GetTrigger gets a trigger by ID
func (s *Service) GetTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*models.Trigger, error) {
	return s.manager.GetTrigger(ctx, userAddress, triggerID)
}

// ListTriggers lists all triggers for a user
func (s *Service) ListTriggers(ctx context.Context, userAddress util.Uint160) ([]*models.Trigger, error) {
	return s.manager.ListTriggers(ctx, userAddress)
}

// ExecuteTrigger executes a trigger
func (s *Service) ExecuteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*models.Execution, error) {
	// Get the trigger
	trigger, err := s.manager.GetTrigger(ctx, userAddress, triggerID)
	if err != nil {
		return nil, fmt.Errorf("failed to get trigger: %w", err)
	}

	if trigger == nil {
		return nil, fmt.Errorf("trigger not found")
	}

	// Create execution record
	execution := &models.Execution{
		ID:        uuid.New().String(),
		TriggerID: triggerID,
		Status:    "pending",
		StartTime: time.Now(),
	}

	// Execute the trigger
	result, err := s.manager.ExecuteTrigger(ctx, userAddress, triggerID)
	if err != nil {
		execution.Status = "failed"
		execution.Error = err.Error()
		execution.EndTime = time.Now()
		return execution, fmt.Errorf("failed to execute trigger: %w", err)
	}

	// Update execution record
	execution.Status = result.Status
	execution.Result = result.Result
	execution.EndTime = time.Now()

	return execution, nil
}

// GetTriggerExecutions gets the execution history for a trigger
func (s *Service) GetTriggerExecutions(ctx context.Context, triggerID string) ([]*models.TriggerExecution, error) {
	// Get the trigger first to verify it exists
	trigger, err := s.manager.GetTrigger(ctx, util.Uint160{}, triggerID)
	if err != nil {
		return nil, fmt.Errorf("failed to get trigger: %w", err)
	}

	if trigger == nil {
		return nil, fmt.Errorf("trigger not found")
	}

	// Get executions from manager
	return s.manager.GetTriggerExecutions(ctx, trigger.UserAddress, triggerID)
}

// GetTriggerMetrics gets metrics for a trigger
func (s *Service) GetTriggerMetrics(ctx context.Context, triggerID string) (*models.TriggerMetrics, error) {
	// Get the trigger first to verify it exists
	trigger, err := s.manager.GetTrigger(ctx, util.Uint160{}, triggerID)
	if err != nil {
		return nil, fmt.Errorf("failed to get trigger: %w", err)
	}

	if trigger == nil {
		return nil, fmt.Errorf("trigger not found")
	}

	// Get metrics from metrics collector
	return s.metrics.GetMetrics(ctx), nil
}

// GetTriggerPolicy gets the current trigger policy
func (s *Service) GetTriggerPolicy(ctx context.Context) (*models.TriggerPolicy, error) {
	// Return the policy from the manager
	return s.manager.GetPolicy(), nil
}

// UpdateTriggerPolicy updates the trigger policy
func (s *Service) UpdateTriggerPolicy(ctx context.Context, policy *models.TriggerPolicy) error {
	// Update the policy in the manager
	return s.manager.UpdatePolicy(policy)
}

// Start starts the trigger service components, including the scheduler check loop
func (s *Service) Start(ctx context.Context) error {
	if err := s.scheduler.Start(ctx); err != nil {
		return fmt.Errorf("failed to start scheduler: %w", err)
	}

	// Start the main check loop
	s.wg.Add(1)
	go s.runSchedulerCheckLoop()

	log.Info("Trigger service started")
	return nil
}

// Stop stops the trigger service components
func (s *Service) Stop(ctx context.Context) error {
	log.Info("Stopping trigger service...")
	close(s.stopChan) // Signal goroutines to stop

	if err := s.scheduler.Stop(ctx); err != nil {
		log.Errorf("Failed to stop scheduler cleanly: %v", err)
		// Continue stopping other components
	}

	s.wg.Wait() // Wait for goroutines to finish
	log.Info("Trigger service stopped")
	return nil
}

// runSchedulerCheckLoop periodically checks for due triggers
func (s *Service) runSchedulerCheckLoop() {
	defer s.wg.Done()
	// Use a check interval from config or default
	checkInterval := s.config.ExecutionWindow / 10 // Example: check 10 times per window
	if checkInterval < 5*time.Second {             // Ensure a minimum check interval
		checkInterval = 5 * time.Second
	}
	ticker := time.NewTicker(checkInterval)
	defer ticker.Stop()

	log.Infof("Starting trigger scheduler check loop with interval: %v", checkInterval)

	for {
		select {
		case <-ticker.C:
			// TODO: Get list of all active trigger IDs from the store
			// For each trigger ID:
			//   isDue, err := s.scheduler.IsDue(triggerID)
			//   if err { log error }
			//   if isDue {
			//     log.Infof("Trigger %s is due for execution", triggerID)
			//     // Spawn goroutine to execute? Avoid blocking loop.
			//     go func(tID string) {
			//        execCtx, cancel := context.WithTimeout(context.Background(), s.config.ExecutionWindow) // Timeout per execution
			//        defer cancel()
			//        // Need userAddress here? GetTrigger requires it.
			//        // Maybe ListTriggers first, then check IsDue?
			//        _, err := s.ExecuteTrigger(execCtx, ???, tID)
			//        if err != nil { log error }
			//     }(triggerID)
			//   }
			log.Debug("Trigger check loop iteration complete.") // Placeholder log
		case <-s.stopChan:
			log.Info("Stopping trigger scheduler check loop.")
			return
		}
	}
}

// RegisterHandler registers a trigger handler
func (s *Service) RegisterHandler(name string, handler TriggerHandler) error {
	// In a real implementation, this would register the handler with the manager
	// For now, we'll just log that the handler was registered
	return nil
}

// NotifyTriggerResult notifies the service about a trigger result
func (s *Service) NotifyTriggerResult(ctx context.Context, result *TriggerResult) error {
	// In a real implementation, this would process the trigger result
	// For now, we'll just log that a result was received
	return nil
}
