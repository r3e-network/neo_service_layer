# API Service

The API Service provides a RESTful HTTP interface for interacting with the Neo Service Layer. It serves as the main entry point for external applications to use the platform's functionality.

## Architecture

The API Service is designed with the following principles:

1. **RESTful Design**: Clean, resource-oriented API endpoints that follow REST principles.
2. **JWT Authentication**: Secure access using NEO wallet signature verification and JWT tokens.
3. **API Versioning**: Support for multiple API versions through URL path prefixes.
4. **Error Handling**: Consistent error responses with appropriate HTTP status codes.
5. **Rate Limiting**: Protection against abuse through configurable rate limiting.

## Features

- **Authentication**: NEO wallet-based authentication through signature verification.
- **Function Management**: Create, read, update, and delete serverless functions.
- **Function Execution**: Invoke functions synchronously or asynchronously.
- **Secret Management**: Secure storage and retrieval of sensitive information.
- **Gas Management**: Allocate and release GAS for operations.
- **Price Feeds**: Access to on-chain price data.
- **Trigger Management**: Create and manage event-based triggers.
- **Health Checks**: Monitor service health and status.
- **Usage Statistics**: Track API usage and performance metrics.

## Configuration

The service can be configured with the following options:

- `Port`: HTTP port to listen on (default: 3000).
- `Host`: Host to bind to (default: 0.0.0.0).
- `ReadTimeout`: HTTP read timeout (default: 30s).
- `WriteTimeout`: HTTP write timeout (default: 30s).
- `IdleTimeout`: HTTP idle timeout (default: 60s).
- `MaxRequestBodySize`: Maximum size of request body (default: 1MB).
- `EnableCORS`: Whether to enable CORS (default: true).
- `AllowedOrigins`: Allowed origins for CORS (default: *).
- `EnableHTTPS`: Whether to enable HTTPS (default: false).
- `CertFile`: TLS certificate file.
- `KeyFile`: TLS key file.
- `EnableRateLimiting`: Whether to enable rate limiting (default: true).
- `RateLimitPerMinute`: Number of requests allowed per minute per IP (default: 60).
- `JWTSecret`: Secret for JWT authentication.
- `JWTExpiryDuration`: JWT token expiry duration (default: 24h).
- `EnableRequestLogging`: Whether to enable request logging (default: true).

## API Endpoints

### Authentication

- `POST /api/v1/auth/verify`: Verify a signature and get a JWT token.

### Functions

- `POST /api/v1/functions`: Create a new function.
- `GET /api/v1/functions`: List all functions owned by the authenticated user.
- `GET /api/v1/functions/{id}`: Get a specific function.
- `PUT /api/v1/functions/{id}`: Update a function.
- `DELETE /api/v1/functions/{id}`: Delete a function.
- `POST /api/v1/functions/{id}/invoke`: Invoke a function.
- `GET /api/v1/functions/{id}/executions`: List function executions.
- `GET /api/v1/functions/{id}/permissions`: Get function permissions.
- `PUT /api/v1/functions/{id}/permissions`: Update function permissions.

### Secrets

- `POST /api/v1/secrets`: Store a secret.
- `GET /api/v1/secrets`: List all secrets owned by the authenticated user.
- `GET /api/v1/secrets/{key}`: Get a specific secret.
- `DELETE /api/v1/secrets/{key}`: Delete a secret.

### Gas

- `POST /api/v1/gas/allocate`: Allocate gas.
- `POST /api/v1/gas/release`: Release gas.
- `GET /api/v1/gas/balance`: Get gas balance.

### Prices

- `GET /api/v1/prices`: List all prices.
- `GET /api/v1/prices/{symbol}`: Get a specific price.

### Triggers

- `POST /api/v1/triggers`: Create a new trigger.
- `GET /api/v1/triggers`: List all triggers owned by the authenticated user.
- `GET /api/v1/triggers/{id}`: Get a specific trigger.
- `PUT /api/v1/triggers/{id}`: Update a trigger.
- `DELETE /api/v1/triggers/{id}`: Delete a trigger.
- `POST /api/v1/triggers/{id}/execute`: Execute a trigger.

### User Profile

- `GET /api/v1/profile`: Get the authenticated user's profile.

### System

- `GET /health`: Check system health.
- `GET /stats`: Get API usage statistics.

## Authentication Flow

1. User signs a message using their NEO wallet.
2. User sends address, message, and signature to `POST /api/v1/auth/verify`.
3. API verifies the signature against the address.
4. If valid, API generates a JWT token and returns it.
5. User includes the JWT token in the `Authorization` header for subsequent requests.

## Error Handling

All API errors follow a consistent format:

```json
{
  "code": 400,
  "message": "Invalid request",
  "details": "Missing required field: name"
}
```

- `code`: HTTP status code
- `message`: Short error message
- `details`: Detailed error information

## Security Considerations

- Use HTTPS in production.
- Set a strong JWT secret.
- Configure rate limiting to prevent abuse.
- Regularly rotate JWT secrets.
- Monitor API usage for suspicious activity.