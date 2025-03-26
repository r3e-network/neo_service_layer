package handlers

import (
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/pkg/errors"
	"github.com/will/neo_service_layer/internal/services/functions"
)

// FunctionsHandler handles function-related API endpoints
type FunctionsHandler struct {
	functionsService *functions.Service
}

// NewFunctionsHandler creates a new functions handler
func NewFunctionsHandler(functionsService *functions.Service) *FunctionsHandler {
	return &FunctionsHandler{
		functionsService: functionsService,
	}
}

// HandleCreateFunction handles creating a new function
func (h *FunctionsHandler) HandleCreateFunction(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	var req struct {
		Name        string                 `json:"name"`
		Description string                 `json:"description"`
		Code        string                 `json:"code"`
		Runtime     string                 `json:"runtime"`
		Metadata    map[string]interface{} `json:"metadata"`
	}

	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Validate request
	if req.Name == "" || req.Code == "" {
		http.Error(w, "Missing required fields: Name and code are required", http.StatusBadRequest)
		return
	}

	// Create function
	runtime := functions.Runtime(req.Runtime)
	if runtime == "" {
		runtime = functions.JavaScriptRuntime
	}

	function, err := h.functionsService.CreateFunction(r.Context(), address, req.Name, req.Description, req.Code, runtime)
	if err != nil {
		http.Error(w, "Failed to create function: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return function
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(function)
}

// HandleListFunctions handles listing functions
func (h *FunctionsHandler) HandleListFunctions(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	// List functions
	functions, err := h.functionsService.ListFunctions(r.Context(), address)
	if err != nil {
		http.Error(w, "Failed to list functions: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return functions
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(functions)
}

// HandleGetFunction handles getting a function
func (h *FunctionsHandler) HandleGetFunction(w http.ResponseWriter, r *http.Request) {
	// Get function ID from path
	functionID := chi.URLParam(r, "id")
	if functionID == "" {
		http.Error(w, "Missing function ID: Function ID is required", http.StatusBadRequest)
		return
	}

	// Get function
	function, err := h.functionsService.GetFunction(r.Context(), functionID)
	if err != nil {
		if errors.Is(err, functions.ErrFunctionNotFound) {
			http.Error(w, "Function not found: "+err.Error(), http.StatusNotFound)
		} else {
			http.Error(w, "Failed to get function: "+err.Error(), http.StatusInternalServerError)
		}
		return
	}

	// Return function
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(function)
}
