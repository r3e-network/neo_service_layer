package providers

import (
	"encoding/json"
	"fmt"
	"net/http"
	"strconv"
	"sync"
	"time"

	"github.com/will/neo_service_layer/internal/services/pricefeed"
)

const (
	// CoinbaseProviderName is the name of the Coinbase price provider
	CoinbaseProviderName = "coinbase"

	// CoinbaseAPIBaseURL is the base URL for the Coinbase API
	CoinbaseAPIBaseURL = "https://api.coinbase.com/v2"

	// CoinbaseSpotPriceEndpoint is the endpoint for spot prices
	CoinbaseSpotPriceEndpoint = "/prices/%s-%s/spot"

	// CoinbaseRateLimitWindow is the time window for rate limiting
	CoinbaseRateLimitWindow = 1 * time.Second

	// CoinbaseRateLimitRequests is the maximum number of requests per window
	CoinbaseRateLimitRequests = 10

	// CoinbaseTimeoutDuration is the timeout for API requests
	CoinbaseTimeoutDuration = 5 * time.Second

	// CoinbaseRetryAttempts is the number of retry attempts for failed requests
	CoinbaseRetryAttempts = 3

	// CoinbaseRetryInterval is the interval between retry attempts
	CoinbaseRetryInterval = 500 * time.Millisecond
)

// CoinbaseSpotPriceResponse represents the response from the Coinbase spot price API
type CoinbaseSpotPriceResponse struct {
	Data struct {
		Base     string `json:"base"`
		Currency string `json:"currency"`
		Amount   string `json:"amount"`
	} `json:"data"`
}

// CoinbasePriceProvider implements the PriceProvider interface for Coinbase
type CoinbasePriceProvider struct {
	httpClient *http.Client

	// Rate limiting
	requestsMu        sync.Mutex
	requestTimestamps []time.Time

	// Cache
	cacheMu    sync.RWMutex
	priceCache map[string]pricefeed.PriceData
}

// NewCoinbasePriceProvider creates a new Coinbase price provider
func NewCoinbasePriceProvider() pricefeed.PriceProvider {
	return &CoinbasePriceProvider{
		httpClient: &http.Client{
			Timeout: CoinbaseTimeoutDuration,
		},
		requestTimestamps: make([]time.Time, 0, CoinbaseRateLimitRequests),
		priceCache:        make(map[string]pricefeed.PriceData),
	}
}

// GetName returns the name of the provider
func (p *CoinbasePriceProvider) GetName() string {
	return CoinbaseProviderName
}

// GetPricePair gets the price for a given asset pair
func (p *CoinbasePriceProvider) GetPricePair(ctx pricefeed.Context, base, quote string) (pricefeed.PriceData, error) {
	cacheKey := fmt.Sprintf("%s-%s", base, quote)

	// Check cache first
	p.cacheMu.RLock()
	cachedData, exists := p.priceCache[cacheKey]
	p.cacheMu.RUnlock()

	// If cached data exists and is fresh (less than 60 seconds old), return it
	if exists && time.Since(cachedData.Timestamp) < 60*time.Second {
		return cachedData, nil
	}

	// Rate limit check
	if err := p.checkRateLimit(); err != nil {
		return pricefeed.PriceData{}, err
	}

	// Build request URL
	url := fmt.Sprintf("%s%s", CoinbaseAPIBaseURL, fmt.Sprintf(CoinbaseSpotPriceEndpoint, base, quote))

	// Create a new request
	req, err := http.NewRequestWithContext(ctx, "GET", url, nil)
	if err != nil {
		return pricefeed.PriceData{}, fmt.Errorf("failed to create request: %w", err)
	}

	// Set headers
	req.Header.Set("Accept", "application/json")

	// Execute request with retry logic
	var resp *http.Response
	for attempt := 0; attempt < CoinbaseRetryAttempts; attempt++ {
		resp, err = p.httpClient.Do(req)
		if err == nil && resp.StatusCode == http.StatusOK {
			break
		}

		if resp != nil {
			resp.Body.Close()
		}

		// Check if context is cancelled
		select {
		case <-ctx.Done():
			return pricefeed.PriceData{}, ctx.Err()
		case <-time.After(CoinbaseRetryInterval):
			// Wait before retrying
		}
	}

	if err != nil {
		return pricefeed.PriceData{}, fmt.Errorf("all request attempts failed: %w", err)
	}
	defer resp.Body.Close()

	// Parse response
	var priceResp CoinbaseSpotPriceResponse
	if err := json.NewDecoder(resp.Body).Decode(&priceResp); err != nil {
		return pricefeed.PriceData{}, fmt.Errorf("failed to decode response: %w", err)
	}

	// Parse price amount
	price, err := strconv.ParseFloat(priceResp.Data.Amount, 64)
	if err != nil {
		return pricefeed.PriceData{}, fmt.Errorf("failed to parse price amount: %w", err)
	}

	// Create price data
	now := time.Now()
	priceData := pricefeed.PriceData{
		Base:       base,
		Quote:      quote,
		Price:      price,
		Timestamp:  now,
		Source:     CoinbaseProviderName,
		Confidence: 0.95, // Confidence level, arbitrary value for Coinbase
	}

	// Update cache
	p.cacheMu.Lock()
	p.priceCache[cacheKey] = priceData
	p.cacheMu.Unlock()

	return priceData, nil
}

// GetSupportedPairs returns the list of supported trading pairs
func (p *CoinbasePriceProvider) GetSupportedPairs(ctx pricefeed.Context) ([]pricefeed.AssetPair, error) {
	// In a real implementation, we might fetch this from Coinbase API
	// For now, return a static list of supported pairs
	return []pricefeed.AssetPair{
		{Base: "BTC", Quote: "USD"},
		{Base: "ETH", Quote: "USD"},
		{Base: "NEO", Quote: "USD"},
		{Base: "GAS", Quote: "USD"},
		{Base: "BTC", Quote: "EUR"},
		{Base: "ETH", Quote: "EUR"},
		{Base: "NEO", Quote: "EUR"},
		{Base: "GAS", Quote: "EUR"},
	}, nil
}

// checkRateLimit checks if the current request would exceed the rate limit
func (p *CoinbasePriceProvider) checkRateLimit() error {
	p.requestsMu.Lock()
	defer p.requestsMu.Unlock()

	now := time.Now()
	windowStart := now.Add(-CoinbaseRateLimitWindow)

	// Remove timestamps outside the current window
	validTimestamps := make([]time.Time, 0, len(p.requestTimestamps))
	for _, ts := range p.requestTimestamps {
		if ts.After(windowStart) {
			validTimestamps = append(validTimestamps, ts)
		}
	}
	p.requestTimestamps = validTimestamps

	// Check if we've hit the rate limit
	if len(p.requestTimestamps) >= CoinbaseRateLimitRequests {
		waitTime := p.requestTimestamps[0].Add(CoinbaseRateLimitWindow).Sub(now)
		return &pricefeed.RateLimitError{
			Provider:  CoinbaseProviderName,
			WaitTime:  waitTime,
			Threshold: CoinbaseRateLimitRequests,
			Window:    CoinbaseRateLimitWindow,
		}
	}

	// Add current timestamp to the list
	p.requestTimestamps = append(p.requestTimestamps, now)
	return nil
}

// ClearCache clears the price cache
func (p *CoinbasePriceProvider) ClearCache() {
	p.cacheMu.Lock()
	p.priceCache = make(map[string]pricefeed.PriceData)
	p.cacheMu.Unlock()
}

// Implements the Provider interface
var _ pricefeed.PriceProvider = (*CoinbasePriceProvider)(nil)
