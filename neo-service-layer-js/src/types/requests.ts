/**
 * Request types for Neo Service Layer SDK
 */

import { Runtime } from './models';

/**
 * Authentication request
 */
export interface SignatureVerificationRequest {
  /** NEO address */
  address: string;
  
  /** Message to verify */
  message: string;
  
  /** Signature to verify */
  signature: string;
}

/**
 * Function creation request
 */
export interface FunctionRequest {
  /** Function name */
  name: string;
  
  /** Function description */
  description: string;
  
  /** Function code */
  code: string;
  
  /** Function runtime */
  runtime: Runtime;
}

/**
 * Function update request
 */
export interface FunctionUpdateRequest {
  /** Function name */
  name?: string;
  
  /** Function description */
  description?: string;
  
  /** Function code */
  code?: string;
  
  /** Function runtime */
  runtime?: Runtime;
  
  /** Function status */
  status?: string;
}

/**
 * Function permissions update request
 */
export interface FunctionPermissionsUpdateRequest {
  /** Users allowed to invoke */
  allowedUsers?: string[];
  
  /** Whether the function is public */
  public?: boolean;
  
  /** Whether the function is read-only */
  readOnly?: boolean;
}

/**
 * Secret storage request
 */
export interface SecretRequest {
  /** Secret key */
  key: string;
  
  /** Secret value */
  value: string;
  
  /** Secret description */
  description?: string;
  
  /** Secret tags */
  tags?: string[];
}

/**
 * Trigger creation request
 */
export interface TriggerRequest {
  /** Trigger name */
  name: string;
  
  /** Trigger description */
  description: string;
  
  /** Trigger type */
  type: string;
  
  /** Trigger condition */
  condition: Record<string, any>;
  
  /** Function to execute */
  functionId: string;
  
  /** Function parameters */
  parameters?: Record<string, any>;
}

/**
 * Gas allocation request
 */
export interface GasAllocationRequest {
  /** Account address */
  address: string;
  
  /** Gas amount */
  amount: number;
  
  /** Allocation expiry in seconds */
  expirySeconds?: number;
}

/**
 * Price feed update request
 */
export interface PriceFeedUpdateRequest {
  /** Symbol */
  symbol: string;
  
  /** Price */
  price: number;
  
  /** Source */
  source: string;
  
  /** 24-hour price change percentage */
  change24h?: number;
  
  /** Additional metadata */
  metadata?: Record<string, any>;
}

/**
 * Transaction request
 */
export interface TransactionRequest {
  /** Transaction type */
  type: string;
  
  /** Transaction recipient (if applicable) */
  recipient?: string;
  
  /** Transaction amount (if applicable) */
  amount?: number;
  
  /** Transaction asset (if applicable) */
  asset?: string;
  
  /** Contract script hash (if applicable) */
  contractHash?: string;
  
  /** Contract method (if applicable) */
  contractMethod?: string;
  
  /** Contract parameters (if applicable) */
  contractParams?: any[];
  
  /** Additional metadata */
  metadata?: Record<string, any>;
}
