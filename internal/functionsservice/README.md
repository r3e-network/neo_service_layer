# Functions Service

The Functions Service provides serverless function execution capabilities for the Neo Service Layer. It allows users to create, deploy, and execute JavaScript code in a secure sandbox environment.

## Architecture

The Functions Service consists of several components:

1. **Service API**: Provides high-level operations for creating, managing, and executing functions.
2. **JavaScript Sandbox**: A secure execution environment for running user-provided code.
3. **Function Storage**: Manages the storage and versioning of function code.
4. **Permissions System**: Controls access to functions and their execution.

## Features

- **Function Management**: Create, update, delete, and retrieve functions.
- **Function Execution**: Execute functions synchronously or asynchronously.
- **Versioning**: Track changes to functions with automatic versioning.
- **Access Control**: Fine-grained permissions for function access and execution.
- **Resource Limits**: Configurable limits on execution time, memory usage, and function size.
- **Execution History**: Track and query function execution history.

## Configuration

The service can be configured with the following options:

- `MaxFunctionSize`: Maximum allowed size for function code (default: 1MB).
- `MaxExecutionTime`: Maximum execution time for functions (default: 5 seconds).
- `MaxMemoryLimit`: Maximum memory limit for functions (default: 128MB).
- `EnableNetworkAccess`: Whether to allow network access from functions (default: false).
- `EnableFileIO`: Whether to allow file I/O from functions (default: false).
- `DefaultRuntime`: Default runtime for functions (default: "javascript").

## Usage

### Creating a Function

```go
function, err := functionservice.CreateFunction(
    ctx,
    userAddress,
    "my-function",
    "My test function",
    `function main(args) {
        return { message: "Hello, " + args.name };
    }`,
    functions.JavaScriptRuntime,
)
```

### Executing a Function

```go
invocation := functions.FunctionInvocation{
    FunctionID: function.ID,
    Parameters: map[string]interface{}{
        "name": "World",
    },
    Async:  false,
    Caller: userAddress,
}
execution, err := functionservice.InvokeFunction(ctx, invocation)
```

### Managing Permissions

```go
// Make a function public
permissions := &functions.FunctionPermissions{
    FunctionID:   function.ID,
    Owner:        userAddress,
    AllowedUsers: []util.Uint160{},
    Public:       true,
    ReadOnly:     false,
}
err := functionservice.UpdatePermissions(ctx, function.ID, userAddress, permissions)
```

## JavaScript Runtime

The Functions Service uses the Goja JavaScript engine to execute code. Functions must define a `main` function that takes an `args` parameter:

```javascript
function main(args) {
    console.log("Processing request for", args.name);
    return {
        message: "Hello, " + args.name,
        timestamp: Date.now()
    };
}
```

Available globals:
- `console`: For logging (log, info, warn, error)
- `args`: Function arguments
- `secrets`: Access to user's secrets (if available)
- `parameters`: Same as args, for compatibility

## Security Considerations

- Functions run in an isolated JavaScript sandbox.
- Network access and file I/O are disabled by default.
- Memory and execution time are limited to prevent abuse.
- Input and output are validated to prevent injection attacks.
- Functions can only access their owner's secrets.