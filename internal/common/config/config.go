package config

import (
	"fmt"
	"os"
	"time"

	"gopkg.in/yaml.v3"
)

// Config represents the application configuration
type Config struct {
	Environment string         `yaml:"environment"`
	LogLevel    string         `yaml:"logLevel"`
	API         APIConfig      `yaml:"api"`
	Database    DatabaseConfig `yaml:"database"`
	Neo         NeoConfig      `yaml:"neo"`
	Services    ServicesConfig `yaml:"services"`
	Security    SecurityConfig `yaml:"security"`
}

// DatabaseConfig represents database configuration
type DatabaseConfig struct {
	Driver   string `yaml:"driver"`
	Host     string `yaml:"host"`
	Port     int    `yaml:"port"`
	Name     string `yaml:"name"`
	User     string `yaml:"user"`
	Password string `yaml:"password"`
	SSLMode  string `yaml:"sslMode"`
}

// NeoConfig represents Neo N3 configuration
type NeoConfig struct {
	Network       string   `yaml:"network"`
	RPC           []string `yaml:"rpc"`
	WIF           string   `yaml:"wif"`
	GasToken      string   `yaml:"gasToken"`
	NeoToken      string   `yaml:"neoToken"`
	BlockTime     int      `yaml:"blockTime"`
	Confirmations int      `yaml:"confirmations"`
}

// ServicesConfig represents services configuration
type ServicesConfig struct {
	GasBank    GasBankConfig    `yaml:"gasBank"`
	PriceFeed  PriceFeedConfig  `yaml:"priceFeed"`
	Automation AutomationConfig `yaml:"automation"`
	Functions  FunctionsConfig  `yaml:"functions"`
	Trigger    TriggerConfig    `yaml:"trigger"`
	Secrets    SecretsConfig    `yaml:"secrets"`
	Metrics    MetricsConfig    `yaml:"metrics"`
	Logging    LoggingConfig    `yaml:"logging"`
}

// GasBankConfig represents gas bank configuration
type GasBankConfig struct {
	InitialGas           float64       `yaml:"initialGas"`
	RefillAmount         float64       `yaml:"refillAmount"`
	RefillThreshold      float64       `yaml:"refillThreshold"`
	MaxAllocationPerUser float64       `yaml:"maxAllocationPerUser"`
	MinAllocationAmount  float64       `yaml:"minAllocationAmount"`
	MaxAllocationTime    time.Duration `yaml:"maxAllocationTime"`
	CooldownPeriod       time.Duration `yaml:"cooldownPeriod"`
	StoreType            string        `yaml:"storeType"`
	MonitorInterval      time.Duration `yaml:"monitorInterval"`
}

// PriceFeedConfig represents price feed configuration
type PriceFeedConfig struct {
	UpdateInterval time.Duration `yaml:"updateInterval"`
	MinDeviation   float64       `yaml:"minDeviation"`
	HeartbeatTime  time.Duration `yaml:"heartbeatTime"`
	Sources        []string      `yaml:"sources"`
}

// AutomationConfig represents automation configuration
type AutomationConfig struct {
	CheckInterval    time.Duration `yaml:"checkInterval"`
	RetryDelay       time.Duration `yaml:"retryDelay"`
	MaxExecutionTime time.Duration `yaml:"maxExecutionTime"`
	MaxRetries       int           `yaml:"maxRetries"`
}

// FunctionsConfig represents functions configuration
type FunctionsConfig struct {
	ExecutionWindow time.Duration `yaml:"executionWindow"`
	MaxMemory       int           `yaml:"maxMemory"`
	MaxTimeout      time.Duration `yaml:"maxTimeout"`
	DefaultRuntime  string        `yaml:"defaultRuntime"`
}

// TriggerConfig represents trigger configuration
type TriggerConfig struct {
	MaxTriggers     int           `yaml:"maxTriggers"`
	MaxExecutions   int           `yaml:"maxExecutions"`
	RetentionPeriod time.Duration `yaml:"retentionPeriod"`
}

// SecretsConfig represents secrets configuration
type SecretsConfig struct {
	RotationInterval time.Duration `yaml:"rotationInterval"`
	StoreType        string        `yaml:"storeType"`
	EncryptionKey    string        `yaml:"encryptionKey"`
}

// MetricsConfig represents metrics configuration
type MetricsConfig struct {
	CollectionInterval time.Duration `yaml:"collectionInterval"`
	RetentionPeriod    time.Duration `yaml:"retentionPeriod"`
	ExportFormat       string        `yaml:"exportFormat"`
}

// LoggingConfig represents logging configuration
type LoggingConfig struct {
	Format          string        `yaml:"format"`
	RetentionPeriod time.Duration `yaml:"retentionPeriod"`
	MaxSize         int           `yaml:"maxSize"`
	MaxBackups      int           `yaml:"maxBackups"`
}

// SecurityConfig represents security configuration
type SecurityConfig struct {
	JWTSecret       string        `yaml:"jwtSecret"`
	TokenExpiration time.Duration `yaml:"tokenExpiration"`
	AllowedOrigins  []string      `yaml:"allowedOrigins"`
}

// LoadConfig loads configuration from a file
func LoadConfig(path string) (*Config, error) {
	data, err := os.ReadFile(path)
	if err != nil {
		return nil, fmt.Errorf("failed to read config file: %w", err)
	}

	var cfg Config
	if err := yaml.Unmarshal(data, &cfg); err != nil {
		return nil, fmt.Errorf("failed to parse config file: %w", err)
	}

	return &cfg, nil
}

// DefaultConfig returns default configuration
func DefaultConfig() *Config {
	return &Config{
		Environment: "development",
		LogLevel:    "info",
		API:         DefaultAPIConfig(),
		Database: DatabaseConfig{
			Driver:   "postgres",
			Host:     "localhost",
			Port:     5432,
			Name:     "neo_service_layer",
			User:     "postgres",
			Password: "postgres",
			SSLMode:  "disable",
		},
		Neo: NeoConfig{
			Network:       "testnet",
			RPC:           []string{"http://seed1t5.neo.org:20332"},
			BlockTime:     15,
			Confirmations: 1,
		},
		Services: ServicesConfig{
			GasBank: GasBankConfig{
				InitialGas:           1000,
				RefillAmount:         100,
				RefillThreshold:      10,
				MaxAllocationPerUser: 100,
				MinAllocationAmount:  1,
				MaxAllocationTime:    24 * time.Hour,
				CooldownPeriod:       1 * time.Hour,
				StoreType:            "postgres",
				MonitorInterval:      5 * time.Minute,
			},
			PriceFeed: PriceFeedConfig{
				UpdateInterval: 1 * time.Minute,
				MinDeviation:   0.5,
				HeartbeatTime:  24 * time.Hour,
				Sources:        []string{"binance", "huobi", "okex"},
			},
			Automation: AutomationConfig{
				CheckInterval:    1 * time.Minute,
				RetryDelay:       5 * time.Minute,
				MaxExecutionTime: 5 * time.Minute,
				MaxRetries:       3,
			},
			Functions: FunctionsConfig{
				ExecutionWindow: 5 * time.Minute,
				MaxMemory:       512,
				MaxTimeout:      30 * time.Second,
				DefaultRuntime:  "python3.9",
			},
			Trigger: TriggerConfig{
				MaxTriggers:     100,
				MaxExecutions:   1000,
				RetentionPeriod: 30 * 24 * time.Hour,
			},
			Secrets: SecretsConfig{
				RotationInterval: 30 * 24 * time.Hour,
				StoreType:        "postgres",
			},
			Metrics: MetricsConfig{
				CollectionInterval: 1 * time.Minute,
				RetentionPeriod:    90 * 24 * time.Hour,
				ExportFormat:       "prometheus",
			},
			Logging: LoggingConfig{
				Format:          "json",
				RetentionPeriod: 90 * 24 * time.Hour,
				MaxSize:         100,
				MaxBackups:      10,
			},
		},
		Security: SecurityConfig{
			TokenExpiration: 24 * time.Hour,
			AllowedOrigins:  []string{"*"},
		},
	}
}
