import { PRICE_FEED_CONSTANTS } from '../constants';

// Price Data Types
export interface PriceData {
  price: number;
  timestamp: number;
  symbol: string;
}

export interface HistoricalPriceData extends PriceData {
  volume: number;
  high: number;
  low: number;
}

// Price Source Types
export interface PriceSource {
  id: string;
  name: string;
  currentPrice: number;
  lastUpdate: number;
  deviation: number;
  status: PriceSourceStatus;
  weight: number;
  latency: number;
  reliability: number;
}

export type PriceSourceStatus = 'active' | 'warning' | 'error';

export interface SourceStats {
  totalSources: number;
  activeSources: number;
  averageDeviation: number;
  lastUpdate: number;
  averageLatency: number;
  reliabilityScore: number;
}

// Price Metrics Types
export interface PriceMetrics {
  volume24h: number;
  volumeChange24h: number;
  priceChange24h: number;
  priceChangePercent24h: number;
  sourceCountChange24h: number;
  highPrice24h: number;
  lowPrice24h: number;
  updateCount24h: number;
  averageLatency: number;
  volatility24h: number;
  marketCap?: number;
  liquidity24h?: number;
}

// Configuration Types
export interface PriceFeedConfig {
  heartbeatInterval: number;
  deviationThreshold: number;
  minSourceCount: number;
  isActive: boolean;
  updateMethod: PriceUpdateMethod;
  customSourceWeights?: Record<string, number>;
  alertThresholds?: AlertThresholds;
  backupSources?: string[];
}

export type PriceUpdateMethod = 'auto' | 'manual';

export interface AlertThresholds {
  priceDeviation: number;
  sourceLatency: number;
  minReliability: number;
}

export interface ValidationError {
  field: string;
  message: string;
}

// WebSocket Event Types
export interface PriceUpdateEvent {
  type: typeof PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.PRICE_UPDATE;
  data: {
    symbol: string;
    price: number;
    timestamp: number;
  };
}

export interface SourceUpdateEvent {
  type: typeof PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.SOURCE_UPDATE;
  data: {
    symbol: string;
    sources: PriceSource[];
  };
}

export interface MetricsUpdateEvent {
  type: typeof PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.METRICS_UPDATE;
  data: {
    symbol: string;
    metrics: PriceMetrics;
  };
}

export interface ConfigUpdateEvent {
  type: typeof PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.CONFIG_UPDATE;
  data: {
    symbol: string;
    config: PriceFeedConfig;
  };
}

export type PriceFeedEvent = 
  | PriceUpdateEvent 
  | SourceUpdateEvent 
  | MetricsUpdateEvent 
  | ConfigUpdateEvent;

// Hook Result Types
export interface UsePriceFeedResult {
  currentPrice: number | null;
  lastUpdate: number | null;
  historicalPrices: HistoricalPriceData[];
  sources: PriceSource[];
  sourceStats: SourceStats;
  metrics: PriceMetrics | null;
  config: PriceFeedConfig;
  isLoading: boolean;
  error: string | null;
  updateConfig: (newConfig: Partial<PriceFeedConfig>) => Promise<void>;
  refreshData: () => Promise<void>;
}

export interface UsePriceHistoryResult {
  data: HistoricalPriceData[];
  isLoading: boolean;
  error: string | null;
  fetchHistory: (timeRange: string) => Promise<void>;
}

export interface UsePriceSourcesResult {
  sources: PriceSource[];
  stats: SourceStats;
  isLoading: boolean;
  error: string | null;
  refreshSources: () => Promise<void>;
}

export interface UsePriceMetricsResult {
  metrics: PriceMetrics | null;
  isLoading: boolean;
  error: string | null;
  refreshMetrics: () => Promise<void>;
}

export interface UsePriceConfigResult {
  config: PriceFeedConfig;
  isLoading: boolean;
  error: string | null;
  validationErrors: ValidationError[];
  updateConfig: (newConfig: Partial<PriceFeedConfig>) => Promise<void>;
  resetConfig: () => Promise<void>;
  validateConfig: (config: Partial<PriceFeedConfig>) => ValidationError[];
}

// Component Props Types
export interface PriceFeedDashboardProps {
  symbol: string;
  className?: string;
}

export interface PriceChartProps {
  data: HistoricalPriceData[];
  isLoading: boolean;
  onTimeRangeChange: (range: string) => void;
  className?: string;
}

export interface PriceSourceListProps {
  sources: PriceSource[];
  stats: SourceStats;
  isLoading: boolean;
  className?: string;
}

export interface PriceFeedConfigProps {
  config: PriceFeedConfig;
  onConfigUpdate: (newConfig: Partial<PriceFeedConfig>) => Promise<void>;
  onConfigReset: () => Promise<void>;
  validationErrors: ValidationError[];
  isLoading: boolean;
  className?: string;
}

export interface MetricsCardProps {
  title: string;
  value: number | string;
  previousValue?: number | string;
  format?: 'number' | 'currency' | 'percent';
  precision?: number;
  isLoading?: boolean;
  className?: string;
}

export interface AlertBannerProps {
  title?: string;
  message: string;
  severity: 'info' | 'success' | 'warning' | 'error';
  onClose?: () => void;
  className?: string;
}