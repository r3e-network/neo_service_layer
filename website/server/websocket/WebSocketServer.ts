import { Server as HTTPServer } from 'http';
import { WebSocket, WebSocketServer as WSServer } from 'ws';
import { EventEmitter } from 'events';
import { CryptoUtils } from '@/utils/crypto';
import { PriceFeedService } from '@/services/price-feeds';
import { Logger } from '@/utils/logger';
import { MetricsService } from '@/services/metrics';

// Define a type that extends WebSocket with our custom properties
type WebSocketClient = WebSocket & {
  id: string;
  isAlive: boolean;
  subscriptions: Set<string>;
  address?: string;
};

interface WebSocketMessage {
  type: string;
  data: any;
}

export class WebSocketServer extends EventEmitter {
  private wss: WSServer;
  private clients: Map<string, WebSocketClient> = new Map();
  private logger: Logger;
  private metrics: MetricsService;
  private heartbeatInterval: NodeJS.Timeout;
  private readonly HEARTBEAT_INTERVAL = 30000;
  private readonly CLIENT_TIMEOUT = 35000;

  constructor(
    server: HTTPServer,
    private priceFeedService: PriceFeedService
  ) {
    super();
    this.logger = Logger.getInstance().child({ service: 'websocket-server' });
    this.metrics = new MetricsService({
      namespace: 'neo_service_layer',
      subsystem: 'websocket_server'
    });

    this.wss = new WSServer({ server });
    this.setupWebSocketServer();
    this.startHeartbeat();
    this.setupPriceFeedEvents();
  }

  private setupWebSocketServer(): void {
    this.wss.on('connection', async (ws: WebSocketClient, request) => {
      const clientId = this.generateClientId();
      (ws as any).id = clientId;
      (ws as any).isAlive = true;
      (ws as any).subscriptions = new Set();

      this.clients.set(clientId, ws);
      this.metrics.incrementCounter('websocket_connections_total');
      this.metrics.setGauge('websocket_connections_active', this.clients.size);

      this.logger.info('Client connected', { clientId });

      // Handle authentication
      const token = this.extractToken(request.url);
      if (token) {
        try {
          const authResult = await this.authenticateClient(token, ws);
          if (authResult) {
            this.sendToClient(ws, {
              type: 'auth_success',
              data: { address: (ws as any).address }
            });
          } else {
            this.sendToClient(ws, {
              type: 'auth_error',
              data: { message: 'Authentication failed' }
            });
            ws.close();
            return;
          }
        } catch (error) {
          this.logger.error('Authentication error', { error, clientId });
          ws.close();
          return;
        }
      }

      ws.on('message', async (data: string) => {
        try {
          const message: WebSocketMessage = JSON.parse(data);
          await this.handleMessage(ws, message);
        } catch (error) {
          this.logger.error('Message handling error', { error, clientId });
          this.sendToClient(ws, {
            type: 'error',
            data: { message: 'Invalid message format' }
          });
        }
      });

      ws.on('pong', () => {
        (ws as any).isAlive = true;
      });

      ws.on('close', () => {
        this.handleClientDisconnect(ws);
      });

      ws.on('error', (error) => {
        this.logger.error('WebSocket error', { error, clientId });
        this.handleClientDisconnect(ws);
      });
    });
  }

  private async handleMessage(client: WebSocketClient, message: WebSocketMessage): Promise<void> {
    const { type, data } = message;

    switch (type) {
      case 'subscribe':
        await this.handleSubscribe(client, data);
        break;

      case 'unsubscribe':
        await this.handleUnsubscribe(client, data);
        break;

      case 'ping':
        this.sendToClient(client, { type: 'pong', data: null });
        break;

      default:
        this.sendToClient(client, {
          type: 'error',
          data: { message: 'Unknown message type' }
        });
    }
  }

  private async handleSubscribe(client: WebSocketClient, data: any): Promise<void> {
    const { channel, params } = data;

    if (!channel) {
      this.sendToClient(client, {
        type: 'error',
        data: { message: 'Channel is required' }
      });
      return;
    }

    // Validate subscription permissions
    if (!await this.validateSubscription(client, channel, params)) {
      this.sendToClient(client, {
        type: 'error',
        data: { message: 'Subscription not allowed' }
      });
      return;
    }

    (client as any).subscriptions.add(channel);
    this.metrics.incrementCounter('websocket_subscriptions_total', {
      channel
    });

    this.sendToClient(client, {
      type: 'subscribed',
      data: { channel }
    });

    // Send initial data if available
    if (channel === 'price_updates' && params?.symbol) {
      try {
        const priceData = await this.priceFeedService.getAggregatedPrice(params.symbol);
        this.sendToClient(client, {
          type: 'price_update',
          data: priceData
        });
      } catch (error) {
        this.logger.error('Error fetching initial price data', { error });
      }
    }
  }

  private async handleUnsubscribe(client: WebSocketClient, data: any): Promise<void> {
    const { channel } = data;

    if (!channel) {
      this.sendToClient(client, {
        type: 'error',
        data: { message: 'Channel is required' }
      });
      return;
    }

    (client as any).subscriptions.delete(channel);
    this.metrics.decrementCounter('websocket_subscriptions_total', {
      channel
    });

    this.sendToClient(client, {
      type: 'unsubscribed',
      data: { channel }
    });
  }

  private setupPriceFeedEvents(): void {
    // Subscribe to price feed events
    this.priceFeedService.on('price_update', (data) => {
      this.broadcast('price_updates', {
        type: 'price_update',
        data
      });
    });

    this.priceFeedService.on('source_update', (data) => {
      this.broadcast('source_updates', {
        type: 'source_update',
        data
      });
    });

    this.priceFeedService.on('config_update', (data) => {
      this.broadcast('config_updates', {
        type: 'config_update',
        data
      });
    });
  }

  private broadcast(channel: string, message: WebSocketMessage): void {
    this.clients.forEach((client) => {
      if ((client as any).subscriptions.has(channel)) {
        this.sendToClient(client, message);
      }
    });
  }

  private sendToClient(client: WebSocketClient, message: WebSocketMessage): void {
    if (client.readyState === WebSocket.OPEN) {
      try {
        client.send(JSON.stringify(message));
        this.metrics.incrementCounter('websocket_messages_sent_total');
      } catch (error) {
        this.logger.error('Error sending message to client', {
          error,
          clientId: (client as any).id
        });
      }
    }
  }

  private startHeartbeat(): void {
    this.heartbeatInterval = setInterval(() => {
      this.clients.forEach((client) => {
        if (!(client as any).isAlive) {
          this.logger.warn('Client timeout', { clientId: (client as any).id });
          client.terminate();
          return;
        }

        (client as any).isAlive = false;
        client.ping();
      });
    }, this.HEARTBEAT_INTERVAL);
  }

  private handleClientDisconnect(client: WebSocketClient): void {
    this.clients.delete((client as any).id);
    this.metrics.decrementCounter('websocket_connections_total');
    this.metrics.setGauge('websocket_connections_active', this.clients.size);
    this.logger.info('Client disconnected', { clientId: (client as any).id });
  }

  private async authenticateClient(token: string, client: WebSocketClient): Promise<boolean> {
    try {
      // Verify JWT token
      const decoded = await this.verifyToken(token);
      
      if (!decoded || !decoded.address) {
        return false;
      }

      (client as any).address = decoded.address;
      return true;
    } catch (error) {
      this.logger.error('Token verification failed', { error });
      return false;
    }
  }

  private async validateSubscription(
    client: WebSocketClient,
    channel: string,
    params?: any
  ): Promise<boolean> {
    // Implement channel-specific validation logic
    switch (channel) {
      case 'price_updates':
        return true; // Public channel

      case 'config_updates':
        return Boolean((client as any).address); // Requires authentication

      case 'source_updates':
        return Boolean((client as any).address); // Requires authentication

      default:
        return false;
    }
  }

  private async verifyToken(token: string): Promise<any> {
    // TODO: Implement proper JWT verification using CryptoUtils
    return CryptoUtils.verifyToken(token);
  }

  private generateClientId(): string {
    return `client_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private extractToken(url: string | undefined): string | null {
    if (!url) return null;
    const params = new URLSearchParams(url.split('?')[1]);
    return params.get('token');
  }

  public shutdown(): void {
    clearInterval(this.heartbeatInterval);
    this.wss.close();
    this.logger.info('WebSocket server shutdown');
  }
}