package pricefeed

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"time"
)

// startDataSource starts a data source listener
func (s *Service) startDataSource(ctx context.Context, config DataSourceConfig) error {
	switch config.Type {
	case "rest":
		go s.pollRESTDataSource(ctx, config)
	case "websocket":
		go s.pollWebSocketDataSource(ctx, config)
	default:
		return fmt.Errorf("%w: %s", ErrInvalidDataSourceType, config.Type)
	}
	return nil
}

// pollRESTDataSource polls a REST API data source
func (s *Service) pollRESTDataSource(ctx context.Context, config DataSourceConfig) {
	ticker := time.NewTicker(s.config.UpdateInterval)
	defer ticker.Stop()

	client := &http.Client{
		Timeout: 10 * time.Second,
	}

	for {
		select {
		case <-ctx.Done():
			return
		case <-s.stopCh:
			return
		case <-ticker.C:
			for _, pair := range config.Pairs {
				price, err := s.fetchRESTPrice(ctx, client, config, pair)
				if err != nil {
					// Log error and continue
					continue
				}

				s.updateCh <- &PriceUpdate{
					Pair:  pair,
					Price: price,
					Time:  time.Now(),
				}
			}
		}
	}
}

// fetchRESTPrice fetches price from a REST API
func (s *Service) fetchRESTPrice(ctx context.Context, client *http.Client, config DataSourceConfig, pair string) (float64, error) {
	url := fmt.Sprintf(config.Endpoint, pair)

	req, err := http.NewRequestWithContext(ctx, "GET", url, nil)
	if err != nil {
		return 0, err
	}

	resp, err := client.Do(req)
	if err != nil {
		return 0, err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return 0, fmt.Errorf("unexpected status code: %d", resp.StatusCode)
	}

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return 0, err
	}

	var result struct {
		Price float64 `json:"price"`
	}
	if err := json.Unmarshal(body, &result); err != nil {
		return 0, err
	}

	return result.Price, nil
}

// pollWebSocketDataSource polls a WebSocket data source
func (s *Service) pollWebSocketDataSource(ctx context.Context, config DataSourceConfig) {
	// WebSocket implementation
	// TODO: Implement WebSocket data source
}
