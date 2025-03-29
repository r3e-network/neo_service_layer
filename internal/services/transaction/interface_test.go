package transaction

import (
	"context"
	"testing"

	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
	"github.com/will/neo_service_layer/internal/core/neo"
	"go.uber.org/zap"
)

// TestServiceInterface verifies that the transaction service interface is properly defined
// and that implementations correctly implement the interface.
func TestServiceInterface(t *testing.T) {
	// Create a logger for testing
	logger, _ := zap.NewDevelopment()

	// Create a mock Neo client
	mockClient := &neo.Client{}

	// Create a transaction service with default config
	config := DefaultConfig()
	service := NewService(config, logger, mockClient)

	// Verify that ServiceImpl implements the Service interface
	var _ Service = (*ServiceImpl)(nil)

	// Test each interface method to ensure it's properly defined
	t.Run("Create method", func(t *testing.T) {
		txConfig := map[string]interface{}{
			"type":    "transfer",
			"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
			"amount":  "1.0",
			"asset":   "NEO",
			"network": "testnet",
		}
		txID, err := service.Create(txConfig)
		assert.NoError(t, err)
		assert.NotEmpty(t, txID)
	})

	t.Run("Sign method", func(t *testing.T) {
		// This test just verifies the method signature, not the actual signing
		// Create a transaction first
		txConfig := map[string]interface{}{
			"type":    "transfer",
			"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
			"amount":  "1.0",
			"asset":   "NEO",
			"network": "testnet",
		}
		txID, _ := service.Create(txConfig)

		// Verify the Sign method exists and has the correct signature
		account := &wallet.Account{}
		_, err := service.Sign(txID, account)
		// We expect an error since we're using a mock account
		assert.Error(t, err)
	})

	t.Run("Send method", func(t *testing.T) {
		// This test just verifies the method signature, not the actual sending
		// Create a transaction first
		txConfig := map[string]interface{}{
			"type":    "transfer",
			"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
			"amount":  "1.0",
			"asset":   "NEO",
			"network": "testnet",
		}
		txID, _ := service.Create(txConfig)

		// Verify the Send method exists and has the correct signature
		ctx := context.Background()
		_, err := service.Send(ctx, txID)
		// We expect an error since the transaction is not signed
		assert.Error(t, err)
		assert.Equal(t, ErrTransactionNotSigned, err)
	})

	t.Run("Status method", func(t *testing.T) {
		// This test just verifies the method signature, not the actual status retrieval
		_, err := service.Status("test-hash")
		// We expect an error since the hash doesn't exist
		assert.Error(t, err)
		assert.Equal(t, ErrTransactionNotFound, err)
	})

	t.Run("Get method", func(t *testing.T) {
		// Create a transaction first
		txConfig := map[string]interface{}{
			"type":    "transfer",
			"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
			"amount":  "1.0",
			"asset":   "NEO",
			"network": "testnet",
		}
		txID, _ := service.Create(txConfig)

		// Verify the Get method exists and has the correct signature
		txDetails, err := service.Get(txID)
		assert.NoError(t, err)
		assert.NotEmpty(t, txDetails)
		assert.Equal(t, txID, txDetails["id"])
	})

	t.Run("List method", func(t *testing.T) {
		// Verify the List method exists and has the correct signature
		txList, err := service.List()
		assert.NoError(t, err)
		assert.NotNil(t, txList)
	})

	t.Run("EstimateFee method", func(t *testing.T) {
		// Verify the EstimateFee method exists and has the correct signature
		txConfig := map[string]interface{}{
			"type":    "transfer",
			"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
			"amount":  "1.0",
			"asset":   "NEO",
			"network": "testnet",
		}
		fee, err := service.EstimateFee(txConfig)
		assert.NoError(t, err)
		assert.NotEmpty(t, fee)
	})
}

// TestMockServiceInterface verifies that the mock transaction service
// correctly implements the Service interface.
func TestMockServiceInterface(t *testing.T) {
	// Create a mock transaction service
	mockService := new(MockService)

	// Verify that MockService implements the Service interface
	var _ Service = mockService

	// Setup mock expectations
	txConfig := map[string]interface{}{
		"type":    "transfer",
		"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
		"amount":  "1.0",
		"asset":   "NEO",
		"network": "testnet",
	}
	
	mockService.On("Create", txConfig).Return("mock-tx-id", nil)
	
	mockService.On("Sign", "mock-tx-id", (*wallet.Account)(nil)).Return(
		map[string]interface{}{
			"id":      "mock-tx-id",
			"signed":  true,
			"status":  "signed",
			"rawData": "0x123456789abcdef",
		}, nil)
	
	mockService.On("Send", context.Background(), "mock-tx-id").Return("mock-hash", nil)
	
	mockService.On("Status", "mock-hash").Return("pending", nil)
	
	mockService.On("Get", "mock-tx-id").Return(
		map[string]interface{}{
			"id":      "mock-tx-id",
			"hash":    "mock-hash",
			"status":  "pending",
			"type":    "transfer",
			"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
			"amount":  "1.0",
			"asset":   "NEO",
			"network": "testnet",
		}, nil)
	
	mockService.On("List").Return(
		[]interface{}{
			map[string]interface{}{
				"id":      "mock-tx-id",
				"hash":    "mock-hash",
				"status":  "pending",
				"type":    "transfer",
				"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
				"amount":  "1.0",
				"asset":   "NEO",
				"network": "testnet",
			},
		}, nil)
	
	mockService.On("EstimateFee", txConfig).Return("0.001", nil)

	// Test each interface method to ensure it's properly implemented
	t.Run("Create method", func(t *testing.T) {
		txID, err := mockService.Create(txConfig)
		assert.NoError(t, err)
		assert.Equal(t, "mock-tx-id", txID)
	})

	t.Run("Sign method", func(t *testing.T) {
		txDetails, err := mockService.Sign("mock-tx-id", nil)
		assert.NoError(t, err)
		assert.Equal(t, "mock-tx-id", txDetails["id"])
		assert.Equal(t, true, txDetails["signed"])
		assert.Equal(t, "signed", txDetails["status"])
		assert.Equal(t, "0x123456789abcdef", txDetails["rawData"])
	})

	t.Run("Send method", func(t *testing.T) {
		txHash, err := mockService.Send(context.Background(), "mock-tx-id")
		assert.NoError(t, err)
		assert.Equal(t, "mock-hash", txHash)
	})

	t.Run("Status method", func(t *testing.T) {
		status, err := mockService.Status("mock-hash")
		assert.NoError(t, err)
		assert.Equal(t, "pending", status)
	})

	t.Run("Get method", func(t *testing.T) {
		txDetails, err := mockService.Get("mock-tx-id")
		assert.NoError(t, err)
		assert.Equal(t, "mock-tx-id", txDetails["id"])
		assert.Equal(t, "mock-hash", txDetails["hash"])
		assert.Equal(t, "pending", txDetails["status"])
	})

	t.Run("List method", func(t *testing.T) {
		txList, err := mockService.List()
		assert.NoError(t, err)
		assert.Len(t, txList, 1)
		txMap := txList[0].(map[string]interface{})
		assert.Equal(t, "mock-tx-id", txMap["id"])
	})

	t.Run("EstimateFee method", func(t *testing.T) {
		fee, err := mockService.EstimateFee(txConfig)
		assert.NoError(t, err)
		assert.Equal(t, "0.001", fee)
	})

	// Verify all expectations were met
	mockService.AssertExpectations(t)
}

// MockService is a mock implementation of the Service interface
type MockService struct {
	mock.Mock
}

// Create mocks the Create method
func (m *MockService) Create(config map[string]interface{}) (string, error) {
	args := m.Called(config)
	return args.String(0), args.Error(1)
}

// Sign mocks the Sign method
func (m *MockService) Sign(id string, account *wallet.Account) (map[string]interface{}, error) {
	args := m.Called(id, account)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(map[string]interface{}), args.Error(1)
}

// Send mocks the Send method
func (m *MockService) Send(ctx context.Context, id string) (string, error) {
	args := m.Called(ctx, id)
	return args.String(0), args.Error(1)
}

// Status mocks the Status method
func (m *MockService) Status(hash string) (string, error) {
	args := m.Called(hash)
	return args.String(0), args.Error(1)
}

// Get mocks the Get method
func (m *MockService) Get(id string) (map[string]interface{}, error) {
	args := m.Called(id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(map[string]interface{}), args.Error(1)
}

// List mocks the List method
func (m *MockService) List() ([]interface{}, error) {
	args := m.Called()
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]interface{}), args.Error(1)
}

// EstimateFee mocks the EstimateFee method
func (m *MockService) EstimateFee(config map[string]interface{}) (string, error) {
	args := m.Called(config)
	return args.String(0), args.Error(1)
}
