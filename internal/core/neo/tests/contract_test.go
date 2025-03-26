package neo_test

import (
	"context"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
	"github.com/will/neo_service_layer/internal/core/neo"
)

func TestContractManager(t *testing.T) {
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

	t.Run("DeployContract", func(t *testing.T) {
		ctx := context.Background()
		nefFile := []byte("test nef file")
		manifest := []byte("test manifest")
		account, err := wallet.NewAccount()
		require.NoError(t, err)
		signer := &neo.Signer{
			Account: account.ScriptHash(),
			Scopes:  1, // CalledByEntry
		}

		hash, err := contractManager.DeployContract(ctx, nefFile, manifest, signer)
		assert.Error(t, err) // Should fail because we're not connected to a real node
		assert.Equal(t, util.Uint160{}, hash)
	})

	t.Run("InvokeContract", func(t *testing.T) {
		ctx := context.Background()
		hash := util.Uint160{}
		method := "test"
		params := []neo.ContractParameter{
			{
				Type:  "String",
				Value: "test",
			},
		}
		account, err := wallet.NewAccount()
		require.NoError(t, err)
		signers := []neo.Signer{
			{
				Account: account.ScriptHash(),
				Scopes:  1, // CalledByEntry
			},
		}

		result, err := contractManager.InvokeContract(ctx, hash, method, params, signers)
		assert.Error(t, err) // Should fail because we're not connected to a real node
		assert.Nil(t, result)
	})
}
