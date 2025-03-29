// Price Feed Types

export interface PriceData {
  symbol: string;
  price: number;
  timestamp: number;
  sources: PriceSource[];
}

export interface HistoricalPriceData {
  price: number;
  timestamp: number;
}

export interface PriceSource {
  id: string;
  currentPrice: number;
  status: 'active' | 'inactive' | 'error';
  lastUpdated: number;
  deviation?: number;
  weight?: number;
}

export interface PriceConfig {
  deviationThreshold: number;
  minSourceCount: number;
  customSourceWeights?: Record<string, number>;
  updateInterval?: number;
  stalePriceThreshold?: number;
}

export interface PriceMetrics {
  totalUpdates: number;
  successRate: number;
  averageLatency: number;
  lastUpdateTime: number;
  errorCount: number;
  sourcesCount: number;
  activeSourcesCount: number;
}

// Hook Return Types

export interface UsePriceConfigResult {
  config: PriceConfig | null;
  isLoading: boolean;
  error: string | null;
  updateConfig: (newConfig: Partial<PriceConfig>) => Promise<boolean>;
}

export interface UsePriceFeedResult {
  data: PriceData | null;
  isLoading: boolean;
  error: string | null;
  subscribe: () => void;
  unsubscribe: () => void;
}

export interface UsePriceHistoryResult {
  data: HistoricalPriceData[];
  isLoading: boolean;
  error: string | null;
  fetchHistory: (timeRange: string) => Promise<void>;
}

export interface UsePriceMetricsResult {
  metrics: PriceMetrics | null;
  isLoading: boolean;
  error: string | null;
  subscribe: () => void;
  unsubscribe: () => void;
}

export interface UsePriceSourcesResult {
  sources: PriceSource[];
  isLoading: boolean;
  error: string | null;
  subscribe: () => void;
  unsubscribe: () => void;
}

// WebSocket Event Types

export interface WebSocketMessage {
  type: string;
  channel?: string;
  data?: any;
}

export interface PriceUpdateEvent {
  symbol: string;
  price: number;
  timestamp: number;
  sources?: PriceSource[];
}

export interface ConfigUpdateEvent {
  symbol: string;
  config: PriceConfig;
}

export interface MetricsUpdateEvent {
  metrics: PriceMetrics;
}

export interface SourcesUpdateEvent {
  symbol: string;
  sources: PriceSource[];
}

// API Response Types

export interface PriceResponse {
  symbol: string;
  price: number;
  timestamp: number;
  sources: PriceSource[];
}

export interface HistoricalPriceResponse {
  symbol: string;
  timeframe: string;
  prices: HistoricalPriceData[];
}

export interface SourcesResponse {
  symbol: string;
  sources: PriceSource[];
}

export interface ConfigResponse {
  symbol: string;
  config: PriceConfig;
}

export interface ConfigUpdateResponse {
  success: boolean;
  errors?: string[];
}

export interface MetricsResponse {
  metrics: PriceMetrics;
}
