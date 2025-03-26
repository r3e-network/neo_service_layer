# Metrics Service Documentation

## Overview

The Metrics Service is a core component of the Neo Service Layer that provides comprehensive monitoring and reporting capabilities. It collects performance and operational metrics from all services, stores them, and makes them available for analysis and visualization through various interfaces.

## Key Features

- **Real-time Metrics Collection**: Gather metrics from all platform services in real-time
- **Prometheus Integration**: Native support for Prometheus metrics format
- **Customizable Dashboards**: Web-based visualization of system metrics
- **Service Health Monitoring**: Track the operational status of all services
- **Alerting Capabilities**: Configure alerts based on metric thresholds
- **Historical Data**: Store and query historical performance data
- **Custom Metric Types**: Support for counters, gauges, histograms, and summaries

## Service Architecture

The Metrics Service consists of several components:

1. **Collector**: Gathers metrics from all services at configurable intervals
2. **Storage**: Maintains metric data with appropriate retention policies
3. **API**: Provides programmatic access to current and historical metrics
4. **Dashboard**: Web interface for visualizing metrics and trends
5. **Alerting**: Monitors metrics for threshold violations and sends notifications

## Metric Types

The service supports several types of metrics:

### Counter

Counters are cumulative metrics that can only increase in value (or be reset to zero). They are ideal for counting events or operations.

```go
// Register a counter
service.RegisterCounter("api_requests_total", "Total number of API requests", labels)

// Increment a counter
service.IncrementCounter("api_requests_total", 1)
```

### Gauge

Gauges represent values that can increase and decrease over time. They are perfect for metrics like memory usage, connection counts, or queue size.

```go
// Register a gauge
service.RegisterGauge("active_connections", "Number of active connections", labels)

// Set a gauge value
service.SetGauge("active_connections", 42)
```

### Histogram

Histograms track the distribution of a value, such as response times. They provide quantile information about the observed values.

```go
// Register a histogram with custom buckets
buckets := []float64{0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10}
service.RegisterHistogram("response_time_seconds", "Response time in seconds", buckets, labels)

// Observe a value
service.ObserveHistogram("response_time_seconds", 0.42)
```

## Service Metrics

Each service in the Neo Service Layer can record its own metrics, which are then collected and stored by the Metrics Service:

```go
// Record service-specific metrics
metrics := map[string]interface{}{
    "active_users": 120,
    "request_rate": 45.5,
    "error_count":  2,
}
tags := map[string]string{
    "environment": "production",
    "region":      "us-west",
}
metricsService.RecordServiceMetrics("api_service", metrics, tags)
```

## Dashboard

The Metrics Service includes a web-based dashboard for visualizing metrics:

![Dashboard Screenshot](../assets/metrics-dashboard.png)

Features of the dashboard include:

- Real-time updates of current metrics
- Historical trend graphs
- Customizable views per service
- Exportable data for reporting
- Authentication for secure access

## API Endpoints

The Metrics Service exposes several API endpoints for programmatic access to metrics data:

### Get All Metrics

```
GET /api/metrics
```

Retrieves metrics for all services.

**Response:**
```json
{
  "api_service": {
    "serviceName": "api_service",
    "timestamp": "2023-03-26T10:30:00Z",
    "metrics": {
      "active_users": 120,
      "request_rate": 45.5,
      "error_count": 2
    },
    "tags": {
      "environment": "production",
      "region": "us-west"
    }
  },
  "gas_bank_service": {
    "serviceName": "gas_bank_service",
    "timestamp": "2023-03-26T10:29:45Z",
    "metrics": {
      "total_gas_allocated": 15000,
      "available_gas": 8500,
      "pending_allocations": 3
    },
    "tags": {
      "environment": "production",
      "region": "us-west"
    }
  }
}
```

### Get Service Metrics

```
GET /api/metrics/{service_name}
```

Retrieves metrics for a specific service.

**Response:**
```json
{
  "serviceName": "api_service",
  "timestamp": "2023-03-26T10:30:00Z",
  "metrics": {
    "active_users": 120,
    "request_rate": 45.5,
    "error_count": 2
  },
  "tags": {
    "environment": "production",
    "region": "us-west"
  }
}
```

## Dashboard Configuration

The metrics dashboard can be configured with the following options:

```go
type Config struct {
    Port           int           // HTTP port for the dashboard
    Host           string        // Host address to bind to
    RefreshSeconds int           // Data refresh interval in seconds
    Title          string        // Dashboard title
    EnableAuth     bool          // Whether to require authentication
    Username       string        // Basic auth username
    Password       string        // Basic auth password
}
```

## Integration with Other Services

The Metrics Service integrates with all other services in the Neo Service Layer:

- **API Service**: Metrics on request rates, response times, and error rates
- **Functions Service**: Execution times, memory usage, and invocation counts
- **Gas Bank Service**: Gas allocation metrics and usage patterns
- **Price Feed Service**: Data update frequency and source reliability
- **Trigger Service**: Trigger execution counts and performance metrics

## Prometheus Integration

The Metrics Service is compatible with Prometheus, allowing you to:

1. Scrape metrics directly using Prometheus' pull model
2. Use PromQL for advanced queries
3. Integrate with the Prometheus alerting system
4. Visualize metrics in Grafana or other Prometheus-compatible dashboards

Example Prometheus configuration:

```yaml
scrape_configs:
  - job_name: 'neo_service_layer'
    scrape_interval: 15s
    static_configs:
      - targets: ['localhost:9090']
```

## Alerting

The Metrics Service can be configured to send alerts when certain conditions are met:

```go
// Example alert configuration
alertConfig := &metrics.AlertConfig{
    MetricName:    "error_rate",
    Threshold:     0.05,
    Operator:      metrics.OperatorGreaterThan,
    Duration:      5 * time.Minute,
    NotifyChannels: []string{"email", "slack"},
}
metricsService.AddAlert(alertConfig)
```

Alert notifications can be sent through multiple channels:

- Email
- Slack
- Webhook
- SMS
- Integrated with external alerting systems

## Configuration Options

The Metrics Service can be configured with the following options:

```go
type ServiceConfig struct {
    CollectionInterval time.Duration    // How often to collect metrics
    RetentionPeriod    time.Duration    // How long to keep historical data
    EnablePrometheus   bool             // Whether to expose Prometheus endpoint
    PrometheusPort     int              // Port for Prometheus metrics
    Tags               map[string]string // Default tags for all metrics
}
```

## Getting Started

### Service Initialization

```go
config := &metrics.ServiceConfig{
    CollectionInterval: 15 * time.Second,
    RetentionPeriod:    72 * time.Hour,
    EnablePrometheus:   true,
    PrometheusPort:     9090,
    Tags: map[string]string{
        "environment": "production",
        "service":     "neo_service_layer",
    },
}

logger := logrus.New()
logger.SetLevel(logrus.InfoLevel)

metricsService, err := metrics.NewService(config, logger)
if err != nil {
    log.Fatalf("Failed to create metrics service: %v", err)
}

// Start the service
ctx := context.Background()
err = metricsService.Start(ctx)
if err != nil {
    log.Fatalf("Failed to start metrics service: %v", err)
}
```

### Dashboard Initialization

```go
dashboardConfig := &dashboard.Config{
    Port:           8080,
    Host:           "0.0.0.0",
    RefreshSeconds: 10,
    Title:          "Neo Service Layer",
    EnableAuth:     true,
    Username:       "admin",
    Password:       "secure_password",
}

metricsDashboard, err := dashboard.NewDashboard(dashboardConfig, metricsService, logger)
if err != nil {
    log.Fatalf("Failed to create metrics dashboard: %v", err)
}

// Start the dashboard
go func() {
    if err := metricsDashboard.Start(); err != nil {
        log.Fatalf("Failed to start metrics dashboard: %v", err)
    }
}()
```

## Best Practices

1. **Consistent Naming**: Use a consistent naming scheme for metrics across all services
2. **Appropriate Labels**: Add relevant labels to metrics for better filtering and analysis
3. **Optimized Collection**: Balance collection frequency against performance impact
4. **Retention Policies**: Configure appropriate retention periods for different types of metrics
5. **Dashboard Organization**: Structure dashboards by service or feature for easier navigation

## Security Considerations

- Enable authentication for dashboard access in production environments
- Use TLS for all metrics communication in production
- Limit access to sensitive metrics with role-based permissions
- Regularly review and audit metrics access patterns
- Ensure alerting channels (email, Slack, etc.) are secured appropriately

## Monitoring the Metrics Service

It's important to monitor the Metrics Service itself:

- Resource usage (CPU, memory, disk)
- Collection performance
- Database size and growth
- Dashboard response times
- Alerting reliability

These metrics should be exposed like any other service metrics, allowing for meta-monitoring of the monitoring system. 