package trigger

import "errors"

var (
	// Service errors
	ErrServiceAlreadyRunning = errors.New("trigger service is already running")
	ErrServiceNotRunning     = errors.New("trigger service is not running")

	// Trigger errors
	ErrTriggerNotFound      = errors.New("trigger not found")
	ErrMaxTriggersReached   = errors.New("maximum number of triggers reached")
	ErrInvalidTriggerID     = errors.New("invalid trigger ID")
	ErrInvalidTriggerName   = errors.New("invalid trigger name")
	ErrInvalidEventName     = errors.New("invalid event name")
	ErrInvalidAction        = errors.New("invalid action")
	ErrInvalidCondition     = errors.New("invalid condition")
	ErrTriggerAlreadyExists = errors.New("trigger already exists")

	// Configuration errors
	ErrInvalidContractHash         = errors.New("invalid contract hash")
	ErrInvalidContractMethod       = errors.New("invalid contract method")
	ErrInvalidMaxTriggers          = errors.New("invalid max triggers")
	ErrInvalidMaxEventsPerTrigger  = errors.New("invalid max events per trigger")
	ErrInvalidEventPollingInterval = errors.New("invalid event polling interval")
	ErrInvalidEventRetentionPeriod = errors.New("invalid event retention period")

	// Event errors
	ErrEventNotFound         = errors.New("event not found")
	ErrInvalidEventData      = errors.New("invalid event data")
	ErrEventProcessingFailed = errors.New("event processing failed")
	ErrEventValidationFailed = errors.New("event validation failed")
	ErrMaxEventsReached      = errors.New("maximum number of events reached")
)
