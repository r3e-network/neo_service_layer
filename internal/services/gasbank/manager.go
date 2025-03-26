package gasbank

import (
	"context"
	"fmt"
	"math/big"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/core/neo"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

// GasManager manages gas allocations and usage
type GasManager struct {
	allocations sync.Map // map[string]*models.Allocation
	policy      *models.GasUsagePolicy
	neoClient   *neo.Client
	mu          sync.RWMutex
}

// NewGasManager creates a new gas manager
func NewGasManager(policy *models.GasUsagePolicy, neoClient *neo.Client) *GasManager {
	return &GasManager{
		policy:    policy,
		neoClient: neoClient,
	}
}

// AllocateGas allocates gas for a user
func (gm *GasManager) AllocateGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*models.Allocation, error) {
	if amount.Cmp(gm.policy.MinAllocationAmount) < 0 {
		return nil, fmt.Errorf("allocation amount too small: minimum %s", gm.policy.MinAllocationAmount.String())
	}

	if amount.Cmp(gm.policy.MaxAllocationPerUser) > 0 {
		return nil, fmt.Errorf("allocation amount too large: maximum %s", gm.policy.MaxAllocationPerUser.String())
	}

	allocation := &models.Allocation{
		ID:          uuid.New().String(),
		UserAddress: userAddress,
		Amount:      amount,
		Used:        big.NewInt(0),
		Status:      "active",
		ExpiresAt:   time.Now().Add(gm.policy.MaxAllocationTime),
	}
	gm.allocations.Store(userAddress.String(), allocation)

	return allocation, nil
}

// ReleaseGas releases allocated gas back to the pool
func (gm *GasManager) ReleaseGas(ctx context.Context, userAddress util.Uint160) error {
	value, ok := gm.allocations.Load(userAddress.String())
	if !ok {
		return fmt.Errorf("no active allocation found for user")
	}

	allocation := value.(*models.Allocation)
	allocation.Status = "released"
	gm.allocations.Delete(userAddress.String())

	return nil
}

// UseGas records gas usage for a user
func (gm *GasManager) UseGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) error {
	value, ok := gm.allocations.Load(userAddress.String())
	if !ok {
		return fmt.Errorf("no active allocation found for user")
	}

	allocation := value.(*models.Allocation)
	if allocation.Used == nil {
		allocation.Used = new(big.Int).SetInt64(0)
	}

	if allocation.RemainingGas().Cmp(amount) < 0 {
		return fmt.Errorf("insufficient gas: available %s, requested %s",
			allocation.RemainingGas().String(), amount.String())
	}

	if err := allocation.UseGas(amount); err != nil {
		return fmt.Errorf("failed to use gas: %w", err)
	}

	gm.allocations.Store(userAddress.String(), allocation)
	return nil
}

// GetAllocation gets the current gas allocation for a user
func (gm *GasManager) GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error) {
	value, ok := gm.allocations.Load(userAddress.String())
	if !ok {
		return nil, fmt.Errorf("no active allocation found for user")
	}

	allocation := value.(*models.Allocation)
	if allocation.IsExpired() {
		gm.allocations.Delete(userAddress.String())
		return nil, fmt.Errorf("allocation has expired")
	}

	return allocation, nil
}

// RefillGas refills the gas pool
func (gm *GasManager) RefillGas(ctx context.Context) error {
	gm.mu.Lock()
	defer gm.mu.Unlock()

	// Implementation would interact with Neo blockchain to refill gas
	return nil
}

// CleanupExpiredAllocations removes expired allocations
func (gm *GasManager) CleanupExpiredAllocations() {
	gm.allocations.Range(func(key, value interface{}) bool {
		allocation := value.(*models.Allocation)
		if allocation.IsExpired() {
			gm.allocations.Delete(key)
		}
		return true
	})
}
