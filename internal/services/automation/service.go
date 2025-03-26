package automation

import (
	"context"
	"fmt"
	"math/big"
	"time"

	"github.com/google/uuid"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/core/neo"
	"github.com/will/neo_service_layer/internal/services/gasbank"
)

// Config represents the automation service configuration
type Config struct {
	CheckInterval  time.Duration
	RetryAttempts  int
	RetryDelay     time.Duration
	GasBuffer      *big.Int
	KeeperRegistry util.Uint160
}

// Service represents the automation service
type Service struct {
	store          upkeepStore
	checker        upkeepChecker
	performer      upkeepPerformer
	scheduler      upkeepScheduler
	policy         *UpkeepPolicy
	neoClient      *neo.Client
	gasBankService gasbank.IService
}

// upkeepStore is an interface for storing upkeeps
type upkeepStore interface {
	SaveUpkeep(ctx context.Context, upkeep *Upkeep) error
	GetUpkeep(ctx context.Context, upkeepID string) (*Upkeep, error)
	ListUpkeeps(ctx context.Context, userAddress util.Uint160) ([]*Upkeep, error)
	ListEligibleUpkeeps(ctx context.Context) ([]*Upkeep, error)
	SavePerformance(ctx context.Context, performance *UpkeepPerformance) error
	GetPerformances(ctx context.Context, upkeepID string) ([]*UpkeepPerformance, error)
}

// upkeepChecker is an interface for checking upkeep eligibility
type upkeepChecker interface {
	CheckUpkeep(ctx context.Context, upkeep *Upkeep) (*UpkeepCheck, error)
}

// upkeepPerformer is an interface for performing upkeeps
type upkeepPerformer interface {
	PerformUpkeep(ctx context.Context, upkeep *Upkeep, performData []byte) (*UpkeepPerformance, error)
}

// upkeepScheduler is an interface for scheduling upkeep checks
type upkeepScheduler interface {
	ScheduleUpkeep(ctx context.Context, upkeep *Upkeep) error
	CancelUpkeep(ctx context.Context, upkeepID string) error
	Start(ctx context.Context) error
	Stop(ctx context.Context) error
}

// mockStore implements the upkeepStore interface for testing
type mockStore struct {
	upkeeps      map[string]*Upkeep
	performances map[string][]*UpkeepPerformance
}

// mockChecker implements the upkeepChecker interface for testing
type mockChecker struct{}

// mockPerformer implements the upkeepPerformer interface for testing
type mockPerformer struct{}

// mockScheduler implements the upkeepScheduler interface for testing
type mockScheduler struct{}

// NewService creates a new automation service
func NewService(config *Config, neoClient *neo.Client, gasBankService gasbank.IService) (*Service, error) {
	store := &mockStore{
		upkeeps:      make(map[string]*Upkeep),
		performances: make(map[string][]*UpkeepPerformance),
	}
	checker := &mockChecker{}
	performer := &mockPerformer{}
	scheduler := &mockScheduler{}

	policy := &UpkeepPolicy{
		MaxUpkeepsPerUser:    50,
		MinInterval:          time.Minute,
		MaxGasLimit:          100000,
		MinBalance:           big.NewInt(10000),
		CheckGracePeriod:     time.Second * 30,
		MaxPerformDataSize:   1024, // 1 KB
		MaxConsecutiveErrors: 5,
	}

	return &Service{
		store:          store,
		checker:        checker,
		performer:      performer,
		scheduler:      scheduler,
		policy:         policy,
		neoClient:      neoClient,
		gasBankService: gasBankService,
	}, nil
}

// RegisterUpkeep registers a new upkeep
func (s *Service) RegisterUpkeep(ctx context.Context, userAddress util.Uint160, upkeep *Upkeep) (bool, error) {
	// Set upkeep defaults
	upkeep.ID = uuid.New().String()
	upkeep.Owner = userAddress
	upkeep.CreatedAt = time.Now()
	upkeep.NextEligibleAt = time.Now().Add(time.Minute) // Eligible after 1 minute
	if upkeep.Status == "" {
		upkeep.Status = "active"
	}

	// Save the upkeep
	if err := s.store.SaveUpkeep(ctx, upkeep); err != nil {
		return false, err
	}

	// Schedule the upkeep
	if err := s.scheduler.ScheduleUpkeep(ctx, upkeep); err != nil {
		return false, err
	}

	return true, nil
}

// CancelUpkeep cancels an upkeep
func (s *Service) CancelUpkeep(ctx context.Context, userAddress util.Uint160, upkeepID string) (bool, error) {
	// Get the upkeep
	upkeep, err := s.store.GetUpkeep(ctx, upkeepID)
	if err != nil {
		return false, err
	}

	if upkeep == nil {
		return false, nil
	}

	// Check ownership
	if upkeep.Owner != userAddress {
		return false, nil
	}

	// Cancel the upkeep
	upkeep.Status = "cancelled"
	if err := s.store.SaveUpkeep(ctx, upkeep); err != nil {
		return false, err
	}

	// Cancel scheduling
	if err := s.scheduler.CancelUpkeep(ctx, upkeepID); err != nil {
		return false, err
	}

	return true, nil
}

// PauseUpkeep pauses an upkeep
func (s *Service) PauseUpkeep(ctx context.Context, userAddress util.Uint160, upkeepID string) (bool, error) {
	// Get the upkeep
	upkeep, err := s.store.GetUpkeep(ctx, upkeepID)
	if err != nil {
		return false, err
	}

	if upkeep == nil {
		return false, nil
	}

	// Check ownership
	if upkeep.Owner != userAddress {
		return false, nil
	}

	// Pause the upkeep
	upkeep.Status = "paused"
	if err := s.store.SaveUpkeep(ctx, upkeep); err != nil {
		return false, err
	}

	return true, nil
}

// ResumeUpkeep resumes a paused upkeep
func (s *Service) ResumeUpkeep(ctx context.Context, userAddress util.Uint160, upkeepID string) (bool, error) {
	// Get the upkeep
	upkeep, err := s.store.GetUpkeep(ctx, upkeepID)
	if err != nil {
		return false, err
	}

	if upkeep == nil {
		return false, nil
	}

	// Check ownership
	if upkeep.Owner != userAddress {
		return false, nil
	}

	// Check if the upkeep is paused
	if upkeep.Status != "paused" {
		return false, nil
	}

	// Resume the upkeep
	upkeep.Status = "active"
	if err := s.store.SaveUpkeep(ctx, upkeep); err != nil {
		return false, err
	}

	// Reschedule the upkeep
	if err := s.scheduler.ScheduleUpkeep(ctx, upkeep); err != nil {
		return false, err
	}

	return true, nil
}

// GetUpkeep gets an upkeep by ID
func (s *Service) GetUpkeep(ctx context.Context, upkeepID string) (*Upkeep, error) {
	return s.store.GetUpkeep(ctx, upkeepID)
}

// ListUpkeeps lists upkeeps for a user
func (s *Service) ListUpkeeps(ctx context.Context, userAddress util.Uint160) ([]*Upkeep, error) {
	return s.store.ListUpkeeps(ctx, userAddress)
}

// CheckUpkeep checks if an upkeep is eligible for execution
func (s *Service) CheckUpkeep(ctx context.Context, upkeepID string) (*UpkeepCheck, error) {
	// Get the upkeep
	upkeep, err := s.store.GetUpkeep(ctx, upkeepID)
	if err != nil {
		return nil, err
	}

	if upkeep == nil {
		return nil, nil
	}

	// Check eligibility
	return s.checker.CheckUpkeep(ctx, upkeep)
}

// PerformUpkeep performs an upkeep execution
func (s *Service) PerformUpkeep(ctx context.Context, upkeepID string, performData []byte) (*UpkeepPerformance, error) {
	// Get the upkeep
	upkeep, err := s.store.GetUpkeep(ctx, upkeepID)
	if err != nil {
		return nil, err
	}

	if upkeep == nil {
		return nil, fmt.Errorf("upkeep not found: %s", upkeepID)
	}

	// Check if upkeep is active
	if upkeep.Status != "active" {
		return nil, fmt.Errorf("upkeep is not active: %s", upkeep.Status)
	}

	// Check if upkeep is eligible
	check, err := s.checker.CheckUpkeep(ctx, upkeep)
	if err != nil {
		return nil, fmt.Errorf("failed to check upkeep eligibility: %w", err)
	}

	if !check.Eligible {
		return nil, fmt.Errorf("upkeep is not eligible: %s", check.EligibilityError)
	}

	// Perform the upkeep
	performance, err := s.performer.PerformUpkeep(ctx, upkeep, performData)
	if err != nil {
		return nil, fmt.Errorf("failed to perform upkeep: %w", err)
	}

	// Save the performance record
	if err := s.store.SavePerformance(ctx, performance); err != nil {
		return nil, fmt.Errorf("failed to save performance record: %w", err)
	}

	// Update upkeep's last run time
	upkeep.LastRunAt = performance.StartTime
	if err := s.store.SaveUpkeep(ctx, upkeep); err != nil {
		return nil, fmt.Errorf("failed to update upkeep: %w", err)
	}

	return performance, nil
}

// GetUpkeepPerformance gets the performance history for an upkeep
func (s *Service) GetUpkeepPerformance(ctx context.Context, upkeepID string) ([]*UpkeepPerformance, error) {
	// Get the upkeep
	upkeep, err := s.store.GetUpkeep(ctx, upkeepID)
	if err != nil {
		return nil, err
	}

	if upkeep == nil {
		return nil, fmt.Errorf("upkeep not found: %s", upkeepID)
	}

	// Get performance records
	performances, err := s.store.GetPerformances(ctx, upkeepID)
	if err != nil {
		return nil, fmt.Errorf("failed to get performance records: %w", err)
	}

	return performances, nil
}

// Start starts the automation service
func (s *Service) Start(ctx context.Context) error {
	// Start the scheduler
	if err := s.scheduler.Start(ctx); err != nil {
		return fmt.Errorf("failed to start scheduler: %w", err)
	}

	// Load all active upkeeps and schedule them
	upkeeps, err := s.store.ListEligibleUpkeeps(ctx)
	if err != nil {
		return fmt.Errorf("failed to list eligible upkeeps: %w", err)
	}

	for _, upkeep := range upkeeps {
		if err := s.scheduler.ScheduleUpkeep(ctx, upkeep); err != nil {
			return fmt.Errorf("failed to schedule upkeep %s: %w", upkeep.ID, err)
		}
	}

	return nil
}

// Stop stops the automation service
func (s *Service) Stop(ctx context.Context) error {
	// Stop the scheduler
	if err := s.scheduler.Stop(ctx); err != nil {
		return fmt.Errorf("failed to stop scheduler: %w", err)
	}

	return nil
}

// Mock implementations for interfaces

// SaveUpkeep saves an upkeep to the mock store
func (m *mockStore) SaveUpkeep(ctx context.Context, upkeep *Upkeep) error {
	m.upkeeps[upkeep.ID] = upkeep
	return nil
}

// GetUpkeep gets an upkeep from the mock store
func (m *mockStore) GetUpkeep(ctx context.Context, upkeepID string) (*Upkeep, error) {
	upkeep, exists := m.upkeeps[upkeepID]
	if !exists {
		return nil, nil
	}
	return upkeep, nil
}

// ListUpkeeps lists upkeeps for a user from the mock store
func (m *mockStore) ListUpkeeps(ctx context.Context, userAddress util.Uint160) ([]*Upkeep, error) {
	var upkeeps []*Upkeep
	for _, upkeep := range m.upkeeps {
		if upkeep.Owner == userAddress {
			upkeeps = append(upkeeps, upkeep)
		}
	}
	return upkeeps, nil
}

// ListEligibleUpkeeps lists all eligible upkeeps from the mock store
func (m *mockStore) ListEligibleUpkeeps(ctx context.Context) ([]*Upkeep, error) {
	var upkeeps []*Upkeep
	now := time.Now()
	for _, upkeep := range m.upkeeps {
		if upkeep.Status == "active" && now.After(upkeep.NextEligibleAt) {
			upkeeps = append(upkeeps, upkeep)
		}
	}
	return upkeeps, nil
}

// SavePerformance saves a performance record to the mock store
func (m *mockStore) SavePerformance(ctx context.Context, performance *UpkeepPerformance) error {
	if m.performances[performance.UpkeepID] == nil {
		m.performances[performance.UpkeepID] = []*UpkeepPerformance{}
	}
	m.performances[performance.UpkeepID] = append(m.performances[performance.UpkeepID], performance)
	return nil
}

// GetPerformances gets performance records from the mock store
func (m *mockStore) GetPerformances(ctx context.Context, upkeepID string) ([]*UpkeepPerformance, error) {
	performances, exists := m.performances[upkeepID]
	if !exists {
		return []*UpkeepPerformance{}, nil
	}
	return performances, nil
}

func (m *mockChecker) CheckUpkeep(ctx context.Context, upkeep *Upkeep) (*UpkeepCheck, error) {
	// For testing, always return eligible
	return &UpkeepCheck{
		UpkeepID:      upkeep.ID,
		CheckTime:     time.Now(),
		Eligible:      true,
		PerformData:   upkeep.CheckData,
		GasEstimation: upkeep.ExecuteGas,
	}, nil
}

func (m *mockPerformer) PerformUpkeep(ctx context.Context, upkeep *Upkeep, performData []byte) (*UpkeepPerformance, error) {
	now := time.Now()
	return &UpkeepPerformance{
		ID:              uuid.New().String(),
		UpkeepID:        upkeep.ID,
		StartTime:       now,
		EndTime:         now.Add(time.Second),
		Status:          "success",
		GasUsed:         upkeep.ExecuteGas,
		BlockNumber:     1000,
		TransactionHash: util.Uint256{1, 2, 3},
		Result:          "Executed successfully",
	}, nil
}

func (m *mockScheduler) ScheduleUpkeep(ctx context.Context, upkeep *Upkeep) error {
	return nil
}

func (m *mockScheduler) CancelUpkeep(ctx context.Context, upkeepID string) error {
	return nil
}

func (m *mockScheduler) Start(ctx context.Context) error {
	return nil
}

func (m *mockScheduler) Stop(ctx context.Context) error {
	return nil
}
