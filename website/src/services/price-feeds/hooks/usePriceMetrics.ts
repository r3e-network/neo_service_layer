import React from 'react';
import { useWebSocketEvents } from '../../../hooks/useWebSocketEvents';
import { PRICE_FEED_CONSTANTS } from '../constants';
import {
  UsePriceMetricsResult,
  PriceMetrics,
  MetricsUpdateEvent
} from '../types/types';

export function usePriceMetrics(symbol: string): UsePriceMetricsResult {
  const [metrics, setMetrics] = React.useState<PriceMetrics | null>(null);
  const [isLoading, setIsLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  const { subscribe, unsubscribe } = useWebSocketEvents({
    url: `${PRICE_FEED_CONSTANTS.WS_ENDPOINT}/price-feeds`
  });

  const refreshMetrics = React.useCallback(async () => {
    try {
      const response = await fetch(`${PRICE_FEED_CONSTANTS.API_ENDPOINT}/${symbol}/metrics`);
      if (!response.ok) throw new Error('Failed to fetch price metrics');

      const data: PriceMetrics = await response.json();
      setMetrics(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch price metrics');
    } finally {
      setIsLoading(false);
    }
  }, [symbol]);

  React.useEffect(() => {
    let isSubscribed = true;

    const handleMetricsUpdate = (event: MetricsUpdateEvent['data']) => {
      if (!isSubscribed || event.symbol !== symbol) return;
      setMetrics(event.metrics);
    };

    // Initial fetch
    refreshMetrics();

    // Subscribe to metrics updates
    subscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.METRICS_UPDATE, handleMetricsUpdate);

    // Polling fallback
    const pollInterval = setInterval(() => {
      refreshMetrics();
    }, PRICE_FEED_CONSTANTS.UPDATE_INTERVAL);

    return () => {
      isSubscribed = false;
      clearInterval(pollInterval);
      unsubscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.METRICS_UPDATE, handleMetricsUpdate);
    };
  }, [symbol, subscribe, unsubscribe, refreshMetrics]);

  return {
    metrics,
    isLoading,
    error,
    refreshMetrics
  };
}