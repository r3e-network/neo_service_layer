// @ts-ignore
import * as React from 'react';
import { websocketService } from '@/services/websocket';
import { PriceData, PriceStats } from '@/services/price-feeds';

interface UsePriceFeedOptions {
  symbol: string;
  updateInterval?: number;
}

interface UsePriceFeedResult {
  priceData: PriceData | null;
  stats: PriceStats | null;
  error: string | null;
  isLoading: boolean;
  isConnected: boolean;
}

export function usePriceFeed({ symbol, updateInterval = 5000 }: UsePriceFeedOptions): UsePriceFeedResult {
  const [priceData, setPriceData] = React.useState<PriceData | null>(null);
  const [stats, setStats] = React.useState<PriceStats | null>(null);
  const [error, setError] = React.useState<string | null>(null);
  const [isLoading, setIsLoading] = React.useState(true);
  const [isConnected, setIsConnected] = React.useState(false);

  const fetchPriceData = React.useCallback(async () => {
    try {
      const response = await fetch(`/api/price-feed/${symbol}`);
      if (!response.ok) {
        throw new Error('Failed to fetch price data');
      }
      const data = await response.json();
      setPriceData(data);
      setStats(data.stats || null);
      setError(null);
    } catch (err) {
      setError('Failed to fetch price data');
      console.error('Error fetching price data:', err);
    } finally {
      setIsLoading(false);
    }
  }, [symbol]);

  // Handle WebSocket connection and events
  React.useEffect(() => {
    const handlePriceUpdate = (data: PriceData) => {
      if (data.symbol === symbol) {
        setPriceData(data);
        setStats(data.stats || null);
      }
    };

    const handleConnect = () => {
      setIsConnected(true);
      setError(null);
      // Subscribe to price updates for the symbol
      websocketService.subscribe('price_updates', { symbol });
    };

    const handleDisconnect = () => {
      setIsConnected(false);
      setError('WebSocket disconnected');
    };

    const handleError = (err: Error) => {
      setError(`WebSocket error: ${err.message}`);
      setIsConnected(false);
    };

    // Initial fetch
    fetchPriceData();

    // Set up WebSocket listeners
    websocketService.on('price_update', handlePriceUpdate);
    websocketService.on('connected', handleConnect);
    websocketService.on('disconnected', handleDisconnect);
    websocketService.on('error', handleError);

    // Connect WebSocket if not already connected
    if (!isConnected) {
      // TODO: Get actual token from authentication service
      const token = localStorage.getItem('auth_token') || '';
      websocketService.connect(token);
    }

    // Fallback polling for when WebSocket is not available
    const pollInterval = setInterval(() => {
      if (!isConnected) {
        fetchPriceData();
      }
    }, updateInterval);

    return () => {
      websocketService.unsubscribe('price_updates');
      websocketService.off('price_update', handlePriceUpdate);
      websocketService.off('connected', handleConnect);
      websocketService.off('disconnected', handleDisconnect);
      websocketService.off('error', handleError);
      clearInterval(pollInterval);
    };
  }, [symbol, updateInterval, isConnected, fetchPriceData]);

  return {
    priceData,
    stats,
    error,
    isLoading,
    isConnected
  };
}