export const METRICS_CONSTANTS = {
  COLLECTION_INTERVAL: 15000, // 15 seconds
  REFRESH_INTERVAL: 60000, // 1 minute
  RETENTION_PERIOD: 2592000, // 30 days
  MAX_DATAPOINTS: 1000,
  AGGREGATION_INTERVALS: {
    MINUTE: 60,
    HOUR: 3600,
    DAY: 86400
  },
  METRIC_TYPES: {
    COUNTER: 'counter',
    GAUGE: 'gauge',
    HISTOGRAM: 'histogram'
  },
  ALERT_LEVELS: {
    INFO: 'info',
    WARNING: 'warning',
    ERROR: 'error',
    CRITICAL: 'critical'
  },
  WEBSOCKET_EVENTS: {
    METRIC_UPDATE: 'metric_update',
    ALERT_UPDATE: 'alert_update'
  }
};

export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:3001/api';