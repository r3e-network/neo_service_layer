package wallet

import (
	"context"
	"fmt"
	"os"

	"github.com/nspcc-dev/neo-go/pkg/config/netmode"
	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	log "github.com/sirupsen/logrus"
)

// Config represents wallet configuration
type Config struct {
	WalletPath     string
	Password       string
	AddressVersion byte
	NetworkMagic   netmode.Magic
}

// DefaultConfig returns default wallet configuration
func DefaultConfig() *Config {
	return &Config{
		WalletPath:     "wallet.json",
		Password:       "",
		AddressVersion: 0x35,            // Version 53 (Neo N3 MainNet)
		NetworkMagic:   netmode.MainNet, // Use netmode constants
	}
}

// Service is a wallet service implementation
type Service struct {
	config  *Config
	wallet  *wallet.Wallet
	network netmode.Magic
}

// NewWalletService creates a new wallet service
func NewWalletService(config *Config) *Service {
	if config == nil {
		config = DefaultConfig()
	}

	// Load wallet on startup
	w, err := wallet.NewWalletFromFile(config.WalletPath)
	if err != nil {
		// Check if file doesn't exist, then create a new wallet
		if isFileNotExistError(err) {
			log.Warnf("Wallet file not found at %s, creating new wallet", config.WalletPath)
			w, err = createNewWallet(config)
			if err != nil {
				log.Fatalf("Failed to create new wallet: %v", err)
			}
		} else {
			log.Fatalf("Failed to load wallet: %v", err)
		}
	}

	log.Infof("Loaded wallet with %d accounts", len(w.Accounts))

	return &Service{
		config:  config,
		wallet:  w,
		network: config.NetworkMagic,
	}
}

// SignTx signs a transaction with a specified account
func (s *Service) SignTx(ctx context.Context, acc util.Uint160, tx *transaction.Transaction) error {
	// Find the account in wallet matching the requested address
	var signAccount *wallet.Account
	for _, account := range s.wallet.Accounts {
		if account.ScriptHash().Equals(acc) {
			signAccount = account
			break
		}
	}

	if signAccount == nil {
		return fmt.Errorf("account %s not found in wallet", acc.StringLE())
	}

	// Check if the account is default (no password) or needs unlocking
	if !signAccount.CanSign() {
		err := signAccount.Decrypt(s.config.Password, s.wallet.Scrypt)
		if err != nil {
			return fmt.Errorf("failed to decrypt account %s: %w", acc.StringLE(), err)
		}
		// Neo-go doesn't have Lock() method, no need to defer a lock
	}

	// Find signer index
	signerIndex := -1
	for i, signer := range tx.Signers {
		if signer.Account.Equals(acc) {
			signerIndex = i
			break
		}
	}

	if signerIndex == -1 {
		return fmt.Errorf("account %s not found in transaction signers", acc.StringLE())
	}

	// Sign the transaction with network magic parameter
	err := signAccount.SignTx(s.network, tx)
	if err != nil {
		return fmt.Errorf("failed to sign transaction: %w", err)
	}

	log.Debugf("Transaction signed successfully with account %s", acc.StringLE())
	return nil
}

// createNewWallet creates and saves a new wallet file
func createNewWallet(config *Config) (*wallet.Wallet, error) {
	w, err := wallet.NewWallet(config.WalletPath)
	if err != nil {
		return nil, fmt.Errorf("failed to create new wallet: %w", err)
	}

	// Create a new account
	account, err := wallet.NewAccount()
	if err != nil {
		return nil, fmt.Errorf("failed to create account: %w", err)
	}

	if config.Password != "" {
		err = account.Encrypt(config.Password, w.Scrypt)
		if err != nil {
			return nil, fmt.Errorf("failed to encrypt account: %w", err)
		}
	}

	w.AddAccount(account)

	// Save the wallet file
	if err := w.Save(); err != nil {
		return nil, fmt.Errorf("failed to save wallet: %w", err)
	}

	log.Infof("Created new wallet at %s with address: %s",
		config.WalletPath, account.Address)

	return w, nil
}

// Helpers

// isFileNotExistError checks if an error is because a file doesn't exist
func isFileNotExistError(err error) bool {
	if err == nil {
		return false
	}
	return os.IsNotExist(err) ||
		fmt.Sprintf("%v", err) == "wallet file doesn't exist"
}
