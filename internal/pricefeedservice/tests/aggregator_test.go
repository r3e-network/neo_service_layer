package tests

import (
	"context"
	"math/big"
	"testing"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/pricefeedservice/internal"
	"github.com/r3e-network/neo_service_layer/internal/pricefeedservice/models"
	"github.com/stretchr/testify/mock"
	"github.com/stretchr/testify/require"
)

// MockPriceStore is a mock implementation of PriceStore
type MockPriceStore struct {
	mock.Mock
}

func (m *MockPriceStore) SavePrice(ctx context.Context, price *models.Price) error {
	args := m.Called(ctx, price)
	return args.Error(0)
}

func (m *MockPriceStore) GetPrice(ctx context.Context, assetID string) (*models.Price, error) {
	args := m.Called(ctx, assetID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Price), args.Error(1)
}

func (m *MockPriceStore) GetPriceHistory(ctx context.Context, assetID string, start, end time.Time) ([]*models.Price, error) {
	args := m.Called(ctx, assetID, start, end)
	return args.Get(0).([]*models.Price), args.Error(1)
}

func (m *MockPriceStore) DeletePrice(ctx context.Context, assetID string, timestamp time.Time) error {
	args := m.Called(ctx, assetID, timestamp)
	return args.Error(0)
}

// MockMetricsCollector is a mock implementation of PriceMetricsCollector
type MockMetricsCollector struct {
	mock.Mock
}

func (m *MockMetricsCollector) Start(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockMetricsCollector) Stop(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockMetricsCollector) RecordUpdate(ctx context.Context, price *models.Price, latency time.Duration) {
	m.Called(ctx, price, latency)
}

func (m *MockMetricsCollector) RecordFailedUpdate(ctx context.Context, assetID string, reason string) {
	m.Called(ctx, assetID, reason)
}

func (m *MockMetricsCollector) UpdateDataSourceHealth(ctx context.Context, source string, health float64) {
	m.Called(ctx, source, health)
}

func (m *MockMetricsCollector) GetMetrics(ctx context.Context) *models.PriceMetrics {
	args := m.Called(ctx)
	return args.Get(0).(*models.PriceMetrics)
}

// MockAlertManager is a mock implementation of PriceAlertManager
type MockAlertManager struct {
	mock.Mock
}

func (m *MockAlertManager) Start(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockAlertManager) Stop(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockAlertManager) AlertPriceDeviation(ctx context.Context, assetID string, oldPrice, newPrice *big.Float) {
	m.Called(ctx, assetID, oldPrice, newPrice)
}

func (m *MockAlertManager) AlertStalePrice(ctx context.Context, assetID string, lastUpdate time.Time) {
	m.Called(ctx, assetID, lastUpdate)
}

func (m *MockAlertManager) AlertDataSourceFailure(ctx context.Context, source string, reason string) {
	m.Called(ctx, source, reason)
}

// MockPriceValidator is a mock implementation of PriceValidator
type MockPriceValidator struct {
	mock.Mock
}

func (m *MockPriceValidator) ValidatePrice(ctx context.Context, price *models.Price) error {
	args := m.Called(ctx, price)
	return args.Error(0)
}

func (m *MockPriceValidator) ValidateUpdateInterval(ctx context.Context, assetID string, timestamp time.Time) error {
	args := m.Called(ctx, assetID, timestamp)
	return args.Error(0)
}

func (m *MockPriceValidator) ValidateDataSources(ctx context.Context, assetID string) error {
	args := m.Called(ctx, assetID)
	return args.Error(0)
}

func TestPriceAggregator(t *testing.T) {
	ctx := context.Background()

	t.Run("PublishPrice", func(t *testing.T) {
		// Setup mocks
		store := new(MockPriceStore)
		metrics := new(MockMetricsCollector)
		alerts := new(MockAlertManager)
		validator := new(MockPriceValidator)

		updatePolicy := &models.PriceUpdatePolicy{
			MinUpdateInterval: time.Second * 30,
			MaxPriceDeviation: big.NewFloat(0.01),
			MinDataSources:    2,
			MaxDataAge:        time.Minute,
		}

		// Create test price
		price := &models.Price{
			AssetID:    "NEO/USD",
			Price:      big.NewFloat(1000),
			Timestamp:  time.Now(),
			Source:     "oracle",
			Confidence: 1.0,
		}

		// Setup expectations
		store.On("GetPrice", ctx, price.AssetID).Return(nil, nil)
		store.On("SavePrice", ctx, price).Return(nil)
		metrics.On("RecordUpdate", ctx, price, mock.Anything).Return()
		validator.On("ValidatePrice", ctx, mock.MatchedBy(func(p *models.Price) bool {
			return p.AssetID == price.AssetID && p.Price.Cmp(price.Price) == 0 && p.Source == price.Source && p.Confidence == price.Confidence
		})).Return(nil)
		validator.On("ValidateUpdateInterval", ctx, price.AssetID, price.Timestamp).Return(nil)

		// Create aggregator
		aggregator := internal.NewPriceAggregator(
			store,
			metrics,
			alerts,
			validator,
			updatePolicy,
			nil, // optional logger
		)

		// Test publishing price
		err := aggregator.PublishPrice(ctx, price.AssetID, price.Price, price.Timestamp)
		require.NoError(t, err)

		// Verify mock expectations
		store.AssertExpectations(t)
		metrics.AssertExpectations(t)
		alerts.AssertExpectations(t)
		validator.AssertExpectations(t)
	})
}
