import { ethers, Contract, JsonRpcProvider, Wallet } from 'ethers';
import { PRICE_FEED_CONSTANTS } from '../../../services/price-feeds/constants';
import {
  PriceFeedService,
  PriceData,
  HistoricalPriceData,
  PriceSource,
  PriceMetrics,
  PriceFeedConfig,
  ValidationError,
  PriceFeedError,
  PriceFeedErrorCode,
  OnChainPriceData,
  PriceUpdateTransaction
} from './types';

export class PriceFeedServiceImpl implements PriceFeedService {
  private readonly provider: JsonRpcProvider;
  private readonly priceFeedContract: Contract;
  private readonly signer: Wallet;

  constructor(
    provider: JsonRpcProvider,
    priceFeedContractAddress: string,
    signer: Wallet
  ) {
    this.provider = provider;
    this.signer = signer;
    this.priceFeedContract = new Contract(
      priceFeedContractAddress,
      ['function getPrice(string) view returns (uint256, uint256)'],
      provider
    );
  }

  async getCurrentPrice(symbol: string): Promise<PriceData> {
    try {
      const [price, timestamp] = await this.priceFeedContract.getPrice(symbol);
      
      return {
        symbol,
        price: parseFloat(ethers.formatUnits(price, PRICE_FEED_CONSTANTS.PRICE_DECIMALS)),
        timestamp: Number(timestamp)
      };
    } catch (error) {
      throw this.handleError(error, PriceFeedErrorCode.BLOCKCHAIN_ERROR);
    }
  }

  async getHistoricalPrices(
    symbol: string,
    timeRange: string
  ): Promise<HistoricalPriceData[]> {
    try {
      // Calculate time range in seconds
      const now = Math.floor(Date.now() / 1000);
      const rangeInSeconds = this.timeRangeToSeconds(timeRange);
      const startTime = now - rangeInSeconds;

      // Fetch historical prices from database or blockchain
      const prices = await this.fetchHistoricalPrices(symbol, startTime, now);
      
      return prices.map(price => ({
        ...price,
        volume: 0, // Add volume data if available
        high: price.price,
        low: price.price
      }));
    } catch (error) {
      throw this.handleError(error, PriceFeedErrorCode.INTERNAL_ERROR);
    }
  }

  async getPriceSources(
    symbol: string
  ): Promise<{ sources: PriceSource[]; stats: any }> {
    try {
      const sources = await this.fetchPriceSources(symbol);
      const stats = this.calculateSourceStats(sources);
      
      return { sources, stats };
    } catch (error) {
      throw this.handleError(error, PriceFeedErrorCode.INTERNAL_ERROR);
    }
  }

  async getPriceMetrics(symbol: string): Promise<PriceMetrics> {
    try {
      const [
        currentPrice,
        historicalPrices,
        sources
      ] = await Promise.all([
        this.getCurrentPrice(symbol),
        this.getHistoricalPrices(symbol, '24h'),
        this.getPriceSources(symbol)
      ]);

      return this.calculateMetrics(
        currentPrice,
        historicalPrices,
        sources.sources
      );
    } catch (error) {
      throw this.handleError(error, PriceFeedErrorCode.INTERNAL_ERROR);
    }
  }

  async getConfig(symbol: string): Promise<PriceFeedConfig> {
    try {
      // Fetch config from database or blockchain
      const config = await this.fetchConfig(symbol);
      return config;
    } catch (error) {
      throw this.handleError(error, PriceFeedErrorCode.INTERNAL_ERROR);
    }
  }

  async updateConfig(
    symbol: string,
    config: Partial<PriceFeedConfig>
  ): Promise<PriceFeedConfig> {
    try {
      const errors = this.validateConfig(config);
      if (errors.length > 0) {
        throw new Error('Config validation failed');
      }

      // Update config in database and blockchain
      const updatedConfig = await this.saveConfig(symbol, config);
      return updatedConfig;
    } catch (error) {
      throw this.handleError(error, PriceFeedErrorCode.CONFIG_VALIDATION_FAILED);
    }
  }

  validateConfig(config: Partial<PriceFeedConfig>): ValidationError[] {
    const errors: ValidationError[] = [];

    if ('heartbeatInterval' in config) {
      const interval = config.heartbeatInterval;
      if (interval !== undefined && (interval < 1000 || interval > 3600000)) {
        errors.push({
          field: 'heartbeatInterval',
          message: 'Heartbeat interval must be between 1 second and 1 hour'
        });
      }
    }

    if ('deviationThreshold' in config) {
      const threshold = config.deviationThreshold;
      if (threshold !== undefined && (threshold < 0.001 || threshold > 1)) {
        errors.push({
          field: 'deviationThreshold',
          message: 'Deviation threshold must be between 0.1% and 100%'
        });
      }
    }

    if ('minSourceCount' in config) {
      const count = config.minSourceCount;
      if (count !== undefined && (count < 1 || count > 10)) {
        errors.push({
          field: 'minSourceCount',
          message: 'Minimum source count must be between 1 and 10'
        });
      }
    }

    return errors;
  }

  async resetConfig(symbol: string): Promise<PriceFeedConfig> {
    try {
      const defaultConfig: PriceFeedConfig = {
        heartbeatInterval: PRICE_FEED_CONSTANTS.HEARTBEAT_INTERVAL,
        deviationThreshold: PRICE_FEED_CONSTANTS.DEFAULT_DEVIATION_THRESHOLD,
        minSourceCount: PRICE_FEED_CONSTANTS.MIN_SOURCE_COUNT,
        isActive: true,
        updateMethod: 'auto'
      };

      return await this.updateConfig(symbol, defaultConfig);
    } catch (error) {
      throw this.handleError(error, PriceFeedErrorCode.INTERNAL_ERROR);
    }
  }

  // Private helper methods
  private timeRangeToSeconds(timeRange: string): number {
    const value = parseInt(timeRange);
    const unit = timeRange.slice(-1);
    
    switch (unit) {
      case 'h':
        return value * 3600;
      case 'd':
        return value * 86400;
      default:
        throw new Error('Invalid time range format');
    }
  }

  private async fetchHistoricalPrices(
    symbol: string,
    startTime: number,
    endTime: number
  ): Promise<PriceData[]> {
    // Implementation to fetch historical prices from database
    return [];
  }

  private async fetchPriceSources(symbol: string): Promise<PriceSource[]> {
    // Implementation to fetch price sources from database
    return [];
  }

  private calculateSourceStats(sources: PriceSource[]): any {
    const activeSources = sources.filter(s => s.status === 'active');
    
    return {
      totalSources: sources.length,
      activeSources: activeSources.length,
      averageDeviation: this.calculateAverageDeviation(sources),
      lastUpdate: Math.max(...sources.map(s => s.lastUpdate)),
      averageLatency: this.calculateAverageLatency(sources),
      reliabilityScore: this.calculateReliabilityScore(sources)
    };
  }

  private calculateAverageDeviation(sources: PriceSource[]): number {
    const deviations = sources.map(s => s.deviation);
    return deviations.reduce((a, b) => a + b, 0) / deviations.length;
  }

  private calculateAverageLatency(sources: PriceSource[]): number {
    const latencies = sources.map(s => s.latency);
    return latencies.reduce((a, b) => a + b, 0) / latencies.length;
  }

  private calculateReliabilityScore(sources: PriceSource[]): number {
    return sources.reduce((acc, source) => acc + source.reliability, 0) / sources.length;
  }

  private calculateMetrics(
    currentPrice: PriceData,
    historicalPrices: HistoricalPriceData[],
    sources: PriceSource[]
  ): PriceMetrics {
    const day = 24 * 60 * 60 * 1000;
    const dayAgo = Date.now() - day;
    const oldPrice = historicalPrices.find(p => p.timestamp < dayAgo)?.price || currentPrice.price;
    
    return {
      volume24h: historicalPrices.reduce((acc, p) => acc + p.volume, 0),
      volumeChange24h: 0, // Calculate if previous day's volume is available
      priceChange24h: currentPrice.price - oldPrice,
      priceChangePercent24h: (currentPrice.price - oldPrice) / oldPrice,
      sourceCountChange24h: 0, // Calculate if previous day's source count is available
      highPrice24h: Math.max(...historicalPrices.map(p => p.high)),
      lowPrice24h: Math.min(...historicalPrices.map(p => p.low)),
      updateCount24h: historicalPrices.length,
      averageLatency: this.calculateAverageLatency(sources),
      volatility24h: this.calculateVolatility(historicalPrices)
    };
  }

  private calculateVolatility(prices: HistoricalPriceData[]): number {
    if (prices.length < 2) return 0;
    
    const returns = prices.slice(1).map((p, i) => {
      const prev = prices[i];
      return (p.price - prev.price) / prev.price;
    });
    
    const mean = returns.reduce((a, b) => a + b, 0) / returns.length;
    const variance = returns.reduce((a, b) => a + Math.pow(b - mean, 2), 0) / returns.length;
    
    return Math.sqrt(variance);
  }

  private async fetchConfig(symbol: string): Promise<PriceFeedConfig> {
    // Implementation to fetch config from database
    return {
      heartbeatInterval: PRICE_FEED_CONSTANTS.HEARTBEAT_INTERVAL,
      deviationThreshold: PRICE_FEED_CONSTANTS.DEFAULT_DEVIATION_THRESHOLD,
      minSourceCount: PRICE_FEED_CONSTANTS.MIN_SOURCE_COUNT,
      isActive: true,
      updateMethod: 'auto'
    };
  }

  private async saveConfig(
    symbol: string,
    config: Partial<PriceFeedConfig>
  ): Promise<PriceFeedConfig> {
    // Implementation to save config to database
    return {
      ...await this.fetchConfig(symbol),
      ...config
    };
  }

  private handleError(error: any, code: PriceFeedErrorCode): PriceFeedError {
    const priceFeedError = new Error(error.message) as PriceFeedError;
    priceFeedError.code = code;
    priceFeedError.details = error;
    priceFeedError.httpStatus = this.getHttpStatus(code);
    return priceFeedError;
  }

  private getHttpStatus(code: PriceFeedErrorCode): number {
    switch (code) {
      case PriceFeedErrorCode.INVALID_SYMBOL:
      case PriceFeedErrorCode.CONFIG_VALIDATION_FAILED:
        return 400;
      case PriceFeedErrorCode.UNAUTHORIZED:
        return 401;
      case PriceFeedErrorCode.INSUFFICIENT_SOURCES:
      case PriceFeedErrorCode.PRICE_DEVIATION_TOO_HIGH:
      case PriceFeedErrorCode.SOURCE_TIMEOUT:
        return 503;
      case PriceFeedErrorCode.BLOCKCHAIN_ERROR:
      case PriceFeedErrorCode.INTERNAL_ERROR:
      default:
        return 500;
    }
  }
}