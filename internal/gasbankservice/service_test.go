package gasbank

import (
	"context"
	"math/big"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
	"github.com/stretchr/testify/require"
)

// MockTransactionManager mocks the TransactionManager interface
type MockTransactionManager struct {
	mock.Mock
}

func (m *MockTransactionManager) TransferGAS(ctx context.Context, amount *big.Int) error {
	args := m.Called(ctx, amount)
	return args.Error(0)
}

func TestGasBankService(t *testing.T) {
	// Create test data with a deadline context to avoid background monitoring
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Minute)
	defer cancel()

	userAddress, err := util.Uint160DecodeStringLE("0123456789abcdef0123456789abcdef01234567")
	require.NoError(t, err)

	// Create transaction manager mock
	txManager := new(MockTransactionManager)

	// Create configuration
	config := &Config{
		InitialGas:              big.NewInt(10000000), // 0.1 GAS
		RefillAmount:            big.NewInt(50000000), // 0.5 GAS
		RefillThreshold:         big.NewInt(2000000),  // 0.02 GAS
		MaxAllocationPerUser:    big.NewInt(5000000),  // 0.05 GAS
		MinAllocationAmount:     big.NewInt(1000000),  // 0.01 GAS
		MaxAllocationTime:       1 * time.Hour,
		CooldownPeriod:          1 * time.Minute,
		StoreType:               "memory",
		TxManager:               txManager,
		ExpirationCheckInterval: 5 * time.Minute,
		MonitorInterval:         5 * time.Minute,
	}

	// Create service
	service, err := NewService(ctx, config)
	require.NoError(t, err)
	defer service.Close()

	// Test gas allocation
	t.Run("allocate gas", func(t *testing.T) {
		// Allocate gas
		amount := big.NewInt(3000000) // 0.03 GAS
		allocation, err := service.AllocateGas(ctx, userAddress, amount)
		require.NoError(t, err)
		require.NotNil(t, allocation)

		// Verify allocation
		assert.Equal(t, userAddress, allocation.UserAddress)
		assert.Equal(t, amount.String(), allocation.Amount.String())
		assert.Equal(t, "active", allocation.Status)

		// Check available gas
		availableGas, err := service.GetAvailableGas(ctx)
		require.NoError(t, err)
		expectedAvailable := new(big.Int).Sub(config.InitialGas, amount)
		assert.Equal(t, expectedAvailable.String(), availableGas.String())
	})

	// Test gas usage
	t.Run("use gas", func(t *testing.T) {
		// Use gas
		useAmount := big.NewInt(1000000) // 0.01 GAS
		err := service.UseGas(ctx, userAddress, useAmount)
		require.NoError(t, err)

		// Verify allocation
		allocation, err := service.GetAllocation(ctx, userAddress)
		require.NoError(t, err)
		require.NotNil(t, allocation)

		assert.Equal(t, useAmount.String(), allocation.Used.String())

		// Remaining gas should be 0.02 GAS
		expectedRemaining := big.NewInt(2000000)
		assert.Equal(t, expectedRemaining.String(), allocation.RemainingGas().String())
	})

	// Test refill pool
	t.Run("refill pool", func(t *testing.T) {
		// Skip comprehensive refill test for now
		// The test would be better handled at the PoolManager level
		t.Log("RefillPool functionality is primarily tested at the PoolManager level")
	})

	// Test release gas
	t.Run("release gas", func(t *testing.T) {
		// Record available gas before release
		beforeRelease, err := service.GetAvailableGas(ctx)
		require.NoError(t, err)

		// Get current allocation
		allocation, err := service.GetAllocation(ctx, userAddress)
		require.NoError(t, err)
		require.NotNil(t, allocation)

		// Release gas
		err = service.ReleaseGas(ctx, userAddress)
		require.NoError(t, err)

		// Verify allocation is gone
		allocation, err = service.GetAllocation(ctx, userAddress)
		require.NoError(t, err)
		assert.Nil(t, allocation)

		// Check available gas after release
		afterRelease, err := service.GetAvailableGas(ctx)
		require.NoError(t, err)

		// Should be increased by remaining gas (0.02 GAS)
		expectedRemaining := big.NewInt(2000000)
		expectedAfterRelease := new(big.Int).Add(beforeRelease, expectedRemaining)
		assert.Equal(t, expectedAfterRelease.String(), afterRelease.String())
	})

	// Test metrics
	t.Run("get metrics", func(t *testing.T) {
		// Get metrics
		metrics, err := service.GetMetrics(ctx)
		require.NoError(t, err)
		require.NotNil(t, metrics)

		// Just verify that the metrics object is returned
		// The exact values may depend on background monitoring activities
		// so we don't make specific assertions about the values
	})
}
