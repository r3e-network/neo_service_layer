package internal

import (
	"context"
	"errors"
	"math/big"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/stretchr/testify/assert"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

func TestBasicAlertManager(t *testing.T) {
	ctx := context.Background()

	t.Run("new alert manager with default config", func(t *testing.T) {
		manager := NewBasicAlertManager()
		assert.NotNil(t, manager)

		// Test alert thresholds
		assert.Equal(t, big.NewInt(5000000000), manager.config.LowGasThreshold)
		assert.Equal(t, big.NewInt(1000000000), manager.config.CriticalGasThreshold)
		assert.Equal(t, 0.85, manager.config.HighUtilizationThreshold)
		assert.Equal(t, 5*time.Minute, manager.config.AlertCooldown)
		assert.True(t, manager.config.EnableConsoleLogging)
	})

	t.Run("new alert manager with custom config", func(t *testing.T) {
		config := &AlertConfig{
			LowGasThreshold:          big.NewInt(2000000000),
			CriticalGasThreshold:     big.NewInt(500000000),
			HighUtilizationThreshold: 0.9,
			AlertCooldown:            10 * time.Minute,
			EnableConsoleLogging:     false,
		}

		manager := NewBasicAlertManagerWithConfig(config)
		assert.NotNil(t, manager)

		assert.Equal(t, big.NewInt(2000000000), manager.config.LowGasThreshold)
		assert.Equal(t, big.NewInt(500000000), manager.config.CriticalGasThreshold)
		assert.Equal(t, 0.9, manager.config.HighUtilizationThreshold)
		assert.Equal(t, 10*time.Minute, manager.config.AlertCooldown)
		assert.False(t, manager.config.EnableConsoleLogging)
	})

	t.Run("low gas alerts", func(t *testing.T) {
		config := &AlertConfig{
			LowGasThreshold:      big.NewInt(2000000000),
			CriticalGasThreshold: big.NewInt(500000000),
			EnableConsoleLogging: false, // Disable to avoid console output during tests
		}

		manager := NewBasicAlertManagerWithConfig(config)

		// Test with normal gas level (above threshold)
		manager.AlertLowGas(ctx, big.NewInt(3000000000))

		// Test with low gas level (below low threshold)
		manager.AlertLowGas(ctx, big.NewInt(1000000000))

		// Test with critical gas level
		manager.AlertLowGas(ctx, big.NewInt(100000000))
	})

	t.Run("should_send_alert cooldown", func(t *testing.T) {
		// Create manager with very short cooldown for testing
		config := &AlertConfig{
			AlertCooldown:        100 * time.Millisecond,
			EnableConsoleLogging: false,
		}

		manager := NewBasicAlertManagerWithConfig(config)

		// First alert should be sent
		assert.True(t, manager.shouldSendAlert("test_alert"))

		// Second immediate alert should not be sent
		assert.False(t, manager.shouldSendAlert("test_alert"))

		// After cooldown, alert should be sent
		time.Sleep(150 * time.Millisecond)
		assert.True(t, manager.shouldSendAlert("test_alert"))
	})

	t.Run("allocation expired alert", func(t *testing.T) {
		config := &AlertConfig{
			EnableConsoleLogging: false,
		}

		manager := NewBasicAlertManagerWithConfig(config)

		userAddress, err := util.Uint160DecodeStringLE("0123456789abcdef0123456789abcdef01234567")
		assert.NoError(t, err)

		allocation := &models.Allocation{
			ID:          "test-allocation-id",
			UserAddress: userAddress,
			Amount:      big.NewInt(1000000000),
			ExpiresAt:   time.Now().Add(-1 * time.Hour), // Expired 1 hour ago
			Status:      "active",
			LastUsedAt:  time.Now(),
		}

		// This should not cause any runtime errors
		manager.AlertAllocationExpired(ctx, allocation)
	})

	t.Run("system error alert", func(t *testing.T) {
		config := &AlertConfig{
			EnableConsoleLogging: false,
		}

		manager := NewBasicAlertManagerWithConfig(config)

		testErr := errors.New("test system error")

		// This should not cause any runtime errors
		manager.AlertSystemError(ctx, "test_component", testErr)

		// Test with nil error (should not alert)
		manager.AlertSystemError(ctx, "test_component", nil)
	})

	t.Run("high utilization alert", func(t *testing.T) {
		config := &AlertConfig{
			HighUtilizationThreshold: 0.8,
			EnableConsoleLogging:     false,
		}

		manager := NewBasicAlertManagerWithConfig(config)

		totalGas := big.NewInt(10000000000)
		allocatedGas := big.NewInt(8500000000)
		utilization := 0.85

		// This should not cause any runtime errors
		manager.AlertHighUtilization(ctx, utilization, totalGas, allocatedGas)

		// Test with utilization below threshold (should not alert)
		manager.AlertHighUtilization(ctx, 0.7, totalGas, allocatedGas)
	})

	t.Run("large allocation alert", func(t *testing.T) {
		config := &AlertConfig{
			LowGasThreshold:      big.NewInt(5000000000),
			EnableConsoleLogging: false,
		}

		manager := NewBasicAlertManagerWithConfig(config)

		userAddress, err := util.Uint160DecodeStringLE("0123456789abcdef0123456789abcdef01234567")
		assert.NoError(t, err)

		allocation := &models.Allocation{
			ID:          "test-large-allocation",
			UserAddress: userAddress,
			Amount:      big.NewInt(2000000000), // This is large enough to trigger alert
			Status:      "active",
			LastUsedAt:  time.Now(),
		}

		// This should not cause any runtime errors
		manager.AlertLargeAllocation(ctx, allocation)

		// Test with small allocation (should not alert)
		smallAllocation := &models.Allocation{
			ID:          "test-small-allocation",
			UserAddress: userAddress,
			Amount:      big.NewInt(100000000), // Too small to trigger alert
			Status:      "active",
			LastUsedAt:  time.Now(),
		}
		manager.AlertLargeAllocation(ctx, smallAllocation)
	})
}
