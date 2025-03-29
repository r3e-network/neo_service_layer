/**
 * Neo Service Layer Client
 * 
 * Specialized client for JavaScript functions running within the Neo Function Service
 * to interact with the Neo Service Layer.
 */

import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';
import { ClientConfig } from '../types/config';
import { FunctionsService } from '../services/functions';
import { GasBankService } from '../services/gasbank';
import { PriceFeedService } from '../services/pricefeed';
import { SecretsService } from '../services/secrets';
import { TriggerService } from '../services/trigger';
import { TransactionService } from '../services/transaction';
import { ApiError } from './errors';

/**
 * Main client for the Neo Service Layer, designed specifically for
 * JavaScript functions running within the Neo Function Service.
 */
export class NeoServiceLayer {
  private config: ClientConfig;
  private httpClient: AxiosInstance;
  private functionContext: {
    functionId?: string;
    executionId?: string;
    traceId?: string;
  } = {};

  // Services
  public functions: FunctionsService;
  public gasBank: GasBankService;
  public priceFeed: PriceFeedService;
  public secrets: SecretsService;
  public trigger: TriggerService;
  public transaction: TransactionService;

  /**
   * Create a new Neo Service Layer client
   * @param config Client configuration
   */
  constructor(config: ClientConfig) {
    this.config = {
      baseUrl: config.baseUrl || 'http://localhost:3000',
      timeout: config.timeout || 30000,
      ...config
    };

    // Store function context if provided
    if (config.headers) {
      this.functionContext = {
        functionId: config.headers['X-Function-Id'],
        executionId: config.headers['X-Execution-Id'],
        traceId: config.headers['X-Trace-Id']
      };
    }

    // Initialize HTTP client
    this.httpClient = axios.create({
      baseURL: this.config.baseUrl,
      timeout: this.config.timeout,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        ...config.headers
      }
    });

    // Add response interceptor for error handling
    this.httpClient.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response) {
          throw new ApiError(
            error.response.data.message || 'API Error',
            error.response.status,
            error.response.data
          );
        }
        throw error;
      }
    );

    // Initialize services
    this.functions = new FunctionsService(this);
    this.gasBank = new GasBankService(this);
    this.priceFeed = new PriceFeedService(this);
    this.secrets = new SecretsService(this);
    this.trigger = new TriggerService(this);
    this.transaction = new TransactionService(this);
  }

  /**
   * Get the HTTP client instance
   * @returns Axios instance
   */
  public getHttpClient(): AxiosInstance {
    return this.httpClient;
  }

  /**
   * Get function context information
   * @returns Function context
   */
  public getFunctionContext() {
    return this.functionContext;
  }

  /**
   * Get system health status
   * @returns Health status
   */
  public async getHealth(): Promise<any> {
    const response = await this.httpClient.get('/health');
    return response.data;
  }

  /**
   * Make a raw API request
   * @param method HTTP method
   * @param endpoint API endpoint
   * @param data Request data
   * @param config Axios request config
   * @returns Response data
   */
  public async request<T = any>(
    method: string,
    endpoint: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const requestConfig: AxiosRequestConfig = {
      method,
      url: endpoint,
      ...config
    };

    if (data) {
      if (method.toLowerCase() === 'get') {
        requestConfig.params = data;
      } else {
        requestConfig.data = data;
      }
    }

    try {
      const response = await this.httpClient.request(requestConfig);
      return response.data;
    } catch (error) {
      this.handleError(error);
    }
  }

  /**
   * Make a GET request
   * @param endpoint API endpoint
   * @param config Axios request config
   * @returns Response data
   */
  public async get<T = any>(
    endpoint: string,
    config?: AxiosRequestConfig
  ): Promise<T> {
    return this.request<T>('get', endpoint, undefined, config);
  }

  /**
   * Make a POST request
   * @param endpoint API endpoint
   * @param data Request data
   * @param config Axios request config
   * @returns Response data
   */
  public async post<T = any>(
    endpoint: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<T> {
    return this.request<T>('post', endpoint, data, config);
  }

  /**
   * Make a PUT request
   * @param endpoint API endpoint
   * @param data Request data
   * @param config Axios request config
   * @returns Response data
   */
  public async put<T = any>(
    endpoint: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<T> {
    return this.request<T>('put', endpoint, data, config);
  }

  /**
   * Make a DELETE request
   * @param endpoint API endpoint
   * @param config Axios request config
   * @returns Response data
   */
  public async delete<T = any>(
    endpoint: string,
    config?: AxiosRequestConfig
  ): Promise<T> {
    return this.request<T>('delete', endpoint, undefined, config);
  }

  /**
   * Handle API errors
   * @param error Error object
   * @throws Appropriate error type
   */
  public handleError(error: any): never {
    if (error instanceof ApiError) {
      throw error;
    }
    throw new ApiError(error.message || 'Unknown error', 500);
  }
}
