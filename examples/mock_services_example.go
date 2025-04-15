// Example demonstrating how to use the mock NeoClient and WalletService
package main

import (
	"context"
	"fmt"
	"log"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/smartcontract"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/core/neo"
	trigger "github.com/r3e-network/neo_service_layer/internal/triggerservice"
	wallet "github.com/r3e-network/neo_service_layer/internal/walletservice"
)

func main() {
	// Create mock services
	mockNeoClient, mockWalletService := createMockServices()

	// Configure function service (simplified for example)
	functionService := createSimpleFunctionService()

	// Configure trigger service
	triggerConfig := &trigger.ServiceConfig{
		MaxTriggers:           100,
		MaxExecutions:         1000,
		ExecutionWindow:       time.Hour * 24,
		MaxConcurrentTriggers: 10,
	}

	// Create trigger service with mock dependencies
	triggerService, err := trigger.NewService(
		triggerConfig,
		mockNeoClient,
		functionService,
		mockWalletService,
	)
	if err != nil {
		log.Fatalf("Failed to create trigger service: %v", err)
	}

	// Start the trigger service
	if err := triggerService.Start(context.Background()); err != nil {
		log.Fatalf("Failed to start trigger service: %v", err)
	}
	defer triggerService.Stop(context.Background())

	// Example of using the NeoClient directly
	exampleNeoClientUsage(mockNeoClient)

	// Example of creating and executing a trigger
	exampleTriggerUsage(triggerService)

	fmt.Println("Example completed successfully!")
}

// Create mock clients for development
func createMockServices() (*neo.MockNeoClient, *wallet.MockWalletService) {
	// Create the mock NeoClient (with default successful responses)
	mockNeoClient := neo.NewMockNeoClient()

	// Create the mock WalletService
	mockWalletService := wallet.NewMockWalletService()

	return mockNeoClient, mockWalletService
}

// Simple function service for demonstration
func createSimpleFunctionService() functions.IService {
	// This would be a real function service in production
	// For this example, we'll return a simple mock
	return &mockFunctionService{}
}

type mockFunctionService struct{}

func (m *mockFunctionService) InvokeFunction(ctx context.Context, invocation functions.FunctionInvocation) (*functions.FunctionResult, error) {
	return &functions.FunctionResult{
		ID:        "mock-result-id",
		Status:    "completed",
		Result:    "Mock function executed successfully",
		StartTime: time.Now().Add(-1 * time.Second),
		EndTime:   time.Now(),
	}, nil
}

// Remaining function service methods would be implemented here...

// Example of using the NeoClient directly
func exampleNeoClientUsage(client neo.NeoClient) {
	fmt.Println("--- NeoClient Example ---")

	// Example contract hash (would be a real contract in production)
	contractHash, _ := util.Uint160DecodeStringLE("0x1234567890abcdef1234567890abcdef12345678")

	// Example parameters
	params := []smartcontract.Parameter{
		{Type: smartcontract.StringType, Value: "hello"},
	}

	// Invoke a contract function
	result, err := client.InvokeFunction(
		contractHash,
		"someMethod",
		params,
		nil,
	)
	if err != nil {
		fmt.Printf("Mock invoke failed: %v\n", err)
	} else {
		fmt.Printf("Mock invoke result: %v\n", result)
	}

	// Get block count
	blockCount, err := client.GetBlockCount()
	if err != nil {
		fmt.Printf("Failed to get block count: %v\n", err)
	} else {
		fmt.Printf("Current block count: %d\n", blockCount)
	}

	fmt.Println()
}

// Example of creating and executing a trigger
func exampleTriggerUsage(triggerService *trigger.Service) {
	fmt.Println("--- Trigger Service Example ---")

	// Mock user address
	userAddress, _ := util.Uint160DecodeStringLE("0xabcdef1234567890abcdef1234567890abcdef12")

	// Example contract hash for the trigger
	contractHash, _ := util.Uint160DecodeStringLE("0x1234567890abcdef1234567890abcdef12345678")

	// Create a function action trigger (would be created from API in production)
	ctx := context.Background()
	functionTrigger := &trigger.Trigger{
		Name:        "Example Function Trigger",
		Type:        "schedule",
		Action:      "function",
		Schedule:    "0 * * * *", // Every hour
		Description: "An example function trigger created in the mock services example",
		FunctionParams: map[string]interface{}{
			"param1": "value1",
			"param2": 42,
		},
		TargetFunctionID: "example-function-id",
	}

	// Create the trigger
	createdTrigger, err := triggerService.CreateTrigger(ctx, userAddress, functionTrigger)
	if err != nil {
		fmt.Printf("Failed to create function trigger: %v\n", err)
	} else {
		fmt.Printf("Created function trigger with ID: %s\n", createdTrigger.ID)
	}

	// Create a contract action trigger
	contractTrigger := &trigger.Trigger{
		Name:           "Example Contract Trigger",
		Type:           "schedule",
		Action:         "contract",
		Schedule:       "30 * * * *", // 30 minutes past every hour
		Description:    "An example contract trigger created in the mock services example",
		TargetContract: contractHash,
		TargetMethod:   "transfer",
		Signer:         userAddress, // Using the same address as signer
		ContractParams: []interface{}{
			userAddress.StringLE(),                       // from
			"0xabababababababababababababababababababab", // to
			10, // amount
		},
	}

	// Create the trigger
	createdContractTrigger, err := triggerService.CreateTrigger(ctx, userAddress, contractTrigger)
	if err != nil {
		fmt.Printf("Failed to create contract trigger: %v\n", err)
	} else {
		fmt.Printf("Created contract trigger with ID: %s\n", createdContractTrigger.ID)

		// Execute the trigger manually (would normally be triggered by scheduler)
		execution, err := triggerService.ExecuteTrigger(ctx, userAddress, createdContractTrigger.ID)
		if err != nil {
			fmt.Printf("Failed to execute trigger: %v\n", err)
		} else {
			fmt.Printf("Trigger execution completed with status: %s\n", execution.Status)
			fmt.Printf("Result: %v\n", execution.Result)
		}
	}

	fmt.Println()
}
