package config

import (
	"fmt"
	"io/ioutil"
	"os"
	"path/filepath"
	"strings"

	"gopkg.in/yaml.v2"
)

// Config represents the application configuration
type Config struct {
	Environment string          `yaml:"environment"`
	LogLevel    string          `yaml:"logLevel"`
	API         APIConfig       `yaml:"api"`
	Database    DatabaseConfig  `yaml:"database"`
	Neo         NeoConfig       `yaml:"neo"`
	Services    ServicesConfig  `yaml:"services"`
	Security    SecurityConfig  `yaml:"security"`
}

// APIConfig represents the API configuration
type APIConfig struct {
	Host              string `yaml:"host"`
	Port              int    `yaml:"port"`
	EnableCORS        bool   `yaml:"enableCORS"`
	MaxRequestBodySize int   `yaml:"maxRequestBodySize"`
}

// DatabaseConfig represents the database configuration
type DatabaseConfig struct {
	Type     string `yaml:"type"`
	Host     string `yaml:"host"`
	Port     int    `yaml:"port"`
	Username string `yaml:"username"`
	Password string `yaml:"password"`
	Name     string `yaml:"name"`
	SSLMode  string `yaml:"sslMode"`
}

// NeoConfig represents the Neo node configuration
type NeoConfig struct {
	RPCEndpoint      string `yaml:"rpcEndpoint"`
	NetworkMagic     uint32 `yaml:"networkMagic"`
	WalletPath       string `yaml:"walletPath"`
	WalletPassword   string `yaml:"walletPassword"`
	DefaultAccount   string `yaml:"defaultAccount"`
	RequestTimeout   int    `yaml:"requestTimeout"`
	MaxRetryAttempts int    `yaml:"maxRetryAttempts"`
}

// ServicesConfig represents the services configuration
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

// GasBankConfig represents the GasBank service configuration
type GasBankConfig struct {
	InitialGas           string `yaml:"initialGas"`
	MaxAllocationPerUser string `yaml:"maxAllocationPerUser"`
	StoreType            string `yaml:"storeType"`
}

// PriceFeedConfig represents the PriceFeed service configuration
type PriceFeedConfig struct {
	UpdateInterval    int      `yaml:"updateInterval"`
	SupportedSymbols  []string `yaml:"supportedSymbols"`
	DefaultHeartbeat  int      `yaml:"defaultHeartbeat"`
	MaxDeviationRate  float64  `yaml:"maxDeviationRate"`
	AnswerAggregation string   `yaml:"answerAggregation"`
}

// AutomationConfig represents the Automation service configuration
type AutomationConfig struct {
	CheckInterval   int    `yaml:"checkInterval"`
	RetryAttempts   int    `yaml:"retryAttempts"`
	RetryDelay      int    `yaml:"retryDelay"`
	GasBuffer       string `yaml:"gasBuffer"`
	KeeperRegistry  string `yaml:"keeperRegistry"`
}

// FunctionsConfig represents the Functions service configuration
type FunctionsConfig struct {
	MaxExecutionTime int    `yaml:"maxExecutionTime"`
	DefaultRuntime   string `yaml:"defaultRuntime"`
	MaxMemory        int    `yaml:"maxMemory"`
	MaxCPU           int    `yaml:"maxCPU"`
}

// TriggerConfig represents the Trigger service configuration
type TriggerConfig struct {
	MaxTriggers     int `yaml:"maxTriggers"`
	MaxExecutions   int `yaml:"maxExecutions"`
	ExecutionWindow int `yaml:"executionWindow"`
}

// SecretsConfig represents the Secrets service configuration
type SecretsConfig struct {
	EncryptionKey       string `yaml:"encryptionKey"`
	RotationInterval    int    `yaml:"rotationInterval"`
	MaxSecretsPerUser   int    `yaml:"maxSecretsPerUser"`
	MaxSecretSize       int    `yaml:"maxSecretSize"`
	EnableAccessLogging bool   `yaml:"enableAccessLogging"`
}

// MetricsConfig represents the Metrics service configuration
type MetricsConfig struct {
	CollectionInterval int    `yaml:"collectionInterval"`
	RetentionPeriod    int    `yaml:"retentionPeriod"`
	StorageBackend     string `yaml:"storageBackend"`
}

// LoggingConfig represents the Logging service configuration
type LoggingConfig struct {
	LogLevel          string `yaml:"logLevel"`
	EnableJSONLogs    bool   `yaml:"enableJSONLogs"`
	LogFilePath       string `yaml:"logFilePath"`
	MaxSizeInMB       int    `yaml:"maxSizeInMB"`
	RetainedFiles     int    `yaml:"retainedFiles"`
	EnableCompression bool   `yaml:"enableCompression"`
}

// SecurityConfig represents the security configuration
type SecurityConfig struct {
	TEEEnabled     bool   `yaml:"teeEnabled"`
	APIKeyRequired bool   `yaml:"apiKeyRequired"`
	JWTSecret      string `yaml:"jwtSecret"`
	JWTExpiryHours int    `yaml:"jwtExpiryHours"`
}

// LoadConfig loads configuration from a file
func LoadConfig(path string) (*Config, error) {
	// Ensure path exists
	if _, err := os.Stat(path); os.IsNotExist(err) {
		return nil, fmt.Errorf("config file not found: %s", path)
	}

	// Read file
	data, err := ioutil.ReadFile(path)
	if err != nil {
		return nil, fmt.Errorf("failed to read config file: %w", err)
	}

	// Parse YAML
	var config Config
	if err := yaml.Unmarshal(data, &config); err != nil {
		return nil, fmt.Errorf("failed to parse config file: %w", err)
	}

	// Apply environment variables
	applyEnvironmentVariables(&config)

	return &config, nil
}

// applyEnvironmentVariables overrides config values with environment variables
func applyEnvironmentVariables(config *Config) {
	// Environment
	if env := os.Getenv("NSL_ENVIRONMENT"); env != "" {
		config.Environment = env
	}

	// Log level
	if logLevel := os.Getenv("NSL_LOG_LEVEL"); logLevel != "" {
		config.LogLevel = logLevel
	}

	// Example of applying env vars to nested fields
	if port := os.Getenv("NSL_API_PORT"); port != "" {
		fmt.Sscanf(port, "%d", &config.API.Port)
	}

	// Apply more environment variables as needed
}

// SaveConfig saves configuration to a file
func SaveConfig(config *Config, path string) error {
	// Ensure directory exists
	dir := filepath.Dir(path)
	if err := os.MkdirAll(dir, 0755); err != nil {
		return fmt.Errorf("failed to create directory: %w", err)
	}

	// Marshal YAML
	data, err := yaml.Marshal(config)
	if err != nil {
		return fmt.Errorf("failed to marshal config: %w", err)
	}

	// Write file
	if err := ioutil.WriteFile(path, data, 0644); err != nil {
		return fmt.Errorf("failed to write config file: %w", err)
	}

	return nil
}

// DefaultConfig returns a default configuration
func DefaultConfig() *Config {
	return &Config{
		Environment: "development",
		LogLevel:    "info",
		API: APIConfig{
			Host:              "0.0.0.0",
			Port:              8080,
			EnableCORS:        true,
			MaxRequestBodySize: 10 * 1024 * 1024, // 10MB
		},
		Database: DatabaseConfig{
			Type:     "postgres",
			Host:     "localhost",
			Port:     5432,
			Username: "postgres",
			Password: "postgres",
			Name:     "neo_service_layer",
			SSLMode:  "disable",
		},
		Neo: NeoConfig{
			RPCEndpoint:      "http://localhost:10332",
			NetworkMagic:     860833102, // TestNet
			WalletPath:       "wallet.json",
			WalletPassword:   "",
			DefaultAccount:   "",
			RequestTimeout:   30,
			MaxRetryAttempts: 3,
		},
		Services: ServicesConfig{
			GasBank: GasBankConfig{
				InitialGas:           "1000000",
				MaxAllocationPerUser: "100000",
				StoreType:            "memory",
			},
			PriceFeed: PriceFeedConfig{
				UpdateInterval:    60,
				SupportedSymbols:  []string{"NEO", "GAS", "BTC", "ETH"},
				DefaultHeartbeat:  3600,
				MaxDeviationRate:  0.5,
				AnswerAggregation: "median",
			},
			Automation: AutomationConfig{
				CheckInterval:   300,
				RetryAttempts:   3,
				RetryDelay:      15,
				GasBuffer:       "10000",
				KeeperRegistry:  "",
			},
			Functions: FunctionsConfig{
				MaxExecutionTime: 30,
				DefaultRuntime:   "javascript",
				MaxMemory:        128,
				MaxCPU:           1,
			},
			Trigger: TriggerConfig{
				MaxTriggers:     10,
				MaxExecutions:   100,
				ExecutionWindow: 86400,
			},
			Secrets: SecretsConfig{
				EncryptionKey:       "",
				RotationInterval:    30,
				MaxSecretsPerUser:   100,
				MaxSecretSize:       4096,
				EnableAccessLogging: true,
			},
			Metrics: MetricsConfig{
				CollectionInterval: 15,
				RetentionPeriod:    604800, // 7 days
				StorageBackend:     "memory",
			},
			Logging: LoggingConfig{
				LogLevel:          "info",
				EnableJSONLogs:    true,
				LogFilePath:       "/var/log/neo-service-layer/app.log",
				MaxSizeInMB:       100,
				RetainedFiles:     7,
				EnableCompression: true,
			},
		},
		Security: SecurityConfig{
			TEEEnabled:     true,
			APIKeyRequired: true,
			JWTSecret:      "",
			JWTExpiryHours: 24,
		},
	}
}

// GetConfigValue gets a config value by path
func GetConfigValue(config *Config, path string) (interface{}, error) {
	parts := strings.Split(path, ".")
	if len(parts) == 0 {
		return nil, fmt.Errorf("invalid path: %s", path)
	}

	// This is a simplified implementation
	// A real implementation would use reflection to traverse the config struct
	switch parts[0] {
	case "environment":
		return config.Environment, nil
	case "logLevel":
		return config.LogLevel, nil
	// Add more cases as needed
	default:
		return nil, fmt.Errorf("unknown config path: %s", path)
	}
}