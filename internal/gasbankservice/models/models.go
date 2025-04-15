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

// Config represents the GasBank service configuration
type Config struct {
	// Pool Management
	InitialGas      *big.Int      `yaml:"initialGas"`
	RefillAmount    *big.Int      `yaml:"refillAmount"`
	RefillThreshold *big.Int      `yaml:"refillThreshold"`
	CooldownPeriod  time.Duration `yaml:"cooldownPeriod"`

	// Temporary Allocation Policy (Existing)
	MaxAllocationPerUser    *big.Int      `yaml:"maxAllocationPerUser"`
	MinAllocationAmount     *big.Int      `yaml:"minAllocationAmount"`
	MaxAllocationTime       time.Duration `yaml:"maxAllocationTime"`
	ExpirationCheckInterval time.Duration `yaml:"expirationCheckInterval"`

	// Persistent Balances & Fees (New)
	EnableUserBalances bool     `yaml:"enableUserBalances"` // Flag to enable persistent balances
	MinDepositAmount   *big.Int `yaml:"minDepositAmount"`
	WithdrawalFee      *big.Int `yaml:"withdrawalFee"`
	// Add policy configurations for fee payments?

	// General
	StoreType       string        `yaml:"storeType"` // memory, badger, etc.
	StorePath       string        `yaml:"storePath"`
	MonitorInterval time.Duration `yaml:"monitorInterval"`
	// AlertConfig     *alerting.AlertConfig `yaml:"alertConfig"` // Assuming alerting exists
	NeoNodeURL string `yaml:"neoNodeUrl"`
	WalletPath string `yaml:"walletPath"`
	WalletPass string `yaml:"walletPass"` // For service signing
}

// GasPoolState represents the state of the global gas pool.
type GasPoolState struct {
	AvailableGas *big.Int
	LastRefill   time.Time
	LockedGas    *big.Int // Total gas currently allocated/locked
}

// UserBalance represents a user's persistent GAS balance managed by the service.
type UserBalance struct {
	UserID        util.Uint160 `json:"userId"`
	Balance       *big.Int     `json:"balance"`       // Current available balance (GAS decimals = 8)
	LockedBalance *big.Int     `json:"lockedBalance"` // Balance currently locked for operations
	UpdatedAt     time.Time    `json:"updatedAt"`
}

// FeePolicy defines rules for when the GasBank pays fees.
// This is a simplified example; could be more complex.
type FeePolicy struct {
	PolicyID         string         `json:"policyId"`
	UserID           util.Uint160   `json:"userId"`           // User this policy applies to
	PayForOthers     bool           `json:"payForOthers"`     // Allow paying for arbitrary transactions signed by this user?
	AllowedContracts []util.Uint160 `json:"allowedContracts"` // Pay only if tx calls one of these contracts
	MaxFeePerTx      *big.Int       `json:"maxFeePerTx"`      // Max fee to cover per transaction
	IsEnabled        bool           `json:"isEnabled"`
}

// GasClaim represents a request from a user to claim GAS.
type GasClaim struct {
	UserID          util.Uint160  `json:"userId"`
	ClaimTxUnsigned []byte        `json:"claimTxUnsigned"` // User-signed tx (claim GAS), needs service signature
	Status          string        `json:"status"`          // Pending, Submitted, Failed, Confirmed?
	SubmittedTxHash *util.Uint256 `json:"submittedTxHash,omitempty"`
	Error           string        `json:"error,omitempty"`
	CreatedAt       time.Time     `json:"createdAt"`
	RequestID       string        `json:"requestId"` // Added Request ID
}

// WithdrawalRecord tracks the state of a withdrawal request.
type WithdrawalRecord struct {
	RequestID   string        `json:"requestId"`
	UserID      util.Uint160  `json:"userId"`
	Amount      *big.Int      `json:"amount"`      // Amount requested by user (excluding fee)
	TotalLocked *big.Int      `json:"totalLocked"` // Amount + Fee locked
	Status      string        `json:"status"`      // Pending, Processing, Submitted, Confirmed, Failed
	TxHash      *util.Uint256 `json:"txHash,omitempty"`
	Error       string        `json:"error,omitempty"`
	CreatedAt   time.Time     `json:"createdAt"`
	UpdatedAt   time.Time     `json:"updatedAt"`
}

// PendingSponsorship tracks funds locked for potential fee payment.
type PendingSponsorship struct {
	SponsorshipID string       `json:"sponsorshipId"` // Unique ID for this lock
	TxHash        util.Uint256 `json:"txHash"`        // Hash of the transaction needing sponsorship (or proposed hash?)
	UserID        util.Uint160 `json:"userId"`
	LockedAmount  *big.Int     `json:"lockedAmount"` // Amount locked from user balance
	Status        string       `json:"status"`       // Pending, Confirmed, Cancelled
	CreatedAt     time.Time    `json:"createdAt"`
	ExpiresAt     time.Time    `json:"expiresAt"` // Add expiry for automatic cleanup?
}
