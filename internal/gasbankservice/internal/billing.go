package internal

import (
	"context"
	"math/big"
	"sync"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// BasicBillingManager implements a basic billing manager
type BasicBillingManager struct {
	mu         sync.RWMutex
	feeBalance *big.Int
	feeRate    *big.Int
	minFee     *big.Int
	maxFee     *big.Int
	totalFees  *big.Int
	feeHistory map[util.Uint160][]*FeeRecord
}

// FeeRecord represents a fee charge record
type FeeRecord struct {
	UserAddress util.Uint160
	Amount      *big.Int
	Timestamp   int64
}

// NewBasicBillingManager creates a new basic billing manager
func NewBasicBillingManager() *BasicBillingManager {
	return &BasicBillingManager{
		feeBalance: big.NewInt(0),
		feeRate:    big.NewInt(1), // 1% fee rate
		minFee:     big.NewInt(100000),
		maxFee:     big.NewInt(1000000000),
		totalFees:  big.NewInt(0),
		feeHistory: make(map[util.Uint160][]*FeeRecord),
	}
}

// Start starts the billing manager
func (bm *BasicBillingManager) Start(ctx context.Context) error {
	return nil
}

// Stop stops the billing manager
func (bm *BasicBillingManager) Stop(ctx context.Context) error {
	return nil
}

// ChargeFee charges a fee for gas usage
func (bm *BasicBillingManager) ChargeFee(ctx context.Context, userAddress util.Uint160, amount *big.Int) error {
	bm.mu.Lock()
	defer bm.mu.Unlock()

	// Calculate fee based on amount and rate
	fee := new(big.Int).Mul(amount, bm.feeRate)
	fee.Div(fee, big.NewInt(100)) // Convert percentage to actual fee

	// Apply min/max fee constraints
	if fee.Cmp(bm.minFee) < 0 {
		fee = new(big.Int).Set(bm.minFee)
	} else if fee.Cmp(bm.maxFee) > 0 {
		fee = new(big.Int).Set(bm.maxFee)
	}

	// Add fee to balance
	bm.feeBalance.Add(bm.feeBalance, fee)
	bm.totalFees.Add(bm.totalFees, fee)

	// Record fee charge
	record := &FeeRecord{
		UserAddress: userAddress,
		Amount:      new(big.Int).Set(fee),
		Timestamp:   ctx.Value("timestamp").(int64),
	}

	bm.feeHistory[userAddress] = append(bm.feeHistory[userAddress], record)

	return nil
}

// GetFeeBalance gets the total fees collected
func (bm *BasicBillingManager) GetFeeBalance(ctx context.Context) (*big.Int, error) {
	bm.mu.RLock()
	defer bm.mu.RUnlock()
	return new(big.Int).Set(bm.feeBalance), nil
}
