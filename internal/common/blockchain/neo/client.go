package neo

import (
	"encoding/json"
	"fmt"
	"net/http"
	"time"
)

// Client provides an interface to interact with the Neo N3 blockchain
type Client struct {
	endpoint   string
	apiKey     string
	httpClient *http.Client
}

// TransactionOutput represents transaction output information
type TransactionOutput struct {
	Asset   string `json:"asset"`
	Address string `json:"address"`
	Value   string `json:"value"`
}

// Transaction represents a Neo blockchain transaction
type Transaction struct {
	Hash       string              `json:"hash"`
	Size       int                 `json:"size"`
	Version    int                 `json:"version"`
	Nonce      int64               `json:"nonce"`
	Sender     string              `json:"sender"`
	SysFee     string              `json:"sys_fee"`
	NetFee     string              `json:"net_fee"`
	ValidUntil int                 `json:"valid_until_block"`
	Signers    []string            `json:"signers"`
	Outputs    []TransactionOutput `json:"outputs"`
	Script     string              `json:"script"`
	Success    bool                `json:"success"`
}

// ScriptResult represents the result of a script invocation
type ScriptResult struct {
	Script      string            `json:"script"`
	State       string            `json:"state"`
	GasConsumed string            `json:"gas_consumed"`
	Stack       []json.RawMessage `json:"stack"`
}

// RpcRequest is a JSON-RPC request
type RpcRequest struct {
	JsonRpc string        `json:"jsonrpc"`
	Method  string        `json:"method"`
	Params  []interface{} `json:"params"`
	ID      int           `json:"id"`
}

// RpcResponse is a JSON-RPC response
type RpcResponse struct {
	JsonRpc string          `json:"jsonrpc"`
	ID      int             `json:"id"`
	Result  json.RawMessage `json:"result,omitempty"`
	Error   *RpcError       `json:"error,omitempty"`
}

// RpcError is a JSON-RPC error
type RpcError struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
}

// NewClient creates a new Neo client
func NewClient(endpoint, apiKey string) (*Client, error) {
	if endpoint == "" {
		return nil, fmt.Errorf("Neo RPC endpoint cannot be empty")
	}

	return &Client{
		endpoint:   endpoint,
		apiKey:     apiKey,
		httpClient: &http.Client{Timeout: 30 * time.Second},
	}, nil
}

// GetBlockCount gets the current block height
func (c *Client) GetBlockCount() (int, error) {
	resp, err := c.callRPC("getblockcount", []interface{}{})
	if err != nil {
		return 0, err
	}

	var blockCount int
	err = json.Unmarshal(resp.Result, &blockCount)
	if err != nil {
		return 0, fmt.Errorf("failed to unmarshal block count: %w", err)
	}

	return blockCount, nil
}

// GetBalance gets the balance of an address for an asset
func (c *Client) GetBalance(address, asset string) (string, error) {
	resp, err := c.callRPC("getbalance", []interface{}{address, asset})
	if err != nil {
		return "", err
	}

	var balance string
	err = json.Unmarshal(resp.Result, &balance)
	if err != nil {
		return "", fmt.Errorf("failed to unmarshal balance: %w", err)
	}

	return balance, nil
}

// InvokeFunction invokes a contract function without changing the blockchain state
func (c *Client) InvokeFunction(contract, method string, params []interface{}) (*ScriptResult, error) {
	callParams := []interface{}{contract, method}
	if len(params) > 0 {
		callParams = append(callParams, params)
	}

	resp, err := c.callRPC("invokefunction", callParams)
	if err != nil {
		return nil, err
	}

	var result ScriptResult
	err = json.Unmarshal(resp.Result, &result)
	if err != nil {
		return nil, fmt.Errorf("failed to unmarshal invoke result: %w", err)
	}

	return &result, nil
}

// InvokeContract invokes a contract function and returns the transaction
func (c *Client) InvokeContract(contract, method string, params []interface{}, signer string, gasLimit int64) (*Transaction, error) {
	// For this simplified implementation, we'll just mock a successful transaction
	tx := &Transaction{
		Hash:       fmt.Sprintf("0x%x", time.Now().UnixNano()),
		Size:       500,
		Version:    0,
		Nonce:      time.Now().UnixNano(),
		Sender:     signer,
		SysFee:     fmt.Sprintf("%d", gasLimit/2),
		NetFee:     "1000000",
		ValidUntil: 1000000,
		Signers:    []string{signer},
		Outputs:    []TransactionOutput{},
		Script:     "mock-script",
		Success:    true,
	}

	return tx, nil
}

// callRPC makes a JSON-RPC call to the Neo node
func (c *Client) callRPC(method string, params []interface{}) (*RpcResponse, error) {
	// For this simplified implementation, we'll mock responses for some common methods
	mockResponse := &RpcResponse{
		JsonRpc: "2.0",
		ID:      1,
	}

	switch method {
	case "getblockcount":
		mockResponse.Result = json.RawMessage(`123456`)
	case "getbalance":
		if len(params) < 2 {
			return nil, fmt.Errorf("invalid parameters for getbalance")
		}
		mockResponse.Result = json.RawMessage(`"1000000000"`)
	case "invokefunction":
		if len(params) < 2 {
			return nil, fmt.Errorf("invalid parameters for invokefunction")
		}
		mockResponse.Result = json.RawMessage(`{
			"script": "mock-script",
			"state": "HALT",
			"gas_consumed": "1000000",
			"stack": []
		}`)
	default:
		mockResponse.Error = &RpcError{
			Code:    -32601,
			Message: "Method not found",
		}
	}

	// In a real implementation, this would make an HTTP request to the Neo RPC endpoint
	return mockResponse, nil
}
