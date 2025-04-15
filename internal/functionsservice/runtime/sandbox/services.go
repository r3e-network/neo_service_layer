package sandbox

import (
	"context"
	"errors"
)

// WalletService defines the interface for wallet operations
type WalletService interface {
	// CreateWallet creates a new wallet with the given name and password
	CreateWallet(ctx context.Context, name, password string) error

	// OpenWallet opens an existing wallet
	OpenWallet(ctx context.Context, name, password string) error

	// CloseWallet closes an open wallet
	CloseWallet(ctx context.Context, name string) error

	// ListWallets returns a list of available wallets
	ListWallets(ctx context.Context) ([]string, error)

	// GetWalletInfo returns information about a wallet
	GetWalletInfo(ctx context.Context, name string) (map[string]interface{}, error)

	// CreateAccount creates a new account in the wallet
	CreateAccount(ctx context.Context, walletName string) (map[string]interface{}, error)

	// ListAccounts returns a list of accounts in the wallet
	ListAccounts(ctx context.Context, walletName string) ([]map[string]interface{}, error)

	// GetAccountInfo returns information about an account
	GetAccountInfo(ctx context.Context, walletName, address string) (map[string]interface{}, error)

	// SignMessage signs a message with the specified account
	SignMessage(ctx context.Context, walletName, address, message string) (string, error)
}

// StorageService defines the interface for storage operations
type StorageService interface {
	// Put stores a value with the given key
	Put(ctx context.Context, key string, value []byte) error

	// Get retrieves a value by key
	Get(ctx context.Context, key string) ([]byte, error)

	// Delete removes a key-value pair
	Delete(ctx context.Context, key string) error

	// List returns keys matching a prefix
	List(ctx context.Context, prefix string) ([]string, error)
}

// OracleService defines the interface for oracle operations
type OracleService interface {
	// GetData retrieves oracle data for a feed
	GetData(ctx context.Context, feedID string) (map[string]interface{}, error)

	// SubmitRequest submits a new oracle request
	SubmitRequest(ctx context.Context, feedType string, params map[string]interface{}) (string, error)

	// GetRequestStatus checks the status of an oracle request
	GetRequestStatus(ctx context.Context, requestID string) (map[string]interface{}, error)
}

// ServiceFactory creates service clients for JavaScript interoperability
type ServiceFactory interface {
	// CreateWalletService creates a new wallet service client
	CreateWalletService(url string) (WalletService, error)

	// CreateStorageService creates a new storage service client
	CreateStorageService(url string) (StorageService, error)

	// CreateOracleService creates a new oracle service client
	CreateOracleService(url string) (OracleService, error)
}

// DefaultServiceFactory is the default implementation of ServiceFactory
type DefaultServiceFactory struct{}

// CreateWalletService creates a new wallet service client
func (f *DefaultServiceFactory) CreateWalletService(url string) (WalletService, error) {
	return nil, errors.New("wallet service not implemented")
}

// CreateStorageService creates a new storage service client
func (f *DefaultServiceFactory) CreateStorageService(url string) (StorageService, error) {
	return nil, errors.New("storage service not implemented")
}

// CreateOracleService creates a new oracle service client
func (f *DefaultServiceFactory) CreateOracleService(url string) (OracleService, error) {
	return nil, errors.New("oracle service not implemented")
}
