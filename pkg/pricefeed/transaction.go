package pricefeed

import (
	"context"
	"encoding/json"

	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/rpcclient"
	"github.com/nspcc-dev/neo-go/pkg/smartcontract"
	"github.com/nspcc-dev/neo-go/pkg/util"
)

// createPriceUpdateTx creates a transaction to update prices on the blockchain
func (s *Service) createPriceUpdateTx(ctx context.Context, prices map[string]float64) (*transaction.Transaction, error) {
	// Convert contract hash to Uint160
	hash, err := util.Uint160DecodeStringLE(s.config.ContractHash)
	if err != nil {
		return nil, err
	}

	// Convert prices to JSON
	pricesJSON, err := json.Marshal(prices)
	if err != nil {
		return nil, err
	}

	// Create script for invoking contract method
	script, err := smartcontract.CreateCallScript(hash, s.config.ContractMethod, pricesJSON)
	if err != nil {
		return nil, err
	}

	// Create transaction
	tx := transaction.New(script, int64(s.config.GasPerUpdate))
	tx.Signers = []transaction.Signer{
		{
			Account: s.wallet.Accounts[0].ScriptHash(),
			Scopes:  transaction.CalledByEntry,
		},
	}

	return tx, nil
}

// publishTransaction publishes a transaction to the blockchain
func (s *Service) publishTransaction(ctx context.Context, tx *transaction.Transaction) error {
	// Create RPC client
	client, err := rpcclient.New(ctx, s.config.RPCEndpoint, rpcclient.Options{})
	if err != nil {
		return err
	}
	defer client.Close()

	// Get first account from wallet
	account := s.wallet.Accounts[0]

	// Sign transaction
	if err := account.SignTx(0x00000000, tx); err != nil { // TODO: Get correct network magic
		return err
	}

	// Send transaction
	_, err = client.SendRawTransaction(tx)
	return err
}
