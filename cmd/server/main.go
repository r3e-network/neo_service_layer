package main

/*
SERVICE LAYER IMPLEMENTATION DOCUMENTATION

CURRENT STATUS:
- Several critical services are temporarily disabled due to dependency issues:
  1. Logging service: Package structure issues - needs restructuring of models
  2. PriceFeed service: Missing neo-go dependency
  3. Trigger service: Missing neo-go dependency

STEPS TO FULLY ENABLE SERVICES:

1. Fix neo-go Dependency:
   - Run: go get github.com/nspcc-dev/neo-go
   - Uncomment the necessary imports at the top of this file
   - Uncomment the implementation sections in each service's init function

2. Fix Logging Package Structure:
   - The fix_logging_package.sh script should have created the models directory structure
   - Verify models directory exists: internal/services/logging/models/models.go
   - Run: go mod tidy && go mod vendor
   - If problems persist, manually check imports in service.go

3. When Dependencies Are Fixed:
   - Uncomment each service initialization function's implementation
   - Build and test the server

WARNING: Do not attempt to run the server until these issues are resolved.
*/

import (
	"context"
	"flag"
	"fmt"
	"math/big"
	"os"
	"os/signal"
	"strconv"
	"strings"
	"syscall"
	"time"

	// Temporarily comment out these imports until dependencies are fixed
	"github.com/nspcc-dev/neo-go/pkg/config/netmode"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/common/config"
	"github.com/will/neo_service_layer/internal/common/logger"
	"github.com/will/neo_service_layer/internal/core/neo"
	"github.com/will/neo_service_layer/internal/services/api"
	"github.com/will/neo_service_layer/internal/services/functions"
	"github.com/will/neo_service_layer/internal/services/gasbank"
	gasbankmodels "github.com/will/neo_service_layer/internal/services/gasbank/models"
	"github.com/will/neo_service_layer/internal/services/logging"

	// Comment this out until we can get the imports properly working
	loggingmodels "github.com/will/neo_service_layer/internal/services/logging/models"
	"github.com/will/neo_service_layer/internal/services/metrics"
	"github.com/will/neo_service_layer/internal/services/pricefeed"
	"github.com/will/neo_service_layer/internal/services/secrets"
	"github.com/will/neo_service_layer/internal/services/trigger"
	"github.com/will/neo_service_layer/internal/tee"
)

// To fix neo-go package import errors, run:
//   go get github.com/nspcc-dev/neo-go

var log logger.Logger // Use interface type, not pointer

func main() {
	// CRITICAL FIXES NEEDED:
	// 1. Fix neo-go dependency by running:
	//    go get github.com/nspcc-dev/neo-go
	//
	// 2. Fix logging package structure by:
	//    a. Creating directory: mkdir -p internal/services/logging/models
	//    b. Creating file: internal/services/logging/models/models.go
	//       - Move all model definitions from logging/models.go there
	//       - Change package declaration to "package models"
	//    c. Update imports in main.go (uncomment loggingmodels import line)
	//    d. Update initLoggingService function (uncomment implementation)
	//
	// 3. After fixing dependencies, uncomment the full implementations of:
	//    - initPriceFeedService
	//    - initTriggerService

	// Parse command line flags
	configFile := flag.String("config", "config.yaml", "Path to configuration file")
	flag.Parse()

	// Initialize default logger first
	log = logger.NewLogger("info") // Assign to global var

	// Load configuration
	cfg, err := config.LoadConfig(*configFile)
	if err != nil {
		// Use fmt for initial logging if logger failed
		fmt.Printf("Failed to load configuration: %v, file: %s\n", err, *configFile)
		os.Exit(1)
	}

	// Re-initialize logger with config after loading config successfully
	if cfg.LogLevel != "" {
		log = logger.NewLogger(cfg.LogLevel)
	} else {
		log.Warn("LogLevel missing or empty in config, using default level 'info' from initial setup", nil)
	}

	// Create a context that will be cancelled on SIGINT or SIGTERM
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	// Setup signal handling
	sigCh := make(chan os.Signal, 1)
	signal.Notify(sigCh, syscall.SIGINT, syscall.SIGTERM)
	go func() {
		sig := <-sigCh
		log.Info("Received signal, shutting down", map[string]interface{}{ // Call methods directly on interface
			"signal": sig.String(),
		})
		cancel()
	}()

	// --- Initialize Neo Client ---
	if len(cfg.Neo.RPC) == 0 {
		log.Error("Neo configuration missing RPC URLs", nil)
		os.Exit(1)
	}
	neoClientConfig := &neo.ClientConfig{
		NodeURLs: cfg.Neo.RPC, // Use RPC field from config.NeoConfig
		Timeout:  30,          // Default or fetch from elsewhere if needed
		Retries:  3,           // Default or fetch from elsewhere if needed
	}
	neoClient, err := neo.NewClient(neoClientConfig)
	if err != nil {
		log.Error("Failed to initialize Neo client", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}
	defer neoClient.Close()

	// --- Initialize Services ---
	logSvc, err := initLoggingService(cfg)
	if err != nil {
		log.Error("Failed to initialize logging service", map[string]interface{}{"error": err.Error()})
	}

	metricsSvc := initMetricsService(cfg)

	gasBankSvc, err := initGasBankService(ctx, cfg)
	if err != nil {
		log.Error("Failed to initialize gasbank service", map[string]interface{}{"error": err.Error()})
		os.Exit(1)
	}

	priceFeedSvc, err := initPriceFeedService(cfg, neoClient)
	if err != nil {
		log.Error("Failed to initialize price feed service", map[string]interface{}{"error": err.Error()})
		os.Exit(1)
	}

	// Initialize TEE Provider (using dummy for now)
	teeProvider, err := tee.NewDummyTEEProvider(cfg.Security.JWTSecret)
	if err != nil {
		log.Error("Failed to initialize TEE provider", map[string]interface{}{"error": err.Error()})
		os.Exit(1)
	}

	secretsSvc, err := initSecretsService(cfg, teeProvider)
	if err != nil {
		log.Error("Failed to initialize secrets service", map[string]interface{}{"error": err.Error()})
		os.Exit(1)
	}

	// Initialize Functions Service (depends on Secrets, GasBank, PriceFeed)
	functionsSvc, err := initFunctionsService(cfg, gasBankSvc, priceFeedSvc, secretsSvc)
	if err != nil {
		log.Error("Failed to initialize functions service", map[string]interface{}{"error": err.Error()})
		os.Exit(1)
	}

	// Initialize Trigger Service (depends on Functions, Wallet)
	triggerSvc, err := initTriggerService(cfg, neoClient, functionsSvc)
	if err != nil {
		log.Error("Failed to initialize trigger service", map[string]interface{}{"error": err.Error()})
		os.Exit(1)
	}

	// automationSvc, err := initAutomationService(cfg, neoClient, gasBankSvc) // Commented out due to missing package
	// if err != nil {
	// 	log.Error("Failed to initialize automation service", map[string]interface{}{
	// 		"error": err.Error(),
	// 	})
	// 	os.Exit(1)
	// }

	// Initialize API service (pass nil for automationSvc)
	apiSvc, err := initAPIService(cfg, gasBankSvc, priceFeedSvc, triggerSvc, functionsSvc, secretsSvc)
	if err != nil {
		log.Error("Failed to initialize API service", map[string]interface{}{
			"error": err.Error(),
		})
		os.Exit(1)
	}

	// --- Start Services ---
	if logSvc != nil {
		// Check if logSvc has a Start method. service.go doesn't show one.
		// Assuming no Start method for logging service based on service.go contents.
	}
	if metricsSvc != nil {
		if err := metricsSvc.Start(ctx); err != nil {
			log.Error("Failed to start metrics service", map[string]interface{}{"error": err.Error()})
			os.Exit(1)
		}
	}
	if gasBankSvc != nil {
		// Assuming gasbank service Start(ctx) exists based on interface
		if err := gasBankSvc.Start(ctx); err != nil {
			log.Error("Failed to start gasbank service", map[string]interface{}{"error": err.Error()})
			os.Exit(1)
		}
	}
	if apiSvc != nil {
		if err := apiSvc.Start(); err != nil {
			log.Error("Failed to start API service", map[string]interface{}{"error": err.Error()})
			os.Exit(1)
		}
	}
	// Automation service commented out

	if triggerSvc != nil {
		if err := triggerSvc.Start(ctx); err != nil {
			log.Error("Failed to start trigger service", map[string]interface{}{"error": err.Error()})
			os.Exit(1)
		}
	}
	if priceFeedSvc != nil {
		if err := priceFeedSvc.Start(ctx); err != nil {
			log.Error("Failed to start price feed service", map[string]interface{}{"error": err.Error()})
			os.Exit(1)
		}
	}
	// Add Start calls for Functions, Secrets if needed (currently none found)

	log.Info("All services initialized and started successfully", map[string]interface{}{})

	// --- Wait for Shutdown Signal ---
	<-ctx.Done()

	log.Info("Shutting down services...", nil)

	// --- Graceful Shutdown ---
	shutdownCtx, shutdownCancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer shutdownCancel()

	// Shutdown services in reverse order of startup (or based on dependencies)
	if apiSvc != nil {
		apiSvc.Stop(shutdownCtx)
	}
	// Automation service commented out

	if triggerSvc != nil {
		triggerSvc.Stop(shutdownCtx)
	}
	if priceFeedSvc != nil {
		// PriceFeed Stop() takes context
		if err := priceFeedSvc.Stop(shutdownCtx); err != nil {
			log.Error("Failed to stop price feed service", map[string]interface{}{"error": err.Error()})
		}
	}
	if gasBankSvc != nil {
		// Assuming gasbank service Stop(ctx) exists based on interface
		if err := gasBankSvc.Stop(shutdownCtx); err != nil {
			log.Error("Failed to stop gasbank service", map[string]interface{}{"error": err.Error()})
		}
	}
	if functionsSvc != nil {
		// No Stop/Shutdown found for Functions service
	}
	if secretsSvc != nil {
		// No Stop/Shutdown found for Secrets service
	}
	if metricsSvc != nil {
		if err := metricsSvc.Stop(); err != nil {
			log.Error("Failed to stop metrics service", map[string]interface{}{"error": err.Error()})
		}
	}
	if logSvc != nil {
		logSvc.Shutdown(shutdownCtx)
	}

	log.Info("Server stopped gracefully", nil)
	fmt.Println("Server stopped")
}

func initLoggingService(cfg *config.Config) (*logging.Service, error) {
	// TEMPORARY FIX: We're not using the full logging service due to package structure issues
	log.Warn("Using simplified logging. Logging service temporarily disabled until package structure is fixed", map[string]interface{}{
		"suggestion": "Fix package structure and imports",
	})

	// Return nil for now as we're just using the global logger
	return nil, nil
}

func initMetricsService(cfg *config.Config) *metrics.Service {
	metricsCfg := cfg.Services.Metrics // Get the config section from main config
	// Use metrics.Config from the metrics service package
	metricsSvcConfig := &metrics.Config{
		CollectionInterval: metricsCfg.CollectionInterval,
		RetentionPeriod:    metricsCfg.RetentionPeriod,
		StorageBackend:     metricsCfg.ExportFormat,  // Map ExportFormat to StorageBackend
		StorageConfig:      metricsCfg.StorageConfig, // Use config from cfg instead of empty map
	}
	service, err := metrics.NewService(metricsSvcConfig)
	if err != nil {
		log.Error("Failed to initialize metrics service", map[string]interface{}{"error": err.Error()})
		return nil // Return nil if initialization fails
	}
	log.Info("Metrics service initialized", nil)
	return service
}

func initGasBankService(ctx context.Context, cfg *config.Config) (gasbank.Service, error) {
	gasBankCfg := cfg.Services.GasBank
	gasBankConfig := &gasbankmodels.Config{
		InitialGas:              convertFloatToBigInt(gasBankCfg.InitialGas, 8),
		RefillAmount:            convertFloatToBigInt(gasBankCfg.RefillAmount, 8),
		RefillThreshold:         convertFloatToBigInt(gasBankCfg.RefillThreshold, 8),
		MaxAllocationPerUser:    convertFloatToBigInt(gasBankCfg.MaxAllocationPerUser, 8),
		MinAllocationAmount:     convertFloatToBigInt(gasBankCfg.MinAllocationAmount, 8),
		StoreType:               gasBankCfg.StoreType,
		StorePath:               gasBankCfg.StorePath,
		MaxAllocationTime:       gasBankCfg.MaxAllocationTime,
		CooldownPeriod:          gasBankCfg.CooldownPeriod,
		MonitorInterval:         gasBankCfg.MonitorInterval,
		ExpirationCheckInterval: gasBankCfg.ExpirationCheckInterval,
		EnableUserBalances:      gasBankCfg.EnableUserBalances,
		NeoNodeURL:              gasBankCfg.NeoNodeURL,
		WalletPath:              gasBankCfg.WalletPath,
		WalletPass:              gasBankCfg.WalletPass,
		MinDepositAmount:        convertFloatToBigInt(gasBankCfg.MinDepositAmount, 8),
		WithdrawalFee:           convertFloatToBigInt(gasBankCfg.WithdrawalFee, 8),
	}
	serviceImpl, err := gasbank.NewServiceImpl(gasBankConfig)
	if err != nil {
		log.Error("Failed to initialize gasbank service", map[string]interface{}{"error": err.Error()})
		return nil, fmt.Errorf("failed to initialize gasbank service: %w", err)
	}
	log.Info("GasBank service initialized", nil)
	return serviceImpl, nil
}

func initPriceFeedService(cfg *config.Config, neoClient *neo.Client) (*pricefeed.Service, error) {
	// TEMPORARY SOLUTION: Return nil until neo-go imports are fixed
	log.Warn("PriceFeed service disabled until neo-go package imports are fixed", map[string]interface{}{
		"fix": "Run: go get github.com/nspcc-dev/neo-go",
	})

	priceFeedCfg := cfg.Services.PriceFeed

	// Convert ContractHash string from config to util.Uint160
	contractHash, err := util.Uint160DecodeStringLE(priceFeedCfg.ContractHash)
	if err != nil {
		return nil, fmt.Errorf("invalid price feed ContractHash '%s' in config: %w", priceFeedCfg.ContractHash, err)
	}
	// Check if the hash is zero
	if contractHash == (util.Uint160{}) {
		// Contract hash is mandatory for the service
		return nil, fmt.Errorf("price feed ContractHash cannot be empty or zero in config")
	}

	// Use pricefeed.Config from pricefeed/service package
	priceFeedSvcConfig := &pricefeed.Config{
		UpdateInterval: priceFeedCfg.UpdateInterval,
		PriceContract:  contractHash, // Assign the converted Uint160 hash
	}
	service, err := pricefeed.NewService(priceFeedSvcConfig, neoClient) // Pass *neo.Client
	if err != nil {
		log.Error("Failed to initialize price feed service", map[string]interface{}{"error": err.Error()})
		return nil, fmt.Errorf("failed to initialize price feed service: %w", err)
	}
	log.Info("PriceFeed service initialized", nil)
	return service, nil
}

func initTriggerService(cfg *config.Config, neoClient neo.RealNeoClient, functionsService *functions.Service) (*trigger.Service, error) {
	// TEMPORARY SOLUTION: Return nil until neo-go imports are fixed
	log.Warn("Trigger service disabled until neo-go package imports are fixed", map[string]interface{}{
		"fix": "Run: go get github.com/nspcc-dev/neo-go",
	})

	triggerCfg := cfg.Services.Trigger
	walletCfg := cfg.Services.Wallet

	if walletCfg.Path == "" {
		log.Warn("Wallet path not specified in config, TriggerService might fail signing", nil)
	}

	// Use trigger.ServiceConfig from trigger package
	triggerSvcConfig := &trigger.ServiceConfig{
		MaxTriggers:   triggerCfg.MaxTriggers,
		MaxExecutions: triggerCfg.MaxExecutions,
		// Use fields added to config.TriggerConfig
		ExecutionWindow:       triggerCfg.ExecutionWindow,
		MaxConcurrentTriggers: triggerCfg.MaxConcurrentTriggers,
	}

	var networkMagic netmode.Magic
	switch strings.ToLower(walletCfg.Network) {
	case "mainnet":
		networkMagic = netmode.MainNet
	case "testnet":
		networkMagic = netmode.TestNet
	default:
		if magicU, err := parseUint32(walletCfg.Network); err == nil {
			networkMagic = netmode.Magic(magicU)
		} else {
			return nil, fmt.Errorf("invalid network '%s'", walletCfg.Network)
		}
	}

	walletConfig := &wallet.Config{
		WalletPath:     walletCfg.Path,
		Password:       walletCfg.Password,
		AddressVersion: walletCfg.AddressVersion,
		NetworkMagic:   networkMagic,
	}
	walletService := wallet.NewWalletService(walletConfig)

	// Assuming trigger.NewService requires neoClient, functionsService, and walletService
	service, err := trigger.NewService(triggerSvcConfig, neoClient, functionsService, walletService)
	if err != nil {
		log.Error("Failed to initialize trigger service", map[string]interface{}{"error": err.Error()})
		return nil, fmt.Errorf("failed to initialize trigger service: %w", err)
	}
	log.Info("Trigger service initialized", nil)
	return service, nil
}

func initFunctionsService(cfg *config.Config,
	gasBankSvc gasbank.Service,
	priceFeedSvc *pricefeed.Service,
	secretsSvc secrets.Service,
) (*functions.Service, error) {
	functionsCfg := cfg.Services.Functions
	// Use functions.Config directly from functions package
	functionsSvcConfig := &functions.Config{
		MaxExecutionTime: functionsCfg.MaxTimeout,
		MaxMemoryLimit:   int64(functionsCfg.MaxMemory) * 1024 * 1024,
		DefaultRuntime:   functionsCfg.DefaultRuntime,
		// Use fields added to config.FunctionsConfig
		MaxFunctionSize:        functionsCfg.MaxFunctionSize,
		EnableNetworkAccess:    functionsCfg.EnableNetworkAccess,
		EnableFileIO:           functionsCfg.EnableFileIO,
		ServiceLayerURL:        functionsCfg.ServiceLayerURL,
		EnableInteroperability: functionsCfg.EnableInteroperability,

		// Assign service references from parameters
		GasBankService:   gasBankSvc,
		PriceFeedService: priceFeedSvc,
		SecretsService:   secretsSvc,
		// TriggerService:     triggerSvc, // Need to pass triggerSvc here
		// TransactionService: nil, // Need implementation
	}
	service, err := functions.NewService(functionsSvcConfig)
	if err != nil {
		log.Error("Failed to initialize functions service", map[string]interface{}{"error": err.Error()})
		return nil, fmt.Errorf("failed to initialize functions service: %w", err)
	}
	log.Info("Functions service initialized", nil)
	return service, nil
}

func initSecretsService(cfg *config.Config, teeProvider secrets.TEESecurityProvider) (secrets.Service, error) {
	secretsCfg := cfg.Services.Secrets
	secretsSvcConfig := &secrets.Config{
		EncryptionKey:       secretsCfg.EncryptionKey,
		MaxSecretSize:       secretsCfg.MaxSecretSize,
		MaxSecretsPerUser:   secretsCfg.MaxSecretsPerUser,
		SecretExpiryEnabled: secretsCfg.SecretExpiryEnabled,
		DefaultTTL:          secretsCfg.DefaultTTL,
		StoreType:           secretsCfg.StoreType,
		StorePath:           secretsCfg.StorePath,
	}

	var secretStore secrets.Store
	switch strings.ToLower(secretsSvcConfig.StoreType) {
	case "memory", "":
		secretStore = secrets.NewMemoryStore()
	case "badger":
		storePath := secretsSvcConfig.StorePath
		if storePath == "" {
			storePath = "./secrets_db"
		}
		var storeErr error
		secretStore, storeErr = secrets.NewBadgerStore(storePath)
		if storeErr != nil {
			return nil, fmt.Errorf("failed to init badger store: %w", storeErr)
		}
	default:
		return nil, fmt.Errorf("unsupported secrets store type: %s", secretsSvcConfig.StoreType)
	}
	log.Info("Initialized secrets store", map[string]interface{}{"type": secretsSvcConfig.StoreType})

	// Pass the initialized TEE provider to NewServiceImpl
	serviceImpl, err := secrets.NewServiceImpl(secretsSvcConfig, secretStore, teeProvider)
	if err != nil {
		log.Error("Failed to initialize secrets service", map[string]interface{}{"error": err.Error()})
		return nil, fmt.Errorf("failed to initialize secrets service: %w", err)
	}
	log.Info("Secrets service initialized", nil)
	return serviceImpl, nil
}

/* // Comment out initAutomationService as the package is missing/unused
func initAutomationService(cfg *config.Config, neoClient neo.RealNeoClient, gasBankSvc *gasbank.Service) (*automation.Service, error) {
	return nil, fmt.Errorf("automation service not implemented")
}
*/

func initAPIService(cfg *config.Config, gasBankSvc gasbank.Service, priceFeedSvc *pricefeed.Service, triggerSvc *trigger.Service, functionsSvc *functions.Service, secretsSvc secrets.Service) (*api.Service, error) {
	apiCfg := cfg.API
	// Use api.Config from api package
	apiConfig := &api.Config{
		Port: apiCfg.Port,
		// These fields aren't in the APIConfig from api.go
		// Provide default values for fields not in config
		EnableCORS:           true,             // Default value
		AllowedOrigins:       []string{"*"},    // Default value
		MaxRequestBodySize:   10 * 1024 * 1024, // Default value (10MB)
		EnableRequestLogging: true,             // Default value
		JWTSecret:            cfg.Security.JWTSecret,
	}

	deps := &api.Dependencies{
		GasBankService:   gasBankSvc,
		PriceFeedService: priceFeedSvc,
		TriggerService:   triggerSvc,
		FunctionsService: functionsSvc,
		SecretsService:   secretsSvc,
		// AutomationService: nil,
	}

	service, err := api.NewService(apiConfig, deps)
	if err != nil {
		log.Error("Failed to initialize API service", map[string]interface{}{"error": err.Error()})
		return nil, fmt.Errorf("failed to initialize API service: %w", err)
	}
	log.Info("API service initialized", nil)
	return service, nil
}

func convertFloatToBigInt(f float64, decimals int) *big.Int {
	bf := big.NewFloat(f)
	mul := big.NewFloat(1)
	mul.SetMantExp(mul, decimals)
	bf.Mul(bf, mul)

	result := new(big.Int)
	bf.Int(result)
	return result
}

func parseUint32(s string) (uint32, error) {
	val, err := strconv.ParseUint(s, 10, 32)
	if err != nil {
		return 0, err
	}
	return uint32(val), nil
}

// Create mock clients for development
// ... (Commented out code remains unchanged) ...
/*
func createMockServices() (*neo.MockNeoClient, *wallet.MockWalletService) {
	// ...
}

func setupServices() {
	// ...
}
*/
