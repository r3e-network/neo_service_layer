package functions

import (
	"context"
	"fmt"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/functions/runtime"
)

// FunctionStatus values
const (
	FunctionStatusDeploying FunctionStatus = "deploying"
	FunctionStatusUpdating  FunctionStatus = "updating"
	FunctionStatusRollback  FunctionStatus = "rollback"
)

// DeploymentOptions contains options for function deployment
type DeploymentOptions struct {
	AutoActivate   bool                   // Whether to activate the function immediately
	ValidateCode   bool                   // Whether to validate the function code
	Permissions    *FunctionPermissions   // Function permissions
	Triggers       []string               // Associated triggers
	Metadata       map[string]interface{} // Additional metadata
	CompileOptions *CompilationOptions    // Compilation options
	TestTimeout    time.Duration          // Timeout for test execution
}

// DeploymentResult contains the result of a function deployment
type DeploymentResult struct {
	Function            *Function          // The deployed function
	ValidationResult    *ValidationResult  // Result of validation (if performed)
	CompilationResult   *CompilationResult // Result of compilation (if performed)
	TestExecutionResult *ExecutionResult   // Result of test execution (if performed)
	Status              string             // Deployment status
	Message             string             // Status message
	DeployedAt          time.Time          // When the function was deployed
	DeployedBy          util.Uint160       // Who deployed the function
}

// FunctionManager manages the lifecycle of functions
type FunctionManager struct {
	service     *Service
	validator   *FunctionValidator
	compiler    *FunctionCompiler
	executor    *FunctionExecutor
	deployments map[string]*DeploymentResult
	mu          sync.RWMutex
}

// NewFunctionManager creates a new function manager
func NewFunctionManager(service *Service, config *Config) *FunctionManager {
	// Set up validator
	validator := NewFunctionValidator()

	// Set up compiler
	compiler := NewFunctionCompiler()

	// Set up executor
	sandboxConfig := runtime.SandboxConfig{
		MemoryLimit:   config.MaxMemoryLimit,
		TimeoutMillis: config.MaxExecutionTime.Milliseconds(),
		AllowNetwork:  config.EnableNetworkAccess,
		AllowFileIO:   config.EnableFileIO,
	}
	executor := NewFunctionExecutor(sandboxConfig)

	return &FunctionManager{
		service:     service,
		validator:   validator,
		compiler:    compiler,
		executor:    executor,
		deployments: make(map[string]*DeploymentResult),
	}
}

// DeployFunction deploys a new function
func (m *FunctionManager) DeployFunction(ctx context.Context, owner util.Uint160, name, description, code string, runtime Runtime, options *DeploymentOptions) (*DeploymentResult, error) {
	// Apply default options if not provided
	if options == nil {
		options = &DeploymentOptions{
			AutoActivate: true,
			ValidateCode: true,
		}
	}

	// Create deployment result
	deploymentID := uuid.New().String()
	result := &DeploymentResult{
		Status:     "deploying",
		DeployedAt: time.Now(),
		DeployedBy: owner,
	}

	// Start tracking deployment
	m.mu.Lock()
	m.deployments[deploymentID] = result
	m.mu.Unlock()

	// Create the function (status will be set based on options)
	var status FunctionStatus
	if options.AutoActivate {
		status = FunctionStatusDeploying
	} else {
		status = FunctionStatusDisabled
	}

	// Create the function
	function, err := m.service.CreateFunction(ctx, owner, name, description, code, runtime)
	if err != nil {
		result.Status = "failed"
		result.Message = fmt.Sprintf("Failed to create function: %s", err.Error())
		return result, err
	}

	// Set initial status
	function.Status = status
	result.Function = function

	// Validate the function if requested
	if options.ValidateCode {
		validationResult := m.validator.Validate(code)
		result.ValidationResult = validationResult

		if !validationResult.Valid {
			// Set error message but continue with deployment
			result.Message = fmt.Sprintf("Validation failed with %d errors", validationResult.ErrorCount)

			// If validation failed, don't auto-activate
			if options.AutoActivate {
				function.Status = FunctionStatusError
			}
		}
	}

	// Compile the function
	compilationResult, err := m.compiler.CompileFunction(function, options.CompileOptions)
	if err != nil {
		result.Status = "failed"
		result.Message = fmt.Sprintf("Compilation failed: %s", err.Error())
		return result, err
	}

	result.CompilationResult = compilationResult

	// Run a test execution if timeout is specified
	if options.TestTimeout > 0 {
		testOptions := &ExecutionOptions{
			Timeout:  options.TestTimeout,
			Memory:   m.service.config.MaxMemoryLimit,
			GasLimit: 1000000, // Default test gas limit
			Caller:   owner,
			TraceID:  "test-" + deploymentID,
			Network:  m.service.config.EnableNetworkAccess,
			FileIO:   m.service.config.EnableFileIO,
		}

		// Prepare a function with compiled code for testing
		testFunction := *function
		testFunction.Code = compilationResult.CompiledCode

		// Execute the function
		executionResult, err := m.executor.Execute(ctx, &testFunction, map[string]interface{}{
			"_test": true,
		}, testOptions)

		if err != nil {
			result.Message = fmt.Sprintf("Test execution failed: %s", err.Error())
			if options.AutoActivate {
				function.Status = FunctionStatusError
			}
		} else {
			result.TestExecutionResult = executionResult

			// Check execution status
			if executionResult.Status != "success" {
				result.Message = fmt.Sprintf("Test execution failed: %s", executionResult.Error)
				if options.AutoActivate {
					function.Status = FunctionStatusError
				}
			}
		}
	}

	// Update function status if auto-activate is enabled
	if options.AutoActivate && function.Status == FunctionStatusDeploying {
		function.Status = FunctionStatusActive
	}

	// Set permissions if provided
	if options.Permissions != nil {
		if err := m.service.UpdatePermissions(ctx, function.ID, owner, options.Permissions); err != nil {
			result.Message = fmt.Sprintf("%s; Failed to set permissions: %s", result.Message, err.Error())
		}
	}

	// Associate triggers if provided
	if len(options.Triggers) > 0 {
		function.Triggers = options.Triggers
	}

	// Add metadata if provided
	if options.Metadata != nil {
		function.Metadata = options.Metadata
	}

	// Update the function
	if _, err := m.service.UpdateFunction(ctx, function.ID, owner, map[string]interface{}{
		"status":   string(function.Status),
		"triggers": function.Triggers,
		"metadata": function.Metadata,
	}); err != nil {
		result.Message = fmt.Sprintf("%s; Failed to update function: %s", result.Message, err.Error())
	}

	// Update deployment result
	result.Status = "completed"
	if result.Message == "" {
		result.Message = "Function deployed successfully"
	}

	return result, nil
}

// UpdateFunction updates an existing function
func (m *FunctionManager) UpdateFunction(ctx context.Context, functionID string, updater util.Uint160, updates map[string]interface{}, options *DeploymentOptions) (*DeploymentResult, error) {
	// Apply default options if not provided
	if options == nil {
		options = &DeploymentOptions{
			AutoActivate: true,
			ValidateCode: true,
		}
	}

	// Create deployment result
	deploymentID := uuid.New().String()
	result := &DeploymentResult{
		Status:     "updating",
		DeployedAt: time.Now(),
		DeployedBy: updater,
	}

	// Get the function
	function, err := m.service.GetFunction(ctx, functionID)
	if err != nil {
		result.Status = "failed"
		result.Message = fmt.Sprintf("Failed to get function: %s", err.Error())
		return result, err
	}

	result.Function = function

	// Validate code if present and validation is requested
	if code, ok := updates["code"].(string); ok && options.ValidateCode {
		validationResult := m.validator.Validate(code)
		result.ValidationResult = validationResult

		if !validationResult.Valid {
			// Set error message but continue with update
			result.Message = fmt.Sprintf("Validation failed with %d errors", validationResult.ErrorCount)

			// If validation failed, don't auto-activate
			if options.AutoActivate {
				updates["status"] = string(FunctionStatusError)
			}
		}
	}

	// Create a backup of the function for rollback if needed
	originalFunction := *function

	// Update the function status to indicate it's being updated
	if options.AutoActivate {
		updates["status"] = string(FunctionStatusUpdating)
	}

	// Update the function
	updatedFunction, err := m.service.UpdateFunction(ctx, functionID, updater, updates)
	if err != nil {
		result.Status = "failed"
		result.Message = fmt.Sprintf("Failed to update function: %s", err.Error())
		return result, err
	}

	result.Function = updatedFunction

	// Compile the function if code was updated
	if _, ok := updates["code"].(string); ok {
		compilationResult, err := m.compiler.CompileFunction(updatedFunction, options.CompileOptions)
		if err != nil {
			// Handle compilation failure
			result.Status = "failed"
			result.Message = fmt.Sprintf("Compilation failed: %s", err.Error())

			// Attempt to rollback
			_, rollbackErr := m.service.UpdateFunction(ctx, functionID, updater, map[string]interface{}{
				"code":   originalFunction.Code,
				"status": string(originalFunction.Status),
			})

			if rollbackErr != nil {
				result.Message = fmt.Sprintf("%s; Rollback failed: %s", result.Message, rollbackErr.Error())
			} else {
				result.Message = fmt.Sprintf("%s; Rolled back to previous version", result.Message)
			}

			return result, err
		}

		result.CompilationResult = compilationResult

		// Run a test execution if timeout is specified
		if options.TestTimeout > 0 {
			testOptions := &ExecutionOptions{
				Timeout:  options.TestTimeout,
				Memory:   m.service.config.MaxMemoryLimit,
				GasLimit: 1000000, // Default test gas limit
				Caller:   updater,
				TraceID:  "update-test-" + deploymentID,
				Network:  m.service.config.EnableNetworkAccess,
				FileIO:   m.service.config.EnableFileIO,
			}

			// Prepare a function with compiled code for testing
			testFunction := *updatedFunction
			testFunction.Code = compilationResult.CompiledCode

			// Execute the function
			executionResult, err := m.executor.Execute(ctx, &testFunction, map[string]interface{}{
				"_test": true,
			}, testOptions)

			if err != nil {
				// Handle test execution failure
				result.Message = fmt.Sprintf("Test execution failed: %s", err.Error())

				if options.AutoActivate {
					// Update function status to error
					updates["status"] = string(FunctionStatusError)
					m.service.UpdateFunction(ctx, functionID, updater, map[string]interface{}{
						"status": string(FunctionStatusError),
					})
				}
			} else {
				result.TestExecutionResult = executionResult

				// Check execution status
				if executionResult.Status != "success" {
					result.Message = fmt.Sprintf("Test execution failed: %s", executionResult.Error)

					if options.AutoActivate {
						// Update function status to error
						updates["status"] = string(FunctionStatusError)
						m.service.UpdateFunction(ctx, functionID, updater, map[string]interface{}{
							"status": string(FunctionStatusError),
						})
					}
				}
			}
		}
	}

	// Update function status if auto-activate is enabled
	if options.AutoActivate && updatedFunction.Status == FunctionStatusUpdating {
		// Final update to set status to active
		updates["status"] = string(FunctionStatusActive)
		updatedFunction, err = m.service.UpdateFunction(ctx, functionID, updater, map[string]interface{}{
			"status": string(FunctionStatusActive),
		})

		if err != nil {
			result.Message = fmt.Sprintf("%s; Failed to update status: %s", result.Message, err.Error())
		}

		result.Function = updatedFunction
	}

	// Update deployment result
	result.Status = "completed"
	if result.Message == "" {
		result.Message = "Function updated successfully"
	}

	return result, nil
}

// RollbackFunction rolls back a function to a previous version
func (m *FunctionManager) RollbackFunction(ctx context.Context, functionID string, version int, rollbacker util.Uint160) (*DeploymentResult, error) {
	// Create deployment result
	result := &DeploymentResult{
		Status:     "rollback",
		DeployedAt: time.Now(),
		DeployedBy: rollbacker,
	}

	// Get the function
	function, err := m.service.GetFunction(ctx, functionID)
	if err != nil {
		result.Status = "failed"
		result.Message = fmt.Sprintf("Failed to get function: %s", err.Error())
		return result, err
	}

	result.Function = function

	// Get the previous version
	previousVersion, err := m.service.GetFunctionVersion(ctx, functionID, version)
	if err != nil {
		result.Status = "failed"
		result.Message = fmt.Sprintf("Failed to get version %d: %s", version, err.Error())
		return result, err
	}

	// Update the function status to indicate it's being rolled back
	_, err = m.service.UpdateFunction(ctx, functionID, rollbacker, map[string]interface{}{
		"status": string(FunctionStatusRollback),
	})

	if err != nil {
		result.Status = "failed"
		result.Message = fmt.Sprintf("Failed to update function status: %s", err.Error())
		return result, err
	}

	// Update the function with the previous version's code
	updatedFunction, err := m.service.UpdateFunction(ctx, functionID, rollbacker, map[string]interface{}{
		"code":        previousVersion.Code,
		"description": fmt.Sprintf("%s (Rolled back to v%d)", previousVersion.Description, version),
	})

	if err != nil {
		result.Status = "failed"
		result.Message = fmt.Sprintf("Failed to update function code: %s", err.Error())
		return result, err
	}

	result.Function = updatedFunction

	// Set the function status back to active
	updatedFunction, err = m.service.UpdateFunction(ctx, functionID, rollbacker, map[string]interface{}{
		"status": string(FunctionStatusActive),
	})

	if err != nil {
		result.Status = "failed"
		result.Message = fmt.Sprintf("Failed to update function status: %s", err.Error())
		return result, err
	}

	result.Function = updatedFunction

	// Update deployment result
	result.Status = "completed"
	result.Message = fmt.Sprintf("Successfully rolled back to version %d", version)

	return result, nil
}

// DeleteFunction deletes a function
func (m *FunctionManager) DeleteFunction(ctx context.Context, functionID string, deleter util.Uint160) error {
	return m.service.DeleteFunction(ctx, functionID, deleter)
}

// GetDeployment gets a deployment result by ID
func (m *FunctionManager) GetDeployment(deploymentID string) (*DeploymentResult, bool) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	deployment, exists := m.deployments[deploymentID]
	return deployment, exists
}

// ClearDeploymentHistory clears deployment history older than the given age
func (m *FunctionManager) ClearDeploymentHistory(maxAge time.Duration) {
	m.mu.Lock()
	defer m.mu.Unlock()

	threshold := time.Now().Add(-maxAge)
	for id, deployment := range m.deployments {
		if deployment.DeployedAt.Before(threshold) {
			delete(m.deployments, id)
		}
	}
}
