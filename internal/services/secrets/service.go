package secrets

import (
	"context"
	"crypto/aes"
	"crypto/cipher"
	"crypto/rand"
	"crypto/sha256"
	"encoding/base64"
	"errors"
	"fmt"
	"io"
	"sync"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// Service implements the Secrets service
type Service struct {
	config        *Config
	secrets       map[string]map[string]string // user -> key -> encrypted value
	metadata      map[string]map[string]SecretMetadata // user -> key -> metadata
	mu            sync.RWMutex
}

// NewService creates a new Secrets service
func NewService(config *Config) (*Service, error) {
	if config == nil {
		return nil, errors.New("config cannot be nil")
	}

	// Validate config
	if config.EncryptionKey == "" {
		return nil, errors.New("encryption key cannot be empty")
	}
	if config.MaxSecretSize <= 0 {
		config.MaxSecretSize = 10 * 1024 // 10KB default
	}
	if config.MaxSecretsPerUser <= 0 {
		config.MaxSecretsPerUser = 100 // 100 secrets default
	}
	if config.SecretExpiryEnabled && config.DefaultTTL <= 0 {
		config.DefaultTTL = 24 * time.Hour // 24 hours default
	}

	return &Service{
		config:   config,
		secrets:  make(map[string]map[string]string),
		metadata: make(map[string]map[string]SecretMetadata),
	}, nil
}

// StoreSecret stores a secret for a user
func (s *Service) StoreSecret(ctx context.Context, userAddress util.Uint160, key, value string, options map[string]interface{}) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Check secret size
	if len(value) > s.config.MaxSecretSize {
		return fmt.Errorf("secret size exceeds maximum size of %d bytes", s.config.MaxSecretSize)
	}

	// Get or initialize user's secrets
	userKey := userAddress.StringLE()
	if _, exists := s.secrets[userKey]; !exists {
		s.secrets[userKey] = make(map[string]string)
		s.metadata[userKey] = make(map[string]SecretMetadata)
	}

	// Check if user has reached max secrets
	if len(s.secrets[userKey]) >= s.config.MaxSecretsPerUser && s.secrets[userKey][key] == "" {
		return fmt.Errorf("user has reached maximum of %d secrets", s.config.MaxSecretsPerUser)
	}

	// Encrypt the secret
	encryptedValue, err := s.encrypt(value)
	if err != nil {
		return fmt.Errorf("failed to encrypt secret: %w", err)
	}

	// Store the secret
	s.secrets[userKey][key] = encryptedValue

	// Create or update metadata
	meta := SecretMetadata{
		UpdatedAt: time.Now(),
	}

	// Set creation time if it's a new secret
	if _, exists := s.metadata[userKey][key]; !exists {
		meta.CreatedAt = meta.UpdatedAt
	} else {
		meta.CreatedAt = s.metadata[userKey][key].CreatedAt
	}

	// Parse options
	if options != nil {
		// Set expiration if provided
		if ttl, ok := options["ttl"].(time.Duration); ok && ttl > 0 {
			meta.ExpiresAt = meta.UpdatedAt.Add(ttl)
		} else if s.config.SecretExpiryEnabled {
			meta.ExpiresAt = meta.UpdatedAt.Add(s.config.DefaultTTL)
		}

		// Set tags if provided
		if tags, ok := options["tags"].([]string); ok {
			meta.Tags = tags
		}

		// Set read-only flag if provided
		if readOnly, ok := options["readOnly"].(bool); ok {
			meta.IsReadOnly = readOnly
		}
	} else if s.config.SecretExpiryEnabled {
		// Apply default TTL if enabled
		meta.ExpiresAt = meta.UpdatedAt.Add(s.config.DefaultTTL)
	}

	// Store metadata
	s.metadata[userKey][key] = meta

	return nil
}

// GetSecret retrieves a secret for a user
func (s *Service) GetSecret(ctx context.Context, userAddress util.Uint160, key string) (string, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	// Get user's secrets
	userKey := userAddress.StringLE()
	userSecrets, exists := s.secrets[userKey]
	if !exists {
		return "", fmt.Errorf("no secrets found for user")
	}

	// Get the encrypted secret
	encryptedValue, exists := userSecrets[key]
	if !exists {
		return "", fmt.Errorf("secret with key %s not found", key)
	}

	// Check if secret is expired
	if meta, exists := s.metadata[userKey][key]; exists && !meta.ExpiresAt.IsZero() && meta.ExpiresAt.Before(time.Now()) {
		// Clean up expired secret
		s.mu.RUnlock()
		s.mu.Lock()
		defer s.mu.Unlock()
		
		delete(s.secrets[userKey], key)
		delete(s.metadata[userKey], key)
		
		return "", fmt.Errorf("secret with key %s has expired", key)
	}

	// Decrypt the secret
	decryptedValue, err := s.decrypt(encryptedValue)
	if err != nil {
		return "", fmt.Errorf("failed to decrypt secret: %w", err)
	}

	return decryptedValue, nil
}

// DeleteSecret deletes a secret for a user
func (s *Service) DeleteSecret(ctx context.Context, userAddress util.Uint160, key string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Get user's secrets
	userKey := userAddress.StringLE()
	userSecrets, exists := s.secrets[userKey]
	if !exists {
		return fmt.Errorf("no secrets found for user")
	}

	// Check if secret exists
	if _, exists := userSecrets[key]; !exists {
		return fmt.Errorf("secret with key %s not found", key)
	}

	// Check if secret is read-only
	if meta, exists := s.metadata[userKey][key]; exists && meta.IsReadOnly {
		return fmt.Errorf("cannot delete read-only secret")
	}

	// Delete the secret and metadata
	delete(s.secrets[userKey], key)
	delete(s.metadata[userKey], key)

	return nil
}

// ListSecrets lists all secrets for a user
func (s *Service) ListSecrets(ctx context.Context, userAddress util.Uint160) ([]string, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	// Get user's secrets
	userKey := userAddress.StringLE()
	userSecrets, exists := s.secrets[userKey]
	if !exists {
		return []string{}, nil
	}

	// Get list of secret keys
	keys := make([]string, 0, len(userSecrets))
	for key := range userSecrets {
		// Check if secret is expired
		if meta, exists := s.metadata[userKey][key]; exists && !meta.ExpiresAt.IsZero() && meta.ExpiresAt.Before(time.Now()) {
			continue
		}
		keys = append(keys, key)
	}

	return keys, nil
}

// encrypt encrypts a value
func (s *Service) encrypt(value string) (string, error) {
	// Create a key from the encryption key
	key := sha256.Sum256([]byte(s.config.EncryptionKey))

	// Create the AES cipher
	block, err := aes.NewCipher(key[:])
	if err != nil {
		return "", err
	}

	// Create the GCM cipher mode
	aesGCM, err := cipher.NewGCM(block)
	if err != nil {
		return "", err
	}

	// Create a nonce
	nonce := make([]byte, aesGCM.NonceSize())
	if _, err := io.ReadFull(rand.Reader, nonce); err != nil {
		return "", err
	}

	// Encrypt the value
	ciphertext := aesGCM.Seal(nonce, nonce, []byte(value), nil)

	// Encode the result as base64
	return base64.StdEncoding.EncodeToString(ciphertext), nil
}

// decrypt decrypts a value
func (s *Service) decrypt(encryptedValue string) (string, error) {
	// Create a key from the encryption key
	key := sha256.Sum256([]byte(s.config.EncryptionKey))

	// Create the AES cipher
	block, err := aes.NewCipher(key[:])
	if err != nil {
		return "", err
	}

	// Create the GCM cipher mode
	aesGCM, err := cipher.NewGCM(block)
	if err != nil {
		return "", err
	}

	// Decode the base64 value
	ciphertext, err := base64.StdEncoding.DecodeString(encryptedValue)
	if err != nil {
		return "", err
	}

	// Check if the ciphertext is valid
	if len(ciphertext) < aesGCM.NonceSize() {
		return "", errors.New("invalid ciphertext")
	}

	// Split the nonce and ciphertext
	nonce, ciphertext := ciphertext[:aesGCM.NonceSize()], ciphertext[aesGCM.NonceSize():]

	// Decrypt the value
	plaintext, err := aesGCM.Open(nil, nonce, ciphertext, nil)
	if err != nil {
		return "", err
	}

	return string(plaintext), nil
}