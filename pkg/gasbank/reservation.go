package gasbank

import (
	"fmt"
	"math/big"
	"time"

	"github.com/google/uuid"
)

// ReserveGas reserves gas for an account
func (s *Service) ReserveGas(req *ReservationRequest) (*ReservationResponse, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Get account
	account, err := s.store.GetAccount(req.AccountID)
	if err != nil {
		return nil, fmt.Errorf("failed to get account: %w", err)
	}

	// Check account status
	if account.Status == AccountStatusLocked {
		return nil, fmt.Errorf("account is locked")
	}

	// Validate amount
	if req.Amount.Sign() <= 0 {
		return nil, fmt.Errorf("amount must be positive")
	}

	if req.Amount.Cmp(account.Balance) > 0 {
		return nil, fmt.Errorf("insufficient balance")
	}

	if req.Amount.Cmp(s.config.MaxAllocationPerUser) > 0 {
		return nil, fmt.Errorf("amount exceeds maximum allocation")
	}

	if req.Amount.Cmp(s.config.MinAllocationAmount) < 0 {
		return nil, fmt.Errorf("amount below minimum allocation")
	}

	// Create reservation
	reservation := &Reservation{
		ID:         uuid.New().String(),
		AccountID:  req.AccountID,
		Amount:     req.Amount,
		CreatedAt:  time.Now(),
		ExpiresAt:  time.Now().Add(req.TTL),
		Status:     "active",
		AmountUsed: new(big.Int),
	}

	// Update account
	account.ReservedGas = account.ReservedGas.Add(account.ReservedGas, req.Amount)
	account.UpdatedAt = time.Now()

	// Save changes
	if err := s.store.SaveReservation(reservation); err != nil {
		return nil, fmt.Errorf("failed to save reservation: %w", err)
	}

	if err := s.store.SaveAccount(account); err != nil {
		return nil, fmt.Errorf("failed to update account: %w", err)
	}

	// Record metrics
	s.metrics.RecordGasReservation(req.Amount)

	return &ReservationResponse{
		ReservationID: reservation.ID,
		Amount:        reservation.Amount,
		ExpiresAt:     reservation.ExpiresAt,
	}, nil
}

// ReleaseGas releases reserved gas back to the account
func (s *Service) ReleaseGas(req *ReleaseRequest) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Get reservation
	reservation, err := s.store.GetReservation(req.ReservationID)
	if err != nil {
		return fmt.Errorf("failed to get reservation: %w", err)
	}

	// Validate reservation
	if reservation.Status != "active" {
		return fmt.Errorf("reservation is not active")
	}

	if reservation.AccountID != req.AccountID {
		return fmt.Errorf("account ID mismatch")
	}

	// Get account
	account, err := s.store.GetAccount(req.AccountID)
	if err != nil {
		return fmt.Errorf("failed to get account: %w", err)
	}

	// Update reservation
	reservation.Status = "released"
	reservation.AmountUsed = req.AmountUsed
	reservation.ReleasedAt = time.Now()

	// Update account
	account.ReservedGas = account.ReservedGas.Sub(account.ReservedGas, reservation.Amount)
	account.TotalUsed = account.TotalUsed.Add(account.TotalUsed, req.AmountUsed)
	account.UpdatedAt = time.Now()
	account.LastUsedAt = time.Now()

	// Save changes
	if err := s.store.SaveReservation(reservation); err != nil {
		return fmt.Errorf("failed to save reservation: %w", err)
	}

	if err := s.store.SaveAccount(account); err != nil {
		return fmt.Errorf("failed to update account: %w", err)
	}

	// Record metrics
	s.metrics.RecordGasRelease(reservation.Amount)
	s.metrics.RecordGasUsage(req.AmountUsed)

	return nil
}

// CleanupExpiredReservations removes expired gas reservations
func (s *Service) CleanupExpiredReservations() error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Get all reservations
	reservations, err := s.store.ListReservations()
	if err != nil {
		return fmt.Errorf("failed to list reservations: %w", err)
	}

	now := time.Now()
	for _, reservation := range reservations {
		if reservation.Status == "active" && now.After(reservation.ExpiresAt) {
			// Get account
			account, err := s.store.GetAccount(reservation.AccountID)
			if err != nil {
				s.logger.Error("Failed to get account", "error", err)
				continue
			}

			// Update reservation
			reservation.Status = "expired"

			// Update account
			account.ReservedGas = account.ReservedGas.Sub(account.ReservedGas, reservation.Amount)
			account.UpdatedAt = now

			// Save changes
			if err := s.store.SaveReservation(reservation); err != nil {
				s.logger.Error("Failed to save reservation", "error", err)
				continue
			}

			if err := s.store.SaveAccount(account); err != nil {
				s.logger.Error("Failed to update account", "error", err)
				continue
			}

			// Record metrics
			s.metrics.RecordGasRelease(reservation.Amount)
		}
	}

	return nil
}
