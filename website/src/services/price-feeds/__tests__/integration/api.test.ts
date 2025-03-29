import { createServer } from 'http';
import { Server as WebSocketServer, WebSocket } from 'ws';
import { ethers } from 'ethers';
import request from 'supertest';
import { app } from '@/server/app';
import { PRICE_FEED_CONSTANTS } from '../../constants';
import { mockContract } from '../__mocks__/contract';
import { PriceSource } from '../../types/types';

describe('Price Feed API Integration', () => {
  let server: ReturnType<typeof createServer>;
  let wss: WebSocketServer;
  let mockProvider: jest.Mocked<ethers.providers.Provider>;

  beforeAll(async () => {
    server = createServer(app);
    wss = new WebSocketServer({ server });
    mockProvider = {
      getNetwork: jest.fn().mockResolvedValue({ chainId: 1 }),
      getBlockNumber: jest.fn().mockResolvedValue(1000),
    } as any;

    await new Promise<void>((resolve) => server.listen(0, resolve));
  });

  afterAll(async () => {
    await new Promise((resolve) => server.close(resolve));
    await new Promise((resolve) => wss.close(resolve));
  });

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('GET /api/price-feeds/:symbol/price', () => {
    it('should return current price data', async () => {
      const symbol = 'NEO/USD';
      const response = await request(server)
        .get(`/api/price-feeds/${symbol}/price`)
        .expect(200);

      expect(response.body).toHaveProperty('price');
      expect(response.body).toHaveProperty('timestamp');
      expect(response.body).toHaveProperty('sources');
    });

    it('should validate symbol parameter', async () => {
      await request(server)
        .get('/api/price-feeds/INVALID/price')
        .expect(400);
    });
  });

  describe('GET /api/price-feeds/:symbol/historical', () => {
    it('should return historical price data', async () => {
      const symbol = 'NEO/USD';
      const response = await request(server)
        .get(`/api/price-feeds/${symbol}/historical`)
        .query({ timeframe: '24h' })
        .expect(200);

      expect(Array.isArray(response.body.prices)).toBeTruthy();
      expect(response.body.prices[0]).toHaveProperty('price');
      expect(response.body.prices[0]).toHaveProperty('timestamp');
    });

    it('should handle invalid timeframe parameter', async () => {
      await request(server)
        .get('/api/price-feeds/NEO/USD/historical')
        .query({ timeframe: 'invalid' })
        .expect(400);
    });
  });

  describe('GET /api/price-feeds/:symbol/sources', () => {
    it('should return price source data', async () => {
      const symbol = 'NEO/USD';
      const response = await request(server)
        .get(`/api/price-feeds/${symbol}/sources`)
        .expect(200);

      expect(Array.isArray(response.body.sources)).toBeTruthy();
      response.body.sources.forEach((source: PriceSource) => {
        expect(source).toHaveProperty('id');
        expect(source).toHaveProperty('currentPrice');
        expect(source).toHaveProperty('status');
      });
    });
  });

  describe('PUT /api/price-feeds/:symbol/config', () => {
    const validConfig = {
      deviationThreshold: 0.02,
      minSourceCount: 4,
      customSourceWeights: {
        source1: 0.5,
        source2: 0.5
      }
    };

    it('should update configuration with valid data', async () => {
      const symbol = 'NEO/USD';
      const response = await request(server)
        .put(`/api/price-feeds/${symbol}/config`)
        .send(validConfig)
        .expect(200);

      expect(response.body.success).toBeTruthy();
    });

    it('should reject invalid configuration', async () => {
      const symbol = 'NEO/USD';
      const invalidConfig = {
        deviationThreshold: -1,
        minSourceCount: 0
      };

      const response = await request(server)
        .put(`/api/price-feeds/${symbol}/config`)
        .send(invalidConfig)
        .expect(400);

      expect(response.body.errors).toBeDefined();
    });

    it('should require authentication', async () => {
      const symbol = 'NEO/USD';
      await request(server)
        .put(`/api/price-feeds/${symbol}/config`)
        .send(validConfig)
        .expect(401);
    });
  });

  describe('WebSocket Integration', () => {
    it('should broadcast price updates to connected clients', (done) => {
      const ws = new WebSocket(`ws://localhost:${(server.address() as any).port}`);
      const symbol = 'NEO/USD';
      const price = 100;

      ws.addEventListener('open', () => {
        // Subscribe to price updates
        ws.send(JSON.stringify({
          type: 'subscribe',
          channel: `price_feed_${symbol}`
        }));

        // Simulate price update
        mockContract.emit('PriceUpdated', symbol, price);
      });

      ws.addEventListener('message', (event) => {
        const message = JSON.parse(event.data.toString());
        expect(message.type).toBe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.PRICE_UPDATE);
        expect(message.data.symbol).toBe(symbol);
        expect(message.data.price).toBe(price);
        ws.close();
        done();
      });
    });

    it('should handle multiple subscriptions', (done) => {
      const ws = new WebSocket(`ws://localhost:${(server.address() as any).port}`);
      const symbols = ['NEO/USD', 'GAS/USD'];
      let receivedUpdates = 0;

      ws.addEventListener('open', () => {
        // Subscribe to multiple symbols
        symbols.forEach(symbol => {
          ws.send(JSON.stringify({
            type: 'subscribe',
            channel: `price_feed_${symbol}`
          }));
        });

        // Simulate updates for both symbols
        symbols.forEach(symbol => {
          mockContract.emit('PriceUpdated', symbol, 100);
        });
      });

      ws.addEventListener('message', () => {
        receivedUpdates++;
        if (receivedUpdates === symbols.length) {
          ws.close();
          done();
        }
      });
    });

    it('should handle reconnection and maintain subscriptions', (done) => {
      let ws = new WebSocket(`ws://localhost:${(server.address() as any).port}`);
      const symbol = 'NEO/USD';
      let reconnected = false;

      ws.addEventListener('open', () => {
        if (!reconnected) {
          ws.send(JSON.stringify({
            type: 'subscribe',
            channel: `price_feed_${symbol}`
          }));

          // Force disconnect
          ws.close();
          reconnected = true;

          // Reconnect
          setTimeout(() => {
            ws = new WebSocket(`ws://localhost:${(server.address() as any).port}`);
            
            ws.addEventListener('open', () => {
              // Should receive updates without resubscribing
              mockContract.emit('PriceUpdated', symbol, 100);
            });

            ws.addEventListener('message', (event) => {
              const message = JSON.parse(event.data.toString());
              expect(message.type).toBe(PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.PRICE_UPDATE);
              ws.close();
              done();
            });
          }, 100);
        }
      });
    });
  });

  describe('Blockchain Integration', () => {
    it('should sync on-chain prices with API', async () => {
      const symbol = 'NEO/USD';
      const price = 100;

      // Update price on-chain
      await mockContract.updatePrice(
        symbol,
        ethers.parseUnits(price.toString(), PRICE_FEED_CONSTANTS.PRICE_DECIMALS),
        Date.now(),
        '0xhash',
        '0xsignature'
      );

      // Verify API returns updated price
      const response = await request(server)
        .get(`/api/price-feeds/${symbol}/price`)
        .expect(200);

      expect(response.body.price).toBe(price);
    });

    it('should handle blockchain reorgs', async () => {
      const symbol = 'NEO/USD';
      const price1 = 100;
      const price2 = 110;

      // Simulate chain reorg
      mockProvider.getBlockNumber.mockResolvedValueOnce(1000)
        .mockResolvedValueOnce(999); // Block number decreased

      // Update price twice
      await mockContract.updatePrice(
        symbol,
        ethers.parseUnits(price1.toString(), PRICE_FEED_CONSTANTS.PRICE_DECIMALS),
        Date.now(),
        '0xhash1',
        '0xsignature1'
      );

      await mockContract.updatePrice(
        symbol,
        ethers.parseUnits(price2.toString(), PRICE_FEED_CONSTANTS.PRICE_DECIMALS),
        Date.now(),
        '0xhash2',
        '0xsignature2'
      );

      // Verify API handles reorg gracefully
      const response = await request(server)
        .get(`/api/price-feeds/${symbol}/price`)
        .expect(200);

      expect(response.body.price).toBe(price2);
    });
  });
});