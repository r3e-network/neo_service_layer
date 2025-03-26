package automation

import (
	"context"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/core/neo"
)

// Keeper manages upkeeps
type Keeper struct {
	neoClient         *neo.Client
	registryAddress   util.Uint160
	upkeeps           map[string]*Upkeep
	checkFrequency    time.Duration
	checkTimeout      time.Duration
	performLock       sync.Mutex
	activePerformance map[string]bool
	checkResults      map[string]*UpkeepCheck
}

// NewKeeper creates a new Keeper
func NewKeeper(neoClient *neo.Client, registryAddress util.Uint160, checkFrequency time.Duration) *Keeper {
	return &Keeper{
		neoClient:         neoClient,
		registryAddress:   registryAddress,
		upkeeps:           make(map[string]*Upkeep),
		checkFrequency:    checkFrequency,
		checkTimeout:      time.Second * 30,
		activePerformance: make(map[string]bool),
		checkResults:      make(map[string]*UpkeepCheck),
	}
}

// RegisterUpkeep registers a new upkeep with the keeper
func (k *Keeper) RegisterUpkeep(upkeep *Upkeep) error {
	if upkeep.ID == "" {
		upkeep.ID = uuid.New().String()
	}

	k.upkeeps[upkeep.ID] = upkeep
	return nil
}

// RemoveUpkeep removes an upkeep from the keeper
func (k *Keeper) RemoveUpkeep(upkeepID string) {
	delete(k.upkeeps, upkeepID)
}

// CheckUpkeep checks if an upkeep is eligible for execution
func (k *Keeper) CheckUpkeep(ctx context.Context, upkeepID string) (*UpkeepCheck, error) {
	upkeep, exists := k.upkeeps[upkeepID]
	if !exists {
		return nil, nil
	}

	// For production, this would call the contract's checkUpkeep method
	// For now we'll just return a mock result
	check := &UpkeepCheck{
		UpkeepID:      upkeepID,
		CheckTime:     time.Now(),
		Eligible:      true,
		PerformData:   []byte("check-result"),
		GasEstimation: upkeep.ExecuteGas,
	}

	// Store the check result
	k.checkResults[upkeepID] = check

	return check, nil
}

// PerformUpkeep performs an upkeep
func (k *Keeper) PerformUpkeep(ctx context.Context, upkeepID string, performData []byte) (*UpkeepPerformance, error) {
	k.performLock.Lock()
	defer k.performLock.Unlock()

	// Check if there's already a performance in progress
	if k.activePerformance[upkeepID] {
		return nil, nil
	}

	upkeep, exists := k.upkeeps[upkeepID]
	if !exists {
		return nil, nil
	}

	// Mark as active
	k.activePerformance[upkeepID] = true
	defer func() {
		delete(k.activePerformance, upkeepID)
	}()

	// For production, this would call the contract's performUpkeep method
	// For now we'll just return a mock result
	result := &UpkeepPerformance{
		ID:              uuid.New().String(),
		UpkeepID:        upkeepID,
		StartTime:       time.Now(),
		EndTime:         time.Now().Add(time.Second),
		Status:          "success",
		GasUsed:         upkeep.ExecuteGas,
		BlockNumber:     12345,
		TransactionHash: util.Uint256{1, 2, 3},
		Result:          "Success",
	}

	// Update the upkeep
	upkeep.LastRunAt = time.Now()
	upkeep.NextEligibleAt = time.Now().Add(time.Minute)

	return result, nil
}

// GetAllUpkeeps returns all upkeeps
func (k *Keeper) GetAllUpkeeps() []*Upkeep {
	upkeeps := make([]*Upkeep, 0, len(k.upkeeps))
	for _, upkeep := range k.upkeeps {
		upkeeps = append(upkeeps, upkeep)
	}
	return upkeeps
}

// GetUpkeepsForUser returns upkeeps for a user
func (k *Keeper) GetUpkeepsForUser(userAddress util.Uint160) []*Upkeep {
	upkeeps := make([]*Upkeep, 0)
	for _, upkeep := range k.upkeeps {
		if upkeep.Owner == userAddress {
			upkeeps = append(upkeeps, upkeep)
		}
	}
	return upkeeps
}

// GetEligibleUpkeeps returns upkeeps eligible for execution
func (k *Keeper) GetEligibleUpkeeps() []*Upkeep {
	now := time.Now()
	eligible := make([]*Upkeep, 0)

	for _, upkeep := range k.upkeeps {
		if upkeep.Status == "active" && now.After(upkeep.NextEligibleAt) {
			eligible = append(eligible, upkeep)
		}
	}

	return eligible
}
