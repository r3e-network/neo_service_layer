package integration

import (
	"context"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/crypto/keys"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/r3e-network/neo_service_layer/internal/services/functions"
	"github.com/stretchr/testify/require"
)

func TestSimpleFunctions(t *testing.T) {
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

	// Test creating a function
	functionCode := `
function main(args) {
    return { message: "Hello, World" };
}
`
	function, err := functionservice.CreateFunction(ctx, userAddress, "test-function", "A test function", functionCode, functions.JavaScriptRuntime)
	require.NoError(t, err)
	require.NotEmpty(t, function.ID)
	require.Equal(t, "test-function", function.Name)

	// Test getting function permissions
	permissions, err := functionservice.GetPermissions(ctx, function.ID)
	require.NoError(t, err)
	require.Equal(t, function.ID, permissions.FunctionID)
	require.Equal(t, userAddress, permissions.Owner)
	require.False(t, permissions.Public)

	// Test updating permissions
	permissions.Public = true
	err = functionservice.UpdatePermissions(ctx, function.ID, userAddress, permissions)
	require.NoError(t, err)

	// Verify permissions were updated
	updatedPermissions, err := functionservice.GetPermissions(ctx, function.ID)
	require.NoError(t, err)
	require.True(t, updatedPermissions.Public)
}
