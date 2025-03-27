package api

import (
	"context"
	"encoding/json"
	"io"
	"math/big"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"
	"time"

	"github.com/go-chi/jwtauth/v5"
	"github.com/nspcc-dev/neo-go/pkg/encoding/address"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/sirupsen/logrus"
	"github.com/stretchr/testify/mock"
	"github.com/stretchr/testify/require"
	"github.com/will/neo_service_layer/internal/services/functions"
	"github.com/will/neo_service_layer/internal/services/gasbank"
	gasbankmodels "github.com/will/neo_service_layer/internal/services/gasbank/models"
	"github.com/will/neo_service_layer/internal/services/pricefeed"
	"github.com/will/neo_service_layer/internal/services/pricefeed/models"
	"github.com/will/neo_service_layer/internal/services/secrets"
	"github.com/will/neo_service_layer/internal/services/trigger"
	triggermodels "github.com/will/neo_service_layer/internal/services/trigger/models"
)

// Mock dependencies for testing
type mockDependencies struct {
	FunctionsService functions.IService
	SecretsService   secrets.IService
	GasBankService   gasbank.IService
	PriceFeedService pricefeed.IService
	TriggerService   trigger.IService
}

type mockFunctionsService struct {
	mock.Mock
}

type mockSecretsService struct {
	mock.Mock
}

type mockGasBankService struct {
	mock.Mock
}

type mockPriceFeedService struct {
	mock.Mock
}

type mockTriggerService struct {
	mock.Mock
}

// Mock implementations for service interfaces

func (m *mockFunctionsService) CreateFunction(ctx context.Context, owner util.Uint160, name, description, code string, runtime functions.Runtime) (*functions.Function, error) {
	args := m.Called(ctx, owner, name, description, code, runtime)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*functions.Function), args.Error(1)
}

func (m *mockFunctionsService) GetFunction(ctx context.Context, functionID string) (*functions.Function, error) {
	args := m.Called(ctx, functionID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*functions.Function), args.Error(1)
}

func (m *mockFunctionsService) UpdateFunction(ctx context.Context, functionID string, updater util.Uint160, updates map[string]interface{}) (*functions.Function, error) {
	args := m.Called(ctx, functionID, updater, updates)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*functions.Function), args.Error(1)
}

func (m *mockFunctionsService) DeleteFunction(ctx context.Context, functionID string, deleter util.Uint160) error {
	args := m.Called(ctx, functionID, deleter)
	return args.Error(0)
}

func (m *mockFunctionsService) InvokeFunction(ctx context.Context, invocation functions.FunctionInvocation) (*functions.FunctionExecution, error) {
	args := m.Called(ctx, invocation)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*functions.FunctionExecution), args.Error(1)
}

func (m *mockFunctionsService) ListFunctions(ctx context.Context, owner util.Uint160) ([]*functions.Function, error) {
	args := m.Called(ctx, owner)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]*functions.Function), args.Error(1)
}

func (m *mockFunctionsService) ListExecutions(ctx context.Context, functionID string, limit int) ([]*functions.FunctionExecution, error) {
	args := m.Called(ctx, functionID, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]*functions.FunctionExecution), args.Error(1)
}

func (m *mockFunctionsService) GetPermissions(ctx context.Context, functionID string) (*functions.FunctionPermissions, error) {
	args := m.Called(ctx, functionID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*functions.FunctionPermissions), args.Error(1)
}

func (m *mockFunctionsService) UpdatePermissions(ctx context.Context, functionID string, updater util.Uint160, permissions *functions.FunctionPermissions) error {
	args := m.Called(ctx, functionID, updater, permissions)
	return args.Error(0)
}

func (m *mockSecretsService) StoreSecret(ctx context.Context, userAddress util.Uint160, key, value string, options map[string]interface{}) error {
	args := m.Called(ctx, userAddress, key, value, options)
	return args.Error(0)
}

func (m *mockSecretsService) GetSecret(ctx context.Context, userAddress util.Uint160, key string) (string, error) {
	args := m.Called(ctx, userAddress, key)
	return args.String(0), args.Error(1)
}

func (m *mockSecretsService) DeleteSecret(ctx context.Context, userAddress util.Uint160, key string) error {
	args := m.Called(ctx, userAddress, key)
	return args.Error(0)
}

func (m *mockSecretsService) ListSecrets(ctx context.Context, userAddress util.Uint160) ([]string, error) {
	args := m.Called(ctx, userAddress)
	return args.Get(0).([]string), args.Error(1)
}

func (m *mockGasBankService) Start(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *mockGasBankService) Stop(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *mockGasBankService) GetAllocation(ctx context.Context, userAddress util.Uint160) (*gasbankmodels.Allocation, error) {
	args := m.Called(ctx, userAddress)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*gasbankmodels.Allocation), args.Error(1)
}

func (m *mockGasBankService) RequestAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*gasbankmodels.Allocation, error) {
	args := m.Called(ctx, userAddress, amount)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*gasbankmodels.Allocation), args.Error(1)
}

func (m *mockGasBankService) ReleaseAllocation(ctx context.Context, userAddress util.Uint160) error {
	args := m.Called(ctx, userAddress)
	return args.Error(0)
}

func (m *mockGasBankService) AllocateGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*gasbankmodels.Allocation, error) {
	args := m.Called(ctx, userAddress, amount)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*gasbankmodels.Allocation), args.Error(1)
}

func (m *mockGasBankService) ReleaseGas(ctx context.Context, userAddress util.Uint160) error {
	args := m.Called(ctx, userAddress)
	return args.Error(0)
}

func (m *mockPriceFeedService) Start(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *mockPriceFeedService) Stop(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *mockPriceFeedService) GetPrice(ctx context.Context, symbol string) (*models.Price, error) {
	args := m.Called(ctx, symbol)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Price), args.Error(1)
}

func (m *mockTriggerService) Start(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *mockTriggerService) Stop(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *mockTriggerService) CreateTrigger(ctx context.Context, userAddress util.Uint160, trigger *triggermodels.Trigger) (*triggermodels.Trigger, error) {
	args := m.Called(ctx, userAddress, trigger)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.Trigger), args.Error(1)
}

func (m *mockTriggerService) GetTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*triggermodels.Trigger, error) {
	args := m.Called(ctx, userAddress, triggerID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.Trigger), args.Error(1)
}

func (m *mockTriggerService) UpdateTrigger(ctx context.Context, userAddress util.Uint160, triggerID string, trigger *triggermodels.Trigger) (*triggermodels.Trigger, error) {
	args := m.Called(ctx, userAddress, triggerID, trigger)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.Trigger), args.Error(1)
}

func (m *mockTriggerService) DeleteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) error {
	args := m.Called(ctx, userAddress, triggerID)
	return args.Error(0)
}

func (m *mockTriggerService) ListTriggers(ctx context.Context, userAddress util.Uint160) ([]*triggermodels.Trigger, error) {
	args := m.Called(ctx, userAddress)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]*triggermodels.Trigger), args.Error(1)
}

func (m *mockTriggerService) ExecuteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*triggermodels.Execution, error) {
	args := m.Called(ctx, userAddress, triggerID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.Execution), args.Error(1)
}

func (m *mockTriggerService) GetTriggerExecutions(ctx context.Context, triggerID string) ([]*triggermodels.TriggerExecution, error) {
	args := m.Called(ctx, triggerID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]*triggermodels.TriggerExecution), args.Error(1)
}

func (m *mockTriggerService) GetTriggerMetrics(ctx context.Context, triggerID string) (*triggermodels.TriggerMetrics, error) {
	args := m.Called(ctx, triggerID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.TriggerMetrics), args.Error(1)
}

func (m *mockTriggerService) GetTriggerPolicy(ctx context.Context) (*triggermodels.TriggerPolicy, error) {
	args := m.Called(ctx)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*triggermodels.TriggerPolicy), args.Error(1)
}

func (m *mockTriggerService) UpdateTriggerPolicy(ctx context.Context, policy *triggermodels.TriggerPolicy) error {
	args := m.Called(ctx, policy)
	return args.Error(0)
}

func setupTestService(t *testing.T) (*Service, *mockDependencies) {
	mocks := &mockDependencies{
		FunctionsService: &mockFunctionsService{},
		SecretsService:   &mockSecretsService{},
		GasBankService:   &mockGasBankService{},
		PriceFeedService: &mockPriceFeedService{},
		TriggerService:   &mockTriggerService{},
	}

	config := &Config{
		Port:               3000,
		Host:               "localhost",
		ReadTimeout:        30 * time.Second,
		WriteTimeout:       30 * time.Second,
		IdleTimeout:        60 * time.Second,
		MaxRequestBodySize: 1024 * 1024,
		JWTSecret:          "test-secret",
		JWTExpiryDuration:  24 * time.Hour,
		EnableCORS:         true,
		AllowedOrigins:     []string{"*"},
	}

	deps := &Dependencies{
		FunctionsService: mocks.FunctionsService,
		SecretsService:   mocks.SecretsService,
		GasBankService:   mocks.GasBankService,
		PriceFeedService: mocks.PriceFeedService,
		TriggerService:   mocks.TriggerService,
		Logger:           logrus.New(),
	}

	service, err := NewService(config, deps)
	require.NoError(t, err)
	require.NotNil(t, service)

	return service, mocks
}

// Helper function to create an authenticated request
func createAuthenticatedRequest(t *testing.T, service *Service, method, path string, body io.Reader) *http.Request {
	req := httptest.NewRequest(method, path, body)

	// Create a valid JWT token
	claims := map[string]interface{}{
		"address": "NYxb4fSZVKAz8YsgaPK2WkT3KcAE9b3Vag",
		"exp":     time.Now().Add(24 * time.Hour).Unix(),
	}
	_, tokenString, err := service.tokenAuth.Encode(claims)
	require.NoError(t, err)

	// Set Authorization header
	req.Header.Set("Authorization", "Bearer "+tokenString)

	// Set JWT context
	token, err := service.tokenAuth.Decode(tokenString)
	require.NoError(t, err)
	ctx := jwtauth.NewContext(req.Context(), token, nil)

	// Set address in context
	scriptHash, err := address.StringToUint160(claims["address"].(string))
	require.NoError(t, err)
	ctx = context.WithValue(ctx, "address", scriptHash)

	return req.WithContext(ctx)
}

func TestAPIService(t *testing.T) {
	service, mocks := setupTestService(t)

	// Setup mock expectations
	triggerService := mocks.TriggerService.(*mockTriggerService)
	triggerService.On("GetTriggerMetrics", mock.Anything, mock.Anything).Return(&triggermodels.TriggerMetrics{}, nil)

	// Test trigger metrics endpoint with auth
	req := createAuthenticatedRequest(t, service, http.MethodGet, "/api/v1/triggers/123/metrics", nil)
	w := httptest.NewRecorder()
	service.router.ServeHTTP(w, req)

	// Should get OK with auth
	require.Equal(t, http.StatusOK, w.Code)

	// Test health endpoint
	req = httptest.NewRequest(http.MethodGet, "/health", nil)
	w = httptest.NewRecorder()
	service.router.ServeHTTP(w, req)

	require.Equal(t, http.StatusOK, w.Code)

	var healthResponse SystemHealthResponse
	err := json.Unmarshal(w.Body.Bytes(), &healthResponse)
	require.NoError(t, err)
	require.True(t, healthResponse.Healthy)
	require.Len(t, healthResponse.Services, 6) // Number of services registered

	// Verify mock expectations
	triggerService.AssertExpectations(t)
}

func TestAuthFlow(t *testing.T) {
	service, mocks := setupTestService(t)

	// Setup mock expectations
	triggerService := mocks.TriggerService.(*mockTriggerService)
	triggerService.On("GetTriggerMetrics", mock.Anything, mock.Anything).Return(&triggermodels.TriggerMetrics{}, nil)

	// Test trigger metrics endpoint with auth
	req := createAuthenticatedRequest(t, service, http.MethodGet, "/api/v1/triggers/123/metrics", nil)
	w := httptest.NewRecorder()
	service.router.ServeHTTP(w, req)

	// Should get OK with auth
	require.Equal(t, http.StatusOK, w.Code)

	// Test signature verification endpoint
	reqBody := `{
		"address": "0x1234567890abcdef1234567890abcdef12345678",
		"message": "test message",
		"signature": "test signature"
	}`

	req = httptest.NewRequest(http.MethodPost, "/api/v1/auth/verify", strings.NewReader(reqBody))
	req.Header.Set("Content-Type", "application/json")
	w = httptest.NewRecorder()
	service.router.ServeHTTP(w, req)

	// Should get a 400 because the address format is invalid
	require.Equal(t, http.StatusBadRequest, w.Code)

	// Try with a valid address format
	reqBody = `{
		"address": "NYxb4fSZVKAz8YsgaPK2WkT3KcAE9b3Vag",
		"message": "test message",
		"signature": "test signature"
	}`

	req = httptest.NewRequest(http.MethodPost, "/api/v1/auth/verify", strings.NewReader(reqBody))
	req.Header.Set("Content-Type", "application/json")
	w = httptest.NewRecorder()
	service.router.ServeHTTP(w, req)

	// Should get an error still, but we're just testing flow not actual signature verification
	require.NotEqual(t, http.StatusOK, w.Code)

	// Verify mock expectations
	triggerService.AssertExpectations(t)
}

func TestService_GetTriggerExecutions(t *testing.T) {
	service, mocks := setupTestService(t)

	// Setup mock expectations
	triggerService := mocks.TriggerService.(*mockTriggerService)
	triggerService.On("GetTriggerExecutions", mock.Anything, mock.Anything).Return([]*triggermodels.TriggerExecution{}, nil)

	// Test trigger executions endpoint without auth token
	req := httptest.NewRequest(http.MethodGet, "/api/v1/triggers/123/executions", nil)
	w := httptest.NewRecorder()
	service.router.ServeHTTP(w, req)

	// Should get unauthorized without auth token
	require.Equal(t, http.StatusUnauthorized, w.Code)

	// Test with auth token
	req = createAuthenticatedRequest(t, service, http.MethodGet, "/api/v1/triggers/123/executions", nil)
	w = httptest.NewRecorder()
	service.router.ServeHTTP(w, req)

	// Should get OK with auth token
	require.Equal(t, http.StatusOK, w.Code)

	// Verify mock expectations
	triggerService.AssertExpectations(t)
}

func TestService_GetTriggerMetrics(t *testing.T) {
	service, mocks := setupTestService(t)

	// Setup mock expectations
	triggerService := mocks.TriggerService.(*mockTriggerService)
	triggerService.On("GetTriggerMetrics", mock.Anything, mock.Anything).Return(&triggermodels.TriggerMetrics{}, nil)

	// Test trigger metrics endpoint without auth token
	req := httptest.NewRequest(http.MethodGet, "/api/v1/triggers/123/metrics", nil)
	w := httptest.NewRecorder()
	service.router.ServeHTTP(w, req)

	// Should get unauthorized without auth token
	require.Equal(t, http.StatusUnauthorized, w.Code)

	// Test with auth token
	req = createAuthenticatedRequest(t, service, http.MethodGet, "/api/v1/triggers/123/metrics", nil)
	w = httptest.NewRecorder()
	service.router.ServeHTTP(w, req)

	// Should get OK with auth token
	require.Equal(t, http.StatusOK, w.Code)

	// Verify mock expectations
	triggerService.AssertExpectations(t)
}
