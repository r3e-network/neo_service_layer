package trigger

import (
	"testing"

	"github.com/stretchr/testify/assert"
	"go.uber.org/zap"
)

func TestEvaluator_Evaluate(t *testing.T) {
	logger := zap.NewNop()
	evaluator := NewEvaluator(logger)

	tests := []struct {
		name      string
		condition string
		context   map[string]interface{}
		want      interface{}
		wantErr   bool
	}{
		{
			name:      "simple boolean condition",
			condition: "true",
			context:   map[string]interface{}{},
			want:      true,
			wantErr:   false,
		},
		{
			name:      "simple arithmetic condition",
			condition: "1 + 1",
			context:   map[string]interface{}{},
			want:      float64(2),
			wantErr:   false,
		},
		{
			name:      "context variable condition",
			condition: "event.value > 10",
			context: map[string]interface{}{
				"event": map[string]interface{}{
					"value": 15,
				},
			},
			want:    true,
			wantErr: false,
		},
		{
			name:      "invalid condition",
			condition: "invalid syntax",
			context:   map[string]interface{}{},
			want:      nil,
			wantErr:   true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got, err := evaluator.Evaluate(tt.condition, tt.context)
			if tt.wantErr {
				assert.Error(t, err)
				return
			}
			assert.NoError(t, err)
			assert.Equal(t, tt.want, got)
		})
	}
}

func TestEvaluator_EvaluateComparison(t *testing.T) {
	logger := zap.NewNop()
	evaluator := NewEvaluator(logger)

	context := map[string]interface{}{
		"event": map[string]interface{}{
			"value":  15,
			"name":   "test",
			"active": true,
		},
	}

	tests := []struct {
		name     string
		field    string
		operator string
		value    interface{}
		want     bool
		wantErr  bool
	}{
		{
			name:     "equals operator",
			field:    "value",
			operator: "eq",
			value:    15,
			want:     true,
			wantErr:  false,
		},
		{
			name:     "not equals operator",
			field:    "value",
			operator: "ne",
			value:    10,
			want:     true,
			wantErr:  false,
		},
		{
			name:     "greater than operator",
			field:    "value",
			operator: "gt",
			value:    10,
			want:     true,
			wantErr:  false,
		},
		{
			name:     "less than operator",
			field:    "value",
			operator: "lt",
			value:    20,
			want:     true,
			wantErr:  false,
		},
		{
			name:     "contains operator",
			field:    "name",
			operator: "contains",
			value:    "test",
			want:     true,
			wantErr:  false,
		},
		{
			name:     "unsupported operator",
			field:    "value",
			operator: "invalid",
			value:    10,
			want:     false,
			wantErr:  true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got, err := evaluator.EvaluateComparison(tt.field, tt.operator, tt.value, context)
			if tt.wantErr {
				assert.Error(t, err)
				return
			}
			assert.NoError(t, err)
			assert.Equal(t, tt.want, got)
		})
	}
}

func TestEvaluator_EvaluateMultiple(t *testing.T) {
	logger := zap.NewNop()
	evaluator := NewEvaluator(logger)

	context := map[string]interface{}{
		"event": map[string]interface{}{
			"value":  15,
			"name":   "test",
			"active": true,
		},
	}

	tests := []struct {
		name       string
		conditions []string
		operator   string
		want       bool
		wantErr    bool
	}{
		{
			name: "AND operator - all true",
			conditions: []string{
				"event.value > 10",
				"event.name == 'test'",
				"event.active == true",
			},
			operator: "and",
			want:     true,
			wantErr:  false,
		},
		{
			name: "AND operator - one false",
			conditions: []string{
				"event.value > 10",
				"event.name == 'wrong'",
				"event.active == true",
			},
			operator: "and",
			want:     false,
			wantErr:  false,
		},
		{
			name: "OR operator - one true",
			conditions: []string{
				"event.value < 10",
				"event.name == 'test'",
				"event.active == false",
			},
			operator: "or",
			want:     true,
			wantErr:  false,
		},
		{
			name: "OR operator - all false",
			conditions: []string{
				"event.value < 10",
				"event.name == 'wrong'",
				"event.active == false",
			},
			operator: "or",
			want:     false,
			wantErr:  false,
		},
		{
			name:       "empty conditions",
			conditions: []string{},
			operator:   "and",
			want:       true,
			wantErr:    false,
		},
		{
			name: "invalid operator",
			conditions: []string{
				"event.value > 10",
			},
			operator: "invalid",
			want:     false,
			wantErr:  true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got, err := evaluator.EvaluateMultiple(tt.conditions, tt.operator, context)
			if tt.wantErr {
				assert.Error(t, err)
				return
			}
			assert.NoError(t, err)
			assert.Equal(t, tt.want, got)
		})
	}
}
