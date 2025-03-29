# Neo N3 Service Layer API Documentation

## Overview

The Neo N3 Service Layer provides a RESTful API for interacting with various services including functions, triggers, gas bank, price feeds, and secrets management. This document provides detailed information about available endpoints, authentication, and usage examples.

## Authentication

All API endpoints (except health check and authentication) require JWT authentication. To authenticate:

1. Sign a message with your Neo N3 wallet
2. Send the signed message to the authentication endpoint
3. Use the returned JWT token in subsequent requests

```bash
# Example authentication request
curl -X POST https://api.neo-service-layer.io/v1/auth/verify \
  -H "Content-Type: application/json" \
  -d '{
    "address": "NXxx...xxxx",
    "message": "Login to Neo Service Layer at 2024-03-27T00:00:00Z",
    "signature": "0x..."
  }'
```

Include the JWT token in subsequent requests:

```bash
curl -X GET https://api.neo-service-layer.io/v1/functions \
  -H "Authorization: Bearer eyJ..."
```

## API Endpoints

### Functions Service

#### List Functions
```http
GET /v1/functions
```

Query Parameters:
- `page` (optional): Page number (default: 1)
- `limit` (optional): Items per page (default: 10)
- `status` (optional): Filter by status (active, inactive)

Response:
```json
{
  "functions": [
    {
      "id": "func_123",
      "name": "price_alert",
      "description": "NEO price alert function",
      "runtime": "javascript",
      "created_at": "2024-03-27T00:00:00Z",
      "status": "active"
    }
  ],
  "pagination": {
    "total": 100,
    "page": 1,
    "limit": 10
  }
}
```

#### Create Function
```http
POST /v1/functions
```

Request Body:
```json
{
  "name": "price_alert",
  "description": "NEO price alert function",
  "runtime": "javascript",
  "code": "function handler(context, event) { ... }",
  "environment": {
    "ALERT_THRESHOLD": "100"
  }
}
```

### Triggers Service

#### List Triggers
```http
GET /v1/triggers
```

Query Parameters:
- `page` (optional): Page number (default: 1)
- `limit` (optional): Items per page (default: 10)
- `status` (optional): Filter by status (active, inactive)
- `type` (optional): Filter by type (schedule, event, condition)

Response:
```json
{
  "triggers": [
    {
      "id": "trig_123",
      "name": "daily_price_check",
      "function_id": "func_123",
      "type": "schedule",
      "schedule": "0 0 * * *",
      "status": "active"
    }
  ],
  "pagination": {
    "total": 100,
    "page": 1,
    "limit": 10
  }
}
```

### Gas Bank Service

#### Get Gas Balance
```http
GET /v1/gas/balance
```

Response:
```json
{
  "address": "NXxx...xxxx",
  "balance": "1000000",
  "allocated": "500000",
  "available": "500000",
  "last_refill": "2024-03-27T00:00:00Z"
}
```

#### Request Gas
```http
POST /v1/gas/request
```

Request Body:
```json
{
  "amount": "100000"
}
```

### Price Feed Service

#### Get Current Price
```http
GET /v1/prices/{symbol}
```

Path Parameters:
- `symbol`: Trading pair (e.g., NEO/USD)

Response:
```json
{
  "symbol": "NEO/USD",
  "price": "50.00",
  "timestamp": "2024-03-27T00:00:00Z",
  "sources": [
    {
      "name": "binance",
      "price": "50.00",
      "weight": 1.0
    }
  ]
}
```

### Secrets Service

#### List Secrets
```http
GET /v1/secrets
```

Response:
```json
{
  "secrets": [
    {
      "id": "sec_123",
      "name": "api_key",
      "created_at": "2024-03-27T00:00:00Z",
      "last_accessed": "2024-03-27T00:00:00Z"
    }
  ]
}
```

#### Create Secret
```http
POST /v1/secrets
```

Request Body:
```json
{
  "name": "api_key",
  "value": "secret_value",
  "description": "API key for external service"
}
```

## Error Handling

The API uses standard HTTP status codes and returns errors in the following format:

```json
{
  "error": {
    "code": "INVALID_REQUEST",
    "message": "Invalid request parameters",
    "details": {
      "field": "amount",
      "reason": "must be a positive number"
    }
  }
}
```

Common Error Codes:
- `400 Bad Request`: Invalid request parameters
- `401 Unauthorized`: Missing or invalid authentication
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: Server error

## Rate Limiting

API endpoints are rate limited to protect the service. Rate limits are specified in the response headers:

```
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 59
X-RateLimit-Reset: 1616799600
```

## Versioning

The API is versioned through the URL path (e.g., `/v1/`). When breaking changes are necessary, a new API version will be released, and the old version will be maintained for a deprecation period.

## WebSocket API

For real-time updates, connect to our WebSocket API:

```javascript
const ws = new WebSocket('wss://api.neo-service-layer.io/v1/ws');

ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  console.log('Received:', data);
};

// Subscribe to price updates
ws.send(JSON.stringify({
  type: 'subscribe',
  channel: 'prices',
  symbols: ['NEO/USD']
}));
```

Available WebSocket channels:
- `prices`: Real-time price updates
- `triggers`: Trigger execution events
- `gas`: Gas pool updates
- `functions`: Function execution events

## SDK Support

Official SDKs are available for:
- JavaScript/TypeScript: [@neo-service/sdk](https://github.com/will/neo-service-sdk-js)
- Python: [neo-service-sdk](https://github.com/will/neo-service-sdk-python)
- Go: [neo-service-sdk-go](https://github.com/will/neo-service-sdk-go)

## Support

For API support:
- Email: api@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/will/neo_service_layer/issues)
