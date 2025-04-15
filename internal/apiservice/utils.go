package api

import (
	"encoding/json"
	"errors"
	"net/http"
)

// Custom errors for better error handling
var (
	ErrInvalidRequest   = errors.New("invalid request")
	ErrPermissionDenied = errors.New("permission denied")
	ErrNotFound         = errors.New("not found")
	ErrInternalServer   = errors.New("internal server error")
)

// respondJSON responds with JSON
func respondJSON(w http.ResponseWriter, status int, data interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)

	if data != nil {
		// For empty responses like 204 No Content
		if err := json.NewEncoder(w).Encode(data); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
		}
	}
}

// respondError responds with a JSON error
func respondError(w http.ResponseWriter, status int, message, details string) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)

	response := APIError{
		Code:    status,
		Message: message,
		Details: details,
	}

	if err := json.NewEncoder(w).Encode(response); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
	}
}

// mapError maps service errors to HTTP status codes
func mapError(err error) (int, string, string) {
	switch {
	case errors.Is(err, functions.ErrFunctionNotFound) || errors.Is(err, secrets.ErrSecretNotFound):
		return http.StatusNotFound, "Not found", err.Error()
	case errors.Is(err, functions.ErrPermissionDenied):
		return http.StatusForbidden, "Permission denied", err.Error()
	case errors.Is(err, functions.ErrInvalidFunctionCode) || errors.Is(err, functions.ErrInvalidRuntime):
		return http.StatusBadRequest, "Invalid request", err.Error()
	default:
		return http.StatusInternalServerError, "Internal server error", err.Error()
	}
}
