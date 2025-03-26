package main

import (
	"flag"
	"fmt"
	"os"
	"strings"

	"github.com/will/neo_service_layer/internal/common/config"
	"github.com/will/neo_service_layer/internal/common/logger"
)

func main() {
	// Parse command line arguments
	configFile := flag.String("config", "config.yaml", "Path to config file")
	verbose := flag.Bool("verbose", false, "Enable verbose logging")
	flag.Parse()

	// Initialize logger
	logLevel := "info"
	if *verbose {
		logLevel = "debug"
	}
	log := logger.NewLogger(logLevel)

	// Load configuration
	cfg, err := config.LoadConfig(*configFile)
	if err != nil {
		log.Error("Failed to load configuration", map[string]interface{}{
			"error": err.Error(),
			"file":  *configFile,
		})
		os.Exit(1)
	}

	// Process command
	args := flag.Args()
	if len(args) == 0 {
		printUsage()
		os.Exit(1)
	}

	command := strings.ToLower(args[0])
	switch command {
	case "version":
		fmt.Println("Neo Service Layer CLI v0.1.0")
	case "status":
		fmt.Printf("Service status: Running\n")
		fmt.Printf("Config loaded from: %s\n", *configFile)
		fmt.Printf("Environment: %s\n", cfg.Environment)
	case "help":
		printUsage()
	default:
		fmt.Printf("Unknown command: %s\n", command)
		printUsage()
		os.Exit(1)
	}
}

func printUsage() {
	fmt.Println("Usage: cli [options] command")
	fmt.Println("\nOptions:")
	flag.PrintDefaults()
	fmt.Println("\nCommands:")
	fmt.Println("  version     Display version information")
	fmt.Println("  status      Check service status")
	fmt.Println("  help        Show this help message")
}
