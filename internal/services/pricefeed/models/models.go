package models

import (
	"math/big"
	"time"
)

// Price represents a price point for an asset
type Price struct {
	AssetID    string
	Price      *big.Float
	Timestamp  time.Time
	Source     string
	Confidence float64
}

// PriceUpdatePolicy represents the policy for price updates
type PriceUpdatePolicy struct {
	MinUpdateInterval time.Duration
	MaxPriceDeviation *big.Float
	MinDataSources    int
	MaxDataAge        time.Duration
}

// PriceMetrics represents metrics for price updates
type PriceMetrics struct {
	TotalUpdates     int
	FailedUpdates    int
	AverageLatency   time.Duration
	LastUpdateTime   time.Time
	DataSourceHealth map[string]float64
}