package secrets

import (
	"context"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// Store defines the interface for persistent storage of secrets and permissions.
type Store interface {
	// SaveSecret stores the secret metadata and its encrypted value.
	// It should handle both creation and updates based on whether metadata.ID is new.
	SaveSecret(ctx context.Context, userAddress util.Uint160, metadata SecretMetadata, encryptedValue string) error

	// GetSecretMetadata retrieves the metadata for a specific secret ID owned by the user.
	GetSecretMetadata(ctx context.Context, userAddress util.Uint160, secretID string) (*SecretMetadata, error)

	// GetSecretValue retrieves the encrypted value for a specific secret ID owned by the user.
	GetSecretValue(ctx context.Context, userAddress util.Uint160, secretID string) (string, error)

	// GetSecretValueByID retrieves the encrypted value for a specific secret ID, regardless of user.
	// This is intended for internal use where ownership/permissions are checked separately (e.g., GetSecretForFunction).
	GetSecretValueByID(ctx context.Context, secretID string) (string, error)

	// ListSecretsByUser retrieves metadata for all secrets owned by a specific user.
	ListSecretsByUser(ctx context.Context, userAddress util.Uint160) ([]SecretMetadata, error)

	// DeleteSecret removes a secret (metadata and value) by its ID, verifying ownership.
	DeleteSecret(ctx context.Context, userAddress util.Uint160, secretID string) error

	// SavePermissions stores the permissions for a specific secret.
	SavePermissions(ctx context.Context, userAddress util.Uint160, secretID string, permissions SecretPermission) error

	// GetPermissions retrieves the permissions for a specific secret.
	GetPermissions(ctx context.Context, secretID string) (*SecretPermission, error)

	// DeletePermissions removes the permissions associated with a secret (used when deleting the secret).
	DeletePermissions(ctx context.Context, secretID string) error

	// Close closes the database connection.
	Close() error
}
