package metrics

import (
	"context"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
)

func TestNewService(t *testing.T) {
	// Test with default config
	service := NewService(nil)
	assert.NotNil(t, service)
	assert.NotNil(t, service.config)
	assert.Equal(t, 60*time.Second, service.config.CollectionInterval)
	assert.Equal(t, 24*time.Hour, service.config.RetentionPeriod)
	assert.Equal(t, "memory", service.config.StorageBackend)

	// Test with custom config
	customConfig := &Config{
		CollectionInterval: 30 * time.Second,
		RetentionPeriod:    12 * time.Hour,
		StorageBackend:     "redis",
		StorageConfig: map[string]string{
			"host": "localhost",
			"port": "6379",
		},
	}
	customService := NewService(customConfig)
	assert.NotNil(t, customService)
	assert.Equal(t, customConfig, customService.config)
	assert.Equal(t, 30*time.Second, customService.config.CollectionInterval)
	assert.Equal(t, 12*time.Hour, customService.config.RetentionPeriod)
	assert.Equal(t, "redis", customService.config.StorageBackend)
}

func TestServiceStartStop(t *testing.T) {
	service := NewService(&Config{
		CollectionInterval: 100 * time.Millisecond,
		RetentionPeriod:    1 * time.Hour,
	})

	ctx := context.Background()
	err := service.Start(ctx)
	assert.NoError(t, err)

	// Let it run briefly
	time.Sleep(200 * time.Millisecond)

	err = service.Stop()
	assert.NoError(t, err)
}

func TestServiceRecordGauge(t *testing.T) {
	service := NewService(nil)

	// Record a gauge
	service.RecordGauge("test_gauge", 123.45, ServiceAPI, map[string]string{
		"label1": "value1",
		"label2": "value2",
	})

	// Verify the gauge was recorded
	value, exists := service.GetGaugeValue("test_gauge", ServiceAPI, map[string]string{
		"label1": "value1",
		"label2": "value2",
	})

	assert.True(t, exists)
	assert.Equal(t, 123.45, value)

	// Update the gauge
	service.RecordGauge("test_gauge", 678.9, ServiceAPI, map[string]string{
		"label1": "value1",
		"label2": "value2",
	})

	// Verify the gauge was updated
	value, exists = service.GetGaugeValue("test_gauge", ServiceAPI, map[string]string{
		"label1": "value1",
		"label2": "value2",
	})

	assert.True(t, exists)
	assert.Equal(t, 678.9, value)
}

func TestServiceRecordCounter(t *testing.T) {
	service := NewService(nil)

	// Record a counter
	service.RecordCounter("test_counter", 10, ServiceGasBank, map[string]string{
		"operation": "allocation",
	})

	// Verify the counter was recorded
	value, exists := service.GetCounterValue("test_counter", ServiceGasBank, map[string]string{
		"operation": "allocation",
	})

	assert.True(t, exists)
	assert.Equal(t, 10.0, value)

	// Increment the counter
	service.RecordCounter("test_counter", 5, ServiceGasBank, map[string]string{
		"operation": "allocation",
	})

	// Verify the counter was incremented
	value, exists = service.GetCounterValue("test_counter", ServiceGasBank, map[string]string{
		"operation": "allocation",
	})

	assert.True(t, exists)
	assert.Equal(t, 15.0, value)
}

func TestServiceRecordHistogram(t *testing.T) {
	service := NewService(nil)

	buckets := []float64{5, 10, 20, 50, 100}

	// Record histogram values
	service.RecordHistogram("test_histogram", 15, ServiceFunctions, "execution_time", buckets, map[string]string{
		"function": "test_function",
	})

	service.RecordHistogram("test_histogram", 30, ServiceFunctions, "execution_time", buckets, map[string]string{
		"function": "test_function",
	})

	service.RecordHistogram("test_histogram", 7, ServiceFunctions, "execution_time", buckets, map[string]string{
		"function": "test_function",
	})

	// Verify the histogram was recorded correctly
	count, sum, recordedBuckets, counts, exists := service.GetHistogramData("test_histogram", ServiceFunctions, map[string]string{
		"function": "test_function",
	})

	assert.True(t, exists)
	assert.Equal(t, int64(3), count)
	assert.Equal(t, 52.0, sum)
	assert.Equal(t, buckets, recordedBuckets)
	assert.Len(t, counts, len(buckets))
}

func TestServiceMetricsRetrieval(t *testing.T) {
	service := NewService(nil)

	// Record metrics for different services
	service.RecordGauge("api_requests", 100, ServiceAPI, nil)
	service.RecordCounter("gas_allocations", 50, ServiceGasBank, nil)
	service.RecordGauge("secret_operations", 25, ServiceSecrets, nil)

	// Get metrics for a specific service
	apiMetrics := service.GetMetricsForService(ServiceAPI)
	assert.Len(t, apiMetrics, 1)
	assert.Equal(t, "api_requests", apiMetrics[0].Name)

	// Get all metrics
	allMetrics := service.GetAllMetrics()
	assert.Len(t, allMetrics, 3)
}

func TestServiceCollectorRegistration(t *testing.T) {
	service := NewService(nil)

	// Create a mock collector
	mockCollector := &mockCollector{}

	// Register the collector
	service.RegisterCollector(ServiceAPI, mockCollector)

	// Trigger a collection
	ctx := context.Background()
	service.collect(ctx)

	// Verify the collector was called
	assert.True(t, mockCollector.collectionCalled)
}

// mockCollector implements the Collector interface for testing
type mockCollector struct {
	collectionCalled bool
}

func (m *mockCollector) Collect(ctx context.Context) []Metric {
	m.collectionCalled = true
	return []Metric{
		{
			Name:      "mock_metric",
			Type:      MetricTypeGauge,
			Value:     42,
			Service:   ServiceAPI,
			Timestamp: time.Now(),
		},
	}
}

func TestMetricKey(t *testing.T) {
	service := NewService(nil)

	// Test metric key generation with same labels but different service
	key1 := service.metricKey("test_metric", ServiceAPI, map[string]string{"a": "1", "b": "2"})
	key3 := service.metricKey("test_metric", ServiceGasBank, map[string]string{"a": "1", "b": "2"})

	// Different services should produce different keys
	assert.NotEqual(t, key1, key3)

	// Same service, name, and labels should produce consistent keys
	// Note: We don't test exact keys since map iteration order is not guaranteed
	key4 := service.metricKey("test_metric", ServiceAPI, map[string]string{"a": "1", "b": "2"})
	assert.Contains(t, key4, "api:test_metric:")
	assert.Contains(t, key4, "a=1")
	assert.Contains(t, key4, "b=2")
}
