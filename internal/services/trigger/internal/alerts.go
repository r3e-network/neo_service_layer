package internal

import (
	"context"
	"sync"
	"time"

	"github.com/will/neo_service_layer/internal/services/trigger/models"
)

// TriggerAlert represents a trigger-related alert
type TriggerAlert struct {
	Type      string
	Message   string
	Severity  string
	Timestamp time.Time
	Data      map[string]interface{}
}

// TriggerAlertManagerImpl implements the TriggerAlertManager interface
type TriggerAlertManagerImpl struct {
	alerts []TriggerAlert
	mu     sync.RWMutex
}

// NewTriggerAlertManager creates a new TriggerAlertManager instance
func NewTriggerAlertManager() TriggerAlertManager {
	return &TriggerAlertManagerImpl{}
}

// AlertExecutionFailure alerts on trigger execution failures
func (tam *TriggerAlertManagerImpl) AlertExecutionFailure(ctx context.Context, trigger *models.Trigger, reason string) {
	tam.mu.Lock()
	defer tam.mu.Unlock()

	tam.alerts = append(tam.alerts, TriggerAlert{
		Type:     "ExecutionFailure",
		Message:  "Trigger execution failed",
		Severity: "critical",
		Data: map[string]interface{}{
			"triggerID":   trigger.ID,
			"userAddress": trigger.UserAddress.String(),
			"reason":      reason,
		},
		Timestamp: time.Now(),
	})
}

// AlertHighGasUsage alerts on high gas usage
func (tam *TriggerAlertManagerImpl) AlertHighGasUsage(ctx context.Context, trigger *models.Trigger, gasUsed int64) {
	tam.mu.Lock()
	defer tam.mu.Unlock()

	tam.alerts = append(tam.alerts, TriggerAlert{
		Type:     "HighGasUsage",
		Message:  "High gas usage detected",
		Severity: "warning",
		Data: map[string]interface{}{
			"triggerID":   trigger.ID,
			"userAddress": trigger.UserAddress.String(),
			"gasUsed":     gasUsed,
		},
		Timestamp: time.Now(),
	})
}

// AlertScheduleDeviation alerts on schedule deviations
func (tam *TriggerAlertManagerImpl) AlertScheduleDeviation(ctx context.Context, trigger *models.Trigger, deviation string) {
	tam.mu.Lock()
	defer tam.mu.Unlock()

	tam.alerts = append(tam.alerts, TriggerAlert{
		Type:     "ScheduleDeviation",
		Message:  "Trigger schedule deviation detected",
		Severity: "warning",
		Data: map[string]interface{}{
			"triggerID":   trigger.ID,
			"userAddress": trigger.UserAddress.String(),
			"deviation":   deviation,
		},
		Timestamp: time.Now(),
	})
}

// GetAlerts gets all recorded alerts
func (tam *TriggerAlertManagerImpl) GetAlerts() []TriggerAlert {
	tam.mu.RLock()
	defer tam.mu.RUnlock()

	alerts := make([]TriggerAlert, len(tam.alerts))
	copy(alerts, tam.alerts)
	return alerts
}

// ClearAlerts clears all recorded alerts
func (tam *TriggerAlertManagerImpl) ClearAlerts() {
	tam.mu.Lock()
	defer tam.mu.Unlock()

	tam.alerts = nil
}