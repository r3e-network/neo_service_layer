/**
 * Gas Bank Service
 * 
 * Service for managing Neo gas allocation and distribution.
 */

import { NeoServiceLayer } from '../../core/client';
import { GasBankError, NotFoundError, ValidationError } from '../../core/errors';
import { 
  GasAllocation,
  GasTransaction
} from '../../types/models';
import { GasAllocationRequest } from '../../types/requests';
import { PaginatedResponse } from '../../types/responses';

/**
 * Gas Bank Service for Neo Service Layer
 */
export class GasBankService {
  private client: NeoServiceLayer;
  private basePath: string = '/api/v1/gasbank';

  /**
   * Create a new Gas Bank service instance
   * @param client Neo Service Layer client
   */
  constructor(client: NeoServiceLayer) {
    this.client = client;
  }

  /**
   * Get gas balance for an address
   * @param address NEO address
   * @returns Gas balance
   */
  public async getBalance(address: string): Promise<number> {
    try {
      if (!address) {
        throw new ValidationError('Address is required', 'address');
      }

      const response = await this.client.request<{ balance: number }>(
        'GET',
        `${this.basePath}/balance/${address}`
      );
      return response.balance;
    } catch (error: any) {
      if (error.statusCode === 404) {
        // If no allocation exists, return 0
        return 0;
      }
      throw error;
    }
  }

  /**
   * Allocate gas to an address
   * @param request Gas allocation request
   * @returns Gas allocation
   */
  public async allocateGas(request: GasAllocationRequest): Promise<GasAllocation> {
    try {
      if (!request.address) {
        throw new ValidationError('Address is required', 'address');
      }
      if (request.amount <= 0) {
        throw new ValidationError('Amount must be greater than 0', 'amount');
      }

      return await this.client.request<GasAllocation>(
        'POST',
        `${this.basePath}/allocate`,
        request
      );
    } catch (error) {
      if (error instanceof Error) {
        throw new GasBankError(`Failed to allocate gas: ${error.message}`);
      }
      throw error;
    }
  }

  /**
   * Get gas allocation for an address
   * @param address NEO address
   * @returns Gas allocation
   */
  public async getAllocation(address: string): Promise<GasAllocation> {
    try {
      if (!address) {
        throw new ValidationError('Address is required', 'address');
      }

      return await this.client.request<GasAllocation>(
        'GET',
        `${this.basePath}/allocation/${address}`
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('GasAllocation', address);
      }
      throw error;
    }
  }

  /**
   * Update gas allocation for an address
   * @param address NEO address
   * @param amount New gas amount
   * @returns Updated gas allocation
   */
  public async updateAllocation(address: string, amount: number): Promise<GasAllocation> {
    try {
      if (!address) {
        throw new ValidationError('Address is required', 'address');
      }
      if (amount <= 0) {
        throw new ValidationError('Amount must be greater than 0', 'amount');
      }

      return await this.client.request<GasAllocation>(
        'PUT',
        `${this.basePath}/allocation/${address}`,
        { amount }
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('GasAllocation', address);
      }
      throw error;
    }
  }

  /**
   * Deactivate gas allocation for an address
   * @param address NEO address
   */
  public async deactivateAllocation(address: string): Promise<void> {
    try {
      if (!address) {
        throw new ValidationError('Address is required', 'address');
      }

      await this.client.request(
        'DELETE',
        `${this.basePath}/allocation/${address}`
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('GasAllocation', address);
      }
      throw error;
    }
  }

  /**
   * List all gas allocations
   * @param page Page number
   * @param pageSize Page size
   * @returns Paginated list of gas allocations
   */
  public async listAllocations(page: number = 1, pageSize: number = 20): Promise<PaginatedResponse<GasAllocation>> {
    try {
      const params = { page, pageSize };
      return await this.client.request<PaginatedResponse<GasAllocation>>(
        'GET',
        `${this.basePath}/allocations`,
        undefined,
        { params }
      );
    } catch (error) {
      throw error;
    }
  }

  /**
   * Get gas transactions for an address
   * @param address NEO address
   * @param page Page number
   * @param pageSize Page size
   * @returns Paginated list of gas transactions
   */
  public async getTransactions(
    address: string,
    page: number = 1,
    pageSize: number = 20
  ): Promise<PaginatedResponse<GasTransaction>> {
    try {
      if (!address) {
        throw new ValidationError('Address is required', 'address');
      }

      const params = { page, pageSize };
      return await this.client.request<PaginatedResponse<GasTransaction>>(
        'GET',
        `${this.basePath}/transactions/${address}`,
        undefined,
        { params }
      );
    } catch (error) {
      throw error;
    }
  }

  /**
   * Estimate gas for a transaction
   * @param txData Transaction data
   * @returns Estimated gas amount
   */
  public async estimateGas(txData: any): Promise<number> {
    try {
      if (!txData) {
        throw new ValidationError('Transaction data is required', 'txData');
      }

      const response = await this.client.request<{ gas: number }>(
        'POST',
        `${this.basePath}/estimate`,
        txData
      );
      return response.gas;
    } catch (error) {
      if (error instanceof Error) {
        throw new GasBankError(`Failed to estimate gas: ${error.message}`);
      }
      throw error;
    }
  }

  /**
   * Get gas price
   * @returns Current gas price
   */
  public async getGasPrice(): Promise<number> {
    try {
      const response = await this.client.request<{ price: number }>(
        'GET',
        `${this.basePath}/price`
      );
      return response.price;
    } catch (error) {
      if (error instanceof Error) {
        throw new GasBankError(`Failed to get gas price: ${error.message}`);
      }
      throw error;
    }
  }
}
