package transaction

import (
	"context"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
	"github.com/will/neo_service_layer/internal/core/neo"
	"go.uber.org/zap"
)

// MockClient is a mock implementation of the neo.Client interface
type MockClient struct {
	mock.Mock
}

func (m *MockClient) GetClient() interface{} {
	args := m.Called()
	return args.Get(0)
}

func (m *MockClient) Close() error {
	args := m.Called()
	return args.Error(0)
}

func (m *MockClient) RotateClient() {
	m.Called()
}

// NewMockTransactionManager creates a new mock transaction manager
func NewMockTransactionManager() *neo.TransactionManager {
	// Create a new mock client
	mockClient := new(MockClient)
	mockClient.On("GetClient").Return(nil)
	mockClient.On("Close").Return(nil)
	mockClient.On("RotateClient").Return()
	
	// Create a new transaction manager with the mock client
	txManager := neo.NewTransactionManager(mockClient)
	
	return txManager
}

func TestNewService(t *testing.T) {
	// Test with nil logger
	config := DefaultConfig()
	service := &ServiceImpl{
		config:             config,
		logger:             zap.NewNop(),
		transactions:       make(map[string]*Transaction),
		transactionsByHash: make(map[string]string),
	}
	assert.NotNil(t, service)
	assert.Equal(t, config, service.config)
	assert.NotNil(t, service.logger)
	assert.NotNil(t, service.transactions)
	assert.NotNil(t, service.transactionsByHash)

	// Test with custom logger
	logger := zap.NewExample()
	service = &ServiceImpl{
		config:             config,
		logger:             logger,
		transactions:       make(map[string]*Transaction),
		transactionsByHash: make(map[string]string),
	}
	assert.Equal(t, logger, service.logger)
}

func TestServiceImpl_Create(t *testing.T) {
	service := &ServiceImpl{
		config:             DefaultConfig(),
		logger:             zap.NewExample(),
		transactions:       make(map[string]*Transaction),
		transactionsByHash: make(map[string]string),
	}

	tests := []struct {
		name    string
		config  map[string]interface{}
		wantErr bool
	}{
		{
			name: "valid transfer transaction",
			config: map[string]interface{}{
				"type":  "transfer",
				"to":    "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
				"value": "1.0",
				"asset": "NEO",
			},
			wantErr: false,
		},
		{
			name: "valid invoke transaction",
			config: map[string]interface{}{
				"type":     "invoke",
				"contract": "0xd2a4cff31913016155e38e474a2c06d08be276cf",
				"method":   "transfer",
				"params":   []interface{}{"from", "to", 100},
			},
			wantErr: false,
		},
		{
			name:    "nil config",
			config:  nil,
			wantErr: true,
		},
		{
			name: "missing type",
			config: map[string]interface{}{
				"to":    "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
				"value": "1.0",
			},
			wantErr: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			id, err := service.Create(tt.config)
			if tt.wantErr {
				assert.Error(t, err)
				assert.Empty(t, id)
			} else {
				assert.NoError(t, err)
				assert.NotEmpty(t, id)

				// Verify transaction was stored
				tx, err := service.Get(id)
				assert.NoError(t, err)
				assert.NotNil(t, tx)
			}
		})
	}
}

func TestServiceImpl_Sign(t *testing.T) {
	mockTxManager := NewMockTransactionManager()
	service := &ServiceImpl{
		config:             DefaultConfig(),
		logger:             zap.NewExample(),
		txManager:          mockTxManager,
		transactions:       make(map[string]*Transaction),
		transactionsByHash: make(map[string]string),
	}

	// Create a test transaction
	tx, _ := NewTransaction(map[string]interface{}{
		"type":  "transfer",
		"to":    "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
		"value": "1.0",
		"asset": "NEO",
	}, "")
	service.transactions[tx.ID] = tx

	// Create a test account
	account := &wallet.Account{}

	tests := []struct {
		name    string
		id      string
		account *wallet.Account
		wantErr bool
	}{
		{
			name:    "valid transaction",
			id:      tx.ID,
			account: account,
			wantErr: false,
		},
		{
			name:    "empty id",
			id:      "",
			account: account,
			wantErr: true,
		},
		{
			name:    "non-existent transaction",
			id:      "non-existent",
			account: account,
			wantErr: true,
		},
		{
			name:    "nil account",
			id:      tx.ID,
			account: nil,
			wantErr: true,
		},
		{
			name:    "already signed transaction",
			id:      tx.ID,
			account: account,
			wantErr: false, // First sign will succeed
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result, err := service.Sign(tt.id, tt.account)
			if tt.wantErr {
				assert.Error(t, err)
				assert.Nil(t, result)
			} else {
				assert.NoError(t, err)
				assert.NotNil(t, result)
				assert.Equal(t, tt.id, result["id"])
				assert.Equal(t, true, result["signed"])
			}
		})
	}
}

func TestServiceImpl_Send(t *testing.T) {
	mockTxManager := NewMockTransactionManager()
	service := &ServiceImpl{
		config:             DefaultConfig(),
		logger:             zap.NewExample(),
		txManager:          mockTxManager,
		transactions:       make(map[string]*Transaction),
		transactionsByHash: make(map[string]string),
	}

	// Create a test transaction
	tx, _ := NewTransaction(map[string]interface{}{
		"type":  "transfer",
		"to":    "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
		"value": "1.0",
		"asset": "NEO",
	}, "")
	tx.Signed = true
	service.transactions[tx.ID] = tx

	// Create an unsigned transaction
	unsignedTx, _ := NewTransaction(map[string]interface{}{
		"type":  "transfer",
		"to":    "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
		"value": "1.0",
		"asset": "NEO",
	}, "")
	service.transactions[unsignedTx.ID] = unsignedTx

	tests := []struct {
		name    string
		id      string
		ctx     context.Context
		wantErr bool
	}{
		{
			name:    "valid transaction",
			id:      tx.ID,
			ctx:     context.Background(),
			wantErr: false,
		},
		{
			name:    "empty id",
			id:      "",
			ctx:     context.Background(),
			wantErr: true,
		},
		{
			name:    "non-existent transaction",
			id:      "non-existent",
			ctx:     context.Background(),
			wantErr: true,
		},
		{
			name:    "unsigned transaction",
			id:      unsignedTx.ID,
			ctx:     context.Background(),
			wantErr: true,
		},
		{
			name:    "cancelled context",
			id:      tx.ID,
			ctx:     func() context.Context { ctx, cancel := context.WithCancel(context.Background()); cancel(); return ctx }(),
			wantErr: true,
		},
		{
			name:    "timeout context",
			id:      tx.ID,
			ctx:     func() context.Context { ctx, _ := context.WithTimeout(context.Background(), 1*time.Nanosecond); time.Sleep(2 * time.Nanosecond); return ctx }(),
			wantErr: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			hash, err := service.Send(tt.ctx, tt.id)
			if tt.wantErr {
				assert.Error(t, err)
				assert.Empty(t, hash)
			} else {
				assert.NoError(t, err)
				assert.NotEmpty(t, hash)
			}
		})
	}
}

func TestServiceImpl_Status(t *testing.T) {
	service := &ServiceImpl{
		config:             DefaultConfig(),
		logger:             zap.NewExample(),
		transactions:       make(map[string]*Transaction),
		transactionsByHash: make(map[string]string),
	}

	// Create a test transaction
	tx, _ := NewTransaction(map[string]interface{}{
		"type":  "transfer",
		"to":    "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
		"value": "1.0",
		"asset": "NEO",
	}, "")
	tx.Hash = "0x1234567890abcdef"
	tx.Status = "pending"
	service.transactions[tx.ID] = tx
	service.transactionsByHash[tx.Hash] = tx.ID

	tests := []struct {
		name       string
		hash       string
		wantStatus string
		wantErr    bool
	}{
		{
			name:       "valid hash",
			hash:       tx.Hash,
			wantStatus: "pending",
			wantErr:    false,
		},
		{
			name:       "empty hash",
			hash:       "",
			wantStatus: "",
			wantErr:    true,
		},
		{
			name:       "non-existent hash",
			hash:       "0xdeadbeef",
			wantStatus: "",
			wantErr:    true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			status, err := service.Status(tt.hash)
			if tt.wantErr {
				assert.Error(t, err)
				assert.Empty(t, status)
			} else {
				assert.NoError(t, err)
				assert.Equal(t, tt.wantStatus, status)
			}
		})
	}
}

func TestServiceImpl_Get(t *testing.T) {
	service := &ServiceImpl{
		config:             DefaultConfig(),
		logger:             zap.NewExample(),
		transactions:       make(map[string]*Transaction),
		transactionsByHash: make(map[string]string),
	}

	// Create a test transaction
	tx, _ := NewTransaction(map[string]interface{}{
		"type":  "transfer",
		"to":    "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
		"value": "1.0",
		"asset": "NEO",
	}, "")
	service.transactions[tx.ID] = tx

	tests := []struct {
		name    string
		id      string
		wantErr bool
	}{
		{
			name:    "valid id",
			id:      tx.ID,
			wantErr: false,
		},
		{
			name:    "empty id",
			id:      "",
			wantErr: true,
		},
		{
			name:    "non-existent id",
			id:      "non-existent",
			wantErr: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result, err := service.Get(tt.id)
			if tt.wantErr {
				assert.Error(t, err)
				assert.Nil(t, result)
			} else {
				assert.NoError(t, err)
				assert.NotNil(t, result)
				assert.Equal(t, tt.id, result["id"])
			}
		})
	}
}

func TestServiceImpl_List(t *testing.T) {
	service := &ServiceImpl{
		config:             DefaultConfig(),
		logger:             zap.NewExample(),
		transactions:       make(map[string]*Transaction),
		transactionsByHash: make(map[string]string),
	}

	// Create test transactions
	tx1, _ := NewTransaction(map[string]interface{}{
		"type":  "transfer",
		"to":    "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
		"value": "1.0",
		"asset": "NEO",
	}, "")
	service.transactions[tx1.ID] = tx1

	tx2, _ := NewTransaction(map[string]interface{}{
		"type":     "invoke",
		"contract": "0xd2a4cff31913016155e38e474a2c06d08be276cf",
		"method":   "transfer",
		"params":   []interface{}{"from", "to", 100},
	}, "")
	service.transactions[tx2.ID] = tx2

	// Test listing transactions
	result, err := service.List()
	assert.NoError(t, err)
	assert.Len(t, result, 2)

	// Verify transaction details
	for _, tx := range result {
		txMap := tx.(map[string]interface{})
		id := txMap["id"].(string)
		assert.Contains(t, []string{tx1.ID, tx2.ID}, id)
	}
}

func TestServiceImpl_EstimateFee(t *testing.T) {
	mockTxManager := NewMockTransactionManager()
	service := &ServiceImpl{
		config:             DefaultConfig(),
		logger:             zap.NewExample(),
		txManager:          mockTxManager,
		transactions:       make(map[string]*Transaction),
		transactionsByHash: make(map[string]string),
	}

	tests := []struct {
		name      string
		config    map[string]interface{}
		wantFee   string
		wantErr   bool
	}{
		{
			name: "valid transfer transaction",
			config: map[string]interface{}{
				"type":  "transfer",
				"to":    "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
				"value": "1.0",
				"asset": "NEO",
			},
			wantFee: "0.001000",
			wantErr: false,
		},
		{
			name: "valid invoke transaction",
			config: map[string]interface{}{
				"type":     "invoke",
				"contract": "0xd2a4cff31913016155e38e474a2c06d08be276cf",
				"method":   "transfer",
				"params":   []interface{}{"from", "to", 100},
			},
			wantFee: "0.010000",
			wantErr: false,
		},
		{
			name:    "nil config",
			config:  nil,
			wantFee: "",
			wantErr: true,
		},
		{
			name: "missing type",
			config: map[string]interface{}{
				"to":    "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
				"value": "1.0",
			},
			wantFee: "",
			wantErr: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			fee, err := service.EstimateFee(tt.config)
			if tt.wantErr {
				assert.Error(t, err)
				assert.Empty(t, fee)
			} else {
				assert.NoError(t, err)
				assert.Equal(t, tt.wantFee, fee)
			}
		})
	}
}
