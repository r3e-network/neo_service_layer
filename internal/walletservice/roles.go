package wallet

import (
	"context"
	"errors"

	"go.uber.org/zap"
)

// AssignWalletToRole assigns a wallet to a specific role in the service layer
func (s *ServiceImpl) AssignWalletToRole(ctx context.Context, walletName string, role string) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	// Validate parameters
	if walletName == "" {
		return errors.New("wallet name cannot be empty")
	}

	if role == "" {
		return errors.New("role cannot be empty")
	}

	// Check if wallet exists
	_, err := s.GetWalletInfo(ctx, walletName)
	if err != nil {
		return err
	}

	// Assign wallet to role
	s.roleAssignments[role] = walletName

	// Log role assignment
	if s.config.AuditLog {
		s.logger.Info("Wallet assigned to role",
			zap.String("wallet", walletName),
			zap.String("role", role))
	}

	return nil
}

// GetWalletForRole gets the wallet assigned to a specific role
func (s *ServiceImpl) GetWalletForRole(ctx context.Context, role string) (*WalletInfo, error) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	// Validate parameters
	if role == "" {
		return nil, errors.New("role cannot be empty")
	}

	// Check if role is assigned
	walletName, exists := s.roleAssignments[role]
	if !exists {
		return nil, ErrRoleNotAssigned
	}

	// Get wallet info
	return s.GetWalletInfo(ctx, walletName)
}

// GetRoleAssignments returns all role assignments
func (s *ServiceImpl) GetRoleAssignments() map[string]string {
	s.mu.RLock()
	defer s.mu.RUnlock()

	// Create a copy to avoid exposing internal map
	assignments := make(map[string]string, len(s.roleAssignments))
	for k, v := range s.roleAssignments {
		assignments[k] = v
	}

	return assignments
}

// predefinedRoles returns a list of predefined roles with descriptions
func (s *ServiceImpl) predefinedRoles() map[string]string {
	return map[string]string{
		"gas_bank":   "Wallet for GasBank service operations",
		"price_feed": "Wallet for PriceFeed service operations",
		"automation": "Wallet for contract automation operations",
		"system":     "System wallet for general service operations",
	}
}
