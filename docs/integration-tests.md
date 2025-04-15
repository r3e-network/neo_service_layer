# Neo Service Layer Integration Tests Documentation

## Overview

This document provides detailed information about the integration tests implemented for the Neo Service Layer. These tests ensure that different services within the platform can interoperate correctly and perform their intended functions as a cohesive system.

## Test Structure

The integration tests are organized in phases, each focusing on specific aspects of the system:

### Phase 2: Core Services Tests

**File**: `tests/integration/phase2_test.go`

Tests the fundamental services that other parts of the system depend on:

- **Gas Bank Service**: Allocation and management of GAS tokens for user operations
- **Price Feed Service**: Publishing and retrieving price data for assets
- **Trigger Service**: Creating and executing triggers based on conditions

Key test scenarios include:
- Creating and retrieving gas allocations
- Checking gas balance and consumption
- Publishing and retrieving price data
- Creating triggers with different conditions

### Phase 3: Advanced Services Tests

**File**: `tests/integration/phase3_test.go`

Tests higher-level services that build on the core functionalities:

- **Functions Service**: Creating and executing serverless functions
- **Secrets Service**: Secure storage and retrieval of user secrets
- **Automation Service**: Scheduling and executing automated tasks

Key test scenarios include:
- Creating and executing JavaScript functions
- Storing and retrieving encrypted secrets
- Setting up automated triggers for contracts

### Phase 4: API Service Tests

**File**: `tests/integration/phase4_test.go`

Tests the RESTful API interface that external applications use to interact with the platform:

- **API Endpoints**: Testing each endpoint for correct behavior
- **Request/Response Handling**: Validating input and output formats
- **Authentication**: Verifying signature-based authentication

Key test scenarios include:
- Registering API endpoints for each service
- Handling valid and invalid requests
- Proper error responses and status codes

### Phase 5: Functions and Secrets Integration

**File**: `tests/integration/phase5_test.go`

Focuses on the integration between the Functions and Secrets services:

- **Function Creation and Execution**: Testing the full lifecycle of serverless functions
- **Secret Management**: Creating, retrieving, and using secrets within functions
- **Permission Control**: Validating access control for functions and secrets

Key test scenarios include:
- Creating functions that access secrets
- Managing function permissions
- Full execution lifecycle tracking

### Phase 6: Full System Integration

**File**: `tests/integration/phase6_test.go`

Tests the complete platform with all services working together:

- **API Service**: As the front-end for all other services
- **Multiple Service Interaction**: Testing cross-service workflows
- **End-to-End Scenarios**: Testing complete user journeys

Key test scenarios include:
- Creating a user account and allocating resources
- Setting up functions, secrets, and triggers
- Executing triggers that invoke functions using secrets

## Test Implementation Details

### Mock Services

The tests use mock implementations of certain services to focus on integration points without external dependencies:

```go
// MockGasBankService is a mock implementation of gasbank.Service
type MockGasBankService struct {
    mock.Mock
}

// MockPriceFeedService is a mock implementation of pricefeed.Service
type MockPriceFeedService struct {
    mock.Mock
}

// MockTriggerService is a mock implementation of trigger.Service
type MockTriggerService struct {
    mock.Mock
}
```

### Test Account Creation

Each test begins by creating a test Neo account to represent a user:

```go
privateKey, err := keys.NewPrivateKey()
require.NoError(t, err)
account := wallet.NewAccountFromPrivateKey(privateKey)
userAddress := util.Uint160(account.ScriptHash())
```

### Service Initialization

Services are initialized with test configurations:

```go
functionsConfig := &functions.Config{
    MaxFunctionSize:     1024 * 1024, // 1MB
    MaxExecutionTime:    5 * time.Second,
    MaxMemoryLimit:      128 * 1024 * 1024, // 128MB
    EnableNetworkAccess: false,
    EnableFileIO:        false,
    DefaultRuntime:      "javascript",
}
functionservice, err := functions.NewService(functionsConfig)
```

### Test Assertions

Tests use the `testify` package for assertions:

```go
require.NoError(t, err)
require.NotNil(t, result)
require.Equal(t, expected, actual)
```

## Running the Tests

### Running All Integration Tests

```bash
go test ./tests/integration/... -v
```

### Running Specific Phase Tests

```bash
go test ./tests/integration/phase2_test.go -v
go test ./tests/integration/phase3_test.go -v
go test ./tests/integration/phase4_test.go -v
go test ./tests/integration/phase5_test.go -v
go test ./tests/integration/phase6_test.go -v
```

## Test Coverage Analysis

The integration tests cover:

1. **Service Initialization**: Testing that all services can be properly initialized
2. **API Endpoints**: Verifying all API endpoints function correctly
3. **Cross-Service Communication**: Ensuring services can interact with each other
4. **Error Handling**: Testing error cases and recovery mechanisms
5. **Resource Management**: Verifying proper allocation and release of resources

## Testing Best Practices

When adding new integration tests:

1. **Isolate Components**: Use mocks for external dependencies
2. **Test Realistic Scenarios**: Design tests that reflect real-world usage
3. **Check Edge Cases**: Test both normal and error paths
4. **Maintain Independence**: Tests should not depend on each other's state
5. **Clear Setup and Teardown**: Initialize and clean up resources properly

## Common Issues and Solutions

### Test Failures Due to Missing Implementations

If a test fails with errors like `nil pointer dereference` or `method not implemented`, check that all service methods have been properly implemented and that mock services correctly implement their interfaces.

### Timeouts in Asynchronous Operations

For tests involving async operations, ensure appropriate context timeout values are set and that the test waits for operations to complete.

### Mock Configuration Issues

Ensure mock services are configured to return appropriate values for all calls they will receive during tests, including error cases.

## Future Test Improvements

1. **Database Integration**: Add tests with actual database instances
2. **Blockchain Integration**: Test with a local Neo blockchain node
3. **Load Testing**: Add performance benchmarks for high-load scenarios
4. **Failure Recovery**: Test system recovery after simulated failures
5. **Security Testing**: Add specific tests for security features and vulnerabilities

## Conclusion

The integration tests provide confidence that the Neo Service Layer functions correctly as a unified system. They verify that services can be initialized with appropriate configurations, that they can communicate with each other, and that they correctly implement their intended functionalities.

By maintaining and extending these tests, we ensure the platform remains reliable and that new features integrate seamlessly with existing functionality. 