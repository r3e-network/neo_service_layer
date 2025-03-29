# Neo Service Layer JavaScript SDK Tests

This directory contains tests for the Neo Service Layer JavaScript SDK. The tests are organized by feature and follow the same structure as the source code.

## Test Structure

- `services/`: Tests for service modules (functions, secrets, transaction, etc.)
- `core/`: Tests for core functionality (client, errors, etc.)
- `utils/`: Tests for utility functions (function-context, etc.)

## Test Strategy

### Unit Tests

Unit tests focus on testing individual components in isolation. Dependencies are mocked to ensure that tests are fast and reliable.

### Integration Tests

Integration tests verify that components work together correctly. These tests may require a running Neo Service Layer instance or mock server.

## Transaction Service Tests

The transaction service tests verify that the transaction service can:

1. Create transactions with various configurations
2. Sign transactions
3. Send transactions to the blockchain
4. Get transaction status
5. List transactions
6. Estimate transaction fees

## Function Context Tests

The function context tests verify that:

1. The function context is created correctly with all required properties
2. Service methods are accessible through the context
3. Transaction service methods are properly integrated
4. Error handling works as expected

## Running Tests

```bash
# Run all tests
npm test

# Run specific tests
npm test -- --testPathPattern=transaction

# Run tests with coverage
npm test -- --coverage
```

## Writing Tests

When writing tests, follow these guidelines:

1. Each test should focus on a single functionality
2. Use descriptive test names that explain what is being tested
3. Set up test data in the test itself or in a shared fixture
4. Clean up after tests to ensure they don't affect other tests
5. Use mocks for external dependencies
6. Test both success and failure cases
