package sandbox

import (
	"time"

	"go.uber.org/zap"
)

// Resource constraints for the sandbox
const (
	DefaultMemoryLimit   = 128 * 1024 * 1024 // 128 MB
	DefaultTimeoutMillis = 5000              // 5 seconds
	DefaultStackSize     = 8 * 1024 * 1024   // 8 MB
	MemoryCheckInterval  = 100 * time.Millisecond
)

// SandboxConfig holds configuration for the JavaScript sandbox
type SandboxConfig struct {
	// Memory limit in bytes
	MemoryLimit int64

	// Execution timeout in milliseconds
	TimeoutMillis int64

	// JavaScript VM stack size in bytes
	StackSize int32

	// Whether to allow network operations
	AllowNetwork bool

	// Whether to allow file I/O operations
	AllowFileIO bool

	// URL of the service layer for interconnected operations
	ServiceLayerURL string

	// Whether to enable interoperability with Neo Service Layer services
	EnableInteroperability bool

	// Logger for sandbox operations
	Logger *zap.Logger
}

// DefaultConfig returns a SandboxConfig with default values
func DefaultConfig() SandboxConfig {
	return SandboxConfig{
		MemoryLimit:            DefaultMemoryLimit,
		TimeoutMillis:          DefaultTimeoutMillis,
		StackSize:              DefaultStackSize,
		AllowNetwork:           false,
		AllowFileIO:            false,
		EnableInteroperability: true,
	}
}

// WithLogger adds a logger to the configuration
func (c SandboxConfig) WithLogger(logger *zap.Logger) SandboxConfig {
	c.Logger = logger
	return c
}

// WithMemoryLimit sets the memory limit
func (c SandboxConfig) WithMemoryLimit(limit int64) SandboxConfig {
	c.MemoryLimit = limit
	return c
}

// WithTimeout sets the execution timeout
func (c SandboxConfig) WithTimeout(timeoutMs int64) SandboxConfig {
	c.TimeoutMillis = timeoutMs
	return c
}

// WithStackSize sets the JavaScript VM stack size
func (c SandboxConfig) WithStackSize(stackSize int32) SandboxConfig {
	c.StackSize = stackSize
	return c
}

// WithNetworkAccess enables or disables network access
func (c SandboxConfig) WithNetworkAccess(allow bool) SandboxConfig {
	c.AllowNetwork = allow
	return c
}

// WithFileIO enables or disables file I/O operations
func (c SandboxConfig) WithFileIO(allow bool) SandboxConfig {
	c.AllowFileIO = allow
	return c
}

// WithInteroperability enables or disables service layer interoperability
func (c SandboxConfig) WithInteroperability(enable bool) SandboxConfig {
	c.EnableInteroperability = enable
	return c
}

// WithServiceLayerURL sets the service layer URL
func (c SandboxConfig) WithServiceLayerURL(url string) SandboxConfig {
	c.ServiceLayerURL = url
	return c
}
