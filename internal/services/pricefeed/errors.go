package pricefeed

import "errors"

// Define common errors for the PriceFeed service
var (
	// ErrPriceNotFound is returned when a price is not found
	ErrPriceNotFound = errors.New("price not found")

	// ErrInvalidPrice is returned when price is invalid
	ErrInvalidPrice = errors.New("invalid price")

	// ErrInvalidSymbol is returned when symbol is invalid
	ErrInvalidSymbol = errors.New("invalid symbol")

	// ErrPermissionDenied is returned when a user does not have permission to publish a price
	ErrPermissionDenied = errors.New("permission denied")

	// ErrPriceStale is returned when a price is too old
	ErrPriceStale = errors.New("price is stale")

	// ErrPublishingFailed is returned when price publishing fails
	ErrPublishingFailed = errors.New("price publishing failed")
)