package trigger

import (
	"context"
	"time"
)

// TriggerStatus represents the status of a trigger
type TriggerStatus string

const (
	// TriggerStatusPending indicates a trigger is waiting to be processed
	TriggerStatusPending TriggerStatus = "pending"

	// TriggerStatusActive indicates a trigger is active and waiting for conditions
	TriggerStatusActive TriggerStatus = "active"

	// TriggerStatusTriggered indicates a trigger has been triggered
	TriggerStatusTriggered TriggerStatus = "triggered"

	// TriggerStatusCompleted indicates a trigger has completed its execution
	TriggerStatusCompleted TriggerStatus = "completed"

	// TriggerStatusFailed indicates a trigger has failed
	TriggerStatusFailed TriggerStatus = "failed"

	// TriggerStatusDisabled indicates a trigger is disabled
	TriggerStatusDisabled TriggerStatus = "disabled"
)

// TriggerType represents the type of a trigger
type TriggerType string

const (
	// TriggerTypeBlockchain represents a blockchain event trigger
	TriggerTypeBlockchain TriggerType = "blockchain"

	// TriggerTypeAPI represents an API-based trigger
	TriggerTypeAPI TriggerType = "api"

	// TriggerTypeTime represents a time-based trigger
	TriggerTypeTime TriggerType = "time"

	// TriggerTypeCondition represents a condition-based trigger
	TriggerTypeCondition TriggerType = "condition"
)

// Trigger represents a trigger that initiates actions when conditions are met
type Trigger struct {
	ID          string            `json:"id"`
	Name        string            `json:"name"`
	Description string            `json:"description"`
	Type        TriggerType       `json:"type"`
	Handler     string            `json:"handler"`
	Parameters  string            `json:"parameters"`
	Status      TriggerStatus     `json:"status"`
	Config      map[string]string `json:"config"`
	Metadata    map[string]string `json:"metadata"`
	CreatedAt   time.Time         `json:"created_at"`
	UpdatedAt   time.Time         `json:"updated_at"`
	CreatedBy   string            `json:"created_by"`
}

// TriggerResult represents the result of a trigger execution
type TriggerResult struct {
	TriggerID   string        `json:"trigger_id"`
	Status      TriggerStatus `json:"status"`
	Message     string        `json:"message"`
	Data        string        `json:"data"`
	TriggeredAt time.Time     `json:"triggered_at"`
	Error       string        `json:"error,omitempty"`
}

// TriggerHandler defines the interface for trigger handlers
type TriggerHandler interface {
	// HandleTrigger processes a trigger
	HandleTrigger(ctx context.Context, trigger *Trigger) (*TriggerResult, error)

	// Start initializes the trigger handler
	Start(ctx context.Context) error

	// Stop stops the trigger handler
	Stop(ctx context.Context) error

	// GetName returns the name of the handler
	GetName() string

	// GetDescription returns the description of the handler
	GetDescription() string

	// GetSupportedEvents returns the list of supported event types
	GetSupportedEvents() []string

	// Initialize registers this handler with the trigger service
	Initialize(ctx context.Context) error

	// Validate validates trigger parameters
	Validate(params string) error
}

// TriggerCallback defines the callback function for trigger results
type TriggerCallback func(ctx Context, result *TriggerResult) error

// CreateTriggerParams represents parameters for creating a trigger
type CreateTriggerParams struct {
	Name        string            `json:"name"`
	Description string            `json:"description"`
	Type        TriggerType       `json:"type"`
	Handler     string            `json:"handler"`
	Parameters  string            `json:"parameters"`
	Config      map[string]string `json:"config"`
	Metadata    map[string]string `json:"metadata"`
}

// UpdateTriggerParams represents parameters for updating a trigger
type UpdateTriggerParams struct {
	ID          string            `json:"id"`
	Name        string            `json:"name,omitempty"`
	Description string            `json:"description,omitempty"`
	Parameters  string            `json:"parameters,omitempty"`
	Config      map[string]string `json:"config,omitempty"`
	Metadata    map[string]string `json:"metadata,omitempty"`
	Status      TriggerStatus     `json:"status,omitempty"`
}

// Context is an alias for context.Context
type Context = interface {
	Deadline() (deadline time.Time, ok bool)
	Done() <-chan struct{}
	Err() error
	Value(key interface{}) interface{}
}
