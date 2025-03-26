package internal

import (
	"context"
	"fmt"
	"math/big"
	"sync"
	"time"

	"github.com/will/neo_service_layer/internal/services/pricefeed/models"
)

// PriceAggregatorImpl implements the PriceAggregator interface
type PriceAggregatorImpl struct {
	store     PriceStore
	metrics   PriceMetricsCollector
	alerts    PriceAlertManager
	validator PriceValidator
	policy    *models.PriceUpdatePolicy
	neoClient interface{} // Replace with actual Neo client interface

	subscribers sync.Map // map[string][]chan *models.Price
	mu          sync.RWMutex
}

// NewPriceAggregator creates a new PriceAggregator instance
func NewPriceAggregator(store PriceStore, metrics PriceMetricsCollector, alerts PriceAlertManager, validator PriceValidator, policy *models.PriceUpdatePolicy, neoClient interface{}) PriceAggregator {
	return &PriceAggregatorImpl{
		store:     store,
		metrics:   metrics,
		alerts:    alerts,
		validator: validator,
		policy:    policy,
		neoClient: neoClient,
	}
}

// PublishPrice publishes a new price for an asset
func (pa *PriceAggregatorImpl) PublishPrice(ctx context.Context, assetID string, price *big.Float, timestamp time.Time) error {
	start := time.Now()

	newPrice := &models.Price{
		AssetID:    assetID,
		Price:      price,
		Timestamp:  timestamp,
		Source:     "oracle", // Replace with actual source
		Confidence: 1.0,      // Replace with actual confidence calculation
	}

	if err := pa.validator.ValidatePrice(ctx, newPrice); err != nil {
		pa.metrics.RecordFailedUpdate(ctx, assetID, err.Error())
		return fmt.Errorf("invalid price update: %w", err)
	}

	if err := pa.validator.ValidateUpdateInterval(ctx, assetID, timestamp); err != nil {
		pa.metrics.RecordFailedUpdate(ctx, assetID, err.Error())
		return fmt.Errorf("invalid update interval: %w", err)
	}

	oldPrice, _ := pa.store.GetPrice(ctx, assetID)
	if oldPrice != nil {
		oldValue := oldPrice.Price
		newValue := price
		if oldValue.Cmp(newValue) != 0 {
			pa.alerts.AlertPriceDeviation(ctx, assetID, oldValue, newValue)
		}
	}

	if err := pa.store.SavePrice(ctx, newPrice); err != nil {
		return fmt.Errorf("failed to save price: %w", err)
	}

	pa.notifySubscribers(ctx, assetID, newPrice)
	pa.metrics.RecordUpdate(ctx, newPrice, time.Since(start))
	return nil
}

// GetPrice gets the current price for an asset
func (pa *PriceAggregatorImpl) GetPrice(ctx context.Context, assetID string) (*models.Price, error) {
	price, err := pa.store.GetPrice(ctx, assetID)
	if err != nil {
		return nil, fmt.Errorf("failed to get price: %w", err)
	}

	if price == nil {
		return nil, fmt.Errorf("no price found for asset")
	}

	if time.Since(price.Timestamp) > pa.policy.MaxDataAge {
		pa.alerts.AlertStalePrice(ctx, assetID, price.Timestamp)
	}

	return price, nil
}

// GetPriceHistory gets the price history for an asset
func (pa *PriceAggregatorImpl) GetPriceHistory(ctx context.Context, assetID string, start, end time.Time) ([]*models.Price, error) {
	prices, err := pa.store.GetPriceHistory(ctx, assetID, start, end)
	if err != nil {
		return nil, fmt.Errorf("failed to get price history: %w", err)
	}

	return prices, nil
}

// SubscribePriceUpdates subscribes to price updates for an asset
func (pa *PriceAggregatorImpl) SubscribePriceUpdates(ctx context.Context, assetID string) (<-chan *models.Price, error) {
	pa.mu.Lock()
	defer pa.mu.Unlock()

	ch := make(chan *models.Price, 100)
	value, _ := pa.subscribers.LoadOrStore(assetID, []chan *models.Price{})
	subscribers := value.([]chan *models.Price)
	subscribers = append(subscribers, ch)
	pa.subscribers.Store(assetID, subscribers)

	go func() {
		<-ctx.Done()
		pa.unsubscribeChannel(assetID, ch)
	}()

	return ch, nil
}

// UnsubscribePriceUpdates unsubscribes from price updates for an asset
func (pa *PriceAggregatorImpl) UnsubscribePriceUpdates(ctx context.Context, assetID string) error {
	pa.mu.Lock()
	defer pa.mu.Unlock()

	pa.subscribers.Delete(assetID)
	return nil
}

func (pa *PriceAggregatorImpl) notifySubscribers(ctx context.Context, assetID string, price *models.Price) {
	value, ok := pa.subscribers.Load(assetID)
	if !ok {
		return
	}

	subscribers := value.([]chan *models.Price)
	for _, ch := range subscribers {
		select {
		case ch <- price:
		default:
			// Channel is full, skip this update
		}
	}
}

func (pa *PriceAggregatorImpl) unsubscribeChannel(assetID string, ch chan *models.Price) {
	pa.mu.Lock()
	defer pa.mu.Unlock()

	value, ok := pa.subscribers.Load(assetID)
	if !ok {
		return
	}

	subscribers := value.([]chan *models.Price)
	for i, sub := range subscribers {
		if sub == ch {
			subscribers = append(subscribers[:i], subscribers[i+1:]...)
			close(ch)
			break
		}
	}

	if len(subscribers) == 0 {
		pa.subscribers.Delete(assetID)
	} else {
		pa.subscribers.Store(assetID, subscribers)
	}
}

// Start starts the price aggregator
func (pa *PriceAggregatorImpl) Start(ctx context.Context) error {
	// Nothing to start for now
	return nil
}

// Stop stops the price aggregator
func (pa *PriceAggregatorImpl) Stop(ctx context.Context) error {
	// Clean up subscribers
	pa.mu.Lock()
	defer pa.mu.Unlock()

	pa.subscribers.Range(func(key, value interface{}) bool {
		subscribers := value.([]chan *models.Price)
		for _, ch := range subscribers {
			close(ch)
		}
		pa.subscribers.Delete(key)
		return true
	})

	return nil
}
