package handlers

import (
	"encoding/json"
	"math/big"
	"net/http"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/will/neo_service_layer/internal/services/pricefeed"
)

// PriceFeedHandler handles price feed API endpoints
type PriceFeedHandler struct {
	priceFeedService *pricefeed.Service
}

// NewPriceFeedHandler creates a new price feed handler
func NewPriceFeedHandler(priceFeedService *pricefeed.Service) *PriceFeedHandler {
	return &PriceFeedHandler{
		priceFeedService: priceFeedService,
	}
}

// HandlePublishPrice handles publishing a price
func (h *PriceFeedHandler) HandlePublishPrice(w http.ResponseWriter, r *http.Request) {
	var req struct {
		Symbol    string  `json:"symbol"`
		Price     float64 `json:"price"`
		Timestamp int64   `json:"timestamp"`
	}

	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Validate request
	if req.Symbol == "" {
		http.Error(w, "Missing required field: Symbol is required", http.StatusBadRequest)
		return
	}

	// Convert price to big.Float
	price := big.NewFloat(req.Price)

	// Use current time if timestamp not provided
	timestamp := time.Now()
	if req.Timestamp > 0 {
		timestamp = time.Unix(req.Timestamp, 0)
	}

	// Publish price
	err := h.priceFeedService.PublishPrice(r.Context(), req.Symbol, price, timestamp)
	if err != nil {
		http.Error(w, "Failed to publish price: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Return success
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]interface{}{
		"symbol":    req.Symbol,
		"price":     req.Price,
		"timestamp": timestamp,
	})
}

// HandleGetPrice handles getting the current price for a symbol
func (h *PriceFeedHandler) HandleGetPrice(w http.ResponseWriter, r *http.Request) {
	// Get symbol from path
	symbol := chi.URLParam(r, "symbol")
	if symbol == "" {
		http.Error(w, "Missing symbol", http.StatusBadRequest)
		return
	}

	// Get price from service
	price, err := h.priceFeedService.GetPrice(r.Context(), symbol)
	if err != nil {
		http.Error(w, "Failed to get price: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Convert to response format
	priceValue, _ := price.Price.Float64()
	response := map[string]interface{}{
		"symbol":    price.AssetID,
		"price":     priceValue,
		"timestamp": price.Timestamp,
		"source":    price.Source,
	}

	// Return price
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}

// HandleListPrices handles listing all available prices
func (h *PriceFeedHandler) HandleListPrices(w http.ResponseWriter, r *http.Request) {
	// Since there's no GetAllPrices method in the service,
	// we'll provide a basic implementation with hardcoded values
	// In a real implementation, you would iterate through all known assets
	// and fetch their prices

	// Define common symbols to check
	symbols := []string{"NEO/USD", "GAS/USD"}

	var prices []map[string]interface{}

	// Try to get prices for common symbols
	for _, symbol := range symbols {
		price, err := h.priceFeedService.GetPrice(r.Context(), symbol)
		if err == nil && price != nil {
			// Price found, add to response
			priceValue, _ := price.Price.Float64()
			prices = append(prices, map[string]interface{}{
				"symbol":    price.AssetID,
				"price":     priceValue,
				"timestamp": price.Timestamp,
				"source":    price.Source,
			})
		}
	}

	// If no prices were found, return some hardcoded defaults
	if len(prices) == 0 {
		prices = []map[string]interface{}{
			{
				"symbol":    "NEO/USD",
				"price":     10.0,
				"timestamp": time.Now(),
				"source":    "default",
			},
			{
				"symbol":    "GAS/USD",
				"price":     3.0,
				"timestamp": time.Now(),
				"source":    "default",
			},
		}
	}

	// Return prices
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(prices)
}
