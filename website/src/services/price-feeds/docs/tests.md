# Price Feed Tests Documentation

This document describes the test files for the price feeds service, including the mocks, unit tests, and integration tests.

## Mock Files

### `contract.ts`

A mock implementation of the blockchain contract used for price feeds.

```typescript
class MockContract extends EventEmitter {
  private prices: { [symbol: string]: string } = {};

  async updatePrice(
    symbol: string,
    price: bigint,
    timestamp: number,
    sourcesHash: string,
    signature: string
  ) {
    // Implementation...
  }

  async getPrice(symbol: string) {
    // Implementation...
  }

  // Mock contract interface methods
  interface = {
    // Contract interface definition...
  };
}
```

**Implementation Notes:**
- Uses the EventEmitter to simulate contract events
- Stores prices in memory for testing
- Updated to use ethers.js v6 API with native `bigint` instead of `BigNumber`

## Integration Tests

### `api.test.ts`

Tests the REST API and WebSocket endpoints for the price feeds service.

**Test Suites:**
1. **GET /api/price-feeds/:symbol/price**
   - Tests retrieving current price data
   - Tests symbol parameter validation

2. **GET /api/price-feeds/:symbol/historical**
   - Tests retrieving historical price data
   - Tests timeframe parameter validation

3. **GET /api/price-feeds/:symbol/sources**
   - Tests retrieving price source data

4. **PUT /api/price-feeds/:symbol/config**
   - Tests updating configuration with valid data
   - Tests rejecting invalid configuration
   - Tests authentication requirements

5. **WebSocket Integration**
   - Tests broadcasting price updates to connected clients
   - Tests handling multiple subscriptions
   - Tests reconnection and subscription maintenance

6. **Blockchain Integration**
   - Tests syncing on-chain prices with API
   - Tests handling blockchain reorganizations

**Implementation Notes:**
- Uses `supertest` for HTTP request testing
- Uses native WebSocket API for WebSocket testing
- Uses ethers.js v6 API for blockchain interactions
- Creates an HTTP server and WebSocket server for each test suite

### `priceFeed.test.ts`

Tests the price feed service implementation.

**Test Suites:**
1. **Price Calculation**
   - Tests calculating weighted average prices
   - Tests handling missing or stale sources
   - Tests applying deviation thresholds

2. **Source Management**
   - Tests adding and removing price sources
   - Tests source validation

3. **Configuration**
   - Tests applying configuration changes
   - Tests configuration validation

**Implementation Notes:**
- Uses Jest mocks for dependencies
- Tests both success and error cases
- Verifies correct event emission

## Unit Tests

### `blockchain.test.ts`

Tests the blockchain utility functions.

**Test Suites:**
1. **signPriceData**
   - Tests signing price data with different signers
   - Tests signature format

2. **verifyPriceData**
   - Tests verifying valid signatures
   - Tests rejecting invalid signatures

3. **formatPriceForChain**
   - Tests formatting prices with different decimal places

4. **parsePriceFromChain**
   - Tests parsing prices from chain format

5. **calculateSourcesHash**
   - Tests calculating consistent hashes for sources
   - Tests sorting behavior

**Implementation Notes:**
- Uses ethers.js v6 API
- Tests with various input values
- Verifies compatibility with on-chain functions

### `calculations.test.ts`

Tests the price calculation utility functions.

**Test Suites:**
1. **calculateWeightedAverage**
   - Tests with equal weights
   - Tests with custom weights
   - Tests handling zero weights

2. **calculateDeviation**
   - Tests deviation calculation
   - Tests percentage formatting

3. **filterOutliers**
   - Tests removing price outliers
   - Tests preserving valid prices

**Implementation Notes:**
- Tests edge cases (empty arrays, null values)
- Verifies calculation precision
- Tests with realistic price data

## TypeScript Updates

All test files have been updated to use the ethers.js v6 API:

1. Replaced `ethers.utils.parseUnits` with `ethers.parseUnits`
2. Replaced `ethers.BigNumber` with native JavaScript `bigint`
3. Updated WebSocket event handling to use `addEventListener` instead of `on`
4. Updated contract method calls to use the new syntax

## Dependencies

The test files require the following dependencies:

- `jest`: Testing framework
- `supertest`: HTTP testing library
- `@types/supertest`: TypeScript definitions for supertest
- `ws`: WebSocket implementation
- `ethers`: Ethereum library (v6)

These dependencies are listed in the `package.json` file.
