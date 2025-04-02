package neo

import (
	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	// "github.com/nspcc-dev/neo-go/pkg/neorpc/result" // Avoid direct import if causing issues
	"github.com/nspcc-dev/neo-go/pkg/smartcontract"
	"github.com/nspcc-dev/neo-go/pkg/smartcontract/trigger"
	"github.com/nspcc-dev/neo-go/pkg/util"
	log "github.com/sirupsen/logrus"
)

// --- NeoClient Interface ---

type NeoClient interface {
	InvokeFunction(contract util.Uint160, operation string, params []smartcontract.Parameter, signers []transaction.Signer) (interface{}, error) // Use interface{} for result
	SendRawTransaction(tx *transaction.Transaction) (util.Uint256, error)
	GetApplicationLog(hash util.Uint256, trig *trigger.Type) (interface{}, error) // Use interface{} for result
	CalculateNetworkFee(tx *transaction.Transaction) (int64, error)
	GetBlockCount() (uint32, error)
	GetNetwork() (uint64, error) // network magic
}

// --- Mock Implementation ---

// MockNeoClient implements the NeoClient interface for testing/development.
type MockNeoClient struct {
	MockInvokeResult    interface{} // Using interface{}
	MockInvokeError     error
	MockSendTxHash      util.Uint256
	MockSendError       error
	MockAppLog          interface{} // Using interface{}
	MockAppLogError     error
	MockNetworkFee      int64
	MockNetworkFeeError error
	MockBlockCount      uint32
	MockBlockCountError error
	MockNetwork         uint64
	MockNetworkError    error
}

// NewMockNeoClient creates a new mock client with default successful responses.
func NewMockNeoClient() *MockNeoClient {
	log.Warn("Using MockNeoClient - No real blockchain interaction will occur!")
	// Use map[string]interface{} or similar basic types for mock results
	mockInvoke := map[string]interface{}{"state": "HALT", "gasconsumed": "1000000", "stack": []interface{}{map[string]interface{}{"type": "Boolean", "value": true}}}
	mockAppLog := map[string]interface{}{"txhash": "0x0403020100000000000000000000000000000000000000000000000000000000", "executions": []interface{}{map[string]interface{}{"trigger": "Application", "vmstate": "HALT", "gasconsumed": "1000000", "stack": []interface{}{map[string]interface{}{"type": "Boolean", "value": true}}}}}
	return &MockNeoClient{
		MockInvokeResult: mockInvoke,
		MockSendTxHash:   util.Uint256{1, 2, 3, 4},
		MockAppLog:       mockAppLog,
		MockNetworkFee:   100000,
		MockBlockCount:   1234567,
		MockNetwork:      860833102,
	}
}

func (m *MockNeoClient) InvokeFunction(contract util.Uint160, operation string, params []smartcontract.Parameter, signers []transaction.Signer) (interface{}, error) {
	log.Debugf("MockNeoClient.InvokeFunction called: %s.%s", contract.StringLE(), operation)
	if m.MockInvokeError != nil {
		return nil, m.MockInvokeError
	}
	return m.MockInvokeResult, nil
}

func (m *MockNeoClient) SendRawTransaction(tx *transaction.Transaction) (util.Uint256, error) {
	log.Debugf("MockNeoClient.SendRawTransaction called for tx with %d signers", len(tx.Signers))
	if m.MockSendError != nil {
		return util.Uint256{}, m.MockSendError
	}
	return m.MockSendTxHash, nil
}

func (m *MockNeoClient) GetApplicationLog(hash util.Uint256, trig *trigger.Type) (interface{}, error) {
	log.Debugf("MockNeoClient.GetApplicationLog called for hash: %s", hash.StringLE())
	if m.MockAppLogError != nil {
		return nil, m.MockAppLogError
	}
	// If mock app log is a map, update the hash for consistency
	if logMap, ok := m.MockAppLog.(map[string]interface{}); ok {
		logMap["txhash"] = hash.StringLE() // Store as string, matching common JSON patterns
		return logMap, nil
	}
	return m.MockAppLog, nil
}

func (m *MockNeoClient) CalculateNetworkFee(tx *transaction.Transaction) (int64, error) {
	log.Debugf("MockNeoClient.CalculateNetworkFee called")
	if m.MockNetworkFeeError != nil {
		return 0, m.MockNetworkFeeError
	}
	return m.MockNetworkFee, nil
}

func (m *MockNeoClient) GetBlockCount() (uint32, error) {
	log.Debugf("MockNeoClient.GetBlockCount called")
	if m.MockBlockCountError != nil {
		return 0, m.MockBlockCountError
	}
	return m.MockBlockCount, nil
}

func (m *MockNeoClient) GetNetwork() (uint64, error) {
	log.Debugf("MockNeoClient.GetNetwork called")
	if m.MockNetworkError != nil {
		return 0, m.MockNetworkError
	}
	return m.MockNetwork, nil
}

var _ NeoClient = (*MockNeoClient)(nil)
