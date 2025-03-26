package tee

import (
	"github.com/prometheus/client_golang/prometheus"
)

var (
	// Attestation metrics
	attestationTotal = prometheus.NewCounterVec(
		prometheus.CounterOpts{
			Name: "attestation_total",
			Help: "Total number of attestation verifications",
		},
		[]string{"type", "platform", "result"},
	)

	attestationDuration = prometheus.NewHistogramVec(
		prometheus.HistogramOpts{
			Name:    "attestation_duration_seconds",
			Help:    "Duration of attestation verifications",
			Buckets: prometheus.ExponentialBuckets(0.01, 2, 10),
		},
		[]string{"type", "platform"},
	)

	attestationErrors = prometheus.NewCounterVec(
		prometheus.CounterOpts{
			Name: "attestation_errors_total",
			Help: "Total number of attestation errors",
		},
		[]string{"type", "error"},
	)

	// Quote verification metrics
	quoteVerificationTotal = prometheus.NewCounterVec(
		prometheus.CounterOpts{
			Name: "quote_verification_total",
			Help: "Total number of quote verifications",
		},
		[]string{"type", "result"},
	)

	quoteVerificationDuration = prometheus.NewHistogramVec(
		prometheus.HistogramOpts{
			Name:    "quote_verification_duration_seconds",
			Help:    "Duration of quote verifications",
			Buckets: prometheus.ExponentialBuckets(0.01, 2, 10),
		},
		[]string{"type"},
	)

	// IAS request metrics
	iasRequestTotal = prometheus.NewCounterVec(
		prometheus.CounterOpts{
			Name: "ias_request_total",
			Help: "Total number of IAS API requests",
		},
		[]string{"endpoint", "status"},
	)

	iasRequestDuration = prometheus.NewHistogramVec(
		prometheus.HistogramOpts{
			Name:    "ias_request_duration_seconds",
			Help:    "Duration of IAS API requests",
			Buckets: prometheus.ExponentialBuckets(0.01, 2, 10),
		},
		[]string{"endpoint"},
	)
)

func init() {
	// Register metrics with Prometheus
	prometheus.MustRegister(attestationTotal)
	prometheus.MustRegister(attestationDuration)
	prometheus.MustRegister(attestationErrors)
	prometheus.MustRegister(quoteVerificationTotal)
	prometheus.MustRegister(quoteVerificationDuration)
	prometheus.MustRegister(iasRequestTotal)
	prometheus.MustRegister(iasRequestDuration)
}
