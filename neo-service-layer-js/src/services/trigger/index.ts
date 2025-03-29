/**
 * Trigger Service
 * 
 * Service for monitoring blockchain events and setting up automated execution.
 */

import { NeoServiceLayer } from '../../core/client';
import { NotFoundError, ValidationError } from '../../core/errors';
import { 
  FunctionExecution,
  Trigger,
  TriggerStatus,
  TriggerType
} from '../../types/models';
import { TriggerRequest } from '../../types/requests';
import { PaginatedResponse } from '../../types/responses';

/**
 * Trigger Service for Neo Service Layer
 */
export class TriggerService {
  private client: NeoServiceLayer;
  private basePath: string = '/api/v1/triggers';

  /**
   * Create a new Trigger service instance
   * @param client Neo Service Layer client
   */
  constructor(client: NeoServiceLayer) {
    this.client = client;
  }

  /**
   * Create a new trigger
   * @param request Trigger creation request
   * @returns Created trigger
   */
  public async createTrigger(request: TriggerRequest): Promise<Trigger> {
    try {
      // Validate request
      if (!request.name) {
        throw new ValidationError('Trigger name is required', 'name');
      }
      if (!request.type) {
        throw new ValidationError('Trigger type is required', 'type');
      }
      if (!request.condition) {
        throw new ValidationError('Trigger condition is required', 'condition');
      }
      if (!request.functionId) {
        throw new ValidationError('Function ID is required', 'functionId');
      }

      // Validate trigger type
      if (!Object.values(TriggerType).includes(request.type as TriggerType)) {
        throw new ValidationError(
          `Invalid trigger type. Must be one of: ${Object.values(TriggerType).join(', ')}`,
          'type'
        );
      }

      // Create trigger
      return await this.client.request<Trigger>('POST', this.basePath, request);
    } catch (error) {
      throw error;
    }
  }

  /**
   * Get a trigger by ID
   * @param triggerId Trigger ID
   * @returns Trigger
   */
  public async getTrigger(triggerId: string): Promise<Trigger> {
    try {
      if (!triggerId) {
        throw new ValidationError('Trigger ID is required', 'triggerId');
      }

      return await this.client.request<Trigger>('GET', `${this.basePath}/${triggerId}`);
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Trigger', triggerId);
      }
      throw error;
    }
  }

  /**
   * Update a trigger
   * @param triggerId Trigger ID
   * @param updates Trigger updates
   * @returns Updated trigger
   */
  public async updateTrigger(
    triggerId: string,
    updates: Partial<TriggerRequest> & { status?: TriggerStatus }
  ): Promise<Trigger> {
    try {
      if (!triggerId) {
        throw new ValidationError('Trigger ID is required', 'triggerId');
      }

      // Validate trigger status if provided
      if (
        updates.status &&
        !Object.values(TriggerStatus).includes(updates.status as TriggerStatus)
      ) {
        throw new ValidationError(
          `Invalid trigger status. Must be one of: ${Object.values(TriggerStatus).join(', ')}`,
          'status'
        );
      }

      return await this.client.request<Trigger>('PUT', `${this.basePath}/${triggerId}`, updates);
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Trigger', triggerId);
      }
      throw error;
    }
  }

  /**
   * Delete a trigger
   * @param triggerId Trigger ID
   */
  public async deleteTrigger(triggerId: string): Promise<void> {
    try {
      if (!triggerId) {
        throw new ValidationError('Trigger ID is required', 'triggerId');
      }

      await this.client.request('DELETE', `${this.basePath}/${triggerId}`);
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Trigger', triggerId);
      }
      throw error;
    }
  }

  /**
   * List all triggers
   * @param page Page number
   * @param pageSize Page size
   * @param type Optional filter by trigger type
   * @returns Paginated list of triggers
   */
  public async listTriggers(
    page: number = 1,
    pageSize: number = 20,
    type?: TriggerType
  ): Promise<PaginatedResponse<Trigger>> {
    try {
      const params: any = { page, pageSize };
      if (type) {
        params.type = type;
      }

      return await this.client.request<PaginatedResponse<Trigger>>(
        'GET',
        this.basePath,
        undefined,
        { params }
      );
    } catch (error) {
      throw error;
    }
  }

  /**
   * Get trigger executions
   * @param triggerId Trigger ID
   * @param page Page number
   * @param pageSize Page size
   * @returns Paginated list of trigger executions
   */
  public async getTriggerExecutions(
    triggerId: string,
    page: number = 1,
    pageSize: number = 20
  ): Promise<PaginatedResponse<FunctionExecution>> {
    try {
      if (!triggerId) {
        throw new ValidationError('Trigger ID is required', 'triggerId');
      }

      const params = { page, pageSize };
      return await this.client.request<PaginatedResponse<FunctionExecution>>(
        'GET',
        `${this.basePath}/${triggerId}/executions`,
        undefined,
        { params }
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Trigger', triggerId);
      }
      throw error;
    }
  }

  /**
   * Get trigger metrics
   * @param triggerId Trigger ID
   * @returns Trigger metrics
   */
  public async getTriggerMetrics(triggerId: string): Promise<any> {
    try {
      if (!triggerId) {
        throw new ValidationError('Trigger ID is required', 'triggerId');
      }

      return await this.client.request<any>(
        'GET',
        `${this.basePath}/${triggerId}/metrics`
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Trigger', triggerId);
      }
      throw error;
    }
  }

  /**
   * Manually execute a trigger
   * @param triggerId Trigger ID
   * @param parameters Optional parameters to override trigger parameters
   * @returns Function execution
   */
  public async executeTrigger(
    triggerId: string,
    parameters?: Record<string, any>
  ): Promise<FunctionExecution> {
    try {
      if (!triggerId) {
        throw new ValidationError('Trigger ID is required', 'triggerId');
      }

      return await this.client.request<FunctionExecution>(
        'POST',
        `${this.basePath}/${triggerId}/execute`,
        { parameters }
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Trigger', triggerId);
      }
      throw error;
    }
  }

  /**
   * Enable a trigger
   * @param triggerId Trigger ID
   * @returns Updated trigger
   */
  public async enableTrigger(triggerId: string): Promise<Trigger> {
    try {
      if (!triggerId) {
        throw new ValidationError('Trigger ID is required', 'triggerId');
      }

      return await this.client.request<Trigger>(
        'PUT',
        `${this.basePath}/${triggerId}/enable`
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Trigger', triggerId);
      }
      throw error;
    }
  }

  /**
   * Disable a trigger
   * @param triggerId Trigger ID
   * @returns Updated trigger
   */
  public async disableTrigger(triggerId: string): Promise<Trigger> {
    try {
      if (!triggerId) {
        throw new ValidationError('Trigger ID is required', 'triggerId');
      }

      return await this.client.request<Trigger>(
        'PUT',
        `${this.basePath}/${triggerId}/disable`
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Trigger', triggerId);
      }
      throw error;
    }
  }
}
