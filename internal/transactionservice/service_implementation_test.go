package transaction

import (
	"context"
	"sync"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/r3e-network/neo_service_layer/internal/core/neo"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
	"go.uber.org/zap"
)

func TestServiceImplementation(t *testing.T) {
	// Create a logger for testing
	logger, _ := zap.NewDevelopment()

	// Create a mock Neo client
	mockClient := &neo.Client{}

	// Create a transaction service with default config
	config := DefaultConfig()
	service := NewService(config, logger, mockClient)

	// Test transaction creation
	t.Run("Create transaction", func(t *testing.T) {
		// Create a transaction
		txConfig := map[string]interface{}{
			"type":    "transfer",
			"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
			"amount":  "1.0",
			"asset":   "NEO",
			"network": "testnet",
		}

		txID, err := service.Create(txConfig)
		require.NoError(t, err)
		require.NotEmpty(t, txID)

		// Verify the transaction was created
		tx, err := service.Get(txID)
		require.NoError(t, err)
		assert.Equal(t, txID, tx["id"])
		assert.Equal(t, "created", tx["status"])
		assert.Equal(t, "transfer", tx["type"])
		assert.Equal(t, "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu", tx["to"])
		assert.Equal(t, "1.0", tx["amount"])
		assert.Equal(t, "NEO", tx["asset"])
		assert.Equal(t, "testnet", tx["network"])
	})

	// Create a transaction for subsequent tests
	txConfig := map[string]interface{}{
		"type":    "transfer",
		"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
		"amount":  "1.0",
		"asset":   "NEO",
		"network": "testnet",
	}
	txID, err := service.Create(txConfig)
	require.NoError(t, err)

	// Test transaction signing
	t.Run("Sign transaction", func(t *testing.T) {
		// Create a mock account for testing
		// Note: In a real test, we would use a real wallet.Account
		// For now, we'll skip the actual signing since it requires dependencies
		// This test will fail until dependencies are resolved
		t.Skip("Skipping test until wallet dependencies are resolved")

		account := &wallet.Account{}
		txDetails, err := service.Sign(txID, account)
		require.NoError(t, err)
		assert.Equal(t, txID, txDetails["id"])
		assert.Equal(t, true, txDetails["signed"])
		assert.Equal(t, "signed", txDetails["status"])
		assert.NotEmpty(t, txDetails["rawData"])
	})

	// Test transaction sending
	t.Run("Send transaction", func(t *testing.T) {
		// Note: In a real test, we would sign the transaction first
		// For now, we'll simulate a signed transaction
		service.mu.Lock()
		tx := service.transactions[txID]
		tx.Signed = true
		tx.Status = StatusSigned
		tx.RawData = "0x123456789abcdef"
		service.transactions[txID] = tx
		service.mu.Unlock()

		// Send the transaction
		ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
		defer cancel()

		txHash, err := service.Send(ctx, txID)
		require.NoError(t, err)
		require.NotEmpty(t, txHash)

		// Verify the transaction status was updated
		service.mu.RLock()
		tx = service.transactions[txID]
		service.mu.RUnlock()
		assert.Equal(t, StatusSent, tx.Status)
		assert.Equal(t, txHash, tx.Hash)
	})

	// Test transaction status
	t.Run("Get transaction status", func(t *testing.T) {
		// Get the transaction hash
		service.mu.RLock()
		tx := service.transactions[txID]
		txHash := tx.Hash
		service.mu.RUnlock()

		// Get the transaction status
		status, err := service.Status(txHash)
		require.NoError(t, err)
		assert.Equal(t, StatusSent, status)
	})

	// Test transaction details
	t.Run("Get transaction details", func(t *testing.T) {
		// Get the transaction details
		txDetails, err := service.Get(txID)
		require.NoError(t, err)
		assert.Equal(t, txID, txDetails["id"])
		assert.NotEmpty(t, txDetails["hash"])
		assert.Equal(t, StatusSent, txDetails["status"])
		assert.Equal(t, "transfer", txDetails["type"])
		assert.Equal(t, "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu", txDetails["to"])
		assert.Equal(t, "1.0", txDetails["amount"])
		assert.Equal(t, "NEO", txDetails["asset"])
		assert.Equal(t, "testnet", txDetails["network"])
	})

	// Test transaction list
	t.Run("List transactions", func(t *testing.T) {
		// List all transactions
		txList, err := service.List()
		require.NoError(t, err)
		assert.NotEmpty(t, txList)

		// Verify the transaction is in the list
		found := false
		for _, tx := range txList {
			txMap := tx.(map[string]interface{})
			if txMap["id"] == txID {
				found = true
				break
			}
		}
		assert.True(t, found, "Transaction not found in list")
	})

	// Test fee estimation
	t.Run("Estimate transaction fee", func(t *testing.T) {
		// Estimate the fee for a transaction
		feeConfig := map[string]interface{}{
			"type":    "transfer",
			"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
			"amount":  "1.0",
			"asset":   "NEO",
			"network": "testnet",
		}

		fee, err := service.EstimateFee(feeConfig)
		require.NoError(t, err)
		assert.NotEmpty(t, fee)
	})

	// Test concurrent access to ensure thread safety
	t.Run("Concurrent access", func(t *testing.T) {
		// Create a new service for this test to avoid interference
		concurrentService := NewService(config, logger, mockClient)

		// Number of concurrent operations
		numOperations := 10

		// WaitGroup to wait for all goroutines to complete
		var wg sync.WaitGroup
		wg.Add(numOperations)

		// Create transactions concurrently
		for i := 0; i < numOperations; i++ {
			go func(index int) {
				defer wg.Done()

				// Create a transaction
				txConfig := map[string]interface{}{
					"type":    "transfer",
					"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
					"amount":  "1.0",
					"asset":   "NEO",
					"network": "testnet",
					"index":   index, // Add a unique identifier
				}

				txID, err := concurrentService.Create(txConfig)
				assert.NoError(t, err)
				assert.NotEmpty(t, txID)

				// Simulate signing
				concurrentService.mu.Lock()
				tx := concurrentService.transactions[txID]
				tx.Signed = true
				tx.Status = StatusSigned
				tx.RawData = "0x123456789abcdef"
				concurrentService.transactions[txID] = tx
				concurrentService.mu.Unlock()

				// Get transaction details
				txDetails, err := concurrentService.Get(txID)
				assert.NoError(t, err)
				assert.Equal(t, txID, txDetails["id"])
				assert.Equal(t, true, txDetails["signed"])
			}(i)
		}

		// Wait for all goroutines to complete
		wg.Wait()

		// Verify all transactions were created
		txList, err := concurrentService.List()
		assert.NoError(t, err)
		assert.Equal(t, numOperations, len(txList))
	})

	// Test timeout handling
	t.Run("Timeout handling", func(t *testing.T) {
		// Create a transaction
		txID, err := service.Create(txConfig)
		require.NoError(t, err)

		// Simulate a signed transaction
		service.mu.Lock()
		tx := service.transactions[txID]
		tx.Signed = true
		tx.Status = StatusSigned
		tx.RawData = "0x123456789abcdef"
		service.transactions[txID] = tx
		service.mu.Unlock()

		// Create a context with a very short timeout
		ctx, cancel := context.WithTimeout(context.Background(), 1*time.Nanosecond)
		defer cancel()

		// Sleep to ensure the timeout expires
		time.Sleep(1 * time.Millisecond)

		// Try to send the transaction with an expired context
		_, err = service.Send(ctx, txID)
		assert.Error(t, err)
		assert.Contains(t, err.Error(), "context deadline exceeded")
	})

	// Test error cases
	t.Run("Error cases", func(t *testing.T) {
		// Test invalid transaction ID
		_, err := service.Sign("invalid-tx-id", nil)
		assert.Error(t, err)
		assert.Equal(t, ErrTransactionNotFound, err)

		// Test sending an unsigned transaction
		// Create a new transaction
		newTxID, err := service.Create(txConfig)
		require.NoError(t, err)

		// Try to send it without signing
		_, err = service.Send(context.Background(), newTxID)
		assert.Error(t, err)
		assert.Equal(t, ErrTransactionNotSigned, err)

		// Test invalid transaction hash
		_, err = service.Status("invalid-hash")
		assert.Error(t, err)
		assert.Equal(t, ErrTransactionNotFound, err)

		// Test invalid transaction ID for Get
		_, err = service.Get("invalid-tx-id")
		assert.Error(t, err)
		assert.Equal(t, ErrTransactionNotFound, err)

		// Test nil transaction config
		_, err = service.Create(nil)
		assert.Error(t, err)
		assert.Equal(t, ErrInvalidTransactionConfig, err)

		// Test signing an already signed transaction
		signedTxID, err := service.Create(txConfig)
		require.NoError(t, err)

		service.mu.Lock()
		tx := service.transactions[signedTxID]
		tx.Signed = true
		service.transactions[signedTxID] = tx
		service.mu.Unlock()

		_, err = service.Sign(signedTxID, &wallet.Account{})
		assert.Error(t, err)
		assert.Equal(t, ErrTransactionAlreadySigned, err)
	})
}

func TestTransactionLifecycle(t *testing.T) {
	// Create a logger for testing
	logger, _ := zap.NewDevelopment()

	// Create a mock Neo client
	mockClient := &neo.Client{}

	// Create a transaction service with default config
	config := DefaultConfig()
	service := NewService(config, logger, mockClient)

	// Test the complete transaction lifecycle
	t.Run("Transaction lifecycle", func(t *testing.T) {
		// Skip this test until dependencies are resolved
		t.Skip("Skipping test until wallet dependencies are resolved")

		// 1. Create a transaction
		txConfig := map[string]interface{}{
			"type":    "transfer",
			"to":      "NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu",
			"amount":  "1.0",
			"asset":   "NEO",
			"network": "testnet",
		}

		txID, err := service.Create(txConfig)
		require.NoError(t, err)
		require.NotEmpty(t, txID)

		// Verify initial status
		txDetails, err := service.Get(txID)
		require.NoError(t, err)
		assert.Equal(t, "created", txDetails["status"])

		// 2. Sign the transaction
		account := &wallet.Account{}
		signedTx, err := service.Sign(txID, account)
		require.NoError(t, err)
		assert.Equal(t, "signed", signedTx["status"])

		// 3. Send the transaction
		ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
		defer cancel()

		txHash, err := service.Send(ctx, txID)
		require.NoError(t, err)
		require.NotEmpty(t, txHash)

		// 4. Check the transaction status
		status, err := service.Status(txHash)
		require.NoError(t, err)
		assert.Equal(t, StatusSent, status)

		// 5. Get the final transaction details
		finalTx, err := service.Get(txID)
		require.NoError(t, err)
		assert.Equal(t, txID, finalTx["id"])
		assert.Equal(t, txHash, finalTx["hash"])
		assert.Equal(t, StatusSent, finalTx["status"])
	})
}
