# Neo Service Layer Integration Tests

This directory contains integration tests for the Neo Service Layer. These tests validate the functionality and integration of different services working together in the application.

## Test Structure

The integration tests are organized by phases:

1. **Phase 2 Tests** (`phase2_test.go`): Tests the core services of the platform:
   - Gas Bank Service: Allocation and management of GAS tokens
   - Price Feed Service: Publishing and retrieving price data
   - Trigger Service: Creating and executing triggers based on conditions

2. **Phase 3 Tests** (`phase3_test.go`): Tests advanced services building on the core functionalities:
   - Functions Service: Creating and executing serverless functions
   - Secrets Service: Secure storage and retrieval of user secrets
   - Automation Service: Contract automation with upkeeps and scheduled triggers

3. **Phase 4 Tests** (`phase4_test.go`): Tests supporting infrastructure services:
   - API Service: RESTful interface for external applications
   - Metrics Service: Monitoring and reporting on system performance
   - Logging Service: Centralized logging and log analysis

4. **Phase 5 Tests** (`phase5_test.go`): Tests additional platform capabilities:
   - Functions Service: Creating and executing JavaScript functions in a sandbox
   - Secrets Service: Secure storage and retrieval of sensitive information

5. **Simple Functions Test** (`simple_test.go`): A focused test for the Functions service:
   - Function creation and management
   - Function permissions
   - Basic service operations

## Helper Functions

The `helpers.go` file contains common utility functions used across tests:

- `InitNeoClient`: Initializes a Neo blockchain client for testing
- `CreateTestAccount`: Creates a test wallet account for transactions

## Running the Tests

To run all integration tests:

```bash
go test ./tests/integration/... -v
```

To run a specific phase test:

```bash
go test ./tests/integration/phase2_test.go -v
go test ./tests/integration/phase3_test.go -v
go test ./tests/integration/phase4_test.go -v
go test ./tests/integration/phase5_test.go -v
```

## Test Prerequisites

The tests rely on mock implementations of services to avoid external dependencies.
In a real environment, you would need:

1. A running Neo blockchain node
2. Valid wallet credentials
3. Deployed smart contracts

## Adding New Tests

When adding new integration tests:

1. Create a new test file following the naming convention: `phase<N>_test.go`
2. Use the helper functions for common setup
3. Follow the existing pattern of service initialization and testing
4. Update this documentation to reflect the new test capabilities

## Known Issues

- Tests are currently using mock implementations rather than real blockchain interactions
- Time-based operations use fixed timestamps rather than real timing

## Test Coverage

The tests cover the following aspects of the services:

1. **Core Services**:
   - Gas allocation and management
   - Price data publication and retrieval
   - Trigger creation and execution

2. **Advanced Services**:
   - Serverless function execution in JavaScript
   - Secure secret storage and retrieval
   - Contract automation

3. **Infrastructure Services**:
   - API endpoint registration and validation
   - Metrics collection and retrieval
   - Logging and log querying

4. **Security Features**:
   - Function permissions and access control
   - Secret encryption and expiration
   - API key authentication