package runtime

import (
	"context"
	"sync"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"go.uber.org/zap"
)

func TestSandboxMemoryMonitoring(t *testing.T) {
	// Create a logger for testing
	logger, _ := zap.NewDevelopment()

	t.Run("memory monitoring start and stop", func(t *testing.T) {
		// Create a sandbox with memory monitoring
		sandbox := NewSandbox(SandboxConfig{
			Logger:      logger,
			MemoryLimit: 10 * 1024 * 1024, // 10 MB
		})

		// Start memory monitoring
		sandbox.startMemoryMonitoring()

		// Verify that monitoring is active
		sandbox.mutex.Lock()
		assert.NotNil(t, sandbox.stopMemCheck)
		sandbox.mutex.Unlock()

		// Stop memory monitoring
		sandbox.stopMemoryMonitoring()

		// Verify that monitoring is stopped
		sandbox.mutex.Lock()
		assert.Nil(t, sandbox.stopMemCheck)
		sandbox.mutex.Unlock()
	})

	t.Run("memory monitoring concurrent access", func(t *testing.T) {
		// Create a sandbox with memory monitoring
		sandbox := NewSandbox(SandboxConfig{
			Logger:      logger,
			MemoryLimit: 10 * 1024 * 1024, // 10 MB
		})

		// Start memory monitoring
		sandbox.startMemoryMonitoring()

		// Create a wait group for concurrent operations
		var wg sync.WaitGroup
		wg.Add(10)

		// Simulate concurrent access to memory monitoring
		for i := 0; i < 10; i++ {
			go func() {
				defer wg.Done()
				// Start and stop memory monitoring concurrently
				sandbox.startMemoryMonitoring()
				time.Sleep(10 * time.Millisecond)
				sandbox.stopMemoryMonitoring()
			}()
		}

		// Wait for all goroutines to complete
		wg.Wait()

		// Verify that memory monitoring is in a consistent state
		sandbox.mutex.Lock()
		assert.Nil(t, sandbox.stopMemCheck)
		sandbox.mutex.Unlock()
	})

	t.Run("memory limit exceeded", func(t *testing.T) {
		// Create a sandbox with a very low memory limit
		sandbox := NewSandbox(SandboxConfig{
			Logger:      logger,
			MemoryLimit: 1, // 1 byte (will be exceeded immediately)
		})

		// Start memory monitoring
		sandbox.startMemoryMonitoring()

		// Wait for memory monitoring to detect the limit exceeded
		time.Sleep(200 * time.Millisecond)

		// Verify that the sandbox was interrupted
		sandbox.mutex.Lock()
		assert.True(t, sandbox.interrupted)
		sandbox.mutex.Unlock()

		// Stop memory monitoring
		sandbox.stopMemoryMonitoring()
	})

	t.Run("memory monitoring in execute", func(t *testing.T) {
		// Create a sandbox with memory monitoring
		sandbox := NewSandbox(SandboxConfig{
			Logger:      logger,
			MemoryLimit: 10 * 1024 * 1024, // 10 MB
		})

		// Execute a simple function that should not exceed memory limits
		input := FunctionInput{
			Code: "function test() { return 'hello'; }; test();",
		}

		// Execute the function
		output, err := sandbox.Execute(context.Background(), input)
		assert.NoError(t, err)
		assert.NotNil(t, output)
		assert.Equal(t, "hello", output.Result)

		// Verify that memory monitoring was started and stopped
		sandbox.mutex.Lock()
		assert.Nil(t, sandbox.stopMemCheck)
		sandbox.mutex.Unlock()
	})

	t.Run("memory limit exceeded in execute", func(t *testing.T) {
		// Create a sandbox with a very low memory limit
		sandbox := NewSandbox(SandboxConfig{
			Logger:      logger,
			MemoryLimit: 1, // 1 byte (will be exceeded immediately)
		})

		// Execute a function that allocates memory
		input := FunctionInput{
			Code: "function test() { const arr = new Array(1000000).fill('x'); return arr.length; }; test();",
		}

		// Execute the function
		_, err := sandbox.Execute(context.Background(), input)
		assert.Error(t, err)
		assert.Contains(t, err.Error(), "memory limit exceeded")

		// Verify that memory monitoring was started and stopped
		sandbox.mutex.Lock()
		assert.Nil(t, sandbox.stopMemCheck)
		sandbox.mutex.Unlock()
	})
}
