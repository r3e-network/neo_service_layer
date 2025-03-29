# JavaScript Interoperability

This document describes the JavaScript interoperability features of the Neo Service Layer Function Service, which enable JavaScript functions to interact with other Neo services.

## Overview

The Neo Service Layer Function Service provides a JavaScript runtime environment that allows functions to interact with other Neo services, including:

- Functions Service: Invoke other serverless functions
- Gas Bank Service: Access Neo gas management capabilities
- Price Feed Service: Get oracle price data
- Secrets Service: Store and retrieve sensitive information
- Trigger Service: Set up and manage event-based triggers
- Transaction Service: Construct and send blockchain transactions

This interoperability is achieved through a function context object that is automatically injected into the JavaScript execution environment.

## Function Context

The function context object provides the following:

1. **Function Metadata**: Information about the current function execution
2. **Service Access**: Methods for interacting with Neo services
3. **Authentication**: Automatic authentication with Neo services
4. **Helper Methods**: Simplified methods for common operations

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

### Function Context Methods

| Method | Description |
|--------|-------------|
| `log(message)` | Log a message |
| `getSecret(key)` | Get a secret value |
| `getGasPrice()` | Get current gas price |
| `getPrice(symbol)` | Get price for a symbol |
| `invokeFunction(functionId, parameters)` | Invoke another function |
| `createTrigger(triggerConfig)` | Create a new trigger |
| `getTrigger(triggerId)` | Get trigger details |
| `updateTrigger(triggerId, updates)` | Update an existing trigger |
| `deleteTrigger(triggerId)` | Delete a trigger |
| `listTriggers()` | List all triggers for the function owner |
| `executeTrigger(triggerId)` | Manually execute a trigger |
| `onEvent(eventType, callback)` | Register an event handler |

### Transaction Service Methods

The function context provides the following methods for managing blockchain transactions:

| Method | Description |
|--------|-------------|
| `transaction.create(txConfig)` | Create a new blockchain transaction |
| `transaction.sign(txId)` | Sign a transaction with the function owner's key |
| `transaction.send(txId)` | Send a transaction to the blockchain |
| `transaction.status(txId)` | Get the current status of a transaction |
| `transaction.get(txId)` | Get transaction details |
| `transaction.list(page, pageSize, status)` | List transactions created by the function owner |
| `transaction.estimateFee(txConfig)` | Estimate the fee for a transaction |

#### Transaction Configuration

When creating a transaction, you can specify the following properties:

```javascript
const txConfig = {
  type: 'transfer',           // Transaction type (transfer, invoke, claim)
  asset: 'NEO',               // Asset symbol (NEO, GAS, etc.)
  from: context.owner,        // Sender address (defaults to function owner)
  to: 'NXV7ZhHiyME6WHymWxTNzYXYhBYLZQYKEn', // Recipient address
  amount: 1.0,                // Amount to transfer
  gasPrice: 1000,             // Gas price in GAS units (optional)
  systemFee: 0.001,           // System fee in GAS units (optional)
  networkFee: 0.001,          // Network fee in GAS units (optional)
  memo: 'Payment for services', // Optional memo
  params: [],                 // Optional parameters for contract invocation
}
```

#### Transaction Status

Transaction status can be one of the following:

- `created`: Transaction has been created but not signed
- `signed`: Transaction has been signed but not sent
- `sent`: Transaction has been sent to the blockchain
- `pending`: Transaction is pending confirmation
- `confirmed`: Transaction has been confirmed
- `failed`: Transaction failed to execute

#### Example: Creating and Sending a Transaction

```javascript
// Create a transaction
const tx = context.transaction.create({
  type: 'transfer',
  asset: 'NEO',
  to: recipientAddress,
  amount: 5.0,
  memo: 'Payment from Neo Service Layer function'
});

// Sign the transaction
const signedTx = context.transaction.sign(tx.txId);

// Send the transaction
const sentTx = context.transaction.send(tx.txId);

// Check transaction status
const status = context.transaction.status(tx.txId);

return {
  txId: tx.txId,
  hash: sentTx.hash,
  status: status.status
};
```

## JavaScript Function Structure

JavaScript functions should follow this structure:

```javascript
// Function code
function main(args) {
  // Access function context
  const functionId = context.functionId;
  const executionId = context.executionId;
  
  // Log messages
  context.log('Function started');
  
  // Get secrets
  const apiKey = await context.getSecret('api-key');
  
  // Get price data
  const neoPrice = await context.getPrice('NEO');
  
  // Invoke another function
  const result = await context.invokeFunction('another-function', {
    price: neoPrice
  });
  
  // Return result
  return {
    success: true,
    price: neoPrice,
    result
  };
}
```

## Authentication

Authentication is handled automatically for JavaScript functions. The function context includes the necessary authentication information to interact with Neo services.

## Configuration

The following configuration options are available for JavaScript interoperability:

| Option | Description | Default |
|--------|-------------|---------|
| `ServiceLayerURL` | URL for the Neo Service Layer API | `http://localhost:3000` |
| `EnableInteroperability` | Enable/disable interoperability features | `true` |

## Implementation Details

### Function Execution Flow

1. A function is invoked through the Neo Function Service
2. The Function Service creates an execution environment
3. The function context is injected into the JavaScript environment
4. The function is executed with access to the context
5. The function interacts with Neo services through the context
6. The function returns a result, which is sent back to the caller

### Security Considerations

1. **Authentication**: The function context includes authentication headers based on the function execution context
2. **Permissions**: Functions can only access services and resources they have permission to access
3. **Isolation**: Each function execution runs in its own isolated sandbox

## Trigger Management

JavaScript functions can create and manage triggers that execute functions in response to events.

### Creating Triggers

```javascript
function main(args) {
  // Create a blockchain event trigger
  const blockchainTrigger = await context.createTrigger({
    name: 'NEO Transfer Trigger',
    description: 'Trigger on NEO token transfers',
    type: 'blockchain',
    handler: 'blockchain',
    parameters: JSON.stringify({
      contractHash: '0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5',
      eventName: 'Transfer',
      minAmount: 100
    }),
    config: {
      functionId: 'process-transfer-function',
      retryCount: '3'
    }
  });
  
  // Create a time-based trigger (cron)
  const timeTrigger = await context.createTrigger({
    name: 'Daily Price Check',
    description: 'Check prices every day at 8:00 AM',
    type: 'time',
    handler: 'time',
    parameters: JSON.stringify({
      cronExpression: '0 0 8 * * *',
      timezone: 'UTC'
    }),
    config: {
      functionId: 'daily-price-check-function'
    }
  });
  
  // Create an API trigger
  const apiTrigger = await context.createTrigger({
    name: 'API Webhook',
    description: 'Trigger on API webhook calls',
    type: 'api',
    handler: 'api',
    parameters: JSON.stringify({
      method: 'POST',
      path: '/webhooks/price-alert',
      requireAuth: true
    }),
    config: {
      functionId: 'webhook-handler-function'
    }
  });
  
  return {
    blockchainTriggerId: blockchainTrigger.id,
    timeTriggerId: timeTrigger.id,
    apiTriggerId: apiTrigger.id
  };
}
```

### Managing Triggers

```javascript
function main(args) {
  const { triggerId } = args;
  
  // Get trigger details
  const trigger = await context.getTrigger(triggerId);
  context.log(`Trigger name: ${trigger.name}`);
  
  // Update trigger
  const updatedTrigger = await context.updateTrigger(triggerId, {
    description: 'Updated trigger description',
    parameters: JSON.stringify({
      cronExpression: '0 0 12 * * *', // Change to noon
      timezone: 'UTC'
    })
  });
  
  // List all triggers
  const triggers = await context.listTriggers();
  context.log(`Found ${triggers.length} triggers`);
  
  // Manually execute a trigger
  const execution = await context.executeTrigger(triggerId);
  context.log(`Trigger execution status: ${execution.status}`);
  
  // Delete a trigger
  await context.deleteTrigger(triggerId);
  
  return {
    success: true,
    triggerCount: triggers.length,
    execution
  };
}
```

## Event Handling

JavaScript functions can register event handlers to respond to events:

```javascript
function main(args) {
  // Register blockchain event handler
  context.onEvent('blockchain:Transfer', async (event) => {
    const { from, to, amount } = event.parameters;
    context.log(`Transfer: ${amount} from ${from} to ${to}`);
    
    // Process the event
    await processTransfer(from, to, amount);
  });
  
  // Register time event handler
  context.onEvent('time:daily', async (event) => {
    context.log('Daily event triggered');
    
    // Get current prices
    const neoPrice = await context.getPrice('NEO');
    const gasPrice = await context.getPrice('GAS');
    
    // Store prices
    await storePrices(neoPrice, gasPrice);
  });
  
  // Register API event handler
  context.onEvent('api:webhook', async (event) => {
    const { body, headers } = event;
    context.log(`Webhook received: ${JSON.stringify(body)}`);
    
    // Process webhook data
    await processWebhook(body, headers);
  });
  
  return { success: true, message: 'Event handlers registered' };
}

async function processTransfer(from, to, amount) {
  // Process transfer logic
}

async function storePrices(neoPrice, gasPrice) {
  // Store prices logic
}

async function processWebhook(body, headers) {
  // Process webhook logic
}
```

## Complex Example

Here's a more complex example that combines multiple interoperability features:

```javascript
function main(args) {
  // Log function execution
  context.log('Function started');
  
  // Get parameters
  const { symbol = 'NEO', threshold = 50 } = args;
  
  // Get current price from price feed service
  const currentPrice = await context.getPrice(symbol);
  context.log(`Current ${symbol} price: $${currentPrice}`);
  
  // Get a secret value
  const apiKey = await context.getSecret('external-api-key');
  
  // Create a price alert trigger
  const trigger = await context.createTrigger({
    name: `${symbol} Price Alert`,
    description: `Trigger when ${symbol} price crosses $${threshold}`,
    type: 'condition',
    handler: 'price-condition',
    parameters: JSON.stringify({
      symbol,
      threshold,
      operator: 'above'
    }),
    config: {
      functionId: 'price-alert-handler',
      notificationEmail: 'user@example.com'
    }
  });
  
  // Register an event handler for price alerts
  context.onEvent('price:alert', async (event) => {
    const { symbol, price, threshold } = event;
    context.log(`Price alert: ${symbol} at $${price} crossed threshold $${threshold}`);
    
    // Create and send a transaction based on the price alert
    const transaction = await context.createTransaction({
      type: 'transfer',
      asset: symbol,
      from: context.owner,
      to: 'NeoTreasuryAddress',
      amount: 1.0
    });
    
    const signedTx = await context.signTransaction(transaction);
    const txResult = await context.sendTransaction(signedTx);
    
    // Invoke another function with the transaction result
    await context.invokeFunction('record-transaction', {
      symbol,
      price,
      threshold,
      txid: txResult.txid
    });
  });
  
  return {
    success: true,
    symbol,
    currentPrice,
    threshold,
    triggerId: trigger.id
  };
}
```
