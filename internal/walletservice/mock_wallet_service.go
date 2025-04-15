package wallet

import (
	"context"
	"fmt"

	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/util"
	log "github.com/sirupsen/logrus"
)

// WalletService is an interface for wallet operations
type WalletService interface {
	SignTx(ctx context.Context, acc util.Uint160, tx *transaction.Transaction) error
}

// MockWalletService provides a mock implementation of WalletService for testing and development
type MockWalletService struct {
	MockError      error
	SignedAccounts map[string]bool
	SignedTxs      []*transaction.Transaction
}

// NewMockWalletService creates a new mock wallet service
func NewMockWalletService() *MockWalletService {
	log.Warn("Using MockWalletService - Transactions will be signed with mock data!")
	return &MockWalletService{
		SignedAccounts: make(map[string]bool),
		SignedTxs:      make([]*transaction.Transaction, 0),
	}
}

// SignTx adds a mock witness to the transaction and tracks the signing request
func (m *MockWalletService) SignTx(ctx context.Context, acc util.Uint160, tx *transaction.Transaction) error {
	if m.MockError != nil {
		return m.MockError
	}

	log.Debugf("MockWalletService: Signing tx for account %s", acc.StringLE())

	// Add a minimal mock witness
	mockScript := []byte{0x40, 0x00} // Simple dummy signature
	mockInvocationScript := []byte{byte(len(mockScript))}
	mockInvocationScript = append(mockInvocationScript, mockScript...)

	// Add verification script (mock keys/etc)
	mockVerificationScript := []byte{
		0x21, // PUSHDATA1 33 bytes (compressed public key)
		0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0xac, // CHECKSIG
	}

	witness := transaction.Witness{
		InvocationScript:   mockInvocationScript,
		VerificationScript: mockVerificationScript,
	}

	// Find signer index
	signerIndex := -1
	for i, signer := range tx.Signers {
		if signer.Account.Equals(acc) {
			signerIndex = i
			break
		}
	}

	if signerIndex == -1 {
		return fmt.Errorf("account %s not found in transaction signers", acc.StringLE())
	}

	// Add or replace witness at correct position
	if len(tx.Scripts) <= signerIndex {
		// Add padding witnesses if needed
		for i := len(tx.Scripts); i < signerIndex; i++ {
			tx.Scripts = append(tx.Scripts, transaction.Witness{})
		}
		tx.Scripts = append(tx.Scripts, witness)
	} else {
		tx.Scripts[signerIndex] = witness
	}

	// Record that we signed this transaction
	m.SignedAccounts[acc.StringLE()] = true
	m.SignedTxs = append(m.SignedTxs, tx)

	return nil
}

// ResetMockState clears the signing history
func (m *MockWalletService) ResetMockState() {
	m.SignedAccounts = make(map[string]bool)
	m.SignedTxs = make([]*transaction.Transaction, 0)
	m.MockError = nil
}

// SetMockError sets an error to be returned by the mock
func (m *MockWalletService) SetMockError(err error) {
	m.MockError = err
}

// Helper to ensure implementation
var _ WalletService = (*MockWalletService)(nil)
