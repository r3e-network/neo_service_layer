package tee

import (
	"encoding/base64"
	"encoding/json"
	"errors"
	"fmt"
	"time"
)

// EnclaveType represents the type of enclave
type EnclaveType string

// Enclave types
const (
	SGXEnclave     EnclaveType = "sgx"
	TrustedVMEnclave EnclaveType = "trusted_vm"
	MockEnclave    EnclaveType = "mock"
)

// EnclaveStatus represents the status of an enclave
type EnclaveStatus string

// Enclave statuses
const (
	EnclaveInitializing EnclaveStatus = "initializing"
	EnclaveRunning      EnclaveStatus = "running"
	EnclaveStopped      EnclaveStatus = "stopped"
	EnclaveFailed       EnclaveStatus = "failed"
)

// EnclaveConfig represents the configuration for an enclave
type EnclaveConfig struct {
	Type          EnclaveType           `json:"type"`
	Memory        int                   `json:"memory"` // in MB
	CPU           int                   `json:"cpu"`    // number of cores
	Attestation   bool                  `json:"attestation"`
	Debug         bool                  `json:"debug"`
	Environment   map[string]string     `json:"environment"`
	MountPoints   []EnclaveMountPoint   `json:"mountPoints"`
	NetworkConfig EnclaveNetworkConfig  `json:"networkConfig"`
	Timeouts      EnclaveTimeoutConfig  `json:"timeouts"`
}

// EnclaveMountPoint represents a mount point for an enclave
type EnclaveMountPoint struct {
	Source      string `json:"source"`
	Destination string `json:"destination"`
	ReadOnly    bool   `json:"readOnly"`
}

// EnclaveNetworkConfig represents the network configuration for an enclave
type EnclaveNetworkConfig struct {
	Enabled       bool     `json:"enabled"`
	AllowedHosts  []string `json:"allowedHosts"`
	AllowedPorts  []int    `json:"allowedPorts"`
	ExposePort    bool     `json:"exposePort"`
	ExposedPort   int      `json:"exposedPort"`
	UseEncryption bool     `json:"useEncryption"`
}

// EnclaveTimeoutConfig represents timeout configuration for an enclave
type EnclaveTimeoutConfig struct {
	Startup   int `json:"startup"`   // in seconds
	Execution int `json:"execution"` // in seconds
	Shutdown  int `json:"shutdown"`  // in seconds
}

// EnclaveInfo represents information about an enclave
type EnclaveInfo struct {
	ID             string                 `json:"id"`
	Name           string                 `json:"name"`
	Type           EnclaveType            `json:"type"`
	Status         EnclaveStatus          `json:"status"`
	Config         EnclaveConfig          `json:"config"`
	Attestation    *AttestationEvidence   `json:"attestation,omitempty"`
	CreatedAt      time.Time              `json:"createdAt"`
	StartedAt      time.Time              `json:"startedAt,omitempty"`
	StoppedAt      time.Time              `json:"stoppedAt,omitempty"`
	Metrics        map[string]interface{} `json:"metrics,omitempty"`
	Error          string                 `json:"error,omitempty"`
}

// EnclaveInput represents input to an enclave
type EnclaveInput struct {
	FunctionID    string                 `json:"functionId"`
	FunctionName  string                 `json:"functionName"`
	Parameters    map[string]interface{} `json:"parameters"`
	Secrets       map[string]string      `json:"secrets,omitempty"`
	Timeout       int                    `json:"timeout,omitempty"` // in seconds
	MaxMemory     int                    `json:"maxMemory,omitempty"` // in MB
	RequireOutput bool                   `json:"requireOutput"`
}

// EnclaveOutput represents output from an enclave
type EnclaveOutput struct {
	FunctionID    string                 `json:"functionId"`
	FunctionName  string                 `json:"functionName"`
	Success       bool                   `json:"success"`
	Result        map[string]interface{} `json:"result,omitempty"`
	Error         string                 `json:"error,omitempty"`
	ExecutionTime int64                  `json:"executionTime"` // in ms
	MemoryUsage   int                    `json:"memoryUsage"`   // in bytes
	CPUUsage      float64                `json:"cpuUsage"`      // percentage
	Attestation   string                 `json:"attestation,omitempty"`
}

// Enclave represents an enclave for secure execution
type Enclave struct {
	info   EnclaveInfo
	config EnclaveConfig
}

// NewEnclave creates a new enclave
func NewEnclave(name string, config EnclaveConfig) (*Enclave, error) {
	// Validate config
	if config.Type == "" {
		return nil, errors.New("enclave type is required")
	}

	if config.Memory <= 0 {
		config.Memory = 128 // Default to 128MB
	}

	if config.CPU <= 0 {
		config.CPU = 1 // Default to 1 core
	}

	// Create enclave
	enclave := &Enclave{
		info: EnclaveInfo{
			ID:        fmt.Sprintf("enclave-%d", time.Now().UnixNano()),
			Name:      name,
			Type:      config.Type,
			Status:    EnclaveInitializing,
			Config:    config,
			CreatedAt: time.Now(),
			Metrics:   make(map[string]interface{}),
		},
		config: config,
	}

	return enclave, nil
}

// Start starts the enclave
func (e *Enclave) Start() error {
	// Check if the enclave is already running
	if e.info.Status == EnclaveRunning {
		return errors.New("enclave is already running")
	}

	// Start the enclave
	e.info.Status = EnclaveRunning
	e.info.StartedAt = time.Now()
	e.info.StoppedAt = time.Time{}
	e.info.Error = ""

	// Generate attestation if required
	if e.config.Attestation {
		attestation, err := e.generateAttestation()
		if err != nil {
			e.info.Status = EnclaveFailed
			e.info.Error = err.Error()
			return fmt.Errorf("failed to generate attestation: %w", err)
		}
		e.info.Attestation = attestation
	}

	return nil
}

// Stop stops the enclave
func (e *Enclave) Stop() error {
	// Check if the enclave is already stopped
	if e.info.Status == EnclaveStopped {
		return errors.New("enclave is already stopped")
	}

	// Stop the enclave
	e.info.Status = EnclaveStopped
	e.info.StoppedAt = time.Now()

	return nil
}

// Execute executes a function in the enclave
func (e *Enclave) Execute(input EnclaveInput) (*EnclaveOutput, error) {
	// Check if the enclave is running
	if e.info.Status != EnclaveRunning {
		return nil, errors.New("enclave is not running")
	}

	// Mock execution
	startTime := time.Now()
	output := &EnclaveOutput{
		FunctionID:    input.FunctionID,
		FunctionName:  input.FunctionName,
		Success:       true,
		Result:        make(map[string]interface{}),
		ExecutionTime: 0,
		MemoryUsage:   1024 * 1024, // 1MB
		CPUUsage:      5.0,         // 5%
	}

	// Simulate execution time
	time.Sleep(100 * time.Millisecond)

	// Update metrics
	e.info.Metrics["lastExecutionTime"] = time.Now()
	e.info.Metrics["totalExecutions"] = e.info.Metrics["totalExecutions"].(int) + 1
	e.info.Metrics["lastMemoryUsage"] = output.MemoryUsage
	e.info.Metrics["lastCPUUsage"] = output.CPUUsage

	// Update output
	output.ExecutionTime = time.Since(startTime).Milliseconds()

	// Add sample output result
	output.Result["message"] = fmt.Sprintf("Successfully executed %s in enclave", input.FunctionName)
	output.Result["timestamp"] = time.Now().Unix()
	output.Result["parameters"] = input.Parameters

	// Add attestation if required
	if e.config.Attestation && e.info.Attestation != nil {
		attestationStr, err := SerializeEvidence(e.info.Attestation)
		if err == nil {
			output.Attestation = attestationStr
		}
	}

	return output, nil
}

// GetInfo returns information about the enclave
func (e *Enclave) GetInfo() EnclaveInfo {
	return e.info
}

// generateAttestation generates attestation evidence for the enclave
func (e *Enclave) generateAttestation() (*AttestationEvidence, error) {
	return generateMockEvidence(), nil
}

// EnclaveManager manages enclaves
type EnclaveManager struct {
	enclaves map[string]*Enclave
	policy   SecurityPolicy
}

// NewEnclaveManager creates a new enclave manager
func NewEnclaveManager(policy SecurityPolicy) *EnclaveManager {
	return &EnclaveManager{
		enclaves: make(map[string]*Enclave),
		policy:   policy,
	}
}

// CreateEnclave creates a new enclave
func (m *EnclaveManager) CreateEnclave(name string, config EnclaveConfig) (string, error) {
	// Create enclave
	enclave, err := NewEnclave(name, config)
	if err != nil {
		return "", err
	}

	// Add enclave to manager
	m.enclaves[enclave.info.ID] = enclave

	return enclave.info.ID, nil
}

// GetEnclave gets an enclave by ID
func (m *EnclaveManager) GetEnclave(id string) (*Enclave, error) {
	enclave, exists := m.enclaves[id]
	if !exists {
		return nil, errors.New("enclave not found")
	}
	return enclave, nil
}

// StartEnclave starts an enclave
func (m *EnclaveManager) StartEnclave(id string) error {
	enclave, err := m.GetEnclave(id)
	if err != nil {
		return err
	}
	return enclave.Start()
}

// StopEnclave stops an enclave
func (m *EnclaveManager) StopEnclave(id string) error {
	enclave, err := m.GetEnclave(id)
	if err != nil {
		return err
	}
	return enclave.Stop()
}

// DestroyEnclave destroys an enclave
func (m *EnclaveManager) DestroyEnclave(id string) error {
	enclave, err := m.GetEnclave(id)
	if err != nil {
		return err
	}

	// Stop the enclave if it's running
	if enclave.info.Status == EnclaveRunning {
		if err := enclave.Stop(); err != nil {
			return err
		}
	}

	// Remove enclave from manager
	delete(m.enclaves, id)

	return nil
}

// ListEnclaves lists all enclaves
func (m *EnclaveManager) ListEnclaves() []EnclaveInfo {
	var enclaves []EnclaveInfo
	for _, enclave := range m.enclaves {
		enclaves = append(enclaves, enclave.info)
	}
	return enclaves
}

// ExecuteInEnclave executes a function in an enclave
func (m *EnclaveManager) ExecuteInEnclave(id string, input EnclaveInput) (*EnclaveOutput, error) {
	enclave, err := m.GetEnclave(id)
	if err != nil {
		return nil, err
	}
	return enclave.Execute(input)
}

// SerializeEnclaveInput serializes enclave input to JSON
func SerializeEnclaveInput(input EnclaveInput) (string, error) {
	data, err := json.Marshal(input)
	if err != nil {
		return "", err
	}
	return base64.StdEncoding.EncodeToString(data), nil
}

// DeserializeEnclaveInput deserializes enclave input from JSON
func DeserializeEnclaveInput(inputStr string) (*EnclaveInput, error) {
	data, err := base64.StdEncoding.DecodeString(inputStr)
	if err != nil {
		return nil, err
	}

	var input EnclaveInput
	if err := json.Unmarshal(data, &input); err != nil {
		return nil, err
	}
	return &input, nil
}

// SerializeEnclaveOutput serializes enclave output to JSON
func SerializeEnclaveOutput(output EnclaveOutput) (string, error) {
	data, err := json.Marshal(output)
	if err != nil {
		return "", err
	}
	return base64.StdEncoding.EncodeToString(data), nil
}

// DeserializeEnclaveOutput deserializes enclave output from JSON
func DeserializeEnclaveOutput(outputStr string) (*EnclaveOutput, error) {
	data, err := base64.StdEncoding.DecodeString(outputStr)
	if err != nil {
		return nil, err
	}

	var output EnclaveOutput
	if err := json.Unmarshal(data, &output); err != nil {
		return nil, err
	}
	return &output, nil
}