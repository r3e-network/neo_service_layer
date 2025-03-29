# Blockchain Utilities Documentation

This document describes the blockchain utility functions used in the price feeds service. These utilities handle the signing, verification, and formatting of price data for blockchain interactions.

## Functions

### `signPriceData`

Signs price data using a provided signer.

```typescript
async function signPriceData(
  signer: ethers.Signer,
  symbol: string,
  price: number,
  timestamp: number
): Promise<string>
```

**Parameters:**
- `signer`: An ethers.js Signer instance
- `symbol`: The price feed symbol (e.g., "BTC")
- `price`: The price value as a number
- `timestamp`: The timestamp of the price data

**Returns:**
A Promise that resolves to the signature string.

**Implementation Notes:**
- Uses ethers.js v6 API for signing
- Converts price to the correct decimal format using `parseUnits`
- Creates a keccak256 hash of the packed data before signing

### `verifyPriceData`

Verifies the signature of price data.

```typescript
function verifyPriceData(
  signature: string,
  symbol: string,
  price: number,
  timestamp: number
): string
```

**Parameters:**
- `signature`: The signature to verify
- `symbol`: The price feed symbol
- `price`: The price value
- `timestamp`: The timestamp of the price data

**Returns:**
The address of the signer if valid.

**Implementation Notes:**
- Uses ethers.js v6 API for verification
- Recreates the message hash using the same parameters as signing

### `formatPriceForChain`

Formats a price value for on-chain use.

```typescript
function formatPriceForChain(price: number): string
```

**Parameters:**
- `price`: The price value as a number

**Returns:**
The price formatted as a string with the correct number of decimals.

### `parsePriceFromChain`

Parses a price value from the blockchain format.

```typescript
function parsePriceFromChain(priceHex: string): number
```

**Parameters:**
- `priceHex`: The price value from the blockchain

**Returns:**
The price as a JavaScript number.

### `calculateSourcesHash`

Calculates a hash of price sources for verification.

```typescript
function calculateSourcesHash(sources: { id: string; price: number }[]): string
```

**Parameters:**
- `sources`: An array of price sources, each with an id and price

**Returns:**
A hash string representing the sources.

**Implementation Notes:**
- Sources are sorted by ID before hashing
- Each source's price is formatted with the correct decimals

### `estimateGasForPriceUpdate`

Estimates the gas required for a price update transaction.

```typescript
async function estimateGasForPriceUpdate(
  contract: ethers.Contract,
  symbol: string,
  price: number,
  timestamp: number,
  sourcesHash: string,
  signature: string
): Promise<bigint>
```

**Parameters:**
- `contract`: The ethers.js Contract instance
- `symbol`: The price feed symbol
- `price`: The price value
- `timestamp`: The timestamp of the price data
- `sourcesHash`: The hash of the price sources
- `signature`: The signature of the price data

**Returns:**
A Promise that resolves to the estimated gas as a bigint.

**Implementation Notes:**
- Uses ethers.js v6 API for gas estimation
- Accesses contract functions using the `getFunction` method

### `getNetworkConfig`

Gets the configuration for a specific blockchain network.

```typescript
function getNetworkConfig(chainId: number): {
  rpcUrl: string;
  contractAddress: string;
  explorerUrl: string;
}
```

**Parameters:**
- `chainId`: The ID of the blockchain network

**Returns:**
An object containing the RPC URL, contract address, and explorer URL.

**Implementation Notes:**
- Supports multiple networks (mainnet and testnet)
- Throws an error for unsupported networks

### `waitForPriceUpdate`

Waits for a price update to be confirmed on the blockchain.

```typescript
async function waitForPriceUpdate(
  contract: ethers.Contract,
  symbol: string,
  expectedPrice: number,
  timeout: number = 60000
): Promise<boolean>
```

**Parameters:**
- `contract`: The ethers.js Contract instance
- `symbol`: The price feed symbol
- `expectedPrice`: The expected price value
- `timeout`: The maximum time to wait in milliseconds (default: 60000)

**Returns:**
A Promise that resolves to a boolean indicating whether the price was updated.

**Implementation Notes:**
- Uses ethers.js v6 API for contract interaction
- Accesses contract functions using the `getFunction` method
- Checks for price updates every second until timeout
- Considers a price update successful if it's within the deviation threshold

## Ethers.js v6 API Changes

This module has been updated to use ethers.js v6 API. Key changes include:

1. Replaced `ethers.utils.solidityKeccak256` with `ethers.solidityPackedKeccak256`
2. Replaced `ethers.utils.arrayify` with `ethers.getBytes`
3. Replaced `ethers.utils.parseUnits` with `ethers.parseUnits`
4. Replaced `ethers.utils.formatUnits` with `ethers.formatUnits`
5. Replaced `contract.estimateGas.functionName` with `contract.getFunction("functionName").estimateGas`
6. Replaced `contract.functionName()` with `contract.getFunction("functionName").staticCall()`
7. Changed return type from `ethers.BigNumber` to native JavaScript `bigint`
