/**
 * @file price-feed.ts
 * @description Advanced price feed service for the Neo N3 network that provides reliable,
 * real-time price data with sophisticated filtering and aggregation capabilities.
 * 
 * Key features:
 * - Multi-source price aggregation with weighted averaging
 * - Kalman filtering with adaptive noise parameters
 * - Multi-dimensional state tracking (price, velocity, acceleration)
 * - Outlier detection and handling
 * - Historical accuracy tracking
 * - Comprehensive metrics and monitoring
 * - Secure API key management
 * - Automatic blockchain updates
 * 
 * @module PriceFeedService
 * @author Neo Service Layer Team
 * @version 1.0.0
 */

import { NeoContractService } from '@/services/neo-contract';
import { SecretVault } from '@/utils/vault';
import { Logger } from '@/utils/logger';
import { MetricsService } from '@/services/metrics';
import { EventEmitter } from 'events';

export interface PriceFeedServiceConfig {
  teeEnabled: boolean;
  contractService: NeoContractService;
  vault: SecretVault;
}

export interface PriceData {
  symbol: string;
  price: number;
  timestamp: string;
  source: string;
  confidence: number;
  details: {
    volume24h?: number;
    marketCap?: number;
    lastUpdate: string;
    sources: Array<{
      name: string;
      price: number;
      weight: number;
      timestamp: string;
    }>;
  };
  stats?: PriceStats;
}

// #region Type Definitions

/**
 * Configuration for individual price data sources
 * @interface DataSourceConfig
 */
export interface DataSourceConfig {
  /** Unique identifier for the data source */
  name: string;
  /** Base weight for price aggregation (0-1) */
  weight: number;
  /** Base URL for the price API */
  baseUrl: string;
  /** Reference to the API key in the vault */
  apiKeySecret: string;
  /** Request timeout in milliseconds */
  timeout: number;
  /** Maximum requests per minute */
  rateLimit: number;
}

/**
 * Raw price data from a single source
 * @interface PriceSourceData
 */
export interface PriceSourceData {
  /** Source configuration */
  source: DataSourceConfig;
  /** Current price value */
  price: number;
  /** ISO timestamp of the price update */
  timestamp: string;
}

/**
 * Statistical metrics for price aggregation
 * @interface PriceStats
 */
export interface PriceStats {
  /** Arithmetic mean of valid prices */
  mean: number;
  /** Median of valid prices */
  median: number;
  /** Standard deviation of valid prices */
  stdDev: number;
  /** Current price volatility */
  volatility: number;
  /** Detected price outliers */
  outliers: Array<{
    /** Source identifier */
    source: string;
    /** Outlier price value */
    price: number;
    /** Deviation from mean in standard deviations */
    deviation: number;
  }>;
}

/**
 * Result of price aggregation algorithm
 * @interface AggregationResult
 */
export interface AggregationResult {
  /** Aggregated price value */
  price: number;
  /** Confidence score (0-1) */
  confidence: number;
  /** Detailed source information */
  details: PriceData['details'];
  /** Statistical metrics */
  stats: PriceStats;
}

/**
 * Historical accuracy metrics for a price source
 * @interface SourceAccuracyData
 */
export interface SourceAccuracyData {
  /** Mean absolute error */
  meanError: number;
  /** Last update timestamp */
  lastUpdate: number;
  /** Total number of updates */
  updateCount: number;
  /** Success rate (0-1) */
  successRate: number;
  /** Volatility score */
  volatilityScore: number;
}

/**
 * Map of source accuracy data by source name
 * @interface HistoricalAccuracyMap
 */
export interface HistoricalAccuracyMap {
  [source: string]: SourceAccuracyData;
}

/**
 * Parameters for adaptive noise estimation
 * @interface AdaptiveNoiseParams
 */
export interface AdaptiveNoiseParams {
  /** Current measurement noise estimate */
  measurementNoise: number;
  /** Current process noise estimate */
  processNoise: number;
  /** Rate of parameter adaptation */
  adaptationRate: number;
  /** Last innovation value */
  lastInnovation: number;
  /** Innovation variance estimate */
  innovationVariance: number;
  /** Total update count */
  updateCount: number;
}

/**
 * Multi-dimensional Kalman filter state
 * @interface MultiStateKalman
 */
export interface MultiStateKalman {
  /** State vector [price, velocity, acceleration] */
  state: number[];
  /** State covariance matrix */
  covariance: number[][];
  /** Last update timestamp */
  lastUpdate: number;
  /** Adaptive noise parameters */
  noiseParams: AdaptiveNoiseParams;
}

/**
 * Single-dimensional Kalman filter state
 * @interface KalmanState
 */
export interface KalmanState {
  /** Current price estimate */
  estimate: number;
  /** Error covariance */
  errorCovariance: number;
  /** Last update timestamp */
  lastUpdate: number;
  /** Adaptive noise parameters */
  noiseParams: AdaptiveNoiseParams;
  /** Optional multi-state tracking */
  multiState?: MultiStateKalman;
}

/**
 * Map of Kalman filter states by source name
 * @interface KalmanFilterMap
 */
export interface KalmanFilterMap {
  [symbol: string]: KalmanState;
}

// #endregion Type Definitions

export class PriceFeedService extends EventEmitter {
  private config: PriceFeedServiceConfig;
  private logger: Logger;
  private metrics: MetricsService;
  private dataSources: DataSourceConfig[];
  private lastUpdate: Map<string, number> = new Map();
  private priceHistory: Map<string, Array<{ price: number; timestamp: number }>> = new Map();
  private historicalAccuracy: Map<string, HistoricalAccuracyMap> = new Map();
  private updateInterval: number = 60 * 1000; // 1 minute
  private retryAttempts: number = 3;
  private retryDelay: number = 1000; // 1 second
  private historyWindow: number = 24 * 60 * 60 * 1000; // 24 hours
  private outlierThreshold: number = 2; // Standard deviations
  private minSourcesRequired: number = 3;
  private volatilityThreshold: number = 0.1; // 10%
  private accuracyDecayFactor: number = 0.95; // Exponential decay for historical accuracy
  private accuracyUpdateInterval: number = 5 * 60 * 1000; // 5 minutes
  private minAccuracyUpdates: number = 10; // Minimum updates before considering historical accuracy
  private kalmanFilters: Map<string, KalmanFilterMap> = new Map();
  private readonly baseProcessNoise: number = 0.001; // Base Q - process noise
  private readonly baseMeasurementNoise: number = 0.1; // Base R - measurement noise
  private readonly adaptationRate: number = 0.1; // Rate of noise parameter adaptation
  private readonly innovationWindow: number = 30; // Window for innovation statistics
  private readonly minInnovationVariance: number = 1e-6; // Minimum innovation variance
  private readonly maxNoiseRatio: number = 100; // Maximum ratio of adapted/base noise
  private readonly minNoiseRatio: number = 0.01; // Minimum ratio of adapted/base noise
  private readonly initialErrorCovariance: number = 1.0; // Initial P
  private readonly stateSize: number = 3; // [price, velocity, acceleration]
  private readonly multiStateEnabled: boolean = true; // Enable multi-state tracking

  constructor(config: PriceFeedServiceConfig) {
    super();
    this.config = config;
    this.logger = Logger.getInstance().child({ service: 'price-feed' });
    this.metrics = new MetricsService({
      namespace: 'neo_service_layer',
      subsystem: 'price_feed_service'
    });
    this.dataSources = [];

    // Initialize data sources with configurations from vault
    this.initializeDataSources().catch(error => {
      this.logger.error('Failed to initialize data sources', { error });
    });
  }

  private async initializeDataSources(): Promise<void> {
    try {
      // Fetch data source configs from vault
      const dataSourcesConfig = await this.config.vault.getSecret('price_feed_sources');
      this.dataSources = dataSourcesConfig ? JSON.parse(dataSourcesConfig.value) : [];

      // Validate and normalize weights
      let totalWeight = this.dataSources.reduce((sum, source) => sum + source.weight, 0);
      this.dataSources = this.dataSources.map(source => ({
        ...source,
        weight: source.weight / totalWeight
      }));

      this.logger.info('Initialized price feed data sources', {
        sourceCount: this.dataSources.length
      });
    } catch (error) {
      this.logger.error('Failed to initialize data sources', { error });
      throw error;
    }
  }

  // #region Price History Management

  /**
   * Updates the price history for a symbol
   * Maintains a rolling window of historical prices for volatility calculation
   * 
   * @param symbol - Trading pair symbol
   * @param price - Current price value
   */
  private updatePriceHistory(symbol: string, price: number): void {
    const now = Date.now();
    const history = this.priceHistory.get(symbol) || [];
    
    history.push({ price, timestamp: now });
    
    // Remove entries outside the history window
    const cutoff = now - this.historyWindow;
    const updatedHistory = history.filter(entry => entry.timestamp > cutoff);
    
    this.priceHistory.set(symbol, updatedHistory);
  }

  /**
   * Calculates price volatility using historical data
   * Uses standard deviation of returns over the history window
   * 
   * @param symbol - Trading pair symbol
   * @returns Volatility as a decimal (e.g., 0.05 = 5% volatility)
   */
  private calculateVolatility(symbol: string): number {
    const history = this.priceHistory.get(symbol) || [];
    if (history.length < 2) return 0;

    // Calculate returns
    const returns: number[] = [];
    for (let i = 1; i < history.length; i++) {
      const returnValue = (history[i].price - history[i-1].price) / history[i-1].price;
      returns.push(returnValue);
    }

    // Calculate volatility as standard deviation of returns
    const mean = returns.reduce((sum, val) => sum + val, 0) / returns.length;
    const variance = returns.reduce((sum, val) => sum + Math.pow(val - mean, 2), 0) / returns.length;
    
    return Math.sqrt(variance);
  }

  // #endregion Price History Management

  // #region Statistical Analysis

  /**
   * Calculates comprehensive statistical metrics for a set of prices
   * Includes mean, median, standard deviation, volatility, and outlier detection
   * 
   * @param prices - Array of price data from different sources
   * @returns Statistical metrics and outlier information
   * @throws Error if no valid prices are available
   */
  private calculateStats(prices: PriceSourceData[]): PriceStats {
    const validPrices = prices.filter(p => p.price > 0).map(p => p.price);
    
    if (validPrices.length === 0) {
      throw new Error('No valid prices for statistics calculation');
    }

    // Calculate core statistics
    const mean = validPrices.reduce((sum, price) => sum + price, 0) / validPrices.length;
    const sortedPrices = [...validPrices].sort((a, b) => a - b);
    const median = sortedPrices.length % 2 === 0
      ? (sortedPrices[sortedPrices.length / 2 - 1] + sortedPrices[sortedPrices.length / 2]) / 2
      : sortedPrices[Math.floor(sortedPrices.length / 2)];

    // Calculate standard deviation
    const variance = validPrices.reduce((sum, price) => sum + Math.pow(price - mean, 2), 0) / validPrices.length;
    const stdDev = Math.sqrt(variance);

    // Identify statistical outliers
    const outliers = prices
      .filter(p => Math.abs(p.price - mean) > this.outlierThreshold * stdDev)
      .map(p => ({
        source: p.source.name,
        price: p.price,
        deviation: (p.price - mean) / stdDev
      }));

    // Calculate price volatility
    const volatility = this.calculateVolatility(prices[0]?.source.name || '');

    return {
      mean,
      median,
      stdDev,
      volatility,
      outliers
    };
  }

  // #endregion Statistical Analysis

  // #region Weight Adjustment

  /**
   * Adjusts source weights based on multiple factors:
   * - Statistical outlier status
   * - Historical accuracy
   * - Data freshness
   * - Kalman filter confidence
   * 
   * @param prices - Array of price data from different sources
   * @param stats - Current statistical metrics
   * @returns Price data with adjusted weights
   */
  private adjustWeights(
    prices: PriceSourceData[],
    stats: PriceStats
  ): PriceSourceData[] {
    return prices.map(priceData => {
      const { source, price } = priceData;
      let adjustedWeight = source.weight;

      // Reduce weight for statistical outliers
      const isOutlier = stats.outliers.some(o => o.source === source.name);
      if (isOutlier) {
        adjustedWeight *= 0.5;
      }

      // Adjust based on historical accuracy
      const historicalAccuracy = this.calculateHistoricalAccuracy(
        source.name, 
        prices[0]?.source.name || 'unknown'
      );
      adjustedWeight *= historicalAccuracy;

      // Adjust based on data freshness
      const timestampAge = Date.now() - new Date(priceData.timestamp).getTime();
      const freshnessScore = Math.max(0, 1 - (timestampAge / (5 * 60 * 1000))); // 5 minutes max age
      adjustedWeight *= freshnessScore;

      return {
        ...priceData,
        source: {
          ...source,
          weight: adjustedWeight
        }
      };
    });
  }

  // #endregion Weight Adjustment

  private async loadHistoricalAccuracy(): Promise<void> {
    try {
      const accuracyData = await this.config.vault.getSecret('historical_accuracy');
      if (accuracyData) {
        this.historicalAccuracy = new Map(JSON.parse(accuracyData.value));
      }
    } catch (error) {
      this.logger.error('Failed to load historical accuracy data:', error);
    }
  }

  private async saveHistoricalAccuracy(): Promise<void> {
    try {
      const accuracyData = {
        id: 'historical_accuracy',
        name: 'Historical Accuracy Data',
        value: JSON.stringify(Array.from(this.historicalAccuracy.entries())),
        neoAddress: 'system',
        createdAt: new Date().toISOString(),
        lastAccessed: new Date().toISOString(),
        accessCount: 0,
        permissions: {
          functionIds: ['price_feed_service'],
          roles: ['system']
        },
        teeConfig: {
          encryptionKeyId: 'default',
          attestationToken: 'system',
          mrEnclave: 'system'
        }
      };
      await this.config.vault.updateSecret(accuracyData);
    } catch (error) {
      this.logger.error('Failed to save historical accuracy data:', error);
    }
  }

  private getSourceAccuracy(symbol: string, sourceName: string): SourceAccuracyData {
    const symbolAccuracy = this.historicalAccuracy.get(symbol) || {};
    return symbolAccuracy[sourceName] || {
      meanError: 0,
      lastUpdate: 0,
      updateCount: 0,
      successRate: 1,
      volatilityScore: 0
    };
  }

  private updateSourceAccuracy(
    symbol: string,
    sourceName: string,
    currentPrice: number,
    aggregatedPrice: number,
    success: boolean
  ): void {
    const now = Date.now();
    const symbolAccuracy = this.historicalAccuracy.get(symbol) || {};
    const currentAccuracy = this.getSourceAccuracy(symbol, sourceName);
    
    // Calculate relative error
    const error = Math.abs(currentPrice - aggregatedPrice) / aggregatedPrice;
    
    // Update mean error with exponential decay
    const newMeanError = currentAccuracy.updateCount === 0 
      ? error 
      : currentAccuracy.meanError * this.accuracyDecayFactor + error * (1 - this.accuracyDecayFactor);

    // Update success rate
    const successWeight = 1 / Math.max(1, currentAccuracy.updateCount);
    const newSuccessRate = success
      ? currentAccuracy.successRate * (1 - successWeight) + successWeight
      : currentAccuracy.successRate * (1 - successWeight);

    // Calculate volatility score based on error variance
    const volatilityDiff = Math.pow(error - currentAccuracy.meanError, 2);
    const newVolatilityScore = currentAccuracy.volatilityScore * this.accuracyDecayFactor +
      volatilityDiff * (1 - this.accuracyDecayFactor);

    symbolAccuracy[sourceName] = {
      meanError: newMeanError,
      lastUpdate: now,
      updateCount: currentAccuracy.updateCount + 1,
      successRate: newSuccessRate,
      volatilityScore: newVolatilityScore
    };

    this.historicalAccuracy.set(symbol, symbolAccuracy);

    // Record metrics
    this.metrics.recordGauge('source_accuracy_error', newMeanError, {
      symbol,
      source: sourceName
    });
    this.metrics.recordGauge('source_accuracy_success_rate', newSuccessRate, {
      symbol,
      source: sourceName
    });
    this.metrics.recordGauge('source_accuracy_volatility', newVolatilityScore, {
      symbol,
      source: sourceName
    });

    // Save accuracy data periodically
    if (now - currentAccuracy.lastUpdate > this.accuracyUpdateInterval) {
      this.saveHistoricalAccuracy().catch(error => {
        this.logger.error('Failed to save accuracy data', { error });
      });
    }
  }

  private calculateHistoricalAccuracy(sourceName: string, symbol: string): number {
    const accuracy = this.getSourceAccuracy(symbol, sourceName);
    
    // Return default weight if not enough data
    if (accuracy.updateCount < this.minAccuracyUpdates) {
      return 1.0;
    }

    // Calculate accuracy score components
    const errorScore = Math.max(0, 1 - accuracy.meanError);
    const successScore = accuracy.successRate;
    const volatilityScore = Math.max(0, 1 - accuracy.volatilityScore);

    // Weighted combination of components
    const weights = {
      error: 0.4,
      success: 0.4,
      volatility: 0.2
    };

    const accuracyScore = 
      errorScore * weights.error +
      successScore * weights.success +
      volatilityScore * weights.volatility;

    return Math.max(0.1, Math.min(1, accuracyScore));
  }

  // #region Kalman Filter Implementation

  /**
   * Initializes a multi-dimensional Kalman filter state
   * Tracks price, velocity, and acceleration
   * 
   * @param price - Initial price value
   * @returns Initialized multi-state Kalman filter
   */
  private initializeMultiState(price: number): MultiStateKalman {
    return {
      state: [price, 0, 0], // Initial state: price, zero velocity and acceleration
      covariance: [
        [this.initialErrorCovariance, 0, 0],
        [0, 0.1, 0], // Initial velocity uncertainty
        [0, 0, 0.01] // Initial acceleration uncertainty
      ],
      lastUpdate: Date.now(),
      noiseParams: {
        measurementNoise: this.baseMeasurementNoise,
        processNoise: this.baseProcessNoise,
        adaptationRate: this.adaptationRate,
        lastInnovation: 0,
        innovationVariance: this.initialErrorCovariance,
        updateCount: 0
      }
    };
  }

  /**
   * Initializes a Kalman filter for a specific symbol and source
   * Creates both single-state and multi-state filters if enabled
   * 
   * @param symbol - Trading pair symbol
   * @param sourceName - Data source identifier
   * @param initialPrice - Initial price value
   * @returns Initialized Kalman filter state
   */
  private initializeKalmanFilter(symbol: string, sourceName: string, initialPrice: number): KalmanState {
    const sourceFilters = this.kalmanFilters.get(symbol) || {};
    
    if (!sourceFilters[sourceName]) {
      sourceFilters[sourceName] = {
        estimate: initialPrice,
        errorCovariance: this.initialErrorCovariance,
        lastUpdate: Date.now(),
        noiseParams: {
          measurementNoise: this.baseMeasurementNoise,
          processNoise: this.baseProcessNoise,
          adaptationRate: this.adaptationRate,
          lastInnovation: 0,
          innovationVariance: this.initialErrorCovariance,
          updateCount: 0
        },
        multiState: this.multiStateEnabled ? this.initializeMultiState(initialPrice) : undefined
      };
      this.kalmanFilters.set(symbol, sourceFilters);
    }
    
    return sourceFilters[sourceName];
  }

  // #region Matrix Operations

  /**
   * Multiplies two matrices
   * @param a - First matrix
   * @param b - Second matrix
   * @returns Result of matrix multiplication
   */
  private matrixMultiply(a: number[][], b: number[][]): number[][] {
    const result: number[][] = Array(a.length).fill(0).map(() => Array(b[0].length).fill(0));
    
    for (let i = 0; i < a.length; i++) {
      for (let j = 0; j < b[0].length; j++) {
        for (let k = 0; k < b.length; k++) {
          result[i][j] += a[i][k] * b[k][j];
        }
      }
    }
    
    return result;
  }

  /**
   * Adds two matrices element-wise
   * @param a - First matrix
   * @param b - Second matrix
   * @returns Result of matrix addition
   */
  private matrixAdd(a: number[][], b: number[][]): number[][] {
    return a.map((row, i) => row.map((val, j) => val + b[i][j]));
  }

  /**
   * Computes the transpose of a matrix
   * @param matrix - Input matrix
   * @returns Transposed matrix
   */
  private matrixTranspose(matrix: number[][]): number[][] {
    return matrix[0].map((_, i) => matrix.map(row => row[i]));
  }

  // #endregion Matrix Operations

  /**
   * Updates the multi-dimensional Kalman filter state
   * Implements a full state-space model for price, velocity, and acceleration
   * 
   * @param multiState - Current multi-state Kalman filter
   * @param measurement - New price measurement
   * @param dt - Time delta since last update (seconds)
   */
  private updateMultiState(
    multiState: MultiStateKalman,
    measurement: number,
    dt: number
  ): void {
    // State transition matrix (constant acceleration model)
    const F = [
      [1, dt, 0.5 * dt * dt], // Position update
      [0, 1, dt],             // Velocity update
      [0, 0, 1]               // Acceleration update
    ];

    // Measurement matrix (we only measure position/price)
    const H = [1, 0, 0];

    // Process noise matrix (continuous-time white noise acceleration model)
    const Q = [
      [multiState.noiseParams.processNoise * dt * dt * dt * dt / 4, multiState.noiseParams.processNoise * dt * dt * dt / 2, multiState.noiseParams.processNoise * dt * dt / 2],
      [multiState.noiseParams.processNoise * dt * dt * dt / 2, multiState.noiseParams.processNoise * dt * dt, multiState.noiseParams.processNoise * dt],
      [multiState.noiseParams.processNoise * dt * dt / 2, multiState.noiseParams.processNoise * dt, multiState.noiseParams.processNoise]
    ];

    // Prediction step
    const stateColumn = multiState.state.map(s => [s]);
    const predictedStateMatrix = this.matrixMultiply(F, stateColumn);
    const predictedState = predictedStateMatrix.map(row => row[0]);

    const predictedCovariance = this.matrixAdd(
      this.matrixMultiply(this.matrixMultiply(F, multiState.covariance), this.matrixTranspose(F)),
      Q
    );

    // Update step
    const innovation = measurement - predictedState[0];
    const S = H.reduce((sum, h, i) => 
      sum + h * predictedCovariance[i].reduce((s, p, j) => s + p * H[j], 0), 0
    ) + multiState.noiseParams.measurementNoise;

    const K = predictedCovariance.map(row => 
      [row.reduce((sum, p, j) => sum + p * H[j], 0) / S]
    );

    // State and covariance update
    multiState.state = predictedState.map((s, i) => s + K[i][0] * innovation);
    const IKH = multiState.covariance.map((row, i) => 
      row.map((_, j) => (i === j ? 1 : 0) - K[i][0] * H[j])
    );
    multiState.covariance = this.matrixMultiply(IKH, predictedCovariance);

    // Update noise parameters and record metrics
    this.updateNoiseParameters(
      { ...multiState, estimate: multiState.state[0], errorCovariance: multiState.covariance[0][0] },
      innovation,
      predictedCovariance[0][0],
      dt
    );

    this.recordKalmanMetrics(multiState);
  }

  /**
   * Records metrics for Kalman filter performance monitoring
   * @param multiState - Current multi-state Kalman filter
   */
  private recordKalmanMetrics(multiState: MultiStateKalman): void {
    this.metrics.recordGauge('kalman_velocity', multiState.state[1], {
      symbol: 'price_velocity'
    });
    this.metrics.recordGauge('kalman_acceleration', multiState.state[2], {
      symbol: 'price_acceleration'
    });
    this.metrics.recordGauge('kalman_velocity_uncertainty', multiState.covariance[1][1], {
      symbol: 'velocity_uncertainty'
    });
    this.metrics.recordGauge('kalman_acceleration_uncertainty', multiState.covariance[2][2], {
      symbol: 'acceleration_uncertainty'
    });
  }

  // #endregion Kalman Filter Implementation

  // #region Adaptive Noise Estimation

  /**
   * Updates Kalman filter noise parameters based on measurement innovations
   * Implements adaptive noise estimation for improved tracking performance
   * 
   * @param filter - Current Kalman filter state
   * @param innovation - Latest measurement innovation
   * @param predictedErrorCovariance - Predicted error covariance
   * @param timeScale - Time scaling factor for noise adaptation
   */
  private updateNoiseParameters(
    filter: KalmanState,
    innovation: number,
    predictedErrorCovariance: number,
    timeScale: number
  ): void {
    const { noiseParams } = filter;
    noiseParams.updateCount++;

    // Update innovation statistics using exponential moving average
    const innovationSquared = innovation * innovation;
    noiseParams.innovationVariance = 
      noiseParams.innovationVariance * (1 - noiseParams.adaptationRate) +
      innovationSquared * noiseParams.adaptationRate;

    // Estimate measurement noise based on innovation statistics
    const estimatedMeasurementNoise = Math.max(
      this.baseMeasurementNoise * this.minNoiseRatio,
      Math.min(
        noiseParams.innovationVariance,
        this.baseMeasurementNoise * this.maxNoiseRatio
      )
    );

    // Estimate process noise based on prediction error
    const predictionError = Math.abs(innovation) - Math.sqrt(predictedErrorCovariance);
    const estimatedProcessNoise = Math.max(
      this.baseProcessNoise * this.minNoiseRatio,
      Math.min(
        Math.abs(predictionError) * timeScale,
        this.baseProcessNoise * this.maxNoiseRatio
      )
    );

    // Smooth adaptation of noise parameters
    noiseParams.measurementNoise = 
      noiseParams.measurementNoise * (1 - noiseParams.adaptationRate) +
      estimatedMeasurementNoise * noiseParams.adaptationRate;

    noiseParams.processNoise = 
      noiseParams.processNoise * (1 - noiseParams.adaptationRate) +
      estimatedProcessNoise * noiseParams.adaptationRate;

    noiseParams.lastInnovation = innovation;

    // Record noise adaptation metrics
    this.recordNoiseMetrics(filter, estimatedMeasurementNoise, estimatedProcessNoise);
  }

  /**
   * Records metrics for noise parameter adaptation
   * @param filter - Current Kalman filter state
   * @param measNoise - Estimated measurement noise
   * @param procNoise - Estimated process noise
   */
  private recordNoiseMetrics(
    filter: KalmanState,
    measNoise: number,
    procNoise: number
  ): void {
    const labels = {
      update_count: filter.noiseParams.updateCount.toString()
    };

    this.metrics.recordGauge('kalman_measurement_noise', measNoise, labels);
    this.metrics.recordGauge('kalman_process_noise', procNoise, labels);
    this.metrics.recordGauge('kalman_innovation_variance', 
      filter.noiseParams.innovationVariance, labels);
  }

  // #endregion Adaptive Noise Estimation

  // #region Price Updates

  /**
   * Updates the price on the blockchain if confidence threshold is met
   * Implements rate limiting and confidence-based updates
   * 
   * @param symbol - Trading pair symbol
   * @throws Error if price update fails
   */
  private async updatePriceOnChain(symbol: string): Promise<void> {
    const timer = this.metrics.startTimer('price_update_chain_duration_seconds', {
      symbol
    });

    try {
      // Get latest aggregated price with confidence metrics
      const priceData = await this.getAggregatedPrice(symbol);
      const { price, confidence, details } = priceData;

      // Only update if confidence meets threshold
      if (confidence >= 0.8) {
        // Update price on blockchain
        await this.config.contractService.updatePrice(symbol, price, {
          confidence,
          timestamp: new Date().toISOString(),
          sources: details ? details.sources.length : 0
        });

        this.lastUpdate.set(symbol, Date.now());

        this.logger.info('Updated price on chain', {
          symbol,
          price,
          confidence,
          sourceCount: details?.sources.length
        });
      } else {
        this.logger.warn('Skipped price update due to low confidence', {
          symbol,
          confidence,
          threshold: 0.8
        });
      }
    } catch (error) {
      this.metrics.incrementCounter('price_update_chain_errors_total', {
        symbol
      });
      
      this.logger.error('Failed to update price on chain', {
        symbol,
        error
      });
      
      throw error;
    } finally {
      timer.end();
    }
  }

  /**
   * Retrieves the current aggregated price for a symbol
   * Implements caching, updates, and comprehensive error handling
   * 
   * @param symbol - Trading pair symbol
   * @param preferredSource - Optional preferred data source
   * @returns Aggregated price data with confidence metrics
   * @throws Error if no valid price data is available
   */
  public async getAggregatedPrice(symbol: string, preferredSource?: string): Promise<PriceData> {
    const timer = this.metrics.startTimer('get_aggregated_price_duration_seconds', {
      symbol
    });

    try {
      // Check if update is needed
      const lastUpdateTime = this.lastUpdate.get(symbol) || 0;
      const now = Date.now();

      if (now - lastUpdateTime > this.updateInterval) {
        await this.updatePriceOnChain(symbol);
      }

      // Collect and aggregate prices
      const prices = await this.collectPricesFromSources(symbol, preferredSource);

      if (prices.length === 0) {
        throw new Error('No valid price data available');
      }

      // Calculate aggregated price with enhanced algorithm
      const { price, confidence, details, stats } = this.calculateAggregatedPrice(prices);

      // Log statistical insights
      this.logPriceStatistics(symbol, stats);

      return {
        symbol,
        price,
        confidence,
        timestamp: new Date().toISOString(),
        source: preferredSource || 'aggregated',
        details: {
          ...details,
          volume24h: undefined,
          marketCap: undefined
        },
        stats
      };
    } finally {
      timer.end();
    }
  }

  /**
   * Logs statistical insights about price aggregation
   * @param symbol - Trading pair symbol
   * @param stats - Price statistics
   */
  private logPriceStatistics(symbol: string, stats: PriceStats): void {
    this.logger.info('Price aggregation statistics', {
      symbol,
      median: stats.median,
      stdDev: stats.stdDev,
      outlierCount: stats.outliers.length,
      volatility: stats.volatility
    });

    if (stats.outliers.length > 0) {
      this.logger.warn('Detected price outliers', {
        symbol,
        outliers: stats.outliers
      });
    }
  }

  // #endregion Price Updates

  private calculateAggregatedPrice(
    prices: PriceSourceData[]
  ): AggregationResult {
    if (prices.length < this.minSourcesRequired) {
      throw new Error(`Insufficient price sources. Required: ${this.minSourcesRequired}, Got: ${prices.length}`);
    }

    // Calculate initial statistics
    const stats = this.calculateStats(prices);

    // Record volatility metrics
    this.metrics.recordGauge('price_volatility', stats.volatility, {
      symbol: prices[0]?.source.name || 'unknown'
    });

    // Adjust weights based on statistics and other factors
    const adjustedPrices = this.adjustWeights(prices, stats);

    // Calculate weighted average with adjusted weights
    let weightedSum = 0;
    let weightSum = 0;
    const validSources: PriceData['details']['sources'] = [];

    adjustedPrices.forEach(({ source, price, timestamp }) => {
      if (price > 0 && !stats.outliers.some(o => o.source === source.name)) {
        // Get Kalman filter error covariance for confidence calculation
        const sourceFilters = this.kalmanFilters.get(prices[0]?.source.name || 'unknown') || {};
        const filterState = sourceFilters[source.name];
        const filterConfidence = filterState 
          ? Math.max(0, 1 - filterState.errorCovariance)
          : 0.5;

        // Adjust weight based on Kalman filter confidence
        const adjustedWeight = source.weight * (1 + filterConfidence);
        
        weightedSum += price * adjustedWeight;
        weightSum += adjustedWeight;
        validSources.push({
          name: source.name,
          price,
          weight: adjustedWeight,
          timestamp
        });
      }
    });

    if (weightSum === 0) {
      throw new Error('No valid prices for aggregation after adjustments');
    }

    const price = weightedSum / weightSum;

    // Update price history
    this.updatePriceHistory(prices[0]?.source.name || 'unknown', price);

    // Calculate confidence based on multiple factors
    const confidenceFactors = {
      sourceAgreement: 1 - (stats.stdDev / stats.mean),
      volatility: Math.max(0, 1 - (stats.volatility / this.volatilityThreshold)),
      sourceCount: Math.min(1, validSources.length / this.minSourcesRequired),
      outlierImpact: 1 - (stats.outliers.length / prices.length),
      kalmanConfidence: this.calculateAverageKalmanConfidence(prices),
      innovationConfidence: this.calculateInnovationConfidence(prices)
    };

    // Weighted confidence calculation with Kalman confidence
    const confidence = Object.values(confidenceFactors).reduce((sum, factor) => sum + factor, 0) / 
      Object.keys(confidenceFactors).length;

    // Record detailed metrics
    this.recordAggregationMetrics(price, stats, confidenceFactors);

    return {
      price,
      confidence: Math.max(0, Math.min(1, confidence)),
      details: {
        lastUpdate: new Date().toISOString(),
        sources: validSources
      },
      stats
    };
  }

  private calculateAverageKalmanConfidence(prices: PriceSourceData[]): number {
    const symbol = prices[0]?.source.name || 'unknown';
    const sourceFilters = this.kalmanFilters.get(symbol) || {};
    
    let totalConfidence = 0;
    let count = 0;

    prices.forEach(({ source }) => {
      const filterState = sourceFilters[source.name];
      if (filterState) {
        totalConfidence += Math.max(0, 1 - filterState.errorCovariance);
        count++;
      }
    });

    return count > 0 ? totalConfidence / count : 0.5;
  }

  private calculateInnovationConfidence(prices: PriceSourceData[]): number {
    const symbol = prices[0]?.source.name || 'unknown';
    const sourceFilters = this.kalmanFilters.get(symbol) || {};
    
    let totalConfidence = 0;
    let count = 0;

    prices.forEach(({ source }) => {
      const filterState = sourceFilters[source.name];
      if (filterState) {
        // Calculate confidence based on innovation variance
        const normalizedVariance = filterState.noiseParams.innovationVariance / 
          (this.baseMeasurementNoise * this.maxNoiseRatio);
        const innovationConfidence = Math.max(0, 1 - normalizedVariance);
        
        totalConfidence += innovationConfidence;
        count++;
      }
    });

    return count > 0 ? totalConfidence / count : 0.5;
  }

  private recordAggregationMetrics(
    price: number,
    stats: PriceStats,
    confidenceFactors: Record<string, number>
  ): void {
    const labels = { price };

    this.metrics.recordGauge('price_standard_deviation', stats.stdDev, labels);
    this.metrics.recordGauge('price_median', stats.median, labels);
    this.metrics.recordGauge('price_outlier_count', stats.outliers.length, labels);

    Object.entries(confidenceFactors).forEach(([factor, value]) => {
      this.metrics.recordGauge(`confidence_${factor}`, value, labels);
    });
  }

  private async collectPricesFromSources(
    symbol: string,
    preferredSource?: string
  ): Promise<PriceSourceData[]> {
    const prices: PriceSourceData[] = [];
    const startTime = Date.now();

    // If preferred source is specified, try it first
    if (preferredSource) {
      const source = this.dataSources.find(s => s.name === preferredSource);
      if (source) {
        try {
          const price = await this.fetchPriceFromSource(symbol, source);
          if (price) {
            // Apply Kalman filter to the price
            const timestamp = new Date(price.timestamp).getTime();
            const filteredPrice = this.updateKalmanFilter(symbol, source.name, price.price, timestamp);
            
            prices.push({
              ...price,
              price: filteredPrice
            });
            
            this.updateSourceAccuracy(symbol, source.name, filteredPrice, filteredPrice, true);
          } else {
            this.updateSourceAccuracy(symbol, source.name, 0, 0, false);
          }
        } catch (error) {
          this.updateSourceAccuracy(symbol, source.name, 0, 0, false);
          this.logger.warn('Failed to fetch price from preferred source', {
            source: preferredSource,
            error
          });
        }
      }
    }

    // Fetch from other sources in parallel
    const otherSources = this.dataSources.filter(s => s.name !== preferredSource);
    const results = await Promise.allSettled(
      otherSources.map(source => this.fetchPriceFromSource(symbol, source))
    );

    // Process results and update accuracy
    results.forEach((result, index) => {
      const source = otherSources[index];
      if (result.status === 'fulfilled' && result.value) {
        // Apply Kalman filter to each price
        const timestamp = new Date(result.value.timestamp).getTime();
        const filteredPrice = this.updateKalmanFilter(symbol, source.name, result.value.price, timestamp);
        
        prices.push({
          ...result.value,
          price: filteredPrice
        });
        
        this.updateSourceAccuracy(symbol, source.name, filteredPrice, filteredPrice, true);
      } else {
        this.updateSourceAccuracy(symbol, source.name, 0, 0, false);
        if (result.status === 'rejected') {
          this.metrics.incrementCounter('price_fetch_errors_total', {
            source: source.name,
            symbol
          });
        }
      }
    });

    // Record latency metrics
    const latency = Date.now() - startTime;
    this.metrics.recordGauge('price_collection_latency_ms', latency, { symbol });

    return prices;
  }

  private async fetchPriceFromSource(
    symbol: string,
    source: DataSourceConfig
  ): Promise<PriceSourceData | null> {
    try {
      const apiKeySecret = await this.config.vault.getSecret(source.apiKeySecret);
      if (!apiKeySecret) {
        throw new Error(`API key not found for source: ${source.name}`);
      }

      // Make API request with the decrypted API key
      // ... rest of the code ...
    } catch (error) {
      this.logger.error('Failed to fetch price from source', {
        source: source.name,
        symbol,
        error
      });
      return null;
    }

    return null;
  }

  private updateKalmanFilter(
    symbol: string,
    sourceName: string,
    measurement: number,
    timestamp: number
  ): number {
    const sourceFilters = this.kalmanFilters.get(symbol) || {};
    let filter = sourceFilters[sourceName];

    if (!filter) {
      filter = this.initializeKalmanFilter(symbol, sourceName, measurement);
    }

    // Calculate time delta in seconds
    const dt = (timestamp - filter.lastUpdate) / 1000;
    const timeScale = Math.min(1, dt / 60); // Scale process noise with time, max 1 minute

    if (filter.multiState && this.multiStateEnabled) {
      // Update multi-dimensional state
      this.updateMultiState(filter.multiState, measurement, dt);
      filter.estimate = filter.multiState.state[0];
      filter.errorCovariance = filter.multiState.covariance[0][0];
    } else {
      // Original single-state Kalman filter update
      const predictedErrorCovariance = filter.errorCovariance + 
        (filter.noiseParams.processNoise * timeScale);

      const innovation = measurement - filter.estimate;

      this.updateNoiseParameters(filter, innovation, predictedErrorCovariance, timeScale);

      const kalmanGain = predictedErrorCovariance / 
        (predictedErrorCovariance + filter.noiseParams.measurementNoise);
      filter.estimate = filter.estimate + kalmanGain * innovation;
      filter.errorCovariance = (1 - kalmanGain) * predictedErrorCovariance;
    }

    filter.lastUpdate = timestamp;

    // Record metrics
    this.metrics.recordGauge('kalman_filter_error_covariance', filter.errorCovariance, {
      symbol,
      source: sourceName
    });

    if (filter.multiState) {
      this.metrics.recordGauge('kalman_state_velocity', filter.multiState.state[1], {
        symbol,
        source: sourceName
      });
      this.metrics.recordGauge('kalman_state_acceleration', filter.multiState.state[2], {
        symbol,
        source: sourceName
      });
      this.metrics.recordGauge('kalman_velocity_uncertainty', filter.multiState.covariance[1][1], {
        symbol,
        source: sourceName
      });
      this.metrics.recordGauge('kalman_acceleration_uncertainty', filter.multiState.covariance[2][2], {
        symbol,
        source: sourceName
      });
    }

    return filter.estimate;
  }
}