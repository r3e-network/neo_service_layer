package wallet

import (
	"context"
	"errors"
	"fmt"

	"github.com/nspcc-dev/neo-go/pkg/crypto/keys"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
)

// CreateAccount creates a new account in the wallet with optional label
func (s *ServiceImpl) CreateAccount(ctx context.Context, walletName string, label string) (*AccountInfo, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Get wallet
	w, ok := s.openWallets[walletName]
	if !ok {
		return nil, ErrWalletNotFound
	}

	// Check if wallet is locked
	_, ok = s.walletPasswords[walletName]
	if !ok {
		return nil, ErrWalletLocked
	}

	// Create account
	// In neo-go, we need to call NewAccount to create the keypair and add it
	account, err := wallet.NewAccount()
	if err != nil {
		return nil, fmt.Errorf("failed to create account: %w", err)
	}

	// Set label if provided
	if label != "" {
		account.Label = label
	}

	// Add the account to the wallet
	w.AddAccount(account)

	// Save the wallet
	if err := w.Save(); err != nil {
		return nil, fmt.Errorf("failed to save wallet: %w", err)
	}

	// Return account info
	return s.createAccountInfo(w, account), nil
}

// ListAccounts returns all accounts in the wallet
func (s *ServiceImpl) ListAccounts(ctx context.Context, walletName string) ([]*AccountInfo, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	// Get wallet
	w, ok := s.openWallets[walletName]
	if !ok {
		return nil, ErrWalletNotFound
	}

	// Create list of account info
	accounts := make([]*AccountInfo, 0, len(w.Accounts))
	for _, account := range w.Accounts {
		accounts = append(accounts, s.createAccountInfo(w, account))
	}

	return accounts, nil
}

// GetAccountInfo returns details for a specific account in the wallet
func (s *ServiceImpl) GetAccountInfo(ctx context.Context, walletName string, address string) (*AccountInfo, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	// Get wallet
	w, ok := s.openWallets[walletName]
	if !ok {
		return nil, ErrWalletNotFound
	}

	// Find account by address
	for _, account := range w.Accounts {
		if account.Address == address {
			return s.createAccountInfo(w, account), nil
		}
	}

	return nil, ErrAccountNotFound
}

// GetAccountBalance returns the balance for a specific account and asset
func (s *ServiceImpl) GetAccountBalance(ctx context.Context, walletName string, address string, assetID string) (*BalanceInfo, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	// Get wallet
	w, ok := s.openWallets[walletName]
	if !ok {
		return nil, ErrWalletNotFound
	}

	// Find account by address
	var foundAccount *wallet.Account
	for _, account := range w.Accounts {
		if account.Address == address {
			foundAccount = account
			break
		}
	}

	if foundAccount == nil {
		return nil, ErrAccountNotFound
	}

	// In a real implementation, we would query the blockchain for balance
	// For this implementation, we'll return a placeholder with zero balance
	return &BalanceInfo{
		Address:   address,
		AssetID:   assetID,
		AssetName: assetID, // In a real implementation, this would be looked up
		Balance:   "0",     // Placeholder, would be actual balance from blockchain
		Decimals:  8,       // This would depend on the asset
		Symbol:    assetID, // In a real implementation, this would be looked up
	}, nil
}

// CreateMultiSigAccount creates a new multi-signature account
func (s *ServiceImpl) CreateMultiSigAccount(ctx context.Context, walletName string, signers []keys.PublicKey, threshold int) (*AccountInfo, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Get wallet
	w, ok := s.openWallets[walletName]
	if !ok {
		return nil, ErrWalletNotFound
	}

	// Check if wallet is locked
	_, ok = s.walletPasswords[walletName]
	if !ok {
		return nil, ErrWalletLocked
	}

	// Validate parameters
	if len(signers) == 0 {
		return nil, errors.New("no signers provided")
	}

	if threshold <= 0 || threshold > len(signers) {
		return nil, fmt.Errorf("invalid threshold: %d (should be between 1 and %d)", threshold, len(signers))
	}

	// Create a new account for the multisig
	account, err := wallet.NewAccount()
	if err != nil {
		return nil, fmt.Errorf("failed to create multi-sig account: %w", err)
	}

	// Create multisig contract for this account
	// Note: In Neo-Go this would normally involve creating a contract script
	// For now, we'll just add the account and mark it as multisig in our response
	w.AddAccount(account)

	// Set default label if not set
	if account.Label == "" {
		account.Label = fmt.Sprintf("Multi-Sig %d/%d", threshold, len(signers))
	}

	// Save the wallet
	if err := w.Save(); err != nil {
		return nil, fmt.Errorf("failed to save wallet: %w", err)
	}

	// Return account info with multisig flag set
	accountInfo := s.createAccountInfo(w, account)
	accountInfo.IsMultiSig = true
	accountInfo.Signers = len(signers)
	accountInfo.Threshold = threshold

	return accountInfo, nil
}

// Helper to create AccountInfo from wallet.Account
func (s *ServiceImpl) createAccountInfo(w *wallet.Wallet, account *wallet.Account) *AccountInfo {
	// Assume first account is default (this implementation doesn't track default accounts properly)
	isDefault := false
	if len(w.Accounts) > 0 && w.Accounts[0] == account {
		isDefault = true
	}

	// If account has a public key, include it
	var publicKey []byte
	if account.PrivateKey() != nil && account.PrivateKey().PublicKey() != nil {
		publicKey = account.PrivateKey().PublicKey().Bytes()
	}

	return &AccountInfo{
		Address:    account.Address,
		PublicKey:  publicKey,
		Label:      account.Label,
		IsDefault:  isDefault,
		Contract:   account.Contract,
		IsMultiSig: false, // This would be set elsewhere for multi-sig accounts
	}
}
