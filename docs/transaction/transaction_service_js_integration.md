# Transaction Service JavaScript Integration

## Overview

The Neo Service Layer provides JavaScript functions with access to blockchain transaction capabilities through the transaction service. This document describes how JavaScript functions can create, sign, send, and manage blockchain transactions using the transaction service methods exposed in the function context.

## Transaction Service Methods

The following methods are available in the JavaScript function context under the `transaction` namespace:

| Method | Description | Parameters | Return Value |
|--------|-------------|------------|--------------|
| `create` | Creates a new blockchain transaction | Transaction configuration object | Object with transaction ID and status |
| `sign` | Signs a transaction with the function owner's key | Transaction ID | Object with transaction details |
| `send` | Sends a signed transaction to the blockchain | Transaction ID | Object with transaction hash and status |
| `status` | Gets the current status of a transaction | Transaction hash | Object with transaction status |
| `get` | Gets detailed information about a transaction | Transaction ID | Object with transaction details |
| `list` | Lists transactions created by the function owner | Optional filter object | Array of transaction objects |
| `estimateFee` | Estimates the fee for a transaction | Transaction configuration object | Object with fee estimate |

## Method Details

### transaction.create(config)

Creates a new blockchain transaction with the specified configuration.

**Parameters:**
- `config` (Object): Transaction configuration object with the following properties:
  - `type` (String): Transaction type (e.g., "transfer", "invoke")
  - `to` (String): Recipient address (for transfer transactions)
  - `amount` (String): Amount to transfer (for transfer transactions)
  - `asset` (String): Asset to transfer (for transfer transactions)
  - `contract` (String): Contract hash (for invoke transactions)
  - `method` (String): Contract method (for invoke transactions)
  - `params` (Array): Method parameters (for invoke transactions)
  - `network` (String, optional): Blockchain network (defaults to "testnet")

**Returns:**
```javascript
{
  success: true,
  txId: "tx-123",
  config: { /* Transaction configuration */ },
  status: "created"
}
```

### transaction.sign(txId)

Signs a transaction with the function owner's key.

**Parameters:**
- `txId` (String): Transaction ID to sign

**Returns:**
```javascript
{
  success: true,
  txId: "tx-123",
  status: "signed"
}
```

### transaction.send(txId)

Sends a signed transaction to the blockchain.

**Parameters:**
- `txId` (String): Transaction ID to send

**Returns:**
```javascript
{
  success: true,
  txId: "tx-123",
  hash: "0xabcdef123456789",
  status: "sent"
}
```

### transaction.status(hash)

Gets the current status of a transaction.

**Parameters:**
- `hash` (String): Transaction hash

**Returns:**
```javascript
{
  success: true,
  txId: "0xabcdef123456789",
  status: "pending" // or "confirmed", "failed", etc.
}
```

### transaction.get(txId)

Gets detailed information about a transaction.

**Parameters:**
- `txId` (String): Transaction ID

**Returns:**
```javascript
{
  success: true,
  transaction: {
    id: "tx-123",
    hash: "0xabcdef123456789",
    status: "pending",
    type: "transfer",
    from: "owner-123",
    to: "recipient-456",
    amount: "100",
    asset: "NEO",
    network: "testnet"
  }
}
```

### transaction.list(filter)

Lists transactions created by the function owner.

**Parameters:**
- `filter` (Object, optional): Filter criteria

**Returns:**
```javascript
{
  success: true,
  transactions: [
    {
      id: "tx-123",
      hash: "0xabcdef123456789",
      status: "pending",
      type: "transfer",
      from: "owner-123",
      to: "recipient-456",
      amount: "100",
      asset: "NEO",
      network: "testnet"
    }
    // Additional transactions...
  ]
}
```

### transaction.estimateFee(config)

Estimates the fee for a transaction.

**Parameters:**
- `config` (Object): Transaction configuration object (same as for `create`)

**Returns:**
```javascript
{
  success: true,
  fee: "0.001",
  asset: "GAS",
  network: "testnet"
}
```

## Transaction Status Values

Transactions can have the following status values:

| Status | Description |
|--------|-------------|
| `created` | Transaction has been created but not signed |
| `signed` | Transaction has been signed but not sent |
| `sent` | Transaction has been sent to the blockchain |
| `pending` | Transaction is pending confirmation |
| `confirmed` | Transaction has been confirmed |
| `failed` | Transaction failed to execute |

## Error Handling

All transaction service methods return an object with a `success` field indicating whether the operation was successful. If `success` is `false`, an `error` field provides details about the error:

```javascript
{
  success: false,
  error: "Transaction ID is required"
}
```

## Complete Example

The following example demonstrates how to create, sign, send, and check the status of a transaction:

```javascript
// Create a transaction
const createResult = context.transaction.create({
  type: "transfer",
  to: "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
  amount: "1.0",
  asset: "NEO",
  network: "testnet"
});

if (createResult.success) {
  context.log(`Transaction created with ID: ${createResult.txId}`);
  
  // Sign the transaction
  const signResult = context.transaction.sign(createResult.txId);
  
  if (signResult.success) {
    context.log(`Transaction signed: ${signResult.status}`);
    
    // Send the transaction
    const sendResult = context.transaction.send(signResult.txId);
    
    if (sendResult.success) {
      context.log(`Transaction sent with hash: ${sendResult.hash}`);
      
      // Check the transaction status
      const statusResult = context.transaction.status(sendResult.hash);
      context.log(`Transaction status: ${statusResult.status}`);
      
      // Get transaction details
      const txDetails = context.transaction.get(createResult.txId);
      context.log(`Transaction details: ${JSON.stringify(txDetails.transaction)}`);
    } else {
      context.error(`Failed to send transaction: ${sendResult.error}`);
    }
  } else {
    context.error(`Failed to sign transaction: ${signResult.error}`);
  }
} else {
  context.error(`Failed to create transaction: ${createResult.error}`);
}

// Estimate fee for a transaction
const feeResult = context.transaction.estimateFee({
  type: "transfer",
  to: "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
  amount: "1.0",
  asset: "NEO"
});

if (feeResult.success) {
  context.log(`Estimated fee: ${feeResult.fee} ${feeResult.asset}`);
} else {
  context.error(`Failed to estimate fee: ${feeResult.error}`);
}
```

## Implementation Details

The transaction service methods are implemented in the `createFunctionContext` method of the `Sandbox` struct. Each method validates its parameters, calls the corresponding method on the transaction service, and returns a standardized response object.

The transaction service uses the Neo blockchain client to interact with the blockchain, and it maintains an in-memory store of transactions for testing purposes. In a production environment, transactions would be stored in a persistent database.

## Security Considerations

- Transaction signing uses the function owner's key, which is securely managed by the Neo Service Layer.
- Transactions are validated before being sent to the blockchain to prevent malicious operations.
- Gas limits and fee estimates help prevent excessive resource consumption.
- Transaction history is only accessible to the function owner or authorized callers.

## Testing

The transaction service integration is thoroughly tested using mock implementations of the transaction service. Tests cover all transaction methods and error cases to ensure reliable operation in the sandbox environment.
