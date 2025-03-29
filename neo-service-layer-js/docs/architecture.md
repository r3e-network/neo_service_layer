# Neo Service Layer JavaScript SDK Architecture

This document outlines the architecture of the Neo Service Layer JavaScript SDK, designed specifically for interoperability between JavaScript functions running within the Neo Function Service and other Neo services.

## Overview

The Neo Service Layer JavaScript SDK is structured as a focused system that provides:

1. A streamlined interoperability interface for JavaScript functions to interact with Neo services
2. Automatic authentication and context management for functions
3. Type-safe access to all Neo Service Layer APIs
4. Simplified utilities for common operations

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                Neo Service Layer JavaScript SDK              │
├─────────────────┬─────────────────────┬─────────────────────┤
│                 │                     │                     │
│   Core Module   │  Service Modules    │  Utility Modules    │
│                 │                     │                     │
├─────────────────┼─────────────────────┼─────────────────────┤
│ - Client        │ - Functions Service │ - Function Context  │
│ - Error Handling│ - Gas Bank Service  │ - Type Definitions  │
│ - Configuration │ - Price Feed Service│                     │
│                 │ - Secrets Service   │                     │
│                 │ - Trigger Service   │                     │
└─────────────────┴─────────────────────┴─────────────────────┘
```

## Core Module

The Core Module provides the foundation for the SDK, handling HTTP requests, error handling, and configuration management specifically for function interoperability.

### Client

The `NeoServiceLayer` class is the main entry point for the SDK. It:

- Handles HTTP requests to the Neo Service Layer API
- Provides access to all service modules
- Manages function context information
- Manages configuration options

```typescript
// Example client initialization in function context
const client = new NeoServiceLayer({
  baseUrl: 'https://api.neo-service-layer.io',
  headers: {
    'X-Function-Id': 'function-123',
    'X-Execution-Id': 'execution-456',
    'X-Trace-Id': 'trace-789'
  }
});
```

### Error Handling

The SDK provides a focused error handling system with specific error classes for different error types:

- `NeoServiceLayerError`: Base error class
- `ApiError`: API request errors
- `ValidationError`: Input validation errors
- `NotFoundError`: Resource not found errors
- `FunctionError`: Function execution errors
- `GasBankError`: Gas bank operation errors

## Service Modules

The SDK is organized into service-specific modules, each providing access to a specific Neo Service Layer service.

### Functions Service

The Functions Service module provides methods for invoking other serverless functions.

Key capabilities:
- Invoke functions with parameters
- Get function execution results
- List function executions

### Gas Bank Service

The Gas Bank Service module provides methods for accessing Neo gas management capabilities.

Key capabilities:
- Get gas price
- Get gas balance
- Estimate gas for transactions

### Price Feed Service

The Price Feed Service module provides access to oracle services for price data.

Key capabilities:
- Get price for a symbol
- Get prices for multiple symbols
- Get price history

### Secrets Service

The Secrets Service module provides methods for securely retrieving sensitive information.

Key capabilities:
- Get secrets
- Check if a secret exists
- List secrets by tag

### Trigger Service

The Trigger Service module provides methods for monitoring blockchain events and setting up automated execution.

Key capabilities:
- Create and manage triggers
- List trigger executions
- Manually execute triggers

## Utility Modules

### Function Context

The Function Context utility provides a streamlined interface for JavaScript functions running within the Neo Function Service.

The `createFunction` utility wraps a function handler with a context object that provides:

- Access to function metadata
- Simplified methods for common operations
- Automatic authentication
- Error handling

```javascript
// Example function using the context
const { createFunction } = require('neo-service-layer-js');

module.exports = createFunction(async function(context) {
  context.log('Function started');
  
  const neoPrice = await context.getPrice('NEO');
  
  return {
    price: neoPrice
  };
});
```

### Type Definitions

The SDK provides focused TypeScript definitions for all interfaces, ensuring type safety and improved developer experience.

Key type categories:
- Configuration types
- Function execution types
- Request types
- Response types

## Data Flow

### Function Execution Flow

1. A function is invoked through the Neo Function Service
2. The Neo Function Service creates an execution environment
3. The `createFunction` utility creates a context object with access to the execution environment
4. The function handler is called with the context object
5. The function interacts with Neo services through the context
6. The function returns a result, which is sent back to the caller

### API Request Flow

1. A context method is called (e.g., `context.getPrice('NEO')`)
2. The method calls the appropriate service method
3. The service method validates the input parameters
4. The service method calls the client's `request` method
5. The client adds function context headers
6. The client sends the HTTP request to the Neo Service Layer API
7. The client processes the response and handles any errors
8. The service method returns the processed response

## Security Considerations

1. **Authentication**: The SDK uses function context headers for authentication, which are automatically provided by the Neo Function Service.
2. **Secrets Management**: Sensitive information is retrieved securely through the Secrets Service.
3. **Error Handling**: Errors are handled gracefully, without exposing sensitive information.

## Performance Considerations

1. **Request Optimization**: The SDK minimizes the number of requests to the API by providing direct methods for common operations.
2. **Memory Usage**: The SDK is designed to be lightweight and efficient, minimizing memory usage.
3. **Error Recovery**: The SDK provides mechanisms for recovering from errors.

## Dependencies

The SDK has minimal dependencies to ensure lightweight deployment:

- `axios`: For making HTTP requests

## Conclusion

The Neo Service Layer JavaScript SDK provides a focused and efficient interoperability interface for JavaScript functions to interact with the Neo Service Layer. Its streamlined architecture and function context utilities make it an ideal tool for developing serverless functions for the Neo ecosystem.
