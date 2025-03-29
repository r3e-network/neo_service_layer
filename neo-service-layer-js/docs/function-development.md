# Neo Function Development Guide

This guide explains how to develop JavaScript functions for the Neo Function Service using the Neo Service Layer JavaScript SDK.

## Overview

The Neo Service Layer JavaScript SDK provides a streamlined interface for JavaScript functions running within the Neo Function Service to interact with other Neo services. It simplifies common operations like:

- Logging and error handling
- Accessing secrets
- Getting price data
- Invoking other functions
- Interacting with the blockchain

## Getting Started

### 1. Install the SDK

```bash
npm install neo-service-layer-js
```

### 2. Create a Function

Create a new JavaScript file for your function using the `createFunction` utility:

```javascript
const { createFunction } = require('neo-service-layer-js');

module.exports = createFunction(async function(context) {
  // Your function logic here
  context.log('Function started');
  
  // Return a result
  return {
    success: true,
    message: 'Hello from Neo Function Service!'
  };
});
```

### 3. Function Context

The `context` object provides access to:

- Function metadata
- Parameters
- Neo Service Layer services
- Helper methods

## Using the Function Context

The function context provides a simplified interface for interacting with Neo services:

### Logging

```javascript
// Log a message
context.log('Processing transaction...');
```

### Accessing Parameters

```javascript
// Access function parameters
const { userId, amount } = context.parameters;
```

### Getting Function Metadata

```javascript
// Get function metadata
const functionId = context.functionId;
const executionId = context.executionId;
const owner = context.owner;
```

### Accessing Secrets

```javascript
// Get a secret value
const apiKey = await context.getSecret('api-key');

// Use the secret in an API call
const response = await fetch('https://api.example.com/data', {
  headers: {
    'Authorization': `Bearer ${apiKey}`
  }
});
```

### Getting Price Data

```javascript
// Get price for a symbol
const neoPrice = await context.getPrice('NEO');
console.log(`Current NEO price: $${neoPrice}`);
```

### Getting Gas Price

```javascript
// Get current gas price
const gasPrice = await context.getGasPrice();
console.log(`Current gas price: ${gasPrice}`);
```

### Invoking Other Functions

```javascript
// Invoke another function
const result = await context.invokeFunction('another-function', {
  param1: 'value1',
  param2: 'value2'
});
```

### Accessing Environment Variables

```javascript
// Access environment variables
const apiUrl = context.env.API_URL;
const debugMode = context.env.DEBUG_MODE === 'true';
```

## Automatic Authentication

When running within the Neo Function Service, your function automatically has access to the Neo Service Layer services without requiring explicit authentication. The function context handles authentication for you.

## Advanced Usage: Direct Service Access

For more advanced use cases, you can access the Neo Service Layer services directly through the context:

```javascript
// Access the Functions service
const functions = await context.neoServiceLayer.functions.listFunctions();

// Access the Secrets service
const secrets = await context.neoServiceLayer.secrets.listSecrets();

// Access the Gas Bank service
const gasAllocations = await context.neoServiceLayer.gasBank.listAllocations();

// Access the Price Feed service
const prices = await context.neoServiceLayer.priceFeed.listPrices();

// Access the Trigger service
const triggers = await context.neoServiceLayer.trigger.listTriggers();
```

## JavaScript Interoperability

The Neo Function Service provides a JavaScript runtime environment that allows your functions to interact with other Neo services seamlessly. This interoperability is achieved through the function context object that is automatically injected into your JavaScript code.

### Function Context Properties

| Property | Type | Description |
|----------|------|-------------|
| `functionId` | string | ID of the current function |
| `executionId` | string | ID of the current execution |
| `owner` | string | Function owner address |
| `caller` | string | Address of the caller (if applicable) |
| `parameters` | object | Function parameters |
| `env` | object | Environment variables |
| `traceId` | string | Trace ID for request tracking |
| `neoServiceLayer` | object | Neo Service Layer client |

### Function Context Methods

| Method | Description |
|--------|-------------|
| `log(message)` | Log a message |
| `getSecret(key)` | Get a secret value |
| `getGasPrice()` | Get current gas price |
| `getPrice(symbol)` | Get price for a symbol |
| `invokeFunction(functionId, parameters)` | Invoke another function |

## Error Handling

Functions should handle errors gracefully to provide meaningful feedback to callers:

```javascript
module.exports = createFunction(async function(context) {
  try {
    // Function logic
    const result = await processData(context.parameters);
    return { success: true, data: result };
  } catch (error) {
    context.log(`Error: ${error.message}`);
    return { success: false, error: error.message };
  }
});
```

## Best Practices

1. **Error Handling**: Always handle errors gracefully and provide meaningful error messages.
2. **Logging**: Use `context.log()` for logging to help with debugging.
3. **Secrets**: Never hardcode sensitive information; use the Secrets service.
4. **Timeouts**: Keep functions lightweight and fast to avoid timeouts.
5. **Idempotency**: Design functions to be idempotent when possible.
6. **Validation**: Validate input parameters before processing.
7. **Resource Cleanup**: Close any resources (e.g., database connections) before returning.

## Example: Complete Function

Here's a complete example of a function that uses multiple Neo services:

```javascript
const { createFunction } = require('neo-service-layer-js');

module.exports = createFunction(async function(context) {
  try {
    // Log function start
    context.log('Function started');
    
    // Get parameters
    const { symbol = 'NEO', threshold = 50 } = context.parameters;
    
    // Get current price
    const price = await context.getPrice(symbol);
    context.log(`Current ${symbol} price: $${price}`);
    
    // Get API key from secrets
    const apiKey = await context.getSecret('market-api-key');
    
    // Call external API with secret
    const response = await fetch('https://api.market.example.com/data', {
      headers: {
        'Authorization': `Bearer ${apiKey}`
      }
    });
    const marketData = await response.json();
    
    // Process data
    const analysis = analyzeMarketData(marketData, price, threshold);
    
    // Invoke another function to store results
    await context.invokeFunction('store-analysis', {
      symbol,
      price,
      analysis,
      timestamp: new Date().toISOString()
    });
    
    // Return results
    return {
      success: true,
      symbol,
      price,
      analysis,
      timestamp: new Date().toISOString()
    };
  } catch (error) {
    context.log(`Error: ${error.message}`);
    return {
      success: false,
      error: error.message
    };
  }
});

function analyzeMarketData(marketData, price, threshold) {
  // Implement your analysis logic here
  return {
    sentiment: marketData.sentiment,
    volatility: marketData.volatility,
    recommendation: price > threshold ? 'SELL' : 'BUY'
  };
}
```

## Interoperability Features

The Neo Function Service provides several interoperability features that enable seamless interaction between functions and other Neo services.

### Function Invocation

Functions can invoke other functions using the `invokeFunction` method:

```javascript
const result = await context.invokeFunction('another-function', {
  param1: 'value1',
  param2: 'value2'
});
```

### Service Access

Functions can access other Neo services directly using the `neoServiceLayer` object:

```javascript
const functions = await context.neoServiceLayer.functions.listFunctions();
const secrets = await context.neoServiceLayer.secrets.listSecrets();
const gasAllocations = await context.neoServiceLayer.gasBank.listAllocations();
const prices = await context.neoServiceLayer.priceFeed.listPrices();
const triggers = await context.neoServiceLayer.trigger.listTriggers();
```

### Event Handling

Functions can handle events triggered by other Neo services using the `onEvent` method:

```javascript
context.onEvent('event-name', async (event) => {
  // Handle event logic here
});
```

## Deployment

Functions are deployed to the Neo Function Service using the Neo Service Layer API or CLI. Refer to the [Neo Service Layer Documentation](https://github.com/neo-project/neo-service-layer/blob/main/README.md) for deployment instructions.

## Reference

For a complete reference of the Neo Service Layer JavaScript SDK, see the [API Reference](./api-reference.md).
