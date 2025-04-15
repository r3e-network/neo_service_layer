# Secure Function Execution in TEE

*Last Updated: 2025-04-14*

## Overview

The Neo N3 Service Layer provides a secure function execution framework based on Trusted Execution Environments (TEEs). This document details how user-defined functions are securely executed within TEEs, protecting both the confidentiality and integrity of the code and data.

## Execution Flow

The secure function execution process involves several components and follows these steps:

1. **Function Registration**: User-defined functions are registered with the Functions Service
2. **Function Verification**: Code is analyzed for security and compatibility
3. **TEE Loading**: Function code is securely loaded into a TEE enclave
4. **Execution Scheduling**: Automation Service triggers function execution based on conditions
5. **Secure Execution**: Function executes in the isolated TEE environment
6. **Result Processing**: Execution results are securely returned and processed
7. **Transaction Submission**: If required, transactions are signed and sent to the blockchain

## TEE Isolation Guarantees

Functions executed within the TEE benefit from several security guarantees:

- **Memory Isolation**: TEE memory is isolated from the host system and other TEEs
- **Runtime Protection**: The TEE runtime is protected by hardware mechanisms
- **Code Confidentiality**: Function code cannot be viewed by the host system
- **Execution Integrity**: Function execution cannot be tampered with
- **Attestation**: TEE can provide cryptographic proof of the code being executed
- **Secure Input/Output**: Data entering and leaving the TEE is protected

## Function Lifecycle

### 1. Development

Developers create functions using supported languages and frameworks, adhering to TEE compatibility requirements. Functions may be written in:

- WebAssembly (WASM) - preferred for cross-platform compatibility
- Native code with TEE-specific protections
- Go with TEE runtime support

Functions must follow specific resource usage patterns and security guidelines to be TEE-compatible.

### 2. Registration

Functions are registered through the Functions Service API, which:

- Validates function code for TEE compatibility
- Generates a unique function identifier
- Securely stores the function code
- Creates a deployment manifest with metadata

### 3. Deployment

When a function is ready for execution, the deployment process:

- Prepares the TEE environment
- Loads the function code into the TEE enclave
- Verifies the integrity of the loaded code
- Initializes any required runtime components

### 4. Execution

Function execution is triggered by:

- Scheduled time-based events
- On-chain events
- API calls with proper authorization

The execution sequence involves:

1. TEE enclave is initialized with function code
2. Input parameters are securely provided to the enclave
3. Function executes in the isolated environment
4. Results are securely returned from the enclave
5. Attestation data can be generated to prove execution correctness

## Security Model

### Threat Mitigation

The TEE-based execution model mitigates several threats:

| Threat | Mitigation |
|--------|------------|
| Code Disclosure | Code runs in TEE with memory encryption |
| Data Theft | User data protected by TEE memory isolation |
| Execution Tampering | Hardware-enforced execution integrity |
| Supply Chain Attacks | Attestation verifies the correct code is running |
| Side-channel Attacks | TEE-specific protections against timing and cache attacks |
| Host OS Compromise | TEE isolation from host system |

### Key Management

Secure function execution relies on a robust key management framework:

- **TEE Identity Keys**: Each TEE has a unique identity key for attestation
- **Function Signing Keys**: Functions can be signed to verify their authenticity
- **Blockchain Keys**: For on-chain operations, protected by the TEE
- **Data Encryption Keys**: For secure data storage and transmission

All keys used within the TEE are protected by hardware security features and never exposed in plaintext outside the TEE.

## TEE Runtime Environment

The TEE Runtime provides a secure execution environment with:

1. **Memory Management**: Secure allocation, protection, and cleanup
2. **I/O Interface**: Controlled and encrypted data channels
3. **Crypto Services**: Hardware-backed cryptographic operations
4. **Secure Storage**: Protected persistent storage for function state
5. **Blockchain Interface**: Secure interaction with the Neo N3 blockchain

## Function Capabilities

Functions executing in the TEE can securely:

- Process confidential data
- Make cryptographically signed blockchain calls
- Access hardware-backed key operations via the Secrets Service
- Interact with other Neo N3 services through authenticated channels
- Maintain persistent state with confidentiality guarantees
- Generate verifiable results with attestation

## Integration Points

The secure function execution framework integrates with:

- **Automation Service**: For scheduling and triggering function execution
- **Secrets Service**: For secure key operations
- **Gas Bank Service**: For managing blockchain transaction costs
- **Metrics Service**: For monitoring execution performance and health

## Performance Considerations

TEE-based execution introduces some overhead:

- Memory copy operations between host and TEE
- Context switches for TEE entry/exit
- Attestation verification
- Encryption/decryption of I/O data

The framework implements several optimizations:

- Batching operations to reduce TEE transitions
- Caching frequently used data within the TEE
- Pre-warming TEE instances for common functions
- Resource pooling for TEE instances

## Development Guidelines

When developing functions for TEE execution:

1. **Minimize TEE Exits**: Each exit from the TEE is expensive and reduces security benefits
2. **Limit Resource Usage**: TEEs have restricted memory and compute resources
3. **Follow Secure Coding Practices**: Avoid patterns that could leak sensitive data
4. **Use Provided TEE APIs**: For secure I/O, crypto, and blockchain operations
5. **Validate All Inputs**: Even in a TEE, input validation is essential
6. **Manage State Carefully**: Persistent state should be encrypted
7. **Consider Side Channels**: Be aware of timing and memory access patterns

## Deployment Architecture

The deployment architecture supports:

- **Multiple TEE Technologies**: Intel SGX, AWS Nitro, AMD SEV
- **Scalable Execution**: Horizontal scaling of TEE instances
- **High Availability**: Redundant TEE pools for reliability
- **Secure Updates**: Mechanisms for safely updating function code
- **Monitoring**: TEE-aware health and performance monitoring

## Example Execution Flow

```
┌──────────────────┐         ┌───────────────────┐         ┌──────────────────┐
│  Function Owner  │         │  Automation Svc   │         │  Functions Svc   │
└────────┬─────────┘         └─────────┬─────────┘         └────────┬─────────┘
         │                             │                             │
         │  Register Function          │                             │
         │ ────────────────────────────────────────────────────────>│
         │                             │                             │
         │  Function ID                │                             │
         │ <────────────────────────────────────────────────────────│
         │                             │                             │
         │  Create Automation Job      │                             │
         │ ────────────────────────────>                             │
         │                             │                             │
         │  Job Created                │                             │
         │ <────────────────────────────                             │
         │                             │                             │
         │                             │  Trigger Detected           │
         │                             │ ─────────────────┐          │
         │                             │                  │          │
         │                             │                  V          │
         │                             │  Request Function Execution │
         │                             │ ────────────────────────────>
         │                             │                             │
         │                             │                             │  ┌──────────────┐
         │                             │                             │  │ TEE Enclave  │
         │                             │                             │──┼───────────┐  │
         │                             │                             │  │           │  │
         │                             │                             │  │  Execute  │  │
         │                             │                             │  │  Function │  │
         │                             │                             │  │           │  │
         │                             │                             │  │           │  │
         │                             │                             │<─┼───────────┘  │
         │                             │                             │  └──────────────┘
         │                             │                             │
         │                             │  Execution Results          │
         │                             │ <────────────────────────────
         │                             │                             │
         │                             │  Update Status              │
         │                             │ ─────────────────┐          │
         │                             │                  │          │
         │                             │                  V          │
┌────────────────────┐                 │                             │
│  Neo N3 Blockchain │                 │                             │
└──────────┬─────────┘                 │                             │
           │                           │                             │
           │  Submit Transaction       │                             │
           │ <──────────────────────────                             │
           │                           │                             │
           │  Transaction Confirmed    │                             │
           │ ──────────────────────────>                             │
           │                           │                             │
         │                             │  Job Execution Complete     │
         │ <────────────────────────────                             │
         │                             │                             │
```

## Security Considerations and Best Practices

1. **Attestation Verification**: Always verify TEE attestation before sending sensitive data
2. **Input Validation**: Validate all inputs even within the TEE
3. **Secure Updates**: Use secure update mechanisms with integrity verification
4. **Resource Isolation**: Set appropriate memory and CPU limits for functions
5. **Logging**: Ensure logs do not contain sensitive information
6. **Error Handling**: Implement secure error handling that doesn't leak information
7. **Monitoring**: Monitor TEE health and detect anomalies

## Related Documentation

- [TEE Service Architecture](ARCHITECTURE.md): Detailed architecture of the TEE service
- [Function Development Guide](../functionservice/DEVELOPMENT.md): Guide for developing TEE-compatible functions
- [Automation Integration](../automationservice/INTEGRATION.md): How to trigger functions via the Automation Service
- [Security Model](SECURITY_MODEL.md): Detailed security assumptions and threat model
