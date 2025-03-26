package tee

import (
	"crypto/sha256"
	"encoding/base64"
	"encoding/binary"
	"encoding/json"
	"errors"
	"fmt"
	"os"
	"time"

	"github.com/sirupsen/logrus"
)

var (
	// ErrInvalidEvidence indicates the attestation evidence is invalid
	ErrInvalidEvidence = errors.New("invalid attestation evidence")
	// ErrExpiredEvidence indicates the attestation evidence has expired
	ErrExpiredEvidence = errors.New("attestation evidence has expired")
	// ErrInvalidSignature indicates the evidence signature is invalid
	ErrInvalidSignature = errors.New("invalid evidence signature")
	// ErrUnsupportedPlatform indicates the TEE platform is not supported
	ErrUnsupportedPlatform = errors.New("unsupported TEE platform")
	// ErrInvalidSecurityVersion indicates the security version is too low
	ErrInvalidSecurityVersion = errors.New("invalid security version")
	// ErrInvalidMeasurement indicates an invalid enclave measurement
	ErrInvalidMeasurement = errors.New("invalid enclave measurement")
	// ErrMissingQuote indicates missing SGX quote data
	ErrMissingQuote = errors.New("missing SGX quote data")
	// ErrInvalidQuote indicates invalid SGX quote data
	ErrInvalidQuote = errors.New("invalid SGX quote data")
	// ErrMissingClaim indicates a required claim is missing
	ErrMissingClaim = errors.New("missing required claim")
)

// AttestationType represents the type of attestation
type AttestationType string

// Attestation types
const (
	SGX       AttestationType = "sgx"
	TrustedVM AttestationType = "trusted_vm"
	Mock      AttestationType = "mock"
)

// AttestationEvidence represents evidence of trusted execution
type AttestationEvidence struct {
	Type              AttestationType   `json:"type"`
	Timestamp         time.Time         `json:"timestamp"`
	TeePlatform       string            `json:"tee_platform"`
	TeeVersion        string            `json:"tee_version"`
	MrEnclave         string            `json:"mr_enclave,omitempty"`
	MrSigner          string            `json:"mr_signer,omitempty"`
	ProductID         string            `json:"product_id,omitempty"`
	SecurityVersion   int               `json:"security_version,omitempty"`
	EnclaveName       string            `json:"enclave_name"`
	QuoteData         string            `json:"quote_data,omitempty"`
	Signature         string            `json:"signature"`
	RuntimeAttributes map[string]string `json:"runtime_attributes,omitempty"`
	Claims            map[string]string `json:"claims,omitempty"`
	Certificate       string            `json:"certificate,omitempty"`
}

// AttestationResult represents the result of attestation verification
type AttestationResult struct {
	IsValid      bool                 `json:"is_valid"`
	Evidence     *AttestationEvidence `json:"evidence,omitempty"`
	Timestamp    time.Time            `json:"timestamp"`
	ErrorMessage string               `json:"error_message,omitempty"`
}

// SecurityPolicy represents a security policy for attestation
type SecurityPolicy struct {
	RequiredType         AttestationType   `json:"required_type"`
	AllowedTeePlatforms  []string          `json:"allowed_tee_platforms"`
	MinSecurityVersion   int               `json:"min_security_version"`
	AllowedMrEnclave     []string          `json:"allowed_mr_enclave,omitempty"`
	AllowedMrSigner      []string          `json:"allowed_mr_signer,omitempty"`
	RequiredClaims       map[string]string `json:"required_claims,omitempty"`
	MaxAttestationAge    time.Duration     `json:"max_attestation_age"`
	RequireDebugDisabled bool              `json:"require_debug_disabled"`
}

// Attestation provides methods for TEE attestation
type Attestation struct {
	policy SecurityPolicy
	log    *logrus.Logger
}

// NewAttestation creates a new attestation with the given policy
func NewAttestation(policy SecurityPolicy) *Attestation {
	logger := logrus.New()
	logger.SetFormatter(&logrus.JSONFormatter{})
	logger.SetLevel(logrus.InfoLevel)

	return &Attestation{
		policy: policy,
		log:    logger,
	}
}

// GenerateEvidence generates attestation evidence for the current TEE environment
func (a *Attestation) GenerateEvidence() (*AttestationEvidence, error) {
	// In a real implementation, this would interact with the TEE to generate real evidence
	// For now, we'll return a mock evidence structure

	if a.policy.RequiredType == Mock {
		return generateMockEvidence(), nil
	}

	// For SGX or other platforms, this would involve platform-specific code
	return nil, fmt.Errorf("attestation type not implemented: %s", a.policy.RequiredType)
}

// VerifyEvidence verifies attestation evidence against the security policy
func (a *Attestation) VerifyEvidence(evidence *AttestationEvidence) (*AttestationResult, error) {
	if evidence == nil {
		a.log.Error("Evidence is nil")
		return &AttestationResult{
			IsValid:      false,
			Timestamp:    time.Now(),
			ErrorMessage: ErrInvalidEvidence.Error(),
		}, ErrInvalidEvidence
	}

	a.log.WithFields(logrus.Fields{
		"type":        evidence.Type,
		"platform":    evidence.TeePlatform,
		"version":     evidence.TeeVersion,
		"enclaveName": evidence.EnclaveName,
	}).Info("Verifying attestation evidence")

	// Check evidence type
	if evidence.Type != a.policy.RequiredType {
		a.log.WithFields(logrus.Fields{
			"expected": a.policy.RequiredType,
			"got":      evidence.Type,
		}).Error("Evidence type mismatch")
		return &AttestationResult{
			IsValid:      false,
			Evidence:     evidence,
			Timestamp:    time.Now(),
			ErrorMessage: fmt.Sprintf("evidence type mismatch: expected %s, got %s", a.policy.RequiredType, evidence.Type),
		}, ErrInvalidEvidence
	}

	// Check evidence timestamp
	maxAge := a.policy.MaxAttestationAge
	if maxAge == 0 {
		maxAge = 24 * time.Hour // Default to 24 hours
	}

	if time.Since(evidence.Timestamp) > maxAge {
		a.log.WithFields(logrus.Fields{
			"timestamp": evidence.Timestamp,
			"maxAge":    maxAge,
		}).Error("Evidence has expired")
		return &AttestationResult{
			IsValid:      false,
			Evidence:     evidence,
			Timestamp:    time.Now(),
			ErrorMessage: ErrExpiredEvidence.Error(),
		}, ErrExpiredEvidence
	}

	// For mock type, just validate signature
	if evidence.Type == Mock {
		isValid := validateMockSignature(evidence)
		if !isValid {
			a.log.Error("Invalid mock signature")
			return &AttestationResult{
				IsValid:      false,
				Evidence:     evidence,
				Timestamp:    time.Now(),
				ErrorMessage: ErrInvalidSignature.Error(),
			}, ErrInvalidSignature
		}
		return &AttestationResult{
			IsValid:   true,
			Evidence:  evidence,
			Timestamp: time.Now(),
		}, nil
	}

	// For SGX type, verify platform and measurements
	if evidence.Type == SGX {
		if err := a.verifySGXEvidence(evidence); err != nil {
			a.log.WithError(err).Error("SGX evidence verification failed")
			return &AttestationResult{
				IsValid:      false,
				Evidence:     evidence,
				Timestamp:    time.Now(),
				ErrorMessage: err.Error(),
			}, err
		}
	}

	a.log.Info("Attestation evidence verified successfully")
	return &AttestationResult{
		IsValid:   true,
		Evidence:  evidence,
		Timestamp: time.Now(),
	}, nil
}

// verifySGXEvidence verifies SGX-specific evidence
func (a *Attestation) verifySGXEvidence(evidence *AttestationEvidence) error {
	// Check platform version
	var found bool
	for _, platform := range a.policy.AllowedTeePlatforms {
		if platform == evidence.TeePlatform {
			found = true
			break
		}
	}
	if !found {
		return fmt.Errorf("%w: %s", ErrUnsupportedPlatform, evidence.TeePlatform)
	}

	// Check security version
	if evidence.SecurityVersion < a.policy.MinSecurityVersion {
		return fmt.Errorf("%w: got %d, want >= %d", ErrInvalidSecurityVersion,
			evidence.SecurityVersion, a.policy.MinSecurityVersion)
	}

	// Check MrEnclave
	if len(a.policy.AllowedMrEnclave) > 0 {
		found = false
		for _, mrEnclave := range a.policy.AllowedMrEnclave {
			if mrEnclave == evidence.MrEnclave {
				found = true
				break
			}
		}
		if !found {
			return fmt.Errorf("%w: invalid MrEnclave", ErrInvalidMeasurement)
		}
	}

	// Check MrSigner
	if len(a.policy.AllowedMrSigner) > 0 {
		found = false
		for _, mrSigner := range a.policy.AllowedMrSigner {
			if mrSigner == evidence.MrSigner {
				found = true
				break
			}
		}
		if !found {
			return fmt.Errorf("%w: invalid MrSigner", ErrInvalidMeasurement)
		}
	}

	// Verify quote data
	if err := a.verifyQuoteData(evidence); err != nil {
		return err
	}

	// Verify required claims
	if err := a.verifyClaims(evidence); err != nil {
		return err
	}

	return nil
}

// verifyQuoteData verifies SGX quote data
func (a *Attestation) verifyQuoteData(evidence *AttestationEvidence) error {
	if len(evidence.QuoteData) == 0 {
		return ErrMissingQuote
	}

	// Decode quote data
	quoteBytes, err := base64.StdEncoding.DecodeString(evidence.QuoteData)
	if err != nil {
		return fmt.Errorf("failed to decode quote data: %w", err)
	}

	// Verify quote using IAS
	verified, err := verifySGXQuote(quoteBytes)
	if err != nil {
		return fmt.Errorf("failed to verify SGX quote: %w", err)
	}

	if !verified {
		return ErrInvalidQuote
	}

	return nil
}

// verifyClaims verifies required claims in the evidence
func (a *Attestation) verifyClaims(evidence *AttestationEvidence) error {
	if len(a.policy.RequiredClaims) > 0 {
		for key, value := range a.policy.RequiredClaims {
			actualValue, exists := evidence.Claims[key]
			if !exists || actualValue != value {
				return fmt.Errorf("%w: %s", ErrMissingClaim, key)
			}
		}
	}
	return nil
}

// SerializeEvidence serializes attestation evidence to JSON
func SerializeEvidence(evidence *AttestationEvidence) (string, error) {
	data, err := json.Marshal(evidence)
	if err != nil {
		return "", err
	}
	return base64.StdEncoding.EncodeToString(data), nil
}

// DeserializeEvidence deserializes attestation evidence from JSON
func DeserializeEvidence(evidenceStr string) (*AttestationEvidence, error) {
	data, err := base64.StdEncoding.DecodeString(evidenceStr)
	if err != nil {
		return nil, err
	}

	var evidence AttestationEvidence
	if err := json.Unmarshal(data, &evidence); err != nil {
		return nil, err
	}
	return &evidence, nil
}

// generateMockEvidence generates mock attestation evidence for testing
func generateMockEvidence() *AttestationEvidence {
	enclaveName := "neo-service-layer"
	timestamp := time.Now()

	// Create a mock signature
	sigData := fmt.Sprintf("%s:%s", enclaveName, timestamp.Format(time.RFC3339))
	hash := sha256.Sum256([]byte(sigData))
	signature := base64.StdEncoding.EncodeToString(hash[:])

	return &AttestationEvidence{
		Type:            Mock,
		Timestamp:       timestamp,
		TeePlatform:     "mock",
		TeeVersion:      "1.0.0",
		EnclaveName:     enclaveName,
		SecurityVersion: 1,
		Signature:       signature,
		RuntimeAttributes: map[string]string{
			"memory_size": "128MB",
			"cpu_cores":   "2",
		},
		Claims: map[string]string{
			"service": "neo-service-layer",
			"version": "0.1.0",
		},
	}
}

// validateMockSignature validates the signature of mock evidence
func validateMockSignature(evidence *AttestationEvidence) bool {
	// Create the expected signature
	sigData := fmt.Sprintf("%s:%s", evidence.EnclaveName, evidence.Timestamp.Format(time.RFC3339))
	hash := sha256.Sum256([]byte(sigData))
	expectedSig := base64.StdEncoding.EncodeToString(hash[:])

	// Compare with the actual signature
	return evidence.Signature == expectedSig
}

// verifyFunction is a placeholder for a function that would verify a function for TEE execution
func verifyFunction(functionName string, functionCode []byte, policy SecurityPolicy) (bool, error) {
	// In a real implementation, this would:
	// 1. Verify the code doesn't contain malicious operations
	// 2. Ensure it's compatible with the TEE runtime
	// 3. Verify memory & computation requirements are within limits
	// ...

	if len(functionCode) == 0 {
		return false, errors.New("empty function code")
	}

	// Simple mock implementation
	return true, nil
}

// SGXQuoteStatus represents the verification status of an SGX quote
type SGXQuoteStatus string

const (
	// SGXQuoteOK indicates the quote is valid
	SGXQuoteOK SGXQuoteStatus = "OK"
	// SGXQuoteSignatureInvalid indicates the quote signature is invalid
	SGXQuoteSignatureInvalid SGXQuoteStatus = "SIGNATURE_INVALID"
	// SGXQuoteGroupRevoked indicates the platform was revoked
	SGXQuoteGroupRevoked SGXQuoteStatus = "GROUP_REVOKED"
)

// SGXQuoteBody represents the structure of an SGX quote
type SGXQuoteBody struct {
	Version       uint16
	SignType      uint16
	QESecurityID  [16]byte
	ISVSecurityID [16]byte
	ReportData    [64]byte
	Signature     []byte
}

// CertificateChain represents an SGX certificate chain
type CertificateChain struct {
	PCKCert    string `json:"pck_cert"`     // Platform Certificate Key
	PCKCertCRL string `json:"pck_cert_crl"` // PCK Certificate Revocation List
	TCBInfo    string `json:"tcb_info"`     // Trusted Computing Base Info
	QEIdentity string `json:"qe_identity"`  // Quoting Enclave Identity
	RootCACRL  string `json:"root_ca_crl"`  // Root CA Certificate Revocation List
}

// validateCertificateChain validates the SGX certificate chain
func validateCertificateChain(chain *CertificateChain) error {
	if chain == nil {
		return errors.New("certificate chain is nil")
	}

	// Validate PCK certificate
	if chain.PCKCert == "" {
		return errors.New("PCK certificate is missing")
	}

	// Validate PCK CRL
	if chain.PCKCertCRL == "" {
		return errors.New("PCK certificate CRL is missing")
	}

	// Validate TCB info
	if chain.TCBInfo == "" {
		return errors.New("TCB info is missing")
	}

	// Validate QE identity
	if chain.QEIdentity == "" {
		return errors.New("QE identity is missing")
	}

	// Validate Root CA CRL
	if chain.RootCACRL == "" {
		return errors.New("Root CA CRL is missing")
	}

	return nil
}

// verifySGXQuote verifies an SGX quote using the Intel Attestation Service
func verifySGXQuote(quoteBytes []byte) (bool, error) {
	if len(quoteBytes) < 432 { // Minimum size for a valid quote
		return false, fmt.Errorf("quote data too short: %d bytes", len(quoteBytes))
	}

	// Parse quote body
	quote := &SGXQuoteBody{
		Version:   binary.LittleEndian.Uint16(quoteBytes[0:2]),
		SignType:  binary.LittleEndian.Uint16(quoteBytes[2:4]),
		Signature: quoteBytes[432:],
	}
	copy(quote.QESecurityID[:], quoteBytes[4:20])
	copy(quote.ISVSecurityID[:], quoteBytes[20:36])
	copy(quote.ReportData[:], quoteBytes[368:432])

	// Verify quote version
	if quote.Version != 2 && quote.Version != 3 {
		return false, fmt.Errorf("unsupported quote version: %d", quote.Version)
	}

	// Verify quote signature type (ECDSA-P256-SHA256)
	if quote.SignType != 2 {
		return false, fmt.Errorf("unsupported signature type: %d", quote.SignType)
	}

	// Get IAS API key from environment
	iasAPIKey := os.Getenv("IAS_API_KEY")
	if iasAPIKey == "" {
		return false, errors.New("IAS_API_KEY environment variable not set")
	}

	// Create IAS client
	iasClient := NewIASClient(iasAPIKey)

	// Send quote to IAS for verification
	iasResp, err := iasClient.VerifyQuote(quoteBytes)
	if err != nil {
		return false, fmt.Errorf("IAS verification failed: %v", err)
	}

	// Check quote status
	switch iasResp.ISVEnclaveQuoteStatus {
	case "OK":
		return true, nil
	case "GROUP_OUT_OF_DATE", "CONFIGURATION_NEEDED":
		// These statuses indicate the platform needs updates but is still trusted
		return true, nil
	case "GROUP_REVOKED", "SIGNATURE_INVALID", "INVALID_SIGNATURE":
		return false, fmt.Errorf("quote verification failed: %s", iasResp.ISVEnclaveQuoteStatus)
	default:
		return false, fmt.Errorf("unknown quote status: %s", iasResp.ISVEnclaveQuoteStatus)
	}
}
