package handlers

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/will/neo_service_layer/internal/services/trigger"
)

// ScheduleType defines types of time-based schedules
type ScheduleType string

const (
	// ScheduleTypeInterval represents an interval-based schedule
	ScheduleTypeInterval ScheduleType = "interval"
	
	// ScheduleTypeCron represents a cron-based schedule
	ScheduleTypeCron ScheduleType = "cron"
	
	// ScheduleTypeOneTime represents a one-time schedule
	ScheduleTypeOneTime ScheduleType = "one_time"
)

// TimeSchedule represents a time-based schedule for triggers
type TimeSchedule struct {
	ID             string       `json:"id"`
	TriggerID      string       `json:"trigger_id"`
	Type           ScheduleType `json:"type"`
	Interval       string       `json:"interval,omitempty"`
	CronExpression string       `json:"cron_expression,omitempty"`
	ExecuteAt      time.Time    `json:"execute_at,omitempty"`
	LastExecution  time.Time    `json:"last_execution,omitempty"`
	NextExecution  time.Time    `json:"next_execution,omitempty"`
	Active         bool         `json:"active"`
	CreatedAt      time.Time    `json:"created_at"`
	UpdatedAt      time.Time    `json:"updated_at"`
}

// TimeTriggerParams represents parameters for a time-based trigger
type TimeTriggerParams struct {
	Type           ScheduleType `json:"type"`
	Interval       string       `json:"interval,omitempty"`
	CronExpression string       `json:"cron_expression,omitempty"`
	ExecuteAt      string       `json:"execute_at,omitempty"`
	Timezone       string       `json:"timezone,omitempty"` // Optional timezone name
}

// TimeTriggerHandler handles time-based triggers
type TimeTriggerHandler struct {
	mu           sync.RWMutex
	schedules    map[string]*TimeSchedule
	triggerSvc   *trigger.Service
	stopChan     chan struct{}
	triggerChans map[string]chan struct{}
}

// NewTimeTriggerHandler creates a new time-based trigger handler
func NewTimeTriggerHandler(triggerSvc *trigger.Service) *TimeTriggerHandler {
	return &TimeTriggerHandler{
		schedules:    make(map[string]*TimeSchedule),
		triggerSvc:   triggerSvc,
		stopChan:     make(chan struct{}),
		triggerChans: make(map[string]chan struct{}),
	}
}

// HandleTrigger processes a time-based trigger
func (h *TimeTriggerHandler) HandleTrigger(ctx context.Context, t *trigger.Trigger) (*trigger.TriggerResult, error) {
	// Parse trigger parameters
	var params TimeTriggerParams
	if err := json.Unmarshal([]byte(t.Parameters), &params); err != nil {
		return nil, fmt.Errorf("failed to parse time trigger parameters: %w", err)
	}
	
	// Validate trigger parameters
	if err := h.validateParams(&params); err != nil {
		return nil, fmt.Errorf("invalid time trigger parameters: %w", err)
	}
	
	// Create schedule
	schedule, err := h.createSchedule(t.ID, &params)
	if err != nil {
		return nil, fmt.Errorf("failed to create schedule: %w", err)
	}
	
	// Add schedule
	h.mu.Lock()
	h.schedules[schedule.ID] = schedule
	h.mu.Unlock()
	
	// Start schedule monitoring in background
	h.startScheduleMonitoring(schedule)
	
	// Return trigger result
	result := &trigger.TriggerResult{
		TriggerID:   t.ID,
		Status:      trigger.TriggerStatusActive,
		Message:     fmt.Sprintf("Time trigger scheduled: %s", formatScheduleDescription(schedule)),
		TriggeredAt: time.Now(),
	}
	
	return result, nil
}

// Start initializes the time trigger handler
func (h *TimeTriggerHandler) Start(ctx context.Context) error {
	// In a real implementation, we might restore schedules from persistent storage
	return nil
}

// Stop stops the time trigger handler
func (h *TimeTriggerHandler) Stop(ctx context.Context) error {
	close(h.stopChan)
	
	// Close all individual trigger channels
	h.mu.Lock()
	for _, ch := range h.triggerChans {
		close(ch)
	}
	h.triggerChans = make(map[string]chan struct{})
	h.mu.Unlock()
	
	return nil
}

// GetName returns the name of the handler
func (h *TimeTriggerHandler) GetName() string {
	return "time"
}

// GetDescription returns the description of the handler
func (h *TimeTriggerHandler) GetDescription() string {
	return "Handles time-based triggers with interval, cron, and one-time schedules"
}

// GetSupportedEvents returns the list of supported event types
func (h *TimeTriggerHandler) GetSupportedEvents() []string {
	return []string{
		string(ScheduleTypeInterval),
		string(ScheduleTypeCron),
		string(ScheduleTypeOneTime),
	}
}

// Initialize registers this handler with the trigger service
func (h *TimeTriggerHandler) Initialize(ctx context.Context) error {
	// Register with trigger service
	if err := h.triggerSvc.RegisterHandler(h.GetName(), h); err != nil {
		return fmt.Errorf("failed to register time trigger handler: %w", err)
	}
	return nil
}

// Validate validates trigger parameters
func (h *TimeTriggerHandler) Validate(params string) error {
	var p TimeTriggerParams
	if err := json.Unmarshal([]byte(params), &p); err != nil {
		return fmt.Errorf("invalid time trigger parameters: %w", err)
	}
	
	return h.validateParams(&p)
}

// validateParams validates time trigger parameters
func (h *TimeTriggerHandler) validateParams(params *TimeTriggerParams) error {
	switch params.Type {
	case ScheduleTypeInterval:
		if params.Interval == "" {
			return fmt.Errorf("interval must be specified for interval schedule")
		}
		
		// Validate that the interval is a valid duration
		_, err := time.ParseDuration(params.Interval)
		if err != nil {
			return fmt.Errorf("invalid interval format: %w", err)
		}
	case ScheduleTypeCron:
		if params.CronExpression == "" {
			return fmt.Errorf("cron expression must be specified for cron schedule")
		}
		
		// In a real implementation, validate the cron expression syntax
	case ScheduleTypeOneTime:
		if params.ExecuteAt == "" {
			return fmt.Errorf("execute at time must be specified for one-time schedule")
		}
		
		// Validate that the execute at time is a valid timestamp
		_, err := time.Parse(time.RFC3339, params.ExecuteAt)
		if err != nil {
			return fmt.Errorf("invalid execute at time format: %w", err)
		}
	default:
		return fmt.Errorf("unsupported schedule type: %s", params.Type)
	}
	
	return nil
}

// createSchedule creates a new time schedule from trigger parameters
func (h *TimeTriggerHandler) createSchedule(triggerID string, params *TimeTriggerParams) (*TimeSchedule, error) {
	schedule := &TimeSchedule{
		ID:        uuid.New().String(),
		TriggerID: triggerID,
		Type:      params.Type,
		Active:    true,
		CreatedAt: time.Now(),
		UpdatedAt: time.Now(),
	}
	
	// Set schedule-specific parameters
	switch params.Type {
	case ScheduleTypeInterval:
		schedule.Interval = params.Interval
		
		// Calculate next execution time
		interval, _ := time.ParseDuration(params.Interval)
		schedule.NextExecution = time.Now().Add(interval)
	case ScheduleTypeCron:
		schedule.CronExpression = params.CronExpression
		
		// In a real implementation, calculate next execution time based on cron expression
		// For now, set it to 1 minute from now
		schedule.NextExecution = time.Now().Add(1 * time.Minute)
	case ScheduleTypeOneTime:
		executeAt, _ := time.Parse(time.RFC3339, params.ExecuteAt)
		schedule.ExecuteAt = executeAt
		schedule.NextExecution = executeAt
	}
	
	return schedule, nil
}

// startScheduleMonitoring starts monitoring for a schedule
func (h *TimeTriggerHandler) startScheduleMonitoring(schedule *TimeSchedule) {
	// Create a channel for this specific schedule
	h.mu.Lock()
	stopCh := make(chan struct{})
	h.triggerChans[schedule.ID] = stopCh
	h.mu.Unlock()
	
	go h.monitorSchedule(schedule, stopCh)
}

// monitorSchedule monitors a schedule for execution
func (h *TimeTriggerHandler) monitorSchedule(schedule *TimeSchedule, stopCh chan struct{}) {
	// Calculate initial wait time
	waitTime := schedule.NextExecution.Sub(time.Now())
	if waitTime < 0 {
		waitTime = 0
	}
	
	// Create ticker for this schedule
	ticker := time.NewTicker(waitTime)
	defer ticker.Stop()
	
	for {
		select {
		case <-stopCh:
			// Schedule has been stopped
			return
		case <-h.stopChan:
			// Handler is stopping
			return
		case <-ticker.C:
			// Time to check if the schedule should be executed
			if h.shouldExecute(schedule) {
				// Execute the schedule
				h.executeSchedule(schedule)
				
				// Calculate next execution time
				h.calculateNextExecution(schedule)
				
				// Update the schedule
				h.mu.Lock()
				h.schedules[schedule.ID] = schedule
				h.mu.Unlock()
				
				// Check if the schedule should continue
				if !h.shouldContinue(schedule) {
					// Remove the schedule
					h.mu.Lock()
					delete(h.schedules, schedule.ID)
					h.mu.Unlock()
					return
				}
				
				// Reset the ticker for the next execution
				waitTime := schedule.NextExecution.Sub(time.Now())
				if waitTime < 0 {
					waitTime = 0
				}
				ticker.Reset(waitTime)
			} else {
				// Wait a short amount of time and check again
				ticker.Reset(1 * time.Second)
			}
		}
	}
}

// shouldExecute checks if a schedule should be executed
func (h *TimeTriggerHandler) shouldExecute(schedule *TimeSchedule) bool {
	// Check if the schedule is active
	if !schedule.Active {
		return false
	}
	
	// Check if it's time to execute
	return time.Now().After(schedule.NextExecution) || time.Now().Equal(schedule.NextExecution)
}

// executeSchedule executes a schedule
func (h *TimeTriggerHandler) executeSchedule(schedule *TimeSchedule) {
	// Set last execution time
	schedule.LastExecution = time.Now()
	
	// Prepare trigger result
	result := &trigger.TriggerResult{
		TriggerID:   schedule.TriggerID,
		Status:      trigger.TriggerStatusTriggered,
		Message:     fmt.Sprintf("Time trigger executed at %s", schedule.LastExecution.Format(time.RFC3339)),
		TriggeredAt: schedule.LastExecution,
	}
	
	// Notify trigger service
	ctx := context.Background()
	if err := h.triggerSvc.NotifyTriggerResult(ctx, result); err != nil {
		log.Printf("Failed to notify trigger service: %v", err)
	}
}

// calculateNextExecution calculates the next execution time for a schedule
func (h *TimeTriggerHandler) calculateNextExecution(schedule *TimeSchedule) {
	switch schedule.Type {
	case ScheduleTypeInterval:
		interval, _ := time.ParseDuration(schedule.Interval)
		schedule.NextExecution = schedule.LastExecution.Add(interval)
	case ScheduleTypeCron:
		// In a real implementation, calculate next execution time based on cron expression
		// For now, set it to 1 minute from now
		schedule.NextExecution = schedule.LastExecution.Add(1 * time.Minute)
	case ScheduleTypeOneTime:
		// One-time schedules don't have a next execution time
		schedule.NextExecution = time.Time{}
	}
}

// shouldContinue checks if a schedule should continue
func (h *TimeTriggerHandler) shouldContinue(schedule *TimeSchedule) bool {
	// One-time schedules should not continue after execution
	if schedule.Type == ScheduleTypeOneTime {
		return false
	}
	
	// All other schedule types should continue
	return true
}

// formatScheduleDescription formats a human-readable description of a schedule
func formatScheduleDescription(schedule *TimeSchedule) string {
	switch schedule.Type {
	case ScheduleTypeInterval:
		return fmt.Sprintf("Interval: %s", schedule.Interval)
	case ScheduleTypeCron:
		return fmt.Sprintf("Cron: %s", schedule.CronExpression)
	case ScheduleTypeOneTime:
		return fmt.Sprintf("One-time: %s", schedule.ExecuteAt.Format(time.RFC3339))
	default:
		return fmt.Sprintf("Unknown schedule type: %s", schedule.Type)
	}
}