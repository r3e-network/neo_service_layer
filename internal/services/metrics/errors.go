package metrics

import "errors"

// Error definitions for the metrics service
var (
	// ErrMetricNotFound is returned when a requested metric doesn't exist
	ErrMetricNotFound = errors.New("metric not found")

	// ErrInvalidMetricsType is returned when a metric has the wrong type
	ErrInvalidMetricsType = errors.New("invalid metrics type")

	// ErrInvalidQuantile is returned when an invalid quantile is requested
	ErrInvalidQuantile = errors.New("invalid quantile value (must be between 0 and 1)")

	// ErrInvalidMetricName is returned when an invalid metric name is provided
	ErrInvalidMetricName = errors.New("invalid metric name")

	// ErrDuplicateMetric is returned when attempting to register a metric with a name that already exists
	ErrDuplicateMetric = errors.New("metric with that name already exists")
)
