package runtime

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	runtime_pkg "runtime"
	"sync"
	"time"

	"github.com/dop251/goja"
	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/will/neo_service_layer/internal/services/transaction"
	"go.uber.org/zap"
)

// Resource constraints for the sandbox
const (
	DefaultMemoryLimit   = 128 * 1024 * 1024 // 128 MB
	DefaultTimeoutMillis = 5000              // 5 seconds
	DefaultStackSize     = 8 * 1024 * 1024   // 8 MB
	MemoryCheckInterval  = 100 * time.Millisecond
)

// SandboxConfig holds configuration for the JavaScript sandbox
type SandboxConfig struct {
	MemoryLimit          int64
	TimeoutMillis        int64
	StackSize            int32
	AllowNetwork         bool
	AllowFileIO          bool
	ServiceLayerURL      string
	EnableInteroperability bool
	Logger               *zap.Logger
}

// Sandbox represents a JavaScript execution environment
type Sandbox struct {
	vm           *goja.Runtime
	config       SandboxConfig
	mutex        sync.Mutex
	interrupted  bool
	memoryUsed   int64
	memoryLimit  int64
	logger       *zap.Logger
	stopMemCheck chan struct{}
}

// FunctionInput represents input to a function execution
type FunctionInput struct {
	Code           string                 `json:"code"`
	Args           map[string]interface{} `json:"args"`
	Secrets        map[string]string      `json:"secrets,omitempty"`
	Parameters     map[string]interface{} `json:"parameters,omitempty"`
	FunctionContext *FunctionContext       `json:"functionContext,omitempty"`
}

// FunctionContext represents the execution context for a function
type FunctionContext struct {
	FunctionID     string                 `json:"functionId"`
	ExecutionID    string                 `json:"executionId"`
	Owner          string                 `json:"owner"`
	Caller         string                 `json:"caller,omitempty"`
	Parameters     map[string]interface{} `json:"parameters,omitempty"`
	Env            map[string]string      `json:"env,omitempty"`
	TraceID        string                 `json:"traceId,omitempty"`
	ServiceLayerURL string                `json:"serviceLayerUrl,omitempty"`
	Services       *ServiceClients        `json:"-"`
}

// ServiceClients holds references to Neo Service Layer services
type ServiceClients struct {
	Functions   interface{} // Functions service client
	GasBank     interface{} // Gas Bank service client
	PriceFeed   interface{} // Price Feed service client
	Secrets     interface{} // Secrets service client
	Trigger     interface{} // Trigger service client
	Transaction interface{} // Transaction service client
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
	
	// Set up logger if not provided
	logger := config.Logger
	if logger == nil {
		// Create a default no-op logger to avoid nil pointer dereferences
		noopLogger, _ := zap.NewProduction()
		if noopLogger == nil {
			// Fallback to a simple logger if production logger fails
			noopLogger = zap.NewNop()
		}
		logger = noopLogger
	}

	// Create VM with options
	vm := goja.New()
	
	return &Sandbox{
		vm:           vm,
		config:       config,
		memoryLimit:  config.MemoryLimit,
		logger:       logger,
		stopMemCheck: make(chan struct{}),
	}
}

// startMemoryMonitoring starts a goroutine to monitor memory usage
func (s *Sandbox) startMemoryMonitoring() {
	s.mutex.Lock()
	if s.stopMemCheck != nil {
		s.mutex.Unlock()
		return
	}
	s.stopMemCheck = make(chan struct{})
	s.mutex.Unlock()
	
	go func() {
		ticker := time.NewTicker(MemoryCheckInterval)
		defer ticker.Stop()
		
		var memStats runtime_pkg.MemStats
		
		for {
			select {
			case <-ticker.C:
				runtime_pkg.ReadMemStats(&memStats)
				
				func() {
					s.mutex.Lock()
					defer s.mutex.Unlock()
					s.memoryUsed = int64(memStats.Alloc)
					
					// Check if memory limit exceeded
					if s.memoryUsed > s.memoryLimit {
						s.logger.Warn("Memory limit exceeded",
							zap.Int64("memoryUsed", s.memoryUsed),
							zap.Int64("memoryLimit", s.memoryLimit))
						s.interrupted = true
					}
				}()
				
				// Check if we need to exit after releasing the lock
				if func() bool {
					s.mutex.Lock()
					defer s.mutex.Unlock()
					return s.interrupted
				}() {
					return
				}
			case <-s.stopMemCheck:
				return
			}
		}
	}()
}

// stopMemoryMonitoring stops the memory monitoring goroutine
func (s *Sandbox) stopMemoryMonitoring() {
	s.mutex.Lock()
	defer s.mutex.Unlock()
	
	if s.stopMemCheck != nil {
		close(s.stopMemCheck)
		s.stopMemCheck = nil
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
	
	// Start memory monitoring
	s.startMemoryMonitoring()
	defer s.stopMemoryMonitoring()
	
	// Ensure we have a valid function context
	if input.FunctionContext == nil {
		input.FunctionContext = &FunctionContext{
			FunctionID:  "unknown",
			ExecutionID: fmt.Sprintf("exec-%s", uuid.New().String()),
		}
	}
	
	// Initialize args if nil
	if input.Args == nil {
		input.Args = make(map[string]interface{})
	}
	
	// Log execution start
	s.logger.Info("Starting function execution",
		zap.String("functionId", input.FunctionContext.FunctionID),
		zap.String("executionId", input.FunctionContext.ExecutionID))

	// Set up console object for logging
	logs := []string{}
	console := map[string]interface{}{
		"log": func(args ...interface{}) {
			message := fmt.Sprint(args...)
			logs = append(logs, message)
			s.logger.Info(message,
				zap.String("functionId", input.FunctionContext.FunctionID),
				zap.String("executionId", input.FunctionContext.ExecutionID))
		},
		"error": func(args ...interface{}) {
			message := fmt.Sprint(args...)
			logs = append(logs, "ERROR: "+message)
			s.logger.Error(message,
				zap.String("functionId", input.FunctionContext.FunctionID),
				zap.String("executionId", input.FunctionContext.ExecutionID))
		},
		"info": func(args ...interface{}) {
			message := fmt.Sprint(args...)
			logs = append(logs, "INFO: "+message)
			s.logger.Info(message,
				zap.String("functionId", input.FunctionContext.FunctionID),
				zap.String("executionId", input.FunctionContext.ExecutionID))
		},
		"warn": func(args ...interface{}) {
			message := fmt.Sprint(args...)
			logs = append(logs, "WARN: "+message)
			s.logger.Warn(message,
				zap.String("functionId", input.FunctionContext.FunctionID),
				zap.String("executionId", input.FunctionContext.ExecutionID))
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

	// Set up function context for interoperability if enabled
	if s.config.EnableInteroperability && input.FunctionContext != nil {
		// Create context object with service access methods
		contextObj := s.createFunctionContext(input.FunctionContext, logs)
		
		err = s.vm.Set("context", contextObj)
		if err != nil {
			return nil, fmt.Errorf("failed to set context object: %w", err)
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
				try {
					%s
					if (typeof main !== 'function') {
						throw new Error('main function is not defined');
					}
					return main(args);
				} catch (e) {
					// Capture stack trace
					if (e instanceof Error) {
						throw {
							message: e.message,
							stack: e.stack,
							name: e.name
						};
					}
					throw e;
				}
			})()
		`, input.Code)

		// Execute the code
		var value goja.Value
		var err error
		
		// Use a panic recovery to prevent VM panics from crashing the whole process
		func() {
			defer func() {
				if r := recover(); r != nil {
					err = fmt.Errorf("VM panic: %v", r)
				}
			}()
			value, err = s.vm.RunString(wrappedCode)
		}()
		
		duration := time.Since(startTime).Milliseconds()

		if err != nil {
			// Handle execution errors
			var jsErr *goja.Exception
			if errors.As(err, &jsErr) {
				// Extract detailed error information
				errObj := jsErr.Value().Export()
				if errMap, ok := errObj.(map[string]interface{}); ok {
					errMsg := fmt.Sprintf("JavaScript error: %v", errMap["message"])
					if stack, ok := errMap["stack"].(string); ok {
						errMsg += "\nStack: " + stack
					}
					errChan <- errors.New(errMsg)
				} else {
					errChan <- fmt.Errorf("javascript error: %s", jsErr.Value())
				}
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
		s.logger.Warn("Function execution timed out",
			zap.String("functionId", input.FunctionContext.FunctionID),
			zap.String("executionId", input.FunctionContext.ExecutionID),
			zap.Int64("timeoutMillis", s.config.TimeoutMillis))
		return &FunctionOutput{
			Error:      "function execution timed out",
			Logs:       logs,
			Duration:   int64(s.config.TimeoutMillis),
			MemoryUsed: s.memoryUsed,
		}, nil
	case <-timer.C:
		s.interrupted = true
		s.logger.Warn("Function execution timed out",
			zap.String("functionId", input.FunctionContext.FunctionID),
			zap.String("executionId", input.FunctionContext.ExecutionID),
			zap.Int64("timeoutMillis", s.config.TimeoutMillis))
		return &FunctionOutput{
			Error:      "function execution timed out",
			Logs:       logs,
			Duration:   int64(s.config.TimeoutMillis),
			MemoryUsed: s.memoryUsed,
		}, nil
	case err := <-errChan:
		s.logger.Error("Function execution failed",
			zap.String("functionId", input.FunctionContext.FunctionID),
			zap.String("executionId", input.FunctionContext.ExecutionID),
			zap.Error(err))
		return &FunctionOutput{
			Error:      err.Error(),
			Logs:       logs,
			Duration:   time.Since(time.Now().Add(-time.Duration(s.config.TimeoutMillis) * time.Millisecond)).Milliseconds(),
			MemoryUsed: s.memoryUsed,
		}, nil
	case result := <-resultChan:
		s.logger.Info("Function execution completed successfully",
			zap.String("functionId", input.FunctionContext.FunctionID),
			zap.String("executionId", input.FunctionContext.ExecutionID),
			zap.Int64("durationMs", result.Duration),
			zap.Int64("memoryUsed", result.MemoryUsed))
		return result, nil
	}
}

// createFunctionContext creates a JavaScript object with methods for interacting with Neo services
func (s *Sandbox) createFunctionContext(ctx *FunctionContext, logs []string) map[string]interface{} {
	// Base context properties
	contextObj := map[string]interface{}{
		"functionId":  ctx.FunctionID,
		"executionId": ctx.ExecutionID,
		"owner":       ctx.Owner,
		"caller":      ctx.Caller,
		"parameters":  ctx.Parameters,
		"env":         ctx.Env,
		"traceId":     ctx.TraceID,
	}

	// Add logging methods
	contextObj["log"] = func(message string) {
		logs = append(logs, message)
		s.logger.Info(message,
			zap.String("functionId", ctx.FunctionID),
			zap.String("executionId", ctx.ExecutionID))
	}
	
	contextObj["error"] = func(message string) {
		logs = append(logs, "ERROR: "+message)
		s.logger.Error(message,
			zap.String("functionId", ctx.FunctionID),
			zap.String("executionId", ctx.ExecutionID))
	}
	
	contextObj["warn"] = func(message string) {
		logs = append(logs, "WARN: "+message)
		s.logger.Warn(message,
			zap.String("functionId", ctx.FunctionID),
			zap.String("executionId", ctx.ExecutionID))
	}

	// Add service interoperability if services are available
	if ctx.Services != nil {
		// ====== Secrets Management ======
		// Create a new secrets object
		secretsObj := map[string]interface{}{}
		
		// Get secret method
		secretsObj["get"] = func(secretName string) interface{} {
			if ctx.Services.Secrets != nil {
				// In a real implementation, we would call the Secrets service
				// For now, we'll return a placeholder
				s.logger.Info("Accessing secret",
					zap.String("functionId", ctx.FunctionID),
					zap.String("secretName", secretName))
				
				// This would be replaced with actual secret retrieval logic
				return map[string]interface{}{
					"success": true,
					"value":   "********", // Never log actual secret values
				}
			}
			s.logger.Error("Secrets service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Secrets service not available",
			}
		}
		
		// Set secret method
		secretsObj["set"] = func(secretName string, secretValue string) interface{} {
			if ctx.Services.Secrets != nil {
				// In a real implementation, we would call the Secrets service
				// For now, we'll return a placeholder
				s.logger.Info("Setting secret",
					zap.String("functionId", ctx.FunctionID),
					zap.String("secretName", secretName))
				
				return map[string]interface{}{
					"success": true,
				}
			}
			s.logger.Error("Secrets service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Secrets service not available",
			}
		}
		
		// Delete secret method
		secretsObj["delete"] = func(secretName string) interface{} {
			if ctx.Services.Secrets != nil {
				// In a real implementation, we would call the Secrets service
				// For now, we'll return a placeholder
				s.logger.Info("Deleting secret",
					zap.String("functionId", ctx.FunctionID),
					zap.String("secretName", secretName))
				
				return map[string]interface{}{
					"success": true,
				}
			}
			s.logger.Error("Secrets service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Secrets service not available",
			}
		}
		
		// Add secrets object to context
		contextObj["secrets"] = secretsObj

		// ====== Function Invocation ======
		// Create a new functions object
		functionsObj := map[string]interface{}{}
		
		// Invoke function method
		functionsObj["invoke"] = func(functionId string, args map[string]interface{}) interface{} {
			if ctx.Services.Functions != nil {
				// In a real implementation, we would call the Functions service
				// For now, we'll return a placeholder
				s.logger.Info("Invoking function",
					zap.String("callerFunctionId", ctx.FunctionID),
					zap.String("targetFunctionId", functionId))
				
				return map[string]interface{}{
					"success": true,
					"result":  fmt.Sprintf("Result from function %s", functionId),
				}
			}
			s.logger.Error("Functions service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Functions service not available",
			}
		}
		
		// Add functions object to context
		contextObj["functions"] = functionsObj

		// ====== Gas Bank ======
		// Create a new gas bank object
		gasBankObj := map[string]interface{}{}
		
		// Get gas balance method
		gasBankObj["getBalance"] = func() interface{} {
			if ctx.Services.GasBank != nil {
				// In a real implementation, we would call the Gas Bank service
				// For now, we'll return a placeholder
				s.logger.Info("Getting gas balance",
					zap.String("functionId", ctx.FunctionID),
					zap.String("owner", ctx.Owner))
				
				return map[string]interface{}{
					"success": true,
					"balance": 1000.0,
					"unit":    "GAS",
				}
			}
			s.logger.Error("Gas Bank service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Gas Bank service not available",
			}
		}
		
		// Add gas bank object to context
		contextObj["gasBank"] = gasBankObj

		// ====== Price Feed ======
		// Create a new price feed object
		priceFeedObj := map[string]interface{}{}
		
		// Get price method
		priceFeedObj["getPrice"] = func(symbol string) interface{} {
			if ctx.Services.PriceFeed != nil {
				// In a real implementation, we would call the Price Feed service
				// For now, we'll return a placeholder
				s.logger.Info("Getting price",
					zap.String("functionId", ctx.FunctionID),
					zap.String("symbol", symbol))
				
				// This would be replaced with actual price feed logic
				mockPrices := map[string]float64{
					"NEO":  50.0,
					"GAS":  15.0,
					"BTC":  30000.0,
					"ETH":  2000.0,
					"USD":  1.0,
				}
				
				price, ok := mockPrices[symbol]
				if !ok {
					return map[string]interface{}{
						"success": false,
						"error":   fmt.Sprintf("Price not available for symbol: %s", symbol),
					}
				}
				
				return map[string]interface{}{
					"success": true,
					"symbol":  symbol,
					"price":   price,
					"currency": "USD",
					"timestamp": time.Now().Unix(),
				}
			}
			s.logger.Error("Price Feed service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Price Feed service not available",
			}
		}
		
		// Add price feed object to context
		contextObj["priceFeed"] = priceFeedObj

		// ====== Trigger Management ======
		// Create a new trigger object
		triggerObj := map[string]interface{}{}
		
		// Create trigger method
		triggerObj["create"] = func(triggerType string, triggerConfig map[string]interface{}) interface{} {
			if ctx.Services.Trigger != nil {
				// In a real implementation, we would call the Trigger service
				// For now, we'll return a placeholder
				s.logger.Info("Creating trigger",
					zap.String("functionId", ctx.FunctionID),
					zap.String("triggerType", triggerType))
				
				return map[string]interface{}{
					"success":   true,
					"triggerId": fmt.Sprintf("trigger-%s", uuid.New().String()),
					"type":      triggerType,
					"config":    triggerConfig,
				}
			}
			s.logger.Error("Trigger service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Trigger service not available",
			}
		}
		
		// Update trigger method
		triggerObj["update"] = func(triggerId string, triggerConfig map[string]interface{}) interface{} {
			if ctx.Services.Trigger != nil {
				// In a real implementation, we would call the Trigger service
				// For now, we'll return a placeholder
				s.logger.Info("Updating trigger",
					zap.String("functionId", ctx.FunctionID),
					zap.String("triggerId", triggerId))
				
				return map[string]interface{}{
					"success":   true,
					"triggerId": triggerId,
					"config":    triggerConfig,
				}
			}
			s.logger.Error("Trigger service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Trigger service not available",
			}
		}
		
		// Delete trigger method
		triggerObj["delete"] = func(triggerId string) interface{} {
			if ctx.Services.Trigger != nil {
				// In a real implementation, we would call the Trigger service
				// For now, we'll return a placeholder
				s.logger.Info("Deleting trigger",
					zap.String("functionId", ctx.FunctionID),
					zap.String("triggerId", triggerId))
				
				return map[string]interface{}{
					"success":   true,
					"triggerId": triggerId,
				}
			}
			s.logger.Error("Trigger service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Trigger service not available",
			}
		}
		
		// List triggers method
		triggerObj["list"] = func() interface{} {
			if ctx.Services.Trigger != nil {
				// In a real implementation, we would call the Trigger service
				// For now, we'll return a placeholder
				s.logger.Info("Listing triggers",
					zap.String("functionId", ctx.FunctionID))
				
				return []map[string]interface{}{
					{
						"triggerId": "trigger-example-1",
						"type":      "blockchain",
						"config":    map[string]interface{}{"event": "Transfer"},
					},
				}
			}
			s.logger.Error("Trigger service not available",
				zap.String("functionId", ctx.FunctionID))
			return []interface{}{}
		}
		
		// Add trigger object to context
		contextObj["trigger"] = triggerObj

		// ====== Event Handling ======
		// Create a new event object
		eventObj := map[string]interface{}{}
		
		// Register blockchain event handler
		eventObj["onBlockchain"] = func(eventConfig map[string]interface{}, handlerFunctionId string) interface{} {
			if ctx.Services.Trigger != nil {
				// In a real implementation, we would call the Trigger service to create a blockchain event trigger
				// For now, we'll return a placeholder
				s.logger.Info("Registering blockchain event handler",
					zap.String("functionId", ctx.FunctionID),
					zap.String("handlerFunctionId", handlerFunctionId))
				
				return map[string]interface{}{
					"success":   true,
					"eventId":   fmt.Sprintf("event-%s", uuid.New().String()),
					"config":    eventConfig,
					"handlerId": handlerFunctionId,
				}
			}
			s.logger.Error("Trigger service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Trigger service not available",
			}
		}
		
		// Register time-based event handler
		eventObj["onSchedule"] = func(cronExpression string, handlerFunctionId string) interface{} {
			if ctx.Services.Trigger != nil {
				// In a real implementation, we would call the Trigger service to create a time-based trigger
				// For now, we'll return a placeholder
				s.logger.Info("Registering schedule event handler",
					zap.String("functionId", ctx.FunctionID),
					zap.String("cronExpression", cronExpression),
					zap.String("handlerFunctionId", handlerFunctionId))
				
				return map[string]interface{}{
					"success":        true,
					"eventId":        fmt.Sprintf("event-%s", uuid.New().String()),
					"cronExpression": cronExpression,
					"handlerId":      handlerFunctionId,
				}
			}
			s.logger.Error("Trigger service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Trigger service not available",
			}
		}
		
		// Register API event handler
		eventObj["onAPI"] = func(endpoint string, handlerFunctionId string) interface{} {
			if ctx.Services.Trigger != nil {
				// In a real implementation, we would call the Trigger service to create an API trigger
				// For now, we'll return a placeholder
				s.logger.Info("Registering API event handler",
					zap.String("functionId", ctx.FunctionID),
					zap.String("endpoint", endpoint),
					zap.String("handlerFunctionId", handlerFunctionId))
				
				return map[string]interface{}{
					"success":   true,
					"eventId":   fmt.Sprintf("event-%s", uuid.New().String()),
					"endpoint":  endpoint,
					"handlerId": handlerFunctionId,
				}
			}
			s.logger.Error("Trigger service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Trigger service not available",
			}
		}
		
		// Add event object to context
		contextObj["event"] = eventObj

		// ====== Transaction Management ======
		// Create a new transaction object
		txObj := map[string]interface{}{}
		
		// Create transaction method
		txObj["create"] = func(txConfig map[string]interface{}) interface{} {
			if ctx.Services.Transaction != nil {
				// Call the Transaction service
				s.logger.Info("Creating transaction",
					zap.String("functionId", ctx.FunctionID),
					zap.String("owner", ctx.Owner))
				
				// Validate required transaction parameters
				if txConfig == nil {
					return map[string]interface{}{
						"success": false,
						"error":   "Transaction configuration is required",
					}
				}
				
				// Add owner to config
				txConfig["owner"] = ctx.Owner
				
				// Create the transaction
				txService, ok := ctx.Services.Transaction.(transaction.Service)
				if !ok {
					s.logger.Error("Invalid transaction service type",
						zap.String("functionId", ctx.FunctionID))
					return map[string]interface{}{
						"success": false,
						"error":   "Invalid transaction service configuration",
					}
				}
				
				txId, err := txService.Create(txConfig)
				if err != nil {
					s.logger.Error("Failed to create transaction",
						zap.String("functionId", ctx.FunctionID),
						zap.Error(err))
					return map[string]interface{}{
						"success": false,
						"error":   err.Error(),
					}
				}
				
				return map[string]interface{}{
					"success": true,
					"txId":    txId,
					"config":  txConfig,
					"status":  "created",
				}
			}
			s.logger.Error("Transaction service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Transaction service not available",
			}
		}
		
		// Sign transaction method
		txObj["sign"] = func(txId string) interface{} {
			if ctx.Services.Transaction != nil {
				// Call the Transaction service
				s.logger.Info("Signing transaction",
					zap.String("functionId", ctx.FunctionID),
					zap.String("txId", txId))
				
				// Validate transaction ID
				if txId == "" {
					return map[string]interface{}{
						"success": false,
						"error":   "Transaction ID is required",
					}
				}
				
				txService, ok := ctx.Services.Transaction.(transaction.Service)
				if !ok {
					s.logger.Error("Invalid transaction service type",
						zap.String("functionId", ctx.FunctionID))
					return map[string]interface{}{
						"success": false,
						"error":   "Invalid transaction service configuration",
					}
				}
				
				// In a real implementation, we would get the account from a wallet service
				// For the sandbox environment, we'll create a mock account for testing
				s.logger.Info("Creating mock account for signing in sandbox environment",
					zap.String("functionId", ctx.FunctionID))
				
				// Create a mock wallet.Account for testing purposes
				mockAccount := &wallet.Account{}
				
				// Sign the transaction with the mock account
				txDetails, err := txService.Sign(txId, mockAccount)
				if err != nil {
					s.logger.Error("Failed to sign transaction",
						zap.String("functionId", ctx.FunctionID),
						zap.Error(err))
					return map[string]interface{}{
						"success": false,
						"error":   err.Error(),
					}
				}
				
				return map[string]interface{}{
					"success": true,
					"txId":    txId,
					"status":  txDetails["status"],
				}
			}
			s.logger.Error("Transaction service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Transaction service not available",
			}
		}
		
		// Send transaction method
		txObj["send"] = func(txId string) interface{} {
			if ctx.Services.Transaction != nil {
				// Call the Transaction service
				s.logger.Info("Sending transaction",
					zap.String("functionId", ctx.FunctionID),
					zap.String("txId", txId))
				
				// Validate transaction ID
				if txId == "" {
					return map[string]interface{}{
						"success": false,
						"error":   "Transaction ID is required",
					}
				}
				
				txService, ok := ctx.Services.Transaction.(transaction.Service)
				if !ok {
					s.logger.Error("Invalid transaction service type",
						zap.String("functionId", ctx.FunctionID))
					return map[string]interface{}{
						"success": false,
						"error":   "Invalid transaction service configuration",
					}
				}
				
				// Send the transaction
				txHash, err := txService.Send(context.Background(), txId)
				if err != nil {
					s.logger.Error("Failed to send transaction",
						zap.String("functionId", ctx.FunctionID),
						zap.Error(err))
					return map[string]interface{}{
						"success": false,
						"error":   err.Error(),
					}
				}
				
				return map[string]interface{}{
					"success": true,
					"txId":    txId,
					"hash":    txHash,
					"status":  "sent",
				}
			}
			s.logger.Error("Transaction service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Transaction service not available",
			}
		}
		
		// Get transaction status method
		txObj["status"] = func(txId string) interface{} {
			if ctx.Services.Transaction != nil {
				// Call the Transaction service
				s.logger.Info("Getting transaction status",
					zap.String("functionId", ctx.FunctionID),
					zap.String("txId", txId))
				
				// Validate transaction ID
				if txId == "" {
					return map[string]interface{}{
						"success": false,
						"error":   "Transaction ID is required",
					}
				}
				
				txService, ok := ctx.Services.Transaction.(transaction.Service)
				if !ok {
					s.logger.Error("Invalid transaction service type",
						zap.String("functionId", ctx.FunctionID))
					return map[string]interface{}{
						"success": false,
						"error":   "Invalid transaction service configuration",
					}
				}
				
				// Get transaction status
				// In a real implementation, this would query the blockchain
				// For now, we'll return a mock result
				status, err := txService.Status(txId)
				if err != nil {
					s.logger.Error("Failed to get transaction status",
						zap.String("functionId", ctx.FunctionID),
						zap.Error(err))
					return map[string]interface{}{
						"success": false,
						"error":   err.Error(),
					}
				}
				
				return map[string]interface{}{
					"success": true,
					"txId":    txId,
					"status":  status,
				}
			}
			s.logger.Error("Transaction service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Transaction service not available",
			}
		}
		
		// Get transaction details method
		txObj["get"] = func(txId string) interface{} {
			if ctx.Services.Transaction != nil {
				// Call the Transaction service
				s.logger.Info("Getting transaction details",
					zap.String("functionId", ctx.FunctionID),
					zap.String("txId", txId))
				
				// Validate transaction ID
				if txId == "" {
					return map[string]interface{}{
						"success": false,
						"error":   "Transaction ID is required",
					}
				}
				
				txService, ok := ctx.Services.Transaction.(transaction.Service)
				if !ok {
					s.logger.Error("Invalid transaction service type",
						zap.String("functionId", ctx.FunctionID))
					return map[string]interface{}{
						"success": false,
						"error":   "Invalid transaction service configuration",
					}
				}
				
				// Get transaction details
				txDetails, err := txService.Get(txId)
				if err != nil {
					s.logger.Error("Failed to get transaction details",
						zap.String("functionId", ctx.FunctionID),
						zap.Error(err))
					return map[string]interface{}{
						"success": false,
						"error":   err.Error(),
					}
				}
				
				return map[string]interface{}{
					"success": true,
					"transaction": txDetails,
				}
			}
			s.logger.Error("Transaction service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Transaction service not available",
			}
		}
		
		// List transactions method
		txObj["list"] = func(filter map[string]interface{}) interface{} {
			if ctx.Services.Transaction != nil {
				// Call the Transaction service
				s.logger.Info("Listing transactions",
					zap.String("functionId", ctx.FunctionID),
					zap.Any("filter", filter))
				
				txService, ok := ctx.Services.Transaction.(transaction.Service)
				if !ok {
					s.logger.Error("Invalid transaction service type",
						zap.String("functionId", ctx.FunctionID))
					return map[string]interface{}{
						"success": false,
						"error":   "Invalid transaction service configuration",
					}
				}
				
				// List transactions
				txList, err := txService.List()
				if err != nil {
					s.logger.Error("Failed to list transactions",
						zap.String("functionId", ctx.FunctionID),
						zap.Error(err))
					return map[string]interface{}{
						"success": false,
						"error":   err.Error(),
					}
				}
				
				return map[string]interface{}{
					"success": true,
					"transactions": txList,
				}
			}
			s.logger.Error("Transaction service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Transaction service not available",
			}
		}
		
		// Estimate transaction fee method
		txObj["estimateFee"] = func(txConfig map[string]interface{}) interface{} {
			if ctx.Services.Transaction != nil {
				// Call the Transaction service
				s.logger.Info("Estimating transaction fee",
					zap.String("functionId", ctx.FunctionID),
					zap.Any("txConfig", txConfig))
				
				// Validate required transaction parameters
				if txConfig == nil {
					return map[string]interface{}{
						"success": false,
						"error":   "Transaction configuration is required",
					}
				}
				
				txService, ok := ctx.Services.Transaction.(transaction.Service)
				if !ok {
					s.logger.Error("Invalid transaction service type",
						zap.String("functionId", ctx.FunctionID))
					return map[string]interface{}{
						"success": false,
						"error":   "Invalid transaction service configuration",
					}
				}
				
				// Estimate fee
				fee, err := txService.EstimateFee(txConfig)
				if err != nil {
					s.logger.Error("Failed to estimate transaction fee",
						zap.String("functionId", ctx.FunctionID),
						zap.Error(err))
					return map[string]interface{}{
						"success": false,
						"error":   err.Error(),
					}
				}
				
				return map[string]interface{}{
					"success": true,
					"fee":     fee,
					"asset":   "GAS",
					"network": "testnet",
				}
			}
			s.logger.Error("Transaction service not available",
				zap.String("functionId", ctx.FunctionID))
			return map[string]interface{}{
				"success": false,
				"error":   "Transaction service not available",
			}
		}
		
		// Add transaction object to context
		contextObj["transaction"] = txObj
	}

	return contextObj
}

// ExecuteJSON runs JavaScript code with JSON-serialized input and output
func (s *Sandbox) ExecuteJSON(ctx context.Context, jsonInput string) (string, error) {
	var input FunctionInput
	err := json.Unmarshal([]byte(jsonInput), &input)
	if err != nil {
		s.logger.Error("Failed to parse input JSON",
			zap.Error(err),
			zap.String("jsonInput", jsonInput))
		return "", fmt.Errorf("failed to parse input JSON: %w", err)
	}

	// Ensure we have a valid function context
	if input.FunctionContext == nil {
		input.FunctionContext = &FunctionContext{
			FunctionID:  "unknown",
			ExecutionID: fmt.Sprintf("exec-%s", uuid.New().String()),
		}
	}

	s.logger.Info("Executing function from JSON input",
		zap.String("functionId", input.FunctionContext.FunctionID),
		zap.String("executionId", input.FunctionContext.ExecutionID))

	output, err := s.Execute(ctx, input)
	if err != nil {
		s.logger.Error("Function execution failed",
			zap.String("functionId", input.FunctionContext.FunctionID),
			zap.String("executionId", input.FunctionContext.ExecutionID),
			zap.Error(err))
		return "", err
	}

	jsonOutput, err := json.Marshal(output)
	if err != nil {
		s.logger.Error("Failed to serialize output to JSON",
			zap.String("functionId", input.FunctionContext.FunctionID),
			zap.String("executionId", input.FunctionContext.ExecutionID),
			zap.Error(err))
		return "", fmt.Errorf("failed to serialize output to JSON: %w", err)
	}

	s.logger.Info("Function execution completed successfully",
		zap.String("functionId", input.FunctionContext.FunctionID),
		zap.String("executionId", input.FunctionContext.ExecutionID),
		zap.Int64("durationMs", output.Duration),
		zap.Int64("memoryUsed", output.MemoryUsed))

	return string(jsonOutput), nil
}