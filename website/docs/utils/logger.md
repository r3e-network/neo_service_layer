# Logger Utility

## Overview
The logger utility provides structured logging capabilities for the Neo Service Layer. It implements a singleton pattern to ensure consistent logging across the application and includes support for different log levels and contextual data.

## Features
- Multiple log levels
- Structured logging
- Timestamp tracking
- Contextual data
- Singleton pattern

## API Reference

### `Logger`

#### `getInstance()`
Gets the singleton instance of the logger.

```typescript
static getInstance(): Logger
```

**Returns:**
- Logger instance

#### `info(message: string, data?: LogData)`
Logs an informational message.

```typescript
info(message: string, data?: LogData): void
```

**Parameters:**
- `message`: Log message
- `data`: Optional contextual data

#### `warn(message: string, data?: LogData)`
Logs a warning message.

```typescript
warn(message: string, data?: LogData): void
```

**Parameters:**
- `message`: Warning message
- `data`: Optional contextual data

#### `error(message: string, data?: LogData)`
Logs an error message.

```typescript
error(message: string, data?: LogData): void
```

**Parameters:**
- `message`: Error message
- `data`: Optional contextual data

## Usage Examples

### Basic Logging
```typescript
import { logger } from './logger';

// Info logging
logger.info('Operation completed successfully');

// Warning logging
logger.warn('Resource running low');

// Error logging
logger.error('Operation failed');
```

### Logging with Context
```typescript
// With simple data
logger.info('User action', { userId: '123', action: 'login' });

// With error context
try {
  // Some operation
} catch (error) {
  logger.error('Operation failed', { error, context: 'user-service' });
}

// With complex data
logger.info('Transaction processed', {
  txId: 'tx123',
  amount: 100,
  timestamp: new Date(),
  metadata: {
    type: 'transfer',
    status: 'success'
  }
});
```

## Log Format

### Structure
```typescript
interface LogEntry {
  timestamp: string;
  level: 'INFO' | 'WARN' | 'ERROR';
  message: string;
  data?: LogData;
}
```

### Example Output
```
[2023-08-15T10:30:45.123Z] INFO: User logged in {"userId":"123","ip":"192.168.1.1"}
[2023-08-15T10:30:46.234Z] WARN: High memory usage {"usage":85,"threshold":80}
[2023-08-15T10:30:47.345Z] ERROR: Database connection failed {"error":"timeout"}
```

## Best Practices

### Log Levels
1. **INFO**
   - Successful operations
   - State changes
   - User actions
   - System events

2. **WARN**
   - Resource constraints
   - Deprecated features
   - Non-critical issues
   - Performance degradation

3. **ERROR**
   - Operation failures
   - System errors
   - Security issues
   - Critical problems

### Contextual Data
1. Always include relevant context
2. Structure data consistently
3. Avoid sensitive information
4. Include timestamps
5. Add correlation IDs

### Performance
1. Use appropriate log levels
2. Avoid excessive logging
3. Implement log rotation
4. Consider async logging
5. Monitor log volume

## Error Handling

### Example
```typescript
try {
  // Operation
} catch (error) {
  logger.error('Operation failed', {
    error: {
      message: error.message,
      stack: error.stack,
      code: error.code
    },
    context: {
      operation: 'database-query',
      params: { /* query params */ }
    }
  });
}
```

## Testing

### Unit Tests
```typescript
describe('Logger', () => {
  let consoleLogSpy: jest.SpyInstance;
  
  beforeEach(() => {
    consoleLogSpy = jest.spyOn(console, 'log').mockImplementation();
  });

  afterEach(() => {
    consoleLogSpy.mockRestore();
  });

  it('should log info messages', () => {
    logger.info('Test message');
    expect(consoleLogSpy).toHaveBeenCalledWith(
      expect.stringMatching(/^\[\d{4}-\d{2}-\d{2}T.*\] INFO: Test message/)
    );
  });
});
```

## Integration

### With Express Middleware
```typescript
app.use((req, res, next) => {
  logger.info('Request received', {
    method: req.method,
    path: req.path,
    ip: req.ip
  });
  next();
});
```

### With Error Handler
```typescript
app.use((error, req, res, next) => {
  logger.error('Request failed', {
    error: error.message,
    stack: error.stack,
    request: {
      method: req.method,
      path: req.path
    }
  });
  res.status(500).json({ error: 'Internal server error' });
});
```

## Monitoring

### Metrics to Track
1. Log volume by level
2. Error frequency
3. Warning patterns
4. Response times
5. Resource usage

### Alerts
```typescript
if (errorCount > threshold) {
  logger.error('Error threshold exceeded', {
    count: errorCount,
    threshold,
    timeWindow: '5m'
  });
  // Trigger alert
}
```

## Configuration

### Log Rotation
```typescript
const config = {
  maxSize: '10m',
  maxFiles: 5,
  compress: true
};
```

### Log Levels
```typescript
const levels = {
  error: 0,
  warn: 1,
  info: 2
};
```

## Security

### Sensitive Data
1. Never log credentials
2. Mask sensitive fields
3. Follow compliance requirements
4. Implement log encryption
5. Control log access

### Example
```typescript
const maskSensitiveData = (data: any) => {
  // Implementation
};

logger.info('User data', maskSensitiveData(userData));
```