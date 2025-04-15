package internal

import (
	"context"
	"math/big"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
)

// GasManager manages gas allocations and usage
type GasManager interface {
	AllocateGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*models.Allocation, error)
	ReleaseGas(ctx context.Context, userAddress util.Uint160) error
	UseGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) error
	GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error)
	RefillGas(ctx context.Context) error
}

// GasStore stores gas allocation data
type GasStore interface {
	SaveAllocation(ctx context.Context, allocation *models.Allocation) error
	GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error)
	DeleteAllocation(ctx context.Context, userAddress util.Uint160) error
	ListAllocations(ctx context.Context) ([]*models.Allocation, error)
	GetPool(ctx context.Context) (*models.GasPool, error)
	SavePool(ctx context.Context, pool *models.GasPool) error
}

// GasMetricsCollector collects gas usage metrics
type GasMetricsCollector interface {
	RecordAllocation(ctx context.Context, allocation *models.Allocation)
	RecordUsage(ctx context.Context, userAddress util.Uint160, amount *big.Int)
	RecordRefill(ctx context.Context, amount *big.Int, success bool)
	GetMetrics(ctx context.Context) *models.GasUsageMetrics
}

// GasAlertManager manages gas-related alerts
type GasAlertManager interface {
	AlertLowGas(ctx context.Context, remaining *big.Int)
	AlertFailedAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int, reason string)
	AlertFailedRefill(ctx context.Context, amount *big.Int, reason string)
	AlertHighUtilization(ctx context.Context, utilization float64, totalGas *big.Int, allocatedGas *big.Int)
	AlertLargeAllocation(ctx context.Context, allocation *models.Allocation)
	AlertAllocationExpired(ctx context.Context, allocation *models.Allocation)
	AlertSystemError(ctx context.Context, component string, err error)
}

// GasValidator validates gas operations
type GasValidator interface {
	ValidateAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int) error
	ValidateUsage(ctx context.Context, allocation *models.Allocation, amount *big.Int) error
	ValidateRefill(ctx context.Context, amount *big.Int) error
}

// BillingManager manages gas billing
type BillingManager interface {
	// Start starts the billing manager
	Start(ctx context.Context) error

	// Stop stops the billing manager
	Stop(ctx context.Context) error

	// ChargeFee charges a fee for gas usage
	ChargeFee(ctx context.Context, userAddress util.Uint160, amount *big.Int) error

	// GetFeeBalance gets the total fees collected
	GetFeeBalance(ctx context.Context) (*big.Int, error)
}
