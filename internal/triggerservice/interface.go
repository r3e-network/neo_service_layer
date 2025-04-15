package trigger

import (
	"context"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/triggerservice/models"
)

// IService defines the interface for the Trigger service
type IService interface {
	// Start starts the service
	Start(ctx context.Context) error

	// Stop stops the service
	Stop(ctx context.Context) error

	// CreateTrigger creates a new trigger
	CreateTrigger(ctx context.Context, userAddress util.Uint160, trigger *models.Trigger) (*models.Trigger, error)

	// GetTrigger gets a trigger by ID
	GetTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*models.Trigger, error)

	// UpdateTrigger updates an existing trigger
	UpdateTrigger(ctx context.Context, userAddress util.Uint160, triggerID string, trigger *models.Trigger) (*models.Trigger, error)

	// DeleteTrigger deletes a trigger
	DeleteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) error

	// ListTriggers lists all triggers for a user
	ListTriggers(ctx context.Context, userAddress util.Uint160) ([]*models.Trigger, error)

	// GetTriggerExecutions gets execution history for a trigger
	GetTriggerExecutions(ctx context.Context, triggerID string) ([]*models.TriggerExecution, error)

	// GetTriggerMetrics gets metrics for a trigger
	GetTriggerMetrics(ctx context.Context, triggerID string) (*models.TriggerMetrics, error)

	// GetTriggerPolicy gets the current trigger policy
	GetTriggerPolicy(ctx context.Context) (*models.TriggerPolicy, error)

	// UpdateTriggerPolicy updates the trigger policy
	UpdateTriggerPolicy(ctx context.Context, policy *models.TriggerPolicy) error

	// ExecuteTrigger executes a trigger
	ExecuteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*models.Execution, error)
}
