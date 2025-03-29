# Neo N3 Service Layer Configuration Guide

## Overview

The Neo N3 Service Layer uses a flexible configuration system that allows you to customize all aspects of the service layer. This guide covers configuration file structure, environment variables, secrets management, and best practices for maintaining configurations across different environments.

## Configuration Structure

### 1. Main Configuration

```yaml
# config/config.yaml
app:
  name: neo-service-layer
  version: 1.0.0
  environment: production

server:
  host: 0.0.0.0
  port: 3000
  timeout: 30s
  cors:
    enabled: true
    origins: ["https://example.com"]

database:
  type: postgresql
  host: localhost
  port: 5432
  name: neo_service
  user: ${DB_USER}
  password: ${DB_PASSWORD}
  pool:
    max_connections: 20
    idle_timeout: 300s

cache:
  type: redis
  host: localhost
  port: 6379
  ttl: 3600s
```

### 2. Service-Specific Configuration

```yaml
# config/services/functions.yaml
functions:
  runtime:
    type: node
    version: 18
    timeout: 60s
    memory: 256Mi
  
  execution:
    concurrent_limit: 100
    retry:
      max_attempts: 3
      backoff:
        initial: 1s
        max: 60s
        multiplier: 2
  
  storage:
    type: s3
    bucket: neo-functions
    region: us-east-1
  
  monitoring:
    metrics_enabled: true
    tracing_enabled: true
    logging_level: info

# config/services/triggers.yaml
triggers:
  scheduler:
    enabled: true
    timezone: UTC
    max_missed: 3
  
  queue:
    type: redis
    prefix: triggers
    batch_size: 100
  
  execution:
    workers: 5
    timeout: 300s
  
  monitoring:
    heartbeat_interval: 60s
    health_check_path: /health

# config/services/gas-bank.yaml
gas_bank:
  allocation:
    strategy: dynamic
    min_balance: 1000
    max_balance: 10000
  
  refill:
    enabled: true
    threshold: 2000
    amount: 5000
  
  monitoring:
    check_interval: 60s
    alert_threshold: 1000
```

## Environment Configuration

### 1. Environment Files

```yaml
# config/environments/production.yaml
app:
  debug: false
  log_level: info

server:
  host: 0.0.0.0
  port: 3000

database:
  host: prod-db.example.com
  pool:
    max_connections: 50

# config/environments/staging.yaml
app:
  debug: true
  log_level: debug

server:
  host: 0.0.0.0
  port: 3001

database:
  host: staging-db.example.com
  pool:
    max_connections: 20

# config/environments/development.yaml
app:
  debug: true
  log_level: debug

server:
  host: localhost
  port: 3002

database:
  host: localhost
  pool:
    max_connections: 10
```

### 2. Environment Variables

```bash
# .env.production
NODE_ENV=production
DB_HOST=prod-db.example.com
DB_USER=prod_user
DB_PASSWORD=prod_password
REDIS_HOST=prod-redis.example.com

# .env.staging
NODE_ENV=staging
DB_HOST=staging-db.example.com
DB_USER=staging_user
DB_PASSWORD=staging_password
REDIS_HOST=staging-redis.example.com

# .env.development
NODE_ENV=development
DB_HOST=localhost
DB_USER=dev_user
DB_PASSWORD=dev_password
REDIS_HOST=localhost
```

## Secret Management

### 1. Secrets Configuration

```yaml
# config/secrets.yaml
secrets:
  provider: vault
  address: https://vault.example.com
  auth:
    method: token
    token: ${VAULT_TOKEN}
  
  paths:
    database: secret/database
    api_keys: secret/api-keys
    certificates: secret/certificates
  
  rotation:
    enabled: true
    schedule: "0 0 * * *"
    
  backup:
    enabled: true
    provider: s3
    bucket: secrets-backup
```

### 2. Secret Access

```typescript
// Access secrets in code
import { SecretsManager } from '@neo-service/sdk';

const secrets = new SecretsManager();

// Get database credentials
const dbCreds = await secrets.get('database/credentials');

// Get API key
const apiKey = await secrets.get('api-keys/external-service');

// Store new secret
await secrets.set('api-keys/new-service', {
  key: 'abc123',
  secret: 'xyz789'
});
```

## Configuration Management

### 1. Configuration Commands

```bash
# View current configuration
neo-service config view

# View specific configuration
neo-service config view functions

# Validate configuration
neo-service config validate

# Update configuration
neo-service config set functions.timeout 60s

# Export configuration
neo-service config export --format yaml

# Import configuration
neo-service config import config.yaml
```

### 2. Configuration Validation

```yaml
# config/validation.yaml
validation:
  schema:
    enabled: true
    strict: true
  
  rules:
    - path: database.pool.max_connections
      type: integer
      min: 1
      max: 100
    
    - path: server.port
      type: integer
      min: 1024
      max: 65535
    
    - path: functions.timeout
      type: duration
      min: 1s
      max: 300s
```

## Feature Flags

### 1. Feature Configuration

```yaml
# config/features.yaml
features:
  new_runtime:
    enabled: true
    percentage: 50
    users: ["user1", "user2"]
  
  beta_api:
    enabled: true
    environments: ["development", "staging"]
  
  improved_scaling:
    enabled: false
    description: "New auto-scaling algorithm"
```

### 2. Feature Management

```typescript
// Check feature flags in code
import { FeatureFlags } from '@neo-service/sdk';

const features = new FeatureFlags();

// Check if feature is enabled
if (await features.isEnabled('new_runtime')) {
  // Use new runtime
}

// Check for specific user
if (await features.isEnabledForUser('beta_api', userId)) {
  // Use beta API
}
```

## Monitoring and Metrics

### 1. Metrics Configuration

```yaml
# config/metrics.yaml
metrics:
  enabled: true
  provider: prometheus
  interval: 10s
  
  collectors:
    - type: system
      enabled: true
    - type: application
      enabled: true
    - type: business
      enabled: true
  
  exporters:
    - type: prometheus
      port: 9090
    - type: statsd
      address: localhost:8125
```

### 2. Alert Configuration

```yaml
# config/alerts.yaml
alerts:
  enabled: true
  providers:
    - type: email
      recipients: ["ops@example.com"]
    - type: slack
      webhook: "https://hooks.slack.com/..."
  
  rules:
    - name: high_error_rate
      condition: "error_rate > 0.1"
      duration: 5m
      severity: critical
```

## Best Practices

1. **Configuration Organization**
   - Use logical file structure
   - Separate concerns
   - Version control configs
   - Document all options

2. **Environment Management**
   - Use environment files
   - Secure sensitive data
   - Validate configurations
   - Test all environments

3. **Secret Handling**
   - Use secret management
   - Rotate regularly
   - Audit access
   - Backup securely

4. **Validation**
   - Schema validation
   - Type checking
   - Range validation
   - Required fields

5. **Monitoring**
   - Track configuration changes
   - Monitor usage
   - Alert on issues
   - Regular audits

6. **Documentation**
   - Document all options
   - Provide examples
   - Explain defaults
   - Update regularly

## Support

For configuration support:
- Email: support@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/will/neo_service_layer/issues)

## Additional Resources

- [Configuration Reference](./REFERENCE.md)
- [Environment Setup](./ENVIRONMENTS.md)
- [Secrets Management](./SECRETS.md)
- [Feature Flags](./FEATURES.md)
- [Validation Rules](./VALIDATION.md) 