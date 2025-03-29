/**
 * Types for the metrics service
 */

export interface ServiceMetric {
  id: string;
  serviceName: string;
  metricName: string;
  value: number;
  timestamp: number;
  unit: string;
  type: MetricType;
  labels: Record<string, string>;
}

export type MetricType = 'counter' | 'gauge' | 'histogram';

export interface MetricSeries {
  id: string;
  serviceName: string;
  metricName: string;
  dataPoints: DataPoint[];
  unit: string;
  type: MetricType;
  labels: Record<string, string>;
}

export interface DataPoint {
  timestamp: number;
  value: number;
}

export interface ServiceHealth {
  serviceName: string;
  status: 'healthy' | 'degraded' | 'unhealthy';
  lastChecked: number;
  message?: string;
  metrics: {
    uptime: number;
    latency: number;
    errorRate: number;
    requestRate: number;
  };
}

export interface SystemMetrics {
  cpu: {
    usage: number;
    cores: number;
  };
  memory: {
    total: number;
    used: number;
    free: number;
  };
  disk: {
    total: number;
    used: number;
    free: number;
  };
  network: {
    bytesIn: number;
    bytesOut: number;
    connectionsActive: number;
  };
}

export interface ServiceMetrics {
  priceFeed: {
    totalFeeds: number;
    activeFeeds: number;
    updateFrequency: number;
    lastUpdateTime: number;
    averageLatency: number;
    errorRate: number;
  };
  gasBank: {
    totalBalance: number;
    activeUsers: number;
    transactionsPerMinute: number;
    averageGasPrice: number;
    failureRate: number;
  };
  trigger: {
    totalTriggers: number;
    activeTriggers: number;
    executionsPerMinute: number;
    averageExecutionTime: number;
    failureRate: number;
  };
  functions: {
    totalFunctions: number;
    activeFunctions: number;
    executionsPerMinute: number;
    averageExecutionTime: number;
    failureRate: number;
  };
  secrets: {
    totalSecrets: number;
    accessesPerMinute: number;
    failureRate: number;
  };
  api: {
    requestsPerMinute: number;
    averageLatency: number;
    errorRate: number;
    activeConnections: number;
  };
}