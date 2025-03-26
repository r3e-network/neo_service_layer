package gasbank

import (
	"context"
	"math/big"
	"sync"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

// GasMonitor monitors gas usage and alerts
type GasMonitor struct {
	metrics    *models.GasUsageMetrics
	alerts     []Alert
	thresholds struct {
		lowGas       *big.Int
		highUsage    *big.Int
		failureRate  float64
		responseTime time.Duration
	}
	mu sync.RWMutex
}

// Alert represents a gas-related alert
type Alert struct {
	Type      string
	Message   string
	Severity  string
	Timestamp time.Time
	Data      map[string]interface{}
}

// NewGasMonitor creates a new gas monitor
func NewGasMonitor(lowGasThreshold, highUsageThreshold *big.Int, failureRateThreshold float64, responseTimeThreshold time.Duration) *GasMonitor {
	return &GasMonitor{
		metrics: &models.GasUsageMetrics{
			TotalAllocated: big.NewInt(0),
			TotalUsed:      big.NewInt(0),
			ActiveUsers:    0,
			Refills:        0,
			FailedRefills:  0,
		},
		thresholds: struct {
			lowGas       *big.Int
			highUsage    *big.Int
			failureRate  float64
			responseTime time.Duration
		}{
			lowGas:       lowGasThreshold,
			highUsage:    highUsageThreshold,
			failureRate:  failureRateThreshold,
			responseTime: responseTimeThreshold,
		},
	}
}

// RecordAllocation records a gas allocation
func (gm *GasMonitor) RecordAllocation(ctx context.Context, allocation *models.Allocation) {
	gm.mu.Lock()
	defer gm.mu.Unlock()

	gm.metrics.TotalAllocated.Add(gm.metrics.TotalAllocated, allocation.Amount)
	gm.metrics.ActiveUsers++

	if gm.metrics.TotalAllocated.Cmp(gm.thresholds.highUsage) > 0 {
		gm.addAlert(Alert{
			Type:     "HighGasUsage",
			Message:  "Total allocated gas exceeds high usage threshold",
			Severity: "warning",
			Data: map[string]interface{}{
				"totalAllocated": gm.metrics.TotalAllocated.String(),
				"threshold":      gm.thresholds.highUsage.String(),
			},
		})
	}
}

// RecordUsage records gas usage
func (gm *GasMonitor) RecordUsage(ctx context.Context, userAddress util.Uint160, amount *big.Int) {
	gm.mu.Lock()
	defer gm.mu.Unlock()

	gm.metrics.TotalUsed.Add(gm.metrics.TotalUsed, amount)

	remaining := new(big.Int).Sub(gm.metrics.TotalAllocated, gm.metrics.TotalUsed)
	if remaining.Cmp(gm.thresholds.lowGas) < 0 {
		gm.addAlert(Alert{
			Type:     "LowGas",
			Message:  "Remaining gas below threshold",
			Severity: "critical",
			Data: map[string]interface{}{
				"remaining": remaining.String(),
				"threshold": gm.thresholds.lowGas.String(),
			},
		})
	}
}

// RecordRefill records a gas refill attempt
func (gm *GasMonitor) RecordRefill(ctx context.Context, amount *big.Int, success bool) {
	gm.mu.Lock()
	defer gm.mu.Unlock()

	if success {
		gm.metrics.Refills++
		gm.metrics.TotalAllocated.Add(gm.metrics.TotalAllocated, amount)
	} else {
		gm.metrics.FailedRefills++
		failureRate := float64(gm.metrics.FailedRefills) / float64(gm.metrics.Refills+gm.metrics.FailedRefills)
		if failureRate > gm.thresholds.failureRate {
			gm.addAlert(Alert{
				Type:     "HighRefillFailureRate",
				Message:  "Gas refill failure rate exceeds threshold",
				Severity: "critical",
				Data: map[string]interface{}{
					"failureRate": failureRate,
					"threshold":   gm.thresholds.failureRate,
				},
			})
		}
	}
}

// GetMetrics gets the current gas usage metrics
func (gm *GasMonitor) GetMetrics() *models.GasUsageMetrics {
	gm.mu.RLock()
	defer gm.mu.RUnlock()

	return &models.GasUsageMetrics{
		TotalAllocated: new(big.Int).Set(gm.metrics.TotalAllocated),
		TotalUsed:      new(big.Int).Set(gm.metrics.TotalUsed),
		ActiveUsers:    gm.metrics.ActiveUsers,
		Refills:        gm.metrics.Refills,
		FailedRefills:  gm.metrics.FailedRefills,
	}
}

// GetAlerts gets all recorded alerts
func (gm *GasMonitor) GetAlerts() []Alert {
	gm.mu.RLock()
	defer gm.mu.RUnlock()

	alerts := make([]Alert, len(gm.alerts))
	copy(alerts, gm.alerts)
	return alerts
}

// ClearAlerts clears all recorded alerts
func (gm *GasMonitor) ClearAlerts() {
	gm.mu.Lock()
	defer gm.mu.Unlock()

	gm.alerts = nil
}

func (gm *GasMonitor) addAlert(alert Alert) {
	alert.Timestamp = time.Now()
	gm.alerts = append(gm.alerts, alert)
}
