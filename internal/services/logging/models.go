package logging

import (
	"time"
)

// Config holds the configuration for the Logging service
type Config struct {
	LogLevel          string // Minimum log level to record (debug, info, warn, error)
	EnableJSONLogs    bool   // Whether to format logs as JSON
	LogFilePath       string // Path to the log file
	MaxSizeInMB       int    // Maximum size of log files before rotation
	RetainedFiles     int    // Number of rotated log files to retain
	EnableCompression bool   // Whether to compress rotated log files
}

// LogLevel represents the severity level of a log entry
type LogLevel string

const (
	// DebugLevel represents debug log level
	DebugLevel LogLevel = "debug"

	// InfoLevel represents info log level
	InfoLevel LogLevel = "info"

	// WarnLevel represents warning log level
	WarnLevel LogLevel = "warn"

	// ErrorLevel represents error log level
	ErrorLevel LogLevel = "error"
)

// LogEntry represents a single log record
type LogEntry struct {
	ID        string                 // Unique identifier
	Timestamp time.Time              // When the log was created
	Level     string                 // Log level (debug, info, warn, error)
	Message   string                 // Log message
	Service   string                 // Source service
	Context   map[string]interface{} // Additional context
}

// LogQuery represents a query for logs
type LogQuery struct {
	Level     string    // Filter by log level
	Service   string    // Filter by service
	StartTime time.Time // Filter by start time
	EndTime   time.Time // Filter by end time
	Query     string    // Free text query
	Limit     int       // Maximum number of results
	Offset    int       // Offset for pagination
	SortBy    string    // Field to sort by
	SortOrder string    // Sort order (asc or desc)
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
