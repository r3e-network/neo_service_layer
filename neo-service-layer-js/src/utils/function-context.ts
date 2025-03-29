/**
 * Function Context Utilities
 * 
 * Provides utilities for JavaScript functions running within the Neo Function Service
 * to interact with the Neo Service Layer.
 */

import { NeoServiceLayer } from '../core/client';
import { FunctionExecutionConfig } from '../types/config';
import { Transaction, TransactionStatus } from '../types/models';
import { TransactionRequest } from '../types/requests';
import { PaginatedResponse } from '../types/responses';

/**
 * Transaction context interface
 */
export interface TransactionContext {
  /** Create a new blockchain transaction */
  create: (config: TransactionRequest) => Promise<Transaction>;
  
  /** Sign a transaction with the function owner's key */
  sign: (txId: string) => Promise<Transaction>;
  
  /** Send a transaction to the blockchain */
  send: (txId: string) => Promise<Transaction>;
  
  /** Get the current status of a transaction */
  status: (txId: string) => Promise<TransactionStatus>;
  
  /** Get transaction details */
  get: (txId: string) => Promise<Transaction>;
  
  /** List transactions created by the function owner */
  list: (page?: number, pageSize?: number, status?: string) => Promise<PaginatedResponse<Transaction>>;
  
  /** Estimate the fee for a transaction */
  estimateFee: (config: TransactionRequest) => Promise<number>;
}

/**
 * Function context interface
 */
export interface FunctionContext {
  /** Function ID */
  functionId: string;
  
  /** Execution ID */
  executionId: string;
  
  /** Function owner */
  owner: string;
  
  /** Caller address */
  caller?: string;
  
  /** Execution parameters */
  parameters: Record<string, any>;
  
  /** Function environment variables */
  env: Record<string, string>;
  
  /** Trace ID for request tracing */
  traceId: string;
  
  /** Neo Service Layer client */
  neoServiceLayer: NeoServiceLayer;
  
  /** Log a message */
  log: (message: string) => void;
  
  /** Get a secret value */
  getSecret: (key: string) => Promise<string>;
  
  /** Get current gas price */
  getGasPrice: () => Promise<number>;
  
  /** Get price for a symbol */
  getPrice: (symbol: string) => Promise<number>;
  
  /** Invoke another function */
  invokeFunction: (functionId: string, parameters?: Record<string, any>) => Promise<any>;
  
  /** Transaction service methods */
  transaction: TransactionContext;
}

/**
 * Create a function context from execution environment
 * This is automatically called by the Neo Function Service runtime
 */
export function createFunctionContext(executionEnv: FunctionExecutionConfig): FunctionContext {
  // Initialize Neo Service Layer client
  const neoServiceLayer = new NeoServiceLayer({
    baseUrl: executionEnv.serviceLayerUrl || process.env.NEO_SERVICE_LAYER_URL,
    // Function execution context already has authentication
    headers: {
      'X-Function-Id': executionEnv.functionId,
      'X-Execution-Id': executionEnv.executionId,
      'X-Trace-Id': executionEnv.traceId
    }
  });
  
  return {
    functionId: executionEnv.functionId,
    executionId: executionEnv.executionId,
    owner: executionEnv.owner,
    caller: executionEnv.caller,
    parameters: executionEnv.parameters || {},
    env: executionEnv.env || {},
    traceId: executionEnv.traceId,
    neoServiceLayer,
    
    // Logging utility
    log: (message: string) => {
      console.log(`[${executionEnv.functionId}] ${message}`);
    },
    
    // Secret retrieval utility
    getSecret: async (key: string) => {
      return await neoServiceLayer.secrets.getSecret(key);
    },
    
    // Gas price utility
    getGasPrice: async () => {
      return await neoServiceLayer.gasBank.getGasPrice();
    },
    
    // Price feed utility
    getPrice: async (symbol: string) => {
      const priceFeed = await neoServiceLayer.priceFeed.getPrice(symbol);
      return priceFeed.price;
    },
    
    // Function invocation utility
    invokeFunction: async (functionId: string, parameters: Record<string, any> = {}) => {
      const execution = await neoServiceLayer.functions.invokeFunction({
        functionId,
        parameters,
        caller: executionEnv.functionId
      });
      return execution.result;
    },
    
    // Transaction service methods
    transaction: {
      create: async (config: TransactionRequest) => {
        return await neoServiceLayer.transaction.createTransaction(config);
      },
      
      sign: async (txId: string) => {
        return await neoServiceLayer.transaction.signTransaction(txId);
      },
      
      send: async (txId: string) => {
        return await neoServiceLayer.transaction.sendTransaction(txId);
      },
      
      status: async (txId: string) => {
        return await neoServiceLayer.transaction.getTransactionStatus(txId);
      },
      
      get: async (txId: string) => {
        return await neoServiceLayer.transaction.getTransaction(txId);
      },
      
      list: async (page: number = 1, pageSize: number = 20, status?: string) => {
        return await neoServiceLayer.transaction.listTransactions(page, pageSize, status);
      },
      
      estimateFee: async (config: TransactionRequest) => {
        return await neoServiceLayer.transaction.estimateFee(config);
      }
    }
  };
}

/**
 * Function wrapper to simplify function development
 * @param handler Function handler
 */
export function createFunction(
  handler: (context: FunctionContext, ...args: any[]) => Promise<any> | any
) {
  return async function(args: any = {}) {
    try {
      // Get execution environment from global context
      const executionEnv = (global as any).__neoExecutionEnv || {};
      
      // Create function context
      const context = createFunctionContext(executionEnv);
      
      // Call handler with context and args
      return await handler(context, args);
    } catch (error: any) {
      console.error('Function execution error:', error);
      return {
        error: error.message || 'Unknown error',
        stack: error.stack
      };
    }
  };
}

/**
 * Wrap a function handler with error handling and context creation
 * @param handler Function handler
 */
export function wrapFunctionHandler(
  handler: (context: FunctionContext, ...args: any[]) => Promise<any> | any
) {
  return async function(args: any = {}) {
    try {
      // Get execution environment from global context
      const executionEnv = (global as any).__neoExecutionEnv || {};
      
      // Create function context
      const context = createFunctionContext(executionEnv);
      
      // Log function start
      context.log(`Function execution started: ${context.functionId}`);
      
      // Call handler with context and args
      const result = await handler(context, args);
      
      // Log function completion
      context.log(`Function execution completed: ${context.functionId}`);
      
      return result;
    } catch (error: any) {
      console.error('Function execution error:', error);
      return {
        error: error.message || 'Unknown error',
        stack: error.stack
      };
    }
  };
}
