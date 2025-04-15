package secrets

import (
	"context"
	"encoding/base64"
	"encoding/json"
	"errors"
	"fmt"
	"net"
	"time"

	"github.com/mdlayher/vsock"
	log "github.com/sirupsen/logrus"
)

// NitroTEESecurityProvider implements TEESecurityProvider for AWS Nitro Enclaves via vsock.
type NitroTEESecurityProvider struct {
	enclaveCID  uint32
	enclavePort uint32
	// TODO: Add connection pooling or persistent connection logic if needed
}

// NitroCommand defines the structure for commands sent to the enclave.
type NitroCommand struct {
	Command string `json:"command"` // "encrypt" or "decrypt"
	Data    string `json:"data"`    // Base64 encoded data
}

// NitroResponse defines the structure for responses from the enclave.
type NitroResponse struct {
	Status  string `json:"status"`            // "ok" or "error"
	Data    string `json:"data,omitempty"`    // Base64 encoded result (if status is "ok")
	Message string `json:"message,omitempty"` // Error message (if status is "error")
}

// NewNitroTEESecurityProvider creates a new Nitro Enclaves provider.
func NewNitroTEESecurityProvider(cid, port uint32) (*NitroTEESecurityProvider, error) {
	if cid == 0 || port == 0 {
		return nil, errors.New("invalid Nitro Enclave CID or Port provided")
	}
	log.Infof("Initializing NitroTEESecurityProvider for CID: %d, Port: %d", cid, port)
	return &NitroTEESecurityProvider{
		enclaveCID:  cid,
		enclavePort: port,
	}, nil
}

// Encrypt sends data to the Nitro Enclave for encryption.
func (n *NitroTEESecurityProvider) Encrypt(ctx context.Context, plaintext []byte) ([]byte, error) {
	cmd := NitroCommand{
		Command: "encrypt",
		Data:    base64.StdEncoding.EncodeToString(plaintext),
	}
	resp, err := n.sendCommandToEnclave(ctx, cmd)
	if err != nil {
		return nil, fmt.Errorf("failed to send encrypt command to enclave: %w", err)
	}
	return resp, nil
}

// Decrypt sends data to the Nitro Enclave for decryption.
func (n *NitroTEESecurityProvider) Decrypt(ctx context.Context, ciphertext []byte) ([]byte, error) {
	cmd := NitroCommand{
		Command: "decrypt",
		Data:    base64.StdEncoding.EncodeToString(ciphertext),
	}
	resp, err := n.sendCommandToEnclave(ctx, cmd)
	if err != nil {
		return nil, fmt.Errorf("failed to send decrypt command to enclave: %w", err)
	}
	return resp, nil
}

// sendCommandToEnclave handles the vsock communication.
func (n *NitroTEESecurityProvider) sendCommandToEnclave(ctx context.Context, cmd NitroCommand) ([]byte, error) {
	log.Debugf("Sending command '%s' to Nitro Enclave (CID: %d, Port: %d)", cmd.Command, n.enclaveCID, n.enclavePort)

	// Establish vsock connection (consider pooling/reuse for performance)
	// Add appropriate timeouts for dialing
	dialer := &net.Dialer{Timeout: 5 * time.Second}                          // Example timeout
	conn, err := vsock.DialContext(ctx, n.enclaveCID, n.enclavePort, dialer) // Use DialContext
	if err != nil {
		log.Errorf("Failed to dial Nitro Enclave vsock (CID: %d, Port: %d): %v", n.enclaveCID, n.enclavePort, err)
		return nil, fmt.Errorf("failed to dial enclave vsock: %w", err)
	}
	defer conn.Close()

	// Set read/write deadlines based on context or default timeout
	_ = conn.SetDeadline(time.Now().Add(10 * time.Second)) // Example fixed deadline

	// Encode command to JSON
	encoder := json.NewEncoder(conn)
	if err := encoder.Encode(cmd); err != nil {
		log.Errorf("Failed to send command to enclave: %v", err)
		return nil, fmt.Errorf("failed to encode/send command: %w", err)
	}

	// Decode response from JSON
	var resp NitroResponse
	decoder := json.NewDecoder(conn)
	if err := decoder.Decode(&resp); err != nil {
		log.Errorf("Failed to receive/decode response from enclave: %v", err)
		return nil, fmt.Errorf("failed to decode/receive response: %w", err)
	}

	// Check response status
	if resp.Status != "ok" {
		log.Errorf("Enclave returned error: %s", resp.Message)
		return nil, fmt.Errorf("enclave error: %s", resp.Message)
	}

	// Decode base64 data
	resultData, err := base64.StdEncoding.DecodeString(resp.Data)
	if err != nil {
		log.Errorf("Failed to decode base64 response data from enclave: %v", err)
		return nil, fmt.Errorf("failed to decode response data: %w", err)
	}

	log.Debugf("Received successful response for command '%s' from Nitro Enclave", cmd.Command)
	return resultData, nil
}
