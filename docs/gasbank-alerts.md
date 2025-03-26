# GasBank Alert System Documentation

## Overview

The GasBank Alert System is designed to monitor Gas resources and notify administrators of potential issues or anomalies in gas allocation and usage. It helps prevent service disruptions by providing timely notifications about low gas levels, high utilization, failed transactions, and other critical events.

## Alert Types

The system supports several types of alerts:

### Resource-based Alerts

- **Low Gas Alert**: Triggered when the available gas in the pool falls below a configurable threshold.
- **Critical Gas Alert**: Triggered when the available gas reaches a critical level that may impact service continuity.
- **High Utilization Alert**: Triggered when the gas utilization (allocated/total ratio) exceeds a configurable threshold.

### Operation-based Alerts

- **Failed Allocation Alert**: Triggered when gas allocation for a user fails.
- **Failed Refill Alert**: Triggered when an attempt to refill the gas pool fails.
- **Large Allocation Alert**: Triggered when a user is allocated a large amount of gas (above a configurable threshold).
- **Allocation Expired Alert**: Triggered when an allocation has expired but wasn't properly released.

### System Alerts

- **System Error Alert**: Triggered for general system errors in any component of the GasBank service.

## Alert Configuration

The alert system is configurable through the `AlertConfig` structure:

```go
type AlertConfig struct {
    // LowGasThreshold is the threshold below which a low gas alert is triggered
    LowGasThreshold *big.Int
    
    // CriticalGasThreshold is the threshold below which a critical gas alert is triggered
    CriticalGasThreshold *big.Int
    
    // HighUtilizationThreshold is the percentage threshold for high utilization alerts
    HighUtilizationThreshold float64
    
    // AlertCooldown is the minimum time between repeated alerts
    AlertCooldown time.Duration
    
    // EnableConsoleLogging determines if alerts should be logged to console
    EnableConsoleLogging bool
}
```

Default values are provided through the `DefaultAlertConfig()` function:

```go
func DefaultAlertConfig() *AlertConfig {
    return &AlertConfig{
        LowGasThreshold:          big.NewInt(5000000000), // 50 GAS in smallest unit
        CriticalGasThreshold:     big.NewInt(1000000000), // 10 GAS in smallest unit
        HighUtilizationThreshold: 0.85,                   // 85% utilization
        AlertCooldown:            5 * time.Minute,
        EnableConsoleLogging:     true,
    }
}
```

## Alert Cooldown and Deduplication

To prevent alert storms, the system implements a cooldown period between alerts of the same type. The `AlertCooldown` parameter in the configuration controls this behavior. Alerts are deduplicated based on their type and key attributes to avoid sending multiple alerts for the same issue.

## Alert Severity Levels

Alerts are categorized into four severity levels:

- **INFO**: Informational events that don't require immediate action.
- **WARNING**: Events that may require attention but don't impact service yet.
- **ERROR**: Events that impact service quality or user operations.
- **CRITICAL**: Events that severely impact service availability or require immediate action.

## Implementation

The alert system is implemented through the `GasAlertManager` interface, which all alert manager implementations must satisfy:

```go
type GasAlertManager interface {
    AlertLowGas(ctx context.Context, remaining *big.Int)
    AlertFailedAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int, reason string)
    AlertFailedRefill(ctx context.Context, amount *big.Int, reason string)
    AlertHighUtilization(ctx context.Context, utilization float64, totalGas *big.Int, allocatedGas *big.Int)
    AlertLargeAllocation(ctx context.Context, allocation *models.GasAllocation)
    AlertAllocationExpired(ctx context.Context, allocation *models.GasAllocation)
    AlertSystemError(ctx context.Context, component string, err error)
}
```

The default implementation is `BasicAlertManager`, which logs alerts to the console with proper formatting and throttling. In a production environment, this can be extended to send alerts to external monitoring systems, email, SMS, or other notification channels.

## Integration with Monitoring Systems

For production deployments, the alert system should be integrated with monitoring solutions like:

- Prometheus/Grafana
- ELK Stack
- PagerDuty
- Slack/Teams notifications
- Email alerting

This requires implementing a custom alert manager that forwards alerts to these systems.

## Best Practices

1. **Set appropriate thresholds**: Adjust alert thresholds based on your expected gas usage patterns.
2. **Configure cooldown periods**: Set cooldown periods long enough to avoid alert storms but short enough to catch recurring issues.
3. **Add external notification channels**: For production systems, always configure external notification channels beyond console logging.
4. **Monitor alert frequency**: A high frequency of alerts may indicate a need to adjust thresholds or investigate systemic issues.
5. **Implement escalation procedures**: Define clear escalation procedures for different alert severity levels.

## Future Enhancements

- Implement adaptive thresholds based on historical gas usage patterns
- Add support for more notification channels (SMS, webhook integrations)
- Develop alert aggregation and correlation to identify related issues
- Implement auto-remediation for common alert scenarios