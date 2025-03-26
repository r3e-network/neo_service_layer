package trigger

import (
	"fmt"
	"time"
)

// TriggerExecutionStatus represents the status of a trigger execution
type TriggerExecutionStatus string

const (
	// TriggerExecutionStatusPending indicates an execution is in progress
	TriggerExecutionStatusPending TriggerExecutionStatus = "pending"

	// TriggerExecutionStatusSuccess indicates an execution completed successfully
	TriggerExecutionStatusSuccess TriggerExecutionStatus = "success"

	// TriggerExecutionStatusFailed indicates an execution failed
	TriggerExecutionStatusFailed TriggerExecutionStatus = "failed"
)

// TriggerExecution represents the execution of a trigger
type TriggerExecution struct {
	ID            string                 `json:"id"`
	TriggerID     string                 `json:"trigger_id"`
	ExecutionTime time.Time              `json:"execution_time"`
	CompletedTime time.Time              `json:"completed_time,omitempty"`
	Status        TriggerExecutionStatus `json:"status"`
	Result        string                 `json:"result,omitempty"`
	Error         string                 `json:"error,omitempty"`
	Duration      time.Duration          `json:"duration,omitempty"`
	Metadata      map[string]string      `json:"metadata,omitempty"`
	Success       bool                   `json:"success"`
}

// NewTriggerExecution creates a new trigger execution
func NewTriggerExecution(triggerID string) *TriggerExecution {
	return &TriggerExecution{
		ID:            GenerateID("exec"),
		TriggerID:     triggerID,
		ExecutionTime: time.Now(),
		Status:        TriggerExecutionStatusPending,
		Metadata:      make(map[string]string),
	}
}

// Complete marks the execution as complete with success
func (e *TriggerExecution) Complete(result string) {
	e.CompletedTime = time.Now()
	e.Duration = e.CompletedTime.Sub(e.ExecutionTime)
	e.Status = TriggerExecutionStatusSuccess
	e.Result = result
	e.Success = true
}

// Fail marks the execution as failed with an error
func (e *TriggerExecution) Fail(err error) {
	e.CompletedTime = time.Now()
	e.Duration = e.CompletedTime.Sub(e.ExecutionTime)
	e.Status = TriggerExecutionStatusFailed
	if err != nil {
		e.Error = err.Error()
	}
	e.Success = false
}

// AddMetadata adds metadata to the execution
func (e *TriggerExecution) AddMetadata(key, value string) {
	e.Metadata[key] = value
}

// GenerateID generates a unique ID with a prefix
func GenerateID(prefix string) string {
	nano := time.Now().UnixNano() % 1000000000000000000
	return prefix + "-" + time.Now().Format("20060102150405") + "-" +
		fmt.Sprintf("%05d", nano%100000)
}
