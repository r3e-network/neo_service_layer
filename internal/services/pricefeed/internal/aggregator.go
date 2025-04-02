package internal

import (
	"context"
	"fmt"
	"sync"
	"time"

	log "github.com/sirupsen/logrus"                      // Added for logging
	"github.com/will/neo_service_layer/internal/core/neo" // Assuming this is the correct client package
	"github.com/will/neo_service_layer/internal/services/pricefeed/models"
)

// PriceAggregatorImpl implements the PriceAggregator interface
type PriceAggregatorImpl struct {
	store          PriceStore
	metrics        PriceMetricsCollector
	alerts         PriceAlertManager
	validator      PriceValidator
	policy         *models.PriceUpdatePolicy
	neoClient      *neo.Client     // Use the specific type
	providers      []PriceProvider // Added: List of price providers
	assets         []string        // Added: List of assets to monitor
	updateInterval time.Duration   // Added: Interval for fetching prices

	subscribers sync.Map // map[string][]chan *models.Price
	mu          sync.RWMutex
	ticker      *time.Ticker  // Added: Ticker for periodic updates
	stopChan    chan struct{} // Added: Channel to signal stop
}

// NewPriceAggregator creates a new PriceAggregator instance
func NewPriceAggregator(store PriceStore, metrics PriceMetricsCollector, alerts PriceAlertManager, validator PriceValidator, policy *models.PriceUpdatePolicy, neoClient *neo.Client, providers []PriceProvider, assets []string, updateInterval time.Duration) PriceAggregator {
	return &PriceAggregatorImpl{
		store:          store,
		metrics:        metrics,
		alerts:         alerts,
		validator:      validator,
		policy:         policy,
		neoClient:      neoClient,
		providers:      providers,           // Added
		assets:         assets,              // Added
		updateInterval: updateInterval,      // Added
		stopChan:       make(chan struct{}), // Added
	}
}

// PublishPrice is likely deprecated if aggregator fetches prices itself
// Kept for now, maybe used for manual override?
// func (pa *PriceAggregatorImpl) PublishPrice(ctx context.Context, assetID string, price *big.Float, timestamp time.Time) error {
// 	// ... existing PublishPrice logic ...
// }

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

// Start starts the price aggregator, including the periodic update loop
func (pa *PriceAggregatorImpl) Start(ctx context.Context) error {
	log.Info("Starting Price Aggregator...")
	if pa.updateInterval <= 0 {
		log.Warn("Price Aggregator update interval is not configured, periodic updates disabled.")
		return nil // Or return an error if periodic updates are mandatory
	}

	pa.ticker = time.NewTicker(pa.updateInterval)
	go pa.runUpdateLoop(ctx) // Start the update loop in a goroutine

	log.Info("Price Aggregator started.")
	return nil
}

// runUpdateLoop is the main loop for fetching, aggregating, and publishing prices
func (pa *PriceAggregatorImpl) runUpdateLoop(ctx context.Context) {
	log.Infof("Price Aggregator update loop running every %s", pa.updateInterval)
	for {
		select {
		case <-pa.ticker.C:
			log.Debug("Price Aggregator tick - starting update cycle")
			pa.updatePrices(ctx)
		case <-pa.stopChan:
			log.Info("Price Aggregator update loop stopping.")
			pa.ticker.Stop()
			return
		case <-ctx.Done():
			log.Info("Price Aggregator context cancelled, stopping update loop.")
			pa.ticker.Stop()
			return
		}
	}
}

// updatePrices fetches prices from providers, aggregates, and publishes
func (pa *PriceAggregatorImpl) updatePrices(ctx context.Context) {
	for _, assetID := range pa.assets {
		log.Debugf("Updating price for asset: %s", assetID)
		// Fetch prices from all providers for this asset
		providerPrices := pa.fetchPricesFromProviders(ctx, assetID)

		// Aggregate the prices (implement actual aggregation logic)
		aggregatedPrice, err := pa.aggregatePrices(ctx, assetID, providerPrices)
		if err != nil {
			log.Errorf("Failed to aggregate price for %s: %v", assetID, err)
			pa.metrics.RecordFailedUpdate(ctx, assetID, fmt.Sprintf("aggregation failed: %v", err))
			continue // Move to the next asset
		}

		if aggregatedPrice == nil {
			log.Warnf("No aggregated price could be determined for %s", assetID)
			pa.metrics.RecordFailedUpdate(ctx, assetID, "no aggregated price determined")
			continue // Move to the next asset
		}

		// Validate the aggregated price (already done partially by aggregation? Add more checks?)
		if err := pa.validator.ValidatePrice(ctx, aggregatedPrice); err != nil {
			log.Errorf("Aggregated price for %s failed validation: %v", assetID, err)
			pa.metrics.RecordFailedUpdate(ctx, assetID, fmt.Sprintf("validation failed: %v", err))
			continue
		}

		// Check deviation from the last stored price
		oldPrice, _ := pa.store.GetPrice(ctx, assetID)
		if oldPrice != nil && oldPrice.Price.Cmp(aggregatedPrice.Price) != 0 {
			pa.alerts.AlertPriceDeviation(ctx, assetID, oldPrice.Price, aggregatedPrice.Price)
		}

		// Save the aggregated price to the store
		if err := pa.store.SavePrice(ctx, aggregatedPrice); err != nil {
			log.Errorf("Failed to save aggregated price for %s: %v", assetID, err)
			// Continue to attempt blockchain update even if store fails?
		}

		// Publish the price to the blockchain (implement blockchain update logic)
		if err := pa.publishToBlockchain(ctx, aggregatedPrice); err != nil {
			log.Errorf("Failed to publish price for %s to blockchain: %v", assetID, err)
			pa.metrics.RecordFailedUpdate(ctx, assetID, fmt.Sprintf("blockchain publish failed: %v", err))
			// Don't continue here? Price was saved locally, maybe retry blockchain later?
		}

		// Notify subscribers
		pa.notifySubscribers(ctx, assetID, aggregatedPrice)

		// Record metrics (assuming latency is measured elsewhere or approximated)
		pa.metrics.RecordUpdate(ctx, aggregatedPrice, 0) // Placeholder for latency
	}
}

// fetchPricesFromProviders fetches prices for a specific asset from all configured providers
func (pa *PriceAggregatorImpl) fetchPricesFromProviders(ctx context.Context, assetID string) []*models.Price {
	var prices []*models.Price
	var wg sync.WaitGroup
	var mu sync.Mutex

	for _, provider := range pa.providers {
		wg.Add(1)
		go func(p PriceProvider) {
			defer wg.Done()
			price, err := p.GetPrice(ctx, assetID)
			if err != nil {
				log.Warnf("Failed to get price for %s from provider %s: %v", assetID, p.Name(), err)
				pa.alerts.AlertDataSourceFailure(ctx, p.Name(), err.Error())
				pa.metrics.UpdateDataSourceHealth(ctx, p.Name(), 0.0) // Mark as unhealthy
				return
			}
			if price != nil {
				price.Source = p.Name() // Ensure source is set correctly
				mu.Lock()
				prices = append(prices, price)
				mu.Unlock()
				pa.metrics.UpdateDataSourceHealth(ctx, p.Name(), 1.0) // Mark as healthy
			}
		}(provider)
	}

	wg.Wait()
	return prices
}

// aggregatePrices calculates the final price from provider data (Placeholder Implementation)
func (pa *PriceAggregatorImpl) aggregatePrices(ctx context.Context, assetID string, providerPrices []*models.Price) (*models.Price, error) {
	if len(providerPrices) == 0 {
		return nil, fmt.Errorf("no prices received from providers")
	}

	// --- Placeholder Aggregation Logic ---
	// Replace with actual weighted average, outlier detection, confidence calculation based on design doc
	// For now, just take the first valid price.
	finalPrice := providerPrices[0]
	finalPrice.Source = "aggregated"  // Mark source as aggregated
	finalPrice.Confidence = 0.75      // Placeholder confidence
	finalPrice.Timestamp = time.Now() // Use current time for aggregated price timestamp
	// --- End Placeholder ---

	log.Debugf("Aggregated price for %s: %s (Confidence: %.2f)", assetID, finalPrice.Price.String(), finalPrice.Confidence)
	return finalPrice, nil
}

// publishToBlockchain sends the aggregated price to the Neo contract (Placeholder Implementation)
func (pa *PriceAggregatorImpl) publishToBlockchain(ctx context.Context, price *models.Price) error {
	// --- Placeholder Blockchain Update Logic ---
	// 1. Get contract script hash from config/policy
	// 2. Format assetID and price.Price according to contract expectations (e.g., integer with decimals)
	// 3. Use pa.neoClient to build and send the transaction invoking the contract update method
	// 4. Handle transaction submission, confirmation, and potential errors.
	log.Infof("[Placeholder] Publishing %s price %s to Neo blockchain...", price.AssetID, price.Price.String())
	// Example (needs actual implementation):
	// contractHash := pa.policy.ContractHash // Assuming contract hash is in policy or config
	// methodName := "updatePrice"
	// args := []interface{}{price.AssetID, price.Price.Int64()} // Adjust based on contract param types
	// txid, vheight, err := pa.neoClient.InvokeFunction(contractHash, methodName, args)
	// if err != nil { return err }
	// log.Infof("Blockchain update successful for %s, TxID: %s", price.AssetID, txid)
	// --- End Placeholder ---

	return nil // Return actual error if publication fails
}

// Stop stops the price aggregator, including the periodic update loop
func (pa *PriceAggregatorImpl) Stop(ctx context.Context) error {
	log.Info("Stopping Price Aggregator...")
	if pa.ticker != nil {
		close(pa.stopChan) // Signal the update loop to stop
	}

	// Clean up subscribers (existing logic)
	pa.mu.Lock()
	pa.subscribers.Range(func(key, value interface{}) bool {
		subscribers := value.([]chan *models.Price)
		for _, ch := range subscribers {
			close(ch)
		}
		pa.subscribers.Delete(key)
		return true
	})
	pa.mu.Unlock()
	log.Info("Price Aggregator stopped.")
	return nil
}
