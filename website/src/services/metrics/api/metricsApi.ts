import { ServiceMetric, ServiceHealth, SystemMetrics, ServiceMetrics } from '../types/types';
import { API_BASE_URL } from '../constants';

/**
 * Metrics API client for interacting with the metrics service
 */
class MetricsApi {
  private baseUrl: string;

  constructor() {
    this.baseUrl = `${API_BASE_URL}/metrics`;
  }

  /**
   * Fetch metrics for a specific service
   */
  async getServiceMetrics(serviceName: string, timeRange: string): Promise<ServiceMetric[]> {
    const response = await fetch(`${this.baseUrl}/services/${serviceName}?timeRange=${timeRange}`);
    if (!response.ok) {
      throw new Error(`Failed to fetch metrics for service ${serviceName}`);
    }
    return response.json();
  }

  /**
   * Fetch health status for all services
   */
  async getServicesHealth(): Promise<ServiceHealth[]> {
    const response = await fetch(`${this.baseUrl}/health`);
    if (!response.ok) {
      throw new Error('Failed to fetch services health');
    }
    return response.json();
  }

  /**
   * Fetch system metrics
   */
  async getSystemMetrics(): Promise<SystemMetrics> {
    const response = await fetch(`${this.baseUrl}/system`);
    if (!response.ok) {
      throw new Error('Failed to fetch system metrics');
    }
    return response.json();
  }

  /**
   * Fetch aggregated metrics for all services
   */
  async getAllServicesMetrics(): Promise<ServiceMetrics> {
    const response = await fetch(`${this.baseUrl}/services`);
    if (!response.ok) {
      throw new Error('Failed to fetch all services metrics');
    }
    return response.json();
  }

  /**
   * Fetch historical metrics for a specific metric name
   */
  async getMetricHistory(
    serviceName: string,
    metricName: string,
    timeRange: string
  ): Promise<ServiceMetric[]> {
    const response = await fetch(`${this.baseUrl}/history/${serviceName}/${metricName}?timeRange=${timeRange}`);
    if (!response.ok) {
      throw new Error(`Failed to fetch metric history for ${metricName}`);
    }
    return response.json();
  }

  /**
   * Fetch alerts and warnings
   */
  async getAlerts(): Promise<{
    alerts: Array<{
      id: string;
      serviceName: string;
      message: string;
      severity: 'critical' | 'warning';
      timestamp: number;
    }>;
  }> {
    const response = await fetch(`${this.baseUrl}/alerts`);
    if (!response.ok) {
      throw new Error('Failed to fetch alerts');
    }
    return response.json();
  }
}

export const metricsApi = new MetricsApi();