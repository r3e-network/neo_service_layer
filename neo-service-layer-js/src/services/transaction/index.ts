/**
 * Transaction Service
 * 
 * Service for creating, signing, and sending blockchain transactions.
 */

import { NeoServiceLayer } from '../../core/client';
import { NotFoundError, ValidationError } from '../../core/errors';
import { Transaction, TransactionStatus } from '../../types/models';
import { TransactionRequest } from '../../types/requests';
import { PaginatedResponse } from '../../types/responses';

/**
 * Transaction Service for Neo Service Layer
 */
export class TransactionService {
  private client: NeoServiceLayer;
  private basePath: string = '/api/v1/transactions';

  /**
   * Create a new Transaction service instance
   * @param client Neo Service Layer client
   */
  constructor(client: NeoServiceLayer) {
    this.client = client;
  }

  /**
   * Create a new transaction
   * @param request Transaction creation request
   * @returns Created transaction
   */
  public async createTransaction(request: TransactionRequest): Promise<Transaction> {
    try {
      // Validate request
      if (!request.type) {
        throw new ValidationError('Transaction type is required', 'type');
      }
      
      const response = await this.client.post<Transaction>(
        `${this.basePath}/create`,
        request
      );
      
      return response;
    } catch (error) {
      throw this.client.handleError(error);
    }
  }

  /**
   * Sign a transaction
   * @param txId Transaction ID
   * @returns Signed transaction
   */
  public async signTransaction(txId: string): Promise<Transaction> {
    try {
      if (!txId) {
        throw new ValidationError('Transaction ID is required', 'txId');
      }
      
      const response = await this.client.post<Transaction>(
        `${this.basePath}/${txId}/sign`,
        {}
      );
      
      return response;
    } catch (error) {
      throw this.client.handleError(error);
    }
  }

  /**
   * Send a transaction to the blockchain
   * @param txId Transaction ID
   * @returns Sent transaction
   */
  public async sendTransaction(txId: string): Promise<Transaction> {
    try {
      if (!txId) {
        throw new ValidationError('Transaction ID is required', 'txId');
      }
      
      const response = await this.client.post<Transaction>(
        `${this.basePath}/${txId}/send`,
        {}
      );
      
      return response;
    } catch (error) {
      throw this.client.handleError(error);
    }
  }

  /**
   * Get transaction status
   * @param txId Transaction ID
   * @returns Transaction status
   */
  public async getTransactionStatus(txId: string): Promise<TransactionStatus> {
    try {
      if (!txId) {
        throw new ValidationError('Transaction ID is required', 'txId');
      }
      
      const response = await this.client.get<TransactionStatus>(
        `${this.basePath}/${txId}/status`
      );
      
      return response;
    } catch (error) {
      throw this.client.handleError(error);
    }
  }

  /**
   * Get transaction by ID
   * @param txId Transaction ID
   * @returns Transaction
   */
  public async getTransaction(txId: string): Promise<Transaction> {
    try {
      if (!txId) {
        throw new ValidationError('Transaction ID is required', 'txId');
      }
      
      const response = await this.client.get<Transaction>(
        `${this.basePath}/${txId}`
      );
      
      return response;
    } catch (error) {
      throw this.client.handleError(error);
    }
  }

  /**
   * List transactions
   * @param page Page number
   * @param pageSize Page size
   * @param status Optional transaction status filter
   * @returns Paginated list of transactions
   */
  public async listTransactions(
    page: number = 1,
    pageSize: number = 20,
    status?: string
  ): Promise<PaginatedResponse<Transaction>> {
    try {
      const params: Record<string, any> = {
        page,
        pageSize
      };
      
      if (status) {
        params.status = status;
      }
      
      const response = await this.client.get<PaginatedResponse<Transaction>>(
        this.basePath,
        { params }
      );
      
      return response;
    } catch (error) {
      throw this.client.handleError(error);
    }
  }

  /**
   * Estimate transaction fee
   * @param request Transaction request
   * @returns Estimated fee in GAS
   */
  public async estimateFee(request: TransactionRequest): Promise<number> {
    try {
      const response = await this.client.post<{ fee: number }>(
        `${this.basePath}/estimate-fee`,
        request
      );
      
      return response.fee;
    } catch (error) {
      throw this.client.handleError(error);
    }
  }
}
