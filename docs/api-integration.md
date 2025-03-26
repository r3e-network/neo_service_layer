# API Integration Guide

This guide explains how to integrate with the Neo Service Layer API to build applications that use functions, secrets, price feeds, and other services.

## Authentication

All authenticated API requests require a valid JWT token, which is obtained by verifying a signature from a NEO wallet.

### Signature Verification

```
POST /api/v1/auth/verify
```

**Request Body:**
```json
{
  "address": "NYxb4fSZVKAz8YsgaPK2WkT3KcAE9b3Vag",
  "message": "Login to Neo Service Layer at 2023-03-26T00:00:00Z",
  "signature": "010203...signature_bytes_in_hex",
  "salt": "optional_random_value"
}
```

**Response:**
```json
{
  "valid": true,
  "address": "NYxb4fSZVKAz8YsgaPK2WkT3KcAE9b3Vag",
  "scriptHash": "0x1234567890abcdef1234567890abcdef12345678"
}
```

The server will also return an `Authorization` header with the JWT token:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Use this token in the `Authorization` header for all authenticated requests:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Functions

### Create a Function

```
POST /api/v1/functions
```

**Request Body:**
```json
{
  "name": "hello-world",
  "description": "A simple hello world function",
  "code": "function main(args) { return { message: 'Hello, ' + (args.name || 'World') }; }",
  "runtime": "javascript",
  "metadata": {
    "category": "example",
    "version": "1.0.0"
  }
}
```

**Response:**
```json
{
  "id": "f123456789abcdef",
  "name": "hello-world",
  "description": "A simple hello world function",
  "owner": "0x1234567890abcdef1234567890abcdef12345678",
  "code": "function main(args) { return { message: 'Hello, ' + (args.name || 'World') }; }",
  "runtime": "javascript",
  "status": "active",
  "triggers": [],
  "createdAt": "2023-03-26T00:00:00Z",
  "updatedAt": "2023-03-26T00:00:00Z",
  "metadata": {
    "category": "example",
    "version": "1.0.0"
  }
}
```

### List Functions

```
GET /api/v1/functions
```

**Response:**
```json
[
  {
    "id": "f123456789abcdef",
    "name": "hello-world",
    "description": "A simple hello world function",
    "owner": "0x1234567890abcdef1234567890abcdef12345678",
    "runtime": "javascript",
    "status": "active",
    "createdAt": "2023-03-26T00:00:00Z",
    "updatedAt": "2023-03-26T00:00:00Z"
  }
]
```

### Get Function Details

```
GET /api/v1/functions/{id}
```

**Response:**
```json
{
  "id": "f123456789abcdef",
  "name": "hello-world",
  "description": "A simple hello world function",
  "owner": "0x1234567890abcdef1234567890abcdef12345678",
  "code": "function main(args) { return { message: 'Hello, ' + (args.name || 'World') }; }",
  "runtime": "javascript",
  "status": "active",
  "triggers": [],
  "createdAt": "2023-03-26T00:00:00Z",
  "updatedAt": "2023-03-26T00:00:00Z",
  "lastExecuted": "2023-03-26T01:00:00Z",
  "metadata": {
    "category": "example",
    "version": "1.0.0"
  }
}
```

### Update Function

```
PUT /api/v1/functions/{id}
```

**Request Body:**
```json
{
  "description": "Updated description",
  "code": "function main(args) { return { message: 'Hello, ' + (args.name || 'Universe') }; }",
  "metadata": {
    "category": "example",
    "version": "1.0.1"
  }
}
```

**Response:**
```json
{
  "id": "f123456789abcdef",
  "name": "hello-world",
  "description": "Updated description",
  "owner": "0x1234567890abcdef1234567890abcdef12345678",
  "code": "function main(args) { return { message: 'Hello, ' + (args.name || 'Universe') }; }",
  "runtime": "javascript",
  "status": "active",
  "triggers": [],
  "createdAt": "2023-03-26T00:00:00Z",
  "updatedAt": "2023-03-26T02:00:00Z",
  "metadata": {
    "category": "example",
    "version": "1.0.1"
  }
}
```

### Delete Function

```
DELETE /api/v1/functions/{id}
```

**Response:**
```
204 No Content
```

### Invoke Function

```
POST /api/v1/functions/{id}/invoke
```

**Request Body:**
```json
{
  "parameters": {
    "name": "John"
  },
  "async": false,
  "traceId": "request-123",
  "idempotency": "idempotency-key-123"
}
```

**Response:**
```json
{
  "id": "e123456789abcdef",
  "functionId": "f123456789abcdef",
  "status": "completed",
  "startTime": "2023-03-26T03:00:00Z",
  "endTime": "2023-03-26T03:00:01Z",
  "duration": 100,
  "memoryUsed": 1048576,
  "parameters": {
    "name": "John"
  },
  "result": {
    "message": "Hello, John"
  },
  "logs": [
    "Function executed successfully"
  ],
  "invokedBy": "0x1234567890abcdef1234567890abcdef12345678",
  "traceId": "request-123"
}
```

## Secrets

### Store Secret

```
POST /api/v1/secrets
```

**Request Body:**
```json
{
  "key": "api_key",
  "value": "secret-api-key-value",
  "ttl": 86400,
  "tags": [
    "api",
    "production"
  ],
  "options": {
    "readOnly": true
  }
}
```

**Response:**
```json
{
  "key": "api_key"
}
```

### List Secrets

```
GET /api/v1/secrets
```

**Response:**
```json
[
  "api_key",
  "database_password",
  "jwt_secret"
]
```

### Get Secret

```
GET /api/v1/secrets/{key}
```

**Response:**
```json
{
  "key": "api_key",
  "value": "secret-api-key-value"
}
```

### Delete Secret

```
DELETE /api/v1/secrets/{key}
```

**Response:**
```
204 No Content
```

## Gas Bank

### Allocate Gas

```
POST /api/v1/gas/allocate
```

**Response:**
```json
{
  "amount": "100"
}
```

### Get Gas Balance

```
GET /api/v1/gas/balance
```

**Response:**
```json
{
  "balance": "98.5"
}
```

### Release Gas

```
POST /api/v1/gas/release
```

**Response:**
```
204 No Content
```

## Price Feed

### List Prices

```
GET /api/v1/prices
```

**Response:**
```json
{
  "NEO/USD": "50",
  "GAS/USD": "15",
  "BTC/USD": "40000",
  "ETH/USD": "2500",
  "LINK/USD": "20"
}
```

### Get Specific Price

```
GET /api/v1/prices/{symbol}
```

**Response:**
```json
{
  "symbol": "NEO/USD",
  "price": "50",
  "time": "2023-03-26T04:00:00Z"
}
```

## Triggers

### Create Trigger

```
POST /api/v1/triggers
```

**Request Body:**
```json
{
  "type": "schedule",
  "condition": "0 * * * *",
  "action": "function:f123456789abcdef",
  "metadata": {
    "parameters": {
      "action": "refresh"
    }
  }
}
```

**Response:**
```json
{
  "id": "t123456789abcdef",
  "type": "schedule",
  "condition": "0 * * * *",
  "action": "function:f123456789abcdef",
  "owner": "0x1234567890abcdef1234567890abcdef12345678",
  "createdAt": "2023-03-26T05:00:00Z",
  "updatedAt": "2023-03-26T05:00:00Z",
  "active": true,
  "metadata": {
    "parameters": {
      "action": "refresh"
    }
  }
}
```

### List Triggers

```
GET /api/v1/triggers
```

**Response:**
```json
[
  {
    "id": "t123456789abcdef",
    "type": "schedule",
    "condition": "0 * * * *",
    "action": "function:f123456789abcdef",
    "owner": "0x1234567890abcdef1234567890abcdef12345678",
    "createdAt": "2023-03-26T05:00:00Z",
    "updatedAt": "2023-03-26T05:00:00Z",
    "active": true
  }
]
```

### Get Trigger Details

```
GET /api/v1/triggers/{id}
```

**Response:**
```json
{
  "id": "t123456789abcdef",
  "type": "schedule",
  "condition": "0 * * * *",
  "action": "function:f123456789abcdef",
  "owner": "0x1234567890abcdef1234567890abcdef12345678",
  "createdAt": "2023-03-26T05:00:00Z",
  "updatedAt": "2023-03-26T05:00:00Z",
  "active": true,
  "metadata": {
    "parameters": {
      "action": "refresh"
    }
  }
}
```

### Update Trigger

```
PUT /api/v1/triggers/{id}
```

**Request Body:**
```json
{
  "condition": "0 */2 * * *",
  "action": "function:f987654321abcdef",
  "active": false
}
```

**Response:**
```json
{
  "id": "t123456789abcdef",
  "type": "schedule",
  "condition": "0 */2 * * *",
  "action": "function:f987654321abcdef",
  "owner": "0x1234567890abcdef1234567890abcdef12345678",
  "createdAt": "2023-03-26T05:00:00Z",
  "updatedAt": "2023-03-26T06:00:00Z",
  "active": false,
  "metadata": {
    "parameters": {
      "action": "refresh"
    }
  }
}
```

### Delete Trigger

```
DELETE /api/v1/triggers/{id}
```

**Response:**
```
204 No Content
```

### Execute Trigger

```
POST /api/v1/triggers/{id}/execute
```

**Response:**
```json
{
  "triggerID": "t123456789abcdef",
  "success": true,
  "result": "Trigger executed successfully",
  "timestamp": "2023-03-26T07:00:00Z"
}
```

## User Profile

### Get User Profile

```
GET /api/v1/profile
```

**Response:**
```json
{
  "address": "NYxb4fSZVKAz8YsgaPK2WkT3KcAE9b3Vag",
  "scriptHash": "0x1234567890abcdef1234567890abcdef12345678",
  "functionCount": 2,
  "secretCount": 3,
  "triggerCount": 1,
  "gasBalance": "98.5",
  "lastActivity": "2023-03-26T08:00:00Z",
  "createdAt": "2023-03-26T00:00:00Z",
  "notificationsEnabled": true
}
```

## System Health

### Check Health

```
GET /health
```

**Response:**
```json
{
  "healthy": true,
  "services": [
    {
      "service": "api",
      "status": "healthy",
      "version": "1.0.0",
      "message": "API service is running",
      "lastChecked": "2023-03-26T09:00:00Z"
    },
    {
      "service": "functions",
      "status": "healthy",
      "version": "1.0.0",
      "message": "Functions service is running",
      "lastChecked": "2023-03-26T09:00:00Z"
    }
  ],
  "uptime": 3600,
  "region": "default",
  "timestamp": "2023-03-26T09:00:00Z",
  "version": "1.0.0"
}
```

### Get Statistics

```
GET /stats
```

**Response:**
```json
{
  "totalRequests": 1000,
  "totalErrors": 5,
  "averageResponseTime": 50,
  "requestsPerMinute": 10,
  "lastUpdated": "2023-03-26T10:00:00Z"
}
```

## Error Handling

All API errors follow a consistent format:

```json
{
  "code": 400,
  "message": "Invalid request",
  "details": "Missing required field: name"
}
```

Common HTTP status codes:

- `200 OK`: The request succeeded
- `201 Created`: A new resource was created
- `204 No Content`: The request succeeded, but there is no content to return
- `400 Bad Request`: The request was invalid
- `401 Unauthorized`: Authentication is required or failed
- `403 Forbidden`: The client does not have permission to access the resource
- `404 Not Found`: The requested resource does not exist
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: An error occurred on the server