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
	Account    AccountConfig    `yaml:"account"`
	Wallet     WalletConfig     `yaml:"wallet"`
}

// GasBankConfig represents gas bank configuration
type GasBankConfig struct {
	InitialGas              float64       `yaml:"initialGas"`
	RefillAmount            float64       `yaml:"refillAmount"`
	RefillThreshold         float64       `yaml:"refillThreshold"`
	MaxAllocationPerUser    float64       `yaml:"maxAllocationPerUser"`
	MinAllocationAmount     float64       `yaml:"minAllocationAmount"`
	MaxAllocationTime       time.Duration `yaml:"maxAllocationTime"`
	CooldownPeriod          time.Duration `yaml:"cooldownPeriod"`
	StoreType               string        `yaml:"storeType"`
	StorePath               string        `yaml:"storePath,omitempty"`
	MonitorInterval         time.Duration `yaml:"monitorInterval"`
	ExpirationCheckInterval time.Duration `yaml:"expirationCheckInterval,omitempty"`
	EnableUserBalances      bool          `yaml:"enableUserBalances,omitempty"`
	MinDepositAmount        float64       `yaml:"minDepositAmount,omitempty"`
	WithdrawalFee           float64       `yaml:"withdrawalFee,omitempty"`
	NeoNodeURL              string        `yaml:"neoNodeUrl,omitempty"`
	WalletPath              string        `yaml:"walletPath,omitempty"`
	WalletPass              string        `yaml:"walletPass,omitempty"`
}

// PriceFeedConfig represents price feed configuration
type PriceFeedConfig struct {
	UpdateInterval time.Duration `yaml:"updateInterval"`
	MinDeviation   float64       `yaml:"minDeviation"`
	HeartbeatTime  time.Duration `yaml:"heartbeatTime"`
	Sources        []string      `yaml:"sources"`
	ContractHash   string        `yaml:"contractHash,omitempty"`
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
	ExecutionWindow        time.Duration `yaml:"executionWindow"`
	MaxMemory              int           `yaml:"maxMemory"`
	MaxTimeout             time.Duration `yaml:"maxTimeout"`
	DefaultRuntime         string        `yaml:"defaultRuntime"`
	MaxFunctionSize        int           `yaml:"maxFunctionSize,omitempty"`
	EnableNetworkAccess    bool          `yaml:"enableNetworkAccess,omitempty"`
	EnableFileIO           bool          `yaml:"enableFileIO,omitempty"`
	ServiceLayerURL        string        `yaml:"serviceLayerUrl,omitempty"`
	EnableInteroperability bool          `yaml:"enableInteroperability,omitempty"`
}

// TriggerConfig represents trigger configuration
type TriggerConfig struct {
	MaxTriggers           int           `yaml:"maxTriggers"`
	MaxExecutions         int           `yaml:"maxExecutions"`
	RetentionPeriod       time.Duration `yaml:"retentionPeriod"`
	ExecutionWindow       time.Duration `yaml:"executionWindow,omitempty"`
	MaxConcurrentTriggers int           `yaml:"maxConcurrentTriggers,omitempty"`
}

// SecretsConfig represents secrets configuration
type SecretsConfig struct {
	RotationInterval    time.Duration `yaml:"rotationInterval"`
	StoreType           string        `yaml:"storeType"`
	EncryptionKey       string        `yaml:"encryptionKey"`
	MaxSecretSize       int           `yaml:"maxSecretSize,omitempty"`
	MaxSecretsPerUser   int           `yaml:"maxSecretsPerUser,omitempty"`
	SecretExpiryEnabled bool          `yaml:"secretExpiryEnabled,omitempty"`
	DefaultTTL          time.Duration `yaml:"defaultTTL,omitempty"`
	StorePath           string        `yaml:"storePath,omitempty"`
}

// MetricsConfig represents metrics configuration
type MetricsConfig struct {
	CollectionInterval time.Duration     `yaml:"collectionInterval"`
	RetentionPeriod    time.Duration     `yaml:"retentionPeriod"`
	ExportFormat       string            `yaml:"exportFormat"`
	StorageConfig      map[string]string `yaml:"storageConfig,omitempty"`
}

// LoggingConfig represents logging configuration
type LoggingConfig struct {
	Format            string        `yaml:"format"`
	RetentionPeriod   time.Duration `yaml:"retentionPeriod"`
	MaxSize           int           `yaml:"maxSize"`
	MaxBackups        int           `yaml:"maxBackups"`
	LogFilePath       string        `yaml:"logFilePath"`
	EnableCompression bool          `yaml:"enableCompression"`
}

// SecurityConfig represents security configuration
type SecurityConfig struct {
	JWTSecret       string        `yaml:"jwtSecret"`
	TokenExpiration time.Duration `yaml:"tokenExpiration"`
	AllowedOrigins  []string      `yaml:"allowedOrigins"`
}

// AccountConfig represents account service configuration
type AccountConfig struct {
	MaxBatchSize     int           `yaml:"maxBatchSize"`
	DefaultGasLimit  int64         `yaml:"defaultGasLimit"`
	SignatureTimeout time.Duration `yaml:"signatureTimeout"`
	RecoveryWindow   time.Duration `yaml:"recoveryWindow"`
	TEERequired      bool          `yaml:"teeRequired"`
}

// WalletConfig represents wallet configuration for the service
// NOTE: This needs to be defined based on wallet.Config needs
// Example structure:
type WalletConfig struct {
	Path           string `yaml:"path"`
	Password       string `yaml:"password"` // Be careful with storing passwords in config
	AddressVersion byte   `yaml:"addressVersion"`
	Network        string `yaml:"network"` // Use string like "mainnet", "testnet"
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
		API: APIConfig{
			Host:               "localhost",
			Port:               8080,
			Endpoint:           "http://localhost:10332",
			Timeout:            30 * time.Second,
			EnableCORS:         true,
			MaxRequestBodySize: 10 * 1024 * 1024, // 10MB
		},
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
				InitialGas:              1000,
				RefillAmount:            100,
				RefillThreshold:         10,
				MaxAllocationPerUser:    100,
				MinAllocationAmount:     1,
				MaxAllocationTime:       24 * time.Hour,
				CooldownPeriod:          1 * time.Hour,
				StoreType:               "memory",
				StorePath:               "",
				MonitorInterval:         5 * time.Minute,
				ExpirationCheckInterval: 15 * time.Minute,
				EnableUserBalances:      false,
				MinDepositAmount:        0.1,
				WithdrawalFee:           0,
				NeoNodeURL:              "",
				WalletPath:              "",
				WalletPass:              "",
			},
			PriceFeed: PriceFeedConfig{
				UpdateInterval: 1 * time.Minute,
				MinDeviation:   0.5,
				HeartbeatTime:  24 * time.Hour,
				Sources:        []string{"binance", "huobi", "okex"},
				ContractHash:   "",
			},
			Automation: AutomationConfig{
				CheckInterval:    1 * time.Minute,
				RetryDelay:       5 * time.Minute,
				MaxExecutionTime: 5 * time.Minute,
				MaxRetries:       3,
			},
			Functions: FunctionsConfig{
				ExecutionWindow:        5 * time.Minute,
				MaxMemory:              512,
				MaxTimeout:             30 * time.Second,
				DefaultRuntime:         "javascript",
				MaxFunctionSize:        1024 * 1024,
				EnableNetworkAccess:    false,
				EnableFileIO:           false,
				ServiceLayerURL:        "http://localhost:8080",
				EnableInteroperability: true,
			},
			Trigger: TriggerConfig{
				MaxTriggers:           100,
				MaxExecutions:         1000,
				RetentionPeriod:       30 * 24 * time.Hour,
				ExecutionWindow:       24 * time.Hour,
				MaxConcurrentTriggers: 10,
			},
			Secrets: SecretsConfig{
				RotationInterval:    30 * 24 * time.Hour,
				StoreType:           "memory",
				EncryptionKey:       "",
				MaxSecretSize:       10 * 1024,
				MaxSecretsPerUser:   100,
				SecretExpiryEnabled: true,
				DefaultTTL:          24 * time.Hour,
				StorePath:           "",
			},
			Metrics: MetricsConfig{
				CollectionInterval: 10 * time.Second,
				RetentionPeriod:    7 * 24 * time.Hour,
				ExportFormat:       "prometheus",
				StorageConfig:      map[string]string{},
			},
			Logging: LoggingConfig{
				Format:            "text",
				RetentionPeriod:   7 * 24 * time.Hour,
				MaxSize:           100,
				MaxBackups:        3,
				LogFilePath:       "./logs/service.log",
				EnableCompression: true,
			},
			Account: AccountConfig{
				MaxBatchSize:     100,
				DefaultGasLimit:  1000000,
				SignatureTimeout: 30 * time.Second,
				RecoveryWindow:   7 * 24 * time.Hour,
				TEERequired:      true,
			},
			Wallet: WalletConfig{
				Path:           "default_service_wallet.json",
				Password:       "",
				AddressVersion: 0x35,
				Network:        "mainnet",
			},
		},
		Security: SecurityConfig{
			JWTSecret:       "very-insecure-default-jwt-secret-replace-me!",
			TokenExpiration: 24 * time.Hour,
			AllowedOrigins:  []string{"*"},
		},
	}
}
