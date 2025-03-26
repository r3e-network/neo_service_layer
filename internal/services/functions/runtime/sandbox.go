package runtime

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"sync"
	"time"

	"github.com/dop251/goja"
)

// Resource constraints for the sandbox
const (
	DefaultMemoryLimit   = 128 * 1024 * 1024 // 128 MB
	DefaultTimeoutMillis = 5000              // 5 seconds
	DefaultStackSize     = 8 * 1024 * 1024   // 8 MB
)

// SandboxConfig holds configuration for the JavaScript sandbox
type SandboxConfig struct {
	MemoryLimit   int64
	TimeoutMillis int64
	StackSize     int32
	AllowNetwork  bool
	AllowFileIO   bool
}

// Sandbox represents a JavaScript execution environment
type Sandbox struct {
	vm         *goja.Runtime
	config     SandboxConfig
	mutex      sync.Mutex
	interrupted bool
	memoryUsed  int64
}

// FunctionInput represents input to a function execution
type FunctionInput struct {
	Code       string                 `json:"code"`
	Args       map[string]interface{} `json:"args"`
	Secrets    map[string]string      `json:"secrets,omitempty"`
	Parameters map[string]interface{} `json:"parameters,omitempty"`
}

// FunctionOutput represents output from a function execution
type FunctionOutput struct {
	Result     interface{} `json:"result"`
	Logs       []string    `json:"logs"`
	Error      string      `json:"error,omitempty"`
	Duration   int64       `json:"duration"`
	MemoryUsed int64       `json:"memoryUsed"`
}

// NewSandbox creates a new JavaScript sandbox
func NewSandbox(config SandboxConfig) *Sandbox {
	// Apply default values if not specified
	if config.MemoryLimit <= 0 {
		config.MemoryLimit = DefaultMemoryLimit
	}
	if config.TimeoutMillis <= 0 {
		config.TimeoutMillis = DefaultTimeoutMillis
	}
	if config.StackSize <= 0 {
		config.StackSize = DefaultStackSize
	}

	vm := goja.New()
	
	return &Sandbox{
		vm:     vm,
		config: config,
	}
}

// Execute runs JavaScript code in the sandbox
func (s *Sandbox) Execute(ctx context.Context, input FunctionInput) (*FunctionOutput, error) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	// Reset state
	s.interrupted = false
	s.memoryUsed = 0
	s.vm = goja.New()

	// Set up console object for logging
	logs := []string{}
	console := map[string]interface{}{
		"log": func(args ...interface{}) {
			message := fmt.Sprint(args...)
			logs = append(logs, message)
		},
		"error": func(args ...interface{}) {
			message := fmt.Sprint(args...)
			logs = append(logs, "ERROR: "+message)
		},
		"info": func(args ...interface{}) {
			message := fmt.Sprint(args...)
			logs = append(logs, "INFO: "+message)
		},
		"warn": func(args ...interface{}) {
			message := fmt.Sprint(args...)
			logs = append(logs, "WARN: "+message)
		},
	}
	err := s.vm.Set("console", console)
	if err != nil {
		return nil, fmt.Errorf("failed to set console object: %w", err)
	}

	// Set up args object
	err = s.vm.Set("args", input.Args)
	if err != nil {
		return nil, fmt.Errorf("failed to set args object: %w", err)
	}

	// Set up secrets object if provided
	if input.Secrets != nil {
		err = s.vm.Set("secrets", input.Secrets)
		if err != nil {
			return nil, fmt.Errorf("failed to set secrets object: %w", err)
		}
	}

	// Set up parameters object if provided
	if input.Parameters != nil {
		err = s.vm.Set("parameters", input.Parameters)
		if err != nil {
			return nil, fmt.Errorf("failed to set parameters object: %w", err)
		}
	}

	// Set up execution timeout
	timer := time.NewTimer(time.Duration(s.config.TimeoutMillis) * time.Millisecond)
	defer timer.Stop()

	// Create a cancellable context if not provided
	if ctx == nil {
		var cancel context.CancelFunc
		ctx, cancel = context.WithTimeout(context.Background(), time.Duration(s.config.TimeoutMillis)*time.Millisecond)
		defer cancel()
	}

	// Run the code with timeout
	resultChan := make(chan *FunctionOutput, 1)
	errChan := make(chan error, 1)

	go func() {
		startTime := time.Now()

		// Wrap user code to call the main function
		wrappedCode := fmt.Sprintf(`
			(function() {
				%s
				if (typeof main !== 'function') {
					throw new Error('main function is not defined');
				}
				return main(args);
			})()
		`, input.Code)

		// Execute the code
		value, err := s.vm.RunString(wrappedCode)
		duration := time.Since(startTime).Milliseconds()

		if err != nil {
			// Handle execution errors
			var jsErr *goja.Exception
			if errors.As(err, &jsErr) {
				errChan <- fmt.Errorf("javascript error: %s", jsErr.Value())
			} else {
				errChan <- fmt.Errorf("execution error: %w", err)
			}
			return
		}

		// Convert the result to Go value
		var result interface{}
		if value != nil {
			result = value.Export()
		}

		resultChan <- &FunctionOutput{
			Result:     result,
			Logs:       logs,
			Duration:   duration,
			MemoryUsed: s.memoryUsed,
		}
	}()

	// Wait for execution to complete or timeout
	select {
	case <-ctx.Done():
		s.interrupted = true
		return &FunctionOutput{
			Error:    "function execution timed out",
			Logs:     logs,
			Duration: int64(s.config.TimeoutMillis),
		}, nil
	case <-timer.C:
		s.interrupted = true
		return &FunctionOutput{
			Error:    "function execution timed out",
			Logs:     logs,
			Duration: int64(s.config.TimeoutMillis),
		}, nil
	case err := <-errChan:
		return &FunctionOutput{
			Error:      err.Error(),
			Logs:       logs,
			Duration:   int64(s.config.TimeoutMillis), // Approximate
			MemoryUsed: s.memoryUsed,
		}, nil
	case result := <-resultChan:
		return result, nil
	}
}

// ExecuteJSON runs JavaScript code with JSON-serialized input and output
func (s *Sandbox) ExecuteJSON(ctx context.Context, jsonInput string) (string, error) {
	var input FunctionInput
	err := json.Unmarshal([]byte(jsonInput), &input)
	if err != nil {
		return "", fmt.Errorf("failed to parse input JSON: %w", err)
	}

	output, err := s.Execute(ctx, input)
	if err != nil {
		return "", err
	}

	jsonOutput, err := json.Marshal(output)
	if err != nil {
		return "", fmt.Errorf("failed to serialize output to JSON: %w", err)
	}

	return string(jsonOutput), nil
}