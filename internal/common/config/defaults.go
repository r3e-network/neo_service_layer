package config

import (
	"time"
)

// Default constants for configuration
const (
	DefaultLogLevel           = "info"
	DefaultAPIPort            = 8080
	DefaultAPIRequestSize     = 1024 * 1024 * 10 // 10MB
	DefaultDBPort             = 5432
	DefaultDBSSLMode          = "disable"
	DefaultNeoRequestTimeout  = 30
	DefaultNeoRetryAttempts   = 3
	DefaultGasBankStoreType   = "memory"
	DefaultPriceFeedInterval  = 60
	DefaultPriceFeedHeartbeat = 3600
	DefaultMetricsInterval    = 15
	DefaultMetricsRetention   = 7 * 24 * 60 * 60 // 7 days
	DefaultMetricsBackend     = "memory"
	DefaultSecretRotation     = 30 * 24 * 60 * 60 // 30 days
	DefaultMaxSecretSize      = 4096
	DefaultLogFileSize        = 100 // MB
	DefaultLogRetention       = 7   // files
	DefaultJWTExpiry          = 24  // hours
)

// GetDefaultConfig returns the default configuration
func GetDefaultConfig() *Config {
	return &Config{
		Environment: "development",
		LogLevel:    DefaultLogLevel,
		API: APIConfig{
			Host:              "0.0.0.0",
			Port:              DefaultAPIPort,
			EnableCORS:        true,
			MaxRequestBodySize: DefaultAPIRequestSize,
		},
		Database: DatabaseConfig{
			Type:     "postgres",
			Host:     "localhost",
			Port:     DefaultDBPort,
			Username: "postgres",
			Password: "postgres",
			Name:     "neo_service_layer",
			SSLMode:  DefaultDBSSLMode,
		},
		Neo: NeoConfig{
			RPCEndpoint:      "http://localhost:10332",
			NetworkMagic:     860833102, // TestNet
			WalletPath:       "wallet.json",
			WalletPassword:   "",
			DefaultAccount:   "",
			RequestTimeout:   DefaultNeoRequestTimeout,
			MaxRetryAttempts: DefaultNeoRetryAttempts,
		},
		Services: ServicesConfig{
			GasBank: GasBankConfig{
				InitialGas:           "1000000",
				MaxAllocationPerUser: "100000",
				StoreType:            DefaultGasBankStoreType,
			},
			PriceFeed: PriceFeedConfig{
				UpdateInterval:    DefaultPriceFeedInterval,
				SupportedSymbols:  []string{"NEO", "GAS", "BTC", "ETH"},
				DefaultHeartbeat:  DefaultPriceFeedHeartbeat,
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
				RotationInterval:    DefaultSecretRotation,
				MaxSecretsPerUser:   100,
				MaxSecretSize:       DefaultMaxSecretSize,
				EnableAccessLogging: true,
			},
			Metrics: MetricsConfig{
				CollectionInterval: DefaultMetricsInterval,
				RetentionPeriod:    DefaultMetricsRetention,
				StorageBackend:     DefaultMetricsBackend,
			},
			Logging: LoggingConfig{
				LogLevel:          DefaultLogLevel,
				EnableJSONLogs:    true,
				LogFilePath:       "/var/log/neo-service-layer/app.log",
				MaxSizeInMB:       DefaultLogFileSize,
				RetainedFiles:     DefaultLogRetention,
				EnableCompression: true,
			},
		},
		Security: SecurityConfig{
			TEEEnabled:     true,
			APIKeyRequired: true,
			JWTSecret:      "",
			JWTExpiryHours: DefaultJWTExpiry,
		},
	}
}

// GetDevConfig returns a development configuration
func GetDevConfig() *Config {
	config := GetDefaultConfig()
	config.Environment = "development"
	config.LogLevel = "debug"
	config.API.Port = 8081
	config.API.EnableCORS = true
	config.Database.Host = "localhost"
	config.Services.Logging.LogFilePath = "./logs/app.log"
	config.Security.APIKeyRequired = false
	return config
}

// GetTestConfig returns a testing configuration
func GetTestConfig() *Config {
	config := GetDefaultConfig()
	config.Environment = "testing"
	config.LogLevel = "debug"
	config.API.Port = 8082
	config.Database.Host = "localhost"
	config.Database.Name = "neo_service_layer_test"
	config.Services.Logging.LogFilePath = "./logs/test.log"
	config.Security.APIKeyRequired = false
	return config
}

// GetProdConfig returns a production configuration
func GetProdConfig() *Config {
	config := GetDefaultConfig()
	config.Environment = "production"
	config.LogLevel = "info"
	config.API.EnableCORS = false
	config.Security.APIKeyRequired = true
	config.Security.TEEEnabled = true
	return config
}

// GetConfigForEnvironment returns a configuration for the given environment
func GetConfigForEnvironment(env string) *Config {
	switch env {
	case "development":
		return GetDevConfig()
	case "testing":
		return GetTestConfig()
	case "production":
		return GetProdConfig()
	default:
		return GetDefaultConfig()
	}
}

// GetDefaultSchedulerSettings returns default scheduler settings
func GetDefaultSchedulerSettings() map[string]interface{} {
	return map[string]interface{}{
		"concurrency":      5,
		"retry_interval":   time.Second * 5,
		"max_retries":      3,
		"check_interval":   time.Minute * 5,
		"heartbeat":        time.Minute * 15,
		"shutdown_timeout": time.Second * 30,
	}
}

// GetDefaultAPISettings returns default API settings
func GetDefaultAPISettings() map[string]interface{} {
	return map[string]interface{}{
		"timeout":           time.Second * 30,
		"idle_timeout":      time.Second * 120,
		"read_timeout":      time.Second * 15,
		"write_timeout":     time.Second * 15,
		"max_header_bytes":  1 << 20, // 1MB
		"max_request_size":  10 << 20, // 10MB
		"max_conns_per_ip":  100,
		"use_compression":   true,
		"use_tls":           true,
		"rate_limit":        100,
		"rate_limit_burst":  200,
		"response_timeout":  time.Second * 30,
		"shutdown_timeout":  time.Second * 30,
	}
}

// GetDefaultWorkerSettings returns default worker settings
func GetDefaultWorkerSettings() map[string]interface{} {
	return map[string]interface{}{
		"concurrency":       10,
		"max_retries":       3,
		"retry_delay":       time.Second * 5,
		"max_queue_size":    1000,
		"processing_timeout": time.Minute * 5,
		"heartbeat_interval": time.Minute * 1,
		"shutdown_timeout":   time.Second * 30,
	}
}