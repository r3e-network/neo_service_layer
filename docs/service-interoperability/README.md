# Neo Service Layer - Service Interoperability

This documentation explains how serverless functions can interact with other Neo Service Layer components through the service interoperability mechanism.

## Overview

Functions in the Neo Service Layer can interact with several services:

1. **Secrets Service** - Store and retrieve sensitive information
2. **Price Feed Service** - Get current market prices for tokens
3. **Gas Bank Service** - Manage GAS allocations
4. **Transaction Service** - Create, sign, and submit blockchain transactions
5. **Functions Service** - Invoke other serverless functions
6. **Trigger Service** - Create and manage event triggers

## Usage from JavaScript Functions

Service access is provided through the `context` object available to all functions:

```javascript
function main(args) {
  // Access secrets
  const secret = context.secrets.get("myApiKey");
  
  // Get current price of NEO
  const neoPrice = context.priceFeed.getPrice("NEO");
  
  // Create a blockchain transaction
  const tx = context.transaction.create({
    script: "...",
    signers: [...]
  });
  
  // Invoke another function
  const result = context.functions.invoke("another-function-id", { param1: "value" });
  
  // Create a scheduled trigger
  const trigger = context.trigger.create("schedule", { 
    cronExpression: "0 */15 * * * *" 
  });
  
  return { success: true };
}
```

## Security Considerations

- Service access is governed by the permissions of the function owner
- Services enforce access controls based on the function's context
- Secret values are never logged or exposed in execution history

## Service Interfaces

Details of each service interface can be found in the corresponding service documentation.