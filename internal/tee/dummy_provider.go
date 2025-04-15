package tee

import (
	"context"
	"crypto/aes"
	"crypto/cipher"
	"crypto/rand"
	"crypto/sha256"
	"errors"
	"fmt"
	"io"

	secrets_iface "github.com/r3e-network/neo_service_layer/internal/secretservice" // Import the interface definition
	log "github.com/sirupsen/logrus"
)

// Ensure DummyTEEProvider implements the interface
var _ secrets_iface.TEESecurityProvider = (*DummyTEEProvider)(nil)

// DummyTEEProvider provides a basic, insecure implementation of TEESecurityProvider
// using local AES-GCM encryption. **DO NOT USE IN PRODUCTION.**
type DummyTEEProvider struct {
	encryptionKey []byte // Should be 32 bytes for AES-256
}

// NewDummyTEEProvider creates a new dummy provider.
// The key provided should be securely managed.
func NewDummyTEEProvider(key string) (*DummyTEEProvider, error) {
	if key == "" {
		return nil, errors.New("dummy TEE provider requires a non-empty key")
	}
	// Use SHA-256 of the provided key string to ensure a 32-byte key for AES-256
	hashedKey := sha256.Sum256([]byte(key))
	log.Warn("Initialized DummyTEEProvider - THIS IS NOT SECURE FOR PRODUCTION.")
	return &DummyTEEProvider{
		encryptionKey: hashedKey[:], // Use the 32-byte hash
	}, nil
}

// Encrypt encrypts plaintext using AES-GCM.
func (p *DummyTEEProvider) Encrypt(ctx context.Context, plaintext []byte) (ciphertext []byte, err error) {
	block, err := aes.NewCipher(p.encryptionKey)
	if err != nil {
		return nil, fmt.Errorf("dummy tee encrypt: failed to create cipher: %w", err)
	}

	aesGCM, err := cipher.NewGCM(block)
	if err != nil {
		return nil, fmt.Errorf("dummy tee encrypt: failed to create GCM: %w", err)
	}

	nonce := make([]byte, aesGCM.NonceSize())
	if _, err := io.ReadFull(rand.Reader, nonce); err != nil {
		return nil, fmt.Errorf("dummy tee encrypt: failed to generate nonce: %w", err)
	}

	ciphertext = aesGCM.Seal(nonce, nonce, plaintext, nil)
	return ciphertext, nil
}

// Decrypt decrypts ciphertext using AES-GCM.
func (p *DummyTEEProvider) Decrypt(ctx context.Context, ciphertext []byte) (plaintext []byte, err error) {
	block, err := aes.NewCipher(p.encryptionKey)
	if err != nil {
		return nil, fmt.Errorf("dummy tee decrypt: failed to create cipher: %w", err)
	}

	aesGCM, err := cipher.NewGCM(block)
	if err != nil {
		return nil, fmt.Errorf("dummy tee decrypt: failed to create GCM: %w", err)
	}

	nonceSize := aesGCM.NonceSize()
	if len(ciphertext) < nonceSize {
		return nil, errors.New("dummy tee decrypt: invalid ciphertext (too short)")
	}

	nonce, actualCiphertext := ciphertext[:nonceSize], ciphertext[nonceSize:]
	plaintext, err = aesGCM.Open(nil, nonce, actualCiphertext, nil)
	if err != nil {
		return nil, fmt.Errorf("dummy tee decrypt: failed to open GCM: %w", err)
	}

	return plaintext, nil
}
