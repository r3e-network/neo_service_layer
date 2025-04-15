# TEE-Based Secrets Backend

*Last Updated: 2025-04-14*

## Overview

The TEE-Based Secrets Backend provides a high-security implementation for the Neo Service Layer Secrets Service that operates entirely within a Trusted Execution Environment (TEE). This backend leverages hardware-backed security guarantees to ensure that sensitive cryptographic material and operations are protected from all external observation, including from privileged system software such as the operating system or hypervisor.

The current implementation supports two TEE technologies:
- **AWS Nitro Enclaves** for deployments on AWS
- **Azure Confidential Computing** (using Intel SGX or AMD SEV-SNP) for deployments on Azure

## Architecture

The TEE-based backend operates using a layered security model:

```
┌─────────────────────────────────────────────────────────────┐
│                    Trusted Execution Environment             │
│                                                             │
│  ┌──────────────┐    ┌──────────────┐    ┌───────────────┐  │
│  │ API Gateway  │━━━▶│ Core Service │━━━▶│ Sealed Storage│  │
│  └──────────────┘    └──────────────┘    └───────────────┘  │
│         ▲                    │                   ▲          │
│         │                    ▼                   │          │
│         │            ┌──────────────┐            │          │
│         └────────────│Cryptographic │━━━━━━━━━━━━━┘          │
│                      │  Operations  │                       │
│                      └──────────────┘                       │
│                             │                              │
│                             ▼                              │
│                     ┌──────────────┐                       │
│                     │ TEE-to-TEE   │                       │
│                     │ Secure Comms │                       │
│                     └──────────────┘                       │
└─────────────────────────────────────────────────────────────┘
         ▲                                        ▲
         │                                        │
┌────────────────┐                      ┌──────────────────┐
│  External API  │                      │ Other Services   │
│  Requests      │                      │ in TEE           │
└────────────────┘                      └──────────────────┘
```

### Key Components

1. **API Gateway**: A minimized interface that validates incoming requests and forwards them to the core service. It runs within the TEE.

2. **Core Service**: The main service logic, implementing secret management operations while running inside the TEE.

3. **Sealed Storage**: A mechanism for securely storing encrypted secrets that are sealed to the TEE identity, ensuring they can only be decrypted by the same TEE environment.

4. **Cryptographic Operations**: Hardware-backed cryptographic operations performed exclusively within the TEE.

5. **TEE-to-TEE Secure Communications**: Authenticated and encrypted channels for communication between different services running in separate TEEs.

## TEE Protection Model

The TEE-based backend provides the following security guarantees:

1. **Memory Encryption**: All memory used by the Secrets Service is encrypted by the TEE hardware.

2. **Execution Isolation**: The service runs in an isolated environment, protected from the host OS.

3. **Attestation**: Cryptographic proof of the service's identity and integrity through TEE attestation.

4. **Key Sealing**: Cryptographic keys are sealed to the TEE's identity, ensuring they can only be used within the genuine TEE.

5. **Secure Channels**: All communication with other services uses attested, encrypted channels.

## Implementation Details

### AWS Nitro Enclaves Implementation

When deployed on AWS, the Secrets Service runs entirely within a Nitro Enclave with the following characteristics:

1. **Enclave Configuration**:
   - Dedicated CPU cores and memory allocation
   - No network access except through the parent instance
   - No persistent storage
   - Communication via VSOCK channels

2. **Attestation Process**:
   - Nitro Security Module (NSM) generates attestation documents
   - PCR measurements verify enclave code integrity
   - Attestation documents include cryptographic proof of the enclave identity

3. **Storage Implementation**:
   - Secrets are encrypted within the enclave
   - Encrypted data is stored in an external database via the parent instance
   - Encryption keys never leave the enclave

4. **Cryptographic Operations**:
   - All operations use AWS Nitro Cryptographic Extensions
   - Hardware-backed key generation and management
   - Signing operations performed inside the enclave

### Azure Confidential Computing Implementation

When deployed on Azure, the Secrets Service uses either Intel SGX or AMD SEV-SNP:

1. **Enclave Types**:
   - SGX Enclaves for fine-grained process-level isolation
   - Confidential VMs using AMD SEV-SNP for VM-level isolation

2. **Attestation Process**:
   - SGX DCAP or Azure Attestation Service for verifying enclave identity
   - Remote attestation with Azure Attestation for establishing trust
   - Quote verification to validate enclave measurements

3. **Storage Implementation**:
   - Sealed data protected by hardware-derived keys
   - External database for encrypted storage
   - Key hierarchy with root keys protected by hardware

4. **Cryptographic Operations**:
   - SGX-specific crypto operations for enclaves
   - AMD SEV secure key handling for Confidential VMs
   - Hardware-backed random number generation

## Integration with Other Services

The TEE-based backend securely integrates with other services also running in TEEs:

### TEE-to-TEE Secure Channels

1. **Mutual Attestation Protocol**:
   - Both TEEs generate attestation evidence
   - Each TEE verifies the other's attestation
   - Establishes a secure identity-bound channel

2. **Channel Establishment**:
   ```
   ┌───────────────┐                 ┌───────────────┐
   │               │                 │               │
   │ Secrets TEE   │                 │ Function TEE  │
   │               │                 │               │
   └───────┬───────┘                 └───────┬───────┘
           │                                 │
           │ 1. Request Channel              │
           │                                 │
           │ 2. Generate Nonce               │
           │                                 │
           │ 3. Attestation Request          │
           │ ───────────────────────────────►│
           │                                 │
           │                                 │ 4. Generate
           │                                 │ Attestation
           │                                 │
           │ 5. Attestation Evidence         │
           │ ◄─────────────────────────────── │
           │                                 │
           │ 6. Verify Attestation           │
           │                                 │
           │ 7. Generate Own Attestation     │
           │                                 │
           │ 8. Send Own Attestation         │
           │ ───────────────────────────────►│
           │                                 │
           │                                 │ 9. Verify
           │                                 │ Attestation
           │                                 │
           │ 10. Session Keys                │
           │ ◄─────────────────────────────── │
           │                                 │
           │ 11. Secure Channel Established  │
           │ ═════════════════════════════════│
           │                                 │
   ```

3. **Secure Channel Properties**:
   - Perfect forward secrecy
   - Hardware-backed identity binding
   - Authenticated encryption of all messages
   - Session key rotation

### Function Service Integration

The Secrets Service provides secure access to secrets for functions running in TEEs:

1. **Secret Access Workflow**:
   - Function requests secret access with attestation evidence
   - Secrets Service verifies the function's TEE attestation
   - Access policy is evaluated based on function identity
   - If authorized, secret is transmitted over secure channel
   - Secrets Service logs the access for audit purposes

2. **Example Secret Access Flow**:
   ```
   ┌───────────────┐                 ┌───────────────┐
   │               │                 │               │
   │ Function TEE  │                 │ Secrets TEE   │
   │               │                 │               │
   └───────┬───────┘                 └───────┬───────┘
           │                                 │
           │ 1. Secret Access Request        │
           │ with Attestation Evidence       │
           │ ───────────────────────────────►│
           │                                 │
           │                                 │ 2. Verify Attestation
           │                                 │ 3. Check Access Policy
           │                                 │ 4. Retrieve Secret
           │                                 │
           │ 5. Encrypted Secret             │
           │ ◄─────────────────────────────── │
           │                                 │
           │ 6. Use Secret                   │
           │ (within TEE only)               │
           │                                 │
   ```

### TEE-based Cryptographic Operations

The Secrets Service provides cryptographic operations as a service to other TEEs:

1. **Remote Signing**:
   - Function TEE sends data to be signed
   - Secrets TEE performs signature using protected keys
   - Signature is returned to the function TEE
   - Keys never leave the Secrets Service TEE

2. **Secure Verification**:
   - Verification operations can be performed inside the Secrets Service TEE
   - Protects sensitive verification logic and data

## Configuration Options

The TEE-based backend is configured through the following settings:

```yaml
backend:
  type: "tee"
  tee:
    provider: "aws_nitro"  # Or "azure_cc"
    attestation:
      verification_service: "https://attestation.example.com"
      allowed_measurements: ["measurement1", "measurement2"]
    storage:
      database_uri: "${SECRETS_SERVICE_DB_URI}"
      encryption_scheme: "AES-GCM-256"
    azure_cc:  # Azure-specific settings
      attestation_url: "https://myattestation.azure.net"
      tee_type: "sgx"  # Or "sevsnp"
    aws_nitro:  # AWS-specific settings
      parent_communication_socket: "vsock://:5000"
      enclave_cpu_count: 2
      enclave_memory_mb: 4096
    secure_channel:
      key_exchange: "ECDHE-P384"
      cipher: "AES-256-GCM"
      key_rotation_interval_sec: 3600
```

## Key Management

### Key Hierarchy

The TEE-based backend implements a robust key hierarchy:

1. **Root Key**: Derived from the TEE hardware identity
2. **Master Encryption Key (MEK)**: Protected by the Root Key
3. **Data Encryption Keys (DEKs)**: Protected by the MEK
4. **User Keys**: Protected by DEKs

### Key Rotation

The backend supports secure key rotation:

1. **DEK Rotation**: New DEKs are generated periodically
2. **MEK Rotation**: Master key is rotated on a schedule
3. **Root Key Continuity**: Root key changes require data migration

## Security Considerations

### Side-Channel Protection

The TEE-based backend implements multiple protections against side-channel attacks:

1. **Constant-time Operations**: Cryptographic operations use constant-time algorithms
2. **Memory Access Patterns**: Uses techniques to hide memory access patterns
3. **Cache Side-channel Mitigation**: Cache access patterns are masked

### Attestation Security

1. **Measurement Allowlisting**: Only known-good code measurements are accepted
2. **Quote Freshness**: Nonces ensure attestation quotes cannot be replayed
3. **Regular Measurement Updates**: Code measurements are updated with patches

## Operational Guidelines

### Deploying in AWS

For AWS Nitro Enclaves deployment:

1. Create an EC2 instance with Nitro Enclaves enabled
2. Build the Secrets Service enclave image
3. Deploy and start the enclave
4. Configure other services to communicate with the enclave

### Deploying in Azure

For Azure Confidential Computing deployment:

1. Create a Confidential VM with appropriate TEE technology
2. Deploy the Secrets Service with SGX or SEV-SNP support
3. Configure Azure Attestation Service
4. Set up secure communication with other services

## Monitoring and Auditing

The TEE-based backend provides comprehensive monitoring while preserving confidentiality:

1. **Health Metrics**: Non-sensitive operational metrics
2. **Audit Logs**: Encrypted logs of all access requests
3. **Attestation Monitoring**: Tracking of attestation success/failure
4. **Performance Metrics**: Execution time and resource utilization

## Troubleshooting

Common issues and solutions:

1. **Attestation Failures**:
   - Verify measurements match expected values
   - Check connectivity to attestation services
   - Ensure TEE is properly configured

2. **Performance Issues**:
   - Adjust TEE resource allocation
   - Optimize database access patterns
   - Configure caching appropriately

3. **Integration Problems**:
   - Verify mutual attestation setup
   - Check network connectivity between TEEs
   - Ensure correct service endpoints

## Related Documentation

- [Secrets Service Architecture](../ARCHITECTURE.md)
- [TEE Overview](../../tee/TEE_OVERVIEW.md)
- [Attestation Process](../../tee/ATTESTATION.md)
- [Function Service TEE Integration](../../functionservice/TEE_INTEGRATION.md)

> **Note:** This service was previously also documented as the Key Service. Both refer to the same service functionality. 