package secrets

import (
	"time"
)

// Config holds configuration for the Secrets service
type Config struct {
	EncryptionKey       string
	MaxSecretSize       int
	MaxSecretsPerUser   int
	SecretExpiryEnabled bool
	DefaultTTL          time.Duration
}

// SecretMetadata holds metadata for a secret
type SecretMetadata struct {
	CreatedAt  time.Time
	UpdatedAt  time.Time
	ExpiresAt  time.Time
	Tags       []string
	IsReadOnly bool
}