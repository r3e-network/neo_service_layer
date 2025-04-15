# Price Feed Service Implementation Guide

*Last Updated: 2025-04-14*

**Last Updated**: 2024-01-05

## Introduction

This document provides detailed implementation guidance for the Price Feed Service in the Neo Service Layer. It covers component implementations, workflows, integration details, and best practices for development and deployment.

## Repository Structure

The Price Feed Service is organized in the following directory structure:

```
internal/pricefeedservice/
├── model/              # Data models and repository interface
├── interface/          # Service and component interfaces
├── service/            # Main service implementation
├── fetcher/            # Price data fetching implementation
├── aggregator/         # Price data aggregation implementation
├── publisher/          # Blockchain publishing implementation
├── scheduler/          # Scheduling implementation
└── repository/         # Data persistence implementation
```

## Components Implementation

### Service Layer Implementation

The service layer implements the main service interface and coordinates the interactions between all components:

```go
// PriceFeedService implements the IPriceFeedService interface
type PriceFeedService struct {
    repository model.IPriceRepository
    fetcher    interface.IFetcher
    aggregator interface.IAggregator
    publisher  interface.IPublisher
    scheduler  interface.IScheduler
    logger     logginginterface.ILogger
    
    // Service state
    running       bool
    lastStartTime time.Time
    fetchErrors   int
    publishErrors int
    mu            sync.RWMutex
}

// NewPriceFeedService creates a new instance of the Price Feed Service
func NewPriceFeedService(
    repository model.IPriceRepository,
    fetcher interface.IFetcher,
    aggregator interface.IAggregator,
    publisher interface.IPublisher,
    scheduler interface.IScheduler,
    logger logginginterface.ILogger,
) *PriceFeedService {
    return &PriceFeedService{
        repository: repository,
        fetcher:    fetcher,
        aggregator: aggregator,
        publisher:  publisher,
        scheduler:  scheduler,
        logger:     logger,
        running:    false,
    }
}

// StartService starts all scheduled price fetching and publishing
func (s *PriceFeedService) StartService(ctx context.Context) error {
    s.mu.Lock()
    defer s.mu.Unlock()
    
    if s.running {
        return nil
    }
    
    // Start the scheduler
    if err := s.scheduler.Start(ctx); err != nil {
        return fmt.Errorf("failed to start scheduler: %w", err)
    }
    
    s.running = true
    s.lastStartTime = time.Now()
    s.logger.Info("Price Feed Service started")
    
    return nil
}

// (Other service methods implemented here)
```

### Fetcher Implementation

The fetcher is responsible for collecting price data from external sources:

```go
// Fetcher implements the IFetcher interface
type Fetcher struct {
    sources    map[model.PriceSource]ISourceAdapter
    repository model.IPriceRepository
    logger     logginginterface.ILogger
}

// NewFetcher creates a new price fetcher
func NewFetcher(
    repository model.IPriceRepository,
    logger logginginterface.ILogger,
) *Fetcher {
    return &Fetcher{
        sources:    make(map[model.PriceSource]ISourceAdapter),
        repository: repository,
        logger:     logger,
    }
}

// RegisterSource adds a data source adapter
func (f *Fetcher) RegisterSource(source model.PriceSource, adapter ISourceAdapter) {
    f.sources[source] = adapter
}

// FetchPrice fetches the current price for an asset from a source
func (f *Fetcher) FetchPrice(
    ctx context.Context,
    assetID string,
    source model.PriceSource,
) (*model.PriceData, error) {
    // Get the source adapter
    adapter, ok := f.sources[source]
    if !ok {
        return nil, fmt.Errorf("unsupported source: %s", source)
    }
    
    // Fetch the price from the source
    priceData, err := adapter.FetchPrice(ctx, assetID)
    if err != nil {
        return nil, fmt.Errorf("failed to fetch price from %s: %w", source, err)
    }
    
    // Set additional metadata
    priceData.ID = uuid.New()
    priceData.FetchedAt = time.Now().UTC()
    priceData.Source = source
    
    // Save to repository
    if err := f.repository.SavePriceData(ctx, priceData); err != nil {
        f.logger.Error("Failed to save price data",
            logginginterface.Field{Key: "asset_id", Value: assetID},
            logginginterface.Field{Key: "source", Value: string(source)},
            logginginterface.Field{Key: "error", Value: err.Error()},
        )
        // Continue even if saving fails
    }
    
    return priceData, nil
}

// (Other fetcher methods implemented here)
```

### Source Adapters

Source adapters provide a consistent interface to various price data sources:

```go
// ISourceAdapter defines the interface for a price data source
type ISourceAdapter interface {
    // FetchPrice fetches the current price for an asset
    FetchPrice(ctx context.Context, assetID string) (*model.PriceData, error)
    
    // GetSupportedAssets returns the list of supported assets
    GetSupportedAssets() []string
}

// BinanceAdapter implements the ISourceAdapter interface for Binance
type BinanceAdapter struct {
    apiKey      string
    apiSecret   string
    httpClient  *http.Client
    baseURL     string
    rateLimiter *rate.Limiter
    logger      logginginterface.ILogger
}

// NewBinanceAdapter creates a new Binance adapter
func NewBinanceAdapter(
    apiKey string,
    apiSecret string,
    logger logginginterface.ILogger,
) *BinanceAdapter {
    return &BinanceAdapter{
        apiKey:     apiKey,
        apiSecret:  apiSecret,
        httpClient: &http.Client{Timeout: 10 * time.Second},
        baseURL:    "https://api.binance.com",
        rateLimiter: rate.NewLimiter(rate.Limit(10), 1), // 10 requests per second
        logger:     logger,
    }
}

// FetchPrice fetches the current price for an asset from Binance
func (a *BinanceAdapter) FetchPrice(
    ctx context.Context,
    assetID string,
) (*model.PriceData, error) {
    // Wait for rate limiter
    if err := a.rateLimiter.Wait(ctx); err != nil {
        return nil, fmt.Errorf("rate limit wait error: %w", err)
    }
    
    // Format the symbol for Binance (e.g., "BTC-USD" to "BTCUSDT")
    symbol := formatBinanceSymbol(assetID)
    
    // Create request
    url := fmt.Sprintf("%s/api/v3/ticker/price?symbol=%s", a.baseURL, symbol)
    req, err := http.NewRequestWithContext(ctx, "GET", url, nil)
    if err != nil {
        return nil, fmt.Errorf("failed to create request: %w", err)
    }
    
    // Add auth headers
    req.Header.Add("X-MBX-APIKEY", a.apiKey)
    
    // Execute request
    resp, err := a.httpClient.Do(req)
    if err != nil {
        return nil, fmt.Errorf("request failed: %w", err)
    }
    defer resp.Body.Close()
    
    // Parse response
    var tickerResp struct {
        Symbol string `json:"symbol"`
        Price  string `json:"price"`
    }
    
    if err := json.NewDecoder(resp.Body).Decode(&tickerResp); err != nil {
        return nil, fmt.Errorf("failed to parse response: %w", err)
    }
    
    // Convert price to int64 with proper decimals
    priceFloat, err := strconv.ParseFloat(tickerResp.Price, 64)
    if err != nil {
        return nil, fmt.Errorf("failed to parse price value: %w", err)
    }
    
    // Use 8 decimals for crypto pricing
    decimals := 8
    priceInt := int64(priceFloat * math.Pow10(decimals))
    
    // Create price data
    priceData := &model.PriceData{
        AssetID:    assetID,
        Price:      priceInt,
        Decimals:   decimals,
        Source:     model.SourceBinance,
        Timestamp:  time.Now().UTC(),
        Confidence: 90, // Default high confidence for Binance
        Metadata: map[string]interface{}{
            "raw_symbol": tickerResp.Symbol,
            "raw_price":  tickerResp.Price,
        },
    }
    
    return priceData, nil
}

// (Other source adapters implemented similarly)
```

### Aggregator Implementation

The aggregator combines prices from multiple sources:

```go
// Aggregator implements the IAggregator interface
type Aggregator struct {
    repository model.IPriceRepository
    logger     logginginterface.ILogger
}

// NewAggregator creates a new price aggregator
func NewAggregator(
    repository model.IPriceRepository,
    logger logginginterface.ILogger,
) *Aggregator {
    return &Aggregator{
        repository: repository,
        logger:     logger,
    }
}

// AggregatePrices combines prices from multiple sources into a single price
func (a *Aggregator) AggregatePrices(
    ctx context.Context,
    assetID string,
    prices []*model.PriceData,
) (*model.AggregatedPrice, error) {
    if len(prices) == 0 {
        return nil, fmt.Errorf("no prices to aggregate")
    }
    
    // Filter out prices that are too old (> 5 minutes)
    now := time.Now().UTC()
    var validPrices []*model.PriceData
    var sourceIDs []uuid.UUID
    
    for _, price := range prices {
        if now.Sub(price.Timestamp) <= 5*time.Minute {
            validPrices = append(validPrices, price)
            sourceIDs = append(sourceIDs, price.ID)
        }
    }
    
    if len(validPrices) == 0 {
        return nil, fmt.Errorf("no recent prices to aggregate")
    }
    
    // Sort prices by value for median calculation
    sort.Slice(validPrices, func(i, j int) bool {
        return normalizePrice(validPrices[i]) < normalizePrice(validPrices[j])
    })
    
    // Calculate the median price
    var medianPrice int64
    var decimals int
    
    if len(validPrices)%2 == 0 {
        // Even number of prices, average the middle two
        middle1 := validPrices[len(validPrices)/2-1]
        middle2 := validPrices[len(validPrices)/2]
        
        // Need to normalize decimals for proper averaging
        norm1 := normalizePrice(middle1)
        norm2 := normalizePrice(middle2)
        avgNorm := (norm1 + norm2) / 2
        
        // Use the higher precision of the two for the result
        decimals = max(middle1.Decimals, middle2.Decimals)
        medianPrice = denormalizePrice(avgNorm, decimals)
    } else {
        // Odd number of prices, use the middle one
        middle := validPrices[len(validPrices)/2]
        medianPrice = middle.Price
        decimals = middle.Decimals
    }
    
    // Calculate average confidence
    var totalConfidence int
    for _, price := range validPrices {
        totalConfidence += price.Confidence
    }
    avgConfidence := totalConfidence / len(validPrices)
    
    // Create the aggregated price
    aggregated := &model.AggregatedPrice{
        ID:                uuid.New(),
        AssetID:           assetID,
        Price:             medianPrice,
        Decimals:          decimals,
        Timestamp:         now,
        AggregationMethod: "median",
        Sources:           sourceIDs,
        Confidence:        avgConfidence,
        Volatility:        calculateVolatility(validPrices),
        Published:         false,
    }
    
    // Save to repository
    if err := a.repository.SaveAggregatedPrice(ctx, aggregated); err != nil {
        a.logger.Error("Failed to save aggregated price",
            logginginterface.Field{Key: "asset_id", Value: assetID},
            logginginterface.Field{Key: "error", Value: err.Error()},
        )
        // Continue even if saving fails
    }
    
    return aggregated, nil
}

// ShouldPublish determines if a price change warrants publishing
func (a *Aggregator) ShouldPublish(
    ctx context.Context,
    newPrice *model.AggregatedPrice,
    lastPublished *model.AggregatedPrice,
    config *model.AssetConfig,
) (bool, string) {
    // Always publish if there's no previous published price
    if lastPublished == nil {
        return true, "initial publication"
    }
    
    // Calculate the time since last publication
    timeSinceLastPublish := time.Since(lastPublished.Timestamp)
    
    // Publish if the heartbeat interval has elapsed
    if config.HeartbeatInterval > 0 && timeSinceLastPublish >= config.HeartbeatInterval {
        return true, "heartbeat interval elapsed"
    }
    
    // Calculate price deviation
    oldNormalized := float64(lastPublished.Price) / math.Pow10(lastPublished.Decimals)
    newNormalized := float64(newPrice.Price) / math.Pow10(newPrice.Decimals)
    
    // Calculate percentage change
    percentChange := math.Abs((newNormalized - oldNormalized) / oldNormalized) * 100
    
    // Publish if deviation threshold is exceeded
    if percentChange >= config.DeviationThreshold {
        reason := fmt.Sprintf("%.2f%% price change exceeds %.2f%% threshold",
            percentChange, config.DeviationThreshold)
        return true, reason
    }
    
    return false, ""
}

// Helper functions
func normalizePrice(price *model.PriceData) float64 {
    return float64(price.Price) / math.Pow10(price.Decimals)
}

func denormalizePrice(normalizedPrice float64, decimals int) int64 {
    return int64(normalizedPrice * math.Pow10(decimals))
}

func calculateVolatility(prices []*model.PriceData) float64 {
    // Implementation of volatility calculation
    // This is a simplified example - real implementation would be more sophisticated
    if len(prices) <= 1 {
        return 0.0
    }
    
    // Convert to normalized prices
    normalizedPrices := make([]float64, len(prices))
    for i, price := range prices {
        normalizedPrices[i] = normalizePrice(price)
    }
    
    // Calculate mean
    var sum float64
    for _, price := range normalizedPrices {
        sum += price
    }
    mean := sum / float64(len(normalizedPrices))
    
    // Calculate variance
    var variance float64
    for _, price := range normalizedPrices {
        variance += math.Pow(price-mean, 2)
    }
    variance /= float64(len(normalizedPrices))
    
    // Standard deviation as volatility measure
    return math.Sqrt(variance)
}

func max(a, b int) int {
    if a > b {
        return a
    }
    return b
}
```

### Scheduler Implementation

The scheduler manages the timing of operations:

```go
// Scheduler implements the IScheduler interface
type Scheduler struct {
    repository model.IPriceRepository
    fetcher    interface.IFetcher
    aggregator interface.IAggregator
    publisher  interface.IPublisher
    logger     logginginterface.ILogger
    
    fetchJobs    map[string]*fetchJob
    publishJobs  map[string]*publishJob
    fetchMu      sync.RWMutex
    publishMu    sync.RWMutex
    running      bool
    stopChan     chan struct{}
}

type fetchJob struct {
    assetID   string
    interval  time.Duration
    timer     *time.Timer
    cancelCtx context.CancelFunc
}

type publishJob struct {
    assetID   string
    interval  time.Duration
    timer     *time.Timer
    cancelCtx context.CancelFunc
}

// NewScheduler creates a new scheduler
func NewScheduler(
    repository model.IPriceRepository,
    fetcher interface.IFetcher,
    aggregator interface.IAggregator,
    publisher interface.IPublisher,
    logger logginginterface.ILogger,
) *Scheduler {
    return &Scheduler{
        repository:  repository,
        fetcher:     fetcher,
        aggregator:  aggregator,
        publisher:   publisher,
        logger:      logger,
        fetchJobs:   make(map[string]*fetchJob),
        publishJobs: make(map[string]*publishJob),
        stopChan:    make(chan struct{}),
    }
}

// Start loads all asset configurations and starts their schedules
func (s *Scheduler) Start(ctx context.Context) error {
    if s.running {
        return nil
    }
    
    // Load all asset configurations
    configs, err := s.repository.ListAssetConfigs(ctx)
    if err != nil {
        return fmt.Errorf("failed to load asset configurations: %w", err)
    }
    
    // Set up schedules for each asset
    for _, config := range configs {
        if !config.Enabled {
            continue
        }
        
        if config.FetchInterval > 0 {
            if err := s.ScheduleFetch(ctx, config.AssetID, config.FetchInterval); err != nil {
                s.logger.Error("Failed to schedule fetch for asset",
                    logginginterface.Field{Key: "asset_id", Value: config.AssetID},
                    logginginterface.Field{Key: "error", Value: err.Error()},
                )
                // Continue with other assets
            }
        }
        
        if config.PublishInterval > 0 {
            if err := s.SchedulePublish(ctx, config.AssetID, config.PublishInterval); err != nil {
                s.logger.Error("Failed to schedule publish for asset",
                    logginginterface.Field{Key: "asset_id", Value: config.AssetID},
                    logginginterface.Field{Key: "error", Value: err.Error()},
                )
                // Continue with other assets
            }
        }
    }
    
    s.running = true
    s.logger.Info("Scheduler started")
    
    return nil
}

// ScheduleFetch schedules regular price fetching for an asset
func (s *Scheduler) ScheduleFetch(ctx context.Context, assetID string, interval time.Duration) error {
    // Check for existing job
    s.fetchMu.Lock()
    defer s.fetchMu.Unlock()
    
    if job, exists := s.fetchJobs[assetID]; exists {
        // Cancel existing job
        job.cancelCtx()
        job.timer.Stop()
    }
    
    // Create a new context for this job
    jobCtx, cancelCtx := context.WithCancel(ctx)
    
    // Create the job
    job := &fetchJob{
        assetID:   assetID,
        interval:  interval,
        cancelCtx: cancelCtx,
    }
    
    // Start the timer
    job.timer = time.AfterFunc(0, func() { s.runFetchJob(jobCtx, job) })
    
    // Store the job
    s.fetchJobs[assetID] = job
    
    s.logger.Info("Scheduled fetch job",
        logginginterface.Field{Key: "asset_id", Value: assetID},
        logginginterface.Field{Key: "interval", Value: interval.String()},
    )
    
    return nil
}

// runFetchJob executes a fetch job and reschedules it
func (s *Scheduler) runFetchJob(ctx context.Context, job *fetchJob) {
    s.logger.Debug("Running fetch job",
        logginginterface.Field{Key: "asset_id", Value: job.assetID},
    )
    
    // Get the asset configuration
    config, err := s.repository.GetAssetConfig(ctx, job.assetID)
    if err != nil {
        s.logger.Error("Failed to get asset config for fetch job",
            logginginterface.Field{Key: "asset_id", Value: job.assetID},
            logginginterface.Field{Key: "error", Value: err.Error()},
        )
        s.rescheduleFetchJob(ctx, job)
        return
    }
    
    // Fetch from all configured sources
    var prices []*model.PriceData
    var fetchErrors []error
    
    for _, source := range config.Sources {
        price, err := s.fetcher.FetchPrice(ctx, job.assetID, source)
        if err != nil {
            s.logger.Error("Failed to fetch price",
                logginginterface.Field{Key: "asset_id", Value: job.assetID},
                logginginterface.Field{Key: "source", Value: string(source)},
                logginginterface.Field{Key: "error", Value: err.Error()},
            )
            fetchErrors = append(fetchErrors, err)
            continue
        }
        
        prices = append(prices, price)
    }
    
    // Check if we have enough prices
    if len(prices) < config.MinSources {
        s.logger.Error("Insufficient sources for aggregation",
            logginginterface.Field{Key: "asset_id", Value: job.assetID},
            logginginterface.Field{Key: "required", Value: config.MinSources},
            logginginterface.Field{Key: "available", Value: len(prices)},
        )
        s.rescheduleFetchJob(ctx, job)
        return
    }
    
    // Aggregate prices
    aggregated, err := s.aggregator.AggregatePrices(ctx, job.assetID, prices)
    if err != nil {
        s.logger.Error("Failed to aggregate prices",
            logginginterface.Field{Key: "asset_id", Value: job.assetID},
            logginginterface.Field{Key: "error", Value: err.Error()},
        )
        s.rescheduleFetchJob(ctx, job)
        return
    }
    
    s.logger.Info("Successfully aggregated prices",
        logginginterface.Field{Key: "asset_id", Value: job.assetID},
        logginginterface.Field{Key: "price", Value: aggregated.Price},
        logginginterface.Field{Key: "sources", Value: len(prices)},
    )
    
    // Reschedule the job
    s.rescheduleFetchJob(ctx, job)
}

// rescheduleFetchJob reschedules a fetch job
func (s *Scheduler) rescheduleFetchJob(ctx context.Context, job *fetchJob) {
    s.fetchMu.RLock()
    defer s.fetchMu.RUnlock()
    
    // Check if we're still running and job hasn't been cancelled
    select {
    case <-s.stopChan:
        return
    case <-ctx.Done():
        return
    default:
        // Continue if not stopped
    }
    
    // Reset the timer
    job.timer = time.AfterFunc(job.interval, func() { s.runFetchJob(ctx, job) })
}

// (Other scheduler methods implemented similarly)
```

## Repository Implementation

The `IPriceRepository` interface can be implemented with various backends. Here's a SQL implementation example:

```go
// SQLPriceRepository implements the IPriceRepository interface using a SQL database
type SQLPriceRepository struct {
    db *sqlx.DB
}

// NewSQLPriceRepository creates a new SQL-based repository
func NewSQLPriceRepository(db *sqlx.DB) *SQLPriceRepository {
    return &SQLPriceRepository{
        db: db,
    }
}

// SavePriceData stores a new price data point
func (r *SQLPriceRepository) SavePriceData(ctx context.Context, price *model.PriceData) error {
    query := `
        INSERT INTO price_data (
            id, asset_id, price, decimals, source, timestamp, fetched_at,
            confidence, metadata
        ) VALUES (
            $1, $2, $3, $4, $5, $6, $7, $8, $9
        )
    `
    
    metadataJSON, err := json.Marshal(price.Metadata)
    if err != nil {
        return fmt.Errorf("failed to marshal metadata: %w", err)
    }
    
    _, err = r.db.ExecContext(ctx, query,
        price.ID,
        price.AssetID,
        price.Price,
        price.Decimals,
        price.Source,
        price.Timestamp,
        price.FetchedAt,
        price.Confidence,
        metadataJSON,
    )
    
    if err != nil {
        return fmt.Errorf("failed to insert price data: %w", err)
    }
    
    return nil
}

// (Other repository methods implemented similarly)
```

## Core Workflows

### Price Feed Lifecycle Workflow

The core price feed process involves fetching, aggregating, and publishing price data to the blockchain:

```
┌────────────────┐      ┌────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                │      │                │     │                 │     │                 │
│ Scheduler      ├─────►│ Service Layer  ├────►│ Fetcher         ├────►│ Data Sources    │
│                │      │                │     │                 │     │                 │
└────────────────┘      └────────────────┘     └─────────────────┘     └────────┬────────┘
                                                                                │
                                                                                │
                                                                                ▼
┌────────────────┐      ┌────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                │      │                │     │                 │     │                 │
│ Blockchain     │◄─────┤ Publisher      │◄────┤ Service Layer   │◄────┤ Aggregator      │
│ (Neo N3)       │      │                │     │                 │     │                 │
│                │      │                │     │                 │     │                 │
└────────────────┘      └────────────────┘     └─────────────────┘     └─────────────────┘
       │                                                                         ▲
       │                                                                         │
       ▼                                                                         │
┌────────────────┐                                                      ┌─────────────────┐
│                │                                                      │                 │
│ Blockchain     │                                                      │ Repository      │
│ Applications   │                                                      │ (PostgreSQL)    │
│                │                                                      │                 │
└────────────────┘                                                      └─────────────────┘
```

#### Implementation Details

The main workflow is implemented in the service layer:

```go
// FetchAndPublishPrices implements the core workflow for asset pricing
func (s *PriceFeedService) FetchAndPublishPrices(ctx context.Context, assetIDs []string) error {
    s.logger.Info("Starting price fetch and publish workflow",
        logginginterface.Field{Key: "assets", Value: strings.Join(assetIDs, ",")})
    
    // Step 1: Fetch latest prices from all configured sources
    var priceDataByAsset = make(map[string][]*model.PriceData)
    for _, assetID := range assetIDs {
        prices, err := s.fetcher.FetchPricesFromAllSources(ctx, assetID)
        if err != nil {
            s.logger.Error("Failed to fetch prices",
                logginginterface.Field{Key: "asset_id", Value: assetID},
                logginginterface.Field{Key: "error", Value: err.Error()})
            s.fetchErrors++
            // Continue with other assets even if one fails
            continue
        }
        
        if len(prices) > 0 {
            priceDataByAsset[assetID] = prices
        }
    }
    
    if len(priceDataByAsset) == 0 {
        return errors.New("no price data fetched for any assets")
    }
    
    // Step 2: Aggregate prices for each asset
    aggregatedPrices := make([]*model.AggregatedPrice, 0)
    for assetID, prices := range priceDataByAsset {
        if len(prices) == 0 {
            continue
        }
        
        aggregated, err := s.aggregator.Aggregate(ctx, prices)
        if err != nil {
            s.logger.Error("Failed to aggregate prices",
                logginginterface.Field{Key: "asset_id", Value: assetID},
                logginginterface.Field{Key: "error", Value: err.Error()})
            // Continue with other assets even if one fails
            continue
        }
        
        aggregatedPrices = append(aggregatedPrices, aggregated)
        
        // Save aggregated price to repository
        if err := s.repository.SaveAggregatedPrice(ctx, aggregated); err != nil {
            s.logger.Error("Failed to save aggregated price",
                logginginterface.Field{Key: "asset_id", Value: assetID},
                logginginterface.Field{Key: "error", Value: err.Error()})
            // Continue even if saving fails
        }
    }
    
    if len(aggregatedPrices) == 0 {
        return errors.New("no aggregated prices available for publishing")
    }
    
    // Step 3: Publish prices to blockchain
    if err := s.publisher.PublishPrices(ctx, aggregatedPrices); err != nil {
        s.logger.Error("Failed to publish prices",
            logginginterface.Field{Key: "error", Value: err.Error()})
        s.publishErrors++
        return fmt.Errorf("failed to publish prices: %w", err)
    }
    
    s.logger.Info("Successfully completed price fetch and publish workflow",
        logginginterface.Field{Key: "asset_count", Value: len(aggregatedPrices)})
    
    return nil
}
```

### Asset Registration Workflow

The process for registering a new asset for price feeding:

```
┌────────────────┐      ┌────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                │      │                │     │                 │     │                 │
│ Admin API      ├─────►│ Service Layer  ├────►│ Data Validation ├────►│ Fetcher         │
│                │      │                │     │                 │     │                 │
└────────────────┘      └────────────────┘     └─────────────────┘     └────────┬────────┘
                                                                                │
                                                                                ▼
┌────────────────┐      ┌────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                │      │                │     │                 │     │                 │
│ Admin API      │◄─────┤ Service Layer  │◄────┤ Repository      │◄────┤ Source Adapters │
│ (Response)     │      │                │     │                 │     │ (Testing)       │
│                │      │                │     │                 │     │                 │
└────────────────┘      └────────────────┘     └─────────────────┘     └─────────────────┘
                                                        │
                                                        ▼
                                               ┌─────────────────┐
                                               │                 │
                                               │ Scheduler       │
                                               │ (Update)        │
                                               │                 │
                                               └─────────────────┘
```

## Integration Examples

### Integration with Gas Bank Service

The Price Feed Service integrates with the Gas Bank Service for transaction funding:

```go
// Example integration with Gas Bank Service in the Publisher component
func (p *Publisher) publishToBlockchain(ctx context.Context, aggregatedPrices []*model.AggregatedPrice) error {
    // Prepare contract parameters
    parameters := make([]*pb.Parameter, 0)
    
    // Add the price data as parameters
    for _, price := range aggregatedPrices {
        parameters = append(parameters, 
            &pb.Parameter{
                Type: "String",
                Value: price.AssetID,
            },
            &pb.Parameter{
                Type: "Integer",
                Value: price.Price.String(),
            },
            &pb.Parameter{
                Type: "Integer",
                Value: strconv.FormatInt(price.Timestamp.Unix(), 10),
            },
        )
    }
    
    // Call Gas Bank Service to submit the transaction
    txReq := &gasbankpb.SendTransactionRequest{
        SigningAddressPurpose:    "price-feed-publisher",
        TargetContractScriptHash: p.contractHash,
        TargetMethod:             "updatePrices",
        Parameters:               parameters,
        WaitForConfirmation:      true,
        MaxSystemFee:             "2000000000", // 2 GAS system fee max
        MaxNetworkFee:            "1000000000", // 1 GAS network fee max
    }
    
    txResp, err := p.gasBankClient.SendTransaction(ctx, txReq)
    if err != nil {
        return fmt.Errorf("failed to send transaction: %w", err)
    }
    
    // Check response status
    if txResp.Status != "EXECUTED" {
        return fmt.Errorf("transaction execution failed: %s", txResp.Error.Message)
    }
    
    // Record transaction details
    for _, price := range aggregatedPrices {
        p.repository.SavePublishRecord(ctx, &model.PublishRecord{
            ID:            uuid.New(),
            AssetID:       price.AssetID,
            Price:         price.Price,
            Timestamp:     price.Timestamp,
            TransactionID: txResp.TxHash,
            PublishedAt:   time.Now().UTC(),
        })
    }
    
    return nil
}
```

### Integration with Metrics Service

The Price Feed Service integrates with the Metrics Service for operational monitoring:

```go
// Example integration with Metrics Service in the Service layer
func (s *PriceFeedService) reportMetrics(ctx context.Context) {
    // Create metrics batch
    metrics := []*metricspb.Metric{
        {
            Name:      "price_feed_fetch_count",
            Value:     float64(s.fetchCount),
            Timestamp: time.Now().Unix(),
            Labels: map[string]string{
                "service": "pricefeed",
                "success": "true",
            },
        },
        {
            Name:      "price_feed_fetch_errors",
            Value:     float64(s.fetchErrors),
            Timestamp: time.Now().Unix(),
            Labels: map[string]string{
                "service": "pricefeed",
                "success": "false",
            },
        },
        {
            Name:      "price_feed_publish_count",
            Value:     float64(s.publishCount),
            Timestamp: time.Now().Unix(),
            Labels: map[string]string{
                "service": "pricefeed",
                "success": "true",
            },
        },
        {
            Name:      "price_feed_publish_errors",
            Value:     float64(s.publishErrors),
            Timestamp: time.Now().Unix(),
            Labels: map[string]string{
                "service": "pricefeed",
                "success": "false",
            },
        },
    }
    
    // Send metrics
    _, err := s.metricsClient.ReportMetrics(ctx, &metricspb.ReportMetricsRequest{
        Metrics: metrics,
    })
    
    if err != nil {
        s.logger.Error("Failed to report metrics",
            logginginterface.Field{Key: "error", Value: err.Error()})
    }
}
```

## Deployment Considerations

### High Availability Configuration

The Price Feed Service should be deployed with high availability in mind:

```yaml
# Example Kubernetes deployment for high availability
apiVersion: apps/v1
kind: Deployment
metadata:
  name: pricefeed-service
spec:
  replicas: 3  # Multiple replicas for redundancy
  selector:
    matchLabels:
      app: pricefeed-service
  template:
    metadata:
      labels:
        app: pricefeed-service
    spec:
      containers:
      - name: pricefeed-service
        image: neo-service-layer/pricefeed-service:latest
        ports:
        - containerPort: 50051
        env:
        - name: DB_CONNECTION_STRING
          valueFrom:
            secretKeyRef:
              name: pricefeed-secrets
              key: db-connection
        - name: BINANCE_API_KEY
          valueFrom:
            secretKeyRef:
              name: pricefeed-secrets
              key: binance-api-key
        - name: BINANCE_API_SECRET
          valueFrom:
            secretKeyRef:
              name: pricefeed-secrets
              key: binance-api-secret
        readinessProbe:
          grpc:
            port: 50051
          initialDelaySeconds: 10
          periodSeconds: 15
        livenessProbe:
          grpc:
            port: 50051
          initialDelaySeconds: 20
          periodSeconds: 30
        resources:
          requests:
            memory: "512Mi"
            cpu: "200m"
          limits:
            memory: "1Gi"
            cpu: "500m"
        volumeMounts:
        - name: config-volume
          mountPath: /app/config
      volumes:
      - name: config-volume
        configMap:
          name: pricefeed-config
```

### Load Balancing Considerations

For optimal performance with multiple data sources, implement an intelligent load balancing strategy:

```go
// Example source adapter load balancing implementation
func (f *LoadBalancedFetcher) FetchPricesFromAllSources(ctx context.Context, assetID string) ([]*model.PriceData, error) {
    var prices []*model.PriceData
    var errors []error
    
    // Get all adapters that support this asset
    adapters := f.getAdaptersForAsset(assetID)
    if len(adapters) == 0 {
        return nil, fmt.Errorf("no sources support asset %s", assetID)
    }
    
    // Create a source selection strategy based on current conditions
    strategy := f.createSourceStrategy(adapters)
    
    // Get the prioritized list of sources
    sourceList := strategy.PrioritizeSources(ctx)
    
    // Try sources in order until enough data is collected or we run out of sources
    for _, source := range sourceList {
        // Check if we have enough data already
        if len(prices) >= f.minSourcesRequired {
            break
        }
        
        // Try to get price from this source
        price, err := f.FetchPrice(ctx, assetID, source)
        if err != nil {
            errors = append(errors, err)
            f.recordFailure(source)
            continue
        }
        
        prices = append(prices, price)
        f.recordSuccess(source)
    }
    
    // Check if we have enough data
    if len(prices) < f.minSourcesRequired {
        return prices, fmt.Errorf("failed to fetch enough prices, required %d, got %d: %v", 
            f.minSourcesRequired, len(prices), errors)
    }
    
    return prices, nil
}
```

## Failure Recovery

Implement robust failure recovery mechanisms:

```go
// Example of circuit breaker implementation for data sources
type CircuitBreaker struct {
    source          model.PriceSource
    failureCount    int
    failureThreshold int
    resetTimeout    time.Duration
    lastFailure     time.Time
    state           CircuitBreakerState
    mu              sync.RWMutex
}

type CircuitBreakerState string

const (
    CircuitClosedState   CircuitBreakerState = "CLOSED"   // Normal operation
    CircuitOpenState     CircuitBreakerState = "OPEN"     // Not allowing operations
    CircuitHalfOpenState CircuitBreakerState = "HALF_OPEN" // Testing if source is back
)

func (cb *CircuitBreaker) AllowRequest() bool {
    cb.mu.RLock()
    defer cb.mu.RUnlock()
    
    now := time.Now()
    
    switch cb.state {
    case CircuitOpenState:
        // Check if we should try half-open state
        if now.Sub(cb.lastFailure) > cb.resetTimeout {
            cb.mu.RUnlock()
            cb.mu.Lock()
            cb.state = CircuitHalfOpenState
            cb.mu.Unlock()
            cb.mu.RLock()
            return true
        }
        return false
        
    case CircuitHalfOpenState, CircuitClosedState:
        return true
    }
    
    return false
}

func (cb *CircuitBreaker) RecordSuccess() {
    cb.mu.Lock()
    defer cb.mu.Unlock()
    
    cb.failureCount = 0
    if cb.state == CircuitHalfOpenState {
        cb.state = CircuitClosedState
    }
}

func (cb *CircuitBreaker) RecordFailure() {
    cb.mu.Lock()
    defer cb.mu.Unlock()
    
    cb.failureCount++
    cb.lastFailure = time.Now()
    
    if cb.state == CircuitHalfOpenState || (cb.state == CircuitClosedState && cb.failureCount >= cb.failureThreshold) {
        cb.state = CircuitOpenState
    }
}
```

## Security Implementation

### API Key Management

External API keys should be securely managed:

```go
// Example secure API key management
type SecureCredentialManager struct {
    secretClient secretspb.secretserviceClient
    keyCache     map[string]cachedKey
    cacheMutex   sync.RWMutex
}

type cachedKey struct {
    value     string
    expiresAt time.Time
}

func (m *SecureCredentialManager) GetAPIKey(ctx context.Context, provider string) (string, error) {
    // Check cache first
    m.cacheMutex.RLock()
    cached, ok := m.keyCache[provider]
    m.cacheMutex.RUnlock()
    
    if ok && time.Now().Before(cached.expiresAt) {
        return cached.value, nil
    }
    
    // Get from secrets service
    secretPath := fmt.Sprintf("api-keys/pricefeed/%s", provider)
    resp, err := m.secretClient.GetSecret(ctx, &secretspb.GetSecretRequest{
        Path: secretPath,
    })
    
    if err != nil {
        return "", fmt.Errorf("failed to get API key for %s: %w", provider, err)
    }
    
    // Cache the key with expiration
    m.cacheMutex.Lock()
    m.keyCache[provider] = cachedKey{
        value:     resp.Secret.Value,
        expiresAt: time.Now().Add(15 * time.Minute),
    }
    m.cacheMutex.Unlock()
    
    return resp.Secret.Value, nil
}
```

### Data Validation

Implement robust data validation to prevent publishing invalid prices:

```go
// Example price validation
func (a *Aggregator) validatePrices(prices []*model.PriceData) ([]*model.PriceData, error) {
    if len(prices) == 0 {
        return nil, errors.New("no prices to validate")
    }
    
    validPrices := make([]*model.PriceData, 0, len(prices))
    
    for _, price := range prices {
        // Check for negative prices
        if price.Price.Sign() <= 0 {
            continue
        }
        
        // Check if price is within acceptable deviation from historical average
        isValid, err := a.isWithinHistoricalDeviation(price)
        if err != nil {
            a.logger.Warn("Failed to check price deviation",
                logginginterface.Field{Key: "asset_id", Value: price.AssetID},
                logginginterface.Field{Key: "source", Value: string(price.Source)},
                logginginterface.Field{Key: "error", Value: err.Error()})
            // If we can't validate, still include the price
            validPrices = append(validPrices, price)
            continue
        }
        
        if !isValid {
            a.logger.Warn("Price outside acceptable deviation",
                logginginterface.Field{Key: "asset_id", Value: price.AssetID},
                logginginterface.Field{Key: "source", Value: string(price.Source)},
                logginginterface.Field{Key: "price", Value: price.Price.String()})
            continue
        }
        
        validPrices = append(validPrices, price)
    }
    
    if len(validPrices) == 0 {
        return nil, errors.New("all prices were rejected during validation")
    }
    
    return validPrices, nil
}
```

## Performance Tuning

For optimal performance, tune the Price Feed Service:

### Database Connection Pooling

```go
// Example database connection configuration
func setupDatabase(ctx context.Context, connString string) (*pgxpool.Pool, error) {
    config, err := pgxpool.ParseConfig(connString)
    if err != nil {
        return nil, fmt.Errorf("unable to parse database connection string: %w", err)
    }
    
    // Tune connection pool settings
    config.MaxConns = 20
    config.MinConns = 5
    config.MaxConnLifetime = 1 * time.Hour
    config.MaxConnIdleTime = 30 * time.Minute
    config.HealthCheckPeriod = 1 * time.Minute
    
    // Connect to database
    pool, err := pgxpool.ConnectConfig(ctx, config)
    if err != nil {
        return nil, fmt.Errorf("unable to connect to database: %w", err)
    }
    
    // Test connection
    if err := pool.Ping(ctx); err != nil {
        return nil, fmt.Errorf("unable to ping database: %w", err)
    }
    
    return pool, nil
}
```

### Fetcher Concurrency

```go
// Example concurrent fetcher implementation
func (f *ConcurrentFetcher) FetchPricesFromAllSources(ctx context.Context, assetID string) ([]*model.PriceData, error) {
    // Get all adapters that support this asset
    adapters := f.getAdaptersForAsset(assetID)
    if len(adapters) == 0 {
        return nil, fmt.Errorf("no sources support asset %s", assetID)
    }
    
    // Create a context with timeout
    fetchCtx, cancel := context.WithTimeout(ctx, f.fetchTimeout)
    defer cancel()
    
    // Create channel for results
    resultChan := make(chan fetchResult, len(adapters))
    
    // Launch a goroutine for each source
    for source, adapter := range adapters {
        go func(s model.PriceSource, a ISourceAdapter) {
            price, err := a.FetchPrice(fetchCtx, assetID)
            resultChan <- fetchResult{
                source: s,
                price:  price,
                err:    err,
            }
        }(source, adapter)
    }
    
    // Collect results
    var prices []*model.PriceData
    var errors []error
    
    // Wait for all fetches or timeout
    for i := 0; i < len(adapters); i++ {
        select {
        case result := <-resultChan:
            if result.err != nil {
                errors = append(errors, fmt.Errorf("source %s: %w", result.source, result.err))
                continue
            }
            
            prices = append(prices, result.price)
            
        case <-fetchCtx.Done():
            // Timeout or cancellation
            return prices, fmt.Errorf("fetch operation timed out after %v: %w", 
                f.fetchTimeout, fetchCtx.Err())
        }
    }
    
    if len(prices) == 0 {
        return nil, fmt.Errorf("failed to fetch prices from any source: %v", errors)
    }
    
    return prices, nil
}
```

## Troubleshooting

Common issues and their resolutions:

### Data Source Connectivity

If the service cannot connect to external data sources:

1. **Check API Keys**: Verify API keys are valid and have not expired

   ```bash
   # Example command to check API key status
   curl -v -H "X-API-Key: $BINANCE_API_KEY" https://api.binance.com/api/v3/ping
   ```

2. **Inspect Network Connectivity**: Ensure outbound connections are allowed

   ```bash
   # Example network connectivity test
   nc -vz api.binance.com 443
   ```

3. **Review Rate Limits**: Check if you're exceeding rate limits

   ```go
   // Example rate limit implementation
   rateLimiter := rate.NewLimiter(rate.Limit(10), 1) // 10 requests per second
   if !rateLimiter.Allow() {
       // Wait for rate limit or adjust request patterns
   }
   ```

### Price Anomalies

If you observe unexpected price data:

1. **Check Aggregation Algorithm**: Ensure the aggregation is working properly

   ```go
   // Example median calculation
   func calculateMedian(prices []*big.Int) *big.Int {
       if len(prices) == 0 {
           return nil
       }
       
       sort.Slice(prices, func(i, j int) bool {
           return prices[i].Cmp(prices[j]) < 0
       })
       
       middle := len(prices) / 2
       if len(prices)%2 == 0 {
           // Average the two middle values
           sum := new(big.Int).Add(prices[middle-1], prices[middle])
           return new(big.Int).Div(sum, big.NewInt(2))
       }
       
       return prices[middle]
   }
   ```

2. **Implement Outlier Detection**: Add outlier detection to your validation

   ```go
   // Example outlier detection using IQR
   func (a *Aggregator) detectOutliers(prices []*model.PriceData) []*model.PriceData {
       if len(prices) < 4 {
           return prices // Not enough data for meaningful outlier detection
       }
       
       // Extract price values
       values := make([]*big.Int, len(prices))
       for i, p := range prices {
           values[i] = p.Price
       }
       
       // Sort values
       sort.Slice(values, func(i, j int) bool {
           return values[i].Cmp(values[j]) < 0
       })
       
       // Calculate Q1 and Q3
       q1Index := len(values) / 4
       q3Index := q1Index * 3
       q1 := values[q1Index]
       q3 := values[q3Index]
       
       // Calculate IQR
       iqr := new(big.Int).Sub(q3, q1)
       
       // Define lower and upper bounds (Q1 - 1.5*IQR, Q3 + 1.5*IQR)
       lowerFactor := new(big.Int).Div(iqr, big.NewInt(2))
       lowerFactor = new(big.Int).Mul(lowerFactor, big.NewInt(3)) // 1.5 = 3/2
       lowerBound := new(big.Int).Sub(q1, lowerFactor)
       
       upperFactor := new(big.Int).Div(iqr, big.NewInt(2))
       upperFactor = new(big.Int).Mul(upperFactor, big.NewInt(3)) // 1.5 = 3/2
       upperBound := new(big.Int).Add(q3, upperFactor)
       
       // Filter outliers
       result := make([]*model.PriceData, 0)
       for _, price := range prices {
           if price.Price.Cmp(lowerBound) >= 0 && price.Price.Cmp(upperBound) <= 0 {
               result = append(result, price)
           }
       }
       
       return result
   }
   ```

### Transaction Publishing Failures

If transactions are failing to publish to the blockchain:

1. **Check Gas Bank Service**: Ensure it's running and accessible

   ```go
   // Example health check for Gas Bank Service
   func (p *Publisher) checkGasBankHealth(ctx context.Context) error {
       ctx, cancel := context.WithTimeout(ctx, 5*time.Second)
       defer cancel()
       
       _, err := p.gasBankClient.GetSigningAddress(ctx, &gasbankpb.GetSigningAddressRequest{
           Purpose: "price-feed-publisher",
       })
       
       if err != nil {
           return fmt.Errorf("gas bank service health check failed: %w", err)
       }
       
       return nil
   }
   ```

2. **Verify Contract Address**: Ensure the contract hash is correct

   ```go
   // Example contract validation
   func (p *Publisher) validateContract(ctx context.Context) error {
       contractAddress, err := p.convertScriptHashToAddress(p.contractHash)
       if err != nil {
           return fmt.Errorf("invalid contract hash: %w", err)
       }
       
       // Verify contract exists on blockchain
       exists, err := p.neoClient.ContractExists(ctx, contractAddress)
       if err != nil {
           return fmt.Errorf("failed to check contract existence: %w", err)
       }
       
       if !exists {
           return fmt.Errorf("contract does not exist at address %s", contractAddress)
       }
       
       return nil
   }
   ```

## Future Enhancements

Planned enhancements for the Price Feed Service:

1. **Machine Learning-Based Anomaly Detection**: Implement advanced ML models to detect price anomalies
2. **Multi-Chain Support**: Expand price feed publishing to additional blockchain networks
3. **Decentralized Oracle Inputs**: Add support for incorporating decentralized oracle data
4. **Custom Aggregation Algorithms**: Allow configurable aggregation algorithms per asset
