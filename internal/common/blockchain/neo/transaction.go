package neo

import (
	"context"
	"fmt"
	"time"
)

// TransactionAttributeType represents the type of a transaction attribute
type TransactionAttributeType byte

const (
	// HighPriority sets the transaction as high priority
	HighPriority TransactionAttributeType = 0x01
	// OracleResponse sets the transaction as an oracle response
	OracleResponse TransactionAttributeType = 0x11
)

// TransactionAttribute represents an attribute of a transaction
type TransactionAttribute struct {
	Type  TransactionAttributeType `json:"type"`
	Value interface{}              `json:"value"`
}

// TransactionBuilder helps build Neo transactions
type TransactionBuilder struct {
	Script       []byte
	Signers      []Signer
	Attributes   []TransactionAttribute
	SystemFee    int64
	NetworkFee   int64
	ValidUntilBlock int
}

// Signer represents a transaction signer
type Signer struct {
	Account      string   `json:"account"`
	Scopes       string   `json:"scopes"`
	AllowedContracts []string `json:"allowedContracts,omitempty"`
	AllowedGroups    []string `json:"allowedGroups,omitempty"`
}

// NewTransactionBuilder creates a new transaction builder
func (c *Client) NewTransactionBuilder() *TransactionBuilder {
	return &TransactionBuilder{
		Signers:      make([]Signer, 0),
		Attributes:   make([]TransactionAttribute, 0),
		SystemFee:    0,
		NetworkFee:   0,
		ValidUntilBlock: 0,
	}
}

// SetScript sets the transaction script
func (b *TransactionBuilder) SetScript(script []byte) *TransactionBuilder {
	b.Script = script
	return b
}

// AddSigner adds a signer to the transaction
func (b *TransactionBuilder) AddSigner(account string, scope string, allowedContracts []string, allowedGroups []string) *TransactionBuilder {
	b.Signers = append(b.Signers, Signer{
		Account:          account,
		Scopes:           scope,
		AllowedContracts: allowedContracts,
		AllowedGroups:    allowedGroups,
	})
	return b
}

// AddAttribute adds an attribute to the transaction
func (b *TransactionBuilder) AddAttribute(attrType TransactionAttributeType, value interface{}) *TransactionBuilder {
	b.Attributes = append(b.Attributes, TransactionAttribute{
		Type:  attrType,
		Value: value,
	})
	return b
}

// SetSystemFee sets the system fee for the transaction
func (b *TransactionBuilder) SetSystemFee(fee int64) *TransactionBuilder {
	b.SystemFee = fee
	return b
}

// SetNetworkFee sets the network fee for the transaction
func (b *TransactionBuilder) SetNetworkFee(fee int64) *TransactionBuilder {
	b.NetworkFee = fee
	return b
}

// SetValidUntilBlock sets the valid until block for the transaction
func (b *TransactionBuilder) SetValidUntilBlock(block int) *TransactionBuilder {
	b.ValidUntilBlock = block
	return b
}

// Build builds the transaction
func (b *TransactionBuilder) Build(client *Client) (*UnsignedTransaction, error) {
	// In a real implementation, this would build a proper transaction object
	// and set all the necessary fields
	
	// For testing, we create a mock unsigned transaction
	tx := &UnsignedTransaction{
		Hash:           fmt.Sprintf("0x%x", time.Now().UnixNano()),
		Version:        0,
		Nonce:          int(time.Now().Unix()),
		Sender:         "",
		SystemFee:      b.SystemFee,
		NetworkFee:     b.NetworkFee,
		ValidUntilBlock: b.ValidUntilBlock,
		Attributes:     b.Attributes,
		Script:         b.Script,
		Signers:        b.Signers,
	}
	
	return tx, nil
}

// UnsignedTransaction represents an unsigned Neo transaction
type UnsignedTransaction struct {
	Hash            string
	Version         int
	Nonce           int
	Sender          string
	SystemFee       int64
	NetworkFee      int64
	ValidUntilBlock int
	Attributes      []TransactionAttribute
	Script          []byte
	Signers         []Signer
}

// Sign signs a transaction with the provided private key
func (c *Client) Sign(tx *UnsignedTransaction, privateKey string) (*Transaction, error) {
	// In a real implementation, this would sign the transaction with the provided private key
	// and create a signed transaction
	
	// For testing, we create a mock signed transaction
	signedTx := &Transaction{
		Hash:         tx.Hash,
		Size:         500,
		Version:      tx.Version,
		Nonce:        int64(tx.Nonce),
		Sender:       tx.Signers[0].Account,
		SysFee:       fmt.Sprintf("%d", tx.SystemFee),
		NetFee:       fmt.Sprintf("%d", tx.NetworkFee),
		ValidUntil:   tx.ValidUntilBlock,
		Signers:      make([]string, 0),
		Outputs:      make([]TransactionOutput, 0),
		Script:       fmt.Sprintf("%x", tx.Script),
		Success:      true,
	}
	
	for _, signer := range tx.Signers {
		signedTx.Signers = append(signedTx.Signers, signer.Account)
	}
	
	return signedTx, nil
}

// SendTransaction sends a transaction to the Neo network
func (c *Client) SendTransaction(ctx context.Context, tx *Transaction) (string, error) {
	// In a real implementation, this would send the transaction to the Neo network
	// and return the transaction hash
	
	// For testing, we just return the transaction hash
	return tx.Hash, nil
}

// GetTransaction gets a transaction from the Neo network
func (c *Client) GetTransaction(ctx context.Context, txHash string) (*Transaction, error) {
	// In a real implementation, this would get the transaction from the Neo network
	
	// For testing, we create a mock transaction
	tx := &Transaction{
		Hash:       txHash,
		Size:       500,
		Version:    0,
		Nonce:      time.Now().UnixNano(),
		Sender:     "NKuyBkoGdZZSLyPbJEetheRhMjeznFZszf",
		SysFee:     "1000000",
		NetFee:     "1000000",
		ValidUntil: 1000000,
		Signers:    []string{"NKuyBkoGdZZSLyPbJEetheRhMjeznFZszf"},
		Outputs:    []TransactionOutput{},
		Script:     "mock-script",
		Success:    true,
	}
	
	return tx, nil
}

// GetTransactionReceipt gets a transaction receipt from the Neo network
func (c *Client) GetTransactionReceipt(ctx context.Context, txHash string) (*TransactionReceipt, error) {
	// In a real implementation, this would get the transaction receipt from the Neo network
	
	// For testing, we create a mock transaction receipt
	receipt := &TransactionReceipt{
		TransactionID: txHash,
		BlockNumber:   123456,
		BlockHash:     "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
		Confirmations: 10,
		Timestamp:     time.Now().Unix(),
		GasUsed:       1000000,
		Success:       true,
	}
	
	return receipt, nil
}

// TransactionReceipt represents a Neo transaction receipt
type TransactionReceipt struct {
	TransactionID string   `json:"transaction_id"`
	BlockNumber   int      `json:"block_number"`
	BlockHash     string   `json:"block_hash"`
	Confirmations int      `json:"confirmations"`
	Timestamp     int64    `json:"timestamp"`
	GasUsed       int64    `json:"gas_used"`
	Events        []Event  `json:"events"`
	Success       bool     `json:"success"`
	Error         string   `json:"error,omitempty"`
}

// Event represents a Neo blockchain event
type Event struct {
	Contract  string                 `json:"contract"`
	EventName string                 `json:"event_name"`
	Parameters map[string]interface{} `json:"parameters"`
}

// WaitForTransaction waits for a transaction to be confirmed
func (c *Client) WaitForTransaction(ctx context.Context, txHash string, confirmations int) (*TransactionReceipt, error) {
	// In a real implementation, this would poll for the transaction receipt until it has the required confirmations
	
	// For testing, we create a mock transaction receipt
	receipt := &TransactionReceipt{
		TransactionID: txHash,
		BlockNumber:   123456,
		BlockHash:     "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
		Confirmations: confirmations,
		Timestamp:     time.Now().Unix(),
		GasUsed:       1000000,
		Success:       true,
	}
	
	return receipt, nil
}