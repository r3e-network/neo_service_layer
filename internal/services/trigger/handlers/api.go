package handlers

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"time"

	"github.com/will/neo_service_layer/internal/services/trigger"
)

// APIHandler handles API-based triggers
type APIHandler struct {
	client        *http.Client
	endpoints     map[string]string
	defaultHeader map[string]string
	timeout       time.Duration
}

// APIConfig represents configuration for the API handler
type APIConfig struct {
	Endpoints     map[string]string `json:"endpoints"`
	DefaultHeader map[string]string `json:"defaultHeader"`
	Timeout       int               `json:"timeout"`
}

// NewAPIHandler creates a new API handler
func NewAPIHandler(config *APIConfig) *APIHandler {
	timeout := 10 * time.Second
	if config.Timeout > 0 {
		timeout = time.Duration(config.Timeout) * time.Second
	}

	return &APIHandler{
		client:        &http.Client{Timeout: timeout},
		endpoints:     config.Endpoints,
		defaultHeader: config.DefaultHeader,
		timeout:       timeout,
	}
}

// HandleTrigger handles a trigger event using an API call
func (h *APIHandler) HandleTrigger(ctx context.Context, t *trigger.Trigger) (*trigger.TriggerResult, error) {
	// Get endpoint URL from trigger configuration or default endpoints
	endpoint := ""
	if epConfig, ok := t.Config["endpoint"]; ok {
		endpoint = epConfig
	}

	// If no endpoint specified in trigger, try to get it from predefined endpoints
	if endpoint == "" {
		if ep, ok := h.endpoints[string(t.Type)]; ok {
			endpoint = ep
		} else {
			return nil, fmt.Errorf("no endpoint specified for trigger")
		}
	}

	// Prepare request payload
	payload := map[string]interface{}{
		"trigger_id":   t.ID,
		"trigger_type": t.Type,
		"timestamp":    time.Now().Unix(),
		"parameters":   t.Parameters,
	}

	// Add custom payload from trigger configuration
	if customPayload, ok := t.Config["payload"]; ok {
		if payloadMap, err := unmarshalToMap(customPayload); err == nil {
			for k, v := range payloadMap {
				payload[k] = v
			}
		}
	}

	// Marshal payload to JSON
	payloadData, err := json.Marshal(payload)
	if err != nil {
		return nil, fmt.Errorf("failed to marshal payload: %w", err)
	}

	// Create request
	req, err := http.NewRequestWithContext(ctx, "POST", endpoint, bytes.NewBuffer(payloadData))
	if err != nil {
		return nil, fmt.Errorf("failed to create request: %w", err)
	}

	// Add headers
	req.Header.Set("Content-Type", "application/json")
	for k, v := range h.defaultHeader {
		req.Header.Set(k, v)
	}

	// Add custom headers from trigger configuration
	if headers, ok := t.Config["headers"]; ok {
		if headerMap, err := unmarshalToMap(headers); err == nil {
			for k, v := range headerMap {
				if vStr, ok := v.(string); ok {
					req.Header.Set(k, vStr)
				}
			}
		}
	}

	// Send request
	resp, err := h.client.Do(req)
	if err != nil {
		return nil, fmt.Errorf("failed to send request: %w", err)
	}
	defer resp.Body.Close()

	// Parse response
	var respData map[string]interface{}
	if err := json.NewDecoder(resp.Body).Decode(&respData); err != nil {
		return nil, fmt.Errorf("failed to parse response: %w", err)
	}

	// Encode response data to JSON string
	respDataJSON, err := json.Marshal(respData)
	if err != nil {
		return nil, fmt.Errorf("failed to marshal response data: %w", err)
	}

	// Check response status
	if resp.StatusCode < 200 || resp.StatusCode >= 300 {
		return &trigger.TriggerResult{
			TriggerID:   t.ID,
			Status:      trigger.TriggerStatusFailed,
			Message:     fmt.Sprintf("HTTP error: %d", resp.StatusCode),
			Data:        string(respDataJSON),
			TriggeredAt: time.Now(),
			Error:       fmt.Sprintf("HTTP error: %d", resp.StatusCode),
		}, nil
	}

	// Create result
	result := &trigger.TriggerResult{
		TriggerID:   t.ID,
		Status:      trigger.TriggerStatusTriggered,
		Message:     "Successfully executed API trigger",
		Data:        string(respDataJSON),
		TriggeredAt: time.Now(),
	}

	return result, nil
}

// GetName returns the name of the handler
func (h *APIHandler) GetName() string {
	return "api"
}

// GetDescription returns the description of the handler
func (h *APIHandler) GetDescription() string {
	return "Handles triggers via API calls"
}

// GetSupportedEvents returns the list of supported event types
func (h *APIHandler) GetSupportedEvents() []string {
	return []string{"api", "webhook", "http"}
}

// Initialize registers this handler with the trigger service
func (h *APIHandler) Initialize(ctx context.Context) error {
	// Nothing to initialize
	return nil
}

// Validate validates trigger parameters
func (h *APIHandler) Validate(params string) error {
	var p map[string]interface{}
	if err := json.Unmarshal([]byte(params), &p); err != nil {
		return fmt.Errorf("invalid API trigger parameters: %w", err)
	}

	// Check if endpoint is specified in trigger parameters
	endpoint, ok := p["endpoint"].(string)
	if !ok || endpoint == "" {
		return fmt.Errorf("endpoint must be specified for API trigger")
	}

	return nil
}

// Helper function to unmarshal string to map
func unmarshalToMap(s string) (map[string]interface{}, error) {
	var result map[string]interface{}
	if err := json.Unmarshal([]byte(s), &result); err != nil {
		return nil, err
	}
	return result, nil
}
