import React from 'react';
import { ServiceMetric, ServiceHealth, SystemMetrics, ServiceMetrics } from '../types/types';
import { metricsApi } from '../api/metricsApi';
import { METRICS_CONSTANTS } from '../constants';

export function useMetrics() {
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<Error | null>(null);
  const [systemMetrics, setSystemMetrics] = React.useState<SystemMetrics | null>(null);
  const [serviceMetrics, setServiceMetrics] = React.useState<ServiceMetrics | null>(null);
  const [serviceHealth, setServiceHealth] = React.useState<ServiceHealth[]>([]);
  const [alerts, setAlerts] = React.useState<any[]>([]);

  const fetchSystemMetrics = React.useCallback(async () => {
    try {
      setLoading(true);
      const metrics = await metricsApi.getSystemMetrics();
      setSystemMetrics(metrics);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch system metrics'));
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchServiceMetrics = React.useCallback(async () => {
    try {
      setLoading(true);
      const metrics = await metricsApi.getAllServicesMetrics();
      setServiceMetrics(metrics);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch service metrics'));
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchServiceHealth = React.useCallback(async () => {
    try {
      setLoading(true);
      const health = await metricsApi.getServicesHealth();
      setServiceHealth(health);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch service health'));
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchAlerts = React.useCallback(async () => {
    try {
      setLoading(true);
      const alertsData = await metricsApi.getAlerts();
      setAlerts(alertsData.alerts || []);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch alerts'));
    } finally {
      setLoading(false);
    }
  }, []);

  const getServiceMetricHistory = React.useCallback(
    async (serviceName: string, metricName: string, timeRange: number) => {
      try {
        return await metricsApi.getMetricHistory(serviceName, metricName, timeRange.toString());
      } catch (err) {
        console.error('Failed to fetch metric history:', err);
        return [];
      }
    },
    []
  );

  React.useEffect(() => {
    fetchSystemMetrics();
    fetchServiceMetrics();
    fetchServiceHealth();
    fetchAlerts();

    const interval = setInterval(() => {
      fetchSystemMetrics();
      fetchServiceMetrics();
      fetchServiceHealth();
      fetchAlerts();
    }, METRICS_CONSTANTS.REFRESH_INTERVAL);

    return () => clearInterval(interval);
  }, [fetchSystemMetrics, fetchServiceMetrics, fetchServiceHealth, fetchAlerts]);

  return {
    loading,
    error,
    systemMetrics,
    serviceMetrics,
    serviceHealth,
    alerts,
    getServiceMetricHistory
  };
}