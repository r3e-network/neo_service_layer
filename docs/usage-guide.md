# Neo N3 Service Layer Usage Guide

This guide provides instructions on how to use the Neo N3 Service Layer in your applications.

## Overview

The Neo N3 Service Layer provides a comprehensive suite of Go-based services for interacting with the Neo N3 blockchain. It includes:

1. **Neo Client** - For blockchain RPC interactions
2. **Wallet Service** - For managing wallets and signing transactions
3. **Trigger Service** - For executing contract actions and handling automation

## Getting Started

### Installation

```bash
go get github.com/your-org/neo_service_layer
```

### Basic Usage

Import the required packages:

```go
import (
    "context"
    
    "github.com/your-org/neo_service_layer/internal/core/neo"
    "github.com/your-org/neo_service_layer/internal/services/wallet"
    "github.com/your-org/neo_service_layer/internal/services/trigger"
)
```

## Neo Client Configuration

The Neo Client is used for all blockchain interactions:

```go
// Configure the Neo client
neoConfig := &neo.ClientConfig{
    NodeURLs: []string{
        "http://seed1.neo.org:10332",
        "http://seed2.neo.org:10332",
    },
    Timeout: 30,  // seconds
    Retries: 3,
}

// Create a Neo client
neoClient, err := neo.NewClient(neoConfig)
if err != nil {
    log.Fatalf("Failed to create Neo client: %v", err)
}
defer neoClient.Close()
```

### Smart Contract Invocation

```go
// Invoke a read-only smart contract method
result, err := neoClient.InvokeFunction(
    "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5",  // Contract script hash
    "balanceOf",                                   // Method name
    []interface{}{                                 // Parameters
        "NZNos2WqTbu5oCgyfss9kUJgBXJqhuYAaj",     // Address
    },
)
if err != nil {
    log.Fatalf("Invocation failed: %v", err)
}

// Process the result
fmt.Println("Invocation result:", result)
```

## Wallet Service Configuration

The Wallet Service manages NEO wallets and transaction signing:

```go
// Configure the wallet service
walletConfig := &wallet.Config{
    WalletPath:     "/path/to/wallet.json",
    Password:       "your-wallet-password",
    AddressVersion: 0x35,  // Neo N3 MainNet
    NetworkMagic:   860833102,  // Neo N3 MainNet
}

// Create a wallet service
walletService := wallet.NewWalletService(walletConfig)
```

## Building and Signing Transactions

```go
// Create a transaction (simplified example)
tx := &transaction.Transaction{
    // ... transaction details
}

// Add a signer
signer := transaction.Signer{
    Account: accountScriptHash,  // Your account
    Scopes:  transaction.CalledByEntry,
}
tx.Signers = append(tx.Signers, signer)

// Calculate network fee
fee, err := neoClient.CalculateNetworkFee(tx)
if err != nil {
    log.Fatalf("Failed to calculate fee: %v", err)
}
tx.NetworkFee = fee

// Sign the transaction
err = walletService.SignTx(context.Background(), accountScriptHash, tx)
if err != nil {
    log.Fatalf("Failed to sign transaction: %v", err)
}

// Send the transaction
txHash, err := neoClient.SendRawTransaction(tx)
if err != nil {
    log.Fatalf("Failed to send transaction: %v", err)
}

// Wait for confirmation
confirmed, err := neoClient.WaitForTransaction(txHash.StringLE(), 60)
if err != nil {
    log.Fatalf("Transaction confirmation failed: %v", err)
}

if confirmed {
    fmt.Println("Transaction confirmed:", txHash.StringLE())
    
    // Get the application log
    appLog, err := neoClient.GetApplicationLog(txHash.StringLE())
    if err != nil {
        log.Fatalf("Failed to get application log: %v", err)
    }
    
    fmt.Println("Application log:", appLog)
}
```

## Trigger Service

The Trigger Service provides high-level operations for executing contract actions:

```go
// Configure trigger service
triggerConfig := &trigger.Config{
    MaxConcurrentTriggers: 10,
}

// Create trigger service
triggerService := trigger.NewService(triggerConfig, neoClient, walletService)

// Define a contract action
action := &trigger.ContractAction{
    Contract: "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5",
    Method:   "transfer",
    Params: []interface{}{
        "NZNos2WqTbu5oCgyfss9kUJgBXJqhuYAaj",  // From
        "Nhfg3TbpwogLvDGVvAvqyThbsHgoSUKwtn",  // To
        100000000,  // Amount (1 GAS)
    },
    Signer: accountScriptHash,
}

// Execute the action
result, err := triggerService.ExecuteAction(action)
if err != nil {
    log.Fatalf("Action execution failed: %v", err)
}

// Process the result
fmt.Println("Action executed successfully:", result)
```

## Error Handling and Retries

The service layer includes built-in retry mechanisms:

1. **Neo Client** has configurable retries for RPC errors
2. **Transaction submission** includes wait-for-confirmation logic
3. **Trigger Service** handles errors with appropriate retry policies

```go
// Example of configuring more aggressive retries for unreliable networks
neoConfig := &neo.ClientConfig{
    NodeURLs: []string{
        "http://seed1.neo.org:10332",
        "http://seed2.neo.org:10332",
        "http://seed3.neo.org:10332",
        "http://seed4.neo.org:10332",
    },
    Timeout: 60,  // seconds
    Retries: 5,
}
```

## Best Practices

1. **Multiple RPC Nodes** - Always configure multiple Neo RPC nodes for reliability
2. **Timeout Handling** - Set appropriate timeouts for your network conditions
3. **Error Handling** - Always check error returns and implement proper handling
4. **Wallet Security** - Store wallet passwords securely, not in code
5. **Transaction Verification** - Always wait for and verify transaction confirmations

## Debugging Tips

1. Enable debug logging to see detailed RPC interactions:
   ```go
   log.SetLevel(log.DebugLevel)
   ```

2. Use shorter timeouts during development:
   ```go
   neoConfig.Timeout = 10 // seconds
   ```

3. Verify blockchain interactions with a block explorer before production use

## Development vs Production

For development, you can use:
- TestNet nodes instead of MainNet
- Mock implementations of services
- Shorter confirmation timeouts
- More verbose logging

For production:
- Use multiple reliable MainNet nodes
- Implement proper error handling and recovery
- Set appropriate timeouts for transaction confirmation
- Consider implementing circuit breakers for service protection 