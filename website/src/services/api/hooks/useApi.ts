// @ts-ignore
import * as React from 'react';
import { ApiClient } from '../api/apiClient';
import {
  ApiEndpoint,
  ApiKey,
  ApiUsage,
  ApiMetrics,
  ApiError
} from '../types/types';
import { API_BASE_URL, REFRESH_INTERVAL } from '../constants';

export function useApi(privateKey?: string) {
  const [client] = React.useState(() => new ApiClient(API_BASE_URL));
  const [endpoints, setEndpoints] = React.useState<ApiEndpoint[]>([]);
  const [apiKeys, setApiKeys] = React.useState<ApiKey[]>([]);
  const [usage, setUsage] = React.useState<ApiUsage[]>([]);
  const [metrics, setMetrics] = React.useState<ApiMetrics | null>(null);
  const [errors, setErrors] = React.useState<ApiError[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<Error | null>(null);

  React.useEffect(() => {
    if (privateKey) {
      client.setAccount(privateKey).catch((err) => {
        setError(err);
      });
    }
  }, [privateKey, client]);

  const fetchEndpoints = React.useCallback(async () => {
    try {
      const data = await client.getEndpoints();
      setEndpoints(data);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch endpoints'));
    }
  }, [client]);

  const fetchApiKeys = React.useCallback(async () => {
    try {
      const data = await client.getApiKeys();
      setApiKeys(data);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch API keys'));
    }
  }, [client]);

  const fetchUsage = React.useCallback(async (
    options?: {
      startTime?: number;
      endTime?: number;
      endpoint?: string;
    }
  ) => {
    try {
      const data = await client.getApiUsage(options);
      setUsage(data);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch API usage'));
    }
  }, [client]);

  const fetchMetrics = React.useCallback(async (
    options?: {
      interval?: '1m' | '5m' | '1h' | '1d';
      duration?: '1h' | '1d' | '7d' | '30d';
    }
  ) => {
    try {
      const data = await client.getApiMetrics(options);
      setMetrics(data);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch API metrics'));
    }
  }, [client]);

  const fetchErrors = React.useCallback(async (
    options?: {
      startTime?: number;
      endTime?: number;
      endpoint?: string;
      limit?: number;
    }
  ) => {
    try {
      const data = await client.getApiErrors(options);
      setErrors(data);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch API errors'));
    }
  }, [client]);

  const createApiKey = React.useCallback(async (
    name: string,
    options?: {
      allowedEndpoints?: string[];
      allowedIPs?: string[];
      expiresAt?: number;
      rateLimit?: {
        requestsPerMinute: number;
        burstLimit: number;
      };
    }
  ) => {
    try {
      const newKey = await client.createApiKey(name, options);
      setApiKeys((prev) => [...prev, newKey]);
      return newKey;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to create API key');
    }
  }, [client]);

  const updateApiKey = React.useCallback(async (
    id: string,
    updates: {
      name?: string;
      allowedEndpoints?: string[];
      allowedIPs?: string[];
      expiresAt?: number;
      rateLimit?: {
        requestsPerMinute: number;
        burstLimit: number;
      };
    }
  ) => {
    try {
      const updatedKey = await client.updateApiKey(id, updates);
      setApiKeys((prev) =>
        prev.map((key) => (key.id === id ? updatedKey : key))
      );
      return updatedKey;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to update API key');
    }
  }, [client]);

  const deleteApiKey = React.useCallback(async (id: string) => {
    try {
      await client.deleteApiKey(id);
      setApiKeys((prev) => prev.filter((key) => key.id !== id));
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to delete API key');
    }
  }, [client]);

  React.useEffect(() => {
    if (!privateKey) return;

    const fetchData = async () => {
      setLoading(true);
      try {
        await Promise.all([
          fetchEndpoints(),
          fetchApiKeys(),
          fetchUsage(),
          fetchMetrics(),
          fetchErrors()
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchData();

    const interval = setInterval(fetchData, REFRESH_INTERVAL);
    return () => clearInterval(interval);
  }, [
    privateKey,
    fetchEndpoints,
    fetchApiKeys,
    fetchUsage,
    fetchMetrics,
    fetchErrors
  ]);

  return {
    endpoints,
    apiKeys,
    usage,
    metrics,
    errors,
    loading,
    error,
    createApiKey,
    updateApiKey,
    deleteApiKey,
    fetchUsage,
    fetchMetrics,
    fetchErrors
  };
}