package internal

import (
	"context"
	"math/big"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/pricefeedservice/models"
)

// PriceProvider defines the interface for external price data sources
type PriceProvider interface {
	// GetPrice fetches the current price for a single asset from the provider
	GetPrice(ctx context.Context, assetID string) (*models.Price, error) // Assuming providers can return models.Price directly or adapt
	// Name returns the name of the provider (e.g., "binance", "coinbase")
	Name() string
}

// PriceAggregator aggregates and manages price data
type PriceAggregator interface {
	// Start starts the aggregator
	Start(ctx context.Context) error

	// Stop stops the aggregator
	Stop(ctx context.Context) error

	PublishPrice(ctx context.Context, assetID string, price *big.Float, timestamp time.Time) error
	GetPrice(ctx context.Context, assetID string) (*models.Price, error)
	GetPriceHistory(ctx context.Context, assetID string, start, end time.Time) ([]*models.Price, error)
	SubscribePriceUpdates(ctx context.Context, assetID string) (<-chan *models.Price, error)
	UnsubscribePriceUpdates(ctx context.Context, assetID string) error
}

// PriceStore stores price data
type PriceStore interface {
	SavePrice(ctx context.Context, price *models.Price) error
	GetPrice(ctx context.Context, assetID string) (*models.Price, error)
	GetPriceHistory(ctx context.Context, assetID string, start, end time.Time) ([]*models.Price, error)
	DeletePrice(ctx context.Context, assetID string, timestamp time.Time) error
}

// PriceMetricsCollector collects price update metrics
type PriceMetricsCollector interface {
	// Start starts the metrics collector
	Start(ctx context.Context) error

	// Stop stops the metrics collector
	Stop(ctx context.Context) error

	RecordUpdate(ctx context.Context, price *models.Price, latency time.Duration)
	RecordFailedUpdate(ctx context.Context, assetID string, reason string)
	UpdateDataSourceHealth(ctx context.Context, source string, health float64)
	GetMetrics(ctx context.Context) *models.PriceMetrics
}

// PriceAlertManager manages price-related alerts
type PriceAlertManager interface {
	// Start starts the alert manager
	Start(ctx context.Context) error

	// Stop stops the alert manager
	Stop(ctx context.Context) error

	AlertPriceDeviation(ctx context.Context, assetID string, oldPrice, newPrice *big.Float)
	AlertStalePrice(ctx context.Context, assetID string, lastUpdate time.Time)
	AlertDataSourceFailure(ctx context.Context, source string, reason string)
}

// PriceValidator validates price updates
type PriceValidator interface {
	ValidatePrice(ctx context.Context, price *models.Price) error
	ValidateUpdateInterval(ctx context.Context, assetID string, timestamp time.Time) error
	ValidateDataSources(ctx context.Context, assetID string) error
}
