package security

import (
	"crypto"
	"crypto/ecdsa"
	"crypto/elliptic"
	"crypto/rand"
	"crypto/sha256"
	"crypto/x509"
	"encoding/base64"
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"math/big"
	"strings"
	"time"
)

// SignatureAlgorithm represents a signature algorithm
type SignatureAlgorithm string

// Signature algorithms
const (
	ECDSA_P256_SHA256 SignatureAlgorithm = "ECDSA_P256_SHA256"
	ECDSA_P384_SHA384 SignatureAlgorithm = "ECDSA_P384_SHA384"
)

// SignatureParams represents parameters for creating a signature
type SignatureParams struct {
	Message           []byte
	PrivateKey        []byte
	Algorithm         SignatureAlgorithm
	IncludeTimestamp  bool
	AdditionalData    map[string]interface{}
	ExpirationMinutes int
}

// SignatureVerificationParams represents parameters for verifying a signature
type SignatureVerificationParams struct {
	Message        []byte
	Signature      string
	PublicKey      []byte
	Algorithm      SignatureAlgorithm
	ValidateExpiry bool
}

// SignedMessage represents a signed message
type SignedMessage struct {
	Message          string                 `json:"message"`
	Signature        string                 `json:"signature"`
	Algorithm        SignatureAlgorithm     `json:"algorithm"`
	Timestamp        int64                  `json:"timestamp,omitempty"`
	Expiration       int64                  `json:"expiration,omitempty"`
	AdditionalData   map[string]interface{} `json:"additionalData,omitempty"`
}

// SignMessage signs a message
func SignMessage(params SignatureParams) (*SignedMessage, error) {
	// Check if private key is provided
	if len(params.PrivateKey) == 0 {
		return nil, errors.New("private key is required")
	}

	// Check if message is provided
	if len(params.Message) == 0 {
		return nil, errors.New("message is required")
	}

	// Use default algorithm if not specified
	algorithm := params.Algorithm
	if algorithm == "" {
		algorithm = ECDSA_P256_SHA256
	}

	// Prepare signed message
	signedMsg := &SignedMessage{
		Message:        base64.StdEncoding.EncodeToString(params.Message),
		Algorithm:      algorithm,
		AdditionalData: params.AdditionalData,
	}

	// Add timestamp if requested
	if params.IncludeTimestamp {
		now := time.Now()
		signedMsg.Timestamp = now.Unix()

		// Add expiration if requested
		if params.ExpirationMinutes > 0 {
			signedMsg.Expiration = now.Add(time.Duration(params.ExpirationMinutes) * time.Minute).Unix()
		}
	}

	// Generate the signature
	signature, err := generateSignature(signedMsg, params.PrivateKey, algorithm)
	if err != nil {
		return nil, fmt.Errorf("failed to generate signature: %w", err)
	}

	signedMsg.Signature = signature
	return signedMsg, nil
}

// VerifySignature verifies a signature
func VerifySignature(params SignatureVerificationParams) (bool, error) {
	// Check if public key is provided
	if len(params.PublicKey) == 0 {
		return false, errors.New("public key is required")
	}

	// Check if message is provided
	if len(params.Message) == 0 {
		return false, errors.New("message is required")
	}

	// Check if signature is provided
	if params.Signature == "" {
		return false, errors.New("signature is required")
	}

	// Parse the signature parts
	parts := strings.Split(params.Signature, ".")
	if len(parts) != 2 {
		return false, errors.New("invalid signature format")
	}

	// Decode the metadata
	metadataBytes, err := base64.StdEncoding.DecodeString(parts[0])
	if err != nil {
		return false, fmt.Errorf("failed to decode metadata: %w", err)
	}

	// Parse the metadata
	var signedMsg SignedMessage
	if err := json.Unmarshal(metadataBytes, &signedMsg); err != nil {
		return false, fmt.Errorf("failed to parse metadata: %w", err)
	}

	// Check if the message matches
	messageBase64 := base64.StdEncoding.EncodeToString(params.Message)
	if signedMsg.Message != messageBase64 {
		return false, errors.New("message mismatch")
	}

	// Check expiration if requested
	if params.ValidateExpiry && signedMsg.Expiration > 0 {
		if time.Now().Unix() > signedMsg.Expiration {
			return false, errors.New("signature has expired")
		}
	}

	// Get the algorithm
	algorithm := params.Algorithm
	if algorithm == "" {
		algorithm = signedMsg.Algorithm
	}

	// Verify the signature
	return verifySignature(signedMsg, parts[1], params.PublicKey, algorithm)
}

// ParseSignedMessage parses a signed message
func ParseSignedMessage(signatureStr string) (*SignedMessage, error) {
	parts := strings.Split(signatureStr, ".")
	if len(parts) != 2 {
		return nil, errors.New("invalid signature format")
	}

	// Decode the metadata
	metadataBytes, err := base64.StdEncoding.DecodeString(parts[0])
	if err != nil {
		return nil, fmt.Errorf("failed to decode metadata: %w", err)
	}

	// Parse the metadata
	var signedMsg SignedMessage
	if err := json.Unmarshal(metadataBytes, &signedMsg); err != nil {
		return nil, fmt.Errorf("failed to parse metadata: %w", err)
	}

	// Add the signature
	signedMsg.Signature = parts[1]

	return &signedMsg, nil
}

// SerializeSignedMessage serializes a signed message
func SerializeSignedMessage(signedMsg *SignedMessage) (string, error) {
	// Create a copy of the signed message without the signature
	metadataMsg := *signedMsg
	signature := metadataMsg.Signature
	metadataMsg.Signature = ""

	// Serialize the metadata
	metadataBytes, err := json.Marshal(metadataMsg)
	if err != nil {
		return "", fmt.Errorf("failed to serialize metadata: %w", err)
	}

	// Encode the metadata
	metadataBase64 := base64.StdEncoding.EncodeToString(metadataBytes)

	// Combine the metadata and signature
	return fmt.Sprintf("%s.%s", metadataBase64, signature), nil
}

// generateSignature generates a signature for a signed message
func generateSignature(signedMsg *SignedMessage, privateKeyBytes []byte, algorithm SignatureAlgorithm) (string, error) {
	// Create a copy of the signed message without the signature
	metadataMsg := *signedMsg
	metadataMsg.Signature = ""

	// Serialize the metadata
	metadataBytes, err := json.Marshal(metadataMsg)
	if err != nil {
		return "", fmt.Errorf("failed to serialize metadata: %w", err)
	}

	// Encode the metadata
	metadataBase64 := base64.StdEncoding.EncodeToString(metadataBytes)

	// Generate the signature based on the algorithm
	var signature []byte
	switch algorithm {
	case ECDSA_P256_SHA256:
		// Parse the private key
		privateKey, err := x509.ParseECPrivateKey(privateKeyBytes)
		if err != nil {
			return "", fmt.Errorf("failed to parse private key: %w", err)
		}

		// Hash the message
		hash := sha256.Sum256([]byte(metadataBase64))

		// Sign the hash
		r, s, err := ecdsa.Sign(rand.Reader, privateKey, hash[:])
		if err != nil {
			return "", fmt.Errorf("failed to sign message: %w", err)
		}

		// Combine r and s
		signature = append(r.Bytes(), s.Bytes()...)
	case ECDSA_P384_SHA384:
		// Parse the private key
		privateKey, err := x509.ParseECPrivateKey(privateKeyBytes)
		if err != nil {
			return "", fmt.Errorf("failed to parse private key: %w", err)
		}

		// Create a SHA-384 hash
		h := crypto.SHA384.New()
		h.Write([]byte(metadataBase64))
		hash := h.Sum(nil)

		// Sign the hash
		r, s, err := ecdsa.Sign(rand.Reader, privateKey, hash)
		if err != nil {
			return "", fmt.Errorf("failed to sign message: %w", err)
		}

		// Combine r and s
		signature = append(r.Bytes(), s.Bytes()...)
	default:
		return "", fmt.Errorf("unsupported algorithm: %s", algorithm)
	}

	// Return the signature
	return fmt.Sprintf("%s.%s", metadataBase64, hex.EncodeToString(signature)), nil
}

// verifySignature verifies a signature for a signed message
func verifySignature(signedMsg SignedMessage, signatureHex string, publicKeyBytes []byte, algorithm SignatureAlgorithm) (bool, error) {
	// Create a copy of the signed message without the signature
	metadataMsg := signedMsg
	metadataMsg.Signature = ""

	// Serialize the metadata
	metadataBytes, err := json.Marshal(metadataMsg)
	if err != nil {
		return false, fmt.Errorf("failed to serialize metadata: %w", err)
	}

	// Encode the metadata
	metadataBase64 := base64.StdEncoding.EncodeToString(metadataBytes)

	// Decode the signature
	signature, err := hex.DecodeString(signatureHex)
	if err != nil {
		return false, fmt.Errorf("failed to decode signature: %w", err)
	}

	// Verify the signature based on the algorithm
	switch algorithm {
	case ECDSA_P256_SHA256:
		// Parse the public key
		publicKey, err := x509.ParsePKIXPublicKey(publicKeyBytes)
		if err != nil {
			return false, fmt.Errorf("failed to parse public key: %w", err)
		}

		ecdsaPublicKey, ok := publicKey.(*ecdsa.PublicKey)
		if !ok {
			return false, errors.New("public key is not an ECDSA public key")
		}

		// Hash the message
		hash := sha256.Sum256([]byte(metadataBase64))

		// Verify the signature
		sigLen := len(signature)
		if sigLen%2 != 0 {
			return false, errors.New("invalid signature length")
		}

		r := new(big.Int).SetBytes(signature[:sigLen/2])
		s := new(big.Int).SetBytes(signature[sigLen/2:])

		return ecdsa.Verify(ecdsaPublicKey, hash[:], r, s), nil
	case ECDSA_P384_SHA384:
		// Parse the public key
		publicKey, err := x509.ParsePKIXPublicKey(publicKeyBytes)
		if err != nil {
			return false, fmt.Errorf("failed to parse public key: %w", err)
		}

		ecdsaPublicKey, ok := publicKey.(*ecdsa.PublicKey)
		if !ok {
			return false, errors.New("public key is not an ECDSA public key")
		}

		// Create a SHA-384 hash
		h := crypto.SHA384.New()
		h.Write([]byte(metadataBase64))
		hash := h.Sum(nil)

		// Verify the signature
		sigLen := len(signature)
		if sigLen%2 != 0 {
			return false, errors.New("invalid signature length")
		}

		r := new(big.Int).SetBytes(signature[:sigLen/2])
		s := new(big.Int).SetBytes(signature[sigLen/2:])

		return ecdsa.Verify(ecdsaPublicKey, hash, r, s), nil
	default:
		return false, fmt.Errorf("unsupported algorithm: %s", algorithm)
	}
}

// GenerateECDSAKeyPair generates an ECDSA key pair
func GenerateECDSAKeyPair(curve elliptic.Curve) ([]byte, []byte, error) {
	// Generate a key pair
	privateKey, err := ecdsa.GenerateKey(curve, rand.Reader)
	if err != nil {
		return nil, nil, fmt.Errorf("failed to generate key pair: %w", err)
	}

	// Marshal the private key
	privateKeyBytes, err := x509.MarshalECPrivateKey(privateKey)
	if err != nil {
		return nil, nil, fmt.Errorf("failed to marshal private key: %w", err)
	}

	// Marshal the public key
	publicKeyBytes, err := x509.MarshalPKIXPublicKey(&privateKey.PublicKey)
	if err != nil {
		return nil, nil, fmt.Errorf("failed to marshal public key: %w", err)
	}

	return privateKeyBytes, publicKeyBytes, nil
}