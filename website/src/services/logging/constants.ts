export const LOGGING_CONSTANTS = {
  LOG_LEVELS: {
    ERROR: 'error',
    WARN: 'warn',
    INFO: 'info',
    DEBUG: 'debug',
    TRACE: 'trace'
  },
  MAX_LOG_SIZE: 10485760, // 10MB
  MAX_LOG_FILES: 10,
  RETENTION_DAYS: 30,
  LOG_ROTATION_INTERVAL: 86400000, // 24 hours
  SEARCH_LIMIT: 1000,
  LOG_SOURCES: {
    SYSTEM: 'system',
    PRICE_FEED: 'price_feed',
    GAS_BANK: 'gas_bank',
    TRIGGER: 'trigger',
    FUNCTION: 'function',
    USER: 'user'
  },
  WEBSOCKET_EVENTS: {
    LOG_UPDATE: 'log_update',
    LEVEL_UPDATE: 'level_update'
  }
};

export const DEFAULT_LOG_LIMIT = 100;
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:3001/api';