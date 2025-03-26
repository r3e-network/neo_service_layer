package neo

import (
	"context"
	"fmt"
	"sync"

	"github.com/nspcc-dev/neo-go/pkg/rpcclient"
)

// Client represents a Neo N3 client
type Client struct {
	config     *Config
	rpcClients []*rpcclient.Client
	mu         sync.RWMutex
	currentIdx int
}

// NewClient creates a new Neo N3 client
func NewClient(config *Config) (*Client, error) {
	if len(config.NodeURLs) == 0 {
		return nil, fmt.Errorf("no node URLs provided")
	}

	c := &Client{
		config:     config,
		rpcClients: make([]*rpcclient.Client, len(config.NodeURLs)),
	}

	for i, url := range config.NodeURLs {
		rpcClient, err := rpcclient.New(context.Background(), url, rpcclient.Options{})
		if err != nil {
			return nil, fmt.Errorf("failed to create RPC client for %s: %w", url, err)
		}
		c.rpcClients[i] = rpcClient
	}

	return c, nil
}

// GetClient returns the current RPC client
func (c *Client) GetClient() *rpcclient.Client {
	c.mu.RLock()
	defer c.mu.RUnlock()
	return c.rpcClients[c.currentIdx]
}

// RotateClient rotates to the next available RPC client
func (c *Client) RotateClient() {
	c.mu.Lock()
	defer c.mu.Unlock()
	c.currentIdx = (c.currentIdx + 1) % len(c.rpcClients)
}

// Close closes all RPC clients
func (c *Client) Close() {
	c.mu.Lock()
	defer c.mu.Unlock()

	for _, client := range c.rpcClients {
		client.Close()
	}
}
