# Price Feed Service Configuration Guide

## Overview

This guide provides detailed information on configuring the Price Feed Service for optimal performance and reliability. The service uses a hierarchical configuration system that allows for fine-tuning of all components.

## Configuration File Structure

The service uses a TypeScript-based configuration system. The main configuration file should be located at `config/price-feed.config.ts`:

```typescript
import { PriceFeedConfig } from '../types';

const config: PriceFeedConfig = {
  service: {
    name: 'price-feed',
    version: '1.2.0',
    environment: process.env.NODE_ENV || 'development'
  },
  tee: {
    enabled: true,
    provider: 'azure-confidential-computing',
    attestationUrl: process.env.TEE_ATTESTATION_URL
  },
  blockchain: {
    network: process.env.NEO_NETWORK || 'testnet',
    rpcUrl: process.env.NEO_RPC_URL,
    oracleContract: process.env.ORACLE_CONTRACT_HASH
  },
  monitoring: {
    prometheusPort: 9090,
    logLevel: 'info',
    metricsPrefix: 'price_feed'
  }
};

export default config;
```

## Environment Variables

Create a `.env` file in the project root with the following variables:

```bash
# Network Configuration
NODE_ENV=production
NEO_NETWORK=mainnet
NEO_RPC_URL=http://seed1.neo.org:10332
ORACLE_CONTRACT_HASH=0x123...abc

# TEE Configuration
TEE_ENABLED=true
TEE_ATTESTATION_URL=https://attest.azure.net

# API Keys (stored in vault)
BINANCE_API_KEY_SECRET=binance-api-key
HUOBI_API_KEY_SECRET=huobi-api-key
COINBASE_API_KEY_SECRET=coinbase-api-key

# Monitoring
PROMETHEUS_PORT=9090
LOG_LEVEL=info
```

## Data Source Configuration

Configure price data sources in `config/sources.config.ts`:

```typescript
import { DataSourceConfig } from '../types';

const sources: DataSourceConfig[] = [
  {
    name: 'binance',
    weight: 1.0,
    baseUrl: 'https://api.binance.com/api/v3',
    apiKeySecret: 'BINANCE_API_KEY',
    timeout: 5000,
    rateLimit: 60,
    endpoints: {
      price: '/ticker/price',
      depth: '/depth'
    }
  },
  {
    name: 'huobi',
    weight: 0.8,
    baseUrl: 'https://api.huobi.pro/market',
    apiKeySecret: 'HUOBI_API_KEY',
    timeout: 5000,
    rateLimit: 100,
    endpoints: {
      price: '/detail/merged',
      depth: '/depth'
    }
  }
];

export default sources;
```

## Kalman Filter Configuration

Fine-tune the Kalman filter in `config/kalman.config.ts`:

```typescript
import { KalmanConfig } from '../types';

const kalmanConfig: KalmanConfig = {
  // Base noise parameters
  baseProcessNoise: 0.001,
  baseMeasurementNoise: 0.1,
  
  // Multi-state tracking
  multiStateEnabled: true,
  stateSize: 3,  // [price, velocity, acceleration]
  
  // Adaptation parameters
  adaptationRate: 0.1,
  innovationThreshold: 3.0,
  
  // Update frequency
  updateIntervalMs: 1000,
  
  // Matrix initialization
  initialCovariance: [
    [1.0, 0.0, 0.0],
    [0.0, 0.1, 0.0],
    [0.0, 0.0, 0.01]
  ]
};

export default kalmanConfig;
```

## Price Aggregation Configuration

Configure price aggregation parameters in `config/aggregation.config.ts`:

```typescript
import { AggregationConfig } from '../types';

const aggregationConfig: AggregationConfig = {
  // Confidence thresholds
  minConfidence: 0.8,
  minSources: 2,
  
  // Statistical parameters
  outlierThreshold: 2.0,
  volatilityWindow: 20,
  
  // Weight adjustment
  accuracyWeight: 0.4,
  freshnessWeight: 0.3,
  volatilityWeight: 0.3,
  
  // Update thresholds
  minPriceChange: 0.001,
  maxPriceChange: 0.1,
  
  // Caching
  cacheDurationMs: 60000
};

export default aggregationConfig;
```

## Monitoring Configuration

Set up monitoring in `config/monitoring.config.ts`:

```typescript
import { MonitoringConfig } from '../types';

const monitoringConfig: MonitoringConfig = {
  metrics: {
    port: 9090,
    prefix: 'price_feed',
    labels: {
      service: 'price-feed',
      environment: process.env.NODE_ENV
    }
  },
  
  logging: {
    level: 'info',
    format: 'json',
    timestamp: true,
    colorize: false
  },
  
  alerting: {
    enabled: true,
    endpoints: {
      slack: process.env.SLACK_WEBHOOK_URL,
      email: process.env.ALERT_EMAIL
    },
    thresholds: {
      errorRate: 0.01,
      latencyP95: 1000,
      confidenceLow: 0.8
    }
  }
};

export default monitoringConfig;
```

## Rate Limiting Configuration

Configure rate limits in `config/rate-limits.config.ts`:

```typescript
import { RateLimitConfig } from '../types';

const rateLimitConfig: RateLimitConfig = {
  sources: {
    binance: {
      maxRequests: 60,
      interval: 60000  // 1 minute
    },
    huobi: {
      maxRequests: 100,
      interval: 60000  // 1 minute
    }
  },
  
  blockchain: {
    priceUpdates: {
      maxRequests: 10,
      interval: 60000  // 1 minute
    }
  },
  
  retryStrategy: {
    maxAttempts: 3,
    baseDelay: 1000,
    maxDelay: 5000,
    backoffFactor: 2
  }
};

export default rateLimitConfig;
```

## Validation Configuration

Set up input validation in `config/validation.config.ts`:

```typescript
import { ValidationConfig } from '../types';

const validationConfig: ValidationConfig = {
  price: {
    min: 0,
    max: 1000000,
    decimals: 8
  },
  
  symbol: {
    pattern: /^[A-Z0-9]+\/[A-Z0-9]+$/,
    maxLength: 12
  },
  
  timestamp: {
    maxAge: 300000  // 5 minutes
  },
  
  confidence: {
    min: 0,
    max: 1,
    decimals: 4
  }
};

export default validationConfig;
```

## Performance Tuning

### Memory Management

```typescript
const memoryConfig = {
  caches: {
    price: {
      maxSize: 1000,
      maxAge: 60000  // 1 minute
    },
    stats: {
      maxSize: 100,
      maxAge: 300000  // 5 minutes
    }
  },
  
  buffers: {
    priceHistory: 1000,  // entries per symbol
    kalmanStates: 100    // states per symbol
  }
};
```

### CPU Optimization

```typescript
const computeConfig = {
  batchSize: 10,
  workerThreads: 4,
  maxParallelRequests: 20,
  computeIntensiveTimeout: 5000
};
```

## Example Usage

```typescript
import { PriceFeedService } from '@neo-service-layer/price-feed';
import config from './config/price-feed.config';
import sources from './config/sources.config';
import kalmanConfig from './config/kalman.config';

const priceFeed = new PriceFeedService({
  ...config,
  sources,
  kalman: kalmanConfig
});

// Initialize the service
await priceFeed.initialize();

// Get aggregated price
const priceData = await priceFeed.getAggregatedPrice('NEO/USD');
console.log(`Current price: $${priceData.price} (confidence: ${priceData.confidence})`);
```

## Troubleshooting

### Common Issues

1. **Low Confidence Scores**
   - Check source availability
   - Verify price deviation thresholds
   - Review weight calculations

2. **High Latency**
   - Adjust cache durations
   - Review rate limits
   - Check network connectivity

3. **Memory Issues**
   - Tune cache sizes
   - Adjust history buffer sizes
   - Review garbage collection settings

### Logging

Enable debug logging for troubleshooting:

```typescript
const debugConfig = {
  logging: {
    level: 'debug',
    components: ['kalman', 'aggregation', 'sources']
  }
};
```

## Security Considerations

1. **API Key Management**
   - Store keys in secure vault
   - Rotate keys regularly
   - Monitor usage patterns

2. **Rate Limiting**
   - Implement per-IP limits
   - Use token bucket algorithm
   - Monitor for abuse

3. **Input Validation**
   - Sanitize all inputs
   - Validate data types
   - Check value ranges

## References

1. [Neo N3 Documentation](https://docs.neo.org/)
2. [TypeScript Configuration](https://www.typescriptlang.org/docs/handbook/tsconfig-json.html)
3. [Environment Variables](https://12factor.net/config)
4. [Prometheus Best Practices](https://prometheus.io/docs/practices/naming/) 