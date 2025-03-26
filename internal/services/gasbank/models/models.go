package models

import (
	"math/big"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
)

// GasAllocation represents a gas allocation for a user
type GasAllocation struct {
	ID             string
	UserAddress    util.Uint160
	Amount         *big.Int
	Used           *big.Int
	AllocatedAt    time.Time
	ExpiresAt      time.Time
	Status         string
	LastUsedAt     time.Time
	LastRefilledAt time.Time
	Transactions   []string
}

// NewGasAllocation creates a new gas allocation
func NewGasAllocation(userAddress util.Uint160, amount *big.Int, expiresAt time.Time) *GasAllocation {
	now := time.Now()
	return &GasAllocation{
		ID:             uuid.New().String(),
		UserAddress:    userAddress,
		Amount:         new(big.Int).Set(amount),
		Used:           new(big.Int).SetInt64(0),
		AllocatedAt:    now,
		ExpiresAt:      expiresAt,
		Status:         "active",
		LastUsedAt:     now,
		LastRefilledAt: now,
		Transactions:   make([]string, 0),
	}
}

// IsExpired checks if the allocation has expired
func (a *GasAllocation) IsExpired() bool {
	return time.Now().After(a.ExpiresAt)
}

// RemainingGas returns the remaining gas in the allocation
func (a *GasAllocation) RemainingGas() *big.Int {
	if a.Used == nil {
		return new(big.Int).Set(a.Amount)
	}
	return new(big.Int).Sub(a.Amount, a.Used)
}

// UseGas attempts to use the specified amount of gas
func (a *GasAllocation) UseGas(amount *big.Int) bool {
	if amount.Cmp(a.RemainingGas()) > 0 {
		return false
	}
	a.Used = new(big.Int).Add(a.Used, amount)
	a.LastUsedAt = time.Now()
	return true
}

// Refill adds more gas to the allocation
func (a *GasAllocation) Refill(amount *big.Int) {
	a.Amount = new(big.Int).Add(a.Amount, amount)
	a.LastRefilledAt = time.Now()
}

// GasPool represents the gas pool
type GasPool struct {
	Amount        *big.Int  `json:"amount"`
	LastRefill    time.Time `json:"last_refill"`
	RefillCount   int64     `json:"refill_count"`
	FailedRefills int64     `json:"failed_refills"`
}

// NewGasPool creates a new gas pool with the specified initial amount
func NewGasPool(initialAmount *big.Int) *GasPool {
	return &GasPool{
		Amount:        new(big.Int).Set(initialAmount),
		LastRefill:    time.Now(),
		RefillCount:   0,
		FailedRefills: 0,
	}
}

// HasSufficientGas checks if the pool has at least the specified amount of gas
func (pool *GasPool) HasSufficientGas(amount *big.Int) bool {
	if pool.Amount == nil || amount == nil || amount.Sign() <= 0 {
		return false
	}
	return pool.Amount.Cmp(amount) >= 0
}

// NeedsRefill checks if the pool needs a refill based on the current amount and threshold
func (pool *GasPool) NeedsRefill(threshold *big.Int) bool {
	if pool.Amount == nil || threshold == nil {
		return true
	}
	return pool.Amount.Cmp(threshold) < 0
}

// CanRefill checks if the pool can be refilled based on the cooldown period
func (pool *GasPool) CanRefill(cooldownPeriod time.Duration) bool {
	return pool.LastRefill.IsZero() || time.Since(pool.LastRefill) > cooldownPeriod
}

// AvailableGas returns the current available gas in the pool
func (pool *GasPool) AvailableGas() *big.Int {
	if pool.Amount == nil {
		return big.NewInt(0)
	}
	return new(big.Int).Set(pool.Amount)
}

// TotalGas returns the total gas that has been in the pool
func (pool *GasPool) TotalGas() *big.Int {
	return pool.AvailableGas()
}

// GasUsagePolicy represents the policy for gas usage
type GasUsagePolicy struct {
	MaxAllocationPerUser *big.Int
	MinAllocationAmount  *big.Int
	MaxAllocationTime    time.Duration
	RefillThreshold      *big.Int
	RefillAmount         *big.Int
	CooldownPeriod       time.Duration
}

// GasUsageMetrics represents metrics for gas usage
type GasUsageMetrics struct {
	TotalAllocated *big.Int
	TotalUsed      *big.Int
	ActiveUsers    int
	Refills        int
	FailedRefills  int
}