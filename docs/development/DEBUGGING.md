# Neo N3 Service Layer Debugging Guide

## Overview

This guide provides comprehensive information about debugging practices, tools, and techniques available in the Neo N3 Service Layer. It covers local debugging, remote debugging, logging, monitoring, and troubleshooting common issues.

## Local Debugging

### 1. Development Server

```bash
# Start development server with debugging enabled
neo-service dev --debug

# Start with specific configuration
neo-service dev --debug --config custom-config.yaml

# Start with function watching
neo-service dev --debug --watch

# Start with verbose logging
neo-service dev --debug --verbose
```

### 2. VS Code Configuration

```json
// .vscode/launch.json
{
  "version": "0.2.0",
  "configurations": [
    {
      "type": "node",
      "request": "launch",
      "name": "Debug Function",
      "program": "${workspaceFolder}/node_modules/@neo-service/cli/bin/neo-service",
      "args": ["dev", "--debug"],
      "env": {
        "NODE_ENV": "development"
      },
      "sourceMaps": true,
      "outFiles": ["${workspaceFolder}/dist/**/*.js"]
    }
  ]
}
```

### 3. Function Debugging

```typescript
// functions/payment/index.ts
import { NeoFunction } from '@neo-service/sdk';

export const processPayment: NeoFunction = async (input, context) => {
  // Debug logging
  context.logger.debug('Processing payment', {
    input,
    timestamp: new Date().toISOString()
  });

  try {
    // Validate input
    if (!input.amount || input.amount <= 0) {
      throw new Error('Invalid amount');
    }

    // Execute transaction
    const tx = await context.blockchain.transfer({
      to: input.recipient,
      amount: input.amount,
      asset: input.asset
    });

    // Debug transaction result
    context.logger.debug('Transaction executed', {
      hash: tx.hash,
      status: tx.status
    });

    return {
      success: true,
      transactionHash: tx.hash
    };
  } catch (error) {
    // Error logging
    context.logger.error('Payment failed', {
      input,
      error: error.message,
      stack: error.stack
    });
    throw error;
  }
};
```

## Remote Debugging

### 1. Setup

```yaml
# config/development.yaml
debug:
  enabled: true
  port: 9229
  host: 0.0.0.0
  break: true
```

### 2. Connection

```bash
# Start remote debugging session
neo-service debug connect

# Connect to specific host
neo-service debug connect --host remote-host --port 9229

# Connect with authentication
neo-service debug connect --token debug-token
```

### 3. Browser DevTools

1. Open Chrome
2. Navigate to `chrome://inspect`
3. Click "Configure" and add your debug target
4. Select your Node.js target
5. Use DevTools for debugging

## Logging

### 1. Log Levels

```typescript
// Configure log levels
context.logger.setLevel('debug');

// Debug level
context.logger.debug('Detailed information', {
  data: someData,
  timestamp: new Date()
});

// Info level
context.logger.info('Normal operation information', {
  operation: 'process',
  status: 'success'
});

// Warning level
context.logger.warn('Warning condition', {
  condition: 'lowBalance',
  threshold: 1000
});

// Error level
context.logger.error('Error condition', {
  error: error.message,
  stack: error.stack
});
```

### 2. Structured Logging

```typescript
// Add context to logs
const logger = context.logger.child({
  function: 'processPayment',
  version: '1.0.0'
});

// Log with context
logger.info('Processing started', {
  requestId: context.requestId,
  userId: context.userId
});

// Log with metrics
logger.info('Processing completed', {
  duration: performance.now() - startTime,
  success: true
});
```

### 3. Log Filtering

```typescript
// Filter sensitive data
const sanitizedInput = {
  ...input,
  privateKey: '[REDACTED]'
};

logger.info('Processing input', {
  input: sanitizedInput
});

// Filter by category
logger.info('Database operation', {
  category: 'database',
  operation: 'insert'
});
```

## Monitoring

### 1. Metrics

```typescript
// Track function execution time
const timer = context.metrics.startTimer();
try {
  // Function logic
} finally {
  timer.end();
}

// Track custom metrics
context.metrics.increment('function.invocations');
context.metrics.gauge('function.memory', process.memoryUsage().heapUsed);
context.metrics.histogram('function.duration', executionTime);
```

### 2. Traces

```typescript
// Start trace
const trace = context.tracer.startSpan('processPayment');

try {
  // Add trace attributes
  trace.setAttribute('amount', input.amount);
  trace.setAttribute('recipient', input.recipient);

  // Create child span
  const txSpan = context.tracer.startSpan('blockchain.transfer', {
    parent: trace
  });

  try {
    // Execute transaction
    await context.blockchain.transfer(/* ... */);
  } finally {
    txSpan.end();
  }
} finally {
  trace.end();
}
```

### 3. Health Checks

```typescript
// Health check endpoint
export const healthCheck: NeoFunction = async (_, context) => {
  const checks = await Promise.all([
    checkDatabase(context),
    checkBlockchain(context),
    checkCache(context)
  ]);

  return {
    status: checks.every(c => c.status === 'healthy') ? 'healthy' : 'unhealthy',
    checks
  };
};
```

## Troubleshooting

### 1. Common Issues

#### Function Timeouts

```typescript
// Set appropriate timeout
export const longRunningFunction: NeoFunction = {
  handler: async (input, context) => {
    // Function logic
  },
  timeout: 300 // 5 minutes
};

// Monitor execution time
const startTime = Date.now();
context.logger.info('Function started', { startTime });

// Check remaining time
const remainingTime = context.getRemainingTimeInMillis();
if (remainingTime < 5000) {
  context.logger.warn('Function about to timeout', {
    remainingTime
  });
}
```

#### Memory Issues

```typescript
// Monitor memory usage
const memoryUsage = process.memoryUsage();
context.logger.info('Memory usage', {
  heapUsed: memoryUsage.heapUsed,
  heapTotal: memoryUsage.heapTotal,
  external: memoryUsage.external
});

// Handle large data
async function processLargeData(data: any[], context: Context) {
  const batchSize = 100;
  for (let i = 0; i < data.length; i += batchSize) {
    const batch = data.slice(i, i + batchSize);
    await processBatch(batch, context);
    
    // Log progress
    context.logger.debug('Batch processed', {
      progress: `${i + batchSize}/${data.length}`
    });
  }
}
```

#### Network Issues

```typescript
// Handle network timeouts
const fetchWithTimeout = async (url: string, timeout: number) => {
  const controller = new AbortController();
  const id = setTimeout(() => controller.abort(), timeout);

  try {
    const response = await fetch(url, {
      signal: controller.signal
    });
    return response;
  } catch (error) {
    context.logger.error('Network error', {
      url,
      error: error.message
    });
    throw error;
  } finally {
    clearTimeout(id);
  }
};
```

### 2. Debugging Tools

#### CLI Tools

```bash
# View function logs
neo-service logs function myFunction

# View function metrics
neo-service metrics function myFunction

# View function traces
neo-service traces function myFunction

# View function profile
neo-service profile function myFunction
```

#### Diagnostic Tools

```typescript
// Memory snapshot
const snapshot = require('heapdump');
snapshot.writeSnapshot('./heap.heapsnapshot');

// CPU profile
const profiler = require('v8-profiler-node8');
profiler.startProfiling('CPU Profile');
setTimeout(() => {
  const profile = profiler.stopProfiling();
  profile.export().pipe(fs.createWriteStream('./profile.cpuprofile'));
}, 30000);
```

### 3. Error Handling

```typescript
// Custom error types
class ValidationError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'ValidationError';
  }
}

// Error handling middleware
const errorHandler = async (error: Error, context: Context) => {
  if (error instanceof ValidationError) {
    context.logger.warn('Validation error', {
      error: error.message
    });
    return {
      statusCode: 400,
      body: {
        error: 'Validation Error',
        message: error.message
      }
    };
  }

  context.logger.error('Unhandled error', {
    error: error.message,
    stack: error.stack
  });

  return {
    statusCode: 500,
    body: {
      error: 'Internal Server Error'
    }
  };
};
```

## Best Practices

1. **Debugging Setup**
   - Use proper IDE configuration
   - Enable source maps
   - Configure debug logging
   - Set up monitoring
   - Use diagnostic tools

2. **Logging**
   - Use appropriate log levels
   - Include relevant context
   - Structure log messages
   - Filter sensitive data
   - Rotate log files

3. **Monitoring**
   - Track key metrics
   - Set up alerts
   - Monitor resource usage
   - Use distributed tracing
   - Implement health checks

4. **Error Handling**
   - Use custom error types
   - Implement error boundaries
   - Log error context
   - Handle async errors
   - Provide meaningful messages

5. **Performance**
   - Profile code execution
   - Monitor memory usage
   - Track network calls
   - Optimize resource usage
   - Handle timeouts

## Support

For debugging support:
- Email: dev@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/will/neo_service_layer/issues) 