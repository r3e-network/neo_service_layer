package main

import (
	"context"
	"flag"
	"fmt"
	"math/big"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/will/neo_service_layer/internal/common/config"
	"github.com/will/neo_service_layer/internal/common/logger"
	"github.com/will/neo_service_layer/internal/core/neo"
	"github.com/will/neo_service_layer/internal/services/api"
	"github.com/will/neo_service_layer/internal/services/automation"
	"github.com/will/neo_service_layer/internal/services/functions"
	"github.com/will/neo_service_layer/internal/services/gasbank"
	"github.com/will/neo_service_layer/internal/services/logging"
	"github.com/will/neo_service_layer/internal/services/metrics"
	"github.com/will/neo_service_layer/internal/services/pricefeed"
	"github.com/will/neo_service_layer/internal/services/secrets"
	"github.com/will/neo_service_layer/internal/services/trigger"
)

func main() {
	// Parse command line flags
	configFile := flag.String("config", "config.yaml", "Path to configuration file")
	flag.Parse()

	// Initialize logger
	log := logger.NewLogger("info")

	// Load configuration
	cfg, err := config.LoadConfig(*configFile)
	if err != nil {
		log.Error("Failed to load configuration", map[string]interface{}{
			"error": err.Error(),
			"file":  *configFile,
		})
		os.Exit(1)
	}

	// Create a context that will be cancelled on SIGINT or SIGTERM
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	// Setup signal handling
	sigCh := make(chan os.Signal, 1)
	signal.Notify(sigCh, syscall.SIGINT, syscall.SIGTERM)
	go func() {
		sig := <-sigCh
		log.Info("Received signal, shutting down", map[string]interface{}{
			"signal": sig.String(),
		})
		cancel()
	}()

	// Initialize Neo client
	neoClient := &neo.Client{}

	// Initialize services
	logSvc, err := initLoggingService(cfg)
	if err != nil {
		log.Error("Failed to initialize logging service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	metricsSvc := initMetricsService(cfg)

	gasBankSvc, err := initGasBankService(ctx, cfg)
	if err != nil {
		log.Error("Failed to initialize gasbank service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	priceFeedSvc, err := initPriceFeedService(cfg, neoClient)
	if err != nil {
		log.Error("Failed to initialize price feed service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	triggerSvc, err := initTriggerService(cfg, neoClient)
	if err != nil {
		log.Error("Failed to initialize trigger service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	functionsSvc, err := initFunctionsService(cfg)
	if err != nil {
		log.Error("Failed to initialize functions service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	secretsSvc, err := initSecretsService(cfg)
	if err != nil {
		log.Error("Failed to initialize secrets service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	automationSvc, err := initAutomationService(cfg, neoClient, gasBankSvc)
	if err != nil {
		log.Error("Failed to initialize automation service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	// Initialize API service
	apiSvc, err := initAPIService(cfg, gasBankSvc, priceFeedSvc, triggerSvc, functionsSvc, secretsSvc)
	if err != nil {
		log.Error("Failed to initialize API service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	// Start all services
	if err := metricsSvc.Start(ctx); err != nil {
		log.Error("Failed to start metrics service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	if err := apiSvc.Start(); err != nil {
		log.Error("Failed to start API service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	if err := automationSvc.Start(ctx); err != nil {
		log.Error("Failed to start automation service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	// Block until context is cancelled
	<-ctx.Done()

	// Allow some time for graceful shutdown
	shutdownCtx, shutdownCancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer shutdownCancel()

	// Shutdown all services
	logSvc.Shutdown(shutdownCtx)
	metricsSvc.Stop()
	apiSvc.Stop(shutdownCtx)
	automationSvc.Stop(shutdownCtx)

	fmt.Println("Server stopped")
}

func initLoggingService(cfg *config.Config) (*logging.Service, error) {
	logConfig := &logging.Config{
		LogLevel:          "info",
		EnableJSONLogs:    true,
		LogFilePath:       "/var/log/neo-service-layer/app.log",
		MaxSizeInMB:       100,
		RetainedFiles:     7,
		EnableCompression: true,
	}
	return logging.NewService(logConfig)
}

func initMetricsService(cfg *config.Config) *metrics.Service {
	metricsConfig := &metrics.Config{
		CollectionInterval: time.Second * 15,
		RetentionPeriod:    time.Hour * 24 * 7, // 7 days
		StorageBackend:     "memory",
		StorageConfig:      make(map[string]string),
	}
	return metrics.NewService(metricsConfig)
}

func initGasBankService(ctx context.Context, cfg *config.Config) (*gasbank.Service, error) {
	gasBankConfig := &gasbank.Config{
		InitialGas:              big.NewInt(1000000000), // 10 GAS
		RefillAmount:            big.NewInt(500000000),  // 5 GAS
		RefillThreshold:         big.NewInt(200000000),  // 2 GAS
		MaxAllocationPerUser:    big.NewInt(100000000),  // 1 GAS
		MinAllocationAmount:     big.NewInt(1000000),    // 0.01 GAS
		StoreType:               "memory",
		MaxAllocationTime:       24 * time.Hour,
		CooldownPeriod:          5 * time.Minute,
		ExpirationCheckInterval: 15 * time.Minute,
		MonitorInterval:         5 * time.Minute,
	}
	return gasbank.NewService(ctx, gasBankConfig)
}

func initPriceFeedService(cfg *config.Config, neoClient *neo.Client) (*pricefeed.Service, error) {
	priceFeedConfig := &pricefeed.Config{}
	return pricefeed.NewService(priceFeedConfig, neoClient)
}

func initTriggerService(cfg *config.Config, neoClient *neo.Client) (*trigger.Service, error) {
	triggerConfig := &trigger.ServiceConfig{
		MaxTriggers:     10,
		MaxExecutions:   100,
		ExecutionWindow: time.Hour * 24,
	}
	return trigger.NewService(triggerConfig, neoClient)
}

func initFunctionsService(cfg *config.Config) (*functions.Service, error) {
	functionsConfig := &functions.Config{
		MaxExecutionTime: time.Second * 30,
	}
	return functions.NewService(functionsConfig)
}

func initSecretsService(cfg *config.Config) (*secrets.Service, error) {
	secretsConfig := &secrets.Config{
		EncryptionKey: "test-encryption-key-12345",
	}
	return secrets.NewService(secretsConfig)
}

func initAutomationService(cfg *config.Config, neoClient *neo.Client, gasBankSvc *gasbank.Service) (*automation.Service, error) {
	automationConfig := &automation.Config{
		CheckInterval:  time.Minute * 5,
		RetryAttempts:  3,
		RetryDelay:     time.Second * 15,
		GasBuffer:      big.NewInt(10000),
		KeeperRegistry: [20]byte{4, 5, 6},
	}
	return automation.NewService(automationConfig, neoClient, gasBankSvc)
}

func initAPIService(cfg *config.Config, gasBankSvc *gasbank.Service, priceFeedSvc *pricefeed.Service, triggerSvc *trigger.Service, functionsSvc *functions.Service, secretsSvc *secrets.Service) (*api.Service, error) {
	apiConfig := &api.Config{
		Port:                 8080,
		EnableCORS:           true,
		MaxRequestBodySize:   10 * 1024 * 1024, // 10MB
		EnableRequestLogging: true,
	}

	deps := &api.Dependencies{
		GasBankService:   gasBankSvc,
		PriceFeedService: priceFeedSvc,
		TriggerService:   triggerSvc,
		FunctionsService: functionsSvc,
		SecretsService:   secretsSvc,
	}

	return api.NewService(apiConfig, deps)
}
