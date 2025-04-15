package wallet

import (
	"context"
	"errors"
	"sync"

	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/crypto/keys"
)

// MockService implements the IService interface for testing
type MockService struct {
	wallets           map[string]*WalletInfo
	accounts          map[string][]*AccountInfo
	roleAssignments   map[string]string
	balances          map[string]*BalanceInfo
	openWallets       map[string]bool
	failNextOperation bool
	mu                sync.RWMutex
}

// NewMockService creates a new mock wallet service for testing
func NewMockService() *MockService {
	return &MockService{
		wallets:         make(map[string]*WalletInfo),
		accounts:        make(map[string][]*AccountInfo),
		roleAssignments: make(map[string]string),
		balances:        make(map[string]*BalanceInfo),
		openWallets:     make(map[string]bool),
	}
}

// SetFailNextOperation configures the mock to fail the next operation
func (m *MockService) SetFailNextOperation(fail bool) {
	m.mu.Lock()
	defer m.mu.Unlock()
	m.failNextOperation = fail
}

// checkFailure checks if the next operation should fail and resets the flag
func (m *MockService) checkFailure() error {
	if m.failNextOperation {
		m.failNextOperation = false
		return errors.New("mock operation failure")
	}
	return nil
}

// Start initializes the mock service
func (m *MockService) Start() error {
	m.mu.Lock()
	defer m.mu.Unlock()

	if err := m.checkFailure(); err != nil {
		return err
	}

	// Create some default wallets and role assignments for testing
	m.wallets["system"] = &WalletInfo{
		Name:     "system",
		Path:     "/mock/path/system.json",
		Version:  6,
		Accounts: 1,
		IsOpen:   false,
		IsLocked: true,
	}

	m.roleAssignments["system"] = "system"

	return nil
}

// Stop gracefully shuts down the mock service
func (m *MockService) Stop() error {
	m.mu.Lock()
	defer m.mu.Unlock()

	if err := m.checkFailure(); err != nil {
		return err
	}

	m.openWallets = make(map[string]bool)
	return nil
}

// CreateWallet creates a new mock wallet
func (m *MockService) CreateWallet(ctx context.Context, name string, password string, overwrite bool) (*WalletInfo, error) {
	m.mu.Lock()
	defer m.mu.Unlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if name == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	if password == "" {
		return nil, errors.New("password cannot be empty")
	}

	if _, exists := m.wallets[name]; exists && !overwrite {
		return nil, ErrWalletAlreadyExists
	}

	walletInfo := &WalletInfo{
		Name:     name,
		Path:     "/mock/path/" + name + ".json",
		Version:  6,
		Accounts: 0,
		IsOpen:   true,
		IsLocked: false,
	}

	m.wallets[name] = walletInfo
	m.openWallets[name] = true
	m.accounts[name] = []*AccountInfo{}

	return walletInfo, nil
}

// OpenWallet opens an existing mock wallet
func (m *MockService) OpenWallet(ctx context.Context, name string, password string) (*WalletInfo, error) {
	m.mu.Lock()
	defer m.mu.Unlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if name == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	if password == "" {
		return nil, errors.New("password cannot be empty")
	}

	walletInfo, exists := m.wallets[name]
	if !exists {
		return nil, ErrWalletNotFound
	}

	if password != "correct_password" && password != "mock_password" {
		return nil, ErrInvalidPassword
	}

	walletInfo.IsOpen = true
	walletInfo.IsLocked = false
	m.openWallets[name] = true

	return walletInfo, nil
}

// CloseWallet closes an open mock wallet
func (m *MockService) CloseWallet(ctx context.Context, name string) error {
	m.mu.Lock()
	defer m.mu.Unlock()

	if err := m.checkFailure(); err != nil {
		return err
	}

	if name == "" {
		return errors.New("wallet name cannot be empty")
	}

	walletInfo, exists := m.wallets[name]
	if !exists {
		return ErrWalletNotFound
	}

	if !walletInfo.IsOpen {
		return ErrWalletNotOpen
	}

	walletInfo.IsOpen = false
	walletInfo.IsLocked = true
	delete(m.openWallets, name)

	return nil
}

// ListWallets lists all available mock wallets
func (m *MockService) ListWallets(ctx context.Context) ([]*WalletInfo, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	wallets := make([]*WalletInfo, 0, len(m.wallets))
	for _, w := range m.wallets {
		wallets = append(wallets, w)
	}

	return wallets, nil
}

// GetWalletInfo gets information about a specific mock wallet
func (m *MockService) GetWalletInfo(ctx context.Context, name string) (*WalletInfo, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if name == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	walletInfo, exists := m.wallets[name]
	if !exists {
		return nil, ErrWalletNotFound
	}

	return walletInfo, nil
}

// BackupWallet creates a backup of a mock wallet
func (m *MockService) BackupWallet(ctx context.Context, name string, destination string) error {
	m.mu.RLock()
	defer m.mu.RUnlock()

	if err := m.checkFailure(); err != nil {
		return err
	}

	if name == "" {
		return errors.New("wallet name cannot be empty")
	}

	if destination == "" {
		return errors.New("destination cannot be empty")
	}

	_, exists := m.wallets[name]
	if !exists {
		return ErrWalletNotFound
	}

	// In a mock, we just pretend to backup
	return nil
}

// RestoreWallet restores a mock wallet from a backup
func (m *MockService) RestoreWallet(ctx context.Context, source string, password string) (*WalletInfo, error) {
	m.mu.Lock()
	defer m.mu.Unlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if source == "" {
		return nil, errors.New("source cannot be empty")
	}

	if password == "" {
		return nil, errors.New("password cannot be empty")
	}

	// Extract wallet name from source path
	name := "restored_wallet"

	walletInfo := &WalletInfo{
		Name:     name,
		Path:     "/mock/path/" + name + ".json",
		Version:  6,
		Accounts: 1,
		IsOpen:   true,
		IsLocked: false,
	}

	m.wallets[name] = walletInfo
	m.openWallets[name] = true

	// Add a mock account
	m.accounts[name] = []*AccountInfo{
		{
			Address:   "NZNos2WqTbu5oCgyfss9kUJgBXJqhuYAaj",
			PublicKey: []byte("mock_pubkey"),
			Label:     "mock_account",
			IsDefault: true,
		},
	}

	return walletInfo, nil
}

// CreateAccount creates a new account in the specified mock wallet
func (m *MockService) CreateAccount(ctx context.Context, walletName string, label string) (*AccountInfo, error) {
	m.mu.Lock()
	defer m.mu.Unlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if walletName == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	walletInfo, exists := m.wallets[walletName]
	if !exists {
		return nil, ErrWalletNotFound
	}

	if !walletInfo.IsOpen {
		return nil, ErrWalletNotOpen
	}

	if walletInfo.IsLocked {
		return nil, ErrWalletLocked
	}

	// Create a mock account
	accountInfo := &AccountInfo{
		Address:   "NTAF9wArZs3yg4a5KVZACeXNiGV73xBEPG",
		PublicKey: []byte("mock_pubkey_" + label),
		Label:     label,
		IsDefault: len(m.accounts[walletName]) == 0,
	}

	m.accounts[walletName] = append(m.accounts[walletName], accountInfo)
	walletInfo.Accounts++

	return accountInfo, nil
}

// ListAccounts lists all accounts in the specified mock wallet
func (m *MockService) ListAccounts(ctx context.Context, walletName string) ([]*AccountInfo, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if walletName == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	walletInfo, exists := m.wallets[walletName]
	if !exists {
		return nil, ErrWalletNotFound
	}

	if !walletInfo.IsOpen {
		return nil, ErrWalletNotOpen
	}

	accounts, exists := m.accounts[walletName]
	if !exists {
		accounts = []*AccountInfo{}
	}

	return accounts, nil
}

// GetAccountInfo gets information about a specific account in a mock wallet
func (m *MockService) GetAccountInfo(ctx context.Context, walletName string, address string) (*AccountInfo, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if walletName == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	if address == "" {
		return nil, errors.New("address cannot be empty")
	}

	walletInfo, exists := m.wallets[walletName]
	if !exists {
		return nil, ErrWalletNotFound
	}

	if !walletInfo.IsOpen {
		return nil, ErrWalletNotOpen
	}

	accounts, exists := m.accounts[walletName]
	if !exists {
		return nil, ErrAccountNotFound
	}

	for _, acc := range accounts {
		if acc.Address == address {
			return acc, nil
		}
	}

	return nil, ErrAccountNotFound
}

// GetAccountBalance gets the balance of a specific asset for an account in a mock wallet
func (m *MockService) GetAccountBalance(ctx context.Context, walletName string, address string, assetID string) (*BalanceInfo, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if walletName == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	if address == "" {
		return nil, errors.New("address cannot be empty")
	}

	walletInfo, exists := m.wallets[walletName]
	if !exists {
		return nil, ErrWalletNotFound
	}

	if !walletInfo.IsOpen {
		return nil, ErrWalletNotOpen
	}

	// Check if account exists
	var accountExists bool
	accounts, exists := m.accounts[walletName]
	if exists {
		for _, acc := range accounts {
			if acc.Address == address {
				accountExists = true
				break
			}
		}
	}

	if !accountExists {
		return nil, ErrAccountNotFound
	}

	// Check if we have a balance for this account+asset
	balanceKey := walletName + ":" + address + ":" + assetID
	balance, exists := m.balances[balanceKey]

	if !exists {
		// Return default balance
		if assetID == "neo" || assetID == "" {
			balance = &BalanceInfo{
				Address:   address,
				AssetID:   "neo",
				AssetName: "NEO",
				Balance:   "100",
				Decimals:  0,
				Symbol:    "NEO",
			}
		} else if assetID == "gas" {
			balance = &BalanceInfo{
				Address:   address,
				AssetID:   "gas",
				AssetName: "GAS",
				Balance:   "50.5",
				Decimals:  8,
				Symbol:    "GAS",
			}
		} else {
			balance = &BalanceInfo{
				Address:   address,
				AssetID:   assetID,
				AssetName: "Unknown Asset",
				Balance:   "0",
				Decimals:  8,
				Symbol:    "???",
			}
		}

		m.balances[balanceKey] = balance
	}

	return balance, nil
}

// SignTransaction signs a transaction using the specified account in a mock wallet
func (m *MockService) SignTransaction(ctx context.Context, walletName string, address string, tx *transaction.Transaction) (*transaction.Transaction, error) {
	m.mu.Lock()
	defer m.mu.Unlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if walletName == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	if address == "" {
		return nil, errors.New("address cannot be empty")
	}

	if tx == nil {
		return nil, errors.New("transaction cannot be nil")
	}

	walletInfo, exists := m.wallets[walletName]
	if !exists {
		return nil, ErrWalletNotFound
	}

	if !walletInfo.IsOpen {
		return nil, ErrWalletNotOpen
	}

	if walletInfo.IsLocked {
		return nil, ErrWalletLocked
	}

	// Check if account exists
	var accountExists bool
	accounts, exists := m.accounts[walletName]
	if exists {
		for _, acc := range accounts {
			if acc.Address == address {
				accountExists = true
				break
			}
		}
	}

	if !accountExists {
		return nil, ErrAccountNotFound
	}

	// In a mock, we just return the same transaction
	// We could enhance this to actually add a witness script if needed
	return tx, nil
}

// SignMessage signs an arbitrary message using the specified account in a mock wallet
func (m *MockService) SignMessage(ctx context.Context, walletName string, address string, message []byte) ([]byte, error) {
	m.mu.Lock()
	defer m.mu.Unlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if walletName == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	if address == "" {
		return nil, errors.New("address cannot be empty")
	}

	if len(message) == 0 {
		return nil, errors.New("message cannot be empty")
	}

	walletInfo, exists := m.wallets[walletName]
	if !exists {
		return nil, ErrWalletNotFound
	}

	if !walletInfo.IsOpen {
		return nil, ErrWalletNotOpen
	}

	if walletInfo.IsLocked {
		return nil, ErrWalletLocked
	}

	// Check if account exists
	var accountExists bool
	accounts, exists := m.accounts[walletName]
	if exists {
		for _, acc := range accounts {
			if acc.Address == address {
				accountExists = true
				break
			}
		}
	}

	if !accountExists {
		return nil, ErrAccountNotFound
	}

	// Return a mock signature (just append "signed" to the message)
	return append(message, []byte("_signed")...), nil
}

// VerifySignature verifies a signature against a message and public key
func (m *MockService) VerifySignature(ctx context.Context, message []byte, signature []byte, publicKey []byte) (bool, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	if err := m.checkFailure(); err != nil {
		return false, err
	}

	if len(message) == 0 {
		return false, errors.New("message cannot be empty")
	}

	if len(signature) == 0 {
		return false, errors.New("signature cannot be empty")
	}

	if len(publicKey) == 0 {
		return false, errors.New("public key cannot be empty")
	}

	// In a mock, we just check if the signature ends with "_signed"
	expectedSig := append(message, []byte("_signed")...)
	return string(signature) == string(expectedSig), nil
}

// AssignWalletToRole assigns a wallet to a specific role in the mock service
func (m *MockService) AssignWalletToRole(ctx context.Context, walletName string, role string) error {
	m.mu.Lock()
	defer m.mu.Unlock()

	if err := m.checkFailure(); err != nil {
		return err
	}

	if walletName == "" {
		return errors.New("wallet name cannot be empty")
	}

	if role == "" {
		return errors.New("role cannot be empty")
	}

	_, exists := m.wallets[walletName]
	if !exists {
		return ErrWalletNotFound
	}

	m.roleAssignments[role] = walletName
	return nil
}

// GetWalletForRole gets the wallet assigned to a specific role in the mock service
func (m *MockService) GetWalletForRole(ctx context.Context, role string) (*WalletInfo, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if role == "" {
		return nil, errors.New("role cannot be empty")
	}

	walletName, exists := m.roleAssignments[role]
	if !exists {
		return nil, ErrRoleNotAssigned
	}

	walletInfo, exists := m.wallets[walletName]
	if !exists {
		return nil, ErrWalletNotFound
	}

	return walletInfo, nil
}

// CreateMultiSigAccount creates a multi-signature account in the specified mock wallet
func (m *MockService) CreateMultiSigAccount(ctx context.Context, walletName string, signers []keys.PublicKey, threshold int) (*AccountInfo, error) {
	m.mu.Lock()
	defer m.mu.Unlock()

	if err := m.checkFailure(); err != nil {
		return nil, err
	}

	if walletName == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	if len(signers) == 0 {
		return nil, errors.New("signers cannot be empty")
	}

	if threshold <= 0 || threshold > len(signers) {
		return nil, errors.New("invalid threshold")
	}

	walletInfo, exists := m.wallets[walletName]
	if !exists {
		return nil, ErrWalletNotFound
	}

	if !walletInfo.IsOpen {
		return nil, ErrWalletNotOpen
	}

	if walletInfo.IsLocked {
		return nil, ErrWalletLocked
	}

	// Create a mock multi-sig account
	accountInfo := &AccountInfo{
		Address:    "NiM8EXWWMJWLLhcquZLCMo7BBVy9GH83Ny",
		PublicKey:  []byte("mock_multisig_pubkey"),
		Label:      "multisig_account",
		IsDefault:  false,
		IsMultiSig: true,
		Signers:    len(signers),
		Threshold:  threshold,
	}

	m.accounts[walletName] = append(m.accounts[walletName], accountInfo)
	walletInfo.Accounts++

	return accountInfo, nil
}

// AddSignatureToTx adds a signature to a partially signed transaction
func (m *MockService) AddSignatureToTx(ctx context.Context, partiallySignedTx *transaction.Transaction, walletName string, address string) (*transaction.Transaction, error) {
	// In a mock, this is the same as signing the transaction
	return m.SignTransaction(ctx, walletName, address, partiallySignedTx)
}
