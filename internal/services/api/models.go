package api

import (
	"time"

	"github.com/nspcc-dev/neo-go/pkg/util"
)

// Config holds configuration for the API service
type Config struct {
	Port                 int           // HTTP port to listen on
	Host                 string        // Host to bind to
	ReadTimeout          time.Duration // HTTP read timeout
	WriteTimeout         time.Duration // HTTP write timeout
	IdleTimeout          time.Duration // HTTP idle timeout
	MaxRequestBodySize   int64         // Maximum size of request body in bytes
	EnableCORS           bool          // Whether to enable CORS
	AllowedOrigins       []string      // Allowed origins for CORS
	EnableHTTPS          bool          // Whether to enable HTTPS
	CertFile             string        // TLS certificate file
	KeyFile              string        // TLS key file
	EnableRateLimiting   bool          // Whether to enable rate limiting
	RateLimitPerMinute   int           // Number of requests allowed per minute per IP
	JWTSecret            string        // Secret for JWT authentication
	JWTExpiryDuration    time.Duration // JWT token expiry duration
	EnableRequestLogging bool          // Whether to enable request logging
}

// Route represents an API route
type Route struct {
	Path        string   // URL path
	Method      string   // HTTP method
	Handler     string   // Handler function name
	Middlewares []string // Middleware function names
	Protected   bool     // Whether route requires authentication
}

// APIError represents an API error response
type APIError struct {
	Code    int    `json:"code"`    // Error code
	Message string `json:"message"` // Error message
	Details string `json:"details"` // Error details
}

// SignatureVerificationRequest represents a request to verify a signature
type SignatureVerificationRequest struct {
	Address   string `json:"address"`   // NEO address
	Message   string `json:"message"`   // Message that was signed
	Signature string `json:"signature"` // Signature to verify
	PublicKey string `json:"publicKey"` // Public key (hex) for verification
	Salt      string `json:"salt"`      // Optional salt used in signing
}

// SignatureVerificationResponse represents the response from signature verification
type SignatureVerificationResponse struct {
	Valid      bool         `json:"valid"`      // Whether the signature is valid
	Address    string       `json:"address"`    // NEO address
	ScriptHash util.Uint160 `json:"scriptHash"` // Script hash of the address
}

// FunctionRequest represents a request to manage a function
type FunctionRequest struct {
	Name        string                 `json:"name"`        // Function name
	Description string                 `json:"description"` // Function description
	Code        string                 `json:"code"`        // Function code
	Runtime     string                 `json:"runtime"`     // Function runtime
	Metadata    map[string]interface{} `json:"metadata"`    // Additional metadata
}

// FunctionInvocationRequest represents a request to invoke a function
type FunctionInvocationRequest struct {
	FunctionID  string                 `json:"functionId"`  // Function ID
	Parameters  map[string]interface{} `json:"parameters"`  // Invocation parameters
	Async       bool                   `json:"async"`       // Whether to run asynchronously
	TraceID     string                 `json:"traceId"`     // For request tracing
	Idempotency string                 `json:"idempotency"` // Idempotency key
}

// SecretRequest represents a request to manage a secret
type SecretRequest struct {
	Key     string                 `json:"key"`     // Secret key
	Value   string                 `json:"value"`   // Secret value
	TTL     int64                  `json:"ttl"`     // Time-to-live in seconds
	Tags    []string               `json:"tags"`    // Secret tags
	Options map[string]interface{} `json:"options"` // Additional options
}

// StatsResponse represents API usage statistics
type StatsResponse struct {
	TotalRequests       int64     `json:"totalRequests"`       // Total number of requests
	TotalErrors         int64     `json:"totalErrors"`         // Total number of errors
	AverageResponseTime int64     `json:"averageResponseTime"` // Average response time in ms
	RequestsPerMinute   int64     `json:"requestsPerMinute"`   // Requests per minute
	LastUpdated         time.Time `json:"lastUpdated"`         // When stats were last updated
}

// UserProfile represents a user's profile
type UserProfile struct {
	Address              string       `json:"address"`              // NEO address
	ScriptHash           util.Uint160 `json:"scriptHash"`           // Script hash
	FunctionCount        int          `json:"functionCount"`        // Number of functions
	SecretCount          int          `json:"secretCount"`          // Number of secrets
	TriggerCount         int          `json:"triggerCount"`         // Number of triggers
	GasBalance           string       `json:"gasBalance"`           // GAS balance
	LastActivity         time.Time    `json:"lastActivity"`         // Last activity time
	CreatedAt            time.Time    `json:"createdAt"`            // When the user was created
	NotificationsEnabled bool         `json:"notificationsEnabled"` // Whether notifications are enabled
}

// HealthStatus represents the health status of a service
type HealthStatus struct {
	Service     string    `json:"service"`     // Service name
	Status      string    `json:"status"`      // Service status
	Version     string    `json:"version"`     // Service version
	Message     string    `json:"message"`     // Status message
	LastChecked time.Time `json:"lastChecked"` // When the service was last checked
}

// SystemHealthResponse represents the overall system health
type SystemHealthResponse struct {
	Healthy   bool           `json:"healthy"`   // Whether the system is healthy
	Services  []HealthStatus `json:"services"`  // Service health statuses
	Uptime    int64          `json:"uptime"`    // System uptime in seconds
	Region    string         `json:"region"`    // System region
	Timestamp time.Time      `json:"timestamp"` // Current timestamp
	Version   string         `json:"version"`   // System version
}
