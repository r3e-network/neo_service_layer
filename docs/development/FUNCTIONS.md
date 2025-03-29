# Neo N3 Service Layer Function Reference

## Overview

This reference guide provides detailed information about creating, configuring, and managing serverless functions in the Neo N3 Service Layer.

## Function Structure

### Basic Function Structure

```typescript
import { NeoFunction } from '@neo-service/sdk';

interface Input {
  // Input parameters
}

interface Output {
  // Output parameters
}

export const myFunction: NeoFunction<Input, Output> = async (input, context) => {
  // Function implementation
  return output;
};
```

### Context Object

The context object provides access to various services and utilities:

```typescript
interface FunctionContext {
  // Blockchain interaction
  blockchain: {
    transfer(params: TransferParams): Promise<TransactionResult>;
    invoke(params: InvokeParams): Promise<InvokeResult>;
    // ... other blockchain methods
  };
  
  // Function invocation
  functions: {
    execute<T>(name: string, input: any): Promise<T>;
    // ... other function methods
  };
  
  // Logging
  logger: {
    debug(message: string, meta?: object): void;
    info(message: string, meta?: object): void;
    warn(message: string, meta?: object): void;
    error(message: string, meta?: object): void;
  };
  
  // Metrics
  metrics: {
    increment(name: string, value?: number): void;
    gauge(name: string, value: number): void;
    histogram(name: string, value: number): void;
    startTimer(): Timer;
  };
  
  // Secrets
  secrets: {
    get(name: string): Promise<string>;
    // ... other secrets methods
  };
  
  // Price feed
  priceFeed: {
    getPrice(symbol: string): Promise<number>;
    // ... other price feed methods
  };
}
```

## Function Types

### 1. HTTP Functions

```typescript
import { NeoHttpFunction } from '@neo-service/sdk';

interface HttpInput {
  method: string;
  path: string;
  headers: Record<string, string>;
  query: Record<string, string>;
  body: any;
}

interface HttpOutput {
  statusCode: number;
  headers: Record<string, string>;
  body: any;
}

export const httpFunction: NeoHttpFunction = async (input, context) => {
  return {
    statusCode: 200,
    headers: {
      'Content-Type': 'application/json'
    },
    body: {
      message: 'Success'
    }
  };
};
```

### 2. Blockchain Functions

```typescript
import { NeoBlockchainFunction } from '@neo-service/sdk';

interface TransferInput {
  from: string;
  to: string;
  amount: number;
  asset: string;
}

interface TransferOutput {
  txHash: string;
  status: 'success' | 'failed';
}

export const transferFunction: NeoBlockchainFunction<TransferInput, TransferOutput> = async (input, context) => {
  const tx = await context.blockchain.transfer({
    from: input.from,
    to: input.to,
    amount: input.amount,
    asset: input.asset
  });
  
  return {
    txHash: tx.hash,
    status: tx.status
  };
};
```

### 3. Event Functions

```typescript
import { NeoEventFunction } from '@neo-service/sdk';

interface PriceEvent {
  symbol: string;
  price: number;
  timestamp: number;
}

export const priceEventHandler: NeoEventFunction<PriceEvent> = async (event, context) => {
  // Process price event
  await context.metrics.gauge(`price.${event.symbol}`, event.price);
  
  // Log event
  context.logger.info('Price event processed', {
    symbol: event.symbol,
    price: event.price
  });
};
```

## Function Configuration

### 1. Basic Configuration

```yaml
functions:
  myFunction:
    handler: functions/myFunction/index.myFunction
    runtime: node18
    memory: 256
    timeout: 30s
```

### 2. Environment Variables

```yaml
functions:
  myFunction:
    environment:
      API_KEY: ${secrets.API_KEY}
      NODE_ENV: production
      DEBUG: false
```

### 3. Permissions

```yaml
functions:
  myFunction:
    permissions:
      - blockchain:transfer
      - blockchain:invoke
      - functions:execute
      - secrets:read
```

### 4. Retry Configuration

```yaml
functions:
  myFunction:
    retry:
      attempts: 3
      backoff: exponential
      initialDelay: 1s
      maxDelay: 30s
```

### 5. Scaling Configuration

```yaml
functions:
  myFunction:
    scaling:
      minInstances: 1
      maxInstances: 10
      targetConcurrency: 80
```

### 6. Network Configuration

```yaml
functions:
  myFunction:
    network:
      vpc: default
      subnets:
        - subnet-1
        - subnet-2
      securityGroups:
        - sg-1
```

## Function Patterns

### 1. Input Validation

```typescript
import { z } from 'zod';
import { NeoFunction } from '@neo-service/sdk';

const InputSchema = z.object({
  amount: z.number().positive(),
  recipient: z.string().min(1),
  asset: z.string().min(1)
});

export const validateInput: NeoFunction = async (input, context) => {
  // Validate input
  const validatedInput = InputSchema.parse(input);
  
  // Process validated input
  // ...
};
```

### 2. Error Handling

```typescript
import { NeoFunction, NeoError } from '@neo-service/sdk';

export const handleErrors: NeoFunction = async (input, context) => {
  try {
    // Function logic
  } catch (error) {
    if (error instanceof NeoError) {
      // Handle known errors
      context.logger.warn('Known error occurred', {
        code: error.code,
        message: error.message
      });
      throw error;
    }
    
    // Handle unknown errors
    context.logger.error('Unknown error occurred', {
      error: error.message,
      stack: error.stack
    });
    throw new NeoError('INTERNAL_ERROR', 'An unexpected error occurred');
  }
};
```

### 3. Caching

```typescript
import { NeoFunction } from '@neo-service/sdk';

export const cacheExample: NeoFunction = async (input, context) => {
  const cacheKey = `data:${input.id}`;
  
  // Try to get from cache
  const cached = await context.cache.get(cacheKey);
  if (cached) {
    return JSON.parse(cached);
  }
  
  // Get fresh data
  const data = await getData(input.id);
  
  // Cache the result
  await context.cache.set(cacheKey, JSON.stringify(data), {
    ttl: 3600 // 1 hour
  });
  
  return data;
};
```

### 4. Circuit Breaker

```typescript
import { NeoFunction, CircuitBreaker } from '@neo-service/sdk';

const breaker = new CircuitBreaker({
  name: 'external-api',
  timeout: 5000,
  errorThreshold: 50,
  resetTimeout: 30000
});

export const circuitBreakerExample: NeoFunction = async (input, context) => {
  return breaker.execute(async () => {
    // Make external API call
    const response = await fetch('https://api.example.com/data');
    return response.json();
  });
};
```

## Testing Functions

### 1. Unit Testing

```typescript
import { TestClient } from '@neo-service/testing';
import { myFunction } from './index';

describe('My Function', () => {
  let client: TestClient;
  
  beforeEach(async () => {
    client = await TestClient.create();
  });
  
  afterEach(async () => {
    await client.cleanup();
  });
  
  it('should process input correctly', async () => {
    // Arrange
    const input = { /* test input */ };
    
    // Mock dependencies
    client.blockchain
      .mockTransfer()
      .returns({ /* mock response */ });
    
    // Act
    const result = await myFunction(input, client.context);
    
    // Assert
    expect(result).toEqual({ /* expected output */ });
  });
});
```

### 2. Integration Testing

```typescript
import { IntegrationTestClient } from '@neo-service/testing';

describe('My Function Integration', () => {
  let client: IntegrationTestClient;
  
  beforeAll(async () => {
    client = await IntegrationTestClient.create({
      configPath: './config/test.yaml'
    });
  });
  
  afterAll(async () => {
    await client.cleanup();
  });
  
  it('should integrate with other services', async () => {
    // Arrange
    const input = { /* test input */ };
    
    // Act
    const result = await client.functions.execute('myFunction', input);
    
    // Assert
    expect(result).toEqual({ /* expected output */ });
  });
});
```

## Deployment

### 1. Development

```bash
# Deploy single function
neo-service deploy function myFunction

# Deploy with specific config
neo-service deploy function myFunction --config dev.yaml

# Deploy with environment variables
neo-service deploy function myFunction --env-file .env.dev
```

### 2. Production

```bash
# Deploy to production
neo-service deploy function myFunction --stage prod

# Deploy with specific version
neo-service deploy function myFunction --version 1.0.0

# Deploy with canary
neo-service deploy function myFunction --canary 10
```

## Monitoring

### 1. Logging

```typescript
export const monitoredFunction: NeoFunction = async (input, context) => {
  // Debug logging
  context.logger.debug('Processing input', { input });
  
  try {
    // Process input
    const result = await processInput(input);
    
    // Info logging
    context.logger.info('Input processed successfully', {
      input,
      result
    });
    
    return result;
  } catch (error) {
    // Error logging
    context.logger.error('Failed to process input', {
      input,
      error: error.message,
      stack: error.stack
    });
    throw error;
  }
};
```

### 2. Metrics

```typescript
export const metricFunction: NeoFunction = async (input, context) => {
  // Start timer
  const timer = context.metrics.startTimer();
  
  try {
    // Track invocation
    context.metrics.increment('function.invocations');
    
    // Process input
    const result = await processInput(input);
    
    // Track success
    context.metrics.increment('function.success');
    
    return result;
  } catch (error) {
    // Track failure
    context.metrics.increment('function.errors');
    throw error;
  } finally {
    // End timer
    timer.end();
  }
};
```

## Best Practices

1. **Function Design**
   - Keep functions small and focused
   - Follow single responsibility principle
   - Use proper error handling
   - Implement input validation
   - Add comprehensive logging

2. **Performance**
   - Optimize cold starts
   - Use caching when appropriate
   - Implement timeouts
   - Monitor resource usage
   - Profile performance bottlenecks

3. **Security**
   - Validate all inputs
   - Use proper authentication
   - Handle secrets securely
   - Follow least privilege principle
   - Implement rate limiting

4. **Testing**
   - Write comprehensive tests
   - Use proper mocking
   - Test edge cases
   - Maintain high coverage
   - Implement integration tests

5. **Monitoring**
   - Use proper logging levels
   - Track relevant metrics
   - Set up alerts
   - Monitor performance
   - Track error rates

## Support

For function development support:
- Email: dev@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/will/neo_service_layer/issues) 