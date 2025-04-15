package pricefeed

import (
	"fmt"
	"math/big"
	"time"
)

// AssetPair represents a trading pair
type AssetPair struct {
	Base  string `json:"base"`
	Quote string `json:"quote"`
}

// PriceData represents price data for an asset pair
type PriceData struct {
	Base       string    `json:"base"`
	Quote      string    `json:"quote"`
	Price      float64   `json:"price"`
	Timestamp  time.Time `json:"timestamp"`
	Source     string    `json:"source"`
	Confidence float64   `json:"confidence"`
}

// RateLimitError represents a rate limit error from a price provider
type RateLimitError struct {
	Provider  string
	WaitTime  time.Duration
	Threshold int
	Window    time.Duration
}

// Error implements the error interface
func (e *RateLimitError) Error() string {
	return fmt.Sprintf("rate limit exceeded for %s provider: %d requests per %v, wait %v",
		e.Provider, e.Threshold, e.Window, e.WaitTime)
}

// PriceProvider defines the interface for price data providers
type PriceProvider interface {
	// GetName returns the name of the provider
	GetName() string

	// GetPricePair gets the price for a given asset pair
	GetPricePair(ctx Context, base, quote string) (PriceData, error)

	// GetSupportedPairs returns the list of supported trading pairs
	GetSupportedPairs(ctx Context) ([]AssetPair, error)

	// ClearCache clears the price cache
	ClearCache()
}

// Context is an alias for context.Context
type Context = interface {
	Deadline() (deadline time.Time, ok bool)
	Done() <-chan struct{}
	Err() error
	Value(key interface{}) interface{}
}

// PriceAggregate represents an aggregated price from multiple sources
type PriceAggregate struct {
	Base       string      `json:"base"`
	Quote      string      `json:"quote"`
	Price      float64     `json:"price"`
	Timestamp  time.Time   `json:"timestamp"`
	Sources    []string    `json:"sources"`
	SourceData []PriceData `json:"source_data"`
	Confidence float64     `json:"confidence"`
}

// PriceAggregationMethod defines the method for aggregating prices
type PriceAggregationMethod string

const (
	// PriceAggregationMethodMean is the mean aggregation method
	PriceAggregationMethodMean PriceAggregationMethod = "mean"

	// PriceAggregationMethodMedian is the median aggregation method
	PriceAggregationMethodMedian PriceAggregationMethod = "median"

	// PriceAggregationMethodWeighted is the weighted mean aggregation method
	PriceAggregationMethodWeighted PriceAggregationMethod = "weighted"

	// PriceAggregationMethodTrimmedMean is the trimmed mean aggregation method
	PriceAggregationMethodTrimmedMean PriceAggregationMethod = "trimmed_mean"
)

// PriceConfig represents the configuration for a price feed
type PriceConfig struct {
	Base                 string                 `json:"base"`
	Quote                string                 `json:"quote"`
	Providers            []string               `json:"providers"`
	AggregationMethod    PriceAggregationMethod `json:"aggregation_method"`
	MinProviders         int                    `json:"min_providers"`
	UpdateInterval       time.Duration          `json:"update_interval"`
	MaxStaleData         time.Duration          `json:"max_stale_data"`
	ConfidenceThreshold  float64                `json:"confidence_threshold"`
	TrimPercentage       float64                `json:"trim_percentage,omitempty"`
	ProviderWeights      map[string]float64     `json:"provider_weights,omitempty"`
	HeartbeatInterval    time.Duration          `json:"heartbeat_interval,omitempty"`
	FailureRetryInterval time.Duration          `json:"failure_retry_interval,omitempty"`
}

// Price represents a price from a provider
type Price struct {
	Asset      string     `json:"asset"`
	Value      *big.Float `json:"value"`
	Timestamp  time.Time  `json:"timestamp"`
	Provider   string     `json:"provider"`
	Heartbeat  int64      `json:"heartbeat"`
	Confidence float64    `json:"confidence"`
}
