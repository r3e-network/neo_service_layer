/**
 * Functions Service
 * 
 * Service for creating, managing, and invoking serverless functions.
 */

import { AxiosRequestConfig } from 'axios';
import { NeoServiceLayer } from '../../core/client';
import { FunctionExecutionError, NotFoundError, ValidationError } from '../../core/errors';
import { 
  Function,
  FunctionExecution,
  FunctionInvocation,
  FunctionPermissions,
  FunctionStatus
} from '../../types/models';
import {
  FunctionPermissionsUpdateRequest,
  FunctionRequest,
  FunctionUpdateRequest
} from '../../types/requests';
import { PaginatedResponse } from '../../types/responses';

/**
 * Functions Service for Neo Service Layer
 */
export class functionservice {
  private client: NeoServiceLayer;
  private basePath: string = '/api/v1/functions';

  /**
   * Create a new Functions service instance
   * @param client Neo Service Layer client
   */
  constructor(client: NeoServiceLayer) {
    this.client = client;
  }

  /**
   * Create a new function
   * @param request Function creation request
   * @returns Created function
   */
  public async createFunction(request: FunctionRequest): Promise<Function> {
    try {
      // Validate request
      if (!request.name) {
        throw new ValidationError('Function name is required', 'name');
      }
      if (!request.code) {
        throw new ValidationError('Function code is required', 'code');
      }

      // Create function
      return await this.client.request<Function>('POST', this.basePath, request);
    } catch (error) {
      throw error;
    }
  }

  /**
   * Get a function by ID
   * @param functionId Function ID
   * @returns Function
   */
  public async getFunction(functionId: string): Promise<Function> {
    try {
      if (!functionId) {
        throw new ValidationError('Function ID is required', 'functionId');
      }

      const response = await this.client.request<Function>('GET', `${this.basePath}/${functionId}`);
      return response;
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Function', functionId);
      }
      throw error;
    }
  }

  /**
   * Update a function
   * @param functionId Function ID
   * @param updates Function updates
   * @returns Updated function
   */
  public async updateFunction(functionId: string, updates: FunctionUpdateRequest): Promise<Function> {
    try {
      if (!functionId) {
        throw new ValidationError('Function ID is required', 'functionId');
      }

      return await this.client.request<Function>('PUT', `${this.basePath}/${functionId}`, updates);
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Function', functionId);
      }
      throw error;
    }
  }

  /**
   * Delete a function
   * @param functionId Function ID
   */
  public async deleteFunction(functionId: string): Promise<void> {
    try {
      if (!functionId) {
        throw new ValidationError('Function ID is required', 'functionId');
      }

      await this.client.request('DELETE', `${this.basePath}/${functionId}`);
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Function', functionId);
      }
      throw error;
    }
  }

  /**
   * List all functions
   * @param page Page number
   * @param pageSize Page size
   * @returns Paginated list of functions
   */
  public async listFunctions(page: number = 1, pageSize: number = 20): Promise<PaginatedResponse<Function>> {
    try {
      const params = { page, pageSize };
      return await this.client.request<PaginatedResponse<Function>>('GET', this.basePath, undefined, {
        params
      });
    } catch (error) {
      throw error;
    }
  }

  /**
   * Invoke a function
   * @param invocation Function invocation request
   * @returns Function execution result
   */
  public async invokeFunction(invocation: FunctionInvocation): Promise<FunctionExecution> {
    try {
      if (!invocation.functionId) {
        throw new ValidationError('Function ID is required', 'functionId');
      }

      // Generate trace ID if not provided
      if (!invocation.traceId) {
        invocation.traceId = this.generateTraceId();
      }

      const response = await this.client.request<FunctionExecution>(
        'POST',
        `${this.basePath}/${invocation.functionId}/invoke`,
        invocation
      );

      // Check for execution errors
      if (response.status === 'error' && response.error) {
        throw new FunctionExecutionError(
          response.error,
          invocation.functionId,
          response.id,
          response.logs
        );
      }

      return response;
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Function', invocation.functionId);
      }
      throw error;
    }
  }

  /**
   * List function executions
   * @param functionId Function ID
   * @param page Page number
   * @param pageSize Page size
   * @returns Paginated list of function executions
   */
  public async listExecutions(
    functionId: string,
    page: number = 1,
    pageSize: number = 20
  ): Promise<PaginatedResponse<FunctionExecution>> {
    try {
      if (!functionId) {
        throw new ValidationError('Function ID is required', 'functionId');
      }

      const params = { page, pageSize };
      return await this.client.request<PaginatedResponse<FunctionExecution>>(
        'GET',
        `${this.basePath}/${functionId}/executions`,
        undefined,
        { params }
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Function', functionId);
      }
      throw error;
    }
  }

  /**
   * Get function permissions
   * @param functionId Function ID
   * @returns Function permissions
   */
  public async getPermissions(functionId: string): Promise<FunctionPermissions> {
    try {
      if (!functionId) {
        throw new ValidationError('Function ID is required', 'functionId');
      }

      return await this.client.request<FunctionPermissions>(
        'GET',
        `${this.basePath}/${functionId}/permissions`
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Function', functionId);
      }
      throw error;
    }
  }

  /**
   * Update function permissions
   * @param functionId Function ID
   * @param permissions Function permissions updates
   * @returns Updated function permissions
   */
  public async updatePermissions(
    functionId: string,
    permissions: FunctionPermissionsUpdateRequest
  ): Promise<FunctionPermissions> {
    try {
      if (!functionId) {
        throw new ValidationError('Function ID is required', 'functionId');
      }

      return await this.client.request<FunctionPermissions>(
        'PUT',
        `${this.basePath}/${functionId}/permissions`,
        permissions
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Function', functionId);
      }
      throw error;
    }
  }

  /**
   * Generate a unique trace ID
   * @returns Trace ID
   * @private
   */
  private generateTraceId(): string {
    return `trace-${Date.now()}-${Math.random().toString(36).substring(2, 15)}`;
  }
}
