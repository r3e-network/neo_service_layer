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
			Host:               "0.0.0.0",
			Port:               DefaultAPIPort,
			EnableCORS:         true,
			MaxRequestBodySize: DefaultAPIRequestSize,
		},
		Database: DatabaseConfig{
			Driver:   "postgres",
			Host:     "localhost",
			Port:     DefaultDBPort,
			Name:     "neo_service_layer",
			User:     "postgres",
			Password: "postgres",
			SSLMode:  DefaultDBSSLMode,
		},
		Neo: NeoConfig{
			Network:       "testnet",
			RPC:           []string{"http://localhost:10332"},
			WIF:           "",
			GasToken:      "0xd2a4cff31913016155e38e474a2c06d08be276cf",
			NeoToken:      "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5",
			BlockTime:     15,
			Confirmations: 1,
		},
		Services: ServicesConfig{
			GasBank: GasBankConfig{
				InitialGas:           1000000.0,
				RefillAmount:         100000.0,
				RefillThreshold:      10000.0,
				MaxAllocationPerUser: 100000.0,
				MinAllocationAmount:  1000.0,
				MaxAllocationTime:    24 * time.Hour,
				CooldownPeriod:       1 * time.Hour,
				StoreType:            DefaultGasBankStoreType,
				MonitorInterval:      5 * time.Minute,
			},
			PriceFeed: PriceFeedConfig{
				UpdateInterval: time.Duration(DefaultPriceFeedInterval) * time.Second,
				MinDeviation:   0.5,
				HeartbeatTime:  time.Duration(DefaultPriceFeedHeartbeat) * time.Second,
				Sources:        []string{"binance", "huobi", "okex"},
			},
			Automation: AutomationConfig{
				CheckInterval:    5 * time.Minute,
				RetryDelay:       15 * time.Second,
				MaxExecutionTime: 5 * time.Minute,
				MaxRetries:       3,
			},
			Functions: FunctionsConfig{
				ExecutionWindow: 30 * time.Second,
				MaxMemory:       128,
				MaxTimeout:      30 * time.Second,
				DefaultRuntime:  "javascript",
			},
			Trigger: TriggerConfig{
				MaxTriggers:     10,
				MaxExecutions:   100,
				RetentionPeriod: 24 * time.Hour,
			},
			Secrets: SecretsConfig{
				RotationInterval: time.Duration(DefaultSecretRotation) * time.Second,
				StoreType:        "memory",
				EncryptionKey:    "",
			},
			Metrics: MetricsConfig{
				CollectionInterval: time.Duration(DefaultMetricsInterval) * time.Second,
				RetentionPeriod:    time.Duration(DefaultMetricsRetention) * time.Second,
				ExportFormat:       "prometheus",
			},
			Logging: LoggingConfig{
				Format:          "json",
				RetentionPeriod: time.Duration(DefaultLogRetention) * 24 * time.Hour,
				MaxSize:         DefaultLogFileSize,
				MaxBackups:      DefaultLogRetention,
			},
			Account: AccountConfig{
				MaxBatchSize:     50,
				DefaultGasLimit:  1000000,
				SignatureTimeout: 60 * time.Second,
				RecoveryWindow:   24 * time.Hour,
				TEERequired:      true,
			},
		},
		Security: SecurityConfig{
			JWTSecret:       "default-secret-key",
			TokenExpiration: time.Duration(DefaultJWTExpiry) * time.Hour,
			AllowedOrigins:  []string{"*"},
		},
	}
}

// GetDevConfig returns a development configuration
func GetDevConfig() *Config {
	config := GetDefaultConfig()
	config.Environment = "development"
	config.LogLevel = "debug"
	return config
}

// GetTestConfig returns a testing configuration
func GetTestConfig() *Config {
	config := GetDefaultConfig()
	config.Environment = "testing"
	config.LogLevel = "debug"
	config.Database.Name = "neo_service_layer_test"
	return config
}

// GetProdConfig returns a production configuration
func GetProdConfig() *Config {
	config := GetDefaultConfig()
	config.Environment = "production"
	config.LogLevel = "info"
	config.Security.AllowedOrigins = []string{}
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
		"timeout":          time.Second * 30,
		"idle_timeout":     time.Second * 120,
		"read_timeout":     time.Second * 15,
		"write_timeout":    time.Second * 15,
		"max_header_bytes": 1 << 20,  // 1MB
		"max_request_size": 10 << 20, // 10MB
		"max_conns_per_ip": 100,
		"use_compression":  true,
		"use_tls":          true,
		"rate_limit":       100,
		"rate_limit_burst": 200,
		"response_timeout": time.Second * 30,
		"shutdown_timeout": time.Second * 30,
	}
}

// GetDefaultWorkerSettings returns default worker settings
func GetDefaultWorkerSettings() map[string]interface{} {
	return map[string]interface{}{
		"concurrency":        10,
		"max_retries":        3,
		"retry_delay":        time.Second * 5,
		"max_queue_size":     1000,
		"processing_timeout": time.Minute * 5,
		"heartbeat_interval": time.Minute * 1,
		"shutdown_timeout":   time.Second * 30,
	}
}
