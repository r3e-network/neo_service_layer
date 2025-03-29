package trigger

import (
	"context"
	"fmt"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/pkg/errors"
	"github.com/robfig/cron/v3"
	"go.uber.org/zap"
)

// DefaultScheduler implements the Scheduler interface
type DefaultScheduler struct {
	cron     *cron.Cron
	entries  map[string]cron.EntryID
	service  *Service
	logger   *zap.Logger
	mu       sync.RWMutex
	stopCh   chan struct{}
	stopOnce sync.Once
}

// NewScheduler creates a new scheduler
func NewScheduler(service *Service, logger *zap.Logger) *DefaultScheduler {
	cronOptions := cron.WithParser(cron.NewParser(
		cron.SecondOptional | cron.Minute | cron.Hour | cron.Dom | cron.Month | cron.Dow | cron.Descriptor,
	))

	return &DefaultScheduler{
		cron:    cron.New(cronOptions),
		entries: make(map[string]cron.EntryID),
		service: service,
		logger:  logger,
		stopCh:  make(chan struct{}),
	}
}

// Start starts the scheduler
func (s *DefaultScheduler) Start() error {
	s.logger.Info("Starting scheduler")
	s.cron.Start()
	return nil
}

// Stop stops the scheduler
func (s *DefaultScheduler) Stop() error {
	s.stopOnce.Do(func() {
		s.logger.Info("Stopping scheduler")
		close(s.stopCh)
		ctx := s.cron.Stop()
		<-ctx.Done()
	})
	return nil
}

// AddTrigger adds a trigger to the scheduler
func (s *DefaultScheduler) AddTrigger(trigger *Trigger) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Skip non-schedule triggers
	if trigger.Type != TriggerTypeSchedule {
		return nil
	}

	// Remove existing entry if any
	if entryID, exists := s.entries[trigger.ID]; exists {
		s.cron.Remove(entryID)
		delete(s.entries, trigger.ID)
	}

	// Create job function
	job := func() {
		if err := s.executeTrigger(trigger); err != nil {
			s.logger.Error("Failed to execute scheduled trigger",
				zap.Error(err),
				zap.String("trigger_id", trigger.ID))
		}
	}

	// Add job to cron
	entryID, err := s.cron.AddFunc(trigger.Schedule, job)
	if err != nil {
		return errors.Wrap(err, "failed to add cron job")
	}

	// Store entry ID
	s.entries[trigger.ID] = entryID

	s.logger.Info("Added trigger to scheduler",
		zap.String("trigger_id", trigger.ID),
		zap.String("schedule", trigger.Schedule))

	return nil
}

// RemoveTrigger removes a trigger from the scheduler
func (s *DefaultScheduler) RemoveTrigger(id string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Remove entry if exists
	if entryID, exists := s.entries[id]; exists {
		s.cron.Remove(entryID)
		delete(s.entries, id)

		s.logger.Info("Removed trigger from scheduler",
			zap.String("trigger_id", id))
	}

	return nil
}

// executeTrigger executes a scheduled trigger
func (s *DefaultScheduler) executeTrigger(trigger *Trigger) error {
	// Create execution record
	execution := &TriggerExecution{
		ID:        fmt.Sprintf("exec-%d", time.Now().UnixNano()),
		TriggerID: trigger.ID,
		Status:    ExecutionStatusPending,
		StartTime: time.Now(),
	}

	// Create a context with timeout
	ctx, cancel := context.WithTimeout(context.Background(), s.service.config.ExecutionTimeout)
	defer cancel()

	// Create an event for the scheduled trigger
	event := &Event{
		ID:           uuid.New().String(),
		ContractHash: trigger.ContractHash,
		Name:         "scheduled",
		Data:         make(map[string]interface{}),
		Timestamp:    time.Now(),
	}

	// Execute the trigger
	if _, err := s.service.executor.Execute(ctx, trigger, event); err != nil {
		execution.Status = ExecutionStatusFailed
		execution.Error = err.Error()
		return fmt.Errorf("failed to execute scheduled trigger: %w", err)
	}

	// Update execution record
	execution.Status = ExecutionStatusCompleted
	execution.Result = event.Data
	execution.EndTime = time.Now()
	execution.Duration = execution.EndTime.Sub(execution.StartTime).Milliseconds()

	// Save execution
	if err := s.service.store.SaveExecution(execution); err != nil {
		s.logger.Error("Failed to save execution",
			zap.Error(err),
			zap.String("execution_id", execution.ID))
	}

	// Update trigger metadata
	trigger.LastExecuted = time.Now()
	trigger.ExecutionCount++
	if err := s.service.store.SaveTrigger(trigger); err != nil {
		s.logger.Error("Failed to update trigger",
			zap.Error(err),
			zap.String("trigger_id", trigger.ID))
	}

	s.service.metrics.RecordExecution(execution)

	return nil
}
