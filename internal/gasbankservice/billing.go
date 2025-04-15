package gasbank

import (
	"context"
	"fmt"
	"math/big"
	"sync"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// BillingRecord represents a gas usage billing record
type BillingRecord struct {
	UserAddress  util.Uint160
	GasUsed     *big.Int
	Cost        *big.Int
	Timestamp   time.Time
	Description string
}

// BillingManager manages gas usage billing
type BillingManager struct {
	records   sync.Map // map[string][]*BillingRecord
	gasPrice  *big.Int
	mu        sync.RWMutex
}

// NewBillingManager creates a new billing manager
func NewBillingManager(initialGasPrice *big.Int) *BillingManager {
	return &BillingManager{
		gasPrice: initialGasPrice,
	}
}

// RecordUsage records gas usage for billing
func (bm *BillingManager) RecordUsage(ctx context.Context, userAddress util.Uint160, gasUsed *big.Int, description string) error {
	if gasUsed.Sign() <= 0 {
		return fmt.Errorf("invalid gas usage: must be positive")
	}

	record := &BillingRecord{
		UserAddress:  userAddress,
		GasUsed:     gasUsed,
		Cost:        new(big.Int).Mul(gasUsed, bm.gasPrice),
		Timestamp:   time.Now(),
		Description: description,
	}

	key := userAddress.String()
	value, _ := bm.records.LoadOrStore(key, []*BillingRecord{})
	records := value.([]*BillingRecord)
	records = append(records, record)
	bm.records.Store(key, records)

	return nil
}

// GetUserRecords gets billing records for a user
func (bm *BillingManager) GetUserRecords(ctx context.Context, userAddress util.Uint160, start, end time.Time) ([]*BillingRecord, error) {
	value, ok := bm.records.Load(userAddress.String())
	if !ok {
		return nil, nil
	}

	records := value.([]*BillingRecord)
	var filtered []*BillingRecord
	for _, record := range records {
		if record.Timestamp.After(start) && record.Timestamp.Before(end) {
			filtered = append(filtered, record)
		}
	}

	return filtered, nil
}

// GetTotalCost gets the total cost for a user within a time range
func (bm *BillingManager) GetTotalCost(ctx context.Context, userAddress util.Uint160, start, end time.Time) (*big.Int, error) {
	records, err := bm.GetUserRecords(ctx, userAddress, start, end)
	if err != nil {
		return nil, err
	}

	total := big.NewInt(0)
	for _, record := range records {
		total.Add(total, record.Cost)
	}

	return total, nil
}

// UpdateGasPrice updates the gas price
func (bm *BillingManager) UpdateGasPrice(newPrice *big.Int) {
	bm.mu.Lock()
	defer bm.mu.Unlock()
	bm.gasPrice = newPrice
}

// GetGasPrice gets the current gas price
func (bm *BillingManager) GetGasPrice() *big.Int {
	bm.mu.RLock()
	defer bm.mu.RUnlock()
	return new(big.Int).Set(bm.gasPrice)
}

// CleanupOldRecords removes records older than the specified duration
func (bm *BillingManager) CleanupOldRecords(age time.Duration) {
	cutoff := time.Now().Add(-age)
	bm.records.Range(func(key, value interface{}) bool {
		records := value.([]*BillingRecord)
		var filtered []*BillingRecord
		for _, record := range records {
			if record.Timestamp.After(cutoff) {
				filtered = append(filtered, record)
			}
		}
		if len(filtered) > 0 {
			bm.records.Store(key, filtered)
		} else {
			bm.records.Delete(key)
		}
		return true
	})
}