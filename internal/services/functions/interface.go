package functions

import (
	"context"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// IService defines the interface for the Functions service
type IService interface {
	// CreateFunction creates a new function
	CreateFunction(ctx context.Context, owner util.Uint160, name, description, code string, runtime Runtime) (*Function, error)

	// GetFunction retrieves a function by ID
	GetFunction(ctx context.Context, functionID string) (*Function, error)

	// UpdateFunction updates an existing function
	UpdateFunction(ctx context.Context, functionID string, updater util.Uint160, updates map[string]interface{}) (*Function, error)

	// DeleteFunction deletes a function
	DeleteFunction(ctx context.Context, functionID string, deleter util.Uint160) error

	// InvokeFunction invokes a function
	InvokeFunction(ctx context.Context, invocation FunctionInvocation) (*FunctionExecution, error)

	// ListFunctions lists all functions for an owner
	ListFunctions(ctx context.Context, owner util.Uint160) ([]*Function, error)

	// ListExecutions lists executions for a function
	ListExecutions(ctx context.Context, functionID string, limit int) ([]*FunctionExecution, error)

	// GetPermissions gets permissions for a function
	GetPermissions(ctx context.Context, functionID string) (*FunctionPermissions, error)

	// UpdatePermissions updates permissions for a function
	UpdatePermissions(ctx context.Context, functionID string, updater util.Uint160, permissions *FunctionPermissions) error
}
