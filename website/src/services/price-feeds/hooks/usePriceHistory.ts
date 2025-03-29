import React from 'react';
import { PRICE_FEED_CONSTANTS } from '../constants';
import {
  UsePriceHistoryResult,
  HistoricalPriceData
} from '../types/types';

export function usePriceHistory(symbol: string): UsePriceHistoryResult {
  const [data, setData] = React.useState<HistoricalPriceData[]>([]);
  const [isLoading, setIsLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  const fetchHistory = React.useCallback(async (timeRange: string) => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await fetch(
        `${PRICE_FEED_CONSTANTS.API_ENDPOINT}/${symbol}/history?timeRange=${timeRange}`
      );

      if (!response.ok) {
        throw new Error('Failed to fetch price history');
      }

      const historyData: HistoricalPriceData[] = await response.json();
      setData(historyData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch price history');
      setData([]);
    } finally {
      setIsLoading(false);
    }
  }, [symbol]);

  React.useEffect(() => {
    // Fetch initial data for last 24 hours
    fetchHistory('24h');
  }, [fetchHistory]);

  return {
    data,
    isLoading,
    error,
    fetchHistory
  };
}