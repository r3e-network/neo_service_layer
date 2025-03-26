# Functions Service User Guide

## Introduction

The Neo Service Layer Functions service allows you to create, deploy, and execute JavaScript functions that can interact with the Neo blockchain and external systems. This guide will help you get started with using the Functions service.

## Getting Started

### Prerequisites

- An account with access to the Neo Service Layer
- A valid Neo address for function ownership
- Basic knowledge of JavaScript programming

### Function Basics

Functions in the Neo Service Layer are JavaScript code modules with a standardized structure:

- Each function must have a `main(args)` entry point
- Functions can use built-in libraries and APIs
- Functions can access blockchain data through provided SDKs
- Functions can interact with external systems through allowed APIs

## Creating Your First Function

### Function Structure

A basic function follows this structure:

```javascript
/**
 * My First Function
 * 
 * This function demonstrates the basic structure of a function.
 */

/**
 * Main function - entry point for execution
 * @param {Object} args - Function arguments
 * @returns {Object} Execution result
 */
function main(args) {
  // Your code here
  console.info("Hello from my function!");
  
  return {
    success: true,
    message: "Function executed successfully",
    input: args
  };
}
```

### Creating a Function via API

To create a function, make a POST request to the Functions API:

```
POST /api/v1/functions

{
  "name": "My First Function",
  "description": "A simple test function",
  "code": "function main(args) { return { success: true, message: 'Hello World!' }; }",
  "runtime": "javascript"
}
```

### Function Validation

When you create or update a function, the code is automatically validated for:

- Security issues (e.g., dangerous API calls)
- Code quality (e.g., potential infinite loops)
- Required structure (e.g., presence of main function)

If validation fails, you'll receive detailed error messages explaining the issues.

## Deploying Functions

### Deployment Options

When deploying a function, you can specify several options:

- `autoActivate`: Whether to activate the function immediately (default: true)
- `validateCode`: Whether to validate the function code (default: true)
- `triggers`: Array of trigger IDs to associate with the function
- `permissions`: Access permissions for the function
- `metadata`: Additional metadata for the function

### Deployment Process

The deployment process involves:

1. Function creation or update
2. Code validation
3. Code compilation (optimizations, instrumentation)
4. Test execution (optional)
5. Activation (if auto-activate is enabled)

### Example Deployment Request

```
POST /api/v1/functions/deploy

{
  "name": "Token Balance Monitor",
  "description": "Monitors token balance and alerts when below threshold",
  "code": "function main(args) { ... }",
  "runtime": "javascript",
  "options": {
    "autoActivate": true,
    "validateCode": true,
    "triggers": ["daily-check-trigger-id"],
    "permissions": {
      "public": false,
      "allowedUsers": ["AZDXdXS3AdM7PI7fcaUQnSP3bvN8CJWpVW"]
    }
  }
}
```

## Executing Functions

### Execution Methods

Functions can be executed in several ways:

1. **Direct invocation** - Calling the function directly via API
2. **Trigger-based** - Executing in response to triggers (time-based, event-based)
3. **Chained execution** - Called from another function

### Direct Invocation

To invoke a function directly:

```
POST /api/v1/functions/{functionId}/invoke

{
  "parameters": {
    "tokenHash": "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5",
    "address": "AZDXdXS3AdM7PI7fcaUQnSP3bvN8CJWpVW",
    "thresholdAmount": 1000000000,
    "alertEndpoint": "https://example.com/alerts"
  },
  "async": false
}
```

### Function Parameters

Functions receive parameters via the `args` object in the main function. For example:

```javascript
function main(args) {
  // Access parameters
  const tokenHash = args.tokenHash;
  const address = args.address;
  
  // Rest of function...
}
```

### Execution Context

Functions execute in a secure sandbox with:

- Limited memory and CPU resources
- Restricted API access
- Gas metering
- Execution timeout

## Managing Functions

### Listing Functions

To list your functions:

```
GET /api/v1/functions
```

### Getting Function Details

To get details about a specific function:

```
GET /api/v1/functions/{functionId}
```

### Updating Functions

To update a function:

```
PUT /api/v1/functions/{functionId}

{
  "description": "Updated description",
  "code": "function main(args) { return { message: 'Updated function' }; }"
}
```

### Function Versions

When you update a function, a new version is created automatically. You can:

- List versions: `GET /api/v1/functions/{functionId}/versions`
- Get a specific version: `GET /api/v1/functions/{functionId}/versions/{version}`
- Rollback to a previous version: `POST /api/v1/functions/{functionId}/rollback/{version}`

### Deleting Functions

To delete a function:

```
DELETE /api/v1/functions/{functionId}
```

## Function Permissions

### Permission Levels

Functions support the following permission levels:

- `owner`: Only the owner can invoke the function
- `allowedUsers`: Specific users can invoke the function
- `public`: Anyone can invoke the function
- `readOnly`: Function cannot be modified

### Setting Permissions

To update function permissions:

```
PUT /api/v1/functions/{functionId}/permissions

{
  "public": false,
  "allowedUsers": [
    "AZDXdXS3AdM7PI7fcaUQnSP3bvN8CJWpVW",
    "AXozScHr7mPu4P7GhkQ3HCXzmWyvsYyZHy"
  ],
  "readOnly": false
}
```

## Working with Triggers

### Associating Triggers

Functions can be associated with triggers to execute automatically. To associate a trigger:

```
POST /api/v1/functions/{functionId}/triggers

{
  "triggerId": "trigger-id"
}
```

### Trigger Types

The system supports several trigger types:

- **Schedule**: Time-based triggers (cron expressions)
- **Event**: Blockchain event triggers
- **Condition**: State-based triggers
- **External**: External API webhook triggers

## Monitoring and Debugging

### Execution Logs

To view execution logs for a function:

```
GET /api/v1/functions/{functionId}/executions
```

### Execution Details

To get details about a specific execution:

```
GET /api/v1/functions/{functionId}/executions/{executionId}
```

### Debugging

You can debug functions by:

1. Adding `console.log`, `console.info`, etc. statements
2. Checking execution logs
3. Using test invocations before production use

## Best Practices

### Code Organization

- Keep functions small and focused
- Break complex logic into helper functions
- Use meaningful variable and function names
- Add comments to explain complex logic

### Error Handling

- Use try/catch blocks for error handling
- Return clear error messages
- Handle all possible error scenarios
- Validate input parameters

### Performance

- Minimize blockchain RPC calls
- Cache results when possible
- Avoid unnecessary computations
- Be mindful of gas usage

### Security

- Validate all inputs
- Don't store sensitive data in function code
- Use the Secrets service for sensitive information
- Limit function permissions to the minimum required

## Examples

### Token Balance Monitor

See the [Token Balance Monitor](examples/token_balance_monitor.js) example for a function that monitors token balances and sends alerts.

### More Examples

Additional examples can be found in the [examples](examples/) directory.

## Troubleshooting

### Common Issues

1. **Function validation fails**: Check validation errors and fix code issues
2. **Function execution times out**: Optimize code or increase timeout limit
3. **Gas limit exceeded**: Reduce resource usage or increase gas limit
4. **Permission denied**: Check function permissions

### Getting Help

If you encounter issues not covered in this guide:

1. Check the detailed error messages in API responses
2. Review function execution logs
3. Contact support with the function ID and execution ID 