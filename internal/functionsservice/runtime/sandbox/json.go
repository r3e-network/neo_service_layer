package sandbox

import (
	"context"
	"encoding/json"
	"fmt"

	"go.uber.org/zap"
)

// ExecuteJSON runs JavaScript code with JSON-serialized input and output
func (s *Sandbox) ExecuteJSON(ctx context.Context, jsonInput string) (string, error) {
	var input FunctionInput
	err := json.Unmarshal([]byte(jsonInput), &input)
	if err != nil {
		s.logger.Error("Failed to parse input JSON",
			zap.Error(err),
			zap.String("jsonInput", jsonInput))
		return "", fmt.Errorf("failed to parse input JSON: %w", err)
	}

	// Ensure we have a valid function context
	if input.Context == nil {
		input.Context = NewFunctionContext("unknown")
	}

	s.logger.Info("Executing function from JSON input",
		zap.String("functionId", input.Context.FunctionID),
		zap.String("executionId", input.Context.ExecutionID))

	// Execute the function
	output := s.Execute(ctx, input)

	// Serialize the output to JSON
	jsonOutput, err := json.Marshal(output)
	if err != nil {
		s.logger.Error("Failed to serialize output to JSON",
			zap.String("functionId", input.Context.FunctionID),
			zap.String("executionId", input.Context.ExecutionID),
			zap.Error(err))
		return "", fmt.Errorf("failed to serialize output to JSON: %w", err)
	}

	s.logger.Info("Function execution completed successfully",
		zap.String("functionId", input.Context.FunctionID),
		zap.String("executionId", input.Context.ExecutionID),
		zap.Duration("duration", output.Duration),
		zap.Uint64("memoryUsed", output.MemoryUsed))

	return string(jsonOutput), nil
}
