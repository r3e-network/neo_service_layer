# Neo N3 Service Layer Trigger Reference

## Overview

This reference guide provides detailed information about creating, configuring, and managing triggers in the Neo N3 Service Layer. Triggers allow you to automate function execution based on various events and conditions.

## Trigger Types

### 1. Schedule Triggers

Schedule triggers execute functions based on time intervals using cron expressions.

```typescript
import { NeoTrigger } from '@neo-service/sdk';

interface ScheduleData {
  // Custom data for scheduled execution
}

export const scheduledTask: NeoTrigger<ScheduleData> = {
  type: 'schedule',
  schedule: '0 0 * * *', // Daily at midnight
  
  async handler(context) {
    // Handle scheduled execution
    await context.functions.execute('dailyReport', {
      date: new Date().toISOString()
    });
  }
};
```

### 2. Event Triggers

Event triggers execute functions in response to blockchain events.

```typescript
import { NeoTrigger } from '@neo-service/sdk';

interface TokenTransferEvent {
  from: string;
  to: string;
  amount: number;
  asset: string;
}

export const tokenTransferTrigger: NeoTrigger<TokenTransferEvent> = {
  type: 'event',
  contract: 'TokenContract',
  event: 'Transfer',
  
  async handler(context) {
    // Handle transfer event
    await context.functions.execute('notifyTransfer', {
      from: context.data.from,
      to: context.data.to,
      amount: context.data.amount,
      asset: context.data.asset
    });
  }
};
```

### 3. HTTP Triggers

HTTP triggers execute functions in response to HTTP requests.

```typescript
import { NeoTrigger } from '@neo-service/sdk';

interface WebhookData {
  method: string;
  path: string;
  headers: Record<string, string>;
  body: any;
}

export const webhookTrigger: NeoTrigger<WebhookData> = {
  type: 'http',
  method: 'POST',
  path: '/webhook',
  
  async handler(context) {
    // Validate webhook signature
    if (!validateSignature(context.data.headers)) {
      throw new Error('Invalid signature');
    }
    
    // Process webhook
    await context.functions.execute('processWebhook', {
      payload: context.data.body
    });
  }
};
```

### 4. Price Triggers

Price triggers execute functions based on price conditions.

```typescript
import { NeoTrigger } from '@neo-service/sdk';

interface PriceAlert {
  symbol: string;
  threshold: number;
  direction: 'above' | 'below';
}

export const priceAlertTrigger: NeoTrigger<PriceAlert> = {
  type: 'price',
  symbol: 'NEO/USD',
  interval: '5m',
  
  async handler(context) {
    const price = await context.priceFeed.getPrice(context.data.symbol);
    
    const isTriggered = context.data.direction === 'above'
      ? price > context.data.threshold
      : price < context.data.threshold;
    
    if (isTriggered) {
      await context.functions.execute('sendAlert', {
        message: `Price ${context.data.direction} ${context.data.threshold}`
      });
    }
  }
};
```

## Trigger Configuration

### 1. Basic Configuration

```yaml
triggers:
  dailyReport:
    handler: triggers/dailyReport/index.scheduledTask
    schedule: '0 0 * * *'
    enabled: true
```

### 2. Event Configuration

```yaml
triggers:
  tokenTransfer:
    handler: triggers/tokenTransfer/index.tokenTransferTrigger
    contract: TokenContract
    event: Transfer
    filters:
      amount:
        min: 1000
```

### 3. HTTP Configuration

```yaml
triggers:
  webhook:
    handler: triggers/webhook/index.webhookTrigger
    method: POST
    path: /webhook
    auth:
      type: hmac
      secret: ${secrets.WEBHOOK_SECRET}
```

### 4. Price Configuration

```yaml
triggers:
  priceAlert:
    handler: triggers/priceAlert/index.priceAlertTrigger
    symbol: NEO/USD
    interval: 5m
    conditions:
      - type: threshold
        value: 50
        operator: gt
```

### 5. Retry Configuration

```yaml
triggers:
  myTrigger:
    retry:
      attempts: 3
      backoff: exponential
      initialDelay: 1s
      maxDelay: 30s
```

### 6. Concurrency Configuration

```yaml
triggers:
  myTrigger:
    concurrency:
      limit: 10
      strategy: fifo
```

## Trigger Context

The trigger context provides access to various services and utilities:

```typescript
interface TriggerContext<T> {
  // Trigger data
  data: T;
  
  // Function execution
  functions: {
    execute<T>(name: string, input: any): Promise<T>;
  };
  
  // Blockchain interaction
  blockchain: {
    getTransaction(hash: string): Promise<Transaction>;
    getBlock(hash: string): Promise<Block>;
  };
  
  // Price feed
  priceFeed: {
    getPrice(symbol: string): Promise<number>;
    getHistory(symbol: string, interval: string): Promise<PriceHistory>;
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
}
```

## Testing Triggers

### 1. Unit Testing

```typescript
import { TestClient } from '@neo-service/testing';
import { priceAlertTrigger } from './index';

describe('Price Alert Trigger', () => {
  let client: TestClient;
  
  beforeEach(async () => {
    client = await TestClient.create();
  });
  
  afterEach(async () => {
    await client.cleanup();
  });
  
  it('should trigger alert when price is above threshold', async () => {
    // Arrange
    const data = {
      symbol: 'NEO/USD',
      threshold: 50,
      direction: 'above'
    };
    
    // Mock price feed
    client.priceFeed
      .mockGetPrice()
      .withArgs('NEO/USD')
      .returns(60);
    
    // Mock function execution
    const executeMock = client.functions.mockExecute();
    
    // Act
    await priceAlertTrigger.handler(client.createContext(data));
    
    // Assert
    expect(executeMock).toHaveBeenCalledWith(
      'sendAlert',
      expect.any(Object)
    );
  });
});
```

### 2. Integration Testing

```typescript
import { IntegrationTestClient } from '@neo-service/testing';

describe('Price Alert Trigger Integration', () => {
  let client: IntegrationTestClient;
  
  beforeAll(async () => {
    client = await IntegrationTestClient.create({
      configPath: './config/test.yaml'
    });
  });
  
  afterAll(async () => {
    await client.cleanup();
  });
  
  it('should integrate with price feed and functions', async () => {
    // Arrange
    const trigger = {
      symbol: 'NEO/USD',
      threshold: 50,
      direction: 'above'
    };
    
    // Act
    await client.triggers.create('priceAlert', trigger);
    
    // Wait for trigger execution
    await client.triggers.waitForExecution('priceAlert');
    
    // Assert
    const executions = await client.triggers.getExecutions('priceAlert');
    expect(executions).toHaveLength(1);
  });
});
```

## Deployment

### 1. Development

```bash
# Deploy single trigger
neo-service deploy trigger priceAlert

# Deploy with specific config
neo-service deploy trigger priceAlert --config dev.yaml

# Deploy with environment variables
neo-service deploy trigger priceAlert --env-file .env.dev
```

### 2. Production

```bash
# Deploy to production
neo-service deploy trigger priceAlert --stage prod

# Deploy with specific version
neo-service deploy trigger priceAlert --version 1.0.0

# Deploy with gradual rollout
neo-service deploy trigger priceAlert --rollout 10
```

## Monitoring

### 1. Logging

```typescript
export const monitoredTrigger: NeoTrigger = {
  type: 'schedule',
  schedule: '*/5 * * * *',
  
  async handler(context) {
    // Debug logging
    context.logger.debug('Processing trigger', {
      data: context.data
    });
    
    try {
      // Process trigger
      const result = await processTrigger(context);
      
      // Info logging
      context.logger.info('Trigger processed successfully', {
        data: context.data,
        result
      });
    } catch (error) {
      // Error logging
      context.logger.error('Failed to process trigger', {
        data: context.data,
        error: error.message,
        stack: error.stack
      });
      throw error;
    }
  }
};
```

### 2. Metrics

```typescript
export const metricTrigger: NeoTrigger = {
  type: 'event',
  contract: 'TokenContract',
  event: 'Transfer',
  
  async handler(context) {
    // Start timer
    const timer = context.metrics.startTimer();
    
    try {
      // Track execution
      context.metrics.increment('trigger.executions');
      
      // Process event
      await processEvent(context);
      
      // Track success
      context.metrics.increment('trigger.success');
    } catch (error) {
      // Track failure
      context.metrics.increment('trigger.errors');
      throw error;
    } finally {
      // End timer
      timer.end();
    }
  }
};
```

## Best Practices

1. **Trigger Design**
   - Keep triggers focused
   - Handle edge cases
   - Implement proper error handling
   - Use appropriate retry strategies
   - Monitor execution time

2. **Performance**
   - Optimize trigger conditions
   - Use efficient filters
   - Implement caching
   - Monitor resource usage
   - Handle backpressure

3. **Security**
   - Validate trigger data
   - Implement authentication
   - Use secure configurations
   - Follow least privilege
   - Monitor for abuse

4. **Testing**
   - Write comprehensive tests
   - Test edge cases
   - Use proper mocking
   - Test integrations
   - Maintain test coverage

5. **Monitoring**
   - Use proper logging
   - Track metrics
   - Set up alerts
   - Monitor latency
   - Track error rates

## Support

For trigger development support:
- Email: dev@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/will/neo_service_layer/issues) 