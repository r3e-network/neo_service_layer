package runtime

import (
	"context"
	"fmt"
	"sync"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
	"go.uber.org/zap"
)

// TestSandbox_Performance tests the performance characteristics of the sandbox
// under various load conditions.
func TestSandbox_Performance(t *testing.T) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()

	// Create a standard sandbox configuration for testing
	sandboxConfig := SandboxConfig{
		Logger:        logger,
		MemoryLimit:   10 * 1024 * 1024, // 10MB
		TimeoutMillis: 5000,
	}

	t.Run("ExecutionTime", func(t *testing.T) {
		sandbox := NewSandbox(sandboxConfig)

		// Test code that performs a computationally intensive task
		code := `
function main(args) {
    const start = Date.now();
    
    // Perform a computationally intensive task
    let result = 0;
    for (let i = 0; i < 1000000; i++) {
        result += Math.sqrt(i);
    }
    
    const end = Date.now();
    return {
        result: result,
        executionTime: end - start
    };
}
`
		input := FunctionInput{
			Code: code,
		}

		// Measure the execution time
		startTime := time.Now()
		output, err := sandbox.Execute(context.Background(), input)
		executionTime := time.Since(startTime)

		require.NoError(t, err, "Execute should not return an error")
		assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)

		// Log the execution time
		t.Logf("Execution time: %v", executionTime)

		// Verify the result
		result, ok := output.Result.(map[string]interface{})
		assert.True(t, ok, "Result should be a map")
		assert.NotNil(t, result["result"], "Result should not be nil")
		assert.NotNil(t, result["executionTime"], "Execution time should not be nil")

		// The JavaScript execution time should be less than the Go execution time
		jsExecutionTime, ok := result["executionTime"].(float64)
		assert.True(t, ok, "JS execution time should be a number")
		assert.Less(t, jsExecutionTime, float64(executionTime.Milliseconds()),
			"JS execution time should be less than Go execution time")
	})

	t.Run("MemoryUsage", func(t *testing.T) {
		sandbox := NewSandbox(sandboxConfig)

		// Test code that allocates memory
		code := `
function main(args) {
    // Allocate memory in chunks to measure usage
    const arrays = [];
    const chunkSize = 100000;
    const numChunks = 10;
    
    for (let i = 0; i < numChunks; i++) {
        const arr = new Array(chunkSize).fill(i);
        arrays.push(arr);
    }
    
    return {
        allocatedChunks: arrays.length,
        totalElements: arrays.length * chunkSize
    };
}
`
		input := FunctionInput{
			Code: code,
		}

		// Execute the code
		output, err := sandbox.Execute(context.Background(), input)
		require.NoError(t, err, "Execute should not return an error")
		assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)

		// Verify the result
		result, ok := output.Result.(map[string]interface{})
		assert.True(t, ok, "Result should be a map")
		assert.Equal(t, float64(10), result["allocatedChunks"], "Should have allocated 10 chunks")
		assert.Equal(t, float64(1000000), result["totalElements"], "Should have allocated 1,000,000 elements")

		// Log the memory usage
		t.Logf("Memory used: %d bytes", sandbox.memoryUsed)
		assert.Greater(t, sandbox.memoryUsed, int64(0), "Memory usage should be greater than 0")
	})

	t.Run("ConcurrentExecution", func(t *testing.T) {
		// Create multiple sandboxes
		numSandboxes := 10
		sandboxes := make([]*Sandbox, numSandboxes)
		for i := 0; i < numSandboxes; i++ {
			sandboxes[i] = NewSandbox(sandboxConfig)
		}

		// Test code that performs a simple calculation
		code := `
function main(args) {
    const id = args.id;
    let result = 0;
    
    // Do some work
    for (let i = 0; i < 100000; i++) {
        result += i;
    }
    
    return {
        id: id,
        result: result
    };
}
`
		// Execute the code concurrently
		var wg sync.WaitGroup
		results := make([]interface{}, numSandboxes)
		errors := make([]error, numSandboxes)

		startTime := time.Now()

		for i := 0; i < numSandboxes; i++ {
			wg.Add(1)
			go func(index int) {
				defer wg.Done()

				input := FunctionInput{
					Code: code,
					Args: map[string]interface{}{
						"id": index,
					},
				}

				output, err := sandboxes[index].Execute(context.Background(), input)
				if err != nil {
					errors[index] = err
					return
				}

				if output.Error != "" {
					errors[index] = fmt.Errorf("sandbox error: %s", output.Error)
					return
				}

				results[index] = output.Result
			}(i)
		}

		wg.Wait()
		executionTime := time.Since(startTime)

		// Log the total execution time
		t.Logf("Total concurrent execution time for %d sandboxes: %v", numSandboxes, executionTime)

		// Verify the results
		for i := 0; i < numSandboxes; i++ {
			assert.NoError(t, errors[i], "Sandbox %d should not return an error", i)

			result, ok := results[i].(map[string]interface{})
			assert.True(t, ok, "Result %d should be a map", i)
			assert.Equal(t, float64(i), result["id"], "Result %d should have correct ID", i)
			assert.NotNil(t, result["result"], "Result %d should have a result", i)
		}
	})

	t.Run("LongRunningExecution", func(t *testing.T) {
		// Create a sandbox with a longer timeout
		sandbox := NewSandbox(SandboxConfig{
			Logger:        logger,
			TimeoutMillis: 10000, // 10 seconds
		})

		// Test code that runs for a long time but should complete within the timeout
		code := `
function main(args) {
    const start = Date.now();
    
    // Sleep for 5 seconds using a busy wait
    const sleepTime = 5000; // 5 seconds
    while (Date.now() - start < sleepTime) {
        // Busy wait
    }
    
    const end = Date.now();
    return {
        sleepTime: sleepTime,
        actualTime: end - start
    };
}
`
		input := FunctionInput{
			Code: code,
		}

		// Execute the code
		startTime := time.Now()
		output, err := sandbox.Execute(context.Background(), input)
		executionTime := time.Since(startTime)

		require.NoError(t, err, "Execute should not return an error")
		assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)

		// Log the execution time
		t.Logf("Long-running execution time: %v", executionTime)

		// Verify the result
		result, ok := output.Result.(map[string]interface{})
		assert.True(t, ok, "Result should be a map")
		assert.Equal(t, float64(5000), result["sleepTime"], "Sleep time should be 5000ms")

		actualTime, ok := result["actualTime"].(float64)
		assert.True(t, ok, "Actual time should be a number")
		assert.GreaterOrEqual(t, actualTime, float64(5000), "Actual time should be at least 5000ms")
	})

	t.Run("TimeoutExecution", func(t *testing.T) {
		// Create a sandbox with a short timeout
		sandbox := NewSandbox(SandboxConfig{
			Logger:        logger,
			TimeoutMillis: 1000, // 1 second
		})

		// Test code that runs for longer than the timeout
		code := `
function main(args) {
    const start = Date.now();
    
    // Try to run for 5 seconds
    while (true) {
        if (Date.now() - start > 5000) {
            break;
        }
    }
    
    return "This should not be returned due to timeout";
}
`
		input := FunctionInput{
			Code: code,
		}

		// Execute the code
		startTime := time.Now()
		output, err := sandbox.Execute(context.Background(), input)
		executionTime := time.Since(startTime)

		require.NoError(t, err, "Execute should not return an error")
		assert.NotEmpty(t, output.Error, "Expected error in output due to timeout")
		assert.Contains(t, output.Error, "timeout", "Error should mention timeout")

		// Log the execution time
		t.Logf("Timeout execution time: %v", executionTime)

		// The execution time should be close to the timeout
		assert.Less(t, executionTime, 2*time.Second, "Execution time should be less than 2 seconds")
		assert.GreaterOrEqual(t, executionTime, 1*time.Second, "Execution time should be at least 1 second")
	})

	t.Run("MemoryLimitExceeded", func(t *testing.T) {
		// Create a sandbox with a small memory limit
		sandbox := NewSandbox(SandboxConfig{
			Logger:      logger,
			MemoryLimit: 1 * 1024 * 1024, // 1MB
		})

		// Test code that tries to allocate more memory than the limit
		code := `
function main(args) {
    // Try to allocate more memory than the limit
    const arrays = [];
    const chunkSize = 100000;
    
    // Keep allocating until we hit the limit
    while (true) {
        const arr = new Array(chunkSize).fill(Math.random());
        arrays.push(arr);
    }
    
    return "This should not be returned due to memory limit";
}
`
		input := FunctionInput{
			Code: code,
		}

		// Execute the code
		output, err := sandbox.Execute(context.Background(), input)
		require.NoError(t, err, "Execute should not return an error")
		assert.NotEmpty(t, output.Error, "Expected error in output due to memory limit")
		assert.Contains(t, output.Error, "memory", "Error should mention memory limit")

		// Log the memory usage
		t.Logf("Memory used before limit exceeded: %d bytes", sandbox.memoryUsed)
		assert.Greater(t, sandbox.memoryUsed, int64(0), "Memory usage should be greater than 0")
	})

	t.Run("RepeatedExecution", func(t *testing.T) {
		sandbox := NewSandbox(sandboxConfig)

		// Simple test code
		code := `
function main(args) {
    return {
        iteration: args.iteration,
        timestamp: Date.now()
    };
}
`
		// Execute the code multiple times
		numIterations := 100
		executionTimes := make([]time.Duration, numIterations)

		for i := 0; i < numIterations; i++ {
			input := FunctionInput{
				Code: code,
				Args: map[string]interface{}{
					"iteration": i,
				},
			}

			startTime := time.Now()
			output, err := sandbox.Execute(context.Background(), input)
			executionTimes[i] = time.Since(startTime)

			require.NoError(t, err, "Execute should not return an error")
			assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)

			// Verify the result
			result, ok := output.Result.(map[string]interface{})
			assert.True(t, ok, "Result should be a map")

			// Handle potential int64 or float64 from Goja
			iterationValue := result["iteration"]
			assert.Equal(t, fmt.Sprintf("%v", i), fmt.Sprintf("%v", iterationValue), "Iteration should match")
		}

		// Calculate average execution time
		var totalTime time.Duration
		for _, execTime := range executionTimes {
			totalTime += execTime
		}
		averageTime := totalTime / time.Duration(numIterations)

		// Log the execution times
		t.Logf("Average execution time over %d iterations: %v", numIterations, averageTime)

		// Verify that the sandbox is reusable
		assert.Less(t, averageTime, 50*time.Millisecond,
			"Average execution time should be reasonable for a simple function")
	})

	t.Run("ComplexJSOperations", func(t *testing.T) {
		sandbox := NewSandbox(sandboxConfig)

		// Test code that performs complex JS operations
		code := `
function main(args) {
    // Create a complex object
    const obj = {
        nested: {
            array: [1, 2, 3, 4, 5],
            map: new Map([
                ['a', 1],
                ['b', 2],
                ['c', 3]
            ]),
            set: new Set([1, 2, 3, 4, 5])
        },
        date: new Date(),
        regex: /^test.*/i,
        func: function(x) { return x * x; }
    };
    
    // Perform operations on the object
    const results = {
        mapValues: Array.from(obj.nested.map.values()),
        setSize: obj.nested.set.size,
        arraySum: obj.nested.array.reduce((a, b) => a + b, 0),
        dateString: obj.date.toISOString(),
        regexTest: obj.regex.test('Test string'),
        functionResult: obj.func(5)
    };
    
    // Test JSON operations
    const jsonString = JSON.stringify(results);
    const parsedJson = JSON.parse(jsonString);
    
    return {
        original: results,
        roundTrip: parsedJson
    };
}
`
		input := FunctionInput{
			Code: code,
		}

		// Execute the code
		startTime := time.Now()
		output, err := sandbox.Execute(context.Background(), input)
		executionTime := time.Since(startTime)

		require.NoError(t, err, "Execute should not return an error")
		assert.Empty(t, output.Error, "Unexpected error in output: %s", output.Error)

		// Log the execution time
		t.Logf("Complex JS operations execution time: %v", executionTime)

		// Verify the result
		result, ok := output.Result.(map[string]interface{})
		assert.True(t, ok, "Result should be a map")

		original, ok := result["original"].(map[string]interface{})
		assert.True(t, ok, "Original should be a map")

		roundTrip, ok := result["roundTrip"].(map[string]interface{})
		assert.True(t, ok, "RoundTrip should be a map")

		// Verify specific values, comparing string representations for type flexibility
		assert.Equal(t, "15", fmt.Sprintf("%v", original["arraySum"]), "Array sum should be 15")
		assert.Equal(t, "25", fmt.Sprintf("%v", original["functionResult"]), "Function result should be 25")
		assert.Equal(t, "15", fmt.Sprintf("%v", roundTrip["arraySum"]), "Round-trip array sum should be 15")
	})
}

// BenchmarkSandbox_Execute benchmarks the execution of JavaScript code in the sandbox.
func BenchmarkSandbox_Execute(b *testing.B) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()

	// Create a sandbox
	sandbox := NewSandbox(SandboxConfig{
		Logger: logger,
	})

	// Simple test code
	code := `
function main(args) {
    let result = 0;
    for (let i = 0; i < 1000; i++) {
        result += i;
    }
    return result;
}
`
	input := FunctionInput{
		Code: code,
	}

	// Reset the timer before the benchmark loop
	b.ResetTimer()

	// Run the benchmark
	for i := 0; i < b.N; i++ {
		output, err := sandbox.Execute(context.Background(), input)
		if err != nil {
			b.Fatalf("Execute returned an error: %v", err)
		}
		if output.Error != "" {
			b.Fatalf("Execute returned an error in output: %s", output.Error)
		}
	}
}

// BenchmarkSandbox_ComplexCode benchmarks the execution of complex JavaScript code in the sandbox.
func BenchmarkSandbox_ComplexCode(b *testing.B) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()

	// Create a sandbox
	sandbox := NewSandbox(SandboxConfig{
		Logger: logger,
	})

	// Complex test code
	code := `
function fibonacci(n) {
    if (n <= 1) return n;
    return fibonacci(n - 1) + fibonacci(n - 2);
}

function isPrime(num) {
    if (num <= 1) return false;
    if (num <= 3) return true;
    if (num % 2 === 0 || num % 3 === 0) return false;
    
    let i = 5;
    while (i * i <= num) {
        if (num % i === 0 || num % (i + 2) === 0) return false;
        i += 6;
    }
    return true;
}

function main(args) {
    const results = {
        fibonacci: [],
        primes: []
    };
    
    // Calculate Fibonacci numbers
    for (let i = 0; i < 20; i++) {
        results.fibonacci.push(fibonacci(i));
    }
    
    // Find prime numbers
    for (let i = 2; i < 100; i++) {
        if (isPrime(i)) {
            results.primes.push(i);
        }
    }
    
    return results;
}
`
	input := FunctionInput{
		Code: code,
	}

	// Reset the timer before the benchmark loop
	b.ResetTimer()

	// Run the benchmark
	for i := 0; i < b.N; i++ {
		output, err := sandbox.Execute(context.Background(), input)
		if err != nil {
			b.Fatalf("Execute returned an error: %v", err)
		}
		if output.Error != "" {
			b.Fatalf("Execute returned an error in output: %s", output.Error)
		}
	}
}

// BenchmarkSandbox_Concurrent benchmarks concurrent execution of JavaScript code in multiple sandboxes.
func BenchmarkSandbox_Concurrent(b *testing.B) {
	// Create a test logger
	logger, _ := zap.NewDevelopment()

	// Number of concurrent sandboxes
	numSandboxes := 10

	// Create multiple sandboxes
	sandboxes := make([]*Sandbox, numSandboxes)
	for i := 0; i < numSandboxes; i++ {
		sandboxes[i] = NewSandbox(SandboxConfig{
			Logger: logger,
		})
	}

	// Simple test code
	code := `
function main(args) {
    let result = 0;
    for (let i = 0; i < 1000; i++) {
        result += i;
    }
    return {
        id: args.id,
        result: result
    };
}
`
	// Reset the timer before the benchmark loop
	b.ResetTimer()

	// Run the benchmark
	for i := 0; i < b.N; i++ {
		var wg sync.WaitGroup

		for j := 0; j < numSandboxes; j++ {
			wg.Add(1)
			go func(index int) {
				defer wg.Done()

				input := FunctionInput{
					Code: code,
					Args: map[string]interface{}{
						"id": index,
					},
				}

				_, err := sandboxes[index].Execute(context.Background(), input)
				if err != nil {
					b.Errorf("Execute returned an error: %v", err)
				}
			}(j)
		}

		wg.Wait()
	}
}
