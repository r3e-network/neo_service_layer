# Metrics Service Examples

## Overview
These examples demonstrate how to use the Metrics service to track and monitor various aspects of your Neo Service Layer application.

## Basic Metrics Collection

### Function Performance Metrics
```typescript
import { Handler } from '@netlify/functions';
import { MetricsService } from '../services/metrics';

const metrics = new MetricsService({
  namespace: 'neo_service_layer',
  subsystem: 'functions'
});

export const handler: Handler = async (event, context) => {
  // Start timing the function execution
  const timer = metrics.startTimer('function_execution_duration_seconds', {
    function_name: 'get_price_feeds'
  });

  try {
    // Increment request counter
    metrics.incrementCounter('function_requests_total', {
      function_name: 'get_price_feeds',
      method: event.httpMethod
    });

    // Your function logic here
    const result = await processRequest();

    // Record successful execution
    metrics.incrementCounter('function_success_total', {
      function_name: 'get_price_feeds'
    });

    return {
      statusCode: 200,
      body: JSON.stringify(result)
    };
  } catch (error) {
    // Record error
    metrics.incrementCounter('function_errors_total', {
      function_name: 'get_price_feeds',
      error_type: error.name
    });

    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'Internal server error' })
    };
  } finally {
    // Stop and record the timer
    timer.end();
  }
};
```

### Gas Usage Metrics
```typescript
import { MetricsService } from '../services/metrics';
import { GasBank } from '../services/gas-bank';

const metrics = new MetricsService({
  namespace: 'neo_service_layer',
  subsystem: 'gas_bank'
});

class GasBankWithMetrics extends GasBank {
  async requestGas(amount: number, address: string) {
    const timer = metrics.startTimer('gas_request_duration_seconds');

    try {
      // Record gas request amount
      metrics.recordGauge('gas_request_amount', amount, {
        address
      });

      const result = await super.requestGas(amount, address);

      // Record successful gas transfer
      metrics.incrementCounter('gas_transfers_total', {
        status: 'success',
        address
      });

      metrics.recordGauge('gas_balance', await this.getBalance(address), {
        address
      });

      return result;
    } catch (error) {
      // Record failed gas transfer
      metrics.incrementCounter('gas_transfers_total', {
        status: 'error',
        error_type: error.name,
        address
      });

      throw error;
    } finally {
      timer.end();
    }
  }
}
```

## Advanced Usage

### Custom Metrics Collection
```typescript
import { MetricsService } from '../services/metrics';

class CustomMetricsCollector {
  private metrics: MetricsService;

  constructor() {
    this.metrics = new MetricsService({
      namespace: 'neo_service_layer',
      subsystem: 'custom'
    });

    // Initialize custom metrics
    this.metrics.createGauge('system_memory_usage_bytes');
    this.metrics.createHistogram('request_payload_size_bytes', {
      buckets: [1024, 10240, 102400, 1048576] // 1KB, 10KB, 100KB, 1MB
    });
    this.metrics.createSummary('request_latency_seconds', {
      quantiles: [0.5, 0.9, 0.99]
    });
  }

  recordMemoryUsage() {
    const usage = process.memoryUsage();
    this.metrics.recordGauge('system_memory_usage_bytes', usage.heapUsed, {
      type: 'heap'
    });
    this.metrics.recordGauge('system_memory_usage_bytes', usage.heapTotal, {
      type: 'heap_total'
    });
    this.metrics.recordGauge('system_memory_usage_bytes', usage.rss, {
      type: 'rss'
    });
  }

  recordPayloadSize(size: number, endpoint: string) {
    this.metrics.recordHistogram('request_payload_size_bytes', size, {
      endpoint
    });
  }

  recordLatency(durationMs: number, endpoint: string) {
    this.metrics.recordSummary('request_latency_seconds', durationMs / 1000, {
      endpoint
    });
  }
}
```

### Metrics Aggregation
```typescript
import { MetricsService } from '../services/metrics';
import { MetricsAggregator } from '../services/metrics-aggregator';

class ServiceMetricsAggregator {
  private metrics: MetricsService;
  private aggregator: MetricsAggregator;

  constructor() {
    this.metrics = new MetricsService({
      namespace: 'neo_service_layer',
      subsystem: 'aggregated'
    });

    this.aggregator = new MetricsAggregator({
      interval: 60000, // Aggregate every minute
      metrics: this.metrics
    });
  }

  startAggregation() {
    // Aggregate function execution times
    this.aggregator.addAggregation({
      sourceMetric: 'function_execution_duration_seconds',
      aggregations: [
        {
          type: 'average',
          name: 'function_execution_duration_average',
          labels: ['function_name']
        },
        {
          type: 'max',
          name: 'function_execution_duration_max',
          labels: ['function_name']
        }
      ]
    });

    // Aggregate error rates
    this.aggregator.addAggregation({
      sourceMetric: 'function_errors_total',
      aggregations: [
        {
          type: 'rate',
          name: 'function_error_rate',
          labels: ['function_name'],
          windowSeconds: 300 // 5-minute window
        }
      ]
    });

    // Start aggregation
    this.aggregator.start();
  }

  stopAggregation() {
    this.aggregator.stop();
  }
}
```

### Metrics Export and Dashboard Integration
```typescript
import { MetricsService } from '../services/metrics';
import { PrometheusExporter } from '../services/metrics-exporters';

class MetricsExporter {
  private metrics: MetricsService;
  private exporter: PrometheusExporter;

  constructor() {
    this.metrics = new MetricsService({
      namespace: 'neo_service_layer'
    });

    this.exporter = new PrometheusExporter({
      port: 9090,
      endpoint: '/metrics'
    });
  }

  async startExporter() {
    // Configure metric formatting
    this.exporter.addFormatter({
      metric: 'function_execution_duration_seconds',
      type: 'histogram',
      help: 'Duration of function execution in seconds'
    });

    this.exporter.addFormatter({
      metric: 'gas_transfers_total',
      type: 'counter',
      help: 'Total number of gas transfers'
    });

    // Start the exporter
    await this.exporter.start();

    // Register shutdown handler
    process.on('SIGTERM', () => {
      this.exporter.stop();
    });
  }

  // Generate Grafana dashboard configuration
  generateDashboardConfig() {
    return {
      dashboard: {
        title: 'Neo Service Layer Metrics',
        panels: [
          {
            title: 'Function Execution Duration',
            type: 'graph',
            metrics: [
              'rate(function_execution_duration_seconds_sum[5m]) / ' +
              'rate(function_execution_duration_seconds_count[5m])'
            ]
          },
          {
            title: 'Gas Transfer Volume',
            type: 'graph',
            metrics: [
              'sum(rate(gas_transfers_total{status="success"}[5m])) by (address)'
            ]
          },
          {
            title: 'Error Rate',
            type: 'graph',
            metrics: [
              'rate(function_errors_total[5m])'
            ]
          }
        ]
      }
    };
  }
}
```

## Testing Examples

### Unit Testing Metrics Collection
```typescript
import { MetricsService } from '../services/metrics';

describe('MetricsService', () => {
  let metrics: MetricsService;

  beforeEach(() => {
    metrics = new MetricsService({
      namespace: 'test',
      subsystem: 'unit'
    });
  });

  it('records counter metrics correctly', () => {
    const counterName = 'test_counter_total';
    metrics.createCounter(counterName);

    metrics.incrementCounter(counterName, { label: 'test' });
    
    const value = metrics.getMetricValue(counterName, { label: 'test' });
    expect(value).toBe(1);
  });

  it('records gauge metrics correctly', () => {
    const gaugeName = 'test_gauge';
    metrics.createGauge(gaugeName);

    metrics.recordGauge(gaugeName, 42, { label: 'test' });
    
    const value = metrics.getMetricValue(gaugeName, { label: 'test' });
    expect(value).toBe(42);
  });

  it('records histogram metrics correctly', () => {
    const histogramName = 'test_histogram';
    metrics.createHistogram(histogramName, {
      buckets: [1, 5, 10]
    });

    metrics.recordHistogram(histogramName, 7, { label: 'test' });
    
    const histogram = metrics.getHistogram(histogramName, { label: 'test' });
    expect(histogram.sum).toBe(7);
    expect(histogram.count).toBe(1);
  });
});
```

### Integration Testing with Metrics
```typescript
import { MetricsService } from '../services/metrics';
import { handler } from './function-with-metrics';

describe('Function with metrics integration', () => {
  let metrics: MetricsService;

  beforeEach(() => {
    metrics = new MetricsService({
      namespace: 'test',
      subsystem: 'integration'
    });
  });

  it('records function execution metrics', async () => {
    const response = await handler(
      {
        httpMethod: 'GET',
        path: '/test',
        headers: {},
        body: null
      } as any,
      {} as any
    );

    // Verify request counter
    const requestCount = metrics.getMetricValue('function_requests_total', {
      function_name: 'test_function',
      method: 'GET'
    });
    expect(requestCount).toBe(1);

    // Verify execution duration was recorded
    const duration = metrics.getMetricValue(
      'function_execution_duration_seconds',
      { function_name: 'test_function' }
    );
    expect(duration).toBeGreaterThan(0);

    // Verify response status was recorded
    const successCount = metrics.getMetricValue('function_success_total', {
      function_name: 'test_function'
    });
    expect(successCount).toBe(1);
  });

  it('records error metrics on failure', async () => {
    // Force an error condition
    const response = await handler(
      {
        httpMethod: 'GET',
        path: '/error',
        headers: {},
        body: null
      } as any,
      {} as any
    );

    // Verify error counter
    const errorCount = metrics.getMetricValue('function_errors_total', {
      function_name: 'test_function',
      error_type: 'Error'
    });
    expect(errorCount).toBe(1);
  });
});
```