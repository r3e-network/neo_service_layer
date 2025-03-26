package internal

import (
	"context"
	"fmt"
	"log"
	"math/big"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/gasbank/models"
)

// AlertLevel represents the severity of an alert
type AlertLevel string

const (
	// AlertLevelInfo is for informational alerts
	AlertLevelInfo AlertLevel = "INFO"

	// AlertLevelWarning is for warning alerts
	AlertLevelWarning AlertLevel = "WARNING"

	// AlertLevelError is for error alerts
	AlertLevelError AlertLevel = "ERROR"

	// AlertLevelCritical is for critical alerts
	AlertLevelCritical AlertLevel = "CRITICAL"
)

// AlertConfig contains configuration parameters for the alert manager
type AlertConfig struct {
	// LowGasThreshold is the threshold below which a low gas alert is triggered
	LowGasThreshold *big.Int

	// CriticalGasThreshold is the threshold below which a critical gas alert is triggered
	CriticalGasThreshold *big.Int

	// HighUtilizationThreshold is the percentage threshold for high utilization alerts
	HighUtilizationThreshold float64

	// AlertCooldown is the minimum time between repeated alerts
	AlertCooldown time.Duration

	// EnableConsoleLogging determines if alerts should be logged to console
	EnableConsoleLogging bool
}

// DefaultAlertConfig returns the default alert configuration
func DefaultAlertConfig() *AlertConfig {
	return &AlertConfig{
		LowGasThreshold:          big.NewInt(5000000000), // 50 GAS in smallest unit
		CriticalGasThreshold:     big.NewInt(1000000000), // 10 GAS in smallest unit
		HighUtilizationThreshold: 0.85,                   // 85% utilization
		AlertCooldown:            5 * time.Minute,
		EnableConsoleLogging:     true,
	}
}

// BasicAlertManager implements a simple alert manager that logs alerts
type BasicAlertManager struct {
	config        *AlertConfig
	lastAlertTime map[string]time.Time
}

// NewBasicAlertManager creates a new BasicAlertManager with default config
func NewBasicAlertManager() *BasicAlertManager {
	return NewBasicAlertManagerWithConfig(DefaultAlertConfig())
}

// NewBasicAlertManagerWithConfig creates a new BasicAlertManager with specified config
func NewBasicAlertManagerWithConfig(config *AlertConfig) *BasicAlertManager {
	return &BasicAlertManager{
		config:        config,
		lastAlertTime: make(map[string]time.Time),
	}
}

// shouldSendAlert checks if an alert should be sent based on cooldown period
func (a *BasicAlertManager) shouldSendAlert(alertType string) bool {
	lastTime, exists := a.lastAlertTime[alertType]
	now := time.Now()

	if !exists || now.Sub(lastTime) > a.config.AlertCooldown {
		a.lastAlertTime[alertType] = now
		return true
	}

	return false
}

// logAlert logs an alert to the console if enabled
func (a *BasicAlertManager) logAlert(level AlertLevel, format string, args ...interface{}) {
	if a.config.EnableConsoleLogging {
		message := fmt.Sprintf(format, args...)
		log.Printf("[%s] %s", level, message)
	}
}

// AlertLowGas alerts when gas is low
func (a *BasicAlertManager) AlertLowGas(ctx context.Context, remaining *big.Int) {
	// Critical alert
	if remaining.Cmp(a.config.CriticalGasThreshold) < 0 {
		if a.shouldSendAlert("critical_gas") {
			a.logAlert(AlertLevelCritical, "CRITICAL GAS LEVEL: %s remaining", remaining.String())
		}
		return
	}

	// Warning alert
	if remaining.Cmp(a.config.LowGasThreshold) < 0 {
		if a.shouldSendAlert("low_gas") {
			a.logAlert(AlertLevelWarning, "Low gas: %s remaining", remaining.String())
		}
	}
}

// AlertFailedAllocation alerts when gas allocation fails
func (a *BasicAlertManager) AlertFailedAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int, reason string) {
	alertKey := fmt.Sprintf("failed_allocation_%s", userAddress.StringLE())

	if a.shouldSendAlert(alertKey) {
		a.logAlert(AlertLevelError, "Failed allocation for user %s, amount %s: %s",
			userAddress.StringLE(), amount.String(), reason)
	}
}

// AlertFailedRefill alerts when gas refill fails
func (a *BasicAlertManager) AlertFailedRefill(ctx context.Context, amount *big.Int, reason string) {
	if a.shouldSendAlert("failed_refill") {
		a.logAlert(AlertLevelError, "Failed refill of %s GAS: %s", amount.String(), reason)
	}
}

// AlertHighUtilization alerts when gas utilization is high
func (a *BasicAlertManager) AlertHighUtilization(ctx context.Context, utilization float64, totalGas *big.Int, allocatedGas *big.Int) {
	if utilization > a.config.HighUtilizationThreshold {
		if a.shouldSendAlert("high_utilization") {
			a.logAlert(AlertLevelWarning, "High gas utilization: %.2f%% (Total: %s, Allocated: %s)",
				utilization*100, totalGas.String(), allocatedGas.String())
		}
	}
}

// AlertLargeAllocation alerts when a large allocation is made
func (a *BasicAlertManager) AlertLargeAllocation(ctx context.Context, allocation *models.Allocation) {
	// Consider an allocation large if it's more than 20% of the low gas threshold
	threshold := new(big.Int).Div(a.config.LowGasThreshold, big.NewInt(5))

	if allocation.Amount.Cmp(threshold) > 0 {
		alertKey := fmt.Sprintf("large_allocation_%s", allocation.UserAddress.StringLE())

		if a.shouldSendAlert(alertKey) {
			a.logAlert(AlertLevelInfo, "Large gas allocation: %s to user %s",
				allocation.Amount.String(), allocation.UserAddress.StringLE())
		}
	}
}

// AlertAllocationExpired alerts when an allocation has expired but wasn't released
func (a *BasicAlertManager) AlertAllocationExpired(ctx context.Context, allocation *models.Allocation) {
	alertKey := fmt.Sprintf("allocation_expired_%s", allocation.ID)

	if a.shouldSendAlert(alertKey) {
		a.logAlert(AlertLevelWarning, "Gas allocation expired without release: ID %s, user %s, amount %s, expiry %s",
			allocation.ID, allocation.UserAddress.StringLE(), allocation.Amount.String(),
			allocation.ExpiresAt.Format(time.RFC3339))
	}
}

// AlertSystemError alerts about system-level errors
func (a *BasicAlertManager) AlertSystemError(ctx context.Context, component string, err error) {
	alertKey := fmt.Sprintf("system_error_%s", component)

	if a.shouldSendAlert(alertKey) {
		a.logAlert(AlertLevelError, "System error in %s: %v", component, err)
	}
}
