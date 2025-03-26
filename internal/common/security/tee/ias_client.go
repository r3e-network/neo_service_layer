package tee

import (
	"bytes"
	"context"
	"crypto"
	"crypto/rsa"
	"crypto/sha256"
	"crypto/x509"
	"encoding/base64"
	"encoding/json"
	"encoding/pem"
	"errors"
	"fmt"
	"io"
	"net/http"
	"time"

	"github.com/sirupsen/logrus"
)

var (
	// ErrIASRequestFailed indicates an IAS API request failed
	ErrIASRequestFailed = errors.New("IAS request failed")
	// ErrIASSignatureInvalid indicates an invalid IAS response signature
	ErrIASSignatureInvalid = errors.New("invalid IAS signature")
	// ErrIASCertificateInvalid indicates an invalid IAS certificate
	ErrIASCertificateInvalid = errors.New("invalid IAS certificate")
)

const (
	// IAS API endpoints
	iasBaseURL         = "https://api.trustedservices.intel.com/sgx/dev"
	iasReportEndpoint  = "/attestation/v4/report"
	iasSigrlEndpoint   = "/attestation/v4/sigrl"
	iasRootCACRLPath   = "/rootcacrl"
	iasTCBInfoEndpoint = "/tcb"
	iasQEIdentityPath  = "/qe/identity"

	// HTTP headers
	iasSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key"
	iasReportSignatureHeader = "X-IASReport-Signature"
	iasReportCertHeader      = "X-IASReport-Signing-Certificate"
)

// IASClient represents an Intel Attestation Service client
type IASClient struct {
	apiKey     string
	httpClient *http.Client
	baseURL    string
	log        *logrus.Logger
	cache      *Cache
}

// NewIASClient creates a new IAS client
func NewIASClient(apiKey string) *IASClient {
	logger := logrus.New()
	logger.SetFormatter(&logrus.JSONFormatter{})
	logger.SetLevel(logrus.InfoLevel)

	cacheConfig := &CacheConfig{
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

	rateConfig := &RateLimitConfig{
		QuoteVerifyRate:  100.0 / 60.0, // convert to per second
		QuoteVerifyBurst: 10,
		IASRequestRate:   50.0 / 60.0, // convert to per second
		IASRequestBurst:  5,
	}

	cache, err := NewCache(cacheConfig, rateConfig, logger)
	if err != nil {
		logger.WithError(err).Error("Failed to create cache, proceeding without caching")
	}

	return &IASClient{
		apiKey: apiKey,
		httpClient: &http.Client{
			Timeout: 30 * time.Second,
		},
		baseURL: iasBaseURL,
		log:     logger,
		cache:   cache,
	}
}

// VerifyQuote sends a quote to IAS for verification
func (c *IASClient) VerifyQuote(quoteBytes []byte) (*IASResponse, error) {
	start := time.Now()
	defer func() {
		duration := time.Since(start).Seconds()
		quoteVerificationDuration.WithLabelValues("sgx").Observe(duration)
	}()

	c.log.WithField("quoteSize", len(quoteBytes)).Info("Verifying quote with IAS")

	// Check cache first
	if c.cache != nil {
		if resp, ok := c.cache.GetCachedQuote(quoteBytes); ok {
			c.log.Info("Using cached quote verification result")
			quoteVerificationTotal.WithLabelValues("sgx", "cache_hit").Inc()
			return resp, nil
		}
	}

	// Apply rate limiting
	if c.cache != nil {
		ctx := context.Background()
		if err := c.cache.WaitQuoteVerification(ctx); err != nil {
			c.log.WithError(err).Error("Quote verification rate limit exceeded")
			quoteVerificationTotal.WithLabelValues("sgx", "rate_limited").Inc()
			return nil, fmt.Errorf("quote verification rate limit exceeded: %w", err)
		}
	}

	// Encode quote as base64
	quoteBase64 := base64.StdEncoding.EncodeToString(quoteBytes)

	// Create request body
	body := map[string]string{
		"isvEnclaveQuote": quoteBase64,
	}

	bodyBytes, err := json.Marshal(body)
	if err != nil {
		c.log.WithError(err).Error("Failed to marshal request body")
		quoteVerificationTotal.WithLabelValues("sgx", "error").Inc()
		return nil, fmt.Errorf("failed to marshal request body: %w", err)
	}

	// Create request
	req, err := http.NewRequest(http.MethodPost, c.baseURL+iasReportEndpoint, bytes.NewReader(bodyBytes))
	if err != nil {
		c.log.WithError(err).Error("Failed to create request")
		quoteVerificationTotal.WithLabelValues("sgx", "error").Inc()
		return nil, fmt.Errorf("failed to create request: %w", err)
	}

	// Add headers
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set(iasSubscriptionKeyHeader, c.apiKey)

	// Apply IAS request rate limiting
	if c.cache != nil {
		ctx := context.Background()
		if err := c.cache.WaitIASRequest(ctx); err != nil {
			c.log.WithError(err).Error("IAS request rate limit exceeded")
			iasRequestTotal.WithLabelValues("report", "rate_limited").Inc()
			return nil, fmt.Errorf("IAS request rate limit exceeded: %w", err)
		}
	}

	// Send request
	requestStart := time.Now()
	resp, err := c.httpClient.Do(req)
	requestDuration := time.Since(requestStart).Seconds()
	iasRequestDuration.WithLabelValues("report").Observe(requestDuration)

	if err != nil {
		c.log.WithError(err).Error("Failed to send request")
		iasRequestTotal.WithLabelValues("report", "error").Inc()
		quoteVerificationTotal.WithLabelValues("sgx", "error").Inc()
		return nil, fmt.Errorf("failed to send request: %w", err)
	}
	defer resp.Body.Close()

	// Read response body
	respBody, err := io.ReadAll(resp.Body)
	if err != nil {
		c.log.WithError(err).Error("Failed to read response body")
		iasRequestTotal.WithLabelValues("report", "error").Inc()
		quoteVerificationTotal.WithLabelValues("sgx", "error").Inc()
		return nil, fmt.Errorf("failed to read response body: %w", err)
	}

	// Check response status
	if resp.StatusCode != http.StatusOK {
		c.log.WithFields(logrus.Fields{
			"status": resp.StatusCode,
			"body":   string(respBody),
		}).Error("IAS request failed")
		iasRequestTotal.WithLabelValues("report", fmt.Sprintf("%d", resp.StatusCode)).Inc()
		quoteVerificationTotal.WithLabelValues("sgx", "error").Inc()
		return nil, fmt.Errorf("%w: status %d: %s", ErrIASRequestFailed, resp.StatusCode, string(respBody))
	}
	iasRequestTotal.WithLabelValues("report", "200").Inc()

	// Get IAS signature and certificate
	signature := resp.Header.Get(iasReportSignatureHeader)
	certificate := resp.Header.Get(iasReportCertHeader)

	if signature == "" || certificate == "" {
		c.log.Error("Missing IAS signature or certificate")
		quoteVerificationTotal.WithLabelValues("sgx", "error").Inc()
		return nil, fmt.Errorf("%w: missing signature or certificate", ErrIASSignatureInvalid)
	}

	// Verify IAS signature
	if err := c.verifyIASSignature(respBody, signature, certificate); err != nil {
		c.log.WithError(err).Error("Failed to verify IAS signature")
		quoteVerificationTotal.WithLabelValues("sgx", "invalid_signature").Inc()
		return nil, fmt.Errorf("failed to verify IAS signature: %w", err)
	}

	// Parse response
	var iasResp IASResponse
	if err := json.Unmarshal(respBody, &iasResp); err != nil {
		c.log.WithError(err).Error("Failed to parse IAS response")
		quoteVerificationTotal.WithLabelValues("sgx", "error").Inc()
		return nil, fmt.Errorf("failed to parse IAS response: %w", err)
	}

	// Cache successful response
	if c.cache != nil {
		c.cache.CacheQuote(quoteBytes, &iasResp)
	}

	c.log.WithFields(logrus.Fields{
		"id":      iasResp.ID,
		"status":  iasResp.ISVEnclaveQuoteStatus,
		"version": iasResp.Version,
	}).Info("Successfully verified quote with IAS")

	quoteVerificationTotal.WithLabelValues("sgx", "success").Inc()
	return &iasResp, nil
}

// verifyIASSignature verifies the signature on an IAS response
func (c *IASClient) verifyIASSignature(body []byte, signature string, certStr string) error {
	// Decode signature
	sigBytes, err := base64.StdEncoding.DecodeString(signature)
	if err != nil {
		return fmt.Errorf("%w: failed to decode signature: %v", ErrIASSignatureInvalid, err)
	}

	// Decode and parse certificate
	certBytes, err := base64.StdEncoding.DecodeString(certStr)
	if err != nil {
		return fmt.Errorf("%w: failed to decode certificate: %v", ErrIASCertificateInvalid, err)
	}

	cert, err := x509.ParseCertificate(certBytes)
	if err != nil {
		return fmt.Errorf("%w: failed to parse certificate: %v", ErrIASCertificateInvalid, err)
	}

	// Load Intel's root CA certificate
	rootCACert, err := c.loadIntelRootCACert()
	if err != nil {
		return fmt.Errorf("failed to load Intel root CA: %w", err)
	}

	// Create certificate pool with Intel's root CA
	roots := x509.NewCertPool()
	roots.AddCert(rootCACert)

	// Create certificate chain verification options
	opts := x509.VerifyOptions{
		Roots:         roots,
		CurrentTime:   time.Now(),
		Intermediates: x509.NewCertPool(),
	}

	// Verify certificate chain
	chains, err := cert.Verify(opts)
	if err != nil {
		return fmt.Errorf("%w: chain verification failed: %v", ErrIASCertificateInvalid, err)
	}

	// Check that at least one valid chain exists
	if len(chains) == 0 {
		return fmt.Errorf("%w: no valid chain found", ErrIASCertificateInvalid)
	}

	// Verify signature using certificate's public key
	hash := sha256.Sum256(body)
	err = rsa.VerifyPKCS1v15(cert.PublicKey.(*rsa.PublicKey), crypto.SHA256, hash[:], sigBytes)
	if err != nil {
		return fmt.Errorf("%w: verification failed: %v", ErrIASSignatureInvalid, err)
	}

	return nil
}

// loadIntelRootCACert loads Intel's root CA certificate
func (c *IASClient) loadIntelRootCACert() (*x509.Certificate, error) {
	// In production, this would load Intel's root CA certificate from a secure location
	// For now, we'll use a placeholder
	// TODO: Replace with actual Intel root CA certificate
	certPEM := []byte(`-----BEGIN CERTIFICATE-----
MIIFSzCCA7OgAwIBAgIJANEHdl0yo7CUMA0GCSqGSIb3DQEBCwUAMH4xCzAJBgNV
BAYTAlVTMQswCQYDVQQIDAJDQTEUMBIGA1UEBwwLU2FudGEgQ2xhcmExGjAYBgNV
BAoMEUludGVsIENvcnBvcmF0aW9uMTAwLgYDVQQDDCdJbnRlbCBTR1ggQXR0ZXN0
YXRpb24gUmVwb3J0IFNpZ25pbmcgQ0EwIBcNMTYxMTE0MTUzNzMxWhgPMjA0OTEy
MzEyMzU5NTlaMH4xCzAJBgNVBAYTAlVTMQswCQYDVQQIDAJDQTEUMBIGA1UEBwwL
U2FudGEgQ2xhcmExGjAYBgNVBAoMEUludGVsIENvcnBvcmF0aW9uMTAwLgYDVQQD
DCdJbnRlbCBTR1ggQXR0ZXN0YXRpb24gUmVwb3J0IFNpZ25pbmcgQ0EwggGiMA0G
CSqGSIb3DQEBAQUAA4IBjwAwggGKAoIBgQCfPGR+tXc8u1EtJzLA10Feu1Wg+p7e
LmSRmeaCHbkQ1TF3Nwl3RmpqXkeGzNLd69QUnWovYyVSndEMyYc3sHecGgfinEeh
rgBJSEdsSJ9FpaFdesjsxqzGRa20PYdnnfWcCTvFoulpbFR4VBuXnnVLVzkUvlXT
L/TAnd8nIZk0zZkFJ7P5LtePvykkar7LcSQO85wtcQe0R1Raf/sQ6wYKaKmFgCGe
NpEJUmg4ktal4qgIAxk+QHUxQE42sxViN5mqglB0QJdUot/o9a/V/mMeH8KvOAiQ
byinkNndn+Bgk5sSV5DFgF0DffVqmVMblt5p3jPtImzBIH0QQrXJq39AT8cRwP5H
afuVeLHcDsRp6hol4P+ZFIhu8mmbI1u0hH3W/0C2BuYXB5PC+5izFFh/nP0lc2Lf
6rELO9LZdnOhpL1ExFOq9H/B8tPQ84T3Sgb4nAifDabNt/zu6MmCGo5U8lwEFtGM
RoOaX4AS+909x00lYnmtwsDVWv9vBiJCXRsCAwEAAaOByTCBxjBgBgNVHR8EWTBX
MFWgU6BRhk9odHRwOi8vdHJ1c3RlZHNlcnZpY2VzLmludGVsLmNvbS9jb250ZW50
L0NSTC9TR1gvQXR0ZXN0YXRpb25SZXBvcnRTaWduaW5nQ0EuY3JsMB0GA1UdDgQW
BBR4Q3t2pn680K9+QjfrNXw7hwFRPDAfBgNVHSMEGDAWgBR4Q3t2pn680K9+Qjfr
NXw7hwFRPDAOBgNVHQ8BAf8EBAMCAQYwEgYDVR0TAQH/BAgwBgEB/wIBADANBgkq
hkiG9w0BAQsFAAOCAYEAeF8tYMXICvQqeXYQITkV2oLJsp6J4JAqJabHWxYJHGir
IEqucRiJSSx+HjIJEUVaj8E0QjEud6Y5lNmXlcjqRXaCPOqK0eGRz6hi+ripMtPZ
sFNaBwLQVV905SDjAzDzNIDnrcnXyB4gcDFCvwDFKKgLRjOB/WAqgscDUoGq5ZVi
zLUzTqiQPmULAQaB9c6Oti6snEFJiCQ67JLyW/E83/frzCmO5Ru6WjU4tmsmy8Ra
Ud4APK0wZTGtfPXU7w+IBdG5Ez0kE1qzxGQaL4gINJ1zMyleDnbuS8UicjJijvqA
152Sq049ESDz+1rRGc2NVEqh1KaGXmtXvqxXcTB+Ljy5Bw2ke0v8iGngFBPqCTVB
3op5KBG3RjbF6RRSzwzuWfL7QErNC8WEy5yDVARzTA5+xmBc388v9Dm21HGfcC8O
DD+gT9sSpssq0ascmvH49MOgjt1yoysLtdCtJW/9FZpoOypaHx0R+mJTLwPXVMrv
DaVzWh5aiEx+idkSGMnX
-----END CERTIFICATE-----`)

	block, _ := pem.Decode(certPEM)
	if block == nil {
		return nil, errors.New("failed to decode PEM block")
	}

	cert, err := x509.ParseCertificate(block.Bytes)
	if err != nil {
		return nil, fmt.Errorf("failed to parse certificate: %v", err)
	}

	return cert, nil
}

// GetSigRL retrieves the Signature Revocation List for a given EPID group ID
func (c *IASClient) GetSigRL(gid []byte) ([]byte, error) {
	start := time.Now()
	defer func() {
		duration := time.Since(start).Seconds()
		iasRequestDuration.WithLabelValues("sigrl").Observe(duration)
	}()

	c.log.WithField("gid", fmt.Sprintf("%x", gid)).Info("Retrieving SigRL from IAS")

	// Check cache first
	if c.cache != nil {
		if sigRL, ok := c.cache.GetCachedSigRL(gid); ok {
			c.log.Info("Using cached SigRL")
			iasRequestTotal.WithLabelValues("sigrl", "cache_hit").Inc()
			return sigRL, nil
		}
	}

	// Apply rate limiting
	if c.cache != nil {
		ctx := context.Background()
		if err := c.cache.WaitIASRequest(ctx); err != nil {
			c.log.WithError(err).Error("IAS request rate limit exceeded")
			iasRequestTotal.WithLabelValues("sigrl", "rate_limited").Inc()
			return nil, fmt.Errorf("IAS request rate limit exceeded: %w", err)
		}
	}

	// Convert group ID to hex string
	gidHex := fmt.Sprintf("%02x", gid)

	// Create request
	req, err := http.NewRequest(http.MethodGet, c.baseURL+iasSigrlEndpoint+"/"+gidHex, nil)
	if err != nil {
		c.log.WithError(err).Error("Failed to create request")
		iasRequestTotal.WithLabelValues("sigrl", "error").Inc()
		return nil, fmt.Errorf("failed to create request: %w", err)
	}

	// Add headers
	req.Header.Set(iasSubscriptionKeyHeader, c.apiKey)

	// Send request
	resp, err := c.httpClient.Do(req)
	if err != nil {
		c.log.WithError(err).Error("Failed to send request")
		iasRequestTotal.WithLabelValues("sigrl", "error").Inc()
		return nil, fmt.Errorf("failed to send request: %w", err)
	}
	defer resp.Body.Close()

	// Read response body
	body, err := io.ReadAll(resp.Body)
	if err != nil {
		c.log.WithError(err).Error("Failed to read response body")
		iasRequestTotal.WithLabelValues("sigrl", "error").Inc()
		return nil, fmt.Errorf("failed to read response body: %w", err)
	}

	// Check response status
	if resp.StatusCode != http.StatusOK {
		if resp.StatusCode == http.StatusNotFound {
			// No SigRL for this group ID
			c.log.Info("No SigRL found for group ID")
			iasRequestTotal.WithLabelValues("sigrl", "404").Inc()
			return nil, nil
		}
		c.log.WithFields(logrus.Fields{
			"status": resp.StatusCode,
			"body":   string(body),
		}).Error("SigRL request failed")
		iasRequestTotal.WithLabelValues("sigrl", fmt.Sprintf("%d", resp.StatusCode)).Inc()
		return nil, fmt.Errorf("%w: status %d: %s", ErrIASRequestFailed, resp.StatusCode, string(body))
	}
	iasRequestTotal.WithLabelValues("sigrl", "200").Inc()

	// Decode SigRL from base64
	sigRL, err := base64.StdEncoding.DecodeString(string(body))
	if err != nil {
		c.log.WithError(err).Error("Failed to decode SigRL")
		return nil, fmt.Errorf("failed to decode SigRL: %w", err)
	}

	// Cache successful response
	if c.cache != nil {
		c.cache.CacheSigRL(gid, sigRL)
	}

	c.log.WithField("size", len(sigRL)).Info("Successfully retrieved SigRL")
	return sigRL, nil
}
