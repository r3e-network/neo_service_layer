package neo

import (
	"context"
	"errors"
	"fmt"
	"math"
	"strings"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/neorpc/result"
	"github.com/nspcc-dev/neo-go/pkg/rpcclient"
	"github.com/nspcc-dev/neo-go/pkg/smartcontract"
	"github.com/nspcc-dev/neo-go/pkg/smartcontract/trigger"
	"github.com/nspcc-dev/neo-go/pkg/util"
	log "github.com/sirupsen/logrus"
)

// ClientConfig represents configuration for the Neo client
type ClientConfig struct {
	NodeURLs []string
	Timeout  int
	Retries  int
}

// DefaultConfig returns default client configuration
func DefaultConfig() *ClientConfig {
	return &ClientConfig{
		NodeURLs: []string{"http://localhost:10332"},
		Timeout:  30,
		Retries:  3,
	}
}

// Client manages connections to Neo RPC nodes
type Client struct {
	config      *ClientConfig
	rpcClients  []*rpcclient.Client
	activeIndex int
}

// NewClient creates a new Neo client
func NewClient(config *ClientConfig) (*Client, error) {
	if config == nil {
		config = DefaultConfig()
	}

	if len(config.NodeURLs) == 0 {
		return nil, errors.New("no Neo node URLs provided")
	}

	c := &Client{
		config:      config,
		rpcClients:  make([]*rpcclient.Client, 0, len(config.NodeURLs)),
		activeIndex: 0,
	}

	// Initialize RPC clients
	for _, url := range config.NodeURLs {
		opts := rpcclient.Options{
			DialTimeout: time.Duration(config.Timeout) * time.Second,
		}
		rpcClient, err := rpcclient.New(context.Background(), url, opts)
		if err != nil {
			log.Warnf("Failed to create RPC client for %s: %v", url, err)
			continue
		}
		c.rpcClients = append(c.rpcClients, rpcClient)
	}

	if len(c.rpcClients) == 0 {
		return nil, errors.New("failed to create any RPC clients")
	}

	log.Infof("Neo client initialized with %d RPC nodes", len(c.rpcClients))
	return c, nil
}

// rotateClient moves to the next available RPC client
func (c *Client) rotateClient() {
	if len(c.rpcClients) <= 1 {
		return
	}
	c.activeIndex = (c.activeIndex + 1) % len(c.rpcClients)
	log.Debugf("Rotated to next Neo RPC node: %d", c.activeIndex)
}

// Close closes all RPC client connections
func (c *Client) Close() {
	for _, rpcClient := range c.rpcClients {
		if rpcClient != nil {
			// Close the client if there's a method to do so
			// Currently neo-go client doesn't have an explicit Close method
		}
	}
}

// InvokeFunction invokes a smart contract method with parameters
func (c *Client) InvokeFunction(contractHash util.Uint160, operation string, params []smartcontract.Parameter, signers []transaction.Signer) (interface{}, error) {
	var lastErr error

	for i := 0; i < c.config.Retries; i++ {
		if i > 0 {
			log.Debugf("Retrying InvokeFunction call (attempt %d/%d)", i+1, c.config.Retries)
			c.rotateClient()
			time.Sleep(time.Duration(math.Pow(2, float64(i))) * time.Second) // Exponential backoff
		}

		// Pass parameters directly
		invokeResult, err := c.rpcClients[c.activeIndex].InvokeFunction(contractHash, operation, params, signers)
		if err == nil {
			return invokeResult, nil // Return the result directly as interface{}
		}

		lastErr = err
		log.Warnf("InvokeFunction call failed: %v", err)
	}
	return nil, fmt.Errorf("all InvokeFunction attempts failed: %w", lastErr)
}

// SendRawTransaction sends a signed transaction to the network
func (c *Client) SendRawTransaction(tx *transaction.Transaction) (util.Uint256, error) {
	var lastErr error
	for i := 0; i < c.config.Retries; i++ {
		if i > 0 {
			log.Debugf("Retrying SendRawTransaction call (attempt %d/%d)", i+1, c.config.Retries)
			c.rotateClient()
			time.Sleep(time.Duration(math.Pow(2, float64(i))) * time.Second) // Exponential backoff
		}

		hash, err := c.rpcClients[c.activeIndex].SendRawTransaction(tx)
		if err == nil {
			return hash, nil
		}

		lastErr = err
		log.Warnf("SendRawTransaction call failed: %v", err)
	}
	return util.Uint256{}, fmt.Errorf("all SendRawTransaction attempts failed: %w", lastErr)
}

// GetApplicationLog retrieves the application log for a transaction
func (c *Client) GetApplicationLog(txHash util.Uint256, trig *trigger.Type) (interface{}, error) {
	var lastErr error
	for i := 0; i < c.config.Retries; i++ {
		if i > 0 {
			log.Debugf("Retrying GetApplicationLog call (attempt %d/%d)", i+1, c.config.Retries)
			c.rotateClient()
			time.Sleep(time.Duration(math.Pow(2, float64(i))) * time.Second) // Exponential backoff
		}

		appLog, err := c.rpcClients[c.activeIndex].GetApplicationLog(txHash, trig)
		if err == nil {
			return appLog, nil // Return the result directly as interface{}
		}

		lastErr = err
		log.Warnf("GetApplicationLog call failed: %v", err)
	}
	return nil, fmt.Errorf("all GetApplicationLog attempts failed: %w", lastErr)
}

// CalculateNetworkFee calculates the network fee for a transaction
func (c *Client) CalculateNetworkFee(tx *transaction.Transaction) (int64, error) {
	var lastErr error
	for i := 0; i < c.config.Retries; i++ {
		if i > 0 {
			log.Debugf("Retrying CalculateNetworkFee call (attempt %d/%d)", i+1, c.config.Retries)
			c.rotateClient()
			time.Sleep(time.Duration(math.Pow(2, float64(i))) * time.Second) // Exponential backoff
		}

		fee, err := c.rpcClients[c.activeIndex].CalculateNetworkFee(tx)
		if err == nil {
			return fee, nil
		}

		lastErr = err
		log.Warnf("CalculateNetworkFee call failed: %v", err)
	}
	return 0, fmt.Errorf("all CalculateNetworkFee attempts failed: %w", lastErr)
}

// GetBlockCount gets the current block height
func (c *Client) GetBlockCount() (uint32, error) {
	var lastErr error
	for i := 0; i < c.config.Retries; i++ {
		if i > 0 {
			log.Debugf("Retrying GetBlockCount call (attempt %d/%d)", i+1, c.config.Retries)
			c.rotateClient()
			time.Sleep(time.Duration(math.Pow(2, float64(i))) * time.Second) // Exponential backoff
		}

		count, err := c.rpcClients[c.activeIndex].GetBlockCount()
		if err == nil {
			return count, nil
		}

		lastErr = err
		log.Warnf("GetBlockCount call failed: %v", err)
	}
	return 0, fmt.Errorf("all GetBlockCount attempts failed: %w", lastErr)
}

// GetNetwork gets the network magic number
func (c *Client) GetNetwork() (uint64, error) {
	var lastErr error
	for i := 0; i < c.config.Retries; i++ {
		if i > 0 {
			log.Debugf("Retrying GetVersion call (attempt %d/%d)", i+1, c.config.Retries)
			c.rotateClient()
			time.Sleep(time.Duration(math.Pow(2, float64(i))) * time.Second) // Exponential backoff
		}

		version, err := c.rpcClients[c.activeIndex].GetVersion()
		if err == nil {
			// Convert network magic to uint64
			return uint64(version.Protocol.Network), nil
		}

		lastErr = err
		log.Warnf("GetVersion call failed: %v", err)
	}
	return 0, fmt.Errorf("all GetVersion attempts failed: %w", lastErr)
}

// WaitForTransaction waits until a transaction is confirmed on the blockchain
func (c *Client) WaitForTransaction(txHashString string, timeout int) (bool, error) {
	deadline := time.Now().Add(time.Duration(timeout) * time.Second)
	ticker := time.NewTicker(2 * time.Second)
	defer ticker.Stop()

	txHash, err := util.Uint256DecodeStringLE(txHashString)
	if err != nil {
		return false, fmt.Errorf("invalid transaction hash string '%s': %w", txHashString, err)
	}

	for {
		select {
		case <-ticker.C:
			// Try to get application log for the transaction
			// Pass txHash (util.Uint256) and nil for trigger (common case for waiting)
			appLogResult, err := c.GetApplicationLog(txHash, nil)
			if err != nil {
				if c.isUnknownTransactionError(err) {
					// Transaction not yet processed, continue waiting
					log.Debugf("Transaction %s not found yet, continuing wait...", txHashString)
					continue
				}
				return false, fmt.Errorf("failed to get application log while waiting for tx %s: %w", txHashString, err)
			}

			// If we got a result (non-nil interface{}), the transaction is confirmed
			// Perform type assertion to log state if possible
			if appLog, ok := appLogResult.(*result.ApplicationLog); ok && len(appLog.Executions) > 0 {
				log.Infof("Transaction %s confirmed! State: %s", txHashString, appLog.Executions[0].VMState)
			} else {
				// Log confirmation without state if type assertion fails or no executions
				log.Infof("Transaction %s confirmed! (Could not determine final state)", txHashString)
			}
			return true, nil

		case <-time.After(time.Until(deadline)):
			return false, fmt.Errorf("timeout waiting for transaction %s", txHashString)
		}
	}
}

// isUnknownTransactionError checks if an error is because a transaction is not yet processed
func (c *Client) isUnknownTransactionError(err error) bool {
	if err == nil {
		return false
	}
	// Check for typical error messages when a transaction hasn't been processed yet
	return strings.Contains(err.Error(), "Unknown transaction") ||
		strings.Contains(err.Error(), "transaction not found") ||
		strings.Contains(err.Error(), "not found") ||
		strings.Contains(err.Error(), "Unknown tx")
}

// Ensure Client implements NeoClient interface
var _ RealNeoClient = (*Client)(nil)
