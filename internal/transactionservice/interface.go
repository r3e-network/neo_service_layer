package transaction

import (
	"context"

	"github.com/nspcc-dev/neo-go/pkg/wallet"
)

// Service defines the interface for transaction operations
type Service interface {
	// Create creates a new transaction with the given configuration
	// Returns a unique transaction ID or an error
	Create(config map[string]interface{}) (string, error)

	// Sign signs a transaction with the given ID and account
	// Returns the signed transaction details or an error
	Sign(id string, account *wallet.Account) (map[string]interface{}, error)

	// Send sends a transaction with the given ID to the blockchain
	// Returns the transaction hash or an error
	Send(ctx context.Context, id string) (string, error)

	// Status gets the status of a transaction with the given hash
	// Returns the transaction status or an error
	Status(hash string) (string, error)

	// Get gets the details of a transaction with the given ID
	// Returns the transaction details or an error
	Get(id string) (map[string]interface{}, error)

	// List lists all transactions
	// Returns a list of transactions or an error
	List() ([]interface{}, error)

	// EstimateFee estimates the fee for a transaction with the given configuration
	// Returns the estimated fee as a string or an error
	EstimateFee(config map[string]interface{}) (string, error)
}