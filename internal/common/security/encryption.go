package security

import (
	"crypto/aes"
	"crypto/cipher"
	"crypto/rand"
	"crypto/sha256"
	"encoding/base64"
	"errors"
	"io"
)

// EncryptionService provides methods for encrypting and decrypting data
type EncryptionService struct {
	key []byte
}

// NewEncryptionService creates a new encryption service with the provided key
func NewEncryptionService(key string) (*EncryptionService, error) {
	if len(key) == 0 {
		return nil, errors.New("encryption key cannot be empty")
	}

	// Hash the key to ensure it's the right size for AES-256
	hashedKey := sha256.Sum256([]byte(key))
	return &EncryptionService{key: hashedKey[:]}, nil
}

// Encrypt encrypts the given plaintext using AES-GCM
func (s *EncryptionService) Encrypt(plaintext []byte) (string, error) {
	// Create the AES cipher
	block, err := aes.NewCipher(s.key)
	if err != nil {
		return "", err
	}

	// Create a GCM cipher
	aesGCM, err := cipher.NewGCM(block)
	if err != nil {
		return "", err
	}

	// Create a nonce
	nonce := make([]byte, aesGCM.NonceSize())
	if _, err := io.ReadFull(rand.Reader, nonce); err != nil {
		return "", err
	}

	// Encrypt the data
	ciphertext := aesGCM.Seal(nonce, nonce, plaintext, nil)

	// Base64 encode the result
	return base64.StdEncoding.EncodeToString(ciphertext), nil
}

// Decrypt decrypts the given ciphertext using AES-GCM
func (s *EncryptionService) Decrypt(encryptedText string) ([]byte, error) {
	// Base64 decode the ciphertext
	ciphertext, err := base64.StdEncoding.DecodeString(encryptedText)
	if err != nil {
		return nil, err
	}

	// Create the AES cipher
	block, err := aes.NewCipher(s.key)
	if err != nil {
		return nil, err
	}

	// Create a GCM cipher
	aesGCM, err := cipher.NewGCM(block)
	if err != nil {
		return nil, err
	}

	// Get the nonce size
	nonceSize := aesGCM.NonceSize()
	if len(ciphertext) < nonceSize {
		return nil, errors.New("ciphertext too short")
	}

	// Extract the nonce and ciphertext
	nonce, ciphertext := ciphertext[:nonceSize], ciphertext[nonceSize:]

	// Decrypt the data
	plaintext, err := aesGCM.Open(nil, nonce, ciphertext, nil)
	if err != nil {
		return nil, err
	}

	return plaintext, nil
}

// EncryptString encrypts a string
func (s *EncryptionService) EncryptString(plaintext string) (string, error) {
	return s.Encrypt([]byte(plaintext))
}

// DecryptString decrypts a string
func (s *EncryptionService) DecryptString(encryptedText string) (string, error) {
	plaintext, err := s.Decrypt(encryptedText)
	if err != nil {
		return "", err
	}
	return string(plaintext), nil
}

// GenerateRandomKey generates a random encryption key
func GenerateRandomKey(length int) (string, error) {
	bytes := make([]byte, length)
	if _, err := rand.Read(bytes); err != nil {
		return "", err
	}
	return base64.StdEncoding.EncodeToString(bytes), nil
}

// HashPassword hashes a password (simplified implementation)
func HashPassword(password string) (string, error) {
	// In a real application, we would use bcrypt or argon2
	// This is a simplified implementation for illustration purposes only
	hash := sha256.Sum256([]byte(password))
	return base64.StdEncoding.EncodeToString(hash[:]), nil
}

// VerifyPassword verifies a hashed password (simplified implementation)
func VerifyPassword(password, hashedPassword string) (bool, error) {
	// In a real application, we would use bcrypt or argon2
	// This is a simplified implementation for illustration purposes only
	hash, err := HashPassword(password)
	if err != nil {
		return false, err
	}
	return hash == hashedPassword, nil
}