package logger

import (
	"fmt"
	"io"
	"log"
	"os"
	"strings"
	"time"
)

// LogLevel represents logging level
type LogLevel int

// Log levels
const (
	DebugLevel LogLevel = iota
	InfoLevel
	WarnLevel
	ErrorLevel
)

// Logger represents a logger interface
type Logger interface {
	Debug(msg string, fields map[string]interface{})
	Info(msg string, fields map[string]interface{})
	Warn(msg string, fields map[string]interface{})
	Error(msg string, fields map[string]interface{})
	Fatal(msg string, fields map[string]interface{})
	WithField(key string, value interface{}) Logger
	WithFields(fields map[string]interface{}) Logger
}

// SimpleLogger represents a simple logger implementation
type SimpleLogger struct {
	level  LogLevel
	fields map[string]interface{}
	writer io.Writer
}

// NewLogger creates a new logger with the specified level
func NewLogger(level string) Logger {
	var logLevel LogLevel
	switch strings.ToLower(level) {
	case "debug":
		logLevel = DebugLevel
	case "info":
		logLevel = InfoLevel
	case "warn":
		logLevel = WarnLevel
	case "error":
		logLevel = ErrorLevel
	default:
		logLevel = InfoLevel
	}

	return &SimpleLogger{
		level:  logLevel,
		fields: make(map[string]interface{}),
		writer: os.Stdout,
	}
}

// Debug logs a debug message
func (l *SimpleLogger) Debug(message string, fields map[string]interface{}) {
	if l.level <= DebugLevel {
		l.log("DEBUG", message, fields)
	}
}

// Info logs an info message
func (l *SimpleLogger) Info(message string, fields map[string]interface{}) {
	if l.level <= InfoLevel {
		l.log("INFO", message, fields)
	}
}

// Warn logs a warning message
func (l *SimpleLogger) Warn(message string, fields map[string]interface{}) {
	if l.level <= WarnLevel {
		l.log("WARN", message, fields)
	}
}

// Error logs an error message
func (l *SimpleLogger) Error(message string, fields map[string]interface{}) {
	if l.level <= ErrorLevel {
		l.log("ERROR", message, fields)
	}
}

// Fatal logs a fatal message and exits the program
func (l *SimpleLogger) Fatal(msg string, fields map[string]interface{}) {
	l.log("FATAL", msg, fields)
	os.Exit(1)
}

// WithField returns a new logger with the field added
func (l *SimpleLogger) WithField(key string, value interface{}) Logger {
	newLogger := &SimpleLogger{
		level:  l.level,
		fields: make(map[string]interface{}),
		writer: l.writer,
	}

	// Copy existing fields
	for k, v := range l.fields {
		newLogger.fields[k] = v
	}

	// Add the new field
	newLogger.fields[key] = value

	return newLogger
}

// WithFields returns a new logger with the fields added
func (l *SimpleLogger) WithFields(fields map[string]interface{}) Logger {
	newLogger := &SimpleLogger{
		level:  l.level,
		fields: make(map[string]interface{}),
		writer: l.writer,
	}

	// Copy existing fields
	for k, v := range l.fields {
		newLogger.fields[k] = v
	}

	// Add the new fields
	for k, v := range fields {
		newLogger.fields[k] = v
	}

	return newLogger
}

// log writes a log message
func (l *SimpleLogger) log(level, message string, fields map[string]interface{}) {
	// Create a combined fields map
	combinedFields := make(map[string]interface{})
	for k, v := range l.fields {
		combinedFields[k] = v
	}
	for k, v := range fields {
		combinedFields[k] = v
	}

	// Format the message
	timestamp := time.Now().Format(time.RFC3339)
	logMessage := fmt.Sprintf("[%s] %s: %s", level, timestamp, message)

	// Add fields if any
	if len(combinedFields) > 0 {
		fieldStrings := make([]string, 0, len(combinedFields))
		for k, v := range combinedFields {
			fieldStrings = append(fieldStrings, fmt.Sprintf("%s=%v", k, v))
		}
		logMessage += " " + strings.Join(fieldStrings, " ")
	}

	// Write the log message
	log.Println(logMessage)
}

// SetWriter sets the writer for the logger
func (l *SimpleLogger) SetWriter(writer io.Writer) {
	l.writer = writer
}

// SetLevel sets the logging level
func (l *SimpleLogger) SetLevel(level LogLevel) {
	l.level = level
}

// GetLevel returns the current logging level
func (l *SimpleLogger) GetLevel() LogLevel {
	return l.level
}
