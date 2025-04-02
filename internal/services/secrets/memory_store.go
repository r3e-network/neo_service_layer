package secrets

import (
	"context"
	"fmt"
	sync "sync"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// ensure memoryStore implements Store
var _ Store = (*memoryStore)(nil)

// memoryStore is an in-memory implementation of the Store interface.
type memoryStore struct {
	secrets     map[string]map[string]string         // userAddr -> secretID -> encryptedValue
	metadata    map[string]map[string]SecretMetadata // userAddr -> secretID -> metadata
	permissions map[string]SecretPermission          // secretID -> permission
	mu          sync.RWMutex
}

// NewMemoryStore creates a new in-memory store.
func NewMemoryStore() *memoryStore {
	return &memoryStore{
		secrets:     make(map[string]map[string]string),
		metadata:    make(map[string]map[string]SecretMetadata),
		permissions: make(map[string]SecretPermission),
	}
}

func (m *memoryStore) SaveSecret(ctx context.Context, userAddress util.Uint160, metadata SecretMetadata, encryptedValue string) error {
	m.mu.Lock()
	defer m.mu.Unlock()

	userKey := userAddress.StringLE()
	if _, ok := m.secrets[userKey]; !ok {
		m.secrets[userKey] = make(map[string]string)
		m.metadata[userKey] = make(map[string]SecretMetadata)
	}

	m.secrets[userKey][metadata.ID] = encryptedValue
	m.metadata[userKey][metadata.ID] = metadata

	return nil
}

func (m *memoryStore) GetSecretMetadata(ctx context.Context, userAddress util.Uint160, secretID string) (*SecretMetadata, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	userKey := userAddress.StringLE()
	userMeta, ok := m.metadata[userKey]
	if !ok {
		return nil, fmt.Errorf("%w: no secrets found for user %s", ErrNotFound, userKey)
	}

	meta, ok := userMeta[secretID]
	if !ok {
		return nil, fmt.Errorf("%w: secret %s not found for user %s", ErrNotFound, secretID, userKey)
	}

	// Check expiry
	if !meta.ExpiresAt.IsZero() && meta.ExpiresAt.Before(time.Now()) {
		// Although we return not found, we should clean up lazily or via a background job
		// For simplicity here, just return not found
		return nil, fmt.Errorf("%w: secret %s has expired", ErrNotFound, secretID)
	}

	return &meta, nil
}

func (m *memoryStore) GetSecretValue(ctx context.Context, userAddress util.Uint160, secretID string) (string, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	userKey := userAddress.StringLE()
	userSecrets, ok := m.secrets[userKey]
	if !ok {
		return "", fmt.Errorf("%w: no secrets found for user %s", ErrNotFound, userKey)
	}

	encryptedValue, ok := userSecrets[secretID]
	if !ok {
		return "", fmt.Errorf("%w: secret %s not found for user %s", ErrNotFound, secretID, userKey)
	}

	// We also need to check expiry based on metadata store
	userMeta, ok := m.metadata[userKey]
	if !ok {
		// Should ideally not happen if value exists, indicates inconsistency
		return "", fmt.Errorf("internal error: metadata missing for secret %s", secretID)
	}
	meta, ok := userMeta[secretID]
	if !ok {
		// Should ideally not happen if value exists, indicates inconsistency
		return "", fmt.Errorf("internal error: metadata missing for secret %s", secretID)
	}
	if !meta.ExpiresAt.IsZero() && meta.ExpiresAt.Before(time.Now()) {
		return "", fmt.Errorf("%w: secret %s has expired", ErrNotFound, secretID)
	}

	return encryptedValue, nil
}

// GetSecretValueByID retrieves the encrypted value for a specific secret ID, regardless of user.
// It checks for expiry before returning.
func (m *memoryStore) GetSecretValueByID(ctx context.Context, secretID string) (string, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	now := time.Now()
	var encryptedValue string
	var foundMeta *SecretMetadata
	var foundValue bool

	// Iterate through all users to find the secret ID
userLoop:
	for userKey, userSecrets := range m.secrets {
		if val, ok := userSecrets[secretID]; ok {
			// Found the value, now check metadata for expiry
			if userMetaMap, metaUserExists := m.metadata[userKey]; metaUserExists {
				if meta, metaExists := userMetaMap[secretID]; metaExists {
					if !meta.ExpiresAt.IsZero() && meta.ExpiresAt.Before(now) {
						// Found but expired, continue searching in case it exists non-expired under another user (unlikely scenario but possible)
						continue userLoop
					}
					// Found valid, non-expired secret
					encryptedValue = val
					foundMeta = &meta
					foundValue = true
					break userLoop
				} else {
					// Data inconsistency: value exists but metadata doesn't
					// Log or handle this appropriately. For now, treat as not found.
					continue userLoop
				}
			} else {
				// Data inconsistency: value exists but user metadata map doesn't
				continue userLoop
			}
		}
	}

	if !foundValue {
		return "", fmt.Errorf("%w: secret %s not found or has expired", ErrNotFound, secretID)
	}

	// Double check expiry just in case (should be covered above)
	if foundMeta != nil && !foundMeta.ExpiresAt.IsZero() && foundMeta.ExpiresAt.Before(now) {
		return "", fmt.Errorf("%w: secret %s has expired", ErrNotFound, secretID)
	}

	return encryptedValue, nil
}

func (m *memoryStore) ListSecretsByUser(ctx context.Context, userAddress util.Uint160) ([]SecretMetadata, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	userKey := userAddress.StringLE()
	userMetaMap, ok := m.metadata[userKey]
	if !ok {
		return []SecretMetadata{}, nil
	}

	secrets := make([]SecretMetadata, 0, len(userMetaMap))
	now := time.Now()
	for _, meta := range userMetaMap {
		// Skip expired secrets
		if !meta.ExpiresAt.IsZero() && meta.ExpiresAt.Before(now) {
			continue
		}
		secrets = append(secrets, meta)
	}

	return secrets, nil
}

func (m *memoryStore) DeleteSecret(ctx context.Context, userAddress util.Uint160, secretID string) error {
	m.mu.Lock()
	defer m.mu.Unlock()

	userKey := userAddress.StringLE()
	userSecrets, ok := m.secrets[userKey]
	if !ok {
		return fmt.Errorf("%w: no secrets found for user %s", ErrNotFound, userKey)
	}

	if _, ok := userSecrets[secretID]; !ok {
		return fmt.Errorf("%w: secret %s not found for user %s", ErrNotFound, secretID, userKey)
	}

	// Delete from all maps
	delete(m.secrets[userKey], secretID)
	delete(m.metadata[userKey], secretID)
	delete(m.permissions, secretID) // Also delete associated permissions

	// If user map becomes empty, remove user entry
	if len(m.secrets[userKey]) == 0 {
		delete(m.secrets, userKey)
		delete(m.metadata, userKey)
	}

	return nil
}

func (m *memoryStore) SavePermissions(ctx context.Context, userAddress util.Uint160, secretID string, permissions SecretPermission) error {
	m.mu.Lock()
	defer m.mu.Unlock()

	// Verify ownership before saving permissions
	userKey := userAddress.StringLE()
	userMeta, ok := m.metadata[userKey]
	if !ok {
		return fmt.Errorf("%w: user %s not found", ErrNotFound, userKey)
	}
	if _, ok := userMeta[secretID]; !ok {
		return fmt.Errorf("%w: secret %s not found for user %s", ErrNotFound, secretID, userKey)
	}

	m.permissions[secretID] = permissions
	return nil
}

func (m *memoryStore) GetPermissions(ctx context.Context, secretID string) (*SecretPermission, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()

	perms, ok := m.permissions[secretID]
	if !ok {
		// Return empty permissions if none explicitly set?
		// Or return ErrNotFound? Let's return ErrNotFound for clarity.
		return nil, fmt.Errorf("%w: permissions not found for secret %s", ErrNotFound, secretID)
	}
	return &perms, nil
}

func (m *memoryStore) DeletePermissions(ctx context.Context, secretID string) error {
	m.mu.Lock()
	defer m.mu.Unlock()

	delete(m.permissions, secretID)
	return nil
}

func (m *memoryStore) Close() error {
	// No-op for memory store
	return nil
}
