package neo

import (
	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/smartcontract"
	"github.com/nspcc-dev/neo-go/pkg/smartcontract/trigger"
	"github.com/nspcc-dev/neo-go/pkg/util"
)

// RealNeoClient defines an interface for interacting with the Neo blockchain
// Aligned with the older NeoClient definition for compatibility
type RealNeoClient interface {
	// InvokeFunction invokes a smart contract method with parameters
	InvokeFunction(contract util.Uint160, operation string, params []smartcontract.Parameter, signers []transaction.Signer) (interface{}, error)

	// SendRawTransaction sends a signed transaction to the network
	SendRawTransaction(tx *transaction.Transaction) (util.Uint256, error)

	// GetApplicationLog retrieves the application log for a transaction
	GetApplicationLog(txHash util.Uint256, trig *trigger.Type) (interface{}, error)

	// CalculateNetworkFee calculates the network fee for a transaction
	CalculateNetworkFee(tx *transaction.Transaction) (int64, error)

	// GetBlockCount gets the current block height
	GetBlockCount() (uint32, error)

	// GetNetwork gets the network magic number (as uint64 for compatibility)
	GetNetwork() (uint64, error)

	// WaitForTransaction waits until a transaction is confirmed on the blockchain
	WaitForTransaction(txHash string, timeout int) (bool, error)
}
