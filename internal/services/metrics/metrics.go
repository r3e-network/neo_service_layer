// Package metrics provides a service for collecting and storing metrics
package metrics

import (
	"context"
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
	config         *Config
	log            *logrus.Logger
	gauges         map[string]*Gauge
	counters       map[string]*Counter
	histograms     map[string]*Histogram
	metrics        []Metric
	mutex          sync.RWMutex
	stopCollection chan struct{}
	collectors     map[ServiceType][]Collector
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
func NewService(config *Config) *Service {
	if config == nil {
		config = DefaultConfig()
	}

	logger := logrus.New()
	logger.SetLevel(logrus.InfoLevel)

	return &Service{
		config:         config,
		log:            logger,
		gauges:         make(map[string]*Gauge),
		counters:       make(map[string]*Counter),
		histograms:     make(map[string]*Histogram),
		metrics:        make([]Metric, 0),
		stopCollection: make(chan struct{}),
		collectors:     make(map[ServiceType][]Collector),
	}
}

// Start begins the metrics collection process
func (s *Service) Start(ctx context.Context) error {
	go s.collectMetrics(ctx)
	return nil
}

// Stop halts the metrics collection process
func (s *Service) Stop() error {
	close(s.stopCollection)
	return nil
}

// collectMetrics regularly collects metrics from all registered collectors
func (s *Service) collectMetrics(ctx context.Context) {
	ticker := time.NewTicker(s.config.CollectionInterval)
	defer ticker.Stop()

	for {
		select {
		case <-ticker.C:
			s.collect(ctx)
		case <-s.stopCollection:
			return
		case <-ctx.Done():
			return
		}
	}
}

// collect gathers metrics from all collectors
func (s *Service) collect(ctx context.Context) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	for serviceType, collectors := range s.collectors {
		for _, collector := range collectors {
			metrics := collector.Collect(ctx)
			for _, metric := range metrics {
				// Ensure the service type is set
				if metric.Service == "" {
					metric.Service = serviceType
				}
				s.storeMetric(metric)
			}
		}
	}

	// Clean up old metrics based on retention period
	s.cleanupOldMetrics()
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

// storeMetric adds a metric to the storage
func (s *Service) storeMetric(metric Metric) {
	metric.Timestamp = time.Now()
	s.metrics = append(s.metrics, metric)

	// Process based on metric type
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

// RecordGauge records a gauge metric
func (s *Service) RecordGauge(name string, value float64, service ServiceType, labels map[string]string) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	s.storeMetric(Metric{
		Name:      name,
		Type:      MetricTypeGauge,
		Value:     value,
		Labels:    labels,
		Service:   service,
		Timestamp: time.Now(),
	})
}

// RecordCounter records a counter metric
func (s *Service) RecordCounter(name string, value float64, service ServiceType, labels map[string]string) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	s.storeMetric(Metric{
		Name:      name,
		Type:      MetricTypeCounter,
		Value:     value,
		Labels:    labels,
		Service:   service,
		Timestamp: time.Now(),
	})
}

// RecordHistogram records a histogram metric
func (s *Service) RecordHistogram(name string, value float64, service ServiceType, dimension string, buckets []float64, labels map[string]string) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	s.storeMetric(Metric{
		Name:       name,
		Type:       MetricTypeHistogram,
		Value:      value,
		Labels:     labels,
		Service:    service,
		Timestamp:  time.Now(),
		Dimensions: []string{dimension},
		Buckets:    buckets,
	})
}

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

// GetMetricsForService retrieves all metrics for a specific service
func (s *Service) GetMetricsForService(service ServiceType) []Metric {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	result := make([]Metric, 0)
	for _, metric := range s.metrics {
		if metric.Service == service {
			result = append(result, metric)
		}
	}
	return result
}

// GetAllMetrics retrieves all stored metrics
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
