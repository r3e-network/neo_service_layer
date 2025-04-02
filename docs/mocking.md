# Mock Services

This document describes the mocking approach used in the Neo Service Layer to facilitate development
and testing without requiring actual Neo blockchain interaction.

## Overview

The Neo Service Layer uses mock implementations of key dependencies to allow development
and testing without relying on actual blockchain connections or wallet operations. This is
particularly useful for:

1. Development without a Neo blockchain connection
2. Testing without requiring real transactions
3. CI/CD pipelines that need to run in isolated environments
4. Local development with deterministic behavior

## Available Mock Services

### 1. Neo Client Mock (`MockNeoClient`)

Location: `internal/core/neo/mock_client.go`

This mock implements the `NeoClient` interface and provides simulated responses for all
Neo blockchain interactions:

- Contract invocation (`InvokeFunction`)
- Transaction submission (`SendRawTransaction`)
- Application logs retrieval (`GetApplicationLog`)
- Fee calculation (`CalculateNetworkFee`)
- Block count queries (`GetBlockCount`)
- Network magic queries (`GetNetwork`)

The mock client returns pre-configured responses that can be customized as needed.

### 2. Wallet Service Mock (`MockWalletService`)

Location: `internal/services/wallet/mock_wallet_service.go`

Implements the `WalletService` interface to provide transaction signing capabilities without
requiring real private keys:

- Transaction signing with mock signatures (`SignTx`)
- Tracking of signed transactions for verification
- Ability to simulate errors for testing error paths

## Using Mock Services

### Basic Setup

```go
// Create mock clients
mockNeoClient := neo.NewMockNeoClient()
mockWalletService := wallet.NewMockWalletService()

// Create service with mock dependencies
service := trigger.NewService(
    config,
    mockNeoClient,
    functionService,
    mockWalletService,
)
```

See the `examples/mock_services_example.go` file for a complete working example.

### Customizing Mock Behavior

Both mock services allow customizing their behavior:

```go
// Set custom mock response for contract invocation
mockNeoClient.MockInvokeResult = map[string]interface{}{
    "state": "FAULT",
    "gasconsumed": "2000000",
    "stack": []interface{}{},
}

// Set error to simulate failure
mockNeoClient.MockSendError = errors.New("simulated transaction error")

// Set an error for the wallet service
mockWalletService.SetMockError(errors.New("signing failure"))

// Reset mock state tracking
mockWalletService.ResetMockState()
```

## Best Practices

1. **Default to Mocks in Development**: Use mock services by default in development
   environments to avoid blockchain dependency.

2. **Test with Real Services**: Switch to real services in specific test environments
   to validate actual blockchain interaction.

3. **Make Mocks Configurable**: Allow configuration to switch between real and mock
   implementations based on environment.

4. **Track Mock Interactions**: Use the tracking capabilities of mocks to verify that
   the expected methods were called with correct parameters.

5. **Document Mock Limitations**: Be clear about what aspects of real behavior are not
   accurately represented by mocks.

## Extending Mocks

When adding new functionality to the Neo Service Layer that requires blockchain interaction,
follow this process:

1. Define the interface for the new service
2. Implement the real service that uses actual blockchain
3. Create a mock implementation for development/testing
4. Use dependency injection to allow switching between implementations

## Production Readiness

Before deploying to production:

1. Ensure all mock implementations are properly excluded from production builds
2. Verify that the real service implementations are properly tested
3. Set up configuration to use real services in production environments
4. Consider implementing feature flags to easily switch between implementations if issues arise