package functions

import (
	"context"
	"testing"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

// TestComprehensiveMockScenario tests a comprehensive real-world scenario
// with multiple users, functions, and interactions
func TestComprehensiveMockScenario(t *testing.T) {
	// Create a mock service
	mockService := NewMockService()
	ctx := context.Background()

	// Create test users
	owner1, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")
	require.NoError(t, err)

	owner2, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000002")
	require.NoError(t, err)

	collaborator, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000003")
	require.NoError(t, err)

	// Simulate Owner1 creating two functions
	temperatureFunction, err := mockService.CreateFunction(
		ctx,
		owner1,
		"temperature-converter",
		"Converts between Celsius and Fahrenheit",
		`function main(params) {
			if (params.unit === 'C') {
				return (params.temp * 9/5) + 32;
			} else {
				return (params.temp - 32) * 5/9;
			}
		}`,
		JavaScriptRuntime,
	)
	require.NoError(t, err)

	calculatorFunction, err := mockService.CreateFunction(
		ctx,
		owner1,
		"calculator",
		"Basic calculator operations",
		`function main(params) {
			switch(params.operation) {
				case 'add': return params.a + params.b;
				case 'subtract': return params.a - params.b;
				case 'multiply': return params.a * params.b;
				case 'divide': return params.a / params.b;
				default: return 'Invalid operation';
			}
		}`,
		JavaScriptRuntime,
	)
	require.NoError(t, err)

	// Simulate Owner2 creating a function
	greetingFunction, err := mockService.CreateFunction(
		ctx,
		owner2,
		"greeting",
		"Returns a greeting message",
		`function main(params) {
			return 'Hello, ' + (params.name || 'World') + '!';
		}`,
		JavaScriptRuntime,
	)
	require.NoError(t, err)

	// 1. Owner1 shares temperature function with Collaborator
	tempPermissions := &FunctionPermissions{
		FunctionID:   temperatureFunction.ID,
		Owner:        owner1,
		AllowedUsers: []util.Uint160{collaborator},
		Public:       false,
		ReadOnly:     false,
	}

	err = mockService.UpdatePermissions(ctx, temperatureFunction.ID, owner1, tempPermissions)
	assert.NoError(t, err)

	// 2. Owner1 makes calculator function public
	calcPermissions := &FunctionPermissions{
		FunctionID:   calculatorFunction.ID,
		Owner:        owner1,
		AllowedUsers: []util.Uint160{},
		Public:       true,
		ReadOnly:     true,
	}

	err = mockService.UpdatePermissions(ctx, calculatorFunction.ID, owner1, calcPermissions)
	assert.NoError(t, err)

	// 3. Invoke temperature function as Owner1
	tempInvocation := FunctionInvocation{
		FunctionID: temperatureFunction.ID,
		Parameters: map[string]interface{}{
			"unit": "C",
			"temp": 25,
		},
		Caller: owner1,
	}

	tempExecution, err := mockService.InvokeFunction(ctx, tempInvocation)
	assert.NoError(t, err)
	assert.NotNil(t, tempExecution)

	// 4. Invoke temperature function as Collaborator
	tempCollabInvocation := FunctionInvocation{
		FunctionID: temperatureFunction.ID,
		Parameters: map[string]interface{}{
			"unit": "F",
			"temp": 70,
		},
		Caller: collaborator,
	}

	tempCollabExecution, err := mockService.InvokeFunction(ctx, tempCollabInvocation)
	assert.NoError(t, err)
	assert.NotNil(t, tempCollabExecution)

	// 5. Invoke calculator function as Owner2 (public function)
	calcInvocation := FunctionInvocation{
		FunctionID: calculatorFunction.ID,
		Parameters: map[string]interface{}{
			"operation": "add",
			"a":         10,
			"b":         20,
		},
		Caller: owner2,
	}

	calcExecution, err := mockService.InvokeFunction(ctx, calcInvocation)
	assert.NoError(t, err)
	assert.NotNil(t, calcExecution)

	// 6. Try to invoke greeting function as Owner1 (should fail - not shared)
	greetingInvocation := FunctionInvocation{
		FunctionID: greetingFunction.ID,
		Parameters: map[string]interface{}{
			"name": "Test",
		},
		Caller: owner1,
	}

	_, err = mockService.InvokeFunction(ctx, greetingInvocation)
	assert.Error(t, err)
	assert.Contains(t, err.Error(), "permission denied")

	// 7. Update greeting function to be public
	greetingPermissions := &FunctionPermissions{
		FunctionID:   greetingFunction.ID,
		Owner:        owner2,
		AllowedUsers: []util.Uint160{},
		Public:       true,
		ReadOnly:     false,
	}

	err = mockService.UpdatePermissions(ctx, greetingFunction.ID, owner2, greetingPermissions)
	assert.NoError(t, err)

	// 8. Now Owner1 should be able to invoke the greeting function
	_, err = mockService.InvokeFunction(ctx, greetingInvocation)
	assert.NoError(t, err)

	// 9. List functions for each owner
	owner1Functions, err := mockService.ListFunctions(ctx, owner1)
	assert.NoError(t, err)
	assert.Len(t, owner1Functions, 2)

	owner2Functions, err := mockService.ListFunctions(ctx, owner2)
	assert.NoError(t, err)
	assert.Len(t, owner2Functions, 1)

	// 10. Verify collaborator can't update read-only calculator function
	calcUpdateInvocation := map[string]interface{}{
		"code": "function main() { return 'hacked'; }",
	}

	_, err = mockService.UpdateFunction(ctx, calculatorFunction.ID, collaborator, calcUpdateInvocation)
	assert.Error(t, err)

	// 11. Owner1 deletes temperature function
	err = mockService.DeleteFunction(ctx, temperatureFunction.ID, owner1)
	assert.NoError(t, err)

	// Verify it's deleted
	_, err = mockService.GetFunction(ctx, temperatureFunction.ID)
	assert.Error(t, err)

	// 12. List executions for calculator function
	calcExecutions, err := mockService.ListExecutions(ctx, calculatorFunction.ID, 10)
	assert.NoError(t, err)
	assert.Len(t, calcExecutions, 1)

	// 13. Verify Owner2 can't delete Owner1's function
	err = mockService.DeleteFunction(ctx, calculatorFunction.ID, owner2)
	assert.Error(t, err)
	assert.Contains(t, err.Error(), "permission denied")
}

// TestMockErrorHandling tests error handling in the mock service
func TestMockErrorHandling(t *testing.T) {
	mockService := NewMockService()
	ctx := context.Background()

	// Test users
	owner, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")
	require.NoError(t, err)

	nonOwner, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000002")
	require.NoError(t, err)

	// 1. Test non-existent function
	_, err = mockService.GetFunction(ctx, "non-existent-function")
	assert.Error(t, err)
	assert.Equal(t, ErrFunctionNotFound, err)

	// 2. Create a function to test with
	function, err := mockService.CreateFunction(
		ctx,
		owner,
		"test-function",
		"Test function description",
		"function main() { return 'test'; }",
		JavaScriptRuntime,
	)
	require.NoError(t, err)

	// 3. Test permission denied for update
	updates := map[string]interface{}{
		"description": "Updated by non-owner",
	}

	_, err = mockService.UpdateFunction(ctx, function.ID, nonOwner, updates)
	assert.Error(t, err)
	assert.Equal(t, ErrPermissionDenied, err)

	// 4. Test permission denied for delete
	err = mockService.DeleteFunction(ctx, function.ID, nonOwner)
	assert.Error(t, err)
	assert.Equal(t, ErrPermissionDenied, err)

	// 5. Test permission denied for invoke
	invocation := FunctionInvocation{
		FunctionID: function.ID,
		Parameters: map[string]interface{}{},
		Caller:     nonOwner,
	}

	_, err = mockService.InvokeFunction(ctx, invocation)
	assert.Error(t, err)
	assert.Equal(t, ErrPermissionDenied, err)

	// 6. Test permission denied for updating permissions
	permissions := &FunctionPermissions{
		FunctionID:   function.ID,
		Owner:        owner,
		AllowedUsers: []util.Uint160{nonOwner},
		Public:       true,
		ReadOnly:     false,
	}

	err = mockService.UpdatePermissions(ctx, function.ID, nonOwner, permissions)
	assert.Error(t, err)
	assert.Equal(t, ErrPermissionDenied, err)

	// 7. Make function public and test again
	err = mockService.UpdatePermissions(ctx, function.ID, owner, &FunctionPermissions{
		FunctionID:   function.ID,
		Owner:        owner,
		AllowedUsers: []util.Uint160{},
		Public:       true,
		ReadOnly:     true,
	})
	assert.NoError(t, err)

	// 8. Now non-owner should be able to invoke but not update
	_, err = mockService.InvokeFunction(ctx, invocation)
	assert.NoError(t, err)

	_, err = mockService.UpdateFunction(ctx, function.ID, nonOwner, updates)
	assert.Error(t, err)
	assert.Contains(t, err.Error(), "read-only")
}
