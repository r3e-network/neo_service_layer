# Sandbox Memory Monitoring

## Overview

The Neo Service Layer sandbox includes a memory monitoring system that tracks and limits the memory usage of JavaScript functions executing within the sandbox environment. This document describes the memory monitoring implementation, including recent improvements to address race conditions and prevent deadlocks.

## Memory Monitoring Implementation

### Key Components

- **Memory Limit**: Configurable maximum memory allowed for a JavaScript function execution
- **Memory Check Interval**: Time between memory usage checks (default: 100ms)
- **Monitoring Goroutine**: Background process that periodically checks memory usage
- **Interrupt Mechanism**: System to terminate function execution when memory limits are exceeded

### Thread Safety Improvements

The memory monitoring system has been enhanced with the following thread safety improvements:

1. **Mutex Protection**: All shared state access is now protected by a mutex to prevent race conditions
2. **Initialization Safety**: The monitoring system now checks if monitoring is already active before starting
3. **Cleanup Safety**: The stopMemCheck channel is properly closed and reset to nil to prevent double-close panics
4. **Deadlock Prevention**: Memory usage checks and interruption signals are handled in separate mutex-protected blocks to prevent deadlocks

### Implementation Details

#### Starting Memory Monitoring

```go
// startMemoryMonitoring starts a goroutine to monitor memory usage
func (s *Sandbox) startMemoryMonitoring() {
    s.mutex.Lock()
    if s.stopMemCheck != nil {
        s.mutex.Unlock()
        return
    }
    s.stopMemCheck = make(chan struct{})
    s.mutex.Unlock()
    
    go func() {
        ticker := time.NewTicker(MemoryCheckInterval)
        defer ticker.Stop()
        
        var memStats runtime_pkg.MemStats
        
        for {
            select {
            case <-ticker.C:
                runtime_pkg.ReadMemStats(&memStats)
                
                func() {
                    s.mutex.Lock()
                    defer s.mutex.Unlock()
                    s.memoryUsed = int64(memStats.Alloc)
                    
                    // Check if memory limit exceeded
                    if s.memoryUsed > s.memoryLimit {
                        s.logger.Warn("Memory limit exceeded",
                            zap.Int64("memoryUsed", s.memoryUsed),
                            zap.Int64("memoryLimit", s.memoryLimit))
                        s.interrupted = true
                    }
                }()
                
                // Check if we need to exit after releasing the lock
                if func() bool {
                    s.mutex.Lock()
                    defer s.mutex.Unlock()
                    return s.interrupted
                }() {
                    return
                }
            case <-s.stopMemCheck:
                return
            }
        }
    }()
}
```

#### Stopping Memory Monitoring

```go
// stopMemoryMonitoring stops the memory monitoring goroutine
func (s *Sandbox) stopMemoryMonitoring() {
    s.mutex.Lock()
    defer s.mutex.Unlock()
    
    if s.stopMemCheck != nil {
        close(s.stopMemCheck)
        s.stopMemCheck = nil
    }
}
```

## Usage in Tests

When testing the sandbox functionality, it's important to ensure that memory monitoring is properly started and stopped for each test case. The test suite includes comprehensive tests for memory monitoring, including:

1. Basic memory usage tracking
2. Memory limit enforcement
3. Proper cleanup after execution

## Race Condition Prevention

The implementation addresses several potential race conditions:

1. **Concurrent Access to Memory Usage**: Protected by mutex
2. **Concurrent Start/Stop**: Prevents multiple monitoring goroutines
3. **Channel Close Safety**: Prevents double-close panics
4. **Deadlock Prevention**: Uses separate mutex-protected blocks for checking and acting on memory usage

These improvements ensure that the memory monitoring system is thread-safe and reliable, even in high-concurrency environments.
