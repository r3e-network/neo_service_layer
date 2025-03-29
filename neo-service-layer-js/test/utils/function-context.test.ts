/**
 * Function Context Tests
 * 
 * Tests for the Function Context utilities of the Neo Service Layer JavaScript SDK.
 */

import { NeoServiceLayer } from '../../src/core/client';
import { createFunctionContext, wrapFunctionHandler, FunctionContext } from '../../src/utils/function-context';
import { FunctionExecutionConfig } from '../../src/types/config';
import { Transaction, TransactionStatus, TransactionType } from '../../src/types/models';

// Add Jest types
declare const jest: any;
declare const describe: any;
declare const beforeEach: any;
declare const it: any;
declare const expect: any;

// Mock the NeoServiceLayer client
jest.mock('../../src/core/client');
const MockedNeoServiceLayer = NeoServiceLayer as any;

describe('Function Context', () => {
  let mockClient: any;
  let executionConfig: FunctionExecutionConfig;
  
  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();
    
    // Create mock client
    mockClient = {
      transaction: {
        createTransaction: jest.fn(),
        signTransaction: jest.fn(),
        sendTransaction: jest.fn(),
        getTransactionStatus: jest.fn(),
        getTransaction: jest.fn(),
        listTransactions: jest.fn(),
        estimateFee: jest.fn()
      },
      secrets: {
        getSecret: jest.fn()
      },
      gasBank: {
        getGasPrice: jest.fn()
      },
      priceFeed: {
        getPrice: jest.fn()
      },
      functions: {
        invokeFunction: jest.fn()
      }
    };
    
    // Mock the constructor
    MockedNeoServiceLayer.mockImplementation(() => mockClient);
    
    // Set up execution config
    executionConfig = {
      functionId: 'function-123',
      executionId: 'execution-456',
      owner: 'owner-address',
      caller: 'caller-address',
      parameters: { param1: 'value1' },
      env: { ENV_VAR: 'test' },
      traceId: 'trace-789',
      serviceLayerUrl: 'https://api.example.com'
    };
  });
  
  describe('createFunctionContext', () => {
    it('should create a function context with all required properties', () => {
      // Create function context
      const context = createFunctionContext(executionConfig);
      
      // Verify context properties
      expect(context.functionId).toBe('function-123');
      expect(context.executionId).toBe('execution-456');
      expect(context.owner).toBe('owner-address');
      expect(context.caller).toBe('caller-address');
      expect(context.parameters).toEqual({ param1: 'value1' });
      expect(context.env).toEqual({ ENV_VAR: 'test' });
      expect(context.traceId).toBe('trace-789');
      expect(context.neoServiceLayer).toBeDefined();
      
      // Verify methods exist
      expect(typeof context.log).toBe('function');
      expect(typeof context.getSecret).toBe('function');
      expect(typeof context.getGasPrice).toBe('function');
      expect(typeof context.getPrice).toBe('function');
      expect(typeof context.invokeFunction).toBe('function');
      
      // Verify transaction methods exist
      expect(context.transaction).toBeDefined();
      expect(typeof context.transaction.create).toBe('function');
      expect(typeof context.transaction.sign).toBe('function');
      expect(typeof context.transaction.send).toBe('function');
      expect(typeof context.transaction.status).toBe('function');
      expect(typeof context.transaction.get).toBe('function');
      expect(typeof context.transaction.list).toBe('function');
      expect(typeof context.transaction.estimateFee).toBe('function');
    });
    
    it('should initialize NeoServiceLayer client with correct parameters', () => {
      // Create function context
      createFunctionContext(executionConfig);
      
      // Verify NeoServiceLayer constructor was called with correct parameters
      expect(MockedNeoServiceLayer).toHaveBeenCalledWith({
        baseUrl: 'https://api.example.com',
        headers: {
          'X-Function-Id': 'function-123',
          'X-Execution-Id': 'execution-456',
          'X-Trace-Id': 'trace-789'
        }
      });
    });
  });
  
  describe('transaction methods', () => {
    let context: FunctionContext;
    
    beforeEach(() => {
      // Create function context
      context = createFunctionContext(executionConfig);
    });
    
    it('should call createTransaction with correct parameters', async () => {
      // Mock response
      const mockTransaction: Partial<Transaction> = {
        id: 'tx-123',
        type: TransactionType.TRANSFER,
        status: TransactionStatus.CREATED,
        sender: 'address-1',
        recipient: 'address-2',
        amount: 5,
        asset: 'NEO',
        fee: 0.001,
        networkFee: 0.0005,
        createdAt: new Date().toISOString()
      };
      mockClient.transaction.createTransaction.mockResolvedValue(mockTransaction);
      
      // Call method
      const txConfig = {
        type: 'transfer',
        recipient: 'address-2',
        amount: 5,
        asset: 'NEO'
      };
      const result = await context.transaction.create(txConfig);
      
      // Verify
      expect(mockClient.transaction.createTransaction).toHaveBeenCalledWith(txConfig);
      expect(result).toEqual(mockTransaction);
    });
    
    it('should call signTransaction with correct parameters', async () => {
      // Mock response
      const mockTransaction: Partial<Transaction> = {
        id: 'tx-123',
        type: TransactionType.TRANSFER,
        status: TransactionStatus.SIGNED,
        sender: 'address-1',
        recipient: 'address-2',
        amount: 5,
        asset: 'NEO',
        fee: 0.001,
        networkFee: 0.0005,
        createdAt: new Date().toISOString()
      };
      mockClient.transaction.signTransaction.mockResolvedValue(mockTransaction);
      
      // Call method
      const result = await context.transaction.sign('tx-123');
      
      // Verify
      expect(mockClient.transaction.signTransaction).toHaveBeenCalledWith('tx-123');
      expect(result).toEqual(mockTransaction);
    });
    
    it('should call sendTransaction with correct parameters', async () => {
      // Mock response
      const mockTransaction: Partial<Transaction> = {
        id: 'tx-123',
        type: TransactionType.TRANSFER,
        status: TransactionStatus.SENT,
        sender: 'address-1',
        recipient: 'address-2',
        amount: 5,
        asset: 'NEO',
        fee: 0.001,
        networkFee: 0.0005,
        hash: '0x1234567890abcdef',
        createdAt: new Date().toISOString()
      };
      mockClient.transaction.sendTransaction.mockResolvedValue(mockTransaction);
      
      // Call method
      const result = await context.transaction.send('tx-123');
      
      // Verify
      expect(mockClient.transaction.sendTransaction).toHaveBeenCalledWith('tx-123');
      expect(result).toEqual(mockTransaction);
    });
    
    it('should call getTransactionStatus with correct parameters', async () => {
      // Mock response
      const mockStatus = {
        status: TransactionStatus.CONFIRMED,
        blockHeight: 12345,
        confirmations: 10,
        timestamp: new Date().toISOString()
      };
      mockClient.transaction.getTransactionStatus.mockResolvedValue(mockStatus);
      
      // Call method
      const result = await context.transaction.status('tx-123');
      
      // Verify
      expect(mockClient.transaction.getTransactionStatus).toHaveBeenCalledWith('tx-123');
      expect(result).toEqual(mockStatus);
    });
  });
  
  describe('wrapFunctionHandler', () => {
    it('should create a wrapped function that handles success case', async () => {
      // Create a handler function
      const handler = jest.fn().mockImplementation(
        (context: FunctionContext, args: any) => ({ result: 'success', args })
      );
      
      // Wrap the handler
      const wrappedHandler = wrapFunctionHandler(handler);
      
      // Call the wrapped handler
      const result = await wrappedHandler({ param1: 'value1' });
      
      // Verify
      expect(handler).toHaveBeenCalled();
      expect(result).toEqual({ result: 'success', args: { param1: 'value1' } });
    });
    
    it('should create a wrapped function that handles errors', async () => {
      // Create a handler function that throws an error
      const handler = jest.fn().mockImplementation(
        () => { throw new Error('Test error'); }
      );
      
      // Wrap the handler
      const wrappedHandler = wrapFunctionHandler(handler);
      
      // Call the wrapped handler
      const result = await wrappedHandler({ param1: 'value1' });
      
      // Verify
      expect(handler).toHaveBeenCalled();
      expect(result).toEqual({
        error: 'Test error',
        stack: expect.any(String)
      });
    });
  });
});
