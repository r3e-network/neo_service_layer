/**
 * Price Feed Service
 * 
 * Service for accessing oracle services for price data.
 */

import { NeoServiceLayer } from '../../core/client';
import { NotFoundError, ValidationError } from '../../core/errors';
import { PriceFeed } from '../../types/models';
import { PriceFeedUpdateRequest } from '../../types/requests';
import { PaginatedResponse } from '../../types/responses';

/**
 * Price Feed Service for Neo Service Layer
 */
export class PriceFeedService {
  private client: NeoServiceLayer;
  private basePath: string = '/api/v1/pricefeed';

  /**
   * Create a new Price Feed service instance
   * @param client Neo Service Layer client
   */
  constructor(client: NeoServiceLayer) {
    this.client = client;
  }

  /**
   * Get price for a symbol
   * @param symbol Asset symbol (e.g., 'NEO', 'GAS')
   * @returns Price feed
   */
  public async getPrice(symbol: string): Promise<PriceFeed> {
    try {
      if (!symbol) {
        throw new ValidationError('Symbol is required', 'symbol');
      }

      return await this.client.request<PriceFeed>(
        'GET',
        `${this.basePath}/price/${symbol.toUpperCase()}`
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('PriceFeed', symbol);
      }
      throw error;
    }
  }

  /**
   * Get prices for multiple symbols
   * @param symbols Asset symbols (e.g., ['NEO', 'GAS'])
   * @returns Map of symbols to price feeds
   */
  public async getPrices(symbols: string[]): Promise<Record<string, PriceFeed>> {
    try {
      if (!symbols || symbols.length === 0) {
        throw new ValidationError('At least one symbol is required', 'symbols');
      }

      const symbolsParam = symbols.map(s => s.toUpperCase()).join(',');
      return await this.client.request<Record<string, PriceFeed>>(
        'GET',
        `${this.basePath}/prices?symbols=${symbolsParam}`
      );
    } catch (error) {
      throw error;
    }
  }

  /**
   * List all price feeds
   * @param page Page number
   * @param pageSize Page size
   * @returns Paginated list of price feeds
   */
  public async listPriceFeeds(page: number = 1, pageSize: number = 20): Promise<PaginatedResponse<PriceFeed>> {
    try {
      const params = { page, pageSize };
      return await this.client.request<PaginatedResponse<PriceFeed>>(
        'GET',
        `${this.basePath}/list`,
        undefined,
        { params }
      );
    } catch (error) {
      throw error;
    }
  }

  /**
   * Get price history for a symbol
   * @param symbol Asset symbol
   * @param interval Time interval (e.g., '1h', '1d', '7d', '30d')
   * @param limit Number of data points
   * @returns Price history
   */
  public async getPriceHistory(
    symbol: string,
    interval: string = '1d',
    limit: number = 30
  ): Promise<Array<{ timestamp: string; price: number }>> {
    try {
      if (!symbol) {
        throw new ValidationError('Symbol is required', 'symbol');
      }

      const params = { interval, limit };
      return await this.client.request<Array<{ timestamp: string; price: number }>>(
        'GET',
        `${this.basePath}/history/${symbol.toUpperCase()}`,
        undefined,
        { params }
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('PriceFeed', symbol);
      }
      throw error;
    }
  }

  /**
   * Get price sources
   * @returns List of available price sources
   */
  public async getSources(): Promise<string[]> {
    try {
      const response = await this.client.request<{ sources: string[] }>(
        'GET',
        `${this.basePath}/sources`
      );
      return response.sources;
    } catch (error) {
      throw error;
    }
  }

  /**
   * Subscribe to price updates
   * @param symbol Asset symbol
   * @param callback Callback function for price updates
   * @returns Subscription ID
   */
  public async subscribe(
    symbol: string,
    callback: (priceFeed: PriceFeed) => void
  ): Promise<string> {
    try {
      if (!symbol) {
        throw new ValidationError('Symbol is required', 'symbol');
      }
      if (!callback) {
        throw new ValidationError('Callback is required', 'callback');
      }

      // This is a placeholder for WebSocket subscription
      // In a real implementation, this would establish a WebSocket connection
      // and register the callback for price updates
      const subscriptionId = `sub-${Date.now()}-${Math.random().toString(36).substring(2, 15)}`;
      
      // For now, we'll just return a subscription ID
      return subscriptionId;
    } catch (error) {
      throw error;
    }
  }

  /**
   * Unsubscribe from price updates
   * @param subscriptionId Subscription ID
   */
  public async unsubscribe(subscriptionId: string): Promise<void> {
    try {
      if (!subscriptionId) {
        throw new ValidationError('Subscription ID is required', 'subscriptionId');
      }

      // This is a placeholder for WebSocket unsubscription
      // In a real implementation, this would close the WebSocket connection
      // or unregister the callback for price updates
    } catch (error) {
      throw error;
    }
  }

  /**
   * Update price feed (for oracle providers only)
   * @param request Price feed update request
   * @returns Updated price feed
   */
  public async updatePrice(request: PriceFeedUpdateRequest): Promise<PriceFeed> {
    try {
      if (!request.symbol) {
        throw new ValidationError('Symbol is required', 'symbol');
      }
      if (request.price === undefined || request.price < 0) {
        throw new ValidationError('Valid price is required', 'price');
      }
      if (!request.source) {
        throw new ValidationError('Source is required', 'source');
      }

      return await this.client.request<PriceFeed>(
        'POST',
        `${this.basePath}/update`,
        request
      );
    } catch (error) {
      throw error;
    }
  }
}
