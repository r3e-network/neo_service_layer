package tee

import "time"

// IASResponse represents a response from the Intel Attestation Service
type IASResponse struct {
	ID                    string    `json:"id"`
	Timestamp             time.Time `json:"timestamp"`
	Version               int       `json:"version"`
	ISVEnclaveQuoteStatus string    `json:"isvEnclaveQuoteStatus"`
	ISVEnclaveQuoteBody   string    `json:"isvEnclaveQuoteBody"`
	RevocationReason      *int      `json:"revocationReason,omitempty"`
	PSEManifestStatus     string    `json:"pseManifestStatus,omitempty"`
	PSEManifestHash       string    `json:"pseManifestHash,omitempty"`
	PlatformInfoBlob      string    `json:"platformInfoBlob,omitempty"`
	Nonce                 string    `json:"nonce,omitempty"`
	EpidPseudonym         string    `json:"epidPseudonym,omitempty"`
	AdvisoryURL           string    `json:"advisoryURL,omitempty"`
	AdvisoryIDs           []string  `json:"advisoryIDs,omitempty"`
}

// QuoteStatus constants for IAS response
const (
	QuoteStatusOK                         = "OK"
	QuoteStatusSignatureInvalid           = "SIGNATURE_INVALID"
	QuoteStatusGroupRevoked               = "GROUP_REVOKED"
	QuoteStatusSignatureRevoked           = "SIGNATURE_REVOKED"
	QuoteStatusKeyRevoked                 = "KEY_REVOKED"
	QuoteStatusConfigNeeded               = "CONFIGURATION_NEEDED"
	QuoteStatusConfigAndSWHardeningNeeded = "CONFIGURATION_AND_SW_HARDENING_NEEDED"
	QuoteStatusUnknown                    = "UNKNOWN"
)
