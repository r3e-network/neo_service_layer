package commands

import (
	"github.com/spf13/cobra"
	"github.com/will/neo_service_layer/internal/common/config"
	"github.com/will/neo_service_layer/internal/common/logger"
)

var (
	configFile string
	verbose    bool
	rootCmd    = &cobra.Command{
		Use:   "cli",
		Short: "Neo N3 Service Layer CLI",
		Long:  `Command line interface for Neo N3 Service Layer`,
	}
)

func init() {
	rootCmd.PersistentFlags().StringVarP(&configFile, "config", "c", "config.yaml", "Path to config file")
	rootCmd.PersistentFlags().BoolVarP(&verbose, "verbose", "v", false, "Enable verbose logging")

	// Add subcommands
	rootCmd.AddCommand(
		newAccountsCmd(),
		newSecretsCmd(),
		newContractsCmd(),
		newFunctionsCmd(),
		newTriggersCmd(),
		newPriceFeedCmd(),
		newGasBankCmd(),
		newMetricsCmd(),
		newLogsCmd(),
		newHealthCmd(),
	)
}

// Execute executes the root command
func Execute() error {
	return rootCmd.Execute()
}

// getConfig loads configuration from file
func getConfig() (*config.Config, error) {
	return config.LoadConfig(configFile)
}

// getLogger creates a new logger instance
func getLogger() *logger.Logger {
	level := "info"
	if verbose {
		level = "debug"
	}
	return logger.NewLogger(level)
}