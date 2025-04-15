package functions

import (
	"context"
	"errors"
	"sync"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/functionservice/runtime"
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
)

// ExecutionResult contains the result of a function execution
type ExecutionResult struct {
	Status     string      `json:"status"`     // Execution status (success, error, timeout)
	Result     interface{} `json:"result"`     // Function return value
	Logs       []string    `json:"logs"`       // Function logs
	Error      string      `json:"error"`      // Error message, if any
	Duration   int64       `json:"duration"`   // Execution duration in milliseconds
	MemoryUsed int64       `json:"memoryUsed"` // Memory used in bytes
	GasUsed    int64       `json:"gasUsed"`    // Gas used for execution
}

// ExecutionOptions contains options for function execution
type ExecutionOptions struct {
	Timeout  time.Duration          // Maximum execution time
	Memory   int64                  // Maximum memory allocation
	Secrets  map[string]string      // Secrets available to the function
	GasLimit int64                  // Maximum gas to use
	Caller   util.Uint160           // Address of the caller
	TraceID  string                 // Trace ID for request tracing
	BatchID  string                 // Batch ID if part of a batch execution
	Metadata map[string]interface{} // Additional metadata
	GasUsage *models.Allocation     // Gas allocation record if using GasBank
	Network  bool                   // Allow network access
	FileIO   bool                   // Allow file system access
}

// FunctionExecutor executes functions in a secure environment
type FunctionExecutor struct {
	sandbox       *runtime.Sandbox
	active        map[string]*FunctionExecution // Currently active executions
	execResults   map[string]*ExecutionResult   // Results of finished executions
	mu            sync.RWMutex
	gasCalculator *GasCalculator
}

// GasCalculator calculates gas usage for function execution
type GasCalculator struct {
	// Base cost for function invocation
	BaseCost int64

	// Cost per millisecond of execution time
	TimeCostPerMs int64

	// Cost per byte of memory used
	MemoryCostPerByte int64

	// Cost per external API call
	ApiCallCost int64

	// Cost per byte of storage used
	StorageCostPerByte int64
}

// NewFunctionExecutor creates a new function executor
func NewFunctionExecutor(sandboxConfig runtime.SandboxConfig) *FunctionExecutor {
	// Create default gas calculator
	gasCalculator := &GasCalculator{
		BaseCost:           1000,
		TimeCostPerMs:      1,
		MemoryCostPerByte:  1,
		ApiCallCost:        500,
		StorageCostPerByte: 10,
	}

	return &FunctionExecutor{
		sandbox:       runtime.NewSandbox(sandboxConfig),
		active:        make(map[string]*FunctionExecution),
		execResults:   make(map[string]*ExecutionResult),
		gasCalculator: gasCalculator,
	}
}

// Execute executes a function with the given input and options
func (e *FunctionExecutor) Execute(ctx context.Context, function *Function, input map[string]interface{}, options *ExecutionOptions) (*ExecutionResult, error) {
	// Apply default options if not provided
	if options == nil {
		options = &ExecutionOptions{
			Timeout:  5 * time.Second,
			Memory:   128 * 1024 * 1024, // 128MB
			GasLimit: 1000000,           // Default gas limit
		}
	}

	// Create a timeout context if needed
	execCtx := ctx
	if options.Timeout > 0 {
		var cancel context.CancelFunc
		execCtx, cancel = context.WithTimeout(ctx, options.Timeout)
		defer cancel()
	}

	// Prepare input for the sandbox
	functionInput := runtime.FunctionInput{
		Code:    function.Code,
		Args:    input,
		Secrets: options.Secrets,
		Parameters: map[string]interface{}{
			"function_id": function.ID,
			"caller":      options.Caller.StringLE(),
			"trace_id":    options.TraceID,
			"timestamp":   time.Now().Unix(),
		},
	}

	// Create a tracking record for the execution
	execution := &FunctionExecution{
		ID:         function.ID + "_" + options.TraceID,
		FunctionID: function.ID,
		Status:     "running",
		StartTime:  time.Now(),
		Parameters: input,
		InvokedBy:  options.Caller,
		TraceID:    options.TraceID,
		BatchID:    options.BatchID,
	}

	// Track this execution
	e.mu.Lock()
	e.active[execution.ID] = execution
	e.mu.Unlock()

	// Execute the function in the sandbox
	output, err := e.sandbox.Execute(execCtx, functionInput)
	if err != nil {
		errResult := &ExecutionResult{
			Status:   "error",
			Error:    err.Error(),
			Duration: time.Since(execution.StartTime).Milliseconds(),
			GasUsed:  e.calculateGas(0, 0, options),
		}

		// Update execution record
		execution.Status = "error"
		execution.Error = err.Error()
		execution.EndTime = time.Now()
		execution.Duration = errResult.Duration

		// Save the result
		e.mu.Lock()
		delete(e.active, execution.ID)
		e.execResults[execution.ID] = errResult
		e.mu.Unlock()

		return errResult, err
	}

	// Calculate gas used
	gasUsed := e.calculateGas(output.Duration, output.MemoryUsed, options)

	// Check if gas limit exceeded
	if gasUsed > options.GasLimit {
		gasExceededErr := errors.New("gas limit exceeded")
		gasResult := &ExecutionResult{
			Status:     "error",
			Error:      gasExceededErr.Error(),
			Logs:       output.Logs,
			Duration:   output.Duration,
			MemoryUsed: output.MemoryUsed,
			GasUsed:    gasUsed,
		}

		// Update execution record
		execution.Status = "error"
		execution.Error = gasExceededErr.Error()
		execution.EndTime = time.Now()
		execution.Duration = output.Duration
		execution.MemoryUsed = output.MemoryUsed
		execution.CostInGas = gasUsed

		// Save the result
		e.mu.Lock()
		delete(e.active, execution.ID)
		e.execResults[execution.ID] = gasResult
		e.mu.Unlock()

		return gasResult, gasExceededErr
	}

	// Create successful result
	result := &ExecutionResult{
		Status:     "success",
		Result:     output.Result,
		Logs:       output.Logs,
		Duration:   output.Duration,
		MemoryUsed: output.MemoryUsed,
		GasUsed:    gasUsed,
	}

	// Check for error in output
	if output.Error != "" {
		result.Status = "error"
		result.Error = output.Error
	}

	// Update execution record
	execution.Status = result.Status
	execution.EndTime = time.Now()
	execution.Duration = output.Duration
	execution.MemoryUsed = output.MemoryUsed
	execution.CostInGas = gasUsed

	if result.Status == "success" {
		execution.Result = output.Result
	} else {
		execution.Error = output.Error
	}

	// Record logs
	execution.Logs = output.Logs

	// Save the result and clean up active execution
	e.mu.Lock()
	delete(e.active, execution.ID)
	e.execResults[execution.ID] = result
	e.mu.Unlock()

	return result, nil
}

// GetActiveExecutions returns the list of currently active executions
func (e *FunctionExecutor) GetActiveExecutions() []*FunctionExecution {
	e.mu.RLock()
	defer e.mu.RUnlock()

	activeList := make([]*FunctionExecution, 0, len(e.active))
	for _, execution := range e.active {
		activeList = append(activeList, execution)
	}

	return activeList
}

// GetExecution retrieves an execution by ID
func (e *FunctionExecutor) GetExecution(executionID string) (*FunctionExecution, bool) {
	e.mu.RLock()
	defer e.mu.RUnlock()

	// First check active executions
	if execution, found := e.active[executionID]; found {
		return execution, true
	}

	// Then check if we have a result for it
	result, found := e.execResults[executionID]
	if !found {
		return nil, false
	}

	// Reconstruct a basic execution record from the result
	parts := []string{}
	for i := 0; i < len(executionID); i++ {
		if executionID[i] == '_' {
			parts = []string{executionID[:i], executionID[i+1:]}
			break
		}
	}

	functionID := ""
	traceID := ""
	if len(parts) == 2 {
		functionID = parts[0]
		traceID = parts[1]
	} else {
		functionID = executionID
	}

	execution := &FunctionExecution{
		ID:         executionID,
		FunctionID: functionID,
		TraceID:    traceID,
		Status:     result.Status,
		Duration:   result.Duration,
		MemoryUsed: result.MemoryUsed,
		Result:     result.Result,
		Error:      result.Error,
		Logs:       result.Logs,
		CostInGas:  result.GasUsed,
	}

	return execution, true
}

// Clear clears old execution results to free memory
func (e *FunctionExecutor) Clear(maxAge time.Duration) {
	e.mu.Lock()
	defer e.mu.Unlock()

	// Only keep recent results
	threshold := time.Now().Add(-maxAge)
	for id, execution := range e.active {
		if execution.StartTime.Before(threshold) {
			delete(e.active, id)
		}
	}
}

// calculateGas calculates gas used for function execution
func (e *FunctionExecutor) calculateGas(durationMs int64, memoryUsed int64, options *ExecutionOptions) int64 {
	baseCost := e.gasCalculator.BaseCost
	timeCost := e.gasCalculator.TimeCostPerMs * durationMs
	memoryCost := e.gasCalculator.MemoryCostPerByte * (memoryUsed / 1024) // Convert to KB

	// Add extra costs for network and file I/O access
	extraCost := int64(0)
	if options.Network {
		extraCost += e.gasCalculator.ApiCallCost
	}
	if options.FileIO {
		extraCost += e.gasCalculator.ApiCallCost
	}

	return baseCost + timeCost + memoryCost + extraCost
}
