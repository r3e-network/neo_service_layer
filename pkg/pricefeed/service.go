package pricefeed

import (
	"context"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"go.uber.org/zap"
)

// Config represents the configuration for the price feed service
type Config struct {
	RPCEndpoint    string             `json:"rpc_endpoint"`
	ContractHash   string             `json:"contract_hash"`
	ContractMethod string             `json:"contract_method"`
	GasPerUpdate   float64            `json:"gas_per_update"`
	UpdateInterval time.Duration      `json:"update_interval"`
	DataSources    []DataSourceConfig `json:"data_sources"`
}

// DataSourceConfig represents the configuration for a price data source
type DataSourceConfig struct {
	Type     string   `json:"type"`
	Name     string   `json:"name"`
	Endpoint string   `json:"endpoint"`
	Pairs    []string `json:"pairs"`
}

// PriceUpdate represents a price update from a data source
type PriceUpdate struct {
	Pair  string    `json:"pair"`
	Price float64   `json:"price"`
	Time  time.Time `json:"time"`
}

// Service represents the price feed service
type Service struct {
	config     Config
	wallet     *wallet.Wallet
	logger     *zap.Logger
	stopCh     chan struct{}
	updateCh   chan *PriceUpdate
	prices     map[string]float64
	lastUpdate map[string]time.Time
}

// NewService creates a new price feed service
func NewService(config Config, wallet *wallet.Wallet, logger *zap.Logger) *Service {
	return &Service{
		config:     config,
		wallet:     wallet,
		logger:     logger,
		stopCh:     make(chan struct{}),
		updateCh:   make(chan *PriceUpdate, 100),
		prices:     make(map[string]float64),
		lastUpdate: make(map[string]time.Time),
	}
}

// Start starts the price feed service
func (s *Service) Start(ctx context.Context) error {
	// Start data sources
	for _, ds := range s.config.DataSources {
		if err := s.startDataSource(ctx, ds); err != nil {
			return err
		}
	}

	// Start price update handler
	go s.handlePriceUpdates(ctx)

	return nil
}

// Stop stops the price feed service
func (s *Service) Stop() {
	close(s.stopCh)
}

// handlePriceUpdates handles price updates from data sources
func (s *Service) handlePriceUpdates(ctx context.Context) {
	for {
		select {
		case <-ctx.Done():
			return
		case <-s.stopCh:
			return
		case update := <-s.updateCh:
			s.prices[update.Pair] = update.Price
			s.lastUpdate[update.Pair] = update.Time
			s.logger.Info("Price updated",
				zap.String("pair", update.Pair),
				zap.Float64("price", update.Price),
				zap.Time("time", update.Time))
		}
	}
}

// GetPrice returns the current price for a pair
func (s *Service) GetPrice(pair string) (float64, time.Time, error) {
	price, ok := s.prices[pair]
	if !ok {
		return 0, time.Time{}, ErrPairNotFound
	}
	
	lastUpdate, ok := s.lastUpdate[pair]
	if !ok {
		return 0, time.Time{}, ErrPairNotFound
	}
	
	return price, lastUpdate, nil
}
