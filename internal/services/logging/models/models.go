package models

import (
	"strings"
	"time"
)

// --- Log Level ---
type LogLevel int

const (
	LogLevelDebug LogLevel = iota
	LogLevelInfo
	LogLevelWarn
	LogLevelError
)

func (l LogLevel) String() string {
	switch l {
	case LogLevelDebug:
		return "debug"
	case LogLevelInfo:
		return "info"
	case LogLevelWarn:
		return "warn"
	case LogLevelError:
		return "error"
	default:
		return "unknown"
	}
}

func ParseLogLevel(levelStr string) LogLevel {
	switch strings.ToLower(levelStr) {
	case "debug":
		return LogLevelDebug
	case "info":
		return LogLevelInfo
	case "warn":
		return LogLevelWarn
	case "error":
		return LogLevelError
	default:
		return LogLevelInfo // Default level
	}
}

// --- Other Models ---

// Config represents the configuration for the logging service
type Config struct {
	LogLevel          string // Minimum log level to record (debug, info, warn, error)
	EnableJSONLogs    bool   // Whether to format logs as JSON
	LogFilePath       string // Path to the log file (enables FileStorage)
	MaxSizeInMB       int    // Maximum size of log files before rotation
	RetainedFiles     int    // Number of rotated log files to retain
	EnableCompression bool   // Whether to compress rotated log files
	// StorageBackend field removed, deduced from LogFilePath
	// StorageConfig field removed, specific configs passed directly
}

// LogEntry represents a single log record
type LogEntry struct {
	ID        string                 `json:"id"`
	Timestamp time.Time              `json:"timestamp"`
	Level     string                 `json:"level"` // Use string representation
	Message   string                 `json:"message"`
	Service   string                 `json:"service"`
	Context   map[string]interface{} `json:"context"`
}

// LogQuery represents criteria for querying logs
type LogQuery struct {
	Query     string    `json:"query"`     // Free text search query
	Level     string    `json:"level"`     // Filter by log level
	Service   string    `json:"service"`   // Filter by service name
	StartTime time.Time `json:"startTime"` // Start of time range
	EndTime   time.Time `json:"endTime"`   // End of time range
	Limit     int       `json:"limit"`     // Max number of results
	Offset    int       `json:"offset"`    // Offset for pagination
	SortBy    string    `json:"sortBy"`    // Field to sort by (e.g., "timestamp")
	SortOrder string    `json:"sortOrder"` // "asc" or "desc"
}

// LogStats represents statistics about logs
type LogStats struct {
	TotalLogs     int            // Total number of logs
	LogsByLevel   map[string]int // Logs by level
	LogsByService map[string]int // Logs by service
	ErrorRate     float64        // Error rate
	LogsPerMinute float64        // Logs per minute
	AverageSize   int            // Average log size in bytes
}

// LogAlert represents an alert based on log patterns
type LogAlert struct {
	ID            string            // Unique identifier
	Name          string            // Alert name
	Description   string            // Alert description
	Query         string            // Query to match logs
	Threshold     int               // Threshold for alerting
	Window        time.Duration     // Time window for threshold
	Actions       []LogAlertAction  // Actions to take when alert fires
	CreatedAt     time.Time         // When the alert was created
	UpdatedAt     time.Time         // When the alert was last updated
	LastTriggered time.Time         // When the alert was last triggered
	Status        string            // Alert status
	Labels        map[string]string // Labels for the alert
}

// LogAlertAction represents an action to take when an alert fires
type LogAlertAction struct {
	Type   string                 // Action type (email, webhook, etc.)
	Config map[string]interface{} // Action configuration
}

// LogRetentionPolicy represents a policy for log retention
type LogRetentionPolicy struct {
	ID          string        // Unique identifier
	Name        string        // Policy name
	Description string        // Policy description
	Duration    time.Duration // How long to retain logs
	Query       string        // Query to match logs
	CreatedAt   time.Time     // When the policy was created
	UpdatedAt   time.Time     // When the policy was last updated
}

// LogExportJob represents a job to export logs
type LogExportJob struct {
	ID          string    // Unique identifier
	Query       string    // Query to match logs
	StartTime   time.Time // Export logs from this time
	EndTime     time.Time // Export logs until this time
	Format      string    // Export format (json, csv, etc.)
	Status      string    // Job status
	CreatedAt   time.Time // When the job was created
	CompletedAt time.Time // When the job was completed
	FileURL     string    // URL to the exported file
}

// LogStreamSubscription represents a subscription to a log stream
type LogStreamSubscription struct {
	ID          string                 // Unique identifier
	Name        string                 // Subscription name
	Description string                 // Subscription description
	Query       string                 // Query to match logs
	Destination string                 // Destination for logs
	Config      map[string]interface{} // Subscription configuration
	CreatedAt   time.Time              // When the subscription was created
	UpdatedAt   time.Time              // When the subscription was last updated
	Status      string                 // Subscription status
}
