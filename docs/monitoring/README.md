# Neo N3 Service Layer Metrics and Monitoring Guide

## Overview

The Neo N3 Service Layer provides comprehensive metrics and monitoring capabilities to help you understand the performance, health, and usage patterns of your serverless functions, triggers, and gas allocations.

## Metrics Collection

### Available Metrics

1. **Function Metrics**
   - Execution count
   - Execution duration
   - Success/failure rate
   - Memory usage
   - CPU usage
   - Network I/O
   - Error rate
   - Cold starts

2. **Trigger Metrics**
   - Trigger count
   - Execution success rate
   - Average latency
   - Missed executions
   - Error rate

3. **Gas Metrics**
   - Gas usage per function
   - Gas allocation rate
   - Gas refill rate
   - Gas pool balance
   - Failed allocations

4. **System Metrics**
   - API latency
   - Request rate
   - Error rate
   - Resource utilization
   - Network performance

### Metric Tags

All metrics include the following tags for better filtering and aggregation:

```
function_id: "func_123"
trigger_id: "trig_123"
runtime: "javascript"
environment: "production"
region: "us-east-1"
user_id: "user_123"
```

## Monitoring Setup

### 1. Prometheus Integration

```yaml
# prometheus.yaml
scrape_configs:
  - job_name: 'neo-service'
    scrape_interval: 15s
    static_configs:
      - targets: ['api.neo-service-layer.io:9090']
    basic_auth:
      username: 'prometheus'
      password: 'your_metrics_token'
```

Example metrics:
```
# HELP neo_function_executions_total Total number of function executions
# TYPE neo_function_executions_total counter
neo_function_executions_total{function_id="func_123",runtime="javascript"} 1234

# HELP neo_function_execution_duration_seconds Function execution duration
# TYPE neo_function_execution_duration_seconds histogram
neo_function_execution_duration_seconds_bucket{function_id="func_123",le="0.1"} 123
neo_function_execution_duration_seconds_bucket{function_id="func_123",le="0.5"} 456
neo_function_execution_duration_seconds_bucket{function_id="func_123",le="1.0"} 789
```

### 2. Grafana Dashboard

Import our pre-built Grafana dashboards:

1. Function Performance Dashboard (ID: 12345)
   ```bash
   grafana-cli dashboard install neo-service-functions
   ```

2. Trigger Monitoring Dashboard (ID: 12346)
   ```bash
   grafana-cli dashboard install neo-service-triggers
   ```

3. Gas Usage Dashboard (ID: 12347)
   ```bash
   grafana-cli dashboard install neo-service-gas
   ```

### 3. Alerting Rules

```yaml
# alerting.yaml
groups:
  - name: neo-service
    rules:
      - alert: HighFunctionErrorRate
        expr: rate(neo_function_errors_total[5m]) > 0.1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: High error rate for function {{ $labels.function_id }}
          
      - alert: LowGasBalance
        expr: neo_gas_pool_balance < 100000
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: Low gas balance for user {{ $labels.user_id }}
          
      - alert: TriggerMisfires
        expr: rate(neo_trigger_misfires_total[15m]) > 0
        for: 15m
        labels:
          severity: warning
        annotations:
          summary: Trigger {{ $labels.trigger_id }} is misfiring
```

## Monitoring API

### 1. Query Metrics

```bash
# Get function metrics
curl -X GET "https://api.neo-service-layer.io/v1/metrics/functions/func_123" \
  -H "Authorization: Bearer your_jwt_token" \
  -d '{
    "metrics": ["executions", "duration", "errors"],
    "start": "2024-03-26T00:00:00Z",
    "end": "2024-03-27T00:00:00Z",
    "step": "5m"
  }'

# Get trigger metrics
curl -X GET "https://api.neo-service-layer.io/v1/metrics/triggers/trig_123" \
  -H "Authorization: Bearer your_jwt_token" \
  -d '{
    "metrics": ["executions", "success_rate"],
    "start": "2024-03-26T00:00:00Z",
    "end": "2024-03-27T00:00:00Z",
    "step": "5m"
  }'

# Get gas metrics
curl -X GET "https://api.neo-service-layer.io/v1/metrics/gas" \
  -H "Authorization: Bearer your_jwt_token" \
  -d '{
    "metrics": ["usage", "balance"],
    "start": "2024-03-26T00:00:00Z",
    "end": "2024-03-27T00:00:00Z",
    "step": "5m"
  }'
```

### 2. Configure Alerts

```bash
# Create alert rule
curl -X POST "https://api.neo-service-layer.io/v1/alerts" \
  -H "Authorization: Bearer your_jwt_token" \
  -d '{
    "name": "high_error_rate",
    "description": "Alert when function error rate is high",
    "query": "rate(neo_function_errors_total[5m]) > 0.1",
    "duration": "5m",
    "severity": "warning",
    "notifications": [
      {
        "type": "email",
        "target": "alerts@example.com"
      },
      {
        "type": "slack",
        "target": "#alerts"
      }
    ]
  }'

# List alert rules
curl -X GET "https://api.neo-service-layer.io/v1/alerts" \
  -H "Authorization: Bearer your_jwt_token"

# Update alert rule
curl -X PUT "https://api.neo-service-layer.io/v1/alerts/alert_123" \
  -H "Authorization: Bearer your_jwt_token" \
  -d '{
    "severity": "critical",
    "notifications": [
      {
        "type": "pagerduty",
        "target": "service_key"
      }
    ]
  }'
```

## SDK Integration

### JavaScript/TypeScript

```typescript
import { NeoServiceClient } from '@neo-service/sdk';

const client = new NeoServiceClient({
  baseUrl: 'https://api.neo-service-layer.io/v1',
  jwt: 'your_jwt_token_here'
});

// Query metrics
const metrics = await client.metrics.query({
  functionId: 'func_123',
  metrics: ['executions', 'duration'],
  start: '2024-03-26T00:00:00Z',
  end: '2024-03-27T00:00:00Z',
  step: '5m'
});

// Create alert
const alert = await client.alerts.create({
  name: 'high_error_rate',
  query: 'rate(neo_function_errors_total[5m]) > 0.1',
  duration: '5m',
  severity: 'warning',
  notifications: [
    {
      type: 'email',
      target: 'alerts@example.com'
    }
  ]
});

// Subscribe to real-time metrics
const ws = client.createWebSocket();
ws.subscribe('metrics', {
  functionId: 'func_123',
  metrics: ['executions', 'errors']
});

ws.on('metrics', (data) => {
  console.log('Metric update:', data);
});
```

### Python

```python
from neo_service import NeoServiceClient

client = NeoServiceClient(
    base_url='https://api.neo-service-layer.io/v1',
    jwt='your_jwt_token_here'
)

# Query metrics
metrics = await client.metrics.query(
    function_id='func_123',
    metrics=['executions', 'duration'],
    start='2024-03-26T00:00:00Z',
    end='2024-03-27T00:00:00Z',
    step='5m'
)

# Create alert
alert = await client.alerts.create(
    name='high_error_rate',
    query='rate(neo_function_errors_total[5m]) > 0.1',
    duration='5m',
    severity='warning',
    notifications=[
        {
            'type': 'email',
            'target': 'alerts@example.com'
        }
    ]
)

# Subscribe to real-time metrics
ws = client.create_websocket()
ws.subscribe('metrics', {
    'function_id': 'func_123',
    'metrics': ['executions', 'errors']
})

def on_metric_update(data):
    print('Metric update:', data)

ws.on('metrics', on_metric_update)
```

## Best Practices

1. **Metric Collection**
   - Collect metrics at appropriate intervals
   - Use appropriate metric types (counter, gauge, histogram)
   - Add relevant tags for better filtering
   - Monitor resource usage carefully

2. **Alert Configuration**
   - Set appropriate thresholds
   - Use proper alert severity levels
   - Configure meaningful alert descriptions
   - Set up proper notification channels

3. **Dashboard Organization**
   - Create role-specific dashboards
   - Use consistent naming conventions
   - Include documentation panels
   - Set appropriate refresh intervals

4. **Performance Optimization**
   - Monitor cold start frequency
   - Track resource utilization
   - Identify bottlenecks
   - Optimize based on metrics

5. **Cost Management**
   - Monitor gas usage patterns
   - Track function execution costs
   - Set up budget alerts
   - Optimize resource allocation

## Support

For metrics and monitoring support:
- Email: monitoring@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/r3e-network/neo_service_layer/issues) 