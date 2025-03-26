package strategies

import (
	"context"
	"fmt"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/core/neo"
	"github.com/will/neo_service_layer/internal/services/automation"
	"github.com/will/neo_service_layer/internal/services/gasbank"
)

// EventType represents a type of blockchain event
type EventType string

// Event types
const (
	TransferEvent        EventType = "transfer"
	ContractDeployEvent  EventType = "contract_deploy"
	ContractUpdateEvent  EventType = "contract_update"
	NotificationEvent    EventType = "notification"
	TransactionEvent     EventType = "transaction"
	BlockEvent           EventType = "block"
	OracleResponseEvent  EventType = "oracle_response"
	CommitteeChangeEvent EventType = "committee_change"
)

// EventFilter represents a filter for blockchain events
type EventFilter struct {
	EventType     EventType          `json:"event_type"`
	ContractHash  util.Uint160       `json:"contract_hash,omitempty"`
	FromAddress   util.Uint160       `json:"from_address,omitempty"`
	ToAddress     util.Uint160       `json:"to_address,omitempty"`
	TokenID       string             `json:"token_id,omitempty"`
	MinAmount     string             `json:"min_amount,omitempty"`
	MaxAmount     string             `json:"max_amount,omitempty"`
	StartBlock    uint32             `json:"start_block,omitempty"`
	EndBlock      uint32             `json:"end_block,omitempty"`
	Confirmations uint32             `json:"confirmations,omitempty"`
	CustomFilter  map[string]interface{} `json:"custom_filter,omitempty"`
}

// EventExecutionStrategy implements an event-based execution strategy
type EventExecutionStrategy struct {
	neoClient      *neo.Client
	gasBankService *gasbank.Service
	eventFilters   map[string]*EventFilter // map of upkeep ID to event filter
}

// NewEventExecutionStrategy creates a new event-based execution strategy
func NewEventExecutionStrategy(
	neoClient *neo.Client,
	gasBankService *gasbank.Service,
) *EventExecutionStrategy {
	return &EventExecutionStrategy{
		neoClient:      neoClient,
		gasBankService: gasBankService,
		eventFilters:   make(map[string]*EventFilter),
	}
}

// RegisterEventFilter registers an event filter for an upkeep
func (s *EventExecutionStrategy) RegisterEventFilter(upkeepID string, filter *EventFilter) {
	s.eventFilters[upkeepID] = filter
}

// UnregisterEventFilter unregisters an event filter for an upkeep
func (s *EventExecutionStrategy) UnregisterEventFilter(upkeepID string) {
	delete(s.eventFilters, upkeepID)
}

// GetEventFilter gets an event filter for an upkeep
func (s *EventExecutionStrategy) GetEventFilter(upkeepID string) *EventFilter {
	return s.eventFilters[upkeepID]
}

// Execute executes an upkeep based on event data
func (s *EventExecutionStrategy) Execute(ctx context.Context, upkeep *automation.Upkeep, performData []byte) (*automation.UpkeepPerformance, error) {
	startTime := time.Now()

	// Get the event filter for this upkeep
	filter := s.GetEventFilter(upkeep.ID)
	if filter == nil {
		// Try to parse event filter from upkeep offchain config
		var err error
		filter, err = parseEventFilter(upkeep.OffchainConfig)
		if err != nil {
			return &automation.UpkeepPerformance{
				ID:        fmt.Sprintf("perf-%d", time.Now().UnixNano()),
				UpkeepID:  upkeep.ID,
				StartTime: startTime,
				EndTime:   time.Now(),
				Status:    "failed",
				Error:     fmt.Sprintf("failed to parse event filter: %v", err),
			}, err
		}

		// Register the filter for future use
		s.RegisterEventFilter(upkeep.ID, filter)
	}

	// Parse event data from the perform data
	// In a real implementation, this would parse the event data from the blockchain
	// For now, we'll just mock successful execution

	// Prepare transaction parameters
	contractAddress := upkeep.TargetContract
	method := upkeep.UpkeepFunction

	// Execute the transaction
	// In a real implementation, we would call the contract
	// For now, simulate with a mock response
	txHash := util.Uint256{1, 2, 3, 4, 5}
	blockNumber := uint32(12345)

	// Record the end time
	endTime := time.Now()

	// Create performance record
	performance := &automation.UpkeepPerformance{
		ID:              uuid.New().String(),
		UpkeepID:        upkeep.ID,
		StartTime:       startTime,
		EndTime:         endTime,
		Status:          "success",
		GasUsed:         upkeep.ExecuteGas,
		BlockNumber:     blockNumber,
		TransactionHash: txHash,
		Result:          fmt.Sprintf("Successfully called %s on contract %s based on event", method, contractAddress.StringLE()),
	}

	return performance, nil
}

// parseEventFilter parses an event filter from upkeep offchain config
func parseEventFilter(config map[string]interface{}) (*EventFilter, error) {
	// Check if event filter exists
	eventData, ok := config["eventFilter"]
	if !ok {
		return nil, fmt.Errorf("no event filter found in offchain config")
	}

	// Try to convert to map
	eventMap, ok := eventData.(map[string]interface{})
	if !ok {
		return nil, fmt.Errorf("invalid event filter format")
	}

	// Extract event type
	eventTypeStr, ok := eventMap["event_type"].(string)
	if !ok {
		return nil, fmt.Errorf("missing event type")
	}

	// Create event filter
	filter := &EventFilter{
		EventType:     EventType(eventTypeStr),
		CustomFilter:  make(map[string]interface{}),
	}

	// Extract contract hash if present
	if contractHashStr, ok := eventMap["contract_hash"].(string); ok && contractHashStr != "" {
		contractHash, err := util.Uint160DecodeStringLE(contractHashStr)
		if err != nil {
			return nil, fmt.Errorf("invalid contract hash: %w", err)
		}
		filter.ContractHash = contractHash
	}

	// Extract from address if present
	if fromAddressStr, ok := eventMap["from_address"].(string); ok && fromAddressStr != "" {
		fromAddress, err := util.Uint160DecodeStringLE(fromAddressStr)
		if err != nil {
			return nil, fmt.Errorf("invalid from address: %w", err)
		}
		filter.FromAddress = fromAddress
	}

	// Extract to address if present
	if toAddressStr, ok := eventMap["to_address"].(string); ok && toAddressStr != "" {
		toAddress, err := util.Uint160DecodeStringLE(toAddressStr)
		if err != nil {
			return nil, fmt.Errorf("invalid to address: %w", err)
		}
		filter.ToAddress = toAddress
	}

	// Extract token ID if present
	if tokenID, ok := eventMap["token_id"].(string); ok {
		filter.TokenID = tokenID
	}

	// Extract min amount if present
	if minAmount, ok := eventMap["min_amount"].(string); ok {
		filter.MinAmount = minAmount
	}

	// Extract max amount if present
	if maxAmount, ok := eventMap["max_amount"].(string); ok {
		filter.MaxAmount = maxAmount
	}

	// Extract start block if present
	if startBlock, ok := eventMap["start_block"].(float64); ok {
		filter.StartBlock = uint32(startBlock)
	}

	// Extract end block if present
	if endBlock, ok := eventMap["end_block"].(float64); ok {
		filter.EndBlock = uint32(endBlock)
	}

	// Extract confirmations if present
	if confirmations, ok := eventMap["confirmations"].(float64); ok {
		filter.Confirmations = uint32(confirmations)
	}

	// Extract custom filter if present
	if customFilter, ok := eventMap["custom_filter"].(map[string]interface{}); ok {
		filter.CustomFilter = customFilter
	}

	return filter, nil
}

// CheckEvent checks if an event matches the filter
func (f *EventFilter) CheckEvent(event map[string]interface{}) bool {
	// Check event type
	eventType, ok := event["type"].(string)
	if !ok || EventType(eventType) != f.EventType {
		return false
	}

	// Check contract hash if specified
	if !f.ContractHash.Equals(util.Uint160{}) {
		contractHashStr, ok := event["contract"].(string)
		if !ok {
			return false
		}

		contractHash, err := util.Uint160DecodeStringLE(contractHashStr)
		if err != nil || !contractHash.Equals(f.ContractHash) {
			return false
		}
	}

	// Check from address if specified
	if !f.FromAddress.Equals(util.Uint160{}) {
		fromAddressStr, ok := event["from"].(string)
		if !ok {
			return false
		}

		fromAddress, err := util.Uint160DecodeStringLE(fromAddressStr)
		if err != nil || !fromAddress.Equals(f.FromAddress) {
			return false
		}
	}

	// Check to address if specified
	if !f.ToAddress.Equals(util.Uint160{}) {
		toAddressStr, ok := event["to"].(string)
		if !ok {
			return false
		}

		toAddress, err := util.Uint160DecodeStringLE(toAddressStr)
		if err != nil || !toAddress.Equals(f.ToAddress) {
			return false
		}
	}

	// Check token ID if specified
	if f.TokenID != "" {
		tokenID, ok := event["token_id"].(string)
		if !ok || tokenID != f.TokenID {
			return false
		}
	}

	// Check amount if specified
	if f.MinAmount != "" || f.MaxAmount != "" {
		amountStr, ok := event["amount"].(string)
		if !ok {
			return false
		}

		// In a real implementation, we would parse and compare the amounts
		// For now, just do simple string comparison
		if f.MinAmount != "" && amountStr < f.MinAmount {
			return false
		}

		if f.MaxAmount != "" && amountStr > f.MaxAmount {
			return false
		}
	}

	// Check custom filter if specified
	for key, value := range f.CustomFilter {
		eventValue, ok := event[key]
		if !ok || eventValue != value {
			return false
		}
	}

	return true
}