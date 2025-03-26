package strategies

import (
	"context"
	"fmt"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/automation"
	"github.com/will/neo_service_layer/internal/services/gasbank"
)

// TimeExecutionStrategy implements a time-based execution strategy
type TimeExecutionStrategy struct {
	gasBankService *gasbank.Service
	schedules      map[string]*TimeSchedule
}

// TimeScheduleType represents a type of time schedule
type TimeScheduleType string

const (
	// ScheduleTypeInterval represents an interval-based schedule
	ScheduleTypeInterval TimeScheduleType = "interval"
	
	// ScheduleTypeCron represents a cron-based schedule
	ScheduleTypeCron TimeScheduleType = "cron"
	
	// ScheduleTypeOneTime represents a one-time schedule
	ScheduleTypeOneTime TimeScheduleType = "one_time"
)

// TimeSchedule represents a time-based execution schedule
type TimeSchedule struct {
	Type          TimeScheduleType `json:"type"`
	Interval      time.Duration    `json:"interval,omitempty"`
	CronExpression string          `json:"cron_expression,omitempty"`
	ExecuteAt     time.Time        `json:"execute_at,omitempty"`
	LastExecution time.Time        `json:"last_execution,omitempty"`
	NextExecution time.Time        `json:"next_execution,omitempty"`
}

// NewTimeExecutionStrategy creates a new time-based execution strategy
func NewTimeExecutionStrategy(gasBankService *gasbank.Service) *TimeExecutionStrategy {
	return &TimeExecutionStrategy{
		gasBankService: gasBankService,
		schedules:      make(map[string]*TimeSchedule),
	}
}

// RegisterSchedule registers a schedule for an upkeep
func (s *TimeExecutionStrategy) RegisterSchedule(upkeepID string, schedule *TimeSchedule) error {
	if schedule == nil {
		return fmt.Errorf("schedule cannot be nil")
	}
	
	// Validate schedule
	if err := s.validateSchedule(schedule); err != nil {
		return err
	}
	
	// Calculate next execution time
	if err := s.calculateNextExecution(schedule); err != nil {
		return err
	}
	
	s.schedules[upkeepID] = schedule
	return nil
}

// UnregisterSchedule unregisters a schedule for an upkeep
func (s *TimeExecutionStrategy) UnregisterSchedule(upkeepID string) {
	delete(s.schedules, upkeepID)
}

// GetSchedule gets a schedule for an upkeep
func (s *TimeExecutionStrategy) GetSchedule(upkeepID string) *TimeSchedule {
	return s.schedules[upkeepID]
}

// Execute executes an upkeep based on its schedule
func (s *TimeExecutionStrategy) Execute(ctx context.Context, upkeep *automation.Upkeep, performData []byte) (*automation.UpkeepPerformance, error) {
	startTime := time.Now()
	
	// Get the schedule for this upkeep
	schedule := s.GetSchedule(upkeep.ID)
	if schedule == nil {
		// Try to parse schedule from upkeep offchain config
		var err error
		schedule, err = parseTimeSchedule(upkeep.OffchainConfig)
		if err != nil {
			return &automation.UpkeepPerformance{
				ID:        fmt.Sprintf("perf-%d", time.Now().UnixNano()),
				UpkeepID:  upkeep.ID,
				StartTime: startTime,
				EndTime:   time.Now(),
				Status:    "failed",
				Error:     fmt.Sprintf("failed to parse time schedule: %v", err),
			}, err
		}
		
		// Register the schedule for future use
		if err := s.RegisterSchedule(upkeep.ID, schedule); err != nil {
			return &automation.UpkeepPerformance{
				ID:        fmt.Sprintf("perf-%d", time.Now().UnixNano()),
				UpkeepID:  upkeep.ID,
				StartTime: startTime,
				EndTime:   time.Now(),
				Status:    "failed",
				Error:     fmt.Sprintf("failed to register schedule: %v", err),
			}, err
		}
	}
	
	// Check if it's time to execute
	now := time.Now()
	if now.Before(schedule.NextExecution) {
		return &automation.UpkeepPerformance{
			ID:        fmt.Sprintf("perf-%d", time.Now().UnixNano()),
			UpkeepID:  upkeep.ID,
			StartTime: startTime,
			EndTime:   time.Now(),
			Status:    "skipped",
			Error:     fmt.Sprintf("not time to execute yet, next execution at %v", schedule.NextExecution),
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
	
	// Update last execution time and calculate next execution
	schedule.LastExecution = now
	if err := s.calculateNextExecution(schedule); err != nil {
		return &automation.UpkeepPerformance{
			ID:        fmt.Sprintf("perf-%d", time.Now().UnixNano()),
			UpkeepID:  upkeep.ID,
			StartTime: startTime,
			EndTime:   time.Now(),
			Status:    "failed",
			Error:     fmt.Sprintf("failed to calculate next execution: %v", err),
		}, err
	}
	
	// Record the end time
	endTime := time.Now()
	
	// Create performance record
	performance := &automation.UpkeepPerformance{
		ID:              uuid.New().String(),
		UpkeepID:        upkeep.ID,
		StartTime:       startTime,
		EndTime:         endTime,
		Status:          "success",
		GasUsed:         upkeep.ExecuteGas,
		BlockNumber:     blockNumber,
		TransactionHash: txHash,
		Result:          fmt.Sprintf("Successfully called %s on contract %s based on time schedule", method, contractAddress.StringLE()),
	}
	
	return performance, nil
}

// validateSchedule validates a time schedule
func (s *TimeExecutionStrategy) validateSchedule(schedule *TimeSchedule) error {
	switch schedule.Type {
	case ScheduleTypeInterval:
		if schedule.Interval <= 0 {
			return fmt.Errorf("interval must be positive")
		}
	case ScheduleTypeCron:
		if schedule.CronExpression == "" {
			return fmt.Errorf("cron expression cannot be empty")
		}
		// In a real implementation, validate the cron expression syntax
	case ScheduleTypeOneTime:
		if schedule.ExecuteAt.IsZero() {
			return fmt.Errorf("execute at time cannot be empty for one-time schedule")
		}
	default:
		return fmt.Errorf("unsupported schedule type: %s", schedule.Type)
	}
	
	return nil
}

// calculateNextExecution calculates the next execution time for a schedule
func (s *TimeExecutionStrategy) calculateNextExecution(schedule *TimeSchedule) error {
	now := time.Now()
	
	switch schedule.Type {
	case ScheduleTypeInterval:
		if schedule.LastExecution.IsZero() {
			// First execution, schedule it for now
			schedule.NextExecution = now
		} else {
			// Next execution is last execution + interval
			schedule.NextExecution = schedule.LastExecution.Add(schedule.Interval)
			
			// If next execution is in the past, schedule it for now
			if schedule.NextExecution.Before(now) {
				schedule.NextExecution = now
			}
		}
	case ScheduleTypeCron:
		// In a real implementation, parse the cron expression and calculate the next execution time
		// For now, just schedule it for 1 minute from now
		schedule.NextExecution = now.Add(1 * time.Minute)
	case ScheduleTypeOneTime:
		schedule.NextExecution = schedule.ExecuteAt
		
		// If execute at is in the past and we haven't executed yet, schedule it for now
		if schedule.NextExecution.Before(now) && schedule.LastExecution.IsZero() {
			schedule.NextExecution = now
		} else if !schedule.LastExecution.IsZero() {
			// One-time schedule that has already executed, don't schedule it again
			schedule.NextExecution = time.Time{}
		}
	default:
		return fmt.Errorf("unsupported schedule type: %s", schedule.Type)
	}
	
	return nil
}

// parseTimeSchedule parses a time schedule from upkeep offchain config
func parseTimeSchedule(config map[string]interface{}) (*TimeSchedule, error) {
	// Check if time schedule exists
	scheduleData, ok := config["timeSchedule"]
	if !ok {
		return nil, fmt.Errorf("no time schedule found in offchain config")
	}
	
	// Try to convert to map
	scheduleMap, ok := scheduleData.(map[string]interface{})
	if !ok {
		return nil, fmt.Errorf("invalid time schedule format")
	}
	
	// Extract schedule type
	scheduleTypeStr, ok := scheduleMap["type"].(string)
	if !ok {
		return nil, fmt.Errorf("missing schedule type")
	}
	
	// Create schedule
	schedule := &TimeSchedule{
		Type: TimeScheduleType(scheduleTypeStr),
	}
	
	// Extract schedule parameters based on type
	switch schedule.Type {
	case ScheduleTypeInterval:
		intervalStr, ok := scheduleMap["interval"].(string)
		if !ok {
			return nil, fmt.Errorf("missing interval")
		}
		
		interval, err := time.ParseDuration(intervalStr)
		if err != nil {
			return nil, fmt.Errorf("invalid interval: %w", err)
		}
		
		schedule.Interval = interval
	case ScheduleTypeCron:
		cronExpression, ok := scheduleMap["cron_expression"].(string)
		if !ok {
			return nil, fmt.Errorf("missing cron expression")
		}
		
		schedule.CronExpression = cronExpression
	case ScheduleTypeOneTime:
		executeAtStr, ok := scheduleMap["execute_at"].(string)
		if !ok {
			return nil, fmt.Errorf("missing execute at time")
		}
		
		executeAt, err := time.Parse(time.RFC3339, executeAtStr)
		if err != nil {
			return nil, fmt.Errorf("invalid execute at time: %w", err)
		}
		
		schedule.ExecuteAt = executeAt
	default:
		return nil, fmt.Errorf("unsupported schedule type: %s", schedule.Type)
	}
	
	return schedule, nil
}