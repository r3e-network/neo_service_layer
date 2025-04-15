package account

import (
	"time"

	"gopkg.in/yaml.v3"
)

// ServiceConfig represents the configuration for the account service
type ServiceConfig struct {
	MaxBatchSize     int           `yaml:"maxBatchSize"`
	DefaultGasLimit  int64         `yaml:"defaultGasLimit"`
	SignatureTimeout Duration      `yaml:"signatureTimeout"`
	RecoveryWindow   Duration      `yaml:"recoveryWindow"`
	TEERequired      bool          `yaml:"teeRequired"`
}

// Duration is a wrapper around time.Duration for YAML unmarshaling
type Duration struct {
	time.Duration
}

// UnmarshalYAML implements yaml.Unmarshaler interface
func (d *Duration) UnmarshalYAML(value *yaml.Node) error {
	var str string
	if err := value.Decode(&str); err != nil {
		return err
	}

	duration, err := time.ParseDuration(str)
	if err != nil {
		return err
	}

	d.Duration = duration
	return nil
}

// DefaultConfig returns the default configuration
func DefaultConfig() *ServiceConfig {
	return &ServiceConfig{
		MaxBatchSize:     50,
		DefaultGasLimit:  1000,
		SignatureTimeout: Duration{Duration: 60 * time.Second},
		RecoveryWindow:   Duration{Duration: 24 * time.Hour},
		TEERequired:      true,
	}
}