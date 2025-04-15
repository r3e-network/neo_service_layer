# Wallet Service Testing Guide

This document describes the testing approach for the wallet service layer, including test coverage areas, mock implementations, and guidelines for writing effective tests.

## Test Coverage Areas

The wallet service tests cover the following aspects:

1. **Service Initialization**
   - Configuration validation
   - Service creation with different config parameters
   - Error handling for invalid configurations

2. **Wallet Management**
   - Creating wallets with different parameters
   - Opening and closing wallets
   - Listing available wallets
   - Backup and restore functionality
   - Error handling for non-existent wallets

3. **Account Management**
   - Creating accounts within wallets
   - Listing accounts
   - Getting account information
   - Getting account balances
   - Error handling for invalid accounts

4. **Role-Based Wallet Management**
   - Assigning wallets to specific roles
   - Retrieving wallets by role
   - Updating role assignments
   - Error handling for undefined roles

5. **Multi-Signature Operations**
   - Creating multi-signature accounts
   - Setting various threshold values
   - Adding signatures to transactions
   - Error handling for invalid thresholds

6. **Signing Operations**
   - Signing transactions with different accounts
   - Signing messages
   - Verifying signatures
   - Error handling for invalid signatures

## Mock Implementation

The mock implementation (`MockService`) provides an in-memory wallet service that follows the same interface contract as the real implementation but doesn't rely on the filesystem or blockchain. This allows for faster, more isolated testing.

Key features of the mock implementation:

- Stores wallets, accounts, and role assignments in memory
- Generates deterministic addresses for predictable testing
- Can be set to failure mode to test error handling
- Simulates password validation and wallet locking

## Writing New Tests

When writing tests for the wallet service, follow these guidelines:

1. **Isolate test cases**: Each test case should be independent and not rely on the state created by other tests.

2. **Use descriptive names**: Test names should clearly indicate what aspect they are testing.

3. **Test error conditions**: Don't just test the happy path; also test how the service handles errors.

4. **Clean up resources**: Tests should clean up any resources they create, especially when working with the actual implementation that creates files.

5. **Use subtests**: Group related tests using the `t.Run()` function for better organization.

Example test structure:

```go
func TestFeature(t *testing.T) {
    // Setup common test resources
    service := setupTestService(t)
    ctx := context.Background()
    
    t.Run("SuccessCase", func(t *testing.T) {
        // Test happy path
    })
    
    t.Run("ErrorCase", func(t *testing.T) {
        // Test error handling
    })
}
```

## Running Tests

To run all wallet service tests:

```bash
go test ./internal/services/wallet/... -v
```

To run a specific test:

```bash
go test ./internal/services/wallet/... -run TestSpecificFeature -v
```

To run tests with race detection:

```bash
go test ./internal/services/wallet/... -race
```

## Test Data

The tests use the following test data:

- **Test wallets**: Named using predictable patterns like "test_wallet", "signing_test_wallet"
- **Test passwords**: Simple passwords like "password" for positive tests, "wrong" for negative tests
- **Test accounts**: Named using patterns like "Account 1", "Signing Account"

## Integration Tests

In addition to unit tests, integration tests can verify the wallet service works correctly with other services:

1. **Transaction service integration**: Tests that verify wallet signing works with transaction creation
2. **Gas bank integration**: Tests that verify wallet operations work with gas allocation
3. **Function service integration**: Tests that verify wallet operations work with contract deployment 