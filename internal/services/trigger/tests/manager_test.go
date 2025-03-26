package tests

import (
	"context"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
	"github.com/stretchr/testify/require"
	"github.com/will/neo_service_layer/internal/services/trigger/internal"
	"github.com/will/neo_service_layer/internal/services/trigger/models"
)

// Mock implementations
type MockTriggerStore struct {
	mock.Mock
}

func (m *MockTriggerStore) SaveTrigger(ctx context.Context, trigger *models.Trigger) error {
	args := m.Called(ctx, trigger)
	return args.Error(0)
}

func (m *MockTriggerStore) GetTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*models.Trigger, error) {
	args := m.Called(ctx, userAddress, triggerID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Trigger), args.Error(1)
}

func (m *MockTriggerStore) ListTriggers(ctx context.Context, userAddress util.Uint160) ([]*models.Trigger, error) {
	args := m.Called(ctx, userAddress)
	return args.Get(0).([]*models.Trigger), args.Error(1)
}

func (m *MockTriggerStore) DeleteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) error {
	args := m.Called(ctx, userAddress, triggerID)
	return args.Error(0)
}

func (m *MockTriggerStore) SaveExecution(ctx context.Context, execution *models.TriggerExecution) error {
	args := m.Called(ctx, execution)
	return args.Error(0)
}

func (m *MockTriggerStore) GetExecutions(ctx context.Context, userAddress util.Uint160, triggerID string) ([]*models.TriggerExecution, error) {
	args := m.Called(ctx, userAddress, triggerID)
	return args.Get(0).([]*models.TriggerExecution), args.Error(1)
}

type MockTriggerMetricsCollector struct {
	mock.Mock
}

func (m *MockTriggerMetricsCollector) RecordExecution(ctx context.Context, execution *models.TriggerExecution) {
	m.Called(ctx, execution)
}

func (m *MockTriggerMetricsCollector) RecordFailedExecution(ctx context.Context, triggerID string, reason string) {
	m.Called(ctx, triggerID, reason)
}

func (m *MockTriggerMetricsCollector) GetMetrics(ctx context.Context) *models.TriggerMetrics {
	args := m.Called(ctx)
	return args.Get(0).(*models.TriggerMetrics)
}

type MockTriggerAlertManager struct {
	mock.Mock
}

func (m *MockTriggerAlertManager) AlertExecutionFailure(ctx context.Context, trigger *models.Trigger, reason string) {
	m.Called(ctx, trigger, reason)
}

func (m *MockTriggerAlertManager) AlertHighGasUsage(ctx context.Context, trigger *models.Trigger, gasUsed int64) {
	m.Called(ctx, trigger, gasUsed)
}

func (m *MockTriggerAlertManager) AlertScheduleDeviation(ctx context.Context, trigger *models.Trigger, deviation string) {
	m.Called(ctx, trigger, deviation)
}

type MockTriggerScheduler struct {
	mock.Mock
}

func (m *MockTriggerScheduler) Start(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockTriggerScheduler) Stop(ctx context.Context) error {
	args := m.Called(ctx)
	return args.Error(0)
}

func (m *MockTriggerScheduler) ScheduleTrigger(ctx context.Context, trigger *models.Trigger) error {
	args := m.Called(ctx, trigger)
	return args.Error(0)
}

func (m *MockTriggerScheduler) UnscheduleTrigger(ctx context.Context, triggerID string) error {
	args := m.Called(ctx, triggerID)
	return args.Error(0)
}

func (m *MockTriggerScheduler) GetNextExecutionTime(ctx context.Context, schedule string) (time.Time, error) {
	args := m.Called(ctx, schedule)
	return args.Get(0).(time.Time), args.Error(1)
}

func TestTriggerManager(t *testing.T) {
	ctx := context.Background()
	userAddress := util.Uint160{1, 2, 3}

	store := new(MockTriggerStore)
	metrics := new(MockTriggerMetricsCollector)
	alerts := new(MockTriggerAlertManager)
	scheduler := new(MockTriggerScheduler)

	policy := &models.TriggerPolicy{
		MaxTriggersPerUser:      10,
		MaxExecutionsPerTrigger: 100,
		ExecutionWindow:         time.Hour * 24,
		MinInterval:             time.Minute * 5,
		MaxInterval:             time.Hour * 24,
		CooldownPeriod:          time.Minute,
	}

	manager := internal.NewTriggerManager(
		store,
		metrics,
		alerts,
		scheduler,
		policy,
		nil, // optional logger
	)

	t.Run("CreateTrigger", func(t *testing.T) {
		trigger := &models.Trigger{
			Name:        "Test Trigger",
			Description: "Test trigger description",
			UserAddress: userAddress,
			Condition:   "amount > 1000",
			Function:    "process",
			Parameters: map[string]interface{}{
				"param1": 1,
				"param2": "test",
			},
			Schedule: "*/5 * * * *",
		}

		nextExecution := time.Now().Add(5 * time.Minute)

		// Setup expectations
		scheduler.On("GetNextExecutionTime", ctx, trigger.Schedule).Return(nextExecution, nil)
		store.On("SaveTrigger", ctx, mock.AnythingOfType("*models.Trigger")).Return(nil)
		scheduler.On("ScheduleTrigger", ctx, mock.AnythingOfType("*models.Trigger")).Return(nil)

		// Test trigger creation
		createdTrigger, err := manager.CreateTrigger(ctx, userAddress, trigger)
		require.NoError(t, err)
		assert.NotEmpty(t, createdTrigger.ID)
		assert.Equal(t, "active", createdTrigger.Status)
		assert.Equal(t, nextExecution, createdTrigger.NextExecution)
	})

	t.Run("ExecuteTrigger", func(t *testing.T) {
		triggerID := "test-trigger"
		trigger := &models.Trigger{
			ID:          triggerID,
			UserAddress: userAddress,
			Function:    "process",
			Parameters: map[string]interface{}{
				"param1": 1,
				"param2": "test",
			},
		}

		// Setup expectations
		store.On("GetTrigger", ctx, userAddress, triggerID).Return(trigger, nil)
		store.On("SaveExecution", ctx, mock.AnythingOfType("*models.TriggerExecution")).Return(nil)
		metrics.On("RecordExecution", ctx, mock.AnythingOfType("*models.TriggerExecution"))
		scheduler.On("GetNextExecutionTime", ctx, trigger.Schedule).Return(time.Now().Add(5*time.Minute), nil)
		store.On("SaveTrigger", ctx, mock.AnythingOfType("*models.Trigger")).Return(nil)

		// Test trigger execution
		execution, err := manager.ExecuteTrigger(ctx, userAddress, triggerID)
		require.NoError(t, err)
		assert.NotNil(t, execution)
		assert.Equal(t, "success", execution.Status)
		assert.NotEmpty(t, execution.Result)
	})
}
