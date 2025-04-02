package store

import (
	"context"
	"errors"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

// Define common errors for the store implementations.
var (
	ErrNotFound      = errors.New("item not found")
	ErrAlreadyExists = errors.New("item already exists")
	ErrUpdateFailed  = errors.New("update failed") // Used when transactional update logic fails
)

// Store defines the interface for GasBank persistent storage.
type Store interface {
	// --- User Balances ---
	GetUserBalance(ctx context.Context, userAddress util.Uint160) (*models.UserBalance, error)
	// SaveUserBalance creates or updates a user's balance.
	SaveUserBalance(ctx context.Context, balance *models.UserBalance) error
	// UpdateUserBalance applies updates transactionally (e.g., add/subtract, lock/unlock).
	// This requires careful implementation to avoid race conditions.
	UpdateUserBalance(ctx context.Context, userAddress util.Uint160, updateFunc func(*models.UserBalance) (*models.UserBalance, error)) error

	// --- Fee Policies ---
	GetFeePolicy(ctx context.Context, userAddress util.Uint160) (*models.FeePolicy, error)
	SaveFeePolicy(ctx context.Context, policy *models.FeePolicy) error
	DeleteFeePolicy(ctx context.Context, userAddress util.Uint160) error

	// --- Gas Claims ---
	SaveGasClaim(ctx context.Context, claim *models.GasClaim) error
	GetGasClaim(ctx context.Context, userAddress util.Uint160, requestID string) (*models.GasClaim, error)
	UpdateGasClaimStatus(ctx context.Context, userAddress util.Uint160, requestID, status string, txHash *util.Uint256, errorMsg string) error
	// ListPendingGasClaims might be needed for processing.
	ListPendingGasClaims(ctx context.Context) ([]*models.GasClaim, error)

	// --- Withdrawals ---
	SaveWithdrawalRecord(ctx context.Context, record *models.WithdrawalRecord) error
	GetWithdrawalRecord(ctx context.Context, requestID string) (*models.WithdrawalRecord, error)
	UpdateWithdrawalStatus(ctx context.Context, requestID, status string, txHash *util.Uint256, errorMsg string) error
	// ListPendingWithdrawals might be needed

	// --- Fee Sponsorships ---
	SavePendingSponsorship(ctx context.Context, sponsorship *models.PendingSponsorship) error
	GetPendingSponsorshipByTx(ctx context.Context, userAddress util.Uint160, txHash util.Uint256) (*models.PendingSponsorship, error)
	// GetPendingSponsorshipByID(ctx context.Context, sponsorshipID string) (*models.PendingSponsorship, error)
	DeletePendingSponsorship(ctx context.Context, sponsorshipID string) error
	// ListExpiredSponsorships might be needed for cleanup

	// --- Temporary Allocations (If persistence needed) ---
	// SaveAllocation(ctx context.Context, alloc *models.Allocation) error
	// GetAllocation(ctx context.Context, userID util.Uint160, taskID string) (*models.Allocation, error)
	// DeleteAllocation(ctx context.Context, userID util.Uint160, taskID string) error
	// ListExpiredAllocations(ctx context.Context) ([]*models.Allocation, error)

	// Close closes the database connection.
	Close() error
}
