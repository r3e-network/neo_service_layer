package functions

import (
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// Config holds configuration for the Functions service
type Config struct {
	MaxFunctionSize     int           // Maximum size of function code in bytes
	MaxExecutionTime    time.Duration // Maximum execution time for a function
	MaxMemoryLimit      int64         // Maximum memory limit for functions in bytes
	EnableNetworkAccess bool          // Whether to allow network access in functions
	EnableFileIO        bool          // Whether to allow file I/O in functions
	DefaultRuntime      string        // Default runtime for functions (e.g., "javascript")
}

// FunctionStatus represents the status of a function
type FunctionStatus string

const (
	// FunctionStatusActive indicates an active function
	FunctionStatusActive FunctionStatus = "active"

	// FunctionStatusDisabled indicates a disabled function
	FunctionStatusDisabled FunctionStatus = "disabled"

	// FunctionStatusError indicates a function with errors
	FunctionStatusError FunctionStatus = "error"
)

// Runtime represents a supported runtime environment
type Runtime string

const (
	// JavaScriptRuntime represents the JavaScript runtime
	JavaScriptRuntime Runtime = "javascript"
)

// Function represents a serverless function
type Function struct {
	ID           string                 `json:"id"`           // Unique identifier
	Name         string                 `json:"name"`         // Function name
	Description  string                 `json:"description"`  // Function description
	Owner        util.Uint160           `json:"owner"`        // Function owner
	Code         string                 `json:"code"`         // Function code
	Runtime      Runtime                `json:"runtime"`      // Function runtime
	Status       FunctionStatus         `json:"status"`       // Function status
	Triggers     []string               `json:"triggers"`     // Associated triggers
	CreatedAt    time.Time              `json:"createdAt"`    // When the function was created
	UpdatedAt    time.Time              `json:"updatedAt"`    // When the function was last updated
	LastExecuted time.Time              `json:"lastExecuted"` // When the function was last executed
	Metadata     map[string]interface{} `json:"metadata"`     // Additional metadata
}

// FunctionExecution represents a single execution of a function
type FunctionExecution struct {
	ID         string                 `json:"id"`         // Unique identifier
	FunctionID string                 `json:"functionId"` // Function ID
	Trigger    string                 `json:"trigger"`    // Trigger ID (if applicable)
	Status     string                 `json:"status"`     // Execution status
	StartTime  time.Time              `json:"startTime"`  // When execution started
	EndTime    time.Time              `json:"endTime"`    // When execution ended
	Duration   int64                  `json:"duration"`   // Execution duration in milliseconds
	MemoryUsed int64                  `json:"memoryUsed"` // Memory used in bytes
	Parameters map[string]interface{} `json:"parameters"` // Execution parameters
	Result     interface{}            `json:"result"`     // Execution result
	Logs       []string               `json:"logs"`       // Execution logs
	Error      string                 `json:"error"`      // Error message (if any)
	InvokedBy  util.Uint160           `json:"invokedBy"`  // Who invoked the execution
	BatchID    string                 `json:"batchId"`    // Batch ID (if part of a batch)
	CostInGas  int64                  `json:"costInGas"`  // Execution cost in GAS
	TraceID    string                 `json:"traceId"`    // For request tracing
}

// FunctionInvocation represents a request to invoke a function
type FunctionInvocation struct {
	FunctionID  string                 `json:"functionId"`  // Function ID
	Parameters  map[string]interface{} `json:"parameters"`  // Invocation parameters
	Async       bool                   `json:"async"`       // Whether to run asynchronously
	Caller      util.Uint160           `json:"caller"`      // Who is calling the function
	TraceID     string                 `json:"traceId"`     // For request tracing
	Idempotency string                 `json:"idempotency"` // Idempotency key
}

// FunctionPermissions represents access permissions for a function
type FunctionPermissions struct {
	FunctionID   string         `json:"functionId"`   // Function ID
	Owner        util.Uint160   `json:"owner"`        // Function owner
	AllowedUsers []util.Uint160 `json:"allowedUsers"` // Users allowed to invoke
	Public       bool           `json:"public"`       // Whether the function is public
	ReadOnly     bool           `json:"readOnly"`     // Whether the function is read-only
}

// FunctionVersion represents a version of a function
type FunctionVersion struct {
	FunctionID  string       `json:"functionId"`  // Function ID
	Version     int          `json:"version"`     // Version number
	Code        string       `json:"code"`        // Function code
	Description string       `json:"description"` // Version description
	CreatedAt   time.Time    `json:"createdAt"`   // When the version was created
	CreatedBy   util.Uint160 `json:"createdBy"`   // Who created the version
	Status      string       `json:"status"`      // Version status
}
