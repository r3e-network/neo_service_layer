package commands

import (
	"fmt"
	"sync"

	"github.com/will/neo_service_layer/internal/common/config"
	"github.com/will/neo_service_layer/internal/common/logger"
)

// CommandContext holds shared command context and configuration
type CommandContext struct {
	configFile string
	config     *config.Config
	configOnce sync.Once
	logger     logger.Logger
	loggerOnce sync.Once
}

var globalContext = &CommandContext{}

// SetConfigFile sets the configuration file path
func SetConfigFile(path string) {
	globalContext.configFile = path
}

// GetConfig returns the global configuration instance
func GetConfig() (*config.Config, error) {
	var err error
	globalContext.configOnce.Do(func() {
		if globalContext.configFile == "" {
			globalContext.config = config.DefaultConfig()
			return
		}
		globalContext.config, err = config.LoadConfig(globalContext.configFile)
	})
	if err != nil {
		return nil, fmt.Errorf("failed to load config: %w", err)
	}
	return globalContext.config, nil
}

// GetLogger returns the global logger instance
func GetLogger() logger.Logger {
	globalContext.loggerOnce.Do(func() {
		cfg, err := GetConfig()
		if err != nil {
			// If we can't get config, create a default logger
			globalContext.logger = logger.NewLogger("info")
			return
		}
		globalContext.logger = logger.NewLogger(cfg.LogLevel)
	})
	return globalContext.logger
}
