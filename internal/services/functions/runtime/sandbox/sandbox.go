package sandbox

import (
	"context"
	"encoding/base64"
	"errors"
	"fmt"
	"sync"
	"time"

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

// Sandbox represents a JavaScript execution environment
type Sandbox struct {
	// JavaScript VM
	vm *goja.Runtime

	// Sandbox configuration
	config SandboxConfig

	// Memory monitor
	memoryMonitor *MemoryMonitor

	// Mutex for thread safety
	mutex sync.Mutex

	// Flag indicating if execution was interrupted
	interrupted bool

	// Logger for sandbox operations
	logger *zap.Logger

	// TEE provider for secure operations (e.g., decryption)
	teeProvider secrets_iface.TEESecurityProvider

	// Store encrypted secrets passed during execution for the current execution
	currentSecrets map[string]string
}

// New creates a new JavaScript sandbox
func New(config SandboxConfig, teeProvider secrets_iface.TEESecurityProvider) *Sandbox {
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

	// Set up logger if not provided
	logger := config.Logger
	if logger == nil {
		noopLogger, _ := zap.NewProduction()
		if noopLogger == nil {
			noopLogger = zap.NewNop()
		}
		logger = noopLogger
	}

	// Check TEE provider
	if teeProvider == nil {
		logger.Warn("Sandbox created without a TEE provider. Secrets decryption will not be available.")
	}

	// Create a new sandbox
	s := &Sandbox{
		config:         config,
		logger:         logger,
		teeProvider:    teeProvider,
		currentSecrets: make(map[string]string), // Initialize map
	}

	// Create a memory monitor
	s.memoryMonitor = NewMemoryMonitor(uint64(config.MemoryLimit), logger, func() {
		s.mutex.Lock()
		defer s.mutex.Unlock()
		s.interrupted = true
	})

	// Initialize VM
	s.resetVM()

	return s
}

// resetVM creates a new VM instance
func (s *Sandbox) resetVM() {
	// Create VM with options
	s.vm = goja.New()
	// Potential: Set memory limits via goja options if available/needed
}

// Close releases resources used by the sandbox
func (s *Sandbox) Close() {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	if s.memoryMonitor != nil {
		s.memoryMonitor.Stop()
	}
	s.vm = nil // Allow GC
}

// IsInterrupted returns true if the execution was interrupted
func (s *Sandbox) IsInterrupted() bool {
	s.mutex.Lock()
	defer s.mutex.Unlock()
	return s.interrupted
}

// SetInterrupted sets the interrupted flag
func (s *Sandbox) SetInterrupted(interrupted bool) {
	s.mutex.Lock()
	defer s.mutex.Unlock()
	s.interrupted = interrupted
}

// executionResult holds the result of script execution from the goroutine
type executionResult struct {
	value goja.Value
	err   error
}

// Execute runs JavaScript code in the sandbox with the provided input
func (s *Sandbox) Execute(ctx context.Context, input FunctionInput) FunctionOutput {
	startTime := time.Now()

	if ctx == nil {
		ctx = context.Background()
	}

	var cancel context.CancelFunc
	if s.config.TimeoutMillis > 0 {
		ctx, cancel = context.WithTimeout(ctx, time.Duration(s.config.TimeoutMillis)*time.Millisecond)
		defer cancel()
	}

	// Reset interrupted flag for this execution
	s.SetInterrupted(false)

	// Store secrets temporarily for this execution, ensure cleared afterwards
	s.mutex.Lock()
	s.currentSecrets = input.Secrets
	s.mutex.Unlock()
	defer func() {
		s.mutex.Lock()
		s.currentSecrets = nil
		s.mutex.Unlock()
	}()

	output := FunctionOutput{
		Logs: []string{},
	}

	if s.memoryMonitor != nil {
		s.memoryMonitor.Start()
		defer s.memoryMonitor.Stop()
	}

	// Lock mutex for VM setup
	s.mutex.Lock()

	// Reset VM to clean state
	s.resetVM()

	// Set up function context
	fnCtx := input.Context
	if fnCtx == nil {
		fnCtx = NewFunctionContext("anonymous")
	}

	// Initialize the execution environment (console, context, secrets)
	err := s.setupExecutionEnvironment(fnCtx, &output)
	if err != nil {
		s.mutex.Unlock()
		output.Error = fmt.Sprintf("Failed to set up execution environment: %v", err)
		output.Duration = time.Since(startTime)
		if s.memoryMonitor != nil {
			output.MemoryUsed = s.memoryMonitor.GetCurrentUsage()
		}
		return output
	}

	// Execute script in goroutine
	resultCh := make(chan *executionResult, 1)
	go func() {
		defer func() {
			if r := recover(); r != nil {
				// Capture panics (e.g., from secrets.get or script itself)
				resultCh <- &executionResult{
					err: fmt.Errorf("script execution panicked: %v", r),
				}
			}
		}()

		// Evaluate script to define functions
		_, err := s.vm.RunString(input.Code)
		if err != nil {
			resultCh <- &executionResult{err: err}
			return
		}

		// Call main function
		mainFn, ok := goja.AssertFunction(s.vm.Get("main"))
		if !ok {
			resultCh <- &executionResult{err: errors.New("main function not found in script")}
			return
		}

		jsArgs := make([]goja.Value, len(input.Args))
		for i, arg := range input.Args {
			jsArgs[i] = s.vm.ToValue(arg)
		}

		result, err := mainFn(goja.Undefined(), jsArgs...)
		resultCh <- &executionResult{value: result, err: err}
	}()

	// Unlock mutex once goroutine is launched
	s.mutex.Unlock()

	// Wait for result or context cancellation
	var execResult *executionResult
	select {
	case <-ctx.Done():
		if ctx.Err() == context.DeadlineExceeded {
			output.Error = ErrTimeout.Error()
		} else {
			output.Error = fmt.Sprintf("Script execution cancelled: %v", ctx.Err())
		}
		s.SetInterrupted(true)

	case execResult = <-resultCh:
		if execResult.err != nil {
			output.Error = execResult.err.Error()
		} else if s.IsInterrupted() {
			output.Error = ErrMemoryLimit.Error()
		} else {
			if execResult.value != nil {
				output.Result = execResult.value.Export() // Export result
			}
		}
	}

	// Set final output metadata
	output.Duration = time.Since(startTime)
	if s.memoryMonitor != nil {
		output.MemoryUsed = s.memoryMonitor.GetCurrentUsage()
	}

	return output
}

// setupExecutionEnvironment prepares the VM with necessary bindings and context
func (s *Sandbox) setupExecutionEnvironment(ctx *FunctionContext, output *FunctionOutput) error {
	// Console
	console := s.createConsoleObject(output)
	if err := s.vm.Set("console", console); err != nil {
		return fmt.Errorf("failed to set console object: %w", err)
	}

	// Context
	ctxObj, err := s.createContextObject(ctx)
	if err != nil {
		return fmt.Errorf("failed to create context object: %w", err)
	}
	if err = s.vm.Set("context", ctxObj); err != nil {
		return fmt.Errorf("failed to set context object: %w", err)
	}

	// Secrets
	secretsObj, err := s.createSecretsObject(ctx)
	if err != nil {
		return fmt.Errorf("failed to create secrets object: %w", err)
	}
	if err = s.vm.Set("secrets", secretsObj); err != nil {
		return fmt.Errorf("failed to set secrets object: %w", err)
	}

	// TODO: Add Service Bindings (if config.EnableInteroperability)
	// err = s.setupServiceBindings(ctx)

	return nil
}

// createConsoleObject creates a JavaScript console object with logging functions
func (s *Sandbox) createConsoleObject(output *FunctionOutput) map[string]interface{} {
	logFn := func(level string, args ...interface{}) {
		logMsg := s.formatLogArgs(args...)
		output.Logs = append(output.Logs, fmt.Sprintf("%s: %s", level, logMsg))
		s.logger.Debug("Script log", zap.String("level", level), zap.String("message", logMsg))
	}
	return map[string]interface{}{
		"log":   func(args ...interface{}) { logFn("LOG", args...) },
		"info":  func(args ...interface{}) { logFn("INFO", args...) },
		"warn":  func(args ...interface{}) { logFn("WARN", args...) },
		"error": func(args ...interface{}) { logFn("ERROR", args...) },
	}
}

// formatLogArgs formats log arguments into a string
func (s *Sandbox) formatLogArgs(args ...interface{}) string {
	if len(args) == 0 {
		return ""
	}
	// Basic formatting, consider more sophisticated options if needed
	strArgs := make([]string, len(args))
	for i, v := range args {
		strArgs[i] = fmt.Sprintf("%+v", v)
	}
	return fmt.Sprint(strArgs)
}

// createContextObject creates the context object exposed to JavaScript
func (s *Sandbox) createContextObject(ctx *FunctionContext) (map[string]interface{}, error) {
	return map[string]interface{}{
		"functionId":  ctx.FunctionID,
		"executionId": ctx.ExecutionID,
		"owner":       ctx.Owner,
		"caller":      ctx.Caller,
		"parameters":  ctx.Parameters, // Passed via args as well, maybe redundant?
		"environment": ctx.Environment,
		"traceId":     ctx.TraceID,
	}, nil
}

// createSecretsObject creates the secrets object exposed to JavaScript
func (s *Sandbox) createSecretsObject(fnCtx *FunctionContext) (map[string]interface{}, error) {
	return map[string]interface{}{
		"get": func(call goja.FunctionCall) goja.Value {
			s.mutex.RLock() // Lock required to safely access teeProvider and currentSecrets
			provider := s.teeProvider
			secrets := s.currentSecrets
			vmInstance := s.vm // Capture current vm instance for panic/return
			loggerInstance := s.logger
			s.mutex.RUnlock()

			if provider == nil {
				panic(vmInstance.NewGoError(errors.New("Secrets decryption unavailable: TEE provider not configured")))
			}

			secretName := call.Argument(0).String()
			if secretName == "" {
				panic(vmInstance.NewGoError(errors.New("secrets.get requires a non-empty secret name")))
			}

			encryptedValueBase64, exists := secrets[secretName]
			if !exists {
				loggerInstance.Warn("Attempted to get secret not provided to function", zap.String("secretName", secretName), zap.String("functionId", fnCtx.FunctionID))
				return goja.Null()
			}

			encryptedBytes, err := base64.StdEncoding.DecodeString(encryptedValueBase64)
			if err != nil {
				loggerInstance.Error("Failed to decode base64 secret before TEE decryption", zap.String("secretName", secretName), zap.Error(err))
				panic(vmInstance.NewGoError(fmt.Errorf("failed to decode secret '%s'", secretName)))
			}

			// Use the context from the Execute call if possible, otherwise fallback
			// TODO: Plumb the parent context down if needed by TEE provider
			decryptionCtx := context.TODO()
			decryptedBytes, err := provider.Decrypt(decryptionCtx, encryptedBytes)
			if err != nil {
				loggerInstance.Error("TEE decryption failed for secret", zap.String("secretName", secretName), zap.Error(err))
				panic(vmInstance.NewGoError(fmt.Errorf("failed to decrypt secret '%s' via TEE", secretName)))
			}

			return vmInstance.ToValue(string(decryptedBytes))
		},
	}, nil
}
