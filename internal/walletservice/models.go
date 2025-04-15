package wallet

import (
	"github.com/nspcc-dev/neo-go/pkg/wallet"
)

// WalletInfo provides information about a wallet without exposing private keys
type WalletInfo struct {
	Name           string                 `json:"name"`
	Path           string                 `json:"path"`
	Version        int                    `json:"version"`
	Accounts       int                    `json:"accounts"`
	IsOpen         bool                   `json:"is_open"`
	IsLocked       bool                   `json:"is_locked"`
	ScryptParams   interface{}            `json:"scrypt_params,omitempty"`
	DefaultAccount string                 `json:"default_account,omitempty"`
	Extra          map[string]interface{} `json:"extra,omitempty"`
}

// AccountInfo provides information about an account
type AccountInfo struct {
	Address    string           `json:"address"`
	PublicKey  []byte           `json:"public_key"`
	Label      string           `json:"label,omitempty"`
	IsDefault  bool             `json:"is_default"`
	Contract   *wallet.Contract `json:"contract,omitempty"`
	IsMultiSig bool             `json:"is_multi_sig"`
	Signers    int              `json:"signers,omitempty"`
	Threshold  int              `json:"threshold,omitempty"`
}

// BalanceInfo contains balance information for an account
type BalanceInfo struct {
	Address   string `json:"address"`
	AssetID   string `json:"asset_id"`
	AssetName string `json:"asset_name"`
	Balance   string `json:"balance"` // String to handle large numbers precisely
	Decimals  int    `json:"decimals"`
	Symbol    string `json:"symbol"`
}

// WalletRole associates a wallet with a specific role in the service layer
type WalletRole struct {
	Role        string `json:"role"`
	WalletName  string `json:"wallet_name"`
	Description string `json:"description,omitempty"`
}

// SignatureInfo provides information about a signature operation
type SignatureInfo struct {
	Address     string `json:"address"`
	WalletName  string `json:"wallet_name"`
	Timestamp   int64  `json:"timestamp"`
	Success     bool   `json:"success"`
	MessageHash []byte `json:"message_hash,omitempty"`
}

// WalletConfig contains configuration for the wallet service
type WalletConfig struct {
	// Directory where wallets are stored
	WalletDir string `json:"wallet_dir"`

	// Default password for system wallets (not user wallets)
	SystemWalletPassword string `json:"-"` // Don't include in JSON output

	// Whether to create default service wallets if they don't exist
	AutoCreateServiceWallets bool `json:"auto_create_service_wallets"`

	// Default neo-go network to connect to (mainnet, testnet, etc.)
	Network string `json:"network"`

	// Whether to require authentication for all wallet operations
	RequireAuth bool `json:"require_auth"`

	// How long to keep wallets unlocked in memory (in seconds, 0 = indefinitely)
	AutoLockTimeout int `json:"auto_lock_timeout"`

	// Max number of wallets that can be open simultaneously
	MaxOpenWallets int `json:"max_open_wallets"`

	// Log all wallet operations for security audit
	AuditLog bool `json:"audit_log"`
}

// Error types specific to wallet service
var (
	ErrWalletNotFound      = NewWalletError("wallet not found")
	ErrWalletAlreadyExists = NewWalletError("wallet already exists")
	ErrWalletNotOpen       = NewWalletError("wallet not open")
	ErrWalletLocked        = NewWalletError("wallet is locked")
	ErrInvalidPassword     = NewWalletError("invalid password")
	ErrAccountNotFound     = NewWalletError("account not found")
	ErrInsufficientFunds   = NewWalletError("insufficient funds")
	ErrInvalidSignature    = NewWalletError("invalid signature")
	ErrUnauthorized        = NewWalletError("unauthorized operation")
	ErrRoleNotAssigned     = NewWalletError("role not assigned to any wallet")
)

// WalletError represents a wallet-specific error
type WalletError struct {
	Message string
}

// NewWalletError creates a new wallet error
func NewWalletError(message string) *WalletError {
	return &WalletError{Message: message}
}

// Error returns the error message
func (e *WalletError) Error() string {
	return e.Message
}
