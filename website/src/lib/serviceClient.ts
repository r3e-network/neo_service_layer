import { Buffer } from 'buffer';

interface ServiceConfig {
  baseUrl?: string;
  signMessage: (message: string) => Promise<string>;
}

interface RequestOptions {
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  endpoint: string;
  params?: Record<string, any>;
}

export class ServiceClient {
  private baseUrl: string;
  private signMessage: (message: string) => Promise<string>;

  constructor(config: ServiceConfig) {
    this.baseUrl = config.baseUrl || 'https://api.neo-service-layer.io/v1';
    this.signMessage = config.signMessage;
  }

  private async createSignedRequest(options: RequestOptions): Promise<Request> {
    const timestamp = Date.now().toString();
    const nonce = crypto.randomUUID();
    
    // Create message to sign
    const message = Buffer.from(JSON.stringify({
      method: options.method,
      endpoint: options.endpoint,
      params: options.params || {},
      timestamp,
      nonce,
    })).toString('base64');

    // Get signature
    const signature = await this.signMessage(message);

    // Prepare request
    const url = new URL(options.endpoint, this.baseUrl);
    if (options.method === 'GET' && options.params) {
      Object.entries(options.params).forEach(([key, value]) => {
        url.searchParams.append(key, value.toString());
      });
    }

    const headers = new Headers({
      'Content-Type': 'application/json',
      'X-Timestamp': timestamp,
      'X-Nonce': nonce,
      'X-Signature': signature,
    });

    const requestOptions: RequestInit = {
      method: options.method,
      headers,
    };

    if (options.method !== 'GET' && options.params) {
      requestOptions.body = JSON.stringify(options.params);
    }

    return new Request(url.toString(), requestOptions);
  }

  public async execute(service: string, endpoint: string, params?: Record<string, any>): Promise<any> {
    try {
      // Determine method based on endpoint
      const method = this.getMethodForEndpoint(endpoint);
      
      // Create signed request
      const request = await this.createSignedRequest({
        method,
        endpoint: `/${service}/${endpoint}`,
        params,
      });

      // Execute request
      const response = await fetch(request);
      
      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Request failed');
      }

      return await response.json();
    } catch (error) {
      console.error('Service request failed:', error);
      throw error;
    }
  }

  private getMethodForEndpoint(endpoint: string): RequestOptions['method'] {
    // Map endpoints to HTTP methods
    const methodMap: Record<string, RequestOptions['method']> = {
      'get-balance': 'GET',
      'get-price': 'GET',
      'get-history': 'GET',
      'get-task-status': 'GET',
      'get-task-history': 'GET',
      'get-logs': 'GET',
      'get-metrics': 'GET',
      'get-service-metrics': 'GET',
      'get-contract-metrics': 'GET',
      'get-function-metrics': 'GET',
      'get-usage-stats': 'GET',
      'get-alerts': 'GET',
      'get-service-logs': 'GET',
      'get-contract-logs': 'GET',
      'get-function-logs': 'GET',
      'search-logs': 'GET',
      'list-tasks': 'GET',
      'list-functions': 'GET',
      'list-secrets': 'GET',
      'list-triggers': 'GET',
      'get-auto-funding-config': 'GET',
      'get-transaction-history': 'GET',
      'get-access-history': 'GET',
      'get-event-history': 'GET',
      'get-trigger-status': 'GET',
      'get-transaction-status': 'GET',
      'get-transaction': 'GET',
      'list-transactions': 'GET',
      
      'create-task': 'POST',
      'deploy-function': 'POST',
      'set-secret': 'POST',
      'create-trigger': 'POST',
      'configure-alerts': 'POST',
      'configure-log-retention': 'POST',
      'top-up': 'POST',
      'set-auto-funding': 'POST',
      'subscribe': 'POST',
      'create-transaction': 'POST',
      'sign-transaction': 'POST',
      'send-transaction': 'POST',
      'estimate-fee': 'POST',
      
      'update-task': 'PUT',
      'update-function': 'PUT',
      'update-trigger': 'PUT',
      'rotate-secret': 'PUT',
      
      'delete-task': 'DELETE',
      'delete-function': 'DELETE',
      'delete-secret': 'DELETE',
      'delete-trigger': 'DELETE',
    };

    return methodMap[endpoint] || 'GET';
  }
} 