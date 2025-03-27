package account

import (
	"encoding/json"
	"net/http"

	"github.com/gorilla/mux"
)

// Handler represents the HTTP handler for account operations
type Handler struct {
	service *Service
}

// NewHandler creates a new account HTTP handler
func NewHandler(service *Service) *Handler {
	return &Handler{service: service}
}

// RegisterRoutes registers the account API routes
func (h *Handler) RegisterRoutes(router *mux.Router) {
	router.HandleFunc("/v1/accounts/create", h.handleCreateAccount).Methods(http.MethodPost)
	router.HandleFunc("/v1/accounts/{address}", h.handleGetAccount).Methods(http.MethodGet)
	router.HandleFunc("/v1/accounts/{address}/transactions", h.handleSubmitTransaction).Methods(http.MethodPost)
	router.HandleFunc("/v1/accounts/batch", h.handleBatchTransactions).Methods(http.MethodPost)
}

// CreateAccountRequest represents the request to create a new account
type CreateAccountRequest struct {
	Type AccountType `json:"type"`
}

// CreateAccountResponse represents the response from creating a new account
type CreateAccountResponse struct {
	Address   string                 `json:"address"`
	Type     AccountType            `json:"type"`
	Metadata map[string]interface{} `json:"metadata"`
}

func (h *Handler) handleCreateAccount(w http.ResponseWriter, r *http.Request) {
	var req CreateAccountRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	account, err := h.service.CreateAccount(r.Context(), req.Type)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	response := CreateAccountResponse{
		Address:   account.Address,
		Type:     account.Type,
		Metadata: account.Metadata,
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}

func (h *Handler) handleGetAccount(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	address := vars["address"]

	account, err := h.service.GetAccount(r.Context(), address)
	if err != nil {
		http.Error(w, err.Error(), http.StatusNotFound)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(account)
}

// TransactionRequest represents a transaction submission request
type TransactionRequest struct {
	Hash      string `json:"hash"`
	Signature string `json:"signature"`
	GasLimit  int64  `json:"gasLimit"`
}

func (h *Handler) handleSubmitTransaction(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	address := vars["address"]

	var req TransactionRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	tx := &Transaction{
		Hash:      []byte(req.Hash),
		Signature: []byte(req.Signature),
		GasLimit:  req.GasLimit,
	}

	if err := h.service.SubmitTransaction(r.Context(), address, tx); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusAccepted)
}

// BatchTransactionRequest represents a batch transaction request
type BatchTransactionRequest struct {
	Transactions []TransactionRequest `json:"transactions"`
}

func (h *Handler) handleBatchTransactions(w http.ResponseWriter, r *http.Request) {
	var req BatchTransactionRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	if len(req.Transactions) > h.service.config.MaxBatchSize {
		http.Error(w, "batch size exceeds maximum allowed", http.StatusBadRequest)
		return
	}

	// Process batch transactions
	// Implementation would go here

	w.WriteHeader(http.StatusAccepted)
}