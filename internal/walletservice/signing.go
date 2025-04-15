package wallet

import (
	"context"
	"crypto/elliptic"
	"crypto/sha256"
	"errors"
	"fmt"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/config/netmode"
	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/crypto/keys"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"go.uber.org/zap"
)

// SignTransaction signs a transaction using the specified account
func (s *ServiceImpl) SignTransaction(ctx context.Context, walletName string, address string, tx *transaction.Transaction) (*transaction.Transaction, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Validate parameters
	if tx == nil {
		return nil, errors.New("transaction cannot be nil")
	}

	// Get open wallet
	w, err := s.getOpenWallet(walletName)
	if err != nil {
		return nil, err
	}

	// Check if wallet is unlocked
	password, exists := s.walletPasswords[walletName]
	if !exists {
		return nil, ErrWalletLocked
	}

	// Find account by address
	var account *wallet.Account
	for _, acc := range w.Accounts {
		if acc.Address == address {
			account = acc
			break
		}
	}

	if account == nil {
		return nil, ErrAccountNotFound
	}

	// Decrypt the account with the password
	if err := account.Decrypt(password, w.Scrypt); err != nil {
		return nil, fmt.Errorf("failed to decrypt account: %w", err)
	}
	defer account.Close() // Make sure to clean up private key after use

	// Sign the transaction
	err = account.SignTx(netmode.Magic(s.getNetworkMagic()), tx)
	if err != nil {
		return nil, fmt.Errorf("failed to sign transaction: %w", err)
	}

	// Log signature operation
	if s.config.AuditLog {
		s.logger.Info("Transaction signed",
			zap.String("wallet", walletName),
			zap.String("address", address),
			zap.String("tx_hash", tx.Hash().StringLE()))
	}

	// Reset auto-lock timer
	s.setupAutoLockTimer(walletName)

	return tx, nil
}

// SignMessage signs an arbitrary message using the specified account
func (s *ServiceImpl) SignMessage(ctx context.Context, walletName string, address string, message []byte) ([]byte, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Validate parameters
	if len(message) == 0 {
		return nil, errors.New("message cannot be empty")
	}

	// Get open wallet
	w, err := s.getOpenWallet(walletName)
	if err != nil {
		return nil, err
	}

	// Check if wallet is unlocked
	password, exists := s.walletPasswords[walletName]
	if !exists {
		return nil, ErrWalletLocked
	}

	// Find account by address
	var account *wallet.Account
	for _, acc := range w.Accounts {
		if acc.Address == address {
			account = acc
			break
		}
	}

	if account == nil {
		return nil, ErrAccountNotFound
	}

	// Decrypt the account with the password
	if err := account.Decrypt(password, w.Scrypt); err != nil {
		return nil, fmt.Errorf("failed to decrypt account: %w", err)
	}
	defer account.Close() // Make sure to clean up private key after use

	// Hash the message (SHA-256)
	h := sha256.Sum256(message)

	// Convert to util.Uint256 for Neo-Go compatibility
	var hash util.Uint256
	copy(hash[:], h[:])

	// Get the private key to sign manually since we're not signing a transaction
	privateKey := account.PrivateKey()
	if privateKey == nil {
		return nil, errors.New("account private key not available")
	}

	// Sign the hash with the account's private key
	signature := privateKey.Sign(h[:])

	// Log signature operation
	if s.config.AuditLog {
		s.logger.Info("Message signed",
			zap.String("wallet", walletName),
			zap.String("address", address),
			zap.String("message_hash", fmt.Sprintf("%x", h)))
	}

	// For future audit purposes, we could store the signature info
	// This is just created but not used, can be implemented later
	_ = &SignatureInfo{
		Address:     address,
		WalletName:  walletName,
		Timestamp:   time.Now().Unix(),
		Success:     true,
		MessageHash: h[:],
	}

	// Reset auto-lock timer
	s.setupAutoLockTimer(walletName)

	return signature, nil
}

// VerifySignature verifies a signature against a message and public key
func (s *ServiceImpl) VerifySignature(ctx context.Context, message []byte, signature []byte, publicKey []byte) (bool, error) {
	// Validate parameters
	if len(message) == 0 {
		return false, errors.New("message cannot be empty")
	}

	if len(signature) == 0 {
		return false, errors.New("signature cannot be empty")
	}

	if len(publicKey) == 0 {
		return false, errors.New("public key cannot be empty")
	}

	// Parse public key
	pubKey, err := keys.NewPublicKeyFromBytes(publicKey, elliptic.P256())
	if err != nil {
		return false, fmt.Errorf("invalid public key: %w", err)
	}

	// Hash the message (SHA-256)
	hash := sha256.Sum256(message)

	// Verify the signature
	result := pubKey.Verify(signature, hash[:])

	return result, nil
}

// AddSignatureToTx adds a signature to a partially signed transaction
func (s *ServiceImpl) AddSignatureToTx(ctx context.Context, partiallySignedTx *transaction.Transaction, walletName string, address string) (*transaction.Transaction, error) {
	// This is basically the same as SignTransaction but for partially signed multi-sig transactions
	return s.SignTransaction(ctx, walletName, address, partiallySignedTx)
}

// getNetworkMagic returns the network magic number based on the configured network
func (s *ServiceImpl) getNetworkMagic() uint32 {
	// This is a simplified version that should be replaced with actual network magic retrieval
	// based on the s.config.Network setting

	// For example:
	switch s.config.Network {
	case "mainnet":
		return 0x334F454E // NEO3 MainNet magic number
	case "testnet":
		return 0x74746E41 // NEO3 TestNet magic number
	default:
		return 0x73746E41 // Default to TestNet
	}
}
