# GasBank Service

The GasBank service manages the allocation and tracking of NEO GAS for users of the Neo Service Layer.

## Architecture

The GasBank service follows a clean architecture pattern with separate layers for:

1. **Service** - The entry point for other components to interact with the gas bank
2. **Domain Models** - Core business objects and entities
3. **Allocation Management** - Managing user-specific gas allocations
4. **Pool Management** - Managing the global gas pool
5. **Persistence** - Storage for gas allocations and the gas pool
6. **Metrics** - Collection of usage metrics and statistics
7. **Alerting** - Notifications for critical events

![GasBank Architecture](../assets/gasbank-architecture.png)

## Components

### Gas Allocation

The Gas Allocation system allows users to request, use, and release GAS allocations:

- **AllocateGas** - Allocates a specific amount of GAS to a user
- **UseGas** - Records the use of allocated GAS
- **ReleaseGas** - Returns unused GAS to the pool
- **GetAllocation** - Retrieves current allocation information

Gas allocations have a lifecycle:

1. **Creation** - When a user requests GAS
2. **Active** - While the user is consuming GAS
3. **Expiration** - When the allocation time window closes
4. **Release** - When the allocation is no longer needed

### Gas Pool

The Gas Pool manages the global supply of GAS:

- **RefillPool** - Adds GAS to the pool when needed
- **ConsumeGas** - Removes GAS from the pool
- **AddGas** - Manually adds GAS to the pool
- **GetAvailableGas** - Returns the current available GAS

The pool includes automatic refill capabilities:

1. **Threshold Monitoring** - Checks if pool is below threshold
2. **Cooldown Period** - Prevents excessive refill attempts
3. **Transaction Management** - Handles on-chain GAS transfers
4. **Refill Metrics** - Tracks refill success and failure rates

### Usage Policy

Gas usage is governed by configurable policies:

- **MaxAllocationPerUser** - Maximum GAS a single user can allocate
- **MinAllocationAmount** - Minimum GAS allocation size
- **MaxAllocationTime** - Maximum time an allocation can be held
- **RefillThreshold** - Pool level that triggers refills
- **RefillAmount** - Amount of GAS to add during refills
- **CooldownPeriod** - Minimum wait between refill attempts

### Metrics Collection

The service collects comprehensive metrics:

- **TotalAllocated** - Total GAS allocated to users
- **TotalUsed** - Total GAS consumed by users
- **ActiveUsers** - Number of users with active allocations
- **Refills** - Number of successful pool refills
- **FailedRefills** - Number of failed pool refills

## Usage Examples

### Allocating Gas

```go
// Allocate 0.5 GAS to a user
userAddress, _ := util.Uint160DecodeStringLE("0123456789abcdef0123456789abcdef01234567")
amount := big.NewInt(50000000) // 0.5 GAS (in smallest units)
allocation, err := gasBankService.AllocateGas(ctx, userAddress, amount)
```

### Using Gas

```go
// Record the use of 0.1 GAS
useAmount := big.NewInt(10000000) // 0.1 GAS
err := gasBankService.UseGas(ctx, userAddress, useAmount)
```

### Releasing Gas

```go
// Release all allocated gas for a user
err := gasBankService.ReleaseGas(ctx, userAddress)
```

### Refilling the Pool

```go
// Manually trigger a pool refill
err := gasBankService.RefillPool(ctx)
```

## Configuration

When initializing the GasBank service, it's essential to properly configure all components, including the AlertManager. The AlertConfig must be explicitly initialized to avoid nil pointer dereferences during operation, especially in the monitoring routines.

Example configuration:

```go
gasBankConfig := &gasbank.Config{
    InitialGas:              big.NewInt(1000000000), // 10 GAS
    RefillAmount:            big.NewInt(500000000),  // 5 GAS
    RefillThreshold:         big.NewInt(200000000),  // 2 GAS
    MaxAllocationPerUser:    big.NewInt(100000000),  // 1 GAS
    MinAllocationAmount:     big.NewInt(1000000),    // 0.01 GAS
    StoreType:               "memory",
    MaxAllocationTime:       24 * time.Hour,
    CooldownPeriod:          5 * time.Minute,
    ExpirationCheckInterval: 15 * time.Minute,
    MonitorInterval:         5 * time.Minute,
    AlertConfig:             internal.DefaultAlertConfig(), // Important: Always initialize the AlertConfig
}
```

The GasBank service is configured through a configuration structure:

```go
type Config struct {
    // Initial gas amount in the pool
    InitialGas *big.Int
    
    // Amount to refill when threshold is reached
    RefillAmount *big.Int
    
    // Threshold that triggers refills
    RefillThreshold *big.Int
    
    // Maximum allocation per user
    MaxAllocationPerUser *big.Int
    
    // Minimum allocation amount
    MinAllocationAmount *big.Int
    
    // Maximum time allocation can be held
    MaxAllocationTime time.Duration
    
    // Minimum wait between refills
    CooldownPeriod time.Duration
}
```

## Testing

The GasBank service is thoroughly tested with both unit and integration tests. All core functionality has comprehensive test coverage including:

- Allocation management
- Pool management
- Policy enforcement
- Metrics collection
- Error handling

## Best Practices

When using the GasBank service:

1. Set reasonable allocation limits based on expected user activity
2. Configure appropriate refill thresholds to maintain service availability
3. Monitor usage metrics to adjust policies as needed
4. Handle allocation errors gracefully in client applications
5. Set up alerts for low gas conditions and failed refills

## Resources

- [NEO Whitepaper](https://docs.neo.org/docs/en-us/basic/whitepaper.html)
- [NEO GAS System](https://docs.neo.org/docs/en-us/basic/gas.html)
- [Neo-Go SDK](https://pkg.go.dev/github.com/nspcc-dev/neo-go)