package types

import (
	"math/big"
	"time"
)

// BlockchainType represents a type of blockchain
type BlockchainType string

// Blockchain types
const (
	Neo     BlockchainType = "neo"
	Ethereum BlockchainType = "ethereum"
)

// Block represents a blockchain block
type Block struct {
	Height       uint64
	Hash         string
	PrevHash     string
	Timestamp    time.Time
	Transactions []Transaction
	Size         int
	MerkleRoot   string
	StateRoot    string
	Nonce        uint64
	Difficulty   *big.Int
	ExtraData    []byte
}

// Transaction represents a blockchain transaction
type Transaction struct {
	Hash       string
	From       string
	To         string
	Value      *big.Int
	Data       []byte
	Gas        uint64
	GasPrice   *big.Int
	Nonce      uint64
	Timestamp  time.Time
	BlockHash  string
	BlockIndex uint64
	Status     TransactionStatus
}

// TransactionStatus represents the status of a transaction
type TransactionStatus string

// Transaction statuses
const (
	Pending   TransactionStatus = "pending"
	Confirmed TransactionStatus = "confirmed"
	Failed    TransactionStatus = "failed"
)

// TransactionReceipt represents a transaction receipt
type TransactionReceipt struct {
	TransactionHash string
	BlockHash       string
	BlockIndex      uint64
	GasUsed         uint64
	Status          bool
	Logs            []EventLog
	ContractAddress string
}

// EventLog represents an event log
type EventLog struct {
	Address        string
	Topics         []string
	Data           []byte
	BlockHash      string
	BlockIndex     uint64
	TransactionHash string
	LogIndex       uint
}

// ContractABI represents a contract ABI (Application Binary Interface)
type ContractABI struct {
	Name       string
	Methods    []ContractMethod
	Events     []ContractEvent
	Properties []ContractProperty
}

// ContractMethod represents a contract method
type ContractMethod struct {
	Name       string
	Parameters []ContractParameter
	ReturnType string
	Constant   bool
	Payable    bool
}

// ContractEvent represents a contract event
type ContractEvent struct {
	Name       string
	Parameters []ContractParameter
}

// ContractProperty represents a contract property
type ContractProperty struct {
	Name string
	Type string
}

// ContractParameter represents a contract parameter
type ContractParameter struct {
	Name string
	Type string
}

// DeploymentParams represents parameters for deploying a contract
type DeploymentParams struct {
	Code            []byte
	ABI             ContractABI
	Constructor     string
	ConstructorArgs []interface{}
	GasLimit        uint64
	GasPrice        *big.Int
	Value           *big.Int
}

// CallParams represents parameters for calling a contract method
type CallParams struct {
	To        string
	Method    string
	Args      []interface{}
	Value     *big.Int
	GasLimit  uint64
	GasPrice  *big.Int
	Nonce     uint64
	Signature []byte
}

// Account represents a blockchain account
type Account struct {
	Address    string
	PrivateKey []byte
	PublicKey  []byte
	Balance    *big.Int
	Nonce      uint64
}

// Network represents a blockchain network
type Network struct {
	Name        string
	ChainID     uint64
	Currency    string
	Symbol      string
	RPC         string
	BlockTime   time.Duration
	Explorers   []string
	Type        BlockchainType
	TestNet     bool
}

// Fee represents a blockchain fee
type Fee struct {
	Gas      uint64
	GasPrice *big.Int
	Total    *big.Int
}

// CalculateTotal calculates the total fee
func (f *Fee) CalculateTotal() *big.Int {
	if f.Total != nil {
		return f.Total
	}
	return new(big.Int).Mul(new(big.Int).SetUint64(f.Gas), f.GasPrice)
}