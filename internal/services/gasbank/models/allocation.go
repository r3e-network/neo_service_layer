// Package models contains the models for the gasbank service
package models

import (
	"math/big"
	"time"

	"errors"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

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

	// Transactions is a list of transaction hashes associated with this allocation
	Transactions []string

	// LastUsedAt is when this allocation was last used
	LastUsedAt time.Time
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

// UseGas updates the allocation's used gas amount
func (a *Allocation) UseGas(amount *big.Int) error {
	if amount == nil {
		return errors.New("amount cannot be nil")
	}
	if amount.Sign() <= 0 {
		return errors.New("amount must be positive")
	}
	if a.Used == nil {
		a.Used = big.NewInt(0)
	}
	a.Used.Add(a.Used, amount)
	a.LastUsedAt = time.Now()
	return nil
}

// Refill adds more gas to the allocation
func (a *Allocation) Refill(amount *big.Int) error {
	if amount == nil {
		return errors.New("amount cannot be nil")
	}
	if amount.Sign() <= 0 {
		return errors.New("amount must be positive")
	}
	if a.Amount == nil {
		a.Amount = big.NewInt(0)
	}
	a.Amount.Add(a.Amount, amount)
	return nil
}
