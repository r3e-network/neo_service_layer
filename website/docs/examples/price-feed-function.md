# Price Feed Function Example

## Overview
This example demonstrates how to implement a Netlify Function that provides price feed data with integrated logging, metrics, and error handling.

## Implementation Example

```typescript
import { Handler } from '@netlify/functions';
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { PriceFeedService } from '../../services/price-feed';
import { CacheService } from '../../services/cache';

// Initialize services
const logger = Logger.getInstance().child({ service: 'price-feeds' });
const metrics = new MetricsService({
  namespace: 'neo_service_layer',
  subsystem: 'price_feeds'
});
const cache = new CacheService();

export const handler: Handler = async (event, context) => {
  const timer = metrics.startTimer('price_feed_request_duration_seconds');
  
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

    // Initialize price feed service
    const priceFeedService = new PriceFeedService();

    // Parse request parameters
    const { symbol, source } = event.queryStringParameters || {};

    if (!symbol) {
      throw new Error('Symbol parameter is required');
    }

    // Check cache first
    const cacheKey = \`price_feed:\${symbol}:\${source || 'default'}\`;
    const cachedPrice = await cache.get(cacheKey);

    if (cachedPrice) {
      metrics.incrementCounter('price_feed_cache_hits_total', {
        symbol,
        source: source || 'default'
      });

      logger.info('Returning cached price feed', {
        ...requestContext,
        symbol,
        source,
        cache: 'hit'
      });

      return {
        statusCode: 200,
        body: cachedPrice
      };
    }

    metrics.incrementCounter('price_feed_cache_misses_total', {
      symbol,
      source: source || 'default'
    });

    // Get latest price
    const priceData = await priceFeedService.getPrice(symbol, source);

    // Cache the result
    await cache.set(cacheKey, JSON.stringify(priceData), {
      ttl: 60 // Cache for 1 minute
    });

    // Record metrics
    metrics.recordGauge('price_feed_value', priceData.price, {
      symbol,
      source: priceData.source
    });

    metrics.recordGauge('price_feed_timestamp', 
      new Date(priceData.timestamp).getTime() / 1000,
      {
        symbol,
        source: priceData.source
      }
    );

    // Log success
    logger.info('Price feed request completed', {
      ...requestContext,
      symbol,
      source: priceData.source,
      cache: 'miss'
    });

    return {
      statusCode: 200,
      body: JSON.stringify(priceData)
    };
  } catch (error) {
    // Record error metrics
    metrics.incrementCounter('price_feed_errors_total', {
      error_type: error.name
    });

    // Log error details
    logger.error('Price feed request failed', {
      ...requestContext,
      error: {
        name: error.name,
        message: error.message,
        stack: error.stack
      }
    });

    return {
      statusCode: error.message === 'Symbol parameter is required' ? 400 : 500,
      body: JSON.stringify({
        error: error.message,
        requestId: context.awsRequestId
      })
    };
  } finally {
    timer.end();
  }
};
```

## Testing Example

```typescript
import { handler } from './get-price-feeds';
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { PriceFeedService } from '../../services/price-feed';
import { CacheService } from '../../services/cache';

jest.mock('../../utils/logger');
jest.mock('../../services/metrics');
jest.mock('../../services/price-feed');
jest.mock('../../services/cache');

describe('Price Feed Function', () => {
  let mockLogger: jest.Mocked<Logger>;
  let mockMetrics: jest.Mocked<MetricsService>;
  let mockPriceFeed: jest.Mocked<PriceFeedService>;
  let mockCache: jest.Mocked<CacheService>;

  beforeEach(() => {
    jest.clearAllMocks();

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

    mockPriceFeed = {
      getPrice: jest.fn().mockResolvedValue({
        symbol: 'NEO',
        price: 100.0,
        timestamp: '2024-03-21T12:00:00Z',
        source: 'binance'
      })
    } as any;

    mockCache = {
      get: jest.fn().mockResolvedValue(null),
      set: jest.fn().mockResolvedValue(true)
    } as any;
  });

  it('successfully returns price feed data', async () => {
    const event = {
      httpMethod: 'GET',
      path: '/api/price-feeds',
      queryStringParameters: {
        symbol: 'NEO'
      }
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(200);
    const body = JSON.parse(response.body);
    expect(body.symbol).toBe('NEO');
    expect(body.price).toBe(100.0);

    // Verify cache check
    expect(mockCache.get).toHaveBeenCalledWith(
      'price_feed:NEO:default'
    );

    // Verify price feed service call
    expect(mockPriceFeed.getPrice).toHaveBeenCalledWith(
      'NEO',
      undefined
    );

    // Verify metrics recording
    expect(mockMetrics.recordGauge).toHaveBeenCalledWith(
      'price_feed_value',
      100.0,
      { symbol: 'NEO', source: 'binance' }
    );

    // Verify logging
    expect(mockLogger.info).toHaveBeenCalledWith(
      'Price feed request completed',
      expect.any(Object)
    );
  });

  it('returns cached data when available', async () => {
    const cachedData = {
      symbol: 'NEO',
      price: 99.0,
      timestamp: '2024-03-21T11:59:00Z',
      source: 'binance'
    };

    mockCache.get.mockResolvedValue(JSON.stringify(cachedData));

    const event = {
      httpMethod: 'GET',
      path: '/api/price-feeds',
      queryStringParameters: {
        symbol: 'NEO'
      }
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(200);
    expect(response.body).toBe(JSON.stringify(cachedData));

    // Verify cache hit metric
    expect(mockMetrics.incrementCounter).toHaveBeenCalledWith(
      'price_feed_cache_hits_total',
      { symbol: 'NEO', source: 'default' }
    );

    // Verify price feed service was not called
    expect(mockPriceFeed.getPrice).not.toHaveBeenCalled();
  });

  it('handles missing symbol parameter', async () => {
    const event = {
      httpMethod: 'GET',
      path: '/api/price-feeds',
      queryStringParameters: {}
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(400);
    const body = JSON.parse(response.body);
    expect(body.error).toBe('Symbol parameter is required');

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

  it('handles price feed service errors', async () => {
    mockPriceFeed.getPrice.mockRejectedValue(
      new Error('Failed to fetch price')
    );

    const event = {
      httpMethod: 'GET',
      path: '/api/price-feeds',
      queryStringParameters: {
        symbol: 'NEO'
      }
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(500);
    const body = JSON.parse(response.body);
    expect(body.error).toBe('Failed to fetch price');

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

## Key Features

1. **Request Handling**
   - Parameter validation
   - Error handling
   - Response formatting

2. **Caching**
   - Cache-first approach
   - TTL management
   - Cache metrics

3. **Metrics**
   - Request duration
   - Cache hit/miss rates
   - Error counts
   - Price values

4. **Logging**
   - Request context
   - Cache status
   - Error details
   - Success responses

5. **Testing**
   - Unit tests
   - Mock services
   - Error scenarios
   - Cache behavior

## Best Practices

1. **Performance**
   - Cache implementation
   - Request timing
   - Efficient error handling
   - Response optimization

2. **Reliability**
   - Error handling
   - Cache fallback
   - Service recovery
   - Request validation

3. **Observability**
   - Detailed metrics
   - Structured logging
   - Request tracking
   - Error monitoring

4. **Maintainability**
   - Modular design
   - Clear error handling
   - Comprehensive testing
   - Documentation