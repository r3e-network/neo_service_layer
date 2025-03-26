package secrets

import (
	"context"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// IService defines the interface for the Secrets service
type IService interface {
	// StoreSecret stores a secret for a user
	StoreSecret(ctx context.Context, userAddress util.Uint160, key, value string, options map[string]interface{}) error

	// GetSecret retrieves a secret for a user
	GetSecret(ctx context.Context, userAddress util.Uint160, key string) (string, error)

	// DeleteSecret deletes a secret for a user
	DeleteSecret(ctx context.Context, userAddress util.Uint160, key string) error

	// ListSecrets lists all secrets for a user
	ListSecrets(ctx context.Context, userAddress util.Uint160) ([]string, error)
}
