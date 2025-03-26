# Neo N3 Service Layer CLI

A command-line interface for managing Neo N3 Service Layer components including accounts, triggers, functions, price feeds, and gas banks.

## Installation

```bash
go install github.com/will/neo_service_layer/cmd/cli@latest
```

## Configuration

The CLI can be configured using a YAML configuration file. By default, it looks for a file named `config.yaml` in the current directory. You can specify a different configuration file using the `--config` flag.

Example configuration:

```yaml
environment: development
logLevel: info
api:
  host: localhost
  port: 8080
  endpoint: http://localhost:10332
  timeout: 30s
  enableCors: true
  maxRequestBodySize: 10485760 # 10MB
```

## Usage

### Global Flags

- `--config`: Path to configuration file
- `--verbose`: Enable verbose logging

### Commands

#### Triggers

Manage function triggers that invoke functions based on events, schedules, or contract conditions.

```bash
# Create a new trigger
cli triggers create \
  --name my-trigger \
  --function my-function \
  --schedule "*/5 * * * *" \
  --params '{"key": "value"}'

# Create a contract event trigger
cli triggers create \
  --name contract-trigger \
  --function handle-event \
  --contract 0x1234...5678 \
  --event Transfer

# Create a contract method trigger
cli triggers create \
  --name method-trigger \
  --function check-balance \
  --contract 0x1234...5678 \
  --method balanceOf \
  --condition "result > 100"

# List all triggers
cli triggers list

# List active triggers
cli triggers list --status active

# Get trigger details in JSON format
cli triggers list --format json

# Delete a trigger
cli triggers delete my-trigger

# Force delete an active trigger
cli triggers delete my-trigger --force

# Enable a trigger
cli triggers enable my-trigger

# Disable a trigger
cli triggers disable my-trigger

# View trigger execution history
cli triggers history my-trigger

# View recent history with custom limit
cli triggers history my-trigger --limit 50 --since 1h

# Check trigger status
cli triggers status my-trigger

# Get status in JSON format
cli triggers status my-trigger --format json
```

#### Accounts

Manage Neo N3 accounts.

```bash
# Create a new account
cli accounts create

# Create account with custom name
cli accounts create --name my-account

# List all accounts
cli accounts list

# Delete an account
cli accounts delete my-account
```

## Development

### Prerequisites

- Go 1.21 or later
- Neo N3 node (TestNet or PrivateNet)

### Building

```bash
make build
```

### Testing

```bash
make test
```

### Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.