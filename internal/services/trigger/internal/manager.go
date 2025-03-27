package internal

import (
	"context"
	"fmt"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/trigger/models"
)

// TriggerManagerImpl implements the TriggerManager interface
type TriggerManagerImpl struct {
	store     TriggerStore
	metrics   TriggerMetricsCollector
	alerts    TriggerAlertManager
	scheduler TriggerScheduler
	policy    *models.TriggerPolicy
	neoClient interface{} // Replace with actual Neo client interface
}

// NewTriggerManager creates a new TriggerManager instance
func NewTriggerManager(store TriggerStore, metrics TriggerMetricsCollector, alerts TriggerAlertManager, scheduler TriggerScheduler, policy *models.TriggerPolicy, neoClient interface{}) TriggerManager {
	return &TriggerManagerImpl{
		store:     store,
		metrics:   metrics,
		alerts:    alerts,
		scheduler: scheduler,
		policy:    policy,
		neoClient: neoClient,
	}
}

// CreateTrigger creates a new trigger
func (tm *TriggerManagerImpl) CreateTrigger(ctx context.Context, userAddress util.Uint160, trigger *models.Trigger) (*models.Trigger, error) {
	triggers, err := tm.store.ListTriggers(ctx, userAddress)
	if err != nil {
		return nil, fmt.Errorf("failed to list triggers: %w", err)
	}

	if len(triggers) >= tm.policy.MaxTriggersPerUser {
		return nil, fmt.Errorf("maximum number of triggers reached for user")
	}

	trigger.ID = uuid.New().String()
	trigger.UserAddress = userAddress
	trigger.Status = "active"
	trigger.CreatedAt = time.Now()
	trigger.UpdatedAt = time.Now()

	nextExecution, err := tm.scheduler.GetNextExecutionTime(ctx, trigger.Schedule)
	if err != nil {
		return nil, fmt.Errorf("failed to calculate next execution time: %w", err)
	}
	trigger.NextExecution = nextExecution

	if err := tm.store.SaveTrigger(ctx, trigger); err != nil {
		return nil, fmt.Errorf("failed to save trigger: %w", err)
	}

	if err := tm.scheduler.ScheduleTrigger(ctx, trigger); err != nil {
		return nil, fmt.Errorf("failed to schedule trigger: %w", err)
	}

	return trigger, nil
}

// UpdateTrigger updates an existing trigger
func (tm *TriggerManagerImpl) UpdateTrigger(ctx context.Context, userAddress util.Uint160, triggerID string, trigger *models.Trigger) (*models.Trigger, error) {
	existing, err := tm.store.GetTrigger(ctx, userAddress, triggerID)
	if err != nil {
		return nil, fmt.Errorf("failed to get trigger: %w", err)
	}

	if existing == nil {
		return nil, fmt.Errorf("trigger not found")
	}

	trigger.ID = existing.ID
	trigger.UserAddress = userAddress
	trigger.CreatedAt = existing.CreatedAt
	trigger.UpdatedAt = time.Now()

	nextExecution, err := tm.scheduler.GetNextExecutionTime(ctx, trigger.Schedule)
	if err != nil {
		return nil, fmt.Errorf("failed to calculate next execution time: %w", err)
	}
	trigger.NextExecution = nextExecution

	if err := tm.store.SaveTrigger(ctx, trigger); err != nil {
		return nil, fmt.Errorf("failed to save trigger: %w", err)
	}

	if err := tm.scheduler.UnscheduleTrigger(ctx, triggerID); err != nil {
		return nil, fmt.Errorf("failed to unschedule trigger: %w", err)
	}

	if err := tm.scheduler.ScheduleTrigger(ctx, trigger); err != nil {
		return nil, fmt.Errorf("failed to schedule trigger: %w", err)
	}

	return trigger, nil
}

// DeleteTrigger deletes a trigger
func (tm *TriggerManagerImpl) DeleteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) error {
	trigger, err := tm.store.GetTrigger(ctx, userAddress, triggerID)
	if err != nil {
		return fmt.Errorf("failed to get trigger: %w", err)
	}

	if trigger == nil {
		return fmt.Errorf("trigger not found")
	}

	if err := tm.scheduler.UnscheduleTrigger(ctx, triggerID); err != nil {
		return fmt.Errorf("failed to unschedule trigger: %w", err)
	}

	if err := tm.store.DeleteTrigger(ctx, userAddress, triggerID); err != nil {
		return fmt.Errorf("failed to delete trigger: %w", err)
	}

	return nil
}

// GetTrigger gets a trigger by ID
func (tm *TriggerManagerImpl) GetTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*models.Trigger, error) {
	trigger, err := tm.store.GetTrigger(ctx, userAddress, triggerID)
	if err != nil {
		return nil, fmt.Errorf("failed to get trigger: %w", err)
	}

	if trigger == nil {
		return nil, fmt.Errorf("trigger not found")
	}

	return trigger, nil
}

// ListTriggers lists all triggers for a user
func (tm *TriggerManagerImpl) ListTriggers(ctx context.Context, userAddress util.Uint160) ([]*models.Trigger, error) {
	triggers, err := tm.store.ListTriggers(ctx, userAddress)
	if err != nil {
		return nil, fmt.Errorf("failed to list triggers: %w", err)
	}

	return triggers, nil
}

// ExecuteTrigger executes a trigger
func (tm *TriggerManagerImpl) ExecuteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*models.TriggerExecution, error) {
	trigger, err := tm.store.GetTrigger(ctx, userAddress, triggerID)
	if err != nil {
		return nil, fmt.Errorf("failed to get trigger: %w", err)
	}

	if trigger == nil {
		return nil, fmt.Errorf("trigger not found")
	}

	execution := &models.TriggerExecution{
		ID:          uuid.New().String(),
		TriggerID:   triggerID,
		UserAddress: userAddress,
		StartTime:   time.Now(),
		Status:      "running",
	}

	// Implementation would evaluate condition and execute function
	execution.EndTime = time.Now()
	execution.Status = "completed"
	execution.GasUsed = 1000                           // Replace with actual gas usage
	execution.Result = "Trigger executed successfully" // Add result

	if err := tm.store.SaveExecution(ctx, execution); err != nil {
		return nil, fmt.Errorf("failed to save execution: %w", err)
	}

	tm.metrics.RecordExecution(ctx, execution)

	if execution.GasUsed > int64(float64(tm.policy.MaxExecutionsPerTrigger)*1.5) {
		tm.alerts.AlertHighGasUsage(ctx, trigger, execution.GasUsed)
	}

	return execution, nil
}

// GetTriggerExecutions gets the execution history for a trigger
func (tm *TriggerManagerImpl) GetTriggerExecutions(ctx context.Context, userAddress util.Uint160, triggerID string) ([]*models.TriggerExecution, error) {
	executions, err := tm.store.GetExecutions(ctx, userAddress, triggerID)
	if err != nil {
		return nil, fmt.Errorf("failed to get executions: %w", err)
	}

	return executions, nil
}

// GetPolicy returns the current trigger policy
func (tm *TriggerManagerImpl) GetPolicy() *models.TriggerPolicy {
	return tm.policy
}

// UpdatePolicy updates the trigger policy
func (tm *TriggerManagerImpl) UpdatePolicy(policy *models.TriggerPolicy) error {
	if policy == nil {
		return fmt.Errorf("policy cannot be nil")
	}

	// Validate policy
	if policy.MaxTriggersPerUser <= 0 {
		return fmt.Errorf("max triggers per user must be positive")
	}
	if policy.MaxExecutionsPerTrigger <= 0 {
		return fmt.Errorf("max executions per trigger must be positive")
	}
	if policy.ExecutionWindow <= 0 {
		return fmt.Errorf("execution window must be positive")
	}
	if policy.MinInterval <= 0 {
		return fmt.Errorf("min interval must be positive")
	}
	if policy.MaxInterval <= 0 {
		return fmt.Errorf("max interval must be positive")
	}
	if policy.CooldownPeriod <= 0 {
		return fmt.Errorf("cooldown period must be positive")
	}

	tm.policy = policy
	return nil
}
