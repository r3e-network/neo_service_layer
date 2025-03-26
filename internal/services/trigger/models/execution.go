package models

import (
	"time"
)

// Execution represents a trigger execution
type Execution struct {
	ID        string    `json:"id"`
	TriggerID string    `json:"trigger_id"`
	Status    string    `json:"status"`
	Result    string    `json:"result"`
	Error     string    `json:"error,omitempty"`
	StartTime time.Time `json:"start_time"`
	EndTime   time.Time `json:"end_time"`
}
