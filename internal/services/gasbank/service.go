package gasbank

import (
	"context"
	"errors"
	"fmt"
	"math/big"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/gasbank/internal"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
	"github.com/will/neo_service_layer/internal/services/gasbank/store"
)

// Service implements the GasBank service
type Service struct {
	config            *Config
	allocationManager *internal.AllocationManager
	poolManager       *internal.PoolManager
	store             store.GasStore
	metrics           internal.GasMetricsCollector
	alerts            internal.GasAlertManager
	billingManager    internal.BillingManager
}

// Config defines the configuration for the GasBank service
type Config struct {
	// Initial gas amount in the pool
	InitialGas *big.Int

	// Amount to refill when threshold is reached
	RefillAmount *big.Int

	// Threshold that triggers refills
	RefillThreshold *big.Int

	// Maximum allocation per user
	MaxAllocationPerUser *big.Int

	// Minimum allocation amount
	MinAllocationAmount *big.Int

	// Maximum time allocation can be held
	MaxAllocationTime time.Duration

	// Minimum wait between refills
	CooldownPeriod time.Duration

	// Store type (memory or persistent)
	StoreType string

	// Store options for persistent storage
	StorePath string

	// Transaction manager for interacting with blockchain
	TxManager internal.TransactionManager

	// Alert configuration
	AlertConfig *internal.AlertConfig

	// Expiration check interval
	ExpirationCheckInterval time.Duration

	// Monitor interval for pool status
	MonitorInterval time.Duration
}

// DefaultConfig returns a default configuration
func DefaultConfig() *Config {
	return &Config{
		InitialGas:              big.NewInt(1000000000), // 10 GAS
		RefillAmount:            big.NewInt(500000000),  // 5 GAS
		RefillThreshold:         big.NewInt(200000000),  // 2 GAS
		MaxAllocationPerUser:    big.NewInt(100000000),  // 1 GAS
		MinAllocationAmount:     big.NewInt(1000000),    // 0.01 GAS
		MaxAllocationTime:       24 * time.Hour,
		CooldownPeriod:          5 * time.Minute,
		StoreType:               "memory",
		AlertConfig:             internal.DefaultAlertConfig(),
		ExpirationCheckInterval: 15 * time.Minute,
		MonitorInterval:         5 * time.Minute,
	}
}

// NewService creates a new GasBank service
func NewService(ctx context.Context, config *Config) (*Service, error) {
	if config == nil {
		config = DefaultConfig()
	}

	// Create gas usage policy
	policy := &models.GasUsagePolicy{
		MaxAllocationPerUser: config.MaxAllocationPerUser,
		MinAllocationAmount:  config.MinAllocationAmount,
		MaxAllocationTime:    config.MaxAllocationTime,
		RefillThreshold:      config.RefillThreshold,
		RefillAmount:         config.RefillAmount,
		CooldownPeriod:       config.CooldownPeriod,
	}

	// Create store
	var gasStore store.GasStore
	var err error

	switch config.StoreType {
	case "memory":
		gasStore = store.NewMemoryStore()
	case "persistent":
		// TODO: Replace with BadgerStore when dependencies are available
		// gasStore, err = store.NewBadgerStore(store.BadgerStoreOptions{
		//     DbPath: config.StorePath,
		// })
		gasStore = store.NewMemoryStore()
	default:
		return nil, fmt.Errorf("unsupported store type: %s", config.StoreType)
	}

	if err != nil {
		return nil, fmt.Errorf("failed to create store: %w", err)
	}

	// Create metrics collector
	metricsCollector := internal.NewBasicMetricsCollector()

	// Create alerts manager with custom config if provided
	alertsManager := internal.NewBasicAlertManagerWithConfig(config.AlertConfig)

	// Create allocation manager
	allocationManager := internal.NewAllocationManager(
		gasStore,
		policy,
		metricsCollector,
		alertsManager,
	)

	// Create pool manager
	poolManager := internal.NewPoolManager(
		gasStore,
		config.TxManager,
		policy,
		metricsCollector,
		alertsManager,
	)

	// Initialize pool if needed
	pool, err := gasStore.GetPool(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to get gas pool: %w", err)
	}

	if pool == nil && config.InitialGas.Sign() > 0 {
		pool = models.NewGasPool(config.InitialGas)
		err = gasStore.SavePool(ctx, pool)
		if err != nil {
			return nil, fmt.Errorf("failed to initialize gas pool: %w", err)
		}
	}

	// Create billing manager
	billingManager := internal.NewBasicBillingManager()

	service := &Service{
		config:            config,
		allocationManager: allocationManager,
		poolManager:       poolManager,
		store:             gasStore,
		metrics:           metricsCollector,
		alerts:            alertsManager,
		billingManager:    billingManager,
	}

	// Start background monitoring if context is not short-lived
	if _, ok := ctx.Deadline(); !ok {
		go service.startMonitoring(context.Background())
	}

	return service, nil
}

// startMonitoring starts background monitoring tasks
func (s *Service) startMonitoring(ctx context.Context) {
	// Monitor gas pool utilization
	go s.monitorPoolUtilization(ctx)

	// Check for expired allocations
	go s.monitorExpiredAllocations(ctx)
}

// monitorPoolUtilization periodically checks the gas pool utilization
func (s *Service) monitorPoolUtilization(ctx context.Context) {
	ticker := time.NewTicker(s.config.MonitorInterval)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			return
		case <-ticker.C:
			pool, err := s.store.GetPool(ctx)
			if err != nil {
				s.alerts.AlertSystemError(ctx, "pool_monitor", fmt.Errorf("failed to get pool: %w", err))
				continue
			}

			if pool != nil {
				// Check if refill is needed
				if pool.Amount.Cmp(s.config.RefillThreshold) < 0 {
					if err := s.RefillPool(ctx); err != nil {
						s.alerts.AlertFailedRefill(ctx, s.config.RefillAmount, err.Error())
					}
				}

				// Alert on low gas
				s.alerts.AlertLowGas(ctx, pool.Amount)
			}
		}
	}
}

// monitorExpiredAllocations checks for and handles expired allocations
func (s *Service) monitorExpiredAllocations(ctx context.Context) {
	ticker := time.NewTicker(s.config.ExpirationCheckInterval)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			return
		case <-ticker.C:
			allocations, err := s.store.GetAllAllocations(ctx)
			if err != nil {
				s.alerts.AlertSystemError(ctx, "expiration_monitor", fmt.Errorf("failed to get allocations: %w", err))
				continue
			}

			for _, allocation := range allocations {
				if allocation.IsExpired() && allocation.Status == "active" {
					// Alert about expired allocation
					s.alerts.AlertAllocationExpired(ctx, allocation)

					// Auto-release expired allocations
					if err := s.ReleaseGas(ctx, allocation.UserAddress); err != nil {
						s.alerts.AlertSystemError(ctx, "expiration_release",
							fmt.Errorf("failed to release expired allocation for %s: %w",
								allocation.UserAddress.StringLE(), err))
					}
				}
			}
		}
	}
}

// AllocateGas allocates gas for a user
func (s *Service) AllocateGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*models.Allocation, error) {
	// Validate request
	if amount == nil || amount.Sign() <= 0 {
		return nil, errors.New("invalid amount")
	}

	// Check available gas in pool
	available, err := s.poolManager.GetAvailableGas(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to get available gas: %w", err)
	}

	if available.Cmp(amount) < 0 {
		return nil, fmt.Errorf("insufficient gas in pool: available=%s, requested=%s",
			available.String(), amount.String())
	}

	// Allocate gas for user
	allocation, err := s.allocationManager.AllocateGas(ctx, userAddress, amount)
	if err != nil {
		return nil, fmt.Errorf("failed to allocate gas: %w", err)
	}

	// Consume gas from pool
	err = s.poolManager.ConsumeGas(ctx, amount)
	if err != nil {
		// Attempt to rollback allocation
		_ = s.allocationManager.ReleaseGas(ctx, userAddress)
		return nil, fmt.Errorf("failed to consume gas from pool: %w", err)
	}

	return allocation, nil
}

// UseGas records the use of gas by a user
func (s *Service) UseGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) error {
	return s.allocationManager.UseGas(ctx, userAddress, amount)
}

// ReleaseGas releases gas allocation for a user
func (s *Service) ReleaseGas(ctx context.Context, userAddress util.Uint160) error {
	// Get current allocation
	allocation, err := s.allocationManager.GetAllocation(ctx, userAddress)
	if err != nil {
		return fmt.Errorf("failed to get allocation: %w", err)
	}

	if allocation == nil {
		return nil // Nothing to release
	}

	// Calculate unused gas
	unusedGas := allocation.RemainingGas()

	// Release allocation
	err = s.allocationManager.ReleaseGas(ctx, userAddress)
	if err != nil {
		return fmt.Errorf("failed to release allocation: %w", err)
	}

	// Return unused gas to pool
	if unusedGas.Sign() > 0 {
		err = s.poolManager.AddGas(ctx, unusedGas)
		if err != nil {
			return fmt.Errorf("failed to return gas to pool: %w", err)
		}
	}

	return nil
}

// GetAllocation gets gas allocation for a user
func (s *Service) GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error) {
	return s.allocationManager.GetAllocation(ctx, userAddress)
}

// GetAvailableGas gets available gas in the pool
func (s *Service) GetAvailableGas(ctx context.Context) (*big.Int, error) {
	return s.poolManager.GetAvailableGas(ctx)
}

// RefillPool refills the gas pool
func (s *Service) RefillPool(ctx context.Context) error {
	return s.poolManager.RefillPool(ctx)
}

// GetMetrics gets usage metrics
func (s *Service) GetMetrics(ctx context.Context) (*models.GasUsageMetrics, error) {
	return s.metrics.GetMetrics(ctx), nil
}

// AllocateGasForTransaction allocates gas for a specific transaction
func (s *Service) AllocateGasForTransaction(ctx context.Context, userAddress util.Uint160, amount *big.Int, txHash string) (*models.Allocation, error) {
	// First allocate gas normally
	allocation, err := s.AllocateGas(ctx, userAddress, amount)
	if err != nil {
		return nil, err
	}

	// Add transaction to allocation
	allocation.Transactions = append(allocation.Transactions, txHash)

	// Save updated allocation
	err = s.store.SaveAllocation(ctx, allocation)
	if err != nil {
		// If saving fails, try to release the allocation
		_ = s.ReleaseGas(ctx, userAddress)
		return nil, fmt.Errorf("failed to save allocation with transaction: %w", err)
	}

	// Alert for large allocations
	s.alerts.AlertLargeAllocation(ctx, allocation)

	return allocation, nil
}

// ExtendAllocation extends the expiration time of an existing allocation
func (s *Service) ExtendAllocation(ctx context.Context, userAddress util.Uint160, extension time.Duration) error {
	allocation, err := s.allocationManager.GetAllocation(ctx, userAddress)
	if err != nil {
		return fmt.Errorf("failed to get allocation: %w", err)
	}

	if allocation == nil {
		return errors.New("no active allocation found")
	}

	// Calculate new expiration time
	newExpiry := allocation.ExpiresAt.Add(extension)
	maxExpiry := time.Now().Add(s.config.MaxAllocationTime)

	// Don't allow extending beyond the maximum allocation time
	if newExpiry.After(maxExpiry) {
		newExpiry = maxExpiry
	}

	// Update expiration time
	allocation.ExpiresAt = newExpiry

	// Save updated allocation
	err = s.store.SaveAllocation(ctx, allocation)
	if err != nil {
		return fmt.Errorf("failed to save extended allocation: %w", err)
	}

	return nil
}

// AddGasToPool adds gas to the pool (for testing or manual operations)
func (s *Service) AddGasToPool(ctx context.Context, amount *big.Int) error {
	if amount == nil || amount.Sign() <= 0 {
		return errors.New("invalid amount")
	}

	return s.poolManager.AddGas(ctx, amount)
}

// GetAllAllocations returns all active gas allocations
func (s *Service) GetAllAllocations(ctx context.Context) ([]*models.Allocation, error) {
	return s.store.GetAllAllocations(ctx)
}

// Close closes the service and its resources
func (s *Service) Close() error {
	if closer, ok := s.store.(store.Closer); ok {
		return closer.Close()
	}
	return nil
}

// ReleaseAllocation releases a user's gas allocation
func (s *Service) ReleaseAllocation(ctx context.Context, userAddress util.Uint160) error {
	// Get current allocation
	allocation, err := s.allocationManager.GetAllocation(ctx, userAddress)
	if err != nil {
		return fmt.Errorf("failed to get allocation: %w", err)
	}

	if allocation == nil {
		return fmt.Errorf("no active allocation found for user")
	}

	// Return gas to pool
	remaining := allocation.RemainingGas()
	if remaining.Sign() > 0 {
		err = s.poolManager.ReturnGas(ctx, remaining)
		if err != nil {
			return fmt.Errorf("failed to return gas to pool: %w", err)
		}
	}

	// Release the allocation
	err = s.allocationManager.ReleaseGas(ctx, userAddress)
	if err != nil {
		return fmt.Errorf("failed to release allocation: %w", err)
	}

	return nil
}

// RequestAllocation requests a new gas allocation for a user
func (s *Service) RequestAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*models.Allocation, error) {
	// Check if user already has an allocation
	existing, err := s.allocationManager.GetAllocation(ctx, userAddress)
	if err != nil {
		return nil, fmt.Errorf("failed to check existing allocation: %w", err)
	}
	if existing != nil {
		return nil, fmt.Errorf("user already has an active allocation")
	}

	// Check available gas in pool
	available, err := s.poolManager.GetAvailableGas(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to get available gas: %w", err)
	}

	if available.Cmp(amount) < 0 {
		return nil, fmt.Errorf("insufficient gas in pool: available=%s, requested=%s",
			available.String(), amount.String())
	}

	// Create allocation
	allocation, err := s.allocationManager.AllocateGas(ctx, userAddress, amount)
	if err != nil {
		return nil, fmt.Errorf("failed to create allocation: %w", err)
	}

	// Consume gas from pool
	err = s.poolManager.ConsumeGas(ctx, amount)
	if err != nil {
		// Attempt to rollback allocation
		_ = s.allocationManager.ReleaseGas(ctx, userAddress)
		return nil, fmt.Errorf("failed to consume gas from pool: %w", err)
	}

	return allocation, nil
}

// Start starts the gas bank service
func (s *Service) Start(ctx context.Context) error {
	// Initialize components
	if err := s.poolManager.Start(ctx); err != nil {
		return fmt.Errorf("failed to start pool manager: %w", err)
	}

	if err := s.allocationManager.Start(ctx); err != nil {
		// Try to stop pool manager if allocation manager fails
		_ = s.poolManager.Stop(ctx)
		return fmt.Errorf("failed to start allocation manager: %w", err)
	}

	if err := s.billingManager.Start(ctx); err != nil {
		// Try to stop other components if billing manager fails
		_ = s.allocationManager.Stop(ctx)
		_ = s.poolManager.Stop(ctx)
		return fmt.Errorf("failed to start billing manager: %w", err)
	}

	return nil
}

// Stop stops the gas bank service
func (s *Service) Stop(ctx context.Context) error {
	// Stop components in reverse order
	if err := s.billingManager.Stop(ctx); err != nil {
		return fmt.Errorf("failed to stop billing manager: %w", err)
	}

	if err := s.allocationManager.Stop(ctx); err != nil {
		return fmt.Errorf("failed to stop allocation manager: %w", err)
	}

	if err := s.poolManager.Stop(ctx); err != nil {
		return fmt.Errorf("failed to stop pool manager: %w", err)
	}

	return nil
}
