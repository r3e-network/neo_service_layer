package logging

import (
	"context"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

func TestNewService(t *testing.T) {
	tests := []struct {
		name       string
		config     *Config
		shouldFail bool
	}{
		{
			name: "valid config",
			config: &Config{
				LogLevel: "info",
			},
			shouldFail: false,
		},
		{
			name: "invalid log level",
			config: &Config{
				LogLevel: "invalid",
			},
			shouldFail: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			service, err := NewService(tt.config)
			if tt.shouldFail {
				assert.Error(t, err)
				assert.Nil(t, service)
			} else {
				assert.NoError(t, err)
				assert.NotNil(t, service)
				assert.Equal(t, tt.config, service.config)
				assert.NotNil(t, service.logStore)
			}
		})
	}
}

func TestLoggingLevels(t *testing.T) {
	// Create a service with info level
	config := &Config{
		LogLevel: "info",
	}
	service, err := NewService(config)
	require.NoError(t, err)
	require.NotNil(t, service)

	// Test logging at different levels
	ctx := map[string]interface{}{
		"service": "test",
		"key":     "value",
	}

	// Debug should not be logged at info level
	err = service.LogDebug("debug message", ctx)
	assert.NoError(t, err)
	assert.Empty(t, service.logStore.indexByLevel["debug"])

	// Info should be logged
	err = service.LogInfo("info message", ctx)
	assert.NoError(t, err)
	assert.NotEmpty(t, service.logStore.indexByLevel["info"])

	// Warning should be logged
	err = service.LogWarning("warning message", ctx)
	assert.NoError(t, err)
	assert.NotEmpty(t, service.logStore.indexByLevel["warn"])

	// Error should be logged
	err = service.LogError("error message", ctx)
	assert.NoError(t, err)
	assert.NotEmpty(t, service.logStore.indexByLevel["error"])

	// Check total log count
	assert.Equal(t, 3, len(service.logStore.entries))
}

func TestQueryLogs(t *testing.T) {
	// Create a service
	config := &Config{
		LogLevel: "debug",
	}
	service, err := NewService(config)
	require.NoError(t, err)
	require.NotNil(t, service)

	// Add some logs
	ctx := map[string]interface{}{
		"service":    "test-service",
		"request_id": "123",
	}

	service.LogInfo("test message 1", ctx)
	service.LogWarning("test message 2", ctx)
	service.LogError("test message 3", ctx)

	// Test query with time range
	now := time.Now()
	startTime := now.Add(-time.Hour)
	endTime := now.Add(time.Hour)

	logs, err := service.QueryLogs(context.Background(), "service:test-service", startTime, endTime, 10)
	assert.NoError(t, err)
	assert.NotEmpty(t, logs)

	// Verify log properties
	for _, log := range logs {
		assert.Equal(t, "test-service", log.Service)
		assert.Contains(t, []string{"info", "warn", "error"}, log.Level)
		assert.NotEmpty(t, log.ID)
		assert.NotEmpty(t, log.Message)
		assert.NotNil(t, log.Context)
	}
}

func TestStartAndShutdown(t *testing.T) {
	// Create a service
	config := &Config{
		LogLevel: "info",
	}
	service, err := NewService(config)
	require.NoError(t, err)
	require.NotNil(t, service)

	// Start the service
	err = service.Start()
	assert.NoError(t, err)

	// Shutdown the service
	err = service.Shutdown(context.Background())
	assert.NoError(t, err)
}

func TestLogContextExtraction(t *testing.T) {
	// Test with valid context
	validCtx := map[string]interface{}{
		"service": "test-service",
	}
	service := getServiceFromContext(validCtx)
	assert.Equal(t, "test-service", service)

	// Test with nil context
	nilService := getServiceFromContext(nil)
	assert.Equal(t, "unknown", nilService)

	// Test with missing service
	missingCtx := map[string]interface{}{
		"other": "value",
	}
	missingService := getServiceFromContext(missingCtx)
	assert.Equal(t, "unknown", missingService)

	// Test with non-string service
	nonStringCtx := map[string]interface{}{
		"service": 123,
	}
	nonStringService := getServiceFromContext(nonStringCtx)
	assert.Equal(t, "unknown", nonStringService)
}
