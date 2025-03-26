package trigger

import (
	"context"
	"fmt"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/core/neo"
	"github.com/will/neo_service_layer/internal/services/trigger/internal"
	"github.com/will/neo_service_layer/internal/services/trigger/models"
)

// ServiceConfig represents the trigger service configuration
type ServiceConfig struct {
	MaxTriggers     int
	MaxExecutions   int
	ExecutionWindow time.Duration
}

// Service represents the trigger service
type Service struct {
	manager   internal.TriggerManager
	store     internal.TriggerStore
	metrics   internal.TriggerMetricsCollector
	alerts    internal.TriggerAlertManager
	scheduler internal.TriggerScheduler
	neoClient *neo.Client
}

// NewService creates a new trigger service
func NewService(config *ServiceConfig, neoClient *neo.Client) (*Service, error) {
	store := internal.NewTriggerStore()
	metrics := internal.NewTriggerMetricsCollector()
	alerts := internal.NewTriggerAlertManager()
	scheduler := internal.NewTriggerScheduler()

	policy := &models.TriggerPolicy{
		MaxTriggersPerUser:      config.MaxTriggers,
		MaxExecutionsPerTrigger: config.MaxExecutions,
		ExecutionWindow:         config.ExecutionWindow,
		MinInterval:             time.Minute,
		MaxInterval:             time.Hour * 24,
		CooldownPeriod:          time.Minute * 5,
	}

	manager := internal.NewTriggerManager(store, metrics, alerts, scheduler, policy, neoClient)

	return &Service{
		manager:   manager,
		store:     store,
		metrics:   metrics,
		alerts:    alerts,
		scheduler: scheduler,
		neoClient: neoClient,
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

// handlers stores the registered trigger handlers
type handlers map[string]TriggerHandler

// Start starts the trigger service
func (s *Service) Start(ctx context.Context) error {
	// In a real implementation, this would start all components
	// For now, return nil to indicate success
	return nil
}

// Stop stops the trigger service
func (s *Service) Stop(ctx context.Context) error {
	// In a real implementation, this would stop all components
	// For now, return nil to indicate success
	return nil
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
