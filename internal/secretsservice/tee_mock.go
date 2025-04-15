package secrets

import (
	"context"
	"fmt"

	log "github.com/sirupsen/logrus"
)

// MockTEESecurityProvider is a placeholder implementation for TEESecurityProvider.
// It does not perform real encryption/decryption.
type MockTEESecurityProvider struct{}

// NewMockTEESecurityProvider creates a new mock provider.
func NewMockTEESecurityProvider() *MockTEESecurityProvider {
	log.Warn("Using MockTEESecurityProvider - NO REAL ENCRYPTION/DECRYPTION WILL OCCUR!")
	return &MockTEESecurityProvider{}
}

// Encrypt simulates encryption by simply returning the input bytes.
func (m *MockTEESecurityProvider) Encrypt(ctx context.Context, plaintext []byte) (ciphertext []byte, err error) {
	log.Debugf("MockTEESecurityProvider: Simulating Encrypt for %d bytes", len(plaintext))
	// In a real implementation, interact with TEE here.
	// For mock, just return the plaintext (or add a prefix/suffix).
	// Using a prefix to make it obvious it was "encrypted".
	result := append([]byte("mock_encrypted:"), plaintext...)
	return result, nil
}

// Decrypt simulates decryption by removing the mock prefix.
func (m *MockTEESecurityProvider) Decrypt(ctx context.Context, ciphertext []byte) (plaintext []byte, err error) {
	log.Debugf("MockTEESecurityProvider: Simulating Decrypt for %d bytes", len(ciphertext))
	// In a real implementation, interact with TEE here.
	prefix := []byte("mock_encrypted:")
	if len(ciphertext) < len(prefix) || string(ciphertext[:len(prefix)]) != string(prefix) {
		log.Errorf("MockTEESecurityProvider: Ciphertext does not have expected mock prefix.")
		return nil, fmt.Errorf("mock decryption failed: invalid mock format")
	}
	// Return the original data after the prefix
	result := ciphertext[len(prefix):]
	return result, nil
}
