# Neo Service Layer JavaScript Runtime

This package provides the JavaScript runtime environment for Neo Service Layer functions.

## Sandbox Implementation

The JavaScript sandbox has been refactored into a modular design to improve maintainability, readability, and testability. The sandbox implementation is now split across several files in the `sandbox` subdirectory.

### Migration from Original to Refactored Sandbox

The original monolithic implementation has been split into multiple components:

1. **Original Implementation**: `sandbox.go` - A large file containing all sandbox functionality
2. **Refactored Implementation**: Multiple files in the `sandbox/` directory

For backward compatibility, a `LegacySandbox` wrapper is provided in `sandbox_wrapper.go` which adapts the new implementation to match the original API.

## How to Use the Sandbox

### Using the Legacy Wrapper (Backward Compatible)

```go
import (
    "context"
    "github.com/r3e-network/neo_service_layer/internal/functionservice/runtime"
)

// Create a sandbox with legacy API
config := runtime.SandboxConfig{
    MemoryLimit:   64 * 1024 * 1024,
    TimeoutMillis: 5000,
}

sandbox := runtime.NewLegacySandbox(config)
defer sandbox.Close()

// Execute JavaScript code
input := runtime.FunctionInput{
    Code: `function main() { return "Hello, world!"; }`,
}

output, err := sandbox.Execute(context.Background(), input)
```

### Using the New Implementation Directly

```go
import (
    "context"
    "github.com/r3e-network/neo_service_layer/internal/functionservice/runtime/sandbox"
)

// Create a sandbox with new API
config := sandbox.SandboxConfig{
    MemoryLimit:   64 * 1024 * 1024,
    TimeoutMillis: 5000,
}

sb := sandbox.New(config)
defer sb.Close()

// Execute JavaScript code
input := sandbox.FunctionInput{
    Code: `function main() { return "Hello, world!"; }`,
    Args: []interface{}{},
    Context: sandbox.NewFunctionContext("test-function"),
}

output := sb.Execute(context.Background(), input)
```

## Key Improvements in the Refactored Implementation

1. **Modularity**: Each component has a single responsibility
2. **Memory Management**: Improved memory monitoring and limit enforcement
3. **JavaScript Integration**: Better handling of JavaScript function execution
4. **Service Bindings**: Cleaner approach to service interoperability
5. **Testing**: Comprehensive unit tests for each component
6. **Configuration**: More flexible configuration options
7. **Documentation**: Detailed documentation of each component

## File Structure

- `sandbox.go` - Original sandbox implementation (maintained for reference)
- `sandbox_wrapper.go` - Compatibility wrapper for the refactored implementation
- `sandbox/` - Directory containing the refactored implementation:
  - `sandbox.go` - Core VM lifecycle management
  - `config.go` - Configuration options
  - `models.go` - Data structures for input/output
  - `memory.go` - Memory monitoring and limits
  - `execution.go` - JavaScript execution logic
  - `context.go` - Function context and service bindings
  - `json.go` - JSON serialization support
  - `services.go` - Service client interfaces
  - `README.md` - Detailed documentation of the refactored implementation
  - `REFACTORING.md` - Notes on the refactoring process

## Transitioning to the New Implementation

For new code, it's recommended to use the new implementation directly. The wrapper is provided to maintain compatibility with existing code that uses the original API.

To transition existing code:

1. Import the new package: `github.com/r3e-network/neo_service_layer/internal/functionservice/runtime/sandbox`
2. Update configuration instantiation to use the new `SandboxConfig` type
3. Update function input creation to follow the new structure with array-based arguments
4. Update result handling to work with the new `FunctionOutput` structure

See the [sandbox README](/internal/functionservice/runtime/sandbox/README.md) for detailed usage examples. 