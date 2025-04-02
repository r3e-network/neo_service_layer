# Wallet Service

The Wallet Service provides secure wallet management capabilities for the Neo Service Layer. It's responsible for creating, opening, and managing wallet files, accounts, and signing operations.

## Service Components

The wallet service consists of the following components:

1. **Service Interface (`IService`)**: Defines the contract for wallet management operations.
2. **Service Implementation (`ServiceImpl`)**: Implements the service interface using Neo-Go wallet libraries.
3. **Mock Implementation (`MockService`)**: Provides an in-memory implementation for testing.
4. **Models**: Defines data structures for wallet information, accounts, and balances.

## Features

- Wallet lifecycle management (create, open, close, backup, restore)
- Account management within wallets
- Transaction and message signing
- Role-based wallet assignment
- Multi-signature account support

## Configuration

The wallet service accepts the following configuration options:

```go
type WalletConfig struct {
    // Directory where wallets are stored
    WalletDir string

    // Default password for system wallets (not user wallets)
    SystemWalletPassword string

    // Whether to create default service wallets if they don't exist
    AutoCreateServiceWallets bool

    // Default neo-go network to connect to (mainnet, testnet, etc.)
    Network string

    // Whether to require authentication for all wallet operations
    RequireAuth bool

    // How long to keep wallets unlocked in memory (in seconds, 0 = indefinitely)
    AutoLockTimeout int

    // Max number of wallets that can be open simultaneously
    MaxOpenWallets int

    // Log all wallet operations for security audit
    AuditLog bool
}
```

## Testing

The service includes unit tests that verify its functionality:

- **Interface Tests (`interface_test.go`)**: Tests the API contract using mock implementations.
- **Service Tests (`service_test.go`)**: Tests the actual service implementation.

To run the tests:

```bash
go test ./internal/services/wallet/... -v
```

## Integration with Other Services

The wallet service is used by other services that require wallet functionality:

1. **Transaction Service**: For signing and sending transactions
2. **Gas Bank Service**: For managing gas allocations
3. **Function Service**: For contract deployments and updates

## Security Considerations

- Wallet passwords are never stored in plaintext
- Private keys are only kept in memory when needed
- Auto-locking prevents access to wallets after idle periods
- Role-based assignment restricts access to specific wallets
- Audit logging tracks all wallet operations 