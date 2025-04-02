package wallet

import (
	"context"
	"errors"
	"fmt"
	"io"
	"os"
	"path/filepath"
	"strconv"
	"sync"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"go.uber.org/zap"
)

// ServiceImpl implements the IService interface
type ServiceImpl struct {
	config          *WalletConfig
	logger          *zap.Logger
	openWallets     map[string]*wallet.Wallet
	walletPasswords map[string]string // For temporarily keeping passwords in memory
	roleAssignments map[string]string // Maps roles to wallet names
	autoLockTimers  map[string]*time.Timer
	mu              sync.RWMutex
}

// NewService creates a new wallet service
func NewService(config *WalletConfig, logger *zap.Logger) (IService, error) {
	// Validate config
	if config == nil {
		return nil, errors.New("config cannot be nil")
	}

	if logger == nil {
		return nil, errors.New("logger cannot be nil")
	}

	// Create wallet directory if it doesn't exist
	if config.WalletDir != "" {
		if err := os.MkdirAll(config.WalletDir, 0700); err != nil {
			return nil, fmt.Errorf("failed to create wallet directory: %w", err)
		}
	}

	// Apply defaults
	if config.MaxOpenWallets <= 0 {
		config.MaxOpenWallets = 10 // Default to 10 open wallets max
	}

	if config.Network == "" {
		config.Network = "testnet" // Default to testnet
	}

	if config.AutoLockTimeout <= 0 {
		config.AutoLockTimeout = 3600 // Default to 1 hour
	}

	return &ServiceImpl{
		config:          config,
		logger:          logger,
		openWallets:     make(map[string]*wallet.Wallet),
		walletPasswords: make(map[string]string),
		roleAssignments: make(map[string]string),
		autoLockTimers:  make(map[string]*time.Timer),
	}, nil
}

// Start initializes the wallet service
func (s *ServiceImpl) Start() error {
	s.logger.Info("Starting wallet service")

	// Create default service wallets if configured
	if s.config.AutoCreateServiceWallets {
		s.createDefaultServiceWallets()
	}

	return nil
}

// Stop gracefully shuts down the wallet service
func (s *ServiceImpl) Stop() error {
	s.logger.Info("Stopping wallet service")

	// Close all open wallets
	s.mu.Lock()
	defer s.mu.Unlock()

	for name := range s.openWallets {
		s.logger.Info("Closing wallet", zap.String("name", name))
		delete(s.openWallets, name)
		delete(s.walletPasswords, name)

		if timer, exists := s.autoLockTimers[name]; exists {
			timer.Stop()
			delete(s.autoLockTimers, name)
		}
	}

	// Clear sensitive data
	s.walletPasswords = make(map[string]string)

	return nil
}

// createDefaultServiceWallets creates standard wallets for service roles
func (s *ServiceImpl) createDefaultServiceWallets() {
	roles := []string{
		"gas_bank",
		"price_feed",
		"automation",
		"system",
	}

	for _, role := range roles {
		walletName := fmt.Sprintf("%s_wallet", role)
		walletPath := filepath.Join(s.config.WalletDir, walletName+".json")

		// Check if wallet already exists
		if _, err := os.Stat(walletPath); os.IsNotExist(err) {
			s.logger.Info("Creating default wallet for role", zap.String("role", role), zap.String("wallet", walletName))

			ctx := context.Background()
			info, err := s.CreateWallet(ctx, walletName, s.config.SystemWalletPassword, false)
			if err != nil {
				s.logger.Error("Failed to create default wallet", zap.String("role", role), zap.Error(err))
				continue
			}

			// Assign wallet to role
			err = s.AssignWalletToRole(ctx, walletName, role)
			if err != nil {
				s.logger.Error("Failed to assign wallet to role", zap.String("role", role), zap.Error(err))
			}

			s.logger.Info("Created and assigned default wallet", zap.String("role", role), zap.Any("info", info))
		}
	}
}

// CreateWallet creates a new NEP-6 wallet
func (s *ServiceImpl) CreateWallet(ctx context.Context, name string, password string, overwrite bool) (*WalletInfo, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Validate parameters
	if name == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	if password == "" {
		return nil, errors.New("password cannot be empty")
	}

	// Check if wallet already exists
	walletPath := filepath.Join(s.config.WalletDir, name+".json")
	if _, err := os.Stat(walletPath); err == nil && !overwrite {
		return nil, ErrWalletAlreadyExists
	}

	// Create new wallet
	w, err := wallet.NewWallet(walletPath)
	if err != nil {
		return nil, fmt.Errorf("failed to create wallet: %w", err)
	}

	// Save wallet
	if err := w.Save(); err != nil {
		return nil, fmt.Errorf("failed to save wallet: %w", err)
	}

	// Store wallet in memory
	s.openWallets[name] = w
	s.walletPasswords[name] = password

	// Create wallet info
	info := s.createWalletInfo(name, w)

	// Start auto-lock timer if configured
	if s.config.AutoLockTimeout > 0 {
		s.setupAutoLockTimer(name)
	}

	// Log wallet creation
	if s.config.AuditLog {
		s.logger.Info("Wallet created", zap.String("name", name), zap.String("path", walletPath))
	}

	return info, nil
}

// OpenWallet opens an existing wallet
func (s *ServiceImpl) OpenWallet(ctx context.Context, name string, password string) (*WalletInfo, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Validate parameters
	if name == "" {
		return nil, errors.New("wallet name cannot be empty")
	}

	if password == "" {
		return nil, errors.New("password cannot be empty")
	}

	// Check if wallet is already open
	if _, exists := s.openWallets[name]; exists {
		// Update password and refresh timer
		s.walletPasswords[name] = password
		if s.config.AutoLockTimeout > 0 {
			s.setupAutoLockTimer(name)
		}
		return s.createWalletInfo(name, s.openWallets[name]), nil
	}

	// Open wallet file
	walletPath := filepath.Join(s.config.WalletDir, name+".json")
	w, err := wallet.NewWalletFromFile(walletPath)
	if err != nil {
		return nil, fmt.Errorf("failed to open wallet: %w", err)
	}

	// Verify password - there's no direct Unlock method, but we can try to decrypt
	// an account if one exists to verify the password
	if len(w.Accounts) > 0 {
		// Try to decrypt the first account using the password
		account := w.Accounts[0]
		err := account.Decrypt(password, w.Scrypt)
		if err != nil {
			return nil, ErrInvalidPassword
		}
		// Clean up private key after verification
		account.Close()
	}

	// Store wallet in memory
	s.openWallets[name] = w
	s.walletPasswords[name] = password

	// Setup auto-lock timer
	if s.config.AutoLockTimeout > 0 {
		s.setupAutoLockTimer(name)
	}

	// Log wallet open
	if s.config.AuditLog {
		s.logger.Info("Wallet opened", zap.String("name", name))
	}

	return s.createWalletInfo(name, w), nil
}

// CloseWallet closes an open wallet
func (s *ServiceImpl) CloseWallet(ctx context.Context, name string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Check if wallet is open
	if _, exists := s.openWallets[name]; !exists {
		return ErrWalletNotOpen
	}

	// Remove wallet from memory
	delete(s.openWallets, name)
	delete(s.walletPasswords, name)

	// Stop auto-lock timer if exists
	if timer, exists := s.autoLockTimers[name]; exists {
		timer.Stop()
		delete(s.autoLockTimers, name)
	}

	// Log wallet closing
	if s.config.AuditLog {
		s.logger.Info("Wallet closed", zap.String("name", name))
	}

	return nil
}

// ListWallets lists all available wallets
func (s *ServiceImpl) ListWallets(ctx context.Context) ([]*WalletInfo, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	var wallets []*WalletInfo

	// First add open wallets
	for name, w := range s.openWallets {
		info := s.createWalletInfo(name, w)
		wallets = append(wallets, info)
	}

	// Then look for wallet files in the wallet directory
	files, err := os.ReadDir(s.config.WalletDir)
	if err != nil {
		return wallets, fmt.Errorf("failed to read wallet directory: %w", err)
	}

	for _, file := range files {
		if file.IsDir() || filepath.Ext(file.Name()) != ".json" {
			continue
		}

		name := filepath.Base(file.Name())
		name = name[:len(name)-5] // Remove .json extension

		// Skip already added open wallets
		if _, exists := s.openWallets[name]; exists {
			continue
		}

		// Add closed wallet info
		info := &WalletInfo{
			Name:     name,
			Path:     filepath.Join(s.config.WalletDir, file.Name()),
			IsOpen:   false,
			IsLocked: true,
		}
		wallets = append(wallets, info)
	}

	return wallets, nil
}

// GetWalletInfo gets information about a specific wallet
func (s *ServiceImpl) GetWalletInfo(ctx context.Context, name string) (*WalletInfo, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	// Check if wallet is open
	if w, exists := s.openWallets[name]; exists {
		return s.createWalletInfo(name, w), nil
	}

	// Check if wallet file exists
	walletPath := filepath.Join(s.config.WalletDir, name+".json")
	if _, err := os.Stat(walletPath); os.IsNotExist(err) {
		return nil, ErrWalletNotFound
	}

	// Return basic info for closed wallet
	return &WalletInfo{
		Name:     name,
		Path:     walletPath,
		IsOpen:   false,
		IsLocked: true,
	}, nil
}

// BackupWallet creates a backup of a wallet
func (s *ServiceImpl) BackupWallet(ctx context.Context, name string, destination string) error {
	// Check if destination directory exists
	destDir := filepath.Dir(destination)
	if _, err := os.Stat(destDir); os.IsNotExist(err) {
		return fmt.Errorf("destination directory does not exist: %s", destDir)
	}

	// Check if wallet exists
	walletPath := filepath.Join(s.config.WalletDir, name+".json")
	if _, err := os.Stat(walletPath); os.IsNotExist(err) {
		return ErrWalletNotFound
	}

	// Copy wallet file to destination
	srcFile, err := os.Open(walletPath)
	if err != nil {
		return fmt.Errorf("failed to open wallet file: %w", err)
	}
	defer srcFile.Close()

	destFile, err := os.Create(destination)
	if err != nil {
		return fmt.Errorf("failed to create backup file: %w", err)
	}
	defer destFile.Close()

	_, err = io.Copy(destFile, srcFile)
	if err != nil {
		return fmt.Errorf("failed to copy wallet data: %w", err)
	}

	// Log backup operation
	if s.config.AuditLog {
		s.logger.Info("Wallet backup created",
			zap.String("name", name),
			zap.String("source", walletPath),
			zap.String("destination", destination))
	}

	return nil
}

// RestoreWallet restores a wallet from a backup
func (s *ServiceImpl) RestoreWallet(ctx context.Context, source string, password string) (*WalletInfo, error) {
	// Check if source file exists
	if _, err := os.Stat(source); os.IsNotExist(err) {
		return nil, fmt.Errorf("source file does not exist: %s", source)
	}

	// Try to open source wallet to validate it
	w, err := wallet.NewWalletFromFile(source)
	if err != nil {
		return nil, fmt.Errorf("invalid wallet file: %w", err)
	}

	// Validate password by trying to decrypt an account if one exists
	if len(w.Accounts) > 0 {
		account := w.Accounts[0]
		err := account.Decrypt(password, w.Scrypt)
		if err != nil {
			return nil, ErrInvalidPassword
		}
		// Clean up private key after verification
		account.Close()
	}

	// Get wallet name from filename
	baseName := filepath.Base(source)
	name := baseName[:len(baseName)-5] // Remove .json extension

	// Check if wallet already exists
	destPath := filepath.Join(s.config.WalletDir, name+".json")
	if _, err := os.Stat(destPath); err == nil {
		return nil, ErrWalletAlreadyExists
	}

	// Copy wallet file to wallet directory
	srcFile, err := os.Open(source)
	if err != nil {
		return nil, fmt.Errorf("failed to open source file: %w", err)
	}
	defer srcFile.Close()

	destFile, err := os.Create(destPath)
	if err != nil {
		return nil, fmt.Errorf("failed to create wallet file: %w", err)
	}
	defer destFile.Close()

	_, err = io.Copy(destFile, srcFile)
	if err != nil {
		return nil, fmt.Errorf("failed to copy wallet data: %w", err)
	}

	// Open restored wallet
	return s.OpenWallet(ctx, name, password)
}

// setupAutoLockTimer creates a timer to automatically lock a wallet after the configured timeout
func (s *ServiceImpl) setupAutoLockTimer(name string) {
	// Stop existing timer if any
	if timer, exists := s.autoLockTimers[name]; exists {
		timer.Stop()
	}

	// Create new timer
	timer := time.AfterFunc(time.Duration(s.config.AutoLockTimeout)*time.Second, func() {
		s.mu.Lock()
		defer s.mu.Unlock()

		// Remove password from memory but keep wallet loaded
		delete(s.walletPasswords, name)

		if s.config.AuditLog {
			s.logger.Info("Wallet auto-locked", zap.String("name", name))
		}
	})

	s.autoLockTimers[name] = timer
}

// getOpenWallet retrieves an open wallet or returns an error
func (s *ServiceImpl) getOpenWallet(name string) (*wallet.Wallet, error) {
	w, exists := s.openWallets[name]
	if !exists {
		return nil, ErrWalletNotOpen
	}
	return w, nil
}

// createWalletInfo creates a WalletInfo struct from a wallet
func (s *ServiceImpl) createWalletInfo(name string, w *wallet.Wallet) *WalletInfo {
	// Get default account if any
	var defaultAccount string

	// In a real implementation, we would get the default account
	// For this implementation, we'll leave it empty

	// Check if wallet is locked (no password in memory)
	_, passwordExists := s.walletPasswords[name]

	// Convert version string to int
	version := 1
	if w.Version != "" {
		if v, err := strconv.Atoi(w.Version); err == nil {
			version = v
		}
	}

	return &WalletInfo{
		Name:           name,
		Path:           w.Path(),
		Version:        version,
		Accounts:       len(w.Accounts),
		IsOpen:         true,
		IsLocked:       !passwordExists,
		DefaultAccount: defaultAccount,
		ScryptParams:   w.Scrypt,
	}
}
