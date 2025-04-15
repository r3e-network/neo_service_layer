package api

import (
	"encoding/json"
	"math/big"
	"net/http"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/r3e-network/neo_service_layer/internal/gasbankservice/models"
	triggermodels "github.com/r3e-network/neo_service_layer/internal/triggerservice/models"
)

// Define our custom trigger parameter types that match the actual API requirements
type CreateTriggerParams struct {
	Type      string            `json:"type"`
	Condition string            `json:"condition"`
	Action    string            `json:"action"`
	Metadata  map[string]string `json:"metadata"`
}

type UpdateTriggerParams struct {
	Condition string            `json:"condition,omitempty"`
	Action    string            `json:"action,omitempty"`
	Metadata  map[string]string `json:"metadata,omitempty"`
	Active    *bool             `json:"active,omitempty"`
}

// handleAllocateGas handles POST /api/v1/gas/allocate
func (s *Service) handleAllocateGas(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Use default initial allocation amount
	initialAmount := big.NewInt(10000) // Default amount if not specified

	// Allocate gas
	allocation, err := s.gasBankSvc.RequestAllocation(r.Context(), address, initialAmount)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to allocate gas", err.Error())
		return
	}

	respondJSON(w, http.StatusOK, map[string]string{
		"amount":    allocation.Amount.String(),
		"id":        allocation.ID,
		"status":    allocation.Status,
		"expiresAt": allocation.ExpiresAt.Format(time.RFC3339),
	})
}

// handleReleaseGas handles POST /api/v1/gas/release
func (s *Service) handleReleaseGas(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Release gas
	err := s.gasBankSvc.ReleaseAllocation(r.Context(), address)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to release gas", err.Error())
		return
	}

	respondJSON(w, http.StatusNoContent, nil)
}

// handleGetGasBalance handles GET /api/v1/gas/balance
func (s *Service) handleGetGasBalance(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get gas allocation
	allocation, err := s.gasBankSvc.GetAllocation(r.Context(), address)
	if err != nil {
		// If no allocation found, just continue with empty allocation data
		allocation = &models.Allocation{
			Amount: big.NewInt(0),
			Used:   big.NewInt(0),
			Status: "none",
		}
	}

	// Calculate remaining gas
	remaining := allocation.RemainingGas()

	respondJSON(w, http.StatusOK, map[string]string{
		"balance":   remaining.String(),
		"total":     allocation.Amount.String(),
		"used":      allocation.Used.String(),
		"status":    allocation.Status,
		"expiresAt": allocation.ExpiresAt.Format(time.RFC3339),
	})
}

// handleListPrices handles GET /api/v1/prices
func (s *Service) handleListPrices(w http.ResponseWriter, r *http.Request) {
	// List prices (placeholder implementation)
	prices := map[string]*big.Int{
		"NEO/USD":  big.NewInt(50),
		"GAS/USD":  big.NewInt(15),
		"BTC/USD":  big.NewInt(40000),
		"ETH/USD":  big.NewInt(2500),
		"LINK/USD": big.NewInt(20),
	}

	respondJSON(w, http.StatusOK, prices)
}

// handleGetPrice handles GET /api/v1/prices/{symbol}
func (s *Service) handleGetPrice(w http.ResponseWriter, r *http.Request) {
	// Get symbol from path
	symbol := chi.URLParam(r, "symbol")
	if symbol == "" {
		respondError(w, http.StatusBadRequest, "Missing symbol", "Price symbol is required")
		return
	}

	// Get price
	price, err := s.priceFeedSvc.GetPrice(r.Context(), symbol)
	if err != nil {
		respondError(w, http.StatusNotFound, "Price not found", err.Error())
		return
	}

	// Convert big.Float to string for JSON
	priceStr := price.Price.Text('f', 8)

	respondJSON(w, http.StatusOK, map[string]interface{}{
		"symbol":     symbol,
		"price":      priceStr,
		"time":       price.Timestamp.Format(time.RFC3339),
		"source":     price.Source,
		"confidence": price.Confidence,
	})
}

// handleCreateTrigger handles POST /api/v1/triggers
func (s *Service) handleCreateTrigger(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Parse request
	var req CreateTriggerParams
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondError(w, http.StatusBadRequest, "Invalid request body", err.Error())
		return
	}

	// Validate request
	if req.Type == "" || req.Condition == "" || req.Action == "" {
		respondError(w, http.StatusBadRequest, "Missing required fields", "Type, condition, and action are required")
		return
	}

	// Convert the params to a trigger model
	triggerModel := &triggermodels.Trigger{
		Name:        req.Type,
		UserAddress: address,
		Condition:   req.Condition,
		Function:    req.Action,
		Parameters:  convertMapToInterface(req.Metadata),
		Status:      "active",
		CreatedAt:   time.Now(),
		UpdatedAt:   time.Now(),
	}

	// Create trigger
	createdTrigger, err := s.triggerSvc.CreateTrigger(r.Context(), address, triggerModel)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to create trigger", err.Error())
		return
	}

	respondJSON(w, http.StatusCreated, createdTrigger)
}

// Convert map[string]string to map[string]interface{}
func convertMapToInterface(input map[string]string) map[string]interface{} {
	result := make(map[string]interface{}, len(input))
	for k, v := range input {
		result[k] = v
	}
	return result
}

// handleListTriggers handles GET /api/v1/triggers
func (s *Service) handleListTriggers(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// List triggers
	triggers, err := s.triggerSvc.ListTriggers(r.Context(), address)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to list triggers", err.Error())
		return
	}

	respondJSON(w, http.StatusOK, triggers)
}

// handleGetTrigger handles GET /api/v1/triggers/{id}
func (s *Service) handleGetTrigger(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get trigger ID from path
	triggerID := chi.URLParam(r, "id")
	if triggerID == "" {
		respondError(w, http.StatusBadRequest, "Missing trigger ID", "Trigger ID is required")
		return
	}

	// Get trigger
	trigger, err := s.triggerSvc.GetTrigger(r.Context(), address, triggerID)
	if err != nil {
		respondError(w, http.StatusNotFound, "Trigger not found", err.Error())
		return
	}

	respondJSON(w, http.StatusOK, trigger)
}

// handleUpdateTrigger handles PUT /api/v1/triggers/{id}
func (s *Service) handleUpdateTrigger(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get trigger ID from path
	triggerID := chi.URLParam(r, "id")
	if triggerID == "" {
		respondError(w, http.StatusBadRequest, "Missing trigger ID", "Trigger ID is required")
		return
	}

	// Parse request
	var req UpdateTriggerParams
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondError(w, http.StatusBadRequest, "Invalid request body", err.Error())
		return
	}

	// Get existing trigger
	trigger, err := s.triggerSvc.GetTrigger(r.Context(), address, triggerID)
	if err != nil {
		respondError(w, http.StatusNotFound, "Trigger not found", err.Error())
		return
	}

	// Update fields if provided
	if req.Condition != "" {
		trigger.Condition = req.Condition
	}
	if req.Action != "" {
		trigger.Function = req.Action
	}
	if req.Metadata != nil {
		trigger.Parameters = convertMapToInterface(req.Metadata)
	}
	if req.Active != nil {
		if *req.Active {
			trigger.Status = "active"
		} else {
			trigger.Status = "paused"
		}
	}
	trigger.UpdatedAt = time.Now()

	// Update trigger
	updatedTrigger, err := s.triggerSvc.UpdateTrigger(r.Context(), address, triggerID, trigger)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to update trigger", err.Error())
		return
	}

	respondJSON(w, http.StatusOK, updatedTrigger)
}

// handleDeleteTrigger handles DELETE /api/v1/triggers/{id}
func (s *Service) handleDeleteTrigger(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get trigger ID from path
	triggerID := chi.URLParam(r, "id")
	if triggerID == "" {
		respondError(w, http.StatusBadRequest, "Missing trigger ID", "Trigger ID is required")
		return
	}

	// Delete trigger
	err := s.triggerSvc.DeleteTrigger(r.Context(), address, triggerID)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to delete trigger", err.Error())
		return
	}

	respondJSON(w, http.StatusNoContent, nil)
}

// handleExecuteTrigger handles POST /api/v1/triggers/{id}/execute
func (s *Service) handleExecuteTrigger(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get trigger ID from path
	triggerID := chi.URLParam(r, "id")
	if triggerID == "" {
		respondError(w, http.StatusBadRequest, "Missing trigger ID", "Trigger ID is required")
		return
	}

	// Execute trigger
	execution, err := s.triggerSvc.ExecuteTrigger(r.Context(), address, triggerID)
	if err != nil {
		respondError(w, http.StatusInternalServerError, "Failed to execute trigger", err.Error())
		return
	}

	respondJSON(w, http.StatusOK, execution)
}

// handleGetUserProfile handles GET /api/v1/profile
func (s *Service) handleGetUserProfile(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		respondError(w, http.StatusUnauthorized, "Unauthorized", "Invalid or missing address")
		return
	}

	// Get gas allocation
	allocation, err := s.gasBankSvc.GetAllocation(r.Context(), address)
	if err != nil {
		// If no allocation found, just continue with empty allocation data
		allocation = &models.Allocation{
			Amount: big.NewInt(0),
			Used:   big.NewInt(0),
			Status: "none",
		}
	}

	// Get triggers
	triggers, err := s.triggerSvc.ListTriggers(r.Context(), address)
	if err != nil {
		// If error listing triggers, just use empty list
		triggers = []*triggermodels.Trigger{}
	}

	// Build profile
	profile := map[string]interface{}{
		"address": address.StringLE(),
		"gas": map[string]string{
			"allocated": allocation.Amount.String(),
			"used":      allocation.Used.String(),
			"remaining": allocation.RemainingGas().String(),
			"status":    allocation.Status,
		},
		"triggers": map[string]interface{}{
			"count":  len(triggers),
			"active": countActiveTriggers(triggers),
		},
		"lastActive": time.Now().Format(time.RFC3339),
	}

	respondJSON(w, http.StatusOK, profile)
}

// countActiveTriggers counts the number of active triggers
func countActiveTriggers(triggers []*triggermodels.Trigger) int {
	count := 0
	for _, t := range triggers {
		if t.Status == "active" {
			count++
		}
	}
	return count
}
