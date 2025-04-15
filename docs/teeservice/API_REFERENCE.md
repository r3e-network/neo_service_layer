# TEE Service: API Reference

*Last Updated: 2024-01-15*

## Overview

This document provides a comprehensive reference for the TEE Service API, which enables interaction with Trusted Execution Environments (TEEs) across AWS Nitro and Azure SGX platforms. The API allows services to create, manage, and communicate with TEEs securely.

## Base URL

For internal service-to-service communication:
```
https://teeservice.internal.neo.org/v1
```

For management interface (restricted access):
```
https://teeservice-admin.internal.neo.org/v1
```

## Authentication

All API requests require authentication using:

- Service-to-service mTLS with certificate-based authentication
- JWT tokens for user interfaces 
- TEE attestation evidence for TEE-to-service communication

## Common Data Types

### EnclaveStatus

```json
{
  "id": "enc_1a2b3c4d5e6f",
  "provider": "aws_nitro",
  "status": "running",
  "measurements": {
    "pcr0": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
    "pcr1": "fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210",
    "pcr2": "0f1e2d3c4b5a6978"
  },
  "resources": {
    "cpu_count": 2,
    "memory_mb": 4096
  },
  "creation_time": "2024-01-10T12:34:56Z",
  "last_attestation_time": "2024-01-10T13:45:32Z"
}
```

### AttestationData

```json
{
  "provider": "aws_nitro",
  "data": "base64_encoded_attestation_document",
  "nonce": "random_nonce_used_for_freshness",
  "timestamp": "2024-01-10T12:34:56Z"
}
```

### VerificationResult

```json
{
  "valid": true,
  "identity": {
    "provider": "aws_nitro",
    "measurements": {
      "pcr0": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
    },
    "service_id": "functionservice",
    "instance_id": "enc_1a2b3c4d5e6f"
  },
  "timestamp": "2024-01-10T12:34:56Z",
  "expiration": "2024-01-10T13:34:56Z"
}
```

### ChannelInfo

```json
{
  "channel_id": "ch_1a2b3c4d5e6f",
  "source_enclave_id": "enc_1a2b3c4d5e6f",
  "target_enclave_id": "enc_6f5e4d3c2b1a",
  "established_time": "2024-01-10T12:34:56Z",
  "status": "active",
  "last_activity": "2024-01-10T13:45:32Z"
}
```

## API Endpoints

### Enclave Management

#### Create Enclave

Creates a new TEE enclave.

```
POST /enclaves
```

**Request Body (AWS Nitro):**

```json
{
  "provider": "aws_nitro",
  "service_id": "functionservice",
  "cpu_count": 2,
  "memory_mb": 4096,
  "image_path": "s3://neo-enclaves/function_runtime.eif",
  "security_options": {
    "allowed_measurements": [
      {
        "pcr0": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
      }
    ]
  }
}
```

**Request Body (Azure SGX):**

```json
{
  "provider": "azure_sgx",
  "service_id": "secretservice",
  "memory_mb": 2048,
  "image_path": "https://neoimages.blob.core.windows.net/enclaves/secrets_runtime.signed.so",
  "security_options": {
    "allowed_measurements": [
      {
        "mr_enclave": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
        "mr_signer": "fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210"
      }
    ],
    "attestation_url": "https://neoattestation.azure.net"
  }
}
```

**Response:**

```json
{
  "enclave_id": "enc_1a2b3c4d5e6f",
  "status": "creating"
}
```

#### Get Enclave Status

Retrieves the status of a specific enclave.

```
GET /enclaves/{enclave_id}
```

**Response:**

```json
{
  "id": "enc_1a2b3c4d5e6f",
  "provider": "aws_nitro",
  "status": "running",
  "measurements": {
    "pcr0": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
    "pcr1": "fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210"
  },
  "resources": {
    "cpu_count": 2,
    "memory_mb": 4096
  },
  "creation_time": "2024-01-10T12:34:56Z",
  "last_attestation_time": "2024-01-10T13:45:32Z"
}
```

#### List Enclaves

Lists all enclaves managed by the TEE Service.

```
GET /enclaves
```

**Query Parameters:**

- `status` - Filter by enclave status (creating, running, stopping, terminated)
- `provider` - Filter by provider (aws_nitro, azure_sgx)
- `service_id` - Filter by service ID

**Response:**

```json
{
  "enclaves": [
    {
      "id": "enc_1a2b3c4d5e6f",
      "provider": "aws_nitro",
      "status": "running",
      "service_id": "functionservice",
      "creation_time": "2024-01-10T12:34:56Z"
    },
    {
      "id": "enc_6f5e4d3c2b1a",
      "provider": "azure_sgx",
      "status": "running",
      "service_id": "secretservice",
      "creation_time": "2024-01-09T10:11:12Z"
    }
  ],
  "page": 1,
  "page_size": 10,
  "total_count": 2
}
```

#### Terminate Enclave

Terminates an enclave.

```
DELETE /enclaves/{enclave_id}
```

**Response:**

```json
{
  "id": "enc_1a2b3c4d5e6f",
  "status": "terminating"
}
```

### Attestation

#### Request Attestation Challenge

Requests a challenge for attestation.

```
POST /attestation/challenge
```

**Request Body:**

```json
{
  "provider": "aws_nitro",
  "enclave_id": "enc_1a2b3c4d5e6f"
}
```

**Response:**

```json
{
  "challenge": "random_challenge_bytes_base64_encoded",
  "expiration": "2024-01-10T13:34:56Z"
}
```

#### Verify Attestation

Verifies attestation evidence.

```
POST /attestation/verify
```

**Request Body:**

```json
{
  "attestation_data": "base64_encoded_attestation_document",
  "challenge": "random_challenge_bytes_base64_encoded",
  "provider": "aws_nitro"
}
```

**Response:**

```json
{
  "valid": true,
  "identity": {
    "provider": "aws_nitro",
    "measurements": {
      "pcr0": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
    },
    "service_id": "functionservice",
    "instance_id": "enc_1a2b3c4d5e6f"
  },
  "timestamp": "2024-01-10T12:34:56Z",
  "expiration": "2024-01-10T13:34:56Z"
}
```

### Measurements Management

#### Get Authorized Measurements

Retrieves the list of authorized measurements.

```
GET /measurements
```

**Query Parameters:**

- `provider` - Filter by provider (aws_nitro, azure_sgx)
- `service_id` - Filter by service ID

**Response:**

```json
{
  "authorized_measurements": [
    {
      "provider": "aws_nitro",
      "service_id": "functionservice",
      "measurements": {
        "pcr0": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
      },
      "added_at": "2024-01-01T00:00:00Z",
      "added_by": "admin"
    },
    {
      "provider": "azure_sgx",
      "service_id": "secretservice",
      "measurements": {
        "mr_enclave": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
        "mr_signer": "fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210"
      },
      "added_at": "2024-01-02T00:00:00Z",
      "added_by": "admin"
    }
  ]
}
```

#### Add Authorized Measurement

Adds a new authorized measurement.

```
POST /measurements
```

**Request Body:**

```json
{
  "provider": "aws_nitro",
  "service_id": "functionservice",
  "measurements": {
    "pcr0": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
  },
  "description": "Function Runtime v1.2.3"
}
```

**Response:**

```json
{
  "id": "meas_1a2b3c4d5e6f",
  "provider": "aws_nitro",
  "service_id": "functionservice",
  "measurements": {
    "pcr0": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
  },
  "added_at": "2024-01-10T12:34:56Z",
  "added_by": "admin",
  "description": "Function Runtime v1.2.3"
}
```

#### Remove Authorized Measurement

Removes an authorized measurement.

```
DELETE /measurements/{measurement_id}
```

**Response:**

```json
{
  "status": "success",
  "message": "Measurement removed successfully"
}
```

### Secure Channel Management

#### Establish Channel

Establishes a secure channel between enclaves.

```
POST /channels
```

**Request Body:**

```json
{
  "source_enclave_id": "enc_1a2b3c4d5e6f",
  "target_enclave_id": "enc_6f5e4d3c2b1a",
  "attestation_data": "base64_encoded_attestation_document"
}
```

**Response:**

```json
{
  "channel_id": "ch_1a2b3c4d5e6f",
  "status": "establishing"
}
```

#### Get Channel Status

Gets the status of a secure channel.

```
GET /channels/{channel_id}
```

**Response:**

```json
{
  "channel_id": "ch_1a2b3c4d5e6f",
  "source_enclave_id": "enc_1a2b3c4d5e6f",
  "target_enclave_id": "enc_6f5e4d3c2b1a",
  "established_time": "2024-01-10T12:34:56Z",
  "status": "active",
  "last_activity": "2024-01-10T13:45:32Z"
}
```

#### List Channels

Lists all secure channels.

```
GET /channels
```

**Query Parameters:**

- `enclave_id` - Filter by enclave ID (source or target)
- `status` - Filter by status (establishing, active, closing, closed)

**Response:**

```json
{
  "channels": [
    {
      "channel_id": "ch_1a2b3c4d5e6f",
      "source_enclave_id": "enc_1a2b3c4d5e6f",
      "target_enclave_id": "enc_6f5e4d3c2b1a",
      "established_time": "2024-01-10T12:34:56Z",
      "status": "active",
      "last_activity": "2024-01-10T13:45:32Z"
    }
  ],
  "page": 1,
  "page_size": 10,
  "total_count": 1
}
```

#### Close Channel

Closes a secure channel.

```
DELETE /channels/{channel_id}
```

**Response:**

```json
{
  "channel_id": "ch_1a2b3c4d5e6f",
  "status": "closing"
}
```

### Health and Monitoring

#### Get Service Health

Retrieves the health status of the TEE Service.

```
GET /health
```

**Response:**

```json
{
  "status": "healthy",
  "version": "1.2.3",
  "dependencies": {
    "database": "healthy",
    "aws_nitro_support": "healthy",
    "azure_sgx_support": "healthy"
  },
  "metrics": {
    "active_enclaves": 5,
    "active_channels": 8,
    "attestation_requests_per_minute": 120,
    "verification_success_rate": 99.8
  }
}
```

## Error Responses

The API uses standard HTTP status codes and includes detailed error information:

```json
{
  "error": {
    "code": "invalid_attestation",
    "message": "Attestation data could not be verified",
    "details": "PCR0 measurement does not match any authorized values",
    "request_id": "req_1a2b3c4d5e6f"
  }
}
```

Common error codes:

| Code | Description |
|------|-------------|
| `invalid_request` | The request parameters are invalid |
| `enclave_not_found` | The specified enclave could not be found |
| `channel_not_found` | The specified channel could not be found |
| `invalid_attestation` | The attestation data could not be verified |
| `unauthorized` | The requester is not authorized for this operation |
| `provider_error` | An error occurred in the TEE provider |
| `internal_error` | An internal server error occurred |

## Go Client SDK

The TEE Service provides a Go client SDK for easy integration:

```go
import "github.com/neo-project/neo-service-layer/teeservice/client"

// Create a client
teeClient, err := client.NewClient(context.Background(), &client.ClientConfig{
    BaseURL: "https://teeservice.internal.neo.org/v1",
    ServiceID: "functionservice",
    CertPath: "/path/to/client.crt",
    KeyPath: "/path/to/client.key",
    CAPath: "/path/to/ca.crt",
})
if err != nil {
    return err
}

// Create an enclave
enclave, err := teeClient.CreateEnclave(context.Background(), &client.CreateEnclaveRequest{
    Provider: "aws_nitro",
    ServiceID: "functionservice",
    CPUCount: 2,
    MemoryMB: 4096,
    ImagePath: "s3://neo-enclaves/function_runtime.eif",
})
if err != nil {
    return err
}

// Verify attestation
result, err := teeClient.VerifyAttestation(context.Background(), &client.VerifyAttestationRequest{
    AttestationData: "base64_encoded_attestation_document",
    Challenge: "random_challenge_bytes_base64_encoded",
    Provider: "aws_nitro",
})
if err != nil {
    return err
}

// Establish a secure channel
channel, err := teeClient.EstablishChannel(context.Background(), &client.EstablishChannelRequest{
    SourceEnclaveID: "enc_1a2b3c4d5e6f",
    TargetEnclaveID: "enc_6f5e4d3c2b1a",
    AttestationData: "base64_encoded_attestation_document",
})
if err != nil {
    return err
}
```

## Versioning

The TEE Service API follows semantic versioning. The current version is v1.

- Major version changes (e.g., v1 to v2) may introduce breaking changes
- Minor version changes (e.g., v1.1 to v1.2) add new functionality without breaking changes
- Patch version changes (e.g., v1.1.0 to v1.1.1) include bug fixes

## Rate Limiting

The API implements rate limiting to protect the service:

- 1000 requests per minute for non-attestation endpoints
- 5000 requests per minute for attestation verification endpoints

Rate limit headers are included in each response:

```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 995
X-RateLimit-Reset: 1673352896
```

## Permissions

API permissions are based on service identity:

- `teeservice.enclaves.create` - Create enclaves
- `teeservice.enclaves.read` - Read enclave status
- `teeservice.enclaves.terminate` - Terminate enclaves
- `teeservice.attestation.verify` - Verify attestation
- `teeservice.measurements.manage` - Manage authorized measurements
- `teeservice.channels.manage` - Manage secure channels
