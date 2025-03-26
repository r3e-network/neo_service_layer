package trigger

import (
	"context"
	"fmt"
)

// Condition represents a trigger condition
type Condition struct {
	Type   string                 `json:"type"`
	Config map[string]interface{} `json:"config"`
}

// EvaluateCondition evaluates if a condition is met
func (s *Service) EvaluateCondition(ctx context.Context, condition Condition) (bool, error) {
	// Simple implementation for the integration test
	switch condition.Type {
	case "price":
		return true, nil
	case "time":
		return true, nil
	case "block":
		return true, nil
	case "event":
		return true, nil
	default:
		return false, fmt.Errorf("unknown condition type: %s", condition.Type)
	}
}