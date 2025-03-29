export const GAS_BANK_CONSTANTS = {
  MIN_GAS_BALANCE: '1000000000', // 10 GAS (8 decimals)
  MAX_GAS_PER_REQUEST: '100000000', // 1 GAS
  REFILL_THRESHOLD: '5000000000', // 50 GAS
  GAS_PRICE_UPDATE_INTERVAL: 60000, // 1 minute
  MAX_REQUESTS_PER_MINUTE: 60,
  DEFAULT_GAS_LIMIT: 20,
  NETWORK_FEE_ADJUSTMENT: 1.1, // 10% buffer
  SUPPORTED_NETWORKS: ['MainNet', 'TestNet'],
  WEBSOCKET_EVENTS: {
    BALANCE_UPDATE: 'balance_update',
    GAS_PRICE_UPDATE: 'gas_price_update',
    USAGE_UPDATE: 'usage_update'
  }
};

// API base URL for the gas bank service
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:3001/api';

// Refresh interval for polling data (in milliseconds)
export const REFRESH_INTERVAL = 30000; // 30 seconds