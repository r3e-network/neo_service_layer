package secrets

import (
	"context"
	"fmt"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

func TestNewService(t *testing.T) {
	tests := []struct {
		name       string
		config     *Config
		shouldFail bool
	}{
		{
			name: "valid config",
			config: &Config{
				EncryptionKey:     "test-key-123",
				MaxSecretSize:     1024,
				MaxSecretsPerUser: 50,
			},
			shouldFail: false,
		},
		{
			name:       "nil config",
			config:     nil,
			shouldFail: true,
		},
		{
			name: "empty encryption key",
			config: &Config{
				EncryptionKey: "",
			},
			shouldFail: true,
		},
		{
			name: "zero max secret size",
			config: &Config{
				EncryptionKey: "test-key-123",
				MaxSecretSize: 0,
			},
			shouldFail: false, // Should use default
		},
		{
			name: "expiry enabled but no TTL",
			config: &Config{
				EncryptionKey:       "test-key-123",
				SecretExpiryEnabled: true,
				DefaultTTL:          0,
			},
			shouldFail: false, // Should use default
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			service, err := NewService(tt.config)
			if tt.shouldFail {
				assert.Error(t, err)
				assert.Nil(t, service)
			} else {
				assert.NoError(t, err)
				assert.NotNil(t, service)
				assert.NotNil(t, service.config)
				assert.NotNil(t, service.secrets)
				assert.NotNil(t, service.metadata)

				// Check default values if needed
				if tt.config != nil && tt.config.MaxSecretSize == 0 {
					assert.Greater(t, service.config.MaxSecretSize, 0)
				}
				if tt.config != nil && tt.config.MaxSecretsPerUser == 0 {
					assert.Greater(t, service.config.MaxSecretsPerUser, 0)
				}
				if tt.config != nil && tt.config.SecretExpiryEnabled && tt.config.DefaultTTL == 0 {
					assert.Greater(t, service.config.DefaultTTL, time.Duration(0))
				}
			}
		})
	}
}

func TestBasicFunctionality(t *testing.T) {
	// Setup
	service, err := NewService(&Config{
		EncryptionKey:     "test-key-123",
		MaxSecretSize:     1024,
		MaxSecretsPerUser: 10,
	})
	require.NoError(t, err)
	require.NotNil(t, service)

	// Create test user address
	userBytes, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")
	require.NoError(t, err)

	ctx := context.Background()

	// Store a secret
	err = service.StoreSecret(ctx, userBytes, "test-key", "test-value", nil)
	assert.NoError(t, err)

	// Get the secret
	value, err := service.GetSecret(ctx, userBytes, "test-key")
	assert.NoError(t, err)
	assert.Equal(t, "test-value", value)

	// List secrets
	keys, err := service.ListSecrets(ctx, userBytes)
	assert.NoError(t, err)
	assert.Contains(t, keys, "test-key")
	assert.Len(t, keys, 1)

	// Update secret
	err = service.StoreSecret(ctx, userBytes, "test-key", "updated-value", nil)
	assert.NoError(t, err)

	// Get updated secret
	value, err = service.GetSecret(ctx, userBytes, "test-key")
	assert.NoError(t, err)
	assert.Equal(t, "updated-value", value)

	// Delete secret
	err = service.DeleteSecret(ctx, userBytes, "test-key")
	assert.NoError(t, err)

	// Confirm deletion
	_, err = service.GetSecret(ctx, userBytes, "test-key")
	assert.Error(t, err)

	// List after deletion
	keys, err = service.ListSecrets(ctx, userBytes)
	assert.NoError(t, err)
	assert.Empty(t, keys)
}

func TestEncryptionFunctions(t *testing.T) {
	service, err := NewService(&Config{
		EncryptionKey: "test-key-123",
	})
	require.NoError(t, err)
	require.NotNil(t, service)

	// Test various inputs
	inputs := []string{
		"test",
		"longer test string with spaces",
		"special characters !@#$%^&*()",
		"",       // empty string
		"数据加密测试", // non-ASCII characters
	}

	for _, input := range inputs {
		// Encrypt
		encrypted, err := service.encrypt(input)
		assert.NoError(t, err)
		assert.NotEqual(t, input, encrypted, "Input: %s", input)

		// Decrypt
		decrypted, err := service.decrypt(encrypted)
		assert.NoError(t, err)
		assert.Equal(t, input, decrypted, "Input: %s", input)
	}

	// Test with invalid encrypted data
	_, err = service.decrypt("not-valid-base64!")
	assert.Error(t, err)
}

func TestSecretLimits(t *testing.T) {
	maxSize := 20
	maxSecretsPerUser := 3

	service, err := NewService(&Config{
		EncryptionKey:     "test-key-123",
		MaxSecretSize:     maxSize,
		MaxSecretsPerUser: maxSecretsPerUser,
	})
	require.NoError(t, err)
	require.NotNil(t, service)

	userBytes, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")
	require.NoError(t, err)

	ctx := context.Background()

	// Test size limit
	largeValue := "this string is longer than twenty characters"
	assert.Greater(t, len(largeValue), maxSize)

	err = service.StoreSecret(ctx, userBytes, "large-key", largeValue, nil)
	assert.Error(t, err)

	// Test count limit
	for i := 1; i <= maxSecretsPerUser; i++ {
		key := "key" + string(rune('0'+i))
		err = service.StoreSecret(ctx, userBytes, key, "value", nil)
		assert.NoError(t, err)
	}

	// Try to add one more
	err = service.StoreSecret(ctx, userBytes, "one-too-many", "value", nil)
	assert.Error(t, err)
}

func TestReadOnlyOption(t *testing.T) {
	service, err := NewService(&Config{
		EncryptionKey: "test-key-123",
	})
	require.NoError(t, err)
	require.NotNil(t, service)

	userBytes, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")
	require.NoError(t, err)

	ctx := context.Background()

	// Store a read-only secret
	options := map[string]interface{}{
		"readOnly": true,
	}

	err = service.StoreSecret(ctx, userBytes, "readonly-key", "protected-value", options)
	assert.NoError(t, err)

	// Try to delete it
	err = service.DeleteSecret(ctx, userBytes, "readonly-key")
	assert.Error(t, err)
	assert.Contains(t, err.Error(), "read-only")

	// Secret should still exist
	value, err := service.GetSecret(ctx, userBytes, "readonly-key")
	assert.NoError(t, err)
	assert.Equal(t, "protected-value", value)
}

func TestSecretSizeLimit(t *testing.T) {
	// Create service with small size limit
	maxSize := 10
	service, err := NewService(&Config{
		EncryptionKey:     "test-key-123",
		MaxSecretSize:     maxSize,
		MaxSecretsPerUser: 10,
	})
	require.NoError(t, err)
	require.NotNil(t, service)

	// Create test user address
	userBytes, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")
	require.NoError(t, err)

	ctx := context.Background()

	// Try to store a secret that's too large
	largeValue := "this-is-a-large-value-that-exceeds-the-limit"
	assert.Greater(t, len(largeValue), maxSize)

	err = service.StoreSecret(ctx, userBytes, "large-key", largeValue, nil)
	assert.Error(t, err)
	assert.Contains(t, err.Error(), "exceeds maximum size")

	// Try to store a valid size secret
	smallValue := "small"
	assert.LessOrEqual(t, len(smallValue), maxSize)

	err = service.StoreSecret(ctx, userBytes, "small-key", smallValue, nil)
	assert.NoError(t, err)
}

func TestMaxSecretsPerUser(t *testing.T) {
	// Create service with limit of 3 secrets per user
	maxSecrets := 3
	service, err := NewService(&Config{
		EncryptionKey:     "test-key-123",
		MaxSecretSize:     1024,
		MaxSecretsPerUser: maxSecrets,
	})
	require.NoError(t, err)
	require.NotNil(t, service)

	// Create test user address
	userBytes, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")
	require.NoError(t, err)

	ctx := context.Background()

	// Add secrets up to the limit
	for i := 0; i < maxSecrets; i++ {
		key := fmt.Sprintf("key-%d", i)
		err = service.StoreSecret(ctx, userBytes, key, "value", nil)
		assert.NoError(t, err)
	}

	// Try to add one more secret
	err = service.StoreSecret(ctx, userBytes, "one-too-many", "value", nil)
	assert.Error(t, err)
	assert.Contains(t, err.Error(), "maximum")

	// Updating an existing secret should work
	err = service.StoreSecret(ctx, userBytes, "key-0", "updated-value", nil)
	assert.NoError(t, err)

	// Delete a secret and add a new one
	err = service.DeleteSecret(ctx, userBytes, "key-1")
	assert.NoError(t, err)

	err = service.StoreSecret(ctx, userBytes, "replacement", "value", nil)
	assert.NoError(t, err)
}

func TestSecretWithOptions(t *testing.T) {
	// Create service
	service, err := NewService(&Config{
		EncryptionKey:       "test-key-123",
		MaxSecretSize:       1024,
		MaxSecretsPerUser:   10,
		SecretExpiryEnabled: true,
		DefaultTTL:          time.Hour,
	})
	require.NoError(t, err)
	require.NotNil(t, service)

	// Create test user address
	userBytes, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")
	require.NoError(t, err)

	ctx := context.Background()

	// Test storing a secret with custom TTL and options
	customTTL := 5 * time.Second
	options := map[string]interface{}{
		"ttl":      customTTL,
		"tags":     []string{"test", "temporary"},
		"readOnly": true,
	}

	err = service.StoreSecret(ctx, userBytes, "option-key", "option-value", options)
	assert.NoError(t, err)

	// Secret should exist
	value, err := service.GetSecret(ctx, userBytes, "option-key")
	assert.NoError(t, err)
	assert.Equal(t, "option-value", value)

	// Test read-only flag
	err = service.DeleteSecret(ctx, userBytes, "option-key")
	assert.Error(t, err)
	assert.Contains(t, err.Error(), "read-only")
}
