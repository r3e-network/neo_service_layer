# Neo Service Layer - Metrics Service

The Metrics Service provides a centralized system for collecting, storing, and querying metrics within the Neo Service Layer ecosystem. It enables real-time monitoring of service performance, health status, and resource usage.

## Quick Start

```go
package main

import (
    "context"
    "time"
    
    "github.com/will/neo_service_layer/internal/services/metrics"
)

func main() {
    // Create metrics service configuration
    config := &metrics.Config{
        CollectionInterval: 30 * time.Second,
        RetentionPeriod:    24 * time.Hour,
        StorageBackend:     "memory",
    }
    
    // Initialize metrics service
    metricsService := metrics.NewService(config)
    
    // Start metrics collection
    ctx := context.Background()
    metricsService.Start(ctx)
    
    // Record some metrics
    metricsService.RecordCounter("requests_total", 1, metrics.ServiceAPI, map[string]string{
        "endpoint": "/v1/users",
        "method": "GET",
    })
    
    // Get metrics
    apiMetrics := metricsService.GetMetricsForService(metrics.ServiceAPI)
    
    // Use metrics in your application...
    
    // Stop metrics service when done
    metricsService.Stop()
}
```

## Directory Structure

```
metrics/
├── metrics.go           # Core service implementation
├── collector.go         # Metric collectors
├── errors.go            # Error definitions
├── exporter.go          # Metrics export functionality
├── dashboard/           # Dashboard components
│   └── dashboards.go    # Dashboard definitions
├── docs/                # Documentation
│   └── metrics_service.md # Comprehensive service documentation
└── README.md            # This file
```

## Features

- **Multi-service metrics**: Collect metrics from all Neo Service Layer components
- **Type diversity**: Support for counters, gauges, and histograms
- **Configurable collection**: Control how often metrics are collected
- **Customizable retention**: Set how long metrics should be stored
- **Low overhead**: Optimized for minimal performance impact
- **Real-time dashboards**: Visualize metrics through pre-built dashboards

## Available Metric Types

- **Counters**: For values that only increase (e.g., request count)
- **Gauges**: For values that can go up and down (e.g., memory usage)
- **Histograms**: For measuring value distributions (e.g., response time)

## Integration with Neo Services

The Metrics Service seamlessly integrates with other Neo Service Layer components:

- **API Service**: Request counts, latency metrics, error rates
- **Gas Bank Service**: Gas allocation and consumption
- **Functions Service**: Execution time, memory usage, error rates
- **Price Feed Service**: Update frequency, latency, source diversity
- **Trigger Service**: Activation counts, execution time, success rates

## Dashboard Access

The Metrics Service provides pre-configured dashboards accessible through:

1. **Web Interface**: `http://localhost:8080/metrics/dashboard` (when enabled)
2. **API Endpoint**: `GET /api/v1/metrics/dashboard`

## Documentation

For comprehensive documentation, please see:

- [Metrics Service Documentation](./docs/metrics_service.md)
- [API Documentation](../../api/docs/metrics_api.md)
- [Dashboard Documentation](./dashboard/README.md)

## Contributing

When contributing to the Metrics Service:

1. Follow standard Go code conventions
2. Add appropriate tests for new metrics or collectors
3. Update documentation when changing public interfaces
4. Optimize for performance when dealing with high-volume metrics