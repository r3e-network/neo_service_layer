# Logging Service

The Logging service provides centralized logging, log management, and analysis capabilities for the Neo Service Layer.

## Overview

The Logging service collects, stores, and facilitates the analysis of logs from all components of the service layer. It enables tracking system events, debugging issues, and monitoring application health.

## Configuration

The Logging service is configured using the `Config` struct:

```go
type Config struct {
    LogLevel        string // Minimum log level to record (debug, info, warn, error)
    EnableJSONLogs  bool   // Whether to format logs as JSON
    LogFilePath     string // Path to the log file
    MaxSizeInMB     int    // Maximum size of log files before rotation
    RetainedFiles   int    // Number of rotated log files to retain
    EnableCompression bool  // Whether to compress rotated log files
}
```

## Core Components

### Service

The `Service` is the main component of the Logging service:

```go
type Service struct {
    config     *Config
    logger     *zap.Logger
    rotator    *lumberjack.Logger
    logStore   *logStore
}
```

### Log Entry

A `LogEntry` represents a single log record:

```go
type LogEntry struct {
    ID        string                 // Unique identifier
    Timestamp time.Time              // When the log was created
    Level     string                 // Log level (debug, info, warn, error)
    Message   string                 // Log message
    Service   string                 // Source service
    Context   map[string]interface{} // Additional context
}
```

### Log Store

The `logStore` manages log storage and retrieval:

```go
type logStore struct {
    entries      []LogEntry
    indexByLevel map[string][]int
    indexByTime  []int
    mutex        sync.RWMutex
}
```

## Key Operations

### Logging

The service provides methods for different log levels:

```go
func (s *Service) LogDebug(message string, context map[string]interface{}) error
func (s *Service) LogInfo(message string, context map[string]interface{}) error
func (s *Service) LogWarning(message string, context map[string]interface{}) error
func (s *Service) LogError(message string, context map[string]interface{}) error
```

### Log Retrieval

Logs can be retrieved with filtering:

```go
func (s *Service) QueryLogs(ctx context.Context, query string, startTime time.Time, endTime time.Time, limit int) ([]LogEntry, error)
```

### Service Lifecycle

The Logging service can be started and stopped:

```go
func (s *Service) Start() error
func (s *Service) Shutdown(ctx context.Context) error
```

## Log Querying

The Logging service supports a query language for filtering logs:

### Query Syntax
- `level:info` - Logs with info level
- `message:error` - Logs with message containing "error"
- `service:api` - Logs from the API service
- `context.userAddress:Neo1...` - Logs with specific context field
- `timestamp>2023-01-01` - Logs after a specific date
- `AND`, `OR`, `NOT` - Logical operators

### Examples
- `level:error AND service:api` - Error logs from the API service
- `context.userAddress:Neo1... OR context.contractHash:0x...` - Logs for a specific user or contract
- `message:timeout AND NOT service:api` - Timeout logs not from the API service

## Integration

The Logging service integrates with:

- All other services in the Neo Service Layer
- External log management systems like ELK stack
- Alert management systems for critical errors
- Metrics service for log-based metrics

## Use Cases

- Debugging application issues
- Monitoring system health
- Tracking user activity
- Security monitoring and auditing
- Performance analysis
- Compliance reporting

## Future Improvements

- Distributed log collection
- Advanced log analysis
- Machine learning for anomaly detection
- Custom log retention policies
- Enhanced search capabilities
- Real-time log streaming