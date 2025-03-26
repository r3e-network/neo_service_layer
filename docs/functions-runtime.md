# Functions Runtime Documentation

This document describes the JavaScript runtime environment available in the Neo Service Layer for executing serverless functions.

## Overview

The Functions Runtime provides a secure, sandboxed environment for executing JavaScript code. It is designed to:

1. Isolate execution from the host system
2. Limit resource consumption
3. Provide controlled access to system features
4. Enable interaction with other Neo Service Layer components

## Runtime Architecture

The Functions Runtime is built on the Goja JavaScript engine, a pure Go implementation of ECMAScript 5.1. The runtime is structured as follows:

```
┌───────────────────────────────────────────────────────────┐
│                     Functions Service                     │
│                                                           │
│  ┌───────────────┐ ┌───────────┐ ┌────────────────────┐  │
│  │  Function     │ │ Execution │ │ Function           │  │
│  │  Registry     │ │ Queue     │ │ Logs               │  │
│  └───────┬───────┘ └─────┬─────┘ └────────┬───────────┘  │
│          │               │                │              │
│          └───────────────┼────────────────┘              │
│                          │                               │
│                   ┌──────▼─────────┐                     │
│                   │  Execution     │                     │
│                   │  Manager       │                     │
│                   └──────┬─────────┘                     │
│                          │                               │
└──────────────────────────┼───────────────────────────────┘
                           │
                  ┌────────▼────────┐
                  │                 │
                  │ Runtime Sandbox │
                  │                 │
                  └────────┬────────┘
                           │
            ┌──────────────┴────────────────┐
            │                               │
┌───────────▼───────────┐    ┌─────────────▼─────────────┐
│                       │    │                           │
│   Goja JavaScript     │    │  Built-in Objects and     │
│   Engine              │    │  Runtime Utilities        │
│                       │    │                           │
└───────────────────────┘    └───────────────────────────┘
```

## Sandbox Environment

The Runtime Sandbox provides an isolated environment for executing untrusted code with the following characteristics:

### Resource Constraints

The sandbox enforces the following resource limits:

| Resource         | Default Limit | Description                        |
|------------------|---------------|------------------------------------|
| Memory           | 128 MB        | Maximum memory usage               |
| Execution Time   | 5 seconds     | Maximum function runtime           |
| Stack Size       | 8 MB          | Maximum stack depth                |

These limits are configurable and can be adjusted based on function requirements.

### Security Isolation

The sandbox provides the following security features:

1. **Memory Isolation**: Functions cannot access memory outside their allocated heap
2. **Computation Limits**: CPU usage is bounded by the execution timeout
3. **Network Access Control**: Network access can be enabled/disabled
4. **File System Isolation**: File I/O can be enabled/disabled or restricted to specific directories

## JavaScript Environment

### ECMAScript Compatibility

The runtime supports ECMAScript 5.1 with some ES6 features, including:

- Arrow functions
- Template literals
- let/const declarations
- Promises
- Map and Set objects
- Array methods (find, findIndex, etc.)
- JSON object

### Global Objects

The following global objects are available to functions:

```javascript
// Console for logging
console.log("Hello world");
console.info("Informational message");
console.warn("Warning message");
console.error("Error message");

// JSON parsing/serializing
const obj = JSON.parse('{"key":"value"}');
const str = JSON.stringify({number: 42});

// Math utilities
const value = Math.random();
const rounded = Math.round(3.14);

// Date handling
const now = new Date();
const timestamp = now.toISOString();

// Array utilities
const arr = [1, 2, 3, 4, 5];
const sum = arr.reduce((a, b) => a + b, 0);
const doubled = arr.map(x => x * 2);
```

### Function Input Context

Functions receive input through several predefined objects:

#### args

The `args` object contains the input parameters passed to the function:

```javascript
function main(args) {
    // Access input parameters
    const name = args.name;
    const id = args.id;
    
    // Return result
    return {
        message: `Hello, ${name}!`,
        timestamp: new Date().toISOString()
    };
}
```

#### secrets

The `secrets` object provides access to configured secrets:

```javascript
function main(args) {
    // Access a stored API key
    const apiKey = secrets.apiKey;
    
    // Use the secret (e.g., in a simulated API call)
    console.log(`Using API key: ${apiKey.substring(0, 4)}...`);
    
    return {
        success: true,
        hasApiKey: apiKey !== undefined
    };
}
```

#### parameters

The `parameters` object contains configuration parameters that can be used to customize function behavior:

```javascript
function main(args) {
    // Access configuration parameters
    const debugMode = parameters.debug === true;
    const apiEndpoint = parameters.apiEndpoint || "https://default-api.example.com";
    
    if (debugMode) {
        console.log(`Using API endpoint: ${apiEndpoint}`);
    }
    
    return {
        endpoint: apiEndpoint,
        debugEnabled: debugMode
    };
}
```

## Function Structure

Each JavaScript function must follow a specific structure to be compatible with the runtime:

### Main Function Entry Point

Every function must define a `main` function that serves as the entry point:

```javascript
function main(args) {
    // Function logic goes here
    return {
        // Result to be returned
    };
}
```

### Return Values

Functions can return any valid JavaScript value that can be serialized to JSON:

- Primitive types (string, number, boolean, null)
- Objects
- Arrays

Example return values:

```javascript
// String return
return "Function completed successfully";

// Number return
return 42;

// Object return
return {
    success: true,
    data: {
        id: "1234",
        name: "Example",
        values: [1, 2, 3]
    },
    timestamp: new Date().toISOString()
};

// Array return
return [1, 2, 3, 4, 5];
```

### Error Handling

Functions can handle errors using standard JavaScript try/catch blocks:

```javascript
function main(args) {
    try {
        // Potentially error-prone code
        if (!args.requiredParam) {
            throw new Error("Required parameter missing");
        }
        
        return {
            success: true,
            result: processData(args.requiredParam)
        };
    } catch (error) {
        console.error("Function failed:", error.message);
        return {
            success: false,
            error: error.message
        };
    }
}
```

## Function Output

The runtime captures the following information from function execution:

```json
{
    "result": {
        "success": true,
        "message": "Function completed successfully"
    },
    "logs": [
        "Processing started",
        "INFO: Step 1 completed",
        "INFO: Step 2 completed",
        "Processing finished"
    ],
    "error": "",
    "duration": 127,
    "memoryUsed": 1458176
}
```

- **result**: The value returned by the function
- **logs**: Array of console log messages
- **error**: Error message (if execution failed)
- **duration**: Execution time in milliseconds
- **memoryUsed**: Memory used in bytes

## Usage Examples

### Basic Function

```javascript
function main(args) {
    console.log("Function started");
    
    const name = args.name || "World";
    const greeting = `Hello, ${name}!`;
    
    console.log(`Greeting: ${greeting}`);
    
    return {
        message: greeting,
        timestamp: new Date().toISOString()
    };
}
```

### Data Processing Function

```javascript
function main(args) {
    const numbers = args.numbers || [];
    
    if (!Array.isArray(numbers)) {
        throw new Error("'numbers' must be an array");
    }
    
    const result = {
        count: numbers.length,
        sum: 0,
        average: 0,
        min: numbers.length > 0 ? numbers[0] : null,
        max: numbers.length > 0 ? numbers[0] : null
    };
    
    // Process the numbers
    if (numbers.length > 0) {
        result.sum = numbers.reduce((a, b) => a + b, 0);
        result.average = result.sum / numbers.length;
        result.min = Math.min(...numbers);
        result.max = Math.max(...numbers);
    }
    
    console.log(`Processed ${result.count} numbers`);
    
    return result;
}
```

### Using Secrets and Parameters

```javascript
function main(args) {
    // Get configuration from parameters
    const apiUrl = parameters.apiUrl || "https://api.example.com";
    const timeout = parameters.timeout || 5000;
    
    // Get API key from secrets
    const apiKey = secrets.apiKey;
    
    if (!apiKey) {
        throw new Error("API key not configured");
    }
    
    console.log(`Using API endpoint: ${apiUrl}`);
    console.log(`Timeout configured: ${timeout}ms`);
    console.log(`API key length: ${apiKey.length}`);
    
    // Simulate API call
    return {
        success: true,
        endpoint: apiUrl,
        configuredTimeout: timeout,
        results: simulateApiCall(args.query)
    };
}

function simulateApiCall(query) {
    // Simulation function
    return {
        query: query,
        resultCount: Math.floor(Math.random() * 100),
        processingTime: Math.floor(Math.random() * 1000)
    };
}
```

## Best Practices

### Performance Optimization

1. **Minimize Memory Usage**:
   - Use generators for large data sets
   - Process data in chunks
   - Avoid creating large arrays or objects

2. **Reduce Execution Time**:
   - Use efficient algorithms and data structures
   - Avoid unnecessary operations
   - Use async/await for I/O operations (when supported)

3. **Optimize Startup**:
   - Keep function code small and focused
   - Minimize dependencies
   - Initialize resources only when needed

### Error Handling

1. **Validate Inputs**:
   - Check all input parameters at the start of the function
   - Provide clear error messages
   - Use appropriate error types

2. **Graceful Degradation**:
   - Handle missing optional parameters
   - Provide default values
   - Catch and handle expected exceptions

3. **Detailed Logging**:
   - Log the start and end of execution
   - Include relevant context in error logs
   - Use appropriate log levels (info, warn, error)

### Security Considerations

1. **Secret Management**:
   - Never log or expose secrets
   - Validate secrets are available before use
   - Use the minimum required permissions

2. **Input Sanitization**:
   - Validate all user-provided inputs
   - Escape/sanitize data before use
   - Avoid using `eval()` or similar functions

3. **Output Sanitization**:
   - Filter sensitive information from responses
   - Validate return values

## Limitations

The Functions Runtime has the following limitations:

1. **ES6+ Support**: Limited support for newer ECMAScript features
2. **Runtime Environment**: No DOM or browser APIs
3. **External Modules**: No built-in `require()` or `import` support
4. **Network Access**: Restricted by default
5. **File System Access**: Restricted by default
6. **Execution Time**: Limited to configured timeout
7. **Memory Usage**: Limited to configured maximum

## Troubleshooting

### Common Issues

1. **Function Timeout**:
   - Increase the timeout limit
   - Optimize function code
   - Break the function into smaller units

2. **Memory Limit Exceeded**:
   - Reduce memory usage
   - Process data in smaller chunks
   - Check for memory leaks

3. **Missing Dependencies**:
   - Include all required code in the function
   - Use included libraries and utilities
   - Simplify function requirements

### Debugging

1. **Console Logging**:
   - Use `console.log()` for debugging
   - Log variable values and execution flow
   - Check logs in the function output

2. **Error Inspection**:
   - Check error messages in the function output
   - Inspect stack traces
   - Test functions with simplified inputs

3. **Performance Analysis**:
   - Monitor execution duration in output
   - Check memory usage in output
   - Add timing logs at key points