# Trusted Execution Environment (TEE) Developer Guide

*Last Updated: 2023-08-15*

## Overview

This guide provides developers with comprehensive guidance for building applications and services that leverage Trusted Execution Environments (TEEs) within the Neo Service Layer. TEEs provide hardware-backed security guarantees for sensitive operations, ensuring code and data confidentiality and integrity.

## Table of Contents

- [Overview](#overview)
- [TEE Fundamentals](#tee-fundamentals)
  - [What is a TEE?](#what-is-a-tee)
  - [Supported TEE Technologies](#supported-tee-technologies)
- [Architecture Overview](#architecture-overview)
  - [TEE Integration Points](#tee-integration-points)
  - [Security Boundaries](#security-boundaries)
- [Attestation Process](#attestation-process)
  - [Attestation Flow](#attestation-flow)
  - [Attestation Data](#attestation-data)
  - [Verification Service](#verification-service)
- [Working with TEE-Based Functions](#working-with-tee-based-functions)
  - [Function Lifecycle](#function-lifecycle)
  - [Writing TEE-Compatible Functions](#writing-tee-compatible-functions)
  - [Function Input/Output](#function-inputoutput)
- [Security Best Practices](#security-best-practices)
- [Monitoring and Audit](#monitoring-and-audit)
- [Troubleshooting](#troubleshooting)
- [Related Documentation](#related-documentation)
- [Implementation Plan for Multi-Platform TEE Services](#implementation-plan-for-multi-platform-tee-services)

## TEE Fundamentals

### What is a TEE?

A Trusted Execution Environment (TEE) is a secure area within a processor that ensures:

1. **Code Integrity**: Only authorized code can execute within the environment
2. **Data Confidentiality**: Data processed within the TEE is protected from external observation
3. **Data Integrity**: Data cannot be tampered with by unauthorized entities
4. **Attestation**: The ability to cryptographically prove the TEE's identity and state

### Supported TEE Technologies

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

### Attestation Flow

```
┌────────────┐     ┌────────────────┐     ┌────────────────┐     ┌───────────────┐
│            │     │                │     │                │     │               │
│   Client   │◄────┤ Function Service├────►│ Verification   ├────►│ TEE Environment│
│            │     │                │     │ Service        │     │               │
└────────────┘     └────────────────┘     └────────────────┘     └───────────────┘
       │                   │                      │                      │
       │  Request with     │                      │                      │
       │  attestation      │                      │                      │
       ├──────────────────►│                      │                      │
       │                   │                      │                      │
       │                   │  Execute in TEE      │                      │
       │                   ├───────────────────────────────────────────►│
       │                   │                      │                      │
       │                   │                      │  Generate            │
       │                   │                      │  attestation report  │
       │                   │                      │◄─────────────────────┤
       │                   │                      │                      │
       │                   │ Verify attestation   │                      │
       │                   ├─────────────────────►│                      │
       │                   │                      │                      │
       │                   │ Verification result  │                      │
       │                   │◄─────────────────────┤                      │
       │                   │                      │                      │
       │ Secure operation  │                      │                      │
       │◄──────────────────┤                      │                      │
       │                   │                      │                      │
```

The attestation process follows these key steps:

1. **TEE Initialization**:
   - The TEE is initialized with the runtime code
   - Measurements (cryptographic hashes) of the code and initial data are recorded
   - A unique attestation key pair is generated within the TEE

2. **Quote Generation**:
   - When attestation is requested, the TEE generates a "quote"
   - The quote contains the measurements, a nonce for freshness, and additional claims
   - The quote is signed by the attestation key, which is rooted in the hardware

3. **Quote Verification**:
   - The verifier (e.g., Secrets Service) receives the quote
   - Verifier checks the signature against known good root certificates
   - Verifier compares measurements against an allowlist of trusted code
   - If verification succeeds, a secure channel is established

### Attestation Data

The attestation quote contains:

- **PCR values**: Measurements of the code and configuration
- **Nonce**: A unique value to prevent replay attacks
- **TEE identity**: Unique identifier for the TEE instance
- **Timestamp**: Time when the quote was generated
- **Additional claims**: Custom claims about the TEE environment

### Verification Service

The Neo Service Layer includes a dedicated verification service that:

- Maintains an allowlist of authorized TEE measurements
- Verifies signatures against hardware manufacturer certificates
- Implements caching to improve performance
- Provides detailed verification results for auditing

## Working with TEE-Based Functions

### Function Lifecycle

```
┌────────────┐     ┌────────────┐     ┌────────────┐
│ Developer  │     │ Function   │     │ Automation │
│            │     │ Service    │     │ Service    │
└──────┬─────┘     └──────┬─────┘     └──────┬─────┘
       │                  │                  │
       │ Register Function│                  │
       ├─────────────────►│                  │
       │                  │                  │
       │                  │ Validate & Store │
       │                  │────┐             │
       │                  │    │             │
       │                  │◄───┘             │
       │                  │                  │
       │                  │  Load Function   │
       │                  │  when Triggered  │
       │                  │◄─────────────────┤
       │                  │                  │
       │                  │  Create TEE      │
       │                  │  Environment     │
       │                  │────┐             │
       │                  │    │             │
       │                  │◄───┘             │
       │                  │                  │
       │                  │  Execute in TEE  │
       │                  │────┐             │
       │                  │    │             │
       │                  │◄───┘             │
       │                  │                  │
       │                  │ Return Result    │
       │                  ├─────────────────►│
       │                  │                  │
```

Detailed function lifecycle in TEE:

1. **Preparation**:
   - Function code and dependencies are packaged
   - Package is encrypted with a session key
   - Package metadata is prepared for the TEE

2. **TEE Initialization**:
   - TEE environment is created with appropriate runtime
   - Runtime performs attestation to establish trust
   - Upon successful attestation, session keys are exchanged

3. **Function Loading**:
   - Encrypted package is transmitted to the TEE
   - TEE decrypts the package using the session key
   - Runtime verifies package integrity and loads the function

4. **Execution**:
   - Function executes in the isolated environment
   - All intermediate states remain within the TEE
   - Resource limits are enforced by the TEE and runtime

5. **Result Handling**:
   - Execution results are encrypted within the TEE
   - Encrypted results are returned to the caller
   - TEE resources are released and reset for the next execution

### Writing TEE-Compatible Functions

Functions must follow specific patterns to work securely within TEEs:

#### JavaScript Example

```javascript
/**
 * Example TEE-compatible function for processing sensitive data
 * @param {Object} params - Input parameters
 * @param {Object} secrets - Injected secrets (available only within TEE)
 * @param {Object} context - Execution context
 */
function processData(params, secrets, context) {
  // Log start of execution (logged securely within TEE)
  context.logger.info('Starting secure data processing');
  
  // Access sensitive secrets (only available within TEE)
  const apiKey = secrets.API_KEY;
  
  // Process input data
  const result = {
    processedData: transformData(params.inputData),
    timestamp: new Date().toISOString(),
    // Never include secrets in the result
  };
  
  // Return result (will be securely transmitted out of TEE)
  return result;
}

// Helper function
function transformData(data) {
  // Implement your secure transformation logic
  return data.map(item => item.value * 2);
}

// Export the function
module.exports = processData;
```

#### Python Example

```python
def process_data(params, secrets, context):
    """
    Example TEE-compatible function for processing sensitive data
    
    Args:
        params: Input parameters
        secrets: Injected secrets (available only within TEE)
        context: Execution context
    
    Returns:
        Processed result
    """
    # Log start of execution (logged securely within TEE)
    context.logger.info('Starting secure data processing')
    
    # Access sensitive secrets (only available within TEE)
    api_key = secrets['API_KEY']
    
    # Process input data
    result = {
        'processed_data': transform_data(params['input_data']),
        'timestamp': context.current_time(),
        # Never include secrets in the result
    }
    
    # Return result (will be securely transmitted out of TEE)
    return result

def transform_data(data):
    """Helper function to transform data"""
    return [item['value'] * 2 for item in data]
```

### Function Input/Output

#### Input Structure

Functions receive three main input objects:

1. **params**: User-provided parameters (potentially untrusted)
2. **secrets**: Injected secrets (only available within TEE)
3. **context**: Runtime context with utilities and information

Example input:

```json
{
  "params": {
    "inputData": [{"id": 1, "value": 10}, {"id": 2, "value": 20}],
    "operation": "process"
  },
  "secrets": {
    "API_KEY": "sk_live_123456789abcdef",
    "DATABASE_URL": "postgresql://user:pass@host:port/db"
  },
  "context": {
    "functionId": "func_12345",
    "executionId": "exec_abcdef",
    "deadlineMs": 1677721600000,
    "remainingTimeMs": 29000,
    "logger": {}
  }
}
```

#### Output Structure

Functions should return a structured result:

```json
{
  "result": {
    "processedData": [20, 40],
    "timestamp": "2023-03-01T12:00:00Z"
  },
  "metadata": {
    "processingTimeMs": 250,
    "memoryUsedKb": 1024
  }
}
```

## Security Best Practices

1. **Never exfiltrate secrets**
   - Don't include secrets in logs, results, or external calls
   - Treat all secrets as highly sensitive

2. **Validate input data**
   - Treat all user inputs as untrusted
   - Implement strict validation

3. **Minimize external calls**
   - Each call outside the TEE is a potential security boundary crossing
   - Batch operations when possible

4. **Limit execution time**
   - Functions have execution time limits
   - Implement timeouts for external operations

5. **Manage memory carefully**
   - TEEs have memory constraints
   - Clean up resources after use

6. **Implement secure error handling**
   - Don't expose sensitive information in error messages
   - Log errors appropriately

7. **Attestation verification**
   - Always verify attestation in production environments
   - Keep attestation verification keys secure

## Monitoring and Audit

1. **Secret Access Logs**
   - All secret access is logged (without recording secret values)
   - Logs include function ID, execution ID, and timestamp

2. **TEE Metrics**
   - TEE creation, execution time, and resource usage are tracked
   - Attestation success/failure rates are monitored

3. **Audit Trail**
   - Comprehensive audit trail for compliance and security review

## Troubleshooting

Common attestation issues and their solutions:

1. **Outdated Firmware**: Update TEE firmware to latest version
2. **TCB Outdated**: Update the Trusted Computing Base components
3. **Measurement Mismatch**: Verify the correct version of code is deployed
4. **Revoked Certificates**: Check for certificate revocation notices
5. **Configuration Errors**: Verify attestation configuration settings

## Related Documentation

- [Functions Service: TEE Integration](./functionservice/TEE_INTEGRATION.md) - Functions Service-specific TEE integration
- [Security Model](./SECURITY_MODEL.md) - Overall security architecture
- [Service Integration](./SERVICE_INTEGRATION.md) - Service integration patterns
- [Secrets Service](./secretservice/OVERVIEW.md) - Secrets management

## Implementation Plan for Multi-Platform TEE Services

*Last Updated: 2024-01-15*

This section outlines a comprehensive, phased approach for implementing all Neo Service Layer services within Trusted Execution Environments (TEEs) across multiple platforms (AWS Nitro and Azure CC/SGX).

### Assumptions

- Services need to be capable of running on both AWS Nitro and Azure CC/SGX platforms
- A suitable TEE framework/SDK is used to abstract some platform differences
- Secure inter-enclave communication involves platform-specific attestation followed by establishing a secure channel
- Each service runs in its own dedicated TEE enclave

### Phase 0: TEE Foundation, Environment, and Secure Channel Prototyping (2-6 Weeks)

**Goal**: Establish the fundamental TEE capabilities, development environment, and a working prototype of secure inter-enclave communication on both target platforms.

**Key Tasks**:

1. **Deep Dive Documentation Review**
   - Analyze architecture overview, security model, encryption design, and TEE documentation
   - Identify specific TEE libraries, tools, and integration patterns
   - Understand attestation flow and secure channel mechanisms

2. **Cloud Environment Setup**
   - AWS: Configure Nitro-compatible EC2 instances, IAM roles, VPC networking, and Nitro tooling
   - Azure: Provision SGX-enabled VMs, configure VNet networking, and Azure Attestation service
   - Create base AMIs/VM images for development and deployment

3. **TEE SDK & Tooling Installation**
   - Install chosen TEE framework/SDK (e.g., Gramine, Open Enclave SDK)
   - Configure platform SDKs/CLIs with TEE support
   - Test basic enclave creation and management

4. **Build Pipeline Foundation**
   - Create CI/CD structure for TEE application building
   - Implement secure storage for enclave signing keys
   - Define deployment workflows for both platforms

5. **Secure Channel Design & Prototype**
   - Design a common API/library interface for secure communications
   - Implement AWS Nitro backend using Nitro attestation and VSOCK
   - Implement Azure SGX backend using SGX quotes and Azure Attestation
   - Test secure communications within respective platforms

6. **Infrastructure & Configuration Strategy**
   - Deploy external dependencies (databases, message queues)
   - Define configuration distribution mechanisms for each platform

**Definition of Done**: Development environments functional for both platforms. Basic enclave applications can be built and run. Secure channel prototype works independently on AWS and Azure. Build pipeline exists. Infrastructure dependencies accessible.

### Phase 1: Secure Secrets Management Implementation (3-5 Weeks)

**Goal**: Deploy the `secretservice` securely on both platforms to serve as the foundation for other services.

**Key Tasks**:

1. **Secret Bootstrap Design**
   - AWS: Design using KMS integration with Nitro Enclaves
   - Azure: Design using Azure Attestation + Azure Key Vault with Key Release Policies
   - Define required IAM/Azure AD permissions and roles

2. **Adapt `secretservice` for TEE**
   - Refactor code for platform-specific bootstrapping
   - Integrate secure channel library for API exposure
   - Handle TEE-specific limitations and constraints

3. **Build, Sign, and Configure**
   - Generate platform-specific enclave images (EIF for Nitro, signed binaries for SGX)
   - Create platform-specific configurations
   - Test build and configuration processes

4. **Deploy & Bootstrap Secrets**
   - AWS: Deploy Nitro enclave and bootstrap using KMS
   - Azure: Deploy SGX enclave and bootstrap using AKV
   - Verify deployment and bootstrapping processes

5. **Secure Client Library Development**
   - Create a platform-aware client library for service consumers
   - Implement attestation, secure channel establishment, and API calls
   - Test across both platforms

**Definition of Done**: `secretservice` running securely in enclaves on both AWS and Azure. Client library allows other services to securely fetch secrets. Bootstrap procedures documented.

### Phase 2: Observability & Core TEE Service Implementation (2-4 Weeks)

**Goal**: Enable secure logging/metrics export and deploy central TEE helper functionality.

**Key Tasks**:

1. **Secure Observability Implementation**
   - Implement log/metric export mechanisms for both platforms
   - Configure logging and metrics backends
   - Develop platform-aware logging/metrics library wrappers

2. **TEE Service Implementation**
   - Adapt the `teeservice` for cross-platform operation
   - Build, sign, and deploy on both platforms
   - Implement secure channel communication

3. **Integration & Testing**
   - Integrate observability into the `secretservice`
   - Verify log and metric flow to backend systems
   - Test TEE service functionality across platforms

**Definition of Done**: Secure logging and metrics export functional from enclaves on both platforms. TEE service functionality verified and operational.

### Phase 3: Iterative Service Implementation (1-3 Weeks per Service)

**Goal**: Incrementally implement, deploy, and test the remaining services within enclaves on both platforms.

**Service Implementation Tasks** (for each service - `pricefeedservice`, `gasbankservice`, `functionservice`, `automationservice`, `apiservice`):

1. **Review Service Documentation**
   - Analyze service-specific architecture and implementation docs
   - Identify integration points and dependencies
   - Understand service-specific security requirements

2. **TEE Adaptation**
   - Integrate secure secrets client library
   - Implement secure observability wrappers
   - Use secure channel library for communications
   - Refactor code for TEE constraints

3. **Configure, Build & Deploy**
   - Create platform-specific configurations
   - Generate and sign enclave images
   - Deploy to AWS and Azure environments

4. **Testing**
   - Run unit and integration tests
   - Verify secure communications with dependencies
   - Test core service functionality

**Implementation Order and Estimated Timeline**:
1. `pricefeedservice` (2-3 weeks)
2. `gasbankservice` (2-3 weeks)
3. `functionservice` (3-4 weeks)
4. `automationservice` (2-3 weeks)
5. `apiservice` (2 weeks)

**Definition of Done (per service)**: Service successfully built, signed, and deployed in enclaves on both AWS and Azure. Securely configured with functional communication with dependencies.

### Phase 4: End-to-End Integration & Security Testing (3-6 Weeks)

**Goal**: Verify the complete system works correctly and securely across both platforms.

**Key Tasks**:

1. **End-to-End Test Scenarios**
   - Define scenarios covering cross-service workflows
   - Create test cases for AWS, Azure, and mixed deployments
   - Implement automated testing scripts

2. **TEE Security Audit & Penetration Testing**
   - Test attestation validation in secure channels
   - Verify enclave signing processes
   - Assess side-channel vulnerabilities
   - Validate IAM/Azure AD permissions
   - Test resilience against host/parent interference

3. **Performance Testing**
   - Measure request latency across platforms
   - Evaluate secure channel establishment overhead
   - Load test critical services
   - Identify performance bottlenecks

4. **Failure Injection Testing**
   - Simulate enclave crashes and recovery
   - Test attestation failure scenarios
   - Measure resilience to infrastructure disruptions

5. **Documentation Finalization**
   - Update operational procedures for TEE-specific scenarios
   - Document troubleshooting steps for both platforms
   - Create runbooks for common failure modes

**Definition of Done**: All major E2E scenarios pass on both platforms. Security vulnerabilities addressed. Performance characteristics understood. Operational documentation completed.

### Phase 5: Production Rollout & Monitoring (2-4 Weeks)

**Goal**: Deploy the TEE-secured system to production environments and establish robust monitoring.

**Key Tasks**:

1. **Production Environment Preparation**
   - Build production-grade AWS Nitro and Azure SGX environments
   - Configure hardened networking, IAM/AD roles, and KMS/AKV
   - Prepare observability backends for production

2. **Production Deployment**
   - Generate production-signed enclave images
   - Execute staged deployment plan (e.g., blue/green deployment)
   - Verify production configuration and connectivity

3. **Monitoring Activation**
   - Enable comprehensive monitoring dashboards
   - Configure alerting for TEE-specific metrics
   - Test alerting and incident response procedures

4. **Operational Readiness**
   - Finalize runbooks and documentation
   - Train operations team on TEE-specific procedures
   - Verify backup and recovery processes

**Definition of Done**: System operational in production on both AWS and Azure platforms. Monitoring and alerting active. Operations team trained and ready to support the system.

### Risk Management

| Risk | Mitigation |
|------|------------|
| **Platform-specific TEE limitations** | Early prototyping, abstraction layers, platform-specific optimizations |
| **Attestation compatibility issues** | Comprehensive attestation testing in Phase 0, separate verification paths per platform |
| **Performance overhead** | Benchmarking during development, performance optimization sprints if needed |
| **TEE resource constraints** | Service design accounting for memory/CPU limitations, load testing |
| **Key management complexity** | Thorough security review of key hierarchies, regular key rotation testing |
| **Communication channel security** | Formal verification of secure channel design, penetration testing |
| **Cross-platform compatibility** | Clear abstractions, extensive testing on both platforms throughout |

### Implementation Checklists

#### AWS Nitro Implementation Checklist

- [ ] Parent EC2 instance configuration and hardening
- [ ] Nitro Enclaves CLI and SDK setup
- [ ] IAM role configuration for KMS access
- [ ] VSOCK communication channel setup
- [ ] EIF (Enclave Image File) build pipeline
- [ ] Nitro attestation integration
- [ ] KMS crypto operations from within enclave
- [ ] Secure logging via VSOCK forwarding
- [ ] Host → enclave configuration distribution
- [ ] Deployment and startup automation

#### Azure SGX Implementation Checklist

- [ ] SGX-enabled VM configuration and hardening
- [ ] Open Enclave SDK / SGX SDK setup
- [ ] Azure AD managed identity configuration
- [ ] Azure Attestation Service integration
- [ ] Azure Key Vault integration
- [ ] SGX quote generation and verification
- [ ] Secure channel establishment
- [ ] Enclave memory management
- [ ] Configuration distribution mechanism
- [ ] Deployment and startup automation

This phased implementation plan provides a structured approach to progressively build, deploy, and secure all Neo Service Layer services within TEEs across both AWS Nitro and Azure Confidential Computing platforms.
