package internal

import (
	"context"
	"errors"
	"math/big"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

// GasValidatorImpl implements the GasValidator interface
type GasValidatorImpl struct {
	policy *models.GasUsagePolicy
}

// NewGasValidator creates a new GasValidator
func NewGasValidator(policy *models.GasUsagePolicy) GasValidator {
	return &GasValidatorImpl{
		policy: policy,
	}
}

// ValidateAllocation validates a gas allocation request
func (v *GasValidatorImpl) ValidateAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int) error {
	if amount == nil {
		return errors.New("amount cannot be nil")
	}
	if amount.Sign() <= 0 {
		return errors.New("amount must be positive")
	}
	if amount.Cmp(v.policy.MaxAllocationPerUser) > 0 {
		return errors.New("amount exceeds maximum allowed")
	}
	if amount.Cmp(v.policy.MinAllocationAmount) < 0 {
		return errors.New("amount below minimum allowed")
	}
	return nil
}

// ValidateUsage validates gas usage
func (v *GasValidatorImpl) ValidateUsage(ctx context.Context, allocation *models.Allocation, amount *big.Int) error {
	if allocation == nil {
		return errors.New("allocation cannot be nil")
	}
	if amount == nil {
		return errors.New("amount cannot be nil")
	}
	if amount.Sign() <= 0 {
		return errors.New("amount must be positive")
	}
	if allocation.IsExpired() {
		return errors.New("allocation has expired")
	}
	remaining := allocation.RemainingGas()
	if remaining.Cmp(amount) < 0 {
		return errors.New("insufficient gas in allocation")
	}
	return nil
}

// ValidateRefill validates a gas refill request
func (v *GasValidatorImpl) ValidateRefill(ctx context.Context, amount *big.Int) error {
	if amount == nil {
		return errors.New("amount cannot be nil")
	}
	if amount.Sign() <= 0 {
		return errors.New("amount must be positive")
	}
	if amount.Cmp(v.policy.RefillAmount) > 0 {
		return errors.New("amount exceeds maximum refill amount")
	}
	return nil
}
