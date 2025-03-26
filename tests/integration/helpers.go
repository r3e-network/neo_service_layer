package integration

import (
	"testing"

	"github.com/nspcc-dev/neo-go/pkg/crypto/keys"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/stretchr/testify/require"
)

// InitNeoClient initializes a Neo blockchain client for testing
func InitNeoClient(t *testing.T) interface{} {
	// This is a mock implementation for testing purposes
	// In a real implementation, you would initialize a real Neo client
	// and return it for use in tests
	return struct{}{}
}

// CreateTestAccount creates a test account for testing
func CreateTestAccount(t *testing.T) *wallet.Account {
	// Generate a new private key
	privateKey, err := keys.NewPrivateKey()
	require.NoError(t, err)

	// Create a new account from the private key
	account := wallet.NewAccountFromPrivateKey(privateKey)
	return account
}
