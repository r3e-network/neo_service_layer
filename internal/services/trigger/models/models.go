package models

import (
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// Trigger represents a trigger configuration
type Trigger struct {
	ID            string
	UserAddress   util.Uint160
	Name          string
	Description   string
	Condition     string
	Function      string
	Parameters    map[string]interface{}
	Schedule      string
	Status        string
	CreatedAt     time.Time
	UpdatedAt     time.Time
	LastExecuted  time.Time
	NextExecution time.Time
}

// TriggerExecution represents a trigger execution record
type TriggerExecution struct {
	ID          string
	TriggerID   string
	UserAddress util.Uint160
	StartTime   time.Time
	EndTime     time.Time
	Status      string
	Result      string
	Error       string
	GasUsed     int64
}

// TriggerPolicy represents the policy for triggers
type TriggerPolicy struct {
	MaxTriggersPerUser     int
	MaxExecutionsPerTrigger int
	ExecutionWindow        time.Duration
	MinInterval            time.Duration
	MaxInterval            time.Duration
	CooldownPeriod         time.Duration
}

// TriggerMetrics represents metrics for trigger executions
type TriggerMetrics struct {
	TotalExecutions    int
	SuccessfulExecutions int
	FailedExecutions   int
	AverageGasUsed    int64
	AverageLatency    time.Duration
}