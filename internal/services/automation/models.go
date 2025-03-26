package automation

import (
	"time"

	"math/big"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// Upkeep represents a contract automation upkeep
type Upkeep struct {
	ID             string
	Name           string
	Owner          util.Uint160
	TargetContract util.Uint160
	ExecuteGas     int64
	CheckData      []byte
	UpkeepFunction string
	CreatedAt      time.Time
	LastRunAt      time.Time
	NextEligibleAt time.Time
	Status         string // "active", "paused", "cancelled"
	OffchainConfig map[string]interface{}
}

// UpkeepPerformance represents a record of upkeep execution
type UpkeepPerformance struct {
	ID              string
	UpkeepID        string
	StartTime       time.Time
	EndTime         time.Time
	Status          string // "success", "failed", "cancelled"
	GasUsed         int64
	BlockNumber     uint32
	TransactionHash util.Uint256
	Result          string
	Error           string
}

// UpkeepCheck represents the result of a check for upkeep eligibility
type UpkeepCheck struct {
	UpkeepID         string
	CheckTime        time.Time
	Eligible         bool
	PerformData      []byte
	GasEstimation    int64
	EligibilityError string
}

// UpkeepPolicy represents the policy for upkeeps
type UpkeepPolicy struct {
	MaxUpkeepsPerUser    int
	MinInterval          time.Duration
	MaxGasLimit          int64
	MinBalance           *big.Int
	CheckGracePeriod     time.Duration
	MaxPerformDataSize   int
	MaxConsecutiveErrors int
}
