package store

import (
	"context"
	"fmt"
	"math/big"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

func TestMemoryStore_Allocations(t *testing.T) {
	// Create test data
	ctx := context.Background()
	userAddress, err := util.Uint160DecodeStringLE("0123456789abcdef0123456789abcdef01234567")
	require.NoError(t, err)
	amount := big.NewInt(1000000)

	// Create a new memory store
	store := NewMemoryStore()
	defer store.Close()

	// Test saving an allocation
	t.Run("save allocation", func(t *testing.T) {
		// Create allocation
		allocation := &models.Allocation{
			ID:          "test-allocation",
			UserAddress: userAddress,
			Amount:      amount,
			Used:        big.NewInt(0),
			ExpiresAt:   time.Now().Add(24 * time.Hour),
			Status:      "active",
			LastUsedAt:  time.Now(),
		}

		// Save allocation
		err := store.SaveAllocation(ctx, allocation)
		require.NoError(t, err)

		// Retrieve allocation
		retrieved, err := store.GetAllocation(ctx, userAddress)
		require.NoError(t, err)
		require.NotNil(t, retrieved)

		// Compare allocations
		assert.Equal(t, allocation.ID, retrieved.ID)
		assert.Equal(t, allocation.UserAddress, retrieved.UserAddress)
		assert.Equal(t, allocation.Amount.String(), retrieved.Amount.String())
		assert.Equal(t, allocation.Status, retrieved.Status)
	})

	// Test retrieving a non-existent allocation
	t.Run("get non-existent allocation", func(t *testing.T) {
		nonExistentAddress, err := util.Uint160DecodeStringLE("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")
		require.NoError(t, err)

		retrieved, err := store.GetAllocation(ctx, nonExistentAddress)
		require.NoError(t, err)
		assert.Nil(t, retrieved)
	})

	// Test deleting an allocation
	t.Run("delete allocation", func(t *testing.T) {
		// Delete allocation
		err := store.DeleteAllocation(ctx, userAddress)
		require.NoError(t, err)

		// Verify it's gone
		retrieved, err := store.GetAllocation(ctx, userAddress)
		require.NoError(t, err)
		assert.Nil(t, retrieved)
	})

	// Test listing allocations
	t.Run("list allocations", func(t *testing.T) {
		// Create multiple allocations
		addresses := []util.Uint160{}
		for i := 0; i < 5; i++ {
			// Create a unique address for each allocation
			addrStr := fmt.Sprintf("%040x", 1000000+i) // Ensure 40 character hex string
			addr, err := util.Uint160DecodeStringLE(addrStr)
			require.NoError(t, err)
			addresses = append(addresses, addr)

			allocation := &models.Allocation{
				ID:          fmt.Sprintf("test-allocation-%d", i),
				UserAddress: addr,
				Amount:      big.NewInt(int64(i+1) * 100000),
				Used:        big.NewInt(0),
				ExpiresAt:   time.Now().Add(24 * time.Hour),
				Status:      "active",
				LastUsedAt:  time.Now(),
			}
			err = store.SaveAllocation(ctx, allocation)
			require.NoError(t, err)
		}

		// List allocations
		allocations, err := store.ListAllocations(ctx)
		require.NoError(t, err)
		assert.Len(t, allocations, 5)

		// Verify each allocation is in the list
		for _, addr := range addresses {
			found := false
			for _, allocation := range allocations {
				if allocation.UserAddress.Equals(addr) {
					found = true
					break
				}
			}
			assert.True(t, found, "Allocation for address %s not found", addr.StringLE())
		}
	})
}

func TestMemoryStore_Pool(t *testing.T) {
	// Create test data
	ctx := context.Background()
	initialAmount := big.NewInt(5000000)

	// Create a new memory store
	store := NewMemoryStore()
	defer store.Close()

	// Test saving a pool
	t.Run("save and get pool", func(t *testing.T) {
		// Create pool
		pool := models.NewGasPool(initialAmount)

		// Save pool
		err := store.SavePool(ctx, pool)
		require.NoError(t, err)

		// Retrieve pool
		retrieved, err := store.GetPool(ctx)
		require.NoError(t, err)
		require.NotNil(t, retrieved)

		// Compare pools
		assert.Equal(t, pool.Amount.String(), retrieved.Amount.String())
		assert.Equal(t, pool.RefillCount, retrieved.RefillCount)
	})

	// Test retrieving a non-existent pool
	t.Run("get non-existent pool", func(t *testing.T) {
		// Create a new store without a pool
		emptyStore := NewMemoryStore()
		defer emptyStore.Close()

		retrieved, err := emptyStore.GetPool(ctx)
		require.NoError(t, err)
		assert.Nil(t, retrieved)
	})

	// Test updating a pool
	t.Run("update pool", func(t *testing.T) {
		// Retrieve current pool
		pool, err := store.GetPool(ctx)
		require.NoError(t, err)
		require.NotNil(t, pool)

		// Update pool
		additionalAmount := big.NewInt(1000000)
		pool.Amount = new(big.Int).Add(pool.Amount, additionalAmount)
		pool.RefillCount++

		// Save updated pool
		err = store.SavePool(ctx, pool)
		require.NoError(t, err)

		// Retrieve updated pool
		retrieved, err := store.GetPool(ctx)
		require.NoError(t, err)
		require.NotNil(t, retrieved)

		// Compare pools
		expectedAmount := new(big.Int).Add(initialAmount, additionalAmount)
		assert.Equal(t, expectedAmount.String(), retrieved.Amount.String())
		assert.Equal(t, int64(1), retrieved.RefillCount)
	})
}

func TestMemoryStore_InvalidInput(t *testing.T) {
	// Create test data
	ctx := context.Background()
	invalidAddress := util.Uint160{}

	// Create a new memory store
	store := NewMemoryStore()
	defer store.Close()

	// Test invalid inputs
	t.Run("nil allocation", func(t *testing.T) {
		err := store.SaveAllocation(ctx, nil)
		require.Error(t, err)
		assert.Contains(t, err.Error(), "allocation cannot be nil")
	})

	t.Run("invalid address", func(t *testing.T) {
		err := store.SaveAllocation(ctx, &models.Allocation{UserAddress: invalidAddress})
		require.Error(t, err)
		assert.Contains(t, err.Error(), "invalid user address")

		_, err = store.GetAllocation(ctx, invalidAddress)
		require.Error(t, err)
		assert.Contains(t, err.Error(), "invalid user address")

		err = store.DeleteAllocation(ctx, invalidAddress)
		require.Error(t, err)
		assert.Contains(t, err.Error(), "invalid user address")
	})

	t.Run("nil pool", func(t *testing.T) {
		err := store.SavePool(ctx, nil)
		require.Error(t, err)
		assert.Contains(t, err.Error(), "pool cannot be nil")
	})
}

func TestMemoryStore_GetAllAllocations(t *testing.T) {
	ctx := context.Background()
	store := NewMemoryStore()

	// Create test addresses
	addr1, err := util.Uint160DecodeStringLE("0123456789abcdef0123456789abcdef01234567")
	require.NoError(t, err)
	addr2, err := util.Uint160DecodeStringLE("76543210fedcba9876543210fedcba9876543210")
	require.NoError(t, err)

	// Test with empty store
	allocations, err := store.GetAllAllocations(ctx)
	require.NoError(t, err)
	assert.Empty(t, allocations)

	// Add allocations
	allocation1 := &models.Allocation{
		ID:          "test-id-1",
		UserAddress: addr1,
		Amount:      big.NewInt(1000000),
		Status:      "active",
	}
	allocation2 := &models.Allocation{
		ID:          "test-id-2",
		UserAddress: addr2,
		Amount:      big.NewInt(2000000),
		Status:      "inactive",
	}

	require.NoError(t, store.SaveAllocation(ctx, allocation1))
	require.NoError(t, store.SaveAllocation(ctx, allocation2))

	// Test retrieval
	allocations, err = store.GetAllAllocations(ctx)
	require.NoError(t, err)
	assert.Len(t, allocations, 2)

	// Verify allocations are found regardless of status
	found1, found2 := false, false
	for _, a := range allocations {
		if a.ID == "test-id-1" {
			found1 = true
			assert.Equal(t, addr1, a.UserAddress)
			assert.Equal(t, "active", a.Status)
		}
		if a.ID == "test-id-2" {
			found2 = true
			assert.Equal(t, addr2, a.UserAddress)
			assert.Equal(t, "inactive", a.Status)
		}
	}
	assert.True(t, found1, "First allocation not found")
	assert.True(t, found2, "Second allocation not found")

	// Test that it returns the same as ListAllocations in this implementation
	listResult, err := store.ListAllocations(ctx)
	require.NoError(t, err)
	getAllResult, err := store.GetAllAllocations(ctx)
	require.NoError(t, err)
	assert.Equal(t, len(listResult), len(getAllResult))
}
