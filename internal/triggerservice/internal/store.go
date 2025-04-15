package internal

import (
	"context"
	"sync"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/triggerservice/models"
)

// TriggerStoreImpl implements the TriggerStore interface
type TriggerStoreImpl struct {
	triggers   sync.Map // map[string]*models.Trigger
	executions sync.Map // map[string][]*models.TriggerExecution
	mu         sync.RWMutex
}

// NewTriggerStore creates a new TriggerStore instance
func NewTriggerStore() TriggerStore {
	return &TriggerStoreImpl{}
}

// SaveTrigger saves a trigger to the store
func (ts *TriggerStoreImpl) SaveTrigger(ctx context.Context, trigger *models.Trigger) error {
	ts.triggers.Store(trigger.ID, trigger)
	return nil
}

// GetTrigger gets a trigger by ID
func (ts *TriggerStoreImpl) GetTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*models.Trigger, error) {
	value, ok := ts.triggers.Load(triggerID)
	if !ok {
		return nil, nil
	}

	trigger := value.(*models.Trigger)
	if trigger.UserAddress != userAddress {
		return nil, nil
	}

	return trigger, nil
}

// DeleteTrigger deletes a trigger from the store
func (ts *TriggerStoreImpl) DeleteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) error {
	trigger, err := ts.GetTrigger(ctx, userAddress, triggerID)
	if err != nil {
		return err
	}

	if trigger == nil {
		return nil
	}

	ts.triggers.Delete(triggerID)
	return nil
}

// ListTriggers lists all triggers for a user
func (ts *TriggerStoreImpl) ListTriggers(ctx context.Context, userAddress util.Uint160) ([]*models.Trigger, error) {
	var triggers []*models.Trigger

	ts.triggers.Range(func(key, value interface{}) bool {
		trigger := value.(*models.Trigger)
		if trigger.UserAddress == userAddress {
			triggers = append(triggers, trigger)
		}
		return true
	})

	return triggers, nil
}

// SaveExecution saves a trigger execution record
func (ts *TriggerStoreImpl) SaveExecution(ctx context.Context, execution *models.TriggerExecution) error {
	ts.mu.Lock()
	defer ts.mu.Unlock()

	value, _ := ts.executions.LoadOrStore(execution.TriggerID, []*models.TriggerExecution{})
	executions := value.([]*models.TriggerExecution)
	executions = append(executions, execution)
	ts.executions.Store(execution.TriggerID, executions)

	return nil
}

// GetExecutions gets the execution history for a trigger
func (ts *TriggerStoreImpl) GetExecutions(ctx context.Context, userAddress util.Uint160, triggerID string) ([]*models.TriggerExecution, error) {
	trigger, err := ts.GetTrigger(ctx, userAddress, triggerID)
	if err != nil {
		return nil, err
	}

	if trigger == nil {
		return nil, nil
	}

	value, ok := ts.executions.Load(triggerID)
	if !ok {
		return nil, nil
	}

	executions := value.([]*models.TriggerExecution)
	return executions, nil
}
