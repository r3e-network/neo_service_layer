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

func TestSandbox_Execute(t *testing.T) {
	logger, _ := zap.NewDevelopment()
	sandbox := newTestSandbox(logger)

	tests := []struct {
		name           string
		code           string
		args           map[string]interface{}
		secrets        map[string]string
		expectedResult interface{}
		wantError      bool
		checkFunc      func(*testing.T, *FunctionOutput)
	}{
		{
			name:           "simple_addition",
			code:           `function main(args) { return args.a + args.b; }`,
			args:           map[string]interface{}{"a": 5, "b": 3},
			expectedResult: int64(8), // Goja returns int64 for whole numbers
			wantError:      false,
		},
		{
			name:           "console_logging",
			code:           `function main(args) { console.log("Hello, world!"); console.info("Info message"); console.warn("Warning message"); console.error("Error message"); return 1; }`,
			args:           map[string]interface{}{},
			expectedResult: int64(1),
			wantError:      false,
			checkFunc: func(t *testing.T, output *FunctionOutput) {
				assert.Len(t, output.Logs, 4)
				assert.Contains(t, output.Logs, "Hello, world!")
				assert.Contains(t, output.Logs, "INFO: Info message")
				assert.Contains(t, output.Logs, "WARN: Warning message")
				assert.Contains(t, output.Logs, "ERROR: Error message")
			},
		},
		{
			name:           "missing_main_function",
			code:           `function notMain() { return 1; }`,
			expectedResult: nil,
			wantError:      true,
			checkFunc: func(t *testing.T, output *FunctionOutput) {
				assert.Contains(t, output.Error, "main function is not defined")
			},
		},
		{
			name:           "syntax_error",
			code:           `function main(args { return 1; }`, // Syntax error
			expectedResult: nil,
			wantError:      true,
			checkFunc: func(t *testing.T, output *FunctionOutput) {
				assert.NotEmpty(t, output.Error)
			},
		},
		{
			name: "argument_passing",
			code: `
function main(args) {
    return {
        sum: args.a + args.b,
        product: args.a * args.b,
        message: args.message
    };
}`,
			args: map[string]interface{}{
				"a":       5,
				"b":       3,
				"message": "hello",
			},
			wantError: false,
			checkFunc: func(t *testing.T, output *FunctionOutput) {
				result, ok := output.Result.(map[string]interface{})
				assert.True(t, ok)
				assert.Equal(t, int64(8), result["sum"])
				assert.Equal(t, int64(15), result["product"])
				assert.Equal(t, "hello", result["message"])
			},
		},
		{
			name: "using_secrets",
			code: `function main(args) { console.log("API Key:" + secrets.apiKey); return secrets.apiKey; }`,
			secrets: map[string]string{
				"apiKey": "test-api-key",
			},
			expectedResult: "test-api-key",
			wantError:      false,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			input := FunctionInput{
				Code:    tt.code,
				Args:    tt.args,
				Secrets: tt.secrets,
			}
			output, err := sandbox.Execute(context.Background(), input)
			require.NoError(t, err) // Execute itself should not error
			assert.NotNil(t, output)

			if tt.wantError {
				assert.NotEmpty(t, output.Error, "Expected error, but got none")
			} else {
				assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)
				// Check the result only if no error was expected
				if tt.checkFunc != nil {
					tt.checkFunc(t, output)
				} else if tt.expectedResult != nil {
					assert.Equal(t, tt.expectedResult, output.Result)
				}
			}
		})
	}
}

func TestSandbox_Timeout(t *testing.T) {
	// Create sandbox with short timeout
	sandbox := NewSandbox(SandboxConfig{
		TimeoutMillis: 100, // 100ms timeout
	})

	input := FunctionInput{
		Code: `
function main(args) {
    // Infinite loop
    while(true) {}
    return "This should never be reached";
}`,
	}

	// Execute function
	output, err := sandbox.Execute(context.Background(), input)
	require.NoError(t, err, "Execute should not return an error")

	// Check for timeout
	assert.NotEmpty(t, output.Error, "Expected timeout error")
	assert.Equal(t, "function execution timed out", output.Error, "Error should mention timeout")
	assert.GreaterOrEqual(t, output.Duration, int64(100), "Duration should be at least timeout value")
}

func TestSandbox_ExecuteJSON(t *testing.T) {
	sandbox := NewSandbox(SandboxConfig{})

	jsonInput := `{
		"code": "function main(args) { return args.a + args.b; }",
		"args": { "a": 5, "b": 3 }
	}`

	jsonOutput, err := sandbox.ExecuteJSON(context.Background(), jsonInput)
	require.NoError(t, err)

	// Check output format
	assert.True(t, strings.Contains(jsonOutput, `"result":8`), "Result should be 8")
	assert.True(t, strings.Contains(jsonOutput, `"logs":`), "Output should contain logs array")
	assert.True(t, strings.Contains(jsonOutput, `"duration":`), "Output should contain duration")
}

func TestSandbox_InvalidJSON(t *testing.T) {
	sandbox := NewSandbox(SandboxConfig{})

	// Invalid JSON input
	jsonInput := `{ invalid json }`

	_, err := sandbox.ExecuteJSON(context.Background(), jsonInput)
	assert.Error(t, err, "Should return error for invalid JSON")
	assert.Contains(t, err.Error(), "parse", "Error should mention parsing failure")
}

func TestSandbox_ContextCancellation(t *testing.T) {
	sandbox := NewSandbox(SandboxConfig{
		TimeoutMillis: 5000, // 5s timeout in sandbox
	})

	// Create a context with a short timeout
	ctx, cancel := context.WithTimeout(context.Background(), 100*time.Millisecond)
	defer cancel()

	input := FunctionInput{
		Code: `
function main(args) {
    // Long running operation
    for(let i = 0; i < 1000000000; i++) {
        // Just waste time
    }
    return "This should never be reached";
}`,
	}

	// Execute function
	output, err := sandbox.Execute(ctx, input)
	require.NoError(t, err)

	// Check for timeout
	assert.NotEmpty(t, output.Error, "Expected timeout error")
	assert.Contains(t, output.Error, "timed out")
}

func TestSandbox_Parameters(t *testing.T) {
	sandbox := NewSandbox(SandboxConfig{})

	input := FunctionInput{
		Code: `
function main(args) {
    return {
        fromArgs: args.value,
        fromParams: parameters.config.setting
    };
}`,
		Args: map[string]interface{}{
			"value": "hello",
		},
		Parameters: map[string]interface{}{
			"config": map[string]interface{}{
				"setting": "enabled",
			},
		},
	}

	output, err := sandbox.Execute(context.Background(), input)
	require.NoError(t, err)
	require.Empty(t, output.Error)

	result, ok := output.Result.(map[string]interface{})
	assert.True(t, ok)
	assert.Equal(t, "hello", result["fromArgs"])
	assert.Equal(t, "enabled", result["fromParams"])
}

func TestSandbox_ComplexDataStructures(t *testing.T) {
	sandbox := NewSandbox(SandboxConfig{})

	input := FunctionInput{
		Code: `
function main(args) {
    // Test array manipulation
    const sum = args.numbers.reduce((a, b) => a + b, 0);
    
    // Test nested object access
    const nestedValue = args.nested.level1.level2.value;
    
    // Test date handling
    const date = new Date(args.timestamp);
    const year = date.getUTCFullYear();
    
    return {
        sum: sum,
        nestedValue: nestedValue,
        year: year,
        array: args.numbers.map(n => n * 2)
    };
}`,
		Args: map[string]interface{}{
			"numbers": []interface{}{1, 2, 3, 4, 5},
			"nested": map[string]interface{}{
				"level1": map[string]interface{}{
					"level2": map[string]interface{}{
						"value": "deeply nested",
					},
				},
			},
			"timestamp": "2023-06-15T12:00:00Z",
		},
	}

	output, err := sandbox.Execute(context.Background(), input)
	require.NoError(t, err)
	require.Empty(t, output.Error)

	result, ok := output.Result.(map[string]interface{})
	assert.True(t, ok)

	// Check sum calculation - handle both int64 and float64
	sumValue := result["sum"]
	switch v := sumValue.(type) {
	case int64:
		assert.Equal(t, int64(15), v)
	case float64:
		assert.Equal(t, float64(15), v)
	default:
		t.Fatalf("unexpected type for sum: %T", v)
	}

	// Check nested value access
	assert.Equal(t, "deeply nested", result["nestedValue"])

	// Check date handling - handle both int64 and float64
	yearValue := result["year"]
	switch v := yearValue.(type) {
	case int64:
		assert.Equal(t, int64(2023), v)
	case float64:
		assert.Equal(t, float64(2023), v)
	default:
		t.Fatalf("unexpected type for year: %T", v)
	}

	// Check array transformation
	array, ok := result["array"].([]interface{})
	assert.True(t, ok)

	// Check each element's type individually
	assert.Len(t, array, 5)
	for i, v := range array {
		expected := (i + 1) * 2
		switch val := v.(type) {
		case int64:
			assert.Equal(t, int64(expected), val)
		case float64:
			assert.Equal(t, float64(expected), val)
		default:
			t.Fatalf("unexpected type for array element: %T", val)
		}
	}
}

func TestSandbox_MultipleExecutions(t *testing.T) {
	sandbox := NewSandbox(SandboxConfig{})

	// First execution
	input1 := FunctionInput{
		Code: `
function main(args) {
    global = 42; // Try to set a global variable
    return "first";
}`,
	}

	output1, err := sandbox.Execute(context.Background(), input1)
	require.NoError(t, err)
	require.Empty(t, output1.Error)
	assert.Equal(t, "first", output1.Result)

	// Second execution
	input2 := FunctionInput{
		Code: `
function main(args) {
    // The global variable should not be accessible
    return typeof global === "undefined" ? "isolated" : global;
}`,
	}

	output2, err := sandbox.Execute(context.Background(), input2)
	require.NoError(t, err)
	require.Empty(t, output2.Error)
	assert.Equal(t, "isolated", output2.Result)
}

// Helper function to create a basic sandbox for testing
func newTestSandbox(logger *zap.Logger) *Sandbox {
	return NewSandbox(SandboxConfig{
		Logger: logger,
	})
}
