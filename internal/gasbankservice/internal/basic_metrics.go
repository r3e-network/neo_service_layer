package internal

import (
	"context"
	"math/big"
	"sync"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
)

// BasicMetricsCollector implements a simple in-memory metrics collector
type BasicMetricsCollector struct {
	totalAllocated *big.Int
	totalUsed      *big.Int
	activeUsers    map[string]struct{}
	refills        int
	failedRefills  int
	mu             sync.RWMutex
}

// NewBasicMetricsCollector creates a new BasicMetricsCollector
func NewBasicMetricsCollector() *BasicMetricsCollector {
	return &BasicMetricsCollector{
		totalAllocated: big.NewInt(0),
		totalUsed:      big.NewInt(0),
		activeUsers:    make(map[string]struct{}),
		refills:        0,
		failedRefills:  0,
	}
}

// RecordAllocation records a new gas allocation
func (c *BasicMetricsCollector) RecordAllocation(ctx context.Context, allocation *models.Allocation) {
	if allocation == nil {
		return
	}

	c.mu.Lock()
	defer c.mu.Unlock()

	if allocation.Amount != nil {
		c.totalAllocated = new(big.Int).Add(c.totalAllocated, allocation.Amount)
	}

	userKey := allocation.UserAddress.StringLE()
	c.activeUsers[userKey] = struct{}{}
}

// RecordUsage records gas usage from an allocation
func (c *BasicMetricsCollector) RecordUsage(ctx context.Context, userAddress util.Uint160, amount *big.Int) {
	if amount == nil {
		return
	}

	c.mu.Lock()
	defer c.mu.Unlock()

	c.totalUsed = new(big.Int).Add(c.totalUsed, amount)
}

// RecordRefill records a gas pool refill attempt
func (c *BasicMetricsCollector) RecordRefill(ctx context.Context, amount *big.Int, success bool) {
	c.mu.Lock()
	defer c.mu.Unlock()

	if success {
		c.refills++
	} else {
		c.failedRefills++
	}
}

// RemoveUser removes a user from the active users tracking
func (c *BasicMetricsCollector) RemoveUser(ctx context.Context, userAddress util.Uint160) {
	c.mu.Lock()
	defer c.mu.Unlock()

	userKey := userAddress.StringLE()
	delete(c.activeUsers, userKey)
}

// GetMetrics returns current usage metrics
func (c *BasicMetricsCollector) GetMetrics(ctx context.Context) *models.GasUsageMetrics {
	c.mu.RLock()
	defer c.mu.RUnlock()

	return &models.GasUsageMetrics{
		TotalAllocated: new(big.Int).Set(c.totalAllocated),
		TotalUsed:      new(big.Int).Set(c.totalUsed),
		ActiveUsers:    len(c.activeUsers),
		Refills:        c.refills,
		FailedRefills:  c.failedRefills,
	}
}
