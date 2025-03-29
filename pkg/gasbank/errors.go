package gasbank

import "errors"

var (
	// ErrServiceAlreadyRunning is returned when trying to start an already running service
	ErrServiceAlreadyRunning = errors.New("service is already running")

	// ErrAccountNotFound is returned when an account is not found
	ErrAccountNotFound = errors.New("account not found")

	// ErrAccountAlreadyExists is returned when trying to create an existing account
	ErrAccountAlreadyExists = errors.New("account already exists")

	// ErrInvalidContractHash is returned when the contract hash is invalid
	ErrInvalidContractHash = errors.New("invalid contract hash")

	// ErrInvalidContractMethod is returned when the contract method is invalid
	ErrInvalidContractMethod = errors.New("invalid contract method")

	// ErrInvalidMinBalance is returned when the minimum balance is invalid
	ErrInvalidMinBalance = errors.New("invalid minimum balance")

	// ErrInvalidMaxBalance is returned when the maximum balance is invalid
	ErrInvalidMaxBalance = errors.New("invalid maximum balance")

	// ErrInvalidReservationTimeout is returned when the reservation timeout is invalid
	ErrInvalidReservationTimeout = errors.New("invalid reservation timeout")

	// ErrInsufficientBalance is returned when there is insufficient balance for a reservation
	ErrInsufficientBalance = errors.New("insufficient balance")

	// ErrReservationNotFound is returned when a reservation is not found
	ErrReservationNotFound = errors.New("reservation not found")

	// ErrReservationExpired is returned when a reservation has expired
	ErrReservationExpired = errors.New("reservation expired")

	// ErrReservationAlreadyConsumed is returned when a reservation has already been consumed
	ErrReservationAlreadyConsumed = errors.New("reservation already consumed")

	// ErrReservationAlreadyReleased is returned when a reservation has already been released
	ErrReservationAlreadyReleased = errors.New("reservation already released")

	// ErrInvalidReservationAmount is returned when the reservation amount is invalid
	ErrInvalidReservationAmount = errors.New("invalid reservation amount")

	// ErrAccountLocked is returned when trying to operate on a locked account
	ErrAccountLocked = errors.New("account is locked")
)
