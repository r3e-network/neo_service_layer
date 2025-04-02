'use client';

import React from 'react';
import { PriceFeedService, PriceData } from '@/services/price-feeds';

// Define types for the hook
interface UsePriceFeedOptions {
  initialSymbol?: string;
  autoFetch?: boolean;
  refreshInterval?: number;
}

interface UsePriceFeedReturn {
  loading: boolean;
  error: string | null;
  data: PriceData | null;
  symbols: string[];
  fetchPrice: (symbol: string, preferredSource?: string) => Promise<void>;
  selectedSymbol: string | null;
  setSelectedSymbol: (symbol: string) => void;
  preferredSource: string | null;
  setPreferredSource: (source: string | null) => void;
}

// Common crypto symbols
const DEFAULT_SYMBOLS = ['NEO/USD', 'BTC/USD', 'ETH/USD', 'GAS/USD'];

/**
 * Hook for interacting with the price feed service
 * 
 * @param options - Configuration options for the hook
 * @returns Interface for interacting with price feed data
 */
export function usePriceFeed(options: UsePriceFeedOptions = {}): UsePriceFeedReturn {
  const { 
    initialSymbol = 'NEO/USD',
    autoFetch = true,
    refreshInterval = 30000 // 30 seconds
  } = options;

  // State
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const [data, setData] = React.useState<PriceData | null>(null);
  const [symbols] = React.useState<string[]>(DEFAULT_SYMBOLS);
  const [selectedSymbol, setSelectedSymbol] = React.useState<string | null>(initialSymbol);
  const [preferredSource, setPreferredSource] = React.useState<string | null>(null);
  const [service, setService] = React.useState<PriceFeedService | null>(null);

  // Initialize service
  React.useEffect(() => {
    // In a real implementation, we would need to initialize the service
    // with proper configuration. For demonstration purposes, we'll mock this.
    const initService = async () => {
      try {
        // This would typically come from a DI container or context
        // const newService = new PriceFeedService({
        //   teeEnabled: true,
        //   contractService: neoContractService,
        //   vault: secretVault
        // });
        
        // For now, we'll assume the service is globally available or mocked
        // setService(newService);
        
        // Mock: In real implementation remove this code
        setService({
          getAggregatedPrice: async (symbol: string) => {
            // Simulate API delay
            await new Promise(resolve => setTimeout(resolve, 500));
            
            // Generate mock price data
            const basePrice = symbol.includes('BTC') ? 67500 : 
                           symbol.includes('ETH') ? 3490 : 
                           symbol.includes('NEO') ? 21.5 : 
                           symbol.includes('GAS') ? 11.2 : 100;
            
            // Add some randomness
            const randomFactor = 0.995 + Math.random() * 0.01; // ±0.5%
            
            return {
              symbol,
              price: basePrice * randomFactor,
              timestamp: new Date().toISOString(),
              source: 'aggregated',
              confidence: 0.95 + Math.random() * 0.05,
              details: {
                lastUpdate: new Date().toISOString(),
                sources: [
                  { name: 'Coinbase', price: basePrice * (1 + 0.001 * Math.random()), weight: 0.4, timestamp: new Date().toISOString() },
                  { name: 'Binance', price: basePrice * (1 - 0.002 * Math.random()), weight: 0.4, timestamp: new Date().toISOString() },
                  { name: 'Kraken', price: basePrice * (1 + 0.003 * Math.random()), weight: 0.2, timestamp: new Date().toISOString() }
                ]
              },
              stats: {
                mean: basePrice,
                median: basePrice * 0.999,
                stdDev: basePrice * 0.005,
                volatility: 0.03,
                outliers: []
              }
            } as PriceData;
          }
        } as unknown as PriceFeedService);
      } catch (err) {
        setError(`Failed to initialize price feed service: ${err instanceof Error ? err.message : String(err)}`);
      }
    };

    initService();
    
    // Cleanup function
    return () => {
      // Any cleanup needed for the service
    };
  }, []);

  // Fetch price data function
  const fetchPrice = React.useCallback(async (symbol: string, source?: string) => {
    if (!service) {
      setError('Price feed service not initialized');
      return;
    }

    setLoading(true);
    setError(null);
    
    try {
      const priceData = await service.getAggregatedPrice(symbol, source || undefined);
      setData(priceData);
    } catch (err) {
      setError(`Failed to fetch price data: ${err instanceof Error ? err.message : String(err)}`);
      setData(null);
    } finally {
      setLoading(false);
    }
  }, [service]);

  // Auto-fetch when selected symbol changes
  React.useEffect(() => {
    if (selectedSymbol && autoFetch) {
      fetchPrice(selectedSymbol, preferredSource || undefined);
    }
  }, [selectedSymbol, preferredSource, fetchPrice, autoFetch]);

  // Set up refresh interval
  React.useEffect(() => {
    if (!autoFetch || !selectedSymbol || refreshInterval <= 0) {
      return;
    }

    const intervalId = setInterval(() => {
      if (selectedSymbol) {
        fetchPrice(selectedSymbol, preferredSource || undefined);
      }
    }, refreshInterval);

    return () => clearInterval(intervalId);
  }, [selectedSymbol, preferredSource, fetchPrice, autoFetch, refreshInterval]);

  return {
    loading,
    error,
    data,
    symbols,
    fetchPrice,
    selectedSymbol,
    setSelectedSymbol,
    preferredSource,
    setPreferredSource
  };
} 