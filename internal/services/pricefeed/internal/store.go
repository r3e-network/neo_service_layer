package internal

import (
	"context"
	"sync"
	"time"

	"github.com/will/neo_service_layer/internal/services/pricefeed/models"
)

// PriceStoreImpl implements the PriceStore interface
type PriceStoreImpl struct {
	prices sync.Map // map[string]*models.Price
	history sync.Map // map[string][]*models.Price
	mu sync.RWMutex
}

// NewPriceStore creates a new PriceStore instance
func NewPriceStore() PriceStore {
	return &PriceStoreImpl{}
}

// SavePrice saves a price to the store
func (ps *PriceStoreImpl) SavePrice(ctx context.Context, price *models.Price) error {
	ps.prices.Store(price.AssetID, price)

	ps.mu.Lock()
	defer ps.mu.Unlock()

	value, _ := ps.history.LoadOrStore(price.AssetID, []*models.Price{})
	history := value.([]*models.Price)
	history = append(history, price)
	ps.history.Store(price.AssetID, history)

	return nil
}

// GetPrice gets the current price for an asset
func (ps *PriceStoreImpl) GetPrice(ctx context.Context, assetID string) (*models.Price, error) {
	value, ok := ps.prices.Load(assetID)
	if !ok {
		return nil, nil
	}
	return value.(*models.Price), nil
}

// GetPriceHistory gets the price history for an asset
func (ps *PriceStoreImpl) GetPriceHistory(ctx context.Context, assetID string, start, end time.Time) ([]*models.Price, error) {
	value, ok := ps.history.Load(assetID)
	if !ok {
		return nil, nil
	}

	history := value.([]*models.Price)
	var filtered []*models.Price
	for _, price := range history {
		if price.Timestamp.After(start) && price.Timestamp.Before(end) {
			filtered = append(filtered, price)
		}
	}

	return filtered, nil
}

// DeletePrice deletes a price from the store
func (ps *PriceStoreImpl) DeletePrice(ctx context.Context, assetID string, timestamp time.Time) error {
	ps.prices.Delete(assetID)

	ps.mu.Lock()
	defer ps.mu.Unlock()

	value, ok := ps.history.Load(assetID)
	if !ok {
		return nil
	}

	history := value.([]*models.Price)
	var filtered []*models.Price
	for _, price := range history {
		if !price.Timestamp.Equal(timestamp) {
			filtered = append(filtered, price)
		}
	}

	if len(filtered) > 0 {
		ps.history.Store(assetID, filtered)
	} else {
		ps.history.Delete(assetID)
	}

	return nil
}