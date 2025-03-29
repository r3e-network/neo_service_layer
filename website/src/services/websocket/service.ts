import { EventEmitter } from 'events';

export interface WebSocketMessage {
  type: string;
  data: any;
}

export class WebSocketService extends EventEmitter {
  private socket: WebSocket | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectDelay = 1000;
  private pingInterval: NodeJS.Timeout | null = null;
  private authenticated = false;

  constructor(private baseUrl: string) {
    super();
  }

  public connect(token: string): void {
    if (this.socket?.readyState === WebSocket.OPEN) {
      return;
    }

    try {
      this.socket = new WebSocket(`${this.baseUrl}?token=${token}`);
      this.setupEventHandlers();
      this.startPingInterval();
    } catch (error) {
      console.error('WebSocket connection error:', error);
      this.handleReconnect();
    }
  }

  public disconnect(): void {
    if (this.socket) {
      this.socket.close();
      this.socket = null;
    }
    if (this.pingInterval) {
      clearInterval(this.pingInterval);
      this.pingInterval = null;
    }
    this.authenticated = false;
  }

  public subscribe(channel: string, params?: Record<string, any>): void {
    if (!this.authenticated) {
      throw new Error('WebSocket not authenticated');
    }

    this.send({
      type: 'subscribe',
      data: {
        channel,
        params
      }
    });
  }

  public unsubscribe(channel: string): void {
    this.send({
      type: 'unsubscribe',
      data: {
        channel
      }
    });
  }

  private send(message: WebSocketMessage): void {
    if (this.socket?.readyState === WebSocket.OPEN) {
      this.socket.send(JSON.stringify(message));
    } else {
      console.warn('WebSocket not connected, message not sent:', message);
    }
  }

  private setupEventHandlers(): void {
    if (!this.socket) return;

    this.socket.onopen = () => {
      console.log('WebSocket connected');
      this.reconnectAttempts = 0;
      this.emit('connected');
    };

    this.socket.onclose = () => {
      console.log('WebSocket disconnected');
      this.handleReconnect();
      this.emit('disconnected');
    };

    this.socket.onerror = (error) => {
      console.error('WebSocket error:', error);
      this.emit('error', error);
    };

    this.socket.onmessage = (event) => {
      try {
        const message: WebSocketMessage = JSON.parse(event.data);
        
        switch (message.type) {
          case 'auth_success':
            this.authenticated = true;
            this.emit('authenticated');
            break;

          case 'auth_error':
            this.authenticated = false;
            this.emit('auth_error', message.data);
            break;

          case 'price_update':
            this.emit('price_update', message.data);
            break;

          case 'source_update':
            this.emit('source_update', message.data);
            break;

          case 'config_update':
            this.emit('config_update', message.data);
            break;

          case 'error':
            this.emit('message_error', message.data);
            break;

          default:
            this.emit('message', message);
        }
      } catch (error) {
        console.error('Error parsing WebSocket message:', error);
      }
    };
  }

  private handleReconnect(): void {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      this.emit('max_reconnect_attempts');
      return;
    }

    this.reconnectAttempts++;
    const delay = this.reconnectDelay * Math.pow(2, this.reconnectAttempts - 1);

    setTimeout(() => {
      this.connect(this.getStoredToken());
    }, delay);
  }

  private startPingInterval(): void {
    if (this.pingInterval) {
      clearInterval(this.pingInterval);
    }

    this.pingInterval = setInterval(() => {
      this.send({ type: 'ping', data: null });
    }, 30000); // Send ping every 30 seconds
  }

  private getStoredToken(): string {
    // TODO: Implement proper token storage/retrieval
    return localStorage.getItem('ws_token') || '';
  }
}

// Create singleton instance
export const websocketService = new WebSocketService(
  process.env.NEXT_PUBLIC_WS_URL || 'ws://localhost:3001'
);