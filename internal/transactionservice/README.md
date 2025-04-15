# Transaction Service

The Transaction Service provides functionality for creating, signing, sending, and managing blockchain transactions within the Neo Service Layer.

## Features

- Create transactions with various configurations
- Sign transactions using secure key management
- Send transactions to the blockchain
- Check transaction status
- Retrieve transaction details
- List transactions with filtering options
- Estimate transaction fees

## Architecture

The Transaction Service consists of the following components:

1. **Service Interface**
   - Defines the contract for transaction operations
   - Enables easy mocking for testing

2. **Service Implementation**
   - Implements the transaction operations
   - Handles validation and error handling
   - Manages transaction state

3. **Transaction Repository**
   - Stores transaction data
   - Provides CRUD operations for transactions

4. **Transaction Models**
   - Defines the data structures for transactions
   - Includes status enums and validation rules

## API

The Transaction Service exposes the following methods:

- `Create(config map[string]interface{}) (string, error)` - Create a new transaction
- `Sign(id string) (map[string]interface{}, error)` - Sign a transaction
- `Send(id string) (string, error)` - Send a transaction to the blockchain
- `Status(hash string) (string, error)` - Get the status of a transaction
- `Get(id string) (map[string]interface{}, error)` - Get transaction details
- `List() ([]interface{}, error)` - List transactions
- `EstimateFee(config map[string]interface{}) (string, error)` - Estimate transaction fee

## Configuration

The Transaction Service can be configured with the following options:

- Network selection (mainnet, testnet)
- Gas price strategy
- Transaction timeout settings
- Retry policy

## Integration

The Transaction Service integrates with:

- Blockchain nodes for transaction submission
- Key management service for signing
- Event system for transaction status updates
- Metrics for monitoring transaction performance

## Metrics

The Transaction Service tracks the following metrics:

- Transaction creation rate
- Transaction success/failure rate
- Average transaction confirmation time
- Fee estimation accuracy

## Error Handling

The Transaction Service provides detailed error information for:

- Validation errors
- Network errors
- Blockchain errors
- Signing errors

## Example Usage

```go
// Create a transaction
txId, err := transactionService.Create(map[string]interface{}{
    "type": "transfer",
    "to": "NeoRecipientAddress",
    "value": "1.5",
    "asset": "NEO",
})

// Sign the transaction
signedTx, err := transactionService.Sign(txId)

// Send the transaction
txHash, err := transactionService.Send(txId)

// Check transaction status
status, err := transactionService.Status(txHash)
```