package gasbank

import (
	"fmt"
	"math/big"
	"time"
)

// Config represents the configuration for the gas bank service
type Config struct {
	// InitialGas is the initial amount of gas allocated to new accounts
	InitialGas *big.Int `yaml:"initial_gas" validate:"required"`

	// RefillAmount is the amount of gas to refill when an account is low
	RefillAmount *big.Int `yaml:"refill_amount" validate:"required"`

	// RefillThreshold is the threshold at which to refill an account
	RefillThreshold *big.Int `yaml:"refill_threshold" validate:"required"`

	// MaxAllocationPerUser is the maximum amount of gas that can be allocated to a single user
	MaxAllocationPerUser *big.Int `yaml:"max_allocation_per_user" validate:"required"`

	// MinAllocationAmount is the minimum amount of gas that can be allocated
	MinAllocationAmount *big.Int `yaml:"min_allocation_amount" validate:"required"`

	// MaxAllocationTime is the maximum time a gas allocation can be held
	MaxAllocationTime time.Duration `yaml:"max_allocation_time" validate:"required"`

	// CooldownPeriod is the time a user must wait between allocations
	CooldownPeriod time.Duration `yaml:"cooldown_period" validate:"required"`

	// StoreType is the type of storage to use (memory, postgres, etc.)
	StoreType string `yaml:"store_type" validate:"required,oneof=memory postgres"`

	// MonitorInterval is the interval at which to monitor gas usage
	MonitorInterval time.Duration `yaml:"monitor_interval" validate:"required"`

	// MetricsEnabled determines if metrics collection is enabled
	MetricsEnabled bool `yaml:"metrics_enabled"`

	// LogLevel sets the logging level for the service
	LogLevel string `yaml:"log_level" validate:"oneof=debug info warn error"`
}

// Validate validates the configuration
func (c *Config) Validate() error {
	if c.InitialGas.Sign() <= 0 {
		return fmt.Errorf("initial gas must be positive")
	}

	if c.RefillAmount.Sign() <= 0 {
		return fmt.Errorf("refill amount must be positive")
	}

	if c.RefillThreshold.Sign() <= 0 {
		return fmt.Errorf("refill threshold must be positive")
	}

	if c.MaxAllocationPerUser.Sign() <= 0 {
		return fmt.Errorf("max allocation per user must be positive")
	}

	if c.MinAllocationAmount.Sign() <= 0 {
		return fmt.Errorf("min allocation amount must be positive")
	}

	if c.MaxAllocationTime < time.Minute {
		return fmt.Errorf("max allocation time must be at least 1 minute")
	}

	if c.CooldownPeriod < time.Second {
		return fmt.Errorf("cooldown period must be at least 1 second")
	}

	if c.MonitorInterval < time.Second {
		return fmt.Errorf("monitor interval must be at least 1 second")
	}

	return nil
}

// DefaultConfig returns a default configuration
func DefaultConfig() *Config {
	return &Config{
		InitialGas:           big.NewInt(1000000000), // 10 GAS
		RefillAmount:         big.NewInt(500000000),  // 5 GAS
		RefillThreshold:      big.NewInt(200000000),  // 2 GAS
		MaxAllocationPerUser: big.NewInt(100000000),  // 1 GAS
		MinAllocationAmount:  big.NewInt(1000000),    // 0.01 GAS
		MaxAllocationTime:    24 * time.Hour,
		CooldownPeriod:       5 * time.Minute,
		StoreType:            "memory",
		MonitorInterval:      time.Minute,
		MetricsEnabled:       true,
		LogLevel:             "info",
	}
}
