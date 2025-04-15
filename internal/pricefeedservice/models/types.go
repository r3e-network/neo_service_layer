package models

import (
	"math/big"
	"time"
)

// PricePoint represents a single price data point
type PricePoint struct {
	Symbol    string
	Price     *big.Int
	Timestamp time.Time
	Source    string
	Weight    float64
}

// PriceFeed represents a configured price feed
type PriceFeed struct {
	Symbol     string
	Sources    []DataSource
	Heartbeat  time.Duration
	Deviation  float64
	Decimals   uint8
	LastUpdate time.Time
}

// DataSource represents a price data source configuration
type DataSource struct {
	Name      string
	Weight    float64
	Endpoint  string
	APIKey    string
	APISecret string
}

// UpdateRule represents price feed update rules
type UpdateRule struct {
	MinSources      int
	UpdateThreshold float64
	MaxDelay        time.Duration
	GasLimit        uint64
}

// PriceUpdate represents a price update to be published on-chain
type PriceUpdate struct {
	Symbol       string
	Price        *big.Int
	Timestamp    time.Time
	NumSources   int
	Signatures   [][]byte
	GasEstimate  uint64
	UpdateReason string
}

// SourceStatus represents the current status of a data source
type SourceStatus struct {
	Name          string
	LastUpdate    time.Time
	LastError     error
	ResponseTime  time.Duration
	Reliability   float64
	FailureCount  uint64
	SuccessCount  uint64
}

// Alert represents a price feed alert
type Alert struct {
	Type      AlertType
	Symbol    string
	Message   string
	Timestamp time.Time
	Severity  AlertSeverity
	Data      map[string]interface{}
}

// AlertType represents different types of alerts
type AlertType string

const (
	AlertTypePriceAnomaly    AlertType = "PRICE_ANOMALY"
	AlertTypeSourceFailure   AlertType = "SOURCE_FAILURE"
	AlertTypeNetworkIssue    AlertType = "NETWORK_ISSUE"
	AlertTypeGasIssue        AlertType = "GAS_ISSUE"
	AlertTypeContractError   AlertType = "CONTRACT_ERROR"
	AlertTypeStaleData       AlertType = "STALE_DATA"
	AlertTypeDeviationBreak  AlertType = "DEVIATION_BREAK"
	AlertTypeHeartbeatMissed AlertType = "HEARTBEAT_MISSED"
)

// AlertSeverity represents the severity level of an alert
type AlertSeverity string

const (
	AlertSeverityInfo     AlertSeverity = "INFO"
	AlertSeverityWarning  AlertSeverity = "WARNING"
	AlertSeverityError    AlertSeverity = "ERROR"
	AlertSeverityCritical AlertSeverity = "CRITICAL"
)