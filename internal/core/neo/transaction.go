package neo

import (
	"context"
	"fmt"
	"math/big"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
)

// TransactionManager handles transaction creation and submission
type TransactionManager struct {
	client *Client
}

// NewTransactionManager creates a new transaction manager
func NewTransactionManager(client *Client) *TransactionManager {
	return &TransactionManager{
		client: client,
	}
}

// CreateTransaction creates a new transaction
func (tm *TransactionManager) CreateTransaction(
	script []byte,
	signers []Signer,
) (*transaction.Transaction, error) {
	if len(script) == 0 {
		return nil, fmt.Errorf("empty script")
	}

	tx := transaction.New(script, 0)

	// Convert signers
	for _, signer := range signers {
		tx.Signers = append(tx.Signers, transaction.Signer{
			Account: signer.Account,
			Scopes:  transaction.WitnessScope(signer.Scopes),
		})
	}

	return tx, nil
}

// SignTransaction signs a transaction with the provided account
func (tm *TransactionManager) SignTransaction(
	tx *transaction.Transaction,
	account *wallet.Account,
) error {
	if tx == nil {
		return fmt.Errorf("nil transaction")
	}
	if account == nil {
		return fmt.Errorf("nil account")
	}

	// Sign the transaction
	signature := account.PrivateKey().SignHash(tx.Hash())

	// Create witness
	witness := transaction.Witness{
		InvocationScript:   append([]byte{byte(len(signature))}, signature...),
		VerificationScript: account.Contract.Script,
	}

	tx.Scripts = append(tx.Scripts, witness)
	return nil
}

// SendTransaction sends a transaction to the network
func (tm *TransactionManager) SendTransaction(
	ctx context.Context,
	tx *transaction.Transaction,
) (*TransactionResult, error) {
	var lastErr error
	for i := 0; i <= tm.client.config.MaxRetries; i++ {
		client := tm.client.GetClient()
		hash, err := client.SendRawTransaction(tx)
		if err == nil {
			// Wait for transaction to be included in a block
			result, err := tm.waitForTransaction(ctx, hash)
			if err == nil {
				return result, nil
			}
			lastErr = err
		} else {
			lastErr = err
		}

		tm.client.RotateClient()
		if i < tm.client.config.MaxRetries {
			time.Sleep(tm.client.config.RetryDelay)
		}
	}

	return nil, fmt.Errorf("failed to send transaction after %d retries: %w",
		tm.client.config.MaxRetries, lastErr)
}

// waitForTransaction waits for a transaction to be included in a block
func (tm *TransactionManager) waitForTransaction(
	ctx context.Context,
	hash util.Uint256,
) (*TransactionResult, error) {
	ticker := time.NewTicker(time.Second)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			return nil, ctx.Err()
		case <-ticker.C:
			client := tm.client.GetClient()
			_, err := client.GetRawTransaction(hash)
			if err == nil {
				return &TransactionResult{
					Hash:        hash,
					BlockIndex:  0,
					Timestamp:   time.Now(),
					Success:     true,
					GasConsumed: big.NewInt(0),
				}, nil
			}
		}
	}
}
