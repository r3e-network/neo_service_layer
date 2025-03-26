package integration

import (
	"context"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"github.com/will/neo_service_layer/internal/services/logging"
)

func TestLoggingServiceIntegration(t *testing.T) {
	// Create a new logging service with custom configuration
	config := &logging.Config{
		LogLevel:          "debug",
		EnableJSONLogs:    true,
		LogFilePath:       "test_logs.log",
		MaxSizeInMB:       10,
		RetainedFiles:     3,
		EnableCompression: true,
	}

	loggingService, err := logging.NewService(config)
	assert.NoError(t, err, "Creating logging service should not return an error")
	assert.NotNil(t, loggingService, "Logging service should not be nil")

	// Start the service
	err = loggingService.Start()
	assert.NoError(t, err, "Starting logging service should not return an error")

	// Log various message types
	apiContext := map[string]interface{}{
		"service":     "api",
		"request_id":  "req-123",
		"user_id":     "user-456",
		"endpoint":    "/api/v1/function",
		"method":      "POST",
		"status_code": 200,
	}

	err = loggingService.LogDebug("Processing API request", apiContext)
	assert.NoError(t, err, "Logging debug message should not return an error")

	err = loggingService.LogInfo("API request completed", apiContext)
	assert.NoError(t, err, "Logging info message should not return an error")

	err = loggingService.LogWarning("Rate limit approaching", apiContext)
	assert.NoError(t, err, "Logging warning message should not return an error")

	err = loggingService.LogError("Database connection failed", apiContext)
	assert.NoError(t, err, "Logging error message should not return an error")

	// Log from a different service
	functionContext := map[string]interface{}{
		"service":        "functions",
		"request_id":     "req-789",
		"user_id":        "user-456",
		"function_id":    "func-123",
		"function_name":  "calculate_gas",
		"execution_time": 150,
	}

	err = loggingService.LogInfo("Function executed successfully", functionContext)
	assert.NoError(t, err, "Logging info message should not return an error")

	// Query logs
	ctx := context.Background()
	startTime := time.Now().Add(-1 * time.Hour)
	endTime := time.Now().Add(1 * time.Hour)

	// Query all API logs
	logs, err := loggingService.QueryLogs(ctx, "service:api", startTime, endTime, 10)
	assert.NoError(t, err, "Querying logs should not return an error")
	assert.NotEmpty(t, logs, "Should have found API logs")

	// Verify log contents
	for _, log := range logs {
		assert.Equal(t, "api", log.Service, "Log should be from API service")
		serviceContext, ok := log.Context["service"]
		assert.True(t, ok, "Log context should contain service field")
		assert.Equal(t, "api", serviceContext, "Service in context should be api")
	}

	// Query all function logs
	functionLogs, err := loggingService.QueryLogs(ctx, "service:functions", startTime, endTime, 10)
	assert.NoError(t, err, "Querying logs should not return an error")
	assert.NotEmpty(t, functionLogs, "Should have found function logs")

	// Verify function log contents
	for _, log := range functionLogs {
		assert.Equal(t, "functions", log.Service, "Log should be from functions service")
		serviceContext, ok := log.Context["service"]
		assert.True(t, ok, "Log context should contain service field")
		assert.Equal(t, "functions", serviceContext, "Service in context should be functions")
	}

	// Graceful shutdown
	err = loggingService.Shutdown(ctx)
	assert.NoError(t, err, "Shutting down logging service should not return an error")

	t.Log("Logging Service integration test completed successfully")
}
