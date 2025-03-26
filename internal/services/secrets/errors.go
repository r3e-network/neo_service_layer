package secrets

import "errors"

// Define common errors for the Secrets service
var (
	// ErrSecretNotFound is returned when a secret is not found
	ErrSecretNotFound = errors.New("secret not found")

	// ErrSecretExpired is returned when a secret has expired
	ErrSecretExpired = errors.New("secret has expired")

	// ErrSecretTooLarge is returned when a secret is too large
	ErrSecretTooLarge = errors.New("secret is too large")

	// ErrMaxSecretsReached is returned when user has reached the maximum number of secrets
	ErrMaxSecretsReached = errors.New("maximum number of secrets reached")

	// ErrReadOnlySecret is returned when trying to modify a read-only secret
	ErrReadOnlySecret = errors.New("cannot modify read-only secret")

	// ErrPermissionDenied is returned when a user does not have permission to access a secret
	ErrPermissionDenied = errors.New("permission denied")

	// ErrEncryptionFailed is returned when encryption fails
	ErrEncryptionFailed = errors.New("encryption failed")

	// ErrDecryptionFailed is returned when decryption fails
	ErrDecryptionFailed = errors.New("decryption failed")
)