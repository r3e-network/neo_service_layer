package gasbank

import (
	"context"
	"math/big"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

// IService defines the interface for the GasBank service
type IService interface {
	// Start starts the service
	Start(ctx context.Context) error

	// Stop stops the service
	Stop(ctx context.Context) error

	// GetAllocation gets the current gas allocation for a user
	GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error)

	// RequestAllocation requests a new gas allocation for a user
	RequestAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*models.Allocation, error)

	// ReleaseAllocation releases a user's gas allocation
	ReleaseAllocation(ctx context.Context, userAddress util.Uint160) error

	// AllocateGas allocates gas to a user
	AllocateGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*models.Allocation, error)

	// ReleaseGas releases gas from a user
	ReleaseGas(ctx context.Context, userAddress util.Uint160) error
}

// Allocation represents a gas allocation
type Allocation struct {
	// ID is the unique identifier for this allocation
	ID string

	// UserAddress is the address of the user who owns this allocation
	UserAddress util.Uint160

	// Amount is the total amount of gas allocated
	Amount *big.Int

	// Used is the amount of gas used so far
	Used *big.Int

	// ExpiresAt is when this allocation expires
	ExpiresAt time.Time

	// Status is the current status of the allocation
	Status string
}

// RemainingGas returns the amount of gas remaining in the allocation
func (a *Allocation) RemainingGas() *big.Int {
	if a.Amount == nil || a.Used == nil {
		return big.NewInt(0)
	}
	remaining := new(big.Int).Sub(a.Amount, a.Used)
	if remaining.Sign() < 0 {
		return big.NewInt(0)
	}
	return remaining
}

// IsExpired returns true if the allocation has expired
func (a *Allocation) IsExpired() bool {
	return time.Now().After(a.ExpiresAt)
}
