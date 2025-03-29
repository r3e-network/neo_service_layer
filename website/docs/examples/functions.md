# Serverless Functions Examples

## Overview
These examples demonstrate how to create, deploy, and manage serverless functions in the Neo Service Layer.

## Basic Function Creation

### Simple Price Feed Function
```typescript
import { Handler } from '@netlify/functions';
import { PriceFeedService } from '../services/price-feed';

export const handler: Handler = async (event, context) => {
  // Initialize price feed service
  const priceFeed = new PriceFeedService();

  try {
    // Get latest price for NEO/USD
    const price = await priceFeed.getLatestPrice('NEO', 'USD');

    return {
      statusCode: 200,
      body: JSON.stringify({
        price: price.value,
        timestamp: price.timestamp,
        pair: 'NEO/USD'
      })
    };
  } catch (error) {
    console.error('Error fetching price:', error);
    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'Failed to fetch price data' })
    };
  }
};
```

### Function with Authentication
```typescript
import { Handler } from '@netlify/functions';
import { verifyNeoSignature } from '../utils/auth';

export const handler: Handler = async (event, context) => {
  // Extract authentication headers
  const neoAddress = event.headers['x-neo-address'];
  const signature = event.headers['x-neo-signature'];
  const timestamp = event.headers['x-timestamp'];

  if (!neoAddress || !signature || !timestamp) {
    return {
      statusCode: 401,
      body: JSON.stringify({ error: 'Missing authentication headers' })
    };
  }

  try {
    // Verify the signature
    const isValid = await verifyNeoSignature(
      neoAddress,
      signature,
      \`\${event.path}:\${timestamp}\`
    );

    if (!isValid) {
      return {
        statusCode: 401,
        body: JSON.stringify({ error: 'Invalid signature' })
      };
    }

    // Process the authenticated request
    return {
      statusCode: 200,
      body: JSON.stringify({ message: 'Authenticated successfully' })
    };
  } catch (error) {
    console.error('Authentication error:', error);
    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'Authentication failed' })
    };
  }
};
```

## Advanced Usage

### Function with Secret Access
```typescript
import { Handler } from '@netlify/functions';
import { SecretVault } from '../utils/vault';

export const handler: Handler = async (event, context) => {
  const vault = new SecretVault({
    teeEnabled: true,
    backupEnabled: true,
    rotationPeriod: 24 * 60 * 60 * 1000
  });

  try {
    // Access a secret within the function
    const apiKey = await vault.getSecret('external_api_key');

    // Use the secret to make an authenticated API call
    const response = await fetch('https://api.example.com/data', {
      headers: {
        'Authorization': \`Bearer \${apiKey.value}\`
      }
    });

    const data = await response.json();

    return {
      statusCode: 200,
      body: JSON.stringify(data)
    };
  } catch (error) {
    console.error('Error accessing secret:', error);
    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'Failed to process request' })
    };
  }
};
```

### Function with Rate Limiting
```typescript
import { Handler } from '@netlify/functions';
import { RateLimiter } from '../utils/rate-limiter';

const limiter = new RateLimiter({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100 // limit each IP to 100 requests per windowMs
});

export const handler: Handler = async (event, context) => {
  const clientIP = event.headers['client-ip'] || 
                  event.headers['x-forwarded-for'];

  try {
    // Check rate limit
    await limiter.checkLimit(clientIP);

    // Process the request
    return {
      statusCode: 200,
      body: JSON.stringify({ message: 'Request processed' })
    };
  } catch (error) {
    if (error.code === 'RATE_LIMIT_EXCEEDED') {
      return {
        statusCode: 429,
        body: JSON.stringify({ 
          error: 'Too many requests',
          retryAfter: error.retryAfter
        })
      };
    }

    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'Internal server error' })
    };
  }
};
```

### Function with Caching
```typescript
import { Handler } from '@netlify/functions';
import { Cache } from '../utils/cache';

const cache = new Cache({
  ttl: 60 * 1000, // 1 minute
  maxSize: 100 // maximum number of items to cache
});

export const handler: Handler = async (event, context) => {
  const cacheKey = event.path + event.queryStringParameters?.toString();

  try {
    // Check cache first
    const cachedResponse = await cache.get(cacheKey);
    if (cachedResponse) {
      return {
        statusCode: 200,
        body: JSON.stringify(cachedResponse),
        headers: {
          'X-Cache': 'HIT'
        }
      };
    }

    // If not in cache, fetch fresh data
    const data = await fetchExpensiveData();

    // Store in cache
    await cache.set(cacheKey, data);

    return {
      statusCode: 200,
      body: JSON.stringify(data),
      headers: {
        'X-Cache': 'MISS'
      }
    };
  } catch (error) {
    console.error('Error processing request:', error);
    return {
      statusCode: 500,
      body: JSON.stringify({ error: 'Failed to process request' })
    };
  }
};

async function fetchExpensiveData() {
  // Simulate expensive operation
  await new Promise(resolve => setTimeout(resolve, 1000));
  return { data: 'Expensive computation result' };
}
```

## Testing Examples

### Unit Testing Functions
```typescript
import { handler } from './get-price-feeds';
import { PriceFeedService } from '../services/price-feed';

jest.mock('../services/price-feed');

describe('get-price-feeds function', () => {
  let mockPriceFeed: jest.Mocked<PriceFeedService>;

  beforeEach(() => {
    mockPriceFeed = new PriceFeedService() as jest.Mocked<PriceFeedService>;
    mockPriceFeed.getLatestPrice = jest.fn();
  });

  it('returns latest price data', async () => {
    const mockPrice = {
      value: '50.00',
      timestamp: new Date().toISOString(),
      pair: 'NEO/USD'
    };

    mockPriceFeed.getLatestPrice.mockResolvedValue(mockPrice);

    const response = await handler(
      {
        httpMethod: 'GET',
        path: '/api/get-price-feeds',
        headers: {},
        queryStringParameters: null,
        body: null
      } as any,
      {} as any
    );

    expect(response.statusCode).toBe(200);
    expect(JSON.parse(response.body)).toEqual(mockPrice);
  });

  it('handles errors gracefully', async () => {
    mockPriceFeed.getLatestPrice.mockRejectedValue(
      new Error('Failed to fetch price')
    );

    const response = await handler(
      {
        httpMethod: 'GET',
        path: '/api/get-price-feeds',
        headers: {},
        queryStringParameters: null,
        body: null
      } as any,
      {} as any
    );

    expect(response.statusCode).toBe(500);
    expect(JSON.parse(response.body)).toEqual({
      error: 'Failed to fetch price data'
    });
  });
});
```

### Integration Testing Functions
```typescript
import { handler } from './authenticated-function';
import { verifyNeoSignature } from '../utils/auth';

jest.mock('../utils/auth');

describe('authenticated-function integration', () => {
  beforeEach(() => {
    (verifyNeoSignature as jest.Mock).mockReset();
  });

  it('processes authenticated requests', async () => {
    const mockNeoAddress = 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd';
    const mockSignature = 'mock_signature';
    const mockTimestamp = Date.now().toString();

    (verifyNeoSignature as jest.Mock).mockResolvedValue(true);

    const response = await handler(
      {
        httpMethod: 'POST',
        path: '/api/authenticated-function',
        headers: {
          'x-neo-address': mockNeoAddress,
          'x-neo-signature': mockSignature,
          'x-timestamp': mockTimestamp
        },
        body: JSON.stringify({ data: 'test' })
      } as any,
      {} as any
    );

    expect(response.statusCode).toBe(200);
    expect(verifyNeoSignature).toHaveBeenCalledWith(
      mockNeoAddress,
      mockSignature,
      \`/api/authenticated-function:\${mockTimestamp}\`
    );
  });

  it('rejects unauthenticated requests', async () => {
    const response = await handler(
      {
        httpMethod: 'POST',
        path: '/api/authenticated-function',
        headers: {},
        body: JSON.stringify({ data: 'test' })
      } as any,
      {} as any
    );

    expect(response.statusCode).toBe(401);
    expect(JSON.parse(response.body)).toEqual({
      error: 'Missing authentication headers'
    });
  });
});
```