export const FUNCTIONS_CONSTANTS = {
  MAX_FUNCTIONS_PER_USER: 100,
  MAX_CODE_SIZE: 102400, // 100KB
  EXECUTION_TIMEOUT: 30000, // 30 seconds
  MAX_MEMORY: 256, // 256MB
  MAX_CPU: 1, // 1 CPU core
  RUNTIME_ENVIRONMENTS: {
    NODE_16: 'node16',
    PYTHON_3_9: 'python3.9',
    GO_1_17: 'go1.17'
  },
  FUNCTION_STATUS: {
    DRAFT: 'draft',
    PUBLISHED: 'published',
    ARCHIVED: 'archived',
    FAILED: 'failed'
  },
  EXECUTION_STATUS: {
    PENDING: 'pending',
    RUNNING: 'running',
    COMPLETED: 'completed',
    FAILED: 'failed',
    TIMEOUT: 'timeout'
  },
  WEBSOCKET_EVENTS: {
    FUNCTION_UPDATE: 'function_update',
    EXECUTION_UPDATE: 'execution_update',
    LOG_UPDATE: 'log_update'
  }
};

// API base URL for the functions service
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:3001/api';

// Refresh interval for polling data (in milliseconds)
export const REFRESH_INTERVAL = 30000; // 30 seconds