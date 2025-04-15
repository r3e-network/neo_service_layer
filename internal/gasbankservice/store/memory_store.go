package store

import (
	"context"
	"errors"
	"fmt"
	"math/big"
	sync "sync"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
)

// MemoryStore implements an in-memory storage for gas allocations and pool
type MemoryStore struct {
	allocations      map[string]*models.Allocation
	pool             *models.GasPool
	balances         map[string]*models.UserBalance        // userID -> UserBalance
	policies         map[string]*models.FeePolicy          // userID -> FeePolicy
	claims           map[string]*models.GasClaim           // requestID -> GasClaim
	claimsByUser     map[string][]string                   // userID -> list of requestIDs
	withdrawals      map[string]*models.WithdrawalRecord   // requestID -> WithdrawalRecord
	sponsorships     map[string]*models.PendingSponsorship // sponsorshipID -> PendingSponsorship
	sponsorshipsByTx map[string]string                     // txHash -> sponsorshipID
	mu               sync.RWMutex
}

// NewMemoryStore creates a new in-memory store
func NewMemoryStore() *MemoryStore {
	return &MemoryStore{
		allocations:      make(map[string]*models.Allocation),
		balances:         make(map[string]*models.UserBalance),
		policies:         make(map[string]*models.FeePolicy),
		claims:           make(map[string]*models.GasClaim),
		claimsByUser:     make(map[string][]string),
		withdrawals:      make(map[string]*models.WithdrawalRecord),
		sponsorships:     make(map[string]*models.PendingSponsorship),
		sponsorshipsByTx: make(map[string]string),
	}
}

// SaveAllocation saves a gas allocation to the store
func (s *MemoryStore) SaveAllocation(ctx context.Context, allocation *models.Allocation) error {
	if allocation == nil {
		return errors.New("allocation cannot be nil")
	}

	if allocation.UserAddress.Equals(util.Uint160{}) {
		return errors.New("invalid user address")
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	// Make a deep copy to ensure thread safety
	allocationCopy := *allocation

	// Use user address string as key
	key := allocation.UserAddress.StringLE()
	s.allocations[key] = &allocationCopy

	return nil
}

// GetAllocation retrieves a gas allocation from the store
func (s *MemoryStore) GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error) {
	if userAddress.Equals(util.Uint160{}) {
		return nil, errors.New("invalid user address")
	}

	s.mu.RLock()
	defer s.mu.RUnlock()

	key := userAddress.StringLE()
	allocation, exists := s.allocations[key]
	if !exists {
		return nil, nil // Not an error, allocation doesn't exist
	}

	// Return a copy to ensure thread safety
	allocationCopy := *allocation
	return &allocationCopy, nil
}

// DeleteAllocation removes a gas allocation from the store
func (s *MemoryStore) DeleteAllocation(ctx context.Context, userAddress util.Uint160) error {
	if userAddress.Equals(util.Uint160{}) {
		return errors.New("invalid user address")
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	key := userAddress.StringLE()
	delete(s.allocations, key)

	return nil
}

// ListAllocations retrieves all gas allocations from the store
func (s *MemoryStore) ListAllocations(ctx context.Context) ([]*models.Allocation, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	allocations := make([]*models.Allocation, 0, len(s.allocations))

	for _, allocation := range s.allocations {
		// Return a copy to ensure thread safety
		allocationCopy := *allocation
		allocations = append(allocations, &allocationCopy)
	}

	return allocations, nil
}

// GetAllAllocations retrieves all allocations regardless of status
func (s *MemoryStore) GetAllAllocations(ctx context.Context) ([]*models.Allocation, error) {
	// In this implementation, ListAllocations already returns all allocations
	// In a more complex implementation, this might filter by status differently
	return s.ListAllocations(ctx)
}

// SavePool saves the gas pool to the store
func (s *MemoryStore) SavePool(ctx context.Context, pool *models.GasPool) error {
	if pool == nil {
		return errors.New("pool cannot be nil")
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	// Make a deep copy to ensure thread safety
	poolCopy := *pool
	s.pool = &poolCopy

	return nil
}

// GetPool retrieves the gas pool from the store
func (s *MemoryStore) GetPool(ctx context.Context) (*models.GasPool, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	if s.pool == nil {
		return nil, nil // Not an error, pool doesn't exist
	}

	// Return a copy to ensure thread safety
	poolCopy := *s.pool
	return &poolCopy, nil
}

// Close closes the store and releases resources
func (s *MemoryStore) Close() error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Clear all data
	s.allocations = make(map[string]*models.Allocation)
	s.pool = nil
	s.balances = make(map[string]*models.UserBalance)
	s.policies = make(map[string]*models.FeePolicy)
	s.claims = make(map[string]*models.GasClaim)
	s.claimsByUser = make(map[string][]string)
	s.withdrawals = make(map[string]*models.WithdrawalRecord)
	s.sponsorships = make(map[string]*models.PendingSponsorship)
	s.sponsorshipsByTx = make(map[string]string)

	return nil
}

// --- User Balances ---

func (m *MemoryStore) GetUserBalance(ctx context.Context, userAddress util.Uint160) (*models.UserBalance, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()
	userID := userAddress.StringLE()
	balance, ok := m.balances[userID]
	if !ok {
		// Return a zero balance if not found
		return &models.UserBalance{
			UserID:        userAddress,
			Balance:       big.NewInt(0),
			LockedBalance: big.NewInt(0),
			UpdatedAt:     time.Now(),
		}, nil
		// Alternatively: return nil, fmt.Errorf("user balance not found for %s", userID)
	}
	// Return a copy to prevent modification outside of UpdateUserBalance
	cpy := *balance
	cpy.Balance = new(big.Int).Set(balance.Balance)
	cpy.LockedBalance = new(big.Int).Set(balance.LockedBalance)
	return &cpy, nil
}

func (m *MemoryStore) SaveUserBalance(ctx context.Context, balance *models.UserBalance) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	userID := balance.UserID.StringLE()
	m.balances[userID] = balance // Store pointer directly for simplicity in memory store
	return nil
}

// UpdateUserBalance applies updates transactionally (simulated with mutex).
func (m *MemoryStore) UpdateUserBalance(ctx context.Context, userAddress util.Uint160, updateFunc func(*models.UserBalance) (*models.UserBalance, error)) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	userID := userAddress.StringLE()

	// Get current balance or initialize if not found
	currentBalance, ok := m.balances[userID]
	if !ok {
		currentBalance = &models.UserBalance{
			UserID:        userAddress,
			Balance:       big.NewInt(0),
			LockedBalance: big.NewInt(0),
		}
	}

	// Create a temporary copy for the update function
	tempBalance := *currentBalance
	tempBalance.Balance = new(big.Int).Set(currentBalance.Balance)
	tempBalance.LockedBalance = new(big.Int).Set(currentBalance.LockedBalance)

	// Apply the update function
	updatedBalance, err := updateFunc(&tempBalance)
	if err != nil {
		return fmt.Errorf("update function failed: %w", err) // Use fmt.Errorf
	}

	// Validate results (e.g., ensure balance doesn't go negative)
	if updatedBalance.Balance.Sign() < 0 || updatedBalance.LockedBalance.Sign() < 0 {
		return fmt.Errorf("insufficient funds or invalid balance state after update") // Use fmt.Errorf
	}

	// Save the updated balance back
	updatedBalance.UpdatedAt = time.Now()
	m.balances[userID] = updatedBalance
	return nil
}

// --- Fee Policies ---

func (m *MemoryStore) GetFeePolicy(ctx context.Context, userAddress util.Uint160) (*models.FeePolicy, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()
	userID := userAddress.StringLE()
	policy, ok := m.policies[userID]
	if !ok {
		return nil, fmt.Errorf("fee policy not found for user %s", userID) // Use fmt.Errorf
	}
	cpy := *policy // Return a copy
	return &cpy, nil
}

func (m *MemoryStore) SaveFeePolicy(ctx context.Context, policy *models.FeePolicy) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	userID := policy.UserID.StringLE()
	m.policies[userID] = policy // Store pointer directly
	return nil
}

func (m *MemoryStore) DeleteFeePolicy(ctx context.Context, userAddress util.Uint160) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	userID := userAddress.StringLE()
	if _, ok := m.policies[userID]; !ok {
		return fmt.Errorf("fee policy not found for user %s", userID) // Use fmt.Errorf
	}
	delete(m.policies, userID)
	return nil
}

// --- Gas Claims ---

func (m *MemoryStore) SaveGasClaim(ctx context.Context, claim *models.GasClaim) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	// Generate an ID if not present (though SubmitGasClaim should generate it)
	requestID := uuid.NewString()
	userID := claim.UserID.StringLE()
	m.claims[requestID] = claim
	m.claimsByUser[userID] = append(m.claimsByUser[userID], requestID)
	return nil
}

func (m *MemoryStore) GetGasClaim(ctx context.Context, userAddress util.Uint160, requestID string) (*models.GasClaim, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()
	claim, ok := m.claims[requestID]
	if !ok {
		return nil, fmt.Errorf("gas claim not found with ID %s", requestID) // Use fmt.Errorf
	}
	// Verify ownership
	if !claim.UserID.Equals(userAddress) {
		return nil, fmt.Errorf("permission denied for gas claim ID %s", requestID) // Use fmt.Errorf
	}
	cpy := *claim // Return a copy
	return &cpy, nil
}

func (m *MemoryStore) UpdateGasClaimStatus(ctx context.Context, userAddress util.Uint160, requestID, status string, txHash *util.Uint256, errorMsg string) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	claim, ok := m.claims[requestID]
	if !ok {
		return fmt.Errorf("gas claim not found with ID %s", requestID) // Use fmt.Errorf
	}
	// Verify ownership
	if !claim.UserID.Equals(userAddress) {
		return fmt.Errorf("permission denied for gas claim ID %s", requestID) // Use fmt.Errorf
	}
	claim.Status = status
	claim.SubmittedTxHash = txHash
	claim.Error = errorMsg
	return nil
}

func (m *MemoryStore) ListPendingGasClaims(ctx context.Context) ([]*models.GasClaim, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()
	pending := make([]*models.GasClaim, 0)
	for _, claim := range m.claims {
		if claim.Status == "Pending" { // Assuming "Pending" is the status string
			cpy := *claim // Copy
			pending = append(pending, &cpy)
		}
	}
	return pending, nil
}

// --- Withdrawals ---

func (m *MemoryStore) SaveWithdrawalRecord(ctx context.Context, record *models.WithdrawalRecord) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	if record.RequestID == "" {
		return errors.New("cannot save withdrawal record with empty request ID")
	}
	m.withdrawals[record.RequestID] = record
	return nil
}

func (m *MemoryStore) GetWithdrawalRecord(ctx context.Context, requestID string) (*models.WithdrawalRecord, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()
	record, ok := m.withdrawals[requestID]
	if !ok {
		return nil, fmt.Errorf("withdrawal record not found with ID %s", requestID) // Use fmt.Errorf
	}
	cpy := *record // Return copy
	cpy.Amount = new(big.Int).Set(record.Amount)
	cpy.TotalLocked = new(big.Int).Set(record.TotalLocked)
	return &cpy, nil
}

func (m *MemoryStore) UpdateWithdrawalStatus(ctx context.Context, requestID, status string, txHash *util.Uint256, errorMsg string) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	record, ok := m.withdrawals[requestID]
	if !ok {
		return fmt.Errorf("withdrawal record not found with ID %s", requestID) // Use fmt.Errorf
	}
	record.Status = status
	record.TxHash = txHash
	record.Error = errorMsg
	record.UpdatedAt = time.Now()
	return nil
}

// --- Fee Sponsorships ---

func (m *MemoryStore) SavePendingSponsorship(ctx context.Context, sponsorship *models.PendingSponsorship) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	if sponsorship.SponsorshipID == "" {
		sponsorship.SponsorshipID = uuid.NewString() // Generate ID if needed
	}
	txHashStr := sponsorship.TxHash.StringLE()
	// Check if sponsorship for this tx already exists
	if existingID, ok := m.sponsorshipsByTx[txHashStr]; ok {
		if existingID != sponsorship.SponsorshipID {
			return fmt.Errorf("sponsorship for tx %s already exists", txHashStr) // Use fmt.Errorf
		}
	}
	m.sponsorships[sponsorship.SponsorshipID] = sponsorship
	m.sponsorshipsByTx[txHashStr] = sponsorship.SponsorshipID
	return nil
}

func (m *MemoryStore) GetPendingSponsorshipByTx(ctx context.Context, userAddress util.Uint160, txHash util.Uint256) (*models.PendingSponsorship, error) {
	m.mu.RLock()
	defer m.mu.RUnlock()
	txHashStr := txHash.StringLE()
	sponsorshipID, ok := m.sponsorshipsByTx[txHashStr]
	if !ok {
		return nil, fmt.Errorf("pending sponsorship not found for tx %s", txHashStr) // Use fmt.Errorf
	}
	sponsorship, ok := m.sponsorships[sponsorshipID]
	if !ok {
		// Data inconsistency
		delete(m.sponsorshipsByTx, txHashStr) // Clean up index
		return nil, fmt.Errorf("pending sponsorship inconsistency for tx %s", txHashStr)
	}
	// Verify owner
	if !sponsorship.UserID.Equals(userAddress) {
		return nil, fmt.Errorf("permission denied for pending sponsorship tx %s", txHashStr)
	}
	cpy := *sponsorship // Return copy
	cpy.LockedAmount = new(big.Int).Set(sponsorship.LockedAmount)
	return &cpy, nil
}

func (m *MemoryStore) DeletePendingSponsorship(ctx context.Context, sponsorshipID string) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	sponsorship, ok := m.sponsorships[sponsorshipID]
	if !ok {
		return nil // Already deleted or never existed
	}
	txHashStr := sponsorship.TxHash.StringLE()
	delete(m.sponsorships, sponsorshipID)
	delete(m.sponsorshipsByTx, txHashStr)
	return nil
}

// ensure memoryStore implements Store
var _ Store = (*MemoryStore)(nil)
