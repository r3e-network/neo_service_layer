package errors

import (
	"fmt"
	"runtime"
	"strings"
)

// ErrorType represents a type of error
type ErrorType string

// Error types
const (
	Internal      ErrorType = "internal"
	BadRequest    ErrorType = "bad_request"
	Unauthorized  ErrorType = "unauthorized"
	Forbidden     ErrorType = "forbidden"
	NotFound      ErrorType = "not_found"
	AlreadyExists ErrorType = "already_exists"
	Unavailable   ErrorType = "unavailable"
	Timeout       ErrorType = "timeout"
)

// ServiceError represents a service error
type ServiceError struct {
	Type    ErrorType
	Message string
	Cause   error
	Stack   string
	Code    string
	Params  map[string]interface{}
}

// Error returns the error message
func (e *ServiceError) Error() string {
	if e.Cause != nil {
		return fmt.Sprintf("%s: %s", e.Message, e.Cause.Error())
	}
	return e.Message
}

// Unwrap returns the wrapped error
func (e *ServiceError) Unwrap() error {
	return e.Cause
}

// WithParams adds parameters to the error
func (e *ServiceError) WithParams(params map[string]interface{}) *ServiceError {
	if e.Params == nil {
		e.Params = make(map[string]interface{})
	}
	for k, v := range params {
		e.Params[k] = v
	}
	return e
}

// WithCode adds a code to the error
func (e *ServiceError) WithCode(code string) *ServiceError {
	e.Code = code
	return e
}

// New creates a new service error
func New(errType ErrorType, message string) *ServiceError {
	return &ServiceError{
		Type:    errType,
		Message: message,
		Stack:   captureStack(),
		Params:  make(map[string]interface{}),
	}
}

// Wrap wraps an error with a new service error
func Wrap(err error, errType ErrorType, message string) *ServiceError {
	return &ServiceError{
		Type:    errType,
		Message: message,
		Cause:   err,
		Stack:   captureStack(),
		Params:  make(map[string]interface{}),
	}
}

// IsType checks if an error is of a specific type
func IsType(err error, errType ErrorType) bool {
	if svcErr, ok := err.(*ServiceError); ok {
		return svcErr.Type == errType
	}
	return false
}

// captureStack captures a stack trace
func captureStack() string {
	const depth = 32
	var pcs [depth]uintptr
	n := runtime.Callers(3, pcs[:])
	frames := runtime.CallersFrames(pcs[:n])

	var builder strings.Builder
	for {
		frame, more := frames.Next()
		if !more {
			break
		}

		// Skip standard library and runtime frames
		if strings.Contains(frame.File, "runtime/") {
			continue
		}

		builder.WriteString(fmt.Sprintf("%s:%d - %s\n", frame.File, frame.Line, frame.Function))
	}

	return builder.String()
}

// InternalError returns a new internal error
func InternalError(message string) *ServiceError {
	return New(Internal, message)
}

// BadRequestError returns a new bad request error
func BadRequestError(message string) *ServiceError {
	return New(BadRequest, message)
}

// UnauthorizedError returns a new unauthorized error
func UnauthorizedError(message string) *ServiceError {
	return New(Unauthorized, message)
}

// ForbiddenError returns a new forbidden error
func ForbiddenError(message string) *ServiceError {
	return New(Forbidden, message)
}

// NotFoundError returns a new not found error
func NotFoundError(message string) *ServiceError {
	return New(NotFound, message)
}

// AlreadyExistsError returns a new already exists error
func AlreadyExistsError(message string) *ServiceError {
	return New(AlreadyExists, message)
}

// UnavailableError returns a new unavailable error
func UnavailableError(message string) *ServiceError {
	return New(Unavailable, message)
}

// TimeoutError returns a new timeout error
func TimeoutError(message string) *ServiceError {
	return New(Timeout, message)
}