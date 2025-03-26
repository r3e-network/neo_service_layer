package internal

import (
	"context"
	"errors"
	"fmt"
	"math/big"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/core/neo"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
	"github.com/will/neo_service_layer/internal/services/gasbank/store"
)

var (
	ErrInsufficientGas = errors.New("insufficient gas in pool")
)

const (
	AllocationStatusActive = "active"
)

// GasManagerImpl implements the GasManager interface
type GasManagerImpl struct {
	store     GasStore
	metrics   GasMetricsCollector
	alerts    GasAlertManager
	validator GasValidator
	policy    *models.GasUsagePolicy
	neoClient *neo.Client
}

// NewGasManager creates a new GasManager
func NewGasManager(
	store GasStore,
	metrics GasMetricsCollector,
	alerts GasAlertManager,
	validator GasValidator,
	policy *models.GasUsagePolicy,
	neoClient *neo.Client,
) GasManager {
	return &GasManagerImpl{
		store:     store,
		metrics:   metrics,
		alerts:    alerts,
		validator: validator,
		policy:    policy,
		neoClient: neoClient,
	}
}

// AllocateGas allocates gas for a user
func (gm *GasManagerImpl) AllocateGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*models.Allocation, error) {
	// Check if user already has an allocation
	existingAllocation, err := gm.store.GetAllocation(ctx, userAddress)
	if err != nil && !errors.Is(err, store.ErrNotFound) {
		return nil, fmt.Errorf("failed to check existing allocation: %w", err)
	}

	// If allocation exists, return it
	if existingAllocation != nil {
		return existingAllocation, nil
	}

	// Get current gas pool
	pool, err := gm.store.GetPool(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to get gas pool: %w", err)
	}

	// Validate pool has enough gas
	if pool.Amount.Cmp(amount) < 0 {
		return nil, ErrInsufficientGas
	}

	// Create new allocation
	allocation := &models.Allocation{
		ID:          uuid.New().String(),
		UserAddress: userAddress,
		Amount:      amount,
		Used:        big.NewInt(0),
		Status:      AllocationStatusActive,
		ExpiresAt:   time.Now().Add(gm.policy.MaxAllocationTime),
	}

	// Save allocation
	if err := gm.store.SaveAllocation(ctx, allocation); err != nil {
		return nil, fmt.Errorf("failed to save allocation: %w", err)
	}

	// Update pool
	pool.Amount = new(big.Int).Sub(pool.Amount, amount)
	if err := gm.store.SavePool(ctx, pool); err != nil {
		return nil, fmt.Errorf("failed to update pool: %w", err)
	}

	// Record metrics
	gm.metrics.RecordAllocation(ctx, allocation)

	// Alert if large allocation
	gm.alerts.AlertLargeAllocation(ctx, allocation)

	return allocation, nil
}

// ReleaseGas releases allocated gas back to the pool
func (gm *GasManagerImpl) ReleaseGas(ctx context.Context, userAddress util.Uint160) error {
	// Get current allocation
	allocation, err := gm.store.GetAllocation(ctx, userAddress)
	if err != nil {
		return fmt.Errorf("failed to get allocation: %w", err)
	}

	if allocation == nil {
		// No allocation to release
		return nil
	}

	// Get current pool
	pool, err := gm.store.GetPool(ctx)
	if err != nil {
		return fmt.Errorf("failed to get gas pool: %w", err)
	}

	if pool == nil {
		// This shouldn't happen, but handle it gracefully
		pool = &models.GasPool{
			Amount:        new(big.Int).Set(gm.policy.MaxAllocationPerUser),
			LastRefill:    time.Now(),
			RefillCount:   0,
			FailedRefills: 0,
		}
	}

	// Calculate unused gas
	unusedGas := allocation.RemainingGas()

	// Update pool
	pool.Amount = new(big.Int).Add(pool.Amount, unusedGas)

	// Save updated pool
	if err := gm.store.SavePool(ctx, pool); err != nil {
		return fmt.Errorf("failed to update gas pool: %w", err)
	}

	// Delete allocation
	if err := gm.store.DeleteAllocation(ctx, userAddress); err != nil {
		return fmt.Errorf("failed to delete allocation: %w", err)
	}

	return nil
}

// UseGas records gas usage for a user
func (gm *GasManagerImpl) UseGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) error {
	// Get allocation
	allocation, err := gm.store.GetAllocation(ctx, userAddress)
	if err != nil {
		return fmt.Errorf("failed to get allocation: %w", err)
	}

	// Validate usage
	if err := gm.validator.ValidateUsage(ctx, allocation, amount); err != nil {
		return err
	}

	// Update allocation
	if err := allocation.UseGas(amount); err != nil {
		return fmt.Errorf("failed to use gas: %w", err)
	}

	// Save allocation
	if err := gm.store.SaveAllocation(ctx, allocation); err != nil {
		return fmt.Errorf("failed to save allocation: %w", err)
	}

	return nil
}

// GetAllocation gets the current gas allocation for a user
func (gm *GasManagerImpl) GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error) {
	allocation, err := gm.store.GetAllocation(ctx, userAddress)
	if err != nil {
		return nil, fmt.Errorf("failed to get allocation: %w", err)
	}
	return allocation, nil
}

// RefillGas refills the gas pool
func (gm *GasManagerImpl) RefillGas(ctx context.Context) error {
	// Get current pool
	pool, err := gm.store.GetPool(ctx)
	if err != nil {
		return fmt.Errorf("failed to get gas pool: %w", err)
	}

	// Create pool if it doesn't exist
	if pool == nil {
		pool = &models.GasPool{
			Amount:        new(big.Int).Set(gm.policy.MaxAllocationPerUser),
			LastRefill:    time.Now(),
			RefillCount:   0,
			FailedRefills: 0,
		}
	}

	// Check if refill is needed
	if !NeedsRefill(pool, gm.policy.RefillThreshold) {
		return nil
	}

	// Check if refill is allowed
	if !CanRefill(pool, gm.policy.CooldownPeriod) {
		return fmt.Errorf("cannot refill during cooldown period")
	}

	// Add refill amount to pool
	pool.Amount = new(big.Int).Add(pool.Amount, gm.policy.RefillAmount)
	pool.LastRefill = time.Now()
	pool.RefillCount++

	// Save updated pool
	if err := gm.store.SavePool(ctx, pool); err != nil {
		return fmt.Errorf("failed to save updated pool: %w", err)
	}

	return nil
}

// GetAvailableGas gets the available gas in the pool
func (gm *GasManagerImpl) RefillAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int) error {
	// Get allocation
	allocation, err := gm.store.GetAllocation(ctx, userAddress)
	if err != nil {
		return fmt.Errorf("failed to get allocation: %w", err)
	}

	// Get pool
	pool, err := gm.store.GetPool(ctx)
	if err != nil {
		return fmt.Errorf("failed to get pool: %w", err)
	}

	// Validate pool has enough gas
	if pool.Amount.Cmp(amount) < 0 {
		return ErrInsufficientGas
	}

	// Update allocation
	if err := allocation.Refill(amount); err != nil {
		return fmt.Errorf("failed to refill allocation: %w", err)
	}

	// Save allocation
	if err := gm.store.SaveAllocation(ctx, allocation); err != nil {
		return fmt.Errorf("failed to save allocation: %w", err)
	}

	// Update pool
	pool.Amount = new(big.Int).Sub(pool.Amount, amount)
	if err := gm.store.SavePool(ctx, pool); err != nil {
		return fmt.Errorf("failed to update pool: %w", err)
	}

	return nil
}

// Start starts the allocation manager
func (am *AllocationManager) Start(ctx context.Context) error {
	return nil
}

// Stop stops the allocation manager
func (am *AllocationManager) Stop(ctx context.Context) error {
	return nil
}
