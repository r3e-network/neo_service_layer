# Neo Integration Guide

This document outlines how the service layer interacts with the Neo N3 blockchain, including contract deployment, transaction management, and gas handling.

## Architecture Overview

The Neo Service Layer integrates with the Neo N3 blockchain through several components:

1. **Neo Client**: Manages the connection to Neo N3 nodes
2. **Transaction Manager**: Handles transaction creation, signing, and broadcasting
3. **Contract Manager**: Manages smart contract deployment and invocation
4. **Service Components**: Specialized services (GasBank, PriceFeed, Trigger) that use the blockchain

```
┌─────────────────────────────────────────┐
│              Service Layer              │
│                                         │
│  ┌─────────┐  ┌────────┐  ┌──────────┐  │
│  │ GasBank │  │PriceFeed│  │ Trigger  │  │
│  └────┬────┘  └────┬────┘  └────┬─────┘  │
│       │            │            │        │
│  ┌────┴────────────┴────────────┴─────┐  │
│  │          Contract Manager          │  │
│  └────────────────┬──────────────────┘  │
│                   │                      │
│  ┌────────────────┴──────────────────┐  │
│  │         Transaction Manager        │  │
│  └────────────────┬──────────────────┘  │
│                   │                      │
│  ┌────────────────┴──────────────────┐  │
│  │             Neo Client             │  │
│  └────────────────┬──────────────────┘  │
└────────────────────┼───────────────────┘
                     │
                     ▼
             ┌───────────────┐
             │   Neo RPC     │
             │    Server     │
             └───────────────┘
```

## Neo Client

The Neo Client establishes a connection to the Neo N3 blockchain. It manages the connection pool, handles RPC calls, and provides a unified interface for interacting with the blockchain.

### Configuration

```go
type ClientConfig struct {
    RPCEndpoint string    // RPC endpoint URL
    WS          bool      // Whether to use WebSocket
    MaxConns    int       // Maximum number of connections
    Timeout     time.Duration // Request timeout
}
```

### Key Methods

```go
// Initialize a new Neo client
func NewClient(config ClientConfig) (*Client, error)

// Get current blockchain height
func (c *Client) GetBlockCount() (uint32, error)

// Get a block by height or hash
func (c *Client) GetBlock(indexOrHash interface{}) (*block.Block, error)

// Get transaction by hash
func (c *Client) GetTransaction(hash util.Uint256) (*transaction.Transaction, error)

// Get application log for a transaction
func (c *Client) GetApplicationLog(hash util.Uint256) (*result.ApplicationLog, error)

// Invoke a contract method without sending a transaction
func (c *Client) TestInvoke(script []byte) (*result.Invoke, error)
```

## Transaction Manager

The Transaction Manager creates, signs, and broadcasts transactions to the Neo blockchain. It handles gas calculation, signature collection, and transaction lifecycle.

### Configuration

```go
type TxManagerConfig struct {
    Client         *Client      // Neo client instance
    SystemFee      *big.Int     // Default system fee
    NetworkFee     *big.Int     // Default network fee
    NetworkMagic   uint32       // Network magic number
    MaxGasInvoke   *big.Int     // Maximum gas for invoke transactions
    DefaultAccount *wallet.Account // Default account for signing
}
```

### Transaction Types

1. **Contract Deployment**: Deploying smart contracts to the blockchain
2. **Invocation**: Calling methods on deployed contracts
3. **Transfer**: Transferring assets
4. **Claim**: Claiming GAS

### Key Methods

```go
// Create and sign a transaction
func (tm *TxManager) CreateAndSignTx(script []byte, signers []transaction.Signer) (*transaction.Transaction, error)

// Calculate fees for a transaction
func (tm *TxManager) CalculateFee(tx *transaction.Transaction) (*big.Int, error)

// Sign a transaction with the specified account
func (tm *TxManager) SignTx(tx *transaction.Transaction, account *wallet.Account) error

// Send a transaction to the blockchain
func (tm *TxManager) SendTx(tx *transaction.Transaction) (util.Uint256, error)

// Wait for transaction confirmation
func (tm *TxManager) WaitForTransaction(hash util.Uint256, timeout time.Duration) (*result.ApplicationLog, error)
```

## Contract Manager

The Contract Manager facilitates smart contract deployment and interaction. It handles contract compilation, deployment transactions, and method invocation.

### Configuration

```go
type ContractManagerConfig struct {
    TxManager      *TxManager   // Transaction manager
    Client         *Client      // Neo client
    DefaultAccount *wallet.Account // Default account for signing
}
```

### Contract Deployment

To deploy a contract, you need:
1. NEF file (Neo Executable Format)
2. Contract manifest (JSON)
3. Account with sufficient GAS

```go
// Deploy a contract with the specified manifest and NEF
func (cm *ContractManager) DeployContract(nef []byte, manifest []byte, signers []transaction.Signer) (util.Uint160, error)
```

### Contract Invocation

```go
// Invoke a contract method
func (cm *ContractManager) InvokeFunction(
    scriptHash util.Uint160,
    method string,
    params []smartcontract.Parameter,
    signers []transaction.Signer,
) (*transaction.Transaction, error)

// Test invoke a contract method without sending the transaction
func (cm *ContractManager) TestInvokeFunction(
    scriptHash util.Uint160,
    method string,
    params []smartcontract.Parameter,
) (*result.Invoke, error)
```

## Script Hashes and Addresses

NEO uses two address formats:
1. **Script Hash** (util.Uint160): Internal representation used in contract code
2. **Address** (string): User-friendly format used in wallets and applications

### Converting Between Formats

```go
// Convert address to script hash
scriptHash, err := address.StringToUint160("NYxb4fSZVKAz8YsgaPK2WkT3KcAE9b3Vag")

// Convert script hash to address
addr := address.Uint160ToString(scriptHash)
```

## Gas Bank Service

The Gas Bank service manages GAS allocation for users within the service. It handles:
1. Initial GAS allocation for new users
2. GAS refills when a user's balance drops below a threshold
3. GAS reclamation when a user's service is terminated

### Configuration

```go
type GasBankConfig struct {
    TxManager       *TxManager // Transaction manager
    ContractManager *ContractManager // Contract manager
    InitialGas      *big.Int   // Initial GAS allocation
    RefillAmount    *big.Int   // Amount to refill
    RefillThreshold *big.Int   // Threshold to trigger refill
    GasToken        util.Uint160 // GAS token hash
}
```

### Key Methods

```go
// Allocate initial GAS to a user
func (g *GasBankService) AllocateGas(userAddress util.Uint160) (*big.Int, error)

// Release GAS from a user
func (g *GasBankService) ReleaseGas(userAddress util.Uint160) error

// Check GAS balance of a user
func (g *GasBankService) CheckBalance(userAddress util.Uint160) (*big.Int, error)

// Refill GAS for a user if below threshold
func (g *GasBankService) RefillGas(userAddress util.Uint160) (*big.Int, error)
```

## Price Feed Service

The Price Feed service publishes and retrieves price data from the blockchain. It handles:
1. Publishing price updates from oracles
2. Retrieving current prices for assets
3. Maintaining price history

### Configuration

```go
type PriceFeedConfig struct {
    TxManager        *TxManager // Transaction manager
    ContractManager  *ContractManager // Contract manager
    UpdateInterval   time.Duration // Price update interval
    PriceContract    util.Uint160 // Price oracle contract
}
```

### Key Methods

```go
// Publish a price update to the blockchain
func (p *PriceFeedService) PublishPrice(symbol string, price *big.Int, publisher util.Uint160) error

// Get the current price for a symbol
func (p *PriceFeedService) GetPrice(symbol string) (*big.Int, error)

// Get price history for a symbol
func (p *PriceFeedService) GetPriceHistory(symbol string, limit int) ([]PricePoint, error)
```

## Trigger Service

The Trigger service manages automated execution based on conditions. It handles:
1. Trigger creation (time-based, event-based)
2. Trigger execution
3. Execution history

### Configuration

```go
type TriggerConfig struct {
    TxManager       *TxManager // Transaction manager
    ContractManager *ContractManager // Contract manager
    MaxTriggers     int        // Maximum triggers per user
    MaxExecutions   int        // Maximum executions per trigger
    ExecutionWindow time.Duration // Time window for executions
}
```

### Key Methods

```go
// Create a new trigger
func (t *TriggerService) CreateTrigger(trigger Trigger, owner util.Uint160) (string, error)

// Update an existing trigger
func (t *TriggerService) UpdateTrigger(triggerID string, trigger Trigger, owner util.Uint160) error

// Delete a trigger
func (t *TriggerService) DeleteTrigger(triggerID string, owner util.Uint160) error

// Execute a trigger
func (t *TriggerService) ExecuteTrigger(triggerID string, owner util.Uint160) (string, error)

// Get triggers for a user
func (t *TriggerService) GetTriggers(owner util.Uint160) ([]Trigger, error)
```

## Integration Testing

When testing Neo blockchain integration, consider these approaches:

1. **Unit Tests with Mocks**: For testing service logic without blockchain interaction
2. **Integration Tests with Neo Private Net**: For testing with a local Neo blockchain
3. **End-to-End Tests with Testnet**: For testing with the public testnet

### Example Integration Test

```go
func TestContractDeployment(t *testing.T) {
    // Initialize client
    client, err := initNeoClient()
    if err != nil {
        t.Fatalf("Failed to initialize Neo client: %v", err)
    }
    
    // Create test account
    account, err := createTestAccount()
    if err != nil {
        t.Fatalf("Failed to create test account: %v", err)
    }
    
    // Initialize transaction manager
    txManager := NewTxManager(TxManagerConfig{
        Client:       client,
        SystemFee:    big.NewInt(1000000),
        NetworkFee:   big.NewInt(1000000),
        NetworkMagic: 844378958, // Private net magic
    })
    
    // Initialize contract manager
    contractManager := NewContractManager(ContractManagerConfig{
        TxManager: txManager,
        Client:    client,
    })
    
    // Read contract files
    nefFile, err := os.ReadFile("test_contract.nef")
    manifestFile, err := os.ReadFile("test_contract.manifest.json")
    
    // Create signers
    signers := []transaction.Signer{
        {
            Account: account.ScriptHash(),
            Scopes:  transaction.CalledByEntry,
        },
    }
    
    // Deploy contract
    contractHash, err := contractManager.DeployContract(
        nefFile,
        manifestFile,
        signers,
    )
    
    if err != nil {
        t.Fatalf("Contract deployment failed: %v", err)
    }
    
    // Verify contract exists
    result, err := client.TestInvoke(nefFile)
    if err != nil || result.State != "HALT" {
        t.Fatalf("Contract verification failed: %v", err)
    }
}
```

## Common Issues and Solutions

### Transaction Rejections

1. **Insufficient GAS**: Ensure the account has enough GAS for system fees and network fees
2. **Incorrect Signatures**: Verify that all required signers have signed the transaction
3. **Script Verification Failure**: Check that the contract code is valid and parameters are correct

### Contract Deployment Errors

1. **Invalid NEF**: Ensure the NEF file is correctly compiled for Neo N3
2. **Invalid Manifest**: Verify the manifest JSON is correctly formatted
3. **Contract Already Exists**: Check if a contract with the same hash already exists

### Script Hash Conversion

1. **Little-Endian vs Big-Endian**: Be aware of byte order when converting between formats
2. **Address Format**: Ensure the correct network prefix is used for addresses

## Best Practices

1. **Error Handling**: Implement robust error handling for blockchain interactions
2. **Gas Management**: Estimate and monitor gas usage to prevent transaction failures
3. **Idempotency**: Design operations to be idempotent to handle network issues
4. **Confirmation Checking**: Wait for sufficient confirmations before considering transactions final
5. **Script Hash Handling**: Use proper utility functions for address and script hash conversions
6. **Timeouts**: Implement proper timeouts for blockchain operations
7. **Retry Mechanisms**: Use exponential backoff for retrying failed operations