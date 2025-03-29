import { ethers } from 'ethers';
import { EventEmitter } from 'events';

class MockContract extends EventEmitter {
  private prices: { [symbol: string]: string } = {};

  async updatePrice(
    symbol: string,
    price: bigint,
    timestamp: number,
    sourcesHash: string,
    signature: string
  ) {
    this.prices[symbol] = price.toString();
    this.emit('PriceUpdated', symbol, price);
    return {
      wait: async () => ({
        status: 1,
        events: [{
          event: 'PriceUpdated',
          args: [symbol, price, timestamp, sourcesHash]
        }]
      })
    };
  }

  async getPrice(symbol: string) {
    return this.prices[symbol] ? BigInt(this.prices[symbol]) : BigInt(0);
  }

  // Mock contract interface methods
  interface = {
    events: {
      PriceUpdated: {
        name: 'PriceUpdated',
        inputs: [
          { name: 'symbol', type: 'string' },
          { name: 'price', type: 'uint256' },
          { name: 'timestamp', type: 'uint256' },
          { name: 'sourcesHash', type: 'bytes32' }
        ]
      }
    },
    functions: {
      updatePrice: {
        name: 'updatePrice',
        inputs: [
          { name: 'symbol', type: 'string' },
          { name: 'price', type: 'uint256' },
          { name: 'timestamp', type: 'uint256' },
          { name: 'sourcesHash', type: 'bytes32' },
          { name: 'signature', type: 'bytes' }
        ]
      },
      getPrice: {
        name: 'getPrice',
        inputs: [
          { name: 'symbol', type: 'string' }
        ]
      }
    }
  };
}

export const mockContract = new MockContract();