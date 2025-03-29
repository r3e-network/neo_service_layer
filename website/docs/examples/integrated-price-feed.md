# Integrated Price Feed Example

## Overview
This example demonstrates how to integrate multiple Neo Service Layer services to create a robust price feed system. It combines:
- Price Feed Service for fetching and publishing prices
- Secrets Management for API keys and credentials
- Metrics Service for monitoring and alerting
- Logging Service for structured logging and debugging
- Functions Service for serverless endpoints

## Complete Integration Example

### Price Feed Function with Full Integration
```typescript
import { Handler } from '@netlify/functions';
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { SecretVault } from '../../utils/vault';
import { PriceFeedService } from '../../services/price-feed';
import { verifyNeoSignature } from '../../utils/auth';

// Initialize services
const logger = Logger.getInstance().child({ service: 'price-feeds' });
const metrics = new MetricsService({
  namespace: 'neo_service_layer',
  subsystem: 'price_feeds'
});
const vault = new SecretVault({
  teeEnabled: true,
  backupEnabled: true,
  rotationPeriod: 24 * 60 * 60 * 1000
});

export const handler: Handler = async (event, context) => {
  // Start performance monitoring
  const timer = metrics.startTimer('price_feed_request_duration_seconds');
  
  // Create request context for logging
  const requestContext = {
    requestId: context.awsRequestId,
    path: event.path,
    method: event.httpMethod,
    timestamp: new Date().toISOString()
  };

  try {
    // Log incoming request
    logger.info('Processing price feed request', {
      ...requestContext,
      queryParams: event.queryStringParameters
    });

    // Verify authentication
    const neoAddress = event.headers['x-neo-address'];
    const signature = event.headers['x-neo-signature'];
    const timestamp = event.headers['x-timestamp'];

    if (!neoAddress || !signature || !timestamp) {
      metrics.incrementCounter('price_feed_auth_errors_total', {
        error_type: 'missing_headers'
      });
      
      logger.warn('Missing authentication headers', requestContext);
      
      return {
        statusCode: 401,
        body: JSON.stringify({ error: 'Missing authentication headers' })
      };
    }

    const isValid = await verifyNeoSignature(
      neoAddress,
      signature,
      \`\${event.path}:\${timestamp}\`
    );

    if (!isValid) {
      metrics.incrementCounter('price_feed_auth_errors_total', {
        error_type: 'invalid_signature'
      });
      
      logger.warn('Invalid signature', {
        ...requestContext,
        neoAddress
      });
      
      return {
        statusCode: 401,
        body: JSON.stringify({ error: 'Invalid signature' })
      };
    }

    // Initialize price feed service with API keys from vault
    const apiKeys = await vault.getSecret('price_feed_api_keys');
    const priceFeed = new PriceFeedService({
      apiKeys: JSON.parse(apiKeys.value),
      retryConfig: {
        maxRetries: 3,
        backoffMs: 1000
      }
    });

    // Get requested pairs from query parameters
    const pairs = event.queryStringParameters?.pairs?.split(',') || ['NEO/USD'];
    
    // Record metric for requested pairs
    metrics.incrementCounter('price_feed_pairs_requested_total', {
      pair_count: pairs.length.toString()
    });

    // Fetch prices for all requested pairs
    const pricePromises = pairs.map(async (pair) => {
      const pairTimer = metrics.startTimer('price_feed_pair_fetch_duration_seconds', {
        pair
      });

      try {
        const price = await priceFeed.getLatestPrice(...pair.split('/'));
        
        // Record successful price fetch
        metrics.incrementCounter('price_feed_fetch_total', {
          pair,
          status: 'success'
        });

        // Record price value in metrics
        metrics.recordGauge('price_feed_value', price.value, {
          pair
        });

        logger.info('Price fetched successfully', {
          ...requestContext,
          pair,
          timestamp: price.timestamp
        });

        return {
          pair,
          ...price
        };
      } catch (error) {
        // Record failed price fetch
        metrics.incrementCounter('price_feed_fetch_total', {
          pair,
          status: 'error',
          error_type: error.name
        });

        logger.error('Failed to fetch price', {
          ...requestContext,
          pair,
          error: {
            name: error.name,
            message: error.message,
            stack: error.stack
          }
        });

        throw error;
      } finally {
        pairTimer.end();
      }
    });

    // Wait for all price fetches to complete
    const prices = await Promise.all(pricePromises);

    // Record successful response
    metrics.incrementCounter('price_feed_responses_total', {
      status: 'success'
    });

    logger.info('Price feed request completed', {
      ...requestContext,
      pairCount: pairs.length,
      duration: timer.getDuration()
    });

    return {
      statusCode: 200,
      body: JSON.stringify({
        prices,
        timestamp: new Date().toISOString()
      })
    };
  } catch (error) {
    // Record error metrics
    metrics.incrementCounter('price_feed_errors_total', {
      error_type: error.name
    });

    // Log detailed error
    logger.error('Price feed request failed', {
      ...requestContext,
      error: {
        name: error.name,
        message: error.message,
        stack: error.stack
      }
    });

    return {
      statusCode: 500,
      body: JSON.stringify({
        error: 'Failed to fetch price data',
        requestId: context.awsRequestId
      })
    };
  } finally {
    // End the request timer
    timer.end();
  }
};
```

### Price Feed Monitor Component
```typescript
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { SecretVault } from '../../utils/vault';
import { PriceFeedService } from '../../services/price-feed';
import { AlertService } from '../../services/alerts';

class PriceFeedMonitor {
  private logger: Logger;
  private metrics: MetricsService;
  private vault: SecretVault;
  private priceFeed: PriceFeedService;
  private alertService: AlertService;
  private monitoringInterval: NodeJS.Timeout | null = null;

  constructor() {
    this.logger = Logger.getInstance().child({ 
      component: 'PriceFeedMonitor' 
    });
    
    this.metrics = new MetricsService({
      namespace: 'neo_service_layer',
      subsystem: 'price_feed_monitor'
    });
    
    this.vault = new SecretVault({
      teeEnabled: true,
      backupEnabled: true,
      rotationPeriod: 24 * 60 * 60 * 1000
    });
    
    this.alertService = new AlertService();
  }

  async initialize() {
    try {
      // Get configuration from vault
      const config = await this.vault.getSecret('price_feed_monitor_config');
      const apiKeys = await this.vault.getSecret('price_feed_api_keys');

      this.priceFeed = new PriceFeedService({
        apiKeys: JSON.parse(apiKeys.value),
        retryConfig: {
          maxRetries: 3,
          backoffMs: 1000
        }
      });

      return JSON.parse(config.value);
    } catch (error) {
      this.logger.error('Failed to initialize price feed monitor', {
        error: {
          name: error.name,
          message: error.message,
          stack: error.stack
        }
      });
      throw error;
    }
  }

  async startMonitoring(pairs: string[], interval: number) {
    this.logger.info('Starting price feed monitoring', {
      pairs,
      interval
    });

    this.monitoringInterval = setInterval(async () => {
      await this.checkPrices(pairs);
    }, interval);

    // Record monitoring status
    this.metrics.recordGauge('price_feed_monitor_status', 1);
  }

  async checkPrices(pairs: string[]) {
    const checkContext = {
      timestamp: new Date().toISOString(),
      pairs
    };

    this.logger.info('Checking price feeds', checkContext);

    for (const pair of pairs) {
      const timer = this.metrics.startTimer(
        'price_feed_monitor_check_duration_seconds',
        { pair }
      );

      try {
        const price = await this.priceFeed.getLatestPrice(...pair.split('/'));
        
        // Record price in metrics
        this.metrics.recordGauge('price_feed_monitor_value', price.value, {
          pair
        });

        // Check for significant price changes
        const previousPrice = await this.getPreviousPrice(pair);
        if (previousPrice) {
          const changePercent = Math.abs(
            ((price.value - previousPrice) / previousPrice) * 100
          );

          this.metrics.recordGauge('price_feed_change_percent', changePercent, {
            pair
          });

          // Alert on significant price changes
          if (changePercent > 5) {
            await this.alertService.sendAlert({
              type: 'PRICE_CHANGE',
              severity: changePercent > 10 ? 'high' : 'medium',
              message: \`Significant price change detected for \${pair}\`,
              data: {
                pair,
                previousPrice,
                currentPrice: price.value,
                changePercent
              }
            });
          }
        }

        // Store current price for future comparison
        await this.storePreviousPrice(pair, price.value);

        this.logger.info('Price check completed', {
          ...checkContext,
          pair,
          price: price.value,
          timestamp: price.timestamp
        });
      } catch (error) {
        // Record error metrics
        this.metrics.incrementCounter('price_feed_monitor_errors_total', {
          pair,
          error_type: error.name
        });

        this.logger.error('Price check failed', {
          ...checkContext,
          pair,
          error: {
            name: error.name,
            message: error.message,
            stack: error.stack
          }
        });

        // Send alert for failed checks
        await this.alertService.sendAlert({
          type: 'PRICE_CHECK_FAILED',
          severity: 'high',
          message: \`Failed to check price for \${pair}\`,
          data: {
            pair,
            error: error.message
          }
        });
      } finally {
        timer.end();
      }
    }
  }

  private async getPreviousPrice(pair: string): Promise<number | null> {
    try {
      const key = \`previous_price_\${pair}\`;
      const stored = await this.vault.getSecret(key);
      return stored ? parseFloat(stored.value) : null;
    } catch (error) {
      this.logger.error('Failed to get previous price', {
        pair,
        error: {
          name: error.name,
          message: error.message
        }
      });
      return null;
    }
  }

  private async storePreviousPrice(pair: string, price: number): Promise<void> {
    try {
      const key = \`previous_price_\${pair}\`;
      await this.vault.createOrUpdateSecret(key, price.toString());
    } catch (error) {
      this.logger.error('Failed to store previous price', {
        pair,
        price,
        error: {
          name: error.name,
          message: error.message
        }
      });
    }
  }

  stop() {
    if (this.monitoringInterval) {
      clearInterval(this.monitoringInterval);
      this.monitoringInterval = null;
      
      // Update monitoring status
      this.metrics.recordGauge('price_feed_monitor_status', 0);
      
      this.logger.info('Price feed monitoring stopped');
    }
  }
}

// Usage example
async function startPriceMonitoring() {
  const monitor = new PriceFeedMonitor();
  
  try {
    const config = await monitor.initialize();
    await monitor.startMonitoring(
      config.pairs,
      config.interval || 60000 // Default to 1 minute
    );
  } catch (error) {
    console.error('Failed to start price monitoring:', error);
  }
}
```

### Testing the Integrated System

```typescript
import { handler } from './get-price-feeds';
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { SecretVault } from '../../utils/vault';
import { PriceFeedService } from '../../services/price-feed';
import { verifyNeoSignature } from '../../utils/auth';

// Mock all dependencies
jest.mock('../../utils/logger');
jest.mock('../../services/metrics');
jest.mock('../../utils/vault');
jest.mock('../../services/price-feed');
jest.mock('../../utils/auth');

describe('Integrated Price Feed Handler', () => {
  let mockLogger: jest.Mocked<Logger>;
  let mockMetrics: jest.Mocked<MetricsService>;
  let mockVault: jest.Mocked<SecretVault>;
  let mockPriceFeed: jest.Mocked<PriceFeedService>;

  beforeEach(() => {
    // Reset all mocks
    jest.clearAllMocks();

    // Setup mock implementations
    mockLogger = {
      info: jest.fn(),
      warn: jest.fn(),
      error: jest.fn(),
      child: jest.fn().mockReturnThis()
    } as any;

    mockMetrics = {
      startTimer: jest.fn().mockReturnValue({
        end: jest.fn(),
        getDuration: jest.fn().mockReturnValue(100)
      }),
      incrementCounter: jest.fn(),
      recordGauge: jest.fn()
    } as any;

    mockVault = {
      getSecret: jest.fn().mockResolvedValue({
        value: JSON.stringify({
          coinmarketcap: 'test-api-key'
        })
      })
    } as any;

    mockPriceFeed = {
      getLatestPrice: jest.fn().mockResolvedValue({
        value: 50.0,
        timestamp: new Date().toISOString()
      })
    } as any;

    // Mock signature verification
    (verifyNeoSignature as jest.Mock).mockResolvedValue(true);
  });

  it('successfully fetches prices with valid authentication', async () => {
    const event = {
      httpMethod: 'GET',
      path: '/api/price-feeds',
      headers: {
        'x-neo-address': 'test-address',
        'x-neo-signature': 'test-signature',
        'x-timestamp': Date.now().toString()
      },
      queryStringParameters: {
        pairs: 'NEO/USD,GAS/USD'
      }
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    // Verify successful response
    expect(response.statusCode).toBe(200);
    const body = JSON.parse(response.body);
    expect(body.prices).toHaveLength(2);

    // Verify logging
    expect(mockLogger.info).toHaveBeenCalledWith(
      'Processing price feed request',
      expect.any(Object)
    );

    // Verify metrics
    expect(mockMetrics.incrementCounter).toHaveBeenCalledWith(
      'price_feed_pairs_requested_total',
      { pair_count: '2' }
    );

    // Verify secret access
    expect(mockVault.getSecret).toHaveBeenCalledWith('price_feed_api_keys');

    // Verify price feed calls
    expect(mockPriceFeed.getLatestPrice).toHaveBeenCalledTimes(2);
  });

  it('handles authentication errors correctly', async () => {
    const event = {
      httpMethod: 'GET',
      path: '/api/price-feeds',
      headers: {}, // Missing auth headers
      queryStringParameters: {
        pairs: 'NEO/USD'
      }
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    // Verify error response
    expect(response.statusCode).toBe(401);
    
    // Verify error metrics
    expect(mockMetrics.incrementCounter).toHaveBeenCalledWith(
      'price_feed_auth_errors_total',
      { error_type: 'missing_headers' }
    );

    // Verify error logging
    expect(mockLogger.warn).toHaveBeenCalledWith(
      'Missing authentication headers',
      expect.any(Object)
    );
  });

  it('handles price feed errors correctly', async () => {
    // Mock price feed error
    mockPriceFeed.getLatestPrice.mockRejectedValue(
      new Error('API error')
    );

    const event = {
      httpMethod: 'GET',
      path: '/api/price-feeds',
      headers: {
        'x-neo-address': 'test-address',
        'x-neo-signature': 'test-signature',
        'x-timestamp': Date.now().toString()
      },
      queryStringParameters: {
        pairs: 'NEO/USD'
      }
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    // Verify error response
    expect(response.statusCode).toBe(500);

    // Verify error metrics
    expect(mockMetrics.incrementCounter).toHaveBeenCalledWith(
      'price_feed_errors_total',
      { error_type: 'Error' }
    );

    // Verify error logging
    expect(mockLogger.error).toHaveBeenCalledWith(
      'Price feed request failed',
      expect.any(Object)
    );
  });
});
```

## Key Integration Points

1. **Authentication Flow**
   - Signature verification using Neo wallet addresses
   - Integration with metrics and logging for auth failures
   - Secure handling of authentication data

2. **Secret Management**
   - Secure storage and retrieval of API keys
   - Integration with TEE for sensitive operations
   - Automatic key rotation handling

3. **Metrics Collection**
   - Request duration tracking
   - Error rate monitoring
   - Price change tracking
   - Custom metrics for business logic

4. **Structured Logging**
   - Request context propagation
   - Error tracking with stack traces
   - Performance monitoring
   - Business event logging

5. **Error Handling**
   - Graceful error recovery
   - Detailed error reporting
   - Integration with alerting system
   - Error metrics collection

6. **Monitoring and Alerting**
   - Price change detection
   - Error rate monitoring
   - Performance tracking
   - Integration with alert service

## Best Practices Demonstrated

1. **Security**
   - Signature verification for all requests
   - Secure secret management
   - Input validation
   - Error message sanitization

2. **Reliability**
   - Comprehensive error handling
   - Retry logic for external calls
   - Fallback mechanisms
   - Circuit breaking patterns

3. **Observability**
   - Detailed logging
   - Comprehensive metrics
   - Performance tracking
   - Alert integration

4. **Performance**
   - Parallel price fetching
   - Caching considerations
   - Resource cleanup
   - Efficient error handling

5. **Maintainability**
   - Modular code structure
   - Clear separation of concerns
   - Comprehensive testing
   - Clear logging and metrics