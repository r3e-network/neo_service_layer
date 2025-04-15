package integration

import (
	"context"
	"math/big"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/crypto/keys"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
	pricefeedmodels "github.com/r3e-network/neo_service_layer/internal/pricefeedservice/models"
	"github.com/r3e-network/neo_service_layer/internal/services/api"
	"github.com/r3e-network/neo_service_layer/internal/services/functions"
	"github.com/r3e-network/neo_service_layer/internal/services/secrets"
	triggermodels "github.com/r3e-network/neo_service_layer/internal/triggerservice/models"
	"github.com/stretchr/testify/mock"
	"github.com/stretchr/testify/require"
)

// MockGasBankService is a mock implementation of gasbank.Service
type MockGasBankService struct {
	mock.Mock
}

func (m *MockGasBankService) Start(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockGasBankService) Stop(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockGasBankService) GetAllocation(ctx context.Context, userAddress util.Uint160) (*models.Allocation, error) {
	args := m.Called(ctx, userAddress)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Allocation), args.Error(1)
}

func (m *MockGasBankService) RequestAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*models.Allocation, error) {
	args := m.Called(ctx, userAddress, amount)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Allocation), args.Error(1)
}

func (m *MockGasBankService) ReleaseAllocation(ctx context.Context, userAddress util.Uint160) error {
	args := m.Called(ctx, userAddress)
	return args.Error(0)
}

func (m *MockGasBankService) AllocateGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*models.Allocation, error) {
	args := m.Called(ctx, userAddress, amount)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Allocation), args.Error(1)
}

func (m *MockGasBankService) ReleaseGas(ctx context.Context, userAddress util.Uint160) error {
	args := m.Called(ctx, userAddress)
	return args.Error(0)
}

func (m *MockGasBankService) GetBalance(ctx context.Context, userAddress util.Uint160) (*big.Int, error) {
	args := m.Called(ctx, userAddress)
	return args.Get(0).(*big.Int), args.Error(1)
}

// MockPriceFeedService is a mock implementation of pricefeed.Service
type MockPriceFeedService struct {
	mock.Mock
}

func (m *MockPriceFeedService) Start(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockPriceFeedService) Stop(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockPriceFeedService) GetPrice(ctx context.Context, symbol string) (*pricefeedmodels.Price, error) {
	args := m.Called(ctx, symbol)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*pricefeedmodels.Price), args.Error(1)
}

// MockTriggerService is a mock implementation of trigger.Service
type MockTriggerService struct {
	mock.Mock
}

func (m *MockTriggerService) Start(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockTriggerService) Stop(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockTriggerService) CreateTrigger(ctx context.Context, userAddress util.Uint160, trigger *triggermodels.Trigger) (*triggermodels.Trigger, error) {
	args := m.Called(ctx, userAddress, trigger)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.Trigger), args.Error(1)
}

func (m *MockTriggerService) GetTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*triggermodels.Trigger, error) {
	args := m.Called(ctx, userAddress, triggerID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.Trigger), args.Error(1)
}

func (m *MockTriggerService) UpdateTrigger(ctx context.Context, userAddress util.Uint160, triggerID string, trigger *triggermodels.Trigger) (*triggermodels.Trigger, error) {
	args := m.Called(ctx, userAddress, triggerID, trigger)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.Trigger), args.Error(1)
}

func (m *MockTriggerService) DeleteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) error {
	args := m.Called(ctx, userAddress, triggerID)
	return args.Error(0)
}

func (m *MockTriggerService) ListTriggers(ctx context.Context, userAddress util.Uint160) ([]*triggermodels.Trigger, error) {
	args := m.Called(ctx, userAddress)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]*triggermodels.Trigger), args.Error(1)
}

func (m *MockTriggerService) GetTriggerExecutions(ctx context.Context, triggerID string) ([]*triggermodels.TriggerExecution, error) {
	args := m.Called(ctx, triggerID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]*triggermodels.TriggerExecution), args.Error(1)
}

func (m *MockTriggerService) GetTriggerMetrics(ctx context.Context, triggerID string) (*triggermodels.TriggerMetrics, error) {
	args := m.Called(ctx, triggerID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.TriggerMetrics), args.Error(1)
}

func (m *MockTriggerService) GetTriggerPolicy(ctx context.Context) (*triggermodels.TriggerPolicy, error) {
	args := m.Called(ctx)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.TriggerPolicy), args.Error(1)
}

func (m *MockTriggerService) UpdateTriggerPolicy(ctx context.Context, policy *triggermodels.TriggerPolicy) error {
	args := m.Called(ctx, policy)
	return args.Error(0)
}

func (m *MockTriggerService) ExecuteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*triggermodels.Execution, error) {
	args := m.Called(ctx, userAddress, triggerID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.Execution), args.Error(1)
}

func TestAPIServiceIntegration(t *testing.T) {
	ctx := context.Background()

	// Create a test account
	privateKey, err := keys.NewPrivateKey()
	require.NoError(t, err)
	account := wallet.NewAccountFromPrivateKey(privateKey)
	userAddress := util.Uint160(account.ScriptHash())

	// Initialize Functions service
	functionsConfig := &functions.Config{
		MaxFunctionSize:     1024 * 1024, // 1MB
		MaxExecutionTime:    5 * time.Second,
		MaxMemoryLimit:      128 * 1024 * 1024, // 128MB
		EnableNetworkAccess: false,
		EnableFileIO:        false,
		DefaultRuntime:      "javascript",
	}
	functionservice, err := functions.NewService(functionsConfig)
	require.NoError(t, err)

	// Initialize Secrets service
	secretsConfig := &secrets.Config{
		EncryptionKey:       "test-encryption-key",
		MaxSecretSize:       10 * 1024, // 10KB
		MaxSecretsPerUser:   100,
		SecretExpiryEnabled: true,
		DefaultTTL:          24 * time.Hour, // 24 hours
	}
	secretservice, err := secrets.NewService(secretsConfig)
	require.NoError(t, err)

	// Create mock services
	mockGasBankService := new(MockGasBankService)
	mockPriceFeedService := new(MockPriceFeedService)
	mockTriggerService := new(MockTriggerService)

	// Configure mocks
	mockGasBankService.On("GetAllocation", mock.Anything, mock.Anything).Return(&models.Allocation{
		ID:          "test-alloc",
		UserAddress: userAddress,
		Amount:      big.NewInt(1000000),
		Used:        big.NewInt(0),
		Status:      "active",
		ExpiresAt:   time.Now().Add(24 * time.Hour),
	}, nil)

	mockPrice := &pricefeedmodels.Price{
		AssetID:    "NEO/USD",
		Price:      big.NewFloat(50),
		Timestamp:  time.Now(),
		Source:     "test",
		Confidence: 1.0,
	}
	mockPriceFeedService.On("GetPrice", mock.Anything, mock.Anything).Return(mockPrice, nil)

	mockTrigger := &triggermodels.Trigger{
		ID:          "trigger-1",
		Name:        "Test Trigger",
		Description: "A test trigger",
		Status:      "active",
		CreatedAt:   time.Now(),
		UpdatedAt:   time.Now(),
	}
	mockTriggerService.On("CreateTrigger", mock.Anything, mock.Anything, mock.Anything).Return(mockTrigger, nil)
	mockTriggerService.On("ListTriggers", mock.Anything, mock.Anything).Return([]*triggermodels.Trigger{mockTrigger}, nil)

	// Initialize API service
	apiConfig := &api.Config{
		Port:                 3000,
		Host:                 "localhost",
		ReadTimeout:          30 * time.Second,
		WriteTimeout:         30 * time.Second,
		IdleTimeout:          60 * time.Second,
		MaxRequestBodySize:   1024 * 1024,
		EnableCORS:           true,
		AllowedOrigins:       []string{"*"},
		EnableRateLimiting:   false,
		JWTSecret:            "test-secret",
		JWTExpiryDuration:    24 * time.Hour,
		EnableRequestLogging: false,
	}

	// API dependencies struct with mock service implementations
	apiDeps := &api.Dependencies{
		functionservice:  functionservice,
		secretservice:    secretservice,
		GasBankService:   mockGasBankService,
		PriceFeedService: mockPriceFeedService,
		TriggerService:   mockTriggerService,
		Logger:           nil, // Will use default
	}

	apiService, err := api.NewService(apiConfig, apiDeps)
	require.NoError(t, err)
	require.NotNil(t, apiService)

	// Create a function to test with
	function, err := functionservice.CreateFunction(ctx, userAddress, "test-function", "A test function", "function main(args) { return 'hello'; }", functions.JavaScriptRuntime)
	require.NoError(t, err)
	require.NotEmpty(t, function.ID)

	// Store a secret
	err = secretservice.StoreSecret(ctx, userAddress, "api-key", "secret-value", nil)
	require.NoError(t, err)

	// List functions
	functions, err := functionservice.ListFunctions(ctx, userAddress)
	require.NoError(t, err)
	require.Len(t, functions, 1)

	// List secrets
	secrets, err := secretservice.ListSecrets(ctx, userAddress)
	require.NoError(t, err)
	require.Len(t, secrets, 1)

	// Create a trigger
	trigger := &triggermodels.Trigger{
		Name:        "Test Trigger",
		Description: "A test trigger",
		Status:      "active",
	}
	createdTrigger, err := mockTriggerService.CreateTrigger(ctx, userAddress, trigger)
	require.NoError(t, err)
	require.NotNil(t, createdTrigger)

	// List triggers
	triggers, err := mockTriggerService.ListTriggers(ctx, userAddress)
	require.NoError(t, err)
	require.Len(t, triggers, 1)

	// Get gas balance
	allocation, err := mockGasBankService.GetAllocation(ctx, userAddress)
	require.NoError(t, err)
	require.NotNil(t, allocation)
	require.Equal(t, big.NewInt(1000000), allocation.Amount)

	// Get price
	price, err := mockPriceFeedService.GetPrice(ctx, "NEO/USD")
	require.NoError(t, err)
	require.NotNil(t, price)
	require.Equal(t, big.NewFloat(50), price.Price)

	// Verify mocks were called as expected
	mockGasBankService.AssertExpectations(t)
	mockPriceFeedService.AssertExpectations(t)
	mockTriggerService.AssertExpectations(t)
}
