export const TRIGGER_CONSTANTS = {
  MAX_TRIGGERS_PER_USER: 100,
  MIN_INTERVAL: 60, // 1 minute
  MAX_INTERVAL: 86400, // 24 hours
  DEFAULT_TIMEOUT: 30000, // 30 seconds
  MAX_RETRIES: 3,
  RETRY_DELAY: 1000, // 1 second
  WEBSOCKET_URL: 'ws://localhost:8080/api/triggers/ws',
  EVENT_TYPES: {
    BLOCK: 'block',
    TRANSACTION: 'transaction',
    CONTRACT: 'contract',
    PRICE: 'price',
    TIME: 'time'
  },
  TRIGGER_STATUS: {
    ACTIVE: 'active',
    PAUSED: 'paused',
    FAILED: 'failed',
    COMPLETED: 'completed'
  },
  WEBSOCKET_EVENTS: {
    TRIGGER_UPDATE: 'trigger_update',
    EXECUTION_UPDATE: 'execution_update'
  }
};