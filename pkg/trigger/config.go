package trigger

import (
	"fmt"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// Config represents the configuration for the trigger service
type Config struct {
	// RPCEndpoint is the Neo N3 RPC endpoint URL
	RPCEndpoint string `yaml:"rpc_endpoint" validate:"required,url"`

	// ContractHash is the hash of the contract to monitor
	ContractHash util.Uint160 `yaml:"contract_hash" validate:"required"`

	// NetworkMagic is the magic number of the Neo N3 network
	NetworkMagic uint32 `yaml:"network_magic" validate:"required"`

	// EventPollingInterval is the interval between event polling
	EventPollingInterval time.Duration `yaml:"event_polling_interval" validate:"required"`

	// MaxEventChannelSize is the maximum size of the event channel buffer
	MaxEventChannelSize int `yaml:"max_event_channel_size" validate:"required,min=100"`

	// MaxConcurrentExecutions is the maximum number of concurrent trigger executions
	MaxConcurrentExecutions int `yaml:"max_concurrent_executions" validate:"required,min=1"`

	// ExecutionTimeout is the maximum time allowed for a trigger execution
	ExecutionTimeout time.Duration `yaml:"execution_timeout" validate:"required"`

	// RetryAttempts is the number of retry attempts for failed executions
	RetryAttempts int `yaml:"retry_attempts" validate:"min=0"`

	// RetryDelay is the delay between retry attempts
	RetryDelay time.Duration `yaml:"retry_delay" validate:"required_with=RetryAttempts"`

	// MetricsEnabled determines if metrics collection is enabled
	MetricsEnabled bool `yaml:"metrics_enabled"`

	// LogLevel sets the logging level for the service
	LogLevel string `yaml:"log_level" validate:"oneof=debug info warn error"`
}

// Validate validates the configuration
func (c *Config) Validate() error {
	if c.EventPollingInterval < time.Second {
		return fmt.Errorf("event polling interval must be at least 1 second")
	}

	if c.MaxEventChannelSize < 100 {
		return fmt.Errorf("max event channel size must be at least 100")
	}

	if c.MaxConcurrentExecutions < 1 {
		return fmt.Errorf("max concurrent executions must be at least 1")
	}

	if c.ExecutionTimeout < time.Second {
		return fmt.Errorf("execution timeout must be at least 1 second")
	}

	if c.RetryAttempts > 0 && c.RetryDelay < time.Second {
		return fmt.Errorf("retry delay must be at least 1 second when retries are enabled")
	}

	return nil
}

// DefaultConfig returns a default configuration
func DefaultConfig() *Config {
	return &Config{
		EventPollingInterval:    time.Second * 15,
		MaxEventChannelSize:     1000,
		MaxConcurrentExecutions: 10,
		ExecutionTimeout:        time.Minute * 5,
		RetryAttempts:           3,
		RetryDelay:              time.Second * 30,
		MetricsEnabled:          true,
		LogLevel:                "info",
		NetworkMagic:            769, // Default to Neo N3 TestNet magic number
	}
}
