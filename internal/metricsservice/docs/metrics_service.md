# Metrics Service

## Overview

The Metrics Service provides a comprehensive solution for collecting, storing, and analyzing metrics from the Neo Service Layer. It enables real-time monitoring, alerting, and visualization of system performance and health indicators.

## Features

- **Service-specific metrics collection**: Capture metrics from various services (API, Gas Bank, Functions, etc.)
- **Multiple metric types**: Support for counters, gauges, and histograms
- **In-memory storage**: Fast access to recent metrics
- **Configurable retention**: Control how long metrics are stored
- **Extensible collector system**: Add custom collectors for specific metrics

## Architecture

The Metrics Service consists of several key components:

1. **Core Service**: Central metrics management and orchestration
2. **Collectors**: Components that gather metrics from specific sources
3. **Storage**: In-memory data structure for metrics
4. **Dashboard**: Visual representation of metrics (optional)

### Service Components

```
metrics/
├── metrics.go           # Core service implementation
├── collector.go         # Metric collectors
├── dashboard/           # Dashboard components
│   └── dashboards.go    # Dashboard definitions
├── exporter.go          # Metrics export functionality
└── errors.go            # Error definitions
```

## Configuration

The Metrics Service is configured through a `Config` struct:

```go
type Config struct {
    CollectionInterval time.Duration  // How often metrics are collected
    RetentionPeriod    time.Duration  // How long metrics are retained
    StorageBackend     string         // Storage backend ("memory", etc.)
    StorageConfig      map[string]string // Backend-specific configuration
}
```

## Metric Types

### Counter

A cumulative metric that only increases in value (e.g., request count, error count).

### Gauge

A metric that can go up or down (e.g., memory usage, connection count).

### Histogram

A metric that tracks the distribution of values (e.g., request duration).

## Service Integration

### Initializing the Metrics Service

```go
// Create configuration
config := &metrics.Config{
    CollectionInterval: 60 * time.Second,
    RetentionPeriod:    24 * time.Hour,
    StorageBackend:     "memory",
    StorageConfig:      make(map[string]string),
}

// Initialize service
metricsService := metrics.NewService(config)

// Start the service
ctx := context.Background()
metricsService.Start(ctx)
```

### Recording Metrics

```go
// Record a counter
metricsService.RecordCounter(
    "http_requests_total", 
    1, 
    metrics.ServiceAPI, 
    map[string]string{"endpoint": "/users", "method": "GET"}
)

// Record a gauge
metricsService.RecordGauge(
    "memory_usage_bytes", 
    104857600, 
    metrics.ServiceAPI, 
    map[string]string{"instance": "server-1"}
)

// Record a histogram
metricsService.RecordHistogram(
    "request_duration_seconds",
    0.42,
    metrics.ServiceAPI,
    "duration",
    []float64{0.01, 0.05, 0.1, 0.5, 1, 5},
    map[string]string{"endpoint": "/users"}
)
```

### Retrieving Metrics

```go
// Get a counter value
value, exists := metricsService.GetCounterValue(
    "http_requests_total", 
    metrics.ServiceAPI, 
    map[string]string{"endpoint": "/users", "method": "GET"}
)

// Get a gauge value
value, exists := metricsService.GetGaugeValue(
    "memory_usage_bytes", 
    metrics.ServiceAPI, 
    map[string]string{"instance": "server-1"}
)

// Get service-specific metrics
apiMetrics := metricsService.GetMetricsForService(metrics.ServiceAPI)

// Get all metrics
allMetrics := metricsService.GetAllMetrics()
```

## Metrics Collection

The service automatically collects system metrics, including:

- Memory usage (heap, total allocation, system)
- Goroutine count
- CPU usage

Service-specific metric collectors are registered for different components:

```go
// Create and register a collector
collector := metrics.NewServiceCollector(
    interval, 
    metricsService, 
    []string{"api", "function_calls"}, 
    metrics.ServiceAPI
)

// Register the collector
metricsService.RegisterCollector(metrics.ServiceAPI, collector)
```

## Dashboards

The Metrics Service includes a dashboard component that provides pre-configured dashboards for visualizing metrics:

- **System Overview Dashboard**: High-level system health and performance
- **API Service Dashboard**: Detailed API metrics
- **Gas Bank Dashboard**: Gas allocation and usage metrics

## Best Practices

1. **Use consistent naming**: Follow a pattern like `service_subsystem_metric_unit`
2. **Add descriptive labels**: Include relevant dimensions but avoid high cardinality
3. **Choose appropriate metric types**: Use counters for events, gauges for values that can change, histograms for distributions
4. **Set reasonable retention**: Balance history needs with memory consumption

## Monitoring Recommendations

- Monitor system resource usage (memory, CPU)
- Track service response times and error rates
- Set up alerts for abnormal metric values
- Regularly review dashboard for trends

## Integration with Other Services

The Metrics Service integrates with:

- **API Service**: Records request count, latency, and error metrics
- **Gas Bank Service**: Tracks gas allocation and consumption
- **Functions Service**: Monitors function execution and performance
- **Price Feed Service**: Records price update frequency and latency
- **Trigger Service**: Tracks trigger activations and performance

## Troubleshooting

Common issues:

1. **High memory usage**: Reduce retention period or collection frequency
2. **Missing metrics**: Check if services are properly recording metrics
3. **Performance impact**: Adjust collection interval for less frequent sampling

## Future Enhancements

- Prometheus integration
- Persistent storage options
- Grafana dashboard templates
- Distributed metrics collection 