# Logging Service

## Overview

The Logging Service provides a centralized, structured logging solution for the Neo Service Layer. It enables all services to log messages with consistent formatting, contextual information, and configurable output formats. The service supports both simple and complex logging scenarios, from basic message logging to advanced querying and filtering capabilities.

## Core Features

- **Unified Logging**: Centralized logging across all Neo Service Layer components
- **Structured Logging**: Consistent log format with metadata and context
- **Multiple Log Levels**: Support for debug, info, warning, and error levels
- **Context Enrichment**: Add service information, request IDs, and custom context to logs
- **Log Rotation**: Automatic log file rotation based on size limits
- **Compression**: Optional compression for rotated log files to save storage
- **Retention Management**: Configure how long logs are retained
- **Query Capability**: Retrieve and filter logs based on various criteria

## Configuration

The Logging Service can be configured with the following options:

| Option | Description | Default |
|--------|-------------|---------|
| LogLevel | Minimum log level (debug, info, warn, error) | info |
| EnableJSONLogs | Output logs in JSON format | false |
| LogFilePath | Path to the log file | logs/neo_service.log |
| MaxSizeInMB | Maximum size of log files before rotation | 10 |
| RetainedFiles | Number of rotated log files to keep | 5 |
| EnableCompression | Compress rotated log files | true |

## Service Interface

The Logging Service exposes the following primary methods:

```go
// Log messages at different levels
LogDebug(message string, context map[string]interface{}) error
LogInfo(message string, context map[string]interface{}) error
LogWarning(message string, context map[string]interface{}) error
LogError(message string, context map[string]interface{}) error

// Query logs with filtering
QueryLogs(ctx context.Context, query string, startTime time.Time, endTime time.Time, limit int) ([]LogEntry, error)

// Lifecycle management
Start() error
Shutdown(ctx context.Context) error
```

## Usage Examples

### Basic Logging

```go
// Initialize the logging service
config := &logging.Config{
    LogLevel:          "info",
    EnableJSONLogs:    true,
    LogFilePath:       "logs/app.log",
    MaxSizeInMB:       10,
    RetainedFiles:     5,
    EnableCompression: true,
}

loggingService, err := logging.NewService(config)
if err != nil {
    panic(err)
}

// Start the service
loggingService.Start()

// Log a simple message
loggingService.LogInfo("Application started", map[string]interface{}{
    "service": "api",
    "version": "1.0.0",
})
```

### Contextual Logging

```go
// Log with request context
loggingService.LogInfo("Processing request", map[string]interface{}{
    "service":     "api",
    "request_id":  "req-123",
    "user_id":     "user-456",
    "endpoint":    "/api/v1/function",
    "method":      "POST",
    "status_code": 200,
})

// Log an error with context
err := someOperation()
if err != nil {
    loggingService.LogError("Operation failed", map[string]interface{}{
        "service": "functions",
        "error":   err.Error(),
        "context": "function execution",
    })
}
```

### Querying Logs

```go
// Query logs from the last hour
ctx := context.Background()
startTime := time.Now().Add(-1 * time.Hour)
endTime := time.Now()
limit := 100

// Get all error logs from the API service
logs, err := loggingService.QueryLogs(ctx, "service:api level:error", startTime, endTime, limit)
if err != nil {
    panic(err)
}

// Process the results
for _, log := range logs {
    fmt.Printf("[%s] %s: %s\n", log.Timestamp, log.Level, log.Message)
}
```

## Integration with Other Services

The Logging Service integrates with other Neo Service Layer components to provide comprehensive logging:

### API Service

The API Service uses the Logging Service to record:
- Incoming requests and their parameters
- Response status codes and times
- Authentication and authorization events
- Error conditions and exceptions

```go
// Example of API Service logging
apiContext := map[string]interface{}{
    "service":     "api",
    "request_id":  requestID,
    "user_id":     userID,
    "endpoint":    endpoint,
    "method":      method,
    "status_code": statusCode,
    "latency_ms":  latency,
}
loggingService.LogInfo("API request processed", apiContext)
```

### Functions Service

The Functions Service logs function execution details:
- Function invocation with parameters
- Execution time and resource usage
- Success or failure status
- Return values or error details

```go
// Example of Functions Service logging
functionContext := map[string]interface{}{
    "service":       "functions",
    "function_id":   functionID,
    "function_name": functionName,
    "user_id":       userID,
    "execution_time_ms": executionTime,
    "memory_usage_mb":   memoryUsage,
}
loggingService.LogInfo("Function executed", functionContext)
```

### Gas Bank Service

The Gas Bank Service logs gas allocation and usage:
- Gas allocations to users or functions
- Gas consumption by transactions
- Gas balances and refills
- Pricing and cost calculations

```go
// Example of Gas Bank Service logging
gasBankContext := map[string]interface{}{
    "service":   "gas_bank",
    "user_id":   userID,
    "gas_used":  gasUsed,
    "operation": operation,
    "tx_hash":   txHash,
    "balance":   newBalance,
}
loggingService.LogInfo("Gas allocated", gasBankContext)
```

### Trigger Service

The Trigger Service logs trigger events and executions:
- Trigger conditions and evaluations
- Triggered function calls
- Scheduling and execution timing
- Success or failure of triggered actions

```go
// Example of Trigger Service logging
triggerContext := map[string]interface{}{
    "service":     "trigger",
    "trigger_id":  triggerID,
    "function_id": functionID,
    "condition":   condition,
    "status":      status,
}
loggingService.LogInfo("Trigger executed", triggerContext)
```

## Best Practices

1. **Use Context Appropriately**: Always include relevant context information such as service name, request ID, and user ID.

2. **Choose Log Levels Carefully**:
   - `debug`: Detailed information for debugging
   - `info`: General operational information
   - `warn`: Warning conditions that don't prevent normal operation
   - `error`: Error conditions that affect functionality

3. **Structured Data**: Use structured context data instead of formatting strings to make logs more searchable and filterable.

4. **Performance Considerations**: Be mindful of high-volume logging and its impact on performance. Consider sampling or filtering logs in high-throughput scenarios.

5. **Sensitive Information**: Never log sensitive data such as passwords, private keys, or personal information.

## Monitoring and Alerting

The Logging Service can be integrated with monitoring and alerting systems:

1. **Error Rate Monitoring**: Track the rate of error-level logs to detect system issues.
2. **Log Volume Alerts**: Set up alerts for unusual increases in log volume that could indicate problems.
3. **Pattern Detection**: Configure alerts for specific error patterns or log messages that require immediate attention.

## Security Considerations

1. **Access Control**: Restrict access to log files and API endpoints to authorized personnel only.
2. **Data Protection**: Ensure logs containing sensitive information are properly protected, both at rest and in transit.
3. **Retention Policies**: Implement appropriate retention policies to comply with data protection regulations.

## Troubleshooting

Common issues and their solutions:

1. **Missing Logs**: Check the configured log level - it might be set too high for the messages you're looking for.
2. **High Disk Usage**: Review log rotation settings and consider more aggressive retention policies.
3. **Performance Impact**: If logging causes performance issues, consider asynchronous logging or batch processing.
4. **Query Performance**: For large log volumes, consider implementing more efficient indexing or a dedicated log aggregation solution.

## Implementation Details

The Logging Service is built upon the following key components:

1. **Core Service**: Manages configuration, log creation, and service lifecycle
2. **Log Store**: In-memory storage with indexing for efficient queries
3. **Formatter**: Handles log formatting in different output formats
4. **File Handler**: Manages log files, rotation, and compression
5. **Query Engine**: Processes log queries with filtering and sorting