package internal

import (
	"context"
	"errors"
	"fmt"
	"math/big"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
)

// AllocationManager handles gas allocation operations
type AllocationManager struct {
	store    GasStore
	policy   *models.GasUsagePolicy
	metrics  GasMetricsCollector
	alertMgr GasAlertManager
}

// NewAllocationManager creates a new AllocationManager
func NewAllocationManager(
	store GasStore,
	policy *models.GasUsagePolicy,
	metrics GasMetricsCollector,
	alertMgr GasAlertManager,
) *AllocationManager {
	return &AllocationManager{
		store:    store,
		policy:   policy,
		metrics:  metrics,
		alertMgr: alertMgr,
	}
}

// AllocateGas allocates gas to a user
func (am *AllocationManager) AllocateGas(
	ctx context.Context,
	userAddress util.Uint160,
	amount *big.Int,
) (*models.Allocation, error) {
	// Validate inputs
	if userAddress.Equals(util.Uint160{}) {
		return nil, errors.New("invalid user address")
	}
	if amount == nil || amount.Cmp(big.NewInt(0)) <= 0 {
		return nil, errors.New("invalid allocation amount")
	}

	// Check policy limits
	if am.policy != nil && amount.Cmp(am.policy.MaxAllocationPerUser) > 0 {
		return nil, fmt.Errorf("allocation exceeds maximum allowed: %s", am.policy.MaxAllocationPerUser.String())
	}

	// Check existing allocation
	existing, err := am.store.GetAllocation(ctx, userAddress)
	if err != nil {
		return nil, fmt.Errorf("failed to check existing allocation: %w", err)
	}

	// If there's an existing active allocation, return it
	if existing != nil && !existing.IsExpired() {
		return existing, nil
	}

	// Create new allocation with expiration based on policy
	var expiresAt time.Time
	if am.policy != nil {
		expiresAt = time.Now().Add(am.policy.MaxAllocationTime)
	} else {
		// Default to 24 hours if no policy
		expiresAt = time.Now().Add(24 * time.Hour)
	}

	// Create and save allocation
	allocation := &models.Allocation{
		UserAddress: userAddress,
		Amount:      amount,
		Used:        big.NewInt(0),
		ExpiresAt:   expiresAt,
		Status:      "active",
	}
	if err := am.store.SaveAllocation(ctx, allocation); err != nil {
		return nil, fmt.Errorf("failed to save allocation: %w", err)
	}

	// Record metrics
	if am.metrics != nil {
		am.metrics.RecordAllocation(ctx, allocation)
	}

	return allocation, nil
}

// ReleaseGas releases allocated gas
func (am *AllocationManager) ReleaseGas(ctx context.Context, userAddress util.Uint160) error {
	// Validate address
	if userAddress.Equals(util.Uint160{}) {
		return errors.New("invalid user address")
	}

	// Get current allocation
	allocation, err := am.store.GetAllocation(ctx, userAddress)
	if err != nil {
		return fmt.Errorf("failed to get allocation: %w", err)
	}

	// If no allocation, nothing to release
	if allocation == nil {
		return nil
	}

	// Delete the allocation
	if err := am.store.DeleteAllocation(ctx, userAddress); err != nil {
		return fmt.Errorf("failed to delete allocation: %w", err)
	}

	return nil
}

// GetAllocation gets a user's gas allocation
func (am *AllocationManager) GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error) {
	// Validate address
	if userAddress.Equals(util.Uint160{}) {
		return nil, errors.New("invalid user address")
	}

	// Get allocation from store
	allocation, err := am.store.GetAllocation(ctx, userAddress)
	if err != nil {
		return nil, fmt.Errorf("failed to get allocation: %w", err)
	}

	return allocation, nil
}

// UseGas records gas usage for a user
func (am *AllocationManager) UseGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) error {
	// Validate inputs
	if userAddress.Equals(util.Uint160{}) {
		return errors.New("invalid user address")
	}
	if amount == nil || amount.Cmp(big.NewInt(0)) <= 0 {
		return errors.New("invalid gas usage amount")
	}

	// Get allocation
	allocation, err := am.store.GetAllocation(ctx, userAddress)
	if err != nil {
		return fmt.Errorf("failed to get allocation: %w", err)
	}
	if allocation == nil {
		return errors.New("no gas allocation found")
	}

	// Check if expired
	if allocation.IsExpired() {
		if am.alertMgr != nil {
			am.alertMgr.AlertAllocationExpired(ctx, allocation)
		}
		return errors.New("gas allocation has expired")
	}

	// Check if there's enough gas
	remaining := allocation.RemainingGas()
	if remaining.Cmp(amount) < 0 {
		if am.alertMgr != nil {
			am.alertMgr.AlertLowGas(ctx, remaining)
		}
		return errors.New("insufficient gas in allocation")
	}

	// Record usage
	allocation.Used = new(big.Int).Add(allocation.Used, amount)

	// Save updated allocation
	if err := am.store.SaveAllocation(ctx, allocation); err != nil {
		return fmt.Errorf("failed to save updated allocation: %w", err)
	}

	// Record metrics
	if am.metrics != nil {
		am.metrics.RecordUsage(ctx, userAddress, amount)
	}

	// Check if refill needed
	if am.policy != nil && remaining.Cmp(am.policy.RefillThreshold) <= 0 {
		if am.alertMgr != nil {
			am.alertMgr.AlertLowGas(ctx, remaining)
		}
	}

	return nil
}
