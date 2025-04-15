# Neo Service Layer - Service Interoperability Testing

This document describes the integrated testing approach for Neo Service Layer's service interoperability capabilities.

## Overview

The service interoperability feature allows JavaScript functions running in the Neo Service Layer to interact with other services like:

- Secret Service
- Price Feed Service
- Gas Bank Service
- Transaction Service
- Functions Service
- Trigger Service

These tests verify that functions can successfully use these services to perform complex operations.

## Testing Approach

The testing approach uses several mock service implementations that simulate the behavior of real services:

1. **Mocksecretservice**: For storing and retrieving sensitive information
2. **MockPriceFeedService**: For getting token price data
3. **MockTransactionService**: For creating, signing, and submitting transactions

An extended mock service implementation called `InteropMockService` provides a simulated JavaScript execution environment that mimics how real functions would interact with these services.

## Tests Implemented

### 1. Basic Service Interoperability Tests

- **SecretService Test**: Verifies functions can store, retrieve, and delete secrets
- **PriceFeedService Test**: Verifies functions can get token prices
- **TransactionService Test**: Verifies functions can create, sign, and send transactions

### 2. Combined Service Interoperability Test

A more complex test that simulates a token swap operation using multiple services:

1. Retrieves API keys from the Secret Service
2. Gets token prices from the Price Feed Service
3. Calculates the swap amounts based on current token prices
4. Creates, signs, and sends a swap transaction
5. Stores the transaction reference as a secret for later retrieval

## How to Run the Tests

```bash
# Run all service interoperability tests
go test ./internal/services/functions -v -run "TestServiceInteroperability|TestCombinedServiceInteroperability"

# Run individual tests
go test ./internal/services/functions -v -run TestServiceInteroperability
go test ./internal/services/functions -v -run TestCombinedServiceInteroperability
```

## Benefits of This Testing Approach

1. **Fully Mocked**: Tests don't require external services or real blockchain interaction
2. **Comprehensive**: Covers the full service interoperability feature set
3. **Extensible**: Easy to add new service types to test
4. **Fast**: Tests run quickly without external dependencies
5. **Integration-Focused**: Tests the integration points between services

## Future Improvements

1. Extend testing to cover error cases more thoroughly
2. Add more complex scenarios leveraging multiple services
3. Add tests for background service operations (like triggers)
4. Expand test coverage for edge cases like permission handling
5. Consider adding end-to-end tests that use real service implementations