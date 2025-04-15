# Wallet Service API Specification

This document outlines the complete API for the Wallet Service, including method signatures, parameters, and return values.

## Service Interface

The Wallet Service implements the following interface:

```go
// IService defines the interface for wallet management operations
type IService interface {
    // Service lifecycle
    Start() error
    Stop() error
    
    // Wallet management
    CreateWallet(ctx context.Context, name string, password string, overwrite bool) (*WalletInfo, error)
    OpenWallet(ctx context.Context, name string, password string) (*WalletInfo, error)
    CloseWallet(ctx context.Context, name string) error
    ListWallets(ctx context.Context) ([]*WalletInfo, error)
    GetWalletInfo(ctx context.Context, name string) (*WalletInfo, error)
    BackupWallet(ctx context.Context, name string, destination string) error
    RestoreWallet(ctx context.Context, source string, password string) (*WalletInfo, error)
    
    // Account management
    CreateAccount(ctx context.Context, walletName string, label string) (*AccountInfo, error)
    ListAccounts(ctx context.Context, walletName string) ([]*AccountInfo, error)
    GetAccountInfo(ctx context.Context, walletName string, address string) (*AccountInfo, error)
    GetAccountBalance(ctx context.Context, walletName string, address string, assetID string) (*BalanceInfo, error)
    
    // Signing operations
    SignTransaction(ctx context.Context, walletName string, address string, tx *transaction.Transaction) (*transaction.Transaction, error)
    SignMessage(ctx context.Context, walletName string, address string, message []byte) ([]byte, error)
    VerifySignature(ctx context.Context, message []byte, signature []byte, publicKey []byte) (bool, error)
    
    // Role-based wallet management
    AssignWalletToRole(ctx context.Context, walletName string, role string) error
    GetWalletForRole(ctx context.Context, role string) (*WalletInfo, error)
    
    // Multi-sig operations
    CreateMultiSigAccount(ctx context.Context, walletName string, signers []keys.PublicKey, threshold int) (*AccountInfo, error)
    AddSignatureToTx(ctx context.Context, partiallySignedTx *transaction.Transaction, walletName string, address string) (*transaction.Transaction, error)
}
```

## Data Types

### WalletInfo

```go
// WalletInfo provides information about a wallet without exposing private keys
type WalletInfo struct {
    Name string
    Path string
    Version int
    Accounts int
    IsOpen bool
    IsLocked bool
    ScryptParams *wallet.ScryptParams
    DefaultAccount string
    Extra map[string]interface{}
}
```

### AccountInfo

```go
// AccountInfo provides information about an account
type AccountInfo struct {
    Address string
    PublicKey []byte
    Label string
    IsDefault bool
    Contract *wallet.Contract
    // No private key exposed
}
```

### BalanceInfo

```go
// BalanceInfo contains balance information for an account
type BalanceInfo struct {
    Address string
    AssetID string
    AssetName string
    Balance string // String to handle large numbers precisely
    Decimals int
    Symbol string
}
```

## Method Details

### Service Lifecycle

#### Start

```go
Start() error
```

Initializes the wallet service, loading any configured default wallets.

**Returns:**
- `error`: Any error that occurred during startup

#### Stop

```go
Stop() error
```

Gracefully shuts down the wallet service, ensuring all wallets are properly closed.

**Returns:**
- `error`: Any error that occurred during shutdown

### Wallet Management

#### CreateWallet

```go
CreateWallet(ctx context.Context, name string, password string, overwrite bool) (*WalletInfo, error)
```

Creates a new NEP-6 wallet with the specified name and password.

**Parameters:**
- `ctx`: Context for the operation
- `name`: Wallet name/identifier
- `password`: Password to encrypt the wallet
- `overwrite`: Whether to overwrite if the wallet already exists

**Returns:**
- `*WalletInfo`: Information about the created wallet
- `error`: Any error that occurred during creation

#### OpenWallet

```go
OpenWallet(ctx context.Context, name string, password string) (*WalletInfo, error)
```

Opens an existing wallet and decrypts it with the provided password.

**Parameters:**
- `ctx`: Context for the operation
- `name`: Wallet name/identifier
- `password`: Password to decrypt the wallet

**Returns:**
- `*WalletInfo`: Information about the opened wallet
- `error`: Any error that occurred during opening

#### CloseWallet

```go
CloseWallet(ctx context.Context, name string) error
```

Closes an open wallet, freeing resources and securing keys.

**Parameters:**
- `ctx`: Context for the operation
- `name`: Wallet name/identifier

**Returns:**
- `error`: Any error that occurred during closing

#### ListWallets

```go
ListWallets(ctx context.Context) ([]*WalletInfo, error)
```

Lists all available wallets.

**Parameters:**
- `ctx`: Context for the operation

**Returns:**
- `[]*WalletInfo`: Array of wallet information
- `error`: Any error that occurred

#### GetWalletInfo

```go
GetWalletInfo(ctx context.Context, name string) (*WalletInfo, error)
```

Gets information about a specific wallet.

**Parameters:**
- `ctx`: Context for the operation
- `name`: Wallet name/identifier

**Returns:**
- `*WalletInfo`: Information about the wallet
- `error`: Any error that occurred

#### BackupWallet

```go
BackupWallet(ctx context.Context, name string, destination string) error
```

Creates a backup of a wallet to the specified destination.

**Parameters:**
- `ctx`: Context for the operation
- `name`: Wallet name/identifier
- `destination`: Path where the backup will be stored

**Returns:**
- `error`: Any error that occurred during backup

#### RestoreWallet

```go
RestoreWallet(ctx context.Context, source string, password string) (*WalletInfo, error)
```

Restores a wallet from a backup file.

**Parameters:**
- `ctx`: Context for the operation
- `source`: Path to the backup file
- `password`: Password to decrypt the wallet

**Returns:**
- `*WalletInfo`: Information about the restored wallet
- `error`: Any error that occurred during restoration

### Account Management

#### CreateAccount

```go
CreateAccount(ctx context.Context, walletName string, label string) (*AccountInfo, error)
```

Creates a new account in the specified wallet.

**Parameters:**
- `ctx`: Context for the operation
- `walletName`: Wallet name/identifier
- `label`: Label for the new account

**Returns:**
- `*AccountInfo`: Information about the created account
- `error`: Any error that occurred during creation

#### ListAccounts

```go
ListAccounts(ctx context.Context, walletName string) ([]*AccountInfo, error)
```

Lists all accounts in the specified wallet.

**Parameters:**
- `ctx`: Context for the operation
- `walletName`: Wallet name/identifier

**Returns:**
- `[]*AccountInfo`: Array of account information
- `error`: Any error that occurred

#### GetAccountInfo

```go
GetAccountInfo(ctx context.Context, walletName string, address string) (*AccountInfo, error)
```

Gets information about a specific account.

**Parameters:**
- `ctx`: Context for the operation
- `walletName`: Wallet name/identifier
- `address`: Account address

**Returns:**
- `*AccountInfo`: Information about the account
- `error`: Any error that occurred

#### GetAccountBalance

```go
GetAccountBalance(ctx context.Context, walletName string, address string, assetID string) (*BalanceInfo, error)
```

Gets the balance of a specific asset for an account.

**Parameters:**
- `ctx`: Context for the operation
- `walletName`: Wallet name/identifier
- `address`: Account address
- `assetID`: Asset identifier (empty for NEO/GAS)

**Returns:**
- `*BalanceInfo`: Balance information
- `error`: Any error that occurred

### Signing Operations

#### SignTransaction

```go
SignTransaction(ctx context.Context, walletName string, address string, tx *transaction.Transaction) (*transaction.Transaction, error)
```

Signs a transaction using the specified account.

**Parameters:**
- `ctx`: Context for the operation
- `walletName`: Wallet name/identifier
- `address`: Account address to sign with
- `tx`: Transaction to sign

**Returns:**
- `*transaction.Transaction`: Signed transaction
- `error`: Any error that occurred during signing

#### SignMessage

```go
SignMessage(ctx context.Context, walletName string, address string, message []byte) ([]byte, error)
```

Signs an arbitrary message using the specified account.

**Parameters:**
- `ctx`: Context for the operation
- `walletName`: Wallet name/identifier
- `address`: Account address to sign with
- `message`: Message to sign

**Returns:**
- `[]byte`: Signature
- `error`: Any error that occurred during signing

#### VerifySignature

```go
VerifySignature(ctx context.Context, message []byte, signature []byte, publicKey []byte) (bool, error)
```

Verifies a signature against a message and public key.

**Parameters:**
- `ctx`: Context for the operation
- `message`: Original message
- `signature`: Signature to verify
- `publicKey`: Public key to verify against

**Returns:**
- `bool`: Whether the signature is valid
- `error`: Any error that occurred during verification

### Role-based Wallet Management

#### AssignWalletToRole

```go
AssignWalletToRole(ctx context.Context, walletName string, role string) error
```

Assigns a wallet to a specific role in the service layer.

**Parameters:**
- `ctx`: Context for the operation
- `walletName`: Wallet name/identifier
- `role`: Role identifier (e.g., "gas_bank", "price_feed")

**Returns:**
- `error`: Any error that occurred during assignment

#### GetWalletForRole

```go
GetWalletForRole(ctx context.Context, role string) (*WalletInfo, error)
```

Gets the wallet assigned to a specific role.

**Parameters:**
- `ctx`: Context for the operation
- `role`: Role identifier

**Returns:**
- `*WalletInfo`: Information about the assigned wallet
- `error`: Any error that occurred

### Multi-signature Operations

#### CreateMultiSigAccount

```go
CreateMultiSigAccount(ctx context.Context, walletName string, signers []keys.PublicKey, threshold int) (*AccountInfo, error)
```

Creates a multi-signature account in the specified wallet.

**Parameters:**
- `ctx`: Context for the operation
- `walletName`: Wallet name/identifier
- `signers`: Array of public keys of signers
- `threshold`: Number of required signatures

**Returns:**
- `*AccountInfo`: Information about the created multi-sig account
- `error`: Any error that occurred during creation

#### AddSignatureToTx

```go
AddSignatureToTx(ctx context.Context, partiallySignedTx *transaction.Transaction, walletName string, address string) (*transaction.Transaction, error)
```

Adds a signature to a partially signed transaction.

**Parameters:**
- `ctx`: Context for the operation
- `partiallySignedTx`: Transaction with some signatures
- `walletName`: Wallet name/identifier
- `address`: Account address to sign with

**Returns:**
- `*transaction.Transaction`: Transaction with added signature
- `error`: Any error that occurred during signing 