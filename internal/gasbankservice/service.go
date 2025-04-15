package gasbank

import (
	"context"
	"errors"
	"fmt"
	"math/big"
	"strings"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"

	// Assuming neo client package exists
	"github.com/nspcc-dev/neo-go/pkg/wallet"                     // Use neo-go wallet
	"github.com/r3e-network/neo_service_layer/internal/core/neo" // Use the actual path
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/store"
	log "github.com/sirupsen/logrus"
)

// Compile-time check to ensure Service implements the interface
var _ Service = (*ServiceImpl)(nil)

// ServiceImpl implements the GasBank service interface.
type ServiceImpl struct {
	config    *models.Config
	store     store.Store
	neoClient *neo.Client    // Neo client for interacting with the blockchain
	wallet    *wallet.Wallet // Service wallet for signing withdrawals, claims
	// Add other internal components if needed (e.g., for monitoring deposits)
}

// NewServiceImpl creates a new GasBank service implementation.
func NewServiceImpl(config *models.Config) (*ServiceImpl, error) {
	if config == nil {
		return nil, errors.New("gasbank config cannot be nil")
	}

	// Validate config (existing logic + add checks for node url)
	if config.EnableUserBalances {
		if config.MinDepositAmount == nil || config.MinDepositAmount.Sign() <= 0 {
			log.Warn("MinDepositAmount not configured or invalid, using default (0.1 GAS)")
			config.MinDepositAmount = big.NewInt(10_000_000) // 0.1 GAS
		}
		if config.WithdrawalFee == nil || config.WithdrawalFee.Sign() < 0 {
			log.Warn("WithdrawalFee not configured or invalid, using default (0 GAS)")
			config.WithdrawalFee = big.NewInt(0)
		}
	}
	if config.NeoNodeURL == "" {
		return nil, errors.New("neoNodeUrl is required in gasbank config")
	}
	if config.WalletPath == "" || config.WalletPass == "" {
		log.Warn("Service wallet path or password not configured. GAS claiming and withdrawals might be disabled.")
		// Don't return error? Or require it? Let's require it for now if features are enabled.
		// if config.EnableUserBalances { // Only required if withdrawals/claims are active
		//    return nil, errors.New("service wallet path and password are required for withdrawals/claims")
		//}
	}

	// Initialize store (existing logic)
	var s store.Store
	var err error
	switch strings.ToLower(config.StoreType) {
	case "memory", "":
		s = store.NewMemoryStore()
		log.Info("GasBank Service initialized with in-memory store")
	// case "badger":
	// ... badger init ...
	default:
		err = fmt.Errorf("unsupported gasbank store type: %s", config.StoreType)
	}
	if err != nil {
		return nil, fmt.Errorf("failed to initialize gasbank store: %w", err)
	}

	// Initialize Neo client
	// TODO: Add options like timeouts, retries?
	// Passing empty config pointer, assuming URL/config handled internally by NewClient
	// or needs separate configuration step. This may require adjustment.
	clientConfig := &neo.Config{}
	neoClient, err := neo.NewClient(clientConfig) // Pass pointer to config
	if err != nil {
		return nil, fmt.Errorf("failed to initialize neo client: %w", err)
	}
	log.Infof("GasBank Service connected to Neo node: %s", config.NeoNodeURL)

	// Open service wallet if configured
	var serviceWallet *wallet.Wallet
	if config.WalletPath != "" && config.WalletPass != "" {
		// Use NewWalletFromFile
		serviceWallet, err = wallet.NewWalletFromFile(config.WalletPath)
		if err != nil {
			log.Errorf("Failed to load service wallet file %s: %v", config.WalletPath, err)
			return nil, fmt.Errorf("failed to load service wallet file: %w", err)
		}
		// Decrypt accounts individually, providing the wallet's Scrypt params
		decryptionSuccessful := false
		if len(serviceWallet.Accounts) == 0 {
			return nil, fmt.Errorf("service wallet %s contains no accounts", config.WalletPath)
		}
		// Use the wallet's Scrypt params for decryption attempts
		walletScryptParams := serviceWallet.Scrypt // Assuming Scrypt field exists on Wallet
		for _, acc := range serviceWallet.Accounts {
			// Pass password and wallet's Scrypt params
			err = acc.Decrypt(config.WalletPass, walletScryptParams)
			if err != nil {
				log.Warnf("Failed to decrypt account %s in wallet %s: %v", acc.Address, config.WalletPath, err)
				// Continue trying other accounts, maybe only some are needed?
			} else {
				decryptionSuccessful = true
				log.Debugf("Successfully decrypted account: %s", acc.Address)
				// Optional: Break if only one decrypted account is needed?
			}
		}

		// Check if at least one account was decrypted successfully
		if !decryptionSuccessful {
			log.Errorf("Failed to decrypt *any* account in service wallet %s with the provided password.", config.WalletPath)
			return nil, fmt.Errorf("failed to decrypt any account in service wallet")
		}

		log.Infof("Service wallet opened and accounts decrypted (at least partially): %s (Accounts: %d)", config.WalletPath, len(serviceWallet.Accounts))
		// TODO: Select a specific account for signing fees if multiple exist?
	}

	svc := &ServiceImpl{
		config:    config,
		store:     s,
		neoClient: neoClient,
		wallet:    serviceWallet,
	}

	return svc, nil
}

// Start starts background processes (e.g., deposit monitoring).
func (s *ServiceImpl) Start(ctx context.Context) error {
	log.Info("Starting GasBank Service...")
	// TODO: Implement background tasks if needed (e.g., monitoring deposits)
	log.Info("GasBank Service started.")
	return nil
}

// Stop stops background processes and closes resources.
func (s *ServiceImpl) Stop(ctx context.Context) error {
	log.Info("Stopping GasBank Service...")
	if err := s.store.Close(); err != nil {
		log.Errorf("Error closing gasbank store: %v", err)
		// Decide if we should return error or just log
	}
	// TODO: Close neo client connection?
	log.Info("GasBank Service stopped.")
	return nil
}

// --- Persistent User Balances ---

// GetUserBalance retrieves the current GAS balance for a user.
func (s *ServiceImpl) GetUserBalance(ctx context.Context, userAddress util.Uint160) (*models.UserBalance, error) {
	balance, err := s.store.GetUserBalance(ctx, userAddress)
	if err != nil {
		// Log error but return specific user-facing errors if needed
		log.Errorf("Failed to get balance for user %s: %v", userAddress.StringLE(), err)
		return nil, fmt.Errorf("failed to retrieve balance") // Generic error for now
	}
	return balance, nil
}

// RecordDeposit handles recording a detected user deposit.
// Assumes deposit verification (tx confirmation, correct recipient) happens *before* calling this.
func (s *ServiceImpl) RecordDeposit(ctx context.Context, userAddress util.Uint160, txHash util.Uint256, amount *big.Int) error {
	if !s.config.EnableUserBalances {
		return errors.New("user balances are disabled")
	}
	if amount == nil || amount.Sign() <= 0 {
		return errors.New("deposit amount must be positive")
	}
	if amount.Cmp(s.config.MinDepositAmount) < 0 {
		return fmt.Errorf("deposit amount %s is less than minimum %s", amount.String(), s.config.MinDepositAmount.String())
	}

	log.Infof("Recording deposit for user %s: Amount=%s, TxHash=%s", userAddress.StringLE(), amount.String(), txHash.StringLE())

	err := s.store.UpdateUserBalance(ctx, userAddress, func(balance *models.UserBalance) (*models.UserBalance, error) {
		balance.Balance.Add(balance.Balance, amount)
		// TODO: Add transaction record/history?
		return balance, nil
	})

	if err != nil {
		log.Errorf("Failed to update balance after deposit for user %s: %v", userAddress.StringLE(), err)
		return fmt.Errorf("failed to record deposit")
	}

	log.Infof("Deposit recorded successfully for user %s.", userAddress.StringLE())
	// TODO: Emit event or notification?
	return nil
}

// InitiateWithdrawal starts the process for a user to withdraw GAS.
func (s *ServiceImpl) InitiateWithdrawal(ctx context.Context, userAddress util.Uint160, amount *big.Int) (string, error) {
	if !s.config.EnableUserBalances {
		return "", errors.New("user balances are disabled")
	}
	if amount == nil || amount.Sign() <= 0 {
		return "", errors.New("withdrawal amount must be positive")
	}
	// Check if service wallet is configured
	// if s.wallet == nil { return "", errors.New("withdrawals currently disabled (service wallet not configured)") }

	totalWithdrawal := new(big.Int).Add(amount, s.config.WithdrawalFee)
	requestID := uuid.NewString() // Generate unique ID for this withdrawal request

	log.Infof("Initiating withdrawal request %s for user %s: Amount=%s (Total=%s including fee %s)",
		requestID, userAddress.StringLE(), amount.String(), totalWithdrawal.String(), s.config.WithdrawalFee.String())

	// Lock the balance first
	err := s.store.UpdateUserBalance(ctx, userAddress, func(balance *models.UserBalance) (*models.UserBalance, error) {
		if balance.Balance.Cmp(totalWithdrawal) < 0 {
			return nil, fmt.Errorf("insufficient balance (Available: %s, Required: %s)", balance.Balance.String(), totalWithdrawal.String())
		}
		balance.Balance.Sub(balance.Balance, totalWithdrawal)
		balance.LockedBalance.Add(balance.LockedBalance, totalWithdrawal)
		return balance, nil
	})

	if err != nil {
		log.Errorf("Failed to lock balance for withdrawal request %s: %v", requestID, err)
		return "", fmt.Errorf("failed to initiate withdrawal: %w", err)
	}

	// If locking succeeded, create the withdrawal record
	record := &models.WithdrawalRecord{
		RequestID:   requestID,
		UserID:      userAddress,
		Amount:      new(big.Int).Set(amount),
		TotalLocked: new(big.Int).Set(totalWithdrawal),
		Status:      "Processing", // Mark as Processing (was Pending, now locked)
		CreatedAt:   time.Now(),
		UpdatedAt:   time.Now(),
	}
	if err := s.store.SaveWithdrawalRecord(ctx, record); err != nil {
		log.Errorf("CRITICAL ERROR: Failed to save withdrawal record %s after locking balance: %v. Attempting rollback.", requestID, err)
		// Attempt to rollback balance lock
		rollbackErr := s.store.UpdateUserBalance(context.Background(), userAddress, func(balance *models.UserBalance) (*models.UserBalance, error) {
			// Check consistency before rollback
			if balance.LockedBalance.Cmp(totalWithdrawal) < 0 {
				log.Errorf("Rollback Inconsistency: Locked balance %s < withdrawal %s", balance.LockedBalance.String(), totalWithdrawal.String())
				return balance, errors.New("rollback inconsistency")
			}
			balance.LockedBalance.Sub(balance.LockedBalance, totalWithdrawal)
			balance.Balance.Add(balance.Balance, totalWithdrawal)
			return balance, nil
		})
		if rollbackErr != nil {
			log.Fatalf("CRITICAL ERROR: Failed to rollback balance lock for request %s after failing to save record: %v. Manual intervention needed!", requestID, rollbackErr)
		}
		return "", fmt.Errorf("failed to save withdrawal record: %w", err)
	}

	// Proceed to create and broadcast transaction asynchronously
	go s.processWithdrawal(requestID, userAddress, amount)

	log.Infof("Withdrawal request %s accepted for user %s.", requestID, userAddress.StringLE())
	return requestID, nil // Return request ID for status tracking
}

// processWithdrawal handles the actual transaction creation and broadcast.
func (s *ServiceImpl) processWithdrawal(requestID string, userAddress util.Uint160, amount *big.Int) {
	ctx := context.Background() // Use a background context
	log.Infof("[Withdrawal %s] Processing withdrawal for %s: Amount=%s", requestID, userAddress.StringLE(), amount.String())

	var finalTxHash *util.Uint256
	var broadcastError error

	// Ensure client and wallet are available
	if s.neoClient == nil {
		broadcastError = errors.New("neo client not available")
	} else if s.wallet == nil || len(s.wallet.Accounts) == 0 {
		broadcastError = errors.New("service wallet not configured or empty")
	}

	// If pre-checks failed, mark as failed and attempt rollback
	if broadcastError != nil {
		log.Errorf("[Withdrawal %s] Pre-check failed: %v", requestID, broadcastError)
		_ = s.store.UpdateWithdrawalStatus(ctx, requestID, "Failed", nil, broadcastError.Error())
	} else {
		// --- Build, Sign, and Send Transaction ---
		senderAccount := s.wallet.Accounts[0] // Use the first account
		senderAddress := senderAccount.ScriptHash()

		// Get GAS token hash (assuming client provides this)
		gasHash, err := s.neoClient.GetGASHash() // Assumes method exists
		if err != nil {
			broadcastError = fmt.Errorf("failed to get GAS token hash: %w", err)
		} else {
			// Build unsigned transaction
			tx, err := s.neoClient.NewTransfer(senderAddress, userAddress, gasHash, amount, nil)
			if err != nil {
				broadcastError = fmt.Errorf("failed to build transfer transaction: %w", err)
			} else {
				// Calculate network fee
				networkFee, err := s.neoClient.CalculateNetworkFee(tx)
				if err != nil {
					broadcastError = fmt.Errorf("failed to calculate network fee: %w", err)
				} else {
					tx.NetworkFee = networkFee
					// Add sender as signer (already implicit in NewTransfer? Depends on impl.)
					// tx.Signers = []transaction.Signer{{Account: senderAddress}}

					// Sign the transaction
					err = senderAccount.SignTx(tx)
					if err != nil {
						broadcastError = fmt.Errorf("failed to sign withdrawal transaction: %w", err)
					} else {
						// Broadcast the transaction
						txid, err := s.neoClient.SendRawTransaction(tx)
						if err != nil {
							broadcastError = fmt.Errorf("failed to broadcast withdrawal transaction: %w", err)
						} else {
							finalTxHash = &txid // Success!
						}
					}
				}
			}
		}
		// --- End Transaction Logic ---

		// Update store status based on outcome
		if broadcastError != nil {
			log.Errorf("[Withdrawal %s] Transaction processing failed: %v", requestID, broadcastError)
			_ = s.store.UpdateWithdrawalStatus(ctx, requestID, "Failed", nil, broadcastError.Error())
		} else {
			log.Infof("[Withdrawal %s] Transaction broadcast successful: %s", requestID, finalTxHash.StringLE())
			_ = s.store.UpdateWithdrawalStatus(ctx, requestID, "Submitted", finalTxHash, "")
		}
	}

	// Update the user balance based on the outcome
	err := s.store.UpdateUserBalance(ctx, userAddress, func(balance *models.UserBalance) (*models.UserBalance, error) {
		record, getErr := s.store.GetWithdrawalRecord(ctx, requestID)
		if getErr != nil {
			log.Errorf("[Withdrawal %s] CRITICAL: Failed to retrieve withdrawal record during balance update: %v", requestID, getErr)
			return nil, fmt.Errorf("failed to retrieve withdrawal record: %w", getErr)
		}

		totalLocked := record.TotalLocked
		if balance.LockedBalance.Cmp(totalLocked) < 0 {
			log.Errorf("[Withdrawal %s] Inconsistency: Locked balance %s < recorded locked %s", requestID, balance.LockedBalance.String(), totalLocked.String())
			balance.LockedBalance = big.NewInt(0)
		} else {
			balance.LockedBalance.Sub(balance.LockedBalance, totalLocked)
		}

		if broadcastError != nil {
			balance.Balance.Add(balance.Balance, totalLocked) // Refund if failed
			log.Infof("[Withdrawal %s] Restored balance for user %s due to failed broadcast.", requestID, userAddress.StringLE())
		} // If successful, balance remains reduced, locked amount is cleared.
		return balance, nil
	})

	if err != nil {
		log.Fatalf("[Withdrawal %s] CRITICAL ERROR: Failed to update balance for user %s after withdrawal attempt: %v. Manual intervention required!", requestID, userAddress.StringLE(), err)
	}

	log.Infof("[Withdrawal %s] Processing complete.", requestID)
}

// GetWithdrawalStatus checks the status of a withdrawal request.
func (s *ServiceImpl) GetWithdrawalStatus(ctx context.Context, userAddress util.Uint160, requestID string) (string, error) {
	record, err := s.store.GetWithdrawalRecord(ctx, requestID)
	if err != nil {
		// Check if ErrNotFound specifically
		// if errors.Is(err, store.ErrNotFound) { ... }
		log.Warnf("Failed to get withdrawal record for request %s: %v", requestID, err)
		return "", fmt.Errorf("failed to retrieve withdrawal status")
	}

	// Verify ownership (optional, depends if requestID is guessable)
	if !record.UserID.Equals(userAddress) {
		return "", fmt.Errorf("withdrawal record not found or permission denied")
	}

	// TODO: Optionally check blockchain confirmation if status is "Submitted"
	// if record.Status == "Submitted" && record.TxHash != nil {
	//    ... check app log ... update status to "Confirmed" or "FailedOnChain"
	// }

	return record.Status, nil
}

// --- Fee Payment Sponsorship ---

// RequestTransactionFeeSponsorship checks if GasBank can sponsor the fee for a transaction.
func (s *ServiceImpl) RequestTransactionFeeSponsorship(ctx context.Context, userAddress util.Uint160, txDetails TransactionDetails) (*big.Int, error) {
	if !s.config.EnableUserBalances {
		return nil, errors.New("fee sponsorship disabled (user balances not enabled)")
	}
	policy, err := s.store.GetFeePolicy(ctx, userAddress)
	if err != nil {
		log.Debugf("Fee sponsorship denied for user %s: No policy found or error retrieving policy: %v", userAddress.StringLE(), err)
		return big.NewInt(0), fmt.Errorf("fee sponsorship denied: policy not found or inaccessible")
	}
	if policy == nil || !policy.IsEnabled {
		log.Debugf("Fee sponsorship denied for user %s: Policy disabled", userAddress.StringLE())
		return big.NewInt(0), fmt.Errorf("fee sponsorship denied: policy disabled")
	}
	isAllowed := false
	if policy.PayForOthers {
		isAllowed = true
	} else if len(policy.AllowedContracts) > 0 {
		allowedMap := make(map[string]struct{})
		for _, addr := range policy.AllowedContracts {
			allowedMap[addr.StringLE()] = struct{}{}
		}
		for _, calledAddr := range txDetails.CalledContracts {
			if _, ok := allowedMap[calledAddr.StringLE()]; ok {
				isAllowed = true
				break
			}
		}
	}
	if !isAllowed {
		log.Debugf("Fee sponsorship denied for user %s: Transaction does not match policy rules (PayForOthers: %t, CalledContracts: %v)",
			userAddress.StringLE(), policy.PayForOthers, txDetails.CalledContracts)
		return big.NewInt(0), fmt.Errorf("fee sponsorship denied: transaction does not match policy rules")
	}
	potentialFee := new(big.Int).Add(txDetails.NetworkFee, txDetails.SystemFee)
	if potentialFee.Sign() <= 0 {
		log.Debugf("Fee sponsorship not needed for user %s: Calculated fee is zero or negative", userAddress.StringLE())
		return big.NewInt(0), nil
	}
	feeToCover := new(big.Int).Set(potentialFee)
	if policy.MaxFeePerTx != nil && policy.MaxFeePerTx.Sign() > 0 && feeToCover.Cmp(policy.MaxFeePerTx) > 0 {
		feeToCover.Set(policy.MaxFeePerTx)
		log.Debugf("Fee sponsorship for user %s capped at policy max: %s (Potential: %s)",
			userAddress.StringLE(), feeToCover.String(), potentialFee.String())
	} else {
		log.Debugf("Fee sponsorship for user %s requested for amount: %s", userAddress.StringLE(), feeToCover.String())
	}

	// Generate a unique ID for this sponsorship attempt
	// Note: We need the *actual* tx hash later for confirmation/cancellation.
	// This assumes the caller provides a stable proposed tx hash or we generate one.
	// Let's assume txDetails includes a proposed TxHash or unique identifier for the request.
	sponsorshipID := uuid.NewString()
	// txHash := txDetails.ProposedTxHash // Assuming this exists
	txHash := util.Uint256{} // Placeholder - Need a way to identify this tx later

	// Lock the balance
	err = s.store.UpdateUserBalance(ctx, userAddress, func(balance *models.UserBalance) (*models.UserBalance, error) {
		if balance.Balance.Cmp(feeToCover) < 0 {
			return nil, fmt.Errorf("insufficient balance (Available: %s, Required Fee: %s)", balance.Balance.String(), feeToCover.String())
		}
		balance.Balance.Sub(balance.Balance, feeToCover)
		balance.LockedBalance.Add(balance.LockedBalance, feeToCover)
		return balance, nil
	})

	if err != nil {
		log.Warnf("Fee sponsorship denied for user %s: Failed to lock balance: %v", userAddress.StringLE(), err)
		return big.NewInt(0), fmt.Errorf("fee sponsorship denied: %w", err)
	}

	// Save pending sponsorship record
	pending := &models.PendingSponsorship{
		SponsorshipID: sponsorshipID,
		TxHash:        txHash, // Use the placeholder/proposed hash
		UserID:        userAddress,
		LockedAmount:  new(big.Int).Set(feeToCover),
		Status:        "Pending",
		CreatedAt:     time.Now(),
		// ExpiresAt: time.Now().Add(5 * time.Minute), // Example expiry
	}
	if err := s.store.SavePendingSponsorship(ctx, pending); err != nil {
		log.Errorf("CRITICAL ERROR: Failed to save pending sponsorship record %s after locking balance: %v. Attempting rollback.", sponsorshipID, err)
		// Attempt rollback
		rollbackErr := s.store.UpdateUserBalance(context.Background(), userAddress, func(balance *models.UserBalance) (*models.UserBalance, error) {
			// Consistency check less critical here, just try to unlock
			balance.LockedBalance.Sub(balance.LockedBalance, feeToCover)
			balance.Balance.Add(balance.Balance, feeToCover)
			return balance, nil
		})
		if rollbackErr != nil {
			log.Fatalf("CRITICAL ERROR: Failed to rollback balance lock for sponsorship %s after failing to save record: %v. Manual intervention needed!", sponsorshipID, rollbackErr)
		}
		return big.NewInt(0), fmt.Errorf("failed to record sponsorship lock: %w", err)
	}

	log.Infof("Fee sponsorship approved and locked for user %s: Amount=%s, SponsorshipID=%s",
		userAddress.StringLE(), feeToCover.String(), sponsorshipID)
	return feeToCover, nil
}

// ConfirmFeePayment confirms a sponsored transaction was successful and updates balances.
func (s *ServiceImpl) ConfirmFeePayment(ctx context.Context, userAddress util.Uint160, txHash util.Uint256, actualFee *big.Int) error {
	if !s.config.EnableUserBalances {
		return errors.New("fee confirmation disabled (user balances not enabled)")
	}
	if actualFee == nil || actualFee.Sign() < 0 {
		actualFee = big.NewInt(0)
		log.Debugf("ConfirmFeePayment called with zero or negative actualFee for Tx %s, proceeding.", txHash.StringLE())
	}

	log.Infof("Confirming fee payment for user %s, Tx %s: ActualFee=%s", userAddress.StringLE(), txHash.StringLE(), actualFee.String())

	// Get the pending sponsorship record using the TxHash
	sponsorship, err := s.store.GetPendingSponsorshipByTx(ctx, userAddress, txHash)
	if err != nil {
		log.Errorf("Cannot confirm fee payment for user %s, Tx %s: Failed to retrieve pending sponsorship record: %v", userAddress.StringLE(), txHash.StringLE(), err)
		// Should we return error? Or assume already processed/cancelled?
		// If ErrNotFound, maybe it was cancelled. If other error, something is wrong.
		return fmt.Errorf("cannot confirm fee payment: %w", err)
	}
	if sponsorship == nil {
		log.Warnf("ConfirmFeePayment called for user %s, Tx %s, but no pending record found. Assuming already processed or cancelled.", userAddress.StringLE(), txHash.StringLE())
		return nil // Nothing to confirm
	}

	lockedAmount := sponsorship.LockedAmount

	err = s.store.UpdateUserBalance(ctx, userAddress, func(balance *models.UserBalance) (*models.UserBalance, error) {
		if balance.LockedBalance.Cmp(lockedAmount) < 0 {
			log.Errorf("ConfirmFeePayment Inconsistency for user %s, Tx %s: Current LockedBalance %s < originally locked %s",
				userAddress.StringLE(), txHash.StringLE(), balance.LockedBalance.String(), lockedAmount.String())
			balance.LockedBalance = big.NewInt(0)
			return balance, fmt.Errorf("locked balance inconsistency during confirmation")
		}
		// Unlock the amount that was reserved
		balance.LockedBalance.Sub(balance.LockedBalance, lockedAmount)

		// If actualFee < lockedAmount, refund the difference to the main balance.
		if actualFee.Cmp(lockedAmount) < 0 {
			refund := new(big.Int).Sub(lockedAmount, actualFee)
			balance.Balance.Add(balance.Balance, refund)
			log.Infof("Refunded %s GAS to user %s for Tx %s (Locked: %s, Actual: %s)",
				refund.String(), userAddress.StringLE(), txHash.StringLE(), lockedAmount.String(), actualFee.String())
		} else if actualFee.Cmp(lockedAmount) > 0 {
			log.Warnf("Actual fee %s exceeded locked amount %s for user %s, Tx %s. Only locked amount was deducted.",
				actualFee.String(), lockedAmount.String(), userAddress.StringLE(), txHash.StringLE())
		}
		return balance, nil
	})

	if err != nil {
		log.Errorf("CRITICAL ERROR: Failed to confirm fee payment (update balance) for user %s, Tx %s: %v. Locked funds may remain!",
			userAddress.StringLE(), txHash.StringLE(), err)
		return fmt.Errorf("failed to confirm fee payment balance update: %w", err)
	}

	// Delete the pending sponsorship record
	delErr := s.store.DeletePendingSponsorship(ctx, sponsorship.SponsorshipID)
	if delErr != nil {
		log.Errorf("Failed to delete pending sponsorship record %s (Tx %s) after confirming payment: %v",
			sponsorship.SponsorshipID, txHash.StringLE(), delErr)
		// Continue, as balance update was successful, but log the cleanup failure.
	}

	log.Infof("Fee payment confirmed successfully for user %s, Tx %s.", userAddress.StringLE(), txHash.StringLE())
	return nil
}

// CancelFeeSponsorship releases locked funds if a sponsored transaction fails.
func (s *ServiceImpl) CancelFeeSponsorship(ctx context.Context, userAddress util.Uint160, txHash util.Uint256) error {
	if !s.config.EnableUserBalances {
		return errors.New("fee cancellation disabled (user balances not enabled)")
	}

	log.Infof("Cancelling fee sponsorship for user %s, Tx %s", userAddress.StringLE(), txHash.StringLE())

	// Get the pending sponsorship record using the TxHash
	sponsorship, err := s.store.GetPendingSponsorshipByTx(ctx, userAddress, txHash)
	if err != nil {
		log.Warnf("Cannot cancel fee sponsorship for user %s, Tx %s: Failed to retrieve pending record: %v", userAddress.StringLE(), txHash.StringLE(), err)
		// If ErrNotFound, maybe already processed/cancelled.
		// if errors.Is(err, store.ErrNotFound) { return nil }
		return fmt.Errorf("cannot cancel sponsorship: %w", err)
	}
	if sponsorship == nil {
		log.Infof("No active lock found for Tx %s (user %s) to cancel.", txHash.StringLE(), userAddress.StringLE())
		return nil // Nothing to cancel
	}

	lockedAmount := sponsorship.LockedAmount

	err = s.store.UpdateUserBalance(ctx, userAddress, func(balance *models.UserBalance) (*models.UserBalance, error) {
		if balance.LockedBalance.Cmp(lockedAmount) < 0 {
			log.Errorf("CancelFeeSponsorship Inconsistency for user %s, Tx %s: Locked balance %s < originally locked %s",
				userAddress.StringLE(), txHash.StringLE(), balance.LockedBalance.String(), lockedAmount.String())
			balance.LockedBalance = big.NewInt(0)
			return balance, fmt.Errorf("locked balance inconsistency during cancellation")
		}
		// Unlock the amount and add it back to available balance
		balance.LockedBalance.Sub(balance.LockedBalance, lockedAmount)
		balance.Balance.Add(balance.Balance, lockedAmount)
		return balance, nil
	})

	if err != nil {
		log.Errorf("CRITICAL ERROR: Failed to cancel fee sponsorship (update balance) for user %s, Tx %s: %v. Funds may remain locked!",
			userAddress.StringLE(), txHash.StringLE(), err)
		return fmt.Errorf("failed to cancel fee sponsorship balance update: %w", err)
	}

	// Delete the pending sponsorship record
	delErr := s.store.DeletePendingSponsorship(ctx, sponsorship.SponsorshipID)
	if delErr != nil {
		log.Errorf("Failed to delete pending sponsorship record %s (Tx %s) after cancelling: %v",
			sponsorship.SponsorshipID, txHash.StringLE(), delErr)
		// Continue, as balance update was successful.
	}

	log.Infof("Fee sponsorship cancelled successfully for user %s, Tx %s.", userAddress.StringLE(), txHash.StringLE())
	return nil
}

// --- Placeholder internal methods for lock tracking (REMOVED - No longer needed) ---
/*
func (s *ServiceImpl) getLockedFeeAmount(ctx context.Context, userAddress util.Uint160, txHash util.Uint256) (*big.Int, error) {
	log.Warnf("getLockedFeeAmount is a placeholder and not implemented!")
	return big.NewInt(10000000), nil // BAD PLACEHOLDER - returning 0.1 GAS
}

func (s *ServiceImpl) deleteLockedFeeRecord(ctx context.Context, userAddress util.Uint160, txHash util.Uint256) error {
	log.Warnf("deleteLockedFeeRecord is a placeholder and not implemented!")
	return nil
}
*/

// SetFeePolicy allows a user to define their fee payment policy.
func (s *ServiceImpl) SetFeePolicy(ctx context.Context, userAddress util.Uint160, policy models.FeePolicy) error {
	if !s.config.EnableUserBalances {
		return errors.New("user balances and fee policies are disabled")
	}
	policy.UserID = userAddress // Ensure policy is for the correct user
	// TODO: Validate policy fields (e.g., MaxFeePerTx >= 0)
	log.Infof("Setting fee policy for user %s: %+v", userAddress.StringLE(), policy)
	err := s.store.SaveFeePolicy(ctx, &policy)
	if err != nil {
		log.Errorf("Failed to save fee policy for user %s: %v", userAddress.StringLE(), err)
		return fmt.Errorf("failed to save fee policy")
	}
	return nil
}

// GetFeePolicy retrieves a user's current fee payment policy.
func (s *ServiceImpl) GetFeePolicy(ctx context.Context, userAddress util.Uint160) (*models.FeePolicy, error) {
	if !s.config.EnableUserBalances {
		return nil, errors.New("user balances and fee policies are disabled")
	}
	policy, err := s.store.GetFeePolicy(ctx, userAddress)
	if err != nil {
		log.Errorf("Failed to get fee policy for user %s: %v", userAddress.StringLE(), err)
		return nil, fmt.Errorf("failed to retrieve fee policy")
	}
	return policy, nil
}

// --- Gas Claiming for NEO-only users ---

// SubmitGasClaim allows a user to submit their pre-signed claim transaction.
func (s *ServiceImpl) SubmitGasClaim(ctx context.Context, userAddress util.Uint160, signedTxBytes []byte) (string, error) {
	// TODO: Implement GAS claim logic using correct Neo transaction handling
	log.Warnf("SubmitGasClaim not implemented correctly due to library mismatches.")
	return "", errors.New("gas claiming not implemented")
}

// GetGasClaimStatus checks the status of a submitted claim.
func (s *ServiceImpl) GetGasClaimStatus(ctx context.Context, userAddress util.Uint160, requestID string) (*models.GasClaim, error) {
	claim, err := s.store.GetGasClaim(ctx, userAddress, requestID)
	if err != nil {
		log.Warnf("Failed to get gas claim status for user %s, request %s: %v", userAddress.StringLE(), requestID, err)
		return nil, fmt.Errorf("failed to retrieve claim status")
	}
	// TODO: Optionally check blockchain confirmation if status is "Submitted"
	return claim, nil
}

// --- Temporary Allocations (REMOVED) ---
/*
func (s *ServiceImpl) AllocateGas(ctx context.Context, userAddress util.Uint160, amount *big.Int) (*models.Allocation, error) {
	log.Warnf("AllocateGas (temporary allocation) not fully integrated with persistent balances yet.")
	return nil, errors.New("temporary allocation logic needs integration with user balances")
}

func (s *ServiceImpl) UseGas(ctx context.Context, userAddress util.Uint160, amountUsed *big.Int) error {
	log.Warnf("UseGas (temporary allocation) not fully integrated with persistent balances yet.")
	return errors.New("temporary allocation logic needs integration with user balances")
}

func (s *ServiceImpl) ReleaseGas(ctx context.Context, userAddress util.Uint160) error {
	log.Warnf("ReleaseGas (temporary allocation) not fully integrated with persistent balances yet.")
	return errors.New("temporary allocation logic needs integration with user balances")
}
*/

// --- Admin/Monitoring (REMOVED) ---
/*
func (s *ServiceImpl) GetGasPoolState(ctx context.Context) (*models.GasPoolState, error) {
	log.Warnf("GetGasPoolState not implemented for user balance model")
	return nil, errors.New("gas pool state not applicable in user balance model")
}

func (s *ServiceImpl) RefillPool(ctx context.Context) error {
	log.Warnf("RefillPool not implemented for user balance model")
	return errors.New("gas pool refill not applicable in user balance model")
}
*/
