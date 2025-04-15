# Functions Service: TEE Integration

*Last Updated: 2023-08-15*

## Overview

The Functions Service is built with a security-first approach, executing all user functions exclusively within Trusted Execution Environments (TEEs). This document details how the Functions Service integrates with TEEs and the Secrets Service to provide a secure execution environment for user-defined functions.

## Table of Contents

- [Overview](#overview)
- [Core Principles](#core-principles)
- [TEE Execution Flow](#tee-execution-flow)
- [Secret Access Pattern](#secret-access-pattern)
- [Secret Namespacing and Access Control](#secret-namespacing-and-access-control)
- [Data Protection](#data-protection)
  - [Data Protected by TEE](#data-protected-by-tee)
  - [Input/Output Protection](#inputoutput-protection)
- [TEE Runtime Implementation](#tee-runtime-implementation)
- [TEE Provider Support](#tee-provider-support)
- [Function-to-Secrets Integration](#function-to-secrets-integration)
  - [Secret Access in Functions](#secret-access-in-functions)
  - [Secret Definition and Access Control](#secret-definition-and-access-control)
- [Security Considerations](#security-considerations)
- [Monitoring and Audit](#monitoring-and-audit)
- [Related Documentation](#related-documentation)

## Core Principles

1. **Mandatory TEE Execution**: All functions are executed exclusively within TEE environments.
2. **TEE Provider Abstraction**: The service supports multiple TEE providers through a common interface.
3. **Attestation-Based Trust**: Remote attestation is used to establish trust before sensitive operations.
4. **Secure Secret Access**: Function access to secrets is mediated through attestation verification.

## TEE Execution Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│ Automation   │     │ Function     │     │ TEE          │     │ Secrets      │
│ Service      │     │ Service      │     │ Provider     │     │ Service      │
└──────┬───────┘     └──────┬───────┘     └──────┬───────┘     └──────┬───────┘
       │                    │                    │                    │
       │  Execute Function  │                    │                    │
       ├───────────────────►│                    │                    │
       │                    │                    │                    │
       │                    │ Create Enclave     │                    │
       │                    ├───────────────────►│                    │
       │                    │                    │                    │
       │                    │ Enclave Created    │                    │
       │                    │◄───────────────────┤                    │
       │                    │                    │                    │
       │                    │ Get Attestation    │                    │
       │                    ├───────────────────►│                    │
       │                    │                    │                    │
       │                    │ Return Quote       │                    │
       │                    │◄───────────────────┤                    │
       │                    │                    │                    │
       │                    │ Request Secrets    │                    │
       │                    │ with Attestation   │                    │
       │                    ├─────────────────────────────────────────►
       │                    │                    │                    │
       │                    │                    │                    │ Verify
       │                    │                    │                    │ Attestation
       │                    │                    │                    │───┐
       │                    │                    │                    │   │
       │                    │                    │                    │◄──┘
       │                    │                    │                    │
       │                    │ Return Secrets     │                    │
       │                    │◄─────────────────────────────────────────┤
       │                    │                    │                    │
       │                    │ Execute Function   │                    │
       │                    │ in TEE             │                    │
       │                    ├───────────────────►│                    │
       │                    │                    │                    │
       │                    │                    │ Function           │
       │                    │                    │ Execution          │
       │                    │                    │───┐                │
       │                    │                    │   │                │
       │                    │                    │◄──┘                │
       │                    │                    │                    │
       │                    │ Execution Result   │                    │
       │                    │◄───────────────────┤                    │
       │                    │                    │                    │
       │  Return Result     │                    │                    │
       │◄───────────────────┤                    │                    │
       │                    │                    │                    │
```

## Secret Access Pattern

When a function needs to access secrets during execution, the following secure pattern is followed:

1. The Function Service creates a TEE environment (enclave)
2. The TEE generates attestation evidence proving its identity and integrity
3. The Function Service presents this attestation to the Secrets Service
4. The Secrets Service verifies the attestation evidence
5. Upon successful verification, the Secrets Service provides the requested secrets
6. Secrets are only delivered to verified TEE environments
7. Secrets are only accessible within the secure TEE memory
8. Function execution occurs with access to these secrets

## Secret Namespacing and Access Control

Functions access secrets through a hierarchical namespacing system:

```
function-<functionID>/<environment>/<category>/<n>
```

For example:

- `function-abc123/production/database/credentials`
- `function-abc123/development/api/api-key`

The Secrets Service enforces access control rules:

1. Functions can only access secrets in their own namespace
2. Access is controlled based on the function's identity established through attestation
3. Secret access is logged for audit and compliance purposes

## Data Protection

### Data Protected by TEE

The following sensitive data is protected by the TEE:

1. Function code (confidentiality and integrity)
2. Function inputs (when marked as sensitive)
3. Secrets injected by the Secrets Service
4. Runtime environment during execution
5. Computation results before they're returned

### Input/Output Protection

1. **Function Inputs**
   - Inputs can be marked as sensitive or non-sensitive
   - Sensitive inputs are only accessible within the TEE
   - Non-sensitive inputs can be logged and monitored

2. **Function Outputs**
   - Outputs are integrity-protected
   - Outputs can be optionally encrypted if containing sensitive data

## TEE Runtime Implementation

The Function Service implements TEE execution through a dedicated runtime interface:

```go
// IRuntime defines the interface for executing functions in TEE
type IRuntime interface {
    // ExecuteFunction executes a function within a TEE
    ExecuteFunction(ctx context.Context, params *ExecutionParams) (*ExecutionResult, error)
    
    // GetType returns the runtime type (e.g., "javascript", "python")
    GetType() string
}

// TEERuntime implements the IRuntime interface for TEE execution
type TEERuntime struct {
    provider      tee.Provider
    secretsClient secrets.Client
    logger        *zap.Logger
}
```

## TEE Provider Support

The Function Service supports multiple TEE technologies:

1. **AWS Nitro Enclaves**
   - Used primarily when deployed on AWS
   - Provides VM-level isolation

2. **Intel SGX**
   - Used for environments with SGX hardware
   - Provides enclave-based isolation

3. **AMD SEV**
   - Used for environments with AMD SEV
   - Provides VM memory encryption

## Function-to-Secrets Integration

### Secret Access in Functions

Functions access secrets through a structured parameter:

```javascript
// JavaScript example
function processData(params, secrets, context) {
  // Access database credentials from Secrets Service
  const dbCredentials = secrets.DATABASE_CREDENTIALS;
  
  // Access API key from Secrets Service
  const apiKey = secrets.API_KEY;
  
  // Process data securely within TEE
  // ...
  
  return result;
}
```

```python
# Python example
def process_data(params, secrets, context):
    # Access database credentials from Secrets Service
    db_credentials = secrets['DATABASE_CREDENTIALS']
    
    # Access API key from Secrets Service
    api_key = secrets['API_KEY']
    
    # Process data securely within TEE
    # ...
    
    return result
```

### Secret Definition and Access Control

Functions declare required secrets during registration:

```json
{
  "function": {
    "name": "ProcessUserData",
    "runtime": "javascript",
    "code": "function processData(params, secrets, context) {...}",
    "requiredSecrets": [
      "DATABASE_CREDENTIALS",
      "API_KEY"
    ]
  }
}
```

The Function Service verifies that:

1. The secrets exist in the function's namespace
2. The function has permission to access these secrets
3. The execution environment is a verified TEE

## Security Considerations

1. **Attestation Verification**
   - All TEE attestation evidence is cryptographically verified
   - Measurements are checked against an allowlist of approved values
   - Nonces are used to prevent replay attacks

2. **Memory Protection**
   - Secrets are only present in TEE protected memory
   - Memory is cleared after function execution
   - TEE memory is isolated from host system

3. **Function Code Validation**
   - Function code is validated before execution
   - Code is checked for security issues and resource usage
   - Runtime security policies enforce additional constraints

## Monitoring and Audit

1. **Secret Access Logs**
   - All secret access is logged (without recording secret values)
   - Logs include function ID, execution ID, and timestamp

2. **TEE Metrics**
   - TEE creation, execution time, and resource usage are tracked
   - Attestation success/failure rates are monitored

3. **Audit Trail**
   - Comprehensive audit trail for compliance and security review

## Related Documentation

- [Functions Service Architecture](./ARCHITECTURE.md) - Overall Functions Service architecture
- [TEE Developer Guide](../tee/GUIDE.md) - Comprehensive guide for TEE development
- [TEE Attestation](../security/TEE_ATTESTATION.md) - Details on attestation processes
- [Secrets Service](../secretservice/OVERVIEW.md) - Secrets management and integration
- [Security Model](../SECURITY_MODEL.md) - Overall security architecture
