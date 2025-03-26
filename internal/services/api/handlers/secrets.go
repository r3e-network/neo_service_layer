package handlers

import (
	"encoding/json"
	"net/http"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/nspcc-dev/neo-go/pkg/util"
	"github.com/will/neo_service_layer/internal/services/secrets"
)

// SecretsHandler handles secrets API endpoints
type SecretsHandler struct {
	secretsService *secrets.Service
}

// NewSecretsHandler creates a new secrets handler
func NewSecretsHandler(secretsService *secrets.Service) *SecretsHandler {
	return &SecretsHandler{
		secretsService: secretsService,
	}
}

// HandleStoreSecret handles storing a secret
func (h *SecretsHandler) HandleStoreSecret(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	var req struct {
		Key     string                 `json:"key"`
		Value   string                 `json:"value"`
		TTL     int64                  `json:"ttl"`
		Tags    []string               `json:"tags"`
		Options map[string]interface{} `json:"options"`
	}

	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Validate request
	if req.Key == "" || req.Value == "" {
		http.Error(w, "Missing required fields: Key and value are required", http.StatusBadRequest)
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
	err := h.secretsService.StoreSecret(r.Context(), address, req.Key, req.Value, options)
	if err != nil {
		http.Error(w, "Failed to store secret: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return success
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(map[string]string{"key": req.Key})
}

// HandleGetSecret handles getting a secret
func (h *SecretsHandler) HandleGetSecret(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	// Get key from path
	key := chi.URLParam(r, "key")
	if key == "" {
		http.Error(w, "Missing key", http.StatusBadRequest)
		return
	}

	// Get secret
	value, err := h.secretsService.GetSecret(r.Context(), address, key)
	if err != nil {
		http.Error(w, "Secret not found: "+err.Error(), http.StatusNotFound)
		return
	}

	// Return secret
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(map[string]string{"key": key, "value": value})
}

// HandleListSecrets handles listing secrets
func (h *SecretsHandler) HandleListSecrets(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	// List secrets
	secrets, err := h.secretsService.ListSecrets(r.Context(), address)
	if err != nil {
		http.Error(w, "Failed to list secrets: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return secrets
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(secrets)
}

// HandleDeleteSecret handles deleting a secret
func (h *SecretsHandler) HandleDeleteSecret(w http.ResponseWriter, r *http.Request) {
	// Get address from context
	address, ok := r.Context().Value("address").(util.Uint160)
	if !ok {
		http.Error(w, "Unauthorized: Invalid or missing address", http.StatusUnauthorized)
		return
	}

	// Get key from path
	key := chi.URLParam(r, "key")
	if key == "" {
		http.Error(w, "Missing key", http.StatusBadRequest)
		return
	}

	// Delete secret
	err := h.secretsService.DeleteSecret(r.Context(), address, key)
	if err != nil {
		http.Error(w, "Failed to delete secret: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return success
	w.WriteHeader(http.StatusNoContent)
}
