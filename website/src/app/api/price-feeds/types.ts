import { NextApiRequest } from 'next';
import {
  PriceData as ImportedPriceData,
  HistoricalPriceData as ImportedHistoricalPriceData,
  PriceSource as ImportedPriceSource,
  PriceMetrics as ImportedPriceMetrics,
  PriceFeedConfig as ImportedPriceFeedConfig,
  ValidationError as ImportedValidationError
} from '../../../services/price-feeds/types/types';

// Re-export imported types
export type PriceData = ImportedPriceData;
export type HistoricalPriceData = ImportedHistoricalPriceData;
export type PriceSource = ImportedPriceSource;
export type PriceMetrics = ImportedPriceMetrics;
export type PriceFeedConfig = ImportedPriceFeedConfig;
export type ValidationError = ImportedValidationError;

// Request Types
export interface AuthenticatedRequest extends NextApiRequest {
  auth: {
    isAuthenticated: boolean;
    walletAddress: string;
  };
}

// API Response Types
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}

// Price Feed API Responses
export interface GetPriceResponse {
  success: boolean;
  data?: {
    currentPrice: PriceData;
    lastUpdate: number;
  };
  error?: string;
}

export interface GetHistoricalPricesResponse {
  success: boolean;
  data?: {
    prices: HistoricalPriceData[];
    timeRange: string;
  };
  error?: string;
}

export interface GetPriceSourcesResponse {
  success: boolean;
  data?: {
    sources: PriceSource[];
    stats: {
      totalSources: number;
      activeSources: number;
      averageDeviation: number;
      lastUpdate: number;
      averageLatency: number;
      reliabilityScore: number;
    };
  };
  error?: string;
}

export interface GetPriceMetricsResponse {
  success: boolean;
  data?: {
    metrics: PriceMetrics;
    symbol: string;
  };
  error?: string;
}

export interface GetPriceConfigResponse {
  success: boolean;
  data?: {
    config: PriceFeedConfig;
    symbol: string;
  };
  error?: string;
}

export interface UpdatePriceConfigResponse {
  success: boolean;
  data?: {
    config: PriceFeedConfig;
    validationErrors?: ValidationError[];
  };
  error?: string;
}

// WebSocket Message Types
export interface WebSocketMessage<T> {
  type: string;
  data: T;
}

export interface PriceUpdateMessage {
  type: string;
  data: {
    symbol: string;
    price: number;
    timestamp: number;
  };
}

export interface SourceUpdateMessage {
  type: string;
  data: {
    symbol: string;
    sources: PriceSource[];
  };
}

export interface MetricsUpdateMessage {
  type: string;
  data: {
    symbol: string;
    metrics: PriceMetrics;
  };
}

export interface ConfigUpdateMessage {
  type: string;
  data: {
    symbol: string;
    config: PriceFeedConfig;
  };
}

// Database Models
export interface PriceDataModel extends PriceData {
  id: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface PriceSourceModel extends PriceSource {
  id: string;
  createdAt: Date;
  updatedAt: Date;
  lastHealthCheck: Date;
  errorCount: number;
  successCount: number;
}

export interface PriceMetricsModel extends PriceMetrics {
  id: string;
  symbol: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface PriceFeedConfigModel extends PriceFeedConfig {
  id: string;
  symbol: string;
  createdAt: Date;
  updatedAt: Date;
  lastModifiedBy: string;
  version: number;
}

// Service Layer Types
export interface PriceFeedService {
  getCurrentPrice(symbol: string): Promise<PriceData>;
  getHistoricalPrices(symbol: string, timeRange: string): Promise<HistoricalPriceData[]>;
  getPriceSources(symbol: string): Promise<{ sources: PriceSource[]; stats: any }>;
  getPriceMetrics(symbol: string): Promise<PriceMetrics>;
  getConfig(symbol: string): Promise<PriceFeedConfig>;
  updateConfig(symbol: string, config: Partial<PriceFeedConfig>): Promise<PriceFeedConfig>;
  validateConfig(config: Partial<PriceFeedConfig>): ValidationError[];
  resetConfig(symbol: string): Promise<PriceFeedConfig>;
}

// Blockchain Integration Types
export interface OnChainPriceData {
  symbol: string;
  price: string; // BigNumber string
  timestamp: string; // BigNumber string
  sourcesCount: string; // BigNumber string
  signature: string;
}

export interface PriceUpdateTransaction {
  symbol: string;
  price: string; // BigNumber string
  timestamp: string; // BigNumber string
  sourcesHash: string;
  signature: string;
  gasLimit: string;
  maxFeePerGas: string;
}

// Error Types
export interface PriceFeedError extends Error {
  code: string;
  details?: any;
  httpStatus?: number;
}

export enum PriceFeedErrorCode {
  INVALID_SYMBOL = 'INVALID_SYMBOL',
  INSUFFICIENT_SOURCES = 'INSUFFICIENT_SOURCES',
  PRICE_DEVIATION_TOO_HIGH = 'PRICE_DEVIATION_TOO_HIGH',
  SOURCE_TIMEOUT = 'SOURCE_TIMEOUT',
  CONFIG_VALIDATION_FAILED = 'CONFIG_VALIDATION_FAILED',
  BLOCKCHAIN_ERROR = 'BLOCKCHAIN_ERROR',
  UNAUTHORIZED = 'UNAUTHORIZED',
  INTERNAL_ERROR = 'INTERNAL_ERROR'
}