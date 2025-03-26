package automation

import (
	"context"
	"fmt"
	"math/big"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/core/neo"
	"github.com/will/neo_service_layer/internal/services/gasbank"
)

// ExecutionStrategy represents a strategy for upkeep execution
type ExecutionStrategy interface {
	Execute(ctx context.Context, upkeep *Upkeep, performData []byte) (*UpkeepPerformance, error)
}

// DirectExecutionStrategy executes upkeeps directly
type DirectExecutionStrategy struct {
	neoClient      *neo.Client
	gasBankService *gasbank.Service
}

// ConditionalExecutionStrategy executes upkeeps based on conditions
type ConditionalExecutionStrategy struct {
	neoClient      *neo.Client
	gasBankService *gasbank.Service
	conditionCheck func(ctx context.Context, upkeep *Upkeep) (bool, error)
}

// Executor handles upkeep execution
type Executor struct {
	neoClient        *neo.Client
	gasBankService   *gasbank.Service
	strategies       map[string]ExecutionStrategy
	retryDelay       time.Duration
	maxRetries       int
	activeExecutions map[string]bool
	mu               sync.RWMutex
}

// NewExecutor creates a new Executor
func NewExecutor(neoClient *neo.Client, gasBankService *gasbank.Service, retryDelay time.Duration, maxRetries int) *Executor {
	// Create default strategies
	directStrategy := &DirectExecutionStrategy{
		neoClient:      neoClient,
		gasBankService: gasBankService,
	}

	executor := &Executor{
		neoClient:        neoClient,
		gasBankService:   gasBankService,
		strategies:       make(map[string]ExecutionStrategy),
		retryDelay:       retryDelay,
		maxRetries:       maxRetries,
		activeExecutions: make(map[string]bool),
	}

	// Register the default strategy
	executor.RegisterStrategy("direct", directStrategy)

	return executor
}

// RegisterStrategy registers an execution strategy
func (e *Executor) RegisterStrategy(name string, strategy ExecutionStrategy) {
	e.strategies[name] = strategy
}

// ExecuteUpkeep executes an upkeep
func (e *Executor) ExecuteUpkeep(ctx context.Context, upkeep *Upkeep, performData []byte, strategyName string) (*UpkeepPerformance, error) {
	e.mu.Lock()
	if e.activeExecutions[upkeep.ID] {
		e.mu.Unlock()
		return nil, fmt.Errorf("upkeep already being executed: %s", upkeep.ID)
	}
	e.activeExecutions[upkeep.ID] = true
	e.mu.Unlock()

	defer func() {
		e.mu.Lock()
		delete(e.activeExecutions, upkeep.ID)
		e.mu.Unlock()
	}()

	// Get the strategy
	strategy, exists := e.strategies[strategyName]
	if !exists {
		strategy = e.strategies["direct"] // fall back to direct strategy
	}

	startTime := time.Now()

	// Allocate gas for the execution
	userAddress := upkeep.Owner
	gasNeeded := big.NewInt(upkeep.ExecuteGas)
	_, err := e.gasBankService.AllocateGas(ctx, userAddress, gasNeeded)
	if err != nil {
		return &UpkeepPerformance{
			ID:        uuid.New().String(),
			UpkeepID:  upkeep.ID,
			StartTime: startTime,
			EndTime:   time.Now(),
			Status:    "failed",
			GasUsed:   0,
			Result:    "",
			Error:     fmt.Sprintf("failed to allocate gas: %v", err),
		}, err
	}

	// Execute the upkeep with retries
	var performance *UpkeepPerformance
	var lastErr error

	for attempt := 0; attempt <= e.maxRetries; attempt++ {
		performance, err = strategy.Execute(ctx, upkeep, performData)
		if err == nil {
			break
		}

		lastErr = err
		if attempt < e.maxRetries {
			// Wait before retrying
			select {
			case <-time.After(e.retryDelay):
				// Continue with retry
			case <-ctx.Done():
				// Context cancelled
				return &UpkeepPerformance{
					ID:        uuid.New().String(),
					UpkeepID:  upkeep.ID,
					StartTime: startTime,
					EndTime:   time.Now(),
					Status:    "cancelled",
					GasUsed:   0,
					Result:    "",
					Error:     "execution cancelled",
				}, ctx.Err()
			}
		}
	}

	// If execution failed after all retries, create a failed performance record
	if performance == nil {
		performance = &UpkeepPerformance{
			ID:        uuid.New().String(),
			UpkeepID:  upkeep.ID,
			StartTime: startTime,
			EndTime:   time.Now(),
			Status:    "failed",
			GasUsed:   0,
			Result:    "",
			Error:     fmt.Sprintf("failed after %d attempts: %v", e.maxRetries+1, lastErr),
		}
	}

	// Record gas usage
	if performance.Status == "success" {
		err = e.gasBankService.UseGas(ctx, userAddress, big.NewInt(performance.GasUsed))
		if err != nil {
			// Log the error but don't fail the operation
			performance.Error = fmt.Sprintf("failed to record gas usage: %v", err)
		}
	} else {
		// Release allocated gas if the execution failed
		e.gasBankService.ReleaseGas(ctx, userAddress)
	}

	return performance, nil
}

// IsActive checks if an upkeep is being executed
func (e *Executor) IsActive(upkeepID string) bool {
	e.mu.RLock()
	defer e.mu.RUnlock()
	return e.activeExecutions[upkeepID]
}

// Execute for DirectExecutionStrategy
func (s *DirectExecutionStrategy) Execute(ctx context.Context, upkeep *Upkeep, performData []byte) (*UpkeepPerformance, error) {
	startTime := time.Now()

	// Prepare transaction parameters
	contractAddress := upkeep.TargetContract
	method := upkeep.UpkeepFunction
	// params would be used in a real implementation to call the contract
	// params := []interface{}{
	//	performData, // pass the perform data as a parameter
	// }

	// In a real implementation, we would call the contract
	// For now, simulate with a mock response
	txHash := util.Uint256{1, 2, 3, 4, 5}
	blockNumber := uint32(12345)

	// Record the end time
	endTime := time.Now()

	// Create performance record
	performance := &UpkeepPerformance{
		ID:              uuid.New().String(),
		UpkeepID:        upkeep.ID,
		StartTime:       startTime,
		EndTime:         endTime,
		Status:          "success",
		GasUsed:         upkeep.ExecuteGas,
		BlockNumber:     blockNumber,
		TransactionHash: txHash,
		Result:          fmt.Sprintf("Successfully called %s on contract %s", method, contractAddress.StringLE()),
	}

	return performance, nil
}
