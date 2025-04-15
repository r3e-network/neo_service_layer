package neo_test

import (
	"context"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/r3e-network/neo_service_layer/internal/core/neo"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

func TestIntegration(t *testing.T) {
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

	contractManager := neo.NewContractManager(client, txManager)
	require.NotNil(t, contractManager)

	t.Run("FullContractLifecycle", func(t *testing.T) {
		ctx := context.Background()

		// Create test account
		account, err := wallet.NewAccount()
		require.NoError(t, err)
		require.NotNil(t, account)

		// Deploy contract
		nefFile := []byte("test nef file")
		manifest := []byte("test manifest")
		signer := &neo.Signer{
			Account: account.ScriptHash(),
			Scopes:  1, // CalledByEntry
		}

		contractHash, err := contractManager.DeployContract(ctx, nefFile, manifest, signer)
		assert.Error(t, err) // Should fail because we're not connected to a real node
		assert.Equal(t, util.Uint160{}, contractHash)

		// Invoke contract
		method := "test"
		params := []neo.ContractParameter{
			{
				Type:  "String",
				Value: "test",
			},
		}
		signers := []neo.Signer{*signer}

		result, err := contractManager.InvokeContract(ctx, contractHash, method, params, signers)
		assert.Error(t, err) // Should fail because we're not connected to a real node
		assert.Nil(t, result)
	})
}
