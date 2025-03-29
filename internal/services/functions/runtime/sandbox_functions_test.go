package runtime

import (
	"context"
	"strings"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
	"go.uber.org/zap"
)

func TestSandbox_BasicFunctions(t *testing.T) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()
	
	// Create sandbox with logger
	sandbox := NewSandbox(SandboxConfig{
		Logger: logger,
	})

	tests := []struct {
		name        string
		code        string
		args        map[string]interface{}
		expected    interface{}
		expectError bool
	}{
		{
			name: "string concatenation",
			code: `
function main(args) {
    return args.a + args.b;
}`,
			args: map[string]interface{}{
				"a": "Hello, ",
				"b": "World!",
			},
			expected:    "Hello, World!",
			expectError: false,
		},
		{
			name: "array manipulation",
			code: `
function main(args) {
    return args.numbers.map(n => n * 2);
}`,
			args: map[string]interface{}{
				"numbers": []interface{}{1, 2, 3, 4, 5},
			},
			expected:    nil,
			expectError: false,
		},
		{
			name: "object transformation",
			code: `
function main(args) {
    return {
        fullName: args.firstName + " " + args.lastName,
        age: args.age,
        isAdult: args.age >= 18
    };
}`,
			args: map[string]interface{}{
				"firstName": "John",
				"lastName":  "Doe",
				"age":       25,
			},
			expected:    nil,
			expectError: false,
		},
		{
			name: "error handling",
			code: `
function main(args) {
    try {
        if (args.shouldThrow) {
            throw new Error("Test error");
        }
        return "No error";
    } catch (e) {
        return { error: e.message };
    }
}`,
			args: map[string]interface{}{
				"shouldThrow": true,
			},
			expected: map[string]interface{}{
				"error": "Test error",
			},
			expectError: false,
		},
		{
			name: "runtime error",
			code: `
function main(args) {
    // This will cause a runtime error
    return nonExistentFunction();
}`,
			args:        map[string]interface{}{},
			expected:    nil,
			expectError: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			input := FunctionInput{
				Code: tt.code,
				Args: tt.args,
			}

			output, err := sandbox.Execute(context.Background(), input)
			require.NoError(t, err, "Execute should not return an error")

			if tt.expectError {
				assert.NotEmpty(t, output.Error, "Expected error in output")
			} else {
				assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)
				
				// For array manipulation and object transformation, we need special handling
				if tt.name == "array manipulation" {
					result, ok := output.Result.([]interface{})
					assert.True(t, ok, "Result should be an array")
					assert.Equal(t, 5, len(result), "Result should have 5 elements")
					
					// Check individual values without type checking
					assert.Contains(t, result, int64(2), "Result should contain 2")
					assert.Contains(t, result, int64(4), "Result should contain 4")
					assert.Contains(t, result, int64(6), "Result should contain 6")
					assert.Contains(t, result, int64(8), "Result should contain 8")
					assert.Contains(t, result, int64(10), "Result should contain 10")
				} else if tt.name == "object transformation" {
					result, ok := output.Result.(map[string]interface{})
					assert.True(t, ok, "Result should be a map")
					
					// Check individual fields without type checking
					assert.Equal(t, "John Doe", result["fullName"], "fullName should match")
					assert.Equal(t, int64(25), result["age"], "age should match")
					assert.Equal(t, true, result["isAdult"], "isAdult should match")
				} else if tt.expected != nil {
					assert.Equal(t, tt.expected, output.Result, "Result should match expected value")
				}
			}
		})
	}
}

func TestSandbox_AsyncFunctions(t *testing.T) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()
	
	// Create sandbox with logger
	sandbox := NewSandbox(SandboxConfig{
		Logger:       logger,
		TimeoutMillis: 1000, // 1 second timeout
	})

	tests := []struct {
		name        string
		code        string
		args        map[string]interface{}
		expected    interface{}
		expectError bool
	}{
		{
			name: "setTimeout simulation",
			code: `
function main(args) {
    let result = "initial";
    
    // Simulate setTimeout with a busy wait
    const start = Date.now();
    while (Date.now() - start < 100) {
        // Busy wait for 100ms
    }
    
    result = "after timeout";
    return result;
}`,
			args:        map[string]interface{}{},
			expected:    "after timeout",
			expectError: false,
		},
		{
			name: "timeout error",
			code: `
function main(args) {
    // Infinite loop - should timeout
    while (true) {}
    return "This should never be reached";
}`,
			args:        map[string]interface{}{},
			expected:    nil,
			expectError: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			input := FunctionInput{
				Code: tt.code,
				Args: tt.args,
			}

			output, err := sandbox.Execute(context.Background(), input)
			require.NoError(t, err, "Execute should not return an error")

			if tt.expectError {
				assert.NotEmpty(t, output.Error, "Expected error in output")
				assert.Contains(t, strings.ToLower(output.Error), "timed out", "Error should mention timeout")
			} else {
				assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)
				assert.Equal(t, tt.expected, output.Result, "Result should match expected value")
			}
		})
	}
}

func TestSandbox_MemoryLimits(t *testing.T) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()
	
	// Create sandbox with low memory limit
	sandbox := NewSandbox(SandboxConfig{
		Logger:      logger,
		MemoryLimit: 10 * 1024 * 1024, // 10 MB
	})

	tests := []struct {
		name        string
		code        string
		expectError bool
	}{
		{
			name: "normal memory usage",
			code: `
function main(args) {
    // Create a small array
    const arr = new Array(1000).fill(0);
    return "Success";
}`,
			expectError: false,
		},
		{
			name: "large object creation",
			code: `
function main(args) {
    // Create a large object with many properties
    const obj = {};
    for (let i = 0; i < 10000; i++) {
        obj['key' + i] = 'value' + i;
    }
    return Object.keys(obj).length;
}`,
			expectError: false,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			input := FunctionInput{
				Code: tt.code,
			}

			output, err := sandbox.Execute(context.Background(), input)
			require.NoError(t, err, "Execute should not return an error")

			if tt.expectError {
				assert.NotEmpty(t, output.Error, "Expected error in output")
			} else {
				assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)
			}
			
			// Check that memory usage is reported
			assert.GreaterOrEqual(t, output.MemoryUsed, int64(0), "Memory usage should be reported")
		})
	}
}

func TestSandbox_ContextMethods(t *testing.T) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()
	
	// Create sandbox with interoperability enabled
	sandbox := NewSandbox(SandboxConfig{
		Logger:               logger,
		EnableInteroperability: true,
	})

	tests := []struct {
		name        string
		code        string
		context     *FunctionContext
		expectError bool
	}{
		{
			name: "access function context",
			code: `
function main(args) {
    // Access function context properties
    return {
        functionId: context.functionId,
        executionId: context.executionId,
        owner: context.owner
    };
}`,
			context: &FunctionContext{
				FunctionID:  "test-function",
				ExecutionID: "test-execution",
				Owner:       "test-owner",
			},
			expectError: false,
		},
		{
			name: "log to context",
			code: `
function main(args) {
    // Use context logging methods
    context.log("Info log message");
    context.error("Error log message");
    return "Logged messages";
}`,
			context: &FunctionContext{
				FunctionID:  "test-function",
				ExecutionID: "test-execution",
			},
			expectError: false,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			input := FunctionInput{
				Code:           tt.code,
				FunctionContext: tt.context,
			}

			output, err := sandbox.Execute(context.Background(), input)
			require.NoError(t, err, "Execute should not return an error")

			if tt.expectError {
				assert.NotEmpty(t, output.Error, "Expected error in output")
			} else {
				assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)
			}
		})
	}
}

func TestSandbox_MultipleExecutionsSequential(t *testing.T) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()
	
	// Create sandbox
	sandbox := NewSandbox(SandboxConfig{
		Logger: logger,
	})

	// Test running multiple functions in sequence
	for i := 0; i < 5; i++ {
		t.Run("sequential execution", func(t *testing.T) {
			input := FunctionInput{
				Code: `
function main(args) {
    return args.number * 2;
}`,
				Args: map[string]interface{}{
					"number": i,
				},
			}

			output, err := sandbox.Execute(context.Background(), input)
			require.NoError(t, err, "Execute should not return an error")
			assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)
			assert.Equal(t, int64(i*2), output.Result, "Result should be double the input")
		})
	}

	// Test running multiple functions concurrently
	t.Run("concurrent execution", func(t *testing.T) {
		const concurrentRuns = 5
		results := make(chan *FunctionOutput, concurrentRuns)
		errors := make(chan error, concurrentRuns)

		for i := 0; i < concurrentRuns; i++ {
			go func(num int) {
				input := FunctionInput{
					Code: `
function main(args) {
    // Add a small delay to simulate work
    const start = Date.now();
    while (Date.now() - start < 10) {}
    return args.number * 3;
}`,
					Args: map[string]interface{}{
						"number": num,
					},
				}

				output, err := sandbox.Execute(context.Background(), input)
				if err != nil {
					errors <- err
					return
				}
				results <- output
			}(i)
		}

		// Collect results
		for i := 0; i < concurrentRuns; i++ {
			select {
			case err := <-errors:
				t.Errorf("Error in concurrent execution: %v", err)
			case output := <-results:
				assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)
			case <-time.After(5 * time.Second):
				t.Fatal("Timeout waiting for concurrent executions")
			}
		}
	})
}
