package trigger

// Condition represents a trigger condition
type Condition struct {
	Type       string                 `json:"type"`
	Expression string                 `json:"expression"`
	Context    map[string]interface{} `json:"context"`
}

// Evaluate evaluates the condition with the given context
func (c *Condition) Evaluate(ctx map[string]interface{}) (bool, error) {
	// TODO: Implement condition evaluation
	return true, nil
}
