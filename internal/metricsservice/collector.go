package metrics

import (
	"context"
	"fmt"
	"runtime"
	"sync"
	"time"
)

// MetricCollector interface REMOVED
/*
type MetricCollector interface {
	CollectMetrics() (map[string]interface{}, error)
	Start() error
	Stop() error
}
*/

// SystemCollector collects system metrics and implements the Collector interface from metrics.go
type SystemCollector struct {
	interval     time.Duration
	stopChan     chan struct{}
	lastMetrics  map[string]interface{}
	mutex        sync.RWMutex
	metricsStore *Service
}

// NewSystemCollector creates a new system collector
func NewSystemCollector(interval time.Duration, metricsStore *Service) *SystemCollector {
	return &SystemCollector{
		interval:     interval,
		stopChan:     make(chan struct{}),
		lastMetrics:  make(map[string]interface{}),
		metricsStore: metricsStore,
	}
}

// Collect implements the Collector interface
func (c *SystemCollector) Collect(ctx context.Context) []Metric {
	collectedMetrics := []Metric{}

	// Collect memory stats
	var memStats runtime.MemStats
	runtime.ReadMemStats(&memStats)

	// Convert to Metric struct
	collectedMetrics = append(collectedMetrics, Metric{
		Name: "system_memory_alloc", Type: MetricTypeGauge,
		Value: float64(memStats.Alloc), Service: ServiceAPI, // Assign Service type here or rely on collector registration
		Labels: map[string]string{"collector": "system"},
	})
	collectedMetrics = append(collectedMetrics, Metric{
		Name: "system_memory_total_alloc", Type: MetricTypeCounter, // TotalAlloc is cumulative
		Value: float64(memStats.TotalAlloc), Service: ServiceAPI,
		Labels: map[string]string{"collector": "system"},
	})
	collectedMetrics = append(collectedMetrics, Metric{
		Name: "system_memory_sys", Type: MetricTypeGauge,
		Value: float64(memStats.Sys), Service: ServiceAPI,
		Labels: map[string]string{"collector": "system"},
	})
	collectedMetrics = append(collectedMetrics, Metric{
		Name: "system_memory_num_gc", Type: MetricTypeCounter, // NumGC is cumulative
		Value: float64(memStats.NumGC), Service: ServiceAPI,
		Labels: map[string]string{"collector": "system"},
	})
	collectedMetrics = append(collectedMetrics, Metric{
		Name: "system_goroutines", Type: MetricTypeGauge,
		Value: float64(runtime.NumGoroutine()), Service: ServiceAPI,
		Labels: map[string]string{"collector": "system"},
	})

	// Mock CPU usage
	collectedMetrics = append(collectedMetrics, Metric{
		Name: "system_cpu_usage", Type: MetricTypeGauge,
		Value: 50.0, Service: ServiceAPI,
		Labels: map[string]string{"collector": "system"},
	})

	// Update internal cache (optional, maybe remove CollectMetrics method later)
	metricsMap := make(map[string]interface{})
	for _, m := range collectedMetrics {
		metricsMap[m.Name] = m.Value
	}
	c.mutex.Lock()
	c.lastMetrics = metricsMap
	c.mutex.Unlock()

	return collectedMetrics
}

// CollectMetrics method might become redundant if only Collect interface is used by the service.
// Keeping it for now in case it's used elsewhere.
func (c *SystemCollector) CollectMetrics() (map[string]interface{}, error) {
	metrics := c.Collect(context.Background()) // Call the interface method
	metricsMap := make(map[string]interface{})
	for _, m := range metrics {
		metricsMap[m.Name] = m.Value
	}
	return metricsMap, nil
}

// Start starts the system collector
func (c *SystemCollector) Start() error {
	go c.collectLoop()
	return nil
}

// Stop stops the system collector
func (c *SystemCollector) Stop() error {
	close(c.stopChan)
	return nil
}

// collectLoop periodically collects metrics
func (c *SystemCollector) collectLoop() {
	ticker := time.NewTicker(c.interval)
	defer ticker.Stop()
	ctx := context.Background() // Create a background context
	for {
		select {
		case <-ticker.C:
			// Directly call the Collect method which now returns []Metric
			metrics := c.Collect(ctx)
			// Push metrics to the central service store
			for _, m := range metrics {
				c.metricsStore.storeMetric(m) // Use the reference
			}
		case <-c.stopChan:
			return
		}
	}
}

// Neo3Collector collects Neo blockchain metrics and implements Collector
type Neo3Collector struct {
	interval     time.Duration
	stopChan     chan struct{}
	lastMetrics  map[string]interface{}
	mutex        sync.RWMutex
	metricsStore *Service
}

// NewNeo3Collector creates a new Neo blockchain collector
func NewNeo3Collector(interval time.Duration, metricsStore *Service) *Neo3Collector {
	return &Neo3Collector{
		interval:     interval,
		stopChan:     make(chan struct{}),
		lastMetrics:  make(map[string]interface{}),
		metricsStore: metricsStore,
	}
}

// Collect implements the Collector interface
func (c *Neo3Collector) Collect(ctx context.Context) []Metric {
	collectedMetrics := []Metric{}

	// These are mock implementations
	collectedMetrics = append(collectedMetrics, Metric{
		Name: "neo_blocks_processed", Type: MetricTypeCounter,
		Value: 12345, Service: ServiceAPI, // TODO: Define a Neo service type?
		Labels: map[string]string{"collector": "neo3"},
	})
	collectedMetrics = append(collectedMetrics, Metric{
		Name: "neo_transactions_processed", Type: MetricTypeCounter,
		Value: 67890, Service: ServiceAPI,
		Labels: map[string]string{"collector": "neo3"},
	})
	collectedMetrics = append(collectedMetrics, Metric{
		Name: "neo_gas_used", Type: MetricTypeGauge,
		Value: 123.45, Service: ServiceAPI,
		Labels: map[string]string{"collector": "neo3"},
	})
	collectedMetrics = append(collectedMetrics, Metric{
		Name: "neo_contract_calls", Type: MetricTypeCounter,
		Value: 789, Service: ServiceAPI,
		Labels: map[string]string{"collector": "neo3"},
	})

	// Update internal cache (optional)
	metricsMap := make(map[string]interface{})
	for _, m := range collectedMetrics {
		metricsMap[m.Name] = m.Value
	}
	c.mutex.Lock()
	c.lastMetrics = metricsMap
	c.mutex.Unlock()

	return collectedMetrics
}

// CollectMetrics method might become redundant
func (c *Neo3Collector) CollectMetrics() (map[string]interface{}, error) {
	metrics := c.Collect(context.Background())
	metricsMap := make(map[string]interface{})
	for _, m := range metrics {
		metricsMap[m.Name] = m.Value
	}
	return metricsMap, nil
}

// Start starts the Neo blockchain collector
func (c *Neo3Collector) Start() error {
	go c.collectLoop()
	return nil
}

// Stop stops the Neo blockchain collector
func (c *Neo3Collector) Stop() error {
	close(c.stopChan)
	return nil
}

// collectLoop periodically collects metrics
func (c *Neo3Collector) collectLoop() {
	ticker := time.NewTicker(c.interval)
	defer ticker.Stop()
	ctx := context.Background()
	for {
		select {
		case <-ticker.C:
			metrics := c.Collect(ctx)
			for _, m := range metrics {
				c.metricsStore.storeMetric(m)
			}
		case <-c.stopChan:
			return
		}
	}
}

// ServiceCollector collects service-specific metrics and implements Collector
type ServiceCollector struct {
	interval     time.Duration
	stopChan     chan struct{}
	lastMetrics  map[string]interface{}
	mutex        sync.RWMutex
	metricsStore *Service
	services     []string
	serviceType  ServiceType
}

// NewServiceCollector creates a new service collector
func NewServiceCollector(interval time.Duration, metricsStore *Service, services []string, serviceType ServiceType) *ServiceCollector {
	return &ServiceCollector{
		interval:     interval,
		stopChan:     make(chan struct{}),
		lastMetrics:  make(map[string]interface{}),
		metricsStore: metricsStore,
		services:     services,
		serviceType:  serviceType,
	}
}

// Collect implements the Collector interface
func (c *ServiceCollector) Collect(ctx context.Context) []Metric {
	collectedMetrics := []Metric{}

	// This is a mock implementation
	for _, service := range c.services {
		// Service uptime
		collectedMetrics = append(collectedMetrics, Metric{
			Name: fmt.Sprintf("%s_uptime", service), Type: MetricTypeGauge,
			Value: float64(time.Now().Unix() - 1640995200), Service: c.serviceType,
			Labels: map[string]string{"collector": "service"},
		})
		// Service requests
		collectedMetrics = append(collectedMetrics, Metric{
			Name: fmt.Sprintf("%s_requests_total", service), Type: MetricTypeCounter,
			Value: 1000, Service: c.serviceType,
			Labels: map[string]string{"collector": "service"},
		})
		// Service errors
		collectedMetrics = append(collectedMetrics, Metric{
			Name: fmt.Sprintf("%s_errors_total", service), Type: MetricTypeCounter,
			Value: 10, Service: c.serviceType,
			Labels: map[string]string{"collector": "service"},
		})
		// Service latency
		collectedMetrics = append(collectedMetrics, Metric{
			Name: fmt.Sprintf("%s_latency", service), Type: MetricTypeGauge,
			Value: 0.123, Service: c.serviceType,
			Labels: map[string]string{"collector": "service"},
		})
	}

	// Update internal cache (optional)
	metricsMap := make(map[string]interface{})
	for _, m := range collectedMetrics {
		metricsMap[m.Name] = m.Value
	}
	c.mutex.Lock()
	c.lastMetrics = metricsMap
	c.mutex.Unlock()

	return collectedMetrics
}

// CollectMetrics method might become redundant
func (c *ServiceCollector) CollectMetrics() (map[string]interface{}, error) {
	metrics := c.Collect(context.Background())
	metricsMap := make(map[string]interface{})
	for _, m := range metrics {
		metricsMap[m.Name] = m.Value
	}
	return metricsMap, nil
}

// Start starts the service collector
func (c *ServiceCollector) Start() error {
	go c.collectLoop()
	return nil
}

// Stop stops the service collector
func (c *ServiceCollector) Stop() error {
	close(c.stopChan)
	return nil
}

// collectLoop periodically collects metrics
func (c *ServiceCollector) collectLoop() {
	ticker := time.NewTicker(c.interval)
	defer ticker.Stop()
	ctx := context.Background()
	for {
		select {
		case <-ticker.C:
			metrics := c.Collect(ctx)
			for _, m := range metrics {
				c.metricsStore.storeMetric(m)
			}
		case <-c.stopChan:
			return
		}
	}
}

// RecordAPIServiceMetrics records metrics for the API service
func RecordAPIServiceMetrics(ctx context.Context, metricsService *Service, requestDuration float64, endpoint, method, statusCode string) {
	// Record endpoint request count
	metricsService.RecordCounter(
		"api_requests_total",
		1,
		ServiceAPI,
		map[string]string{
			"endpoint": endpoint,
			"method":   method,
			"status":   statusCode,
		},
	)

	// Record request duration
	metricsService.RecordHistogram(
		"api_request_duration_seconds",
		requestDuration,
		ServiceAPI,
		"duration",
		[]float64{0.01, 0.05, 0.1, 0.5, 1, 2, 5, 10},
		map[string]string{
			"endpoint": endpoint,
			"method":   method,
		},
	)

	// Record request size if available (example values)
	metricsService.RecordGauge(
		"api_request_size_bytes",
		1024, // Example value
		ServiceAPI,
		map[string]string{
			"endpoint": endpoint,
			"method":   method,
		},
	)
}

// RecordGasBankServiceMetrics records metrics for the Gas Bank service
func RecordGasBankServiceMetrics(ctx context.Context, metricsService *Service, operation string, gasAmount float64, userAddress string) {
	// Record gas operation count
	metricsService.RecordCounter(
		"gas_operations_total",
		1,
		ServiceGasBank,
		map[string]string{
			"operation": operation,
			"user":      userAddress,
		},
	)

	// Record gas amount
	metricsService.RecordGauge(
		"gas_amount",
		gasAmount,
		ServiceGasBank,
		map[string]string{
			"operation": operation,
			"user":      userAddress,
		},
	)

	// Record gas distribution histogram
	metricsService.RecordHistogram(
		"gas_distribution",
		gasAmount,
		ServiceGasBank,
		"amount",
		[]float64{10, 50, 100, 500, 1000, 5000, 10000},
		map[string]string{
			"operation": operation,
		},
	)
}

// RecordfunctionserviceMetrics records metrics for the Functions service
func RecordfunctionserviceMetrics(ctx context.Context, metricsService *Service, executionTime float64, functionID, status string) {
	// Record function execution count
	metricsService.RecordCounter(
		"function_executions_total",
		1,
		ServiceFunctions,
		map[string]string{
			"function_id": functionID,
			"status":      status,
		},
	)

	// Record function execution time
	metricsService.RecordHistogram(
		"function_execution_time_seconds",
		executionTime,
		ServiceFunctions,
		"duration",
		[]float64{0.001, 0.01, 0.1, 0.5, 1, 5, 10, 60},
		map[string]string{
			"function_id": functionID,
			"status":      status,
		},
	)

	// Record function memory usage (example value)
	metricsService.RecordGauge(
		"function_memory_usage_bytes",
		104857600, // 100 MB example
		ServiceFunctions,
		map[string]string{
			"function_id": functionID,
		},
	)
}
