package transaction

import (
	"fmt"
	"time"

	"github.com/google/uuid"
)

// TransactionStatus represents the status of a transaction
type TransactionStatus string

const (
	// StatusCreated indicates the transaction has been created but not yet signed
	StatusCreated TransactionStatus = "created"
	// StatusSigned indicates the transaction has been signed but not yet sent
	StatusSigned TransactionStatus = "signed"
	// StatusSent indicates the transaction has been sent to the blockchain
	StatusSent TransactionStatus = "sent"
	// StatusPending indicates the transaction is pending confirmation
	StatusPending TransactionStatus = "pending"
	// StatusConfirmed indicates the transaction has been confirmed
	StatusConfirmed TransactionStatus = "confirmed"
	// StatusFailed indicates the transaction has failed
	StatusFailed TransactionStatus = "failed"
)

// TransactionType represents the type of a transaction
type TransactionType string

const (
	// TypeTransfer indicates a simple token transfer transaction
	TypeTransfer TransactionType = "transfer"
	// TypeInvoke indicates a contract invocation transaction
	TypeInvoke TransactionType = "invoke"
	// TypeDeploy indicates a contract deployment transaction
	TypeDeploy TransactionType = "deploy"
)

// Transaction represents a blockchain transaction
type Transaction struct {
	// ID is the unique identifier for the transaction
	ID string `json:"id"`
	// Hash is the blockchain transaction hash (once sent)
	Hash string `json:"hash,omitempty"`
	// Status is the current status of the transaction
	Status TransactionStatus `json:"status"`
	// Type is the type of transaction
	Type TransactionType `json:"type"`
	// From is the sender address
	From string `json:"from"`
	// To is the recipient address
	To string `json:"to,omitempty"`
	// Value is the amount to transfer (for transfer transactions)
	Value string `json:"value,omitempty"`
	// Asset is the asset being transferred (for transfer transactions)
	Asset string `json:"asset,omitempty"`
	// Contract is the contract address (for invoke transactions)
	Contract string `json:"contract,omitempty"`
	// Method is the contract method to call (for invoke transactions)
	Method string `json:"method,omitempty"`
	// Params are the parameters for the contract method (for invoke transactions)
	Params []interface{} `json:"params,omitempty"`
	// Data is the raw transaction data
	Data string `json:"data,omitempty"`
	// GasLimit is the maximum gas to use
	GasLimit int64 `json:"gasLimit,omitempty"`
	// GasPrice is the gas price to use
	GasPrice string `json:"gasPrice,omitempty"`
	// Network is the blockchain network (mainnet, testnet)
	Network string `json:"network"`
	// Signed indicates whether the transaction has been signed
	Signed bool `json:"signed"`
	// RawData is the raw signed transaction data
	RawData string `json:"rawData,omitempty"`
	// CreatedAt is when the transaction was created
	CreatedAt time.Time `json:"createdAt"`
	// UpdatedAt is when the transaction was last updated
	UpdatedAt time.Time `json:"updatedAt"`
}

// NewTransaction creates a new transaction with the given configuration
func NewTransaction(config map[string]interface{}, owner string) (*Transaction, error) {
	// Extract transaction type
	typeStr, ok := config["type"].(string)
	if !ok || typeStr == "" {
		return nil, ErrInvalidTransactionType
	}

	// Create transaction with basic fields
	tx := &Transaction{
		ID:        uuid.New().String(),
		Status:    StatusCreated,
		Type:      TransactionType(typeStr),
		From:      owner,
		Network:   "testnet", // Default to testnet
		Signed:    false,
		CreatedAt: time.Now(),
		UpdatedAt: time.Now(),
	}

	// Extract common fields
	if to, ok := config["to"].(string); ok {
		tx.To = to
	}

	if value, ok := config["value"].(string); ok {
		tx.Value = value
	} else if value, ok := config["value"].(float64); ok {
		tx.Value = formatFloat(value)
	}

	if asset, ok := config["asset"].(string); ok {
		tx.Asset = asset
	}

	if data, ok := config["data"].(string); ok {
		tx.Data = data
	}

	if gasLimit, ok := config["gasLimit"].(float64); ok {
		tx.GasLimit = int64(gasLimit)
	}

	if gasPrice, ok := config["gasPrice"].(string); ok {
		tx.GasPrice = gasPrice
	}

	if network, ok := config["network"].(string); ok {
		tx.Network = network
	}

	// Extract type-specific fields
	switch tx.Type {
	case TypeInvoke:
		if contract, ok := config["contract"].(string); ok {
			tx.Contract = contract
		}

		if method, ok := config["method"].(string); ok {
			tx.Method = method
		}

		if params, ok := config["params"].([]interface{}); ok {
			tx.Params = params
		}
	}

	return tx, nil
}

// ToMap converts a Transaction to a map[string]interface{}
func (tx *Transaction) ToMap() map[string]interface{} {
	result := map[string]interface{}{
		"id":        tx.ID,
		"status":    tx.Status,
		"type":      tx.Type,
		"from":      tx.From,
		"network":   tx.Network,
		"signed":    tx.Signed,
		"createdAt": tx.CreatedAt.Unix(),
		"updatedAt": tx.UpdatedAt.Unix(),
	}

	if tx.Hash != "" {
		result["hash"] = tx.Hash
	}

	if tx.To != "" {
		result["to"] = tx.To
	}

	if tx.Value != "" {
		result["value"] = tx.Value
	}

	if tx.Asset != "" {
		result["asset"] = tx.Asset
	}

	if tx.Data != "" {
		result["data"] = tx.Data
	}

	if tx.GasLimit > 0 {
		result["gasLimit"] = tx.GasLimit
	}

	if tx.GasPrice != "" {
		result["gasPrice"] = tx.GasPrice
	}

	if tx.Contract != "" {
		result["contract"] = tx.Contract
	}

	if tx.Method != "" {
		result["method"] = tx.Method
	}

	if tx.Params != nil {
		result["params"] = tx.Params
	}

	if tx.RawData != "" {
		result["rawData"] = tx.RawData
	}

	return result
}

// formatFloat formats a float64 as a string
func formatFloat(value float64) string {
	return fmt.Sprintf("%f", value)
}