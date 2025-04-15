package logging

import (
	"encoding/json"
	"fmt"
	"time"
)

// LogFormatter formats log entries for output
type LogFormatter interface {
	Format(entry LogEntry) ([]byte, error)
}

// TextFormatter formats log entries as text
type TextFormatter struct {
	IncludeTimestamp bool
	TimeFormat       string
	IncludeLevel     bool
	IncludeService   bool
}

// NewTextFormatter creates a new text formatter
func NewTextFormatter() *TextFormatter {
	return &TextFormatter{
		IncludeTimestamp: true,
		TimeFormat:       "2006-01-02 15:04:05.000",
		IncludeLevel:     true,
		IncludeService:   true,
	}
}

// Format formats a log entry as text
func (f *TextFormatter) Format(entry LogEntry) ([]byte, error) {
	var parts []string

	if f.IncludeTimestamp {
		parts = append(parts, entry.Timestamp.Format(f.TimeFormat))
	}

	if f.IncludeLevel {
		parts = append(parts, fmt.Sprintf("[%s]", entry.Level))
	}

	if f.IncludeService && entry.Service != "" {
		parts = append(parts, fmt.Sprintf("[%s]", entry.Service))
	}

	parts = append(parts, entry.Message)

	if len(entry.Context) > 0 {
		contextJSON, err := json.Marshal(entry.Context)
		if err != nil {
			return nil, err
		}
		parts = append(parts, string(contextJSON))
	}

	return []byte(joinParts(parts)), nil
}

// JSONFormatter formats log entries as JSON
type JSONFormatter struct {
	IncludeID        bool
	IncludeTimestamp bool
	IncludeLevel     bool
	IncludeService   bool
}

// NewJSONFormatter creates a new JSON formatter
func NewJSONFormatter() *JSONFormatter {
	return &JSONFormatter{
		IncludeID:        true,
		IncludeTimestamp: true,
		IncludeLevel:     true,
		IncludeService:   true,
	}
}

// Format formats a log entry as JSON
func (f *JSONFormatter) Format(entry LogEntry) ([]byte, error) {
	data := make(map[string]interface{})

	if f.IncludeID {
		data["id"] = entry.ID
	}

	if f.IncludeTimestamp {
		data["timestamp"] = entry.Timestamp.Format(time.RFC3339Nano)
	}

	if f.IncludeLevel {
		data["level"] = entry.Level
	}

	if f.IncludeService && entry.Service != "" {
		data["service"] = entry.Service
	}

	data["message"] = entry.Message

	if len(entry.Context) > 0 {
		data["context"] = entry.Context
	}

	return json.Marshal(data)
}

// CustomFormatter allows for custom formatting of log entries
type CustomFormatter struct {
	FormatFunc func(entry LogEntry) ([]byte, error)
}

// NewCustomFormatter creates a new custom formatter
func NewCustomFormatter(formatFunc func(entry LogEntry) ([]byte, error)) *CustomFormatter {
	return &CustomFormatter{
		FormatFunc: formatFunc,
	}
}

// Format formats a log entry using the custom format function
func (f *CustomFormatter) Format(entry LogEntry) ([]byte, error) {
	return f.FormatFunc(entry)
}

// joinParts joins parts with spaces
func joinParts(parts []string) string {
	result := ""
	for i, part := range parts {
		if i > 0 {
			result += " "
		}
		result += part
	}
	return result
}