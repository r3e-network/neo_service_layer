# Neo Service Layer JavaScript SDK API Reference

This document provides a comprehensive reference for the Neo Service Layer JavaScript SDK API.

## Table of Contents

- [Core Client](#core-client)
- [Function Context](#function-context)
- [Services](#services)
  - [Functions Service](#functions-service)
  - [Gas Bank Service](#gas-bank-service)
  - [Price Feed Service](#price-feed-service)
  - [Secrets Service](#secrets-service)
  - [Trigger Service](#trigger-service)
- [Error Handling](#error-handling)
- [Types](#types)

## Core Client

The `NeoServiceLayer` class is the main entry point for the SDK.

### Constructor

```typescript
constructor(config: ClientConfig)
```

**Parameters:**

- `config`: Configuration options for the client
  - `baseUrl`: Base URL for the Neo Service Layer API (default: 'http://localhost:3000')
  - `timeout`: Request timeout in milliseconds (default: 30000)
  - `apiVersion`: API version (default: 'v1')
  - `debug`: Debug mode (default: false)
  - `headers`: Additional headers to include with requests

### Authentication Methods

```typescript
async loginWithPrivateKey(privateKey: string): Promise<void>
```

Authenticates with a private key.

```typescript
async loginWithSignature(address: string, message: string, signature: string): Promise<void>
```

Authenticates with a signature.

```typescript
isAuthenticated(): boolean
```

Checks if the client is authenticated.

```typescript
logout(): void
```

Logs out the current session.

### System Methods

```typescript
async getHealth(): Promise<SystemHealthResponse>
```

Gets the system health status.

```typescript
async getStats(): Promise<StatsResponse>
```

Gets system statistics.

```typescript
async request<T>(method: string, path: string, data?: any, options?: any): Promise<T>
```

Makes a request to the Neo Service Layer API.

## Function Context

The function context provides a simplified interface for JavaScript functions running in the Neo Function Service.

### createFunction

```typescript
function createFunction(
  handler: (context: FunctionContext, ...args: any[]) => Promise<any> | any
): Function
```

Creates a function handler with context.

### FunctionContext Interface

```typescript
interface FunctionContext {
  // Properties
  functionId: string;
  executionId: string;
  owner: string;
  caller?: string;
  parameters: Record<string, any>;
  env: Record<string, string>;
  traceId: string;
  neoServiceLayer: NeoServiceLayer;
  
  // Methods
  log(message: string): void;
  getSecret(key: string): Promise<string>;
  getGasPrice(): Promise<number>;
  getPrice(symbol: string): Promise<number>;
  invokeFunction(functionId: string, parameters?: Record<string, any>): Promise<any>;
}
```

## Services

### Functions Service

The Functions Service allows you to create, manage, and invoke serverless functions.

#### Methods

```typescript
async createFunction(request: FunctionRequest): Promise<Function>
```

Creates a new function.

```typescript
async getFunction(functionId: string): Promise<Function>
```

Gets a function by ID.

```typescript
async updateFunction(functionId: string, updates: FunctionUpdateRequest): Promise<Function>
```

Updates a function.

```typescript
async deleteFunction(functionId: string): Promise<void>
```

Deletes a function.

```typescript
async listFunctions(page: number = 1, pageSize: number = 20): Promise<PaginatedResponse<Function>>
```

Lists all functions.

```typescript
async invokeFunction(invocation: FunctionInvocation): Promise<FunctionExecution>
```

Invokes a function.

```typescript
async getFunctionExecutions(
  functionId: string,
  page: number = 1,
  pageSize: number = 20
): Promise<PaginatedResponse<FunctionExecution>>
```

Gets function executions.

```typescript
async getFunctionPermissions(functionId: string): Promise<FunctionPermissions>
```

Gets function permissions.

```typescript
async updateFunctionPermissions(
  functionId: string,
  permissions: FunctionPermissionsUpdateRequest
): Promise<FunctionPermissions>
```

Updates function permissions.

### Gas Bank Service

The Gas Bank Service allows you to manage Neo gas allocation and distribution.

#### Methods

```typescript
async getBalance(address: string): Promise<number>
```

Gets gas balance for an address.

```typescript
async allocateGas(request: GasAllocationRequest): Promise<GasAllocation>
```

Allocates gas to an address.

```typescript
async getAllocation(address: string): Promise<GasAllocation>
```

Gets gas allocation for an address.

```typescript
async updateAllocation(address: string, amount: number): Promise<GasAllocation>
```

Updates gas allocation for an address.

```typescript
async deactivateAllocation(address: string): Promise<void>
```

Deactivates gas allocation for an address.

```typescript
async listAllocations(page: number = 1, pageSize: number = 20): Promise<PaginatedResponse<GasAllocation>>
```

Lists all gas allocations.

```typescript
async getTransactions(
  address: string,
  page: number = 1,
  pageSize: number = 20
): Promise<PaginatedResponse<GasTransaction>>
```

Gets gas transactions for an address.

```typescript
async estimateGas(txData: any): Promise<number>
```

Estimates gas for a transaction.

```typescript
async getGasPrice(): Promise<number>
```

Gets the current gas price.

### Price Feed Service

The Price Feed Service allows you to access oracle services for price data.

#### Methods

```typescript
async getPrice(symbol: string): Promise<PriceFeed>
```

Gets price for a symbol.

```typescript
async getPrices(symbols: string[]): Promise<Record<string, PriceFeed>>
```

Gets prices for multiple symbols.

```typescript
async listPriceFeeds(page: number = 1, pageSize: number = 20): Promise<PaginatedResponse<PriceFeed>>
```

Lists all price feeds.

```typescript
async getPriceHistory(
  symbol: string,
  interval: string = '1d',
  limit: number = 30
): Promise<Array<{ timestamp: string; price: number }>>
```

Gets price history for a symbol.

```typescript
async getSources(): Promise<string[]>
```

Gets price sources.

```typescript
async subscribe(
  symbol: string,
  callback: (priceFeed: PriceFeed) => void
): Promise<string>
```

Subscribes to price updates.

```typescript
async unsubscribe(subscriptionId: string): Promise<void>
```

Unsubscribes from price updates.

```typescript
async updatePrice(request: PriceFeedUpdateRequest): Promise<PriceFeed>
```

Updates a price feed (for oracle providers only).

### Secrets Service

The Secrets Service allows you to securely store and retrieve sensitive information.

#### Methods

```typescript
async storeSecret(request: SecretRequest): Promise<Secret>
```

Stores a secret.

```typescript
async getSecret(key: string): Promise<string>
```

Gets a secret.

```typescript
async deleteSecret(key: string): Promise<void>
```

Deletes a secret.

```typescript
async listSecrets(page: number = 1, pageSize: number = 20): Promise<PaginatedResponse<Secret>>
```

Lists all secrets (metadata only, no values).

```typescript
async hasSecret(key: string): Promise<boolean>
```

Checks if a secret exists.

```typescript
async updateSecret(
  key: string,
  value: string,
  description?: string,
  tags?: string[]
): Promise<Secret>
```

Updates a secret.

```typescript
async getSecretsByTag(
  tag: string,
  page: number = 1,
  pageSize: number = 20
): Promise<PaginatedResponse<Secret>>
```

Gets secrets by tag.

### Trigger Service

The Trigger Service allows you to monitor blockchain events and set up automated execution.

#### Methods

```typescript
async createTrigger(request: TriggerRequest): Promise<Trigger>
```

Creates a new trigger.

```typescript
async getTrigger(triggerId: string): Promise<Trigger>
```

Gets a trigger by ID.

```typescript
async updateTrigger(
  triggerId: string,
  updates: Partial<TriggerRequest> & { status?: TriggerStatus }
): Promise<Trigger>
```

Updates a trigger.

```typescript
async deleteTrigger(triggerId: string): Promise<void>
```

Deletes a trigger.

```typescript
async listTriggers(
  page: number = 1,
  pageSize: number = 20,
  type?: TriggerType
): Promise<PaginatedResponse<Trigger>>
```

Lists all triggers.

```typescript
async getTriggerExecutions(
  triggerId: string,
  page: number = 1,
  pageSize: number = 20
): Promise<PaginatedResponse<FunctionExecution>>
```

Gets trigger executions.

```typescript
async getTriggerMetrics(triggerId: string): Promise<any>
```

Gets trigger metrics.

```typescript
async executeTrigger(
  triggerId: string,
  parameters?: Record<string, any>
): Promise<FunctionExecution>
```

Manually executes a trigger.

```typescript
async enableTrigger(triggerId: string): Promise<Trigger>
```

Enables a trigger.

```typescript
async disableTrigger(triggerId: string): Promise<Trigger>
```

Disables a trigger.

## Error Handling

The SDK provides a comprehensive error handling system. All errors thrown by the SDK extend the `NeoServiceLayerError` class.

### Error Classes

```typescript
class NeoServiceLayerError extends Error
```

Base error class for all SDK errors.

```typescript
class ApiError extends NeoServiceLayerError
```

Error thrown when an API request fails.

```typescript
class AuthenticationError extends NeoServiceLayerError
```

Error thrown when authentication fails.

```typescript
class ValidationError extends NeoServiceLayerError
```

Error thrown when validation fails.

```typescript
class NotFoundError extends NeoServiceLayerError
```

Error thrown when a resource is not found.

```typescript
class FunctionError extends NeoServiceLayerError
```

Error thrown when a function execution fails.

```typescript
class GasBankError extends NeoServiceLayerError
```

Error thrown when gas bank operations fail.

## Types

### Models

```typescript
enum FunctionStatus {
  ACTIVE = 'active',
  DISABLED = 'disabled',
  ERROR = 'error'
}
```

```typescript
enum Runtime {
  JAVASCRIPT = 'javascript'
}
```

```typescript
interface Function {
  id: string;
  name: string;
  description: string;
  owner: string;
  code: string;
  runtime: Runtime;
  status: FunctionStatus;
  triggers: string[];
  createdAt: string;
  updatedAt: string;
  lastExecuted: string;
  metadata: Record<string, any>;
}
```

```typescript
enum ExecutionStatus {
  PENDING = 'pending',
  RUNNING = 'running',
  SUCCESS = 'success',
  ERROR = 'error',
  TIMEOUT = 'timeout'
}
```

```typescript
interface FunctionExecution {
  id: string;
  functionId: string;
  trigger?: string;
  status: ExecutionStatus;
  startTime: string;
  endTime: string;
  duration: number;
  memoryUsed: number;
  parameters: Record<string, any>;
  result: any;
  logs: string[];
  error?: string;
  invokedBy: string;
  batchId?: string;
  costInGas: number;
  traceId: string;
}
```

```typescript
interface FunctionInvocation {
  functionId: string;
  parameters: Record<string, any>;
  async?: boolean;
  caller?: string;
  traceId?: string;
  idempotency?: string;
}
```

```typescript
interface FunctionPermissions {
  functionId: string;
  owner: string;
  allowedUsers: string[];
  public: boolean;
  readOnly: boolean;
}
```

```typescript
interface GasAllocation {
  id: string;
  address: string;
  amount: number;
  remaining: number;
  createdAt: string;
  expiresAt: string;
  active: boolean;
}
```

```typescript
interface GasTransaction {
  id: string;
  address: string;
  amount: number;
  type: string;
  status: string;
  txHash?: string;
  createdAt: string;
  completedAt?: string;
  metadata: Record<string, any>;
}
```

```typescript
interface PriceFeed {
  id: string;
  symbol: string;
  price: number;
  source: string;
  timestamp: string;
  change24h?: number;
  metadata: Record<string, any>;
}
```

```typescript
interface Secret {
  key: string;
  owner: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
  tags: string[];
}
```

```typescript
enum TriggerType {
  BLOCK = 'block',
  TIME = 'time',
  EVENT = 'event',
  PRICE = 'price',
  CUSTOM = 'custom'
}
```

```typescript
enum TriggerStatus {
  ACTIVE = 'active',
  DISABLED = 'disabled',
  ERROR = 'error'
}
```

```typescript
interface Trigger {
  id: string;
  name: string;
  description: string;
  owner: string;
  type: TriggerType;
  condition: Record<string, any>;
  functionId: string;
  parameters: Record<string, any>;
  status: TriggerStatus;
  createdAt: string;
  updatedAt: string;
  lastExecuted?: string;
  metadata: Record<string, any>;
}
```

### Requests

```typescript
interface SignatureVerificationRequest {
  address: string;
  message: string;
  signature: string;
}
```

```typescript
interface FunctionRequest {
  name: string;
  description: string;
  code: string;
  runtime: Runtime;
}
```

```typescript
interface FunctionUpdateRequest {
  name?: string;
  description?: string;
  code?: string;
  runtime?: Runtime;
  status?: string;
}
```

```typescript
interface FunctionPermissionsUpdateRequest {
  allowedUsers?: string[];
  public?: boolean;
  readOnly?: boolean;
}
```

```typescript
interface SecretRequest {
  key: string;
  value: string;
  description?: string;
  tags?: string[];
}
```

```typescript
interface TriggerRequest {
  name: string;
  description: string;
  type: string;
  condition: Record<string, any>;
  functionId: string;
  parameters?: Record<string, any>;
}
```

```typescript
interface GasAllocationRequest {
  address: string;
  amount: number;
  expirySeconds?: number;
}
```

```typescript
interface PriceFeedUpdateRequest {
  symbol: string;
  price: number;
  source: string;
  change24h?: number;
  metadata?: Record<string, any>;
}
```

### Responses

```typescript
interface SignatureVerificationResponse {
  valid: boolean;
  address: string;
  scriptHash: string;
  token?: string;
}
```

```typescript
interface HealthStatus {
  name: string;
  status: 'healthy' | 'unhealthy' | 'warning';
  message?: string;
  details?: Record<string, any>;
}
```

```typescript
interface SystemHealthResponse {
  healthy: boolean;
  services: HealthStatus[];
  uptime: number;
  region: string;
  timestamp: string;
  version: string;
}
```

```typescript
interface StatsResponse {
  totalFunctions: number;
  totalExecutions: number;
  totalTriggers: number;
  totalTriggerExecutions: number;
  totalUsers: number;
  totalGasUsed: number;
  avgExecutionTime: number;
  lastUpdated: string;
}
```

```typescript
interface PaginationMeta {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNext: boolean;
  hasPrev: boolean;
}
```

```typescript
interface PaginatedResponse<T> {
  items: T[];
  meta: PaginationMeta;
}
```

```typescript
interface ErrorResponse {
  code: string;
  message: string;
  details?: Record<string, any>;
}
```
