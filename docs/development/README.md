# Neo N3 Service Layer Development Guide

## Overview

This guide provides comprehensive information about developing with the Neo N3 Service Layer. It covers local development setup, debugging tools, testing frameworks, and best practices for building serverless functions and triggers.

## Development Environment

### 1. Local Setup

```bash
# Install Neo Service Layer CLI
npm install -g @neo-service/cli

# Initialize new project
neo-service init my-project
cd my-project

# Install dependencies
npm install

# Start local development server
neo-service dev
```

### 2. Project Structure

```
my-project/
├── config/
│   ├── config.yaml           # Main configuration
│   ├── functions.yaml        # Functions configuration
│   └── triggers.yaml         # Triggers configuration
├── functions/
│   ├── payment/
│   │   ├── index.ts         # Function code
│   │   ├── schema.ts        # Input/output schema
│   │   └── test.ts         # Unit tests
│   └── notification/
│       ├── index.ts
│       ├── schema.ts
│       └── test.ts
├── triggers/
│   ├── daily-report/
│   │   ├── index.ts         # Trigger definition
│   │   └── test.ts         # Trigger tests
│   └── price-alert/
│       ├── index.ts
│       └── test.ts
├── test/
│   ├── integration/         # Integration tests
│   └── e2e/                # End-to-end tests
├── package.json
└── README.md
```

## Function Development

### 1. Creating Functions

```typescript
// functions/payment/index.ts
import { NeoFunction } from '@neo-service/sdk';

interface PaymentInput {
  amount: number;
  recipient: string;
  asset: string;
}

interface PaymentOutput {
  success: boolean;
  transactionHash: string;
}

export const processPayment: NeoFunction<PaymentInput, PaymentOutput> = async (
  input,
  context
) => {
  // Validate input
  if (input.amount <= 0) {
    throw new Error('Invalid amount');
  }

  // Get blockchain instance
  const { blockchain } = context;

  // Execute transaction
  const tx = await blockchain.transfer({
    to: input.recipient,
    amount: input.amount,
    asset: input.asset
  });

  // Return result
  return {
    success: true,
    transactionHash: tx.hash
  };
};
```

### 2. Function Configuration

```yaml
# config/functions.yaml
functions:
  payment:
    handler: functions/payment/index.processPayment
    runtime: node18
    memory: 256
    timeout: 30s
    environment:
      NODE_ENV: development
    permissions:
      - blockchain:transfer
    retry:
      attempts: 3
      backoff: exponential
```

### 3. Function Testing

```typescript
// functions/payment/test.ts
import { TestClient } from '@neo-service/testing';
import { processPayment } from './index';

describe('Payment Function', () => {
  let client: TestClient;

  beforeEach(async () => {
    client = await TestClient.create();
  });

  afterEach(async () => {
    await client.cleanup();
  });

  it('should process payment successfully', async () => {
    // Arrange
    const input = {
      amount: 100,
      recipient: 'Neo1...',
      asset: 'NEO'
    };

    // Mock blockchain
    client.blockchain
      .mockTransfer()
      .withArgs({
        to: input.recipient,
        amount: input.amount,
        asset: input.asset
      })
      .returns({
        hash: '0x...'
      });

    // Act
    const result = await processPayment(input, client.context);

    // Assert
    expect(result.success).toBe(true);
    expect(result.transactionHash).toBeDefined();
  });

  it('should handle invalid amount', async () => {
    // Arrange
    const input = {
      amount: -100,
      recipient: 'Neo1...',
      asset: 'NEO'
    };

    // Act & Assert
    await expect(processPayment(input, client.context))
      .rejects
      .toThrow('Invalid amount');
  });
});
```

## Trigger Development

### 1. Creating Triggers

```typescript
// triggers/price-alert/index.ts
import { NeoTrigger } from '@neo-service/sdk';

interface PriceAlert {
  symbol: string;
  threshold: number;
  direction: 'above' | 'below';
}

export const priceAlert: NeoTrigger<PriceAlert> = {
  type: 'schedule',
  schedule: '*/5 * * * *',
  
  async handler(context) {
    const { priceFeed, functions } = context;
    
    // Get current price
    const price = await priceFeed.getPrice(context.data.symbol);
    
    // Check threshold
    const isTriggered = context.data.direction === 'above'
      ? price > context.data.threshold
      : price < context.data.threshold;
    
    if (isTriggered) {
      // Execute notification function
      await functions.execute('notification', {
        message: `Price alert: ${context.data.symbol} is ${context.data.direction} ${context.data.threshold}`
      });
    }
  }
};
```

### 2. Trigger Configuration

```yaml
# config/triggers.yaml
triggers:
  price-alert:
    handler: triggers/price-alert/index.priceAlert
    environment:
      PRICE_FEED_API: https://api.example.com
    permissions:
      - price-feed:read
      - functions:execute
```

### 3. Trigger Testing

```typescript
// triggers/price-alert/test.ts
import { TestClient } from '@neo-service/testing';
import { priceAlert } from './index';

describe('Price Alert Trigger', () => {
  let client: TestClient;

  beforeEach(async () => {
    client = await TestClient.create();
  });

  afterEach(async () => {
    await client.cleanup();
  });

  it('should trigger notification when price is above threshold', async () => {
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
    await priceAlert.handler(client.createContext(data));

    // Assert
    expect(executeMock).toHaveBeenCalledWith(
      'notification',
      expect.objectContaining({
        message: expect.stringContaining('above 50')
      })
    );
  });
});
```

## Development Tools

### 1. CLI Commands

```bash
# Create new function
neo-service create function payment

# Create new trigger
neo-service create trigger daily-report

# Deploy function
neo-service deploy function payment

# Test function
neo-service test function payment

# View function logs
neo-service logs function payment

# Monitor function metrics
neo-service metrics function payment
```

### 2. Development Server

```bash
# Start development server
neo-service dev

# Start with specific configuration
neo-service dev --config custom-config.yaml

# Start with debugging
neo-service dev --debug

# Start with function watching
neo-service dev --watch
```

### 3. Debugging Tools

```typescript
// Enable debug logging
context.logger.debug('Processing payment', {
  amount: input.amount,
  recipient: input.recipient
});

// Profile execution time
const timer = context.metrics.startTimer();
try {
  // Function logic
} finally {
  timer.end();
}

// Track custom metrics
context.metrics.increment('payment_processed');
context.metrics.gauge('payment_amount', input.amount);
```

## Local Development

### 1. Environment Variables

```bash
# .env.development
NEO_RPC_URL=http://localhost:10332
REDIS_URL=redis://localhost:6379
DB_URL=postgresql://localhost:5432/neo_service
```

### 2. Local Services

```yaml
# docker-compose.yaml
version: '3'
services:
  neo-node:
    image: neo-node
    ports:
      - "10332:10332"
  
  redis:
    image: redis
    ports:
      - "6379:6379"
  
  postgres:
    image: postgres
    ports:
      - "5432:5432"
    environment:
      POSTGRES_DB: neo_service
```

### 3. Development Configuration

```yaml
# config/development.yaml
blockchain:
  network: private
  rpc: http://localhost:10332

database:
  host: localhost
  port: 5432
  name: neo_service

cache:
  host: localhost
  port: 6379
```

## Best Practices

1. **Function Development**
   - Keep functions focused
   - Handle errors properly
   - Use input validation
   - Implement proper logging

2. **Trigger Development**
   - Define clear conditions
   - Handle edge cases
   - Implement retries
   - Monitor execution

3. **Testing**
   - Write comprehensive tests
   - Use mocking effectively
   - Test edge cases
   - Maintain test coverage

4. **Performance**
   - Optimize resource usage
   - Implement caching
   - Monitor execution time
   - Profile bottlenecks

5. **Security**
   - Validate inputs
   - Handle secrets properly
   - Implement access control
   - Follow best practices

6. **Monitoring**
   - Use proper logging
   - Track metrics
   - Set up alerts
   - Monitor resources

## Support

For development support:
- Email: dev@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/will/neo_service_layer/issues)

## Additional Resources

- [Function Reference](./FUNCTIONS.md)
- [Trigger Reference](./TRIGGERS.md)
- [Testing Guide](./TESTING.md)
- [Debugging Guide](./DEBUGGING.md)
- [Performance Guide](./PERFORMANCE.md) 