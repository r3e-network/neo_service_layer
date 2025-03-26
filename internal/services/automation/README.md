# Automation Service

The Automation service provides contract automation capabilities for the Neo blockchain, enabling automated and scheduled contract interactions.

## Overview

The Automation service allows users to register upkeeps that automatically maintain smart contracts based on predefined conditions. This service is inspired by keeper networks and enables reliable contract maintenance without manual intervention.

## Configuration

The Automation service is configured using the `Config` struct:

```go
type Config struct {
    CheckInterval  time.Duration   // Interval between upkeep eligibility checks
    RetryAttempts  int             // Number of retry attempts for failed upkeeps
    RetryDelay     time.Duration   // Delay between retry attempts
    GasBuffer      *big.Int        // Additional gas to include as a buffer
    KeeperRegistry util.Uint160    // Address of the keeper registry contract
}
```

## Core Components

### Upkeep

An `Upkeep` represents a contract automation task:

```go
type Upkeep struct {
    ID             string                // Unique identifier
    Name           string                // Human-readable name
    Owner          util.Uint160          // Address of the upkeep owner
    TargetContract util.Uint160          // Address of the contract to maintain
    ExecuteGas     int64                 // Gas limit for execution
    CheckData      []byte                // Data for eligibility check
    UpkeepFunction string                // Function to call on the target contract
    CreatedAt      time.Time             // Creation timestamp
    LastRunAt      time.Time             // Last execution timestamp
    NextEligibleAt time.Time             // Next eligibility timestamp
    Status         string                // "active", "paused", "cancelled"
    OffchainConfig map[string]interface{} // Additional configuration
}
```

### Upkeep Performance

An `UpkeepPerformance` records the execution of an upkeep:

```go
type UpkeepPerformance struct {
    ID              string         // Unique identifier
    UpkeepID        string         // ID of the executed upkeep
    StartTime       time.Time      // Start timestamp
    EndTime         time.Time      // End timestamp
    Status          string         // "success", "failed", "cancelled"
    GasUsed         int64          // Amount of gas used
    BlockNumber     uint32         // Block number of execution
    TransactionHash util.Uint256   // Hash of the transaction
    Result          string         // Result of the execution
    Error           string         // Error message if failed
}
```

## Key Operations

### Registering Upkeeps

Upkeeps are registered with the `RegisterUpkeep` method:

```go
func (s *Service) RegisterUpkeep(ctx context.Context, userAddress util.Uint160, upkeep *Upkeep) (bool, error)
```

### Managing Upkeeps

Upkeeps can be canceled, paused, and resumed:

```go
func (s *Service) CancelUpkeep(ctx context.Context, userAddress util.Uint160, upkeepID string) (bool, error)
func (s *Service) PauseUpkeep(ctx context.Context, userAddress util.Uint160, upkeepID string) (bool, error)
func (s *Service) ResumeUpkeep(ctx context.Context, userAddress util.Uint160, upkeepID string) (bool, error)
```

### Checking and Performing Upkeeps

Upkeeps can be checked for eligibility and manually performed:

```go
func (s *Service) CheckUpkeep(ctx context.Context, upkeepID string) (*UpkeepCheck, error)
func (s *Service) PerformUpkeep(ctx context.Context, upkeepID string, performData []byte) (*UpkeepPerformance, error)
```

### Retrieving Upkeep Information

Upkeep information can be retrieved:

```go
func (s *Service) GetUpkeep(ctx context.Context, upkeepID string) (*Upkeep, error)
func (s *Service) ListUpkeeps(ctx context.Context, userAddress util.Uint160) ([]*Upkeep, error)
func (s *Service) GetUpkeepPerformance(ctx context.Context, upkeepID string) ([]*UpkeepPerformance, error)
```

## Service Lifecycle

The Automation service can be started and stopped:

```go
func (s *Service) Start(ctx context.Context) error
func (s *Service) Stop(ctx context.Context) error
```

## Integration

The Automation service integrates with:

- Gas Bank Service: For paying for upkeep execution
- Trigger Service: For trigger-based upkeep execution
- Functions Service: For custom eligibility checks

## Use Cases

- Liquidation protection for DeFi positions
- Token rebasing operations
- Interest accrual and distribution
- Regular state updates for oracles
- Time-based contract maintenance

## Future Improvements

- Decentralized keeper network
- Economic incentives for keepers
- Custom eligibility checks using Functions service
- Advanced scheduling and prioritization
- Enhanced monitoring and alerting