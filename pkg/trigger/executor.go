package trigger

import (
	"context"

	"github.com/pkg/errors"
	"go.uber.org/zap"
)

// DefaultExecutor implements the Executor interface
type DefaultExecutor struct {
	service *Service
	logger  *zap.Logger
}

// NewExecutor creates a new executor
func NewExecutor(service *Service, logger *zap.Logger) *DefaultExecutor {
	return &DefaultExecutor{
		service: service,
		logger:  logger,
	}
}

// Execute executes a trigger action
func (e *DefaultExecutor) Execute(ctx context.Context, trigger *Trigger, event *Event) (interface{}, error) {
	// Execute action
	if err := e.service.executeAction(ctx, trigger, event); err != nil {
		return nil, errors.Wrap(err, "failed to execute action")
	}

	return nil, nil
}
