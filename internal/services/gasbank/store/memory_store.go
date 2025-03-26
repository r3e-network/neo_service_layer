package store

import (
	"context"
	"errors"
	"sync"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

// MemoryStore implements an in-memory storage for gas allocations and pool
type MemoryStore struct {
	allocations map[string]*models.Allocation
	pool        *models.GasPool
	mu          sync.RWMutex
}

// NewMemoryStore creates a new in-memory store
func NewMemoryStore() *MemoryStore {
	return &MemoryStore{
		allocations: make(map[string]*models.Allocation),
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

	return nil
}
