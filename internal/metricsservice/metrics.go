// Package metrics provides a service for collecting and storing metrics
package metrics

import (
	"context"
	"fmt"
	"sync"
	"time"

	"github.com/sirupsen/logrus"
)

// ServiceType represents different Neo service components
type ServiceType string

const (
	ServiceAPI       ServiceType = "api"
	ServiceGasBank   ServiceType = "gas_bank"
	ServiceSecrets   ServiceType = "secrets"
	ServiceFunctions ServiceType = "functions"
	ServicePriceFeed ServiceType = "price_feed"
	ServiceTrigger   ServiceType = "trigger"
)

// MetricType represents the type of metric
type MetricType string

const (
	MetricTypeGauge     MetricType = "gauge"
	MetricTypeCounter   MetricType = "counter"
	MetricTypeHistogram MetricType = "histogram"
)

// Metric represents a single metric data point
type Metric struct {
	Name       string
	Type       MetricType
	Value      float64
	Labels     map[string]string
	Service    ServiceType
	Timestamp  time.Time
	Dimensions []string  // For histograms
	Buckets    []float64 // For histograms
}

// Config represents the configuration for the metrics service
type Config struct {
	CollectionInterval time.Duration
	RetentionPeriod    time.Duration
	StorageBackend     string // "memory", "redis", "prometheus", etc.
	StorageConfig      map[string]string
}

// DefaultConfig returns a default configuration for the metrics service
func DefaultConfig() *Config {
	return &Config{
		CollectionInterval: 60 * time.Second,
		RetentionPeriod:    24 * time.Hour,
		StorageBackend:     "memory",
		StorageConfig:      make(map[string]string),
	}
}

// Service represents the metrics service
type Service struct {
	config     *Config
	log        *logrus.Logger
	gauges     map[string]*Gauge
	counters   map[string]*Counter
	histograms map[string]*Histogram
	metrics    []Metric
	mutex      sync.RWMutex
	stopChan   chan struct{}
	collectors map[ServiceType][]Collector
	exporter   MetricExporter
}

// Gauge represents a gauge metric that can go up and down
type Gauge struct {
	name    string
	service ServiceType
	value   float64
	labels  map[string]string
}

// Counter represents a counter metric that can only go up
type Counter struct {
	name    string
	service ServiceType
	value   float64
	labels  map[string]string
}

// Histogram represents a histogram metric that tracks value distributions
type Histogram struct {
	name      string
	service   ServiceType
	count     int64
	sum       float64
	buckets   []float64
	counts    []int64
	labels    map[string]string
	dimension string
}

// Collector defines an interface for collecting metrics from services
type Collector interface {
	Collect(ctx context.Context) []Metric
}

// NewService creates a new metrics service
func NewService(config *Config) (*Service, error) {
	if config == nil {
		config = DefaultConfig()
	}

	logger := logrus.New()
	logger.SetLevel(logrus.InfoLevel)

	svc := &Service{
		config:     config,
		log:        logger,
		gauges:     make(map[string]*Gauge),
		counters:   make(map[string]*Counter),
		histograms: make(map[string]*Histogram),
		metrics:    make([]Metric, 0),
		stopChan:   make(chan struct{}),
		collectors: make(map[ServiceType][]Collector),
	}

	// Initialize Exporter based on config
	switch config.StorageBackend {
	case "prometheus":
		// TODO: Get port/path from config.StorageConfig
		promPort := 9091 // Default port
		promPath := "/metrics"
		svc.exporter = NewPrometheusExporter(promPort, promPath)
		logger.Infof("Initialized Prometheus exporter on port %d path %s", promPort, promPath)
	case "json":
		// TODO: Get file path/interval from config.StorageConfig
		filePath := "./metrics.json"
		interval := 60 * time.Second
		svc.exporter = NewJSONExporter(filePath, interval)
		logger.Infof("Initialized JSON exporter to file %s every %v", filePath, interval)
	case "memory", "":
		logger.Info("Using in-memory metric storage only (no exporter configured)")
		// No exporter needed for memory store
	default:
		return nil, fmt.Errorf("unsupported metrics storage backend: %s", config.StorageBackend)
	}

	return svc, nil
}

// Start begins the metrics collection process and starts the exporter
func (s *Service) Start(ctx context.Context) error {
	// Start exporter if it exists
	if s.exporter != nil {
		if err := s.exporter.Start(); err != nil {
			return fmt.Errorf("failed to start metrics exporter: %w", err)
		}
		s.log.Info("Metrics exporter started.")
	}

	go s.runCollectionLoop(ctx)
	s.log.Info("Metrics collection loop started.")
	return nil
}

// Stop halts the metrics collection process and stops the exporter
func (s *Service) Stop() error {
	s.log.Info("Stopping metrics service...")
	close(s.stopChan)

	// Stop exporter if it exists
	if s.exporter != nil {
		if err := s.exporter.Stop(); err != nil {
			s.log.Errorf("Failed to stop metrics exporter cleanly: %v", err)
			// Potentially return error or just log?
		} else {
			s.log.Info("Metrics exporter stopped.")
		}
	}
	s.log.Info("Metrics service stopped.")
	return nil
}

// runCollectionLoop regularly collects metrics from all registered collectors
func (s *Service) runCollectionLoop(ctx context.Context) {
	ticker := time.NewTicker(s.config.CollectionInterval)
	defer ticker.Stop()

	for {
		select {
		case <-ticker.C:
			s.collectAndExport(ctx)
		case <-s.stopChan:
			s.log.Info("Stopping metrics collection loop.")
			return
		case <-ctx.Done():
			s.log.Warnf("Metrics collection loop stopping due to context cancellation: %v", ctx.Err())
			return
		}
	}
}

// collectAndExport gathers metrics and pushes them to the exporter
func (s *Service) collectAndExport(ctx context.Context) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	collectedMetrics := []Metric{}
	for serviceType, collectors := range s.collectors {
		for _, collector := range collectors {
			metrics := collector.Collect(ctx)
			for i := range metrics {
				// Ensure the service type is set correctly
				if metrics[i].Service == "" {
					metrics[i].Service = serviceType
				}
				// Add timestamp here before potential export/storage
				metrics[i].Timestamp = time.Now()
			}
			collectedMetrics = append(collectedMetrics, metrics...)
		}
	}

	// Store/Process all collected metrics (in-memory for now)
	for _, metric := range collectedMetrics {
		s.metrics = append(s.metrics, metric)
		s.processMetricAggregations(metric)
	}

	// Clean up old metrics based on retention period (only for in-memory store)
	if s.config.StorageBackend == "memory" || s.config.StorageBackend == "" {
		s.cleanupOldMetrics()
	}

	// Prepare data for exporter (using aggregated values for simplicity with current exporters)
	exportData := s.getAggregatedMetricsMap()

	s.mutex.Unlock()

	// Export data if exporter exists
	if s.exporter != nil && len(exportData) > 0 {
		if err := s.exporter.Export(exportData); err != nil {
			s.log.Errorf("Failed to export metrics: %v", err)
		}
	}
}

// getAggregatedMetricsMap converts current gauges/counters/histograms to map for exporter
// NOTE: This is a simplification for the current basic exporters. A real Prometheus
// exporter would likely register collectors/metrics directly.
func (s *Service) getAggregatedMetricsMap() map[string]interface{} {
	exportData := make(map[string]interface{})
	for key, gauge := range s.gauges {
		// Maybe add labels to key?
		exportData[key] = gauge.value
	}
	for key, counter := range s.counters {
		exportData[key] = counter.value
	}
	// TODO: How to represent histograms in simple map? Maybe just sum/count?
	// for key, histo := range s.histograms {
	// 	exportData[key+"_count"] = float64(histo.count)
	// 	exportData[key+"_sum"] = histo.sum
	// }
	return exportData
}

// cleanupOldMetrics removes metrics older than the retention period
func (s *Service) cleanupOldMetrics() {
	cutoff := time.Now().Add(-s.config.RetentionPeriod)
	newMetrics := make([]Metric, 0, len(s.metrics))

	for _, metric := range s.metrics {
		if metric.Timestamp.After(cutoff) {
			newMetrics = append(newMetrics, metric)
		}
	}

	s.metrics = newMetrics
}

// RegisterCollector adds a new metrics collector for a specific service
func (s *Service) RegisterCollector(serviceType ServiceType, collector Collector) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	if _, exists := s.collectors[serviceType]; !exists {
		s.collectors[serviceType] = make([]Collector, 0)
	}

	s.collectors[serviceType] = append(s.collectors[serviceType], collector)
}

// storeMetric processes and stores a single metric (potentially rename)
func (s *Service) storeMetric(metric Metric) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	// Timestamp is now set in collectAndExport before calling this
	// metric.Timestamp = time.Now()

	// Append to raw metric log (if memory backend)
	if s.config.StorageBackend == "memory" || s.config.StorageBackend == "" {
		s.metrics = append(s.metrics, metric)
	}

	// Update aggregated views
	s.processMetricAggregations(metric)
}

// processMetricAggregations updates aggregated views (gauges, counters, histograms)
func (s *Service) processMetricAggregations(metric Metric) {
	// Assumes lock is already held
	switch metric.Type {
	case MetricTypeGauge:
		s.processGauge(metric)
	case MetricTypeCounter:
		s.processCounter(metric)
	case MetricTypeHistogram:
		s.processHistogram(metric)
	}
}

// processGauge updates gauge values
func (s *Service) processGauge(metric Metric) {
	key := s.metricKey(metric.Name, metric.Service, metric.Labels)
	if _, exists := s.gauges[key]; !exists {
		s.gauges[key] = &Gauge{
			name:    metric.Name,
			service: metric.Service,
			labels:  metric.Labels,
		}
	}
	s.gauges[key].value = metric.Value
}

// processCounter updates counter values
func (s *Service) processCounter(metric Metric) {
	key := s.metricKey(metric.Name, metric.Service, metric.Labels)
	if _, exists := s.counters[key]; !exists {
		s.counters[key] = &Counter{
			name:    metric.Name,
			service: metric.Service,
			labels:  metric.Labels,
		}
	}
	s.counters[key].value += metric.Value
}

// processHistogram updates histogram values
func (s *Service) processHistogram(metric Metric) {
	key := s.metricKey(metric.Name, metric.Service, metric.Labels)
	if _, exists := s.histograms[key]; !exists {
		// Create new histogram if it doesn't exist
		s.histograms[key] = &Histogram{
			name:      metric.Name,
			service:   metric.Service,
			buckets:   metric.Buckets,
			counts:    make([]int64, len(metric.Buckets)),
			labels:    metric.Labels,
			dimension: metric.Dimensions[0], // Use first dimension
		}
	}

	// Update histogram data
	histogram := s.histograms[key]
	histogram.count++
	histogram.sum += metric.Value

	// Update bucket counts
	for i, bucketLimit := range histogram.buckets {
		if metric.Value <= bucketLimit {
			histogram.counts[i]++
		}
	}
}

// metricKey generates a unique key for a metric
func (s *Service) metricKey(name string, service ServiceType, labels map[string]string) string {
	key := string(service) + ":" + name
	for k, v := range labels {
		key += ":" + k + "=" + v
	}
	return key
}

// Recording methods (RecordGauge, RecordCounter, RecordHistogram)
// These now call storeMetric which handles aggregation and memory storage.
// Export happens periodically via the collectAndExport loop.
func (s *Service) RecordGauge(name string, value float64, service ServiceType, labels map[string]string) {
	s.storeMetric(Metric{
		Name:      name,
		Type:      MetricTypeGauge,
		Value:     value,
		Labels:    labels,
		Service:   service,
		Timestamp: time.Now(), // Timestamp for immediate recording
	})
}

func (s *Service) RecordCounter(name string, value float64, service ServiceType, labels map[string]string) {
	s.storeMetric(Metric{
		Name:      name,
		Type:      MetricTypeCounter,
		Value:     value,
		Labels:    labels,
		Service:   service,
		Timestamp: time.Now(), // Timestamp for immediate recording
	})
}

func (s *Service) RecordHistogram(name string, value float64, service ServiceType, dimension string, buckets []float64, labels map[string]string) {
	s.storeMetric(Metric{
		Name:       name,
		Type:       MetricTypeHistogram,
		Value:      value,
		Labels:     labels,
		Service:    service,
		Timestamp:  time.Now(), // Timestamp for immediate recording
		Dimensions: []string{dimension},
		Buckets:    buckets,
	})
}

// --- Get Methods (Primarily read from aggregated maps for efficiency) ---

// GetGaugeValue retrieves the current value of a gauge
func (s *Service) GetGaugeValue(name string, service ServiceType, labels map[string]string) (float64, bool) {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	key := s.metricKey(name, service, labels)
	gauge, exists := s.gauges[key]
	if !exists {
		return 0, false
	}
	return gauge.value, true
}

// GetCounterValue retrieves the current value of a counter
func (s *Service) GetCounterValue(name string, service ServiceType, labels map[string]string) (float64, bool) {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	key := s.metricKey(name, service, labels)
	counter, exists := s.counters[key]
	if !exists {
		return 0, false
	}
	return counter.value, true
}

// GetHistogramData retrieves the current data of a histogram
func (s *Service) GetHistogramData(name string, service ServiceType, labels map[string]string) (count int64, sum float64, buckets []float64, counts []int64, exists bool) {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	key := s.metricKey(name, service, labels)
	histogram, found := s.histograms[key]
	if !found {
		return 0, 0, nil, nil, false
	}
	return histogram.count, histogram.sum, histogram.buckets, histogram.counts, true
}

// GetMetricsForService retrieves recent raw metrics for a specific service (from in-memory store)
func (s *Service) GetMetricsForService(service ServiceType) []Metric {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	result := make([]Metric, 0)
	// Iterate in reverse to get most recent first?
	for i := len(s.metrics) - 1; i >= 0; i-- {
		metric := s.metrics[i]
		if metric.Service == service {
			result = append(result, metric)
		}
	}
	// Optional: Limit results?
	return result
}

// GetAllMetrics retrieves all stored raw metrics (from in-memory store)
func (s *Service) GetAllMetrics() []Metric {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	result := make([]Metric, len(s.metrics))
	copy(result, s.metrics)
	return result
}

// GetConfig returns the current metrics service configuration
func (s *Service) GetConfig() *Config {
	return s.config
}
