package secrets

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"time"

	"github.com/dgraph-io/badger/v3" // Assuming this dependency exists
	"github.com/nspcc-dev/neo-go/pkg/util"
	log "github.com/sirupsen/logrus"
)

// Ensure BadgerStore implements Store
var _ Store = (*BadgerStore)(nil)

// BadgerStore implements the Store interface using BadgerDB.
type BadgerStore struct {
	db *badger.DB
}

// Key Prefixes
const (
	prefixSecretMeta    = "sm_"
	prefixSecretValue   = "sv_"
	prefixSecretPerm    = "sp_"
	prefixUserSecrets   = "us_" // For listing secrets by user
	prefixSecretNameIdx = "ni_" // For name -> ID lookup
)

// NewBadgerStore creates or opens a BadgerDB database at the given path.
func NewBadgerStore(path string) (*BadgerStore, error) {
	opts := badger.DefaultOptions(path)
	// Customize options if needed (e.g., logging, encryption)
	opts.Logger = nil // Disable Badger's default logger, use zap instead if needed

	db, err := badger.Open(opts)
	if err != nil {
		return nil, fmt.Errorf("failed to open badger database at %s: %w", path, err)
	}
	log.Infof("BadgerDB store opened successfully at %s", path)
	return &BadgerStore{db: db}, nil
}

// --- Key Generation Helpers ---

func metaKey(userID, secretID string) []byte {
	return []byte(prefixSecretMeta + userID + "_" + secretID)
}

func valueKey(userID, secretID string) []byte {
	return []byte(prefixSecretValue + userID + "_" + secretID)
}

func permKey(secretID string) []byte {
	return []byte(prefixSecretPerm + secretID)
}

func userSecretsPrefix(userID string) []byte {
	return []byte(prefixUserSecrets + userID + "_")
}

func userSecretKey(userID, secretID string) []byte {
	return []byte(prefixUserSecrets + userID + "_" + secretID)
}

func nameIndexKey(userID, secretName string) []byte {
	return []byte(prefixSecretNameIdx + userID + "_" + secretName)
}

// Key for global lookup of metadata by secret ID
func globalMetaKey(secretID string) []byte {
	return []byte(prefixSecretMeta + secretID) // Reuse prefix, but without userID
}

// Key for global lookup of value by secret ID
func globalValueKey(secretID string) []byte {
	return []byte(prefixSecretValue + secretID) // Reuse prefix, but without userID
}

// --- Store Interface Implementation ---

func (bs *BadgerStore) SaveSecret(ctx context.Context, userAddress util.Uint160, metadata SecretMetadata, encryptedValue string) error {
	userID := userAddress.StringLE()
	mKey := metaKey(userID, metadata.ID)
	vKey := valueKey(userID, metadata.ID)
	usKey := userSecretKey(userID, metadata.ID)  // Key for user listing
	niKey := nameIndexKey(userID, metadata.Name) // Key for name index
	gmKey := globalMetaKey(metadata.ID)          // Key for global metadata lookup
	gvKey := globalValueKey(metadata.ID)         // Key for global value lookup

	metaBytes, err := json.Marshal(metadata)
	if err != nil {
		return fmt.Errorf("failed to marshal secret metadata: %w", err)
	}

	return bs.db.Update(func(txn *badger.Txn) error {
		// Check if name index already exists (prevent duplicate names per user)
		// Note: This check has a race condition if not done carefully or if transactions are long.
		// A transaction might be needed around name checking and setting.
		// For simplicity now, we check first, then set.
		if _, err := txn.Get(niKey); err == nil {
			// Item exists, name collision
			return fmt.Errorf("%w: secret name '%s' already exists for user %s", ErrAlreadyExists, metadata.Name, userID)
		} else if !errors.Is(err, badger.ErrKeyNotFound) {
			// Unexpected error checking name index
			return fmt.Errorf("failed to check name index: %w", err)
		}

		// Set user-specific metadata
		if err := txn.Set(mKey, metaBytes); err != nil {
			return err
		}
		// Set user-specific value
		if err := txn.Set(vKey, []byte(encryptedValue)); err != nil {
			return err
		}
		// Set user secret link
		if err := txn.Set(usKey, []byte{}); err != nil {
			return err
		}
		// Set name index
		if err := txn.Set(niKey, []byte(metadata.ID)); err != nil {
			return err
		}

		// Also set global metadata (for direct lookup in GetSecretValueByID)
		if err := txn.Set(gmKey, metaBytes); err != nil {
			return err
		}
		// Also set global value (for direct lookup in GetSecretValueByID)
		if err := txn.Set(gvKey, []byte(encryptedValue)); err != nil {
			return err
		}

		return nil
	})
}

func (bs *BadgerStore) GetSecretMetadata(ctx context.Context, userAddress util.Uint160, secretID string) (*SecretMetadata, error) {
	userID := userAddress.StringLE()
	mKey := metaKey(userID, secretID)
	var metadata SecretMetadata

	err := bs.db.View(func(txn *badger.Txn) error {
		item, err := txn.Get(mKey)
		if err != nil {
			if errors.Is(err, badger.ErrKeyNotFound) {
				return fmt.Errorf("%w: secret %s metadata not found for user %s", ErrNotFound, secretID, userID)
			}
			return err // Other potential errors
		}

		return item.Value(func(val []byte) error {
			return json.Unmarshal(val, &metadata)
		})
	})

	if err != nil {
		return nil, err
	}

	// Check expiry
	if !metadata.ExpiresAt.IsZero() && metadata.ExpiresAt.Before(time.Now()) {
		// TODO: Implement lazy deletion or background cleanup for expired secrets
		return nil, fmt.Errorf("%w: secret %s has expired", ErrNotFound, secretID)
	}

	return &metadata, nil
}

// GetSecretValue retrieves the encrypted value. Note: This version still requires userID.
// Consider adding GetSecretValueByID if needed by GetSecretForFunction.
func (bs *BadgerStore) GetSecretValue(ctx context.Context, userAddress util.Uint160, secretID string) (string, error) {
	userID := userAddress.StringLE()
	vKey := valueKey(userID, secretID)
	var encryptedValue string

	err := bs.db.View(func(txn *badger.Txn) error {
		// Also check metadata existence and expiry before returning value
		meta, err := bs.getSecretMetadataInTxn(txn, userID, secretID)
		if err != nil {
			return err // Handles not found and expiry check
		}
		_ = meta // Use meta if needed for other checks

		item, err := txn.Get(vKey)
		if err != nil {
			if errors.Is(err, badger.ErrKeyNotFound) {
				return fmt.Errorf("%w: secret %s value not found for user %s", ErrNotFound, secretID, userID)
			}
			return err
		}

		return item.Value(func(val []byte) error {
			encryptedValue = string(val)
			return nil
		})
	})

	return encryptedValue, err
}

// GetSecretValueByID retrieves the encrypted value for a specific secret ID, regardless of user.
// It checks for expiry before returning.
func (bs *BadgerStore) GetSecretValueByID(ctx context.Context, secretID string) (string, error) {
	var encryptedValue string

	// Use the global keys directly
	lookupMetaKey := globalMetaKey(secretID)
	lookupValKey := globalValueKey(secretID)

	err := bs.db.View(func(txn *badger.Txn) error {
		// 1. Get global metadata to check expiry
		item, err := txn.Get(lookupMetaKey)
		if err != nil {
			if errors.Is(err, badger.ErrKeyNotFound) {
				return fmt.Errorf("%w: secret %s metadata not found", ErrNotFound, secretID)
			}
			return err // Other badger error
		}

		var metadata SecretMetadata
		err = item.Value(func(val []byte) error {
			return json.Unmarshal(val, &metadata)
		})
		if err != nil {
			return fmt.Errorf("failed to unmarshal metadata for expiry check: %w", err)
		}

		// 2. Check expiry
		if !metadata.ExpiresAt.IsZero() && metadata.ExpiresAt.Before(time.Now()) {
			return fmt.Errorf("%w: secret %s has expired", ErrNotFound, secretID)
		}

		// 3. Get the global value
		item, err = txn.Get(lookupValKey)
		if err != nil {
			if errors.Is(err, badger.ErrKeyNotFound) {
				// This indicates potential data inconsistency if metadata was found
				log.Warnf("Metadata found for secret %s, but global value key %s not found", secretID, string(lookupValKey))
				return fmt.Errorf("%w: secret %s value not found (internal data inconsistency)", ErrNotFound, secretID)
			}
			return err // Other badger error
		}

		return item.Value(func(val []byte) error {
			encryptedValue = string(val)
			return nil
		})
	})

	return encryptedValue, err

	// NOTE: This implementation now correctly uses the global keys populated by the modified SaveSecret.
}

// ListSecretsByUser retrieves metadata for all secrets owned by a specific user.
func (bs *BadgerStore) ListSecretsByUser(ctx context.Context, userAddress util.Uint160) ([]SecretMetadata, error) {
	userID := userAddress.StringLE()
	prefix := userSecretsPrefix(userID)
	secrets := make([]SecretMetadata, 0)

	err := bs.db.View(func(txn *badger.Txn) error {
		opts := badger.DefaultIteratorOptions
		opts.Prefix = prefix
		it := txn.NewIterator(opts)
		defer it.Close()

		for it.Rewind(); it.ValidForPrefix(prefix); it.Next() {
			item := it.Item()
			key := item.Key()
			secretID := string(bytes.TrimPrefix(key, prefix))

			// Fetch the full metadata using the secretID and userID
			meta, err := bs.getSecretMetadataInTxn(txn, userID, secretID)
			if err != nil {
				if errors.Is(err, ErrNotFound) {
					// Secret might have been deleted between iterations, log and skip
					log.Warnf("Metadata for secret %s (user %s) listed but not found during iteration, skipping.", secretID, userID)
					continue
				}
				return fmt.Errorf("error fetching metadata for secret %s: %w", secretID, err)
			}

			// Skip expired secrets (already checked in getSecretMetadataInTxn)
			secrets = append(secrets, *meta)
		}
		return nil
	})

	if err != nil {
		return nil, fmt.Errorf("failed to iterate user secrets: %w", err)
	}

	return secrets, nil
}

func (bs *BadgerStore) DeleteSecret(ctx context.Context, userAddress util.Uint160, secretID string) error {
	userID := userAddress.StringLE()
	mKey := metaKey(userID, secretID)
	vKey := valueKey(userID, secretID)
	pKey := permKey(secretID)
	usKey := userSecretKey(userID, secretID)
	gmKey := globalMetaKey(secretID)  // Key for global metadata
	gvKey := globalValueKey(secretID) // Key for global value

	return bs.db.Update(func(txn *badger.Txn) error {
		// Get metadata to verify ownership and find name for index deletion
		meta, err := bs.getSecretMetadataInTxn(txn, userID, secretID)
		if err != nil {
			// Allow ErrNotFound to propagate, but treat it as success for deletion?
			// Or strictly require existence? Let's require existence.
			return fmt.Errorf("cannot delete secret: %w", err)
		}

		niKey := nameIndexKey(userID, meta.Name)

		if err := txn.Delete(mKey); err != nil && !errors.Is(err, badger.ErrKeyNotFound) {
			return fmt.Errorf("failed to delete metadata: %w", err)
		}
		if err := txn.Delete(vKey); err != nil && !errors.Is(err, badger.ErrKeyNotFound) {
			return fmt.Errorf("failed to delete value: %w", err)
		}
		if err := txn.Delete(pKey); err != nil && !errors.Is(err, badger.ErrKeyNotFound) {
			return fmt.Errorf("failed to delete permissions: %w", err)
		}
		if err := txn.Delete(usKey); err != nil && !errors.Is(err, badger.ErrKeyNotFound) {
			return fmt.Errorf("failed to delete user secret link: %w", err)
		}
		if err := txn.Delete(niKey); err != nil && !errors.Is(err, badger.ErrKeyNotFound) {
			return fmt.Errorf("failed to delete name index: %w", err)
		}
		// Also delete global keys
		if err := txn.Delete(gmKey); err != nil && !errors.Is(err, badger.ErrKeyNotFound) {
			log.Warnf("Failed to delete global metadata key %s: %v", string(gmKey), err) // Log warning, but don't fail the whole delete
		}
		if err := txn.Delete(gvKey); err != nil && !errors.Is(err, badger.ErrKeyNotFound) {
			log.Warnf("Failed to delete global value key %s: %v", string(gvKey), err) // Log warning, but don't fail the whole delete
		}
		return nil
	})
}

func (bs *BadgerStore) SavePermissions(ctx context.Context, userAddress util.Uint160, secretID string, permissions SecretPermission) error {
	// Verify ownership before saving permissions
	_, err := bs.GetSecretMetadata(ctx, userAddress, secretID) // Use the public method which includes expiry check
	if err != nil {
		return fmt.Errorf("cannot save permissions: %w", err)
	}

	pKey := permKey(secretID)
	permBytes, err := json.Marshal(permissions)
	if err != nil {
		return fmt.Errorf("failed to marshal secret permissions: %w", err)
	}

	return bs.db.Update(func(txn *badger.Txn) error {
		return txn.Set(pKey, permBytes)
	})
}

func (bs *BadgerStore) GetPermissions(ctx context.Context, secretID string) (*SecretPermission, error) {
	pKey := permKey(secretID)
	var permissions SecretPermission

	err := bs.db.View(func(txn *badger.Txn) error {
		item, err := txn.Get(pKey)
		if err != nil {
			if errors.Is(err, badger.ErrKeyNotFound) {
				// If no permissions are explicitly set, return default (empty/no access)?
				// For now, let's return ErrNotFound, caller must handle.
				return fmt.Errorf("%w: permissions not found for secret %s", ErrNotFound, secretID)
			}
			return err
		}

		return item.Value(func(val []byte) error {
			return json.Unmarshal(val, &permissions)
		})
	})

	if err != nil {
		return nil, err
	}

	return &permissions, nil
}

func (bs *BadgerStore) DeletePermissions(ctx context.Context, secretID string) error {
	pKey := permKey(secretID)
	return bs.db.Update(func(txn *badger.Txn) error {
		if err := txn.Delete(pKey); err != nil && !errors.Is(err, badger.ErrKeyNotFound) {
			return fmt.Errorf("failed to delete permissions: %w", err)
		}
		return nil
	})
}

// Close closes the BadgerDB database connection.
func (bs *BadgerStore) Close() error {
	if bs.db != nil {
		log.Info("Closing BadgerDB store...")
		return bs.db.Close()
	}
	return nil
}

// --- Internal Helper ---

// getSecretMetadataInTxn fetches metadata within an existing transaction.
func (bs *BadgerStore) getSecretMetadataInTxn(txn *badger.Txn, userID, secretID string) (*SecretMetadata, error) {
	mKey := metaKey(userID, secretID)
	var metadata SecretMetadata

	item, err := txn.Get(mKey)
	if err != nil {
		if errors.Is(err, badger.ErrKeyNotFound) {
			return nil, fmt.Errorf("%w: secret %s metadata not found for user %s", ErrNotFound, secretID, userID)
		}
		return nil, err
	}

	err = item.Value(func(val []byte) error {
		return json.Unmarshal(val, &metadata)
	})
	if err != nil {
		return nil, fmt.Errorf("failed to unmarshal metadata for secret %s: %w", secretID, err)
	}

	// Check expiry
	if !metadata.ExpiresAt.IsZero() && metadata.ExpiresAt.Before(time.Now()) {
		return nil, fmt.Errorf("%w: secret %s has expired", ErrNotFound, secretID)
	}

	return &metadata, nil
}

// FindSecretIDByName finds a secret ID by its name for a specific user.
// This adds the missing name->ID lookup capability.
func (bs *BadgerStore) FindSecretIDByName(ctx context.Context, userAddress util.Uint160, secretName string) (string, error) {
	userID := userAddress.StringLE()
	niKey := nameIndexKey(userID, secretName)
	var secretID string

	err := bs.db.View(func(txn *badger.Txn) error {
		item, err := txn.Get(niKey)
		if err != nil {
			if errors.Is(err, badger.ErrKeyNotFound) {
				return fmt.Errorf("%w: secret name '%s' not found for user %s", ErrNotFound, secretName, userID)
			}
			return err
		}
		return item.Value(func(val []byte) error {
			secretID = string(val)
			return nil
		})
	})

	if err != nil {
		return "", err
	}

	// Optional: Verify the secret ID found actually exists by fetching metadata?
	// _, metaErr := bs.GetSecretMetadata(ctx, userAddress, secretID)
	// if metaErr != nil {
	//    log.Warnf("Name index points to non-existent/expired secret ID %s for name %s", secretID, secretName)
	//    // Clean up dangling index?
	//    return "", fmt.Errorf("%w: secret name '%s' points to invalid secret ID %s", ErrNotFound, secretName, secretID)
	// }

	return secretID, nil
}
