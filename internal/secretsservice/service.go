package secrets

import (
	"context"
	"encoding/base64"
	"errors"
	"fmt"
	"strings"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	log "github.com/sirupsen/logrus" // Added for logging
)

// Compile-time check to ensure Service implements the interface
var _ Service = (*ServiceImpl)(nil)

// ServiceImpl implements the Secrets service interface.
type ServiceImpl struct {
	config *Config
	store  Store
	tee    TEESecurityProvider // Interface for TEE operations
	mu     sync.RWMutex        // Mutex for coordinating access if store is not thread-safe
}

// NewServiceImpl creates a new Secrets service implementation.
func NewServiceImpl(config *Config, store Store, teeProvider TEESecurityProvider) (*ServiceImpl, error) {
	if config == nil {
		return nil, errors.New("config cannot be nil")
	}
	if store == nil {
		return nil, errors.New("store cannot be nil")
	}
	if teeProvider == nil {
		// If no TEE provider is given, we might use a fallback (e.g., local crypto) for testing,
		// but for production, it should likely be mandatory.
		// For now, let's return an error if TEE is expected but not provided.
		// Consider adding a config flag like `config.TEEEnabled`.
		// if config.TEEEnabled { // Example check
		return nil, errors.New("TEE security provider cannot be nil")
		// }
	}

	// Validate config (basic checks)
	// EncryptionKey might become irrelevant if TEE handles key management internally.
	// if config.EncryptionKey == "" {
	// 	 return nil, errors.New("encryption key cannot be empty")
	// }
	if config.MaxSecretSize <= 0 {
		config.MaxSecretSize = 10 * 1024 // 10KB default
	}
	// MaxSecretsPerUser might be enforced by the store or checked here

	// Store initialization is now handled outside and passed in.

	return &ServiceImpl{
		config: config,
		store:  store,
		tee:    teeProvider,
	}, nil
}

// CreateSecret stores a new secret and its initial permissions.
func (s *ServiceImpl) CreateSecret(ctx context.Context, owner util.Uint160, name, value string, allowedFuncIDs []string) (string, error) {
	if name == "" {
		return "", errors.New("secret name cannot be empty")
	}
	if len(value) > s.config.MaxSecretSize {
		return "", fmt.Errorf("secret size exceeds maximum limit of %d bytes", s.config.MaxSecretSize)
	}

	// Encrypt using TEE provider
	encryptedBytes, err := s.tee.Encrypt(ctx, []byte(value))
	if err != nil {
		log.Errorf("TEE encryption failed for user %s, name %s: %v", owner.StringLE(), name, err)
		return "", fmt.Errorf("failed to encrypt secret via TEE: %w", err)
	}
	encryptedValue := base64.StdEncoding.EncodeToString(encryptedBytes)

	now := time.Now()
	secretID := uuid.NewString() // Generate a unique ID

	metadata := SecretMetadata{
		ID:        secretID,
		Name:      name,
		Owner:     owner,
		OwnerStr:  owner.StringLE(),
		CreatedAt: now,
		UpdatedAt: now,
		// ExpiresAt, Tags, Metadata could be added via options if needed
	}

	if err := s.store.SaveSecret(ctx, owner, metadata, encryptedValue); err != nil {
		// Check if the error is due to duplicate name
		if errors.Is(err, ErrAlreadyExists) {
			return "", fmt.Errorf("failed to save secret: %w", ErrDuplicateSecretName) // Return specific error
		}
		log.Errorf("Failed to save secret metadata/value for user %s, ID %s: %v", owner.StringLE(), secretID, err)
		return "", fmt.Errorf("failed to save secret: %w", err)
	}

	permissions := SecretPermission{
		SecretID:           secretID,
		AllowedFunctionIDs: allowedFuncIDs,
	}

	if err := s.store.SavePermissions(ctx, owner, secretID, permissions); err != nil {
		log.Errorf("Failed to save initial permissions for secret %s: %v. Attempting cleanup.", secretID, err)
		// Attempt to clean up the created secret if permissions fail
		_ = s.store.DeleteSecret(ctx, owner, secretID)
		return "", fmt.Errorf("failed to save secret permissions: %w", err)
	}

	log.Infof("Secret %s created successfully for user %s", secretID, owner.StringLE())
	return secretID, nil
}

// GetSecretMetadata retrieves non-sensitive metadata for a secret owned by the caller.
func (s *ServiceImpl) GetSecretMetadata(ctx context.Context, caller util.Uint160, secretID string) (*SecretMetadata, error) {
	meta, err := s.store.GetSecretMetadata(ctx, caller, secretID)
	if err != nil {
		if errors.Is(err, ErrNotFound) {
			// Distinguish between not found and expired?
			if strings.Contains(err.Error(), "expired") {
				return nil, fmt.Errorf("secret %s not found: %w", secretID, ErrSecretExpired)
			}
			return nil, fmt.Errorf("secret %s not found or access denied: %w", secretID, ErrSecretNotFound)
		}
		log.Errorf("Failed to get metadata for secret %s, caller %s: %v", secretID, caller.StringLE(), err)
		return nil, fmt.Errorf("failed to retrieve secret metadata: %w", err)
	}
	return meta, nil
}

// ListSecrets lists metadata for all secrets owned by the caller.
func (s *ServiceImpl) ListSecrets(ctx context.Context, caller util.Uint160) ([]SecretMetadata, error) {
	secrets, err := s.store.ListSecretsByUser(ctx, caller)
	if err != nil {
		log.Errorf("Failed to list secrets for caller %s: %v", caller.StringLE(), err)
		return nil, fmt.Errorf("failed to list secrets: %w", err)
	}
	return secrets, nil
}

// UpdateSecretValue updates the encrypted value of an existing secret owned by the caller.
func (s *ServiceImpl) UpdateSecretValue(ctx context.Context, caller util.Uint160, secretID, newValue string) error {
	if len(newValue) > s.config.MaxSecretSize {
		return fmt.Errorf("secret size exceeds maximum limit of %d bytes", s.config.MaxSecretSize)
	}

	// First, verify ownership by fetching metadata
	meta, err := s.store.GetSecretMetadata(ctx, caller, secretID)
	if err != nil {
		// Handle specific errors from GetSecretMetadata
		if errors.Is(err, ErrSecretNotFound) || errors.Is(err, ErrSecretExpired) {
			return fmt.Errorf("cannot update secret: %w", err)
		}
		return fmt.Errorf("failed to update secret: %w", err) // Other errors
	}

	// Encrypt using TEE provider
	encryptedBytes, err := s.tee.Encrypt(ctx, []byte(newValue))
	if err != nil {
		log.Errorf("TEE encryption failed for updated secret value for ID %s: %v", secretID, err)
		return fmt.Errorf("failed to encrypt updated secret via TEE: %w", err)
	}
	encryptedValue := base64.StdEncoding.EncodeToString(encryptedBytes)

	meta.UpdatedAt = time.Now()

	if err := s.store.SaveSecret(ctx, caller, *meta, encryptedValue); err != nil {
		log.Errorf("Failed to save updated secret value for ID %s: %v", secretID, err)
		return fmt.Errorf("failed to save updated secret: %w", err)
	}

	log.Infof("Secret %s value updated successfully by user %s", secretID, caller.StringLE())
	return nil
}

// UpdateSecretPermissions updates the list of allowed function IDs for a secret owned by the caller.
func (s *ServiceImpl) UpdateSecretPermissions(ctx context.Context, caller util.Uint160, secretID string, allowedFuncIDs []string) error {
	// Verify ownership first
	_, err := s.store.GetSecretMetadata(ctx, caller, secretID)
	if err != nil {
		// Handle specific errors from GetSecretMetadata
		if errors.Is(err, ErrSecretNotFound) || errors.Is(err, ErrSecretExpired) {
			return fmt.Errorf("cannot update permissions: %w", err)
		}
		return fmt.Errorf("failed to update permissions: %w", err) // Other errors
	}

	permissions := SecretPermission{
		SecretID:           secretID,
		AllowedFunctionIDs: allowedFuncIDs,
	}

	if err := s.store.SavePermissions(ctx, caller, secretID, permissions); err != nil {
		log.Errorf("Failed to save updated permissions for secret %s: %v", secretID, err)
		return fmt.Errorf("failed to save permissions: %w", err)
	}

	log.Infof("Secret %s permissions updated successfully by user %s", secretID, caller.StringLE())
	return nil
}

// DeleteSecret removes a secret owned by the caller.
func (s *ServiceImpl) DeleteSecret(ctx context.Context, caller util.Uint160, secretID string) error {
	// The store implementation handles ownership check before deleting
	err := s.store.DeleteSecret(ctx, caller, secretID)
	if err != nil {
		if errors.Is(err, ErrNotFound) {
			// Return specific not found error
			return fmt.Errorf("cannot delete secret: %w", ErrSecretNotFound)
		}
		log.Errorf("Failed to delete secret %s for caller %s: %v", secretID, caller.StringLE(), err)
		// Return generic store error?
		return fmt.Errorf("failed to delete secret: %w", ErrStoreFailed)
	}

	log.Infof("Secret %s deleted successfully by user %s", secretID, caller.StringLE())
	return nil
}

// GetSecretForFunction retrieves the decrypted secret value if the requesting function has permission.
func (s *ServiceImpl) GetSecretForFunction(ctx context.Context, requestingFunctionID, secretID string) (string, error) {
	// 1. Get Permissions for the secret
	perms, err := s.store.GetPermissions(ctx, secretID)
	if err != nil {
		if errors.Is(err, ErrNotFound) {
			log.Warnf("Permission check failed for func %s on secret %s: No permissions set", requestingFunctionID, secretID)
			return "", fmt.Errorf("permission denied: no permissions defined for secret %s", secretID)
		}
		log.Warnf("Permission check failed for func %s on secret %s: %v", requestingFunctionID, secretID, err)
		return "", fmt.Errorf("permission check failed: %w", ErrStoreFailed)
	}

	// 2. Check if the requesting function is in the allowed list
	allowed := false
	for _, allowedID := range perms.AllowedFunctionIDs {
		if allowedID == requestingFunctionID {
			allowed = true
			break
		}
	}

	if !allowed {
		log.Warnf("Permission denied for func %s on secret %s", requestingFunctionID, secretID)
		return "", fmt.Errorf("permission denied for function %s to access secret %s", requestingFunctionID, secretID)
	}

	// 3. Get the encrypted value using the new store method.
	encryptedValue, err := s.store.GetSecretValueByID(ctx, secretID)
	if err != nil {
		if errors.Is(err, ErrNotFound) {
			// Check if it was specifically an expiry error
			if strings.Contains(err.Error(), "expired") {
				log.Warnf("Access check passed for func %s on secret %s, but secret is expired", requestingFunctionID, secretID)
				return "", fmt.Errorf("secret %s has expired: %w", secretID, ErrSecretExpired)
			}
			log.Errorf("Permission check passed but failed to get secret value for ID %s (func %s): %v", secretID, requestingFunctionID, err)
			return "", fmt.Errorf("secret value not found (internal error): %w", ErrSecretNotFound)
		}
		// Other store errors (e.g., DB connection issues)
		log.Errorf("Failed to get secret value by ID %s (func %s): %v", secretID, requestingFunctionID, err)
		return "", fmt.Errorf("failed to retrieve secret value: %w", ErrStoreFailed)
	}

	// Note: Expiry should have been checked by GetSecretValueByID store method.

	// 4. Decrypt using TEE Provider
	encryptedBytes, err := base64.StdEncoding.DecodeString(encryptedValue)
	if err != nil {
		log.Errorf("Failed to decode base64 secret %s before TEE decryption (func %s): %v", secretID, requestingFunctionID, err)
		return "", fmt.Errorf("failed to decode secret value: %w", ErrDecryptionFailed)
	}

	decryptedBytes, err := s.tee.Decrypt(ctx, encryptedBytes)
	if err != nil {
		log.Errorf("TEE decryption failed for secret %s for function %s: %v", secretID, requestingFunctionID, err)
		// Distinguish TEE errors from general decryption errors if needed
		return "", fmt.Errorf("failed to decrypt secret via TEE: %w", ErrDecryptionFailed)
	}
	decryptedValue := string(decryptedBytes)

	log.Infof("Secret %s accessed successfully by function %s", secretID, requestingFunctionID)
	return decryptedValue, nil
}

// GetEncryptedSecretForFunction retrieves the encrypted secret value if the requesting function has permission.
func (s *ServiceImpl) GetEncryptedSecretForFunction(ctx context.Context, requestingFunctionID, secretID string) (string, error) {
	// 1. Get Permissions for the secret
	perms, err := s.store.GetPermissions(ctx, secretID)
	if err != nil {
		if errors.Is(err, ErrNotFound) {
			log.Warnf("Permission check failed for func %s on secret %s: No permissions set", requestingFunctionID, secretID)
			return "", fmt.Errorf("permission denied: no permissions defined for secret %s", secretID)
		}
		log.Warnf("Permission check failed for func %s on secret %s: %v", requestingFunctionID, secretID, err)
		return "", fmt.Errorf("permission check failed: %w", ErrStoreFailed)
	}

	// 2. Check if the requesting function is in the allowed list
	allowed := false
	for _, allowedID := range perms.AllowedFunctionIDs {
		if allowedID == requestingFunctionID {
			allowed = true
			break
		}
	}

	if !allowed {
		log.Warnf("Permission denied for func %s on secret %s", requestingFunctionID, secretID)
		return "", fmt.Errorf("permission denied for function %s to access secret %s", requestingFunctionID, secretID)
	}

	// 3. Get the encrypted value using the store method (checks expiry implicitly).
	encryptedValue, err := s.store.GetSecretValueByID(ctx, secretID)
	if err != nil {
		if errors.Is(err, ErrNotFound) {
			// Check if it was specifically an expiry error
			if strings.Contains(err.Error(), "expired") {
				log.Warnf("Access check passed for func %s on secret %s, but secret is expired", requestingFunctionID, secretID)
				return "", fmt.Errorf("secret %s has expired: %w", secretID, ErrSecretExpired)
			}
			log.Errorf("Permission check passed but failed to get encrypted secret value for ID %s (func %s): %v", secretID, requestingFunctionID, err)
			return "", fmt.Errorf("secret value not found (internal error): %w", ErrSecretNotFound)
		}
		// Other store errors (e.g., DB connection issues)
		log.Errorf("Failed to get encrypted secret value by ID %s (func %s): %v", secretID, requestingFunctionID, err)
		return "", fmt.Errorf("failed to retrieve secret value: %w", ErrStoreFailed)
	}

	// 4. Return the base64 encoded encrypted value as stored.
	log.Infof("Encrypted secret %s retrieved successfully for function %s", secretID, requestingFunctionID)
	return encryptedValue, nil
}

// --- Encryption/Decryption (Placeholder - should move to TEE module) ---

// encrypt encrypts a value using AES-GCM -- REMOVED
/*
func (s *ServiceImpl) encrypt(value string) (string, error) {
	key := sha256.Sum256([]byte(s.config.EncryptionKey))
	block, err := aes.NewCipher(key[:])
	if err != nil {
		return "", err
	}
	aesGCM, err := cipher.NewGCM(block)
	if err != nil {
		return "", err
	}
	nonce := make([]byte, aesGCM.NonceSize())
	if _, err := io.ReadFull(rand.Reader, nonce); err != nil {
		return "", err
	}
	ciphertext := aesGCM.Seal(nonce, nonce, []byte(value), nil)
	return base64.StdEncoding.EncodeToString(ciphertext), nil
}
*/

// decrypt decrypts a value using AES-GCM -- REMOVED
/*
func (s *ServiceImpl) decrypt(encryptedValue string) (string, error) {
	key := sha256.Sum256([]byte(s.config.EncryptionKey))
	block, err := aes.NewCipher(key[:])
	if err != nil {
		return "", err
	}
	aesGCM, err := cipher.NewGCM(block)
	if err != nil {
		return "", err
	}
	ciphertext, err := base64.StdEncoding.DecodeString(encryptedValue)
	if err != nil {
		return "", err
	}
	if len(ciphertext) < aesGCM.NonceSize() {
		return "", errors.New("invalid ciphertext: too short")
	}
	nonce, actualCiphertext := ciphertext[:aesGCM.NonceSize()], ciphertext[aesGCM.NonceSize():]
	plaintext, err := aesGCM.Open(nil, nonce, actualCiphertext, nil)
	if err != nil {
		return "", fmt.Errorf("decryption failed: %w", err)
	}
	return string(plaintext), nil
}
*/
