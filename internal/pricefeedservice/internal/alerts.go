package internal

import (
	"context"
	"math/big"
	"sync"
	"time"
)

// PriceAlert represents a price-related alert
type PriceAlert struct {
	Type      string
	Message   string
	Severity  string
	Timestamp time.Time
	Data      map[string]interface{}
}

// PriceAlertManagerImpl implements the PriceAlertManager interface
type PriceAlertManagerImpl struct {
	alerts []PriceAlert
	mu     sync.RWMutex
}

// NewPriceAlertManager creates a new PriceAlertManager instance
func NewPriceAlertManager() PriceAlertManager {
	return &PriceAlertManagerImpl{}
}

// AlertPriceDeviation alerts on significant price deviations
func (pam *PriceAlertManagerImpl) AlertPriceDeviation(ctx context.Context, assetID string, oldPrice, newPrice *big.Float) {
	pam.mu.Lock()
	defer pam.mu.Unlock()

	pam.alerts = append(pam.alerts, PriceAlert{
		Type:     "PriceDeviation",
		Message:  "Significant price deviation detected",
		Severity: "warning",
		Data: map[string]interface{}{
			"assetID":  assetID,
			"oldPrice": oldPrice.String(),
			"newPrice": newPrice.String(),
		},
		Timestamp: time.Now(),
	})
}

// AlertStalePrice alerts on stale price data
func (pam *PriceAlertManagerImpl) AlertStalePrice(ctx context.Context, assetID string, lastUpdate time.Time) {
	pam.mu.Lock()
	defer pam.mu.Unlock()

	pam.alerts = append(pam.alerts, PriceAlert{
		Type:     "StalePrice",
		Message:  "Price data is stale",
		Severity: "warning",
		Data: map[string]interface{}{
			"assetID":    assetID,
			"lastUpdate": lastUpdate,
		},
		Timestamp: time.Now(),
	})
}

// AlertDataSourceFailure alerts on data source failures
func (pam *PriceAlertManagerImpl) AlertDataSourceFailure(ctx context.Context, source string, reason string) {
	pam.mu.Lock()
	defer pam.mu.Unlock()

	pam.alerts = append(pam.alerts, PriceAlert{
		Type:     "DataSourceFailure",
		Message:  "Data source failure detected",
		Severity: "critical",
		Data: map[string]interface{}{
			"source": source,
			"reason": reason,
		},
		Timestamp: time.Now(),
	})
}

// GetAlerts gets all recorded alerts
func (pam *PriceAlertManagerImpl) GetAlerts() []PriceAlert {
	pam.mu.RLock()
	defer pam.mu.RUnlock()

	alerts := make([]PriceAlert, len(pam.alerts))
	copy(alerts, pam.alerts)
	return alerts
}

// ClearAlerts clears all recorded alerts
func (pam *PriceAlertManagerImpl) ClearAlerts() {
	pam.mu.Lock()
	defer pam.mu.Unlock()

	pam.alerts = nil
}

// Start starts the alert manager
func (pam *PriceAlertManagerImpl) Start(ctx context.Context) error {
	// Nothing to start for now
	return nil
}

// Stop stops the alert manager
func (pam *PriceAlertManagerImpl) Stop(ctx context.Context) error {
	// Clear all alerts
	pam.mu.Lock()
	defer pam.mu.Unlock()
	pam.alerts = nil
	return nil
}
