import React from 'react';
import { useWebSocket } from '../../../hooks/useWebSocket';
import { useAuth } from '../../../hooks/useAuth';
import { PRICE_FEED_CONSTANTS } from '../constants';
import { 
  PriceFeedConfig, 
  ValidationError, 
  UsePriceConfigResult 
} from '../types/types';

export function usePriceConfig(symbol: string): UsePriceConfigResult {
  const [config, setConfig] = React.useState<PriceFeedConfig>({
    heartbeatInterval: PRICE_FEED_CONSTANTS.HEARTBEAT_INTERVAL,
    deviationThreshold: PRICE_FEED_CONSTANTS.DEFAULT_DEVIATION_THRESHOLD,
    minSourceCount: PRICE_FEED_CONSTANTS.MIN_SOURCE_COUNT,
    alertThresholds: {
      priceDeviation: 0.05,
      sourceLatency: 5000,
      minReliability: 0.95
    },
    updateMethod: 'auto'
  });
  const [isLoading, setIsLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [validationErrors, setValidationErrors] = React.useState<ValidationError[]>([]);

  const { isAuthenticated } = useAuth();
  const { send, disconnect } = useWebSocket({
    url: `${PRICE_FEED_CONSTANTS.WS_ENDPOINT}/price-feeds`
  });

  const validateConfig = React.useCallback((configToValidate: Partial<PriceFeedConfig>): ValidationError[] => {
    const errors: ValidationError[] = [];
    
    // Add validation logic here
    
    return errors;
  }, []);

  const fetchConfig = React.useCallback(async () => {
    if (!isAuthenticated) return;
    
    setIsLoading(true);
    try {
      // Simulate API call for heartbeat interval
      const interval = config.heartbeatInterval;
      if (interval !== undefined && interval < PRICE_FEED_CONSTANTS.HEARTBEAT_INTERVAL / 2) {
        setValidationErrors([{
          field: 'heartbeatInterval',
          message: `Heartbeat interval must be at least ${PRICE_FEED_CONSTANTS.HEARTBEAT_INTERVAL / 2}ms`
        }]);
        return;
      }
      
      // Simulate API call for deviation threshold
      const threshold = config.deviationThreshold;
      if (threshold !== undefined && (threshold < 0 || threshold > 100)) {
        setValidationErrors([{
          field: 'deviationThreshold',
          message: 'Deviation threshold must be between 0 and 100'
        }]);
        return;
      }
      
      // Simulate API call for min source count
      const count = config.minSourceCount;
      if (count !== undefined && count < 1) {
        setValidationErrors([{
          field: 'minSourceCount',
          message: 'Minimum source count must be at least 1'
        }]);
        return;
      }
      
      // Clear validation errors if all checks pass
      setValidationErrors([]);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch config');
    } finally {
      setIsLoading(false);
    }
  }, [config, isAuthenticated]);

  const updateConfig = React.useCallback((updates: Partial<PriceFeedConfig>) => {
    setConfig(prevConfig => ({ ...prevConfig, ...updates }));
  }, []);

  const resetConfig = React.useCallback(() => {
    setConfig({
      heartbeatInterval: PRICE_FEED_CONSTANTS.HEARTBEAT_INTERVAL,
      deviationThreshold: PRICE_FEED_CONSTANTS.DEFAULT_DEVIATION_THRESHOLD,
      minSourceCount: PRICE_FEED_CONSTANTS.MIN_SOURCE_COUNT,
      alertThresholds: {
        priceDeviation: 0.05,
        sourceLatency: 5000,
        minReliability: 0.95
      },
      updateMethod: 'auto'
    });
  }, []);

  // Fetch config on mount and when symbol changes
  React.useEffect(() => {
    fetchConfig();
    
    // Cleanup
    return () => {
      disconnect();
    };
  }, [symbol, fetchConfig, disconnect]);

  return {
    config,
    isLoading,
    error,
    validationErrors,
    updateConfig,
    resetConfig,
    validateConfig
  };
}