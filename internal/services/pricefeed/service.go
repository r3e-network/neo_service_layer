package pricefeed

import (
	"context"
	"fmt"
	"math/big"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/core/neo"
	"github.com/will/neo_service_layer/internal/services/pricefeed/internal"
	"github.com/will/neo_service_layer/internal/services/pricefeed/models"
)

// Config represents the price feed service configuration
type Config struct {
	UpdateInterval time.Duration
	PriceContract  util.Uint160
}

// Service represents the price feed service
type Service struct {
	aggregator internal.PriceAggregator
	store      internal.PriceStore
	metrics    internal.PriceMetricsCollector
	alerts     internal.PriceAlertManager
	validator  internal.PriceValidator
	neoClient  *neo.Client
}

// NewService creates a new price feed service
func NewService(config *Config, neoClient *neo.Client) (*Service, error) {
	store := internal.NewPriceStore()
	metrics := internal.NewPriceMetricsCollector()
	alerts := internal.NewPriceAlertManager()
	validator := internal.NewPriceValidator()

	policy := &models.PriceUpdatePolicy{
		MinUpdateInterval: config.UpdateInterval,
		MaxPriceDeviation: big.NewFloat(0.1), // 10% max deviation
		MinDataSources:    3,
		MaxDataAge:        time.Minute * 5,
	}

	aggregator := internal.NewPriceAggregator(store, metrics, alerts, validator, policy, neoClient)

	return &Service{
		aggregator: aggregator,
		store:      store,
		metrics:    metrics,
		alerts:     alerts,
		validator:  validator,
		neoClient:  neoClient,
	}, nil
}

// PublishPrice publishes a new price for an asset
func (s *Service) PublishPrice(ctx context.Context, assetID string, price *big.Float, timestamp time.Time) error {
	return s.aggregator.PublishPrice(ctx, assetID, price, timestamp)
}

// GetPrice gets the current price for an asset
func (s *Service) GetPrice(ctx context.Context, assetID string) (*models.Price, error) {
	return s.aggregator.GetPrice(ctx, assetID)
}

// GetPriceHistory gets the price history for an asset
func (s *Service) GetPriceHistory(ctx context.Context, assetID string, start, end time.Time) ([]*models.Price, error) {
	return s.aggregator.GetPriceHistory(ctx, assetID, start, end)
}

// SubscribePriceUpdates subscribes to price updates for an asset
func (s *Service) SubscribePriceUpdates(ctx context.Context, assetID string) (<-chan *models.Price, error) {
	return s.aggregator.SubscribePriceUpdates(ctx, assetID)
}

// UnsubscribePriceUpdates unsubscribes from price updates for an asset
func (s *Service) UnsubscribePriceUpdates(ctx context.Context, assetID string) error {
	return s.aggregator.UnsubscribePriceUpdates(ctx, assetID)
}

// Start starts the price feed service
func (s *Service) Start(ctx context.Context) error {
	// Start the price aggregator
	if err := s.aggregator.Start(ctx); err != nil {
		return fmt.Errorf("failed to start price aggregator: %w", err)
	}

	// Start metrics collection
	if err := s.metrics.Start(ctx); err != nil {
		return fmt.Errorf("failed to start metrics collector: %w", err)
	}

	// Start alert monitoring
	if err := s.alerts.Start(ctx); err != nil {
		return fmt.Errorf("failed to start alert manager: %w", err)
	}

	return nil
}

// Stop stops the price feed service
func (s *Service) Stop(ctx context.Context) error {
	// Stop the price aggregator
	if err := s.aggregator.Stop(ctx); err != nil {
		return fmt.Errorf("failed to stop price aggregator: %w", err)
	}

	// Stop metrics collection
	if err := s.metrics.Stop(ctx); err != nil {
		return fmt.Errorf("failed to stop metrics collector: %w", err)
	}

	// Stop alert monitoring
	if err := s.alerts.Stop(ctx); err != nil {
		return fmt.Errorf("failed to stop alert manager: %w", err)
	}

	return nil
}
