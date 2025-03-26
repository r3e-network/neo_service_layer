package neo

import (
	"context"
	"fmt"
	"time"
	
	"github.com/Jim8y/neo-service-layer/internal/common/logger"
	"github.com/neo-project/neo-go/pkg/rpc/client"
)

// Client represents a Neo N3 client
type Client struct {
	rpcClient *client.Client
	network   string
	log       *logger.Logger
	timeout   time.Duration
}

// Config holds configuration for the Neo client
type Config struct {
	RPCURL  string
	Network string
	Timeout time.Duration
}

// DefaultConfig returns default Neo client configuration
func DefaultConfig() Config {
	return Config{
		RPCURL:  "http://localhost:10332",
		Network: "privnet",
		Timeout: 30 * time.Second,
	}
}

// NewClient creates a new Neo N3 client
func NewClient(cfg Config, log *logger.Logger) (*Client, error) {
	// Apply default timeout if not set
	if cfg.Timeout == 0 {
		cfg.Timeout = 30 * time.Second
	}

	log.Info("Creating Neo N3 client", 
		logger.Field{Key: "rpc_url", Value: cfg.RPCURL},
		logger.Field{Key: "network", Value: cfg.Network},
	)
	
	// Create RPC client
	opts := client.Options{
		Endpoint:    cfg.RPCURL,
		DialTimeout: 10, // seconds
	}
	
	rpcClient, err := client.New(context.Background(), opts)
	if err != nil {
		return nil, fmt.Errorf("failed to create Neo RPC client: %w", err)
	}
	
	// Verify connection
	version, err := rpcClient.GetVersion()
	if err != nil {
		return nil, fmt.Errorf("failed to connect to Neo RPC server: %w", err)
	}
	
	log.Info("Connected to Neo N3 node", 
		logger.Field{Key: "version", Value: version.UserAgent},
	)
	
	return &Client{
		rpcClient: rpcClient,
		network:   cfg.Network,
		log:       log,
		timeout:   cfg.Timeout,
	}, nil
}

// GetBlockCount returns the current block height
func (c *Client) GetBlockCount() (uint32, error) {
	ctx, cancel := context.WithTimeout(context.Background(), c.timeout)
	defer cancel()
	
	blockCount, err := c.rpcClient.GetBlockCount(ctx)
	if err != nil {
		return 0, fmt.Errorf("failed to get block count: %w", err)
	}
	
	return blockCount, nil
}

// Close closes the Neo client connection
func (c *Client) Close() error {
	if c.rpcClient != nil {
		if err := c.rpcClient.Close(); err != nil {
			return fmt.Errorf("failed to close Neo RPC client: %w", err)
		}
	}
	return nil
}