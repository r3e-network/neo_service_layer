package tee

import (
	"context"
	"crypto/sha256"
	"crypto/x509"
	"encoding/hex"
	"fmt"
	"sync"
	"time"

	"github.com/hashicorp/golang-lru/v2"
	"github.com/sirupsen/logrus"
	"golang.org/x/time/rate"
)

const (
	// Default cache sizes
	defaultCertCacheSize    = 1000
	defaultQuoteCacheSize   = 5000
	defaultSigRLCacheSize   = 100
	defaultTCBInfoCacheSize = 100

	// Default cache TTLs
	defaultCertCacheTTL    = 24 * time.Hour
	defaultQuoteCacheTTL   = 1 * time.Hour
	defaultSigRLCacheTTL   = 6 * time.Hour
	defaultTCBInfoCacheTTL = 12 * time.Hour

	// Default cache expiration times
	defaultQuoteCacheExpiration = 24 * time.Hour
	defaultSigRLCacheExpiration = 24 * time.Hour

	// Default rate limits
	defaultQuoteVerificationRateLimit = 100 // per minute
	defaultIASRequestRateLimit        = 50  // per minute

	// Default burst limits
	defaultQuoteVerificationBurst = 10
	defaultIASRequestBurst        = 5
)

// CacheConfig holds configuration for various caches
type CacheConfig struct {
	CertCacheSize    int
	QuoteCacheSize   int
	SigRLCacheSize   int
	TCBInfoCacheSize int

	CertCacheTTL    time.Duration
	QuoteCacheTTL   time.Duration
	SigRLCacheTTL   time.Duration
	TCBInfoCacheTTL time.Duration

	QuoteCacheExpiration       time.Duration
	SigRLCacheExpiration       time.Duration
	QuoteVerificationRateLimit float64 // per minute
	IASRequestRateLimit        float64 // per minute
	QuoteVerificationBurst     int
	IASRequestBurst            int
}

// RateLimitConfig holds configuration for rate limiters
type RateLimitConfig struct {
	QuoteVerifyRate  float64
	QuoteVerifyBurst int
	IASRequestRate   float64
	IASRequestBurst  int
}

// cacheEntry represents a cached item with expiration
type cacheEntry struct {
	Value      interface{}
	Expiration time.Time
}

// Cache implements caching and rate limiting for IAS operations
type Cache struct {
	certCache    *lru.Cache[string, *cacheEntry]
	quotes       map[string]*quoteCacheEntry
	sigRLs       map[string]*sigRLCacheEntry
	tcbInfoCache *lru.Cache[string, *cacheEntry]

	quoteLimiter *rate.Limiter
	iasLimiter   *rate.Limiter

	config *CacheConfig
	mu     sync.RWMutex
}

type quoteCacheEntry struct {
	response   *IASResponse
	expiration time.Time
}

type sigRLCacheEntry struct {
	sigRL      []byte
	expiration time.Time
}

// NewCache creates a new cache instance with the given configuration
func NewCache(cacheConfig *CacheConfig, rateConfig *RateLimitConfig, logger *logrus.Logger) (*Cache, error) {
	if cacheConfig == nil {
		cacheConfig = &CacheConfig{
			CertCacheSize:              defaultCertCacheSize,
			QuoteCacheSize:             defaultQuoteCacheSize,
			SigRLCacheSize:             defaultSigRLCacheSize,
			TCBInfoCacheSize:           defaultTCBInfoCacheSize,
			CertCacheTTL:               defaultCertCacheTTL,
			QuoteCacheTTL:              defaultQuoteCacheTTL,
			SigRLCacheTTL:              defaultSigRLCacheTTL,
			TCBInfoCacheTTL:            defaultTCBInfoCacheTTL,
			QuoteCacheExpiration:       defaultQuoteCacheExpiration,
			SigRLCacheExpiration:       defaultSigRLCacheExpiration,
			QuoteVerificationRateLimit: 100.0, // per minute
			IASRequestRateLimit:        50.0,  // per minute
			QuoteVerificationBurst:     10,
			IASRequestBurst:            5,
		}
	}

	if rateConfig == nil {
		rateConfig = &RateLimitConfig{
			QuoteVerifyRate:  100.0 / 60.0, // convert to per second
			QuoteVerifyBurst: 10,
			IASRequestRate:   50.0 / 60.0, // convert to per second
			IASRequestBurst:  5,
		}
	}

	certCache, err := lru.New[string, *cacheEntry](cacheConfig.CertCacheSize)
	if err != nil {
		return nil, fmt.Errorf("failed to create cert cache: %w", err)
	}

	tcbInfoCache, err := lru.New[string, *cacheEntry](cacheConfig.TCBInfoCacheSize)
	if err != nil {
		return nil, fmt.Errorf("failed to create TCB info cache: %w", err)
	}

	return &Cache{
		certCache:    certCache,
		quotes:       make(map[string]*quoteCacheEntry),
		sigRLs:       make(map[string]*sigRLCacheEntry),
		tcbInfoCache: tcbInfoCache,
		quoteLimiter: rate.NewLimiter(rate.Limit(rateConfig.QuoteVerifyRate), rateConfig.QuoteVerifyBurst),
		iasLimiter:   rate.NewLimiter(rate.Limit(rateConfig.IASRequestRate), rateConfig.IASRequestBurst),
		config:       cacheConfig,
	}, nil
}

// GetCachedCert retrieves a cached certificate
func (c *Cache) GetCachedCert(key string) (*x509.Certificate, bool) {
	c.mu.RLock()
	defer c.mu.RUnlock()

	if entry, ok := c.certCache.Get(key); ok {
		if time.Now().Before(entry.Expiration) {
			if cert, ok := entry.Value.(*x509.Certificate); ok {
				return cert, true
			}
		}
		c.certCache.Remove(key)
	}
	return nil, false
}

// CacheCert caches a certificate
func (c *Cache) CacheCert(key string, cert *x509.Certificate) {
	c.mu.Lock()
	defer c.mu.Unlock()

	c.certCache.Add(key, &cacheEntry{
		Value:      cert,
		Expiration: time.Now().Add(c.config.CertCacheTTL),
	})
}

// GetCachedQuote retrieves a cached quote verification result
func (c *Cache) GetCachedQuote(quoteBytes []byte) (*IASResponse, bool) {
	c.mu.RLock()
	defer c.mu.RUnlock()

	key := c.quoteKey(quoteBytes)
	entry, ok := c.quotes[key]
	if !ok {
		return nil, false
	}

	// Check expiration
	if time.Now().After(entry.expiration) {
		go c.removeExpiredQuote(key) // Clean up in background
		return nil, false
	}

	return entry.response, true
}

// CacheQuote caches a quote verification result
func (c *Cache) CacheQuote(quoteBytes []byte, resp *IASResponse) {
	c.mu.Lock()
	defer c.mu.Unlock()

	key := c.quoteKey(quoteBytes)
	c.quotes[key] = &quoteCacheEntry{
		response:   resp,
		expiration: time.Now().Add(c.config.QuoteCacheExpiration),
	}
}

// GetCachedSigRL retrieves a cached SigRL
func (c *Cache) GetCachedSigRL(gid []byte) ([]byte, bool) {
	c.mu.RLock()
	defer c.mu.RUnlock()

	key := hex.EncodeToString(gid)
	entry, ok := c.sigRLs[key]
	if !ok {
		return nil, false
	}

	// Check expiration
	if time.Now().After(entry.expiration) {
		go c.removeExpiredSigRL(key) // Clean up in background
		return nil, false
	}

	return entry.sigRL, true
}

// CacheSigRL caches a SigRL
func (c *Cache) CacheSigRL(gid []byte, sigRL []byte) {
	c.mu.Lock()
	defer c.mu.Unlock()

	key := hex.EncodeToString(gid)
	c.sigRLs[key] = &sigRLCacheEntry{
		sigRL:      sigRL,
		expiration: time.Now().Add(c.config.SigRLCacheExpiration),
	}
}

// WaitQuoteVerification waits for quote verification rate limit
func (c *Cache) WaitQuoteVerification(ctx context.Context) error {
	if err := c.quoteLimiter.Wait(ctx); err != nil {
		return fmt.Errorf("quote verification rate limit wait failed: %w", err)
	}
	return nil
}

// WaitIASRequest waits for IAS request rate limit
func (c *Cache) WaitIASRequest(ctx context.Context) error {
	if err := c.iasLimiter.Wait(ctx); err != nil {
		return fmt.Errorf("IAS request rate limit wait failed: %w", err)
	}
	return nil
}

// Flush clears all caches
func (c *Cache) Flush() {
	c.mu.Lock()
	defer c.mu.Unlock()

	c.certCache.Purge()
	c.quotes = make(map[string]*quoteCacheEntry)
	c.sigRLs = make(map[string]*sigRLCacheEntry)
	c.tcbInfoCache.Purge()
}

// StartCleanup starts a background goroutine to clean expired cache entries
func (c *Cache) StartCleanup(ctx context.Context) {
	go func() {
		ticker := time.NewTicker(time.Hour)
		defer ticker.Stop()

		for {
			select {
			case <-ctx.Done():
				return
			case <-ticker.C:
				c.cleanExpiredEntries()
			}
		}
	}()
}

// cleanExpiredEntries removes expired entries from all caches
func (c *Cache) cleanExpiredEntries() {
	c.mu.Lock()
	defer c.mu.Unlock()

	now := time.Now()

	// Clean cert cache
	for _, key := range c.certCache.Keys() {
		if entry, ok := c.certCache.Get(key); ok {
			if now.After(entry.Expiration) {
				c.certCache.Remove(key)
			}
		}
	}

	// Clean quotes cache
	for key, entry := range c.quotes {
		if now.After(entry.expiration) {
			delete(c.quotes, key)
		}
	}

	// Clean SigRL cache
	for key, entry := range c.sigRLs {
		if now.After(entry.expiration) {
			delete(c.sigRLs, key)
		}
	}

	// Clean TCB info cache
	for _, key := range c.tcbInfoCache.Keys() {
		if entry, ok := c.tcbInfoCache.Get(key); ok {
			if now.After(entry.Expiration) {
				c.tcbInfoCache.Remove(key)
			}
		}
	}
}

// quoteKey generates a cache key for a quote
func (c *Cache) quoteKey(quoteBytes []byte) string {
	hash := sha256.Sum256(quoteBytes)
	return hex.EncodeToString(hash[:])
}

// removeExpiredQuote removes an expired quote from the cache
func (c *Cache) removeExpiredQuote(key string) {
	c.mu.Lock()
	defer c.mu.Unlock()
	delete(c.quotes, key)
}

// removeExpiredSigRL removes an expired SigRL from the cache
func (c *Cache) removeExpiredSigRL(key string) {
	c.mu.Lock()
	defer c.mu.Unlock()
	delete(c.sigRLs, key)
}
