package internal

import (
	"context"
	"math/big"
	"sync"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

// GasMetricsCollectorImpl implements the GasMetricsCollector interface
type GasMetricsCollectorImpl struct {
	metrics models.GasUsageMetrics
	mu      sync.RWMutex
	// activeUsers keeps track of active user addresses
	activeUsers map[string]bool
}

// NewGasMetricsCollector creates a new GasMetricsCollector
func NewGasMetricsCollector() GasMetricsCollector {
	return &GasMetricsCollectorImpl{
		metrics: models.GasUsageMetrics{
			TotalAllocated: big.NewInt(0),
			TotalUsed:      big.NewInt(0),
			ActiveUsers:    0,
			Refills:        0,
			FailedRefills:  0,
		},
		activeUsers: make(map[string]bool),
	}
}

// RecordAllocation records a gas allocation
func (mc *GasMetricsCollectorImpl) RecordAllocation(ctx context.Context, allocation *models.Allocation) {
	if allocation == nil {
		return
	}

	mc.mu.Lock()
	defer mc.mu.Unlock()

	// Add allocation amount to total
	mc.metrics.TotalAllocated = new(big.Int).Add(mc.metrics.TotalAllocated, allocation.Amount)

	// Mark user as active
	userKey := allocation.UserAddress.String()
	if !mc.activeUsers[userKey] {
		mc.activeUsers[userKey] = true
		mc.metrics.ActiveUsers++
	}
}

// RecordUsage records gas usage
func (mc *GasMetricsCollectorImpl) RecordUsage(ctx context.Context, userAddress util.Uint160, amount *big.Int) {
	if amount == nil {
		return
	}

	mc.mu.Lock()
	defer mc.mu.Unlock()

	// Add usage amount to total
	mc.metrics.TotalUsed = new(big.Int).Add(mc.metrics.TotalUsed, amount)

	// Mark user as active (in case they weren't already)
	userKey := userAddress.String()
	if !mc.activeUsers[userKey] {
		mc.activeUsers[userKey] = true
		mc.metrics.ActiveUsers++
	}
}

// RecordRefill records a gas pool refill
func (mc *GasMetricsCollectorImpl) RecordRefill(ctx context.Context, amount *big.Int, success bool) {
	mc.mu.Lock()
	defer mc.mu.Unlock()

	if success {
		mc.metrics.Refills++
	} else {
		mc.metrics.FailedRefills++
	}
}

// GetMetrics returns the current gas usage metrics
func (mc *GasMetricsCollectorImpl) GetMetrics(ctx context.Context) *models.GasUsageMetrics {
	mc.mu.RLock()
	defer mc.mu.RUnlock()

	// Return a copy to avoid race conditions
	metrics := models.GasUsageMetrics{
		TotalAllocated: new(big.Int).Set(mc.metrics.TotalAllocated),
		TotalUsed:      new(big.Int).Set(mc.metrics.TotalUsed),
		ActiveUsers:    mc.metrics.ActiveUsers,
		Refills:        mc.metrics.Refills,
		FailedRefills:  mc.metrics.FailedRefills,
	}

	return &metrics
}

// ResetMetrics resets all metrics
func (mc *GasMetricsCollectorImpl) ResetMetrics(ctx context.Context) {
	mc.mu.Lock()
	defer mc.mu.Unlock()

	mc.metrics = models.GasUsageMetrics{
		TotalAllocated: big.NewInt(0),
		TotalUsed:      big.NewInt(0),
		ActiveUsers:    0,
		Refills:        0,
		FailedRefills:  0,
	}
	mc.activeUsers = make(map[string]bool)
}

// RemoveInactiveUser removes a user from the active users list
func (mc *GasMetricsCollectorImpl) RemoveInactiveUser(ctx context.Context, userAddress util.Uint160) {
	mc.mu.Lock()
	defer mc.mu.Unlock()

	userKey := userAddress.String()
	if mc.activeUsers[userKey] {
		delete(mc.activeUsers, userKey)
		mc.metrics.ActiveUsers--
	}
}
