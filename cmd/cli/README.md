# Neo N3 Service Layer CLI

Command-line interface for managing the Neo N3 Service Layer.

## Installation

```bash
go install github.com/will/neo_service_layer/cmd/cli
```

## Configuration

The CLI requires a configuration file that specifies connection details and settings for the Neo N3 Service Layer. By default, it looks for `config.yaml` in the current directory.

You can specify a different configuration file using the `--config` flag:

```bash
cli --config /path/to/config.yaml
```

## Commands

### Accounts
- `accounts create` - Create a new Neo N3 account
- `accounts list` - List all accounts
- `accounts delete` - Delete an account

### Secrets
- `secrets set` - Set a secret value
- `secrets get` - Get a secret value
- `secrets list` - List all secrets
- `secrets delete` - Delete a secret

### Contracts
- `contracts deploy` - Deploy a smart contract
- `contracts upgrade` - Upgrade a smart contract
- `contracts list` - List all deployed contracts
- `contracts info` - Get information about a deployed contract

### Functions
- `functions create` - Create a new function
- `functions test` - Test a function
- `functions list` - List all functions
- `functions delete` - Delete a function
- `functions logs` - View function execution logs

### Triggers
- `triggers create` - Create a new trigger
- `triggers list` - List all triggers
- `triggers delete` - Delete a trigger
- `triggers enable` - Enable a trigger
- `triggers disable` - Disable a trigger

### Price Feeds
- `pricefeeds create` - Create a new price feed
- `pricefeeds list` - List all price feeds
- `pricefeeds delete` - Delete a price feed
- `pricefeeds price` - Get current price from a feed
- `pricefeeds history` - Get price history from a feed

### Gas Bank
- `gasbank status` - Show gas bank status
- `gasbank deposit` - Deposit GAS into the bank
- `gasbank withdraw` - Withdraw GAS from the bank
- `gasbank allocations` - List current gas allocations
- `gasbank usage` - Show gas usage history

### Metrics
- `metrics show` - Show current metrics
- `metrics export` - Export metrics data
- `metrics alerts` - Show metric alerts

### Logs
- `logs show` - Show live logs
- `logs search` - Search logs
- `logs export` - Export logs

### Health
- `health check` - Run health check
- `health status` - Show current health status
- `health history` - Show health check history

## Global Flags

- `--config` - Path to configuration file
- `--verbose` - Enable verbose logging

## Examples

Create a new account:
```bash
cli accounts create --name myaccount
```

Deploy a smart contract:
```bash
cli contracts deploy --name MyContract --version 1.0.0 --manifest contract.manifest.json --nef contract.nef
```

Create a function:
```bash
cli functions create --name myfunction --source function.py --runtime python3.9
```

Create a trigger:
```bash
cli triggers create --name mytrigger --function myfunction --schedule "*/5 * * * *"
```

Monitor logs:
```bash
cli logs show --service api --follow
```

Check system health:
```bash
cli health check
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request