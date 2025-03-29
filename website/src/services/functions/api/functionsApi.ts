import {
  UserFunction,
  FunctionExecution,
  FunctionTemplate,
  FunctionTrigger,
  FunctionConfig,
  FunctionPermissions
} from '../types/types';
import { API_BASE_URL } from '../constants';

/**
 * Functions API client for managing user functions
 */
class FunctionsApi {
  private baseUrl: string;

  constructor() {
    this.baseUrl = `${API_BASE_URL}/functions`;
  }

  /**
   * Get all functions for the authenticated user
   */
  async getFunctions(): Promise<UserFunction[]> {
    const response = await fetch(`${this.baseUrl}`);
    if (!response.ok) {
      throw new Error('Failed to fetch functions');
    }
    return response.json();
  }

  /**
   * Get a specific function by ID
   */
  async getFunction(id: string): Promise<UserFunction> {
    const response = await fetch(`${this.baseUrl}/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch function');
    }
    return response.json();
  }

  /**
   * Create a new function
   */
  async createFunction(
    name: string,
    code: string,
    language: string,
    config: Partial<FunctionConfig>
  ): Promise<UserFunction> {
    const response = await fetch(`${this.baseUrl}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ name, code, language, config }),
    });
    if (!response.ok) {
      throw new Error('Failed to create function');
    }
    return response.json();
  }

  /**
   * Update an existing function
   */
  async updateFunction(
    id: string,
    updates: Partial<UserFunction>
  ): Promise<UserFunction> {
    const response = await fetch(`${this.baseUrl}/${id}`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(updates),
    });
    if (!response.ok) {
      throw new Error('Failed to update function');
    }
    return response.json();
  }

  /**
   * Delete a function
   */
  async deleteFunction(id: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) {
      throw new Error('Failed to delete function');
    }
  }

  /**
   * Deploy a function
   */
  async deployFunction(id: string): Promise<UserFunction> {
    const response = await fetch(`${this.baseUrl}/${id}/deploy`, {
      method: 'POST',
    });
    if (!response.ok) {
      throw new Error('Failed to deploy function');
    }
    return response.json();
  }

  /**
   * Execute a function manually
   */
  async executeFunction(
    id: string,
    params?: Record<string, any>
  ): Promise<FunctionExecution> {
    const response = await fetch(`${this.baseUrl}/${id}/execute`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ params }),
    });
    if (!response.ok) {
      throw new Error('Failed to execute function');
    }
    return response.json();
  }

  /**
   * Get function executions
   */
  async getFunctionExecutions(
    id: string,
    options?: {
      limit?: number;
      offset?: number;
      status?: string;
    }
  ): Promise<{ executions: FunctionExecution[]; total: number }> {
    const params = new URLSearchParams();
    if (options) {
      Object.entries(options).forEach(([key, value]) => {
        if (value !== undefined) {
          params.append(key, value.toString());
        }
      });
    }

    const response = await fetch(`${this.baseUrl}/${id}/executions?${params.toString()}`);
    if (!response.ok) {
      throw new Error('Failed to fetch executions');
    }
    return response.json();
  }

  /**
   * Get function templates
   */
  async getTemplates(category?: string): Promise<FunctionTemplate[]> {
    const params = new URLSearchParams();
    if (category) {
      params.append('category', category);
    }

    const response = await fetch(`${this.baseUrl}/templates?${params.toString()}`);
    if (!response.ok) {
      throw new Error('Failed to fetch templates');
    }
    return response.json();
  }

  /**
   * Create a trigger for a function
   */
  async createTrigger(
    functionId: string,
    trigger: Omit<FunctionTrigger, 'id'>
  ): Promise<FunctionTrigger> {
    const response = await fetch(`${this.baseUrl}/${functionId}/triggers`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(trigger),
    });
    if (!response.ok) {
      throw new Error('Failed to create trigger');
    }
    return response.json();
  }

  /**
   * Update a trigger
   */
  async updateTrigger(
    functionId: string,
    triggerId: string,
    updates: Partial<FunctionTrigger>
  ): Promise<FunctionTrigger> {
    const response = await fetch(`${this.baseUrl}/${functionId}/triggers/${triggerId}`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(updates),
    });
    if (!response.ok) {
      throw new Error('Failed to update trigger');
    }
    return response.json();
  }

  /**
   * Delete a trigger
   */
  async deleteTrigger(functionId: string, triggerId: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/${functionId}/triggers/${triggerId}`, {
      method: 'DELETE',
    });
    if (!response.ok) {
      throw new Error('Failed to delete trigger');
    }
  }

  /**
   * Update function permissions
   */
  async updatePermissions(
    id: string,
    permissions: Partial<FunctionPermissions>
  ): Promise<FunctionPermissions> {
    const response = await fetch(`${this.baseUrl}/${id}/permissions`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(permissions),
    });
    if (!response.ok) {
      throw new Error('Failed to update permissions');
    }
    return response.json();
  }

  /**
   * Get function logs
   */
  async getLogs(
    id: string,
    options?: {
      limit?: number;
      offset?: number;
      startTime?: number;
      endTime?: number;
    }
  ): Promise<{ logs: string[]; total: number }> {
    const params = new URLSearchParams();
    if (options) {
      Object.entries(options).forEach(([key, value]) => {
        if (value !== undefined) {
          params.append(key, value.toString());
        }
      });
    }

    const response = await fetch(`${this.baseUrl}/${id}/logs?${params.toString()}`);
    if (!response.ok) {
      throw new Error('Failed to fetch logs');
    }
    return response.json();
  }
}

export const functionsApi = new FunctionsApi();