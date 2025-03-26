package functions

import "errors"

// Define common errors for the Functions service
var (
	// ErrFunctionNotFound is returned when a function is not found
	ErrFunctionNotFound = errors.New("function not found")

	// ErrPermissionDenied is returned when a user does not have permission to access or modify a function
	ErrPermissionDenied = errors.New("permission denied")

	// ErrInvalidFunctionCode is returned when function code is invalid
	ErrInvalidFunctionCode = errors.New("invalid function code")

	// ErrInvalidRuntime is returned when runtime is invalid
	ErrInvalidRuntime = errors.New("invalid runtime")

	// ErrExecutionFailed is returned when function execution fails
	ErrExecutionFailed = errors.New("function execution failed")

	// ErrFunctionDisabled is returned when trying to execute a disabled function
	ErrFunctionDisabled = errors.New("function is disabled")

	// ErrResourceExceeded is returned when function exceeds resource limits
	ErrResourceExceeded = errors.New("function exceeded resource limits")

	// ErrMaxFunctionsReached is returned when user has reached the maximum number of functions
	ErrMaxFunctionsReached = errors.New("maximum number of functions reached")
)