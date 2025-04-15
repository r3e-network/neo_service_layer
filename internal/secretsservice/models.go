package secrets

import (
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// Config holds the configuration for the Secrets service
type Config struct {
	EncryptionKey       string        `yaml:"encryptionKey"`
	MaxSecretSize       int           `yaml:"maxSecretSize"`
	MaxSecretsPerUser   int           `yaml:"maxSecretsPerUser"`
	SecretExpiryEnabled bool          `yaml:"secretExpiryEnabled"`
	DefaultTTL          time.Duration `yaml:"defaultTTL"`
	StoreType           string        `yaml:"storeType"` // e.g., "memory", "postgres", "badger"
	StorePath           string        `yaml:"storePath"` // Path for file-based stores like BadgerDB
	// Add DB connection details if needed (e.g., for postgres)
}

// SecretMetadata holds non-sensitive information about a secret
type SecretMetadata struct {
	ID        string            `json:"id"`    // Unique identifier for the secret
	Name      string            `json:"name"`  // User-defined name for the secret
	Owner     util.Uint160      `json:"-"`     // Owner address (not usually exposed directly in API)
	OwnerStr  string            `json:"owner"` // String representation of owner address for API
	CreatedAt time.Time         `json:"createdAt"`
	UpdatedAt time.Time         `json:"updatedAt"`
	ExpiresAt time.Time         `json:"expiresAt,omitempty"`
	Tags      []string          `json:"tags,omitempty"`
	Metadata  map[string]string `json:"metadata,omitempty"` // Additional non-sensitive metadata
}

// SecretPermission defines who can access a specific secret
type SecretPermission struct {
	SecretID           string   `json:"secretId"`           // ID of the secret these permissions apply to
	AllowedFunctionIDs []string `json:"allowedFunctionIds"` // List of function IDs allowed to access
	// Could add AllowedUserAddresses []util.Uint160 in the future if needed
}
