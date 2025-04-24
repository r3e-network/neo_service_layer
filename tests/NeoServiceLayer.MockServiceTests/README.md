# Neo Service Layer Mock Tests

This project contains mock tests for the Neo Service Layer services. These tests use the Moq library to create mock implementations of the service interfaces, allowing us to test service interactions without depending on the actual service implementations.

## Test Categories

### Basic Service Interaction Tests

1. **AccountWalletMockTests**
   - Tests for account registration, verification, and wallet creation
   - Tests for adding and deducting credits from accounts
   - Tests for error handling when creating duplicate accounts or deducting too many credits

2. **SecretsFunctionMockTests**
   - Tests for creating secrets and functions
   - Tests for granting function access to secrets
   - Tests for secret rotation
   - Tests for error handling when accessing expired secrets or secrets without permission

3. **PriceFeedMockTests**
   - Tests for adding and updating price sources
   - Tests for getting latest prices
   - Tests for getting price history

### Error Handling Tests

1. **AccountErrorHandlingTests**
   - Tests for validation errors during account registration (empty username, email, password)
   - Tests for weak password validation
   - Tests for duplicate username/email detection
   - Tests for insufficient credits when deducting

2. **WalletErrorHandlingTests**
   - Tests for validation errors during wallet creation (empty name, password)
   - Tests for weak password validation
   - Tests for duplicate wallet name detection
   - Tests for insufficient balance when transferring assets
   - Tests for invalid WIF format when importing wallets

3. **SecretsErrorHandlingTests**
   - Tests for validation errors during secret creation (empty name, value)
   - Tests for duplicate secret name detection
   - Tests for access control when retrieving secret values
   - Tests for error handling when rotating secrets

4. **FunctionErrorHandlingTests**
   - Tests for validation errors during function creation (empty name, source code, entry point)
   - Tests for duplicate function name detection
   - Tests for error handling during function execution
   - Tests for error handling when updating function source code

5. **PriceFeedErrorHandlingTests**
   - Tests for error handling when retrieving non-existent price data
   - Tests for validation errors when adding or updating price sources
   - Tests for error handling when retrieving price history with invalid parameters
   - Tests for error handling when submitting prices to the oracle

## Test Fixture

The `MockServiceTestFixture` class provides a common setup for all tests:

- Creates mock implementations of all service interfaces
- Sets up a dependency injection container with the mock services
- Provides access to the mock services for setting up expectations and verifying calls

## Benefits of Mock Tests

1. **Independence from Implementation**: These tests don't depend on the actual service implementations, so they can run even if there are build errors in the Services project.

2. **Focus on Interfaces**: By testing against the interfaces, we ensure that any implementation that adheres to the interface will work correctly.

3. **Faster Execution**: Mock tests are typically faster than tests that use actual implementations, as they don't need to perform real operations like database access or API calls.

4. **Controlled Environment**: We can precisely control the behavior of the mocked services, making it easier to test specific scenarios.

## Running the Tests

To run the tests, use the following command:

```bash
dotnet test
```

## Next Steps

1. **Fix the Services Project**: Now that we have a good understanding of how the services should interact, we can fix the build errors in the Services project.

2. **Create Integration Tests**: Once the Services project is fixed, we can create integration tests that use the actual service implementations.

3. **Add More Test Cases**: We can add more test cases to cover additional scenarios and edge cases.

4. **Improve Test Coverage**: We can analyze the test coverage and add tests for any untested code paths.
