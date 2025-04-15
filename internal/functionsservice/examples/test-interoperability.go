package main

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

func main() {
	// Create a test configuration with interoperability enabled
	config := &functions.Config{
		MaxFunctionSize:        1024 * 1024,
		MaxExecutionTime:       5 * time.Second,
		MaxMemoryLimit:         128 * 1024 * 1024,
		EnableNetworkAccess:    true,
		EnableFileIO:           false,
		DefaultRuntime:         "javascript",
		ServiceLayerURL:        "http://localhost:3000",
		EnableInteroperability: true,
	}

	// Create the function service
	service, err := functions.NewService(config)
	if err != nil {
		log.Fatalf("Failed to create function service: %v", err)
	}

	// Create a test owner address
	ownerHex := "0000000000000000000000000000000000000001"
	owner, err := util.Uint160DecodeStringLE(ownerHex)
	if err != nil {
		log.Fatalf("Failed to create owner address: %v", err)
	}

	// Read the example function code
	functionCode := `
/**
 * Simple test function for JavaScript interoperability
 */
function main(args) {
  // Log function start
  context.log('Function started');
  
  // Access function context properties
  const functionId = context.functionId;
  const executionId = context.executionId;
  
  // Get a test secret
  const testSecret = context.getSecret('test-key');
  
  // Get current price
  const neoPrice = context.getPrice('NEO');
  
  // Return result
  return {
    success: true,
    functionId,
    executionId,
    testSecret,
    neoPrice,
    timestamp: new Date().toISOString()
  };
}
`

	// Create a test function
	function, err := service.CreateFunction(
		context.Background(),
		owner,
		"test-interoperability",
		"Test function for JavaScript interoperability",
		functionCode,
		functions.JavaScriptRuntime,
	)
	if err != nil {
		log.Fatalf("Failed to create function: %v", err)
	}

	fmt.Printf("Created function: %s\n", function.ID)

	// Create a test caller address
	callerHex := "0000000000000000000000000000000000000002"
	caller, err := util.Uint160DecodeStringLE(callerHex)
	if err != nil {
		log.Fatalf("Failed to create caller address: %v", err)
	}

	// Create function permissions
	permissions := &functions.FunctionPermissions{
		FunctionID:   function.ID,
		Owner:        owner,
		AllowedUsers: []util.Uint160{caller},
		Public:       true,
		ReadOnly:     false,
	}

	// Update permissions
	err = service.UpdatePermissions(context.Background(), function.ID, owner, permissions)
	if err != nil {
		log.Fatalf("Failed to update permissions: %v", err)
	}

	// Invoke the function
	invocation := functions.FunctionInvocation{
		FunctionID: function.ID,
		Parameters: map[string]interface{}{
			"testParam": "testValue",
		},
		Async:   false,
		Caller:  caller,
		TraceID: "test-trace-id",
	}

	execution, err := service.InvokeFunction(context.Background(), invocation)
	if err != nil {
		log.Fatalf("Failed to invoke function: %v", err)
	}

	// Wait for execution to complete
	for execution.Status == "running" {
		time.Sleep(100 * time.Millisecond)
		execution, err = service.GetExecution(context.Background(), execution.ID)
		if err != nil {
			log.Fatalf("Failed to get execution: %v", err)
		}
	}

	// Print execution result
	fmt.Printf("Execution ID: %s\n", execution.ID)
	fmt.Printf("Status: %s\n", execution.Status)
	fmt.Printf("Duration: %d ms\n", execution.Duration)
	fmt.Printf("Memory used: %d bytes\n", execution.MemoryUsed)

	if execution.Status == "completed" {
		resultJSON, _ := json.MarshalIndent(execution.Result, "", "  ")
		fmt.Printf("Result: %s\n", string(resultJSON))
	} else {
		fmt.Printf("Error: %s\n", execution.Error)
	}

	// Print logs
	fmt.Println("Logs:")
	for _, log := range execution.Logs {
		fmt.Printf("  %s\n", log)
	}
}
