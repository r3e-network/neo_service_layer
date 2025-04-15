package internal

import (
	"context"
	"fmt"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/pricefeedservice/models"
)

// PriceValidatorImpl implements the PriceValidator interface
type PriceValidatorImpl struct {
	store  PriceStore
	policy *models.PriceUpdatePolicy
}

// NewPriceValidator creates a new PriceValidator instance
func NewPriceValidator() PriceValidator {
	return &PriceValidatorImpl{}
}

// ValidatePrice validates a price update
func (pv *PriceValidatorImpl) ValidatePrice(ctx context.Context, price *models.Price) error {
	if price == nil {
		return fmt.Errorf("price cannot be nil")
	}

	if price.AssetID == "" {
		return fmt.Errorf("asset ID cannot be empty")
	}

	if price.Price == nil {
		return fmt.Errorf("price value cannot be nil")
	}

	if price.Price.Sign() < 0 {
		return fmt.Errorf("price value cannot be negative")
	}

	if price.Timestamp.IsZero() {
		return fmt.Errorf("timestamp cannot be zero")
	}

	if price.Timestamp.After(time.Now()) {
		return fmt.Errorf("timestamp cannot be in the future")
	}

	return nil
}

// ValidateUpdateInterval validates the update interval for a price
func (pv *PriceValidatorImpl) ValidateUpdateInterval(ctx context.Context, assetID string, timestamp time.Time) error {
	if pv.policy == nil {
		return nil
	}

	lastPrice, err := pv.store.GetPrice(ctx, assetID)
	if err != nil {
		return fmt.Errorf("failed to get last price: %w", err)
	}

	if lastPrice != nil {
		timeSinceLastUpdate := timestamp.Sub(lastPrice.Timestamp)
		if timeSinceLastUpdate < pv.policy.MinUpdateInterval {
			return fmt.Errorf("update interval too short: minimum %v, got %v", pv.policy.MinUpdateInterval, timeSinceLastUpdate)
		}
	}

	return nil
}

// ValidateDataSources validates the data sources for an asset
func (pv *PriceValidatorImpl) ValidateDataSources(ctx context.Context, assetID string) error {
	if pv.policy == nil {
		return nil
	}

	// Implementation would check if enough data sources are available and healthy
	return nil
}

// SetPolicy sets the price update policy
func (pv *PriceValidatorImpl) SetPolicy(policy *models.PriceUpdatePolicy) {
	pv.policy = policy
}

// SetStore sets the price store
func (pv *PriceValidatorImpl) SetStore(store PriceStore) {
	pv.store = store
}
