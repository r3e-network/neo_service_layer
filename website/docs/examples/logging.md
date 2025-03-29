# Logging Service Examples

## Overview
These examples demonstrate how to use the Logging service to implement structured logging, error tracking, and monitoring in your Neo Service Layer application.

## Basic Logging

### Function Logging
```typescript
import { Handler } from '@netlify/functions';
import { Logger } from '../utils/logger';

const logger = Logger.getInstance();

export const handler: Handler = async (event, context) => {
  // Log incoming request
  logger.info('Received API request', {
    path: event.path,
    method: event.httpMethod,
    queryParams: event.queryStringParameters,
    requestId: context.awsRequestId
  });

  try {
    // Process request
    const result = await processRequest(event.body);

    // Log successful response
    logger.info('Request processed successfully', {
      requestId: context.awsRequestId,
      processingTime: result.processingTime
    });

    return {
      statusCode: 200,
      body: JSON.stringify(result.data)
    };
  } catch (error) {
    // Log error with full context
    logger.error('Error processing request', {
      requestId: context.awsRequestId,
      error: {
        name: error.name,
        message: error.message,
        stack: error.stack
      },
      request: {
        path: event.path,
        method: event.httpMethod
      }
    });

    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'Internal server error' })
    };
  }
};
```

### Component Logging
```typescript
import { Logger } from '../utils/logger';

class PriceFeedComponent {
  private logger: Logger;

  constructor() {
    this.logger = Logger.getInstance();
  }

  async fetchPrice(symbol: string, currency: string) {
    const context = {
      component: 'PriceFeedComponent',
      method: 'fetchPrice',
      params: { symbol, currency }
    };

    this.logger.info('Fetching price data', context);

    try {
      const startTime = Date.now();
      const price = await this.getPriceFromOracle(symbol, currency);

      this.logger.info('Price data fetched successfully', {
        ...context,
        duration: Date.now() - startTime,
        price: price.value
      });

      return price;
    } catch (error) {
      this.logger.error('Failed to fetch price data', {
        ...context,
        error: {
          name: error.name,
          message: error.message,
          stack: error.stack
        }
      });

      throw error;
    }
  }
}
```

## Advanced Usage

### Request Context Logging
```typescript
import { Logger } from '../utils/logger';
import { createRequestContext, getRequestContext } from '../utils/context';

// Middleware to create request context
export const requestContextMiddleware = (req, res, next) => {
  const requestId = req.headers['x-request-id'] || generateRequestId();
  const context = createRequestContext({
    requestId,
    path: req.path,
    method: req.method,
    userAgent: req.headers['user-agent'],
    timestamp: new Date().toISOString()
  });

  // Attach logger with request context
  req.logger = Logger.getInstance().child({
    requestId,
    sessionId: req.headers['x-session-id']
  });

  next();
};

// Example usage in route handler
app.get('/api/data', async (req, res) => {
  const logger = req.logger;
  const context = getRequestContext();

  logger.info('Processing API request', {
    ...context,
    query: req.query
  });

  try {
    const data = await fetchData(req.query);
    
    logger.info('API request completed', {
      ...context,
      responseTime: Date.now() - new Date(context.timestamp).getTime()
    });

    res.json(data);
  } catch (error) {
    logger.error('API request failed', {
      ...context,
      error: {
        name: error.name,
        message: error.message,
        stack: error.stack
      }
    });

    res.status(500).json({ error: 'Internal server error' });
  }
});
```

### Performance Logging
```typescript
import { Logger } from '../utils/logger';
import { PerformanceMonitor } from '../utils/performance';

class PerformanceLogger {
  private logger: Logger;
  private monitor: PerformanceMonitor;

  constructor() {
    this.logger = Logger.getInstance();
    this.monitor = new PerformanceMonitor();
  }

  startOperation(name: string, context: object = {}) {
    return this.monitor.start(name, {
      onComplete: (metrics) => {
        this.logger.info('Operation completed', {
          operation: name,
          ...context,
          metrics: {
            duration: metrics.duration,
            memory: metrics.memory,
            cpu: metrics.cpu
          }
        });
      },
      onThreshold: (metrics) => {
        this.logger.warn('Operation exceeded threshold', {
          operation: name,
          ...context,
          metrics: {
            duration: metrics.duration,
            memory: metrics.memory,
            cpu: metrics.cpu
          }
        });
      }
    });
  }

  async measureOperation<T>(
    name: string,
    operation: () => Promise<T>,
    context: object = {}
  ): Promise<T> {
    const monitor = this.startOperation(name, context);

    try {
      const result = await operation();
      monitor.end();
      return result;
    } catch (error) {
      monitor.end({ error });
      throw error;
    }
  }
}
```

### Log Aggregation and Analysis
```typescript
import { Logger } from '../utils/logger';
import { LogAnalyzer } from '../utils/log-analyzer';

class LogAnalyticsService {
  private logger: Logger;
  private analyzer: LogAnalyzer;

  constructor() {
    this.logger = Logger.getInstance();
    this.analyzer = new LogAnalyzer({
      patterns: {
        errors: /error/i,
        warnings: /warn/i,
        performance: /duration|latency|timeout/i
      }
    });
  }

  async analyzeLogStream() {
    const analysis = await this.analyzer.analyze({
      timeRange: {
        start: new Date(Date.now() - 24 * 60 * 60 * 1000), // Last 24 hours
        end: new Date()
      },
      metrics: ['error_rate', 'average_response_time', 'request_volume'],
      groupBy: ['component', 'endpoint']
    });

    this.logger.info('Log analysis completed', {
      timeRange: analysis.timeRange,
      metrics: analysis.metrics,
      insights: analysis.insights
    });

    // Generate alerts for concerning patterns
    analysis.alerts.forEach(alert => {
      this.logger.warn('Log analysis alert', {
        type: alert.type,
        metric: alert.metric,
        threshold: alert.threshold,
        value: alert.value,
        context: alert.context
      });
    });

    return analysis;
  }
}
```

### Structured Error Logging
```typescript
import { Logger } from '../utils/logger';

class ErrorLogger {
  private logger: Logger;

  constructor() {
    this.logger = Logger.getInstance();
  }

  logError(error: Error, context: object = {}) {
    const errorContext = {
      ...context,
      error: {
        name: error.name,
        message: error.message,
        stack: error.stack,
        cause: error.cause
      },
      timestamp: new Date().toISOString(),
      environment: process.env.NODE_ENV
    };

    // Log basic error
    this.logger.error('Application error occurred', errorContext);

    // Additional error classification and handling
    if (error instanceof TypeError) {
      this.logger.error('Type error detected', {
        ...errorContext,
        errorType: 'TypeError',
        severity: 'high'
      });
    } else if (error instanceof ReferenceError) {
      this.logger.error('Reference error detected', {
        ...errorContext,
        errorType: 'ReferenceError',
        severity: 'high'
      });
    } else if (error.message.includes('timeout')) {
      this.logger.error('Timeout error detected', {
        ...errorContext,
        errorType: 'TimeoutError',
        severity: 'medium'
      });
    }

    // Log error metrics
    this.logger.error('Error metrics', {
      ...errorContext,
      metrics: {
        timestamp: Date.now(),
        type: error.name,
        count: 1
      }
    });
  }
}
```

## Testing Examples

### Unit Testing Logging
```typescript
import { Logger } from '../utils/logger';

describe('Logger', () => {
  let logger: Logger;
  let mockTransport: jest.Mock;

  beforeEach(() => {
    mockTransport = jest.fn();
    logger = Logger.getInstance();
    logger.setTransport(mockTransport);
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it('logs info messages with correct structure', () => {
    const message = 'Test info message';
    const context = { user: 'test', action: 'login' };

    logger.info(message, context);

    expect(mockTransport).toHaveBeenCalledWith({
      level: 'info',
      message,
      timestamp: expect.any(String),
      ...context
    });
  });

  it('logs error messages with error details', () => {
    const error = new Error('Test error');
    const context = { operation: 'test' };

    logger.error('Error occurred', {
      ...context,
      error
    });

    expect(mockTransport).toHaveBeenCalledWith({
      level: 'error',
      message: 'Error occurred',
      timestamp: expect.any(String),
      operation: 'test',
      error: {
        name: error.name,
        message: error.message,
        stack: error.stack
      }
    });
  });

  it('creates child loggers with inherited context', () => {
    const parentContext = { service: 'test-service' };
    const childLogger = logger.child(parentContext);
    const message = 'Child logger message';
    const childContext = { operation: 'test' };

    childLogger.info(message, childContext);

    expect(mockTransport).toHaveBeenCalledWith({
      level: 'info',
      message,
      timestamp: expect.any(String),
      ...parentContext,
      ...childContext
    });
  });
});
```

### Integration Testing with Logging
```typescript
import { Logger } from '../utils/logger';
import { handler } from './function-with-logging';

describe('Function with logging integration', () => {
  let logger: Logger;
  let loggedMessages: any[];

  beforeEach(() => {
    loggedMessages = [];
    logger = Logger.getInstance();
    logger.setTransport((log) => loggedMessages.push(log));
  });

  afterEach(() => {
    loggedMessages = [];
  });

  it('logs request and response for successful execution', async () => {
    const event = {
      httpMethod: 'GET',
      path: '/test',
      headers: {},
      queryStringParameters: { id: '123' },
      body: null
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    await handler(event as any, context as any);

    // Verify request logging
    expect(loggedMessages[0]).toMatchObject({
      level: 'info',
      message: 'Received API request',
      path: '/test',
      method: 'GET',
      queryParams: { id: '123' },
      requestId: 'test-request-id'
    });

    // Verify response logging
    expect(loggedMessages[1]).toMatchObject({
      level: 'info',
      message: 'Request processed successfully',
      requestId: 'test-request-id'
    });
  });

  it('logs errors appropriately', async () => {
    const event = {
      httpMethod: 'GET',
      path: '/error',
      headers: {},
      queryStringParameters: {},
      body: null
    };

    const context = {
      awsRequestId: 'test-error-request-id'
    };

    await handler(event as any, context as any);

    // Find error log
    const errorLog = loggedMessages.find(log => log.level === 'error');
    
    expect(errorLog).toMatchObject({
      level: 'error',
      message: 'Error processing request',
      requestId: 'test-error-request-id',
      error: {
        name: expect.any(String),
        message: expect.any(String),
        stack: expect.any(String)
      }
    });
  });
});
```