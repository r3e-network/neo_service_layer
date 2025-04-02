package sandbox

import (
	"context"
	"errors"
	"fmt"
	"time"

	"encoding/base64"

	"github.com/dop251/goja"
	secrets_iface "github.com/will/neo_service_layer/internal/services/secrets"
	"go.uber.org/zap"
)

var (
	// ErrTimeout is returned when script execution exceeds time limit
	ErrTimeout = errors.New("script execution timed out")

	// ErrMemoryLimit is returned when script exceeds memory limit
	ErrMemoryLimit = errors.New("script exceeded memory limit")

	// ErrExecutionInterrupted is returned when script execution is interrupted
	ErrExecutionInterrupted = errors.New("script execution was interrupted")
)

// Sandbox represents the JavaScript sandbox
type Sandbox struct {
	// Logger for sandbox operations
	logger *zap.Logger

	// TEE provider for secure operations (e.g., decryption)
	teeProvider secrets_iface.TEESecurityProvider

	// Store encrypted secrets passed during execution
	currentSecrets map[string]string
}

// New creates a new JavaScript sandbox
// ... (rest of function)

// Execute runs JavaScript code in the sandbox with the provided input
func (s *Sandbox) Execute(ctx context.Context, input FunctionInput) FunctionOutput {
	startTime := time.Now()

	// Use the parent context or create a new one
	if ctx == nil {
		ctx = context.Background()
	}

	// Create a context with timeout if configured
	var cancel context.CancelFunc
	if s.config.TimeoutMillis > 0 {
		ctx, cancel = context.WithTimeout(ctx, time.Duration(s.config.TimeoutMillis)*time.Millisecond)
		defer cancel()
	}

	// Reset interrupted flag
	s.SetInterrupted(false)

	// Create output structure
	output := FunctionOutput{
		Logs: []string{},
	}

	// Store secrets for this execution
	s.mutex.Lock() // Lock needed to modify currentSecrets safely
	s.currentSecrets = input.Secrets
	s.mutex.Unlock()
	defer func() { // Ensure secrets are cleared after execution
		s.mutex.Lock()
		s.currentSecrets = nil
		s.mutex.Unlock()
	}()

	// Start memory monitoring
	if s.memoryMonitor != nil {
		s.memoryMonitor.Start()
		defer s.memoryMonitor.Stop()
	}

	// Lock mutex to ensure thread safety
	s.mutex.Lock()

	// Reset VM to clean state
	s.resetVM()

	// Set up function context
	fnCtx := input.Context
	if fnCtx == nil {
		fnCtx = NewFunctionContext("anonymous")
	}

	// Initialize the execution environment
	err := s.setupExecutionEnvironment(fnCtx, &output)
	if err != nil {
		s.mutex.Unlock() // Unlock before returning
		output.Error = fmt.Sprintf("Failed to set up execution environment: %v", err)
		output.Duration = time.Since(startTime)
		if s.memoryMonitor != nil {
			output.MemoryUsed = s.memoryMonitor.GetCurrentUsage()
		}
		return output
	}

	// Execute the script in a separate goroutine to enable interruption
	resultCh := make(chan *executionResult, 1)
	go func() {
		defer func() {
			if r := recover(); r != nil {
				resultCh <- &executionResult{
					err: fmt.Errorf("script execution panicked: %v", r),
				}
			}
		}()

		// First, evaluate the script to define functions
		_, err := s.vm.RunString(input.Code)
		if err != nil {
			resultCh <- &executionResult{
				err: err,
			}
			return
		}

		// Then, try to call the main function with the provided arguments
		mainFn, ok := goja.AssertFunction(s.vm.Get("main"))
		if !ok {
			resultCh <- &executionResult{
				err: errors.New("main function not found in script"),
			}
			return
		}

		// Convert args to JavaScript values
		jsArgs := make([]goja.Value, len(input.Args))
		for i, arg := range input.Args {
			jsArgs[i] = s.vm.ToValue(arg)
		}

		// Call the main function
		result, err := mainFn(goja.Undefined(), jsArgs...)
		resultCh <- &executionResult{
			value: result,
			err:   err,
		}
	}()

	// Unlock to allow interruption (e.g., by memory monitor)
	s.mutex.Unlock()

	// Wait for result or cancellation
	var execResult *executionResult
	select {
	case <-ctx.Done():
		// Check if it's a timeout or cancellation
		if ctx.Err() == context.DeadlineExceeded {
			output.Error = ErrTimeout.Error()
		} else {
			output.Error = fmt.Sprintf("Script execution cancelled: %v", ctx.Err())
		}
		// Interrupt the VM
		s.SetInterrupted(true)
	case execResult = <-resultCh:
		// Process the result
		if execResult.err != nil {
			output.Error = execResult.err.Error()
		} else if s.IsInterrupted() {
			// Check if it was interrupted by memory monitor
			output.Error = ErrMemoryLimit.Error()
		} else {
			// Convert result to string representation
			if execResult.value != nil {
				// Try to export result to a Go value
				if exported, err := s.exportValue(execResult.value); err == nil {
					output.Result = exported
				} else {
					// Fall back to string representation if export fails
					output.Result = execResult.value.String()
				}
			}
		}
	}

	// Set output metadata
	output.Duration = time.Since(startTime)
	if s.memoryMonitor != nil {
		output.MemoryUsed = s.memoryMonitor.GetCurrentUsage()
	}

	return output
}

// executionResult holds the result of script execution
type executionResult struct {
	value goja.Value
	err   error
}

// exportValue attempts to convert a Goja value to a native Go value
func (s *Sandbox) exportValue(val goja.Value) (interface{}, error) {
	if val == nil {
		return nil, nil
	}

	// Try to export to Go value
	return val.Export(), nil
}

// setupExecutionEnvironment prepares the VM with necessary bindings and context
func (s *Sandbox) setupExecutionEnvironment(ctx *FunctionContext, output *FunctionOutput) error {
	// Set up console for logging
	console := s.createConsoleObject(output)
	err := s.vm.Set("console", console)
	if err != nil {
		return fmt.Errorf("failed to set console object: %w", err)
	}

	// Set up context object
	ctxObj, err := s.createContextObject(ctx)
	if err != nil {
		return fmt.Errorf("failed to create context object: %w", err)
	}

	err = s.vm.Set("context", ctxObj)
	if err != nil {
		return fmt.Errorf("failed to set context object: %w", err)
	}

	// Set up secrets object
	secretsObj, err := s.createSecretsObject(ctx)
	if err != nil {
		return fmt.Errorf("failed to create secrets object: %w", err)
	}
	err = s.vm.Set("secrets", secretsObj)
	if err != nil {
		return fmt.Errorf("failed to set secrets object: %w", err)
	}

	// Set up service bindings if enabled
	if s.config.EnableInteroperability && ctx.ServiceLayerURL != "" {
		err = s.setupServiceBindings(ctx)
		if err != nil {
			s.logger.Warn("Failed to set up service bindings", zap.Error(err))
			// Continue execution despite service binding failures
		}
	}

	return nil
}

// createConsoleObject creates a JavaScript console object with logging functions
func (s *Sandbox) createConsoleObject(output *FunctionOutput) map[string]interface{} {
	return map[string]interface{}{
		"log": func(args ...interface{}) {
			log := formatLogArgs(args...)
			output.Logs = append(output.Logs, log)
			s.logger.Debug("Script log", zap.String("message", log))
		},
		"info": func(args ...interface{}) {
			log := formatLogArgs(args...)
			output.Logs = append(output.Logs, fmt.Sprintf("INFO: %s", log))
			s.logger.Info("Script info", zap.String("message", log))
		},
		"warn": func(args ...interface{}) {
			log := formatLogArgs(args...)
			output.Logs = append(output.Logs, fmt.Sprintf("WARN: %s", log))
			s.logger.Warn("Script warning", zap.String("message", log))
		},
		"error": func(args ...interface{}) {
			log := formatLogArgs(args...)
			output.Logs = append(output.Logs, fmt.Sprintf("ERROR: %s", log))
			s.logger.Error("Script error", zap.String("message", log))
		},
	}
}

// formatLogArgs formats log arguments into a string
func formatLogArgs(args ...interface{}) string {
	if len(args) == 0 {
		return ""
	} else if len(args) == 1 {
		return fmt.Sprintf("%v", args[0])
	}
	return fmt.Sprintf("%v", args)
}

// createContextObject creates the context object exposed to JavaScript
func (s *Sandbox) createContextObject(ctx *FunctionContext) (map[string]interface{}, error) {
	// Create a basic context object with function metadata
	contextObj := map[string]interface{}{
		"functionId":  ctx.FunctionID,
		"executionId": ctx.ExecutionID,
		"owner":       ctx.Owner,
		"caller":      ctx.Caller,
		"parameters":  ctx.Parameters,
		"environment": ctx.Environment,
		"traceId":     ctx.TraceID,
	}

	return contextObj, nil
}

// createSecretsObject creates the secrets object exposed to JavaScript
func (s *Sandbox) createSecretsObject(fnCtx *FunctionContext) (map[string]interface{}, error) {
	s.mutex.RLock() // Lock to safely access teeProvider and currentSecrets
	defer s.mutex.RUnlock()

	return map[string]interface{}{
		"get": func(call goja.FunctionCall) goja.Value {
			if s.teeProvider == nil {
				// Throw error in JS if TEE is not available
				panic(s.vm.NewGoError(errors.New("Secrets decryption unavailable: TEE provider not configured")))
			}

			secretName := call.Argument(0).String()
			if secretName == "" {
				panic(s.vm.NewGoError(errors.New("secrets.get requires a non-empty secret name")))
			}

			encryptedValueBase64, exists := s.currentSecrets[secretName]
			if !exists {
				// Secret wasn't provided or fetch failed upstream
				s.logger.Warn("Attempted to get secret not provided to function", zap.String("secretName", secretName), zap.String("functionId", fnCtx.FunctionID))
				return goja.Null() // Return null if secret not found
			}

			// Decode base64
			encryptedBytes, err := base64.StdEncoding.DecodeString(encryptedValueBase64)
			if err != nil {
				s.logger.Error("Failed to decode base64 secret before TEE decryption", zap.String("secretName", secretName), zap.Error(err))
				panic(s.vm.NewGoError(fmt.Errorf("failed to decode secret '%s'", secretName)))
			}

			// Decrypt using TEE provider
			// Note: The context passed here might need adjustment depending on TEE provider requirements
			decryptedBytes, err := s.teeProvider.Decrypt(context.TODO(), encryptedBytes) // Using context.TODO() for now
			if err != nil {
				s.logger.Error("TEE decryption failed for secret", zap.String("secretName", secretName), zap.Error(err))
				panic(s.vm.NewGoError(fmt.Errorf("failed to decrypt secret '%s' via TEE", secretName)))
			}

			// Return decrypted value as string
			return s.vm.ToValue(string(decryptedBytes))
		},
	}, nil
}
