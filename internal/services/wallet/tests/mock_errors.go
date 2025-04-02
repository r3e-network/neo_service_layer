package tests

import "errors"

// Custom errors for mock wallet implementation
var (
	ErrWalletLocked     = errors.New("wallet is locked")
	ErrInvalidPassword  = errors.New("invalid password")
	ErrAccountNotFound  = errors.New("account not found")
	ErrWalletNotFound   = errors.New("wallet not found")
	ErrInvalidSignature = errors.New("invalid signature")
)
