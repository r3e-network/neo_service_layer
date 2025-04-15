package internal

import (
	"context"
	"sync"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
)

// GasStoreImpl implements the GasStore interface
type GasStoreImpl struct {
	allocations sync.Map // map[string]*models.Allocation
	pool        *models.GasPool
	mu          sync.RWMutex
}

// NewGasStore creates a new GasStore instance
func NewGasStore() GasStore {
	return &GasStoreImpl{}
}

// SaveAllocation saves a gas allocation to the store
func (gs *GasStoreImpl) SaveAllocation(ctx context.Context, allocation *models.Allocation) error {
	gs.allocations.Store(allocation.UserAddress.String(), allocation)
	return nil
}

// GetAllocation gets a gas allocation from the store
func (gs *GasStoreImpl) GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error) {
	value, ok := gs.allocations.Load(userAddress.String())
	if !ok {
		return nil, nil
	}
	return value.(*models.Allocation), nil
}

// DeleteAllocation deletes a gas allocation from the store
func (gs *GasStoreImpl) DeleteAllocation(ctx context.Context, userAddress util.Uint160) error {
	gs.allocations.Delete(userAddress.String())
	return nil
}

// ListAllocations lists all gas allocations
func (gs *GasStoreImpl) ListAllocations(ctx context.Context) ([]*models.Allocation, error) {
	var allocations []*models.Allocation
	gs.allocations.Range(func(key, value interface{}) bool {
		allocations = append(allocations, value.(*models.Allocation))
		return true
	})
	return allocations, nil
}

// GetPool gets the gas pool
func (gs *GasStoreImpl) GetPool(ctx context.Context) (*models.GasPool, error) {
	gs.mu.RLock()
	defer gs.mu.RUnlock()
	return gs.pool, nil
}

// SavePool saves the gas pool
func (gs *GasStoreImpl) SavePool(ctx context.Context, pool *models.GasPool) error {
	gs.mu.Lock()
	defer gs.mu.Unlock()
	gs.pool = pool
	return nil
}
