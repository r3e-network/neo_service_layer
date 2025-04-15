package transaction

import "errors"

// Error definitions for the transaction service
var (
	// ErrTransactionNotFound is returned when a transaction is not found
	ErrTransactionNotFound = errors.New("transaction not found")

	// ErrInvalidTransactionID is returned when an invalid transaction ID is provided
	ErrInvalidTransactionID = errors.New("invalid transaction ID")

	// ErrInvalidTransactionType is returned when an invalid transaction type is provided
	ErrInvalidTransactionType = errors.New("invalid transaction type")

	// ErrInvalidTransactionConfig is returned when an invalid transaction configuration is provided
	ErrInvalidTransactionConfig = errors.New("invalid transaction configuration")

	// ErrTransactionAlreadySigned is returned when attempting to sign an already signed transaction
	ErrTransactionAlreadySigned = errors.New("transaction already signed")

	// ErrTransactionNotSigned is returned when attempting to send an unsigned transaction
	ErrTransactionNotSigned = errors.New("transaction not signed")

	// ErrTransactionAlreadySent is returned when attempting to send an already sent transaction
	ErrTransactionAlreadySent = errors.New("transaction already sent")

	// ErrInvalidNetworkType is returned when an invalid network type is provided
	ErrInvalidNetworkType = errors.New("invalid network type")

	// ErrTransactionFailed is returned when a transaction fails
	ErrTransactionFailed = errors.New("transaction failed")

	// ErrInsufficientFunds is returned when there are insufficient funds for a transaction
	ErrInsufficientFunds = errors.New("insufficient funds")

	// ErrServiceUnavailable is returned when the transaction service is unavailable
	ErrServiceUnavailable = errors.New("transaction service unavailable")
)