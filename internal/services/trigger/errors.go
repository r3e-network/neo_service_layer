package trigger

import "errors"

// Define common errors for the Trigger service
var (
	// ErrTriggerNotFound is returned when a trigger is not found
	ErrTriggerNotFound = errors.New("trigger not found")

	// ErrPermissionDenied is returned when a user does not have permission to access or modify a trigger
	ErrPermissionDenied = errors.New("permission denied")

	// ErrInvalidTriggerType is returned when trigger type is invalid
	ErrInvalidTriggerType = errors.New("invalid trigger type")

	// ErrInvalidTriggerCondition is returned when trigger condition is invalid
	ErrInvalidTriggerCondition = errors.New("invalid trigger condition")

	// ErrInvalidTriggerAction is returned when trigger action is invalid
	ErrInvalidTriggerAction = errors.New("invalid trigger action")

	// ErrTriggerDisabled is returned when trying to execute a disabled trigger
	ErrTriggerDisabled = errors.New("trigger is disabled")

	// ErrMaxTriggersReached is returned when user has reached the maximum number of triggers
	ErrMaxTriggersReached = errors.New("maximum number of triggers reached")

	// ErrExecutionFailed is returned when trigger execution fails
	ErrExecutionFailed = errors.New("trigger execution failed")
)