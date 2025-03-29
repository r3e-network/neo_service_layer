/**
 * Configuration types for Neo Service Layer SDK
 * Focused on JavaScript functions running within the Neo Function Service
 */

/**
 * Client configuration for function interoperability
 */
export interface ClientConfig {
  /**
   * Base URL for the Neo Service Layer API
   * @default 'http://localhost:3000'
   */
  baseUrl?: string;

  /**
   * Request timeout in milliseconds
   * @default 30000
   */
  timeout?: number;

  /**
   * API version
   * @default 'v1'
   */
  apiVersion?: string;

  /**
   * Debug mode
   * @default false
   */
  debug?: boolean;

  /**
   * Headers for function context authentication
   * Automatically set by the function context utility
   */
  headers?: Record<string, string>;
}

/**
 * Function execution environment configuration
 */
export interface FunctionExecutionConfig {
  /**
   * Function ID
   */
  functionId: string;
  
  /**
   * Execution ID
   */
  executionId: string;
  
  /**
   * Function owner
   */
  owner: string;
  
  /**
   * Function caller
   */
  caller?: string;
  
  /**
   * Function parameters
   */
  parameters?: Record<string, any>;
  
  /**
   * Function environment variables
   */
  env?: Record<string, string>;
  
  /**
   * Trace ID for request tracking
   */
  traceId: string;
  
  /**
   * Service Layer URL
   */
  serviceLayerUrl?: string;
}
