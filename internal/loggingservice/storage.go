package logging

import (
	"errors"
	"fmt"
	"os"
	"path/filepath"
	"strings"
	"sync"
	"time"

	"go.uber.org/zap"
	"go.uber.org/zap/zapcore"
	"gopkg.in/natefinch/lumberjack.v2"
)

// LogStorage defines the interface for log storage
type LogStorage interface {
	Store(entry LogEntry) error
	Query(query LogQuery) ([]LogEntry, error)
	DeleteBefore(time time.Time) error
	Close() error
}

// InMemoryStorage is an in-memory implementation of LogStorage
type InMemoryStorage struct {
	entries    []LogEntry
	indices    map[string]map[string][]int // Field -> Value -> []index
	mutex      sync.RWMutex
	maxEntries int
}

// NewInMemoryStorage creates a new in-memory log storage
func NewInMemoryStorage(maxEntries int) *InMemoryStorage {
	if maxEntries <= 0 {
		maxEntries = 10000 // Default value
	}

	storage := &InMemoryStorage{
		entries:    make([]LogEntry, 0, maxEntries),
		indices:    make(map[string]map[string][]int),
		maxEntries: maxEntries,
	}

	// Initialize indices
	storage.indices["level"] = make(map[string][]int)
	storage.indices["service"] = make(map[string][]int)

	return storage
}

// Store stores a log entry in memory
func (s *InMemoryStorage) Store(entry LogEntry) error {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	// Check if we need to evict old entries
	if len(s.entries) >= s.maxEntries {
		s.evictOldest()
	}

	// Add entry
	index := len(s.entries)
	s.entries = append(s.entries, entry)

	// Update indices
	s.updateIndices(entry, index)

	return nil
}

// Query queries log entries based on criteria
func (s *InMemoryStorage) Query(query LogQuery) ([]LogEntry, error) {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	result := make([]LogEntry, 0)
	candidates := s.findCandidates(query)

	// Apply filters
	for _, idx := range candidates {
		if idx >= len(s.entries) {
			continue
		}

		entry := s.entries[idx]

		// Check time range
		if !query.StartTime.IsZero() && entry.Timestamp.Before(query.StartTime) {
			continue
		}
		if !query.EndTime.IsZero() && entry.Timestamp.After(query.EndTime) {
			continue
		}

		// Check level
		if query.Level != "" && entry.Level != query.Level {
			continue
		}

		// Check service
		if query.Service != "" && entry.Service != query.Service {
			continue
		}

		// Check free text query
		if query.Query != "" {
			if !s.matchesQuery(entry, query.Query) {
				continue
			}
		}

		result = append(result, entry)
	}

	// Sort results based on sort criteria
	if query.SortBy != "" {
		result = s.sortEntries(result, query.SortBy, query.SortOrder == "desc")
	}

	// Apply pagination
	if query.Offset > 0 && query.Offset < len(result) {
		result = result[query.Offset:]
	}
	if query.Limit > 0 && query.Limit < len(result) {
		result = result[:query.Limit]
	}

	return result, nil
}

// DeleteBefore deletes entries older than a specific time
func (s *InMemoryStorage) DeleteBefore(t time.Time) error {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	if t.IsZero() {
		return errors.New("time cannot be zero")
	}

	newEntries := make([]LogEntry, 0)
	for _, entry := range s.entries {
		if entry.Timestamp.After(t) {
			newEntries = append(newEntries, entry)
		}
	}

	// Rebuild indices
	s.entries = newEntries
	s.rebuildIndices()

	return nil
}

// Close closes the storage (no-op for in-memory)
func (s *InMemoryStorage) Close() error {
	return nil
}

// findCandidates finds candidate entries based on query
func (s *InMemoryStorage) findCandidates(query LogQuery) []int {
	// Use indexed fields for faster lookup
	var candidates []int

	if query.Level != "" && len(s.indices["level"][query.Level]) > 0 {
		return s.indices["level"][query.Level]
	}

	if query.Service != "" && len(s.indices["service"][query.Service]) > 0 {
		return s.indices["service"][query.Service]
	}

	// Fallback to all entries
	candidates = make([]int, len(s.entries))
	for i := range s.entries {
		candidates[i] = i
	}

	return candidates
}

// matchesQuery checks if an entry matches a query string
func (s *InMemoryStorage) matchesQuery(entry LogEntry, query string) bool {
	// Simple implementation - just check if the query is in the message
	return strings.Contains(strings.ToLower(entry.Message), strings.ToLower(query))
}

// sortEntries sorts entries based on a field
func (s *InMemoryStorage) sortEntries(entries []LogEntry, field string, desc bool) []LogEntry {
	// Simple implementation - only sort by timestamp
	if field == "timestamp" {
		if desc {
			for i := 0; i < len(entries); i++ {
				for j := i + 1; j < len(entries); j++ {
					if entries[i].Timestamp.Before(entries[j].Timestamp) {
						entries[i], entries[j] = entries[j], entries[i]
					}
				}
			}
		} else {
			for i := 0; i < len(entries); i++ {
				for j := i + 1; j < len(entries); j++ {
					if entries[i].Timestamp.After(entries[j].Timestamp) {
						entries[i], entries[j] = entries[j], entries[i]
					}
				}
			}
		}
	}

	return entries
}

// evictOldest evicts the oldest entry
func (s *InMemoryStorage) evictOldest() {
	if len(s.entries) == 0 {
		return
	}

	// Remove the oldest entry (assuming entries are in chronological order)
	s.entries = s.entries[1:]

	// Rebuild indices
	s.rebuildIndices()
}

// updateIndices updates indices for a new entry
func (s *InMemoryStorage) updateIndices(entry LogEntry, index int) {
	// Update level index
	levelIndices, ok := s.indices["level"][entry.Level]
	if !ok {
		levelIndices = make([]int, 0)
	}
	s.indices["level"][entry.Level] = append(levelIndices, index)

	// Update service index
	serviceIndices, ok := s.indices["service"][entry.Service]
	if !ok {
		serviceIndices = make([]int, 0)
	}
	s.indices["service"][entry.Service] = append(serviceIndices, index)
}

// rebuildIndices rebuilds all indices
func (s *InMemoryStorage) rebuildIndices() {
	// Clear indices
	s.indices = make(map[string]map[string][]int)
	s.indices["level"] = make(map[string][]int)
	s.indices["service"] = make(map[string][]int)

	// Rebuild indices
	for i, entry := range s.entries {
		s.updateIndices(entry, i)
	}
}

// FileStorage is a file-based implementation of LogStorage using zap and lumberjack
type FileStorage struct {
	logger *zap.Logger
	config Config       // Store config for query/delete logic if needed
	closer func() error // Function to close/sync the logger
}

// NewFileStorage creates a new file-based log storage
func NewFileStorage(config *Config) (*FileStorage, error) {
	if config.LogFilePath == "" {
		return nil, errors.New("LogFilePath is required for FileStorage")
	}

	// Ensure log directory exists
	logDir := filepath.Dir(config.LogFilePath)
	if err := os.MkdirAll(logDir, 0755); err != nil {
		return nil, fmt.Errorf("failed to create log directory %s: %w", logDir, err)
	}

	// Configure lumberjack for rotation
	ljLogger := &lumberjack.Logger{
		Filename:   config.LogFilePath,
		MaxSize:    config.MaxSizeInMB, // megabytes
		MaxBackups: config.RetainedFiles,
		MaxAge:     0, // Retain based on number of files (MaxBackups)
		Compress:   config.EnableCompression,
		LocalTime:  true,
	}

	// Configure zap encoder
	encoderCfg := zap.NewProductionEncoderConfig()
	encoderCfg.TimeKey = "timestamp" // Match LogEntry field
	encoderCfg.EncodeTime = zapcore.ISO8601TimeEncoder
	var encoder zapcore.Encoder
	if config.EnableJSONLogs {
		encoder = zapcore.NewJSONEncoder(encoderCfg)
	} else {
		encoder = zapcore.NewConsoleEncoder(encoderCfg) // Or use TextFormatter simulation?
	}

	// Configure zap core
	logLevel := zapcore.InfoLevel // Default
	if err := logLevel.UnmarshalText([]byte(config.LogLevel)); err != nil {
		// Log warning? Use default.
	}
	core := zapcore.NewCore(
		encoder,
		zapcore.AddSync(ljLogger),
		logLevel,
	)

	// Create zap logger
	logger := zap.New(core)

	closerFunc := func() error {
		// Sync flushes buffered logs
		err := logger.Sync()
		_ = ljLogger.Close() // Close the underlying file
		return err
	}

	return &FileStorage{logger: logger, config: *config, closer: closerFunc}, nil
}

// Store stores a log entry in the file
func (s *FileStorage) Store(entry LogEntry) error {
	// Convert context map to zap fields
	zapFields := make([]zap.Field, 0, len(entry.Context)+2)
	zapFields = append(zapFields, zap.String("service", entry.Service))
	// Maybe add entry.ID?
	// zapFields = append(zapFields, zap.String("log_id", entry.ID))
	for k, v := range entry.Context {
		zapFields = append(zapFields, zap.Any(k, v))
	}

	// Log based on level
	switch ParseLogLevel(entry.Level) { // Use helper from service.go
	case LogLevelDebug:
		s.logger.Debug(entry.Message, zapFields...)
	case LogLevelInfo:
		s.logger.Info(entry.Message, zapFields...)
	case LogLevelWarn:
		s.logger.Warn(entry.Message, zapFields...)
	case LogLevelError:
		s.logger.Error(entry.Message, zapFields...)
	default:
		s.logger.Info(entry.Message, zapFields...) // Default to Info
	}

	return nil
}

// Query queries log entries from the file
func (s *FileStorage) Query(query LogQuery) ([]LogEntry, error) {
	// Implementing efficient querying on rotated log files is complex.
	// Requires reading potentially multiple files, parsing (JSON/text),
	// filtering by timestamp, level, service, context, and free text.
	// This often justifies using external log aggregation systems (ELK, Loki, etc.).
	// Returning "not implemented" for now.
	return nil, errors.New("querying file storage is not implemented")
}

// DeleteBefore deletes entries older than a specific time
func (s *FileStorage) DeleteBefore(t time.Time) error {
	// Log rotation is handled by lumberjack based on size/count.
	// Manual deletion by timestamp is not directly supported by this setup.
	return errors.New("deleting logs by time is not supported by file storage implementation")
}

// Close closes the file logger
func (s *FileStorage) Close() error {
	if s.closer != nil {
		return s.closer()
	}
	return nil
}
