package secrets

import "errors"

// Predefined errors for the secrets service.
var (
	ErrSecretNotFound      = errors.New("secret not found")
	ErrPermissionDenied    = errors.New("permission denied")
	ErrSecretTooLarge      = errors.New("secret size exceeds maximum limit")
	ErrTooManySecrets      = errors.New("user has reached the maximum number of secrets")
	ErrInvalidConfig       = errors.New("invalid service configuration")
	ErrEncryptionFailed    = errors.New("secret encryption failed")
	ErrDecryptionFailed    = errors.New("secret decryption failed")
	ErrStoreFailed         = errors.New("failed to interact with secret store")
	ErrDuplicateSecretName = errors.New("a secret with this name already exists")
	ErrSecretExpired       = errors.New("secret has expired")

	// Generic store errors (can be wrapped)
	ErrNotFound      = errors.New("item not found in store")
	ErrAlreadyExists = errors.New("item already exists in store")
)
