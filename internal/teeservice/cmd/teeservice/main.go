// Package main provides the entry point for the TEE service.
package main

import (
	"context"
	"flag"
	"fmt"
	"os"
	"os/signal"
	"syscall"

	"github.com/neo_service_layer/internal/teeservice/config"
	"github.com/neo_service_layer/internal/teeservice/core"
	"github.com/neo_service_layer/internal/teeservice/server"
	"github.com/neo_service_layer/pkg/logging"
	"github.com/r3e-network/neo_service_layer/pkg/teeservice"
)

var (
	configFile = flag.String("config", "", "Path to configuration file")
	logLevel   = flag.String("log-level", "info", "Logging level (debug, info, warn, error)")
	httpPort   = flag.Int("http-port", 8080, "HTTP server port")
	grpcPort   = flag.Int("grpc-port", 8081, "gRPC server port")
)

func main() {
	flag.Parse()

	// Initialize logger
	logger, err := logging.NewLogger(*logLevel)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Failed to initialize logger: %v\n", err)
		os.Exit(1)
	}
	logger.Info("Starting TEE service")

	// Load configuration
	cfg, err := loadConfig(*configFile)
	if err != nil {
		logger.Error("Failed to load configuration", "error", err)
		os.Exit(1)
	}

	// Create the TEE service
	service := core.NewTEEService(logger)

	// Initialize the service
	ctx := context.Background()
	if err := service.Initialize(ctx, cfg); err != nil {
		logger.Error("Failed to initialize TEE service", "error", err)
		os.Exit(1)
	}
	logger.Info("TEE service initialized")

	// Start the API server
	apiServer, err := server.NewAPIServer(service, *httpPort, *grpcPort, logger)
	if err != nil {
		logger.Error("Failed to create API server", "error", err)
		os.Exit(1)
	}

	// Start the server in a goroutine
	errCh := make(chan error, 1)
	go func() {
		if err := apiServer.Start(); err != nil {
			errCh <- err
		}
	}()

	// Wait for signal to exit
	sigCh := make(chan os.Signal, 1)
	signal.Notify(sigCh, syscall.SIGINT, syscall.SIGTERM)

	// Wait for signal or error
	select {
	case err := <-errCh:
		logger.Error("Server error", "error", err)
	case sig := <-sigCh:
		logger.Info("Received signal", "signal", sig)
	}

	// Shut down gracefully
	logger.Info("Shutting down...")
	if err := apiServer.Shutdown(ctx); err != nil {
		logger.Error("Failed to shut down API server", "error", err)
	}

	// Shut down all enclaves
	if err := service.GetManager().ShutdownAll(ctx); err != nil {
		logger.Error("Failed to shut down all enclaves", "error", err)
	}

	logger.Info("TEE service shut down")
}

// loadConfig loads the configuration from the specified file
func loadConfig(configFile string) (*teeservice.Config, error) {
	if configFile == "" {
		// Use default configuration
		return config.DefaultConfig(), nil
	}

	// Load from file
	return config.LoadFromFile(configFile)
}
