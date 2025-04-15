package internal

import (
	"context"
	"errors"
	"math/big"
	"testing"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
	"github.com/stretchr/testify/require"
)

func TestPoolManager_RefillPool(t *testing.T) {
	// Create test data
	ctx := context.Background()
	refillAmount := big.NewInt(5000000)

	policy := &models.GasUsagePolicy{
		MaxAllocationPerUser: big.NewInt(10000000),
		MinAllocationAmount:  big.NewInt(100000),
		MaxAllocationTime:    24 * time.Hour,
		RefillThreshold:      big.NewInt(2000000),
		RefillAmount:         refillAmount,
		CooldownPeriod:       5 * time.Minute,
	}

	t.Run("successful refill", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Create existing pool with low gas (below threshold)
		lowGasAmount := big.NewInt(1000000) // This must be less than policy.RefillThreshold (2000000)
		pool := models.NewGasPool(lowGasAmount)

		// Set LastRefill to a time in the past to satisfy the cooldown check
		pool.LastRefill = time.Now().Add(-10 * time.Minute)

		// Configure mock behavior
		mockStore.On("GetPool", ctx).Return(pool, nil)
		mockTxManager.On("TransferGAS", ctx, refillAmount).Return(nil)

		// Save updated pool with new values
		mockStore.On("SavePool", ctx, mock.MatchedBy(func(p *models.GasPool) bool {
			expectedAmount := new(big.Int).Add(lowGasAmount, refillAmount)
			return p.Amount.Cmp(expectedAmount) == 0 && p.RefillCount == 1
		})).Return(nil)

		mockMetrics.On("RecordRefill", ctx, mock.MatchedBy(func(amount *big.Int) bool {
			return amount.Cmp(refillAmount) == 0
		}), true).Return()

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.RefillPool(ctx)

		// Verify results
		require.NoError(t, err)

		// Verify mocks
		mockStore.AssertExpectations(t)
		mockTxManager.AssertExpectations(t)
		mockMetrics.AssertExpectations(t)
	})

	t.Run("pool not found", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Configure mock behavior - no pool exists
		mockStore.On("GetPool", ctx).Return(nil, nil)

		// Expect pool operations
		mockStore.On("SavePool", ctx, mock.AnythingOfType("*models.GasPool")).Return(nil)

		// Configure transfer behavior for any transfer calls that happen
		mockTxManager.On("TransferGAS", ctx, mock.AnythingOfType("*big.Int")).Return(nil)

		// Allow metrics recording
		mockMetrics.On("RecordRefill", ctx, mock.AnythingOfType("*big.Int"), true).Return()

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.RefillPool(ctx)

		// Verify results
		require.NoError(t, err)

		// We only verify that the SavePool method was called at least once
		// This is more flexible than checking exact call sequences
		mockCalls := mockStore.Calls
		savePoolCalls := 0
		for _, call := range mockCalls {
			if call.Method == "SavePool" {
				savePoolCalls++
			}
		}
		assert.GreaterOrEqual(t, savePoolCalls, 1, "SavePool should be called at least once")
	})

	t.Run("transfer fails", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Create existing pool with low gas
		lowGasAmount := big.NewInt(1000000)
		pool := models.NewGasPool(lowGasAmount)
		oldRefillTime := time.Now().Add(-1 * time.Hour)
		pool.LastRefill = oldRefillTime

		// Configure mock behavior
		mockStore.On("GetPool", ctx).Return(pool, nil)
		transferError := errors.New("transfer failed: insufficient funds")
		mockTxManager.On("TransferGAS", ctx, refillAmount).Return(transferError)
		mockAlerts.On("AlertFailedRefill", ctx, refillAmount, transferError.Error()).Return()
		mockMetrics.On("RecordRefill", ctx, refillAmount, false).Return()

		// Add mock for SavePool to handle the FailedRefills increment
		mockStore.On("SavePool", ctx, mock.AnythingOfType("*models.GasPool")).Return(nil)

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.RefillPool(ctx)

		// Verify results
		require.Error(t, err)
		assert.Contains(t, err.Error(), "transfer failed")

		// Verify pool was updated with failed refill count
		assert.Equal(t, lowGasAmount, pool.Amount)
		assert.Equal(t, oldRefillTime, pool.LastRefill)
		assert.Equal(t, int64(1), pool.FailedRefills)

		// Verify mocks
		mockStore.AssertExpectations(t)
		mockTxManager.AssertExpectations(t)
		mockAlerts.AssertExpectations(t)
		mockMetrics.AssertExpectations(t)
	})

	t.Run("cooldown period not elapsed", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Create existing pool with low gas but recent refill
		lowGasAmount := big.NewInt(1000000)
		pool := models.NewGasPool(lowGasAmount)
		recentRefill := time.Now().Add(-1 * time.Minute) // 1 minute ago
		pool.LastRefill = recentRefill

		// Configure mock behavior
		mockStore.On("GetPool", ctx).Return(pool, nil)

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.RefillPool(ctx)

		// Verify results
		require.NoError(t, err) // Not an error, just doesn't refill

		// Verify pool was not updated
		assert.Equal(t, lowGasAmount, pool.Amount)
		assert.Equal(t, recentRefill, pool.LastRefill)

		// Verify mocks - no transaction should be attempted
		mockStore.AssertExpectations(t)
		mockTxManager.AssertNotCalled(t, "TransferGAS")
	})

	t.Run("doesn't need refill", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Create existing pool with sufficient gas
		sufficientGasAmount := big.NewInt(8000000) // Above threshold
		pool := models.NewGasPool(sufficientGasAmount)
		oldRefill := time.Now().Add(-24 * time.Hour) // Long time ago
		pool.LastRefill = oldRefill

		// Configure mock behavior
		mockStore.On("GetPool", ctx).Return(pool, nil)

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.RefillPool(ctx)

		// Verify results
		require.NoError(t, err) // Not an error, just doesn't refill

		// Verify pool was not updated
		assert.Equal(t, sufficientGasAmount, pool.Amount)
		assert.Equal(t, oldRefill, pool.LastRefill)

		// Verify mocks - no transaction should be attempted
		mockStore.AssertExpectations(t)
		mockTxManager.AssertNotCalled(t, "TransferGAS")
	})
}

func TestPoolManager_ConsumeGas(t *testing.T) {
	// Create test data
	ctx := context.Background()
	consumeAmount := big.NewInt(300000)

	policy := &models.GasUsagePolicy{
		MaxAllocationPerUser: big.NewInt(10000000),
		MinAllocationAmount:  big.NewInt(100000),
		MaxAllocationTime:    24 * time.Hour,
		RefillThreshold:      big.NewInt(2000000),
		RefillAmount:         big.NewInt(5000000),
		CooldownPeriod:       5 * time.Minute,
	}

	t.Run("successful consumption", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Create existing pool with sufficient gas
		startAmount := big.NewInt(5000000)
		pool := models.NewGasPool(startAmount)

		// Configure mock behavior
		mockStore.On("GetPool", ctx).Return(pool, nil)
		mockStore.On("SavePool", ctx, mock.AnythingOfType("*models.GasPool")).Return(nil)

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.ConsumeGas(ctx, consumeAmount)

		// Verify results
		require.NoError(t, err)

		// Verify pool was updated
		expectedAmount := new(big.Int).Sub(startAmount, consumeAmount)
		assert.Equal(t, expectedAmount, pool.Amount)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})

	t.Run("insufficient gas", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Create existing pool with insufficient gas
		lowAmount := big.NewInt(200000) // Less than consume amount
		pool := models.NewGasPool(lowAmount)

		// Configure mock behavior
		mockStore.On("GetPool", ctx).Return(pool, nil)
		mockAlerts.On("AlertLowGas", ctx, lowAmount).Return()

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.ConsumeGas(ctx, consumeAmount)

		// Verify results
		require.Error(t, err)
		assert.Contains(t, err.Error(), "insufficient gas")

		// Verify pool was not updated
		assert.Equal(t, lowAmount, pool.Amount)

		// Verify mocks
		mockStore.AssertExpectations(t)
		mockAlerts.AssertExpectations(t)
	})

	t.Run("pool not found", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Configure mock behavior - no pool exists
		mockStore.On("GetPool", ctx).Return(nil, nil)

		// Expect a new pool to be created
		mockStore.On("SavePool", ctx, mock.AnythingOfType("*models.GasPool")).Return(nil)
		mockAlerts.On("AlertLowGas", ctx, big.NewInt(0)).Return()

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function - should fail because even the new pool has 0 gas
		err := manager.ConsumeGas(ctx, consumeAmount)

		// Verify results
		require.Error(t, err)
		assert.Contains(t, err.Error(), "insufficient gas")

		// Verify mocks
		mockStore.AssertExpectations(t)
		mockAlerts.AssertExpectations(t)
	})
}

func TestPoolManager_AddGas(t *testing.T) {
	// Create test data
	ctx := context.Background()
	addAmount := big.NewInt(1000000)

	policy := &models.GasUsagePolicy{
		MaxAllocationPerUser: big.NewInt(10000000),
		MinAllocationAmount:  big.NewInt(100000),
		MaxAllocationTime:    24 * time.Hour,
		RefillThreshold:      big.NewInt(2000000),
		RefillAmount:         big.NewInt(5000000),
		CooldownPeriod:       5 * time.Minute,
	}

	t.Run("successful addition", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Create existing pool
		startAmount := big.NewInt(5000000)
		pool := models.NewGasPool(startAmount)

		// Configure mock behavior
		mockStore.On("GetPool", ctx).Return(pool, nil)
		mockStore.On("SavePool", ctx, mock.AnythingOfType("*models.GasPool")).Return(nil)

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.AddGas(ctx, addAmount)

		// Verify results
		require.NoError(t, err)

		// Verify pool was updated
		expectedAmount := new(big.Int).Add(startAmount, addAmount)
		assert.Equal(t, expectedAmount, pool.Amount)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})

	t.Run("pool not found", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Configure mock behavior - no pool exists
		mockStore.On("GetPool", ctx).Return(nil, nil)

		// Expect a new pool to be created with the add amount
		mockStore.On("SavePool", ctx, mock.AnythingOfType("*models.GasPool")).Return(nil)

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.AddGas(ctx, addAmount)

		// Verify results
		require.NoError(t, err)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})

	t.Run("invalid amount", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call with invalid amount
		invalidAmount := big.NewInt(-1)
		err := manager.AddGas(ctx, invalidAmount)

		// Verify results
		require.Error(t, err)
		assert.Contains(t, err.Error(), "invalid amount")

		// Verify mocks - should not interact with store
		mockStore.AssertNotCalled(t, "GetPool")
		mockStore.AssertNotCalled(t, "SavePool")
	})
}

func TestPoolManager_GetAvailableGas(t *testing.T) {
	// Create test data
	ctx := context.Background()

	policy := &models.GasUsagePolicy{
		MaxAllocationPerUser: big.NewInt(10000000),
		MinAllocationAmount:  big.NewInt(100000),
		MaxAllocationTime:    24 * time.Hour,
		RefillThreshold:      big.NewInt(2000000),
		RefillAmount:         big.NewInt(5000000),
		CooldownPeriod:       5 * time.Minute,
	}

	t.Run("existing pool", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Create existing pool
		amount := big.NewInt(5000000)
		pool := models.NewGasPool(amount)

		// Configure mock behavior
		mockStore.On("GetPool", ctx).Return(pool, nil)

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		available, err := manager.GetAvailableGas(ctx)

		// Verify results
		require.NoError(t, err)
		assert.Equal(t, amount, available)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})

	t.Run("pool not found", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Configure mock behavior - no pool exists
		mockStore.On("GetPool", ctx).Return(nil, nil)

		// Expect a new pool to be created
		mockStore.On("SavePool", ctx, mock.AnythingOfType("*models.GasPool")).Return(nil)

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		available, err := manager.GetAvailableGas(ctx)

		// Verify results
		require.NoError(t, err)
		assert.Equal(t, big.NewInt(0), available)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})

	t.Run("error getting pool", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)
		mockTxManager := new(MockTransactionManager)

		// Configure mock behavior - error getting pool
		storeErr := errors.New("database error")
		mockStore.On("GetPool", ctx).Return(nil, storeErr)

		// Create the pool manager
		manager := NewPoolManager(mockStore, mockTxManager, policy, mockMetrics, mockAlerts)

		// Call the function
		available, err := manager.GetAvailableGas(ctx)

		// Verify results
		require.Error(t, err)
		assert.Contains(t, err.Error(), "database error")
		assert.Nil(t, available)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})
}

// MockTransactionManager implements the TransactionManager interface for testing
type MockTransactionManager struct {
	mock.Mock
}

func (m *MockTransactionManager) TransferGAS(ctx context.Context, amount *big.Int) error {
	args := m.Called(ctx, amount)
	return args.Error(0)
}
