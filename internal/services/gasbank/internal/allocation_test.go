package internal

import (
	"context"
	"math/big"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
	"github.com/stretchr/testify/require"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

// MockGasStore is a mock implementation of GasStore
type MockGasStore struct {
	mock.Mock
}

func (m *MockGasStore) SaveAllocation(ctx context.Context, allocation *models.Allocation) error {
	args := m.Called(ctx, allocation)
	return args.Error(0)
}

func (m *MockGasStore) GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error) {
	args := m.Called(ctx, userAddress)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Allocation), args.Error(1)
}

func (m *MockGasStore) DeleteAllocation(ctx context.Context, userAddress util.Uint160) error {
	args := m.Called(ctx, userAddress)
	return args.Error(0)
}

func (m *MockGasStore) ListAllocations(ctx context.Context) ([]*models.Allocation, error) {
	args := m.Called(ctx)
	return args.Get(0).([]*models.Allocation), args.Error(1)
}

func (m *MockGasStore) GetAllAllocations(ctx context.Context) ([]*models.Allocation, error) {
	args := m.Called(ctx)
	return args.Get(0).([]*models.Allocation), args.Error(1)
}

func (m *MockGasStore) GetPool(ctx context.Context) (*models.GasPool, error) {
	args := m.Called(ctx)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.GasPool), args.Error(1)
}

func (m *MockGasStore) SavePool(ctx context.Context, pool *models.GasPool) error {
	args := m.Called(ctx, pool)
	return args.Error(0)
}

// MockGasMetricsCollector is a mock implementation of GasMetricsCollector
type MockGasMetricsCollector struct {
	mock.Mock
}

func (m *MockGasMetricsCollector) RecordAllocation(ctx context.Context, allocation *models.Allocation) {
	m.Called(ctx, allocation)
}

func (m *MockGasMetricsCollector) RecordUsage(ctx context.Context, userAddress util.Uint160, amount *big.Int) {
	m.Called(ctx, userAddress, amount)
}

func (m *MockGasMetricsCollector) RecordRefill(ctx context.Context, amount *big.Int, success bool) {
	m.Called(ctx, amount, success)
}

func (m *MockGasMetricsCollector) GetMetrics(ctx context.Context) *models.GasUsageMetrics {
	args := m.Called(ctx)
	return args.Get(0).(*models.GasUsageMetrics)
}

// MockGasAlertManager is a mock implementation of GasAlertManager
type MockGasAlertManager struct {
	mock.Mock
}

func (m *MockGasAlertManager) AlertLowGas(ctx context.Context, remaining *big.Int) {
	m.Called(ctx, remaining)
}

func (m *MockGasAlertManager) AlertFailedAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int, reason string) {
	m.Called(ctx, userAddress, amount, reason)
}

func (m *MockGasAlertManager) AlertFailedRefill(ctx context.Context, amount *big.Int, reason string) {
	m.Called(ctx, amount, reason)
}

func (m *MockGasAlertManager) AlertHighUtilization(ctx context.Context, utilization float64, totalGas *big.Int, allocatedGas *big.Int) {
	m.Called(ctx, utilization, totalGas, allocatedGas)
}

func (m *MockGasAlertManager) AlertLargeAllocation(ctx context.Context, allocation *models.Allocation) {
	m.Called(ctx, allocation)
}

func (m *MockGasAlertManager) AlertAllocationExpired(ctx context.Context, allocation *models.Allocation) {
	m.Called(ctx, allocation)
}

func (m *MockGasAlertManager) AlertSystemError(ctx context.Context, component string, err error) {
	m.Called(ctx, component, err)
}

func TestAllocationManager_AllocateGas(t *testing.T) {
	// Create test data
	ctx := context.Background()
	userAddress, err := util.Uint160DecodeStringLE("0123456789abcdef0123456789abcdef01234567")
	require.NoError(t, err)
	amount := big.NewInt(1000000)
	useAmount := big.NewInt(100000) // Define the gas usage amount for tests

	policy := &models.GasUsagePolicy{
		MaxAllocationPerUser: big.NewInt(10000000),
		MinAllocationAmount:  big.NewInt(100000),
		MaxAllocationTime:    24 * time.Hour,
		RefillThreshold:      big.NewInt(200000),
		RefillAmount:         big.NewInt(500000),
		CooldownPeriod:       5 * time.Minute,
	}

	t.Run("successful allocation", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(nil, nil)
		mockStore.On("SaveAllocation", ctx, mock.AnythingOfType("*models.Allocation")).Return(nil)
		mockMetrics.On("RecordAllocation", ctx, mock.AnythingOfType("*models.Allocation")).Return()

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		allocation, err := manager.AllocateGas(ctx, userAddress, amount)

		// Verify results
		require.NoError(t, err)
		assert.NotNil(t, allocation)
		assert.Equal(t, userAddress, allocation.UserAddress)
		assert.Equal(t, amount, allocation.Amount)
		assert.Equal(t, "active", allocation.Status)
		assert.True(t, allocation.ExpiresAt.After(time.Now()))

		// Verify mocks
		mockStore.AssertExpectations(t)
		mockMetrics.AssertExpectations(t)
	})

	t.Run("existing allocation", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create existing allocation
		existingAllocation := &models.Allocation{
			ID:          "test-allocation",
			UserAddress: userAddress,
			Amount:      big.NewInt(500000),
			Used:        big.NewInt(0),
			ExpiresAt:   time.Now().Add(12 * time.Hour),
			Status:      "active",
			LastUsedAt:  time.Now(),
		}

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(existingAllocation, nil)

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		allocation, err := manager.AllocateGas(ctx, userAddress, amount)

		// Verify results
		require.NoError(t, err)
		assert.Equal(t, existingAllocation, allocation)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})

	t.Run("invalid address", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call with invalid address
		invalidAddress := util.Uint160{}
		allocation, err := manager.AllocateGas(ctx, invalidAddress, amount)

		// Verify results
		require.Error(t, err)
		assert.Nil(t, allocation)
		assert.Contains(t, err.Error(), "invalid user address")
	})

	t.Run("invalid amount", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call with invalid amount
		invalidAmount := big.NewInt(0)
		allocation, err := manager.AllocateGas(ctx, userAddress, invalidAmount)

		// Verify results
		require.Error(t, err)
		assert.Nil(t, allocation)
		assert.Contains(t, err.Error(), "invalid allocation amount")
	})

	t.Run("exceeds policy limit", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call with amount exceeding policy
		tooLargeAmount := big.NewInt(0).Add(policy.MaxAllocationPerUser, big.NewInt(1))
		allocation, err := manager.AllocateGas(ctx, userAddress, tooLargeAmount)

		// Verify results
		require.Error(t, err)
		assert.Nil(t, allocation)
		assert.Contains(t, err.Error(), "exceeds maximum allowed")
	})

	t.Run("allocation with insufficient gas", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create allocation with insufficient gas
		allocation := &models.Allocation{
			ID:          "test-allocation",
			UserAddress: userAddress,
			Amount:      amount,
			Used:        big.NewInt(0),
			ExpiresAt:   time.Now().Add(12 * time.Hour),
			Status:      "active",
			LastUsedAt:  time.Now(),
		}

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(allocation, nil)
		mockStore.On("SaveAllocation", ctx, mock.AnythingOfType("*models.Allocation")).Return(nil)
		mockMetrics.On("RecordAllocation", ctx, mock.AnythingOfType("*models.Allocation")).Return()

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		allocation, err := manager.AllocateGas(ctx, userAddress, amount)

		// Verify results
		require.NoError(t, err)
		assert.NotNil(t, allocation)
		assert.Equal(t, userAddress, allocation.UserAddress)
		assert.Equal(t, amount, allocation.Amount)
		assert.Equal(t, "active", allocation.Status)
		assert.True(t, allocation.ExpiresAt.After(time.Now()))

		// Verify mocks
		mockStore.AssertExpectations(t)
		mockMetrics.AssertExpectations(t)
	})

	t.Run("expired allocation", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create expired allocation
		allocation := &models.Allocation{
			ID:          "test-allocation",
			UserAddress: userAddress,
			Amount:      amount,
			Used:        big.NewInt(0),
			ExpiresAt:   time.Now().Add(-1 * time.Hour),
			Status:      "expired",
			LastUsedAt:  time.Now(),
		}

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(allocation, nil)
		mockStore.On("DeleteAllocation", ctx, userAddress).Return(nil)
		mockStore.On("SaveAllocation", ctx, mock.AnythingOfType("*models.Allocation")).Return(nil)
		mockMetrics.On("RecordAllocation", ctx, mock.AnythingOfType("*models.Allocation")).Return()
		mockAlerts.On("AlertAllocationExpired", ctx, allocation).Return()

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		allocation, err := manager.AllocateGas(ctx, userAddress, amount)

		// Verify results
		require.NoError(t, err)
		assert.NotNil(t, allocation)
		assert.Equal(t, userAddress, allocation.UserAddress)
		assert.Equal(t, amount, allocation.Amount)
		assert.Equal(t, "active", allocation.Status)
		assert.True(t, allocation.ExpiresAt.After(time.Now()))

		// Verify mocks
		mockStore.AssertExpectations(t)
		mockMetrics.AssertExpectations(t)
		mockAlerts.AssertExpectations(t)
	})

	t.Run("allocation with large amount", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create allocation with large amount
		allocation := &models.Allocation{
			ID:          "test-allocation",
			UserAddress: userAddress,
			Amount:      amount,
			Used:        big.NewInt(0),
			ExpiresAt:   time.Now().Add(12 * time.Hour),
			Status:      "active",
			LastUsedAt:  time.Now(),
		}

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(allocation, nil)
		mockStore.On("SaveAllocation", ctx, mock.AnythingOfType("*models.Allocation")).Return(nil)
		mockMetrics.On("RecordAllocation", ctx, mock.AnythingOfType("*models.Allocation")).Return()
		mockAlerts.On("AlertLargeAllocation", ctx, allocation).Return()

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.UseGas(ctx, userAddress, useAmount)

		// Verify results
		require.Error(t, err)
		assert.Contains(t, err.Error(), "expired")

		// Verify allocation was not updated
		assert.Equal(t, big.NewInt(0), allocation.Used)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})

	t.Run("no allocation", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(nil, nil)

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.UseGas(ctx, userAddress, useAmount)

		// Verify results
		require.Error(t, err)
		assert.Contains(t, err.Error(), "no gas allocation found")

		// Verify mocks
		mockStore.AssertExpectations(t)
	})
}

func TestAllocationManager_ReleaseGas(t *testing.T) {
	// Create test data
	ctx := context.Background()
	userAddress, err := util.Uint160DecodeStringLE("0123456789abcdef0123456789abcdef01234567")
	require.NoError(t, err)

	totalAmount := big.NewInt(1000000)

	policy := &models.GasUsagePolicy{
		MaxAllocationPerUser: big.NewInt(10000000),
		MinAllocationAmount:  big.NewInt(100000),
		MaxAllocationTime:    24 * time.Hour,
		RefillThreshold:      big.NewInt(200000),
		RefillAmount:         big.NewInt(500000),
		CooldownPeriod:       5 * time.Minute,
	}

	t.Run("successful release", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create existing allocation
		allocation := models.NewGasAllocation(userAddress, totalAmount, time.Now().Add(12*time.Hour))

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(allocation, nil)
		mockStore.On("DeleteAllocation", ctx, userAddress).Return(nil)

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.ReleaseGas(ctx, userAddress)

		// Verify results
		require.NoError(t, err)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})

	t.Run("no allocation", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(nil, nil)

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.ReleaseGas(ctx, userAddress)

		// Verify results - should not error when no allocation exists
		require.NoError(t, err)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})

	t.Run("invalid address", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call with invalid address
		invalidAddress := util.Uint160{}
		err := manager.ReleaseGas(ctx, invalidAddress)

		// Verify results
		require.Error(t, err)
		assert.Contains(t, err.Error(), "invalid user address")
	})
}

func TestAllocationManager_GetAllocation(t *testing.T) {
	// Create test data
	ctx := context.Background()
	userAddress, err := util.Uint160DecodeStringLE("0123456789abcdef0123456789abcdef01234567")
	require.NoError(t, err)

	totalAmount := big.NewInt(1000000)

	policy := &models.GasUsagePolicy{
		MaxAllocationPerUser: big.NewInt(10000000),
		MinAllocationAmount:  big.NewInt(100000),
		MaxAllocationTime:    24 * time.Hour,
		RefillThreshold:      big.NewInt(200000),
		RefillAmount:         big.NewInt(500000),
		CooldownPeriod:       5 * time.Minute,
	}

	t.Run("existing allocation", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create existing allocation
		allocation := models.NewGasAllocation(userAddress, totalAmount, time.Now().Add(12*time.Hour))

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(allocation, nil)

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		result, err := manager.GetAllocation(ctx, userAddress)

		// Verify results
		require.NoError(t, err)
		assert.Equal(t, allocation, result)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})

	t.Run("no allocation", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(nil, nil)

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		result, err := manager.GetAllocation(ctx, userAddress)

		// Verify results
		require.NoError(t, err)
		assert.Nil(t, result)

		// Verify mocks
		mockStore.AssertExpectations(t)
	})

	t.Run("invalid address", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call with invalid address
		invalidAddress := util.Uint160{}
		result, err := manager.GetAllocation(ctx, invalidAddress)

		// Verify results
		require.Error(t, err)
		assert.Nil(t, result)
		assert.Contains(t, err.Error(), "invalid user address")
	})
}

func TestAllocationManager_UseGas(t *testing.T) {
	// Create test data
	ctx := context.Background()
	userAddress, err := util.Uint160DecodeStringLE("0123456789abcdef0123456789abcdef01234567")
	require.NoError(t, err)

	totalAmount := big.NewInt(1000000)
	useAmount := big.NewInt(100000) // Define the gas usage amount for tests

	policy := &models.GasUsagePolicy{
		MaxAllocationPerUser: big.NewInt(10000000),
		MinAllocationAmount:  big.NewInt(100000),
		MaxAllocationTime:    24 * time.Hour,
		RefillThreshold:      big.NewInt(200000),
		RefillAmount:         big.NewInt(500000),
		CooldownPeriod:       5 * time.Minute,
	}

	t.Run("successful gas usage", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create existing allocation
		allocation := &models.Allocation{
			ID:          "test-allocation",
			UserAddress: userAddress,
			Amount:      totalAmount,
			Used:        big.NewInt(0),
			ExpiresAt:   time.Now().Add(12 * time.Hour),
			Status:      "active",
			LastUsedAt:  time.Now(),
		}

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(allocation, nil)
		mockStore.On("SaveAllocation", ctx, mock.AnythingOfType("*models.Allocation")).Return(nil)
		mockMetrics.On("RecordUsage", ctx, userAddress, useAmount).Return()

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.UseGas(ctx, userAddress, useAmount)

		// Verify results
		require.NoError(t, err)

		// Verify allocation was updated
		assert.Equal(t, useAmount, allocation.Used)
		expectedRemaining := new(big.Int).Sub(totalAmount, useAmount)
		assert.Equal(t, expectedRemaining, allocation.RemainingGas())

		// Verify mocks
		mockStore.AssertExpectations(t)
		mockMetrics.AssertExpectations(t)
	})

	t.Run("insufficient gas", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create existing allocation with small amount
		smallAmount := big.NewInt(100000)
		allocation := &models.Allocation{
			ID:          "test-allocation",
			UserAddress: userAddress,
			Amount:      smallAmount,
			Used:        big.NewInt(0),
			ExpiresAt:   time.Now().Add(12 * time.Hour),
			Status:      "active",
			LastUsedAt:  time.Now(),
		}

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(allocation, nil)
		mockAlerts.On("AlertLowGas", ctx, allocation.RemainingGas()).Return()

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call with amount larger than allocation
		tooLargeAmount := big.NewInt(200000)
		err := manager.UseGas(ctx, userAddress, tooLargeAmount)

		// Verify results
		require.Error(t, err)
		assert.Contains(t, err.Error(), "insufficient gas")

		// Verify allocation was not updated
		assert.Equal(t, big.NewInt(0), allocation.Used)

		// Verify mocks
		mockStore.AssertExpectations(t)
		mockAlerts.AssertExpectations(t)
	})

	t.Run("expired allocation", func(t *testing.T) {
		// Setup mocks
		mockStore := new(MockGasStore)
		mockMetrics := new(MockGasMetricsCollector)
		mockAlerts := new(MockGasAlertManager)

		// Create expired allocation
		allocation := &models.Allocation{
			ID:          "test-allocation",
			UserAddress: userAddress,
			Amount:      totalAmount,
			Used:        big.NewInt(0),
			ExpiresAt:   time.Now().Add(-1 * time.Hour),
			Status:      "expired",
			LastUsedAt:  time.Now(),
		}

		// Configure mock behavior
		mockStore.On("GetAllocation", ctx, userAddress).Return(allocation, nil)
		mockAlerts.On("AlertAllocationExpired", ctx, allocation).Return()

		// Create the allocation manager
		manager := NewAllocationManager(mockStore, policy, mockMetrics, mockAlerts)

		// Call the function
		err := manager.UseGas(ctx, userAddress, useAmount)

		// Verify results
		require.Error(t, err)
		assert.Contains(t, err.Error(), "allocation expired")

		// Verify allocation was not updated
		assert.Equal(t, big.NewInt(0), allocation.Used)

		// Verify mocks
		mockStore.AssertExpectations(t)
		mockAlerts.AssertExpectations(t)
	})
}
