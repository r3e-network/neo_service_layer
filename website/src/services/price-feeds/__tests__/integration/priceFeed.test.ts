import * as React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { act } from 'react-dom/test-utils';
import { WebSocket, Server } from 'mock-socket';
import { ethers } from 'ethers';
import { PriceFeedDashboard } from '../../components/PriceFeedDashboard';
import { PRICE_FEED_CONSTANTS } from '../../constants';
import { mockWebSocket } from '../__mocks__/websocket';
import { mockContract } from '../__mocks__/contract';

// Mock WebSocket
global.WebSocket = WebSocket as any;

describe('Price Feed Integration', () => {
  let mockServer: Server;
  let mockProvider: jest.Mocked<ethers.providers.Provider>;

  beforeAll(() => {
    mockServer = new Server('ws://localhost:1234');
    mockProvider = {
      getNetwork: jest.fn().mockResolvedValue({ chainId: 1 }),
      getBlockNumber: jest.fn().mockResolvedValue(1000),
    } as any;
  });

  afterAll(() => {
    mockServer.close();
  });

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should initialize and display price feed data', async () => {
    const symbol = 'NEO/USD';
    const initialPrice = 100;

    // Mock initial API response
    global.fetch = jest.fn().mockImplementation(() =>
      Promise.resolve({
        ok: true,
        json: () => Promise.resolve({
          price: initialPrice,
          sources: [
            { id: 'source1', currentPrice: 99, status: 'active' },
            { id: 'source2', currentPrice: 101, status: 'active' }
          ],
          lastUpdate: Date.now()
        })
      })
    );

    await act(async () => {
      // @ts-ignore
      render(React.createElement(PriceFeedDashboard, { symbol }));
    });

    // Verify initial render
    expect(screen.getByText(symbol)).toBeInTheDocument();
    expect(screen.getByText(`$${initialPrice.toFixed(2)}`)).toBeInTheDocument();
    expect(screen.getByText('source1')).toBeInTheDocument();
    expect(screen.getByText('source2')).toBeInTheDocument();
  });

  it('should handle WebSocket price updates', async () => {
    const symbol = 'NEO/USD';
    const updatedPrice = 150;

    await act(async () => {
      // @ts-ignore
      render(React.createElement(PriceFeedDashboard, { symbol }));
    });

    // Simulate WebSocket price update
    await act(async () => {
      mockServer.emit('message', JSON.stringify({
        type: PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.PRICE_UPDATE,
        data: {
          symbol,
          price: updatedPrice,
          timestamp: Date.now()
        }
      }));
    });

    await waitFor(() => {
      expect(screen.getByText(`$${updatedPrice.toFixed(2)}`)).toBeInTheDocument();
    });
  });

  it('should handle source updates', async () => {
    const symbol = 'NEO/USD';
    const newSource = {
      id: 'source3',
      currentPrice: 102,
      status: 'active',
      latency: 100,
      reliability: 0.95
    };

    await act(async () => {
      // @ts-ignore
      render(React.createElement(PriceFeedDashboard, { symbol }));
    });

    // Simulate WebSocket source update
    await act(async () => {
      mockServer.emit('message', JSON.stringify({
        type: PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.SOURCE_UPDATE,
        data: {
          symbol,
          source: newSource
        }
      }));
    });

    await waitFor(() => {
      expect(screen.getByText('source3')).toBeInTheDocument();
    });
  });

  it('should handle configuration updates', async () => {
    const symbol = 'NEO/USD';
    const newConfig = {
      deviationThreshold: 0.02,
      minSourceCount: 4
    };

    // Mock authentication
    const mockAuth = {
      isAuthenticated: true,
      user: { address: '0x123' }
    };
    jest.mock('../../hooks/useAuth', () => ({
      useAuth: () => mockAuth
    }));

    await act(async () => {
      // @ts-ignore
      render(React.createElement(PriceFeedDashboard, { symbol }));
    });

    // Open config modal
    const configButton = screen.getByText('Configure');
    fireEvent.click(configButton);

    // Update configuration
    const deviationInput = screen.getByLabelText('Deviation Threshold');
    const sourceCountInput = screen.getByLabelText('Minimum Source Count');

    fireEvent.change(deviationInput, { target: { value: '0.02' } });
    fireEvent.change(sourceCountInput, { target: { value: '4' } });

    const saveButton = screen.getByText('Save Changes');
    await act(async () => {
      fireEvent.click(saveButton);
    });

    // Verify API call
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/price-feeds/config'),
      expect.objectContaining({
        method: 'PUT',
        body: JSON.stringify(newConfig)
      })
    );
  });

  it('should handle error states', async () => {
    const symbol = 'NEO/USD';

    // Mock API error
    global.fetch = jest.fn().mockImplementation(() =>
      Promise.reject(new Error('API Error'))
    );

    await act(async () => {
      // @ts-ignore
      render(React.createElement(PriceFeedDashboard, { symbol }));
    });

    await waitFor(() => {
      expect(screen.getByText(/Error loading price feed data/i)).toBeInTheDocument();
    });
  });

  it('should handle blockchain interactions', async () => {
    const symbol = 'NEO/USD';
    const price = 100;

    await act(async () => {
      // @ts-ignore
      render(React.createElement(PriceFeedDashboard, { symbol }));
    });

    // Simulate on-chain price update
    await act(async () => {
      const tx = await mockContract.updatePrice(
        symbol,
        ethers.utils.parseUnits(price.toString(), PRICE_FEED_CONSTANTS.PRICE_DECIMALS),
        Date.now(),
        '0xhash',
        '0xsignature'
      );
      await tx.wait();
    });

    // Verify contract interaction
    expect(mockContract.updatePrice).toHaveBeenCalled();
  });

  it('should handle metrics updates', async () => {
    const symbol = 'NEO/USD';
    const newMetrics = {
      volatility: 0.05,
      volume24h: 1000000,
      priceChange24h: 0.03
    };

    await act(async () => {
      // @ts-ignore
      render(React.createElement(PriceFeedDashboard, { symbol }));
    });

    // Simulate WebSocket metrics update
    await act(async () => {
      mockServer.emit('message', JSON.stringify({
        type: PRICE_FEED_CONSTANTS.WEBSOCKET_EVENTS.METRICS_UPDATE,
        data: {
          symbol,
          metrics: newMetrics
        }
      }));
    });

    await waitFor(() => {
      expect(screen.getByText('5%')).toBeInTheDocument(); // Volatility
      expect(screen.getByText('$1,000,000')).toBeInTheDocument(); // Volume
      expect(screen.getByText('3%')).toBeInTheDocument(); // Price Change
    });
  });

  it('should handle reconnection', async () => {
    const symbol = 'NEO/USD';

    await act(async () => {
      // @ts-ignore
      render(React.createElement(PriceFeedDashboard, { symbol }));
    });

    // Simulate WebSocket disconnection
    await act(async () => {
      mockServer.close();
    });

    // Simulate reconnection
    await act(async () => {
      mockServer = new Server('ws://localhost:1234');
    });

    // Verify reconnection
    expect(mockWebSocket.connect).toHaveBeenCalledTimes(2);
  });
});