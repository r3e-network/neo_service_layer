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

// GasAlertManagerImpl implements the GasAlertManager interface
type GasAlertManagerImpl struct {
	lowGasThreshold *big.Int
	alertChannel    chan string
	lastAlerts      map[string]time.Time
	alertCooldown   time.Duration
}

// NewGasAlertManager creates a new GasAlertManager
func NewGasAlertManager() GasAlertManager {
	return &GasAlertManagerImpl{
		lowGasThreshold: big.NewInt(1000000), // Default threshold
		alertChannel:    make(chan string, 100),
		lastAlerts:      make(map[string]time.Time),
		alertCooldown:   time.Minute * 5, // Default cooldown period
	}
}

// SetLowGasThreshold sets the threshold for low gas alerts
func (am *GasAlertManagerImpl) SetLowGasThreshold(threshold *big.Int) {
	am.lowGasThreshold = threshold
}

// SetAlertCooldown sets the cooldown period between repeated alerts
func (am *GasAlertManagerImpl) SetAlertCooldown(cooldown time.Duration) {
	am.alertCooldown = cooldown
}

// Start starts the alert manager's processing routine
func (am *GasAlertManagerImpl) Start(ctx context.Context) {
	go func() {
		for {
			select {
			case <-ctx.Done():
				return
			case alertMsg := <-am.alertChannel:
				// In a real implementation, this would:
				// - Send alerts to monitoring systems
				// - Log to structured logging
				// - Send notifications to administrators
				// - Trigger automatic interventions

				// For now, just log the alert
				log.Printf("GAS ALERT: %s", alertMsg)
			}
		}
	}()
}

// AlertLowGas alerts when gas levels are low
func (am *GasAlertManagerImpl) AlertLowGas(ctx context.Context, remaining *big.Int) {
	// Check if remaining gas is below threshold
	if remaining.Cmp(am.lowGasThreshold) > 0 {
		return // Not below threshold
	}

	// Create alert key to prevent duplicate alerts
	alertKey := fmt.Sprintf("low_gas_%s", remaining.String())

	// Check if we've alerted about this recently
	if lastTime, exists := am.lastAlerts[alertKey]; exists {
		if time.Since(lastTime) < am.alertCooldown {
			return // Too soon to alert again
		}
	}

	// Update last alert time
	am.lastAlerts[alertKey] = time.Now()

	// Send alert
	alertMsg := fmt.Sprintf("Low gas level detected: %s remaining (below threshold of %s)",
		remaining.String(), am.lowGasThreshold.String())

	select {
	case am.alertChannel <- alertMsg:
		// Alert sent
	default:
		// Channel full, log directly
		log.Printf("GAS ALERT: %s (alert channel full)", alertMsg)
	}
}

// AlertFailedAllocation alerts when gas allocation fails
func (am *GasAlertManagerImpl) AlertFailedAllocation(ctx context.Context, userAddress util.Uint160, amount *big.Int, reason string) {
	// Create alert key to prevent duplicate alerts
	alertKey := fmt.Sprintf("failed_allocation_%s_%s", userAddress.String(), reason)

	// Check if we've alerted about this recently
	if lastTime, exists := am.lastAlerts[alertKey]; exists {
		if time.Since(lastTime) < am.alertCooldown {
			return // Too soon to alert again
		}
	}

	// Update last alert time
	am.lastAlerts[alertKey] = time.Now()

	// Send alert
	alertMsg := fmt.Sprintf("Failed to allocate %s gas to user %s: %s",
		amount.String(), userAddress.String(), reason)

	select {
	case am.alertChannel <- alertMsg:
		// Alert sent
	default:
		// Channel full, log directly
		log.Printf("GAS ALERT: %s (alert channel full)", alertMsg)
	}
}

// AlertFailedRefill alerts when gas pool refill fails
func (am *GasAlertManagerImpl) AlertFailedRefill(ctx context.Context, amount *big.Int, reason string) {
	// Create alert key to prevent duplicate alerts
	alertKey := fmt.Sprintf("failed_refill_%s", reason)

	// Check if we've alerted about this recently
	if lastTime, exists := am.lastAlerts[alertKey]; exists {
		if time.Since(lastTime) < am.alertCooldown {
			return // Too soon to alert again
		}
	}

	// Update last alert time
	am.lastAlerts[alertKey] = time.Now()

	// Send alert
	alertMsg := fmt.Sprintf("Failed to refill gas pool with %s gas: %s",
		amount.String(), reason)

	select {
	case am.alertChannel <- alertMsg:
		// Alert sent
	default:
		// Channel full, log directly
		log.Printf("GAS ALERT: %s (alert channel full)", alertMsg)
	}
}

// AlertHighUtilization alerts when gas utilization is high
func (am *GasAlertManagerImpl) AlertHighUtilization(ctx context.Context, utilization float64, totalGas *big.Int, allocatedGas *big.Int) {
	// Only alert if utilization is above a threshold (e.g., 80%)
	if utilization < 0.8 {
		return
	}

	// Create alert key to prevent duplicate alerts
	alertKey := fmt.Sprintf("high_utilization_%.0f", utilization*100)

	// Check if we've alerted about this recently
	if lastTime, exists := am.lastAlerts[alertKey]; exists {
		if time.Since(lastTime) < am.alertCooldown {
			return // Too soon to alert again
		}
	}

	// Update last alert time
	am.lastAlerts[alertKey] = time.Now()

	// Send alert
	alertMsg := fmt.Sprintf("High gas utilization: %.2f%% (Total: %s, Allocated: %s)",
		utilization*100, totalGas.String(), allocatedGas.String())

	select {
	case am.alertChannel <- alertMsg:
		// Alert sent
	default:
		// Channel full, log directly
		log.Printf("GAS ALERT: %s (alert channel full)", alertMsg)
	}
}

// AlertLargeAllocation alerts when a large gas allocation is made
func (am *GasAlertManagerImpl) AlertLargeAllocation(ctx context.Context, allocation *models.Allocation) {
	// Only alert if allocation is above a threshold
	largeThreshold := new(big.Int).Div(am.lowGasThreshold, big.NewInt(10))
	if allocation.Amount.Cmp(largeThreshold) <= 0 {
		return
	}

	// Create alert key to prevent duplicate alerts
	alertKey := fmt.Sprintf("large_allocation_%s_%s", allocation.UserAddress.String(), allocation.ID)

	// Check if we've alerted about this recently
	if lastTime, exists := am.lastAlerts[alertKey]; exists {
		if time.Since(lastTime) < am.alertCooldown {
			return // Too soon to alert again
		}
	}

	// Update last alert time
	am.lastAlerts[alertKey] = time.Now()

	// Send alert
	alertMsg := fmt.Sprintf("Large gas allocation: %s to user %s (ID: %s)",
		allocation.Amount.String(), allocation.UserAddress.String(), allocation.ID)

	select {
	case am.alertChannel <- alertMsg:
		// Alert sent
	default:
		// Channel full, log directly
		log.Printf("GAS ALERT: %s (alert channel full)", alertMsg)
	}
}

// AlertAllocationExpired alerts when an allocation has expired but wasn't released
func (am *GasAlertManagerImpl) AlertAllocationExpired(ctx context.Context, allocation *models.Allocation) {
	// Create alert key to prevent duplicate alerts
	alertKey := fmt.Sprintf("allocation_expired_%s", allocation.ID)

	// Check if we've alerted about this recently
	if lastTime, exists := am.lastAlerts[alertKey]; exists {
		if time.Since(lastTime) < am.alertCooldown {
			return // Too soon to alert again
		}
	}

	// Update last alert time
	am.lastAlerts[alertKey] = time.Now()

	// Calculate how long ago it expired
	expiredDuration := time.Since(allocation.ExpiresAt)

	// Send alert
	alertMsg := fmt.Sprintf("Gas allocation expired without release: ID %s, user %s, amount %s, expired %s ago",
		allocation.ID, allocation.UserAddress.String(), allocation.Amount.String(), expiredDuration.String())

	select {
	case am.alertChannel <- alertMsg:
		// Alert sent
	default:
		// Channel full, log directly
		log.Printf("GAS ALERT: %s (alert channel full)", alertMsg)
	}
}

// AlertSystemError alerts about system-level errors
func (am *GasAlertManagerImpl) AlertSystemError(ctx context.Context, component string, err error) {
	if err == nil {
		return
	}

	// Create alert key to prevent duplicate alerts
	alertKey := fmt.Sprintf("system_error_%s_%s", component, err.Error())

	// Check if we've alerted about this recently
	if lastTime, exists := am.lastAlerts[alertKey]; exists {
		if time.Since(lastTime) < am.alertCooldown {
			return // Too soon to alert again
		}
	}

	// Update last alert time
	am.lastAlerts[alertKey] = time.Now()

	// Send alert
	alertMsg := fmt.Sprintf("System error in %s component: %v", component, err)

	select {
	case am.alertChannel <- alertMsg:
		// Alert sent
	default:
		// Channel full, log directly
		log.Printf("GAS ALERT: %s (alert channel full)", alertMsg)
	}
}
