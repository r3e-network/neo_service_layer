package neo_test

import (
	"context"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/r3e-network/neo_service_layer/internal/core/neo"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

func TestTransactionManager(t *testing.T) {
	config := &neo.Config{
		NodeURLs:   []string{"http://localhost:20332"},
		RetryDelay: time.Second,
		MaxRetries: 3,
	}

	client, err := neo.NewClient(config)
	require.NoError(t, err)
	require.NotNil(t, client)
	defer client.Close()

	txManager := neo.NewTransactionManager(client)
	require.NotNil(t, txManager)

	t.Run("CreateTransaction", func(t *testing.T) {
		account, err := wallet.NewAccount()
		require.NoError(t, err)
		require.NotNil(t, account)

		script := []byte("test script")
		signers := []neo.Signer{
			{
				Account: account.ScriptHash(),
				Scopes:  1, // CalledByEntry
			},
		}

		tx, err := txManager.CreateTransaction(script, signers)
		assert.NoError(t, err)
		assert.NotNil(t, tx)
		assert.Equal(t, script, tx.Script)
		assert.Len(t, tx.Signers, 1)
	})

	t.Run("SignTransaction", func(t *testing.T) {
		account, err := wallet.NewAccount()
		require.NoError(t, err)
		require.NotNil(t, account)

		script := []byte("test script")
		signers := []neo.Signer{
			{
				Account: account.ScriptHash(),
				Scopes:  1, // CalledByEntry
			},
		}

		tx, err := txManager.CreateTransaction(script, signers)
		require.NoError(t, err)
		require.NotNil(t, tx)

		err = txManager.SignTransaction(tx, account)
		assert.NoError(t, err)
		assert.Len(t, tx.Scripts, 1)
	})

	t.Run("SendTransaction", func(t *testing.T) {
		ctx := context.Background()
		account, err := wallet.NewAccount()
		require.NoError(t, err)
		require.NotNil(t, account)

		script := []byte("test script")
		signers := []neo.Signer{
			{
				Account: account.ScriptHash(),
				Scopes:  1, // CalledByEntry
			},
		}

		tx, err := txManager.CreateTransaction(script, signers)
		require.NoError(t, err)
		require.NotNil(t, tx)

		err = txManager.SignTransaction(tx, account)
		require.NoError(t, err)

		result, err := txManager.SendTransaction(ctx, tx)
		assert.Error(t, err) // Should fail because we're not connected to a real node
		assert.Nil(t, result)
	})
}
