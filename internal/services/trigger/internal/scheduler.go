package internal

import (
	"context"
	"fmt"
	"sync"
	"time"

	"github.com/robfig/cron/v3"
	"github.com/will/neo_service_layer/internal/services/trigger/models"
)

type TriggerSchedulerImpl struct {
	mu       sync.RWMutex
	cron     *cron.Cron
	triggers map[string]cron.EntryID
}

func NewTriggerScheduler() *TriggerSchedulerImpl {
	return &TriggerSchedulerImpl{
		cron:     cron.New(cron.WithSeconds()),
		triggers: make(map[string]cron.EntryID),
	}
}

func (ts *TriggerSchedulerImpl) Start(ctx context.Context) error {
	ts.cron.Start()
	return nil
}

func (ts *TriggerSchedulerImpl) Stop(ctx context.Context) error {
	ts.cron.Stop()
	return nil
}

func (ts *TriggerSchedulerImpl) ScheduleTrigger(ctx context.Context, trigger *models.Trigger) error {
	ts.mu.Lock()
	defer ts.mu.Unlock()

	entryID, err := ts.cron.AddFunc(trigger.Schedule, func() {})
	if err != nil {
		return fmt.Errorf("failed to add trigger to cron: %w", err)
	}

	ts.triggers[trigger.ID] = entryID
	return nil
}

func (ts *TriggerSchedulerImpl) UnscheduleTrigger(ctx context.Context, triggerID string) error {
	ts.mu.Lock()
	defer ts.mu.Unlock()

	if entryID, exists := ts.triggers[triggerID]; exists {
		ts.cron.Remove(entryID)
		delete(ts.triggers, triggerID)
	}
	return nil
}

func (ts *TriggerSchedulerImpl) GetNextExecutionTime(ctx context.Context, schedule string) (time.Time, error) {
	parser := cron.NewParser(cron.Second | cron.Minute | cron.Hour | cron.Dom | cron.Month | cron.Dow)
	cronSchedule, err := parser.Parse(schedule)
	if err != nil {
		return time.Time{}, fmt.Errorf("invalid schedule: %w", err)
	}

	return cronSchedule.Next(time.Now()), nil
}

func (ts *TriggerSchedulerImpl) AddTrigger(trigger *models.Trigger) error {
	ts.mu.Lock()
	defer ts.mu.Unlock()

	entryID, err := ts.cron.AddFunc(trigger.Schedule, func() {})
	if err != nil {
		return fmt.Errorf("failed to add trigger to cron: %w", err)
	}

	ts.triggers[trigger.ID] = entryID
	return nil
}

func (ts *TriggerSchedulerImpl) RemoveTrigger(triggerID string) error {
	ts.mu.Lock()
	defer ts.mu.Unlock()

	if entryID, exists := ts.triggers[triggerID]; exists {
		ts.cron.Remove(entryID)
		delete(ts.triggers, triggerID)
	}
	return nil
}

func (ts *TriggerSchedulerImpl) IsDue(triggerID string) (bool, error) {
	ts.mu.RLock()
	defer ts.mu.RUnlock()

	entryID, exists := ts.triggers[triggerID]
	if !exists {
		return false, fmt.Errorf("trigger not found: %s", triggerID)
	}

	entry := ts.cron.Entry(entryID)
	nextTime := entry.Next
	return time.Now().After(nextTime) || time.Now().Equal(nextTime), nil
}
