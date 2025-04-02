package sandbox

import (
	"context"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"go.uber.org/zap"
)

func TestSandboxBasicExecution(t *testing.T) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()

	// Create a sandbox
	config := SandboxConfig{
		MemoryLimit:   64 * 1024 * 1024, // 64MB
		TimeoutMillis: 1000,             // 1 second
		StackSize:     8 * 1024 * 1024,  // 8MB
		Logger:        logger,
	}

	sb := New(config)
	defer sb.Close()

	// Create simple JavaScript code
	code := `
		function main(args) {
			console.log("Hello from sandbox!");
			return {success: true, message: "test completed"};
		}
	`

	// Create function input
	input := FunctionInput{
		Code:    code,
		Args:    []interface{}{"test"},
		Context: NewFunctionContext("test-function"),
	}

	// Execute code
	output := sb.Execute(context.Background(), input)

	// Verify output
	assert.Empty(t, output.Error, "Expected no error")
	assert.NotNil(t, output.Result, "Expected a result")
	assert.Greater(t, output.Duration, time.Duration(0), "Expected duration > 0")
	assert.Greater(t, output.MemoryUsed, uint64(0), "Expected memory usage > 0")
	assert.NotEmpty(t, output.Logs, "Expected logs")

	// Check result
	resultMap, ok := output.Result.(map[string]interface{})
	assert.True(t, ok, "Result should be a map")
	assert.Equal(t, true, resultMap["success"], "Expected success: true")
	assert.Equal(t, "test completed", resultMap["message"], "Expected correct message")
}

func TestSandboxTimeout(t *testing.T) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()

	// Create a sandbox with a very short timeout
	config := SandboxConfig{
		MemoryLimit:   64 * 1024 * 1024, // 64MB
		TimeoutMillis: 100,              // 100ms (very short)
		StackSize:     8 * 1024 * 1024,  // 8MB
		Logger:        logger,
	}

	sb := New(config)
	defer sb.Close()

	// Create JavaScript code with an infinite loop
	code := `
		function main(args) {
			console.log("Starting infinite loop");
			while (true) {
				// Infinite loop
			}
			return "This should never be reached";
		}
	`

	// Create function input
	input := FunctionInput{
		Code:    code,
		Args:    []interface{}{"test"},
		Context: NewFunctionContext("timeout-test"),
	}

	// Execute code
	output := sb.Execute(context.Background(), input)

	// Verify timeout occurred
	assert.NotEmpty(t, output.Error, "Expected error due to timeout")
	assert.Contains(t, output.Error, "timed out", "Expected timeout error message")
	assert.NotEmpty(t, output.Logs, "Expected logs")
}

func TestSandboxErrorHandling(t *testing.T) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()

	// Create a sandbox
	config := SandboxConfig{
		MemoryLimit:   64 * 1024 * 1024, // 64MB
		TimeoutMillis: 1000,             // 1 second
		StackSize:     8 * 1024 * 1024,  // 8MB
		Logger:        logger,
	}

	sb := New(config)
	defer sb.Close()

	// Create JavaScript code with an error
	code := `
		function main(args) {
			console.log("About to throw an error");
			throw new Error("Test error");
			return "This should never be reached";
		}
	`

	// Create function input
	input := FunctionInput{
		Code:    code,
		Args:    []interface{}{"test"},
		Context: NewFunctionContext("error-test"),
	}

	// Execute code
	output := sb.Execute(context.Background(), input)

	// Verify error is captured
	assert.NotEmpty(t, output.Error, "Expected error to be captured")
	assert.Contains(t, output.Error, "Test error", "Expected specific error message")
	assert.NotEmpty(t, output.Logs, "Expected logs")
}
