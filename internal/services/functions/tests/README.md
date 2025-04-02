# Functions Service Test Suite

This directory contains test files for the Functions service, which is a core component of the Neo Service Layer. The Functions service provides serverless function execution capabilities and function management features.

## Test Files

The test suite consists of the following files:

- `service_test.go`: Tests for service initialization, configuration, and basic functionality
- `interface_test.go`: Tests that verify the service implements the `IService` interface correctly
- `mock_test.go`: Comprehensive tests using mock implementations to simulate real-world scenarios

## Test Coverage

The test suite covers the following areas:

### Service Initialization
- Configuration validation
- Default configuration values
- Service struct initialization

### Function Management
- Creating functions with different parameters
- Retrieving functions by ID
- Updating function properties and code
- Deleting functions
- Listing functions by owner

### Execution
- Invoking functions with different parameters
- Handling execution results and errors
- Retrieving execution records
- Listing executions for a function

### Permissions
- Public/private function access control
- Read-only function protection
- User-specific access control
- Owner vs. non-owner permissions

### Versioning
- Function version creation
- Retrieving specific versions
- Listing all versions of a function

### Error Handling
- Invalid inputs and parameters
- Permission validation
- Non-existent resources
- Size and quota limits

## Mock Service

The test suite includes a mock implementation of the `IService` interface that allows comprehensive testing of the service's behavior without requiring actual sandbox execution. This mock service:

- Implements all interface methods with realistic behavior
- Maintains in-memory state for functions, executions, and permissions
- Enforces the same permission rules as the real service
- Simulates execution with deterministic results

## Running Tests

To run the full test suite:

```bash
go test -v ./internal/services/functions/...
```

To run specific tests:

```bash
go test -v ./internal/services/functions -run TestNewService
go test -v ./internal/services/functions -run TestCreateFunction
go test -v ./internal/services/functions -run TestFunctionLifecycle
```

To check code coverage:

```bash
go test -coverprofile=coverage.out ./internal/services/functions/...
go tool cover -html=coverage.out
``` 