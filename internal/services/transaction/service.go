package transaction

import (
	"context"
	"encoding/hex"
	"fmt"
	"sync"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/io"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/will/neo_service_layer/internal/core/neo"
	"go.uber.org/zap"
)

// Config contains configuration for the transaction service
type Config struct {
	// DefaultNetwork is the default blockchain network to use
	DefaultNetwork string
	// DefaultGasPrice is the default gas price to use
	DefaultGasPrice string
	// DefaultGasLimit is the default gas limit to use
	DefaultGasLimit int64
	// TransactionTimeout is the timeout for transaction operations
	TransactionTimeout time.Duration
	// MaxRetries is the maximum number of retries for transaction operations
	MaxRetries int
}

// DefaultConfig returns the default configuration for the transaction service
func DefaultConfig() Config {
	return Config{
		DefaultNetwork:     "testnet",
		DefaultGasPrice:    "0.00000001",
		DefaultGasLimit:    21000,
		TransactionTimeout: 30 * time.Second,
		MaxRetries:         3,
	}
}

// ServiceImpl implements the transaction Service interface
type ServiceImpl struct {
	config    Config
	logger    *zap.Logger
	txManager *neo.TransactionManager

	// In-memory storage for transactions (would be replaced with a database in production)
	transactions       map[string]*Transaction
	transactionsByHash map[string]string // Maps transaction hash to ID
	mu                 sync.RWMutex
}

// NewService creates a new transaction service with the given configuration
func NewService(config Config, logger *zap.Logger, client *neo.Client) *ServiceImpl {
	if logger == nil {
		logger = zap.NewNop()
	}

	return &ServiceImpl{
		config:             config,
		logger:             logger,
		txManager:          neo.NewTransactionManager(client),
		transactions:       make(map[string]*Transaction),
		transactionsByHash: make(map[string]string),
	}
}

// Create creates a new transaction with the given configuration
func (s *ServiceImpl) Create(config map[string]interface{}) (string, error) {
	s.logger.Info("Creating transaction", zap.Any("config", config))

	TransactionAttempts.Inc()

	// Validate transaction configuration
	if config == nil {
		return "", ErrInvalidTransactionConfig
	}

	// Create a new transaction record
	tx, err := NewTransaction(config, "")
	if err != nil {
		s.logger.Error("Failed to create transaction", zap.Error(err))
		return "", err
	}

	// Store the transaction
	s.mu.Lock()
	s.transactions[tx.ID] = tx
	s.mu.Unlock()

	ActiveTransactions.Inc()
	TransactionSuccesses.Inc()

	s.logger.Info("Transaction created", zap.String("id", tx.ID))
	return tx.ID, nil
}

// Sign signs a transaction with the given ID
func (s *ServiceImpl) Sign(id string, account *wallet.Account) (map[string]interface{}, error) {
	s.logger.Info("Signing transaction", zap.String("id", id))

	TransactionAttempts.Inc()

	// Validate transaction ID
	if id == "" {
		return nil, ErrInvalidTransactionID
	}

	// Get the transaction
	s.mu.RLock()
	tx, exists := s.transactions[id]
	s.mu.RUnlock()

	if !exists {
		return nil, ErrTransactionNotFound
	}

	// Check if the transaction is already signed
	if tx.Signed {
		return nil, ErrTransactionAlreadySigned
	}

	// Create NEO transaction
	script := []byte(tx.Data)
	neoTx, err := s.txManager.CreateTransaction(script, []neo.Signer{
		{
			Account: account.ScriptHash(),
			Scopes:  1, // CalledByEntry
		},
	})
	if err != nil {
		return nil, fmt.Errorf("failed to create NEO transaction: %w", err)
	}

	// Sign the transaction
	err = s.txManager.SignTransaction(neoTx, account)
	if err != nil {
		return nil, fmt.Errorf("failed to sign NEO transaction: %w", err)
	}

	// Update transaction status
	tx.Signed = true
	tx.Status = StatusSigned
	tx.RawData = fmt.Sprintf("%x", neoTx.Bytes())
	tx.UpdatedAt = time.Now()

	// Update the transaction
	s.mu.Lock()
	s.transactions[id] = tx
	s.mu.Unlock()

	TransactionSuccesses.Inc()

	s.logger.Info("Transaction signed", zap.String("id", id))
	return tx.ToMap(), nil
}

// Send sends a transaction with the given ID to the blockchain
func (s *ServiceImpl) Send(ctx context.Context, id string) (string, error) {
	s.logger.Info("Sending transaction", zap.String("id", id))

	TransactionAttempts.Inc()

	// Validate transaction ID
	if id == "" {
		return "", ErrInvalidTransactionID
	}

	// Get the transaction
	s.mu.RLock()
	tx, exists := s.transactions[id]
	s.mu.RUnlock()

	if !exists {
		return "", ErrTransactionNotFound
	}

	// Check if the transaction is signed
	if !tx.Signed {
		return "", ErrTransactionNotSigned
	}

	// Check if the transaction is already sent
	if tx.Status == StatusSent || tx.Status == StatusPending || tx.Status == StatusConfirmed {
		return tx.Hash, nil
	}

	// Parse raw transaction
	rawTxBytes, err := hex.DecodeString(tx.RawData)
	if err != nil {
		return "", fmt.Errorf("failed to decode raw transaction: %w", err)
	}

	rawTx := &transaction.Transaction{}
	reader := io.NewBinReaderFromBuf(rawTxBytes)
	rawTx.DecodeBinary(reader)
	if reader.Err != nil {
		return "", fmt.Errorf("failed to decode transaction: %w", reader.Err)
	}

	// Send transaction
	result, err := s.txManager.SendTransaction(ctx, rawTx)
	if err != nil {
		tx.Status = StatusFailed
		tx.UpdatedAt = time.Now()

		s.mu.Lock()
		s.transactions[id] = tx
		s.mu.Unlock()

		return "", fmt.Errorf("failed to send transaction: %w", err)
	}

	// Update transaction status
	tx.Hash = result.Hash.StringLE()
	tx.Status = StatusPending
	tx.UpdatedAt = time.Now()

	// Update transaction mappings
	s.mu.Lock()
	s.transactions[id] = tx
	s.transactionsByHash[tx.Hash] = id
	s.mu.Unlock()

	TransactionSuccesses.Inc()

	s.logger.Info("Transaction sent",
		zap.String("id", id),
		zap.String("hash", tx.Hash))

	return tx.Hash, nil
}

// Status gets the status of a transaction with the given hash
func (s *ServiceImpl) Status(hash string) (string, error) {
	s.logger.Info("Getting transaction status", zap.String("hash", hash))

	// Validate transaction hash
	if hash == "" {
		return "", ErrInvalidTransactionID
	}

	// Get the transaction ID from the hash
	s.mu.RLock()
	id, exists := s.transactionsByHash[hash]
	s.mu.RUnlock()

	if !exists {
		return "", ErrTransactionNotFound
	}

	// Get the transaction
	s.mu.RLock()
	tx, exists := s.transactions[id]
	s.mu.RUnlock()

	if !exists {
		return "", ErrTransactionNotFound
	}

	s.logger.Info("Transaction status retrieved", zap.String("hash", hash), zap.String("status", string(tx.Status)))
	return string(tx.Status), nil
}

// Get gets the details of a transaction with the given ID
func (s *ServiceImpl) Get(id string) (map[string]interface{}, error) {
	s.logger.Info("Getting transaction details", zap.String("id", id))

	// Validate transaction ID
	if id == "" {
		return nil, ErrInvalidTransactionID
	}

	// Get the transaction
	s.mu.RLock()
	tx, exists := s.transactions[id]
	s.mu.RUnlock()

	if !exists {
		return nil, ErrTransactionNotFound
	}

	s.logger.Info("Transaction details retrieved", zap.String("id", id))
	return tx.ToMap(), nil
}

// List lists all transactions
func (s *ServiceImpl) List() ([]interface{}, error) {
	s.logger.Info("Listing transactions")

	// Get all transactions
	s.mu.RLock()
	defer s.mu.RUnlock()

	result := make([]interface{}, 0, len(s.transactions))
	for _, tx := range s.transactions {
		result = append(result, tx.ToMap())
	}

	s.logger.Info("Transactions listed", zap.Int("count", len(result)))
	return result, nil
}

// EstimateFee estimates the fee for a transaction with the given configuration
func (s *ServiceImpl) EstimateFee(config map[string]interface{}) (string, error) {
	s.logger.Info("Estimating transaction fee", zap.Any("config", config))

	// Validate transaction configuration
	if config == nil {
		return "", ErrInvalidTransactionConfig
	}

	// Extract transaction type
	txType, ok := config["type"].(string)
	if !ok || txType == "" {
		return "", ErrInvalidTransactionType
	}

	// Estimate fee based on transaction type (in a real implementation, this would use gas price oracles)
	var fee float64
	switch txType {
	case string(TypeTransfer):
		fee = 0.001
	case string(TypeInvoke):
		fee = 0.01
	case string(TypeDeploy):
		fee = 0.05
	default:
		fee = 0.005
	}

	// Adjust fee based on gas limit if provided
	if gasLimit, ok := config["gasLimit"].(float64); ok && gasLimit > 0 {
		fee = fee * (gasLimit / 21000.0)
	}

	// Record fee estimation
	TransactionFeeEstimation.Observe(fee)

	s.logger.Info("Transaction fee estimated", zap.Float64("fee", fee))
	return fmt.Sprintf("%f", fee), nil
}
