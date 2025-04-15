# Trusted Execution Environment (TEE) Developer Guide

*Last Updated: 2023-12-20*

## Overview

This guide provides developers with comprehensive guidance for building applications and services that leverage Trusted Execution Environments (TEEs) within the Neo Service Layer. TEEs provide hardware-backed security guarantees for sensitive operations, ensuring code and data confidentiality and integrity.

## What is a TEE?

A Trusted Execution Environment (TEE) is a secure area within a processor that ensures:

1. **Code Integrity**: Only authorized code can execute within the environment
2. **Data Confidentiality**: Data processed within the TEE is protected from external observation
3. **Data Integrity**: Data cannot be tampered with by unauthorized entities
4. **Attestation**: The ability to cryptographically prove the TEE's identity and state

## Supported TEE Technologies

The Neo Service Layer supports multiple TEE technologies:

1. **AWS Nitro Enclaves**
   - Isolated VM-based compute environments
   - Hardware-based attestation using the Nitro Security Module (NSM)
   - Completely isolated from the host EC2 instance with no persistent storage
   - Communication via secure local VSOCK channels
   - Used primarily for Function Service when deployed on AWS

2. **Intel SGX (Software Guard Extensions)**
   - Hardware-based memory encryption
   - Fine-grained enclave management
   - Uses the SGX SDK for enclave development and management
   - Employs the DCAP library for attestation verification
   - Custom runtime development using the SGX SDK
   - Encrypted memory pages managed by the processor
   - Used for Secrets Service and other security-critical components

3. **AMD SEV (Secure Encrypted Virtualization)**
   - VM memory encryption
   - VM-level isolation with encrypted state
   - Attestation through the AMD Key Management Server (KMS)
   - Integration with the AMD SEV driver and libraries
   - VM templates optimized for SEV compatibility
   - Used as an alternative for Function Service in supported environments

## Architecture Overview

### TEE Integration Points

TEEs are integrated into multiple services:

```
┌─────────────────┐     ┌────────────────┐     ┌────────────────┐
│ Function Service│     │ Secrets Service    │     │ Automation     │
│                 │     │                │     │ Service        │
│ ┌─────────────┐ │     │ ┌────────────┐ │     │ ┌────────────┐ │
│ │ TEE Runtime │ │     │ │TEE Key Ops │ │     │ │TEE Trigger │ │
│ └─────────────┘ │     │ └────────────┘ │     │ │Verification │ │
└─────────────────┘     └────────────────┘     │ └────────────┘ │
                                               └────────────────┘
```

### Security Boundaries

Understanding TEE security boundaries is critical:

1. **Inside TEE**: Protected from external observation and tampering
   - Encryption keys
   - User function code
   - Sensitive transaction data
   - Authentication tokens
   - Function code (confidentiality and integrity)
   - Function inputs (when marked as sensitive)
   - Secrets injected by the Secrets Service
   - Runtime environment during execution
   - Computation results before they're returned

2. **Outside TEE**: Potentially observable
   - Configuration data
   - Non-sensitive metrics
   - Public blockchain data
   - Non-sensitive log data

### Memory Protection

- Physical memory pages allocated to a TEE are encrypted with keys only accessible to the TEE
- Memory access controls prevent external processes from reading or modifying TEE memory
- Memory is scrubbed when deallocated to prevent data leakage
- TEE memory is isolated from the host OS and other processes

## Attestation Process

Attestation is the process by which a TEE proves its identity and integrity to a remote party, providing verifiable proof that code is running in a genuine TEE with the expected configuration.

For detailed information on the attestation process, see [ATTESTATION.md](ATTESTATION.md).

## Working with TEE-Based Functions

### Function Lifecycle

1. **Registration**: Function code is registered with the Function Service
2. **Validation**: Code is validated for TEE compatibility
3. **Deployment**: Function is deployed to the TEE runtime
4. **Execution**: Function executes in the TEE when triggered
5. **Termination**: Function execution completes and results are securely returned

### Writing TEE-Compatible Functions

When developing functions for TEE execution, consider these guidelines:

1. **Deterministic Behavior**: Functions should be deterministic for predictable behavior
2. **Resource Constraints**: TEEs often have memory and CPU constraints
3. **Minimized Dependencies**: Reduce external dependencies to minimize attack surface
4. **Secure I/O**: All function I/O should be properly validated and sanitized
5. **Secrets Management**: Use the Secrets Service for accessing sensitive data

### Function Input/Output

Function I/O follows these security principles:

1. **Input Validation**: All inputs are validated before processing
2. **Output Encryption**: Sensitive outputs are encrypted before leaving the TEE
3. **Secure Channels**: I/O uses secure channels between services
4. **Signed Results**: Function outputs are signed by the TEE for verification

## Security Best Practices

When developing with TEEs, follow these security practices:

1. **Trust Minimization**: Minimize code running inside the TEE to reduce attack surface
2. **Secure Communication**: Use attested TLS for communication with the TEE
3. **Secret Management**: Never hardcode secrets in function code
4. **Side-Channel Awareness**: Be aware of potential side-channel attacks
5. **Attestation Verification**: Always verify attestation quotes before trusting a TEE
6. **Updated TEE Components**: Keep TEE frameworks and SDKs updated
7. **Secure Dependencies**: Carefully review all dependencies used in TEE code

## Monitoring and Audit

All TEE operations should be monitored:

1. **Attestation Logs**: Log all attestation requests and verification results
2. **Function Execution**: Monitor function execution metrics
3. **Resource Usage**: Track TEE resource utilization
4. **Security Events**: Log security-relevant events for audit

## Troubleshooting

Common TEE-related issues and solutions:

1. **Attestation Failures**
   - Check that the TEE platform is properly configured
   - Verify that the attestation service is accessible
   - Ensure the code measurement is in the allowlist

2. **Function Execution Errors**
   - Check memory limits and resource constraints
   - Verify function inputs are properly formatted
   - Check for environment compatibility issues

3. **Integration Issues**
   - Confirm service endpoint configuration
   - Check authentication between services
   - Verify TLS certificate configuration

## Related Documentation

- [TEE Attestation Process](ATTESTATION.md)
- [TEE Provider Implementation](PROVIDERS.md)
- [Functions Service TEE Integration](../functionservice/TEE_INTEGRATION.md)
- [Security Model](../SECURITY_MODEL.md)
