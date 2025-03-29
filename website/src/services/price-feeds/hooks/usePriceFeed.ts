import React from 'react';
import { useWebSocketEvents } from '../../../hooks/useWebSocketEvents';
import { PRICE_FEED_CONSTANTS } from '../constants';
import {
  UsePriceFeedResult,
  PriceData,
  HistoricalPriceData,
  PriceSource,
  SourceStats,
  PriceMetrics,
  PriceFeedConfig,
  PriceFeedEvent,
  PriceUpdateEvent,
  SourceUpdateEvent,
  MetricsUpdateEvent,
  ConfigUpdateEvent
} from '../types/types';

export function usePriceFeed(symbol: string): UsePriceFeedResult {
  const [currentPrice, setCurrentPrice] = React.useState<number | null>(null);
  const [lastUpdate, setLastUpdate] = React.useState<number | null>(null);
  const [historicalPrices, setHistoricalPrices] = React.useState<HistoricalPriceData[]>([]);
  const [sources, setSources] = React.useState<PriceSource[]>([]);
  const [sourceStats, setSourceStats] = React.useState<SourceStats>({
    totalSources: 0,
    activeSources: 0,
    averageDeviation: 0,
    lastUpdate: 0,
    averageLatency: 0,
    reliabilityScore: 0
  });
  const [metrics, setMetrics] = React.useState<PriceMetrics | null>(null);
  const [config, setConfig] = React.useState<PriceFeedConfig>({
    heartbeatInterval: PRICE_FEED_CONSTANTS.HEARTBEAT_INTERVAL,
    deviationThreshold: PRICE_FEED_CONSTANTS.DEFAULT_DEVIATION_THRESHOLD,
    minSourceCount: PRICE_FEED_CONSTANTS.MIN_SOURCE_COUNT,
    isActive: true,
    updateMethod: 'auto'
  });
  const [isLoading, setIsLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  const { subscribe, unsubscribe } = useWebSocketEvents({
    url: `${PRICE_FEED_CONSTANTS.WS_ENDPOINT}/price-feeds`
  });

  const calculateSourceStats = React.useCallback((sourceList: PriceSource[]): SourceStats => {
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

  const updateConfig = React.useCallback(async (newConfig: Partial<PriceFeedConfig>): Promise<void> => {
    try {
      const response = await fetch(`${PRICE_FEED_CONSTANTS.API_ENDPOINT}/${symbol}/config`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(newConfig)
      });

      if (!response.ok) throw new Error('Failed to update configuration');

      const updatedConfig = await response.json();
      setConfig(updatedConfig);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update configuration');
      throw err;
    }
  }, [symbol]);

  const refreshData = React.useCallback(async (): Promise<void> => {
    setIsLoading(true);
    setError(null);

    try {
      const [priceResponse, sourcesResponse, metricsResponse, configResponse] = await Promise.all([
        fetch(`${PRICE_FEED_CONSTANTS.API_ENDPOINT}/${symbol}/price`),
        fetch(`${PRICE_FEED_CONSTANTS.API_ENDPOINT}/${symbol}/sources`),
        fetch(`${PRICE_FEED_CONSTANTS.API_ENDPOINT}/${symbol}/metrics`),
        fetch(`${PRICE_FEED_CONSTANTS.API_ENDPOINT}/${symbol}/config`)
      ]);

      if (!priceResponse.ok || !sourcesResponse.ok || !metricsResponse.ok || !configResponse.ok) {
        throw new Error('Failed to fetch price feed data');
      }

      const [priceData, sourcesData, metricsData, configData] = await Promise.all([
        priceResponse.json(),
        sourcesResponse.json(),
        metricsResponse.json(),
        configResponse.json()
      ]);

      setCurrentPrice(priceData.price);
      setLastUpdate(priceData.timestamp);
      setSources(sourcesData);
      setSourceStats(calculateSourceStats(sourcesData));
      setMetrics(metricsData);
      setConfig(configData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch price feed data');
    } finally {
      setIsLoading(false);
    }
  }, [symbol, calculateSourceStats]);

  React.useEffect(() => {
    let isSubscribed = true;

    const handlePriceUpdate = (event: PriceUpdateEvent['data']) => {
      if (!isSubscribed || event.symbol !== symbol) return;
      setCurrentPrice(event.price);
      setLastUpdate(event.timestamp);
    };

    const handleSourceUpdate = (event: SourceUpdateEvent['data']) => {
      if (!isSubscribed || event.symbol !== symbol) return;
      setSources(event.sources);
      setSourceStats(calculateSourceStats(event.sources));
    };

    const handleMetricsUpdate = (event: MetricsUpdateEvent['data']) => {
      if (!isSubscribed || event.symbol !== symbol) return;
      setMetrics(event.metrics);
    };

    const handleConfigUpdate = (event: ConfigUpdateEvent['data']) => {
      if (!isSubscribed || event.symbol !== symbol) return;
      setConfig(event.config);
    };

    // Initial data fetch
    refreshData();

    // Subscribe to WebSocket events
    subscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.PRICE_UPDATE, handlePriceUpdate);
    subscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.SOURCE_UPDATE, handleSourceUpdate);
    subscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.METRICS_UPDATE, handleMetricsUpdate);
    subscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.CONFIG_UPDATE, handleConfigUpdate);

    // Polling fallback
    const pollInterval = setInterval(() => {
      refreshData();
    }, PRICE_FEED_CONSTANTS.UPDATE_INTERVAL);

    return () => {
      isSubscribed = false;
      clearInterval(pollInterval);
      unsubscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.PRICE_UPDATE, handlePriceUpdate);
      unsubscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.SOURCE_UPDATE, handleSourceUpdate);
      unsubscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.METRICS_UPDATE, handleMetricsUpdate);
      unsubscribe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.CONFIG_UPDATE, handleConfigUpdate);
    };
  }, [symbol, subscribe, unsubscribe, refreshData, calculateSourceStats]);

  return {
    currentPrice,
    lastUpdate,
    historicalPrices,
    sources,
    sourceStats,
    metrics,
    config,
    isLoading,
    error,
    updateConfig,
    refreshData
  };
}