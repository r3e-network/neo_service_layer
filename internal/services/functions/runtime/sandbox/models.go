package sandbox

import (
	"time"

	"github.com/google/uuid"
)

// FunctionInput represents input to a function execution
type FunctionInput struct {
	// The JavaScript code to execute
	Code string

	// Arguments to pass to the function
	Args []interface{}

	// Secrets available to the function
	Secrets map[string]string

	// Parameters for the function
	Parameters map[string]interface{}

	// Execution context
	Context *FunctionContext
}

// FunctionContext represents the execution context for a function
type FunctionContext struct {
	// Function identifier
	FunctionID string

	// Unique execution ID
	ExecutionID string

	// Owner of the function
	Owner string

	// Caller of the function
	Caller string

	// Parameters for the function
	Parameters map[string]interface{}

	// Environment variables
	Environment map[string]string

	// Trace ID for distributed tracing
	TraceID string

	// URL of the Neo Service Layer
	ServiceLayerURL string

	// Service clients for interoperability
	ServiceClients *ServiceClients
}

// ServiceClients holds references to Neo Service Layer services
type ServiceClients struct {
	// Wallet service client
	Wallet interface{}

	// Storage service client
	Storage interface{}

	// Oracle service client
	Oracle interface{}
}

// FunctionOutput represents output from a function execution
type FunctionOutput struct {
	// The result of the function execution
	Result interface{}

	// Logs from the function execution
	Logs []string

	// Error message if the function failed
	Error string

	// Duration of the execution
	Duration time.Duration

	// Memory used during execution (in bytes)
	MemoryUsed uint64
}

// NewFunctionContext creates a new function context with a generated execution ID
func NewFunctionContext(functionID string) *FunctionContext {
	return &FunctionContext{
		FunctionID:     functionID,
		ExecutionID:    GenerateExecutionID(),
		Parameters:     make(map[string]interface{}),
		Environment:    make(map[string]string),
		ServiceClients: &ServiceClients{},
	}
}

// GenerateExecutionID generates a unique execution ID
func GenerateExecutionID() string {
	return uuid.New().String()
}

// WithOwner sets the owner of the function
func (ctx *FunctionContext) WithOwner(owner string) *FunctionContext {
	ctx.Owner = owner
	return ctx
}

// WithCaller sets the caller of the function
func (ctx *FunctionContext) WithCaller(caller string) *FunctionContext {
	ctx.Caller = caller
	return ctx
}

// WithParameters sets the parameters for the function
func (ctx *FunctionContext) WithParameters(params map[string]interface{}) *FunctionContext {
	ctx.Parameters = params
	return ctx
}

// WithEnvironment sets the environment variables for the function
func (ctx *FunctionContext) WithEnvironment(env map[string]string) *FunctionContext {
	ctx.Environment = env
	return ctx
}

// WithTraceID sets the trace ID for the function
func (ctx *FunctionContext) WithTraceID(traceID string) *FunctionContext {
	ctx.TraceID = traceID
	return ctx
}

// WithServiceLayerURL sets the Neo Service Layer URL
func (ctx *FunctionContext) WithServiceLayerURL(url string) *FunctionContext {
	ctx.ServiceLayerURL = url
	return ctx
}

// WithWalletService adds a wallet service client to the context
func (ctx *FunctionContext) WithWalletService(wallet interface{}) *FunctionContext {
	if ctx.ServiceClients == nil {
		ctx.ServiceClients = &ServiceClients{}
	}
	ctx.ServiceClients.Wallet = wallet
	return ctx
}

// WithStorageService adds a storage service client to the context
func (ctx *FunctionContext) WithStorageService(storage interface{}) *FunctionContext {
	if ctx.ServiceClients == nil {
		ctx.ServiceClients = &ServiceClients{}
	}
	ctx.ServiceClients.Storage = storage
	return ctx
}

// WithOracleService adds an oracle service client to the context
func (ctx *FunctionContext) WithOracleService(oracle interface{}) *FunctionContext {
	if ctx.ServiceClients == nil {
		ctx.ServiceClients = &ServiceClients{}
	}
	ctx.ServiceClients.Oracle = oracle
	return ctx
}
