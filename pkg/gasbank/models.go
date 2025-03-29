package gasbank

import (
	"math/big"
	"sync"
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// AccountStatus represents the status of a gas bank account
type AccountStatus string

const (
	// AccountStatusActive indicates the account is active and can be used
	AccountStatusActive AccountStatus = "active"
	// AccountStatusLocked indicates the account is locked and cannot be used
	AccountStatusLocked AccountStatus = "locked"
	// AccountStatusSuspended indicates the account is suspended due to violations
	AccountStatusSuspended AccountStatus = "suspended"
)

// Account represents a gas bank account
type Account struct {
	ID            string        `json:"id"`
	Owner         util.Uint160  `json:"owner"`
	Balance       *big.Int      `json:"balance"`
	Status        AccountStatus `json:"status"`
	CreatedAt     time.Time     `json:"created_at"`
	UpdatedAt     time.Time     `json:"updated_at"`
	LastUsedAt    time.Time     `json:"last_used_at"`
	TotalUsed     *big.Int      `json:"total_used"`
	ReservedGas   *big.Int      `json:"reserved_gas"`
	AllowedAmount *big.Int      `json:"allowed_amount"`
}

// ReservationRequest represents a request to reserve gas
type ReservationRequest struct {
	AccountID string        `json:"account_id"`
	Amount    *big.Int      `json:"amount"`
	TTL       time.Duration `json:"ttl"`
}

// ReservationResponse represents the response to a gas reservation request
type ReservationResponse struct {
	ReservationID string    `json:"reservation_id"`
	Amount        *big.Int  `json:"amount"`
	ExpiresAt     time.Time `json:"expires_at"`
}

// ReleaseRequest represents a request to release reserved gas
type ReleaseRequest struct {
	AccountID     string   `json:"account_id"`
	ReservationID string   `json:"reservation_id"`
	AmountUsed    *big.Int `json:"amount_used"`
}

// Service represents the gas bank service
type Service struct {
	config       *Config
	store        Store
	metrics      MetricsCollector
	logger       Logger
	neoClient    NeoClient
	reservations map[string]*Reservation
	mu           sync.RWMutex
}

// Store defines the interface for gas bank storage
type Store interface {
	SaveAccount(account *Account) error
	GetAccount(id string) (*Account, error)
	ListAccounts() ([]*Account, error)
	DeleteAccount(id string) error
	SaveReservation(reservation *Reservation) error
	GetReservation(id string) (*Reservation, error)
	ListReservations() ([]*Reservation, error)
	DeleteReservation(id string) error
}

// MetricsCollector defines the interface for metrics collection
type MetricsCollector interface {
	RecordGasReservation(amount *big.Int)
	RecordGasRelease(amount *big.Int)
	RecordGasUsage(amount *big.Int)
	RecordError(category string, err error)
}

// Logger defines the interface for logging
type Logger interface {
	Info(msg string, fields ...interface{})
	Error(msg string, fields ...interface{})
	Debug(msg string, fields ...interface{})
}

// NeoClient defines the interface for Neo blockchain interaction
type NeoClient interface {
	GetGasBalance(address util.Uint160) (*big.Int, error)
	TransferGas(from, to util.Uint160, amount *big.Int) (string, error)
}

// Reservation represents a gas reservation
type Reservation struct {
	ID         string    `json:"id"`
	AccountID  string    `json:"account_id"`
	Amount     *big.Int  `json:"amount"`
	CreatedAt  time.Time `json:"created_at"`
	ExpiresAt  time.Time `json:"expires_at"`
	Status     string    `json:"status"`
	AmountUsed *big.Int  `json:"amount_used"`
	ReleasedAt time.Time `json:"released_at,omitempty"`
}
