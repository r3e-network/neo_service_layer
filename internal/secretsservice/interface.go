package secrets

import (
	"context"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// Service defines the interface for the Secrets service
type Service interface {
	// CreateSecret stores a new secret and its initial permissions.
	CreateSecret(ctx context.Context, owner util.Uint160, name, value string, allowedFuncIDs []string) (string, error)

	// GetSecretMetadata retrieves non-sensitive metadata for a secret owned by the caller.
	GetSecretMetadata(ctx context.Context, caller util.Uint160, secretID string) (*SecretMetadata, error)

	// ListSecrets lists metadata for all secrets owned by the caller.
	ListSecrets(ctx context.Context, caller util.Uint160) ([]SecretMetadata, error)

	// UpdateSecretValue updates the encrypted value of an existing secret owned by the caller.
	UpdateSecretValue(ctx context.Context, caller util.Uint160, secretID, newValue string) error

	// UpdateSecretPermissions updates the list of allowed function IDs for a secret owned by the caller.
	UpdateSecretPermissions(ctx context.Context, caller util.Uint160, secretID string, allowedFuncIDs []string) error

	// DeleteSecret removes a secret owned by the caller.
	DeleteSecret(ctx context.Context, caller util.Uint160, secretID string) error

	// --- Internal methods (potentially called by other services like Functions, likely within TEE) ---

	// GetSecretForFunction retrieves the decrypted secret value if the requesting function has permission.
	// This method assumes it's called from a trusted environment (e.g., TEE) that handles decryption securely.
	GetSecretForFunction(ctx context.Context, requestingFunctionID, secretID string) (string, error)

	// GetEncryptedSecretForFunction retrieves the *encrypted* secret value if the requesting function has permission.
	// This allows passing the encrypted value to a secure environment (like a function sandbox)
	// where it can be decrypted just-in-time via the TEESecurityProvider.
	GetEncryptedSecretForFunction(ctx context.Context, requestingFunctionID, secretID string) (string, error)
}

// TEESecurityProvider defines the interface for interacting with the TEE
// for cryptographic operations related to secrets.
type TEESecurityProvider interface {
	// Encrypt encrypts the given plaintext within the TEE.
	Encrypt(ctx context.Context, plaintext []byte) (ciphertext []byte, err error)

	// Decrypt decrypts the given ciphertext within the TEE.
	Decrypt(ctx context.Context, ciphertext []byte) (plaintext []byte, err error)
}
