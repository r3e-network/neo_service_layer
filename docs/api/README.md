# Neo N3 Service Layer API Documentation Guide

## Overview

This directory contains comprehensive documentation for the Neo N3 Service Layer API. The documentation is provided in multiple formats to suit different needs and workflows:

1. [Markdown Documentation](endpoints.md) - Human-readable API reference
2. [OpenAPI Specification](openapi.yaml) - Machine-readable API definition
3. [Postman Collection](postman_collection.json) - Ready-to-use API test collection

## Getting Started

### 1. Reading the Documentation

Start with the [endpoints.md](endpoints.md) file for a comprehensive overview of all available endpoints. This document provides:
- Detailed endpoint descriptions
- Authentication requirements
- Request/response examples
- Error handling information

### 2. Using the OpenAPI Specification

The [openapi.yaml](openapi.yaml) file is an OpenAPI 3.0.3 specification that can be used with various tools:

1. **Swagger UI**: Import into [Swagger Editor](https://editor.swagger.io/) to:
   - View interactive documentation
   - Test endpoints directly
   - Generate client libraries

2. **Code Generation**:
   ```bash
   # Generate TypeScript client
   openapi-generator generate -i openapi.yaml -g typescript-fetch -o ./client

   # Generate Python client
   openapi-generator generate -i openapi.yaml -g python -o ./client
   ```

3. **IDE Integration**: Many IDEs support OpenAPI files for:
   - Code completion
   - Type checking
   - Documentation lookup

### 3. Testing with Postman

The [postman_collection.json](postman_collection.json) file contains a ready-to-use collection of API requests:

1. Import the collection into Postman:
   - Open Postman
   - Click "Import"
   - Select `postman_collection.json`

2. Set up your environment:
   - Create a new environment in Postman
   - Set the following variables:
     ```
     baseUrl: https://api.neo-service-layer.io/v1
     jwt_token: your_jwt_token_here
     ```

3. Test endpoints:
   - Start with the "Verify Signature" request to get a JWT token
   - Use other endpoints with the obtained token

## Authentication

All API endpoints (except health check and authentication) require JWT authentication:

1. Sign a message with your Neo N3 wallet
2. Call the `/auth/verify` endpoint with:
   - Your wallet address
   - The signed message
   - The signature
3. Store the returned JWT token
4. Include the token in all subsequent requests:
   ```
   Authorization: Bearer your_jwt_token_here
   ```

## Rate Limiting

The API implements rate limiting to ensure fair usage:

- Default limit: 60 requests per minute
- Limits are specified in response headers:
  ```
  X-RateLimit-Limit: 60
  X-RateLimit-Remaining: 59
  X-RateLimit-Reset: 1616799600
  ```

## Error Handling

The API uses standard HTTP status codes and returns detailed error information:

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

Common error codes:
- `400`: Invalid request
- `401`: Authentication required
- `403`: Insufficient permissions
- `404`: Resource not found
- `429`: Rate limit exceeded
- `500`: Server error

## Best Practices

1. **Authentication**
   - Store JWT tokens securely
   - Refresh tokens before expiration
   - Never expose tokens in client-side code

2. **Rate Limiting**
   - Implement exponential backoff
   - Cache responses when possible
   - Monitor rate limit headers

3. **Error Handling**
   - Implement proper error handling
   - Log error responses
   - Provide user-friendly error messages

4. **Testing**
   - Use the Postman collection for testing
   - Create environment-specific configurations
   - Automate tests using Newman

## Support

For API support:
- Email: api@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/r3e-network/neo_service_layer/issues)

## Contributing

We welcome contributions to improve the API documentation:

1. Fork the repository
2. Make your changes
3. Submit a pull request

Please ensure your changes:
- Follow the existing documentation style
- Include examples where appropriate
- Are properly tested
- Update all relevant documentation files

## License

This documentation is licensed under the MIT License. See the [LICENSE](../../LICENSE) file for details. 