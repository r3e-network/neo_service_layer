package neo_test

import (
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
	"github.com/will/neo_service_layer/internal/core/neo"
)

func TestNewClient(t *testing.T) {
	tests := []struct {
		name        string
		config      *neo.Config
		expectError bool
	}{
		{
			name: "valid config",
			config: &neo.Config{
				NodeURLs:   []string{"http://localhost:20332"},
				RetryDelay: time.Second,
				MaxRetries: 3,
			},
			expectError: false,
		},
		{
			name: "empty node URLs",
			config: &neo.Config{
				NodeURLs:   []string{},
				RetryDelay: time.Second,
				MaxRetries: 3,
			},
			expectError: true,
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			client, err := neo.NewClient(tt.config)
			if tt.expectError {
				assert.Error(t, err)
				assert.Nil(t, client)
			} else {
				assert.NoError(t, err)
				assert.NotNil(t, client)
			}
		})
	}
}

func TestClientClose(t *testing.T) {
	config := &neo.Config{
		NodeURLs:   []string{"http://localhost:20332"},
		RetryDelay: time.Second,
		MaxRetries: 3,
	}

	client, err := neo.NewClient(config)
	require.NoError(t, err)
	require.NotNil(t, client)

	client.Close()
}

func TestClientRotation(t *testing.T) {
	config := &neo.Config{
		NodeURLs: []string{
			"http://localhost:20332",
			"http://localhost:20333",
		},
		RetryDelay: time.Second,
		MaxRetries: 3,
	}

	client, err := neo.NewClient(config)
	require.NoError(t, err)
	require.NotNil(t, client)

	// Get initial client
	rpcClient := client.GetClient()
	require.NotNil(t, rpcClient)

	// Rotate client
	client.RotateClient()

	// Get new client
	newRpcClient := client.GetClient()
	require.NotNil(t, newRpcClient)

	client.Close()
}
