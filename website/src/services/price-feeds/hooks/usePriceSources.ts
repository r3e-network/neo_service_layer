import React from 'react';
import { useWebSocketEvents } from '../../../hooks/useWebSocketEvents';
import { PRICE_FEED_CONSTANTS } from '../constants';
import {
  UsePriceSourcesResult,
  PriceSource,
  SourceStats,
  SourceUpdateEvent
} from '../types/types';

export function usePriceSources(symbol: string): UsePriceSourcesResult {
  const [sources, setSources] = React.useState<PriceSource[]>([]);
  const [stats, setStats] = React.useState<SourceStats>({
    totalSources: 0,
    activeSources: 0,
    averageDeviation: 0,
    lastUpdate: 0,
    averageLatency: 0,
    reliabilityScore: 0
  });
  const [isLoading, setIsLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  const { subscribe, unsubscribe } = useWebSocketEvents({
    url: `${PRICE_FEED_CONSTANTS.WS_ENDPOINT}/price-feeds`
  });

  const calculateStats = React.useCallback((sourceList: PriceSource[]): SourceStats => {
    const activeSources = sourceList.filter(s => s.status === 'active');
    const avgDeviation = activeSources.reduce((acc, s) => acc + s.deviation, 0) / 
      (activeSources.length || 1);
    const avgLatency = activeSources.reduce((acc, s) => acc + s.latency, 0) /
      (activeSources.length || 1);
    const reliabilityScore = activeSources.reduce((acc, s) => acc + s.reliability, 0) /
      (activeSources.length || 1);

    return {
      totalSources: sourceList.length,
      activeSources: activeSources.length,
      averageDeviation: avgDeviation,
      lastUpdate: Math.max(...sourceList.map(s => s.lastUpdate)),
      averageLatency: avgLatency,
      reliabilityScore: reliabilityScore
    };
  }, []);

  const refreshSources = React.useCallback(async () => {
    try {
      const response = await fetch(`${PRICE_FEED_CONSTANTS.API_ENDPOINT}/${symbol}/sources`);
      if (!response.ok) throw new Error('Failed to fetch price sources');

      const data: PriceSource[] = await response.json();
      setSources(data);
      setStats(calculateStats(data));
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch price sources');
    } finally {
      setIsLoading(false);
    }
  }, [symbol, calculateStats]);

  React.useEffect(() => {
    let isSubscribed = true;

    const handleSourceUpdate = (event: SourceUpdateEvent['data']) => {
      if (!isSubscribed || event.symbol !== symbol) return;
      setSources(event.sources);
      setStats(calculateStats(event.sources));
    };

    // Initial fetch
    refreshSources();

    // Subscribe to source updates
    subscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.SOURCE_UPDATE, handleSourceUpdate);

    // Polling fallback
    const pollInterval = setInterval(() => {
      refreshSources();
    }, PRICE_FEED_CONSTANTS.UPDATE_INTERVAL);

    return () => {
      isSubscribed = false;
      clearInterval(pollInterval);
      unsubscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.SOURCE_UPDATE, handleSourceUpdate);
    };
  }, [symbol, subscribe, unsubscribe, refreshSources, calculateStats]);

  return {
    sources,
    stats,
    isLoading,
    error,
    refreshSources
  };
}