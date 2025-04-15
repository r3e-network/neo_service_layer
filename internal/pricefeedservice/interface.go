package pricefeed

import (
	"context"

	"github.com/r3e-network/neo_service_layer/internal/pricefeedservice/models"
)

// IService defines the interface for the PriceFeed service
type IService interface {
	// Start starts the service
	Start(ctx context.Context) error

	// Stop stops the service
	Stop(ctx context.Context) error

	// GetPrice gets the current price for a symbol
	GetPrice(ctx context.Context, symbol string) (*models.Price, error)
}
