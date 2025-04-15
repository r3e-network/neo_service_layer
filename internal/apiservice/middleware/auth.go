package middleware

import (
	"context"
	"net/http"
	"sync"
	"time"

	"github.com/go-chi/jwtauth/v5"
	"github.com/nspcc-dev/neo-go/pkg/encoding/address"
	"golang.org/x/time/rate"
)

// ipRateLimiter represents a rate limiter for a specific IP
type ipRateLimiter struct {
	limiter  *rate.Limiter
	lastSeen time.Time
}

// rateLimiterMap stores rate limiters for each IP
type rateLimiterMap struct {
	sync.RWMutex
	limiters map[string]*ipRateLimiter
	rate     rate.Limit
	burst    int
}

// newRateLimiterMap creates a new rate limiter map
func newRateLimiterMap(requestsPerMinute int) *rateLimiterMap {
	return &rateLimiterMap{
		limiters: make(map[string]*ipRateLimiter),
		rate:     rate.Limit(float64(requestsPerMinute) / 60.0),
		burst:    requestsPerMinute,
	}
}

// getLimiter gets or creates a rate limiter for an IP
func (m *rateLimiterMap) getLimiter(ip string) *rate.Limiter {
	m.Lock()
	defer m.Unlock()

	limiter, exists := m.limiters[ip]

	if !exists {
		limiter = &ipRateLimiter{
			limiter:  rate.NewLimiter(m.rate, m.burst),
			lastSeen: time.Now(),
		}
		m.limiters[ip] = limiter
	} else {
		limiter.lastSeen = time.Now()
	}

	return limiter.limiter
}

// cleanup removes old limiters
func (m *rateLimiterMap) cleanup(maxAge time.Duration) {
	m.Lock()
	defer m.Unlock()

	for ip, limiter := range m.limiters {
		if time.Since(limiter.lastSeen) > maxAge {
			delete(m.limiters, ip)
		}
	}
}

// AddressFromToken extracts the address from the JWT token and adds it to the request context
func AddressFromToken(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		_, claims, err := jwtauth.FromContext(r.Context())
		if err != nil {
			http.Error(w, err.Error(), http.StatusUnauthorized)
			return
		}

		// Extract address from claims
		addressStr, ok := claims["address"].(string)
		if !ok || addressStr == "" {
			http.Error(w, "invalid token: missing address", http.StatusUnauthorized)
			return
		}

		// Convert address to Uint160
		scriptHash, err := address.StringToUint160(addressStr)
		if err != nil {
			http.Error(w, "invalid address in token", http.StatusUnauthorized)
			return
		}

		// Set address in context
		ctx := context.WithValue(r.Context(), "address", scriptHash)
		next.ServeHTTP(w, r.WithContext(ctx))
	})
}

// JWTAuthenticator middleware authenticates JWT tokens
func JWTAuthenticator(ja *jwtauth.JWTAuth) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			token, claims, err := jwtauth.FromContext(r.Context())
			if err != nil {
				http.Error(w, err.Error(), http.StatusUnauthorized)
				return
			}

			if token == nil {
				http.Error(w, "unauthorized", http.StatusUnauthorized)
				return
			}

			// Check expiration
			if exp, ok := claims["exp"].(float64); ok {
				if time.Now().Unix() > int64(exp) {
					http.Error(w, "token expired", http.StatusUnauthorized)
					return
				}
			}

			// Check address claim
			if _, ok := claims["address"].(string); !ok {
				http.Error(w, "invalid token: missing address", http.StatusUnauthorized)
				return
			}

			next.ServeHTTP(w, r)
		})
	}
}

var (
	limiterMap     *rateLimiterMap
	limiterMapOnce sync.Once
)

// RateLimiter middleware implements rate limiting
func RateLimiter(requestsPerMinute int) func(http.Handler) http.Handler {
	limiterMapOnce.Do(func() {
		limiterMap = newRateLimiterMap(requestsPerMinute)
		// Start cleanup goroutine
		go func() {
			ticker := time.NewTicker(time.Hour)
			defer ticker.Stop()
			for range ticker.C {
				limiterMap.cleanup(24 * time.Hour)
			}
		}()
	})

	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			ip := r.RemoteAddr
			if forwardedFor := r.Header.Get("X-Forwarded-For"); forwardedFor != "" {
				ip = forwardedFor
			}

			limiter := limiterMap.getLimiter(ip)
			if !limiter.Allow() {
				http.Error(w, "Too many requests", http.StatusTooManyRequests)
				return
			}

			next.ServeHTTP(w, r)
		})
	}
}
