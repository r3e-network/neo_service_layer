package middleware

import (
	"net/http"
	"time"

	"github.com/sirupsen/logrus"
)

// RequestLogger is a middleware that logs HTTP requests
func RequestLogger(logger *logrus.Logger) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			start := time.Now()

			// Create a wrapper for the response writer to capture status code
			wrapper := &responseWriterWrapper{
				ResponseWriter: w,
				statusCode:     http.StatusOK, // Default status code
			}

			// Process the request
			next.ServeHTTP(wrapper, r)

			// Log request details
			logger.WithFields(logrus.Fields{
				"method":     r.Method,
				"path":       r.URL.Path,
				"status":     wrapper.statusCode,
				"duration":   time.Since(start).String(),
				"user_agent": r.UserAgent(),
				"remote_ip":  r.RemoteAddr,
			}).Info("HTTP request")
		})
	}
}

// responseWriterWrapper wraps http.ResponseWriter to capture the status code
type responseWriterWrapper struct {
	http.ResponseWriter
	statusCode int
}

// WriteHeader captures the status code
func (w *responseWriterWrapper) WriteHeader(statusCode int) {
	w.statusCode = statusCode
	w.ResponseWriter.WriteHeader(statusCode)
}

// ErrorLogger logs errors that occur during request processing
func ErrorLogger(logger *logrus.Logger) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			defer func() {
				if err := recover(); err != nil {
					logger.WithFields(logrus.Fields{
						"method":     r.Method,
						"path":       r.URL.Path,
						"error":      err,
						"user_agent": r.UserAgent(),
						"remote_ip":  r.RemoteAddr,
					}).Error("Request panic recovered")

					// Return a 500 Internal Server Error
					http.Error(w, http.StatusText(http.StatusInternalServerError), http.StatusInternalServerError)
				}
			}()

			next.ServeHTTP(w, r)
		})
	}
}
