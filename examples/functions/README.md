# Neo N3 Service Layer Function Examples

This directory contains example functions demonstrating various capabilities of the Neo N3 Service Layer.

## Examples Overview

1. **Price Alert Function** (`price-alert.js`)
   - Monitors NEO/GAS price and sends alerts when thresholds are crossed
   - Demonstrates price feed integration and trigger functionality
   - Uses secrets for notification endpoints

2. **Token Transfer Monitor** (`token-transfer-monitor.js`)
   - Monitors NEP-17 token transfers
   - Shows contract event monitoring capabilities
   - Uses automation for periodic checks

3. **Gas Optimization** (`gas-optimization.js`)
   - Automatically manages gas allocation for a contract
   - Demonstrates gas bank integration
   - Shows automated gas management

4. **Cross-Chain Bridge Monitor** (`bridge-monitor.js`)
   - Monitors cross-chain bridge events
   - Shows complex event processing
   - Demonstrates secure secret management

## Running the Examples

1. Deploy the example functions:
```bash
./bin/cli functions deploy -f examples/functions/price-alert.js
./bin/cli functions deploy -f examples/functions/token-transfer-monitor.js
./bin/cli functions deploy -f examples/functions/gas-optimization.js
./bin/cli functions deploy -f examples/functions/bridge-monitor.js
```

2. Set up required secrets:
```bash
./bin/cli secrets set WEBHOOK_URL "https://your-webhook.com"
./bin/cli secrets set ALERT_EMAIL "alerts@example.com"
```

3. Create triggers:
```bash
./bin/cli triggers create -f examples/functions/triggers/price-alert-trigger.json
./bin/cli triggers create -f examples/functions/triggers/transfer-monitor-trigger.json
```

## Security Considerations

- All functions run in a Trusted Execution Environment (TEE)
- Secrets are encrypted and securely managed
- Function execution is limited by configured resource constraints
- All function calls require proper authentication and authorization

## Best Practices

1. **Error Handling**
   - Always implement proper error handling
   - Use try-catch blocks for critical operations
   - Log errors appropriately

2. **Resource Management**
   - Stay within memory and CPU limits
   - Optimize database queries and API calls
   - Clean up resources after use

3. **Security**
   - Never hardcode secrets in functions
   - Validate all inputs
   - Follow least privilege principle

4. **Monitoring**
   - Use logging for important events
   - Set up alerts for critical failures
   - Monitor resource usage

## Testing

Each example includes test cases demonstrating:
- Unit tests for function logic
- Integration tests with Neo N3 testnet
- Performance tests under load

## Documentation

Each function is documented with:
- Purpose and functionality
- Required permissions and resources
- Expected inputs and outputs
- Error scenarios and handling