//go:build integration
// +build integration

package functions

import (
	"context"
	"sync"
	"testing"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
	"github.com/will/neo_service_layer/internal/services/functions/runtime"
	"github.com/will/neo_service_layer/internal/services/transaction"
	"go.uber.org/zap"
)

// MockTransactionService implements a basic transaction service for testing
type MockTransactionService struct {
	transactions map[string]map[string]interface{}
}

func NewMockTransactionService() *MockTransactionService {
	return &MockTransactionService{
		transactions: make(map[string]map[string]interface{}),
	}
}

// Create creates a new transaction
func (m *MockTransactionService) Create(config map[string]interface{}) (string, error) {
	txID := "tx-" + time.Now().Format(time.RFC3339Nano)
	m.transactions[txID] = config
	m.transactions[txID]["status"] = "created"
	return txID, nil
}

// Sign signs a transaction
func (m *MockTransactionService) Sign(txID string, account *wallet.Account) (map[string]interface{}, error) {
	if tx, exists := m.transactions[txID]; exists {
		tx["status"] = "signed"
		tx["signedBy"] = account
		return tx, nil
	}
	return nil, transaction.ErrTransactionNotFound
}

// Send submits a transaction
func (m *MockTransactionService) Send(ctx context.Context, txID string) (string, error) {
	if tx, exists := m.transactions[txID]; exists {
		if tx["status"] != "signed" {
			return "", transaction.ErrTransactionNotSigned
		}
		tx["status"] = "sent"
		txHash := "0x" + txID
		tx["hash"] = txHash
		return txHash, nil
	}
	return "", transaction.ErrTransactionNotFound
}

// Status gets transaction status
func (m *MockTransactionService) Status(txID string) (string, error) {
	if tx, exists := m.transactions[txID]; exists {
		return tx["status"].(string), nil
	}
	return "", transaction.ErrTransactionNotFound
}

// Get retrieves a transaction
func (m *MockTransactionService) Get(txID string) (map[string]interface{}, error) {
	if tx, exists := m.transactions[txID]; exists {
		return tx, nil
	}
	return nil, transaction.ErrTransactionNotFound
}

// List gets all transactions
func (m *MockTransactionService) List() ([]interface{}, error) {
	var result []interface{}
	for txID, tx := range m.transactions {
		txCopy := make(map[string]interface{})
		for k, v := range tx {
			txCopy[k] = v
		}
		txCopy["id"] = txID
		result = append(result, txCopy)
	}
	return result, nil
}

// EstimateFee estimates transaction fee
func (m *MockTransactionService) EstimateFee(config map[string]interface{}) (string, error) {
	// Simple mock implementation
	return "0.1", nil
}

// The following methods implement the expected service interface for the sandbox

// CreateTransaction is the method expected by the sandbox
func (m *MockTransactionService) CreateTransaction(ctx context.Context, functionID string, config map[string]interface{}) (string, error) {
	config["owner"] = functionID // For simplicity, track the source function
	return m.Create(config)
}

// SignTransaction is the method expected by the sandbox
func (m *MockTransactionService) SignTransaction(ctx context.Context, functionID, txID string) (map[string]interface{}, error) {
	// Create a dummy account for testing
	mockAccount := &wallet.Account{}
	return m.Sign(txID, mockAccount)
}

// SendTransaction is the method expected by the sandbox
func (m *MockTransactionService) SendTransaction(ctx context.Context, functionID, txID string) (string, error) {
	return m.Send(ctx, txID)
}

// GetTransactionStatus is the method expected by the sandbox
func (m *MockTransactionService) GetTransactionStatus(ctx context.Context, functionID, txID string) (string, error) {
	return m.Status(txID)
}

// GetTransaction is the method expected by the sandbox
func (m *MockTransactionService) GetTransaction(ctx context.Context, functionID, txID string) (map[string]interface{}, error) {
	return m.Get(txID)
}

// ListTransactions is the method expected by the sandbox
func (m *MockTransactionService) ListTransactions(ctx context.Context, functionID string) ([]interface{}, error) {
	return m.List()
}

// EstimateTransactionFee is the method expected by the sandbox
func (m *MockTransactionService) EstimateTransactionFee(ctx context.Context, functionID string, config map[string]interface{}) (string, error) {
	return m.EstimateFee(config)
}

// MockSecretsService implements a basic secrets service for testing
type MockSecretsService struct {
	mu      sync.RWMutex
	secrets map[string]string
}

func NewMockSecretsService() *MockSecretsService {
	return &MockSecretsService{
		secrets: make(map[string]string),
	}
}

// Get retrieves a secret value
func (m *MockSecretsService) Get(secretName string) (string, error) {
	m.mu.RLock() // Read lock for secrets
	defer m.mu.RUnlock()

	value, exists := m.secrets[secretName]

	if exists {
		return value, nil
	}
	return "", nil // Return empty string if not found
}

// Set stores a secret value
func (m *MockSecretsService) Set(secretName, value string) error {
	m.mu.Lock() // Write lock
	defer m.mu.Unlock()

	m.secrets[secretName] = value
	return nil
}

// Delete removes a secret
func (m *MockSecretsService) Delete(secretName string) error {
	m.mu.Lock() // Write lock
	defer m.mu.Unlock()

	delete(m.secrets, secretName)
	return nil
}

// The following methods implement the expected service interface for the sandbox

// GetSecret is the method expected by the sandbox
func (m *MockSecretsService) GetSecret(ctx context.Context, functionID, secretName string) (string, error) {
	// m.Get handles its own locking
	return m.Get(secretName)
}

// SetSecret is the method expected by the sandbox
func (m *MockSecretsService) SetSecret(ctx context.Context, functionID, secretName, value string) error {
	// m.Set handles its own locking
	return m.Set(secretName, value)
}

// DeleteSecret is the method expected by the sandbox
func (m *MockSecretsService) DeleteSecret(ctx context.Context, functionID, secretName string) error {
	// m.Delete handles its own locking
	return m.Delete(secretName)
}

// MockPriceFeedService implements a basic price feed service for testing
type MockPriceFeedService struct {
	prices map[string]float64
}

func NewMockPriceFeedService() *MockPriceFeedService {
	return &MockPriceFeedService{
		prices: map[string]float64{
			"NEO": 50.0,
			"GAS": 15.0,
			"BTC": 30000.0,
			"ETH": 2000.0,
		},
	}
}

// GetPrice retrieves a price for a token
func (m *MockPriceFeedService) GetPrice(symbol string) (float64, error) {
	if price, exists := m.prices[symbol]; exists {
		return price, nil
	}
	return 0, nil
}

// The following methods implement the expected service interface for the sandbox

// GetTokenPrice is the method expected by the sandbox
func (m *MockPriceFeedService) GetTokenPrice(ctx context.Context, symbol string) (float64, error) {
	return m.GetPrice(symbol)
}

// realSandboxTest wraps the actual Runtime Sandbox to test real JS execution with mocked services
type realSandboxTest struct {
	sandbox              *runtime.Sandbox
	mockTxService        *MockTransactionService
	mockSecretsService   *MockSecretsService
	mockPriceFeedService *MockPriceFeedService
	mockService          *MockService
}

func newRealSandboxTest() *realSandboxTest {
	// Initialize mock service implementations
	mockTxService := NewMockTransactionService()
	mockSecretsService := NewMockSecretsService()
	mockPriceFeedService := NewMockPriceFeedService()
	mockService := NewMockService()

	// Create logger for the sandbox
	logger, _ := zap.NewDevelopment()

	// Create a sandbox with default configuration
	sandbox := runtime.NewSandbox(runtime.SandboxConfig{
		MemoryLimit:            128 * 1024 * 1024, // 128MB
		TimeoutMillis:          5000,              // 5 seconds
		StackSize:              8 * 1024 * 1024,   // 8MB
		AllowNetwork:           false,
		AllowFileIO:            false,
		EnableInteroperability: true,
		Logger:                 logger,
	})

	return &realSandboxTest{
		sandbox:              sandbox,
		mockTxService:        mockTxService,
		mockSecretsService:   mockSecretsService,
		mockPriceFeedService: mockPriceFeedService,
		mockService:          mockService,
	}
}

func (r *realSandboxTest) ExecuteJS(ctx context.Context, code string, ownerAddress string, params map[string]interface{}) (*runtime.FunctionOutput, error) {
	// Fill in missing params if needed
	if params == nil {
		params = make(map[string]interface{})
	}

	// Create a function context with services
	functionContext := &runtime.FunctionContext{
		FunctionID:  "test-function",
		ExecutionID: "test-execution-" + time.Now().Format(time.RFC3339Nano),
		Owner:       ownerAddress,
		Parameters:  params,
		Services: &runtime.ServiceClients{
			Functions:   r.mockService,
			Transaction: r.mockTxService,
			Secrets:     r.mockSecretsService,
			PriceFeed:   r.mockPriceFeedService,
		},
	}

	// Create input for the sandbox
	input := runtime.FunctionInput{
		Code:            code,
		Args:            params,
		Parameters:      params,
		FunctionContext: functionContext,
	}

	// Execute the code in the sandbox
	return r.sandbox.Execute(ctx, input)
}

// TestServiceInteroperabilityWithRealSandbox tests the ability of functions to interact with other services
// using the actual JS runtime sandbox
func TestServiceInteroperabilityWithRealSandbox(t *testing.T) {
	// Create sandbox test environment
	sandboxTest := newRealSandboxTest()

	// Set a secret for testing
	err := sandboxTest.mockSecretsService.Set("test-api-key", "secret-api-key-12345")
	require.NoError(t, err)

	// Set up test owner
	owner, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")
	require.NoError(t, err)
	ownerAddress := owner.StringLE()

	// Set a timeout for the entire test
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	t.Run("SecretsService", func(t *testing.T) {
		code := `
			function main() {
				// Get an existing secret
				const getResult = context.secrets.get("test-api-key");
				console.log("Retrieved secret:", getResult);
				
				// Set a new secret
				const setResult = context.secrets.set("new-secret", "new-value");
				console.log("Set secret result:", setResult);
				
				// Get the new secret to verify
				const verifyResult = context.secrets.get("new-secret");
				console.log("Verify new secret:", verifyResult);
				
				// Delete a secret
				const deleteResult = context.secrets.delete("new-secret");
				console.log("Delete secret result:", deleteResult);
				
				return {
					getResult: getResult,
					setResult: setResult,
					verifyResult: verifyResult,
					deleteResult: deleteResult
				};
			}
		`

		// Execute the function
		output, err := sandboxTest.ExecuteJS(ctx, code, ownerAddress, nil)
		require.NoError(t, err)
		require.NotNil(t, output)

		// Check logs to see what happened
		assert.Empty(t, output.Error)
		assert.NotEmpty(t, output.Logs)

		// Verify the results returned by the JS function
		resultMap, ok := output.Result.(map[string]interface{})
		assert.True(t, ok)

		// Check getResult for the initial secret
		getResult, ok := resultMap["getResult"].(map[string]interface{})
		assert.True(t, ok)
		assert.True(t, getResult["success"].(bool))
		assert.Equal(t, "secret-api-key-12345", getResult["value"])

		// Check setResult
		setResult, ok := resultMap["setResult"].(map[string]interface{})
		assert.True(t, ok)
		assert.True(t, setResult["success"].(bool))

		// Check verifyResult for the new secret *before* deletion
		verifyResult, ok := resultMap["verifyResult"].(map[string]interface{})
		assert.True(t, ok)
		assert.True(t, verifyResult["success"].(bool))
		assert.Equal(t, "new-value", verifyResult["value"]) // Check the value returned by JS

		// Check deleteResult
		deleteResult, ok := resultMap["deleteResult"].(map[string]interface{})
		assert.True(t, ok)
		assert.True(t, deleteResult["success"].(bool))

		// Optionally, verify the secret is gone from the mock *after* execution
		// _, err = sandboxTest.mockSecretsService.Get("new-secret")
		// assert.Error(t, err) // Or check if it returns empty string depending on mock impl.
		// For this mock, Get returns "" and nil error if not found, so we check for ""
		valAfterDelete, err := sandboxTest.mockSecretsService.Get("new-secret")
		assert.NoError(t, err)              // Mock Get returns nil error even if not found
		assert.Equal(t, "", valAfterDelete) // Should be empty after JS deleted it
	})

	t.Run("PriceFeedService", func(t *testing.T) {
		code := `
			function main() {
				// Get prices for different tokens
				const neoPriceResult = context.priceFeed.getPrice("NEO");
				console.log("NEO price:", neoPriceResult);
				
				const gasPriceResult = context.priceFeed.getPrice("GAS");
				console.log("GAS price:", gasPriceResult);
				
				const invalidPriceResult = context.priceFeed.getPrice("INVALID");
				console.log("Invalid token price:", invalidPriceResult);
				
				return {
					neoPriceResult: neoPriceResult,
					gasPriceResult: gasPriceResult,
					invalidPriceResult: invalidPriceResult
				};
			}
		`

		// Execute the function
		output, err := sandboxTest.ExecuteJS(ctx, code, ownerAddress, nil)
		require.NoError(t, err)
		require.NotNil(t, output)

		// Check logs to see what happened
		assert.Empty(t, output.Error)
		assert.NotEmpty(t, output.Logs)

		// Verify correct prices are set in the mock
		neoPrice, err := sandboxTest.mockPriceFeedService.GetPrice("NEO")
		assert.NoError(t, err)
		assert.Equal(t, 50.0, neoPrice)

		gasPrice, err := sandboxTest.mockPriceFeedService.GetPrice("GAS")
		assert.NoError(t, err)
		assert.Equal(t, 15.0, gasPrice)

		// Verify result directly
		result, ok := output.Result.(map[string]interface{})
		assert.True(t, ok)
		assert.NotNil(t, result["neoPriceResult"])
	})

	t.Run("TransactionService", func(t *testing.T) {
		code := `
			function main() {
				// Create a transaction
				const createResult = context.transaction.create({
					script: "0c0548656c6c6f0c03576f726c64192126dd72c4",
					signers: [
						{
							account: "0000000000000000000000000000000000000001",
							scopes: "CalledByEntry"
						}
					]
				});
				console.log("Create transaction result:", createResult);
				
				if (!createResult.success) {
					return { success: false, error: "Failed to create transaction" };
				}
				
				const txId = createResult.txId;
				
				// Sign the transaction
				const signResult = context.transaction.sign(txId);
				console.log("Sign transaction result:", signResult);
				
				if (!signResult.success) {
					return { success: false, error: "Failed to sign transaction" };
				}
				
				// Send the transaction
				const sendResult = context.transaction.send(txId);
				console.log("Send transaction result:", sendResult);
				
				if (!sendResult.success) {
					return { success: false, error: "Failed to send transaction" };
				}
				
				// Get transaction status
				const statusResult = context.transaction.status(txId);
				console.log("Transaction status:", statusResult);
				
				// Get transaction details
				const getResult = context.transaction.get(txId);
				console.log("Transaction details:", getResult);
				
				// Estimate fee for a similar transaction
				const feeResult = context.transaction.estimateFee({
					script: "0c0548656c6c6f0c03576f726c64192126dd72c4"
				});
				console.log("Fee estimation:", feeResult);
				
				return {
					success: true,
					createResult: createResult,
					signResult: signResult,
					sendResult: sendResult,
					statusResult: statusResult,
					getResult: getResult,
					feeResult: feeResult
				};
			}
		`

		// Execute the function
		output, err := sandboxTest.ExecuteJS(ctx, code, ownerAddress, nil)
		require.NoError(t, err)
		require.NotNil(t, output)

		// Check logs to see what happened
		assert.Empty(t, output.Error)
		assert.NotEmpty(t, output.Logs)

		// Verify transactions were created in the mock service
		txList, err := sandboxTest.mockTxService.List()
		assert.NoError(t, err)
		assert.NotEmpty(t, txList)
	})
}

// TestCombinedServiceInteroperabilityWithRealSandbox tests functions that use multiple services together
// using the actual JS runtime sandbox
func TestCombinedServiceInteroperabilityWithRealSandbox(t *testing.T) {
	// Create sandbox test environment
	sandboxTest := newRealSandboxTest()

	// Set up some test secrets
	err := sandboxTest.mockSecretsService.Set("api-key", "test-api-key-value")
	require.NoError(t, err)
	err = sandboxTest.mockSecretsService.Set("wallet-password", "test-wallet-password")
	require.NoError(t, err)

	// Set up test owner
	owner, err := util.Uint160DecodeStringLE("0000000000000000000000000000000000000001")
	require.NoError(t, err)
	ownerAddress := owner.StringLE()

	// Set a timeout for the entire test
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	// Complex function that uses multiple services
	code := `
	function main(args) {
		// Get token prices from price feed
		const sourceTokenSymbol = args.sourceToken || "NEO";
		const targetTokenSymbol = args.targetToken || "GAS";
		const sourceAmount = args.amount || 1.0;
		
		console.log("Starting token swap:", sourceAmount, sourceTokenSymbol, "to", targetTokenSymbol);
		
		// Get API key from secrets service
		const apiKeyResult = context.secrets.get("api-key");
		if (!apiKeyResult.success) {
			return { success: false, error: "Failed to retrieve API key" };
		}
		console.log("Retrieved API key:", apiKeyResult.success ? "SUCCESS" : "FAILED");
		
		// Get source token price
		const sourcePriceResult = context.priceFeed.getPrice(sourceTokenSymbol);
		if (!sourcePriceResult.success) {
			return { success: false, error: "Failed to get source token price" };
		}
		console.log(sourceTokenSymbol, "price:", sourcePriceResult.price, "USD");
		
		// Get target token price
		const targetPriceResult = context.priceFeed.getPrice(targetTokenSymbol);
		if (!targetPriceResult.success) {
			return { success: false, error: "Failed to get target token price" };
		}
		console.log(targetTokenSymbol, "price:", targetPriceResult.price, "USD");
		
		// Calculate swap amount
		const sourceValueUSD = sourceAmount * sourcePriceResult.price;
		const targetAmount = sourceValueUSD / targetPriceResult.price;
		console.log("Swap calculation:", sourceAmount, sourceTokenSymbol, "=", targetAmount, targetTokenSymbol);
		
		// Get wallet password
		const walletPasswordResult = context.secrets.get("wallet-password");
		if (!walletPasswordResult.success) {
			return { success: false, error: "Failed to retrieve wallet password" };
		}
		console.log("Retrieved wallet password:", walletPasswordResult.success ? "SUCCESS" : "FAILED");
		
		// Create swap transaction
		const swapTxConfig = {
			script: "0c05" + sourceTokenSymbol + "0c05" + targetTokenSymbol + "51c107", // Dummy script
			signers: [
				{
					account: "0000000000000000000000000000000000000001",
					scopes: "CalledByEntry"
				}
			],
			sourceAmount: sourceAmount,
			targetAmount: targetAmount,
			apiKey: apiKeyResult.value.substring(0, 3) + "..." // Only include prefix for logging safety
		};
		
		const createTxResult = context.transaction.create(swapTxConfig);
		if (!createTxResult.success) {
			return { success: false, error: "Failed to create swap transaction" };
		}
		console.log("Created swap transaction:", createTxResult.txId);
		
		// Sign the transaction
		const signResult = context.transaction.sign(createTxResult.txId);
		if (!signResult.success) {
			return { success: false, error: "Failed to sign swap transaction" };
		}
		console.log("Signed swap transaction:", signResult.txId);
		
		// Send the transaction
		const sendResult = context.transaction.send(createTxResult.txId);
		if (!sendResult.success) {
			return { success: false, error: "Failed to submit swap transaction" };
		}
		console.log("Submitted swap transaction with hash:", sendResult.hash);
		
		// Check transaction status
		const statusResult = context.transaction.status(createTxResult.txId);
		console.log("Transaction status:", statusResult.status);
		
		// Store the transaction details in a new secret for reference
		const txReference = {
			txId: createTxResult.txId,
			hash: sendResult.hash,
			sourceToken: sourceTokenSymbol,
			sourceAmount: sourceAmount,
			targetToken: targetTokenSymbol,
			targetAmount: targetAmount,
			timestamp: new Date().toISOString()
		};
		
		const saveTxResult = context.secrets.set("last-swap-tx", JSON.stringify(txReference));
		console.log("Saved transaction reference:", saveTxResult.success ? "SUCCESS" : "FAILED");
		
		return {
			success: true,
			swap: {
				sourceToken: sourceTokenSymbol,
				sourceAmount: sourceAmount,
				targetToken: targetTokenSymbol,
				targetAmount: targetAmount,
				sourceValueUSD: sourceValueUSD,
				exchangeRate: targetAmount / sourceAmount
			},
			transaction: {
				txId: createTxResult.txId,
				hash: sendResult.hash,
				status: statusResult.status
			}
		};
	}
	`

	// Execute the function with parameters
	params := map[string]interface{}{
		"sourceToken": "NEO",
		"targetToken": "GAS",
		"amount":      2.5,
	}

	output, err := sandboxTest.ExecuteJS(ctx, code, ownerAddress, params)
	require.NoError(t, err)
	require.NotNil(t, output)

	// Check execution was successful
	assert.Empty(t, output.Error)
	assert.NotEmpty(t, output.Logs)

	// Print logs for debugging
	for _, log := range output.Logs {
		t.Logf("Log: %s", log)
	}

	// Verify the expected result
	resultMap, ok := output.Result.(map[string]interface{})
	assert.True(t, ok, "Result should be a map")

	if ok {
		// Check overall success
		success, hasSuccess := resultMap["success"]
		assert.True(t, hasSuccess)
		assert.True(t, success.(bool), "Token swap should succeed")

		// Check swap details
		swap, ok := resultMap["swap"].(map[string]interface{})
		assert.True(t, ok, "Swap details should be a map")
		if ok {
			assert.Equal(t, "NEO", swap["sourceToken"])
			assert.Equal(t, 2.5, swap["sourceAmount"])
			assert.Equal(t, "GAS", swap["targetToken"])
			assert.InDelta(t, 8.333, swap["targetAmount"], 0.001) // 2.5 * 50 / 15 = 8.33...
		}

		// Check transaction details
		transaction, ok := resultMap["transaction"].(map[string]interface{})
		assert.True(t, ok, "Transaction details should be a map")
		if ok {
			assert.NotEmpty(t, transaction["txId"])
			assert.NotEmpty(t, transaction["hash"])
			assert.Equal(t, "sent", transaction["status"])
		}
	}

	// Verify that the transaction was stored in the secret service
	// Add a small delay to potentially allow mock service state to sync
	time.Sleep(20 * time.Millisecond)
	lastSwapTx, err := sandboxTest.mockSecretsService.Get("last-swap-tx")
	assert.NoError(t, err)
	assert.NotEmpty(t, lastSwapTx)
	t.Logf("Stored transaction reference: %s", lastSwapTx)
}
