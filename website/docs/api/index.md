# API Documentation

This section contains detailed documentation for all API endpoints provided by the Neo Service Layer.

## Overview

The API is organized into the following services:

- Price Feed API
- Gas Bank API
- Trigger API
- Metrics API
- Logging API
- Secrets API
- Functions API

Each service has its own dedicated documentation section with detailed endpoint descriptions, request/response formats, and examples.

## Authentication

All API endpoints require authentication using JWT tokens. To obtain a token:

1. Sign a message using your Neo N3 wallet
2. Send the signed message to the authentication endpoint
3. Receive a JWT token
4. Include the token in the Authorization header of subsequent requests

## Base URL

```
https://api.neo-service-layer.io/v1
```

## Rate Limiting

The API implements rate limiting based on:
- IP address
- API key
- Endpoint

Rate limits are specified in the response headers:
- `X-RateLimit-Limit`: Number of requests allowed per window
- `X-RateLimit-Remaining`: Number of requests remaining in current window
- `X-RateLimit-Reset`: Time when the rate limit resets

## Error Handling

The API uses standard HTTP status codes and returns errors in the following format:

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable error message",
    "details": {
      // Additional error details if available
    }
  }
}
```

## Versioning

The API uses semantic versioning (major.minor.patch) and includes the version in the URL path. Breaking changes will only be introduced in major version updates. 