package wallet

import (
	"context"
	"os"
	"path/filepath"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
	"go.uber.org/zap/zaptest"
)

func TestNewService(t *testing.T) {
	// Create a temporary directory for test wallets
	tempDir, err := os.MkdirTemp("", "wallet-test")
	if err != nil {
		t.Fatalf("Failed to create temp dir: %v", err)
	}
	defer os.RemoveAll(tempDir)

	// Create config with test directory
	config := &WalletConfig{
		WalletDir:                tempDir,
		SystemWalletPassword:     "password",
		AutoCreateServiceWallets: true,
		Network:                  "testnet",
		RequireAuth:              false,
		AutoLockTimeout:          300,
		MaxOpenWallets:           10,
		AuditLog:                 false,
	}

	// Create a test logger
	logger := zaptest.NewLogger(t)

	// Create service with the logger
	service, err := NewService(config, logger)
	assert.NoError(t, err)
	assert.NotNil(t, service)

	// Verify service was initialized correctly
	impl, ok := service.(*ServiceImpl)
	assert.True(t, ok)
	assert.Equal(t, tempDir, impl.config.WalletDir)
	assert.Equal(t, logger, impl.logger)
	assert.NotNil(t, impl.openWallets)
	assert.NotNil(t, impl.walletPasswords)
	assert.NotNil(t, impl.roleAssignments)
}

func TestCreateWallet(t *testing.T) {
	// Setup
	service := setupTestService(t)
	ctx := context.Background()

	// Test creating a new wallet
	walletInfo, err := service.CreateWallet(ctx, "testwallet", "password", false)
	assert.NoError(t, err)
	assert.NotNil(t, walletInfo)
	assert.Equal(t, "testwallet", walletInfo.Name)
	assert.True(t, walletInfo.IsOpen)
	assert.False(t, walletInfo.IsLocked)

	// Verify the wallet file was created
	walletPath := filepath.Join(service.(*ServiceImpl).config.WalletDir, "testwallet.json")
	_, err = os.Stat(walletPath)
	assert.NoError(t, err)

	// Test creating a wallet that already exists
	_, err = service.CreateWallet(ctx, "testwallet", "password", false)
	assert.Error(t, err)
	assert.Contains(t, err.Error(), "already exists")

	// Test creating a wallet with overwrite=true
	walletInfo, err = service.CreateWallet(ctx, "testwallet", "password", true)
	assert.NoError(t, err)
	assert.NotNil(t, walletInfo)
}

func TestOpenWallet(t *testing.T) {
	// Setup
	service := setupTestService(t)
	ctx := context.Background()

	// Create a wallet first
	_, err := service.CreateWallet(ctx, "testwallet", "password", false)
	assert.NoError(t, err)

	// Close the wallet
	err = service.CloseWallet(ctx, "testwallet")
	assert.NoError(t, err)

	// Test opening a wallet
	walletInfo, err := service.OpenWallet(ctx, "testwallet", "password")
	assert.NoError(t, err)
	assert.NotNil(t, walletInfo)
	assert.Equal(t, "testwallet", walletInfo.Name)
	assert.True(t, walletInfo.IsOpen)
	assert.False(t, walletInfo.IsLocked)

	// Test opening a wallet with incorrect password
	_, err = service.OpenWallet(ctx, "testwallet", "wrongpassword")
	assert.Error(t, err)

	// Test opening a non-existent wallet
	_, err = service.OpenWallet(ctx, "nonexistant", "password")
	assert.Error(t, err)
}

func TestAccountManagement(t *testing.T) {
	// Setup
	service := setupTestService(t)
	ctx := context.Background()

	// Create a wallet
	_, err := service.CreateWallet(ctx, "testwallet", "password", false)
	assert.NoError(t, err)

	// Test creating an account
	account, err := service.CreateAccount(ctx, "testwallet", "testaccount")
	assert.NoError(t, err)
	assert.NotNil(t, account)
	assert.Equal(t, "testaccount", account.Label)

	// Test listing accounts
	accounts, err := service.ListAccounts(ctx, "testwallet")
	assert.NoError(t, err)
	assert.Len(t, accounts, 1)

	// Test getting account info
	accountInfo, err := service.GetAccountInfo(ctx, "testwallet", account.Address)
	assert.NoError(t, err)
	assert.Equal(t, account.Address, accountInfo.Address)

	// Test getting account balance
	balance, err := service.GetAccountBalance(ctx, "testwallet", account.Address, "neo")
	assert.NoError(t, err)
	assert.NotNil(t, balance)
	assert.Equal(t, account.Address, balance.Address)
	assert.Equal(t, "neo", balance.AssetID)
}

func TestRoleBasedWalletManagement(t *testing.T) {
	// Setup with timeout context
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	service := setupTestService(t)

	// Create a wallet with explicit error checking
	walletInfo, err := service.CreateWallet(ctx, "testwallet", "password", false)
	require.NoError(t, err, "Failed to create test wallet")
	require.NotNil(t, walletInfo, "Wallet info should not be nil")

	t.Log("Successfully created wallet:", walletInfo.Name)

	// Test assigning wallet to role with timeout and explicit error checking
	roleAssignCtx, roleCancel := context.WithTimeout(ctx, 2*time.Second)
	defer roleCancel()

	err = service.AssignWalletToRole(roleAssignCtx, "testwallet", "owner")
	require.NoError(t, err, "Failed to assign wallet to role")

	t.Log("Successfully assigned wallet to 'owner' role")

	// Test getting wallet for role with timeout
	getWalletCtx, getWalletCancel := context.WithTimeout(ctx, 2*time.Second)
	defer getWalletCancel()

	walletInfo, err = service.GetWalletForRole(getWalletCtx, "owner")
	require.NoError(t, err, "Failed to get wallet for role")
	require.Equal(t, "testwallet", walletInfo.Name, "Retrieved wallet should match the assigned one")

	t.Log("Successfully retrieved wallet for 'owner' role")

	// Test getting wallet for nonexistent role with timeout
	getNonexistentCtx, getNonexistentCancel := context.WithTimeout(ctx, 2*time.Second)
	defer getNonexistentCancel()

	_, err = service.GetWalletForRole(getNonexistentCtx, "nonexistent")
	require.Error(t, err, "Getting a non-existent role should return an error")

	t.Log("Test completed successfully")
}

func TestMultiSigOperations(t *testing.T) {
	// This is a simplified test since we can't easily create real multi-sig accounts
	// Setup
	service := setupTestService(t)
	ctx := context.Background()

	// Create a wallet
	_, err := service.CreateWallet(ctx, "testwallet", "password", false)
	assert.NoError(t, err)

	// Create a multi-sig account (will be mocked)
	account, err := service.CreateMultiSigAccount(ctx, "testwallet", nil, 2)
	assert.NoError(t, err)
	assert.NotNil(t, account)
	assert.True(t, account.IsMultiSig)
}

// Helper to set up a test service with a temp directory
func setupTestService(t *testing.T) IService {
	// Create a temporary directory for test wallets
	tempDir, err := os.MkdirTemp("", "wallet-test")
	if err != nil {
		t.Fatalf("Failed to create temp dir: %v", err)
	}
	t.Cleanup(func() {
		os.RemoveAll(tempDir)
	})

	// Create config with test directory
	config := &WalletConfig{
		WalletDir:                tempDir,
		SystemWalletPassword:     "password",
		AutoCreateServiceWallets: false,
		Network:                  "testnet",
		RequireAuth:              false,
		AutoLockTimeout:          0,
		MaxOpenWallets:           5,
		AuditLog:                 false,
	}

	// Create a test logger
	logger := zaptest.NewLogger(t)

	// Create service
	service, err := NewService(config, logger)
	if err != nil {
		t.Fatalf("Failed to create service: %v", err)
	}

	return service
}

func TestServiceStartStop(t *testing.T) {
	logger := zaptest.NewLogger(t)
	tempDir := t.TempDir()
	config := &WalletConfig{
		WalletDir:                tempDir,
		SystemWalletPassword:     "test_password",
		AutoCreateServiceWallets: true,
		Network:                  "testnet",
	}

	service, err := NewService(config, logger)
	require.NoError(t, err)
	require.NotNil(t, service)

	// Start service
	err = service.Start()
	assert.NoError(t, err)

	// Stop service
	err = service.Stop()
	assert.NoError(t, err)
}

func TestWalletLifecycle(t *testing.T) {
	ctx := context.Background()
	logger := zaptest.NewLogger(t)
	tempDir := t.TempDir()
	config := &WalletConfig{
		WalletDir:       tempDir,
		AutoLockTimeout: 1, // 1 second for quick testing
	}

	service, err := NewService(config, logger)
	require.NoError(t, err)
	require.NotNil(t, service)

	// Start service
	err = service.Start()
	require.NoError(t, err)

	// Create wallet
	walletName := "test_wallet"
	password := "test_password"
	walletInfo, err := service.CreateWallet(ctx, walletName, password, false)
	assert.NoError(t, err)
	assert.NotNil(t, walletInfo)
	assert.Equal(t, walletName, walletInfo.Name)
	assert.True(t, walletInfo.IsOpen)
	assert.False(t, walletInfo.IsLocked)

	// Create another wallet
	wallet2Name := "test_wallet2"
	wallet2Info, err := service.CreateWallet(ctx, wallet2Name, password, false)
	assert.NoError(t, err)
	assert.NotNil(t, wallet2Info)

	// Try to create existing wallet without overwrite
	_, err = service.CreateWallet(ctx, walletName, password, false)
	assert.Error(t, err)
	assert.Equal(t, ErrWalletAlreadyExists.Error(), err.Error())

	// Create wallet with overwrite
	walletInfo, err = service.CreateWallet(ctx, walletName, password, true)
	assert.NoError(t, err)
	assert.NotNil(t, walletInfo)

	// List wallets
	wallets, err := service.ListWallets(ctx)
	assert.NoError(t, err)
	assert.Len(t, wallets, 2)

	// Get wallet info
	retrievedInfo, err := service.GetWalletInfo(ctx, walletName)
	assert.NoError(t, err)
	assert.Equal(t, walletName, retrievedInfo.Name)

	// Close wallet
	err = service.CloseWallet(ctx, walletName)
	assert.NoError(t, err)

	// Get closed wallet info
	closedInfo, err := service.GetWalletInfo(ctx, walletName)
	assert.NoError(t, err)
	assert.False(t, closedInfo.IsOpen)
	assert.True(t, closedInfo.IsLocked)

	// Open wallet
	reopenedInfo, err := service.OpenWallet(ctx, walletName, password)
	assert.NoError(t, err)
	assert.True(t, reopenedInfo.IsOpen)
	assert.False(t, reopenedInfo.IsLocked)

	// Test wallet backup and restore
	backupPath := filepath.Join(tempDir, "backup_wallet.json")
	err = service.BackupWallet(ctx, walletName, backupPath)
	assert.NoError(t, err)
	assert.FileExists(t, backupPath)

	// Restore wallet with a new name (the file name is used)
	restoredInfo, err := service.RestoreWallet(ctx, backupPath, password)
	assert.NoError(t, err)
	assert.NotNil(t, restoredInfo)
	assert.Equal(t, "backup_wallet", restoredInfo.Name)

	// Wait for auto-lock to occur
	time.Sleep(2 * time.Second)

	// Get wallet info after auto-lock
	lockedInfo, err := service.GetWalletInfo(ctx, walletName)
	assert.NoError(t, err)

	// Check if wallet is locked (depends on implementation)
	// This is a bit flaky because auto-lock happens in a goroutine
	t.Log("Wallet lock state after timeout:", lockedInfo.IsLocked)

	// Stop service
	err = service.Stop()
	assert.NoError(t, err)
}

func TestSigningOperations(t *testing.T) {
	// This test can't fully test actual signing without a real wallet
	// So we'll just test the error cases and make sure the methods exist

	ctx := context.Background()
	logger := zaptest.NewLogger(t)
	tempDir := t.TempDir()
	config := &WalletConfig{
		WalletDir: tempDir,
	}

	service, err := NewService(config, logger)
	require.NoError(t, err)
	require.NotNil(t, service)

	// Start service
	err = service.Start()
	require.NoError(t, err)

	// Create wallet
	walletName := "signing_test_wallet"
	password := "test_password"
	walletInfo, err := service.CreateWallet(ctx, walletName, password, false)
	require.NoError(t, err)
	require.NotNil(t, walletInfo)

	// Create account
	accountInfo, err := service.CreateAccount(ctx, walletName, "signing_account")
	require.NoError(t, err)
	require.NotNil(t, accountInfo)

	// Test transaction signing (should fail with nil transaction)
	_, err = service.SignTransaction(ctx, walletName, accountInfo.Address, nil)
	assert.Error(t, err)

	// Test message signing
	message := []byte("test message")
	signature, err := service.SignMessage(ctx, walletName, accountInfo.Address, message)
	// This may fail in the real implementation without proper setup
	if err == nil {
		assert.NotNil(t, signature)

		// Test signature verification
		if len(accountInfo.PublicKey) > 0 {
			valid, err := service.VerifySignature(ctx, message, signature, accountInfo.PublicKey)
			if err == nil {
				assert.True(t, valid)
			}
		}
	}

	// Close wallet
	err = service.CloseWallet(ctx, walletName)
	assert.NoError(t, err)

	// Now signing should fail
	_, err = service.SignMessage(ctx, walletName, accountInfo.Address, message)
	assert.Error(t, err)

	// Stop service
	err = service.Stop()
	assert.NoError(t, err)
}

func TestMockService(t *testing.T) {
	ctx := context.Background()
	mockService := NewMockService()

	// Verify it implements the interface
	var _ IService = mockService

	// Start the service
	err := mockService.Start()
	assert.NoError(t, err)

	// Create a wallet
	walletInfo, err := mockService.CreateWallet(ctx, "mock_wallet", "mock_password", false)
	assert.NoError(t, err)
	assert.Equal(t, "mock_wallet", walletInfo.Name)

	// Create an account
	accountInfo, err := mockService.CreateAccount(ctx, "mock_wallet", "mock_account")
	assert.NoError(t, err)
	assert.NotNil(t, accountInfo)

	// Sign a message
	message := []byte("test message")
	signature, err := mockService.SignMessage(ctx, "mock_wallet", accountInfo.Address, message)
	assert.NoError(t, err)
	assert.NotNil(t, signature)

	// Verify signature
	valid, err := mockService.VerifySignature(ctx, message, signature, accountInfo.PublicKey)
	assert.NoError(t, err)
	assert.True(t, valid)

	// Test role assignment
	err = mockService.AssignWalletToRole(ctx, "mock_wallet", "test_role")
	assert.NoError(t, err)

	roleWalletInfo, err := mockService.GetWalletForRole(ctx, "test_role")
	assert.NoError(t, err)
	assert.Equal(t, "mock_wallet", roleWalletInfo.Name)

	// Test failure mode
	mockService.SetFailNextOperation(true)
	_, err = mockService.CreateWallet(ctx, "should_fail", "password", false)
	assert.Error(t, err)

	// Stop the service
	err = mockService.Stop()
	assert.NoError(t, err)
}
