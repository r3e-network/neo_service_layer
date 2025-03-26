package integration

import (
	"context"
	"testing"
	"time"

	"github.com/sirupsen/logrus"
	"github.com/stretchr/testify/assert"
	"github.com/will/neo_service_layer/internal/services/metrics"
)

// TestMetricsServiceIntegration tests the basic functionality of the Metrics Service
func TestMetricsServiceIntegration(t *testing.T) {
	// Initialize a logger for testing
	logger := logrus.New()
	logger.SetLevel(logrus.DebugLevel)

	// Create a configuration for the Metrics Service
	metricsConfig := &metrics.Config{
		CollectionInterval: 1 * time.Second,
		RetentionPeriod:    1 * time.Hour,
		StorageBackend:     "memory",
		StorageConfig:      map[string]string{},
	}

	// Initialize the Metrics Service
	metricsService := metrics.NewService(metricsConfig)
	assert.NotNil(t, metricsService)

	// Start the service
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	err := metricsService.Start(ctx)
	assert.NoError(t, err)

	// Register a counter and record values
	labels := map[string]string{"endpoint": "/api/users", "method": "GET"}
	metricsService.RecordCounter("http_requests_total", 10, metrics.ServiceAPI, labels)

	// Record a gauge value
	metricsService.RecordGauge("memory_usage_bytes", 104857600, metrics.ServiceAPI, labels)

	// Record a histogram value
	metricsService.RecordHistogram(
		"request_duration_seconds",
		0.42,
		metrics.ServiceAPI,
		"duration",
		[]float64{0.01, 0.05, 0.1, 0.5, 1, 5},
		labels,
	)

	// Record metrics for Gas Bank service
	gasBankLabels := map[string]string{"operation": "allocate", "user": "test_user"}
	metricsService.RecordCounter("gas_operations_total", 5, metrics.ServiceGasBank, gasBankLabels)
	metricsService.RecordGauge("gas_amount", 10000, metrics.ServiceGasBank, gasBankLabels)

	// Give time for metrics to be processed
	time.Sleep(2 * time.Second)

	// Get all metrics to verify recording
	allMetrics := metricsService.GetAllMetrics()

	// Verify metrics were recorded
	assert.NotEmpty(t, allMetrics)

	// Verify API service metrics exist
	apiMetrics := metricsService.GetMetricsForService(metrics.ServiceAPI)
	assert.NotEmpty(t, apiMetrics)

	// Find our recorded metrics in the slice
	var foundCounter, foundGauge, foundHistogram bool
	for _, m := range apiMetrics {
		if m.Name == "http_requests_total" && m.Type == metrics.MetricTypeCounter {
			foundCounter = true
		}
		if m.Name == "memory_usage_bytes" && m.Type == metrics.MetricTypeGauge {
			foundGauge = true
		}
		if m.Name == "request_duration_seconds" && m.Type == metrics.MetricTypeHistogram {
			foundHistogram = true
		}
	}

	assert.True(t, foundCounter, "Expected to find http_requests_total counter metric")
	assert.True(t, foundGauge, "Expected to find memory_usage_bytes gauge metric")
	assert.True(t, foundHistogram, "Expected to find request_duration_seconds histogram metric")

	// Verify Gas Bank service metrics exist
	gasBankMetrics := metricsService.GetMetricsForService(metrics.ServiceGasBank)
	assert.NotEmpty(t, gasBankMetrics)

	// Find our recorded Gas Bank metrics in the slice
	var foundGasBankCounter, foundGasBankGauge bool
	for _, m := range gasBankMetrics {
		if m.Name == "gas_operations_total" && m.Type == metrics.MetricTypeCounter {
			foundGasBankCounter = true
		}
		if m.Name == "gas_amount" && m.Type == metrics.MetricTypeGauge {
			foundGasBankGauge = true
		}
	}

	assert.True(t, foundGasBankCounter, "Expected to find gas_operations_total counter metric")
	assert.True(t, foundGasBankGauge, "Expected to find gas_amount gauge metric")

	// Stop the service
	err = metricsService.Stop()
	assert.NoError(t, err)

	t.Log("Metrics Service integration test completed successfully")
}
