# Neo Service Layer Setup Guide

## Current Status

The Neo Service Layer implementation is currently in a transitional state with several issues that need to be addressed before the server can be run successfully.

## Critical Issues to Fix

### 1. Package Structure Issues

The logging service has a package structure issue. While we've created a script to fix it (`fix_logging_package.sh`), you should verify that:

- The directory `/internal/services/logging/models` exists
- The file `/internal/services/logging/models/models.go` contains the proper model definitions
- The imports in `/internal/services/logging/service.go` correctly reference the models package

### 2. Missing Dependencies

Several key services rely on the `neo-go` package, which is currently missing:

```bash
go get github.com/nspcc-dev/neo-go
```

After adding this dependency, you'll need to:

- Uncomment the `neo-go` imports at the top of `cmd/server/main.go`
- Uncomment the implementation sections in each service's initialization function

### 3. Service Initialization Sequence

The current initialization sequence in `main.go` is:

1. Logging service (currently disabled)
2. Metrics service 
3. GasBank service
4. PriceFeed service (currently disabled)
5. Secrets service with TEE provider
6. Functions service
7. Trigger service (currently disabled)
8. API service

Each service that's disabled currently returns `nil` with a warning log message.

## How to Fix and Enable the Server

1. **Fix Dependency Issues**:
   ```bash
   # Install the neo-go dependency
   go get github.com/nspcc-dev/neo-go
   
   # Clean and update module dependencies
   go mod tidy
   go mod vendor
   ```

2. **Verify Logging Package Structure**:
   ```bash
   # Check if the models directory was created by the script
   ls -la internal/services/logging/models/
   
   # If not, run the fix script
   chmod +x fix_logging_package.sh
   ./fix_logging_package.sh
   ```

3. **Edit `cmd/server/main.go`**:
   - Uncomment the `neo-go` imports at the top
   - Uncomment the implementation in each disabled service's init function
   - Remove the temporary warning log messages

4. **Build and Test**:
   ```bash 
   go build -o neo-service ./cmd/server
   ```

## Documentation-First Approach

Remember to follow our documentation-first approach:

1. Update the documentation for each service as you fix it
2. Ensure that any changes to service implementations are reflected in their README files
3. Keep the architecture document updated with the latest service interactions

## Optional Improvements

Once the server is running, consider these improvements:

1. Add more comprehensive error handling in service initialization
2. Implement graceful shutdown for all services
3. Add health checks for each service
4. Improve logging to provide better observability

## Support

If you encounter issues during setup, refer to the following resources:

- The NEO blockchain documentation: https://docs.neo.org/
- The service README files in each service directory
- The implementation of existing services as templates