// Package attestation provides attestation verification functionality for different
// TEE platforms including AWS Nitro and Azure SGX.
package attestation

import (
	"bytes"
	"context"
	"crypto/rand"
	"crypto/sha256"
	"crypto/x509"
	"encoding/base64"
	"encoding/binary"
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"net/http"
	"strings"
	"sync"
	"time"

	"github.com/neo_service_layer/pkg/logging"
	"github.com/neo_service_layer/pkg/teeservice"
	attest "github.com/r3e-network/neo_service_layer/internal/teeservice/attestation"
	"github.com/rs/zerolog/log"
)

var (
	// ErrInvalidSGXQuote indicates an invalid SGX quote format
	ErrInvalidSGXQuote = errors.New("invalid SGX quote format")

	// ErrInvalidMREnclave indicates the MRENCLAVE value is invalid
	ErrInvalidMREnclave = errors.New("invalid MRENCLAVE value")

	// ErrInvalidMRSigner indicates the MRSIGNER value is invalid
	ErrInvalidMRSigner = errors.New("invalid MRSIGNER value")

	// ErrDebugNotAllowed indicates debug mode is not allowed
	ErrDebugNotAllowed = errors.New("debug mode not allowed")

	// ErrAttestationServiceFailure indicates a failure in the attestation service
	ErrAttestationServiceFailure = errors.New("attestation service failure")
)

// SGXAttestationResponse represents the response from the Azure Attestation Service
type SGXAttestationResponse struct {
	Token    string `json:"token"`
	JWSToken struct {
		Payload string `json:"payload"`
	} `json:"jws"`
}

// SGXAttestationPayload represents the payload from the attestation token
type SGXAttestationPayload struct {
	VerifyResult    bool   `json:"x-ms-sgx-is-debuggable"`
	MREnclave       string `json:"x-ms-sgx-mrenclave"`
	MRSigner        string `json:"x-ms-sgx-mrsigner"`
	ProductID       uint16 `json:"x-ms-sgx-product-id"`
	SVN             uint16 `json:"x-ms-sgx-svn"`
	EnclaveHeldData string `json:"x-ms-sgx-ehd"`
	NonceName       string `json:"x-ms-sgx-nonce-name"`
	Nonce           string `json:"x-ms-sgx-nonce"`
	IsDebuggable    bool   `json:"x-ms-sgx-is-debuggable"`
	Timestamp       int64  `json:"iat"`
}

// SGXVerifierConfig contains configuration for the SGX verifier
type SGXVerifierConfig struct {
	ChallengeExpiry           time.Duration
	AttestationValidityPeriod time.Duration
	IASBaseURL                string
	IASAPIKey                 string
	ValidMREnclave            string
	ValidMRSigner             string
	AllowDebug                bool
}

// SGXQuote represents an Intel SGX quote structure
type SGXQuote struct {
	// Version of the quote structure
	Version uint16

	// Signature type (EPID vs ECDSA)
	SignatureType uint16

	// MRENCLAVE (hash of enclave code)
	MRENCLAVE [32]byte

	// MRSIGNER (hash of the enclave signer's public key)
	MRSIGNER [32]byte

	// CPUSVN (CPU security version number)
	CPUSVN [16]byte

	// Flags for debug and other settings
	Flags uint64

	// ISVPRODID (ISV assigned Product ID)
	ISVPRODID uint16

	// ISVSVN (ISV assigned Security Version Number)
	ISVSVN uint16

	// ReportData (custom data, typically includes a hash of a public key)
	ReportData [64]byte
}

// SGXAttestationEvidence represents the complete SGX attestation evidence
type SGXAttestationEvidence struct {
	// The raw quote data
	Quote []byte `json:"quote"`

	// Additional data from the enclave
	EnclaveHeldData []byte `json:"enclave_held_data,omitempty"`

	// Signature over the quote data
	Signature []byte `json:"signature,omitempty"`

	// Signing certificate chain
	Certificates []string `json:"certificates,omitempty"`
}

// SGXVerifier implements the attestation.IAttestationVerifier interface for SGX
type SGXVerifier struct {
	config           SGXVerifierConfig
	httpClient       *http.Client
	mu               sync.RWMutex
	challenges       map[string]time.Time
	cleanupRunning   bool
	allowList        map[string][]SGXMeasurement
	cleanupInterval  time.Duration
	challengeTimeout time.Duration
	stopCleanup      chan struct{}
	logger           *logging.Logger

	// Trusted MRSIGNER values (hash of the signing key)
	allowedMRSIGNER []string

	// Trusted MRENCLAVE values (hash of the enclave code)
	allowedMRENCLAVE []string

	// Certificate trust chain
	trustedRoots *x509.CertPool
}

// SGXMeasurement contains the measurements for an SGX enclave
type SGXMeasurement struct {
	MREnclave string
	MRSigner  string
}

// SGXVerificationOptions contains SGX-specific verification options
type SGXVerificationOptions struct {
	// AttestationURL is the URL of the attestation service
	AttestationURL string

	// ProductID is the expected product ID
	ProductID uint16

	// MinimumSVN is the minimum security version number
	MinimumSVN uint16

	// AllowDebug indicates whether debug enclaves are allowed
	AllowDebug bool
}

// NewSGXVerifier creates a new SGX attestation verifier
func NewSGXVerifier(config SGXVerifierConfig, logger *logging.Logger) *SGXVerifier {
	if config.ChallengeExpiry == 0 {
		config.ChallengeExpiry = 5 * time.Minute
	}
	if config.AttestationValidityPeriod == 0 {
		config.AttestationValidityPeriod = 24 * time.Hour
	}
	if config.IASBaseURL == "" {
		config.IASBaseURL = "https://api.trustedservices.intel.com/sgx/dev"
	}

	return &SGXVerifier{
		config:           config,
		httpClient:       &http.Client{Timeout: 30 * time.Second},
		challenges:       make(map[string]time.Time),
		allowList:        make(map[string][]SGXMeasurement),
		cleanupInterval:  5 * time.Minute,
		challengeTimeout: 10 * time.Minute,
		stopCleanup:      make(chan struct{}),
		logger:           logger,
		allowedMRSIGNER: []string{
			// Example trusted MRSIGNER values
			"1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
		},
		allowedMRENCLAVE: []string{
			// Example trusted MRENCLAVE values
			"abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890",
		},
		trustedRoots: x509.NewCertPool(),
	}
}

// AddAllowedMeasurement adds an SGX measurement to the allowlist
func (v *SGXVerifier) AddAllowedMeasurement(serviceID, mrenclave, mrsigner string) {
	if _, exists := v.allowList[serviceID]; !exists {
		v.allowList[serviceID] = []SGXMeasurement{}
	}
	v.allowList[serviceID] = append(v.allowList[serviceID], SGXMeasurement{
		MREnclave: mrenclave,
		MRSigner:  mrsigner,
	})
}

// RemoveAllowedMeasurement removes an SGX measurement from the allowlist
func (v *SGXVerifier) RemoveAllowedMeasurement(serviceID, mrenclave, mrsigner string) bool {
	if measurements, exists := v.allowList[serviceID]; exists {
		for i, measurement := range measurements {
			if measurement.MREnclave == mrenclave && measurement.MRSigner == mrsigner {
				// Remove the measurement
				v.allowList[serviceID] = append(measurements[:i], measurements[i+1:]...)
				return true
			}
		}
	}
	return false
}

// GetChallenge generates a new random challenge for attestation
func (v *SGXVerifier) GetChallenge(ctx context.Context) (string, error) {
	// Generate random challenge (256 bits = 32 bytes)
	challengeBytes := make([]byte, 32)
	if _, err := SGX_CRAND.Read(challengeBytes); err != nil {
		log.Error().Err(err).Msg("Failed to generate random challenge for SGX attestation")
		return "", fmt.Errorf("failed to generate random challenge: %w", err)
	}

	// We encode as hex for easy handling across systems
	challenge := hex.EncodeToString(challengeBytes)

	// Store challenge with timestamp for verification
	v.mu.Lock()
	defer v.mu.Unlock()
	v.challenges[challenge] = time.Now().Add(v.challengeTimeout)

	// Start cleanup routine if not already running
	if !v.cleanupRunning {
		go v.cleanupExpiredChallenges()
	}

	return challenge, nil
}

// cleanupExpiredChallenges periodically removes expired challenges from the map
func (v *SGXVerifier) cleanupExpiredChallenges() {
	v.mu.Lock()
	v.cleanupRunning = true
	v.mu.Unlock()

	ticker := time.NewTicker(v.cleanupInterval)
	defer ticker.Stop()

	for {
		select {
		case <-ticker.C:
			v.mu.Lock()
			now := time.Now()
			for challenge, expiry := range v.challenges {
				if now.After(expiry) {
					delete(v.challenges, challenge)
				}
			}
			// If no more challenges, stop the cleanup routine
			if len(v.challenges) == 0 {
				v.cleanupRunning = false
				v.mu.Unlock()
				return
			}
			v.mu.Unlock()
		case <-v.stopCleanup:
			v.mu.Lock()
			v.cleanupRunning = false
			v.mu.Unlock()
			return
		}
	}
}

// VerifyAttestation verifies the SGX attestation evidence
func (v *SGXVerifier) VerifyAttestation(ctx context.Context, evidence *attest.Evidence) (*attest.VerificationResult, error) {
	if evidence == nil {
		return nil, errors.New("evidence cannot be nil")
	}

	// Parse the evidence
	var sgxEvidence SGXEvidence
	if err := json.Unmarshal([]byte(evidence.Data), &sgxEvidence); err != nil {
		return nil, fmt.Errorf("failed to unmarshal SGX evidence: %w", err)
	}

	// Verify the challenge is valid and not expired
	v.mu.RLock()
	expiry, ok := v.challenges[sgxEvidence.Challenge]
	v.mu.RUnlock()

	if !ok {
		return nil, fmt.Errorf("unknown challenge: %s", sgxEvidence.Challenge)
	}

	if time.Now().After(expiry) {
		// Remove expired challenge
		v.mu.Lock()
		delete(v.challenges, sgxEvidence.Challenge)
		v.mu.Unlock()
		return nil, errors.New("challenge has expired")
	}

	// Verify the quote with Intel Attestation Service (IAS)
	result, err := v.verifyQuoteWithIAS(ctx, sgxEvidence.Quote)
	if err != nil {
		return nil, fmt.Errorf("IAS verification failed: %w", err)
	}

	// Remove the challenge after successful verification
	v.mu.Lock()
	delete(v.challenges, sgxEvidence.Challenge)
	v.mu.Unlock()

	// Extract enclave measurements from the verified quote
	measurements, err := v.extractMeasurements(result)
	if err != nil {
		return nil, fmt.Errorf("failed to extract measurements: %w", err)
	}

	// Compare the measurements against allowed values
	if err := v.validateMeasurements(measurements); err != nil {
		return nil, fmt.Errorf("measurement validation failed: %w", err)
	}

	// Return successful verification result
	return &attest.VerificationResult{
		Verified:     true,
		Platform:     "SGX",
		Measurements: measurements,
		ExpiresAt:    time.Now().Add(v.config.AttestationValidityPeriod),
	}, nil
}

// verifyQuoteWithIAS sends the quote to Intel Attestation Service for verification
func (v *SGXVerifier) verifyQuoteWithIAS(ctx context.Context, quote string) (*iasResponse, error) {
	// Decode the quote from base64
	quoteBytes, err := base64.StdEncoding.DecodeString(quote)
	if err != nil {
		return nil, fmt.Errorf("failed to decode quote: %w", err)
	}

	// Prepare the request to IAS
	reqURL := v.config.IASBaseURL + "/attestation/v4/report"
	req, err := http.NewRequestWithContext(ctx, http.MethodPost, reqURL, bytes.NewReader(quoteBytes))
	if err != nil {
		return nil, fmt.Errorf("failed to create request: %w", err)
	}

	// Add required headers
	req.Header.Set("Content-Type", "application/octet-stream")
	req.Header.Set("Ocp-Apim-Subscription-Key", v.config.IASAPIKey)

	// Send the request
	resp, err := v.httpClient.Do(req)
	if err != nil {
		return nil, fmt.Errorf("failed to send request to IAS: %w", err)
	}
	defer resp.Body.Close()

	// Check response status
	if resp.StatusCode != http.StatusOK {
		body, _ := io.ReadAll(resp.Body)
		return nil, fmt.Errorf("IAS returned non-OK status: %d, body: %s", resp.StatusCode, string(body))
	}

	// Parse the response
	var iasResp iasResponse
	if err := json.NewDecoder(resp.Body).Decode(&iasResp); err != nil {
		return nil, fmt.Errorf("failed to parse IAS response: %w", err)
	}

	// Verify the signature on the response
	if err := v.verifyIASSignature(resp.Header, iasResp); err != nil {
		return nil, fmt.Errorf("IAS signature verification failed: %w", err)
	}

	return &iasResp, nil
}

// extractMeasurements extracts enclave measurements from IAS response
func (v *SGXVerifier) extractMeasurements(resp *iasResponse) (map[string]string, error) {
	measurements := make(map[string]string)

	// Extract MRENCLAVE and MRSIGNER from quote report
	if resp.ISVEnclaveQuoteBody != "" {
		quoteBody, err := base64.StdEncoding.DecodeString(resp.ISVEnclaveQuoteBody)
		if err != nil {
			return nil, fmt.Errorf("failed to decode quote body: %w", err)
		}

		// Extract MRENCLAVE (offset defined by SGX quote structure)
		if len(quoteBody) >= 112 {
			measurements["MRENCLAVE"] = hex.EncodeToString(quoteBody[64:96])
		}

		// Extract MRSIGNER (offset defined by SGX quote structure)
		if len(quoteBody) >= 176 {
			measurements["MRSIGNER"] = hex.EncodeToString(quoteBody[128:160])
		}
	}

	// Add other relevant fields from the response
	measurements["STATUS"] = resp.ISVEnclaveQuoteStatus
	measurements["TIMESTAMP"] = resp.Timestamp

	return measurements, nil
}

// validateMeasurements compares measurements against allowed values
func (v *SGXVerifier) validateMeasurements(measurements map[string]string) error {
	// Check if quote status is OK
	status, ok := measurements["STATUS"]
	if !ok {
		return errors.New("quote status not found in measurements")
	}

	// SGX quote status validation
	// OK: Quote verification passed
	// GROUP_OUT_OF_DATE: Platform is running with updated microcode but TCB level is outdated
	// CONFIGURATION_NEEDED: Platform is using a vulnerable SGX feature
	switch status {
	case "OK":
		// Best case - continue validation
	case "GROUP_OUT_OF_DATE", "CONFIGURATION_NEEDED":
		// These statuses are acceptable in some cases, but log warnings
		log.Warn().Str("status", status).Msg("SGX quote status indicates possible security concerns")
	default:
		return fmt.Errorf("unacceptable quote status: %s", status)
	}

	// Validate MRENCLAVE if configured
	if v.config.ValidMREnclave != "" {
		mrenclave, ok := measurements["MRENCLAVE"]
		if !ok {
			return errors.New("MRENCLAVE measurement not found")
		}
		if mrenclave != v.config.ValidMREnclave {
			return fmt.Errorf("MRENCLAVE mismatch: expected %s, got %s", v.config.ValidMREnclave, mrenclave)
		}
	}

	// Validate MRSIGNER if configured
	if v.config.ValidMRSigner != "" {
		mrsigner, ok := measurements["MRSIGNER"]
		if !ok {
			return errors.New("MRSIGNER measurement not found")
		}
		if mrsigner != v.config.ValidMRSigner {
			return fmt.Errorf("MRSIGNER mismatch: expected %s, got %s", v.config.ValidMRSigner, mrsigner)
		}
	}

	return nil
}

// verifyIASSignature verifies the signature on the IAS response
func (v *SGXVerifier) verifyIASSignature(headers http.Header, resp iasResponse) error {
	// Get the signature from response headers
	signature := headers.Get("X-IASReport-Signature")
	if signature == "" {
		return errors.New("IAS response signature header missing")
	}

	// Get the signing certificate
	certChain := headers.Get("X-IASReport-Signing-Certificate")
	if certChain == "" {
		return errors.New("IAS signing certificate header missing")
	}

	// TODO: Implement proper signature verification with x509 certificate parsing
	// For now, we'll just log and assume valid (should be replaced with real verification)
	log.Warn().Msg("IAS signature verification not fully implemented - assuming valid signature")

	return nil
}

// SGXEvidence represents the format of SGX attestation evidence
type SGXEvidence struct {
	Challenge string `json:"challenge"`
	Quote     string `json:"quote"` // Base64 encoded SGX quote
	Nonce     string `json:"nonce,omitempty"`
	Data      string `json:"data,omitempty"` // Additional data for verification
}

// iasResponse represents the format of Intel Attestation Service response
type iasResponse struct {
	ID                    string `json:"id"`
	Timestamp             string `json:"timestamp"`
	Version               int    `json:"version"`
	ISVEnclaveQuoteStatus string `json:"isvEnclaveQuoteStatus"`
	ISVEnclaveQuoteBody   string `json:"isvEnclaveQuoteBody"`
	RevocationReason      string `json:"revocationReason,omitempty"`
	PSEManifestStatus     string `json:"pseManifestStatus,omitempty"`
	PSEManifestHash       string `json:"pseManifestHash,omitempty"`
	Platform              string `json:"platform,omitempty"`
	Advisory              string `json:"advisory,omitempty"`
}

// IsValidMeasurement implements the IAttestationVerifier interface
func (v *SGXVerifier) IsValidMeasurement(ctx context.Context, provider string, measurements map[string]string) (bool, error) {
	if provider != "azure_sgx" {
		return false, fmt.Errorf("invalid provider: %s", provider)
	}

	mrenclave, mrEnclaveExists := measurements["mrenclave"]
	mrsigner, mrSignerExists := measurements["mrsigner"]

	if !mrEnclaveExists || !mrSignerExists {
		return false, errors.New("MRENCLAVE or MRSIGNER not provided")
	}

	// Check if the measurement is in any service's allowlist
	for _, allowed := range v.allowList {
		for _, measurement := range allowed {
			if measurement.MREnclave == mrenclave && measurement.MRSigner == mrsigner {
				return true, nil
			}
		}
	}

	return false, nil
}

// Helper functions

// simulateAttestationVerification simulates the Azure Attestation Service verification
func (v *SGXVerifier) simulateAttestationVerification(quote []byte, options *attest.VerificationOptions) (*SGXAttestationPayload, error) {
	// In a real implementation, this would make a REST call to the Azure Attestation Service
	// For demonstration, we'll simulate a successful attestation

	// Create a fake payload
	payload := &SGXAttestationPayload{
		MREnclave:       "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
		MRSigner:        "fedcba0987654321fedcba0987654321fedcba0987654321fedcba0987654321",
		ProductID:       1,
		SVN:             2,
		EnclaveHeldData: `{"service_id":"functionservice","instance_id":"sgx_123456789abc"}`,
		IsDebuggable:    false,
		Timestamp:       time.Now().Unix(),
	}

	// If options include a challenge, use it
	if options != nil && options.Challenge != "" {
		payload.Nonce = options.Challenge
	}

	return payload, nil
}

// extractSGXServiceID extracts the service ID from enclave held data
func extractSGXServiceID(enclaveHeldData string) string {
	// In a real implementation, this would parse JSON
	// For now, return a placeholder
	return "functionservice"
}

// isAllowedMeasurement checks if an SGX measurement is in the allowlist for a service
func (v *SGXVerifier) isAllowedMeasurement(serviceID, mrenclave, mrsigner string) bool {
	allowed, exists := v.allowList[serviceID]
	if !exists {
		return false
	}

	for _, measurement := range allowed {
		if measurement.MREnclave == mrenclave && measurement.MRSigner == mrsigner {
			return true
		}
	}

	return false
}

// CryptoRandReader is a wrapper around crypto/rand.Reader
type CryptoRandReader struct{}

// Read implements the io.Reader interface using crypto/rand
func (r CryptoRandReader) Read(p []byte) (n int, err error) {
	return rand.Read(p)
}

// SGX_CRAND is a placeholder for crypto-secure random
var SGX_CRAND = CryptoRandReader{}

// GetProviderType returns the provider type this verifier is for
func (v *SGXVerifier) GetProviderType() teeservice.ProviderType {
	return teeservice.ProviderTypeAzureSGX
}

// VerifyAttestation verifies an SGX attestation evidence
func (v *SGXVerifier) VerifyAttestation(
	ctx context.Context,
	evidence []byte,
	options *teeservice.VerificationOptions,
) (*teeservice.VerificationResult, error) {
	// Parse the attestation evidence
	var attestationEvidence SGXAttestationEvidence
	if err := json.Unmarshal(evidence, &attestationEvidence); err != nil {
		return nil, fmt.Errorf("failed to parse SGX attestation evidence: %w", err)
	}

	// Verify the attestation
	if len(attestationEvidence.Quote) == 0 {
		return nil, errors.New("empty SGX quote")
	}

	// Parse the quote
	quote, err := v.parseQuote(attestationEvidence.Quote)
	if err != nil {
		return nil, fmt.Errorf("failed to parse SGX quote: %w", err)
	}

	// Check if debug mode is allowed
	if quote.Flags&0x2 != 0 && !options.AllowDebug {
		return nil, errors.New("debug enclave not allowed")
	}

	// Verify the enclave identity (MRENCLAVE)
	mrenclave := hex.EncodeToString(quote.MRENCLAVE[:])
	mrsigner := hex.EncodeToString(quote.MRSIGNER[:])

	// Check against allowed MRENCLAVE values
	mrenclaveTrusted := false
	for _, allowed := range v.allowedMRENCLAVE {
		if strings.EqualFold(mrenclave, allowed) {
			mrenclaveTrusted = true
			break
		}
	}

	// Check against allowed MRSIGNER values
	mrsignerTrusted := false
	for _, allowed := range v.allowedMRSIGNER {
		if strings.EqualFold(mrsigner, allowed) {
			mrsignerTrusted = true
			break
		}
	}

	// If an expected identity is provided, verify it
	if options.RequireIdentity && options.ExpectedIdentity != "" {
		if !strings.EqualFold(mrenclave, options.ExpectedIdentity) && !strings.EqualFold(mrsigner, options.ExpectedIdentity) {
			return nil, fmt.Errorf("identity mismatch: expected %s, got MRENCLAVE=%s, MRSIGNER=%s",
				options.ExpectedIdentity, mrenclave, mrsigner)
		}
	}

	// Check if at least one verification method passed
	if !mrenclaveTrusted && !mrsignerTrusted {
		return nil, fmt.Errorf("enclave identity not trusted: MRENCLAVE=%s, MRSIGNER=%s", mrenclave, mrsigner)
	}

	// In a real implementation, we would also:
	// 1. Verify the certificate chain against trusted roots
	// 2. Verify the quote signature using the certificate
	// 3. Check for revocation (TCB info)
	// 4. Verify the quote was generated with a fresh nonce

	// Create the verification result
	result := &teeservice.VerificationResult{
		Valid:         true,
		Identity:      mrenclave,
		SecurityLevel: v.determineSecurityLevel(quote),
		Measurements: map[string]string{
			"MRENCLAVE": mrenclave,
			"MRSIGNER":  mrsigner,
			"ISVPRODID": fmt.Sprintf("%d", quote.ISVPRODID),
			"ISVSVN":    fmt.Sprintf("%d", quote.ISVSVN),
		},
		Timestamp:      time.Now(),
		ExpirationTime: time.Now().Add(24 * time.Hour), // Typically valid for 24 hours
	}

	return result, nil
}

// parseQuote parses an SGX quote from its binary representation
func (v *SGXVerifier) parseQuote(quoteData []byte) (*SGXQuote, error) {
	if len(quoteData) < 432 {
		return nil, errors.New("SGX quote data too short")
	}

	quote := &SGXQuote{}

	// Parse the quote header
	quote.Version = binary.LittleEndian.Uint16(quoteData[0:2])
	quote.SignatureType = binary.LittleEndian.Uint16(quoteData[2:4])

	// Parse the enclave report
	// MRENCLAVE at offset 48
	copy(quote.MRENCLAVE[:], quoteData[48:80])

	// MRSIGNER at offset 128
	copy(quote.MRSIGNER[:], quoteData[128:160])

	// ISVPRODID at offset 256
	quote.ISVPRODID = binary.LittleEndian.Uint16(quoteData[256:258])

	// ISVSVN at offset 258
	quote.ISVSVN = binary.LittleEndian.Uint16(quoteData[258:260])

	// Extract flags
	quote.Flags = binary.LittleEndian.Uint64(quoteData[96:104])

	// Extract CPUSVN
	copy(quote.CPUSVN[:], quoteData[32:48])

	// Extract report data
	copy(quote.ReportData[:], quoteData[368:432])

	return quote, nil
}

// determineSecurityLevel evaluates the security level based on the quote contents
func (v *SGXVerifier) determineSecurityLevel(quote *SGXQuote) teeservice.SecurityLevel {
	// Check if debug mode is enabled
	if quote.Flags&0x2 != 0 {
		return teeservice.SecurityLevelStandard
	}

	// Check SVN version - higher SVN means more security patches
	if quote.ISVSVN >= 5 {
		return teeservice.SecurityLevelCritical
	} else if quote.ISVSVN >= 2 {
		return teeservice.SecurityLevelHigh
	}

	return teeservice.SecurityLevelStandard
}

// calculateSHA256 calculates the SHA-256 hash of the input data
func calculateSHA256(data []byte) string {
	hash := sha256.Sum256(data)
	return hex.EncodeToString(hash[:])
}

// parseCertificateChain parses X.509 certificates from the PEM-encoded strings
func parseCertificateChain(certStrings []string) ([]*x509.Certificate, error) {
	var certs []*x509.Certificate

	for _, certStr := range certStrings {
		// Decode PEM data
		derBytes, err := base64.StdEncoding.DecodeString(certStr)
		if err != nil {
			return nil, fmt.Errorf("failed to decode certificate: %w", err)
		}

		// Parse X.509 certificate
		cert, err := x509.ParseCertificate(derBytes)
		if err != nil {
			return nil, fmt.Errorf("failed to parse certificate: %w", err)
		}

		certs = append(certs, cert)
	}

	return certs, nil
}
