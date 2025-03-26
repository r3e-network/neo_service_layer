# TEE Attestation Service

This package provides a robust implementation of TEE (Trusted Execution Environment) attestation, with a focus on Intel SGX attestation. It includes support for quote verification through the Intel Attestation Service (IAS), certificate chain validation, caching, rate limiting, metrics collection, and structured logging.

## Features

### Attestation Service
- Verification of attestation evidence for Intel SGX enclaves
- Support for different quote formats and signature types
- Configurable security policies and validation rules
- Comprehensive error handling and logging

### IAS Client
- Integration with Intel Attestation Service (IAS) v4 API
- Quote verification with signature validation
- SigRL (Signature Revocation List) retrieval
- Certificate chain validation
- Response caching and rate limiting
- Metrics collection for monitoring
- Structured logging with logrus

### Certificate Chain Validation
- Validation of PCK certificates
- CRL (Certificate Revocation List) checking
- TCB info validation
- QE (Quoting Enclave) identity verification
- Root CA certificate validation

### Caching and Rate Limiting
- In-memory caching for quote verification results
- Caching for SigRL responses
- Configurable cache expiration times
- Rate limiting for quote verification
- Rate limiting for IAS API requests
- Burst control for rate limiters

### Metrics Collection
- Prometheus metrics for monitoring
- Attestation verification counts and durations
- Quote verification statistics
- IAS API request tracking
- Error counts by type
- Latency histograms

### Logging
- Structured JSON logging with logrus
- Detailed error reporting
- Request/response logging
- Performance metrics
- Security-relevant events

## Configuration

### Environment Variables
- \`IAS_API_KEY\`: Intel Attestation Service API key (required)
- \`IAS_SPID\`: Service Provider ID for Intel SGX (required)
- \`LOG_LEVEL\`: Logging level (default: "info")

### Cache Configuration
\`\`\`go
type CacheConfig struct {
    QuoteCacheExpiration         time.Duration
    SigRLCacheExpiration        time.Duration
    QuoteVerificationRateLimit  float64 // per minute
    IASRequestRateLimit        float64 // per minute
    QuoteVerificationBurst     int
    IASRequestBurst           int
}
\`\`\`

Default values:
- Quote cache expiration: 24 hours
- SigRL cache expiration: 24 hours
- Quote verification rate limit: 100 per minute
- IAS request rate limit: 50 per minute
- Quote verification burst: 10
- IAS request burst: 5

## Usage Examples

### Basic Attestation
\`\`\`go
// Create a new attestation service
attestation := tee.NewAttestation()

// Create a security policy
policy := &tee.SecurityPolicy{
    AllowDebugEnclaves: false,
    RequireEncryption:  true,
    MinTCBLevel:       5,
}

// Verify attestation evidence
isValid, err := attestation.VerifyEvidence(evidence, policy)
if err != nil {
    log.WithError(err).Error("Failed to verify evidence")
    return
}
\`\`\`

### IAS Client Usage
\`\`\`go
// Create a new IAS client
iasClient := tee.NewIASClient(os.Getenv("IAS_API_KEY"))

// Verify a quote
resp, err := iasClient.VerifyQuote(quoteBytes)
if err != nil {
    log.WithError(err).Error("Failed to verify quote")
    return
}

// Check quote status
if resp.ISVEnclaveQuoteStatus != tee.QuoteStatusOK {
    log.WithField("status", resp.ISVEnclaveQuoteStatus).Error("Quote verification failed")
    return
}
\`\`\`

## Metrics

### Attestation Metrics
- \`attestation_total\`: Total number of attestation verifications (labels: type, platform, result)
- \`attestation_duration_seconds\`: Duration of attestation verifications (labels: type, platform)
- \`attestation_errors_total\`: Total number of attestation errors (labels: type, error)

### Quote Verification Metrics
- \`quote_verification_total\`: Total number of quote verifications (labels: type, result)
- \`quote_verification_duration_seconds\`: Duration of quote verifications (labels: type)

### IAS Request Metrics
- \`ias_request_total\`: Total number of IAS API requests (labels: endpoint, status)
- \`ias_request_duration_seconds\`: Duration of IAS API requests (labels: endpoint)

## Security Considerations

1. API Key Protection
   - Store the IAS API key securely
   - Use environment variables or secure key management
   - Never log or expose the API key

2. Certificate Management
   - Keep Intel's root CA certificate up to date
   - Validate certificate chains thoroughly
   - Check CRLs regularly

3. Quote Verification
   - Validate quote freshness
   - Check TCB levels
   - Verify enclave measurements
   - Handle debug enclaves appropriately

4. Error Handling
   - Log security-relevant errors
   - Don't expose sensitive information in errors
   - Implement proper error recovery

## Production Checklist

1. Configuration
   - [ ] Set appropriate rate limits
   - [ ] Configure cache expiration times
   - [ ] Set logging levels
   - [ ] Configure security policies

2. Monitoring
   - [ ] Set up metrics collection
   - [ ] Create monitoring dashboards
   - [ ] Configure alerts
   - [ ] Monitor error rates

3. Security
   - [ ] Secure API key storage
   - [ ] Regular certificate updates
   - [ ] CRL checking
   - [ ] TCB updates

4. Performance
   - [ ] Cache tuning
   - [ ] Rate limit optimization
   - [ ] Connection pooling
   - [ ] Request batching

## Dependencies

- \`github.com/sirupsen/logrus\`: Structured logging
- \`github.com/prometheus/client_golang\`: Metrics collection
- \`golang.org/x/time/rate\`: Rate limiting
- Standard library packages for crypto, HTTP, etc.

## Future Improvements

1. Performance
   - [ ] Implement connection pooling for IAS requests
   - [ ] Add request batching
   - [ ] Optimize cache memory usage
   - [ ] Add distributed caching support

2. Security
   - [ ] Add quote freshness verification
   - [ ] Implement TCB recovery handling
   - [ ] Add support for custom root CAs
   - [ ] Enhance certificate chain validation

3. Monitoring
   - [ ] Add more detailed metrics
   - [ ] Create default alerting rules
   - [ ] Add health checks
   - [ ] Implement tracing

4. Integration
   - [ ] Add support for other TEE types
   - [ ] Implement gRPC interface
   - [ ] Add Kubernetes integration
   - [ ] Create client libraries 