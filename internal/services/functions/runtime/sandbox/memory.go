package sandbox

import (
	"runtime"
	"sync"
	"time"

	"go.uber.org/zap"
)

// MemoryMonitor tracks memory usage of the sandbox
type MemoryMonitor struct {
	// Memory limit in bytes
	memoryLimit uint64

	// Channel to signal monitoring to stop
	stopCh chan struct{}

	// Current memory usage in bytes
	currentUsage uint64

	// Logger for memory monitoring
	logger *zap.Logger

	// Function to call when memory limit is exceeded
	limitExceededFn func()

	// Mutex for thread safety
	mutex sync.Mutex

	// Flag to track if monitor is running
	running bool
}

// NewMemoryMonitor creates a new memory monitor
func NewMemoryMonitor(memoryLimit uint64, logger *zap.Logger, limitExceededFn func()) *MemoryMonitor {
	// Use a no-op logger if none is provided
	if logger == nil {
		logger = zap.NewNop()
	}

	return &MemoryMonitor{
		memoryLimit:     memoryLimit,
		logger:          logger,
		limitExceededFn: limitExceededFn,
		currentUsage:    0,
		running:         false,
	}
}

// Start begins memory monitoring
func (m *MemoryMonitor) Start() {
	m.mutex.Lock()
	defer m.mutex.Unlock()

	// If already running, stop it first
	if m.running {
		close(m.stopCh)
	}

	// Create new stop channel
	m.stopCh = make(chan struct{})
	m.running = true

	// Start monitoring in a goroutine
	go m.monitorMemory()
}

// Stop ends memory monitoring
func (m *MemoryMonitor) Stop() {
	m.mutex.Lock()
	defer m.mutex.Unlock()

	// Only close the channel if we're running
	if m.running {
		close(m.stopCh)
		m.running = false
	}
}

// GetCurrentUsage returns the current memory usage in bytes
func (m *MemoryMonitor) GetCurrentUsage() uint64 {
	m.mutex.Lock()
	defer m.mutex.Unlock()

	// Get current memory stats if memory usage is zero
	if m.currentUsage == 0 {
		var memStats runtime.MemStats
		runtime.ReadMemStats(&memStats)
		// Use heap alloc as the current usage
		m.currentUsage = memStats.HeapAlloc
	}

	return m.currentUsage
}

// monitorMemory continuously checks memory usage
func (m *MemoryMonitor) monitorMemory() {
	// Define ticker for memory checks
	ticker := time.NewTicker(MemoryCheckInterval)
	defer ticker.Stop()

	for {
		select {
		case <-m.stopCh:
			// Stop monitoring
			return
		case <-ticker.C:
			// Get current memory usage
			var memStats runtime.MemStats
			runtime.ReadMemStats(&memStats)

			// Update current usage
			m.mutex.Lock()
			m.currentUsage = memStats.HeapAlloc
			m.mutex.Unlock()

			// Check if memory limit is exceeded
			if memStats.HeapAlloc > m.memoryLimit {
				m.logger.Warn("Memory limit exceeded",
					zap.Uint64("memoryLimit", m.memoryLimit),
					zap.Uint64("currentUsage", memStats.HeapAlloc))

				// Call the limit exceeded function if provided
				if m.limitExceededFn != nil {
					m.limitExceededFn()
				}
			}
		}
	}
}
