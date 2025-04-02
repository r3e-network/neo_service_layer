# Wallet Service Implementation

This document describes the wallet service implementation for the Neo N3 blockchain integration.

## Overview

The wallet service provides functionality for managing Neo N3 wallets and signing transactions. It uses the Neo-Go SDK to implement wallet operations including:

- Loading existing wallets
- Creating new wallets if none exist
- Decrypting accounts with passwords
- Transaction signing

## Configuration

The wallet service can be configured with the following options:

```go
type Config struct {
    WalletPath     string  // Path to the wallet file
    Password       string  // Password for the wallet
    AddressVersion byte    // Address version for Neo (typically 0x35 for N3)
    NetworkMagic   uint32  // Network magic number
}
```

## Usage

### Creating a Wallet Service

```go
// With default configuration
walletService := wallet.NewWalletService(nil)

// Or with custom configuration
config := &wallet.Config{
    WalletPath:     "/path/to/wallet.json",
    Password:       "your-password",
    AddressVersion: 0x35,
    NetworkMagic:   860833102, // Neo N3 MainNet
}
walletService := wallet.NewWalletService(config)
```

### Signing Transactions

```go
// Create a transaction
tx := &transaction.Transaction{
    // Transaction fields...
}

// Add a signer with the account you want to sign with
signer := transaction.Signer{
    Account: accountScriptHash,
    Scopes:  transaction.CalledByEntry,
}
tx.Signers = append(tx.Signers, signer)

// Sign the transaction
err := walletService.SignTx(context.Background(), accountScriptHash, tx)
if err != nil {
    // Handle error
}

// Transaction is now signed and ready to be sent
```

## Error Handling

The service handles several error scenarios:

1. Wallet file not found - Creates a new wallet automatically
2. Account not found in wallet - Returns descriptive error
3. Decryption failures - Returns detailed error message
4. Account not found in transaction signers - Validates before attempting to sign

## Implementation Notes

- The service automatically creates a new wallet if one doesn't exist at the specified path
- Account passwords are only used when needed and not stored in memory
- The service implementation follows Neo N3 transaction signing specifications
- Network magic is used to prevent transaction replay across different networks

## Integration with NeoClient

The wallet service is designed to work alongside the Neo client for a complete blockchain interaction experience:

```go
neoClient := neo.NewClient(neoConfig)
walletService := wallet.NewWalletService(walletConfig)

// Use them together in a service
service := trigger.NewService(neoClient, walletService)
``` 