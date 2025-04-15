# Gas Bank Service Implementation Guide

*Last Updated: 2025-04-14*

**Last Updated**: 2024-01-05

## Overview

This document provides detailed implementation information for the Gas Bank Service, including technology stack, code structure, data models, and key components. It serves as the primary reference for developers working on the service implementation.

## Technology Stack

The Gas Bank Service is built using the following technologies:

- **Language**: Go (version 1.21+)
- **API**: gRPC with Protocol Buffers
- **Database**: PostgreSQL (primary data store)
- **Caching**: Redis (for gas price caching and rate limiting)
- **Message Queue**: Kafka (for event processing)
- **Monitoring**: Prometheus and Grafana
- **Containerization**: Docker and Kubernetes
- **Blockchain Interaction**: In-house SDK with connection to multiple RPC providers

## Code Structure

The service follows a clean architecture pattern with clear separation of concerns:

```
gasbank/
├── api/                       # API definitions and handlers
│   ├── proto/                 # Protocol buffer definitions
│   ├── grpc/                  # gRPC server implementation
│   └── middleware/            # API middleware (auth, logging, etc.)
├── core/                      # Core domain logic
│   ├── account/               # Gas account management
│   ├── transaction/           # Transaction operations
│   ├── budget/                # Budget management
│   ├── blockchain/            # Blockchain interaction
│   └── events/                # Event handling
├── infrastructure/            # External dependencies
│   ├── database/              # Database connectivity and repositories
│   ├── cache/                 # Caching implementation
│   ├── messaging/             # Message queue integration
│   ├── blockchain/            # Blockchain client implementations
│   └── metrics/               # Metrics and monitoring
├── cmd/                       # Service entry points
│   ├── server/                # Main service
│   ├── worker/                # Background workers
│   └── cli/                   # CLI tools
├── config/                    # Configuration handling
├── internal/                  # Internal shared packages
│   ├── auth/                  # Authentication utilities
│   ├── crypto/                # Cryptographic utilities
│   ├── logging/               # Logging utilities
│   └── util/                  # Miscellaneous utilities
└── test/                      # Test utilities and integration tests
```

## Data Model

### Database Schema

The service uses PostgreSQL with the following primary schema:

```sql
-- Gas accounts table
CREATE TABLE gas_accounts (
    id VARCHAR(36) PRIMARY KEY,
    blockchain_address VARCHAR(255) NOT NULL,
    blockchain_network VARCHAR(100) NOT NULL,
    account_type VARCHAR(50) NOT NULL,
    metadata JSONB,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UNIQUE(blockchain_address, blockchain_network)
);

-- Transactions table
CREATE TABLE transactions (
    id VARCHAR(36) PRIMARY KEY,
    tx_hash VARCHAR(255),
    blockchain_network VARCHAR(100) NOT NULL,
    from_address VARCHAR(255) NOT NULL,
    to_address VARCHAR(255) NOT NULL,
    data TEXT,
    value NUMERIC(78,0) NOT NULL,
    gas_limit NUMERIC(20,0),
    gas_price NUMERIC(78,0),
    max_fee_per_gas NUMERIC(78,0),
    max_priority_fee_per_gas NUMERIC(78,0),
    used_gas NUMERIC(20,0),
    total_cost NUMERIC(78,0),
    status VARCHAR(50) NOT NULL,
    account_id VARCHAR(36) REFERENCES gas_accounts(id),
    budget_id VARCHAR(36) REFERENCES budgets(id),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Budgets table
CREATE TABLE budgets (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    blockchain_network VARCHAR(100) NOT NULL,
    allocation NUMERIC(78,0) NOT NULL,
    used NUMERIC(78,0) NOT NULL DEFAULT 0,
    period VARCHAR(50) NOT NULL,
    start_time TIMESTAMP WITH TIME ZONE NOT NULL,
    end_time TIMESTAMP WITH TIME ZONE,
    owner_id VARCHAR(255) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Account balances table (for caching)
CREATE TABLE account_balances (
    account_id VARCHAR(36) PRIMARY KEY REFERENCES gas_accounts(id),
    balance NUMERIC(78,0) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Network gas prices table (for historical tracking)
CREATE TABLE network_gas_prices (
    id SERIAL PRIMARY KEY,
    blockchain_network VARCHAR(100) NOT NULL,
    base_fee_per_gas NUMERIC(78,0),
    economic_gas_price NUMERIC(78,0),
    standard_gas_price NUMERIC(78,0),
    fast_gas_price NUMERIC(78,0),
    economic_max_priority_fee NUMERIC(78,0),
    standard_max_priority_fee NUMERIC(78,0),
    fast_max_priority_fee NUMERIC(78,0),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UNIQUE(blockchain_network, updated_at)
);
```

### Core Domain Objects

#### GasAccount

```go
type GasAccount struct {
    ID                string            `json:"id"`
    BlockchainAddress string            `json:"blockchain_address"`
    BlockchainNetwork string            `json:"blockchain_network"`
    AccountType       string            `json:"account_type"`
    Metadata          map[string]string `json:"metadata,omitempty"`
    CreatedAt         time.Time         `json:"created_at"`
    UpdatedAt         time.Time         `json:"updated_at"`
}
```

#### Transaction

```go
type Transaction struct {
    ID                   string    `json:"id"`
    TxHash               string    `json:"tx_hash,omitempty"`
    BlockchainNetwork    string    `json:"blockchain_network"`
    FromAddress          string    `json:"from_address"`
    ToAddress            string    `json:"to_address"`
    Data                 string    `json:"data,omitempty"`
    Value                *big.Int  `json:"value"`
    GasInfo              GasInfo   `json:"gas_info"`
    Status               string    `json:"status"`
    AccountID            string    `json:"account_id"`
    BudgetID             string    `json:"budget_id,omitempty"`
    CreatedAt            time.Time `json:"created_at"`
    UpdatedAt            time.Time `json:"updated_at"`
}

type GasInfo struct {
    GasLimit            *big.Int `json:"gas_limit,omitempty"`
    GasPrice            *big.Int `json:"gas_price,omitempty"`
    MaxFeePerGas        *big.Int `json:"max_fee_per_gas,omitempty"`
    MaxPriorityFeePerGas *big.Int `json:"max_priority_fee_per_gas,omitempty"`
    UsedGas             *big.Int `json:"used_gas,omitempty"`
    TotalCost           *big.Int `json:"total_cost,omitempty"`
}
```

#### Budget

```go
type Budget struct {
    ID                string    `json:"id"`
    Name              string    `json:"name"`
    BlockchainNetwork string    `json:"blockchain_network"`
    Allocation        *big.Int  `json:"allocation"`
    Used              *big.Int  `json:"used"`
    Period            string    `json:"period"`
    StartTime         time.Time `json:"start_time"`
    EndTime           time.Time `json:"end_time,omitempty"`
    OwnerID           string    `json:"owner_id"`
    Status            string    `json:"status"`
    CreatedAt         time.Time `json:"created_at"`
    UpdatedAt         time.Time `json:"updated_at"`
}
```

## Key Components

### Account Manager

The Account Manager is responsible for gas account lifecycle and operations.

```go
// AccountManager manages gas accounts
type AccountManager interface {
    CreateAccount(ctx context.Context, network string, accountType string, metadata map[string]string) (*GasAccount, error)
    GetAccount(ctx context.Context, accountID string) (*GasAccount, error)
    ListAccounts(ctx context.Context, filter AccountFilter, pageSize int, pageToken string) ([]*GasAccount, string, error)
    UpdateAccountMetadata(ctx context.Context, accountID string, metadata map[string]string, merge bool) (*GasAccount, error)
    GetAccountBalance(ctx context.Context, accountID string) (*big.Int, time.Time, error)
    RefreshAccountBalance(ctx context.Context, accountID string) (*big.Int, error)
}

// Example implementation
func (m *accountManager) CreateAccount(ctx context.Context, network string, accountType string, metadata map[string]string) (*GasAccount, error) {
    // Generate account ID
    accountID, err := uuid.NewRandom()
    if err != nil {
        return nil, fmt.Errorf("failed to generate account ID: %w", err)
    }
    
    // Create blockchain address
    blockchainClient, err := m.blockchainClientFactory.GetClient(network)
    if err != nil {
        return nil, fmt.Errorf("failed to get blockchain client: %w", err)
    }
    
    privateKey, err := crypto.GenerateKey()
    if err != nil {
        return nil, fmt.Errorf("failed to generate private key: %w", err)
    }
    
    // Store private key in vault
    keyID := fmt.Sprintf("gas-account-%s", accountID.String())
    hexPrivKey := hex.EncodeToString(crypto.FromECDSA(privateKey))
    err = m.keyManager.StoreKey(ctx, keyID, hexPrivKey)
    if err != nil {
        return nil, fmt.Errorf("failed to store private key: %w", err)
    }
    
    // Create account record
    account := &GasAccount{
        ID:                accountID.String(),
        BlockchainAddress: crypto.PubkeyToAddress(privateKey.PublicKey).Hex(),
        BlockchainNetwork: network,
        AccountType:       accountType,
        Metadata:          metadata,
        CreatedAt:         time.Now(),
        UpdatedAt:         time.Now(),
    }
    
    // Store in database
    err = m.repository.CreateAccount(ctx, account)
    if err != nil {
        return nil, fmt.Errorf("failed to store account: %w", err)
    }
    
    return account, nil
}
```

### Transaction Manager

The Transaction Manager handles transaction preparation, submission, and tracking.

```go
// TransactionManager manages blockchain transactions
type TransactionManager interface {
    FundTransaction(ctx context.Context, req FundTransactionRequest) (*Transaction, error)
    EstimateTransactionFee(ctx context.Context, req EstimateTransactionFeeRequest) (*GasInfo, *big.Int, error)
    GetTransaction(ctx context.Context, txID string) (*Transaction, error)
    ListTransactions(ctx context.Context, filter TransactionFilter, pageSize int, pageToken string) ([]*Transaction, string, error)
    UpdateTransactionStatus(ctx context.Context, txID string, status string) error
}

// Example implementation
func (m *transactionManager) FundTransaction(ctx context.Context, req FundTransactionRequest) (*Transaction, error) {
    account, err := m.accountManager.GetAccount(ctx, req.AccountID)
    if err != nil {
        return nil, fmt.Errorf("failed to get account: %w", err)
    }
    
    // Check account balance
    balance, _, err := m.accountManager.GetAccountBalance(ctx, req.AccountID)
    if err != nil {
        return nil, fmt.Errorf("failed to get account balance: %w", err)
    }
    
    // Estimate gas
    blockchainClient, err := m.blockchainClientFactory.GetClient(account.BlockchainNetwork)
    if err != nil {
        return nil, fmt.Errorf("failed to get blockchain client: %w", err)
    }
    
    // Create transaction with nonce
    nonce, err := blockchainClient.GetNonce(ctx, account.BlockchainAddress)
    if err != nil {
        return nil, fmt.Errorf("failed to get nonce: %w", err)
    }
    
    // Determine gas settings
    gasInfo, err := m.determineGasSettings(ctx, req, account.BlockchainNetwork)
    if err != nil {
        return nil, fmt.Errorf("failed to determine gas settings: %w", err)
    }
    
    // Calculate total cost
    totalCost := calculateTotalCost(req.Value, gasInfo)
    
    if balance.Cmp(totalCost) < 0 {
        return nil, fmt.Errorf("insufficient funds: balance %s, required %s", balance.String(), totalCost.String())
    }
    
    // Check budget if specified
    if req.BudgetID != "" {
        err = m.budgetManager.CheckAndReserveBudget(ctx, req.BudgetID, totalCost)
        if err != nil {
            return nil, fmt.Errorf("budget check failed: %w", err)
        }
    }
    
    // Create transaction record
    txID, err := uuid.NewRandom()
    if err != nil {
        return nil, fmt.Errorf("failed to generate transaction ID: %w", err)
    }
    
    tx := &Transaction{
        ID:                txID.String(),
        BlockchainNetwork: account.BlockchainNetwork,
        FromAddress:       account.BlockchainAddress,
        ToAddress:         req.ToAddress,
        Data:              req.Data,
        Value:             req.Value,
        GasInfo:           gasInfo,
        Status:            "PENDING",
        AccountID:         req.AccountID,
        BudgetID:          req.BudgetID,
        CreatedAt:         time.Now(),
        UpdatedAt:         time.Now(),
    }
    
    // Store in database
    err = m.repository.CreateTransaction(ctx, tx)
    if err != nil {
        return nil, fmt.Errorf("failed to store transaction: %w", err)
    }
    
    // Submit transaction asynchronously
    m.transactionQueue.Submit(tx)
    
    return tx, nil
}
```

### Budget Manager

The Budget Manager handles gas budget creation, tracking, and enforcing.

```go
// BudgetManager manages gas budgets
type BudgetManager interface {
    CreateBudget(ctx context.Context, req CreateBudgetRequest) (*Budget, error)
    GetBudget(ctx context.Context, budgetID string) (*Budget, error)
    UpdateBudget(ctx context.Context, budgetID string, allocation *big.Int, status string) (*Budget, error)
    CheckAndReserveBudget(ctx context.Context, budgetID string, amount *big.Int) error
    CommitBudgetReservation(ctx context.Context, budgetID string, amount *big.Int) error
    ReleaseBudgetReservation(ctx context.Context, budgetID string, amount *big.Int) error
}
```

### Blockchain Client

The blockchain client interfaces with multiple blockchain networks.

```go
// BlockchainClient provides blockchain interaction
type BlockchainClient interface {
    SendTransaction(ctx context.Context, tx *RawTransaction) (string, error)
    GetTransactionReceipt(ctx context.Context, txHash string) (*TransactionReceipt, error)
    EstimateGas(ctx context.Context, tx *RawTransaction) (*big.Int, error)
    GetBalance(ctx context.Context, address string) (*big.Int, error)
    GetNonce(ctx context.Context, address string) (uint64, error)
    GetGasPrice(ctx context.Context) (*GasPriceInfo, error)
}
```

### Gas Price Service

The gas price service monitors and predicts gas prices across networks.

```go
// GasPriceService manages gas price information
type GasPriceService interface {
    GetGasPrices(ctx context.Context, network string) (*GasPriceInfo, error)
    RefreshGasPrices(ctx context.Context, network string) error
    GetFeeSettings(ctx context.Context, network string, strategy string) (*FeeSettings, error)
}
```

## Transaction Processing Flow

The service follows this flow for processing transactions:

1. **Request Validation**: Validates input parameters
2. **Account Verification**: Verifies the gas account exists and has sufficient balance
3. **Budget Verification**: If budget specified, validates and reserves budget
4. **Gas Estimation**: Determines appropriate gas settings based on strategy
5. **Transaction Creation**: Creates transaction record in PENDING state
6. **Transaction Submission**: Submits transaction to blockchain
7. **Status Monitoring**: Monitors transaction status with a background worker
8. **Budget Updating**: Updates budget usage on transaction completion
9. **Event Publishing**: Publishes transaction status events

## Event System

The service publishes events to Kafka for the following actions:

- Account creation and updates
- Transaction status changes
- Low balance alerts
- Budget threshold alerts

Event consumers include:

- Notification service (for alerts)
- Analytics service (for usage metrics)
- Audit service (for security logging)

## Security Implementation

### Key Management

Gas account private keys are stored in HashiCorp Vault. The service never exposes private keys directly; all transaction signing happens within the service.

```go
// KeyManager interface for secure key operations
type KeyManager interface {
    StoreKey(ctx context.Context, keyID string, privateKey string) error
    GetKey(ctx context.Context, keyID string) (string, error)
    SignTransaction(ctx context.Context, keyID string, tx *RawTransaction) (*SignedTransaction, error)
}
```

### Authentication and Authorization

The service implements:

1. **Service-to-service authentication**: Using mTLS with client certificates
2. **User authentication**: JWT validation with role-based access control
3. **Action-based authorization**: Permission checks on all operations

```go
// AuthMiddleware example for gRPC
func AuthMiddleware(ctx context.Context) (context.Context, error) {
    // Extract credentials from context
    credentials, ok := auth.CredentialsFromContext(ctx)
    if !ok {
        return nil, status.Error(codes.Unauthenticated, "missing credentials")
    }
    
    // Validate credentials
    authenticator := auth.GetAuthenticator()
    principal, err := authenticator.Authenticate(ctx, credentials)
    if err != nil {
        return nil, status.Error(codes.Unauthenticated, "invalid credentials")
    }
    
    // Set principal in context for later authorization checks
    ctx = auth.ContextWithPrincipal(ctx, principal)
    return ctx, nil
}
```

## Rate Limiting

Rate limiting is implemented at multiple levels:

1. **API level**: Per-client rate limits using Redis-based token bucket algorithm
2. **Blockchain level**: Rate limits on RPC requests to prevent provider throttling
3. **Account level**: Transaction rate limits to prevent nonce issues

## Error Handling

The service uses a standard error handling approach:

```go
// AppError represents an application error with context
type AppError struct {
    Code    ErrorCode
    Message string
    Details map[string]string
    Cause   error
}

// Error codes with standard gRPC mappings
const (
    ErrorUnknown           ErrorCode = "UNKNOWN"
    ErrorInvalidArgument   ErrorCode = "INVALID_ARGUMENT"
    ErrorNotFound          ErrorCode = "NOT_FOUND"
    ErrorAlreadyExists     ErrorCode = "ALREADY_EXISTS"
    ErrorPermissionDenied  ErrorCode = "PERMISSION_DENIED"
    ErrorUnauthenticated   ErrorCode = "UNAUTHENTICATED"
    ErrorInsufficientFunds ErrorCode = "INSUFFICIENT_FUNDS"
    ErrorBudgetExceeded    ErrorCode = "BUDGET_EXCEEDED"
    ErrorNetworkError      ErrorCode = "NETWORK_ERROR"
    ErrorInternal          ErrorCode = "INTERNAL"
)

// Error handler example
func HandleError(err error) *AppError {
    // Convert to AppError if not already
    var appErr *AppError
    if !errors.As(err, &appErr) {
        appErr = &AppError{
            Code:    ErrorUnknown,
            Message: err.Error(),
            Cause:   err,
        }
    }
    
    // Log error with context
    logger.WithError(err).WithFields(log.Fields{
        "error_code":    appErr.Code,
        "error_details": appErr.Details,
    }).Error(appErr.Message)
    
    return appErr
}
```

## Configuration

The service is configured using environment variables and config files:

```yaml
# config.yaml
server:
  port: 8080
  max_concurrent_requests: 1000
  timeout: 30s

database:
  host: postgres
  port: 5432
  database: gasbank
  username: ${DB_USER}
  password: ${DB_PASSWORD}
  max_connections: 20
  connection_timeout: 5s

redis:
  host: redis
  port: 6379
  password: ${REDIS_PASSWORD}
  database: 0

kafka:
  brokers:
    - kafka-1:9092
    - kafka-2:9092
  topic_prefix: gasbank
  consumer_group: gasbank-service

blockchain:
  networks:
    - id: ethereum-mainnet
      rpc_urls:
        - https://mainnet.infura.io/v3/${INFURA_KEY}
        - https://eth-mainnet.alchemyapi.io/v2/${ALCHEMY_KEY}
      block_time: 12s
      confirmations: 5
      max_gas_limit: 10000000
      min_gas_price: 1000000000  # 1 gwei
    - id: polygon-mainnet
      rpc_urls:
        - https://polygon-rpc.com
        - https://rpc-mainnet.matic.network
      block_time: 2s
      confirmations: 15
      max_gas_limit: 20000000
      min_gas_price: 30000000000  # 30 gwei
```

## Health Monitoring

The service implements health checks for all dependencies:

```go
// HealthCheck interface
type HealthCheck interface {
    Name() string
    Check(ctx context.Context) (status HealthStatus, details map[string]string)
}

// HealthStatus represents health status
type HealthStatus string

const (
    HealthStatusUp      HealthStatus = "UP"
    HealthStatusDown    HealthStatus = "DOWN"
    HealthStatusDegraded HealthStatus = "DEGRADED"
)

// Health check handler
func HealthCheckHandler(ctx context.Context, req *pb.HealthCheckRequest) (*pb.HealthCheckResponse, error) {
    healthService := services.GetHealthService()
    results := healthService.CheckAll(ctx)
    
    overallStatus := pb.HealthCheckResponse_SERVING
    for _, result := range results {
        if result.Status == HealthStatusDown {
            overallStatus = pb.HealthCheckResponse_NOT_SERVING
            break
        }
        if result.Status == HealthStatusDegraded && overallStatus == pb.HealthCheckResponse_SERVING {
            overallStatus = pb.HealthCheckResponse_SERVING_DEGRADED
        }
    }
    
    return &pb.HealthCheckResponse{
        Status:  overallStatus,
        Details: convertHealthResults(results),
    }, nil
}
```

## Deployment

The service is deployed in Kubernetes with the following architecture:

- **API servers**: Stateless servers for handling API requests
- **Workers**: Background processors for transaction monitoring and updates
- **Gas price updaters**: Dedicated workers for gas price monitoring
- **Metrics exporters**: Prometheus exporters for monitoring

### Environment Variables

Required environment variables:

```
DB_USER: Database username
DB_PASSWORD: Database password
REDIS_PASSWORD: Redis password
INFURA_KEY: Infura API key
ALCHEMY_KEY: Alchemy API key
VAULT_TOKEN: Vault access token
KAFKA_SASL_USERNAME: Kafka username
KAFKA_SASL_PASSWORD: Kafka password
JWT_PUBLIC_KEY: JWT validation public key
```

## Testing

The service includes multiple testing levels:

1. **Unit tests**: Testing individual components with mocks
2. **Integration tests**: Testing component interaction with test databases
3. **Blockchain tests**: Testing blockchain interaction with local networks
4. **End-to-end tests**: Testing full service with real dependencies

Example test for the account manager:

```go
func TestCreateAccount(t *testing.T) {
    // Setup test environment
    ctx := context.Background()
    mockRepo := mocks.NewMockAccountRepository()
    mockBlockchainClient := mocks.NewMockBlockchainClient()
    mockKeyManager := mocks.NewMockKeyManager()
    
    manager := NewAccountManager(mockRepo, mockBlockchainClient, mockKeyManager)
    
    // Setup expectations
    mockKeyManager.On("StoreKey", mock.Anything, mock.Anything, mock.Anything).Return(nil)
    mockRepo.On("CreateAccount", mock.Anything, mock.Anything).Return(nil)
    
    // Execute test
    account, err := manager.CreateAccount(ctx, "ethereum-mainnet", "OPERATIONAL", map[string]string{
        "owner": "test-service",
    })
    
    // Assertions
    assert.NoError(t, err)
    assert.NotNil(t, account)
    assert.NotEmpty(t, account.ID)
    assert.NotEmpty(t, account.BlockchainAddress)
    assert.Equal(t, "ethereum-mainnet", account.BlockchainNetwork)
    assert.Equal(t, "OPERATIONAL", account.AccountType)
    assert.Equal(t, "test-service", account.Metadata["owner"])
    
    // Verify expectations
    mockKeyManager.AssertExpectations(t)
    mockRepo.AssertExpectations(t)
}
```

## Performance Considerations

The service is optimized for:

1. **High throughput**: Using connection pooling and caching
2. **Low latency**: Minimizing blockchain RPC calls and using background processing
3. **Reliability**: Implementing retries and circuit breakers for blockchain calls
4. **Scalability**: Horizontally scaling API and worker components

## Related Documentation

- [Overview](OVERVIEW.md)
- [Architecture](ARCHITECTURE.md)
- [API Reference](API_REFERENCE.md)

## Core Workflows

### Transaction Submission Workflow

The transaction submission process is the most critical function of the Gas Bank Service. It involves multiple components working together to ensure reliable and secure transaction execution.

```
┌────────────────┐      ┌────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                │      │                │     │                 │     │                 │
│ Client Service ├─────►│ Gas Bank API   ├────►│ Account Manager ├────►│ Nonce Manager   │
│                │      │                │     │                 │     │                 │
└────────────────┘      └────────────────┘     └─────────────────┘     └────────┬────────┘
                                                                                │
                                                                                │
                                                                                ▼
┌────────────────┐      ┌────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                │      │                │     │                 │     │                 │
│ Blockchain     │◄─────┤ Transaction    │◄────┤ Secrets Service     │◄────┤ Transaction     │
│ Network        │      │ Submitter      │     │ (Signing)       │     │ Builder         │
│                │      │                │     │                 │     │                 │
└────────────────┘      └────────────────┘     └─────────────────┘     └─────────────────┘
       │                                                                         ▲
       │                                                                         │
       ▼                                                                         │
┌────────────────┐      ┌────────────────┐                                      │
│                │      │                │                                      │
│ Transaction    ├─────►│ Client Service │                                      │
│ Monitor        │      │ (Response)     │                                      │
│                │      │                │                                      │
└────────────────┘      └────────────────┘                                      │
                                                                                │
                                                                                │
┌────────────────┐      ┌────────────────┐     ┌─────────────────┐             │
│                │      │                │     │                 │             │
│ Gas Price      ├─────►│ Fee Calculator ├────►│ Budget Manager  ├─────────────┘
│ Service        │      │                │     │                 │
│                │      │                │     │                 │
└────────────────┘      └────────────────┘     └─────────────────┘
```

#### Implementation Details

The transaction submission flow is implemented in the `SendTransaction` method of the `Service` struct:

```go
// SendTransaction handles transaction construction, signing, nonce management, submission, and confirmation.
func (s *Service) SendTransaction(ctx context.Context, req *pb.SendTransactionRequest) (*pb.SendTransactionResponse, error) {
    // Step 1: Input validation
    if err := validateSendTransactionRequest(req); err != nil {
        return errorResponse(err), nil
    }
    
    // Step 2: Identify sender address and key
    senderAddress, keyIdentifier, err := s.getSenderAddressAndKey(req.SigningAddressPurpose)
    if err != nil {
        return errorResponse(err), nil
    }
    
    // Step 3: Nonce management
    nonce, err := s.nonceRepo.GetAndIncrementNonce(ctx, senderAddress)
    if err != nil {
        return errorResponse(err), nil
    }
    
    // Step 4: Transaction construction
    tx, err := s.buildTransaction(ctx, req, senderAddress, nonce)
    if err != nil {
        return errorResponse(err), nil
    }
    
    // Step 5: Sign transaction via Secrets Service
    signedTx, err := s.signTransaction(ctx, tx, keyIdentifier, req.SigningAddressPurpose)
    if err != nil {
        return errorResponse(err), nil
    }
    
    // Step 6: Submit transaction to blockchain
    txHash, err := s.neoClient.SendRawTransaction(ctx, signedTx)
    if err != nil {
        return errorResponse(err), nil
    }
    
    // Step 7: Wait for confirmation (if requested)
    var appLog *result.ApplicationLog
    if req.WaitForConfirmation {
        appLog, err = s.waitForConfirmation(ctx, txHash)
        if err != nil {
            return &pb.SendTransactionResponse{
                TxHash: txHash.StringLE(),
                Status: "SUBMITTED_BUT_CONFIRMATION_FAILED",
                Error: &commonv1.Error{
                    Code:    "ConfirmationFailed",
                    Message: err.Error(),
                },
            }, nil
        }
    }
    
    // Step 8: Return response with transaction details
    return &pb.SendTransactionResponse{
        TxHash: txHash.StringLE(),
        Status: getStatusFromAppLog(appLog),
        // Include other response fields...
    }, nil
}
```

### Address Selection Workflow

The address selection workflow is implemented in the `GetSigningAddress` method:

```
┌────────────────┐      ┌────────────────┐     ┌─────────────────┐
│                │      │                │     │                 │
│ Client Service ├─────►│ Gas Bank API   ├────►│ Address Manager │
│                │      │                │     │                 │
└────────────────┘      └────────────────┘     └────────┬────────┘
                                                        │
                                                        │
                                                        ▼
                                               ┌─────────────────┐
                                               │                 │
                                               │ Purpose Mapping │
                                               │                 │
                                               └────────┬────────┘
                                                        │
                                                        │
                                                        ▼
┌────────────────┐      ┌────────────────┐     ┌─────────────────┐
│                │      │                │     │                 │
│ Client Service │◄─────┤ Response       │◄────┤ Balance Checker │
│ (Response)     │      │ Builder        │     │ (Optional)      │
│                │      │                │     │                 │
└────────────────┘      └────────────────┘     └─────────────────┘
```

## Integration Examples

### Integration with Secrets Service

The Gas Bank Service integrates with the Secrets Service for secure transaction signing:

```go
// Example integration with Secrets Service for transaction signing
signReq := &secretservicev1.SignDigestRequest{
    Identifier:  &secretservicev1.SignDigestRequest_KeyId{KeyId: keyIdentifier},
    Digest:      txHash.BytesBE(),
    AuthContext: map[string]string{"service": "gasbank", "purpose": purposeIdentifier},
}

signResp, err := s.keyServiceClient.SignDigest(ctx, signReq)
if err != nil {
    return nil, fmt.Errorf("secrets service signing failed: %w", err)
}
```

### Integration with Automation Service

The Gas Bank Service is often used by the Automation Service to fund automated transactions:

```go
// Example code from Automation Service calling Gas Bank Service
func (a *Automation) fundTransaction(ctx context.Context, contractAddress string) error {
    // Get an appropriate signing address
    addrResp, err := a.gasBankClient.GetSigningAddress(ctx, &gasbank.GetSigningAddressRequest{
        Purpose:       "automation-executor",
        MinGasBalance: "1000000000", // 1 GAS minimum
    })
    if err != nil {
        return fmt.Errorf("failed to get signing address: %w", err)
    }
    
    // Prepare transaction parameters
    txReq := &gasbank.SendTransactionRequest{
        SigningAddressPurpose:    "automation-executor",
        TargetContractScriptHash: contractAddress,
        TargetMethod:             "execute",
        Parameters:               buildParameters(),
        WaitForConfirmation:      true,
        MaxSystemFee:             "2000000000", // 2 GAS system fee max
        MaxNetworkFee:            "1000000000", // 1 GAS network fee max
    }
    
    // Send transaction through Gas Bank
    txResp, err := a.gasBankClient.SendTransaction(ctx, txReq)
    if err != nil {
        return fmt.Errorf("transaction failed: %w", err)
    }
    
    // Check response status
    if txResp.Status != "EXECUTED" {
        return fmt.Errorf("transaction execution failed: %s", txResp.Error.Message)
    }
    
    return nil
}
```

## Deployment Considerations

### High Availability Configuration

The Gas Bank Service should be deployed with high availability in mind:

```yaml
# Example Kubernetes deployment for high availability
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gasbank-service
spec:
  replicas: 3  # Multiple replicas for redundancy
  selector:
    matchLabels:
      app: gasbank-service
  template:
    metadata:
      labels:
        app: gasbank-service
    spec:
      containers:
      - name: gasbank-service
        image: neo-service-layer/gasbank-service:latest
        ports:
        - containerPort: 50051
        readinessProbe:
          grpc:
            port: 50051
          initialDelaySeconds: 5
          periodSeconds: 10
        livenessProbe:
          grpc:
            port: 50051
          initialDelaySeconds: 15
          periodSeconds: 20
        resources:
          requests:
            memory: "512Mi"
            cpu: "200m"
          limits:
            memory: "1Gi"
            cpu: "500m"
        volumeMounts:
        - name: config-volume
          mountPath: /app/config
      volumes:
      - name: config-volume
        configMap:
          name: gasbank-config
```

### Database Considerations

For production deployment, the database should be properly configured for high performance and reliability:

```yaml
# PostgreSQL configuration recommendations for Gas Bank Service
max_connections = 200
shared_buffers = 4GB
effective_cache_size = 12GB
maintenance_work_mem = 1GB
checkpoint_completion_target = 0.9
wal_buffers = 16MB
default_statistics_target = 100
random_page_cost = 1.1
effective_io_concurrency = 200
work_mem = 20971kB
min_wal_size = 1GB
max_wal_size = 4GB
```

## Security Implementation

The Gas Bank Service implements several security measures:

1. **Secure Key Management**: Private keys are never stored in the Gas Bank Service; it always relies on the Secrets Service for transaction signing.

2. **Nonce Protection**: Database-level locking prevents nonce conflicts when multiple instances handle transactions for the same address.

3. **Input Validation**: All client inputs are validated before processing:

```go
func validateSendTransactionRequest(req *pb.SendTransactionRequest) error {
    if req == nil {
        return errors.New("request cannot be nil")
    }
    if req.SigningAddressPurpose == "" {
        return errors.New("signing_address_purpose cannot be empty")
    }
    if req.TargetContractScriptHash == "" {
        return errors.New("target_contract_script_hash cannot be empty")
    }
    if req.TargetMethod == "" {
        return errors.New("target_method cannot be empty")
    }
    // Additional validation...
    return nil
}
```

4. **Transaction Limits**: The service enforces limits on transaction fees to prevent excessive spending:

```go
// Check if the transaction exceeds the maximum allowed fees
if maxSystemFee != nil && systemFee.Cmp(maxSystemFee) > 0 {
    return nil, fmt.Errorf("calculated system fee (%s) exceeds maximum allowed (%s)", 
        systemFee.String(), maxSystemFee.String())
}
```

## Performance Tuning

For optimal performance, the Gas Bank Service can be tuned in several ways:

1. **Connection Pooling**: Configure database connection pooling:

```go
func newDBPool(dsn string) (*pgxpool.Pool, error) {
    config, err := pgxpool.ParseConfig(dsn)
    if err != nil {
        return nil, err
    }
    
    // Tune connection pool
    config.MaxConns = 20
    config.MinConns = 5
    config.MaxConnLifetime = 1 * time.Hour
    config.MaxConnIdleTime = 30 * time.Minute
    
    return pgxpool.ConnectConfig(context.Background(), config)
}
```

2. **Caching Strategy**: Implement caching for frequently accessed data:

```go
func (s *Service) getGasPrice(ctx context.Context, network string) (*big.Int, error) {
    // Try to get from cache first
    cacheKey := fmt.Sprintf("gas_price:%s", network)
    if cachedPrice, found := s.cache.Get(cacheKey); found {
        return cachedPrice.(*big.Int), nil
    }
    
    // If not in cache, get from blockchain
    price, err := s.neoClient.GetNetworkFeePerByte(ctx)
    if err != nil {
        return nil, err
    }
    
    // Cache the result for 2 minutes
    s.cache.Set(cacheKey, price, 2*time.Minute)
    return price, nil
}
```

## Troubleshooting

Common issues and their resolutions:

1. **Nonce Errors**: If transactions are failing due to nonce issues:
   - Check for concurrent transactions from the same address
   - Verify nonce repository is working correctly
   - Reset nonce if necessary: `UPDATE address_nonces SET nonce = X WHERE address = 'Y'`

2. **Insufficient Funds**: If transactions fail due to insufficient funds:
   - Check balance of signing addresses
   - Verify budget allocation
   - Ensure automatic funding is configured properly

3. **Connection Issues**: If the service cannot connect to blockchain nodes:
   - Verify network connectivity
   - Check RPC endpoint configuration
   - Ensure firewall rules allow the connection

## Future Enhancements

Planned enhancements for the Gas Bank Service:

1. **Multi-chain Support**: Expand beyond Neo N3 to support additional blockchain networks
2. **Enhanced Monitoring**: Implement detailed transaction tracking and analytics
3. **Auto-scaling**: Add support for dynamic resource allocation based on transaction volume
4. **Fee Optimization**: Implement machine learning for optimal gas price prediction
