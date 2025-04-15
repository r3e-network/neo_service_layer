package integration

import (
	"context"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/crypto/keys"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/r3e-network/neo_service_layer/internal/services/secrets"
	"github.com/stretchr/testify/require"
)

func TestFunctionsAndSecretsIntegration(t *testing.T) {
	ctx := context.Background()

	// Create a test account
	privateKey, err := keys.NewPrivateKey()
	require.NoError(t, err)
	account := wallet.NewAccountFromPrivateKey(privateKey)
	userAddress := util.Uint160(account.ScriptHash())

	// Initialize Functions service
	functionsConfig := &functions.Config{
		MaxFunctionSize:     1024 * 1024, // 1MB
		MaxExecutionTime:    5 * time.Second,
		MaxMemoryLimit:      128 * 1024 * 1024, // 128MB
		EnableNetworkAccess: false,
		EnableFileIO:        false,
		DefaultRuntime:      "javascript",
	}
	functionservice, err := functions.NewService(functionsConfig)
	require.NoError(t, err)

	// Initialize Secrets service
	secretsConfig := &secrets.Config{
		EncryptionKey:       "test-encryption-key",
		MaxSecretSize:       10 * 1024, // 10KB
		MaxSecretsPerUser:   100,
		SecretExpiryEnabled: true,
		DefaultTTL:          24 * time.Hour, // 24 hours
	}
	secretservice, err := secrets.NewService(secretsConfig)
	require.NoError(t, err)

	// Test creating a function
	functionCode := `
function main(args) {
    console.log("Hello from function!");
    return {
        message: "Hello, " + (args.name || "World"),
        timestamp: Date.now()
    };
}
`
	function, err := functionservice.CreateFunction(ctx, userAddress, "test-function", "A test function", functionCode, functions.JavaScriptRuntime)
	require.NoError(t, err)
	require.NotEmpty(t, function.ID)
	require.Equal(t, "test-function", function.Name)
	require.Equal(t, userAddress, function.Owner)

	// Test retrieving a function
	retrievedFunction, err := functionservice.GetFunction(ctx, function.ID)
	require.NoError(t, err)
	require.Equal(t, function.ID, retrievedFunction.ID)
	require.Equal(t, function.Name, retrievedFunction.Name)
	require.Equal(t, function.Code, retrievedFunction.Code)

	// Test updating function metadata
	updates := map[string]interface{}{
		"description": "Updated description",
		"metadata": map[string]interface{}{
			"category": "test",
			"version":  "1.0.1",
		},
	}
	updatedFunction, err := functionservice.UpdateFunction(ctx, function.ID, userAddress, updates)
	require.NoError(t, err)
	require.Equal(t, "Updated description", updatedFunction.Description)
	require.Equal(t, "test", updatedFunction.Metadata["category"])
	require.Equal(t, "1.0.1", updatedFunction.Metadata["version"])

	// Test storing a secret
	secretKey := "api-key"
	secretValue := "test-api-key-12345"
	err = secretservice.StoreSecret(ctx, userAddress, secretKey, secretValue, nil)
	require.NoError(t, err)

	// Test retrieving a secret
	retrievedSecret, err := secretservice.GetSecret(ctx, userAddress, secretKey)
	require.NoError(t, err)
	require.Equal(t, secretValue, retrievedSecret)

	// Test listing secrets
	secrets, err := secretservice.ListSecrets(ctx, userAddress)
	require.NoError(t, err)
	require.Contains(t, secrets, secretKey)

	// Test invoking a function
	invocation := functions.FunctionInvocation{
		FunctionID: function.ID,
		Parameters: map[string]interface{}{
			"name": "Integration Test",
		},
		Async:   false,
		Caller:  userAddress,
		TraceID: "test-trace-id",
	}
	execution, err := functionservice.InvokeFunction(ctx, invocation)
	require.NoError(t, err)
	require.NotEmpty(t, execution.ID)
	require.Equal(t, function.ID, execution.FunctionID)
	require.Equal(t, "completed", execution.Status)
	require.NotEmpty(t, execution.Logs)
	require.Contains(t, execution.Logs[0], "Hello from function!")

	// Verify function result
	result, ok := execution.Result.(map[string]interface{})
	require.True(t, ok)
	require.Equal(t, "Hello, Integration Test", result["message"])
	require.NotNil(t, result["timestamp"])

	// Test retrieving function execution
	retrievedExecution, err := functionservice.GetExecution(ctx, execution.ID)
	require.NoError(t, err)
	require.Equal(t, execution.ID, retrievedExecution.ID)
	require.Equal(t, execution.Status, retrievedExecution.Status)

	// Test listing functions
	functions, err := functionservice.ListFunctions(ctx, userAddress)
	require.NoError(t, err)
	require.Len(t, functions, 1)
	require.Equal(t, function.ID, functions[0].ID)

	// Test listing executions
	executions, err := functionservice.ListExecutions(ctx, function.ID, 10)
	require.NoError(t, err)
	require.Len(t, executions, 1)
	require.Equal(t, execution.ID, executions[0].ID)

	// Test function permissions
	permissions, err := functionservice.GetPermissions(ctx, function.ID)
	require.NoError(t, err)
	require.Equal(t, function.ID, permissions.FunctionID)
	require.Equal(t, userAddress, permissions.Owner)
	require.False(t, permissions.Public)

	// Update permissions to make function public
	permissions.Public = true
	err = functionservice.UpdatePermissions(ctx, function.ID, userAddress, permissions)
	require.NoError(t, err)

	// Verify permissions were updated
	updatedPermissions, err := functionservice.GetPermissions(ctx, function.ID)
	require.NoError(t, err)
	require.True(t, updatedPermissions.Public)

	// Test deleting a secret
	err = secretservice.DeleteSecret(ctx, userAddress, secretKey)
	require.NoError(t, err)

	// Verify secret was deleted
	_, err = secretservice.GetSecret(ctx, userAddress, secretKey)
	require.Error(t, err)
	require.Contains(t, err.Error(), "not found")

	// Test deleting a function
	err = functionservice.DeleteFunction(ctx, function.ID, userAddress)
	require.NoError(t, err)

	// Verify function was deleted
	_, err = functionservice.GetFunction(ctx, function.ID)
	require.Error(t, err)
	require.Contains(t, err.Error(), "not found")
}
