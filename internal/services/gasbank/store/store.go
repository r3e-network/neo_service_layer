package store

import (
	"context"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

// Closer is an interface for resources that need to be closed
type Closer interface {
	Close() error
}

// GasStore defines the interface for gas allocation and pool storage
type GasStore interface {
	// SaveAllocation saves a gas allocation to the store
	SaveAllocation(ctx context.Context, allocation *models.Allocation) error

	// GetAllocation retrieves a gas allocation from the store
	GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error)

	// DeleteAllocation removes a gas allocation from the store
	DeleteAllocation(ctx context.Context, userAddress util.Uint160) error

	// ListAllocations retrieves all gas allocations from the store
	ListAllocations(ctx context.Context) ([]*models.Allocation, error)

	// GetAllAllocations retrieves all allocations regardless of status
	GetAllAllocations(ctx context.Context) ([]*models.Allocation, error)

	// SavePool saves the gas pool to the store
	SavePool(ctx context.Context, pool *models.GasPool) error

	// GetPool retrieves the gas pool from the store
	GetPool(ctx context.Context) (*models.GasPool, error)

	// Close closes the store and releases resources
	Close() error
}
