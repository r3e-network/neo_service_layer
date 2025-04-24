# Neo Service Layer API Documentation

This document provides detailed information about the Neo Service Layer API.

## Authentication

The Neo Service Layer API uses JWT (JSON Web Token) for authentication.

### Obtaining a Token

To obtain a token, send a POST request to the `/api/auth/login` endpoint with the following payload:

```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

The response will include a JWT token:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2023-04-20T12:34:56.789Z"
}
```

### Using the Token

Include the token in the `Authorization` header of subsequent requests:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## API Endpoints

### Account Service

#### Register Account

```
POST /api/accounts/register
```

Request:
```json
{
  "email": "user@example.com",
  "password": "password123",
  "firstName": "John",
  "lastName": "Doe"
}
```

Response:
```json
{
  "id": "1234567890",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "createdAt": "2023-04-20T12:34:56.789Z"
}
```

#### Authenticate Account

```
POST /api/accounts/authenticate
```

Request:
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

Response:
```json
{
  "id": "1234567890",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2023-04-20T12:34:56.789Z"
}
```

#### Change Password

```
POST /api/accounts/change-password
```

Request:
```json
{
  "currentPassword": "password123",
  "newPassword": "newPassword123"
}
```

Response:
```json
{
  "success": true,
  "message": "Password changed successfully"
}
```

#### Verify Account

```
POST /api/accounts/verify
```

Request:
```json
{
  "verificationCode": "123456"
}
```

Response:
```json
{
  "success": true,
  "message": "Account verified successfully"
}
```

### Wallet Service

#### Create Wallet

```
POST /api/wallets
```

Request:
```json
{
  "name": "My Wallet",
  "password": "walletPassword123",
  "tags": {
    "type": "personal"
  }
}
```

Response:
```json
{
  "id": "1234567890",
  "name": "My Wallet",
  "address": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "scriptHash": "0x1234567890abcdef1234567890abcdef12345678",
  "publicKey": "02a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2",
  "tags": {
    "type": "personal"
  },
  "createdAt": "2023-04-20T12:34:56.789Z"
}
```

#### Import Wallet from WIF

```
POST /api/wallets/import
```

Request:
```json
{
  "name": "Imported Wallet",
  "wif": "KwDiBf89QgGbjEhKnhXJuH7LrciVrZi3qYjgd9M7rFU73sVHnoWn",
  "password": "walletPassword123",
  "tags": {
    "type": "imported"
  }
}
```

Response:
```json
{
  "id": "1234567890",
  "name": "Imported Wallet",
  "address": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "scriptHash": "0x1234567890abcdef1234567890abcdef12345678",
  "publicKey": "02a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2",
  "tags": {
    "type": "imported"
  },
  "createdAt": "2023-04-20T12:34:56.789Z"
}
```

#### Sign Data

```
POST /api/wallets/{walletId}/sign
```

Request:
```json
{
  "data": "SGVsbG8gV29ybGQ=", // Base64-encoded data
  "password": "walletPassword123"
}
```

Response:
```json
{
  "walletId": "1234567890",
  "address": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "signature": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"
}
```

#### Transfer NEO

```
POST /api/wallets/{walletId}/transfer/neo
```

Request:
```json
{
  "toAddress": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "amount": 10.0,
  "password": "walletPassword123",
  "network": "TestNet"
}
```

Response:
```json
{
  "walletId": "1234567890",
  "fromAddress": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "toAddress": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "amount": 10.0,
  "transactionHash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"
}
```

#### Transfer GAS

```
POST /api/wallets/{walletId}/transfer/gas
```

Request:
```json
{
  "toAddress": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "amount": 5.5,
  "password": "walletPassword123",
  "network": "TestNet"
}
```

Response:
```json
{
  "walletId": "1234567890",
  "fromAddress": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "toAddress": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "amount": 5.5,
  "transactionHash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"
}
```

#### Transfer Token

```
POST /api/wallets/{walletId}/transfer/token
```

Request:
```json
{
  "toAddress": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "tokenScriptHash": "0x1234567890abcdef1234567890abcdef12345678",
  "amount": 100.0,
  "password": "walletPassword123",
  "network": "TestNet"
}
```

Response:
```json
{
  "walletId": "1234567890",
  "fromAddress": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "toAddress": "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
  "tokenScriptHash": "0x1234567890abcdef1234567890abcdef12345678",
  "amount": 100.0,
  "transactionHash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"
}
```

### Secrets Service

#### Create Secret

```
POST /api/secrets
```

Request:
```json
{
  "name": "API Key",
  "description": "API key for external service",
  "value": "secret-api-key-123",
  "allowedFunctionIds": ["1234567890"],
  "tags": {
    "type": "api-key"
  },
  "rotationPeriod": 90
}
```

Response:
```json
{
  "id": "1234567890",
  "name": "API Key",
  "description": "API key for external service",
  "allowedFunctionIds": ["1234567890"],
  "tags": {
    "type": "api-key"
  },
  "version": 1,
  "createdAt": "2023-04-20T12:34:56.789Z",
  "updatedAt": "2023-04-20T12:34:56.789Z",
  "lastRotatedAt": "2023-04-20T12:34:56.789Z",
  "nextRotationAt": "2023-07-19T12:34:56.789Z",
  "rotationPeriod": 90
}
```

#### Get Secret Value

```
GET /api/secrets/{secretId}/value
```

Response:
```json
{
  "id": "1234567890",
  "name": "API Key",
  "value": "secret-api-key-123",
  "version": 1
}
```

#### Update Secret Value

```
PUT /api/secrets/{secretId}/value
```

Request:
```json
{
  "value": "new-secret-api-key-123",
  "description": "Updated API key for external service",
  "allowedFunctionIds": ["1234567890", "0987654321"],
  "tags": {
    "type": "api-key",
    "environment": "production"
  },
  "rotationPeriod": 60
}
```

Response:
```json
{
  "id": "1234567890",
  "name": "API Key",
  "description": "Updated API key for external service",
  "allowedFunctionIds": ["1234567890", "0987654321"],
  "tags": {
    "type": "api-key",
    "environment": "production"
  },
  "version": 2,
  "updatedAt": "2023-04-20T12:34:56.789Z",
  "lastRotatedAt": "2023-04-20T12:34:56.789Z",
  "nextRotationAt": "2023-06-19T12:34:56.789Z",
  "rotationPeriod": 60
}
```

#### Rotate Secret

```
POST /api/secrets/{secretId}/rotate
```

Request:
```json
{
  "newValue": "rotated-secret-api-key-123"
}
```

Response:
```json
{
  "id": "1234567890",
  "name": "API Key",
  "description": "Updated API key for external service",
  "allowedFunctionIds": ["1234567890", "0987654321"],
  "tags": {
    "type": "api-key",
    "environment": "production"
  },
  "version": 3,
  "updatedAt": "2023-04-20T12:34:56.789Z",
  "lastRotatedAt": "2023-04-20T12:34:56.789Z",
  "nextRotationAt": "2023-06-19T12:34:56.789Z",
  "rotationPeriod": 60
}
```

#### Check Secret Access

```
GET /api/secrets/{secretId}/access/{functionId}
```

Response:
```json
{
  "secretId": "1234567890",
  "functionId": "1234567890",
  "hasAccess": true
}
```

### Function Service

#### Execute Function

```
POST /api/functions/{functionId}/execute
```

Request:
```json
{
  "parameters": {
    "param1": "value1",
    "param2": "value2"
  }
}
```

Response:
```json
{
  "functionId": "1234567890",
  "result": {
    "output": "Function output"
  },
  "executionTime": 123.45,
  "timestamp": "2023-04-20T12:34:56.789Z"
}
```

#### Execute Function for Event

```
POST /api/functions/{functionId}/execute-for-event
```

Request:
```json
{
  "event": {
    "type": "blockchain",
    "data": {
      "txid": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
      "blockHeight": 12345,
      "timestamp": "2023-04-20T12:34:56.789Z"
    }
  },
  "parameters": {
    "param1": "value1",
    "param2": "value2"
  }
}
```

Response:
```json
{
  "functionId": "1234567890",
  "result": {
    "output": "Function output"
  },
  "executionTime": 123.45,
  "timestamp": "2023-04-20T12:34:56.789Z"
}
```

### Price Feed Service

#### Fetch Prices

```
POST /api/price-feed/fetch
```

Request:
```json
{
  "baseCurrency": "USD",
  "sources": [
    {
      "id": "1234567890",
      "name": "CoinGecko",
      "type": "REST",
      "url": "https://api.coingecko.com/api/v3/simple/price?ids={asset}&vs_currencies={currency}",
      "supportedAssets": ["BTC", "ETH", "NEO"]
    }
  ]
}
```

Response:
```json
[
  {
    "id": "1234567890",
    "symbol": "BTC",
    "baseCurrency": "USD",
    "value": 50000.0,
    "timestamp": "2023-04-20T12:34:56.789Z",
    "sourcePrices": [
      {
        "id": "1234567890",
        "sourceId": "1234567890",
        "sourceName": "CoinGecko",
        "value": 50000.0,
        "timestamp": "2023-04-20T12:34:56.789Z",
        "weight": 100
      }
    ],
    "confidenceScore": 100,
    "signature": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
    "createdAt": "2023-04-20T12:34:56.789Z"
  },
  {
    "id": "0987654321",
    "symbol": "ETH",
    "baseCurrency": "USD",
    "value": 3000.0,
    "timestamp": "2023-04-20T12:34:56.789Z",
    "sourcePrices": [
      {
        "id": "0987654321",
        "sourceId": "1234567890",
        "sourceName": "CoinGecko",
        "value": 3000.0,
        "timestamp": "2023-04-20T12:34:56.789Z",
        "weight": 100
      }
    ],
    "confidenceScore": 100,
    "signature": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
    "createdAt": "2023-04-20T12:34:56.789Z"
  }
]
```

#### Fetch Price for Symbol

```
POST /api/price-feed/fetch-symbol
```

Request:
```json
{
  "symbol": "BTC",
  "baseCurrency": "USD",
  "sources": [
    {
      "id": "1234567890",
      "name": "CoinGecko",
      "type": "REST",
      "url": "https://api.coingecko.com/api/v3/simple/price?ids={asset}&vs_currencies={currency}"
    },
    {
      "id": "0987654321",
      "name": "CoinMarketCap",
      "type": "REST",
      "url": "https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol={symbol}&convert={currency}",
      "apiKey": "your-api-key"
    }
  ]
}
```

Response:
```json
[
  {
    "id": "1234567890",
    "symbol": "BTC",
    "baseCurrency": "USD",
    "value": 50000.0,
    "timestamp": "2023-04-20T12:34:56.789Z",
    "sourcePrices": [
      {
        "id": "1234567890",
        "sourceId": "1234567890",
        "sourceName": "CoinGecko",
        "value": 50000.0,
        "timestamp": "2023-04-20T12:34:56.789Z",
        "weight": 100
      }
    ],
    "confidenceScore": 100,
    "signature": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
    "createdAt": "2023-04-20T12:34:56.789Z"
  },
  {
    "id": "0987654321",
    "symbol": "BTC",
    "baseCurrency": "USD",
    "value": 50100.0,
    "timestamp": "2023-04-20T12:34:56.789Z",
    "sourcePrices": [
      {
        "id": "0987654321",
        "sourceId": "0987654321",
        "sourceName": "CoinMarketCap",
        "value": 50100.0,
        "timestamp": "2023-04-20T12:34:56.789Z",
        "weight": 100
      }
    ],
    "confidenceScore": 100,
    "signature": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
    "createdAt": "2023-04-20T12:34:56.789Z"
  },
  {
    "id": "abcdef1234",
    "symbol": "BTC",
    "baseCurrency": "USD",
    "value": 50050.0,
    "timestamp": "2023-04-20T12:34:56.789Z",
    "sourcePrices": [
      {
        "id": "1234567890",
        "sourceId": "1234567890",
        "sourceName": "CoinGecko",
        "value": 50000.0,
        "timestamp": "2023-04-20T12:34:56.789Z",
        "weight": 100
      },
      {
        "id": "0987654321",
        "sourceId": "0987654321",
        "sourceName": "CoinMarketCap",
        "value": 50100.0,
        "timestamp": "2023-04-20T12:34:56.789Z",
        "weight": 100
      }
    ],
    "confidenceScore": 99,
    "signature": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
    "createdAt": "2023-04-20T12:34:56.789Z"
  }
]
```

#### Generate Price History

```
POST /api/price-feed/history
```

Request:
```json
{
  "symbol": "BTC",
  "baseCurrency": "USD",
  "interval": "1h",
  "startTime": "2023-04-19T00:00:00.000Z",
  "endTime": "2023-04-20T00:00:00.000Z",
  "prices": [
    {
      "id": "1234567890",
      "symbol": "BTC",
      "baseCurrency": "USD",
      "value": 50000.0,
      "timestamp": "2023-04-19T00:00:00.000Z"
    },
    {
      "id": "0987654321",
      "symbol": "BTC",
      "baseCurrency": "USD",
      "value": 51000.0,
      "timestamp": "2023-04-19T01:00:00.000Z"
    },
    {
      "id": "abcdef1234",
      "symbol": "BTC",
      "baseCurrency": "USD",
      "value": 50500.0,
      "timestamp": "2023-04-19T02:00:00.000Z"
    }
  ]
}
```

Response:
```json
{
  "id": "1234567890",
  "symbol": "BTC",
  "baseCurrency": "USD",
  "interval": "1h",
  "startTime": "2023-04-19T00:00:00.000Z",
  "endTime": "2023-04-20T00:00:00.000Z",
  "dataPoints": [
    {
      "timestamp": "2023-04-19T00:00:00.000Z",
      "open": 50000.0,
      "high": 50000.0,
      "low": 50000.0,
      "close": 50000.0,
      "volume": 0.0
    },
    {
      "timestamp": "2023-04-19T01:00:00.000Z",
      "open": 51000.0,
      "high": 51000.0,
      "low": 51000.0,
      "close": 51000.0,
      "volume": 0.0
    },
    {
      "timestamp": "2023-04-19T02:00:00.000Z",
      "open": 50500.0,
      "high": 50500.0,
      "low": 50500.0,
      "close": 50500.0,
      "volume": 0.0
    }
  ],
  "createdAt": "2023-04-20T12:34:56.789Z",
  "updatedAt": "2023-04-20T12:34:56.789Z"
}
```

#### Validate Source

```
POST /api/price-feed/validate-source
```

Request:
```json
{
  "source": {
    "id": "1234567890",
    "name": "CoinGecko",
    "type": "REST",
    "url": "https://api.coingecko.com/api/v3/simple/price?ids={asset}&vs_currencies={currency}"
  }
}
```

Response:
```json
{
  "isValid": true,
  "message": "Source validated successfully"
}
```

#### Submit Price to Oracle

```
POST /api/price-feed/submit
```

Request:
```json
{
  "price": {
    "id": "1234567890",
    "symbol": "BTC",
    "baseCurrency": "USD",
    "value": 50000.0,
    "timestamp": "2023-04-20T12:34:56.789Z",
    "confidenceScore": 100,
    "signature": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"
  },
  "walletId": "1234567890",
  "password": "walletPassword123",
  "network": "TestNet"
}
```

Response:
```json
{
  "transactionHash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"
}
```

#### Submit Batch to Oracle

```
POST /api/price-feed/submit-batch
```

Request:
```json
{
  "prices": [
    {
      "id": "1234567890",
      "symbol": "BTC",
      "baseCurrency": "USD",
      "value": 50000.0,
      "timestamp": "2023-04-20T12:34:56.789Z",
      "confidenceScore": 100,
      "signature": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"
    },
    {
      "id": "0987654321",
      "symbol": "ETH",
      "baseCurrency": "USD",
      "value": 3000.0,
      "timestamp": "2023-04-20T12:34:56.789Z",
      "confidenceScore": 100,
      "signature": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"
    }
  ],
  "walletId": "1234567890",
  "password": "walletPassword123",
  "network": "TestNet"
}
```

Response:
```json
[
  "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
  "0xabcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890"
]
```

## Error Handling

The API uses standard HTTP status codes to indicate the success or failure of a request:

- `200 OK`: The request was successful
- `201 Created`: The resource was created successfully
- `400 Bad Request`: The request was invalid
- `401 Unauthorized`: Authentication is required
- `403 Forbidden`: The authenticated user does not have permission to access the resource
- `404 Not Found`: The resource was not found
- `500 Internal Server Error`: An error occurred on the server

Error responses include a JSON payload with details about the error:

```json
{
  "error": {
    "code": "INVALID_REQUEST",
    "message": "The request was invalid",
    "details": "The 'email' field is required"
  }
}
```

## Rate Limiting

The API implements rate limiting to prevent abuse. The rate limits are:

- 100 requests per minute per IP address
- 1000 requests per hour per IP address
- 10000 requests per day per IP address

Rate limit information is included in the response headers:

- `X-RateLimit-Limit`: The maximum number of requests allowed in the current time window
- `X-RateLimit-Remaining`: The number of requests remaining in the current time window
- `X-RateLimit-Reset`: The time at which the current time window resets (Unix timestamp)

When the rate limit is exceeded, the API returns a `429 Too Many Requests` response with a `Retry-After` header indicating the number of seconds to wait before retrying.

## Pagination

List endpoints support pagination using the `page` and `pageSize` query parameters:

```
GET /api/wallets?page=1&pageSize=10
```

Pagination information is included in the response:

```json
{
  "items": [...],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 100,
    "totalPages": 10
  }
}
```

## Filtering

List endpoints support filtering using query parameters:

```
GET /api/wallets?name=My%20Wallet&tags.type=personal
```

## Sorting

List endpoints support sorting using the `sort` query parameter:

```
GET /api/wallets?sort=name
GET /api/wallets?sort=-createdAt
```

Use a prefix of `-` to sort in descending order.

## Conclusion

This document provides a comprehensive reference for the Neo Service Layer API. For more detailed information, refer to the documentation in the `docs/` directory.
