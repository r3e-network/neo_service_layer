# API Service Implementation

*Last Updated: 2023-12-20*

## Overview

This document provides implementation details for the API Service, including technology choices, code structure, and design decisions.

## Technology Stack

The API Service is built using the following technologies:

- **Languages**: Go (primary)
- **Frameworks**:
  - gRPC and gRPC-gateway for API endpoints
  - Protobuf for interface definitions
  - Chi router for HTTP endpoints
- **Security**:
  - JWT for authentication
  - TLS 1.3 for transport security
  - mTLS for service-to-service authentication
- **Monitoring**:
  - Prometheus for metrics
  - OpenTelemetry for tracing
  - Structured JSON logging
- **Rate Limiting**: Token bucket algorithm implemented using Redis
- **Caching**: Redis for response caching
- **Documentation**: OpenAPI (Swagger) for REST API documentation
- **Deployment**:
  - Docker containers
  - Kubernetes for orchestration

## Architecture Pattern

The API Service follows several architecture patterns:

- Backend for Frontend (BFF) pattern
- API Gateway pattern for routing and aggregation
- Command Query Responsibility Segregation (CQRS) for operation types

## Code Structure

The service follows a clean architecture pattern with the following structure:

```
/internal/apiservice/
├── api/                   # API layer
│   ├── grpc/              # gRPC service implementation
│   ├── http/              # HTTP handlers (REST API)
│   └── middleware/        # Common middleware for auth, logging, etc.
│
├── core/                  # Core domain logic
│   ├── model/             # Domain models
│   ├── ports/             # Interface definitions
│   ├── service/           # Service implementations
│   └── errors/            # Domain-specific errors
│
├── infrastructure/        # External dependencies implementations
│   ├── auth/              # Authentication services
│   ├── clients/           # Service clients
│   │   ├── functions/     # Functions service client
│   │   ├── automation/    # Automation service client
│   │   └── keys/          # Key service client
│   └── metrics/           # Metrics and monitoring
│
└── delivery/              # Entry points
    ├── cmd/               # Command-line interface
    └── config/            # Configuration loading
```

## Key Components

### API Layer

The API layer defines interfaces for handling different service requests:

```go
// Handler interfaces
type IFunctionHandler interface {
    RegisterFunction(ctx context.Context, request *pb.RegisterFunctionRequest) (*pb.Function, error)
    GetFunction(ctx context.Context, request *pb.GetFunctionRequest) (*pb.Function, error)
    ListFunctions(ctx context.Context, request *pb.ListFunctionsRequest) (*pb.ListFunctionsResponse, error)
    // Additional methods...
}

type IAutomationHandler interface {
    RegisterJob(ctx context.Context, request *pb.RegisterJobRequest) (*pb.Job, error)
    GetJob(ctx context.Context, request *pb.GetJobRequest) (*pb.Job, error)
    ListJobs(ctx context.Context, request *pb.ListJobsRequest) (*pb.ListJobsResponse, error)
    // Additional methods...
}
```

### API Gateway

The API Gateway is responsible for:

1. **Request Routing**
   - Routing requests to appropriate services
   - Protocol translation between HTTP and gRPC
   - URL path normalization

2. **Response Formatting**
   - Consistent JSON response structure
   - Error handling and formatting
   - HTTP status code mapping

Implementation:

```go
// APIGateway handles routing and request/response formatting
type APIGateway struct {
    router          *chi.Mux
    functionClient  FunctionServiceClient
    automationClient AutomationServiceClient
    keyClient       secretserviceClient
    authMiddleware  AuthMiddleware
    logger          *zap.Logger
}

// NewAPIGateway creates a new API gateway
func NewAPIGateway(
    functionClient FunctionServiceClient,
    automationClient AutomationServiceClient,
    keyClient secretserviceClient,
    authMiddleware AuthMiddleware,
    logger *zap.Logger,
) *APIGateway {
    router := chi.NewRouter()
    
    gateway := &APIGateway{
        router:          router,
        functionClient:  functionClient,
        automationClient: automationClient,
        keyClient:       keyClient,
        authMiddleware:  authMiddleware,
        logger:          logger,
    }
    
    gateway.setupRoutes()
    return gateway
}

// setupRoutes configures all API routes
func (g *APIGateway) setupRoutes() {
    // Global middleware
    g.router.Use(middleware.RequestID)
    g.router.Use(middleware.RealIP)
    g.router.Use(middleware.Logger)
    g.router.Use(middleware.Recoverer)
    g.router.Use(middleware.Timeout(60 * time.Second))
    
    // API routes
    g.router.Route("/v1", func(r chi.Router) {
        // Functions API
        r.Route("/functions", func(r chi.Router) {
            r.Get("/", g.authMiddleware.RequireAuth("functions.list"), g.ListFunctions)
            r.Post("/", g.authMiddleware.RequireAuth("functions.create"), g.CreateFunction)
            r.Get("/{id}", g.authMiddleware.RequireAuth("functions.get"), g.GetFunction)
            r.Put("/{id}", g.authMiddleware.RequireAuth("functions.update"), g.UpdateFunction)
            r.Delete("/{id}", g.authMiddleware.RequireAuth("functions.delete"), g.DeleteFunction)
        })
        
        // Automation API
        r.Route("/jobs", func(r chi.Router) {
            r.Get("/", g.authMiddleware.RequireAuth("jobs.list"), g.ListJobs)
            r.Post("/", g.authMiddleware.RequireAuth("jobs.create"), g.CreateJob)
            r.Get("/{id}", g.authMiddleware.RequireAuth("jobs.get"), g.GetJob)
            r.Put("/{id}", g.authMiddleware.RequireAuth("jobs.update"), g.UpdateJob)
            r.Delete("/{id}", g.authMiddleware.RequireAuth("jobs.delete"), g.DeleteJob)
        })
        
        // Additional routes...
    })
    
    // Documentation
    g.router.Get("/swagger/*", httpSwagger.Handler())
    
    // Health check
    g.router.Get("/health", g.HealthCheck)
}
```

### Authentication & Authorization

The Authentication Middleware is responsible for:

1. **Authentication Verification**
   - JWT validation
   - API key validation
   - User identity resolution

2. **Authorization Enforcement**
   - Permission checking
   - Role-based access control
   - Resource-specific permissions

gRPC Implementation:

```go
// AuthInterceptor provides authentication for gRPC services
type AuthInterceptor struct {
    jwtVerifier   auth.JWTVerifier
    permissionSvc auth.PermissionService
    logger        *zap.Logger
}

// UnaryInterceptor returns a gRPC interceptor for unary RPC
func (i *AuthInterceptor) UnaryInterceptor() grpc.UnaryServerInterceptor {
    return func(
        ctx context.Context,
        req interface{},
        info *grpc.UnaryServerInfo,
        handler grpc.UnaryHandler,
    ) (interface{}, error) {
        // Skip authentication for certain methods
        if isPublicMethod(info.FullMethod) {
            return handler(ctx, req)
        }
        
        // Extract token from metadata
        md, ok := metadata.FromIncomingContext(ctx)
        if !ok {
            return nil, status.Error(codes.Unauthenticated, "metadata not provided")
        }
        
        // Get authorization header
        authHeader, ok := md["authorization"]
        if !ok || len(authHeader) == 0 {
            return nil, status.Error(codes.Unauthenticated, "authorization header not provided")
        }
        
        // Verify token
        token := strings.TrimPrefix(authHeader[0], "Bearer ")
        claims, err := i.jwtVerifier.VerifyToken(token)
        if err != nil {
            i.logger.Warn("token verification failed", zap.Error(err))
            return nil, status.Error(codes.Unauthenticated, "invalid token")
        }
        
        // Check permissions for method
        permission := methodToPermission(info.FullMethod)
        if !i.permissionSvc.HasPermission(claims.Subject, permission) {
            i.logger.Warn("permission denied",
                zap.String("user", claims.Subject),
                zap.String("permission", permission))
            return nil, status.Error(codes.PermissionDenied, "permission denied")
        }
        
        // Add claims to context and proceed
        newCtx := context.WithValue(ctx, auth.ClaimsContextKey, claims)
        return handler(newCtx, req)
    }
}
```

HTTP Implementation:

```go
// RequireAuth middleware for HTTP requests
func (m *AuthMiddleware) RequireAuth(permission string) func(http.Handler) http.Handler {
    return func(next http.Handler) http.Handler {
        return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
            // Extract token from Authorization header
            authHeader := r.Header.Get("Authorization")
            if authHeader == "" {
                m.writeError(w, http.StatusUnauthorized, "authorization header required")
                return
            }
            
            // Verify token
            token := strings.TrimPrefix(authHeader, "Bearer ")
            claims, err := m.jwtVerifier.VerifyToken(token)
            if err != nil {
                m.logger.Warn("token verification failed", zap.Error(err))
                m.writeError(w, http.StatusUnauthorized, "invalid token")
                return
            }
            
            // Check permissions
            if !m.permissionSvc.HasPermission(claims.Subject, permission) {
                m.logger.Warn("permission denied",
                    zap.String("user", claims.Subject),
                    zap.String("permission", permission))
                m.writeError(w, http.StatusForbidden, "permission denied")
                return
            }
            
            // Add claims to request context and proceed
            ctx := context.WithValue(r.Context(), auth.ClaimsContextKey, claims)
            next.ServeHTTP(w, r.WithContext(ctx))
        })
    }
}
```

### Client SDK Generation

The API Service automatically generates client SDKs for multiple languages from the Protocol Buffer definitions:

- Go
- JavaScript/TypeScript
- Python
- Rust

SDK generation is part of the build process using the appropriate protocol buffer plugins for each language.

## Design Decisions

### gRPC with REST Gateway

- **Primary API is gRPC for efficient service-to-service communication**
  - Ensures type safety through protocol buffers
  - Provides high-performance binary serialization
  - Enables bi-directional streaming capabilities

- **REST API provided via gRPC-Gateway for broader client compatibility**
  - Automatically translates REST calls to gRPC
  - Serves both REST and gRPC on the same port
  - Provides familiar REST patterns for web/mobile clients

- **Single source of truth for API definitions via Protocol Buffers**
  - API definitions in .proto files generate both client and server code
  - Documentation generated from same source (OpenAPI)
  - Ensures consistent API surface across interfaces

### Authentication Flow

- **JWT-based authentication with configurable providers**
  - Support for multiple identity providers
  - Flexible token validation logic
  - Customizable claims extraction

- **Integration with Secrets Service for signature verification**
  - Cryptographic verification of tokens
  - Centralized key management
  - Support for key rotation

- **Scoped access tokens with fine-grained permissions**
  - Permission-based authorization
  - Support for custom claims and scopes
  - Resource-level permissions

### Response Aggregation

- **Composite endpoints that aggregate data from multiple backend services**
  - Client receives complete data in a single request
  - Reduced network round trips for frontend applications
  - Simplified client implementation

- **Parallel request execution with context-based timeouts**
  - Concurrent execution for improved performance
  - Timeouts prevent slow services from blocking responses
  - Cancellation propagation to backend services

- **Partial responses when some backend services are unavailable**
  - Degraded but functional response when part of the system is down
  - Clear indication of which components failed
  - Fallback strategies for critical operations

## Performance Considerations

- Connection pooling for backend service clients
- Response caching for frequently accessed data
- Rate limiting to prevent abuse and ensure fair usage
- Deadline propagation to prevent cascade failures
- Request batching for high-volume operations

## Monitoring and Observability

- Prometheus metrics for request rates, latencies, and errors
- Distributed tracing with OpenTelemetry
- Structured logging with correlation IDs
- Health check endpoints for service status
- Alert rules for service degradation

## Security Considerations

- All endpoints secured with authentication
- Authorization checks for every operation
- Input validation for all request parameters
- Rate limiting to prevent abuse
- TLS termination and secure communication

## Testing Strategy

1. **Unit Testing**
   - Handler tests with mocked dependencies
   - Middleware tests
   - Utility function tests

2. **Integration Testing**
   - API endpoint tests with simulated backends
   - Authentication flow tests
   - Error handling tests

3. **Load Testing**
   - Performance under high concurrency
   - Rate limiting effectiveness
   - Resource utilization

## Related Documentation

- [Architecture](./ARCHITECTURE.md) - API Service architecture overview
- [API Reference](./API_REFERENCE.md) - API specifications
- [Service Integration](../SERVICE_INTEGRATION.md) - Integration with other services

## Integration Workflows

### Client Authentication Flow

```
┌───────────────┐          ┌───────────────┐          ┌───────────────┐
│               │          │               │          │               │
│    Client     │          │  API Service  │          │  Auth Service │
│               │          │               │          │               │
└───────┬───────┘          └───────┬───────┘          └───────┬───────┘
        │                          │                          │
        │  1. Authentication       │                          │
        │  Request with            │                          │
        │  Signature               │                          │
        │─────────────────────────▶│                          │
        │                          │                          │
        │                          │  2. Verify               │
        │                          │  Signature               │
        │                          │─────────────────────────▶│
        │                          │                          │
        │                          │  3. Verify Result        │
        │                          │◀─────────────────────────│
        │                          │                          │
        │  4. JWT Token            │                          │
        │◀─────────────────────────│                          │
        │                          │                          │
        │  5. API Request          │                          │
        │  with JWT                │                          │
        │─────────────────────────▶│                          │
        │                          │                          │
        │  6. Validate Token       │                          │
        │  & Check Permissions     │                          │
        │                          │                          │
        │  7. API Response         │                          │
        │◀─────────────────────────│                          │
        │                          │                          │
┌───────┴───────┐          ┌───────┴───────┐          ┌───────┴───────┐
│               │          │               │          │               │
│    Client     │          │  API Service  │          │  Auth Service │
│               │          │               │          │               │
└───────────────┘          └───────────────┘          └───────────────┘
```

### Service Integration Flow

```
┌───────────────┐          ┌───────────────┐          ┌───────────────┐
│               │          │               │          │               │
│    Client     │          │  API Service  │          │  Backend      │
│  Application  │          │  (Gateway)    │          │  Services     │
│               │          │               │          │               │
└───────┬───────┘          └───────┬───────┘          └───────┬───────┘
        │                          │                          │
        │  1. API Request          │                          │
        │─────────────────────────▶│                          │
        │                          │                          │
        │                          │  2. Authentication &     │
        │                          │  Authorization Check     │
        │                          │                          │
        │                          │  3. Request Routing      │
        │                          │                          │
        │                          │  4. Protocol             │
        │                          │  Translation             │
        │                          │  (REST → gRPC)           │
        │                          │                          │
        │                          │  5. Backend Service      │
        │                          │  Request                 │
        │                          │─────────────────────────▶│
        │                          │                          │
        │                          │  6. Process Request      │
        │                          │                          │
        │                          │  7. Service Response     │
        │                          │◀─────────────────────────│
        │                          │                          │
        │                          │  8. Response             │
        │                          │  Translation             │
        │                          │  (gRPC → REST)           │
        │                          │                          │
        │  9. API Response         │                          │
        │◀─────────────────────────│                          │
        │                          │                          │
┌───────┴───────┐          ┌───────┴───────┐          ┌───────┴───────┐
│               │          │               │          │               │
│    Client     │          │  API Service  │          │  Backend      │
│  Application  │          │  (Gateway)    │          │  Services     │
│               │          │               │          │               │
└───────────────┘          └───────────────┘          └───────────────┘
```

## Service Integration Examples

### Function Service Integration

The API Service acts as a facade for the Function Service, handling protocol translation and request routing:

```go
// FunctionHandler handles Function Service API requests
type FunctionHandler struct {
    client         functionservice.Client
    logger         *zap.Logger
    responseMapper ResponseMapper
}

// RegisterFunction handles function registration
func (h *FunctionHandler) RegisterFunction(w http.ResponseWriter, r *http.Request) {
    ctx := r.Context()
    
    // Parse and validate request
    var req RegisterFunctionRequest
    if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
        h.responseMapper.WriteErrorResponse(w, http.StatusBadRequest, "Invalid request body")
        return
    }
    
    // Validate request fields
    if err := h.validateRegisterRequest(req); err != nil {
        h.responseMapper.WriteErrorResponse(w, http.StatusBadRequest, err.Error())
        return
    }
    
    // Get user from context
    userClaims, ok := ctx.Value(auth.ClaimsContextKey).(*auth.Claims)
    if !ok {
        h.responseMapper.WriteErrorResponse(w, http.StatusUnauthorized, "User not authenticated")
        return
    }
    
    // Map to gRPC request
    grpcReq := &functionservice.RegisterFunctionRequest{
        Name:        req.Name,
        Description: req.Description,
        SourceCode:  req.SourceCode,
        Runtime:     req.Runtime,
        Owner:       userClaims.Subject,
        Permissions: mapPermissions(req.Permissions),
    }
    
    // Call Function Service
    grpcResp, err := h.client.RegisterFunction(ctx, grpcReq)
    if err != nil {
        h.logger.Error("Failed to register function", zap.Error(err))
        
        // Map gRPC error to HTTP response
        status := h.mapGRPCErrorToHTTPStatus(err)
        h.responseMapper.WriteErrorResponse(w, status, "Failed to register function")
        return
    }
    
    // Map gRPC response to REST response
    resp := RegisterFunctionResponse{
        ID:          grpcResp.Id,
        Name:        grpcResp.Name,
        Description: grpcResp.Description,
        Runtime:     grpcResp.Runtime,
        Owner:       grpcResp.Owner,
        CreatedAt:   timestamppb.New(grpcResp.CreatedAt.AsTime()),
        Status:      mapFunctionStatus(grpcResp.Status),
    }
    
    h.responseMapper.WriteJSONResponse(w, http.StatusCreated, resp)
}
```

### Automation Service Integration

The API Service provides access to the Automation Service with consistent response formats:

```go
// AutomationHandler handles Automation Service API requests
type AutomationHandler struct {
    client         automationservice.Client
    logger         *zap.Logger
    responseMapper ResponseMapper
}

// CreateJob handles job creation
func (h *AutomationHandler) CreateJob(w http.ResponseWriter, r *http.Request) {
    ctx := r.Context()
    
    // Parse and validate request
    var req CreateJobRequest
    if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
        h.responseMapper.WriteErrorResponse(w, http.StatusBadRequest, "Invalid request body")
        return
    }
    
    // Validate request fields
    if err := h.validateCreateJobRequest(req); err != nil {
        h.responseMapper.WriteErrorResponse(w, http.StatusBadRequest, err.Error())
        return
    }
    
    // Get user from context
    userClaims, ok := ctx.Value(auth.ClaimsContextKey).(*auth.Claims)
    if !ok {
        h.responseMapper.WriteErrorResponse(w, http.StatusUnauthorized, "User not authenticated")
        return
    }
    
    // Map to gRPC request
    grpcReq := &automationservice.CreateJobRequest{
        Name:        req.Name,
        Description: req.Description,
        Trigger:     mapTrigger(req.Trigger),
        Action:      mapAction(req.Action),
        Owner:       userClaims.Subject,
    }
    
    // Call Automation Service
    grpcResp, err := h.client.CreateJob(ctx, grpcReq)
    if err != nil {
        h.logger.Error("Failed to create job", zap.Error(err))
        
        // Map gRPC error to HTTP response
        status := h.mapGRPCErrorToHTTPStatus(err)
        h.responseMapper.WriteErrorResponse(w, status, "Failed to create job")
        return
    }
    
    // Map gRPC response to REST response
    resp := CreateJobResponse{
        ID:          grpcResp.Id,
        Name:        grpcResp.Name,
        Description: grpcResp.Description,
        Status:      mapJobStatus(grpcResp.Status),
        CreatedAt:   timestamppb.New(grpcResp.CreatedAt.AsTime()),
    }
    
    h.responseMapper.WriteJSONResponse(w, http.StatusCreated, resp)
}
```

## Caching Strategy

The API Service implements a multi-level caching strategy to improve performance:

```go
// CachingMiddleware provides caching for HTTP responses
type CachingMiddleware struct {
    cache  cache.Cache
    logger *zap.Logger
}

// CacheResponse caches responses for GET requests
func (m *CachingMiddleware) CacheResponse(ttl time.Duration) func(http.Handler) http.Handler {
    return func(next http.Handler) http.Handler {
        return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
            // Only cache GET requests
            if r.Method != http.MethodGet {
                next.ServeHTTP(w, r)
                return
            }
            
            // Generate cache key
            key := m.generateCacheKey(r)
            
            // Try to get from cache
            cachedResponse, found, err := m.cache.Get(r.Context(), key)
            if err != nil {
                m.logger.Warn("cache error", zap.Error(err))
                next.ServeHTTP(w, r)
                return
            }
            
            if found {
                m.logger.Debug("cache hit", zap.String("key", key))
                resp := cachedResponse.(*CachedResponse)
                
                // Set headers
                for k, v := range resp.Headers {
                    w.Header().Set(k, v)
                }
                
                // Set cache header
                w.Header().Set("X-Cache", "HIT")
                
                // Write status and body
                w.WriteHeader(resp.StatusCode)
                w.Write(resp.Body)
                return
            }
            
            // Cache miss, capture response
            recorder := newResponseRecorder(w)
            next.ServeHTTP(recorder, r)
            
            // Don't cache errors
            if recorder.statusCode >= 400 {
                return
            }
            
            // Store in cache
            cachedResp := &CachedResponse{
                StatusCode: recorder.statusCode,
                Body:       recorder.body.Bytes(),
                Headers:    recorder.Header().Clone(),
            }
            
            if err := m.cache.Set(r.Context(), key, cachedResp, ttl); err != nil {
                m.logger.Warn("failed to cache response", zap.Error(err))
            }
        })
    }
}

// generateCacheKey creates a unique cache key for a request
func (m *CachingMiddleware) generateCacheKey(r *http.Request) string {
    // Get user ID from context for per-user caching
    userID := "anonymous"
    if claims, ok := r.Context().Value(auth.ClaimsContextKey).(*auth.Claims); ok {
        userID = claims.Subject
    }
    
    // Create key from method, path, query params, and user ID
    h := sha256.New()
    io.WriteString(h, r.Method)
    io.WriteString(h, r.URL.Path)
    io.WriteString(h, r.URL.RawQuery)
    io.WriteString(h, userID)
    
    return hex.EncodeToString(h.Sum(nil))
}
```

## Rate Limiting Implementation

The API Service implements rate limiting to protect backend services:

```go
// RateLimiterMiddleware provides rate limiting
type RateLimiterMiddleware struct {
    limiter RateLimiter
    logger  *zap.Logger
}

// LimitByIP limits requests by IP address
func (m *RateLimiterMiddleware) LimitByIP(rps int, burst int) func(http.Handler) http.Handler {
    return func(next http.Handler) http.Handler {
        return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
            // Get client IP
            ip := r.RemoteAddr
            if forwardedFor := r.Header.Get("X-Forwarded-For"); forwardedFor != "" {
                ips := strings.Split(forwardedFor, ",")
                ip = strings.TrimSpace(ips[0])
            }
            
            // Create rate limiter key
            key := fmt.Sprintf("ratelimit:ip:%s", ip)
            
            // Check rate limit
            limited, remaining, resetAfter, err := m.limiter.Allow(r.Context(), key, rps, burst)
            if err != nil {
                m.logger.Error("rate limiter error", zap.Error(err))
                // Continue on error, don't block requests
                next.ServeHTTP(w, r)
                return
            }
            
            // Set rate limit headers
            w.Header().Set("X-RateLimit-Limit", strconv.Itoa(rps))
            w.Header().Set("X-RateLimit-Remaining", strconv.Itoa(remaining))
            w.Header().Set("X-RateLimit-Reset", strconv.Itoa(int(resetAfter.Seconds())))
            
            if limited {
                w.Header().Set("Retry-After", strconv.Itoa(int(resetAfter.Seconds())))
                http.Error(w, "Rate limit exceeded", http.StatusTooManyRequests)
                return
            }
            
            next.ServeHTTP(w, r)
        })
    }
}
```

## Deployment Architecture

The API Service should be deployed with redundancy and high availability:

```
┌─────────────────────────────────────────────────────────────────────┐
│                           Load Balancer                             │
└───────────────┬─────────────────────────────────┬───────────────────┘
                │                                 │
                ▼                                 ▼
┌───────────────────────────┐       ┌───────────────────────────┐
│  API Service Instance 1   │       │  API Service Instance 2   │
└───────────────┬───────────┘       └───────────────┬───────────┘
                │                                   │
                └───────────────┬───────────────────┘
                                │
                                ▼
                     ┌────────────────────┐
                     │                    │
                     │   Redis Cluster    │  ◄── Caching & Rate Limiting
                     │                    │
                     └────────────────────┘
                                │
                                │
           ┌───────────────────┼────────────────────┐
           │                   │                    │
           ▼                   ▼                    ▼
┌────────────────────┐ ┌─────────────────┐ ┌─────────────────────┐
│                    │ │                 │ │                     │
│ Function Service   │ │ Automation      │ │ Other Backend       │
│                    │ │ Service         │ │ Services            │
└────────────────────┘ └─────────────────┘ └─────────────────────┘
```

### Resource Requirements

For a production deployment, the following minimum resources are recommended:

| Component          | CPU    | Memory | Disk  | Notes                                  |
|--------------------|--------|--------|-------|----------------------------------------|
| API Service        | 2 CPU  | 4 GB   | 20 GB | Min 2 instances for high availability  |
| Redis Cluster      | 2 CPU  | 8 GB   | 30 GB | For caching and rate limiting          |
| Load Balancer      | 2 CPU  | 4 GB   | N/A   | Managed service recommended            |

### Environment Variables

```
# Server Configuration
HTTP_PORT=8080
GRPC_PORT=50051
SHUTDOWN_TIMEOUT=10s
MAX_REQUEST_SIZE=10MB

# Security
JWT_PUBLIC_KEY_PATH=/keys/jwt-public.pem
TLS_CERT_PATH=/certs/server.crt
TLS_KEY_PATH=/certs/server.key
TLS_CA_PATH=/certs/ca.crt

# Rate Limiting
RATE_LIMIT_RPS=100
RATE_LIMIT_BURST=200
REDIS_RATE_LIMIT_URL=redis://redis:6379/0

# Caching
CACHE_TTL=5m
REDIS_CACHE_URL=redis://redis:6379/1

# Service Integration
FUNCTION_SERVICE_ENDPOINT=function-service:50051
AUTOMATION_SERVICE_ENDPOINT=automation-service:50051
SECRETS_SERVICE_ENDPOINT=secrets-service:50051
GASBANK_SERVICE_ENDPOINT=gasbank-service:50051
PRICEFEED_SERVICE_ENDPOINT=pricefeed-service:50051

# Observability
METRICS_PORT=9090
JAEGER_ENDPOINT=http://jaeger:14268/api/traces
LOG_LEVEL=info
```

### Kubernetes Deployment Example

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: api-service
  namespace: neo-service-layer
spec:
  replicas: 2
  selector:
    matchLabels:
      app: api-service
  template:
    metadata:
      labels:
        app: api-service
    spec:
      containers:
      - name: api-service
        image: neo-service-layer/api-service:latest
        ports:
        - containerPort: 8080
          name: http
        - containerPort: 50051
          name: grpc
        - containerPort: 9090
          name: metrics
        env:
        - name: HTTP_PORT
          value: "8080"
        - name: GRPC_PORT
          value: "50051"
        - name: FUNCTION_SERVICE_ENDPOINT
          value: "function-service:50051"
        - name: AUTOMATION_SERVICE_ENDPOINT
          value: "automation-service:50051"
        - name: REDIS_CACHE_URL
          valueFrom:
            secretKeyRef:
              name: redis-credentials
              key: url
        resources:
          requests:
            cpu: "1"
            memory: "2Gi"
          limits:
            cpu: "2"
            memory: "4Gi"
        volumeMounts:
        - name: certs
          mountPath: /certs
          readOnly: true
        - name: keys
          mountPath: /keys
          readOnly: true
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
      volumes:
      - name: certs
        secret:
          secretName: api-service-certs
      - name: keys
        secret:
          secretName: api-service-keys
---
apiVersion: v1
kind: Service
metadata:
  name: api-service
  namespace: neo-service-layer
spec:
  selector:
    app: api-service
  ports:
  - port: 80
    name: http
    targetPort: 8080
  - port: 50051
    name: grpc
    targetPort: 50051
  type: ClusterIP
---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: api-service-ingress
  namespace: neo-service-layer
  annotations:
    kubernetes.io/ingress.class: "nginx"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
spec:
  tls:
  - hosts:
    - api.neoservice.example.com
    secretName: api-tls-cert
  rules:
  - host: api.neoservice.example.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: api-service
            port:
              name: http
```

## Security Considerations

### TLS Configuration

The API Service uses the following TLS configuration for secure communication:

```go
// ConfigureTLS sets up TLS for the HTTP server
func ConfigureTLS(config *Config) (*tls.Config, error) {
    // Load certificates
    cert, err := tls.LoadX509KeyPair(config.TLSCertPath, config.TLSKeyPath)
    if err != nil {
        return nil, fmt.Errorf("failed to load TLS certificate: %w", err)
    }
    
    // Load CA certificate for mTLS (if configured)
    var caCertPool *x509.CertPool
    if config.TLSCAPath != "" {
        caCertPool = x509.NewCertPool()
        caCert, err := os.ReadFile(config.TLSCAPath)
        if err != nil {
            return nil, fmt.Errorf("failed to load CA certificate: %w", err)
        }
        
        if !caCertPool.AppendCertsFromPEM(caCert) {
            return nil, fmt.Errorf("failed to append CA certificate")
        }
    }
    
    return &tls.Config{
        Certificates: []tls.Certificate{cert},
        ClientCAs:    caCertPool,
        ClientAuth:   tls.RequireAndVerifyClientCert,
        MinVersion:   tls.VersionTLS13,  // TLS 1.3 minimum
        CipherSuites: []uint16{
            tls.TLS_AES_128_GCM_SHA256,
            tls.TLS_AES_256_GCM_SHA384,
            tls.TLS_CHACHA20_POLY1305_SHA256,
        },
    }, nil
}
```

### JWT Security

The API Service uses public key cryptography for JWT validation:

```go
// JWTVerifier validates JWTs with RSA public key
type JWTVerifier struct {
    publicKey *rsa.PublicKey
    logger    *zap.Logger
}

// NewJWTVerifier creates a new JWT verifier
func NewJWTVerifier(publicKeyPath string, logger *zap.Logger) (*JWTVerifier, error) {
    // Load public key
    publicKeyBytes, err := os.ReadFile(publicKeyPath)
    if err != nil {
        return nil, fmt.Errorf("failed to read public key: %w", err)
    }
    
    // Parse public key
    block, _ := pem.Decode(publicKeyBytes)
    if block == nil || block.Type != "PUBLIC KEY" {
        return nil, fmt.Errorf("failed to decode PEM block containing public key")
    }
    
    publicKey, err := x509.ParsePKIXPublicKey(block.Bytes)
    if err != nil {
        return nil, fmt.Errorf("failed to parse public key: %w", err)
    }
    
    rsaPublicKey, ok := publicKey.(*rsa.PublicKey)
    if !ok {
        return nil, fmt.Errorf("not an RSA public key")
    }
    
    return &JWTVerifier{
        publicKey: rsaPublicKey,
        logger:    logger,
    }, nil
}

// VerifyToken validates a JWT token and returns its claims
func (v *JWTVerifier) VerifyToken(tokenString string) (*Claims, error) {
    token, err := jwt.ParseWithClaims(tokenString, &Claims{}, func(token *jwt.Token) (interface{}, error) {
        // Validate signing method
        if _, ok := token.Method.(*jwt.SigningMethodRSA); !ok {
            return nil, fmt.Errorf("unexpected signing method: %v", token.Header["alg"])
        }
        
        return v.publicKey, nil
    })
    
    if err != nil {
        return nil, fmt.Errorf("invalid token: %w", err)
    }
    
    claims, ok := token.Claims.(*Claims)
    if !ok || !token.Valid {
        return nil, fmt.Errorf("invalid token claims")
    }
    
    // Check if token is expired
    if claims.ExpiresAt.Before(time.Now()) {
        return nil, fmt.Errorf("token expired")
    }
    
    return claims, nil
}
```
