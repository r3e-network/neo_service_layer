/**
 * Price Feed Service
 * 
 * This module provides real-time price data with sophisticated filtering and aggregation capabilities.
 * It includes multi-source price aggregation, Kalman filtering, outlier detection, and historical accuracy tracking.
 */

// Re-export the PriceFeedService class
export { PriceFeedService } from './service';

// Re-export the interface types using 'export type' for isolatedModules compatibility
export type { 
  PriceData, 
  PriceStats,
  DataSourceConfig,
  PriceSourceData,
  AggregationResult,
  SourceAccuracyData,
  HistoricalAccuracyMap,
  AdaptiveNoiseParams,
  MultiStateKalman,
  KalmanState,
  KalmanFilterMap
} from './service';
