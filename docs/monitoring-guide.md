# Neo Service Layer Monitoring Guide

This guide outlines the monitoring strategy for the Neo Service Layer, including metrics, logging, and alerting.

## Metrics

The Neo Service Layer exposes metrics via Prometheus endpoints. These metrics can be collected and visualized using tools like Prometheus and Grafana.

### Parent Application Metrics

The parent application exposes metrics at the `/metrics` endpoint. These metrics include:

- HTTP request count, latency, and error rate
- VSOCK communication metrics
- System metrics (CPU, memory, disk, network)
- Application-specific metrics (account operations, wallet operations, etc.)

### Enclave Metrics

The enclave application does not expose metrics directly, but forwards them to the parent application. These metrics include:

- CPU usage
- Memory usage
- Request count
- Uptime
- Operation-specific metrics (secret operations, wallet operations, etc.)

### Metric Naming Convention

Metrics follow the Prometheus naming convention:

- `neo_service_layer_<component>_<metric_name>_<unit>`

Examples:
- `neo_service_layer_http_requests_total`
- `neo_service_layer_http_request_duration_seconds`
- `neo_service_layer_enclave_cpu_usage_percent`
- `neo_service_layer_enclave_memory_usage_bytes`

### Recommended Metrics to Monitor

#### System Metrics

- CPU usage
- Memory usage
- Disk usage
- Network I/O

#### Application Metrics

- HTTP request count, latency, and error rate
- VSOCK communication metrics
- Operation-specific metrics
- Error count

## Logging

The Neo Service Layer uses structured logging to provide detailed information about the application's behavior.

### Log Levels

- `Debug`: Detailed information for debugging
- `Info`: General information about the application's behavior
- `Warning`: Potential issues that do not affect the application's functionality
- `Error`: Issues that affect the application's functionality
- `Critical`: Critical issues that require immediate attention

### Log Format

Logs are written in JSON format with the following fields:

- `timestamp`: ISO 8601 timestamp
- `level`: Log level
- `message`: Log message
- `exception`: Exception details (if applicable)
- `context`: Additional context information
- `service`: Service name
- `operation`: Operation name
- `requestId`: Request ID for correlation

Example:
```json
{
  "timestamp": "2023-04-20T12:34:56.789Z",
  "level": "Info",
  "message": "Processing request",
  "context": {
    "service": "wallet",
    "operation": "createWallet",
    "requestId": "1234567890"
  }
}
```

### Log Collection

Logs are written to stdout/stderr and can be collected using standard Docker/AWS logging mechanisms:

- AWS CloudWatch Logs
- ELK Stack (Elasticsearch, Logstash, Kibana)
- Fluentd/Fluent Bit
- Loki

### Recommended Logs to Monitor

- Error and critical logs
- Authentication and authorization logs
- Operation-specific logs
- System logs

## Alerting

Alerts should be configured to notify the operations team of potential issues.

### Recommended Alerts

#### System Alerts

- High CPU usage (> 80% for 5 minutes)
- High memory usage (> 80% for 5 minutes)
- Disk space running low (< 20% free)
- Instance unreachable

#### Application Alerts

- High error rate (> 1% of requests)
- High latency (> 500ms for 95th percentile)
- Enclave not responding
- Critical log entries

### Alert Severity Levels

- `P1`: Critical issue requiring immediate attention
- `P2`: Major issue requiring attention within 1 hour
- `P3`: Minor issue requiring attention within 24 hours
- `P4`: Informational alert requiring no immediate action

### Alert Channels

Alerts can be sent to various channels:

- Email
- SMS
- Slack/Teams
- PagerDuty
- OpsGenie

## Dashboards

Dashboards provide a visual representation of the application's health and performance.

### Recommended Dashboards

#### System Dashboard

- CPU usage
- Memory usage
- Disk usage
- Network I/O
- Instance health

#### Application Dashboard

- Request count, latency, and error rate
- Operation-specific metrics
- Error count
- Enclave health

#### Service-Specific Dashboards

- Account service metrics
- Wallet service metrics
- Secrets service metrics
- Function service metrics
- Price feed service metrics

## Health Checks

Health checks provide a simple way to determine if the application is functioning correctly.

### Parent Application Health Check

The parent application exposes a health check endpoint at `/health`. This endpoint returns:

- `200 OK`: Application is healthy
- `503 Service Unavailable`: Application is unhealthy

The health check includes:

- Database connectivity
- Enclave connectivity
- External service connectivity

### Enclave Health Check

The enclave application does not expose a health check endpoint directly, but its health is monitored by the parent application.

## Incident Response

When an alert is triggered, the operations team should follow the incident response process:

1. **Acknowledge**: Acknowledge the alert and take ownership of the incident
2. **Investigate**: Investigate the root cause of the incident
3. **Mitigate**: Take immediate action to mitigate the impact of the incident
4. **Resolve**: Resolve the incident and restore normal operation
5. **Post-Mortem**: Conduct a post-mortem analysis to prevent similar incidents in the future

## Conclusion

This guide provides a comprehensive monitoring strategy for the Neo Service Layer. By following these recommendations, the operations team can ensure the application's health, performance, and security.
