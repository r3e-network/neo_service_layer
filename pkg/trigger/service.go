package trigger

import (
	"context"
	"fmt"
	"sync"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/pkg/errors"
	"go.uber.org/zap"
)

// NewService creates a new trigger service
func NewService(config *Config, store Store, scheduler Scheduler, executor Executor, metrics MetricsCollector, evaluator Evaluator, logger *zap.Logger) (*Service, error) {
	if err := config.Validate(); err != nil {
		return nil, errors.Wrap(err, "invalid configuration")
	}

	return &Service{
		config:    config,
		triggers:  make(map[string]*Trigger),
		eventCh:   make(chan *Event, config.MaxEventChannelSize),
		stopCh:    make(chan struct{}),
		store:     store,
		scheduler: scheduler,
		executor:  executor,
		metrics:   metrics,
		logger:    logger,
		evaluator: evaluator,
		mu:        sync.RWMutex{},
	}, nil
}

// Start starts the trigger service
func (s *Service) Start(ctx context.Context) error {
	s.logger.Info("Starting trigger service")

	// Load existing triggers
	if err := s.loadTriggers(); err != nil {
		return errors.Wrap(err, "failed to load triggers")
	}

	// Start scheduler
	if err := s.scheduler.Start(); err != nil {
		return errors.Wrap(err, "failed to start scheduler")
	}

	// Start event processing
	go s.processEvents(ctx)

	// Start event polling
	go s.pollEvents(ctx)

	return nil
}

// Stop stops the trigger service
func (s *Service) Stop() error {
	s.logger.Info("Stopping trigger service")

	// Signal all goroutines to stop
	close(s.stopCh)

	// Stop scheduler
	if err := s.scheduler.Stop(); err != nil {
		return errors.Wrap(err, "failed to stop scheduler")
	}

	return nil
}

// loadTriggers loads existing triggers from storage
func (s *Service) loadTriggers() error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Clear existing triggers
	s.triggers = make(map[string]*Trigger)

	// List all triggers
	triggers, err := s.store.ListTriggers(util.Uint160{})
	if err != nil {
		return errors.Wrap(err, "failed to list triggers")
	}

	// Add triggers to map and scheduler
	for _, trigger := range triggers {
		s.triggers[trigger.ID] = trigger

		// Add active triggers to scheduler
		if trigger.Status == TriggerStatusActive && trigger.Type == TriggerTypeSchedule {
			if err := s.scheduler.AddTrigger(trigger); err != nil {
				s.logger.Error("Failed to add trigger to scheduler",
					zap.Error(err),
					zap.String("trigger_id", trigger.ID))
			}
		}
	}

	s.logger.Info("Loaded triggers",
		zap.Int("count", len(s.triggers)))

	return nil
}

// CreateTrigger creates a new trigger
func (s *Service) CreateTrigger(trigger *Trigger) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Validate trigger
	if err := s.validateTrigger(trigger); err != nil {
		return errors.Wrap(err, "invalid trigger")
	}

	// Save trigger
	if err := s.store.SaveTrigger(trigger); err != nil {
		return errors.Wrap(err, "failed to save trigger")
	}

	// Add to map
	s.triggers[trigger.ID] = trigger

	// Add to scheduler if schedule trigger
	if trigger.Status == TriggerStatusActive && trigger.Type == TriggerTypeSchedule {
		if err := s.scheduler.AddTrigger(trigger); err != nil {
			return errors.Wrap(err, "failed to add trigger to scheduler")
		}
	}

	s.metrics.RecordTriggerCreated(trigger)
	s.logger.Info("Created trigger",
		zap.String("trigger_id", trigger.ID),
		zap.String("type", string(trigger.Type)))

	return nil
}

// UpdateTrigger updates an existing trigger
func (s *Service) UpdateTrigger(trigger *Trigger) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Check if trigger exists
	existing, exists := s.triggers[trigger.ID]
	if !exists {
		return fmt.Errorf("trigger not found: %s", trigger.ID)
	}

	// Validate trigger
	if err := s.validateTrigger(trigger); err != nil {
		return errors.Wrap(err, "invalid trigger")
	}

	// Remove from scheduler if schedule trigger
	if existing.Type == TriggerTypeSchedule {
		if err := s.scheduler.RemoveTrigger(existing.ID); err != nil {
			s.logger.Error("Failed to remove trigger from scheduler",
				zap.Error(err),
				zap.String("trigger_id", existing.ID))
		}
	}

	// Save trigger
	if err := s.store.SaveTrigger(trigger); err != nil {
		return errors.Wrap(err, "failed to save trigger")
	}

	// Update map
	s.triggers[trigger.ID] = trigger

	// Add to scheduler if schedule trigger
	if trigger.Status == TriggerStatusActive && trigger.Type == TriggerTypeSchedule {
		if err := s.scheduler.AddTrigger(trigger); err != nil {
			return errors.Wrap(err, "failed to add trigger to scheduler")
		}
	}

	s.logger.Info("Updated trigger",
		zap.String("trigger_id", trigger.ID),
		zap.String("type", string(trigger.Type)))

	return nil
}

// DeleteTrigger deletes a trigger
func (s *Service) DeleteTrigger(id string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Check if trigger exists
	trigger, exists := s.triggers[id]
	if !exists {
		return fmt.Errorf("trigger not found: %s", id)
	}

	// Remove from scheduler if schedule trigger
	if trigger.Type == TriggerTypeSchedule {
		if err := s.scheduler.RemoveTrigger(trigger.ID); err != nil {
			s.logger.Error("Failed to remove trigger from scheduler",
				zap.Error(err),
				zap.String("trigger_id", trigger.ID))
		}
	}

	// Delete from storage
	if err := s.store.DeleteTrigger(id); err != nil {
		return errors.Wrap(err, "failed to delete trigger")
	}

	// Remove from map
	delete(s.triggers, id)

	s.metrics.RecordTriggerDeleted(trigger)
	s.logger.Info("Deleted trigger",
		zap.String("trigger_id", id))

	return nil
}

// GetTrigger gets a trigger by ID
func (s *Service) GetTrigger(id string) (*Trigger, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	trigger, exists := s.triggers[id]
	if !exists {
		return nil, fmt.Errorf("trigger not found: %s", id)
	}

	return trigger, nil
}

// ListTriggers lists all triggers for an owner
func (s *Service) ListTriggers(owner util.Uint160) ([]*Trigger, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	var triggers []*Trigger
	for _, trigger := range s.triggers {
		if owner.Equals(trigger.Owner) {
			triggers = append(triggers, trigger)
		}
	}

	return triggers, nil
}

// validateTrigger validates a trigger
func (s *Service) validateTrigger(trigger *Trigger) error {
	if trigger.ID == "" {
		return errors.New("trigger ID is required")
	}

	if trigger.Type != TriggerTypeEvent && trigger.Type != TriggerTypeSchedule {
		return fmt.Errorf("invalid trigger type: %s", trigger.Type)
	}

	if trigger.Type == TriggerTypeEvent {
		if trigger.EventName == "" {
			return errors.New("event name is required for event trigger")
		}
	}

	if trigger.Type == TriggerTypeSchedule {
		if trigger.Schedule == "" {
			return errors.New("schedule is required for schedule trigger")
		}
		// TODO: Validate CRON expression
	}

	if trigger.Action == "" {
		return errors.New("action is required")
	}

	return nil
}
