package internal

import (
	"context"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/triggerservice/models"
)

// TriggerManager manages triggers and their executions
type TriggerManager interface {
	CreateTrigger(ctx context.Context, userAddress util.Uint160, trigger *models.Trigger) (*models.Trigger, error)
	UpdateTrigger(ctx context.Context, userAddress util.Uint160, triggerID string, trigger *models.Trigger) (*models.Trigger, error)
	DeleteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) error
	GetTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*models.Trigger, error)
	ListTriggers(ctx context.Context, userAddress util.Uint160) ([]*models.Trigger, error)
	ExecuteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*models.TriggerExecution, error)
	GetTriggerExecutions(ctx context.Context, userAddress util.Uint160, triggerID string) ([]*models.TriggerExecution, error)
	GetPolicy() *models.TriggerPolicy
	UpdatePolicy(policy *models.TriggerPolicy) error
}

// TriggerStore stores trigger data
type TriggerStore interface {
	SaveTrigger(ctx context.Context, trigger *models.Trigger) error
	GetTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*models.Trigger, error)
	DeleteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) error
	ListTriggers(ctx context.Context, userAddress util.Uint160) ([]*models.Trigger, error)
	SaveExecution(ctx context.Context, execution *models.TriggerExecution) error
	GetExecutions(ctx context.Context, userAddress util.Uint160, triggerID string) ([]*models.TriggerExecution, error)
}

// TriggerMetricsCollector collects trigger execution metrics
type TriggerMetricsCollector interface {
	RecordExecution(ctx context.Context, execution *models.TriggerExecution)
	RecordFailedExecution(ctx context.Context, triggerID string, reason string)
	GetMetrics(ctx context.Context) *models.TriggerMetrics
}

// TriggerAlertManager manages trigger-related alerts
type TriggerAlertManager interface {
	AlertExecutionFailure(ctx context.Context, trigger *models.Trigger, reason string)
	AlertHighGasUsage(ctx context.Context, trigger *models.Trigger, gasUsed int64)
	AlertScheduleDeviation(ctx context.Context, trigger *models.Trigger, deviation string)
}

// TriggerScheduler manages trigger scheduling
type TriggerScheduler interface {
	ScheduleTrigger(ctx context.Context, trigger *models.Trigger) error
	UnscheduleTrigger(ctx context.Context, triggerID string) error
	GetNextExecutionTime(ctx context.Context, schedule string) (time.Time, error)
	Start(ctx context.Context) error
	Stop(ctx context.Context) error
}
