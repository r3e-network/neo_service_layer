package neo

import (
	"math/big"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// Config represents the Neo N3 client configuration
type Config struct {
	// NodeURLs is a list of Neo N3 node URLs for redundancy
	NodeURLs []string
	// NetworkMagic is the network identifier
	NetworkMagic uint32
	// MaxRetries is the maximum number of retries for failed operations
	MaxRetries int
	// RetryDelay is the delay between retries
	RetryDelay time.Duration
}

// TransactionResult represents the result of a transaction
type TransactionResult struct {
	// Hash is the transaction hash
	Hash util.Uint256
	// BlockIndex is the block number containing the transaction
	BlockIndex uint32
	// Timestamp is when the transaction was included in a block
	Timestamp time.Time
	// Success indicates if the transaction was successful
	Success bool
	// GasConsumed is the amount of GAS consumed by the transaction
	GasConsumed *big.Int
}

// ContractParameter represents a parameter for contract invocation
type ContractParameter struct {
	// Type is the parameter type
	Type string
	// Value is the parameter value
	Value interface{}
}

// Event represents a contract notification event
type Event struct {
	// Contract is the contract that emitted the event
	Contract util.Uint160
	// Name is the event name
	Name string
	// Parameters contains the event parameters
	Parameters []ContractParameter
	// Timestamp is when the event was emitted
	Timestamp time.Time
	// BlockIndex is the block number containing the event
	BlockIndex uint32
	// TxHash is the transaction hash that triggered the event
	TxHash util.Uint256
}

// Signer represents a transaction signer
type Signer struct {
	// Account is the signer's account
	Account util.Uint160
	// Scopes defines the witness scope
	Scopes WitnessScope
}

// WitnessScope represents the scope of a witness
type WitnessScope byte

const (
	// None represents no witness scope
	None WitnessScope = 0
	// CalledByEntry represents witness scope for entry calls
	CalledByEntry WitnessScope = 1
	// Global represents global witness scope
	Global WitnessScope = 2
)