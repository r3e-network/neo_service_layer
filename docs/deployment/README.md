# Neo N3 Service Layer Deployment Guide

## Overview

The Neo N3 Service Layer provides comprehensive deployment capabilities to help you deploy, manage, and scale your serverless functions and triggers across different environments. This guide covers deployment configuration, continuous integration/deployment (CI/CD), monitoring, and best practices.

## Deployment Configuration

### 1. Environment Configuration

```yaml
# config/deployment.yaml
environments:
  production:
    region: us-east-1
    replicas: 3
    auto_scaling:
      enabled: true
      min_replicas: 2
      max_replicas: 10
      target_cpu: 70
    resources:
      cpu: "1"
      memory: "2Gi"
    storage:
      type: persistent
      size: "20Gi"
    networking:
      domain: api.neo-service-layer.io
      ssl: true
      cdn: true

  staging:
    region: us-east-1
    replicas: 2
    auto_scaling:
      enabled: true
      min_replicas: 1
      max_replicas: 5
      target_cpu: 80
    resources:
      cpu: "0.5"
      memory: "1Gi"
    storage:
      type: persistent
      size: "10Gi"
    networking:
      domain: staging.neo-service-layer.io
      ssl: true

  development:
    region: us-east-1
    replicas: 1
    auto_scaling:
      enabled: false
    resources:
      cpu: "0.2"
      memory: "512Mi"
    storage:
      type: ephemeral
    networking:
      domain: dev.neo-service-layer.io
      ssl: true
```

### 2. Service Configuration

```yaml
# config/services.yaml
services:
  api:
    image: neo-service/api:latest
    port: 3000
    health_check:
      path: /health
      interval: 30s
      timeout: 5s
      retries: 3
    env:
      NODE_ENV: production
      LOG_LEVEL: info

  functions:
    image: neo-service/functions:latest
    port: 3001
    health_check:
      path: /health
      interval: 30s
      timeout: 5s
      retries: 3
    env:
      FUNCTION_TIMEOUT: 60s
      MAX_MEMORY: 256Mi

  triggers:
    image: neo-service/triggers:latest
    port: 3002
    health_check:
      path: /health
      interval: 30s
      timeout: 5s
      retries: 3
    env:
      TRIGGER_WORKERS: 5
      QUEUE_SIZE: 1000
```

## Deployment Commands

### 1. Basic Deployment

```bash
# Deploy to an environment
neo-service deploy --env production

# Deploy specific services
neo-service deploy --env production --services api,functions

# Deploy with specific version
neo-service deploy --env production --version v1.2.3

# Rollback deployment
neo-service rollback --env production --version v1.2.2

# View deployment status
neo-service status --env production

# View deployment history
neo-service history --env production
```

### 2. Advanced Deployment

```bash
# Blue-green deployment
neo-service deploy --env production --strategy blue-green

# Canary deployment
neo-service deploy --env production --strategy canary --weight 10

# Progressive deployment
neo-service deploy --env production --strategy progressive \
  --steps 3 \
  --interval 10m \
  --weight 20

# Deploy with custom configuration
neo-service deploy --env production \
  --config custom-config.yaml \
  --replicas 5 \
  --cpu 2 \
  --memory 4Gi
```

## CI/CD Integration

### 1. GitHub Actions

```yaml
# .github/workflows/deploy.yml
name: Deploy Neo Service Layer

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2

      - name: Setup Node.js
        uses: actions/setup-node@v2
        with:
          node-version: '18'

      - name: Install Dependencies
        run: npm install

      - name: Run Tests
        run: npm test

      - name: Build
        run: npm run build

      - name: Deploy to Staging
        if: github.event_name == 'pull_request'
        run: |
          neo-service deploy \
            --env staging \
            --version ${{ github.sha }}

      - name: Deploy to Production
        if: github.event_name == 'push' && github.ref == 'refs/heads/main'
        run: |
          neo-service deploy \
            --env production \
            --version ${{ github.sha }}
```

### 2. GitLab CI

```yaml
# .gitlab-ci.yml
stages:
  - test
  - build
  - deploy

test:
  stage: test
  script:
    - npm install
    - npm test

build:
  stage: build
  script:
    - npm run build
    - docker build -t neo-service .
  artifacts:
    paths:
      - dist/

deploy_staging:
  stage: deploy
  script:
    - neo-service deploy --env staging
  only:
    - merge_requests

deploy_production:
  stage: deploy
  script:
    - neo-service deploy --env production
  only:
    - main
  when: manual
```

## Monitoring and Logging

### 1. Deployment Monitoring

```bash
# Monitor deployment progress
neo-service monitor deployment --env production

# View deployment logs
neo-service logs deployment --env production

# Check deployment health
neo-service health --env production

# View deployment metrics
neo-service metrics deployment --env production
```

### 2. Service Monitoring

```bash
# Monitor service status
neo-service monitor services --env production

# View service logs
neo-service logs services --env production --service api

# Check service health
neo-service health services --env production

# View service metrics
neo-service metrics services --env production
```

## Scaling and Management

### 1. Scaling Commands

```bash
# Scale services
neo-service scale --env production --service api --replicas 5

# Update resources
neo-service update --env production --service api \
  --cpu 2 \
  --memory 4Gi

# Configure auto-scaling
neo-service autoscale --env production --service api \
  --min 2 \
  --max 10 \
  --cpu 70
```

### 2. Management Commands

```bash
# Restart services
neo-service restart --env production --service api

# Update configuration
neo-service config update --env production \
  --set LOG_LEVEL=debug

# Rotate secrets
neo-service secrets rotate --env production

# Backup data
neo-service backup create --env production
```

## Best Practices

1. **Deployment Strategy**
   - Use environment-specific configurations
   - Implement proper deployment strategies
   - Maintain deployment history
   - Plan for rollbacks

2. **CI/CD Pipeline**
   - Automate deployment process
   - Include proper testing
   - Implement security checks
   - Use deployment approvals

3. **Monitoring**
   - Monitor deployment progress
   - Track service health
   - Collect performance metrics
   - Set up alerts

4. **Scaling**
   - Configure appropriate resources
   - Implement auto-scaling
   - Monitor resource usage
   - Plan for capacity

5. **Security**
   - Secure deployment process
   - Rotate credentials
   - Audit deployments
   - Control access

6. **Documentation**
   - Document deployment process
   - Maintain runbooks
   - Update documentation
   - Share knowledge

7. **Testing**
   - Test in staging environment
   - Validate configurations
   - Verify integrations
   - Test rollback procedures

## Support

For deployment support:
- Email: devops@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/will/neo_service_layer/issues)

## Additional Resources

- [Deployment Architecture](./architecture.md)
- [Environment Setup Guide](./environment-setup.md)
- [CI/CD Pipeline Guide](./cicd.md)
- [Monitoring Guide](./monitoring.md)
- [Troubleshooting Guide](./troubleshooting.md) 