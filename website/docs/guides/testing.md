# Price Feed Service Testing Guide

## Overview

This guide outlines the testing strategy and practices for the Price Feed Service. It covers unit tests, integration tests, performance tests, and monitoring in production.

## Test Structure

The test suite is organized into the following directories:

```
tests/
├── unit/
│   ├── aggregation.test.ts
│   ├── kalman-filter.test.ts
│   ├── price-sources.test.ts
│   └── validation.test.ts
├── integration/
│   ├── api-sources.test.ts
│   ├── blockchain.test.ts
│   └── monitoring.test.ts
├── performance/
│   ├── load-test.ts
│   └── stress-test.ts
└── mocks/
    ├── price-data.ts
    └── sources.ts
```

## Unit Testing

### Price Aggregation Tests

```typescript
import { PriceAggregator } from '../src/aggregation';
import { mockPriceData } from './mocks/price-data';

describe('PriceAggregator', () => {
  let aggregator: PriceAggregator;

  beforeEach(() => {
    aggregator = new PriceAggregator({
      minConfidence: 0.8,
      outlierThreshold: 2.0
    });
  });

  test('should calculate weighted average correctly', () => {
    const prices = [
      { price: 100, weight: 1.0, source: 'source1' },
      { price: 102, weight: 0.8, source: 'source2' },
      { price: 101, weight: 0.9, source: 'source3' }
    ];

    const result = aggregator.calculateWeightedPrice(prices);
    expect(result.price).toBeCloseTo(100.89, 2);
    expect(result.confidence).toBeGreaterThan(0.8);
  });

  test('should detect outliers', () => {
    const prices = [
      { price: 100, weight: 1.0, source: 'source1' },
      { price: 150, weight: 0.8, source: 'source2' }, // outlier
      { price: 101, weight: 0.9, source: 'source3' }
    ];

    const result = aggregator.calculateWeightedPrice(prices);
    expect(result.details.outliers).toContain('source2');
    expect(result.confidence).toBeLessThan(1.0);
  });
});
```

### Kalman Filter Tests

```typescript
import { KalmanFilter } from '../src/kalman';

describe('KalmanFilter', () => {
  let filter: KalmanFilter;

  beforeEach(() => {
    filter = new KalmanFilter({
      baseProcessNoise: 0.001,
      baseMeasurementNoise: 0.1,
      multiStateEnabled: true
    });
  });

  test('should initialize state correctly', () => {
    const state = filter.initializeState(100);
    expect(state.price).toBe(100);
    expect(state.velocity).toBe(0);
    expect(state.acceleration).toBe(0);
  });

  test('should predict next state', () => {
    const state = filter.initializeState(100);
    const prediction = filter.predict(state, 1.0);
    expect(prediction.price).toBeCloseTo(100, 4);
    expect(prediction.covariance[0][0]).toBeGreaterThan(state.covariance[0][0]);
  });

  test('should update state with measurement', () => {
    const state = filter.initializeState(100);
    const updated = filter.update(state, 102);
    expect(updated.price).toBeGreaterThan(100);
    expect(updated.velocity).toBeGreaterThan(0);
  });
});
```

### Validation Tests

```typescript
import { validatePrice, validateSymbol } from '../src/validation';

describe('Input Validation', () => {
  test('should validate price format', () => {
    expect(validatePrice(100.50)).toBe(true);
    expect(validatePrice(-1)).toBe(false);
    expect(validatePrice(1000001)).toBe(false);
  });

  test('should validate symbol format', () => {
    expect(validateSymbol('NEO/USD')).toBe(true);
    expect(validateSymbol('neo/usd')).toBe(false);
    expect(validateSymbol('NEO-USD')).toBe(false);
  });
});
```

## Integration Testing

### API Source Tests

```typescript
import { PriceSource } from '../src/sources';
import { mockApiResponses } from './mocks/sources';

describe('Price Sources Integration', () => {
  let source: PriceSource;

  beforeEach(() => {
    source = new PriceSource({
      name: 'binance',
      baseUrl: 'https://api.binance.com/api/v3'
    });
  });

  test('should fetch price from Binance', async () => {
    const price = await source.fetchPrice('NEO/USD');
    expect(price).toHaveProperty('price');
    expect(price).toHaveProperty('timestamp');
    expect(price.source).toBe('binance');
  });

  test('should handle API errors gracefully', async () => {
    // Mock API failure
    mockApiResponses.mockFailureOnce();
    
    await expect(source.fetchPrice('NEO/USD'))
      .rejects
      .toThrow('API_ERROR');
  });

  test('should respect rate limits', async () => {
    const promises = Array(10).fill(null).map(() => 
      source.fetchPrice('NEO/USD')
    );
    
    const results = await Promise.allSettled(promises);
    const rateLimited = results.some(r => 
      r.status === 'rejected' && 
      r.reason.code === 'RATE_LIMIT'
    );
    
    expect(rateLimited).toBe(true);
  });
});
```

### Blockchain Integration Tests

```typescript
import { BlockchainUpdater } from '../src/blockchain';
import { mockContract } from './mocks/contract';

describe('Blockchain Integration', () => {
  let updater: BlockchainUpdater;

  beforeEach(() => {
    updater = new BlockchainUpdater({
      contract: mockContract,
      network: 'testnet'
    });
  });

  test('should update price on chain', async () => {
    const result = await updater.updatePrice('NEO/USD', 100.50);
    expect(result.txid).toBeDefined();
    expect(result.status).toBe('confirmed');
  });

  test('should handle transaction failures', async () => {
    mockContract.mockTransactionFailure();
    
    await expect(updater.updatePrice('NEO/USD', 100.50))
      .rejects
      .toThrow('BLOCKCHAIN_ERROR');
  });
});
```

### Monitoring Integration Tests

```typescript
import { MetricsRecorder } from '../src/monitoring';

describe('Monitoring Integration', () => {
  let metrics: MetricsRecorder;

  beforeEach(() => {
    metrics = new MetricsRecorder({
      prefix: 'test_price_feed',
      port: 9090
    });
  });

  test('should record price metrics', async () => {
    await metrics.recordPrice({
      symbol: 'NEO/USD',
      price: 100.50,
      confidence: 0.95
    });

    const recorded = await metrics.getMetric('price_value');
    expect(recorded.value).toBe(100.50);
  });

  test('should record error metrics', async () => {
    await metrics.recordError('source_error', {
      source: 'binance',
      type: 'timeout'
    });

    const errors = await metrics.getMetric('error_total');
    expect(errors.value).toBeGreaterThan(0);
  });
});
```

## Performance Testing

### Load Test Configuration

```typescript
import { LoadTest } from '../src/testing/load';

const loadConfig = {
  duration: 300,  // seconds
  rampUp: 30,     // seconds
  targetRPS: 100, // requests per second
  scenarios: [
    {
      name: 'price_fetch',
      weight: 0.8,
      action: async () => {
        await priceFeed.getAggregatedPrice('NEO/USD');
      }
    },
    {
      name: 'price_update',
      weight: 0.2,
      action: async () => {
        await priceFeed.updatePriceOnChain('NEO/USD');
      }
    }
  ]
};

describe('Load Testing', () => {
  test('should handle target RPS', async () => {
    const results = await LoadTest.run(loadConfig);
    
    expect(results.successRate).toBeGreaterThan(0.99);
    expect(results.p95Latency).toBeLessThan(100);
    expect(results.errorRate).toBeLessThan(0.01);
  });
});
```

### Stress Test Configuration

```typescript
import { StressTest } from '../src/testing/stress';

const stressConfig = {
  maxRPS: 500,
  stepSize: 50,
  stepDuration: 60,  // seconds
  failureThreshold: {
    errorRate: 0.05,
    p95Latency: 200
  }
};

describe('Stress Testing', () => {
  test('should identify breaking point', async () => {
    const results = await StressTest.run(stressConfig);
    
    expect(results.breakingPoint).toBeGreaterThan(200);
    expect(results.maxLatency).toBeDefined();
    expect(results.resourceUtilization).toBeDefined();
  });
});
```

## Test Coverage

Maintain high test coverage using Jest:

```bash
# Run tests with coverage report
npm run test:coverage

# Coverage thresholds in jest.config.js
module.exports = {
  coverageThreshold: {
    global: {
      statements: 85,
      branches: 80,
      functions: 85,
      lines: 85
    }
  }
};
```

## Continuous Integration

### GitHub Actions Workflow

```yaml
name: Test Suite

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v2
      
      - name: Setup Node.js
        uses: actions/setup-node@v2
        with:
          node-version: '18'
          
      - name: Install dependencies
        run: npm ci
        
      - name: Run linter
        run: npm run lint
        
      - name: Run unit tests
        run: npm run test:unit
        
      - name: Run integration tests
        run: npm run test:integration
        
      - name: Run performance tests
        run: npm run test:performance
        
      - name: Upload coverage
        uses: codecov/codecov-action@v2
```

## Best Practices

1. **Test Isolation**
   - Reset state before each test
   - Mock external dependencies
   - Use unique test data

2. **Test Organization**
   - Group related tests
   - Use descriptive names
   - Follow AAA pattern (Arrange, Act, Assert)

3. **Mock Data Management**
   - Centralize mock data
   - Version control test data
   - Document data generation

4. **Performance Testing**
   - Test in production-like environment
   - Monitor resource usage
   - Include cleanup procedures

5. **Continuous Monitoring**
   - Track test metrics
   - Monitor coverage trends
   - Review test performance

## References

1. [Jest Documentation](https://jestjs.io/docs/getting-started)
2. [TypeScript Testing](https://www.typescriptlang.org/docs/handbook/testing.html)
3. [Performance Testing Guide](https://k6.io/docs/)
4. [Test Coverage Tools](https://istanbul.js.org/) 