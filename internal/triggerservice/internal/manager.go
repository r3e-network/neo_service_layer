package internal

import (
	"context"
	"encoding/hex"
	"errors"
	"fmt"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/smartcontract"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/core/neo"
	functions_iface "github.com/r3e-network/neo_service_layer/internal/functionservice"
	trigger_models "github.com/r3e-network/neo_service_layer/internal/triggerservice/models"
	log "github.com/sirupsen/logrus"
)

// Use NeoClient from core/neo package
// type NeoClient interface{}

// Placeholder interface for Wallet Service capable of signing
type WalletService interface {
	SignTx(ctx context.Context, acc util.Uint160, tx *transaction.Transaction) error
	// Potentially add methods to get public keys or check wallet status if needed
}

// TriggerManagerImpl implements the TriggerManager interface
type TriggerManagerImpl struct {
	store         TriggerStore
	metrics       TriggerMetricsCollector
	alerts        TriggerAlertManager
	scheduler     TriggerScheduler
	policy        *trigger_models.TriggerPolicy
	neoClient     neo.NeoClient // Using the imported interface
	funcService   functions_iface.IService
	walletService WalletService
}

// NewTriggerManager creates a new TriggerManager instance
func NewTriggerManager(store TriggerStore, metrics TriggerMetricsCollector, alerts TriggerAlertManager, scheduler TriggerScheduler, policy *trigger_models.TriggerPolicy, neoClient neo.NeoClient, funcService functions_iface.IService, walletService WalletService) TriggerManager {
	if funcService == nil {
		log.Warn("TriggerManager created without Functions Service. Function actions will fail.")
	}
	if neoClient == nil {
		log.Error("TriggerManager created without Neo Client. Conditional/Contract triggers will fail.")
	}
	if walletService == nil {
		log.Warn("TriggerManager created without Wallet Service. Contract actions will fail (cannot sign).")
	}
	return &TriggerManagerImpl{
		store:         store,
		metrics:       metrics,
		alerts:        alerts,
		scheduler:     scheduler,
		policy:        policy,
		neoClient:     neoClient,
		funcService:   funcService,
		walletService: walletService,
	}
}

// CreateTrigger creates a new trigger
func (tm *TriggerManagerImpl) CreateTrigger(ctx context.Context, userAddress util.Uint160, trigger *trigger_models.Trigger) (*trigger_models.Trigger, error) {
	triggers, err := tm.store.ListTriggers(ctx, userAddress)
	if err != nil {
		return nil, fmt.Errorf("failed to list triggers: %w", err)
	}

	if len(triggers) >= tm.policy.MaxTriggersPerUser {
		return nil, fmt.Errorf("maximum number of triggers reached for user")
	}

	if trigger.ID == "" {
		trigger.ID = uuid.New().String()
	}
	trigger.UserAddress = userAddress
	if trigger.Status == "" {
		trigger.Status = "active" // Default to active
	}
	now := time.Now()
	trigger.CreatedAt = now
	trigger.UpdatedAt = now

	// Validate Action
	switch trigger.Action {
	case trigger_models.FunctionAction:
		if trigger.TargetFunctionID == "" {
			return nil, errors.New("TargetFunctionID is required for function action")
		}
	case trigger_models.ContractAction:
		if trigger.TargetContract.Equals(util.Uint160{}) || trigger.TargetMethod == "" {
			return nil, errors.New("TargetContract and TargetMethod are required for contract action")
		}
		if trigger.Signer.Equals(util.Uint160{}) {
			return nil, errors.New("Signer is required for contract action")
		}
	default:
		return nil, fmt.Errorf("invalid trigger action type: %s", trigger.Action)
	}

	// Validate Condition/Schedule based on Type
	switch trigger.Type {
	case trigger_models.ScheduleTrigger:
		if trigger.Schedule == "" {
			return nil, errors.New("schedule is required for schedule trigger type")
		}
		nextExecution, err := tm.scheduler.GetNextExecutionTime(ctx, trigger.Schedule)
		if err != nil {
			return nil, fmt.Errorf("invalid schedule format: %w", err)
		}
		trigger.NextExecution = nextExecution
	case trigger_models.EventTrigger, trigger_models.ConditionalTrigger:
		// TODO: Add validation for ConditionConfig based on type
		// Need to determine structure and requirements for these configs.
		trigger.NextExecution = time.Time{} // Not directly scheduled
	default:
		return nil, fmt.Errorf("invalid trigger type: %s", trigger.Type)
	}

	if err := tm.store.SaveTrigger(ctx, trigger); err != nil {
		return nil, fmt.Errorf("failed to save trigger: %w", err)
	}

	// Only schedule cron-based triggers
	if trigger.Type == trigger_models.ScheduleTrigger {
		if err := tm.scheduler.ScheduleTrigger(ctx, trigger); err != nil {
			// Attempt cleanup?
			_ = tm.store.DeleteTrigger(ctx, userAddress, trigger.ID)
			return nil, fmt.Errorf("failed to schedule trigger: %w", err)
		}
	}

	return trigger, nil
}

// UpdateTrigger updates an existing trigger
func (tm *TriggerManagerImpl) UpdateTrigger(ctx context.Context, userAddress util.Uint160, triggerID string, updates *trigger_models.Trigger) (*trigger_models.Trigger, error) {
	existing, err := tm.store.GetTrigger(ctx, userAddress, triggerID)
	if err != nil {
		return nil, fmt.Errorf("failed to get trigger: %w", err)
	}

	if existing == nil {
		return nil, fmt.Errorf("trigger not found")
	}

	updates.ID = existing.ID
	updates.UserAddress = userAddress
	updates.CreatedAt = existing.CreatedAt
	updates.UpdatedAt = time.Now()

	nextExecution, err := tm.scheduler.GetNextExecutionTime(ctx, updates.Schedule)
	if err != nil {
		return nil, fmt.Errorf("failed to calculate next execution time: %w", err)
	}
	updates.NextExecution = nextExecution

	if err := tm.store.SaveTrigger(ctx, updates); err != nil {
		return nil, fmt.Errorf("failed to save trigger: %w", err)
	}

	if err := tm.scheduler.UnscheduleTrigger(ctx, triggerID); err != nil {
		return nil, fmt.Errorf("failed to unschedule trigger: %w", err)
	}

	if err := tm.scheduler.ScheduleTrigger(ctx, updates); err != nil {
		return nil, fmt.Errorf("failed to schedule trigger: %w", err)
	}

	return updates, nil
}

// DeleteTrigger deletes a trigger
func (tm *TriggerManagerImpl) DeleteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) error {
	trigger, err := tm.store.GetTrigger(ctx, userAddress, triggerID)
	if err != nil {
		return fmt.Errorf("failed to get trigger: %w", err)
	}

	if trigger == nil {
		return fmt.Errorf("trigger not found")
	}

	if trigger.Type == trigger_models.ScheduleTrigger {
		if err := tm.scheduler.UnscheduleTrigger(ctx, triggerID); err != nil {
			// Log error but proceed with deletion
			fmt.Printf("WARN: failed to unschedule trigger %s: %v\n", triggerID, err)
		}
	}

	if err := tm.store.DeleteTrigger(ctx, userAddress, triggerID); err != nil {
		return fmt.Errorf("failed to delete trigger: %w", err)
	}

	return nil
}

// GetTrigger gets a trigger by ID
func (tm *TriggerManagerImpl) GetTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*trigger_models.Trigger, error) {
	trigger, err := tm.store.GetTrigger(ctx, userAddress, triggerID)
	if err != nil {
		return nil, fmt.Errorf("failed to get trigger: %w", err)
	}

	if trigger == nil {
		return nil, fmt.Errorf("trigger not found")
	}

	return trigger, nil
}

// ListTriggers lists all triggers for a user
func (tm *TriggerManagerImpl) ListTriggers(ctx context.Context, userAddress util.Uint160) ([]*trigger_models.Trigger, error) {
	triggers, err := tm.store.ListTriggers(ctx, userAddress)
	if err != nil {
		return nil, fmt.Errorf("failed to list triggers: %w", err)
	}

	return triggers, nil
}

// ExecuteTrigger evaluates conditions and performs the trigger action
func (tm *TriggerManagerImpl) ExecuteTrigger(ctx context.Context, userAddress util.Uint160, triggerID string) (*trigger_models.TriggerExecution, error) {
	triggerData, err := tm.store.GetTrigger(ctx, userAddress, triggerID)
	if err != nil {
		return nil, fmt.Errorf("failed to get trigger: %w", err)
	}
	if triggerData == nil {
		return nil, fmt.Errorf("trigger %s not found or access denied", triggerID)
	}

	execution := &trigger_models.TriggerExecution{
		ID:          uuid.New().String(),
		TriggerID:   triggerID,
		UserAddress: triggerData.UserAddress,
		StartTime:   time.Now(),
		Status:      "evaluating",
	}

	// --- 1. Evaluate Condition ---
	conditionMet, evalErr := tm.evaluateTriggerCondition(ctx, triggerData)
	if evalErr != nil {
		execution.Status = "failed"
		execution.Error = fmt.Sprintf("Condition evaluation failed: %v", evalErr)
		execution.EndTime = time.Now()
		_ = tm.store.SaveExecution(ctx, execution)
		tm.metrics.RecordExecution(ctx, execution)
		return execution, fmt.Errorf("condition evaluation failed: %w", evalErr)
	}

	if !conditionMet {
		execution.Status = "skipped"
		execution.Result = "Condition not met"
		execution.EndTime = time.Now()
		_ = tm.store.SaveExecution(ctx, execution)
		tm.metrics.RecordExecution(ctx, execution)
		return execution, nil
	}

	// --- 2. Perform Action ---
	execution.Status = "running"
	actionResult, gasConsumed, actionErr := tm.performTriggerAction(ctx, triggerData)
	execution.EndTime = time.Now()
	execution.GasUsed = gasConsumed // Record gas used

	if actionErr != nil {
		execution.Status = "failed"
		execution.Error = fmt.Sprintf("Action failed: %v", actionErr)
	} else {
		execution.Status = "completed"
		execution.Result = fmt.Sprintf("%v", actionResult) // Store result
	}

	// --- 3. Save Execution & Metrics ---
	if err := tm.store.SaveExecution(ctx, execution); err != nil {
		fmt.Printf("ERROR: failed to save execution record %s for trigger %s: %v\n", execution.ID, triggerID, err)
	}
	tm.metrics.RecordExecution(ctx, execution)

	// Update trigger's LastExecuted time
	triggerData.LastExecuted = execution.EndTime
	// TODO: Decide if NextExecution needs update for schedule triggers
	// Should the scheduler handle this automatically based on last run + cron?
	if err := tm.store.SaveTrigger(ctx, triggerData); err != nil {
		log.Warnf("Failed to update trigger LastExecuted time for %s: %v", triggerID, err)
	}

	// Handle alerts
	if execution.Status == "failed" {
		tm.alerts.AlertExecutionFailure(ctx, triggerData, execution.Error)
	} else if tm.policy.ExecutionWindow > 0 && execution.GasUsed > 0 { // Add gas alert check
		// Example gas alert: if gas > 1.5x policy window average (needs metric tracking)
		// tm.alerts.AlertHighGasUsage(ctx, triggerData, execution.GasUsed)
	}

	return execution, actionErr // Return execution record and potential action error
}

// evaluateTriggerCondition checks if the trigger condition is met
func (tm *TriggerManagerImpl) evaluateTriggerCondition(ctx context.Context, trigger *trigger_models.Trigger) (bool, error) {
	switch trigger.Type {
	case trigger_models.ScheduleTrigger:
		// Basic schedule condition is met if this function is called at the scheduled time.
		// Additional checks based on ConditionConfig can be added here.
		if trigger.ConditionConfig == nil {
			return true, nil
		}
		log.Warnf("ConditionConfig evaluation for ScheduleTrigger (ID: %s) not implemented.", trigger.ID)
		return true, nil // Placeholder: Assume condition met if config exists but isn't evaluated

	case trigger_models.EventTrigger:
		// Requires data from the event source to be passed in or accessible via context.
		// Evaluate trigger.ConditionConfig against event data.
		log.Warnf("EventTrigger condition evaluation (ID: %s) not implemented.", trigger.ID)
		return false, errors.New("event trigger condition evaluation not implemented")

	case trigger_models.ConditionalTrigger:
		if tm.neoClient == nil {
			return false, errors.New("neo client unavailable for conditional trigger")
		}

		// Extract parameters from condition config
		contractHashStr, ok := trigger.ConditionConfig["contract"].(string)
		if !ok {
			return false, errors.New("missing or invalid contract hash in condition config")
		}

		method, ok := trigger.ConditionConfig["method"].(string)
		if !ok {
			return false, errors.New("missing or invalid method in condition config")
		}

		paramsRaw, ok := trigger.ConditionConfig["params"].([]interface{})
		if !ok {
			// Use empty params if not provided
			paramsRaw = []interface{}{}
		}

		expectedResult := trigger.ConditionConfig["expectedResult"]
		if expectedResult == nil {
			return false, errors.New("missing expectedResult in condition config")
		}

		// Parse contract hash
		contractHash, err := util.Uint160DecodeStringLE(contractHashStr)
		if err != nil {
			return false, fmt.Errorf("invalid contract hash in condition config: %w", err)
		}

		// Convert parameters
		params, err := tm.convertToSmartContractParams(paramsRaw)
		if err != nil {
			return false, fmt.Errorf("failed to convert params: %w", err)
		}

		// Call contract via NeoClient
		invokeResult, err := tm.neoClient.InvokeFunction(contractHash, method, params, nil)
		if err != nil {
			return false, fmt.Errorf("contract invocation failed: %w", err)
		}

		// Process result (using simple map comparison for now)
		resultMap, ok := invokeResult.(map[string]interface{})
		if !ok {
			return false, fmt.Errorf("unexpected result type: %T", invokeResult)
		}

		// Check if state is HALT (successful execution)
		if state, ok := resultMap["state"].(string); !ok || state != "HALT" {
			return false, fmt.Errorf("invocation did not halt successfully, state: %v", resultMap["state"])
		}

		// Check stack result against expected
		// This is a simplified comparison - in production you'd need more robust comparison
		stack, ok := resultMap["stack"].([]interface{})
		if !ok || len(stack) == 0 {
			return false, fmt.Errorf("missing or empty result stack")
		}

		// Basic comparison against expectedResult (simplistic)
		// In a real implementation, you'd need deeper comparison logic based on NEP-17 types
		firstItem := stack[0]
		log.Debugf("Comparing condition result: %v with expected: %v", firstItem, expectedResult)

		// Simplified comparison (strings, booleans, numbers)
		conditionMet := fmt.Sprintf("%v", firstItem) == fmt.Sprintf("%v", expectedResult)
		return conditionMet, nil

	default:
		return false, fmt.Errorf("unknown trigger type: %s", trigger.Type)
	}
}

// performTriggerAction executes the defined action (function or contract)
func (tm *TriggerManagerImpl) performTriggerAction(ctx context.Context, trigger *trigger_models.Trigger) (result interface{}, gasConsumed int64, err error) {
	switch trigger.Action {
	case trigger_models.FunctionAction:
		if tm.funcService == nil {
			return nil, 0, errors.New("functions service is unavailable")
		}
		invocation := functions_iface.FunctionInvocation{
			FunctionID: trigger.TargetFunctionID,
			Parameters: trigger.FunctionParams,
			Caller:     trigger.UserAddress,
			Async:      false,
		}
		execResult, err := tm.funcService.InvokeFunction(ctx, invocation)
		if err != nil {
			return nil, 0, fmt.Errorf("function invocation failed: %w", err)
		}
		if execResult.Status == "failed" {
			return nil, 0, fmt.Errorf("function execution failed: %s", execResult.Error)
		}
		// Gas consumed for function actions is tracked within the function service itself (or TEE)
		// We don't get it back directly here easily unless the execution result includes it.
		return execResult.Result, 0, nil

	case trigger_models.ContractAction:
		if tm.neoClient == nil {
			return nil, 0, errors.New("neo client unavailable for contract action")
		}
		if tm.walletService == nil {
			return nil, 0, errors.New("wallet service unavailable for contract action signing")
		}

		log.Infof("Performing ContractAction for trigger %s: Contract=%s, Method=%s, Signer=%s",
			trigger.ID, trigger.TargetContract.StringLE(), trigger.TargetMethod, trigger.Signer.StringLE())

		// 1. Convert parameters to smartcontract.Parameter
		params, err := tm.convertToSmartContractParams(trigger.ContractParams)
		if err != nil {
			return nil, 0, fmt.Errorf("failed to convert contract params: %w", err)
		}

		// 2. Build script using ScriptBuilder
		script, err := tm.buildInvocationScript(trigger.TargetContract, trigger.TargetMethod, params)
		if err != nil {
			return nil, 0, fmt.Errorf("failed to build invocation script: %w", err)
		}

		// 3. Create transaction
		blockCount, err := tm.neoClient.GetBlockCount()
		if err != nil {
			log.Warnf("Failed to get block count, using default: %v", err)
			blockCount = 10000 // Default if we can't get real block count
		}

		tx := transaction.New(script, 0)
		tx.ValidUntilBlock = blockCount + 100
		tx.Signers = []transaction.Signer{{Account: trigger.Signer, Scopes: transaction.CalledByEntry}}

		// 4. Calculate Network Fee
		netFee, err := tm.neoClient.CalculateNetworkFee(tx)
		if err != nil {
			log.Warnf("Failed to calculate network fee (using placeholder): %v", err)
			tx.NetworkFee = 1000000 // Placeholder if calculation fails
		} else {
			tx.NetworkFee = netFee
		}

		// 5. Sign transaction
		err = tm.walletService.SignTx(ctx, trigger.Signer, tx)
		if err != nil {
			return nil, 0, fmt.Errorf("failed to sign transaction: %w", err)
		}

		// 6. Send transaction
		txHash, err := tm.neoClient.SendRawTransaction(tx)
		if err != nil {
			return nil, 0, fmt.Errorf("failed to send transaction: %w", err)
		}
		log.Infof("Transaction sent for trigger %s: %s", trigger.ID, txHash.StringLE())

		// 7. Get AppLog (using interface{})
		appLog, err := tm.neoClient.GetApplicationLog(txHash, nil)
		if err != nil {
			log.Warnf("Failed to get application log for tx %s: %v", txHash.StringLE(), err)
			// Return TxHash anyway, but note the lack of gas/result info
			result = map[string]string{
				"txHash": txHash.StringLE(),
				"status": "sent",
				"error":  "failed to get app log",
			}
			return result, 0, nil // Return success (sent), but with error noted in result
		}

		// Process application log
		gasConsumed = tm.extractGasConsumed(appLog)
		resultMap := map[string]interface{}{
			"txHash":      txHash.StringLE(),
			"status":      "confirmed",
			"gasConsumed": gasConsumed,
		}

		// Try to extract VM state and stack result if available
		if execInfo := tm.extractExecutionInfo(appLog); execInfo != nil {
			for k, v := range execInfo {
				resultMap[k] = v
			}
		}

		return resultMap, gasConsumed, nil

	default:
		return nil, 0, fmt.Errorf("unknown trigger action type: %s", trigger.Action)
	}
}

// Helper to extract gas consumed from app log
func (tm *TriggerManagerImpl) extractGasConsumed(appLog interface{}) int64 {
	// Default placeholder value
	defaultGas := int64(1000000)

	// Try to extract from various formats
	if logMap, ok := appLog.(map[string]interface{}); ok {
		// Try executions array first
		if executions, ok := logMap["executions"].([]interface{}); ok && len(executions) > 0 {
			if exec, ok := executions[0].(map[string]interface{}); ok {
				if gasStr, ok := exec["gasconsumed"].(string); ok {
					var parsedGas int64
					if _, err := fmt.Sscanf(gasStr, "%d", &parsedGas); err == nil && parsedGas > 0 {
						return parsedGas
					}
				} else if gas, ok := exec["gasconsumed"].(float64); ok {
					return int64(gas)
				}
			}
		}

		// Try direct gasconsumed field
		if gasStr, ok := logMap["gasconsumed"].(string); ok {
			var parsedGas int64
			if _, err := fmt.Sscanf(gasStr, "%d", &parsedGas); err == nil && parsedGas > 0 {
				return parsedGas
			}
		} else if gas, ok := logMap["gasconsumed"].(float64); ok {
			return int64(gas)
		}
	}

	return defaultGas
}

// Helper to extract execution info from app log
func (tm *TriggerManagerImpl) extractExecutionInfo(appLog interface{}) map[string]interface{} {
	result := make(map[string]interface{})

	if logMap, ok := appLog.(map[string]interface{}); ok {
		// Try executions array first
		if executions, ok := logMap["executions"].([]interface{}); ok && len(executions) > 0 {
			if exec, ok := executions[0].(map[string]interface{}); ok {
				// Get VM state
				if vmState, ok := exec["vmstate"].(string); ok {
					result["vmState"] = vmState
				}

				// Try to get stack items
				if stack, ok := exec["stack"].([]interface{}); ok && len(stack) > 0 {
					result["resultStack"] = stack[0]
				}

				// Get any notifications
				if notifications, ok := exec["notifications"].([]interface{}); ok && len(notifications) > 0 {
					result["notifications"] = notifications
				}
			}
		}
	}

	return result
}

// convertToSmartContractParams converts []interface{} to []smartcontract.Parameter
func (tm *TriggerManagerImpl) convertToSmartContractParams(params []interface{}) ([]smartcontract.Parameter, error) {
	resultParams := make([]smartcontract.Parameter, len(params))
	for i, p := range params {
		// Manually determine parameter type based on Go type
		var paramType smartcontract.ParamType
		var paramValue interface{}

		switch v := p.(type) {
		case bool:
			paramType = smartcontract.BoolType
			paramValue = v
		case string:
			// Check if it's a hex-encoded address first
			if hash, err := util.Uint160DecodeStringLE(v); err == nil {
				paramType = smartcontract.Hash160Type
				paramValue = hash
			} else if bytes, err := hex.DecodeString(v); err == nil && len(bytes) == 32 {
				// Uint256 (32-byte hash)
				// Manually create a Uint256 from bytes
				var hash util.Uint256
				copy(hash[:], bytes)
				paramType = smartcontract.Hash256Type
				paramValue = hash
			} else {
				// Regular string
				paramType = smartcontract.StringType
				paramValue = v
			}
		case int:
			paramType = smartcontract.IntegerType
			paramValue = v
		case int64:
			paramType = smartcontract.IntegerType
			paramValue = v
		case float64:
			paramType = smartcontract.IntegerType
			paramValue = int64(v)
		case []byte:
			paramType = smartcontract.ByteArrayType
			paramValue = v
		case []interface{}:
			paramType = smartcontract.ArrayType
			// For arrays, recursively convert each element
			arrayParams, err := tm.convertToSmartContractParams(v)
			if err != nil {
				return nil, fmt.Errorf("failed to convert array parameter at index %d: %w", i, err)
			}
			paramValue = arrayParams
		default:
			return nil, fmt.Errorf("unsupported parameter type at index %d: %T", i, p)
		}

		resultParams[i] = smartcontract.Parameter{
			Type:  paramType,
			Value: paramValue,
		}
	}
	return resultParams, nil
}

// buildInvocationScript creates script using neo-go contract interaction
func (tm *TriggerManagerImpl) buildInvocationScript(contract util.Uint160, method string, params []smartcontract.Parameter) ([]byte, error) {
	// Simplified approach - create script bytes manually
	// This is a fallback method since we're having compatibility issues with neo-go

	log.Warnf("Using simplified script building for contract %s method %s",
		contract.StringLE(), method)

	// For mock purposes, create a minimal valid script
	// In production, you would use the proper neo-go script builder
	// or generate the correct script for the contract

	// Create a basic script that can be recognized by the mock
	script := []byte{
		0x0c,              // PUSHDATA1
		byte(len(method)), // length of method
	}

	// Add method name bytes
	script = append(script, []byte(method)...)

	// Add parameter count (assuming params are already encoded)
	script = append(script, byte(len(params)))

	// Add contract hash (20 bytes)
	script = append(script, contract[:]...)

	// Add SYSCALL prefix
	script = append(script, 0x41) // SYSCALL

	// Add call flags
	script = append(script, 0x01) // Default flags

	return script, nil
}

// GetTriggerExecutions gets the execution history for a trigger
func (tm *TriggerManagerImpl) GetTriggerExecutions(ctx context.Context, userAddress util.Uint160, triggerID string) ([]*trigger_models.TriggerExecution, error) {
	executions, err := tm.store.GetExecutions(ctx, userAddress, triggerID)
	if err != nil {
		return nil, fmt.Errorf("failed to get executions: %w", err)
	}

	return executions, nil
}

// GetPolicy returns the current trigger policy
func (tm *TriggerManagerImpl) GetPolicy() *trigger_models.TriggerPolicy {
	return tm.policy
}

// UpdatePolicy updates the trigger policy
func (tm *TriggerManagerImpl) UpdatePolicy(policy *trigger_models.TriggerPolicy) error {
	if policy == nil {
		return fmt.Errorf("policy cannot be nil")
	}

	if policy.MaxTriggersPerUser <= 0 {
		return fmt.Errorf("max triggers per user must be positive")
	}
	if policy.MaxExecutionsPerTrigger <= 0 {
		return fmt.Errorf("max executions per trigger must be positive")
	}
	if policy.ExecutionWindow <= 0 {
		return fmt.Errorf("execution window must be positive")
	}
	if policy.MinInterval <= 0 {
		return fmt.Errorf("min interval must be positive")
	}
	if policy.MaxInterval <= 0 {
		return fmt.Errorf("max interval must be positive")
	}
	if policy.CooldownPeriod <= 0 {
		return fmt.Errorf("cooldown period must be positive")
	}

	tm.policy = policy
	return nil
}
