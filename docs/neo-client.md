# Neo Client Implementation

This document describes the Neo Client implementation for interacting with the Neo N3 blockchain.

## Overview

The Neo Client provides a Go interface for interacting with Neo N3 blockchain nodes through their RPC endpoints. It handles:

- Smart contract invocation
- Transaction submission
- Application log retrieval
- Network fee calculation
- Block height queries
- Network magic number retrieval

## Configuration

The client can be configured with the following options:

```go
type ClientConfig struct {
    NodeURLs []string  // Array of Neo RPC node URLs
    Timeout  int       // Request timeout in seconds
    Retries  int       // Number of retry attempts for failed requests
}
```

## Features

### Multiple Node Support

The client supports multiple Neo RPC nodes with automatic failover:

- If a request to one node fails, the client will try the next node
- Node rotation happens automatically after failures
- The client tracks the last successful node and tries it first on subsequent requests

### Retry Logic

Built-in retry mechanism for transient failures:

- Configurable number of retry attempts
- Exponential backoff between retries
- Different retry strategies for different types of errors

### Transaction Management

Complete transaction lifecycle management:

- Build and submit transactions
- Wait for transaction confirmation
- Retrieve transaction application logs

## Usage Examples

### Creating a Client

```go
// With default configuration
client := neo.NewClient(nil)

// Or with custom configuration
config := &neo.ClientConfig{
    NodeURLs: []string{
        "http://seed1.neo.org:10332",
        "http://seed2.neo.org:10332",
    },
    Timeout: 30,
    Retries: 3,
}
client := neo.NewClient(config)
```

### Invoking a Smart Contract

```go
result, err := client.InvokeFunction(
    "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5",  // Contract script hash
    "balanceOf",                                   // Method name
    []interface{}{                                 // Parameters
        "NZNos2WqTbu5oCgyfss9kUJgBXJqhuYAaj",
    },
)
```

### Sending a Transaction

```go
// Assuming you have a signed transaction
txHash, err := client.SendRawTransaction(signedTx)
if err != nil {
    // Handle error
}

// Wait for transaction to be confirmed
confirmedTx, err := client.WaitForTransaction(txHash.StringLE(), 60)
```

### Getting Application Logs

```go
appLog, err := client.GetApplicationLog(txHash.StringLE())
if err != nil {
    // Handle error
}

// Process the application log
for _, execution := range appLog.Executions {
    // Process execution results
}
```

## Error Handling

The client handles several error types:

1. Network errors - Retried with exponential backoff
2. RPC errors - Parsed and returned with descriptive messages
3. Node failures - Triggers failover to alternative nodes
4. Transaction errors - Distinguished between different failure modes

## Integration with Other Services

The Neo Client is designed to be used alongside other services:

```go
// Create the Neo client
neoClient := neo.NewClient(neoConfig)

// Create a wallet service for signing transactions
walletService := wallet.NewWalletService(walletConfig)

// Use them together in a higher-level service
triggerService := trigger.NewService(neoClient, walletService)
``` 