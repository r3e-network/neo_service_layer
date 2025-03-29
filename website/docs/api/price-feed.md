# Price Feed Service API

## Overview

The Price Feed Service provides reliable, real-time price data for Neo N3 assets through sophisticated aggregation and filtering mechanisms. It implements advanced statistical methods, including Kalman filtering and adaptive noise estimation, to ensure high-quality price feeds.

## Features

- Multi-source price aggregation
- Kalman filtering with adaptive parameters
- Outlier detection and handling
- Historical accuracy tracking
- Automatic blockchain updates
- Comprehensive monitoring

## Installation

```typescript
import { PriceFeedService } from '@neo-service-layer/price-feed';

const priceFeed = new PriceFeedService({
  teeEnabled: true,
  contractService: neoContract,
  vault: secretVault
});
```

## Configuration

### PriceFeedServiceConfig

```typescript
interface PriceFeedServiceConfig {
  /** Whether to enable Trusted Execution Environment */
  teeEnabled: boolean;
  /** Service for interacting with Neo smart contracts */
  contractService: NeoContractService;
  /** Secure storage for sensitive data */
  vault: SecretVault;
}
```

### DataSourceConfig

```typescript
interface DataSourceConfig {
  /** Unique identifier for the data source */
  name: string;
  /** Base weight for price aggregation (0-1) */
  weight: number;
  /** Base URL for the price API */
  baseUrl: string;
  /** Reference to the API key in the vault */
  apiKeySecret: string;
  /** Request timeout in milliseconds */
  timeout: number;
  /** Maximum requests per minute */
  rateLimit: number;
}
```

## API Reference

### getAggregatedPrice

Retrieves the current aggregated price for a trading pair.

```typescript
async getAggregatedPrice(
  symbol: string,
  preferredSource?: string
): Promise<PriceData>
```

#### Parameters

- `symbol` (string) - Trading pair symbol (e.g., 'NEO/USD')
- `preferredSource` (string, optional) - Preferred data source name

#### Returns

```typescript
interface PriceData {
  /** Trading pair symbol */
  symbol: string;
  /** Current price value */
  price: number;
  /** ISO timestamp of the price update */
  timestamp: string;
  /** Data source identifier */
  source: string;
  /** Confidence score (0-1) */
  confidence: number;
  /** Additional price metadata */
  details: {
    /** 24-hour trading volume */
    volume24h?: number;
    /** Total market capitalization */
    marketCap?: number;
    /** Last update timestamp */
    lastUpdate: string;
    /** Contributing price sources */
    sources: Array<{
      /** Source identifier */
      name: string;
      /** Source-specific price */
      price: number;
      /** Source weight in aggregation */
      weight: number;
      /** Source-specific timestamp */
      timestamp: string;
    }>;
  };
}
```

#### Example

```typescript
const priceData = await priceFeed.getAggregatedPrice('NEO/USD');
console.log(`Current price: $${priceData.price} (confidence: ${priceData.confidence})`);

// With preferred source
const binancePriceData = await priceFeed.getAggregatedPrice('NEO/USD', 'binance');
```

### Error Handling

The service uses custom error types for different failure scenarios:

```typescript
class PriceFeedError extends Error {
  constructor(
    message: string,
    public readonly code: string,
    public readonly details?: Record<string, unknown>
  ) {
    super(message);
    this.name = 'PriceFeedError';
  }
}

// Usage example
try {
  const price = await priceFeed.getAggregatedPrice('NEO/USD');
} catch (error) {
  if (error instanceof PriceFeedError) {
    console.error(`Price feed error: ${error.code}`, error.details);
  }
  throw error;
}
```

## Monitoring

### Metrics

The service exposes the following Prometheus metrics:

```typescript
// Price accuracy metrics
price_feed_accuracy_gauge{symbol="NEO/USD"} 0.95
price_feed_confidence_gauge{symbol="NEO/USD"} 0.87

// Latency metrics
price_fetch_duration_seconds{source="binance"} 0.123
price_update_chain_duration_seconds{symbol="NEO/USD"} 0.456

// Error metrics
price_fetch_errors_total{source="binance"} 2
price_update_chain_errors_total{symbol="NEO/USD"} 1
```

### Logging

The service implements structured logging:

```typescript
// Price update success
logger.info('Price update completed', {
  symbol: 'NEO/USD',
  price: 12.34,
  confidence: 0.95,
  sources: ['binance', 'huobi']
});

// Price update failure
logger.error('Failed to update price', {
  symbol: 'NEO/USD',
  error: 'API_TIMEOUT',
  retryCount: 3
});
```

## Advanced Usage

### Custom Data Source Integration

```typescript
interface CustomDataSource extends DataSourceConfig {
  transform: (data: unknown) => Promise<number>;
  validate: (price: number) => boolean;
}

const customSource: CustomDataSource = {
  name: 'custom_exchange',
  weight: 1.0,
  baseUrl: 'https://api.custom.exchange',
  apiKeySecret: 'CUSTOM_API_KEY',
  timeout: 5000,
  rateLimit: 60,
  transform: async (data) => {
    // Custom price transformation logic
    return parseFloat(data.last_price);
  },
  validate: (price) => {
    // Custom validation logic
    return price > 0 && price < 1000;
  }
};
```

### Kalman Filter Configuration

```typescript
interface KalmanConfig {
  /** Base process noise parameter */
  baseProcessNoise: number;
  /** Base measurement noise parameter */
  baseMeasurementNoise: number;
  /** Enable multi-state tracking */
  multiStateEnabled: boolean;
  /** Adaptation rate for noise parameters */
  adaptationRate: number;
}

// Example configuration
const kalmanConfig: KalmanConfig = {
  baseProcessNoise: 0.001,
  baseMeasurementNoise: 0.1,
  multiStateEnabled: true,
  adaptationRate: 0.1
};
```

## Best Practices

1. **Error Handling**
   ```typescript
   try {
     const price = await priceFeed.getAggregatedPrice('NEO/USD');
     if (price.confidence < 0.8) {
       logger.warn('Low confidence price update', {
         symbol: 'NEO/USD',
         confidence: price.confidence
       });
     }
   } catch (error) {
     // Handle specific error types
     if (error instanceof PriceFeedError) {
       // Handle price feed specific errors
     }
     throw error;
   }
   ```

2. **Rate Limiting**
   ```typescript
   const rateLimiter = new RateLimiter({
     maxRequests: 60,
     interval: 60 * 1000 // 1 minute
   });

   async function getPriceWithRateLimit(symbol: string): Promise<PriceData> {
     await rateLimiter.acquire();
     return priceFeed.getAggregatedPrice(symbol);
   }
   ```

3. **Caching**
   ```typescript
   const cache = new Cache({
     maxAge: 60 * 1000, // 1 minute
     maxSize: 1000
   });

   async function getCachedPrice(symbol: string): Promise<PriceData> {
     const cached = cache.get(symbol);
     if (cached) return cached;

     const price = await priceFeed.getAggregatedPrice(symbol);
     cache.set(symbol, price);
     return price;
   }
   ```

## Testing

```typescript
describe('PriceFeedService', () => {
  let priceFeed: PriceFeedService;

  beforeEach(() => {
    priceFeed = new PriceFeedService({
      teeEnabled: false,
      contractService: mockContract,
      vault: mockVault
    });
  });

  it('should aggregate prices correctly', async () => {
    const price = await priceFeed.getAggregatedPrice('NEO/USD');
    expect(price.confidence).toBeGreaterThan(0.8);
    expect(price.details.sources.length).toBeGreaterThan(1);
  });

  it('should handle source failures gracefully', async () => {
    // Mock source failure
    mockSource.fail = true;
    const price = await priceFeed.getAggregatedPrice('NEO/USD');
    expect(price.confidence).toBeLessThan(1.0);
  });
});
```

## References

- [Neo N3 Documentation](https://docs.neo.org/)
- [Kalman Filter Theory](https://en.wikipedia.org/wiki/Kalman_filter)
- [Price Feed Best Practices](https://docs.neo.org/docs/en-us/develop/network/oracle/price-feed.html) 