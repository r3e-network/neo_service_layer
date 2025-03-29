export const API_CONSTANTS = {
  VERSION: 'v1',
  RATE_LIMIT: {
    WINDOW: 60000, // 1 minute
    MAX_REQUESTS: 100
  },
  AUTH_METHODS: {
    NEO_SIGNATURE: 'neo_signature',
    API_KEY: 'api_key'
  },
  RESPONSE_FORMATS: {
    JSON: 'application/json',
    XML: 'application/xml'
  },
  HTTP_METHODS: {
    GET: 'GET',
    POST: 'POST',
    PUT: 'PUT',
    DELETE: 'DELETE'
  },
  STATUS_CODES: {
    OK: 200,
    CREATED: 201,
    BAD_REQUEST: 400,
    UNAUTHORIZED: 401,
    FORBIDDEN: 403,
    NOT_FOUND: 404,
    RATE_LIMITED: 429,
    SERVER_ERROR: 500
  },
  WEBSOCKET_EVENTS: {
    STATUS_UPDATE: 'status_update',
    USAGE_UPDATE: 'usage_update'
  }
};

// API base URL for the API service
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:3001/api';

// Refresh interval for API data in milliseconds (5 minutes)
export const REFRESH_INTERVAL = 5 * 60 * 1000;