package functions

import (
	"context"
	"crypto/sha256"
	"encoding/hex"
	"errors"
	"fmt"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/functions/runtime"
)

// Service implements the Functions service
type Service struct {
	config           *Config
	functions        map[string]*Function
	sandbox          *runtime.Sandbox
	executions       map[string]*FunctionExecution
	permissions      map[string]*FunctionPermissions
	functionVersions map[string]map[int]*FunctionVersion
	mu               sync.RWMutex
}

// NewService creates a new Functions service
func NewService(config *Config) (*Service, error) {
	if config == nil {
		return nil, errors.New("config cannot be nil")
	}

	// Validate config
	if config.MaxFunctionSize <= 0 {
		config.MaxFunctionSize = 1024 * 1024 // 1MB default
	}
	if config.MaxExecutionTime <= 0 {
		config.MaxExecutionTime = 5 * time.Second // 5s default
	}
	if config.MaxMemoryLimit <= 0 {
		config.MaxMemoryLimit = 128 * 1024 * 1024 // 128MB default
	}
	if config.DefaultRuntime == "" {
		config.DefaultRuntime = string(JavaScriptRuntime)
	}

	// Create sandbox for JavaScript execution
	sandboxConfig := runtime.SandboxConfig{
		MemoryLimit:   config.MaxMemoryLimit,
		TimeoutMillis: config.MaxExecutionTime.Milliseconds(),
		AllowNetwork:  config.EnableNetworkAccess,
		AllowFileIO:   config.EnableFileIO,
	}
	sandbox := runtime.NewSandbox(sandboxConfig)

	return &Service{
		config:           config,
		functions:        make(map[string]*Function),
		sandbox:          sandbox,
		executions:       make(map[string]*FunctionExecution),
		permissions:      make(map[string]*FunctionPermissions),
		functionVersions: make(map[string]map[int]*FunctionVersion),
	}, nil
}

// CreateFunction creates a new function
func (s *Service) CreateFunction(ctx context.Context, owner util.Uint160, name, description, code string, runtime Runtime) (*Function, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Validate input
	if len(code) > s.config.MaxFunctionSize {
		return nil, fmt.Errorf("function code exceeds maximum size of %d bytes", s.config.MaxFunctionSize)
	}

	if runtime == "" {
		runtime = Runtime(s.config.DefaultRuntime)
	}

	// Check if runtime is supported
	if runtime != JavaScriptRuntime {
		return nil, fmt.Errorf("unsupported runtime: %s", runtime)
	}

	// Generate function ID
	funcID := generateFunctionID(owner, name)

	// Check if function already exists
	if _, exists := s.functions[funcID]; exists {
		return nil, fmt.Errorf("function with ID %s already exists", funcID)
	}

	// Create the function
	now := time.Now()
	function := &Function{
		ID:          funcID,
		Name:        name,
		Description: description,
		Owner:       owner,
		Code:        code,
		Runtime:     runtime,
		Status:      FunctionStatusActive,
		Triggers:    []string{},
		CreatedAt:   now,
		UpdatedAt:   now,
		Metadata:    make(map[string]interface{}),
	}

	// Store function
	s.functions[funcID] = function

	// Initialize permissions
	s.permissions[funcID] = &FunctionPermissions{
		FunctionID:   funcID,
		Owner:        owner,
		AllowedUsers: []util.Uint160{},
		Public:       false,
		ReadOnly:     false,
	}

	// Initialize versions
	s.functionVersions[funcID] = map[int]*FunctionVersion{
		1: {
			FunctionID:  funcID,
			Version:     1,
			Code:        code,
			Description: description,
			CreatedAt:   now,
			CreatedBy:   owner,
			Status:      "active",
		},
	}

	return function, nil
}

// GetFunction retrieves a function by ID
func (s *Service) GetFunction(ctx context.Context, functionID string) (*Function, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	function, exists := s.functions[functionID]
	if !exists {
		return nil, fmt.Errorf("function with ID %s not found", functionID)
	}

	return function, nil
}

// UpdateFunction updates an existing function
func (s *Service) UpdateFunction(ctx context.Context, functionID string, updater util.Uint160, updates map[string]interface{}) (*Function, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Get the function
	function, exists := s.functions[functionID]
	if !exists {
		return nil, fmt.Errorf("function with ID %s not found", functionID)
	}

	// Check permissions
	if !function.Owner.Equals(updater) {
		perms, exists := s.permissions[functionID]
		if !exists || perms.ReadOnly {
			return nil, errors.New("permission denied: not function owner or function is read-only")
		}
	}

	// Apply updates
	updated := false
	if description, ok := updates["description"].(string); ok {
		function.Description = description
		updated = true
	}

	if code, ok := updates["code"].(string); ok {
		if len(code) > s.config.MaxFunctionSize {
			return nil, fmt.Errorf("function code exceeds maximum size of %d bytes", s.config.MaxFunctionSize)
		}
		function.Code = code
		updated = true

		// Create new version
		maxVersion := 0
		for version := range s.functionVersions[functionID] {
			if version > maxVersion {
				maxVersion = version
			}
		}
		newVersion := maxVersion + 1

		s.functionVersions[functionID][newVersion] = &FunctionVersion{
			FunctionID:  functionID,
			Version:     newVersion,
			Code:        code,
			Description: function.Description,
			CreatedAt:   time.Now(),
			CreatedBy:   updater,
			Status:      "active",
		}
	}

	if status, ok := updates["status"].(string); ok {
		function.Status = FunctionStatus(status)
		updated = true
	}

	if metadata, ok := updates["metadata"].(map[string]interface{}); ok {
		function.Metadata = metadata
		updated = true
	}

	if updated {
		function.UpdatedAt = time.Now()
	}

	return function, nil
}

// DeleteFunction deletes a function
func (s *Service) DeleteFunction(ctx context.Context, functionID string, deleter util.Uint160) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Get the function
	function, exists := s.functions[functionID]
	if !exists {
		return fmt.Errorf("function with ID %s not found", functionID)
	}

	// Check permissions
	if !function.Owner.Equals(deleter) {
		return errors.New("permission denied: not function owner")
	}

	// Remove function and related data
	delete(s.functions, functionID)
	delete(s.permissions, functionID)
	delete(s.functionVersions, functionID)

	return nil
}

// InvokeFunction invokes a function
func (s *Service) InvokeFunction(ctx context.Context, invocation FunctionInvocation) (*FunctionExecution, error) {
	s.mu.RLock()
	function, exists := s.functions[invocation.FunctionID]
	s.mu.RUnlock()

	if !exists {
		return nil, fmt.Errorf("function with ID %s not found", invocation.FunctionID)
	}

	// Check permissions
	s.mu.RLock()
	perms, permsExist := s.permissions[invocation.FunctionID]
	s.mu.RUnlock()

	if !permsExist {
		return nil, errors.New("permission configuration not found")
	}

	// Check if caller has permission
	if !function.Owner.Equals(invocation.Caller) && !perms.Public {
		hasPermission := false
		for _, allowed := range perms.AllowedUsers {
			if allowed.Equals(invocation.Caller) {
				hasPermission = true
				break
			}
		}
		if !hasPermission {
			return nil, errors.New("permission denied: not authorized to invoke this function")
		}
	}

	// Create execution record
	executionID := uuid.New().String()
	execution := &FunctionExecution{
		ID:         executionID,
		FunctionID: invocation.FunctionID,
		Status:     "running",
		StartTime:  time.Now(),
		Parameters: invocation.Parameters,
		InvokedBy:  invocation.Caller,
		TraceID:    invocation.TraceID,
	}

	// Store execution
	s.mu.Lock()
	s.executions[executionID] = execution
	s.mu.Unlock()

	// If async, return immediately
	if invocation.Async {
		go s.executeFunction(function, execution, invocation.Parameters)
		return execution, nil
	}

	// Execute synchronously
	s.executeFunction(function, execution, invocation.Parameters)
	return execution, nil
}

// GetExecution retrieves an execution by ID
func (s *Service) GetExecution(ctx context.Context, executionID string) (*FunctionExecution, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	execution, exists := s.executions[executionID]
	if !exists {
		return nil, fmt.Errorf("execution with ID %s not found", executionID)
	}

	return execution, nil
}

// ListFunctions lists all functions
func (s *Service) ListFunctions(ctx context.Context, owner util.Uint160) ([]*Function, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	var functions []*Function
	for _, function := range s.functions {
		if owner.Equals(util.Uint160{}) || function.Owner.Equals(owner) {
			functions = append(functions, function)
		}
	}

	return functions, nil
}

// ListExecutions lists executions for a function
func (s *Service) ListExecutions(ctx context.Context, functionID string, limit int) ([]*FunctionExecution, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	var executions []*FunctionExecution
	for _, execution := range s.executions {
		if execution.FunctionID == functionID {
			executions = append(executions, execution)
			if limit > 0 && len(executions) >= limit {
				break
			}
		}
	}

	return executions, nil
}

// UpdatePermissions updates function permissions
func (s *Service) UpdatePermissions(ctx context.Context, functionID string, updater util.Uint160, permissions *FunctionPermissions) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Get the function
	function, exists := s.functions[functionID]
	if !exists {
		return fmt.Errorf("function with ID %s not found", functionID)
	}

	// Check if updater is the owner
	if !function.Owner.Equals(updater) {
		return errors.New("permission denied: not function owner")
	}

	// Validate permissions
	if permissions.FunctionID != functionID {
		return errors.New("permission denied: function ID mismatch")
	}

	// Update permissions
	s.permissions[functionID] = permissions

	return nil
}

// GetPermissions gets function permissions
func (s *Service) GetPermissions(ctx context.Context, functionID string) (*FunctionPermissions, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	permissions, exists := s.permissions[functionID]
	if !exists {
		return nil, fmt.Errorf("permissions for function ID %s not found", functionID)
	}

	return permissions, nil
}

// executeFunction executes a function and updates the execution record
func (s *Service) executeFunction(function *Function, execution *FunctionExecution, parameters map[string]interface{}) {
	// Check function status
	if function.Status != FunctionStatusActive {
		execution.Status = "failed"
		execution.Error = fmt.Sprintf("function is not active (status: %s)", function.Status)
		execution.EndTime = time.Now()
		execution.Duration = execution.EndTime.Sub(execution.StartTime).Milliseconds()
		return
	}

	// Prepare input for sandbox
	input := runtime.FunctionInput{
		Code:       function.Code,
		Args:       parameters,
		Parameters: parameters,
	}

	// Execute in sandbox
	output, err := s.sandbox.Execute(context.Background(), input)
	if err != nil {
		execution.Status = "failed"
		execution.Error = fmt.Sprintf("sandbox execution error: %v", err)
		execution.EndTime = time.Now()
		execution.Duration = execution.EndTime.Sub(execution.StartTime).Milliseconds()
		return
	}

	// Update execution with results
	execution.EndTime = time.Now()
	execution.Duration = output.Duration
	execution.MemoryUsed = output.MemoryUsed
	execution.Result = output.Result
	execution.Logs = output.Logs

	if output.Error != "" {
		execution.Status = "failed"
		execution.Error = output.Error
	} else {
		execution.Status = "completed"
	}

	// Update function's last executed time
	s.mu.Lock()
	function.LastExecuted = execution.EndTime
	s.mu.Unlock()
}

// generateFunctionID generates a unique ID for a function
func generateFunctionID(owner util.Uint160, name string) string {
	// Combine owner and name to create a unique identifier
	combined := append(owner.BytesBE(), []byte(name)...)
	hash := sha256.Sum256(combined)
	return hex.EncodeToString(hash[:])[:16] // Use first 16 chars of hash
}

// GetFunctionVersion retrieves a specific version of a function
func (s *Service) GetFunctionVersion(ctx context.Context, functionID string, version int) (*FunctionVersion, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	// Check if function exists
	if _, exists := s.functions[functionID]; !exists {
		return nil, fmt.Errorf("function with ID %s not found", functionID)
	}

	// Check if version map exists
	versionMap, exists := s.functionVersions[functionID]
	if !exists || len(versionMap) == 0 {
		return nil, fmt.Errorf("no versions found for function with ID %s", functionID)
	}

	// Get the specific version
	funcVersion, exists := versionMap[version]
	if !exists {
		return nil, fmt.Errorf("version %d not found for function with ID %s", version, functionID)
	}

	return funcVersion, nil
}

// ListFunctionVersions lists all versions of a function
func (s *Service) ListFunctionVersions(ctx context.Context, functionID string) ([]*FunctionVersion, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	// Check if function exists
	if _, exists := s.functions[functionID]; !exists {
		return nil, fmt.Errorf("function with ID %s not found", functionID)
	}

	// Check if version map exists
	versionMap, exists := s.functionVersions[functionID]
	if !exists || len(versionMap) == 0 {
		return []*FunctionVersion{}, nil
	}

	// Convert map to slice
	versions := make([]*FunctionVersion, 0, len(versionMap))
	for _, version := range versionMap {
		versions = append(versions, version)
	}

	return versions, nil
}
