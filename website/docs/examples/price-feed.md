# Price Feed Integration Examples

## Overview
These examples demonstrate how to integrate and use the price feed service in various scenarios.

## Basic Price Feed Usage

### Fetching Latest Price
```typescript
import { useEffect, useState } from 'react';
import { getPriceFeed } from '../api/price-feeds';

function PriceDisplay() {
  const [price, setPrice] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchPrice = async () => {
      try {
        const feed = await getPriceFeed('NEO/USD');
        setPrice(feed.price);
        setLoading(false);
      } catch (err) {
        setError(err.message);
        setLoading(false);
      }
    };

    fetchPrice();
    // Update every 30 seconds
    const interval = setInterval(fetchPrice, 30000);
    return () => clearInterval(interval);
  }, []);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;
  return <div>NEO/USD: ${price?.toFixed(2)}</div>;
}
```

### Price Feed Subscription
```typescript
import { useEffect, useState } from 'react';
import { subscribeToPriceFeed } from '../api/price-feeds';

function LivePriceDisplay() {
  const [price, setPrice] = useState<number | null>(null);

  useEffect(() => {
    const unsubscribe = subscribeToPriceFeed('NEO/USD', (update) => {
      setPrice(update.price);
    });

    return () => unsubscribe();
  }, []);

  return <div>Live NEO/USD: ${price?.toFixed(2)}</div>;
}
```

## Advanced Usage

### Multiple Price Feeds
```typescript
import { useEffect, useState } from 'react';
import { getPriceFeeds } from '../api/price-feeds';

function MarketDisplay() {
  const [prices, setPrices] = useState<Record<string, number>>({});
  const pairs = ['NEO/USD', 'GAS/USD', 'FLM/USD'];

  useEffect(() => {
    const fetchPrices = async () => {
      try {
        const feeds = await getPriceFeeds(pairs);
        const priceMap = feeds.reduce((acc, feed) => ({
          ...acc,
          [feed.pair]: feed.price
        }), {});
        setPrices(priceMap);
      } catch (error) {
        console.error('Failed to fetch prices:', error);
      }
    };

    fetchPrices();
    const interval = setInterval(fetchPrices, 30000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div>
      {pairs.map(pair => (
        <div key={pair}>
          {pair}: ${prices[pair]?.toFixed(2) ?? 'Loading...'}
        </div>
      ))}
    </div>
  );
}
```

### Price Feed with Historical Data
```typescript
import { useEffect, useState } from 'react';
import { getHistoricalPrices } from '../api/price-feeds';

interface PricePoint {
  timestamp: number;
  price: number;
}

function PriceChart() {
  const [data, setData] = useState<PricePoint[]>([]);

  useEffect(() => {
    const fetchHistorical = async () => {
      try {
        const endTime = Date.now();
        const startTime = endTime - 24 * 60 * 60 * 1000; // Last 24 hours
        const history = await getHistoricalPrices('NEO/USD', startTime, endTime);
        setData(history);
      } catch (error) {
        console.error('Failed to fetch historical data:', error);
      }
    };

    fetchHistorical();
  }, []);

  return (
    <div>
      {/* Render chart using data */}
      {data.map(point => (
        <div key={point.timestamp}>
          Time: {new Date(point.timestamp).toLocaleString()}
          Price: ${point.price.toFixed(2)}
        </div>
      ))}
    </div>
  );
}
```

### Price Feed with Smart Contract Integration
```typescript
import { useEffect, useState } from 'react';
import { wallet } from '@cityofzion/neon-js';
import { getPriceFeed, updateContractPrice } from '../api/price-feeds';

function ContractPriceUpdater() {
  const [lastUpdate, setLastUpdate] = useState<Date | null>(null);
  const [status, setStatus] = useState<'idle' | 'updating' | 'error'>('idle');
  const contractHash = '0x1234567890abcdef1234567890abcdef12345678';

  const updatePrice = async () => {
    try {
      setStatus('updating');
      const feed = await getPriceFeed('NEO/USD');
      
      // Update price in smart contract
      await updateContractPrice(contractHash, feed.price);
      
      setLastUpdate(new Date());
      setStatus('idle');
    } catch (error) {
      console.error('Failed to update contract price:', error);
      setStatus('error');
    }
  };

  useEffect(() => {
    // Update price every 5 minutes
    const interval = setInterval(updatePrice, 5 * 60 * 1000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div>
      <div>Status: {status}</div>
      {lastUpdate && (
        <div>Last Update: {lastUpdate.toLocaleString()}</div>
      )}
      <button 
        onClick={updatePrice}
        disabled={status === 'updating'}
      >
        Update Now
      </button>
    </div>
  );
}
```

### Price Feed with Error Handling and Retry
```typescript
import { useEffect, useState } from 'react';
import { getPriceFeed } from '../api/price-feeds';

function ReliablePriceDisplay() {
  const [price, setPrice] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const maxRetries = 3;
  const retryDelay = 1000; // 1 second

  const fetchWithRetry = async (retries = 0) => {
    try {
      const feed = await getPriceFeed('NEO/USD');
      setPrice(feed.price);
      setError(null);
    } catch (error) {
      if (retries < maxRetries) {
        setTimeout(() => {
          fetchWithRetry(retries + 1);
        }, retryDelay * Math.pow(2, retries)); // Exponential backoff
      } else {
        setError('Failed to fetch price after multiple attempts');
      }
    }
  };

  useEffect(() => {
    fetchWithRetry();
    const interval = setInterval(() => fetchWithRetry(), 30000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div>
      {error ? (
        <div className="error">{error}</div>
      ) : (
        <div>NEO/USD: ${price?.toFixed(2) ?? 'Loading...'}</div>
      )}
    </div>
  );
}
```

## WebSocket Integration

### Real-time Price Updates
```typescript
import { useEffect, useState, useRef } from 'react';
import { connectPriceFeedWebSocket } from '../api/price-feeds';

function RealtimePriceDisplay() {
  const [price, setPrice] = useState<number | null>(null);
  const wsRef = useRef<WebSocket | null>(null);

  useEffect(() => {
    const connect = async () => {
      try {
        wsRef.current = await connectPriceFeedWebSocket('NEO/USD');
        
        wsRef.current.onmessage = (event) => {
          const data = JSON.parse(event.data);
          setPrice(data.price);
        };

        wsRef.current.onerror = (error) => {
          console.error('WebSocket error:', error);
        };

        wsRef.current.onclose = () => {
          // Attempt to reconnect
          setTimeout(connect, 1000);
        };
      } catch (error) {
        console.error('Failed to connect to WebSocket:', error);
      }
    };

    connect();

    return () => {
      if (wsRef.current) {
        wsRef.current.close();
      }
    };
  }, []);

  return (
    <div>
      <h2>Real-time NEO/USD Price</h2>
      <div className="price">
        ${price?.toFixed(2) ?? 'Connecting...'}
      </div>
    </div>
  );
}
```

## Testing Examples

### Unit Testing Price Feed Components
```typescript
import { render, screen, act } from '@testing-library/react';
import { getPriceFeed } from '../api/price-feeds';
import PriceDisplay from './PriceDisplay';

jest.mock('../api/price-feeds');

describe('PriceDisplay', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('displays loading state initially', () => {
    render(<PriceDisplay />);
    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  it('displays price when loaded', async () => {
    (getPriceFeed as jest.Mock).mockResolvedValue({
      pair: 'NEO/USD',
      price: 12.34,
      timestamp: Date.now()
    });

    render(<PriceDisplay />);
    
    await screen.findByText('NEO/USD: $12.34');
    expect(getPriceFeed).toHaveBeenCalledWith('NEO/USD');
  });

  it('handles errors', async () => {
    (getPriceFeed as jest.Mock).mockRejectedValue(
      new Error('Failed to fetch')
    );

    render(<PriceDisplay />);
    
    await screen.findByText('Error: Failed to fetch');
  });
});
```