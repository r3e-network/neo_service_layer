package commands

import (
	"fmt"

	"github.com/r3e-network/neo_service_layer/internal/common/logger"
	"github.com/spf13/cobra"
	"github.com/spf13/viper"
)

var rootCmd = &cobra.Command{
	Use:   "neo",
	Short: "Neo service layer CLI",
	Long:  `Command line interface for managing Neo service layer components.`,
}

var cfgFile string

func init() {
	cobra.OnInitialize(initConfig)
	rootCmd.PersistentFlags().StringVar(&cfgFile, "config", "", "config file (default is $HOME/.neo.yaml)")

	// Add commands
	rootCmd.AddCommand(accountsCmd)
	rootCmd.AddCommand(contractsCmd)
	rootCmd.AddCommand(functionsCmd)
	rootCmd.AddCommand(pricefeedsCmd)
	rootCmd.AddCommand(gasbankCmd)
	rootCmd.AddCommand(metricsCmd)
	rootCmd.AddCommand(logsCmd)
	rootCmd.AddCommand(healthCmd)
}

func initConfig() {
	if cfgFile != "" {
		viper.SetConfigFile(cfgFile)
	} else {
		viper.SetConfigName(".neo")
		viper.AddConfigPath("$HOME")
	}

	if err := viper.ReadInConfig(); err != nil {
		fmt.Printf("Error reading config file: %s\n", err)
	}
}

func getConfig() (*viper.Viper, error) {
	return viper.GetViper(), nil
}

func getLogger() logger.Logger {
	level := viper.GetString("log.level")
	if level == "" {
		level = "info"
	}
	return logger.NewLogger(level)
}

// Execute executes the root command
func Execute() error {
	return rootCmd.Execute()
}
