package api

import (
	"encoding/json"
	"net/http"
	"strconv"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/pkg/errors"
	"github.com/will/neo_service_layer/internal/services/functions"
)

// handleHealthCheck handles GET /health
func (s *Service) handleHealthCheck(w http.ResponseWriter, r *http.Request) {
	s.healthMu.RLock()
	healthStatuses := make([]HealthStatus, 0, len(s.healthChecks))
	for _, checkFn := range s.healthChecks {
		healthStatuses = append(healthStatuses, checkFn())
	}
	s.healthMu.RUnlock()

	healthy := true
	for _, status := range healthStatuses {
		if status.Status != "healthy" {
			healthy = false
			break
		}
	}

	resp := SystemHealthResponse{
		Healthy:   healthy,
		Services:  healthStatuses,
		Uptime:    int64(time.Since(s.stats.LastUpdated).Seconds()),
		Region:    "default",
		Timestamp: time.Now(),
		Version:   "1.0.0",
	}

	respondJSON(w, http.StatusOK, resp)
}

// handleGetStats handles GET /stats
func (s *Service) handleGetStats(w http.ResponseWriter, r *http.Request) {
	s.statsMu.RLock()
	stats := s.stats
	s.statsMu.RUnlock()

	respondJSON(w, http.StatusOK, stats)
}

// handleVerifySignature handles POST /api/v1/auth/verify
func (s *Service) handleVerifySignature(w http.ResponseWriter, r *http.Request) {
	var req SignatureVerificationRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondError(w, http.StatusBadRequest, "Invalid request body", err.Error())
		return
	}

	// Validate request
	if req.Address == "" || req.Message == "" || req.Signature == "" {
		respondError(w, http.StatusBadRequest, "Missing required fields", "Address, message, and signature are required")
		return
	}

	// In a real implementation, this would verify the signature against the address
	// Here we're just providing a placeholder implementation

	// Convert address to script hash
	scriptHash, err := util.Uint160DecodeStringLE(req.Address)
	if err != nil {
		respondError(w, http.StatusBadRequest, "Invalid address", err.Error())
		return
	}

	// Generate JWT token
	_, tokenString, err := s.tokenAuth.Encode(map[string]interface{}{
		"address": req.Address,
		"exp":     time.Now().Add(s.config.JWTExpiryDuration).Unix(),
	})
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to generate token", err.Error())
		return
	}

	// Build response
	resp := SignatureVerificationResponse{
		Valid:      true,
		Address:    req.Address,
		ScriptHash: scriptHash,
	}

	// Set token as header
	w.Header().Set("Authorization", "Bearer "+tokenString)

	respondJSON(w, http.StatusOK, resp)
}

// handleCreateFunction handles POST /api/v1/functions
func (s *Service) handleCreateFunction(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	var req FunctionRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondError(w, http.StatusBadRequest, "Invalid request body", err.Error())
		return
	}

	// Validate request
	if req.Name == "" || req.Code == "" {
		respondError(w, http.StatusBadRequest, "Missing required fields", "Name and code are required")
		return
	}

	// Create function
	runtime := functions.Runtime(req.Runtime)
	if runtime == "" {
		runtime = functions.JavaScriptRuntime
	}

	function, err := s.functionsSvc.CreateFunction(r.Context(), address, req.Name, req.Description, req.Code, runtime)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to create function", err.Error())
		return
	}

	respondJSON(w, http.StatusCreated, function)
}

// handleListFunctions handles GET /api/v1/functions
func (s *Service) handleListFunctions(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// List functions
	functions, err := s.functionsSvc.ListFunctions(r.Context(), address)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to list functions", err.Error())
		return
	}

	respondJSON(w, http.StatusOK, functions)
}

// handleGetFunction handles GET /api/v1/functions/{id}
func (s *Service) handleGetFunction(w http.ResponseWriter, r *http.Request) {
	// Get function ID from path
	functionID := chi.URLParam(r, "id")
	if functionID == "" {
		respondError(w, http.StatusBadRequest, "Missing function ID", "Function ID is required")
		return
	}

	// Get function
	function, err := s.functionsSvc.GetFunction(r.Context(), functionID)
	if err != nil {
		if errors.Is(err, functions.ErrFunctionNotFound) {
			respondError(w, http.StatusNotFound, "Function not found", err.Error())
		} else {
			respondError(w, http.StatusInternalServerError, "Failed to get function", err.Error())
		}
		return
	}

	respondJSON(w, http.StatusOK, function)
}

// handleUpdateFunction handles PUT /api/v1/functions/{id}
func (s *Service) handleUpdateFunction(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get function ID from path
	functionID := chi.URLParam(r, "id")
	if functionID == "" {
		respondError(w, http.StatusBadRequest, "Missing function ID", "Function ID is required")
		return
	}

	var req FunctionRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondError(w, http.StatusBadRequest, "Invalid request body", err.Error())
		return
	}

	// Build updates
	updates := make(map[string]interface{})
	if req.Description != "" {
		updates["description"] = req.Description
	}
	if req.Code != "" {
		updates["code"] = req.Code
	}
	if req.Metadata != nil {
		updates["metadata"] = req.Metadata
	}

	// Update function
	function, err := s.functionsSvc.UpdateFunction(r.Context(), functionID, address, updates)
	if err != nil {
		if errors.Is(err, functions.ErrFunctionNotFound) {
			respondError(w, http.StatusNotFound, "Function not found", err.Error())
		} else if errors.Is(err, functions.ErrPermissionDenied) {
			respondError(w, http.StatusForbidden, "Permission denied", err.Error())
		} else {
			respondError(w, http.StatusInternalServerError, "Failed to update function", err.Error())
		}
		return
	}

	respondJSON(w, http.StatusOK, function)
}

// handleDeleteFunction handles DELETE /api/v1/functions/{id}
func (s *Service) handleDeleteFunction(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get function ID from path
	functionID := chi.URLParam(r, "id")
	if functionID == "" {
		respondError(w, http.StatusBadRequest, "Missing function ID", "Function ID is required")
		return
	}

	// Delete function
	err := s.functionsSvc.DeleteFunction(r.Context(), functionID, address)
	if err != nil {
		if errors.Is(err, functions.ErrFunctionNotFound) {
			respondError(w, http.StatusNotFound, "Function not found", err.Error())
		} else if errors.Is(err, functions.ErrPermissionDenied) {
			respondError(w, http.StatusForbidden, "Permission denied", err.Error())
		} else {
			respondError(w, http.StatusInternalServerError, "Failed to delete function", err.Error())
		}
		return
	}

	respondJSON(w, http.StatusNoContent, nil)
}

// handleInvokeFunction handles POST /api/v1/functions/{id}/invoke
func (s *Service) handleInvokeFunction(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get function ID from path
	functionID := chi.URLParam(r, "id")
	if functionID == "" {
		respondError(w, http.StatusBadRequest, "Missing function ID", "Function ID is required")
		return
	}

	var req FunctionInvocationRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondError(w, http.StatusBadRequest, "Invalid request body", err.Error())
		return
	}

	// Override function ID from path
	req.FunctionID = functionID

	// Create invocation request
	invocation := functions.FunctionInvocation{
		FunctionID:  functionID,
		Parameters:  req.Parameters,
		Async:       req.Async,
		Caller:      address,
		TraceID:     req.TraceID,
		Idempotency: req.Idempotency,
	}

	// Invoke function
	execution, err := s.functionsSvc.InvokeFunction(r.Context(), invocation)
	if err != nil {
		if errors.Is(err, functions.ErrFunctionNotFound) {
			respondError(w, http.StatusNotFound, "Function not found", err.Error())
		} else if errors.Is(err, functions.ErrPermissionDenied) {
			respondError(w, http.StatusForbidden, "Permission denied", err.Error())
		} else {
			respondError(w, http.StatusInternalServerError, "Failed to invoke function", err.Error())
		}
		return
	}

	respondJSON(w, http.StatusOK, execution)
}

// handleListFunctionExecutions handles GET /api/v1/functions/{id}/executions
func (s *Service) handleListFunctionExecutions(w http.ResponseWriter, r *http.Request) {
	// Get function ID from path
	functionID := chi.URLParam(r, "id")
	if functionID == "" {
		respondError(w, http.StatusBadRequest, "Missing function ID", "Function ID is required")
		return
	}

	// Get limit from query string
	limitStr := r.URL.Query().Get("limit")
	limit := 10 // Default limit
	if limitStr != "" {
		var err error
		limit, err = strconv.Atoi(limitStr)
		if err != nil || limit <= 0 {
			respondError(w, http.StatusBadRequest, "Invalid limit", "Limit must be a positive integer")
			return
		}
	}

	// List executions
	executions, err := s.functionsSvc.ListExecutions(r.Context(), functionID, limit)
	if err != nil {
		if errors.Is(err, functions.ErrFunctionNotFound) {
			respondError(w, http.StatusNotFound, "Function not found", err.Error())
		} else {
			respondError(w, http.StatusInternalServerError, "Failed to list executions", err.Error())
		}
		return
	}

	respondJSON(w, http.StatusOK, executions)
}

// handleGetFunctionPermissions handles GET /api/v1/functions/{id}/permissions
func (s *Service) handleGetFunctionPermissions(w http.ResponseWriter, r *http.Request) {
	// Get function ID from path
	functionID := chi.URLParam(r, "id")
	if functionID == "" {
		respondError(w, http.StatusBadRequest, "Missing function ID", "Function ID is required")
		return
	}

	// Get permissions
	permissions, err := s.functionsSvc.GetPermissions(r.Context(), functionID)
	if err != nil {
		if errors.Is(err, functions.ErrFunctionNotFound) {
			respondError(w, http.StatusNotFound, "Function not found", err.Error())
		} else {
			respondError(w, http.StatusInternalServerError, "Failed to get permissions", err.Error())
		}
		return
	}

	respondJSON(w, http.StatusOK, permissions)
}

// handleUpdateFunctionPermissions handles PUT /api/v1/functions/{id}/permissions
func (s *Service) handleUpdateFunctionPermissions(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get function ID from path
	functionID := chi.URLParam(r, "id")
	if functionID == "" {
		respondError(w, http.StatusBadRequest, "Missing function ID", "Function ID is required")
		return
	}

	// Get current permissions
	permissions, err := s.functionsSvc.GetPermissions(r.Context(), functionID)
	if err != nil {
		if errors.Is(err, functions.ErrFunctionNotFound) {
			respondError(w, http.StatusNotFound, "Function not found", err.Error())
		} else {
			respondError(w, http.StatusInternalServerError, "Failed to get permissions", err.Error())
		}
		return
	}

	// Parse request body
	if err := json.NewDecoder(r.Body).Decode(permissions); err != nil {
		respondError(w, http.StatusBadRequest, "Invalid request body", err.Error())
		return
	}

	// Update permissions
	err = s.functionsSvc.UpdatePermissions(r.Context(), functionID, address, permissions)
	if err != nil {
		if errors.Is(err, functions.ErrFunctionNotFound) {
			respondError(w, http.StatusNotFound, "Function not found", err.Error())
		} else if errors.Is(err, functions.ErrPermissionDenied) {
			respondError(w, http.StatusForbidden, "Permission denied", err.Error())
		} else {
			respondError(w, http.StatusInternalServerError, "Failed to update permissions", err.Error())
		}
		return
	}

	respondJSON(w, http.StatusOK, permissions)
}

// handleStoreSecret handles POST /api/v1/secrets
func (s *Service) handleStoreSecret(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	var req SecretRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondError(w, http.StatusBadRequest, "Invalid request body", err.Error())
		return
	}

	// Validate request
	if req.Key == "" || req.Value == "" {
		respondError(w, http.StatusBadRequest, "Missing required fields", "Key and value are required")
		return
	}

	// Prepare options
	var options map[string]interface{}
	if req.TTL > 0 || len(req.Tags) > 0 || req.Options != nil {
		options = make(map[string]interface{})
		if req.TTL > 0 {
			options["ttl"] = time.Duration(req.TTL) * time.Second
		}
		if len(req.Tags) > 0 {
			options["tags"] = req.Tags
		}
		if req.Options != nil {
			for k, v := range req.Options {
				options[k] = v
			}
		}
	}

	// Store secret
	err := s.secretsSvc.StoreSecret(r.Context(), address, req.Key, req.Value, options)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to store secret", err.Error())
		return
	}

	respondJSON(w, http.StatusCreated, map[string]string{"key": req.Key})
}

// handleListSecrets handles GET /api/v1/secrets
func (s *Service) handleListSecrets(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// List secrets
	secrets, err := s.secretsSvc.ListSecrets(r.Context(), address)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to list secrets", err.Error())
		return
	}

	respondJSON(w, http.StatusOK, secrets)
}

// handleGetSecret handles GET /api/v1/secrets/{key}
func (s *Service) handleGetSecret(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get key from path
	key := chi.URLParam(r, "key")
	if key == "" {
		respondError(w, http.StatusBadRequest, "Missing key", "Secret key is required")
		return
	}

	// Get secret
	value, err := s.secretsSvc.GetSecret(r.Context(), address, key)
	if err != nil {
		respondError(w, http.StatusNotFound, "Secret not found", err.Error())
		return
	}

	respondJSON(w, http.StatusOK, map[string]string{"key": key, "value": value})
}

// handleDeleteSecret handles DELETE /api/v1/secrets/{key}
func (s *Service) handleDeleteSecret(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get key from path
	key := chi.URLParam(r, "key")
	if key == "" {
		respondError(w, http.StatusBadRequest, "Missing key", "Secret key is required")
		return
	}

	// Delete secret
	err := s.secretsSvc.DeleteSecret(r.Context(), address, key)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to delete secret", err.Error())
		return
	}

	respondJSON(w, http.StatusNoContent, nil)
}
