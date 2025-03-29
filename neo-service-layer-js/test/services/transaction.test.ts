/**
 * Transaction Service Tests
 * 
 * Tests for the Transaction Service module of the Neo Service Layer JavaScript SDK.
 */

import axios from 'axios';
import { NeoServiceLayer } from '../../src/core/client';
import { TransactionService } from '../../src/services/transaction';
import { Transaction, TransactionStatus, TransactionType } from '../../src/types/models';
import { ValidationError } from '../../src/core/errors';

// Add Jest types
declare const jest: any;
declare const describe: any;
declare const beforeEach: any;
declare const it: any;
declare const expect: any;

// Mock the NeoServiceLayer client
jest.mock('../../src/core/client');
const MockedNeoServiceLayer = NeoServiceLayer as any;

describe('TransactionService', () => {
  let client: any;
  let transactionService: TransactionService;
  
  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();
    
    // Create mock client with request method
    client = {
      request: jest.fn(),
      get: jest.fn(),
      post: jest.fn(),
      put: jest.fn(),
      delete: jest.fn(),
      basePath: '/api/v1',
      handleError: jest.fn((error: any) => {
        if (error.response?.data?.error) {
          return new Error(error.response.data.error);
        }
        return error;
      })
    };
    
    // Set up the mock implementation
    MockedNeoServiceLayer.mockImplementation(() => client);
    
    // Create transaction service with mock client
    transactionService = new TransactionService(client);
  });
  
  describe('createTransaction', () => {
    it('should create a transaction successfully', async () => {
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
      
      // Mock the post method
      client.post.mockResolvedValueOnce(mockTransaction);
      
      // Call service method
      const result = await transactionService.createTransaction({
        type: TransactionType.TRANSFER,
        recipient: 'address-2',
        amount: 5,
        asset: 'NEO'
      });
      
      // Assertions
      expect(client.post).toHaveBeenCalledTimes(1);
      expect(client.post).toHaveBeenCalledWith(
        '/api/v1/transactions/create',
        {
          type: TransactionType.TRANSFER,
          recipient: 'address-2',
          amount: 5,
          asset: 'NEO'
        }
      );
      expect(result).toEqual(mockTransaction);
    });
    
    it('should throw validation error when type is missing', async () => {
      // Call service method and expect error
      await expect(transactionService.createTransaction({
        recipient: 'address-2',
        amount: 5,
        asset: 'NEO'
      } as any)).rejects.toThrow('Transaction type is required');
    });
    
    it('should handle API error', async () => {
      // Mock error response
      const errorResponse = {
        response: {
          data: { error: 'Invalid transaction parameters' },
          status: 400
        }
      };
      client.post.mockRejectedValueOnce(errorResponse);
      
      // Call service method and expect error
      await expect(transactionService.createTransaction({
        type: TransactionType.TRANSFER,
        recipient: 'address-2',
        amount: 5,
        asset: 'NEO'
      })).rejects.toThrow('Invalid transaction parameters');
    });
  });
  
  describe('signTransaction', () => {
    it('should sign a transaction successfully', async () => {
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
      
      // Mock the post method
      client.post.mockResolvedValueOnce(mockTransaction);
      
      // Call service method
      const result = await transactionService.signTransaction('tx-123');
      
      // Assertions
      expect(client.post).toHaveBeenCalledTimes(1);
      expect(client.post).toHaveBeenCalledWith(
        '/api/v1/transactions/tx-123/sign',
        {}
      );
      expect(result).toEqual(mockTransaction);
    });
    
    it('should throw validation error when txId is missing', async () => {
      // Call service method and expect error
      await expect(transactionService.signTransaction('')).rejects.toThrow('Transaction ID is required');
    });
  });
  
  describe('sendTransaction', () => {
    it('should send a transaction successfully', async () => {
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
      
      // Mock the post method
      client.post.mockResolvedValueOnce(mockTransaction);
      
      // Call service method
      const result = await transactionService.sendTransaction('tx-123');
      
      // Assertions
      expect(client.post).toHaveBeenCalledTimes(1);
      expect(client.post).toHaveBeenCalledWith(
        '/api/v1/transactions/tx-123/send',
        {}
      );
      expect(result).toEqual(mockTransaction);
    });
  });
  
  describe('getTransactionStatus', () => {
    it('should get transaction status successfully', async () => {
      // Mock response
      const mockStatus = {
        status: TransactionStatus.CONFIRMED,
        blockHeight: 12345,
        confirmations: 10,
        timestamp: new Date().toISOString()
      };
      
      // Mock the get method
      client.get.mockResolvedValueOnce(mockStatus);
      
      // Call service method
      const result = await transactionService.getTransactionStatus('tx-123');
      
      // Assertions
      expect(client.get).toHaveBeenCalledTimes(1);
      expect(client.get).toHaveBeenCalledWith(
        '/api/v1/transactions/tx-123/status'
      );
      expect(result).toEqual(mockStatus);
    });
  });
  
  describe('estimateFee', () => {
    it('should estimate transaction fee successfully', async () => {
      // Mock response
      const mockResponse = { fee: 0.001 };
      
      // Mock the post method
      client.post.mockResolvedValueOnce(mockResponse);
      
      // Call service method
      const result = await transactionService.estimateFee({
        type: TransactionType.TRANSFER,
        recipient: 'address-2',
        amount: 5,
        asset: 'NEO'
      });
      
      // Assertions
      expect(client.post).toHaveBeenCalledTimes(1);
      expect(client.post).toHaveBeenCalledWith(
        '/api/v1/transactions/estimate-fee',
        {
          type: TransactionType.TRANSFER,
          recipient: 'address-2',
          amount: 5,
          asset: 'NEO'
        }
      );
      expect(result).toBe(0.001);
    });
  });
});
