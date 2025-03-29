package trigger

import (
	"fmt"

	"github.com/expr-lang/expr"
	"github.com/pkg/errors"
	"go.uber.org/zap"
)

// DefaultEvaluator implements the Evaluator interface
type DefaultEvaluator struct {
	logger *zap.Logger
}

// NewEvaluator creates a new evaluator
func NewEvaluator(logger *zap.Logger) *DefaultEvaluator {
	return &DefaultEvaluator{
		logger: logger,
	}
}

// Evaluate evaluates a condition against a context
func (e *DefaultEvaluator) Evaluate(condition string, context map[string]interface{}) (interface{}, error) {
	// Create program
	program, err := expr.Compile(condition, expr.Env(context))
	if err != nil {
		return nil, errors.Wrap(err, "failed to compile condition")
	}

	// Run program
	result, err := expr.Run(program, context)
	if err != nil {
		return nil, errors.Wrap(err, "failed to evaluate condition")
	}

	// Log evaluation result
	e.logger.Debug("Evaluated condition",
		zap.String("condition", condition),
		zap.Any("result", result))

	return result, nil
}

// Helper functions for common condition patterns
func (e *DefaultEvaluator) EvaluateComparison(field string, operator string, value interface{}, context map[string]interface{}) (bool, error) {
	// Build condition expression
	var condition string
	switch operator {
	case "eq", "==":
		condition = fmt.Sprintf("event.%s == %v", field, value)
	case "ne", "!=":
		condition = fmt.Sprintf("event.%s != %v", field, value)
	case "gt", ">":
		condition = fmt.Sprintf("event.%s > %v", field, value)
	case "gte", ">=":
		condition = fmt.Sprintf("event.%s >= %v", field, value)
	case "lt", "<":
		condition = fmt.Sprintf("event.%s < %v", field, value)
	case "lte", "<=":
		condition = fmt.Sprintf("event.%s <= %v", field, value)
	case "contains":
		condition = fmt.Sprintf("contains(event.%s, %v)", field, value)
	case "startsWith":
		condition = fmt.Sprintf("startsWith(event.%s, %v)", field, value)
	case "endsWith":
		condition = fmt.Sprintf("endsWith(event.%s, %v)", field, value)
	default:
		return false, fmt.Errorf("unsupported operator: %s", operator)
	}

	// Evaluate condition
	result, err := e.Evaluate(condition, context)
	if err != nil {
		return false, err
	}

	// Convert result to boolean
	boolResult, ok := result.(bool)
	if !ok {
		return false, fmt.Errorf("condition did not evaluate to boolean: %v", result)
	}

	return boolResult, nil
}

// EvaluateMultiple evaluates multiple conditions with AND/OR logic
func (e *DefaultEvaluator) EvaluateMultiple(conditions []string, operator string, context map[string]interface{}) (bool, error) {
	if len(conditions) == 0 {
		return true, nil
	}

	// Build combined condition
	var condition string
	switch operator {
	case "and", "AND":
		condition = conditions[0]
		for i := 1; i < len(conditions); i++ {
			condition = fmt.Sprintf("(%s) && (%s)", condition, conditions[i])
		}
	case "or", "OR":
		condition = conditions[0]
		for i := 1; i < len(conditions); i++ {
			condition = fmt.Sprintf("(%s) || (%s)", condition, conditions[i])
		}
	default:
		return false, fmt.Errorf("unsupported operator: %s", operator)
	}

	// Evaluate combined condition
	result, err := e.Evaluate(condition, context)
	if err != nil {
		return false, err
	}

	// Convert result to boolean
	boolResult, ok := result.(bool)
	if !ok {
		return false, fmt.Errorf("condition did not evaluate to boolean: %v", result)
	}

	return boolResult, nil
}
