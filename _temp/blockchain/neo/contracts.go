package neo

import (
	"context"
	"fmt"
	
	"github.com/Jim8y/neo-service-layer/internal/common/logger"
	"github.com/neo-project/neo-go/pkg/vm/stackitem"
)

// InvokeFunction invokes a smart contract function and returns the result
func (c *Client) InvokeFunction(scriptHash, operation string, params []any) ([]stackitem.Item, error) {
	ctx, cancel := context.WithTimeout(context.Background(), c.timeout)
	defer cancel()
	
	c.log.Debug("Invoking contract function", 
		logger.Field{Key: "script_hash", Value: scriptHash},
		logger.Field{Key: "operation", Value: operation},
	)
	
	result, err := c.rpcClient.InvokeFunction(ctx, scriptHash, operation, params, nil)
	if err != nil {
		return nil, fmt.Errorf("failed to invoke function: %w", err)
	}
	
	if result.State != "HALT" {
		return nil, fmt.Errorf("invocation failed with state: %s, exception: %s", result.State, result.Exception)
	}
	
	return result.Stack, nil
}

// GetContractState returns the state of a smart contract
func (c *Client) GetContractState(scriptHash string) (*ContractState, error) {
	ctx, cancel := context.WithTimeout(context.Background(), c.timeout)
	defer cancel()
	
	state, err := c.rpcClient.GetContractState(ctx, scriptHash)
	if err != nil {
		return nil, fmt.Errorf("failed to get contract state: %w", err)
	}
	
	contractState := &ContractState{
		Hash:       state.Hash,
		Name:       state.Manifest.Name,
		Version:    state.Manifest.ABI.Version,
		Parameters: state.Manifest.ABI.Parameters,
		ReturnType: state.Manifest.ABI.ReturnType,
	}
	
	return contractState, nil
}

// ContractState represents the state of a smart contract
type ContractState struct {
	Hash       string
	Name       string
	Version    string
	Parameters []ContractParameter
	ReturnType string
}

// ContractParameter represents a smart contract parameter
type ContractParameter struct {
	Name string
	Type string
}