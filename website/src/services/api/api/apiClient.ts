import { wallet } from '@cityofzion/neon-core';
import { sign } from '@cityofzion/neon-core/lib/wallet/signing';
import { SignedMessage, ApiEndpoint, ApiKey, ApiUsage, ApiMetrics, ApiError } from '../types/types';

export class ApiClient {
  private baseUrl: string;
  private account: wallet.Account | null = null;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  private async signMessage(message: string): Promise<SignedMessage> {
    if (!this.account) {
      throw new Error('No account set');
    }

    const salt = crypto.randomUUID();
    const messageToSign = `${message}:${salt}`;
    const signature = sign(messageToSign, this.account.privateKey);
    
    return {
      message,
      signature,
      publicKey: this.account.publicKey,
      salt
    };
  }

  private async fetchWithAuth(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<Response> {
    const timestamp = Date.now().toString();
    const signedMessage = await this.signMessage(timestamp);

    const headers = new Headers(options.headers);
    headers.set('X-Neo-Timestamp', timestamp);
    headers.set('X-Neo-Signature', signedMessage.signature);
    headers.set('X-Neo-PublicKey', signedMessage.publicKey);
    headers.set('X-Neo-Salt', signedMessage.salt);

    return fetch(`${this.baseUrl}${endpoint}`, {
      ...options,
      headers
    });
  }

  async setAccount(privateKey: string): Promise<void> {
    try {
      this.account = new wallet.Account(privateKey);
    } catch (error) {
      throw new Error('Invalid private key');
    }
  }

  async getEndpoints(): Promise<ApiEndpoint[]> {
    const response = await this.fetchWithAuth('/api/endpoints');
    if (!response.ok) {
      throw new Error('Failed to fetch endpoints');
    }
    return response.json();
  }

  async createApiKey(
    name: string,
    options: {
      allowedEndpoints?: string[];
      allowedIPs?: string[];
      expiresAt?: number;
      rateLimit?: {
        requestsPerMinute: number;
        burstLimit: number;
      };
    } = {}
  ): Promise<ApiKey> {
    const response = await this.fetchWithAuth('/api/keys', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        name,
        ...options
      })
    });

    if (!response.ok) {
      throw new Error('Failed to create API key');
    }

    return response.json();
  }

  async getApiKeys(): Promise<ApiKey[]> {
    const response = await this.fetchWithAuth('/api/keys');
    if (!response.ok) {
      throw new Error('Failed to fetch API keys');
    }
    return response.json();
  }

  async updateApiKey(
    id: string,
    updates: {
      name?: string;
      allowedEndpoints?: string[];
      allowedIPs?: string[];
      expiresAt?: number;
      rateLimit?: {
        requestsPerMinute: number;
        burstLimit: number;
      };
    }
  ): Promise<ApiKey> {
    const response = await this.fetchWithAuth(`/api/keys/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(updates)
    });

    if (!response.ok) {
      throw new Error('Failed to update API key');
    }

    return response.json();
  }

  async deleteApiKey(id: string): Promise<void> {
    const response = await this.fetchWithAuth(`/api/keys/${id}`, {
      method: 'DELETE'
    });

    if (!response.ok) {
      throw new Error('Failed to delete API key');
    }
  }

  async getApiUsage(
    options: {
      startTime?: number;
      endTime?: number;
      endpoint?: string;
    } = {}
  ): Promise<ApiUsage[]> {
    const params = new URLSearchParams();
    if (options.startTime) params.set('startTime', options.startTime.toString());
    if (options.endTime) params.set('endTime', options.endTime.toString());
    if (options.endpoint) params.set('endpoint', options.endpoint);

    const response = await this.fetchWithAuth(
      `/api/usage?${params.toString()}`
    );

    if (!response.ok) {
      throw new Error('Failed to fetch API usage');
    }

    return response.json();
  }

  async getApiMetrics(
    options: {
      interval?: '1m' | '5m' | '1h' | '1d';
      duration?: '1h' | '1d' | '7d' | '30d';
    } = {}
  ): Promise<ApiMetrics> {
    const params = new URLSearchParams();
    if (options.interval) params.set('interval', options.interval);
    if (options.duration) params.set('duration', options.duration);

    const response = await this.fetchWithAuth(
      `/api/metrics?${params.toString()}`
    );

    if (!response.ok) {
      throw new Error('Failed to fetch API metrics');
    }

    return response.json();
  }

  async getApiErrors(
    options: {
      startTime?: number;
      endTime?: number;
      endpoint?: string;
      limit?: number;
    } = {}
  ): Promise<ApiError[]> {
    const params = new URLSearchParams();
    if (options.startTime) params.set('startTime', options.startTime.toString());
    if (options.endTime) params.set('endTime', options.endTime.toString());
    if (options.endpoint) params.set('endpoint', options.endpoint);
    if (options.limit) params.set('limit', options.limit.toString());

    const response = await this.fetchWithAuth(
      `/api/errors?${params.toString()}`
    );

    if (!response.ok) {
      throw new Error('Failed to fetch API errors');
    }

    return response.json();
  }
}