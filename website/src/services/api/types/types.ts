import { WitnessScope } from '@cityofzion/neon-core/lib/tx';

export interface SignedMessage {
  message: string;
  signature: string;
  publicKey: string;
  salt: string;
  witnessScope?: WitnessScope;
}

export interface ApiEndpoint {
  id: string;
  path: string;
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  description: string;
  requiresAuth: boolean;
  rateLimit: {
    requestsPerMinute: number;
    burstLimit: number;
  };
}

export interface ApiKey {
  id: string;
  name: string;
  key: string;
  createdAt: number;
  lastUsed?: number;
  expiresAt?: number;
  allowedEndpoints: string[];
  allowedIPs?: string[];
  rateLimit?: {
    requestsPerMinute: number;
    burstLimit: number;
  };
}

export interface ApiUsage {
  endpoint: string;
  requests: {
    timestamp: number;
    count: number;
    errors: number;
    latency: number;
  }[];
  totalRequests: number;
  averageLatency: number;
  errorRate: number;
}

export interface ApiMetrics {
  requestsPerSecond: number;
  averageLatency: number;
  errorRate: number;
  activeConnections: number;
  endpointMetrics: Record<string, {
    requests: number;
    errors: number;
    averageLatency: number;
  }>;
  timeseriesData: {
    timestamp: number;
    requests: number;
    errors: number;
    latency: number;
  }[];
}

export interface ApiError {
  code: string;
  message: string;
  details?: Record<string, any>;
  timestamp: number;
}

export interface ApiConfig {
  baseUrl: string;
  timeout: number;
  maxRetries: number;
  retryDelay: number;
  headers: Record<string, string>;
}