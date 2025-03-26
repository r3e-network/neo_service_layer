package automation

import (
	"sync"
	"time"
)

// Performance metrics for monitoring
type PerformanceMetrics struct {
	TotalChecks           uint64
	EligibleChecks        uint64
	TotalPerforms         uint64
	SuccessfulPerforms    uint64
	FailedPerforms        uint64
	AverageCheckLatency   time.Duration
	AveragePerformLatency time.Duration
	LastCheckTime         time.Time
	LastPerformTime       time.Time
	GasUsed               int64
}

// PerformanceAlert represents a performance alert
type PerformanceAlert struct {
	UpkeepID    string
	AlertType   string // "high-latency", "repeated-failures", "gas-spike", etc.
	Timestamp   time.Time
	Description string
	Severity    string // "info", "warning", "critical"
	Metrics     map[string]interface{}
}

// Monitor tracks upkeep performance and health
type Monitor struct {
	upkeepMetrics    map[string]*PerformanceMetrics
	globalMetrics    *PerformanceMetrics
	alerts           []PerformanceAlert
	alertThresholds  map[string]interface{}
	metricsWindow    time.Duration
	checkHistogram   map[int64]int // ms -> count
	performHistogram map[int64]int // ms -> count
	mu               sync.RWMutex
}

// NewMonitor creates a new Monitor
func NewMonitor(metricsWindow time.Duration) *Monitor {
	return &Monitor{
		upkeepMetrics: make(map[string]*PerformanceMetrics),
		globalMetrics: &PerformanceMetrics{},
		alerts:        make([]PerformanceAlert, 0),
		alertThresholds: map[string]interface{}{
			"maxCheckLatency":      time.Second * 5,
			"maxPerformLatency":    time.Second * 30,
			"maxConsecutiveErrors": 3,
			"maxGasVariance":       0.5, // 50% variation
		},
		metricsWindow:    metricsWindow,
		checkHistogram:   make(map[int64]int),
		performHistogram: make(map[int64]int),
	}
}

// RecordCheck records a check operation
func (m *Monitor) RecordCheck(upkeepID string, duration time.Duration, eligible bool) {
	m.mu.Lock()
	defer m.mu.Unlock()

	// Update upkeep metrics
	metrics, ok := m.upkeepMetrics[upkeepID]
	if !ok {
		metrics = &PerformanceMetrics{}
		m.upkeepMetrics[upkeepID] = metrics
	}

	metrics.TotalChecks++
	if eligible {
		metrics.EligibleChecks++
	}
	metrics.AverageCheckLatency = calculateMovingAverage(metrics.AverageCheckLatency, duration, metrics.TotalChecks)
	metrics.LastCheckTime = time.Now()

	// Update global metrics
	m.globalMetrics.TotalChecks++
	if eligible {
		m.globalMetrics.EligibleChecks++
	}
	m.globalMetrics.AverageCheckLatency = calculateMovingAverage(m.globalMetrics.AverageCheckLatency, duration, m.globalMetrics.TotalChecks)
	m.globalMetrics.LastCheckTime = time.Now()

	// Update histogram
	ms := duration.Milliseconds()
	m.checkHistogram[ms]++

	// Generate alerts if needed
	if duration > m.alertThresholds["maxCheckLatency"].(time.Duration) {
		m.addAlert(PerformanceAlert{
			UpkeepID:    upkeepID,
			AlertType:   "high-check-latency",
			Timestamp:   time.Now(),
			Description: "Check operation exceeded maximum latency threshold",
			Severity:    "warning",
			Metrics: map[string]interface{}{
				"duration":  duration.String(),
				"threshold": m.alertThresholds["maxCheckLatency"].(time.Duration).String(),
				"upkeepID":  upkeepID,
			},
		})
	}
}

// RecordPerform records a perform operation
func (m *Monitor) RecordPerform(upkeepID string, duration time.Duration, success bool, gasUsed int64) {
	m.mu.Lock()
	defer m.mu.Unlock()

	// Update upkeep metrics
	metrics, ok := m.upkeepMetrics[upkeepID]
	if !ok {
		metrics = &PerformanceMetrics{}
		m.upkeepMetrics[upkeepID] = metrics
	}

	metrics.TotalPerforms++
	if success {
		metrics.SuccessfulPerforms++
	} else {
		metrics.FailedPerforms++
	}
	metrics.AveragePerformLatency = calculateMovingAverage(metrics.AveragePerformLatency, duration, metrics.TotalPerforms)
	metrics.LastPerformTime = time.Now()
	metrics.GasUsed += gasUsed

	// Update global metrics
	m.globalMetrics.TotalPerforms++
	if success {
		m.globalMetrics.SuccessfulPerforms++
	} else {
		m.globalMetrics.FailedPerforms++
	}
	m.globalMetrics.AveragePerformLatency = calculateMovingAverage(m.globalMetrics.AveragePerformLatency, duration, m.globalMetrics.TotalPerforms)
	m.globalMetrics.LastPerformTime = time.Now()
	m.globalMetrics.GasUsed += gasUsed

	// Update histogram
	ms := duration.Milliseconds()
	m.performHistogram[ms]++

	// Generate alerts if needed
	if duration > m.alertThresholds["maxPerformLatency"].(time.Duration) {
		m.addAlert(PerformanceAlert{
			UpkeepID:    upkeepID,
			AlertType:   "high-perform-latency",
			Timestamp:   time.Now(),
			Description: "Perform operation exceeded maximum latency threshold",
			Severity:    "warning",
			Metrics: map[string]interface{}{
				"duration":  duration.String(),
				"threshold": m.alertThresholds["maxPerformLatency"].(time.Duration).String(),
				"upkeepID":  upkeepID,
			},
		})
	}

	// Alert on consecutive failures
	if !success && metrics.FailedPerforms >= uint64(m.alertThresholds["maxConsecutiveErrors"].(int)) {
		m.addAlert(PerformanceAlert{
			UpkeepID:    upkeepID,
			AlertType:   "consecutive-failures",
			Timestamp:   time.Now(),
			Description: "Upkeep experienced consecutive failures",
			Severity:    "critical",
			Metrics: map[string]interface{}{
				"failedPerforms": metrics.FailedPerforms,
				"threshold":      m.alertThresholds["maxConsecutiveErrors"],
				"upkeepID":       upkeepID,
			},
		})
	}
}

// GetMetrics gets metrics for an upkeep
func (m *Monitor) GetMetrics(upkeepID string) *PerformanceMetrics {
	m.mu.RLock()
	defer m.mu.RUnlock()

	metrics, ok := m.upkeepMetrics[upkeepID]
	if !ok {
		return &PerformanceMetrics{}
	}
	return metrics
}

// GetGlobalMetrics gets global metrics
func (m *Monitor) GetGlobalMetrics() *PerformanceMetrics {
	m.mu.RLock()
	defer m.mu.RUnlock()

	return m.globalMetrics
}

// GetAlerts gets all alerts
func (m *Monitor) GetAlerts() []PerformanceAlert {
	m.mu.RLock()
	defer m.mu.RUnlock()

	alerts := make([]PerformanceAlert, len(m.alerts))
	copy(alerts, m.alerts)
	return alerts
}

// ClearAlerts clears all alerts
func (m *Monitor) ClearAlerts() {
	m.mu.Lock()
	defer m.mu.Unlock()

	m.alerts = make([]PerformanceAlert, 0)
}

// addAlert adds an alert
func (m *Monitor) addAlert(alert PerformanceAlert) {
	m.alerts = append(m.alerts, alert)
}

// calculateMovingAverage calculates a moving average
func calculateMovingAverage(current time.Duration, newValue time.Duration, count uint64) time.Duration {
	if count <= 1 {
		return newValue
	}

	weight := float64(1) / float64(count)
	newAverage := float64(current)*(1-weight) + float64(newValue)*weight
	return time.Duration(newAverage)
}
