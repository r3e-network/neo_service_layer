# Transaction Service Integration with Sandbox

## Overview

The Neo Service Layer provides a transaction service that allows JavaScript functions to create, sign, send, and manage blockchain transactions. This document describes the integration between the transaction service and the JavaScript sandbox, including the testing approach and implementation details.

## Transaction Service Methods

The transaction service exposes the following methods to JavaScript functions:

| Method | Description |
|--------|-------------|
| `create` | Creates a new blockchain transaction with the specified configuration |
| `sign` | Signs a transaction with the function owner's key |
| `send` | Sends a signed transaction to the blockchain |
| `status` | Gets the current status of a transaction |
| `get` | Gets detailed information about a transaction |
| `list` | Lists transactions created by the function owner |
| `estimateFee` | Estimates the fee for a transaction |

## Mock Implementation for Testing

For testing purposes, a mock implementation of the transaction service is used. This allows tests to verify the integration without requiring actual blockchain interactions.

```go
// MockTransactionService is a mock implementation of the transaction service interface
type MockTransactionService struct {
    mock.Mock
}

func (m *MockTransactionService) Create(config map[string]interface{}) (string, error) {
    args := m.Called(config)
    return args.String(0), args.Error(1)
}

func (m *MockTransactionService) Sign(id string, account *wallet.Account) (map[string]interface{}, error) {
    args := m.Called(id, account)
    return args.Get(0).(map[string]interface{}), args.Error(1)
}

func (m *MockTransactionService) Send(ctx context.Context, id string) (string, error) {
    args := m.Called(ctx, id)
    return args.String(0), args.Error(1)
}

func (m *MockTransactionService) Status(hash string) (string, error) {
    args := m.Called(hash)
    return args.String(0), args.Error(1)
}

func (m *MockTransactionService) Get(id string) (map[string]interface{}, error) {
    args := m.Called(id)
    return args.Get(0).(map[string]interface{}), args.Error(1)
}

func (m *MockTransactionService) List() ([]interface{}, error) {
    args := m.Called()
    return args.Get(0).([]interface{}), args.Error(1)
}

func (m *MockTransactionService) EstimateFee(config map[string]interface{}) (string, error) {
    args := m.Called(config)
    return args.String(0), args.Error(1)
}
```

## Test Cases

The integration tests cover the following scenarios:

1. **Transaction Creation**: Tests that a transaction can be created with the specified configuration
2. **Transaction Signing**: Tests that a transaction can be signed with the function owner's key
3. **Transaction Sending**: Tests that a signed transaction can be sent to the blockchain
4. **Transaction Status**: Tests that the status of a transaction can be retrieved
5. **Transaction Details**: Tests that detailed information about a transaction can be retrieved
6. **Transaction Listing**: Tests that transactions created by the function owner can be listed
7. **Fee Estimation**: Tests that the fee for a transaction can be estimated

## Test Implementation

Each test case follows this pattern:

1. Set up the mock transaction service with expected behavior
2. Create a sandbox with the mock service
3. Create a function context with the mock service
4. Execute JavaScript code that calls the transaction service method
5. Verify that the result matches the expected output

Example test for transaction creation:

```go
t.Run("transaction.create", func(t *testing.T) {
    // Create a JavaScript function context
    jsContext := sandbox.createFunctionContext(functionContext, []string{})
    vm.Set("context", jsContext)

    // Execute JavaScript code to call transaction.create
    result, err := vm.RunString(`
        const txConfig = {
            type: "transfer",
            to: "recipient-456",
            amount: "100",
            asset: "NEO",
            network: "testnet"
        };
        context.transaction.create(txConfig);
    `)

    // Assert the result
    assert.NoError(t, err)
    assert.NotNil(t, result)
    resultObj := result.Export().(map[string]interface{})
    assert.Equal(t, true, resultObj["success"])
    assert.Equal(t, "tx-123", resultObj["txId"])
})
```

## JavaScript API

The JavaScript API for transaction service methods follows a consistent pattern:

1. Each method returns an object with a `success` field indicating whether the operation was successful
2. Additional fields provide relevant information about the operation result
3. If an error occurs, the object includes an `error` field with details

Example JavaScript usage:

```javascript
// Create a transaction
const createResult = context.transaction.create({
    type: "transfer",
    to: "recipient-456",
    amount: "100",
    asset: "NEO",
    network: "testnet"
});

if (createResult.success) {
    // Sign the transaction
    const signResult = context.transaction.sign(createResult.txId);
    
    if (signResult.success) {
        // Send the transaction
        const sendResult = context.transaction.send(signResult.txId);
        
        if (sendResult.success) {
            // Check the transaction status
            const statusResult = context.transaction.status(sendResult.hash);
            console.log(`Transaction status: ${statusResult.status}`);
        }
    }
}
```

## Integration with Sandbox

The transaction service is integrated with the sandbox through the `createFunctionContext` method, which creates a JavaScript object with methods for interacting with Neo services. The transaction service methods are exposed as properties of the `transaction` object in the function context.

This integration allows JavaScript functions to create and manage blockchain transactions in a secure and controlled environment.
