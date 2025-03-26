package internal

import (
	"math/big"
	"time"

	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

// HasSufficientGas checks if the pool has at least the specified amount of gas
func HasSufficientGas(pool *models.GasPool, amount *big.Int) bool {
	if pool == nil || pool.Amount == nil || amount == nil || amount.Sign() <= 0 {
		return false
	}
	return pool.Amount.Cmp(amount) >= 0
}

// NeedsRefill checks if the pool needs a refill based on the current amount and threshold
func NeedsRefill(pool *models.GasPool, threshold *big.Int) bool {
	if pool == nil || pool.Amount == nil || threshold == nil {
		return true
	}
	// Pool needs refill if available gas is less than threshold
	return pool.Amount.Cmp(threshold) < 0
}

// CanRefill checks if the pool can be refilled based on the cooldown period
func CanRefill(pool *models.GasPool, cooldownPeriod time.Duration) bool {
	if pool == nil {
		return true
	}
	return pool.LastRefill.IsZero() || time.Since(pool.LastRefill) > cooldownPeriod
}
