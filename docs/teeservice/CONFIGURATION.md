# TEE Service: Configuration Guide

*Last Updated: 2024-01-15*

## Overview

This document provides configuration options for the TEE Service across AWS Nitro and Azure SGX platforms.

## Configuration Formats

The TEE Service supports:
- YAML files (primary)
- Environment variables
- Command-line arguments (limited)

## Core Configuration

```yaml
# config.yaml
service:
  name: "teeservice"
  version: "1.0.0"
  log_level: "info"  # debug, info, warn, error
  port: 8443
  metrics_port: 8080
  tls:
    cert_file: "/etc/teeservice/certs/server.crt"
    key_file: "/etc/teeservice/certs/server.key"
  database:
    type: "postgres"
    connection_string: "postgres://user:password@host:port/dbname?sslmode=verify-full"
```

## Provider-Specific Configuration

### AWS Nitro

```yaml
providers:
  aws_nitro:
    enabled: true
    region: "us-east-1"
    kms_key_arn: "arn:aws:kms:us-east-1:123456789012:key/1234abcd-12ab-34cd-56ef-1234567890ab"
    role_arn: "arn:aws:iam::123456789012:role/TEEServiceRole"
    vsock_port: 5000
    enclave_pool:
      enabled: true
      min_idle_enclaves: 2
      max_idle_enclaves: 5
```

### Azure SGX

```yaml
providers:
  azure_sgx:
    enabled: true
    tenant_id: "tenant-id"
    subscription_id: "subscription-id"
    resource_group: "neo-tee-service-rg"
    attestation:
      url: "https://neo-attestation.azure.net"
    key_vault:
      url: "https://neo-keyvault.vault.azure.net"
      key_name: "tee-service-key"
    enclave_pool:
      enabled: true
      min_idle_enclaves: 2
      max_idle_enclaves: 5
```

## Attestation Configuration

```yaml
attestation:
  cache:
    enabled: true
    ttl_seconds: 300
  verification:
    allow_debug_enclaves: false  # Only for development
    require_freshness: true      # Require nonce in attestation
    max_age_seconds: 60          # Maximum age of attestation evidence
```

## Secure Channel Configuration

```yaml
secure_channels:
  tls:
    min_version: "TLS1.3"
    cipher_suites:
      - "TLS_AES_256_GCM_SHA384"
      - "TLS_CHACHA20_POLY1305_SHA256"
  management:
    max_channels: 1000
    heartbeat_interval_seconds: 30
    idle_timeout_seconds: 300
```

## Access Control

```yaml
access_control:
  services:
    - id: "functionservice"
      allowed_operations:
        - "teeservice.enclaves.create"
        - "teeservice.enclaves.read"
        - "teeservice.attestation.verify"
    - id: "secretservice"
      allowed_operations:
        - "teeservice.attestation.verify"
        - "teeservice.channels.manage"
```

## Monitoring Configuration

```yaml
monitoring:
  metrics:
    prometheus:
      enabled: true
      endpoint: "/metrics"
  health_check:
    enabled: true
    endpoint: "/health"
  alerts:
    enabled: true
    providers:
      - type: "slack"
        webhook_url: "https://hooks.slack.com/services/XXX/YYY/ZZZ"
```

## Environment-Specific Configurations

### Development

```yaml
# config.dev.yaml
service:
  log_level: "debug"
  port: 8080
  tls:
    enabled: false
attestation:
  verification:
    allow_debug_enclaves: true
providers:
  aws_nitro:
    use_emulator: true
    emulator_endpoint: "http://localhost:8001"
```

### Production

```yaml
# config.prod.yaml
service:
  log_level: "info"
  port: 8443
  tls:
    enabled: true
attestation:
  verification:
    allow_debug_enclaves: false
providers:
  aws_nitro:
    use_emulator: false
```

## Using Environment Variables

Environment variables override YAML configuration:

```sh
# Core service
TEE_SERVICE_LOG_LEVEL=info
TEE_SERVICE_PORT=8443

# AWS Nitro
TEE_PROVIDER_AWS_NITRO_ENABLED=true
TEE_PROVIDER_AWS_NITRO_REGION=us-east-1

# Azure SGX
TEE_PROVIDER_AZURE_SGX_ENABLED=true
TEE_PROVIDER_AZURE_SGX_TENANT_ID=tenant-id
```

## Secret Management

Sensitive configuration values:

1. **AWS Secrets Manager**
   ```yaml
   secrets:
     provider: "aws_secrets_manager"
     secret_name: "neo/teeservice/credentials"
   ```

2. **Azure Key Vault**
   ```yaml
   secrets:
     provider: "azure_key_vault"
     vault_url: "https://neo-keyvault.vault.azure.net"
     secret_name: "teeservice-credentials"
   ```

## Multi-Platform Deployment

```yaml
# config.multi.yaml
service:
  platform_detection:
    enabled: true
    default: "aws_nitro"

providers:
  aws_nitro:
    enabled: true
    region: "${AWS_REGION}"
  azure_sgx:
    enabled: true
    tenant_id: "${AZURE_TENANT_ID}"

cross_platform:
  enabled: true
  attestation_translation:
    enabled: true
  interoperability:
    enabled: true
```

## Service Discovery

```yaml
service_discovery:
  static:
    enabled: true
    services:
      - id: "secretservice"
        endpoint: "https://secretservice.internal.neo.org/v1"
  
  kubernetes:
    enabled: false
    namespace: "neo-service-layer"
    labels:
      app: "teeservice"
```

## Resource Limits

```yaml
resources:
  memory:
    service_max_mb: 1024
    enclave_default_mb: 512
  cpu:
    service_max_cores: 4
    enclave_default_cores: 2
  connections:
    max_concurrent_requests: 1000
```

## Command-Line Usage

```sh
# Start with config file
teeservice --config /etc/teeservice/config.yaml

# Validate configuration
teeservice validate --config /etc/teeservice/config.yaml

# Reload configuration (for supported parameters)
kill -HUP $(pidof teeservice)
```

## Configuration Migration

When upgrading:

```sh
# Backup current config
cp /etc/teeservice/config.yaml /etc/teeservice/config.yaml.bak

# Migrate configuration
teeservice migrate-config --source=/etc/teeservice/config.yaml.bak --target=/etc/teeservice/config.yaml
```
