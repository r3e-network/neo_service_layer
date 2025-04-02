package trigger

import (
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
)

// TestServiceConfig tests that the service configuration structure works correctly
func TestServiceConfig(t *testing.T) {
	// Test valid configuration
	config := &ServiceConfig{
		MaxTriggers:     100,
		MaxExecutions:   50,
		ExecutionWindow: time.Hour * 24,
	}

	assert.Equal(t, 100, config.MaxTriggers)
	assert.Equal(t, 50, config.MaxExecutions)
	assert.Equal(t, time.Hour*24, config.ExecutionWindow)
}
