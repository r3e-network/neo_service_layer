package types

import (
	"fmt"
	"time"

	"github.com/will/neo_service_layer/internal/common/retry"
)

// CreateTriggerRequest represents a request to create a new trigger
type CreateTriggerRequest struct {
	Name         string                 `json:"name"`
	FunctionName string                 `json:"functionName"`
	Schedule     string                 `json:"schedule,omitempty"`
	Contract     string                 `json:"contract,omitempty"`
	Event        string                 `json:"event,omitempty"`
	Method       string                 `json:"method,omitempty"`
	Condition    string                 `json:"condition,omitempty"`
	RetryPolicy  retry.Policy           `json:"retryPolicy"`
	Enabled      bool                   `json:"enabled"`
	MaxGas       float64                `json:"maxGas"`
	Parameters   map[string]interface{} `json:"parameters,omitempty"`
}

// Validate validates the create trigger request
func (r *CreateTriggerRequest) Validate() error {
	if r.Name == "" {
		return fmt.Errorf("name is required")
	}
	if r.FunctionName == "" {
		return fmt.Errorf("function name is required")
	}
	if r.Schedule == "" && r.Contract == "" {
		return fmt.Errorf("either schedule or contract is required")
	}
	if r.Contract != "" {
		if r.Event == "" && r.Method == "" {
			return fmt.Errorf("either event or method is required for contract trigger")
		}
		if r.Method != "" && r.Condition == "" {
			return fmt.Errorf("condition is required for method trigger")
		}
	}
	return nil
}

// ListTriggersRequest represents a request to list triggers
type ListTriggersRequest struct {
	Status string `json:"status,omitempty"`
	Limit  int    `json:"limit,omitempty"`
}

// GetTriggerHistoryRequest represents a request to get trigger execution history
type GetTriggerHistoryRequest struct {
	Limit int           `json:"limit,omitempty"`
	Since time.Duration `json:"since,omitempty"`
}

// Trigger represents a trigger
type Trigger struct {
	ID            string                 `json:"id"`
	Name          string                 `json:"name"`
	FunctionName  string                 `json:"functionName"`
	Schedule      string                 `json:"schedule,omitempty"`
	Contract      string                 `json:"contract,omitempty"`
	Event         string                 `json:"event,omitempty"`
	Method        string                 `json:"method,omitempty"`
	Condition     string                 `json:"condition,omitempty"`
	RetryPolicy   retry.Policy           `json:"retryPolicy"`
	Enabled       bool                   `json:"enabled"`
	MaxGas        float64                `json:"maxGas"`
	Parameters    map[string]interface{} `json:"parameters,omitempty"`
	Status        string                 `json:"status"`
	LastExecution time.Time              `json:"lastExecution"`
	NextExecution time.Time              `json:"nextExecution"`
}

// TriggerExecution represents a trigger execution
type TriggerExecution struct {
	Timestamp time.Time `json:"timestamp"`
	Status    string    `json:"status"`
	GasUsed   float64   `json:"gasUsed"`
	TxHash    string    `json:"txHash"`
}

// TriggerStatus represents trigger status
type TriggerStatus struct {
	Name           string    `json:"name"`
	Status         string    `json:"status"`
	LastExecution  time.Time `json:"lastExecution"`
	NextExecution  time.Time `json:"nextExecution"`
	SuccessCount   int       `json:"successCount"`
	ErrorCount     int       `json:"errorCount"`
	AverageGasUsed float64   `json:"averageGasUsed"`
	LastError      string    `json:"lastError,omitempty"`
}
