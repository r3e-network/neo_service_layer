package runtime

import (
	"context"

	"github.com/r3e-network/neo_service_layer/internal/functionservice/runtime/sandbox"
)

// LegacySandbox provides backward compatibility with the original sandbox implementation
// while using the new modular sandbox under the hood.
type LegacySandbox struct {
	internal *sandbox.Sandbox
}

// NewLegacySandbox creates a backward-compatible sandbox instance with the specified configuration
func NewLegacySandbox(config SandboxConfig) *LegacySandbox {
	// Convert the old config format to the new one
	newConfig := sandbox.SandboxConfig{
		MemoryLimit:            config.MemoryLimit,
		TimeoutMillis:          config.TimeoutMillis,
		StackSize:              config.StackSize,
		AllowNetwork:           config.AllowNetwork,
		AllowFileIO:            config.AllowFileIO,
		ServiceLayerURL:        config.ServiceLayerURL,
		EnableInteroperability: config.EnableInteroperability,
		Logger:                 config.Logger,
	}

	// Create a new sandbox using the refactored implementation
	return &LegacySandbox{
		internal: sandbox.New(newConfig),
	}
}

// Execute runs JavaScript code in the sandbox with the original API
func (s *LegacySandbox) Execute(ctx context.Context, input FunctionInput) (*FunctionOutput, error) {
	// Convert the old input format to the new one
	newInput := sandbox.FunctionInput{
		Code:       input.Code,
		Args:       convertArgs(input.Args),
		Secrets:    input.Secrets,
		Parameters: input.Parameters,
	}

	if input.FunctionContext != nil {
		newInput.Context = &sandbox.FunctionContext{
			FunctionID:      input.FunctionContext.FunctionID,
			ExecutionID:     input.FunctionContext.ExecutionID,
			Owner:           input.FunctionContext.Owner,
			Caller:          input.FunctionContext.Caller,
			Parameters:      input.FunctionContext.Parameters,
			Environment:     input.FunctionContext.Env,
			TraceID:         input.FunctionContext.TraceID,
			ServiceLayerURL: input.FunctionContext.ServiceLayerURL,
		}

		// Set up service clients if available
		if input.FunctionContext.Services != nil {
			newInput.Context.ServiceClients = &sandbox.ServiceClients{
				Wallet:  input.FunctionContext.Services.Transaction,
				Storage: nil, // Not available in old implementation
				Oracle:  input.FunctionContext.Services.PriceFeed,
			}
		}
	}

	// Execute the function
	newOutput := s.internal.Execute(ctx, newInput)

	// Convert the new output format to the old one
	output := &FunctionOutput{
		Result:     newOutput.Result,
		Logs:       newOutput.Logs,
		Error:      newOutput.Error,
		Duration:   int64(newOutput.Duration.Milliseconds()),
		MemoryUsed: int64(newOutput.MemoryUsed),
	}

	return output, nil
}

// ExecuteJSON runs JavaScript code with JSON-serialized input and output
func (s *LegacySandbox) ExecuteJSON(ctx context.Context, jsonInput string) (string, error) {
	return s.internal.ExecuteJSON(ctx, jsonInput)
}

// Close releases resources used by the sandbox
func (s *LegacySandbox) Close() {
	if s.internal != nil {
		s.internal.Close()
	}
}

// Helper function to convert old args format to the new one
func convertArgs(oldArgs map[string]interface{}) []interface{} {
	if oldArgs == nil || len(oldArgs) == 0 {
		return []interface{}{}
	}

	// For backward compatibility, we'll just pass a slice with the map as the first element
	return []interface{}{oldArgs}
}
