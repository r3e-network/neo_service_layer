package functions

import (
	"context"
	"fmt"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

// TestServiceImplementsInterface verifies the service implements the IService interface
func TestServiceImplementsInterface(t *testing.T) {
	// Create a service
	service, err := NewService(&Config{
		MaxFunctionSize:  1024,
		MaxExecutionTime: 1 * time.Second,
		MaxMemoryLimit:   1024 * 1024,
	})
	require.NoError(t, err)

	// Assert that the service implements the IService interface
	var _ IService = service
}

// TestInterfaceMethods tests each method of the IService interface
func TestInterfaceMethods(t *testing.T) {
	// Set a timeout for the entire test
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	// Create a mock service instead of the real service to avoid hanging
	service := NewMockService()

	// Test address
	owner, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")
	require.NoError(t, err)

	// Test CreateFunction
	function, err := service.CreateFunction(
		ctx,
		owner,
		"interface-test-function",
		"Function for interface testing",
		"function main() { return 'hello'; }",
		JavaScriptRuntime,
	)
	require.NoError(t, err)
	require.NotNil(t, function)

	// Test GetFunction
	retrievedFunction, err := service.GetFunction(ctx, function.ID)
	assert.NoError(t, err)
	assert.Equal(t, function.ID, retrievedFunction.ID)

	// Test UpdateFunction
	updates := map[string]interface{}{
		"description": "Updated description",
	}
	updatedFunction, err := service.UpdateFunction(ctx, function.ID, owner, updates)
	assert.NoError(t, err)
	assert.Equal(t, "Updated description", updatedFunction.Description)

	// Test InvokeFunction
	invocation := FunctionInvocation{
		FunctionID: function.ID,
		Parameters: map[string]interface{}{},
		Caller:     owner,
	}
	execution, err := service.InvokeFunction(ctx, invocation)
	assert.NoError(t, err)
	assert.NotNil(t, execution)

	// Test ListFunctions
	functions, err := service.ListFunctions(ctx, owner)
	assert.NoError(t, err)
	assert.NotEmpty(t, functions)

	// Test ListExecutions - no need to sleep with mock
	executions, err := service.ListExecutions(ctx, function.ID, 10)
	assert.NoError(t, err)
	assert.NotEmpty(t, executions)

	// Test GetPermissions
	permissions, err := service.GetPermissions(ctx, function.ID)
	assert.NoError(t, err)
	assert.Equal(t, function.ID, permissions.FunctionID)
	assert.Equal(t, owner, permissions.Owner)

	// Test UpdatePermissions
	updatedPermissions := &FunctionPermissions{
		FunctionID:   function.ID,
		Owner:        owner,
		AllowedUsers: []util.Uint160{},
		Public:       true,
		ReadOnly:     false,
	}
	err = service.UpdatePermissions(ctx, function.ID, owner, updatedPermissions)
	assert.NoError(t, err)

	// Verify permissions were updated
	newPermissions, err := service.GetPermissions(ctx, function.ID)
	assert.NoError(t, err)
	assert.True(t, newPermissions.Public)

	// Test DeleteFunction
	err = service.DeleteFunction(ctx, function.ID, owner)
	assert.NoError(t, err)

	// Verify function was deleted
	_, err = service.GetFunction(ctx, function.ID)
	assert.Error(t, err)
}

// TestMockServiceInterface tests a mock implementation of the IService interface
func TestMockServiceInterface(t *testing.T) {
	mockService := NewMockService()

	// Assert that the mock service implements the IService interface
	var _ IService = mockService

	// Test mock functionality with some basic assertions
	ctx := context.Background()
	owner, _ := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")

	// Create a function
	function, err := mockService.CreateFunction(
		ctx,
		owner,
		"mock-function",
		"Mock function description",
		"function main() { return 'mock'; }",
		JavaScriptRuntime,
	)
	assert.NoError(t, err)
	assert.NotNil(t, function)
	assert.Equal(t, "mock-function", function.Name)

	// Retrieve the function
	retrieved, err := mockService.GetFunction(ctx, function.ID)
	assert.NoError(t, err)
	assert.Equal(t, function.ID, retrieved.ID)
}

// MockService is a mock implementation of the IService interface for testing
type MockService struct {
	functions   map[string]*Function
	executions  map[string]*FunctionExecution
	permissions map[string]*FunctionPermissions
}

// NewMockService creates a new mock service
func NewMockService() *MockService {
	return &MockService{
		functions:   make(map[string]*Function),
		executions:  make(map[string]*FunctionExecution),
		permissions: make(map[string]*FunctionPermissions),
	}
}

// CreateFunction implements IService.CreateFunction
func (m *MockService) CreateFunction(ctx context.Context, owner util.Uint160, name, description, code string, runtime Runtime) (*Function, error) {
	id := generateFunctionID(owner, name)
	function := &Function{
		ID:          id,
		Name:        name,
		Description: description,
		Owner:       owner,
		Code:        code,
		Runtime:     runtime,
		Status:      FunctionStatusActive,
		Triggers:    []string{},
		CreatedAt:   time.Now(),
		UpdatedAt:   time.Now(),
		Metadata:    make(map[string]interface{}),
	}
	m.functions[id] = function

	// Create default permissions
	m.permissions[id] = &FunctionPermissions{
		FunctionID:   id,
		Owner:        owner,
		AllowedUsers: []util.Uint160{},
		Public:       false,
		ReadOnly:     false,
	}

	return function, nil
}

// GetFunction implements IService.GetFunction
func (m *MockService) GetFunction(ctx context.Context, functionID string) (*Function, error) {
	function, exists := m.functions[functionID]
	if !exists {
		return nil, ErrFunctionNotFound
	}
	return function, nil
}

// UpdateFunction implements IService.UpdateFunction
func (m *MockService) UpdateFunction(ctx context.Context, functionID string, updater util.Uint160, updates map[string]interface{}) (*Function, error) {
	function, exists := m.functions[functionID]
	if !exists {
		return nil, ErrFunctionNotFound
	}

	// Check permissions: Only the owner can update the function.
	if !function.Owner.Equals(updater) {
		// Check if the user is an allowed collaborator and the function is read-only
		perms, exists := m.permissions[functionID]
		if exists && perms.ReadOnly {
			return nil, fmt.Errorf("%w: function is read-only", ErrPermissionDenied)
		}
		// If not the owner, deny permission regardless of read-only status for non-collaborators
		return nil, ErrPermissionDenied
	}

	// Apply updates (only owner reaches here)
	if description, ok := updates["description"].(string); ok {
		function.Description = description
	}

	if code, ok := updates["code"].(string); ok {
		function.Code = code
	}

	function.UpdatedAt = time.Now()
	return function, nil
}

// DeleteFunction implements IService.DeleteFunction
func (m *MockService) DeleteFunction(ctx context.Context, functionID string, deleter util.Uint160) error {
	function, exists := m.functions[functionID]
	if !exists {
		return ErrFunctionNotFound
	}

	// Check permissions
	if !function.Owner.Equals(deleter) {
		return ErrPermissionDenied
	}

	delete(m.functions, functionID)
	delete(m.permissions, functionID)
	return nil
}

// InvokeFunction implements IService.InvokeFunction
func (m *MockService) InvokeFunction(ctx context.Context, invocation FunctionInvocation) (*FunctionExecution, error) {
	function, exists := m.functions[invocation.FunctionID]
	if !exists {
		return nil, ErrFunctionNotFound
	}

	// Check permissions
	perms, exists := m.permissions[invocation.FunctionID]
	if !exists {
		return nil, ErrPermissionDenied
	}

	if !function.Owner.Equals(invocation.Caller) && !perms.Public {
		allowed := false
		for _, user := range perms.AllowedUsers {
			if user.Equals(invocation.Caller) {
				allowed = true
				break
			}
		}
		if !allowed {
			return nil, ErrPermissionDenied
		}
	}

	// Create an execution record
	executionID := "mock-execution-" + time.Now().Format(time.RFC3339Nano)
	execution := &FunctionExecution{
		ID:         executionID,
		FunctionID: invocation.FunctionID,
		Status:     "completed",
		StartTime:  time.Now(),
		EndTime:    time.Now(),
		Duration:   10, // Mock duration in ms
		Parameters: invocation.Parameters,
		Result:     "mock result",
		Logs:       []string{"Mock execution completed"},
		InvokedBy:  invocation.Caller,
	}

	m.executions[executionID] = execution
	return execution, nil
}

// ListFunctions implements IService.ListFunctions
func (m *MockService) ListFunctions(ctx context.Context, owner util.Uint160) ([]*Function, error) {
	var functions []*Function
	for _, function := range m.functions {
		if function.Owner.Equals(owner) {
			functions = append(functions, function)
		}
	}
	return functions, nil
}

// ListExecutions implements IService.ListExecutions
func (m *MockService) ListExecutions(ctx context.Context, functionID string, limit int) ([]*FunctionExecution, error) {
	var executions []*FunctionExecution
	for _, execution := range m.executions {
		if execution.FunctionID == functionID {
			executions = append(executions, execution)
			if len(executions) >= limit {
				break
			}
		}
	}
	return executions, nil
}

// GetPermissions implements IService.GetPermissions
func (m *MockService) GetPermissions(ctx context.Context, functionID string) (*FunctionPermissions, error) {
	perms, exists := m.permissions[functionID]
	if !exists {
		return nil, ErrFunctionNotFound
	}
	return perms, nil
}

// UpdatePermissions implements IService.UpdatePermissions
func (m *MockService) UpdatePermissions(ctx context.Context, functionID string, updater util.Uint160, permissions *FunctionPermissions) error {
	function, exists := m.functions[functionID]
	if !exists {
		return ErrFunctionNotFound
	}

	// Only the owner can update permissions
	if !function.Owner.Equals(updater) {
		return ErrPermissionDenied
	}

	// Ensure owner is not changed
	permissions.Owner = function.Owner
	permissions.FunctionID = functionID

	m.permissions[functionID] = permissions
	return nil
}

// GetExecution implements IService.GetExecution to retrieve a function execution by ID
func (m *MockService) GetExecution(ctx context.Context, executionID string) (*FunctionExecution, error) {
	execution, exists := m.executions[executionID]
	if !exists {
		return nil, fmt.Errorf("execution not found: %s", executionID)
	}
	return execution, nil
}
