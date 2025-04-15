package internal

import (
	"context"
	"errors"
	"fmt"
	"math/big"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
)

// PoolManager is responsible for managing the gas pool
type PoolManager struct {
	store     GasStore
	txManager TransactionManager
	policy    *models.GasUsagePolicy
	metrics   GasMetricsCollector
	alerts    GasAlertManager
}

// TransactionManager handles blockchain transactions
type TransactionManager interface {
	// TransferGAS transfers GAS to the specified address
	TransferGAS(ctx context.Context, amount *big.Int) error
}

// NewPoolManager creates a new pool manager
func NewPoolManager(
	store GasStore,
	txManager TransactionManager,
	policy *models.GasUsagePolicy,
	metrics GasMetricsCollector,
	alerts GasAlertManager,
) *PoolManager {
	return &PoolManager{
		store:     store,
		txManager: txManager,
		policy:    policy,
		metrics:   metrics,
		alerts:    alerts,
	}
}

// GetAvailableGas returns the amount of gas currently available in the pool
func (pm *PoolManager) GetAvailableGas(ctx context.Context) (*big.Int, error) {
	pool, err := pm.getOrCreatePool(ctx)
	if err != nil {
		return nil, err
	}

	return new(big.Int).Set(pool.Amount), nil
}

// ConsumeGas consumes gas from the pool
func (pm *PoolManager) ConsumeGas(ctx context.Context, amount *big.Int) error {
	if amount == nil || amount.Sign() <= 0 {
		return errors.New("invalid amount")
	}

	pool, err := pm.getOrCreatePool(ctx)
	if err != nil {
		return err
	}

	// Check if there's enough gas in the pool
	if pool.Amount.Cmp(amount) < 0 {
		pm.alerts.AlertLowGas(ctx, pool.Amount)
		return fmt.Errorf("insufficient gas in pool: available=%s, requested=%s",
			pool.Amount.String(), amount.String())
	}

	// Consume gas
	pool.Amount = new(big.Int).Sub(pool.Amount, amount)

	// Save updated pool
	return pm.store.SavePool(ctx, pool)
}

// AddGas adds gas to the pool
func (pm *PoolManager) AddGas(ctx context.Context, amount *big.Int) error {
	if amount == nil || amount.Sign() <= 0 {
		return errors.New("invalid amount")
	}

	pool, err := pm.getOrCreatePool(ctx, amount)
	if err != nil {
		return err
	}

	// Add gas
	pool.Amount = new(big.Int).Add(pool.Amount, amount)

	// Save updated pool
	return pm.store.SavePool(ctx, pool)
}

// RefillPool refills the gas pool if needed
func (pm *PoolManager) RefillPool(ctx context.Context) error {
	pool, err := pm.getOrCreatePool(ctx)
	if err != nil {
		return err
	}

	// Check if refill is needed
	if !pool.NeedsRefill(pm.policy.RefillThreshold) {
		return nil
	}

	// Check if cooldown period has elapsed
	if !pool.CanRefill(pm.policy.CooldownPeriod) {
		return nil
	}

	// Attempt to refill
	refillAmount := pm.policy.RefillAmount
	err = pm.txManager.TransferGAS(ctx, refillAmount)
	if err != nil {
		pm.alerts.AlertFailedRefill(ctx, refillAmount, err.Error())
		pm.metrics.RecordRefill(ctx, refillAmount, false)
		pool.FailedRefills++
		_ = pm.store.SavePool(ctx, pool) // Best effort save
		return fmt.Errorf("failed to refill gas pool: %w", err)
	}

	// Update pool
	pool.Amount = new(big.Int).Add(pool.Amount, refillAmount)
	pool.LastRefill = time.Now()
	pool.RefillCount++

	// Save updated pool
	err = pm.store.SavePool(ctx, pool)
	if err != nil {
		return fmt.Errorf("failed to save gas pool after refill: %w", err)
	}

	// Record metrics
	pm.metrics.RecordRefill(ctx, refillAmount, true)

	return nil
}

// ReturnGas returns gas to the pool (alias for AddGas)
func (pm *PoolManager) ReturnGas(ctx context.Context, amount *big.Int) error {
	return pm.AddGas(ctx, amount)
}

// getOrCreatePool gets the gas pool from storage or creates a new one if it doesn't exist
func (pm *PoolManager) getOrCreatePool(ctx context.Context, initialGas ...*big.Int) (*models.GasPool, error) {
	pool, err := pm.store.GetPool(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to get gas pool: %w", err)
	}

	if pool == nil {
		// Create a new pool if none exists
		initialAmount := big.NewInt(0)
		if len(initialGas) > 0 && initialGas[0] != nil {
			initialAmount = initialGas[0]
		}
		pool = &models.GasPool{
			Amount:        initialAmount,
			LastRefill:    time.Now(),
			RefillCount:   0,
			FailedRefills: 0,
		}
		err = pm.store.SavePool(ctx, pool)
		if err != nil {
			return nil, fmt.Errorf("failed to create new gas pool: %w", err)
		}
	}

	return pool, nil
}

// Start starts the pool manager
func (pm *PoolManager) Start(ctx context.Context) error {
	return nil
}

// Stop stops the pool manager
func (pm *PoolManager) Stop(ctx context.Context) error {
	return nil
}
