package providers

import (
	"encoding/json"
	"fmt"
	"net/http"
	"sync"
	"time"

	pricefeed "github.com/r3e-network/neo_service_layer/internal/pricefeedservice"
)

const (
	// HuobiProviderName is the name of the Huobi price provider
	HuobiProviderName = "huobi"

	// HuobiAPIBaseURL is the base URL for the Huobi API
	HuobiAPIBaseURL = "https://api.huobi.pro"

	// HuobiMarketTickerEndpoint is the endpoint for market tickers
	HuobiMarketTickerEndpoint = "/market/detail/merged"

	// HuobiSymbolsEndpoint is the endpoint for supported symbols
	HuobiSymbolsEndpoint = "/v1/common/symbols"

	// HuobiRateLimitWindow is the time window for rate limiting
	HuobiRateLimitWindow = 1 * time.Second

	// HuobiRateLimitRequests is the maximum number of requests per window
	HuobiRateLimitRequests = 10

	// HuobiTimeoutDuration is the timeout for API requests
	HuobiTimeoutDuration = 5 * time.Second

	// HuobiRetryAttempts is the number of retry attempts for failed requests
	HuobiRetryAttempts = 3

	// HuobiRetryInterval is the interval between retry attempts
	HuobiRetryInterval = 500 * time.Millisecond
)

// HuobiTickerResponse represents the response from the Huobi ticker API
type HuobiTickerResponse struct {
	Status string `json:"status"`
	Ch     string `json:"ch"`
	Ts     int64  `json:"ts"`
	Tick   struct {
		ID     int64     `json:"id"`
		Amount float64   `json:"amount"`
		Count  int64     `json:"count"`
		Open   float64   `json:"open"`
		Close  float64   `json:"close"`
		Low    float64   `json:"low"`
		High   float64   `json:"high"`
		Vol    float64   `json:"vol"`
		Bid    []float64 `json:"bid"`
		Ask    []float64 `json:"ask"`
	} `json:"tick"`
}

// HuobiSymbolsResponse represents the response from the Huobi symbols API
type HuobiSymbolsResponse struct {
	Status string `json:"status"`
	Data   []struct {
		BaseCurrency    string `json:"base-currency"`
		QuoteCurrency   string `json:"quote-currency"`
		Symbol          string `json:"symbol"`
		State           string `json:"state"`
		ValuePrecision  int    `json:"value-precision"`
		AmountPrecision int    `json:"amount-precision"`
	} `json:"data"`
}

// HuobiPriceProvider implements the PriceProvider interface for Huobi
type HuobiPriceProvider struct {
	httpClient *http.Client

	// Rate limiting
	requestsMu        sync.Mutex
	requestTimestamps []time.Time

	// Cache
	cacheMu    sync.RWMutex
	priceCache map[string]pricefeed.PriceData

	// Symbol mapping
	symbolsMu      sync.RWMutex
	symbolsMapping map[string]string // Maps "BASE-QUOTE" to Huobi's symbol format
}

// NewHuobiPriceProvider creates a new Huobi price provider
func NewHuobiPriceProvider() pricefeed.PriceProvider {
	return &HuobiPriceProvider{
		httpClient: &http.Client{
			Timeout: HuobiTimeoutDuration,
		},
		requestTimestamps: make([]time.Time, 0, HuobiRateLimitRequests),
		priceCache:        make(map[string]pricefeed.PriceData),
		symbolsMapping:    make(map[string]string),
	}
}

// GetName returns the name of the provider
func (p *HuobiPriceProvider) GetName() string {
	return HuobiProviderName
}

// GetPricePair gets the price for a given asset pair
func (p *HuobiPriceProvider) GetPricePair(ctx pricefeed.Context, base, quote string) (pricefeed.PriceData, error) {
	cacheKey := fmt.Sprintf("%s-%s", base, quote)

	// Check cache first
	p.cacheMu.RLock()
	cachedData, exists := p.priceCache[cacheKey]
	p.cacheMu.RUnlock()

	// If cached data exists and is fresh (less than 60 seconds old), return it
	if exists && time.Since(cachedData.Timestamp) < 60*time.Second {
		return cachedData, nil
	}

	// Get Huobi symbol
	symbol, err := p.getHuobiSymbol(ctx, base, quote)
	if err != nil {
		return pricefeed.PriceData{}, err
	}

	// Rate limit check
	if err := p.checkRateLimit(); err != nil {
		return pricefeed.PriceData{}, err
	}

	// Build request URL
	url := fmt.Sprintf("%s%s?symbol=%s", HuobiAPIBaseURL, HuobiMarketTickerEndpoint, symbol)

	// Create a new request
	req, err := http.NewRequestWithContext(ctx, "GET", url, nil)
	if err != nil {
		return pricefeed.PriceData{}, fmt.Errorf("failed to create request: %w", err)
	}

	// Set headers
	req.Header.Set("Accept", "application/json")

	// Execute request with retry logic
	var resp *http.Response
	for attempt := 0; attempt < HuobiRetryAttempts; attempt++ {
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
		case <-time.After(HuobiRetryInterval):
			// Wait before retrying
		}
	}

	if err != nil {
		return pricefeed.PriceData{}, fmt.Errorf("all request attempts failed: %w", err)
	}
	defer resp.Body.Close()

	// Parse response
	var tickerResp HuobiTickerResponse
	if err := json.NewDecoder(resp.Body).Decode(&tickerResp); err != nil {
		return pricefeed.PriceData{}, fmt.Errorf("failed to decode response: %w", err)
	}

	// Check status
	if tickerResp.Status != "ok" {
		return pricefeed.PriceData{}, fmt.Errorf("huobi API returned non-ok status: %s", tickerResp.Status)
	}

	// Get price from bid-ask midpoint
	if len(tickerResp.Tick.Bid) < 1 || len(tickerResp.Tick.Ask) < 1 {
		return pricefeed.PriceData{}, fmt.Errorf("invalid bid/ask data in response")
	}

	bidPrice := tickerResp.Tick.Bid[0]
	askPrice := tickerResp.Tick.Ask[0]
	price := (bidPrice + askPrice) / 2

	// Create price data
	timestamp := time.Unix(0, tickerResp.Ts*int64(time.Millisecond))
	priceData := pricefeed.PriceData{
		Base:       base,
		Quote:      quote,
		Price:      price,
		Timestamp:  timestamp,
		Source:     HuobiProviderName,
		Confidence: 0.95, // Confidence level, arbitrary value for Huobi
	}

	// Update cache
	p.cacheMu.Lock()
	p.priceCache[cacheKey] = priceData
	p.cacheMu.Unlock()

	return priceData, nil
}

// GetSupportedPairs returns the list of supported trading pairs
func (p *HuobiPriceProvider) GetSupportedPairs(ctx pricefeed.Context) ([]pricefeed.AssetPair, error) {
	// Check if we need to fetch the symbols
	p.symbolsMu.RLock()
	isEmpty := len(p.symbolsMapping) == 0
	p.symbolsMu.RUnlock()

	if isEmpty {
		if err := p.fetchSymbols(ctx); err != nil {
			return nil, err
		}
	}

	// Convert symbols to asset pairs
	p.symbolsMu.RLock()
	defer p.symbolsMu.RUnlock()

	pairs := make([]pricefeed.AssetPair, 0, len(p.symbolsMapping))
	for key := range p.symbolsMapping {
		parts := splitPair(key)
		if len(parts) == 2 {
			pairs = append(pairs, pricefeed.AssetPair{
				Base:  parts[0],
				Quote: parts[1],
			})
		}
	}

	return pairs, nil
}

// splitPair splits a "BASE-QUOTE" string into ["BASE", "QUOTE"]
func splitPair(pair string) []string {
	base, quote := "", ""
	for i, c := range pair {
		if c == '-' {
			base = pair[:i]
			quote = pair[i+1:]
			break
		}
	}
	return []string{base, quote}
}

// getHuobiSymbol converts a base-quote pair to Huobi's symbol format
func (p *HuobiPriceProvider) getHuobiSymbol(ctx pricefeed.Context, base, quote string) (string, error) {
	key := fmt.Sprintf("%s-%s", base, quote)

	// Check if we have the symbol mapping
	p.symbolsMu.RLock()
	symbol, exists := p.symbolsMapping[key]
	p.symbolsMu.RUnlock()

	if exists {
		return symbol, nil
	}

	// If not, fetch the symbols and try again
	if err := p.fetchSymbols(ctx); err != nil {
		return "", err
	}

	// Look for the symbol again
	p.symbolsMu.RLock()
	symbol, exists = p.symbolsMapping[key]
	p.symbolsMu.RUnlock()

	if !exists {
		return "", fmt.Errorf("trading pair %s-%s is not supported by Huobi", base, quote)
	}

	return symbol, nil
}

// fetchSymbols fetches the list of supported symbols from Huobi
func (p *HuobiPriceProvider) fetchSymbols(ctx pricefeed.Context) error {
	// Rate limit check
	if err := p.checkRateLimit(); err != nil {
		return err
	}

	// Build request URL
	url := fmt.Sprintf("%s%s", HuobiAPIBaseURL, HuobiSymbolsEndpoint)

	// Create a new request
	req, err := http.NewRequestWithContext(ctx, "GET", url, nil)
	if err != nil {
		return fmt.Errorf("failed to create request: %w", err)
	}

	// Set headers
	req.Header.Set("Accept", "application/json")

	// Execute request with retry logic
	var resp *http.Response
	for attempt := 0; attempt < HuobiRetryAttempts; attempt++ {
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
			return ctx.Err()
		case <-time.After(HuobiRetryInterval):
			// Wait before retrying
		}
	}

	if err != nil {
		return fmt.Errorf("all request attempts failed: %w", err)
	}
	defer resp.Body.Close()

	// Parse response
	var symbolsResp HuobiSymbolsResponse
	if err := json.NewDecoder(resp.Body).Decode(&symbolsResp); err != nil {
		return fmt.Errorf("failed to decode response: %w", err)
	}

	// Check status
	if symbolsResp.Status != "ok" {
		return fmt.Errorf("huobi API returned non-ok status: %s", symbolsResp.Status)
	}

	// Update symbol mappings
	p.symbolsMu.Lock()
	defer p.symbolsMu.Unlock()

	for _, symbolData := range symbolsResp.Data {
		if symbolData.State != "online" {
			continue
		}

		base := symbolData.BaseCurrency
		quote := symbolData.QuoteCurrency

		// Convert to uppercase for consistency with our API
		baseUpper := base
		quoteUpper := quote

		// Store the mapping
		p.symbolsMapping[fmt.Sprintf("%s-%s", baseUpper, quoteUpper)] = symbolData.Symbol
	}

	return nil
}

// checkRateLimit checks if the current request would exceed the rate limit
func (p *HuobiPriceProvider) checkRateLimit() error {
	p.requestsMu.Lock()
	defer p.requestsMu.Unlock()

	now := time.Now()
	windowStart := now.Add(-HuobiRateLimitWindow)

	// Remove timestamps outside the current window
	validTimestamps := make([]time.Time, 0, len(p.requestTimestamps))
	for _, ts := range p.requestTimestamps {
		if ts.After(windowStart) {
			validTimestamps = append(validTimestamps, ts)
		}
	}
	p.requestTimestamps = validTimestamps

	// Check if we've hit the rate limit
	if len(p.requestTimestamps) >= HuobiRateLimitRequests {
		waitTime := p.requestTimestamps[0].Add(HuobiRateLimitWindow).Sub(now)
		return &pricefeed.RateLimitError{
			Provider:  HuobiProviderName,
			WaitTime:  waitTime,
			Threshold: HuobiRateLimitRequests,
			Window:    HuobiRateLimitWindow,
		}
	}

	// Add current timestamp to the list
	p.requestTimestamps = append(p.requestTimestamps, now)
	return nil
}

// ClearCache clears the price cache
func (p *HuobiPriceProvider) ClearCache() {
	p.cacheMu.Lock()
	p.priceCache = make(map[string]pricefeed.PriceData)
	p.cacheMu.Unlock()
}

// Implements the Provider interface
var _ pricefeed.PriceProvider = (*HuobiPriceProvider)(nil)
