package gasbank

import (
	"math/big"
	"time"

	"github.com/will/neo_service_layer/internal/services/gasbank/internal"
)

// DefaultAlertConfig returns a default AlertConfig that can be used when initializing the GasBank service.
// This is a convenience function to avoid nil pointer dereferences in the AlertManager.
func DefaultAlertConfig() *internal.AlertConfig {
	return internal.DefaultAlertConfig()
}

// DefaultConfig returns a default configuration for the GasBank service with all required fields initialized.
// This ensures that all components, including the AlertManager, are properly configured.
func DefaultFullConfig() *Config {
	return &Config{
		InitialGas:              big.NewInt(1000000000), // 10 GAS
		RefillAmount:            big.NewInt(500000000),  // 5 GAS
		RefillThreshold:         big.NewInt(200000000),  // 2 GAS
		MaxAllocationPerUser:    big.NewInt(100000000),  // 1 GAS
		MinAllocationAmount:     big.NewInt(1000000),    // 0.01 GAS
		MaxAllocationTime:       24 * time.Hour,
		CooldownPeriod:          5 * time.Minute,
		StoreType:               "memory",
		AlertConfig:             DefaultAlertConfig(),
		ExpirationCheckInterval: 15 * time.Minute,
		MonitorInterval:         5 * time.Minute,
	}
}
