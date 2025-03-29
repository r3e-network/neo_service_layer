# Neo N3 Service Layer CLI Guide

## Overview

The Neo N3 Service Layer CLI provides a command-line interface for interacting with the service. It enables developers to manage functions, triggers, gas allocation, and other features directly from their terminal.

## Installation

### Using Go

```bash
go install github.com/will/neo-service-cli@latest
```

### Using Homebrew

```bash
brew tap will/neo-service
brew install neo-service-cli
```

### Using npm

```bash
npm install -g @neo-service/cli
```

## Configuration

### Initial Setup

```bash
# Initialize CLI configuration
neo-service init

# Configure API endpoint
neo-service config set api.url https://api.neo-service-layer.io/v1

# Authenticate with your Neo N3 wallet
neo-service auth login
```

### Configuration File

The CLI creates a configuration file at `~/.neo-service/config.yaml`:

```yaml
api:
  url: https://api.neo-service-layer.io/v1
  timeout: 30s
auth:
  jwt_token: your_jwt_token_here
gas:
  default_request_amount: "100000"
```

## Basic Commands

### Functions

```bash
# List all functions
neo-service functions list

# Create a new function
neo-service functions create \
  --name price_alert \
  --runtime javascript \
  --file ./price_alert.js

# Get function details
neo-service functions get func_123

# Update a function
neo-service functions update func_123 \
  --file ./updated_price_alert.js

# Delete a function
neo-service functions delete func_123

# Deploy a function
neo-service functions deploy func_123

# View function logs
neo-service functions logs func_123 \
  --tail 100 \
  --follow

# Test a function locally
neo-service functions test func_123 \
  --event '{"type": "price_update"}'
```

### Triggers

```bash
# List all triggers
neo-service triggers list

# Create a schedule trigger
neo-service triggers create \
  --name daily_check \
  --function func_123 \
  --type schedule \
  --schedule "0 0 * * *"

# Create an event trigger
neo-service triggers create \
  --name transfer_monitor \
  --function func_456 \
  --type event \
  --event-type blockchain \
  --event-network neo_n3 \
  --event-contract 0x... \
  --event-name Transfer

# Get trigger details
neo-service triggers get trig_123

# Update a trigger
neo-service triggers update trig_123 \
  --schedule "0 */6 * * *"

# Delete a trigger
neo-service triggers delete trig_123

# View trigger executions
neo-service triggers executions trig_123 \
  --limit 10
```

### Gas Management

```bash
# View gas balance
neo-service gas balance

# Request gas allocation
neo-service gas request \
  --amount 100000

# View gas usage history
neo-service gas history \
  --from 2024-01-01 \
  --to 2024-03-27

# Set gas allocation limits
neo-service gas limits set \
  --daily 1000000 \
  --function func_123
```

### Price Feed

```bash
# Get current price
neo-service prices get NEO/USD

# List available trading pairs
neo-service prices list

# View price history
neo-service prices history NEO/USD \
  --from 2024-03-26 \
  --to 2024-03-27 \
  --interval 1h
```

### Secrets Management

```bash
# List secrets
neo-service secrets list

# Create a secret
neo-service secrets create \
  --name api_key \
  --value secret_value \
  --description "API key for external service"

# Get secret details
neo-service secrets get sec_123

# Update a secret
neo-service secrets update sec_123 \
  --value new_secret_value

# Delete a secret
neo-service secrets delete sec_123
```

## Advanced Usage

### Project Management

```bash
# Initialize a new project
neo-service init my-project

# Project structure:
my-project/
  ├── functions/
  │   └── price_alert/
  │       ├── index.js
  │       └── package.json
  ├── triggers/
  │   └── daily_check.yaml
  ├── secrets/
  │   └── secrets.yaml
  └── neo-service.yaml

# Deploy entire project
neo-service deploy

# View project status
neo-service status
```

### Batch Operations

```bash
# Deploy multiple functions
neo-service functions deploy-batch \
  --dir ./functions

# Create multiple triggers
neo-service triggers create-batch \
  --file triggers.yaml

# Update multiple secrets
neo-service secrets update-batch \
  --file secrets.yaml
```

### Environment Management

```bash
# List environments
neo-service env list

# Create environment
neo-service env create staging

# Set environment variables
neo-service env set staging \
  API_KEY=value \
  DB_URL=value

# Switch environment
neo-service env use staging

# Deploy to environment
neo-service deploy --env staging
```

### Monitoring

```bash
# View service health
neo-service status

# Monitor function metrics
neo-service monitor functions \
  --function func_123 \
  --metric executions \
  --interval 1m

# View system metrics
neo-service monitor system \
  --metric cpu \
  --interval 5s

# Export metrics
neo-service monitor export \
  --from 2024-03-26 \
  --to 2024-03-27 \
  --format csv
```

## Shell Completion

### Bash

```bash
# Add to ~/.bashrc
source <(neo-service completion bash)
```

### Zsh

```bash
# Add to ~/.zshrc
source <(neo-service completion zsh)
```

### Fish

```bash
# Add to ~/.config/fish/config.fish
neo-service completion fish | source
```

## Best Practices

1. **Project Organization**
   - Use project templates
   - Follow consistent naming conventions
   - Organize functions by domain

2. **Version Control**
   - Version control your functions
   - Use git for project management
   - Tag releases appropriately

3. **Security**
   - Rotate secrets regularly
   - Use environment variables
   - Follow least privilege principle

4. **Monitoring**
   - Monitor function performance
   - Set up alerts for failures
   - Track resource usage

5. **Testing**
   - Test functions locally
   - Use test environments
   - Implement CI/CD pipelines

## Support

For CLI support:
- Email: cli@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/will/neo_service_layer/issues) 