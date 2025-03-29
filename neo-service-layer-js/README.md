# Neo Service Layer JavaScript SDK

[![npm version](https://img.shields.io/npm/v/neo-service-layer-js.svg)](https://www.npmjs.com/package/neo-service-layer-js)
[![License](https://img.shields.io/npm/l/neo-service-layer-js.svg)](https://github.com/neo-project/neo-service-layer/blob/main/LICENSE)

A specialized JavaScript SDK that enables JavaScript functions running within the Neo Function Service to interact with various Neo services.

## Purpose

This SDK provides a streamlined interoperability interface for JavaScript functions deployed to the Neo Function Service to access and utilize other Neo Service Layer capabilities, including:

- **Functions Service**: Invoke other serverless functions
- **Gas Bank Service**: Access Neo gas management capabilities
- **Price Feed Service**: Get oracle price data
- **Secrets Service**: Store and retrieve sensitive information
- **Trigger Service**: Set up and manage event-based triggers

## Installation

```bash
npm install neo-service-layer-js
```

or

```bash
yarn add neo-service-layer-js
```

## Function Development

The SDK is specifically designed for JavaScript functions running in the Neo Function Service:

```javascript
const { createFunction } = require('neo-service-layer-js');

/**
 * Example function that gets a price feed and performs an action
 */
module.exports = createFunction(async function(context) {
  // Log function execution
  context.log('Function started');
  
  // Get parameters
  const { symbol = 'NEO' } = context.parameters;
  
  // Get current price from price feed service
  const currentPrice = await context.getPrice(symbol);
  context.log(`Current ${symbol} price: $${currentPrice}`);
  
  // Get a secret value
  const apiKey = await context.getSecret('external-api-key');
  
  // Invoke another function
  const result = await context.invokeFunction('another-function', {
    price: currentPrice,
    symbol
  });
  
  return {
    success: true,
    price: currentPrice,
    result
  };
});
```

## Function Context

The `createFunction` utility wraps your function handler with a context object that provides:

- **Authentication**: Automatic authentication with Neo Service Layer
- **Logging**: Simple logging utilities
- **Service Access**: Direct access to Neo Service Layer services
- **Helper Methods**: Simplified methods for common operations

Available context properties and methods:

| Property/Method | Description |
|----------------|-------------|
| `functionId` | ID of the current function |
| `executionId` | ID of the current execution |
| `owner` | Function owner address |
| `caller` | Address of the caller (if applicable) |
| `parameters` | Function parameters |
| `env` | Environment variables |
| `traceId` | Trace ID for request tracking |
| `neoServiceLayer` | Neo Service Layer client instance |
| `log(message)` | Log a message |
| `getSecret(key)` | Get a secret value |
| `getGasPrice()` | Get current gas price |
| `getPrice(symbol)` | Get price for a symbol |
| `invokeFunction(functionId, parameters)` | Invoke another function |

## Examples

See the [examples directory](./examples/function-examples) for complete function examples:

- **Price Alert**: Monitor price feeds and send alerts
- **Blockchain Monitor**: Track blockchain events and perform actions

## Documentation

For more detailed documentation:

- [Function Development Guide](./docs/function-development.md): Detailed guide for developing JavaScript functions
- [API Reference](./docs/api-reference.md): Complete API reference for the SDK
- [Architecture](./docs/architecture.md): Overview of the SDK architecture

For detailed documentation on the Neo Service Layer API, refer to the [Neo Service Layer Documentation](https://github.com/neo-project/neo-service-layer/blob/main/README.md).

## Contributing

We welcome contributions! Please see our [Contributing Guide](../CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.
