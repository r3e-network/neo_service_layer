package wallet

import (
	"context"

	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/crypto/keys"
)

// IService defines the interface for wallet management operations
type IService interface {
	// Service lifecycle
	Start() error
	Stop() error

	// Wallet management
	CreateWallet(ctx context.Context, name string, password string, overwrite bool) (*WalletInfo, error)
	OpenWallet(ctx context.Context, name string, password string) (*WalletInfo, error)
	CloseWallet(ctx context.Context, name string) error
	ListWallets(ctx context.Context) ([]*WalletInfo, error)
	GetWalletInfo(ctx context.Context, name string) (*WalletInfo, error)
	BackupWallet(ctx context.Context, name string, destination string) error
	RestoreWallet(ctx context.Context, source string, password string) (*WalletInfo, error)

	// Account management
	CreateAccount(ctx context.Context, walletName string, label string) (*AccountInfo, error)
	ListAccounts(ctx context.Context, walletName string) ([]*AccountInfo, error)
	GetAccountInfo(ctx context.Context, walletName string, address string) (*AccountInfo, error)
	GetAccountBalance(ctx context.Context, walletName string, address string, assetID string) (*BalanceInfo, error)

	// Signing operations
	SignTransaction(ctx context.Context, walletName string, address string, tx *transaction.Transaction) (*transaction.Transaction, error)
	SignMessage(ctx context.Context, walletName string, address string, message []byte) ([]byte, error)
	VerifySignature(ctx context.Context, message []byte, signature []byte, publicKey []byte) (bool, error)

	// Role-based wallet management
	AssignWalletToRole(ctx context.Context, walletName string, role string) error
	GetWalletForRole(ctx context.Context, role string) (*WalletInfo, error)

	// Multi-sig operations
	CreateMultiSigAccount(ctx context.Context, walletName string, signers []keys.PublicKey, threshold int) (*AccountInfo, error)
	AddSignatureToTx(ctx context.Context, partiallySignedTx *transaction.Transaction, walletName string, address string) (*transaction.Transaction, error)
}
