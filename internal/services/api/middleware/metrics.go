package middleware

import (
	"net/http"
	"strconv"
	"time"

	"github.com/prometheus/client_golang/prometheus"
)

// MetricsCollector collects HTTP metrics
type MetricsCollector struct {
	requestsTotal      *prometheus.CounterVec
	requestDuration    *prometheus.HistogramVec
	responseSize       *prometheus.HistogramVec
	requestsInProgress *prometheus.GaugeVec
}

// NewMetricsCollector creates a new metrics collector
func NewMetricsCollector(namespace string) *MetricsCollector {
	subsystem := "http"

	return &MetricsCollector{
		requestsTotal: prometheus.NewCounterVec(
			prometheus.CounterOpts{
				Namespace: namespace,
				Subsystem: subsystem,
				Name:      "requests_total",
				Help:      "Total number of HTTP requests",
			},
			[]string{"method", "path", "status"},
		),
		requestDuration: prometheus.NewHistogramVec(
			prometheus.HistogramOpts{
				Namespace: namespace,
				Subsystem: subsystem,
				Name:      "request_duration_seconds",
				Help:      "HTTP request duration in seconds",
				Buckets:   prometheus.DefBuckets,
			},
			[]string{"method", "path"},
		),
		responseSize: prometheus.NewHistogramVec(
			prometheus.HistogramOpts{
				Namespace: namespace,
				Subsystem: subsystem,
				Name:      "response_size_bytes",
				Help:      "HTTP response size in bytes",
				Buckets:   prometheus.ExponentialBuckets(100, 10, 8),
			},
			[]string{"method", "path"},
		),
		requestsInProgress: prometheus.NewGaugeVec(
			prometheus.GaugeOpts{
				Namespace: namespace,
				Subsystem: subsystem,
				Name:      "requests_in_progress",
				Help:      "Number of HTTP requests in progress",
			},
			[]string{"method", "path"},
		),
	}
}

// Register registers Prometheus metrics
func (c *MetricsCollector) Register(registry prometheus.Registerer) {
	registry.MustRegister(
		c.requestsTotal,
		c.requestDuration,
		c.responseSize,
		c.requestsInProgress,
	)
}

// Middleware returns a middleware for collecting HTTP metrics
func (c *MetricsCollector) Middleware() func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			start := time.Now()

			// Track in-progress requests
			labels := prometheus.Labels{
				"method": r.Method,
				"path":   r.URL.Path,
			}
			c.requestsInProgress.With(labels).Inc()
			defer c.requestsInProgress.With(labels).Dec()

			// Create a wrapper to capture response metadata
			wrapper := newResponseWriterWrapper(w)

			// Process the request
			next.ServeHTTP(wrapper, r)

			// Record metrics
			duration := time.Since(start).Seconds()
			statusCode := strconv.Itoa(wrapper.statusCode)

			c.requestsTotal.WithLabelValues(r.Method, r.URL.Path, statusCode).Inc()
			c.requestDuration.WithLabelValues(r.Method, r.URL.Path).Observe(duration)
			c.responseSize.WithLabelValues(r.Method, r.URL.Path).Observe(float64(wrapper.size))
		})
	}
}

// responseWriterWrapper wraps http.ResponseWriter to capture status code and size
type metricsResponseWriterWrapper struct {
	http.ResponseWriter
	statusCode int
	size       int
}

// newResponseWriterWrapper creates a new response writer wrapper
func newResponseWriterWrapper(w http.ResponseWriter) *metricsResponseWriterWrapper {
	return &metricsResponseWriterWrapper{
		ResponseWriter: w,
		statusCode:     http.StatusOK, // Default status code
	}
}

// WriteHeader captures the status code
func (w *metricsResponseWriterWrapper) WriteHeader(statusCode int) {
	w.statusCode = statusCode
	w.ResponseWriter.WriteHeader(statusCode)
}

// Write captures the response size
func (w *metricsResponseWriterWrapper) Write(b []byte) (int, error) {
	size, err := w.ResponseWriter.Write(b)
	w.size += size
	return size, err
}
