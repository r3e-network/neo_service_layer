package transaction

import (
	"github.com/prometheus/client_golang/prometheus"
)

// Metrics for the transaction service
var (
	// TransactionAttempts counts the total number of transaction attempts
	TransactionAttempts = prometheus.NewCounter(prometheus.CounterOpts{
		Namespace: "neo_service_layer",
		Subsystem: "transaction",
		Name:      "transaction_attempts_total",
		Help:      "The total number of transaction attempts",
	})

	// TransactionSuccesses counts the total number of successful transactions
	TransactionSuccesses = prometheus.NewCounter(prometheus.CounterOpts{
		Namespace: "neo_service_layer",
		Subsystem: "transaction",
		Name:      "transaction_successes_total",
		Help:      "The total number of successful transactions",
	})

	// TransactionFailures counts the total number of failed transactions
	TransactionFailures = prometheus.NewCounter(prometheus.CounterOpts{
		Namespace: "neo_service_layer",
		Subsystem: "transaction",
		Name:      "transaction_failures_total",
		Help:      "The total number of failed transactions",
	})

	// TransactionConfirmationTime measures the time taken for transactions to be confirmed
	TransactionConfirmationTime = prometheus.NewHistogram(prometheus.HistogramOpts{
		Namespace: "neo_service_layer",
		Subsystem: "transaction",
		Name:      "confirmation_time_seconds",
		Help:      "Time taken for transactions to be confirmed",
		Buckets:   prometheus.ExponentialBuckets(1, 2, 10), // 1s to ~17m
	})

	// TransactionFeeEstimation measures the distribution of estimated transaction fees
	TransactionFeeEstimation = prometheus.NewHistogram(prometheus.HistogramOpts{
		Namespace: "neo_service_layer",
		Subsystem: "transaction",
		Name:      "fee_estimation_gas",
		Help:      "Distribution of estimated transaction fees in GAS",
		Buckets:   prometheus.ExponentialBuckets(0.0001, 2, 10), // 0.0001 GAS to ~0.1 GAS
	})

	// ActiveTransactions tracks the number of active transactions
	ActiveTransactions = prometheus.NewGauge(prometheus.GaugeOpts{
		Namespace: "neo_service_layer",
		Subsystem: "transaction",
		Name:      "active_transactions",
		Help:      "The number of active transactions",
	})
)

func init() {
	// Register all metrics with Prometheus
	prometheus.MustRegister(TransactionAttempts)
	prometheus.MustRegister(TransactionSuccesses)
	prometheus.MustRegister(TransactionFailures)
	prometheus.MustRegister(TransactionConfirmationTime)
	prometheus.MustRegister(TransactionFeeEstimation)
	prometheus.MustRegister(ActiveTransactions)
}
