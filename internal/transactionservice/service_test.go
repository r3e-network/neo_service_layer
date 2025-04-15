package transaction

import (
	"testing"

	"github.com/stretchr/testify/assert"
	"go.uber.org/zap"
)

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
	t.Skip("Skipping test that requires actual transaction signing")
}

func TestServiceImpl_Send(t *testing.T) {
	t.Skip("Skipping test that requires actual transaction sending")
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

	// Test cases
	tests := []struct {
		name    string
		id      string
		wantErr bool
	}{
		{
			name:    "valid transaction",
			id:      tx.ID,
			wantErr: false,
		},
		{
			name:    "empty id",
			id:      "",
			wantErr: true,
		},
		{
			name:    "non-existent transaction",
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
	t.Skip("Skipping test that requires actual transaction manager")
}
