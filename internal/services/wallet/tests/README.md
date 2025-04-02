# Wallet Service Tests

This directory contains tests and test utilities for the wallet service layer of the Neo blockchain application.

## Test Structure

The wallet service tests cover:

1. **Service Initialization**: Tests for proper service creation with various configuration parameters.
2. **Wallet Management**: Tests for creating, opening, closing, and managing wallet files.
3. **Account Management**: Tests for creating and managing accounts within wallets.
4. **Role Management**: Tests for assigning wallets to specific functional roles within the application.
5. **Multi-Signature Operations**: Tests for creating and using multi-signature wallets.
6. **Signing Operations**: Tests for transaction and message signing capabilities.

## Mock Implementations

The tests use mock implementations to avoid dependencies on actual blockchain components:

- `mock_wallet.go`: Provides a lightweight implementation of the Neo wallet interface for testing.
- `mock_errors.go`: Defines standard errors used in the mock implementations.

## Running Tests

To run all wallet service tests:

```bash
go test ./internal/services/wallet/... -v
```

To run a specific test:

```bash
go test ./internal/services/wallet/... -run TestCreateWallet -v
```

## Test Data

Tests create temporary directories for wallet storage, which are automatically cleaned up after the tests complete. No permanent files are created during testing. 