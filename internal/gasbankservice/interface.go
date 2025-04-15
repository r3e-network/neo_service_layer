package gasbank

import (
	"context"
	"math/big"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
)

// Service defines the interface for the GasBank service.
type Service interface {
	// Start starts the service background tasks (monitoring, etc.).
	Start(ctx context.Context) error
	// Stop stops the service background tasks.
	Stop(ctx context.Context) error

	// --- Persistent User Balances ---

	// GetUserBalance retrieves the current GAS balance for a user.
	GetUserBalance(ctx context.Context, userAddress util.Uint160) (*models.UserBalance, error)
	// RecordDeposit handles recording a detected user deposit.
	RecordDeposit(ctx context.Context, userAddress util.Uint160, txHash util.Uint256, amount *big.Int) error
	// InitiateWithdrawal starts the process for a user to withdraw GAS.
	InitiateWithdrawal(ctx context.Context, userAddress util.Uint160, amount *big.Int) (string, error)
	// GetWithdrawalStatus checks the status of a withdrawal request.
	GetWithdrawalStatus(ctx context.Context, userAddress util.Uint160, requestID string) (string, error)

	// --- Fee Payment Sponsorship ---

	// RequestTransactionFeeSponsorship checks if GasBank can sponsor the fee for a transaction.
	RequestTransactionFeeSponsorship(ctx context.Context, userAddress util.Uint160, txDetails TransactionDetails) (*big.Int, error)
	// ConfirmFeePayment confirms a sponsored transaction was successful and deducts the fee.
	ConfirmFeePayment(ctx context.Context, userAddress util.Uint160, txHash util.Uint256, actualFee *big.Int) error
	// CancelFeeSponsorship releases locked funds if a sponsored transaction fails.
	CancelFeeSponsorship(ctx context.Context, userAddress util.Uint160, txHash util.Uint256) error
	// SetFeePolicy allows a user to define their fee payment policy.
	SetFeePolicy(ctx context.Context, userAddress util.Uint160, policy models.FeePolicy) error
	// GetFeePolicy retrieves a user's current fee payment policy.
	GetFeePolicy(ctx context.Context, userAddress util.Uint160) (*models.FeePolicy, error)

	// --- Gas Claiming for NEO-only users ---

	// SubmitGasClaim allows a user to submit their pre-signed claim transaction.
	SubmitGasClaim(ctx context.Context, userAddress util.Uint160, signedTxBytes []byte) (string, error)
	// GetGasClaimStatus checks the status of a submitted claim.
	GetGasClaimStatus(ctx context.Context, userAddress util.Uint160, requestID string) (*models.GasClaim, error)
}

// TransactionDetails provides necessary info about a tx needing fee sponsorship.
type TransactionDetails struct {
	Signers         []util.Uint160 // Who signed the tx (primary signer is usually userAddress)
	CalledContracts []util.Uint160 // Contracts invoked by the transaction script
	NetworkFee      *big.Int       // Estimated/Max network fee
	SystemFee       *big.Int       // Estimated/Max system fee
	// Potentially add Tx []byte if needed for deeper inspection
}
