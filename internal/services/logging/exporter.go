package logging

import (
	"encoding/json"
	"fmt"
	"io"
	"os"
	"time"
)

// LogExporter exports logs to various destinations
type LogExporter interface {
	Export(entry LogEntry) error
	Close() error
}

// FileExporter exports logs to a file
type FileExporter struct {
	file         *os.File
	jsonEncoder  *json.Encoder
	enableJSON   bool
	rotationSize int64
	maxSize      int64
	filePath     string
}

// NewFileExporter creates a new file exporter
func NewFileExporter(filePath string, enableJSON bool, maxSizeInMB int) (*FileExporter, error) {
	file, err := os.OpenFile(filePath, os.O_CREATE|os.O_WRONLY|os.O_APPEND, 0644)
	if err != nil {
		return nil, fmt.Errorf("failed to open log file: %w", err)
	}

	exporter := &FileExporter{
		file:         file,
		jsonEncoder:  json.NewEncoder(file),
		enableJSON:   enableJSON,
		rotationSize: 0,
		maxSize:      int64(maxSizeInMB) * 1024 * 1024,
		filePath:     filePath,
	}

	return exporter, nil
}

// Export exports a log entry to the file
func (e *FileExporter) Export(entry LogEntry) error {
	var err error
	var bytesWritten int64

	if e.enableJSON {
		err = e.jsonEncoder.Encode(entry)
		bytesWritten = int64(estimateJSONSize(entry))
	} else {
		var line string
		line = formatLogEntry(entry)
		bytesWritten = int64(len(line))
		_, err = fmt.Fprintln(e.file, line)
	}

	if err != nil {
		return fmt.Errorf("failed to write log entry: %w", err)
	}

	// Check if rotation is needed
	e.rotationSize += bytesWritten
	if e.rotationSize >= e.maxSize {
		if err := e.rotate(); err != nil {
			return fmt.Errorf("failed to rotate log file: %w", err)
		}
	}

	return nil
}

// Close closes the file
func (e *FileExporter) Close() error {
	return e.file.Close()
}

// rotate rotates the log file
func (e *FileExporter) rotate() error {
	// Close current file
	if err := e.file.Close(); err != nil {
		return err
	}

	// Rename file with timestamp
	timestamp := time.Now().Format("20060102-150405")
	newPath := fmt.Sprintf("%s.%s", e.filePath, timestamp)
	if err := os.Rename(e.filePath, newPath); err != nil {
		return err
	}

	// Open new file
	file, err := os.OpenFile(e.filePath, os.O_CREATE|os.O_WRONLY|os.O_APPEND, 0644)
	if err != nil {
		return err
	}

	// Update exporter
	e.file = file
	e.jsonEncoder = json.NewEncoder(file)
	e.rotationSize = 0

	return nil
}

// ConsoleExporter exports logs to the console
type ConsoleExporter struct {
	writer      io.Writer
	jsonEncoder *json.Encoder
	enableJSON  bool
}

// NewConsoleExporter creates a new console exporter
func NewConsoleExporter(enableJSON bool) *ConsoleExporter {
	return &ConsoleExporter{
		writer:      os.Stdout,
		jsonEncoder: json.NewEncoder(os.Stdout),
		enableJSON:  enableJSON,
	}
}

// Export exports a log entry to the console
func (e *ConsoleExporter) Export(entry LogEntry) error {
	if e.enableJSON {
		return e.jsonEncoder.Encode(entry)
	}

	line := formatLogEntry(entry)
	_, err := fmt.Fprintln(e.writer, line)
	return err
}

// Close closes the exporter (no-op for console)
func (e *ConsoleExporter) Close() error {
	return nil
}

// MultiExporter exports logs to multiple destinations
type MultiExporter struct {
	exporters []LogExporter
}

// NewMultiExporter creates a new multi-exporter
func NewMultiExporter(exporters ...LogExporter) *MultiExporter {
	return &MultiExporter{
		exporters: exporters,
	}
}

// Export exports a log entry to all destinations
func (e *MultiExporter) Export(entry LogEntry) error {
	for _, exporter := range e.exporters {
		if err := exporter.Export(entry); err != nil {
			return err
		}
	}

	return nil
}

// Close closes all exporters
func (e *MultiExporter) Close() error {
	for _, exporter := range e.exporters {
		if err := exporter.Close(); err != nil {
			return err
		}
	}

	return nil
}

// formatLogEntry formats a log entry as a string
func formatLogEntry(entry LogEntry) string {
	timestamp := entry.Timestamp.Format("2006-01-02 15:04:05.000")
	var contextStr string
	if len(entry.Context) > 0 {
		contextJSON, _ := json.Marshal(entry.Context)
		contextStr = string(contextJSON)
	}

	return fmt.Sprintf("[%s] %s: %s (%s) %s", timestamp, entry.Level, entry.Message, entry.Service, contextStr)
}

// estimateJSONSize estimates the size of a JSON-encoded log entry
func estimateJSONSize(entry LogEntry) int {
	// This is a rough estimate
	size := 100 // Base size for timestamps, level, etc.
	size += len(entry.Message)
	size += len(entry.Service)
	size += len(entry.ID)

	for k, v := range entry.Context {
		size += len(k)
		switch val := v.(type) {
		case string:
			size += len(val)
		default:
			size += 10 // Rough estimate for non-string values
		}
	}

	return size
}