package pricefeed

import "errors"

var (
	// ErrServiceAlreadyRunning is returned when trying to start an already running service
	ErrServiceAlreadyRunning = errors.New("service is already running")

	// ErrPairNotFound is returned when a requested trading pair is not found
	ErrPairNotFound = errors.New("trading pair not found")

	// ErrInvalidContractHash is returned when the contract hash is invalid
	ErrInvalidContractHash = errors.New("invalid contract hash")

	// ErrInvalidContractMethod is returned when the contract method is invalid
	ErrInvalidContractMethod = errors.New("invalid contract method")

	// ErrInvalidUpdateInterval is returned when the update interval is invalid
	ErrInvalidUpdateInterval = errors.New("invalid update interval")

	// ErrNoDataSources is returned when no data sources are configured
	ErrNoDataSources = errors.New("no data sources configured")

	// ErrDataSourceNotFound is returned when a data source is not found
	ErrDataSourceNotFound = errors.New("data source not found")

	// ErrInvalidDataSourceType is returned when a data source type is invalid
	ErrInvalidDataSourceType = errors.New("invalid data source type")

	// ErrInvalidDataSourceConfig is returned when a data source configuration is invalid
	ErrInvalidDataSourceConfig = errors.New("invalid data source configuration")

	// ErrTransactionFailed is returned when a transaction fails
	ErrTransactionFailed = errors.New("transaction failed")
)
