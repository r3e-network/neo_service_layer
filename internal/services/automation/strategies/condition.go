package strategies

import (
	"context"
	"encoding/json"
	"fmt"
	"math/big"
	"strings"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/core/neo"
	"github.com/will/neo_service_layer/internal/services/automation"
	"github.com/will/neo_service_layer/internal/services/gasbank"
	"github.com/will/neo_service_layer/internal/services/trigger"
)

// ConditionType represents a type of condition for upkeep execution
type ConditionType string

// Condition types
const (
	TimeCondition        ConditionType = "time"
	PriceCondition       ConditionType = "price"
	ContractCondition    ConditionType = "contract"
	BlockCondition       ConditionType = "block"
	TransactionCondition ConditionType = "transaction"
	CustomCondition      ConditionType = "custom"
)

// ConditionConfig represents a condition configuration for upkeep execution
type ConditionConfig struct {
	Type    ConditionType          `json:"type"`
	Params  map[string]interface{} `json:"params"`
	Negated bool                   `json:"negated"`
}

// ConditionExecutionStrategy implements a condition-based execution strategy
type ConditionExecutionStrategy struct {
	neoClient      *neo.Client
	gasBankService *gasbank.Service
	triggerService *trigger.Service
}

// NewConditionExecutionStrategy creates a new condition-based execution strategy
func NewConditionExecutionStrategy(
	neoClient *neo.Client,
	gasBankService *gasbank.Service,
	triggerService *trigger.Service,
) *ConditionExecutionStrategy {
	return &ConditionExecutionStrategy{
		neoClient:      neoClient,
		gasBankService: gasBankService,
		triggerService: triggerService,
	}
}

// Execute executes an upkeep based on condition evaluation
func (s *ConditionExecutionStrategy) Execute(ctx context.Context, upkeep *automation.Upkeep, performData []byte) (*automation.UpkeepPerformance, error) {
	startTime := time.Now()

	// Parse condition from upkeep offchain config
	condition, err := parseCondition(upkeep.OffchainConfig)
	if err != nil {
		return &automation.UpkeepPerformance{
			ID:        fmt.Sprintf("perf-%d", time.Now().UnixNano()),
			UpkeepID:  upkeep.ID,
			StartTime: startTime,
			EndTime:   time.Now(),
			Status:    "failed",
			Error:     fmt.Sprintf("failed to parse condition: %v", err),
		}, err
	}

	// Check if the condition is met
	conditionMet, err := s.evaluateCondition(ctx, condition, upkeep)
	if err != nil {
		return &automation.UpkeepPerformance{
			ID:        fmt.Sprintf("perf-%d", time.Now().UnixNano()),
			UpkeepID:  upkeep.ID,
			StartTime: startTime,
			EndTime:   time.Now(),
			Status:    "failed",
			Error:     fmt.Sprintf("failed to evaluate condition: %v", err),
		}, err
	}

	// If the condition is not met, return a skipped performance
	if !conditionMet {
		return &automation.UpkeepPerformance{
			ID:        fmt.Sprintf("perf-%d", time.Now().UnixNano()),
			UpkeepID:  upkeep.ID,
			StartTime: startTime,
			EndTime:   time.Now(),
			Status:    "skipped",
			Result:    "condition not met",
		}, nil
	}

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
		ID:              fmt.Sprintf("perf-%d", time.Now().UnixNano()),
		UpkeepID:        upkeep.ID,
		StartTime:       startTime,
		EndTime:         endTime,
		Status:          "success",
		GasUsed:         upkeep.ExecuteGas,
		BlockNumber:     blockNumber,
		TransactionHash: txHash,
		Result:          fmt.Sprintf("Successfully called %s on contract %s based on condition", method, contractAddress.StringLE()),
	}

	return performance, nil
}

// evaluateCondition evaluates a condition
func (s *ConditionExecutionStrategy) evaluateCondition(ctx context.Context, condition *ConditionConfig, upkeep *automation.Upkeep) (bool, error) {
	var result bool
	var err error

	switch condition.Type {
	case TimeCondition:
		result, err = s.evaluateTimeCondition(condition, upkeep)
	case PriceCondition:
		result, err = s.evaluatePriceCondition(ctx, condition, upkeep)
	case ContractCondition:
		result, err = s.evaluateContractCondition(ctx, condition, upkeep)
	case BlockCondition:
		result, err = s.evaluateBlockCondition(ctx, condition, upkeep)
	case TransactionCondition:
		result, err = s.evaluateTransactionCondition(ctx, condition, upkeep)
	case CustomCondition:
		result, err = s.evaluateCustomCondition(ctx, condition, upkeep)
	default:
		return false, fmt.Errorf("unknown condition type: %s", condition.Type)
	}

	if err != nil {
		return false, err
	}

	// Apply negation if needed
	if condition.Negated {
		return !result, nil
	}
	return result, nil
}

// parseCondition parses a condition from upkeep offchain config
func parseCondition(config map[string]interface{}) (*ConditionConfig, error) {
	// Check if condition exists
	conditionData, ok := config["condition"]
	if !ok {
		return nil, fmt.Errorf("no condition found in offchain config")
	}

	// Try to convert to map
	conditionMap, ok := conditionData.(map[string]interface{})
	if !ok {
		// Try to parse from JSON string
		conditionStr, ok := conditionData.(string)
		if !ok {
			return nil, fmt.Errorf("invalid condition format")
		}

		if err := json.Unmarshal([]byte(conditionStr), &conditionMap); err != nil {
			return nil, fmt.Errorf("failed to parse condition: %w", err)
		}
	}

	// Extract condition type
	typeStr, ok := conditionMap["type"].(string)
	if !ok {
		return nil, fmt.Errorf("missing condition type")
	}

	// Extract params
	params, ok := conditionMap["params"].(map[string]interface{})
	if !ok {
		params = make(map[string]interface{})
	}

	// Extract negated flag
	negated, _ := conditionMap["negated"].(bool)

	return &ConditionConfig{
		Type:    ConditionType(typeStr),
		Params:  params,
		Negated: negated,
	}, nil
}

// evaluateTimeCondition evaluates a time-based condition
func (s *ConditionExecutionStrategy) evaluateTimeCondition(condition *ConditionConfig, upkeep *automation.Upkeep) (bool, error) {
	params := condition.Params

	// Check if this is a simple interval check
	interval, ok := params["interval"]
	if ok {
		intervalStr, ok := interval.(string)
		if !ok {
			return false, fmt.Errorf("invalid interval format")
		}

		// If lastRunAt is not set, it's the first run, so condition is met
		if upkeep.LastRunAt.IsZero() {
			return true, nil
		}

		// Parse duration
		var duration time.Duration
		switch strings.ToLower(intervalStr) {
		case "hourly":
			duration = time.Hour
		case "daily":
			duration = 24 * time.Hour
		case "weekly":
			duration = 7 * 24 * time.Hour
		case "monthly":
			duration = 30 * 24 * time.Hour
		default:
			// Try to parse as a duration string
			var err error
			duration, err = time.ParseDuration(intervalStr)
			if err != nil {
				return false, fmt.Errorf("invalid interval: %s", intervalStr)
			}
		}

		// Check if enough time has passed since the last run
		return time.Since(upkeep.LastRunAt) >= duration, nil
	}

	// Check if this is a specific time check
	specificTime, ok := params["time"]
	if ok {
		timeStr, ok := specificTime.(string)
		if !ok {
			return false, fmt.Errorf("invalid time format")
		}

		// Parse the time string (format: "HH:MM")
		timeParts := strings.Split(timeStr, ":")
		if len(timeParts) != 2 {
			return false, fmt.Errorf("invalid time format: %s", timeStr)
		}

		// Get the current time
		now := time.Now()
		targetHour, targetMinute := 0, 0
		fmt.Sscanf(timeParts[0], "%d", &targetHour)
		fmt.Sscanf(timeParts[1], "%d", &targetMinute)

		// If lastRunAt is from a previous day, the condition is met if the current time is past the target time
		if now.Hour() > targetHour || (now.Hour() == targetHour && now.Minute() >= targetMinute) {
			// Check if we already ran today
			if !upkeep.LastRunAt.IsZero() {
				lastRunDate := upkeep.LastRunAt.Format("2006-01-02")
				todayDate := now.Format("2006-01-02")
				if lastRunDate == todayDate {
					return false, nil
				}
			}
			return true, nil
		}
		return false, nil
	}

	// If no recognized time condition format, return an error
	return false, fmt.Errorf("invalid time condition format")
}

// evaluatePriceCondition evaluates a price-based condition
func (s *ConditionExecutionStrategy) evaluatePriceCondition(ctx context.Context, condition *ConditionConfig, upkeep *automation.Upkeep) (bool, error) {
	// Mock implementation - in a real application, this would call the price feed service
	params := condition.Params

	// Check required parameters
	asset, ok := params["asset"].(string)
	if !ok {
		return false, fmt.Errorf("missing 'asset' parameter")
	}

	thresholdStr, ok := params["threshold"].(string)
	if !ok {
		thresholdVal, ok := params["threshold"].(float64)
		if !ok {
			return false, fmt.Errorf("missing 'threshold' parameter")
		}
		thresholdStr = fmt.Sprintf("%f", thresholdVal)
	}

	comparison, ok := params["comparison"].(string)
	if !ok {
		return false, fmt.Errorf("missing 'comparison' parameter")
	}

	// Parse threshold
	threshold, ok := new(big.Float).SetString(thresholdStr)
	if !ok {
		return false, fmt.Errorf("invalid threshold: %s", thresholdStr)
	}

	// Mock price for the asset
	var price *big.Float
	switch strings.ToUpper(asset) {
	case "BTC":
		price = big.NewFloat(50000)
	case "ETH":
		price = big.NewFloat(3000)
	case "NEO":
		price = big.NewFloat(50)
	case "GAS":
		price = big.NewFloat(10)
	default:
		price = big.NewFloat(1)
	}

	// Compare price to threshold
	switch strings.ToLower(comparison) {
	case "above", "greaterthan", ">":
		return price.Cmp(threshold) > 0, nil
	case "below", "lessthan", "<":
		return price.Cmp(threshold) < 0, nil
	case "equal", "equals", "=", "==":
		return price.Cmp(threshold) == 0, nil
	default:
		return false, fmt.Errorf("invalid comparison: %s", comparison)
	}
}

// evaluateContractCondition evaluates a contract-based condition
func (s *ConditionExecutionStrategy) evaluateContractCondition(ctx context.Context, condition *ConditionConfig, upkeep *automation.Upkeep) (bool, error) {
	// Mock implementation - in a real application, this would call the contract to check a value
	params := condition.Params

	// Check required parameters
	contract, ok := params["contract"].(string)
	if !ok {
		return false, fmt.Errorf("missing 'contract' parameter")
	}

	method, ok := params["method"].(string)
	if !ok {
		return false, fmt.Errorf("missing 'method' parameter")
	}

	// Mock result - in a real implementation, we would call the contract method
	// Log the contract and method that would be called
	fmt.Printf("Would call contract %s method %s\n", contract, method)

	return true, nil
}

// evaluateBlockCondition evaluates a block-based condition
func (s *ConditionExecutionStrategy) evaluateBlockCondition(ctx context.Context, condition *ConditionConfig, upkeep *automation.Upkeep) (bool, error) {
	// Mock implementation - in a real application, this would check block height or time
	params := condition.Params

	// Check for block height condition
	if height, ok := params["height"]; ok {
		targetHeight, ok := height.(float64)
		if !ok {
			return false, fmt.Errorf("invalid height format")
		}

		// Mock current block height
		currentHeight := uint32(12345)
		return float64(currentHeight) >= targetHeight, nil
	}

	// Check for block time condition
	if timeStr, ok := params["time"]; ok {
		targetTime, ok := timeStr.(string)
		if !ok {
			return false, fmt.Errorf("invalid time format")
		}

		// Parse time
		target, err := time.Parse(time.RFC3339, targetTime)
		if err != nil {
			return false, fmt.Errorf("invalid time format: %v", err)
		}

		// Mock current block time
		now := time.Now()
		return now.After(target), nil
	}

	return false, fmt.Errorf("no recognized block condition format")
}

// evaluateTransactionCondition evaluates a transaction-based condition
func (s *ConditionExecutionStrategy) evaluateTransactionCondition(ctx context.Context, condition *ConditionConfig, upkeep *automation.Upkeep) (bool, error) {
	// Mock implementation - in a real application, this would check for specific transactions
	params := condition.Params

	// Check required parameters
	txType, ok := params["type"].(string)
	if !ok {
		return false, fmt.Errorf("missing 'type' parameter")
	}

	// Mock implementation - always return true
	switch strings.ToLower(txType) {
	case "transfer", "invoke", "deploy", "any":
		return true, nil
	default:
		return false, fmt.Errorf("unknown transaction type: %s", txType)
	}
}

// evaluateCustomCondition evaluates a custom condition
func (s *ConditionExecutionStrategy) evaluateCustomCondition(ctx context.Context, condition *ConditionConfig, upkeep *automation.Upkeep) (bool, error) {
	// In a real implementation, this would call into the trigger service
	// to evaluate a custom condition or function

	// Mock implementation - always return true
	return true, nil
}
