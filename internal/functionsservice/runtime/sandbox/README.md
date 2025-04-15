# JavaScript Sandbox for Neo Service Layer

This package provides a secure JavaScript execution environment for the Neo Service Layer. It allows execution of untrusted JavaScript code with resource constraints and controlled access to Neo Service Layer features.

## Architecture

The sandbox package has been designed with a modular architecture, which improves maintainability, testability, and extensibility. The key components are:

- **Sandbox**: The main execution environment that manages VM lifecycle and provides a safe execution context
- **Config**: Configuration options for memory limits, timeouts, and permissions
- **Memory Monitor**: Tracks and enforces memory limits during execution
- **Context**: Provides execution context and service bindings

## Usage

### Basic Usage

```go
package main

import (
	"context"
	"fmt"
	"log"

	"github.com/r3e-network/neo_service_layer/internal/functionservice/runtime/sandbox"
	"go.uber.org/zap"
)

func main() {
	// Create a logger
	logger, _ := zap.NewDevelopment()

	// Create sandbox config
	config := sandbox.SandboxConfig{
		MemoryLimit:   64 * 1024 * 1024, // 64MB
		TimeoutMillis: 5000,             // 5 seconds
		StackSize:     8 * 1024 * 1024,  // 8MB
		Logger:        logger,
	}

	// Create a new sandbox
	sb := sandbox.New(config)
	defer sb.Close()

	// JavaScript code with a main function
	code := `
		function main(args) {
			console.log("Hello from sandbox!");
			console.log("Arguments:", args);
			return {
				success: true,
				message: "Operation completed",
				data: args
			};
		}
	`

	// Create function input
	input := sandbox.FunctionInput{
		Code: code,
		Args: []interface{}{"test", 123, true},
		Context: sandbox.NewFunctionContext("test-function"),
	}

	// Execute code
	output := sb.Execute(context.Background(), input)

	// Check for errors
	if output.Error != "" {
		log.Fatalf("Execution failed: %s", output.Error)
	}

	// Access the result
	fmt.Printf("Result: %v\n", output.Result)
	fmt.Printf("Duration: %v\n", output.Duration)
	fmt.Printf("Memory used: %d bytes\n", output.MemoryUsed)
	
	// Print logs
	fmt.Println("Logs:")
	for _, log := range output.Logs {
		fmt.Printf("  %s\n", log)
	}
}
```

### Using JSON Input/Output

```go
// JSON-serialized input
jsonInput := `{
	"code": "function main() { return {value: 42}; }",
	"args": [],
	"context": {
		"functionId": "json-test"
	}
}`

// Execute with JSON
jsonOutput, err := sb.ExecuteJSON(context.Background(), jsonInput)
if err != nil {
	log.Fatalf("Failed to execute: %v", err)
}

fmt.Println("JSON output:", jsonOutput)
```

## Configuration Options

The sandbox can be configured with various options:

```go
// Using functional options
config := sandbox.DefaultConfig().
	WithMemoryLimit(128 * 1024 * 1024).
	WithTimeout(10000).
	WithLogger(logger).
	WithNetworkAccess(false).
	WithFileIO(false).
	WithInteroperability(true)

// Or direct struct initialization
config := sandbox.SandboxConfig{
	MemoryLimit:            128 * 1024 * 1024,
	TimeoutMillis:          10000,
	StackSize:              8 * 1024 * 1024,
	AllowNetwork:           false,
	AllowFileIO:            false,
	EnableInteroperability: true,
	ServiceLayerURL:        "http://localhost:8080",
	Logger:                 logger,
}
```

## Service Bindings

When interoperability is enabled, JavaScript code can access Neo Service Layer services:

```javascript
function main() {
    // Access wallet service
    const walletInfo = services.wallet.GetWalletInfo("my-wallet");
    
    // Access storage service
    services.storage.Put("my-key", "my-value");
    const value = services.storage.Get("my-key");
    
    // Access oracle service
    const data = services.oracle.GetData("price-feed");
    
    return {
        walletInfo,
        value,
        data
    };
}
```

## Testing

Run the tests with:

```bash
go test -v
```

For testing memory limits and timeouts, see `sandbox_test.go` for examples 