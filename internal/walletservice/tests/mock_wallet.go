package tests

import (
	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/crypto/keys"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
)

// MockWallet represents a simplified wallet implementation for testing
type MockWallet struct {
	// Wallet info
	path    string
	version string

	// Accounts
	accounts []*wallet.Account

	// State
	isLocked bool
}

// NewMockWallet creates a new test wallet
func NewMockWallet(path string) *MockWallet {
	return &MockWallet{
		path:     path,
		version:  "3.0",
		accounts: make([]*wallet.Account, 0),
		isLocked: false,
	}
}

// Path returns the wallet path
func (mw *MockWallet) Path() string {
	return mw.path
}

// CreateAccount creates a mock account
func (mw *MockWallet) CreateAccount() (*wallet.Account, error) {
	// In a real implementation, this would create a keypair
	// For testing, we'll create a placeholder account
	acc := &wallet.Account{
		Address: "NXyzMock123456789012345678901234",
		Label:   "Mock Account",
	}

	mw.accounts = append(mw.accounts, acc)
	return acc, nil
}

// Accounts returns the wallet's accounts
func (mw *MockWallet) Accounts() []*wallet.Account {
	return mw.accounts
}

// SignTx mocks a transaction signature
func (mw *MockWallet) SignTx(tx *transaction.Transaction, address string) error {
	// Mock implementation doesn't actually sign
	// This would fail if the wallet was locked in a real implementation
	if mw.isLocked {
		return ErrWalletLocked
	}

	return nil
}

// Lock locks the wallet
func (mw *MockWallet) Lock() {
	mw.isLocked = true
}

// Unlock unlocks the wallet
func (mw *MockWallet) Unlock(password string) error {
	if password == "correct" {
		mw.isLocked = false
		return nil
	}
	return ErrInvalidPassword
}

// IsLocked returns whether the wallet is locked
func (mw *MockWallet) IsLocked() bool {
	return mw.isLocked
}

// CreateMultiSigAccount creates a multi-signature account
func (mw *MockWallet) CreateMultiSigAccount(signers []keys.PublicKey, threshold int) (*wallet.Account, error) {
	// Mock implementation doesn't create a real multi-sig account
	acc := &wallet.Account{
		Address: "NXyzMultiSig123456789012345678901234",
		Label:   "Multi-Sig Account",
	}

	mw.accounts = append(mw.accounts, acc)
	return acc, nil
}
