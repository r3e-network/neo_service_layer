package trigger

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/config/netmode"
	"github.com/nspcc-dev/neo-go/pkg/core/transaction"
	"github.com/nspcc-dev/neo-go/pkg/rpcclient"
	"github.com/nspcc-dev/neo-go/pkg/smartcontract"
	"github.com/nspcc-dev/neo-go/pkg/util"
)

// ActionType represents the type of action to execute
type ActionType string

const (
	ActionTypeContractCall ActionType = "contract_call"
	ActionTypeWebhook      ActionType = "webhook"
	ActionTypeNotify       ActionType = "notify"
)

// ActionConfig represents the configuration for an action
type ActionConfig struct {
	Type       ActionType             `json:"type"`
	Parameters map[string]interface{} `json:"parameters"`
}

// executeAction executes the specified action for a trigger
func (s *Service) executeAction(ctx context.Context, trigger *Trigger, event *Event) error {
	// Parse action configuration
	var config ActionConfig
	if err := json.Unmarshal([]byte(trigger.Action), &config); err != nil {
		return fmt.Errorf("failed to parse action config: %w", err)
	}

	// Execute action based on type
	switch config.Type {
	case ActionTypeContractCall:
		return s.executeContractCall(ctx, trigger, event, config.Parameters)
	case ActionTypeWebhook:
		return s.executeWebhook(ctx, trigger, event, config.Parameters)
	case ActionTypeNotify:
		return s.executeNotification(trigger, event, config.Parameters)
	default:
		return fmt.Errorf("unsupported action type: %s", config.Type)
	}
}

// executeContractCall executes a contract call action
func (s *Service) executeContractCall(ctx context.Context, trigger *Trigger, event *Event, params map[string]interface{}) error {
	// Get contract hash
	contractHash, ok := params["contract_hash"].(string)
	if !ok {
		return fmt.Errorf("contract_hash parameter not found")
	}
	hash, err := util.Uint160DecodeStringLE(contractHash)
	if err != nil {
		return fmt.Errorf("invalid contract hash: %w", err)
	}

	// Get method name
	method, ok := params["method"].(string)
	if !ok {
		return fmt.Errorf("method parameter not found")
	}

	// Get method parameters
	methodParams, ok := params["parameters"].([]interface{})
	if !ok {
		methodParams = []interface{}{}
	}

	// Create script
	script, err := smartcontract.CreateCallScript(hash, method, methodParams...)
	if err != nil {
		return fmt.Errorf("failed to create call script: %w", err)
	}

	// Create RPC client
	c, err := rpcclient.New(ctx, s.config.RPCEndpoint, rpcclient.Options{})
	if err != nil {
		return fmt.Errorf("failed to create RPC client: %w", err)
	}
	defer c.Close()

	// Create transaction
	tx := transaction.New(script, 0)
	tx.Signers = []transaction.Signer{
		{
			Account: s.wallet.Accounts[0].ScriptHash(),
			Scopes:  transaction.CalledByEntry,
		},
	}

	// Sign transaction with wallet account
	account := s.wallet.Accounts[0]
	if err := account.SignTx(netmode.Magic(s.config.NetworkMagic), tx); err != nil {
		return fmt.Errorf("failed to sign transaction: %w", err)
	}

	// Send transaction
	txHash, err := c.SendRawTransaction(tx)
	if err != nil {
		return fmt.Errorf("failed to send transaction: %w", err)
	}

	// Wait for confirmation with context timeout
	ticker := time.NewTicker(time.Second)
	defer ticker.Stop()

	for i := 0; i < 60; i++ {
		select {
		case <-ctx.Done():
			return fmt.Errorf("context cancelled while waiting for transaction confirmation: %w", ctx.Err())
		case <-ticker.C:
			_, err := c.GetTransactionHeight(txHash)
			if err == nil {
				return nil
			}
		}
	}

	return fmt.Errorf("transaction not confirmed after 60 seconds")
}

// executeWebhook executes a webhook action
func (s *Service) executeWebhook(ctx context.Context, trigger *Trigger, event *Event, params map[string]interface{}) error {
	// Get webhook URL
	url, ok := params["url"].(string)
	if !ok {
		return fmt.Errorf("url parameter not found")
	}

	// Prepare payload
	payload := map[string]interface{}{
		"trigger_id": trigger.ID,
		"event":      event,
		"timestamp":  time.Now(),
	}

	// Add custom headers if specified
	headers := make(map[string]string)
	if h, ok := params["headers"].(map[string]interface{}); ok {
		for k, v := range h {
			if s, ok := v.(string); ok {
				headers[k] = s
			}
		}
	}

	// Send webhook
	jsonPayload, err := json.Marshal(payload)
	if err != nil {
		return fmt.Errorf("failed to marshal payload: %w", err)
	}

	req, err := http.NewRequestWithContext(ctx, "POST", url, bytes.NewBuffer(jsonPayload))
	if err != nil {
		return fmt.Errorf("failed to create request: %w", err)
	}

	req.Header.Set("Content-Type", "application/json")
	for k, v := range headers {
		req.Header.Set(k, v)
	}

	client := &http.Client{Timeout: 10 * time.Second}
	resp, err := client.Do(req)
	if err != nil {
		return fmt.Errorf("failed to send webhook: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode >= 400 {
		return fmt.Errorf("webhook request failed with status: %d", resp.StatusCode)
	}

	return nil
}

// executeNotification executes a notification action
func (s *Service) executeNotification(trigger *Trigger, event *Event, params map[string]interface{}) error {
	// Get notification type
	notificationType, ok := params["type"].(string)
	if !ok {
		return fmt.Errorf("notification type parameter not found")
	}

	// Get notification message
	message, ok := params["message"].(string)
	if !ok {
		return fmt.Errorf("notification message parameter not found")
	}

	// TODO: Implement notification logic based on type
	// This could include:
	// - Email notifications
	// - SMS notifications
	// - Push notifications
	// - Slack/Discord notifications
	fmt.Printf("Notification sent: Type=%s, Message=%s\n", notificationType, message)

	return nil
}
