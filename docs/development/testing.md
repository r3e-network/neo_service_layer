# Neo N3 Service Layer Testing Guide

## Overview

This guide provides comprehensive information about testing practices, frameworks, and tools available in the Neo N3 Service Layer. It covers unit testing, integration testing, end-to-end testing, and performance testing.

## Testing Framework

The Neo N3 Service Layer uses Jest as its primary testing framework, along with custom testing utilities provided by `@neo-service/testing`.

### Installation

```bash
# Install testing dependencies
npm install --save-dev jest @types/jest @neo-service/testing

# Add test script to package.json
{
  "scripts": {
    "test": "jest",
    "test:watch": "jest --watch",
    "test:coverage": "jest --coverage"
  }
}
```

### Configuration

```javascript
// jest.config.js
module.exports = {
  preset: '@neo-service/testing/jest-preset',
  testEnvironment: 'node',
  testMatch: ['**/*.test.ts'],
  collectCoverageFrom: [
    'src/**/*.ts',
    '!src/**/*.d.ts'
  ],
  coverageThreshold: {
    global: {
      branches: 80,
      functions: 80,
      lines: 80,
      statements: 80
    }
  }
};
```

## Unit Testing

### 1. Function Testing

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
        hash: '0x...',
        status: 'success'
      });

    // Act
    const result = await processPayment(input, client.context);

    // Assert
    expect(result.success).toBe(true);
    expect(result.transactionHash).toBeDefined();
  });

  it('should handle invalid input', async () => {
    // Arrange
    const input = {
      amount: -100,
      recipient: '',
      asset: ''
    };

    // Act & Assert
    await expect(processPayment(input, client.context))
      .rejects
      .toThrow('Invalid input');
  });

  it('should handle blockchain errors', async () => {
    // Arrange
    const input = {
      amount: 100,
      recipient: 'Neo1...',
      asset: 'NEO'
    };

    // Mock blockchain error
    client.blockchain
      .mockTransfer()
      .rejects(new Error('Insufficient funds'));

    // Act & Assert
    await expect(processPayment(input, client.context))
      .rejects
      .toThrow('Insufficient funds');
  });
});
```

### 2. Trigger Testing

```typescript
// triggers/priceAlert/test.ts
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
    await priceAlert.handler(client.createContext(data));

    // Assert
    expect(executeMock).toHaveBeenCalledWith(
      'sendAlert',
      expect.objectContaining({
        message: expect.stringContaining('above 50')
      })
    );
  });
});
```

## Integration Testing

### 1. Setup

```typescript
// test/integration/setup.ts
import { IntegrationTestClient } from '@neo-service/testing';

export async function setupIntegrationTests() {
  const client = await IntegrationTestClient.create({
    configPath: './config/test.yaml',
    services: {
      blockchain: true,
      database: true,
      cache: true
    }
  });

  return client;
}
```

### 2. Function Integration Tests

```typescript
// test/integration/functions/payment.test.ts
import { IntegrationTestClient } from '@neo-service/testing';
import { setupIntegrationTests } from '../setup';

describe('Payment Function Integration', () => {
  let client: IntegrationTestClient;

  beforeAll(async () => {
    client = await setupIntegrationTests();
  });

  afterAll(async () => {
    await client.cleanup();
  });

  it('should integrate with blockchain and database', async () => {
    // Arrange
    const input = {
      amount: 100,
      recipient: 'Neo1...',
      asset: 'NEO'
    };

    // Act
    const result = await client.functions.execute('payment', input);

    // Assert
    expect(result.success).toBe(true);
    expect(result.transactionHash).toBeDefined();

    // Verify blockchain state
    const tx = await client.blockchain.getTransaction(result.transactionHash);
    expect(tx.status).toBe('confirmed');

    // Verify database state
    const record = await client.database.getPayment(result.transactionHash);
    expect(record.amount).toBe(input.amount);
  });
});
```

### 3. Trigger Integration Tests

```typescript
// test/integration/triggers/priceAlert.test.ts
import { IntegrationTestClient } from '@neo-service/testing';
import { setupIntegrationTests } from '../setup';

describe('Price Alert Trigger Integration', () => {
  let client: IntegrationTestClient;

  beforeAll(async () => {
    client = await setupIntegrationTests();
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

    // Create trigger
    await client.triggers.create('priceAlert', trigger);

    // Simulate price change
    await client.priceFeed.setPrice('NEO/USD', 60);

    // Wait for trigger execution
    await client.triggers.waitForExecution('priceAlert');

    // Assert
    const executions = await client.triggers.getExecutions('priceAlert');
    expect(executions).toHaveLength(1);

    // Verify alert was sent
    const alerts = await client.database.getAlerts();
    expect(alerts).toHaveLength(1);
    expect(alerts[0].message).toContain('above 50');
  });
});
```

## End-to-End Testing

### 1. Setup

```typescript
// test/e2e/setup.ts
import { E2ETestClient } from '@neo-service/testing';

export async function setupE2ETests() {
  const client = await E2ETestClient.create({
    baseUrl: process.env.API_URL,
    auth: {
      privateKey: process.env.TEST_PRIVATE_KEY
    }
  });

  return client;
}
```

### 2. API Tests

```typescript
// test/e2e/api/functions.test.ts
import { E2ETestClient } from '@neo-service/testing';
import { setupE2ETests } from '../setup';

describe('Functions API', () => {
  let client: E2ETestClient;

  beforeAll(async () => {
    client = await setupE2ETests();
  });

  it('should create and execute function', async () => {
    // Create function
    const createResponse = await client.api.post('/functions', {
      name: 'test-function',
      code: `
        export async function handler(input, context) {
          return { message: 'Hello ' + input.name };
        }
      `
    });

    expect(createResponse.status).toBe(201);

    // Execute function
    const executeResponse = await client.api.post('/functions/test-function/execute', {
      name: 'World'
    });

    expect(executeResponse.status).toBe(200);
    expect(executeResponse.data).toEqual({
      message: 'Hello World'
    });
  });
});
```

### 3. WebSocket Tests

```typescript
// test/e2e/websocket/triggers.test.ts
import { E2ETestClient } from '@neo-service/testing';
import { setupE2ETests } from '../setup';

describe('Triggers WebSocket', () => {
  let client: E2ETestClient;

  beforeAll(async () => {
    client = await setupE2ETests();
  });

  it('should receive trigger events', async () => {
    // Connect to WebSocket
    await client.ws.connect();

    // Subscribe to trigger events
    await client.ws.subscribe('triggers');

    // Create trigger
    await client.api.post('/triggers', {
      name: 'test-trigger',
      type: 'schedule',
      schedule: '* * * * *'
    });

    // Wait for trigger event
    const event = await client.ws.waitForEvent('trigger.executed');

    expect(event.triggerName).toBe('test-trigger');
    expect(event.status).toBe('success');
  });
});
```

## Performance Testing

### 1. Load Testing

```typescript
// test/performance/load.test.ts
import { LoadTestClient } from '@neo-service/testing';

describe('Load Testing', () => {
  let client: LoadTestClient;

  beforeAll(async () => {
    client = await LoadTestClient.create({
      baseUrl: process.env.API_URL
    });
  });

  it('should handle concurrent requests', async () => {
    const results = await client.load({
      endpoint: '/functions/payment/execute',
      method: 'POST',
      payload: {
        amount: 100,
        recipient: 'Neo1...',
        asset: 'NEO'
      },
      concurrency: 100,
      duration: '1m'
    });

    expect(results.successRate).toBeGreaterThan(99);
    expect(results.averageLatency).toBeLessThan(200);
  });
});
```

### 2. Stress Testing

```typescript
// test/performance/stress.test.ts
import { StressTestClient } from '@neo-service/testing';

describe('Stress Testing', () => {
  let client: StressTestClient;

  beforeAll(async () => {
    client = await StressTestClient.create({
      baseUrl: process.env.API_URL
    });
  });

  it('should handle increasing load', async () => {
    const results = await client.stress({
      endpoint: '/functions/payment/execute',
      method: 'POST',
      payload: {
        amount: 100,
        recipient: 'Neo1...',
        asset: 'NEO'
      },
      initialUsers: 10,
      maxUsers: 1000,
      incrementUsers: 10,
      incrementInterval: '10s',
      duration: '5m'
    });

    expect(results.breakingPoint).toBeGreaterThan(500);
  });
});
```

## Test Coverage

### 1. Coverage Configuration

```javascript
// jest.config.js
module.exports = {
  collectCoverage: true,
  coverageDirectory: 'coverage',
  coverageReporters: ['text', 'lcov', 'html'],
  collectCoverageFrom: [
    'src/**/*.ts',
    '!src/**/*.d.ts',
    '!src/**/*.test.ts'
  ],
  coverageThreshold: {
    global: {
      branches: 80,
      functions: 80,
      lines: 80,
      statements: 80
    }
  }
};
```

### 2. Coverage Reports

```bash
# Generate coverage report
npm run test:coverage

# View coverage report
open coverage/index.html
```

## Best Practices

1. **Test Organization**
   - Keep tests close to implementation
   - Use descriptive test names
   - Follow AAA pattern (Arrange, Act, Assert)
   - Group related tests
   - Maintain test independence

2. **Test Data**
   - Use meaningful test data
   - Avoid hardcoded values
   - Clean up test data
   - Use factories for complex objects
   - Handle test data versioning

3. **Mocking**
   - Mock external dependencies
   - Use meaningful mock responses
   - Verify mock interactions
   - Clean up mocks after tests
   - Document mock behavior

4. **Assertions**
   - Use specific assertions
   - Test edge cases
   - Verify error conditions
   - Check state changes
   - Validate side effects

5. **Performance**
   - Keep tests fast
   - Parallelize test execution
   - Cache test resources
   - Clean up resources
   - Monitor test execution time

## Support

For testing support:
- Email: dev@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/r3e-network/neo_service_layer/issues)
