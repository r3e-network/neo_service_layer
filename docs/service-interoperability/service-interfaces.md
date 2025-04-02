# Neo Service Layer - Service Interfaces

This document describes the interfaces available to serverless functions for interacting with other Neo Service Layer components.

## Secrets Service

The Secrets Service allows secure storage and retrieval of sensitive information.

### Methods

- **get(secretName)** - Retrieves a secret by name
- **set(secretName, secretValue)** - Creates or updates a secret
- **delete(secretName)** - Removes a secret

### Example

```javascript
// Store an API key
const setResult = context.secrets.set("api-key", "my-secret-key-value");

// Retrieve the secret
const getResult = context.secrets.get("api-key");
if (getResult.success) {
  console.log("Retrieved secret value:", getResult.value);
}

// Delete the secret when no longer needed
const deleteResult = context.secrets.delete("api-key");
```

## Price Feed Service

The Price Feed Service provides access to market prices for various tokens.

### Methods

- **getPrice(symbol)** - Gets the current price for a token

### Example

```javascript
// Get the current price of NEO
const neoPrice = context.priceFeed.getPrice("NEO");
if (neoPrice.success) {
  console.log("NEO price:", neoPrice.price, neoPrice.currency);
  console.log("Last updated:", new Date(neoPrice.timestamp * 1000).toISOString());
}

// Get prices for multiple tokens
const tokens = ["NEO", "GAS", "BTC", "ETH"];
const prices = tokens.map(token => {
  const price = context.priceFeed.getPrice(token);
  return { token, price: price.price, success: price.success };
});
```

## Gas Bank Service

The Gas Bank Service manages GAS allocations for function execution.

### Methods

- **getBalance()** - Gets the current GAS balance for the function owner

### Example

```javascript
// Check available GAS balance
const balance = context.gasBank.getBalance();
if (balance.success) {
  console.log("Available GAS:", balance.balance);
  
  if (balance.balance < 10) {
    console.warn("GAS balance is low!");
  }
}
```

## Transaction Service

The Transaction Service enables creating, signing, and submitting blockchain transactions.

### Methods

- **create(txConfig)** - Creates a new transaction with the specified configuration
- **sign(txId)** - Signs a previously created transaction
- **send(txId)** - Submits a signed transaction to the blockchain
- **status(txId)** - Checks the status of a transaction
- **get(txId)** - Gets detailed information about a transaction
- **list(filter)** - Lists transactions matching certain criteria
- **estimateFee(txConfig)** - Estimates the fee for a transaction

### Example

```javascript
// Create a transaction to invoke a contract
const txCreate = context.transaction.create({
  script: "0c0548656c6c6f0c03576f726c64192126dd72c4..."; // Script to invoke a contract
  signers: [
    {
      account: context.owner,
      scopes: "CalledByEntry"
    }
  ]
});

if (!txCreate.success) {
  console.error("Failed to create transaction:", txCreate.error);
  return { success: false, error: txCreate.error };
}

const txId = txCreate.txId;

// Sign the transaction
const txSign = context.transaction.sign(txId);
if (!txSign.success) {
  console.error("Failed to sign transaction:", txSign.error);
  return { success: false, error: txSign.error };
}

// Send the transaction
const txSend = context.transaction.send(txId);
if (!txSend.success) {
  console.error("Failed to send transaction:", txSend.error);
  return { success: false, error: txSend.error };
}

// Get the transaction hash
const txHash = txSend.hash;
console.log("Transaction sent with hash:", txHash);

// Check status (can be polled)
const txStatus = context.transaction.status(txId);
console.log("Transaction status:", txStatus.status);
```

## Functions Service

The Functions Service allows invoking other serverless functions.

### Methods

- **invoke(functionId, args)** - Invokes another function with the specified arguments

### Example

```javascript
// Invoke another function
const result = context.functions.invoke("data-aggregator", {
  sources: ["binance", "coinbase"],
  tokens: ["NEO", "GAS"],
  timeframe: "1h"
});

if (result.success) {
  console.log("Aggregated data:", result.result);
} else {
  console.error("Function invocation failed:", result.error);
}
```

## Trigger Service

The Trigger Service enables creating and managing event triggers.

### Methods

- **create(triggerType, triggerConfig)** - Creates a new trigger
- **update(triggerId, triggerConfig)** - Updates an existing trigger
- **delete(triggerId)** - Deletes a trigger
- **list()** - Lists all triggers for the function

### Event Methods

- **onBlockchain(eventConfig, handlerFunctionId)** - Registers a blockchain event handler
- **onSchedule(cronExpression, handlerFunctionId)** - Registers a time-based event handler
- **onAPI(endpoint, handlerFunctionId)** - Registers an API endpoint handler

### Example

```javascript
// Create a blockchain event trigger for contract events
const neoTransferEvent = context.event.onBlockchain({
  contract: "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5",
  eventName: "Transfer"
}, "transfer-handler-function");

// Create a scheduled trigger
const dailyReport = context.trigger.create("schedule", {
  cronExpression: "0 0 12 * * ?", // Noon every day
  functionId: "generate-daily-report",
  parameters: { format: "pdf" }
});

// List all triggers
const allTriggers = context.trigger.list();
console.log("Active triggers:", allTriggers);
```

## Security Context

Functions receive a security context that identifies the owner and caller:

```javascript
function main(args) {
  console.log("Function owner:", context.owner);
  console.log("Function caller:", context.caller);
  
  // Additional context properties
  console.log("Function ID:", context.functionId);
  console.log("Execution ID:", context.executionId);
  console.log("Trace ID:", context.traceId);
  
  return { success: true };
}
```