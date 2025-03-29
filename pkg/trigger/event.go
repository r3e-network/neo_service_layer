package trigger

import (
	"context"
	"fmt"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/joeqian10/neo3-gogogo/rpc"
	"github.com/nspcc-dev/neo-go/pkg/rpcclient"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/nspcc-dev/neo-go/pkg/vm/stackitem"
	"github.com/pkg/errors"
	"go.uber.org/zap"
)

// EventPoller polls for blockchain events
type EventPoller struct {
	client       *rpc.RpcClient
	logger       *zap.Logger
	eventChan    chan *Event
	stopChan     chan struct{}
	contractHash string
	rpcEndpoint  string
	lastBlock    uint32
}

// NewEventPoller creates a new event poller
func NewEventPoller(client *rpc.RpcClient, logger *zap.Logger) *EventPoller {
	return &EventPoller{
		client:    client,
		logger:    logger,
		eventChan: make(chan *Event),
		stopChan:  make(chan struct{}),
	}
}

// Start starts polling for events
func (p *EventPoller) Start(ctx context.Context, contractHash string, rpcEndpoint string) error {
	p.contractHash = contractHash
	p.rpcEndpoint = rpcEndpoint
	go p.pollEvents(ctx)
	return nil
}

// Stop stops polling for events
func (p *EventPoller) Stop() {
	close(p.stopChan)
}

// Events returns the event channel
func (p *EventPoller) Events() <-chan *Event {
	return p.eventChan
}

func (ep *EventPoller) pollEvents(ctx context.Context) error {
	client, err := rpcclient.New(ctx, ep.rpcEndpoint, rpcclient.Options{
		DialTimeout:    30 * time.Second,
		RequestTimeout: 30 * time.Second,
	})
	if err != nil {
		return fmt.Errorf("failed to create RPC client: %w", err)
	}
	defer client.Close()

	hash, err := client.GetBlockHash(ep.lastBlock)
	if err != nil {
		return fmt.Errorf("failed to get block hash: %w", err)
	}

	appLog, err := client.GetApplicationLog(hash, nil)
	if err != nil {
		return fmt.Errorf("failed to get application log: %w", err)
	}

	ep.logger.Info("fetched notifications", zap.Int("count", len(appLog.Executions)))

	for _, execution := range appLog.Executions {
		for _, notification := range execution.Events {
			data, err := parseStackItem(notification.Item)
			if err != nil {
				ep.logger.Error("failed to parse notification data", zap.Error(err))
				continue
			}

			event := &Event{
				ID:           uuid.New().String(),
				ContractHash: notification.ScriptHash,
				Name:         notification.Name,
				Data:         data,
				Timestamp:    time.Now(),
			}

			select {
			case <-ctx.Done():
				return ctx.Err()
			case ep.eventChan <- event:
			}
		}
	}

	ep.lastBlock++
	return nil
}

func parseStackItem(item stackitem.Item) (map[string]interface{}, error) {
	result := make(map[string]interface{})

	array, ok := item.(*stackitem.Array)
	if !ok {
		return nil, fmt.Errorf("expected Array, got %T", item)
	}

	value := array.Value().([]stackitem.Item)
	if len(value)%2 != 0 {
		return nil, fmt.Errorf("invalid array length: %d", len(value))
	}

	for i := 0; i < len(value); i += 2 {
		key, err := value[i].TryBytes()
		if err != nil {
			return nil, fmt.Errorf("failed to convert key to string: %w", err)
		}
		keyStr := string(key)

		val := value[i+1]
		var valInterface interface{}

		switch v := val.(type) {
		case *stackitem.Array:
			arr := make([]interface{}, 0)
			for _, item := range v.Value().([]stackitem.Item) {
				bytes, err := item.TryBytes()
				if err != nil {
					return nil, fmt.Errorf("failed to convert array item to bytes: %w", err)
				}
				arr = append(arr, string(bytes))
			}
			valInterface = arr
		case *stackitem.ByteArray:
			bytes, err := v.TryBytes()
			if err != nil {
				return nil, fmt.Errorf("failed to convert value to bytes: %w", err)
			}
			valInterface = string(bytes)
		case *stackitem.BigInteger:
			num, err := v.TryInteger()
			if err != nil {
				return nil, fmt.Errorf("failed to convert value to integer: %w", err)
			}
			valInterface = num
		case *stackitem.Bool:
			b, err := v.TryBool()
			if err != nil {
				return nil, fmt.Errorf("failed to convert value to bool: %w", err)
			}
			valInterface = b
		default:
			return nil, fmt.Errorf("unsupported stack item type: %T", v)
		}

		result[keyStr] = valInterface
	}

	return result, nil
}

// pollEvents polls for new events from the blockchain
func (s *Service) pollEvents(ctx context.Context) {
	ticker := time.NewTicker(s.config.EventPollingInterval)
	defer ticker.Stop()

	s.logger.Info("Starting event polling",
		zap.Duration("interval", s.config.EventPollingInterval),
		zap.String("rpc_endpoint", s.config.RPCEndpoint))

	for {
		select {
		case <-ctx.Done():
			s.logger.Info("Event polling stopped due to context cancellation")
			return
		case <-s.stopCh:
			s.logger.Info("Event polling stopped due to service shutdown")
			return
		case <-ticker.C:
			if err := s.fetchAndProcessEvents(ctx); err != nil {
				s.logger.Error("Failed to fetch and process events",
					zap.Error(err))
				s.metrics.RecordError("event_polling", err)
			}
		}
	}
}

// fetchAndProcessEvents fetches new events from the blockchain and sends them for processing
func (s *Service) fetchAndProcessEvents(ctx context.Context) error {
	// Create RPC client with timeout
	rpcCtx, cancel := context.WithTimeout(ctx, time.Second*30)
	defer cancel()

	c, err := rpcclient.New(rpcCtx, s.config.RPCEndpoint, rpcclient.Options{
		DialTimeout:    time.Second * 10,
		RequestTimeout: time.Second * 30,
	})
	if err != nil {
		return errors.Wrap(err, "failed to create RPC client")
	}
	defer c.Close()

	// Get latest block height
	height, err := c.GetBlockCount()
	if err != nil {
		return errors.Wrap(err, "failed to get block height")
	}

	// Get block hash for the previous block
	blockHash, err := c.GetBlockHash(height - 1)
	if err != nil {
		return errors.Wrap(err, "failed to get block hash")
	}

	// Get notifications using the new API
	notifications, err := c.GetBlockNotifications(blockHash)
	if err != nil {
		return errors.Wrap(err, "failed to get notifications")
	}

	s.logger.Debug("Fetched notifications",
		zap.Int("count", len(notifications.Application)),
		zap.Uint32("block_height", height-1))

	// Process each notification
	for _, notification := range notifications.Application {
		data, err := parseStackItem(notification.Item)
		if err != nil {
			s.logger.Error("Failed to parse notification data",
				zap.Error(err))
			continue
		}

		// Create event
		event := &Event{
			ID:           fmt.Sprintf("%s-%d", notification.ScriptHash.StringLE(), time.Now().UnixNano()),
			ContractHash: notification.ScriptHash,
			Name:         notification.Name,
			Data:         data,
			Timestamp:    time.Now(),
		}

		// Send event to channel
		select {
		case s.eventCh <- event:
			s.logger.Debug("Event sent",
				zap.String("id", event.ID),
				zap.String("name", event.Name))
			s.metrics.RecordEventProcessed(event)
		case <-ctx.Done():
			return nil
		case <-s.stopCh:
			return nil
		}
	}

	return nil
}

// processEvents processes events from the event channel
func (s *Service) processEvents(ctx context.Context) {
	// Create worker pool for concurrent processing
	var wg sync.WaitGroup
	semaphore := make(chan struct{}, s.config.MaxConcurrentExecutions)

	s.logger.Info("Starting event processing",
		zap.Int("max_concurrent", s.config.MaxConcurrentExecutions))

	for {
		select {
		case <-ctx.Done():
			s.logger.Info("Event processing stopped due to context cancellation")
			wg.Wait()
			return
		case <-s.stopCh:
			s.logger.Info("Event processing stopped due to service shutdown")
			wg.Wait()
			return
		case event := <-s.eventCh:
			// Acquire semaphore
			semaphore <- struct{}{}
			wg.Add(1)

			// Process event in goroutine
			go func(event *Event) {
				defer wg.Done()
				defer func() { <-semaphore }()

				if err := s.handleEvent(event); err != nil {
					s.logger.Error("Failed to handle event",
						zap.Error(err),
						zap.String("event_id", event.ID))
					s.metrics.RecordError("event_handling", err)
				}
			}(event)
		}
	}
}

// handleEvent processes a single event
func (s *Service) handleEvent(event *Event) error {
	s.mu.RLock()
	defer s.mu.RUnlock()

	s.logger.Debug("Processing event",
		zap.String("event_id", event.ID),
		zap.String("event_name", event.Name))

	// Find matching triggers
	for _, trigger := range s.triggers {
		if trigger.Status != TriggerStatusActive {
			continue
		}

		if trigger.ContractHash.Equals(event.ContractHash) && trigger.EventName == event.Name {
			// Check condition if specified
			if trigger.Condition != "" {
				match, err := s.evaluateCondition(trigger.Condition, event.Data)
				if err != nil {
					s.logger.Error("Failed to evaluate condition",
						zap.Error(err),
						zap.String("trigger_id", trigger.ID))
					continue
				}
				if !match {
					s.logger.Debug("Condition not met",
						zap.String("trigger_id", trigger.ID))
					continue
				}
			}

			// Execute action with retry
			execution := &TriggerExecution{
				ID:        util.Uint256{}.String(), // Generate proper UUID
				TriggerID: trigger.ID,
				EventID:   event.ID,
				Status:    ExecutionStatusPending,
				StartTime: time.Now(),
			}

			if err := s.executeWithRetry(trigger, event, execution); err != nil {
				s.logger.Error("Failed to execute trigger",
					zap.Error(err),
					zap.String("trigger_id", trigger.ID))
				execution.Status = ExecutionStatusFailed
				execution.Error = err.Error()
			} else {
				execution.Status = ExecutionStatusCompleted
			}

			execution.EndTime = time.Now()
			execution.Duration = execution.EndTime.Sub(execution.StartTime).Milliseconds()

			// Save execution
			if err := s.store.SaveExecution(execution); err != nil {
				s.logger.Error("Failed to save execution",
					zap.Error(err),
					zap.String("execution_id", execution.ID))
			}

			// Update trigger metadata
			trigger.LastExecuted = time.Now()
			trigger.ExecutionCount++
			if err := s.store.SaveTrigger(trigger); err != nil {
				s.logger.Error("Failed to update trigger",
					zap.Error(err),
					zap.String("trigger_id", trigger.ID))
			}

			s.metrics.RecordExecution(execution)
		}
	}

	return nil
}

// executeWithRetry executes the trigger action with retry logic
func (s *Service) executeWithRetry(trigger *Trigger, event *Event, execution *TriggerExecution) error {
	var lastErr error

	for attempt := 0; attempt <= s.config.RetryAttempts; attempt++ {
		if attempt > 0 {
			// Wait before retry
			time.Sleep(s.config.RetryDelay)
			execution.RetryCount = attempt
		}

		// Execute with timeout
		execCtx, cancel := context.WithTimeout(context.Background(), s.config.ExecutionTimeout)
		result, err := s.executor.Execute(execCtx, trigger, event)
		cancel()

		if err == nil {
			execution.Result = result
			return nil
		}

		lastErr = err
		s.logger.Warn("Execution attempt failed",
			zap.Error(err),
			zap.Int("attempt", attempt+1),
			zap.String("trigger_id", trigger.ID))
	}

	return errors.Wrap(lastErr, "all execution attempts failed")
}

// evaluateCondition evaluates a condition against event data
func (s *Service) evaluateCondition(condition string, data map[string]interface{}) (bool, error) {
	// Create evaluation context
	ctx := map[string]interface{}{
		"event": data,
	}

	// Use expression evaluator to evaluate condition
	result, err := s.evaluator.Evaluate(condition, ctx)
	if err != nil {
		return false, errors.Wrap(err, "failed to evaluate condition")
	}

	// Convert result to boolean
	match, ok := result.(bool)
	if !ok {
		return false, errors.New("condition did not evaluate to boolean")
	}

	return match, nil
}
