/**
 * Secrets Service
 * 
 * Service for securely storing and retrieving sensitive information.
 */

import { NeoServiceLayer } from '../../core/client';
import { NotFoundError, ValidationError } from '../../core/errors';
import { Secret } from '../../types/models';
import { SecretRequest } from '../../types/requests';
import { PaginatedResponse } from '../../types/responses';

/**
 * Secrets Service for Neo Service Layer
 */
export class secretservice {
  private client: NeoServiceLayer;
  private basePath: string = '/api/v1/secrets';

  /**
   * Create a new Secrets service instance
   * @param client Neo Service Layer client
   */
  constructor(client: NeoServiceLayer) {
    this.client = client;
  }

  /**
   * Store a secret
   * @param request Secret storage request
   * @returns Stored secret (without value)
   */
  public async storeSecret(request: SecretRequest): Promise<Secret> {
    try {
      if (!request.key) {
        throw new ValidationError('Secret key is required', 'key');
      }
      if (!request.value) {
        throw new ValidationError('Secret value is required', 'value');
      }

      return await this.client.request<Secret>(
        'POST',
        this.basePath,
        request
      );
    } catch (error) {
      throw error;
    }
  }

  /**
   * Get a secret
   * @param key Secret key
   * @returns Secret value
   */
  public async getSecret(key: string): Promise<string> {
    try {
      if (!key) {
        throw new ValidationError('Secret key is required', 'key');
      }

      const response = await this.client.request<{ value: string }>(
        'GET',
        `${this.basePath}/${key}`
      );
      return response.value;
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Secret', key);
      }
      throw error;
    }
  }

  /**
   * Delete a secret
   * @param key Secret key
   */
  public async deleteSecret(key: string): Promise<void> {
    try {
      if (!key) {
        throw new ValidationError('Secret key is required', 'key');
      }

      await this.client.request(
        'DELETE',
        `${this.basePath}/${key}`
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Secret', key);
      }
      throw error;
    }
  }

  /**
   * List all secrets (metadata only, no values)
   * @param page Page number
   * @param pageSize Page size
   * @returns Paginated list of secrets
   */
  public async listSecrets(page: number = 1, pageSize: number = 20): Promise<PaginatedResponse<Secret>> {
    try {
      const params = { page, pageSize };
      return await this.client.request<PaginatedResponse<Secret>>(
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
   * Check if a secret exists
   * @param key Secret key
   * @returns Whether the secret exists
   */
  public async hasSecret(key: string): Promise<boolean> {
    try {
      if (!key) {
        throw new ValidationError('Secret key is required', 'key');
      }

      await this.client.request(
        'HEAD',
        `${this.basePath}/${key}`
      );
      return true;
    } catch (error: any) {
      if (error.statusCode === 404) {
        return false;
      }
      throw error;
    }
  }

  /**
   * Update a secret
   * @param key Secret key
   * @param value New secret value
   * @param description Optional new description
   * @param tags Optional new tags
   * @returns Updated secret (without value)
   */
  public async updateSecret(
    key: string,
    value: string,
    description?: string,
    tags?: string[]
  ): Promise<Secret> {
    try {
      if (!key) {
        throw new ValidationError('Secret key is required', 'key');
      }
      if (!value) {
        throw new ValidationError('Secret value is required', 'value');
      }

      const request: SecretRequest = {
        key,
        value,
        description,
        tags
      };

      return await this.client.request<Secret>(
        'PUT',
        `${this.basePath}/${key}`,
        request
      );
    } catch (error: any) {
      if (error.statusCode === 404) {
        throw new NotFoundError('Secret', key);
      }
      throw error;
    }
  }

  /**
   * Get secrets by tag
   * @param tag Tag to filter by
   * @param page Page number
   * @param pageSize Page size
   * @returns Paginated list of secrets with the specified tag
   */
  public async getSecretsByTag(
    tag: string,
    page: number = 1,
    pageSize: number = 20
  ): Promise<PaginatedResponse<Secret>> {
    try {
      if (!tag) {
        throw new ValidationError('Tag is required', 'tag');
      }

      const params = { tag, page, pageSize };
      return await this.client.request<PaginatedResponse<Secret>>(
        'GET',
        `${this.basePath}/tag/${tag}`,
        undefined,
        { params }
      );
    } catch (error) {
      throw error;
    }
  }
}
