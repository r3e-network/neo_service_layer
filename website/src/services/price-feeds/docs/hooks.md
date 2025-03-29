# Price Feed Hooks Documentation

This document describes the React hooks used in the price feeds service. These hooks handle fetching, updating, and managing price data from various sources.

## Hooks

### `usePriceConfig`

Manages the configuration for price feeds.

```typescript
function usePriceConfig(): UsePriceConfigResult
```

**Returns:**
An object containing:
- `config`: The current price feed configuration
- `isLoading`: Boolean indicating if the config is loading
- `error`: Error message if any
- `updateConfig`: Function to update the configuration

**Implementation Notes:**
- Uses WebSocket for real-time updates
- Implements the useWebSocketEvents hook for subscription management
- Handles connection errors and reconnection

### `usePriceFeed`

Fetches and manages price feed data for a specific symbol.

```typescript
function usePriceFeed(symbol: string): UsePriceFeedResult
```

**Parameters:**
- `symbol`: The price feed symbol (e.g., "BTC")

**Returns:**
An object containing:
- `data`: The current price data
- `isLoading`: Boolean indicating if data is loading
- `error`: Error message if any
- `subscribe`: Function to subscribe to price updates
- `unsubscribe`: Function to unsubscribe from price updates

**Implementation Notes:**
- Uses WebSocket for real-time price updates
- Implements the useWebSocketEvents hook for subscription management
- Automatically unsubscribes when the component unmounts

### `usePriceHistory`

Fetches historical price data for a specific symbol.

```typescript
function usePriceHistory(symbol: string): UsePriceHistoryResult
```

**Parameters:**
- `symbol`: The price feed symbol (e.g., "BTC")

**Returns:**
An object containing:
- `data`: Array of historical price data points
- `isLoading`: Boolean indicating if data is loading
- `error`: Error message if any
- `fetchHistory`: Function to fetch history for a specific time range

**Implementation Notes:**
- Uses REST API for fetching historical data
- Supports different time ranges (e.g., "24h", "7d", "30d")
- Automatically fetches 24-hour history on mount

### `usePriceMetrics`

Fetches and manages price metrics data.

```typescript
function usePriceMetrics(): UsePriceMetricsResult
```

**Returns:**
An object containing:
- `metrics`: The current price metrics
- `isLoading`: Boolean indicating if metrics are loading
- `error`: Error message if any
- `subscribe`: Function to subscribe to metrics updates
- `unsubscribe`: Function to unsubscribe from metrics updates

**Implementation Notes:**
- Uses WebSocket for real-time metrics updates
- Implements the useWebSocketEvents hook for subscription management
- Automatically unsubscribes when the component unmounts

### `usePriceSources`

Fetches and manages price source data.

```typescript
function usePriceSources(symbol: string): UsePriceSourcesResult
```

**Parameters:**
- `symbol`: The price feed symbol (e.g., "BTC")

**Returns:**
An object containing:
- `sources`: Array of price sources
- `isLoading`: Boolean indicating if sources are loading
- `error`: Error message if any
- `subscribe`: Function to subscribe to source updates
- `unsubscribe`: Function to unsubscribe from source updates

**Implementation Notes:**
- Uses WebSocket for real-time source updates
- Implements the useWebSocketEvents hook for subscription management
- Automatically unsubscribes when the component unmounts

## WebSocket Integration

All hooks that use WebSocket connections have been updated to use the `useWebSocketEvents` hook, which provides:

1. Consistent connection management
2. Automatic reconnection on connection loss
3. Proper event subscription and unsubscription
4. Error handling and reporting

## TypeScript Updates

The hooks have been updated to use proper TypeScript typing:

1. All React imports use the correct syntax: `import React from 'react'`
2. State types are explicitly defined
3. Return types are defined using interfaces from the types directory
4. Error handling properly narrows types

## API Endpoints

The hooks use constants defined in `constants.ts` for API endpoints:

- `API_ENDPOINT`: For REST API calls
- `WS_ENDPOINT`: For WebSocket connections

This ensures consistency and makes it easy to change endpoints in one place.
