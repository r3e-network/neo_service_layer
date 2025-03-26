package handlers

import (
	"encoding/json"
	"math/big"
	"net/http"

	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/gasbank"
)

// GasBankHandler handles gas bank API endpoints
type GasBankHandler struct {
	gasBankService *gasbank.Service
}

// NewGasBankHandler creates a new gas bank handler
func NewGasBankHandler(gasBankService *gasbank.Service) *GasBankHandler {
	return &GasBankHandler{
		gasBankService: gasBankService,
	}
}

// HandleAllocateGas handles allocating gas to a user
func (h *GasBankHandler) HandleAllocateGas(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	var req struct {
		Amount string `json:"amount"`
	}

	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Convert amount to big.Int
	amount, ok := new(big.Int).SetString(req.Amount, 10)
	if !ok {
		http.Error(w, "Invalid gas amount", http.StatusBadRequest)
		return
	}

	// Allocate gas
	allocation, err := h.gasBankService.AllocateGas(r.Context(), address, amount)
	if err != nil {
		http.Error(w, "Failed to allocate gas: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return allocation
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(allocation)
}

// HandleReleaseGas handles releasing gas allocation
func (h *GasBankHandler) HandleReleaseGas(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	// Release gas
	if err := h.gasBankService.ReleaseGas(r.Context(), address); err != nil {
		http.Error(w, "Failed to release gas: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return success
	w.WriteHeader(http.StatusNoContent)
}

// HandleGetGasBalance handles getting gas balance
func (h *GasBankHandler) HandleGetGasBalance(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	// Get allocation
	allocation, err := h.gasBankService.GetAllocation(r.Context(), address)
	if err != nil {
		http.Error(w, "Failed to get gas allocation: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// If no allocation
	if allocation == nil {
		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(map[string]interface{}{
			"allocated": "0",
			"used":      "0",
			"remaining": "0",
		})
		return
	}

	// Return allocation details
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(map[string]interface{}{
		"allocated": allocation.Amount.String(),
		"used":      allocation.Used.String(),
		"remaining": new(big.Int).Sub(allocation.Amount, allocation.Used).String(),
	})
}
