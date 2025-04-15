package main

import (
	"context"
	"flag"
	"fmt"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/r3e-network/neo_service_layer/internal/common/config"
	"github.com/r3e-network/neo_service_layer/internal/common/logger"
)

func main() {
	// Parse command line flags
	configFile := flag.String("config", "config.yaml", "Path to configuration file")
	workerType := flag.String("type", "", "Worker type (upkeep, trigger, executor)")
	flag.Parse()

	// Initialize logger
	log := logger.NewLogger("info")

	// Validate worker type
	if *workerType == "" {
		log.Error("Worker type is required", nil)
		fmt.Println("Please specify worker type with --type flag (upkeep, trigger, executor)")
		os.Exit(1)
	}

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

	// Start the appropriate worker
	log.Info("Starting worker", map[string]interface{}{
		"type": *workerType,
	})

	switch *workerType {
	case "upkeep":
		startUpkeepWorker(ctx, cfg, log)
	case "trigger":
		startTriggerWorker(ctx, cfg, log)
	case "executor":
		startExecutorWorker(ctx, cfg, log)
	default:
		log.Error("Unknown worker type", map[string]interface{}{
			"type": *workerType,
		})
		fmt.Printf("Unknown worker type: %s\n", *workerType)
		os.Exit(1)
	}

	// Block until context is cancelled
	<-ctx.Done()

	// Allow some time for graceful shutdown
	shutdownCtx, shutdownCancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer shutdownCancel()

	log.Info("Worker shutting down gracefully", nil)

	// Wait for shutdown to complete or timeout
	select {
	case <-shutdownCtx.Done():
		log.Info("Shutdown completed or timed out", nil)
	}

	log.Info("Worker stopped", nil)
}

func startUpkeepWorker(ctx context.Context, cfg *config.Config, log logger.Logger) {
	// Mock implementation - in a real application, we would initialize and start the upkeep worker
	for {
		select {
		case <-ctx.Done():
			return
		case <-time.After(10 * time.Second):
			log.Info("Upkeep worker running", map[string]interface{}{
				"timestamp": time.Now().String(),
			})
		}
	}
}

func startTriggerWorker(ctx context.Context, cfg *config.Config, log logger.Logger) {
	// Mock implementation - in a real application, we would initialize and start the trigger worker
	for {
		select {
		case <-ctx.Done():
			return
		case <-time.After(10 * time.Second):
			log.Info("Trigger worker running", map[string]interface{}{
				"timestamp": time.Now().String(),
			})
		}
	}
}

func startExecutorWorker(ctx context.Context, cfg *config.Config, log logger.Logger) {
	// Mock implementation - in a real application, we would initialize and start the executor worker
	for {
		select {
		case <-ctx.Done():
			return
		case <-time.After(10 * time.Second):
			log.Info("Executor worker running", map[string]interface{}{
				"timestamp": time.Now().String(),
			})
		}
	}
}
