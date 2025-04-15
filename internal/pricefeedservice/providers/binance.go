package providers

import (
	"encoding/json"
	"fmt"
	"math/big"
	"net/http"
	"strconv"
	"strings"
	"time"

	pricefeed "github.com/r3e-network/neo_service_layer/internal/pricefeedservice"
)

const (
	binanceAPIBaseURL = "https://api.binance.com/api/v3"
)

// BinanceProvider implements a price feed provider using the Binance API
type BinanceProvider struct {
	httpClient       *http.Client
	heartbeatSeconds int64
	baseURL          string
}

// BinanceTickerResponse represents the response from the Binance ticker API
type BinanceTickerResponse struct {
	Symbol             string `json:"symbol"`
	PriceChange        string `json:"priceChange"`
	PriceChangePercent string `json:"priceChangePercent"`
	WeightedAvgPrice   string `json:"weightedAvgPrice"`
	PrevClosePrice     string `json:"prevClosePrice"`
	LastPrice          string `json:"lastPrice"`
	LastQty            string `json:"lastQty"`
	BidPrice           string `json:"bidPrice"`
	AskPrice           string `json:"askPrice"`
	OpenPrice          string `json:"openPrice"`
	HighPrice          string `json:"highPrice"`
	LowPrice           string `json:"lowPrice"`
	Volume             string `json:"volume"`
	QuoteVolume        string `json:"quoteVolume"`
	OpenTime           int64  `json:"openTime"`
	CloseTime          int64  `json:"closeTime"`
	FirstId            int64  `json:"firstId"`
	LastId             int64  `json:"lastId"`
	Count              int64  `json:"count"`
}

// NewBinanceProvider creates a new Binance price feed provider
func NewBinanceProvider(heartbeatSeconds int64) *BinanceProvider {
	return &BinanceProvider{
		httpClient: &http.Client{
			Timeout: 10 * time.Second,
		},
		heartbeatSeconds: heartbeatSeconds,
		baseURL:          binanceAPIBaseURL,
	}
}

// GetPrices gets prices for a list of assets
func (p *BinanceProvider) GetPrices(assets []string) ([]*pricefeed.Price, error) {
	var prices []*pricefeed.Price

	for _, asset := range assets {
		price, err := p.GetPrice(asset)
		if err != nil {
			return nil, fmt.Errorf("failed to get price for %s: %w", asset, err)
		}
		prices = append(prices, price)
	}

	return prices, nil
}

// GetPrice gets price for a single asset
func (p *BinanceProvider) GetPrice(asset string) (*pricefeed.Price, error) {
	// Convert asset to Binance symbol format
	symbol := convertToBinanceSymbol(asset)

	// Build the request URL
	url := fmt.Sprintf("%s/ticker/24hr?symbol=%s", p.baseURL, symbol)

	// Make the request
	resp, err := p.httpClient.Get(url)
	if err != nil {
		return nil, fmt.Errorf("failed to get price from Binance: %w", err)
	}
	defer resp.Body.Close()

	// Check response status
	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("unexpected status code: %d", resp.StatusCode)
	}

	// Parse the response
	var tickerResp BinanceTickerResponse
	if err := json.NewDecoder(resp.Body).Decode(&tickerResp); err != nil {
		return nil, fmt.Errorf("failed to decode Binance response: %w", err)
	}

	// Parse the price
	priceVal, err := strconv.ParseFloat(tickerResp.LastPrice, 64)
	if err != nil {
		return nil, fmt.Errorf("failed to parse price: %w", err)
	}

	// Create a price object
	price := &pricefeed.Price{
		Asset:      asset,
		Value:      new(big.Float).SetFloat64(priceVal),
		Timestamp:  time.Unix(0, tickerResp.CloseTime*int64(time.Millisecond)),
		Provider:   "binance",
		Heartbeat:  p.heartbeatSeconds,
		Confidence: 0.95, // High confidence for Binance
	}

	return price, nil
}

// GetHistoricalPrices gets historical prices for an asset
func (p *BinanceProvider) GetHistoricalPrices(asset string, from, to time.Time, interval string) ([]*pricefeed.Price, error) {
	// Convert asset to Binance symbol format
	symbol := convertToBinanceSymbol(asset)

	// Convert interval to Binance format
	binanceInterval := convertToBinanceInterval(interval)

	// Build the request URL
	url := fmt.Sprintf("%s/klines?symbol=%s&interval=%s&startTime=%d&endTime=%d&limit=1000",
		p.baseURL, symbol, binanceInterval, from.UnixNano()/int64(time.Millisecond), to.UnixNano()/int64(time.Millisecond))

	// Make the request
	resp, err := p.httpClient.Get(url)
	if err != nil {
		return nil, fmt.Errorf("failed to get historical prices from Binance: %w", err)
	}
	defer resp.Body.Close()

	// Check response status
	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("unexpected status code: %d", resp.StatusCode)
	}

	// Parse the response
	var klines [][]interface{}
	if err := json.NewDecoder(resp.Body).Decode(&klines); err != nil {
		return nil, fmt.Errorf("failed to decode Binance response: %w", err)
	}

	// Convert klines to prices
	var prices []*pricefeed.Price
	for _, kline := range klines {
		// Kline format: [OpenTime, Open, High, Low, Close, Volume, CloseTime, ...]
		if len(kline) < 6 {
			continue
		}

		// Parse timestamp
		closeTime, ok := kline[6].(float64)
		if !ok {
			continue
		}

		// Parse close price
		closePrice, ok := kline[4].(string)
		if !ok {
			continue
		}

		// Parse the price
		priceVal, err := strconv.ParseFloat(closePrice, 64)
		if err != nil {
			continue
		}

		// Create a price object
		price := &pricefeed.Price{
			Asset:      asset,
			Value:      new(big.Float).SetFloat64(priceVal),
			Timestamp:  time.Unix(0, int64(closeTime)*int64(time.Millisecond)),
			Provider:   "binance",
			Heartbeat:  p.heartbeatSeconds,
			Confidence: 0.95, // High confidence for Binance
		}

		prices = append(prices, price)
	}

	return prices, nil
}

// convertToBinanceSymbol converts an asset name to a Binance symbol
func convertToBinanceSymbol(asset string) string {
	// Default quote asset is USDT
	if !strings.Contains(asset, "/") {
		return asset + "USDT"
	}

	// If the asset contains a slash, convert it to Binance format
	parts := strings.Split(asset, "/")
	return parts[0] + parts[1]
}

// convertToBinanceInterval converts an interval to Binance format
func convertToBinanceInterval(interval string) string {
	switch interval {
	case "1m":
		return "1m"
	case "5m":
		return "5m"
	case "15m":
		return "15m"
	case "30m":
		return "30m"
	case "1h":
		return "1h"
	case "2h":
		return "2h"
	case "4h":
		return "4h"
	case "6h":
		return "6h"
	case "8h":
		return "8h"
	case "12h":
		return "12h"
	case "1d":
		return "1d"
	case "3d":
		return "3d"
	case "1w":
		return "1w"
	case "1M":
		return "1M"
	default:
		return "1h" // Default to 1 hour
	}
}
