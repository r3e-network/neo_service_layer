package metrics

import (
	"context"
	"math/big"
	"sync"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/prometheus/client_golang/prometheus"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

const (
	// Namespace for Prometheus metrics
	metricsNamespace = "neo_service_layer"
	// Subsystem for gas bank metrics
	metricsSubsystem = "gas_bank"
)

// PrometheusCollector implements GasMetricsCollector using Prometheus
type PrometheusCollector struct {
	// Metrics counters
	allocationsTotal   prometheus.Counter
	gasAllocatedTotal  prometheus.Counter
	gasUsedTotal       prometheus.Counter
	gasRefillTotal     prometheus.Counter
	failedRefillsTotal prometheus.Counter

	// Cached metric values for GetMetrics
	totalAllocated     int64
	totalUsed          int64
	totalRefills       int
	totalFailedRefills int

	// Metrics gauges
	activeAllocations prometheus.Gauge
	availableGas      prometheus.Gauge

	// Histograms
	allocationAmounts prometheus.Histogram
	usageAmounts      prometheus.Histogram

	// Active users tracking for gauge updates
	activeUsers map[string]struct{}
	mu          sync.RWMutex
}

// NewPrometheusCollector creates a new Prometheus metrics collector
func NewPrometheusCollector(registry prometheus.Registerer) (*PrometheusCollector, error) {
	collector := &PrometheusCollector{
		allocationsTotal: prometheus.NewCounter(prometheus.CounterOpts{
			Namespace: metricsNamespace,
			Subsystem: metricsSubsystem,
			Name:      "allocations_total",
			Help:      "Total number of gas allocations",
		}),

		gasAllocatedTotal: prometheus.NewCounter(prometheus.CounterOpts{
			Namespace: metricsNamespace,
			Subsystem: metricsSubsystem,
			Name:      "gas_allocated_total",
			Help:      "Total amount of gas allocated (in smallest units)",
		}),

		gasUsedTotal: prometheus.NewCounter(prometheus.CounterOpts{
			Namespace: metricsNamespace,
			Subsystem: metricsSubsystem,
			Name:      "gas_used_total",
			Help:      "Total amount of gas used (in smallest units)",
		}),

		gasRefillTotal: prometheus.NewCounter(prometheus.CounterOpts{
			Namespace: metricsNamespace,
			Subsystem: metricsSubsystem,
			Name:      "gas_refill_total",
			Help:      "Total amount of gas added through refills (in smallest units)",
		}),

		failedRefillsTotal: prometheus.NewCounter(prometheus.CounterOpts{
			Namespace: metricsNamespace,
			Subsystem: metricsSubsystem,
			Name:      "failed_refills_total",
			Help:      "Total number of failed refill attempts",
		}),

		activeAllocations: prometheus.NewGauge(prometheus.GaugeOpts{
			Namespace: metricsNamespace,
			Subsystem: metricsSubsystem,
			Name:      "active_allocations",
			Help:      "Current number of active gas allocations",
		}),

		availableGas: prometheus.NewGauge(prometheus.GaugeOpts{
			Namespace: metricsNamespace,
			Subsystem: metricsSubsystem,
			Name:      "available_gas",
			Help:      "Current amount of gas available in the pool (in smallest units)",
		}),

		allocationAmounts: prometheus.NewHistogram(prometheus.HistogramOpts{
			Namespace: metricsNamespace,
			Subsystem: metricsSubsystem,
			Name:      "allocation_amounts",
			Help:      "Distribution of gas allocation amounts (in smallest units)",
			Buckets:   prometheus.ExponentialBuckets(100000, 2, 10), // 10 buckets from 0.1 GAS to ~100 GAS
		}),

		usageAmounts: prometheus.NewHistogram(prometheus.HistogramOpts{
			Namespace: metricsNamespace,
			Subsystem: metricsSubsystem,
			Name:      "usage_amounts",
			Help:      "Distribution of gas usage amounts (in smallest units)",
			Buckets:   prometheus.ExponentialBuckets(10000, 2, 10), // 10 buckets from 0.01 GAS to ~10 GAS
		}),

		// Initialize cached counter values
		totalAllocated:     0,
		totalUsed:          0,
		totalRefills:       0,
		totalFailedRefills: 0,

		activeUsers: make(map[string]struct{}),
	}

	// Register metrics with Prometheus
	if registry != nil {
		registry.MustRegister(
			collector.allocationsTotal,
			collector.gasAllocatedTotal,
			collector.gasUsedTotal,
			collector.gasRefillTotal,
			collector.failedRefillsTotal,
			collector.activeAllocations,
			collector.availableGas,
			collector.allocationAmounts,
			collector.usageAmounts,
		)
	}

	return collector, nil
}

// RecordAllocation records a new gas allocation
func (c *PrometheusCollector) RecordAllocation(ctx context.Context, allocation *models.GasAllocation) {
	if allocation == nil {
		return
	}

	c.allocationsTotal.Inc()
	c.allocationAmounts.Observe(float64(allocation.Amount.Int64()))

	allocationAmount := allocation.Amount.Int64()
	c.gasAllocatedTotal.Add(float64(allocationAmount))

	// Update cached values
	c.mu.Lock()
	c.totalAllocated += allocationAmount
	c.mu.Unlock()

	// Update active users
	userKey := allocation.UserAddress.String()
	c.mu.Lock()
	c.activeUsers[userKey] = struct{}{}
	c.mu.Unlock()

	// Update active allocations gauge
	c.mu.RLock()
	c.activeAllocations.Set(float64(len(c.activeUsers)))
	c.mu.RUnlock()
}

// RecordUsage records gas usage
func (c *PrometheusCollector) RecordUsage(ctx context.Context, userAddress util.Uint160, amount *big.Int) {
	if amount == nil {
		return
	}

	usageAmount := amount.Int64()
	c.gasUsedTotal.Add(float64(usageAmount))
	c.usageAmounts.Observe(float64(usageAmount))

	// Update cached values
	c.mu.Lock()
	c.totalUsed += usageAmount
	c.mu.Unlock()
}

// RecordRefill records a gas pool refill event
func (c *PrometheusCollector) RecordRefill(ctx context.Context, amount *big.Int, success bool) {
	if amount == nil {
		return
	}

	if success {
		c.gasRefillTotal.Inc()
		// Update cached values
		c.mu.Lock()
		c.totalRefills++
		c.mu.Unlock()
	} else {
		c.failedRefillsTotal.Inc()
		// Update cached values
		c.mu.Lock()
		c.totalFailedRefills++
		c.mu.Unlock()
	}
}

// UpdatePoolGas updates the current available gas in the pool
func (c *PrometheusCollector) UpdatePoolGas(ctx context.Context, availableAmount *big.Int) {
	if availableAmount == nil {
		return
	}

	c.availableGas.Set(float64(availableAmount.Int64()))
}

// RemoveUser removes a user from the active users tracking
func (c *PrometheusCollector) RemoveUser(ctx context.Context, userAddress util.Uint160) {
	c.mu.Lock()
	defer c.mu.Unlock()

	userKey := userAddress.String()
	delete(c.activeUsers, userKey)
	c.activeAllocations.Set(float64(len(c.activeUsers)))
}

// GetMetrics returns current gas usage metrics
func (c *PrometheusCollector) GetMetrics(ctx context.Context) *models.GasUsageMetrics {
	c.mu.RLock()
	activeUsers := len(c.activeUsers)
	totalAllocated := c.totalAllocated
	totalUsed := c.totalUsed
	totalRefills := c.totalRefills
	totalFailedRefills := c.totalFailedRefills
	c.mu.RUnlock()

	return &models.GasUsageMetrics{
		TotalAllocated: big.NewInt(totalAllocated),
		TotalUsed:      big.NewInt(totalUsed),
		ActiveUsers:    activeUsers,
		Refills:        totalRefills,
		FailedRefills:  totalFailedRefills,
	}
}
