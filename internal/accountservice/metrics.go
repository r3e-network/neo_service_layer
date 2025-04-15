package account

import (
	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/promauto"
)

var (
	accountCreations = promauto.NewCounter(prometheus.CounterOpts{
		Namespace: "neo_service",
		Subsystem: "account",
		Name:      "creations_total",
		Help:      "The total number of accounts created",
	})

	transactionAttempts = promauto.NewCounter(prometheus.CounterOpts{
		Namespace: "neo_service",
		Subsystem: "account",
		Name:      "transaction_attempts_total",
		Help:      "The total number of transaction attempts",
	})

	transactionSuccesses = promauto.NewCounter(prometheus.CounterOpts{
		Namespace: "neo_service",
		Subsystem: "account",
		Name:      "transaction_successes_total",
		Help:      "The total number of successful transactions",
	})

	signatureVerificationLatency = promauto.NewHistogram(prometheus.HistogramOpts{
		Namespace: "neo_service",
		Subsystem: "account",
		Name:      "signature_verification_duration_seconds",
		Help:      "Time taken to verify signatures",
		Buckets:   prometheus.DefBuckets,
	})

	gasUsage = promauto.NewHistogram(prometheus.HistogramOpts{
		Namespace: "neo_service",
		Subsystem: "account",
		Name:      "gas_usage",
		Help:      "Distribution of gas usage per transaction",
		Buckets:   prometheus.ExponentialBuckets(10, 2, 10),
	})

	activeAccounts = promauto.NewGauge(prometheus.GaugeOpts{
		Namespace: "neo_service",
		Subsystem: "account",
		Name:      "active_total",
		Help:      "The total number of active accounts",
	})
)
