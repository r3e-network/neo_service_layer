export const SECRETS_CONSTANTS = {
  MAX_SECRETS_PER_USER: 100,
  MAX_SECRET_SIZE: 10240, // 10KB
  SECRET_TYPES: {
    API_KEY: 'api_key',
    PASSWORD: 'password',
    CERTIFICATE: 'certificate',
    TOKEN: 'token',
    PRIVATE_KEY: 'private_key'
  },
  PERMISSION_LEVELS: {
    READ: 'read',
    WRITE: 'write',
    ADMIN: 'admin'
  },
  ROTATION_PERIODS: {
    DAILY: 86400,
    WEEKLY: 604800,
    MONTHLY: 2592000,
    QUARTERLY: 7776000
  },
  ENCRYPTION_ALGORITHMS: {
    AES_256_GCM: 'aes-256-gcm',
    RSA_2048: 'rsa-2048'
  },
  WEBSOCKET_EVENTS: {
    SECRET_UPDATE: 'secret_update',
    PERMISSION_UPDATE: 'permission_update'
  },
  WEBSOCKET_URL: 'ws://localhost:8080/ws/secrets'
};