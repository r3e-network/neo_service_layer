package internal

import (
	"context"
	"sync"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/triggerservice/models"
)

// TriggerMetricsCollectorImpl implements the TriggerMetricsCollector interface
type TriggerMetricsCollectorImpl struct {
	metrics *models.TriggerMetrics
	mu      sync.RWMutex
}

// NewTriggerMetricsCollector creates a new TriggerMetricsCollector instance
func NewTriggerMetricsCollector() TriggerMetricsCollector {
	return &TriggerMetricsCollectorImpl{
		metrics: &models.TriggerMetrics{
			TotalExecutions:      0,
			SuccessfulExecutions: 0,
			FailedExecutions:     0,
			AverageGasUsed:       0,
			AverageLatency:       0,
		},
	}
}

// RecordExecution records a trigger execution
func (tmc *TriggerMetricsCollectorImpl) RecordExecution(ctx context.Context, execution *models.TriggerExecution) {
	tmc.mu.Lock()
	defer tmc.mu.Unlock()

	tmc.metrics.TotalExecutions++
	if execution.Status == "completed" {
		tmc.metrics.SuccessfulExecutions++
	}

	// Update average gas used
	oldGasUsed := tmc.metrics.AverageGasUsed
	oldCount := int64(tmc.metrics.TotalExecutions - 1)
	if oldCount > 0 {
		tmc.metrics.AverageGasUsed = (oldGasUsed*oldCount + execution.GasUsed) / (oldCount + 1)
	} else {
		tmc.metrics.AverageGasUsed = execution.GasUsed
	}

	// Update average latency
	latency := execution.EndTime.Sub(execution.StartTime)
	oldLatency := tmc.metrics.AverageLatency
	if oldCount > 0 {
		tmc.metrics.AverageLatency = time.Duration((float64(oldLatency)*float64(oldCount) + float64(latency)) / float64(oldCount+1))
	} else {
		tmc.metrics.AverageLatency = latency
	}
}

// RecordFailedExecution records a failed trigger execution
func (tmc *TriggerMetricsCollectorImpl) RecordFailedExecution(ctx context.Context, triggerID string, reason string) {
	tmc.mu.Lock()
	defer tmc.mu.Unlock()

	tmc.metrics.FailedExecutions++
}

// GetMetrics gets the current trigger metrics
func (tmc *TriggerMetricsCollectorImpl) GetMetrics(ctx context.Context) *models.TriggerMetrics {
	tmc.mu.RLock()
	defer tmc.mu.RUnlock()

	return &models.TriggerMetrics{
		TotalExecutions:      tmc.metrics.TotalExecutions,
		SuccessfulExecutions: tmc.metrics.SuccessfulExecutions,
		FailedExecutions:     tmc.metrics.FailedExecutions,
		AverageGasUsed:       tmc.metrics.AverageGasUsed,
		AverageLatency:       tmc.metrics.AverageLatency,
	}
}
