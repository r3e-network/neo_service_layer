package trigger

import (
	"context"
	"time"

	"sync"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/wallet"
	"go.uber.org/zap"
)

// TriggerType represents the type of trigger
type TriggerType string

const (
	// TriggerTypeEvent represents an event-based trigger
	TriggerTypeEvent TriggerType = "event"
	// TriggerTypeSchedule represents a schedule-based trigger
	TriggerTypeSchedule TriggerType = "schedule"
)

// TriggerStatus represents the status of a trigger
type TriggerStatus string

const (
	// TriggerStatusActive indicates the trigger is active
	TriggerStatusActive TriggerStatus = "active"
	// TriggerStatusInactive indicates the trigger is inactive
	TriggerStatusInactive TriggerStatus = "inactive"
	// TriggerStatusFailed indicates the trigger has failed
	TriggerStatusFailed TriggerStatus = "failed"
)

// ExecutionStatus represents the status of a trigger execution
type ExecutionStatus string

const (
	// ExecutionStatusPending indicates the execution is pending
	ExecutionStatusPending ExecutionStatus = "pending"
	// ExecutionStatusRunning indicates the execution is running
	ExecutionStatusRunning ExecutionStatus = "running"
	// ExecutionStatusCompleted indicates the execution completed successfully
	ExecutionStatusCompleted ExecutionStatus = "completed"
	// ExecutionStatusFailed indicates the execution failed
	ExecutionStatusFailed ExecutionStatus = "failed"
)

// Trigger represents a trigger in the system
type Trigger struct {
	ID             string                 `json:"id"`
	Type           TriggerType            `json:"type"`
	ContractHash   util.Uint160           `json:"contract_hash"`
	EventName      string                 `json:"event_name,omitempty"`
	Schedule       string                 `json:"schedule,omitempty"`
	Condition      string                 `json:"condition"`
	Action         string                 `json:"action"`
	Owner          util.Uint160           `json:"owner"`
	Status         TriggerStatus          `json:"status"`
	CreatedAt      time.Time              `json:"created_at"`
	UpdatedAt      time.Time              `json:"updated_at"`
	LastExecuted   time.Time              `json:"last_executed,omitempty"`
	ExecutionCount int64                  `json:"execution_count"`
	Metadata       map[string]interface{} `json:"metadata,omitempty"`
}

// Event represents a blockchain event
type Event struct {
	ID           string                 `json:"id"`
	ContractHash util.Uint160           `json:"contract_hash"`
	Name         string                 `json:"name"`
	Data         map[string]interface{} `json:"data"`
	Timestamp    time.Time              `json:"timestamp"`
}

// TriggerExecution represents a single execution of a trigger
type TriggerExecution struct {
	ID          string          `json:"id"`
	TriggerID   string          `json:"trigger_id"`
	EventID     string          `json:"event_id,omitempty"`
	Status      ExecutionStatus `json:"status"`
	StartTime   time.Time       `json:"start_time"`
	EndTime     time.Time       `json:"end_time,omitempty"`
	Duration    int64           `json:"duration,omitempty"`
	Result      interface{}     `json:"result,omitempty"`
	Error       string          `json:"error,omitempty"`
	RetryCount  int             `json:"retry_count"`
	NextRetryAt time.Time       `json:"next_retry_at,omitempty"`
}

// Service represents the trigger service
type Service struct {
	config    *Config
	triggers  map[string]*Trigger
	eventCh   chan *Event
	stopCh    chan struct{}
	store     Store
	scheduler Scheduler
	executor  Executor
	metrics   MetricsCollector
	logger    *zap.Logger
	mu        sync.RWMutex
	evaluator Evaluator
	wallet    *wallet.Wallet
}

// Store defines the interface for trigger storage
type Store interface {
	SaveTrigger(trigger *Trigger) error
	GetTrigger(id string) (*Trigger, error)
	ListTriggers(owner util.Uint160) ([]*Trigger, error)
	DeleteTrigger(id string) error
	SaveExecution(execution *TriggerExecution) error
	GetExecution(id string) (*TriggerExecution, error)
	ListExecutions(triggerID string) ([]*TriggerExecution, error)
}

// Scheduler defines the interface for trigger scheduling
type Scheduler interface {
	Start() error
	Stop() error
	AddTrigger(trigger *Trigger) error
	RemoveTrigger(id string) error
}

// Executor defines the interface for trigger execution
type Executor interface {
	Execute(ctx context.Context, trigger *Trigger, event *Event) (interface{}, error)
}

// MetricsCollector defines the interface for metrics collection
type MetricsCollector interface {
	RecordExecution(execution *TriggerExecution)
	RecordEventProcessed(event *Event)
	RecordTriggerCreated(trigger *Trigger)
	RecordTriggerDeleted(trigger *Trigger)
	RecordError(category string, err error)
}

// Evaluator evaluates conditions against a context
type Evaluator interface {
	// Evaluate evaluates a condition against a context
	Evaluate(condition string, context map[string]interface{}) (interface{}, error)

	// EvaluateComparison evaluates a comparison condition
	EvaluateComparison(field string, operator string, value interface{}, context map[string]interface{}) (bool, error)

	// EvaluateMultiple evaluates multiple conditions with AND/OR logic
	EvaluateMultiple(conditions []string, operator string, context map[string]interface{}) (bool, error)
}
