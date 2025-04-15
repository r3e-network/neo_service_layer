package internal

import (
	"context"
	"sync"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/pricefeedservice/models"
)

// PriceMetricsCollectorImpl implements the PriceMetricsCollector interface
type PriceMetricsCollectorImpl struct {
	totalUpdates     int
	failedUpdates    int
	averageLatency   time.Duration
	lastUpdateTime   time.Time
	dataSourceHealth map[string]float64
	mu               sync.RWMutex
}

// NewPriceMetricsCollector creates a new PriceMetricsCollector instance
func NewPriceMetricsCollector() PriceMetricsCollector {
	return &PriceMetricsCollectorImpl{
		dataSourceHealth: make(map[string]float64),
	}
}

// Start starts the metrics collector
func (pmc *PriceMetricsCollectorImpl) Start(ctx context.Context) error {
	// Nothing to start for now
	return nil
}

// Stop stops the metrics collector
func (pmc *PriceMetricsCollectorImpl) Stop(ctx context.Context) error {
	// Nothing to stop for now
	return nil
}

// RecordUpdate records a successful price update
func (pmc *PriceMetricsCollectorImpl) RecordUpdate(ctx context.Context, price *models.Price, latency time.Duration) {
	pmc.mu.Lock()
	defer pmc.mu.Unlock()

	pmc.totalUpdates++
	pmc.lastUpdateTime = time.Now()

	// Update average latency
	if pmc.averageLatency == 0 {
		pmc.averageLatency = latency
	} else {
		pmc.averageLatency = (pmc.averageLatency + latency) / 2
	}
}

// RecordFailedUpdate records a failed price update
func (pmc *PriceMetricsCollectorImpl) RecordFailedUpdate(ctx context.Context, assetID string, reason string) {
	pmc.mu.Lock()
	defer pmc.mu.Unlock()

	pmc.failedUpdates++
}

// UpdateDataSourceHealth updates the health score for a data source
func (pmc *PriceMetricsCollectorImpl) UpdateDataSourceHealth(ctx context.Context, source string, health float64) {
	pmc.mu.Lock()
	defer pmc.mu.Unlock()

	pmc.dataSourceHealth[source] = health
}

// GetMetrics gets the current metrics
func (pmc *PriceMetricsCollectorImpl) GetMetrics(ctx context.Context) *models.PriceMetrics {
	pmc.mu.RLock()
	defer pmc.mu.RUnlock()

	return &models.PriceMetrics{
		TotalUpdates:     pmc.totalUpdates,
		FailedUpdates:    pmc.failedUpdates,
		AverageLatency:   pmc.averageLatency,
		LastUpdateTime:   pmc.lastUpdateTime,
		DataSourceHealth: pmc.dataSourceHealth,
	}
}
