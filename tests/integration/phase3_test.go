package integration

import (
	"context"
	"math/big"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/r3e-network/neo_service_layer/internal/core/neo"
	"github.com/r3e-network/neo_service_layer/internal/services/automation"
	"github.com/r3e-network/neo_service_layer/internal/services/functions"
	"github.com/r3e-network/neo_service_layer/internal/services/gasbank"
	"github.com/r3e-network/neo_service_layer/internal/services/secrets"
	"github.com/r3e-network/neo_service_layer/internal/services/trigger"
	"github.com/r3e-network/neo_service_layer/internal/triggerservice/models"
	"github.com/stretchr/testify/require"
)

// TestPhase3Integration tests the integration between gasbank, trigger, functions,
// secrets, and automation services.
func TestPhase3Integration(t *testing.T) {
	ctx := context.Background()

	// Initialize a mock Neo client
	neoClient := &neo.Client{}

	// Create a test account
	account, err := wallet.NewAccount()
	require.NoError(t, err)
	userAddress := util.Uint160(account.ScriptHash())

	// Test setup - initialize GasBank service
	gasBankConfig := &gasbank.Config{
		InitialGas:              big.NewInt(1000000),
		MaxAllocationPerUser:    big.NewInt(100000),
		StoreType:               "memory",
		ExpirationCheckInterval: 15 * time.Minute,
		MonitorInterval:         5 * time.Minute,
		CooldownPeriod:          5 * time.Minute,
		RefillAmount:            big.NewInt(500000),
		RefillThreshold:         big.NewInt(100000),
		MinAllocationAmount:     big.NewInt(100),
		MaxAllocationTime:       time.Hour * 24,
	}
	gasBankService, err := gasbank.NewService(ctx, gasBankConfig)
	require.NoError(t, err)

	// Phase 3 adds new services: Functions, Secrets, and Automation

	// Initialize Functions service with correct config structure
	functionsConfig := &functions.Config{
		// Use only fields that exist in the actual Config struct
		MaxExecutionTime: time.Second * 30,
	}
	functionservice, err := functions.NewService(functionsConfig)
	require.NoError(t, err)

	// Initialize Secrets service with correct config structure
	secretsConfig := &secrets.Config{
		// Use only fields that exist in the actual Config struct
		EncryptionKey: "test-encryption-key-12345",
	}
	secretservice, err := secrets.NewService(secretsConfig)
	require.NoError(t, err)

	// Initialize Contract Automation service
	automationConfig := &automation.Config{
		CheckInterval:  time.Minute * 5,
		RetryAttempts:  3,
		RetryDelay:     time.Second * 15,
		GasBuffer:      big.NewInt(10000),
		KeeperRegistry: util.Uint160{4, 5, 6}, // Example registry contract
	}
	automationService, err := automation.NewService(automationConfig, neoClient, gasBankService)
	require.NoError(t, err)

	// Test phase 3 services separately

	// 1. Test function creation and execution using the actual function signature
	functionName := "TestFunction"
	functionDescription := "A test function"
	functionCode := `function main(args) { return { success: true }; }`
	functionRuntime := functions.JavaScriptRuntime // Using the correct runtime constant

	createdFunction, err := functionservice.CreateFunction(ctx, userAddress, functionName, functionDescription, functionCode, functionRuntime)
	require.NoError(t, err)
	require.NotNil(t, createdFunction)

	// 2. Test secret storage and retrieval
	secretName := "test_api_key"
	secretData := "test_secret_value"
	err = secretservice.StoreSecret(ctx, userAddress, secretName, secretData, nil)
	require.NoError(t, err)

	retrievedSecret, err := secretservice.GetSecret(ctx, userAddress, secretName)
	require.NoError(t, err)
	require.NotNil(t, retrievedSecret)
	require.Equal(t, secretData, retrievedSecret)

	// 3. Test contract automation
	upkeep := &automation.Upkeep{
		Name:           "Test Upkeep",
		TargetContract: util.Uint160{7, 8, 9},
		ExecuteGas:     50000,
		UpkeepFunction: "update",
	}
	registered, err := automationService.RegisterUpkeep(ctx, userAddress, upkeep)
	require.NoError(t, err)
	require.True(t, registered)

	// Wait for the upkeep to be registered and verify it exists
	time.Sleep(time.Second)
	retrievedUpkeep, err := automationService.GetUpkeep(ctx, upkeep.ID)
	require.NoError(t, err)
	require.NotNil(t, retrievedUpkeep)
	require.Equal(t, upkeep.ID, retrievedUpkeep.ID)

	// Test that we can perform the upkeep
	performResult, err := automationService.PerformUpkeep(ctx, upkeep.ID, nil)
	require.NoError(t, err)
	require.NotNil(t, performResult)
	require.Equal(t, "success", performResult.Status)

	// 4. Test integration between phase 3 and phase 2
	// Create a trigger that will perform an upkeep
	triggerConfig := &trigger.ServiceConfig{
		MaxTriggers:     10,
		MaxExecutions:   100,
		ExecutionWindow: time.Hour * 24,
	}
	triggerService, err := trigger.NewService(triggerConfig, neoClient)
	require.NoError(t, err)

	autoTrigger := &models.Trigger{
		Name:        "Automated Contract Call",
		Description: "Automatically calls a contract when conditions are met",
		UserAddress: userAddress,
		Condition:   "time > 0",
		Function:    "performUpkeep",
		Parameters: map[string]interface{}{
			"upkeepID": upkeep.ID,
		},
		Schedule: "0 0 * * * *", // Daily at midnight
		Status:   "active",
	}

	createdTrigger, err := triggerService.CreateTrigger(ctx, userAddress, autoTrigger)
	require.NoError(t, err)
	require.NotNil(t, createdTrigger)
}
