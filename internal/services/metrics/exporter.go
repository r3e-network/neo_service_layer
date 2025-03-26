package metrics

import (
	"encoding/json"
	"fmt"
	"net/http"
	"strings"
	"sync"
	"time"
)

// MetricExporter exports metrics to various destinations
type MetricExporter interface {
	Export(metrics map[string]interface{}) error
	Start() error
	Stop() error
}

// PrometheusExporter exports metrics in Prometheus format
type PrometheusExporter struct {
	port       int
	path       string
	server     *http.Server
	metrics    map[string]interface{}
	mutex      sync.RWMutex
}

// NewPrometheusExporter creates a new Prometheus exporter
func NewPrometheusExporter(port int, path string) *PrometheusExporter {
	return &PrometheusExporter{
		port:    port,
		path:    path,
		metrics: make(map[string]interface{}),
	}
}

// Export exports metrics in Prometheus format
func (e *PrometheusExporter) Export(metrics map[string]interface{}) error {
	e.mutex.Lock()
	defer e.mutex.Unlock()

	// Update metrics
	for k, v := range metrics {
		e.metrics[k] = v
	}

	return nil
}

// Start starts the Prometheus exporter
func (e *PrometheusExporter) Start() error {
	// Set up HTTP handler
	mux := http.NewServeMux()
	mux.HandleFunc(e.path, e.handleRequest)

	// Create server
	e.server = &http.Server{
		Addr:    fmt.Sprintf(":%d", e.port),
		Handler: mux,
	}

	// Start server
	go func() {
		if err := e.server.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			// Log error
		}
	}()

	return nil
}

// Stop stops the Prometheus exporter
func (e *PrometheusExporter) Stop() error {
	if e.server != nil {
		return e.server.Close()
	}
	return nil
}

// handleRequest handles HTTP requests for metrics
func (e *PrometheusExporter) handleRequest(w http.ResponseWriter, r *http.Request) {
	e.mutex.RLock()
	defer e.mutex.RUnlock()

	// Build Prometheus format
	var lines []string
	for name, value := range e.metrics {
		// Convert Neo-style metrics to Prometheus style
		promName := strings.Replace(name, "_", ":", -1)
		
		// Add type hints
		lines = append(lines, fmt.Sprintf("# HELP %s %s", promName, "Neo service metric"))
		switch value.(type) {
		case int, int64, float32, float64:
			lines = append(lines, fmt.Sprintf("# TYPE %s gauge", promName))
		default:
			lines = append(lines, fmt.Sprintf("# TYPE %s untyped", promName))
		}
		
		// Add metric value
		lines = append(lines, fmt.Sprintf("%s %v", promName, value))
	}

	// Write response
	w.Header().Set("Content-Type", "text/plain")
	fmt.Fprintln(w, strings.Join(lines, "\n"))
}

// JSONExporter exports metrics as JSON
type JSONExporter struct {
	filePath   string
	interval   time.Duration
	stopChan   chan struct{}
	metrics    map[string]interface{}
	mutex      sync.RWMutex
}

// NewJSONExporter creates a new JSON exporter
func NewJSONExporter(filePath string, interval time.Duration) *JSONExporter {
	return &JSONExporter{
		filePath: filePath,
		interval: interval,
		stopChan: make(chan struct{}),
		metrics:  make(map[string]interface{}),
	}
}

// Export exports metrics as JSON
func (e *JSONExporter) Export(metrics map[string]interface{}) error {
	e.mutex.Lock()
	defer e.mutex.Unlock()

	// Update metrics
	for k, v := range metrics {
		e.metrics[k] = v
	}

	return nil
}

// Start starts the JSON exporter
func (e *JSONExporter) Start() error {
	go e.exportLoop()
	return nil
}

// Stop stops the JSON exporter
func (e *JSONExporter) Stop() error {
	close(e.stopChan)
	return nil
}

// exportLoop periodically exports metrics
func (e *JSONExporter) exportLoop() {
	ticker := time.NewTicker(e.interval)
	defer ticker.Stop()

	for {
		select {
		case <-ticker.C:
			e.exportToFile()
		case <-e.stopChan:
			return
		}
	}
}

// exportToFile exports metrics to a file
func (e *JSONExporter) exportToFile() {
	e.mutex.RLock()
	defer e.mutex.RUnlock()

	// This is a mock implementation
	// In a real implementation, we'd write to the file
	data, _ := json.MarshalIndent(e.metrics, "", "  ")
	_ = data // Avoid unused variable warning
}

// MultiExporter exports metrics to multiple destinations
type MultiExporter struct {
	exporters []MetricExporter
}

// NewMultiExporter creates a new multi-exporter
func NewMultiExporter(exporters ...MetricExporter) *MultiExporter {
	return &MultiExporter{
		exporters: exporters,
	}
}

// Export exports metrics to all destinations
func (e *MultiExporter) Export(metrics map[string]interface{}) error {
	for _, exporter := range e.exporters {
		if err := exporter.Export(metrics); err != nil {
			return err
		}
	}
	return nil
}

// Start starts all exporters
func (e *MultiExporter) Start() error {
	for _, exporter := range e.exporters {
		if err := exporter.Start(); err != nil {
			return err
		}
	}
	return nil
}

// Stop stops all exporters
func (e *MultiExporter) Stop() error {
	for _, exporter := range e.exporters {
		if err := exporter.Stop(); err != nil {
			return err
		}
	}
	return nil
}