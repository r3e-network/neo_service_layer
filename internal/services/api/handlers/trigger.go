package handlers

import (
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/trigger"
	"github.com/will/neo_service_layer/internal/services/trigger/models"
)

// TriggerHandler handles trigger API endpoints
type TriggerHandler struct {
	triggerService *trigger.Service
}

// NewTriggerHandler creates a new trigger handler
func NewTriggerHandler(triggerService *trigger.Service) *TriggerHandler {
	return &TriggerHandler{
		triggerService: triggerService,
	}
}

// HandleCreateTrigger handles creating a new trigger
func (h *TriggerHandler) HandleCreateTrigger(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	var req struct {
		Name           string                 `json:"name"`
		Description    string                 `json:"description"`
		Type           string                 `json:"type"`
		Conditions     map[string]string      `json:"conditions"`
		Actions        []string               `json:"actions"`
		Schedule       string                 `json:"schedule"`
		ExecutionLimit int                    `json:"executionLimit"`
		Parameters     map[string]interface{} `json:"parameters"`
	}

	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Validate request
	if req.Name == "" || req.Type == "" || len(req.Actions) == 0 {
		http.Error(w, "Missing required fields", http.StatusBadRequest)
		return
	}

	// Create trigger model
	newTrigger := &models.Trigger{
		Name:        req.Name,
		Description: req.Description,
		Condition:   req.Type,       // Using Type as Condition for now
		Function:    req.Actions[0], // Using first action as Function
		Parameters:  req.Parameters,
		Schedule:    req.Schedule,
		Status:      "active",
	}

	// Create trigger
	createdTrigger, err := h.triggerService.CreateTrigger(r.Context(), address, newTrigger)
	if err != nil {
		http.Error(w, "Failed to create trigger: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return success
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(createdTrigger)
}

// HandleGetTrigger handles getting a trigger
func (h *TriggerHandler) HandleGetTrigger(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	// Get trigger ID from path
	triggerID := chi.URLParam(r, "id")
	if triggerID == "" {
		http.Error(w, "Missing trigger ID", http.StatusBadRequest)
		return
	}

	// Get trigger
	trigger, err := h.triggerService.GetTrigger(r.Context(), address, triggerID)
	if err != nil {
		http.Error(w, "Failed to get trigger: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return trigger
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(trigger)
}

// HandleListTriggers handles listing triggers
func (h *TriggerHandler) HandleListTriggers(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	// List triggers
	triggers, err := h.triggerService.ListTriggers(r.Context(), address)
	if err != nil {
		http.Error(w, "Failed to list triggers: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return triggers
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(triggers)
}

// HandleUpdateTrigger handles updating a trigger
func (h *TriggerHandler) HandleUpdateTrigger(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	// Get trigger ID from path
	triggerID := chi.URLParam(r, "id")
	if triggerID == "" {
		http.Error(w, "Missing trigger ID", http.StatusBadRequest)
		return
	}

	var req struct {
		Name        string                 `json:"name"`
		Description string                 `json:"description"`
		Conditions  map[string]string      `json:"conditions"`
		Actions     []string               `json:"actions"`
		Schedule    string                 `json:"schedule"`
		Parameters  map[string]interface{} `json:"parameters"`
		Status      string                 `json:"status"`
	}

	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Get existing trigger
	existingTrigger, err := h.triggerService.GetTrigger(r.Context(), address, triggerID)
	if err != nil {
		http.Error(w, "Failed to get trigger: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Update trigger fields
	updatedTrigger := &models.Trigger{
		ID:          existingTrigger.ID,
		UserAddress: existingTrigger.UserAddress,
		CreatedAt:   existingTrigger.CreatedAt,
		Name:        req.Name,
		Description: req.Description,
		Condition:   req.Conditions["type"], // Example mapping
		Function:    req.Actions[0],         // Using first action
		Parameters:  req.Parameters,
		Schedule:    req.Schedule,
		Status:      req.Status,
	}

	// Update trigger
	result, err := h.triggerService.UpdateTrigger(r.Context(), address, triggerID, updatedTrigger)
	if err != nil {
		http.Error(w, "Failed to update trigger: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return updated trigger
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(result)
}

// HandleDeleteTrigger handles deleting a trigger
func (h *TriggerHandler) HandleDeleteTrigger(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	// Get trigger ID from path
	triggerID := chi.URLParam(r, "id")
	if triggerID == "" {
		http.Error(w, "Missing trigger ID", http.StatusBadRequest)
		return
	}

	// Delete trigger
	err := h.triggerService.DeleteTrigger(r.Context(), address, triggerID)
	if err != nil {
		http.Error(w, "Failed to delete trigger: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return success
	w.WriteHeader(http.StatusNoContent)
}

// HandleGetTriggerExecutions handles getting trigger execution history
func (h *TriggerHandler) HandleGetTriggerExecutions(w http.ResponseWriter, r *http.Request) {
	// Get trigger ID from URL
	triggerID := chi.URLParam(r, "id")
	if triggerID == "" {
		http.Error(w, "Missing trigger ID", http.StatusBadRequest)
		return
	}

	// Get trigger executions
	executions, err := h.triggerService.GetTriggerExecutions(r.Context(), triggerID)
	if err != nil {
		http.Error(w, "Failed to get trigger executions: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return executions
	w.Header().Set("Content-Type", "application/json")
	if err := json.NewEncoder(w).Encode(executions); err != nil {
		http.Error(w, "Failed to encode response: "+err.Error(), http.StatusInternalServerError)
		return
	}
}
