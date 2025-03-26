package types

import (
	"encoding/hex"
	"fmt"
	"strings"
)

// Hash represents a 32-byte hash
type Hash [32]byte

// String returns the hash as a string
func (h Hash) String() string {
	return hex.EncodeToString(h[:])
}

// Address represents a Neo N3 address
type Address string

// String returns the address as a string
func (a Address) String() string {
	return string(a)
}

// ScriptHash represents a Neo N3 script hash
type ScriptHash string

// String returns the script hash as a string
func (s ScriptHash) String() string {
	return string(s)
}

// ParseAddress parses a string address into an Address
func ParseAddress(address string) (Address, error) {
	// Implement address validation here
	if address == "" {
		return "", fmt.Errorf("address cannot be empty")
	}
	return Address(address), nil
}

// ParseScriptHash parses a string script hash into a ScriptHash
func ParseScriptHash(hash string) (ScriptHash, error) {
	// Remove "0x" prefix if present
	if strings.HasPrefix(hash, "0x") {
		hash = hash[2:]
	}
	
	// Validate length
	if len(hash) != 40 {
		return "", fmt.Errorf("invalid script hash length: %d", len(hash))
	}
	
	// Validate characters
	_, err := hex.DecodeString(hash)
	if err != nil {
		return "", fmt.Errorf("invalid script hash format: %w", err)
	}
	
	return ScriptHash(hash), nil
}

// Transaction represents a Neo N3 transaction
type Transaction struct {
	Hash      Hash
	Script    []byte
	Witnesses []Witness
}

// Witness represents a Neo N3 witness
type Witness struct {
	InvocationScript   []byte
	VerificationScript []byte
}