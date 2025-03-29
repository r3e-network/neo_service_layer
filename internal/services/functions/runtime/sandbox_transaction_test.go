package runtime

import (
	"context"
	"testing"

	"github.com/dop251/goja"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
	"go.uber.org/zap"
)

// MockTransactionService is a mock implementation of the transaction.Service interface
type MockTransactionService struct {
	mock.Mock
}

func (m *MockTransactionService) Create(config map[string]interface{}) (string, error) {
	args := m.Called(config)
	return args.String(0), args.Error(1)
}

func (m *MockTransactionService) Sign(id string, account *wallet.Account) (map[string]interface{}, error) {
	args := m.Called(id, account)
	return args.Get(0).(map[string]interface{}), args.Error(1)
}

func (m *MockTransactionService) Send(ctx context.Context, id string) (string, error) {
	args := m.Called(ctx, id)
	return args.String(0), args.Error(1)
}

func (m *MockTransactionService) Status(hash string) (string, error) {
	args := m.Called(hash)
	return args.String(0), args.Error(1)
}

func (m *MockTransactionService) Get(id string) (map[string]interface{}, error) {
	args := m.Called(id)
	return args.Get(0).(map[string]interface{}), args.Error(1)
}

func (m *MockTransactionService) List() ([]interface{}, error) {
	args := m.Called()
	return args.Get(0).([]interface{}), args.Error(1)
}

func (m *MockTransactionService) EstimateFee(config map[string]interface{}) (string, error) {
	args := m.Called(config)
	return args.String(0), args.Error(1)
}

func TestSandboxTransactionMethods(t *testing.T) {
	// Create a mock transaction service
	mockTxService := new(MockTransactionService)

	// Setup mock expectations
	mockTxService.On("Create", mock.Anything).Return("tx-123", nil)
	mockTxService.On("Send", mock.Anything, "tx-123").Return("0xabcdef123456789", nil)
	mockTxService.On("Status", "0xabcdef123456789").Return("pending", nil)
	mockTxService.On("Get", "tx-123").Return(map[string]interface{}{
		"id":      "tx-123",
		"hash":    "0xabcdef123456789",
		"status":  "pending",
		"type":    "transfer",
		"from":    "owner-123",
		"to":      "recipient-456",
		"amount":  "100",
		"asset":   "NEO",
		"network": "testnet",
	}, nil)
	mockTxService.On("List").Return([]interface{}{
		map[string]interface{}{
			"id":      "tx-123",
			"hash":    "0xabcdef123456789",
			"status":  "pending",
			"type":    "transfer",
			"from":    "owner-123",
			"to":      "recipient-456",
			"amount":  "100",
			"asset":   "NEO",
			"network": "testnet",
		},
	}, nil)
	mockTxService.On("EstimateFee", mock.Anything).Return("0.001", nil)
	mockTxService.On("Sign", "tx-123", mock.Anything).Return(map[string]interface{}{
		"id":      "tx-123",
		"signed":  true,
		"rawData": "0x123456789abcdef",
		"status":  "signed",
	}, nil)

	// Create a sandbox with the mock transaction service
	logger, _ := zap.NewDevelopment()
	sandbox := NewSandbox(SandboxConfig{
		Logger: logger,
	})

	// Create a function context with the mock transaction service
	functionContext := &FunctionContext{
		FunctionID: "function-123",
		Owner:      "owner-123",
		Services: &ServiceClients{
			Transaction: mockTxService,
		},
	}

	// Create a JavaScript runtime
	vm := goja.New()

	// Test transaction.create method
	t.Run("transaction.create", func(t *testing.T) {
		// Create a JavaScript function context
		jsContext := sandbox.createFunctionContext(functionContext, []string{})
		vm.Set("context", jsContext)

		// Execute JavaScript code to call transaction.create
		result, err := vm.RunString(`
			const txConfig = {
				type: "transfer",
				to: "recipient-456",
				amount: "100",
				asset: "NEO",
				network: "testnet"
			};
			context.transaction.create(txConfig);
		`)

		// Assert the result
		assert.NoError(t, err)
		assert.NotNil(t, result)
		resultObj := result.Export().(map[string]interface{})
		assert.Equal(t, true, resultObj["success"])
		assert.Equal(t, "tx-123", resultObj["txId"])
	})

	// Test transaction.sign method
	t.Run("transaction.sign", func(t *testing.T) {
		// Create a JavaScript function context
		jsContext := sandbox.createFunctionContext(functionContext, []string{})
		vm.Set("context", jsContext)

		// Execute JavaScript code to call transaction.sign
		result, err := vm.RunString(`
			context.transaction.sign("tx-123");
		`)

		// Assert the result
		assert.NoError(t, err)
		assert.NotNil(t, result)
		resultObj := result.Export().(map[string]interface{})
		assert.Equal(t, true, resultObj["success"])
		assert.Equal(t, "tx-123", resultObj["txId"])
		assert.Equal(t, "signed", resultObj["status"])
	})

	// Test transaction.send method
	t.Run("transaction.send", func(t *testing.T) {
		// Create a JavaScript function context
		jsContext := sandbox.createFunctionContext(functionContext, []string{})
		vm.Set("context", jsContext)

		// Execute JavaScript code to call transaction.send
		result, err := vm.RunString(`
			context.transaction.send("tx-123");
		`)

		// Assert the result
		assert.NoError(t, err)
		assert.NotNil(t, result)
		resultObj := result.Export().(map[string]interface{})
		assert.Equal(t, true, resultObj["success"])
		assert.Equal(t, "tx-123", resultObj["txId"])
		assert.Equal(t, "0xabcdef123456789", resultObj["hash"])
		assert.Equal(t, "sent", resultObj["status"])
	})

	// Test transaction.status method
	t.Run("transaction.status", func(t *testing.T) {
		// Create a JavaScript function context
		jsContext := sandbox.createFunctionContext(functionContext, []string{})
		vm.Set("context", jsContext)

		// Execute JavaScript code to call transaction.status
		result, err := vm.RunString(`
			context.transaction.status("0xabcdef123456789");
		`)

		// Assert the result
		assert.NoError(t, err)
		assert.NotNil(t, result)
		resultObj := result.Export().(map[string]interface{})
		assert.Equal(t, true, resultObj["success"])
		assert.Equal(t, "0xabcdef123456789", resultObj["txId"])
		assert.Equal(t, "pending", resultObj["status"])
	})

	// Test transaction.get method
	t.Run("transaction.get", func(t *testing.T) {
		// Create a JavaScript function context
		jsContext := sandbox.createFunctionContext(functionContext, []string{})
		vm.Set("context", jsContext)

		// Execute JavaScript code to call transaction.get
		result, err := vm.RunString(`
			context.transaction.get("tx-123");
		`)

		// Assert the result
		assert.NoError(t, err)
		assert.NotNil(t, result)
		resultObj := result.Export().(map[string]interface{})
		assert.Equal(t, true, resultObj["success"])
		txDetails := resultObj["transaction"].(map[string]interface{})
		assert.Equal(t, "tx-123", txDetails["id"])
		assert.Equal(t, "0xabcdef123456789", txDetails["hash"])
		assert.Equal(t, "pending", txDetails["status"])
	})

	// Test transaction.list method
	t.Run("transaction.list", func(t *testing.T) {
		// Create a JavaScript function context
		jsContext := sandbox.createFunctionContext(functionContext, []string{})
		vm.Set("context", jsContext)

		// Execute JavaScript code to call transaction.list
		result, err := vm.RunString(`
			context.transaction.list({});
		`)

		// Assert the result
		assert.NoError(t, err)
		assert.NotNil(t, result)
		resultObj := result.Export().(map[string]interface{})
		assert.Equal(t, true, resultObj["success"])
		txList := resultObj["transactions"].([]interface{})
		assert.Equal(t, 1, len(txList))
		tx := txList[0].(map[string]interface{})
		assert.Equal(t, "tx-123", tx["id"])
	})

	// Test transaction.estimateFee method
	t.Run("transaction.estimateFee", func(t *testing.T) {
		// Create a JavaScript function context
		jsContext := sandbox.createFunctionContext(functionContext, []string{})
		vm.Set("context", jsContext)

		// Execute JavaScript code to call transaction.estimateFee
		result, err := vm.RunString(`
			const feeConfig = {
				type: "transfer",
				to: "recipient-456",
				amount: "100",
				asset: "NEO",
				network: "testnet"
			};
			context.transaction.estimateFee(feeConfig);
		`)

		// Assert the result
		assert.NoError(t, err)
		assert.NotNil(t, result)
		resultObj := result.Export().(map[string]interface{})
		assert.Equal(t, true, resultObj["success"])
		assert.Equal(t, "0.001", resultObj["fee"])
		assert.Equal(t, "GAS", resultObj["asset"])
	})
}
