#!/bin/bash

# Set the project name
PROJECT_NAME="neo-service-layer"

# Create the main project directory
mkdir -p $PROJECT_NAME
cd $PROJECT_NAME

# Create the base directory structure
mkdir -p cmd/{server,cli,worker}
mkdir -p config
mkdir -p contracts/{pricefeed,gasbank,trigger,functions,automation}
mkdir -p docs/{architecture,api,guides,development}
mkdir -p internal/common/{config,database/migrations,database/models,logger,blockchain/neo,blockchain/types,security/tee,errors}
mkdir -p internal/services/{pricefeed/providers,gasbank,trigger/handlers,metrics/dashboard,logging,secrets,functions/runtime,api/{middleware,handlers,openapi},automation/strategies}
mkdir -p pkg
mkdir -p scripts
mkdir -p test
mkdir -p web

# Create root level files
touch .env.example
touch README.md
touch go.mod
touch Dockerfile
touch docker-compose.yml
touch .gitignore
touch LICENSE

# Create main entry point files (empty)
touch cmd/server/main.go
touch cmd/cli/main.go
touch cmd/worker/main.go

# Create configuration files (empty)
touch config/app.yaml
touch config/logging.yaml
touch config/metrics.yaml
touch config/pricefeed.yaml
touch config/gasbank.yaml
touch config/trigger.yaml
touch config/functions.yaml
touch config/secrets.yaml
touch config/automation.yaml

# Create common infrastructure files
touch internal/common/config/config.go
touch internal/common/config/defaults.go
touch internal/common/logger/logger.go
touch internal/common/blockchain/neo/client.go
touch internal/common/blockchain/neo/transaction.go
touch internal/common/blockchain/neo/contracts.go
touch internal/common/blockchain/types/common.go
touch internal/common/security/signature.go
touch internal/common/security/encryption.go
touch internal/common/security/tee/enclave.go
touch internal/common/security/tee/attestation.go
touch internal/common/errors/errors.go
touch internal/common/database/connection.go

# Create service files
# Price Feed Service
touch internal/services/pricefeed/service.go
touch internal/services/pricefeed/models.go
touch internal/services/pricefeed/aggregator.go
touch internal/services/pricefeed/provider.go
touch internal/services/pricefeed/publisher.go
touch internal/services/pricefeed/subscriber.go
touch internal/services/pricefeed/validator.go
touch internal/services/pricefeed/providers/binance.go
touch internal/services/pricefeed/providers/coinbase.go
touch internal/services/pricefeed/providers/huobi.go

# Gas Bank Service
touch internal/services/gasbank/service.go
touch internal/services/gasbank/models.go
touch internal/services/gasbank/manager.go
touch internal/services/gasbank/allocation.go
touch internal/services/gasbank/billing.go
touch internal/services/gasbank/monitor.go

# Trigger Service
touch internal/services/trigger/service.go
touch internal/services/trigger/models.go
touch internal/services/trigger/monitor.go
touch internal/services/trigger/executor.go
touch internal/services/trigger/scheduler.go
touch internal/services/trigger/handlers/blockchain.go
touch internal/services/trigger/handlers/api.go
touch internal/services/trigger/handlers/time.go

# Metrics Service
touch internal/services/metrics/service.go
touch internal/services/metrics/models.go
touch internal/services/metrics/collector.go
touch internal/services/metrics/exporter.go
touch internal/services/metrics/dashboard/dashboards.go

# Logging Service
touch internal/services/logging/service.go
touch internal/services/logging/models.go
touch internal/services/logging/formatter.go
touch internal/services/logging/exporter.go
touch internal/services/logging/storage.go

# Secrets Service
touch internal/services/secrets/service.go
touch internal/services/secrets/models.go
touch internal/services/secrets/manager.go
touch internal/services/secrets/permissions.go
touch internal/services/secrets/encryption.go
touch internal/services/secrets/storage.go

# Functions Service
touch internal/services/functions/service.go
touch internal/services/functions/models.go
touch internal/services/functions/manager.go
touch internal/services/functions/executor.go
touch internal/services/functions/validator.go
touch internal/services/functions/compiler.go
touch internal/services/functions/runtime/javascript.go
touch internal/services/functions/runtime/sandbox.go

# API Service
touch internal/services/api/service.go
touch internal/services/api/models.go
touch internal/services/api/middleware/auth.go
touch internal/services/api/middleware/logging.go
touch internal/services/api/middleware/metrics.go
touch internal/services/api/handlers/pricefeed.go
touch internal/services/api/handlers/gasbank.go
touch internal/services/api/handlers/trigger.go
touch internal/services/api/handlers/functions.go
touch internal/services/api/handlers/secrets.go
touch internal/services/api/openapi/spec.yaml

# Contract Automation Service
touch internal/services/automation/service.go
touch internal/services/automation/models.go
touch internal/services/automation/keeper.go
touch internal/services/automation/monitor.go
touch internal/services/automation/executor.go
touch internal/services/automation/strategies/time.go
touch internal/services/automation/strategies/condition.go
touch internal/services/automation/strategies/event.go

# Create smart contract files
touch contracts/pricefeed/PriceFeedConsumer.cs
touch contracts/pricefeed/PriceFeedAggregator.cs
touch contracts/gasbank/GasBank.cs
touch contracts/trigger/TriggerConsumer.cs
touch contracts/functions/FunctionsConsumer.cs
touch contracts/automation/UpkeepContract.cs

# Create documentation files
touch docs/architecture/overview.md
touch docs/api/endpoints.md
touch docs/guides/setup.md
touch docs/guides/pricefeed.md
touch docs/guides/gasbank.md
touch docs/guides/trigger.md
touch docs/guides/functions.md
touch docs/guides/secrets.md
touch docs/guides/automation.md
touch docs/development/contributing.md
touch docs/development/testing.md

echo "Project structure created successfully!"
echo "Total files created: $(find . -type f | wc -l)"
echo "Total directories created: $(find . -type d | wc -l)"
