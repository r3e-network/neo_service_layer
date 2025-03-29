/**
 * Model types for Neo Service Layer SDK
 */

/**
 * Function status
 */
export enum FunctionStatus {
  ACTIVE = 'active',
  DISABLED = 'disabled',
  ERROR = 'error'
}

/**
 * Runtime type
 */
export enum Runtime {
  JAVASCRIPT = 'javascript'
}

/**
 * Function model
 */
export interface Function {
  /** Unique identifier */
  id: string;
  
  /** Function name */
  name: string;
  
  /** Function description */
  description: string;
  
  /** Function owner address */
  owner: string;
  
  /** Function code */
  code: string;
  
  /** Function runtime */
  runtime: Runtime;
  
  /** Function status */
  status: FunctionStatus;
  
  /** Associated triggers */
  triggers: string[];
  
  /** When the function was created */
  createdAt: string;
  
  /** When the function was last updated */
  updatedAt: string;
  
  /** When the function was last executed */
  lastExecuted: string;
  
  /** Additional metadata */
  metadata: Record<string, any>;
}

/**
 * Function execution status
 */
export enum ExecutionStatus {
  PENDING = 'pending',
  RUNNING = 'running',
  SUCCESS = 'success',
  ERROR = 'error',
  TIMEOUT = 'timeout'
}

/**
 * Function execution model
 */
export interface FunctionExecution {
  /** Unique identifier */
  id: string;
  
  /** Function ID */
  functionId: string;
  
  /** Trigger ID (if applicable) */
  trigger?: string;
  
  /** Execution status */
  status: ExecutionStatus;
  
  /** When execution started */
  startTime: string;
  
  /** When execution ended */
  endTime: string;
  
  /** Execution duration in milliseconds */
  duration: number;
  
  /** Memory used in bytes */
  memoryUsed: number;
  
  /** Execution parameters */
  parameters: Record<string, any>;
  
  /** Execution result */
  result: any;
  
  /** Execution logs */
  logs: string[];
  
  /** Error message (if any) */
  error?: string;
  
  /** Who invoked the execution */
  invokedBy: string;
  
  /** Batch ID (if part of a batch) */
  batchId?: string;
  
  /** Execution cost in GAS */
  costInGas: number;
  
  /** For request tracing */
  traceId: string;
}

/**
 * Function invocation request
 */
export interface FunctionInvocation {
  /** Function ID */
  functionId: string;
  
  /** Invocation parameters */
  parameters: Record<string, any>;
  
  /** Whether to run asynchronously */
  async?: boolean;
  
  /** Who is calling the function */
  caller?: string;
  
  /** For request tracing */
  traceId?: string;
  
  /** Idempotency key */
  idempotency?: string;
}

/**
 * Function permissions model
 */
export interface FunctionPermissions {
  /** Function ID */
  functionId: string;
  
  /** Function owner */
  owner: string;
  
  /** Users allowed to invoke */
  allowedUsers: string[];
  
  /** Whether the function is public */
  public: boolean;
  
  /** Whether the function is read-only */
  readOnly: boolean;
}

/**
 * Gas allocation model
 */
export interface GasAllocation {
  /** Unique identifier */
  id: string;
  
  /** Account address */
  address: string;
  
  /** Allocated gas amount */
  amount: number;
  
  /** Remaining gas amount */
  remaining: number;
  
  /** When the allocation was created */
  createdAt: string;
  
  /** When the allocation expires */
  expiresAt: string;
  
  /** Whether the allocation is active */
  active: boolean;
}

/**
 * Gas transaction model
 */
export interface GasTransaction {
  /** Unique identifier */
  id: string;
  
  /** Account address */
  address: string;
  
  /** Transaction type */
  type: 'deposit' | 'withdraw' | 'usage';
  
  /** Gas amount */
  amount: number;
  
  /** Transaction reference */
  reference?: string;
  
  /** When the transaction occurred */
  timestamp: string;
  
  /** Transaction status */
  status: 'pending' | 'confirmed' | 'failed';
  
  /** Additional metadata */
  metadata: Record<string, any>;
}

/**
 * Price feed model
 */
export interface PriceFeed {
  /** Unique identifier */
  id: string;
  
  /** Asset symbol */
  symbol: string;
  
  /** Price in USD */
  price: number;
  
  /** Price source */
  source: string;
  
  /** When the price was updated */
  timestamp: string;
  
  /** Price change percentage (24h) */
  change24h?: number;
  
  /** Additional metadata */
  metadata: Record<string, any>;
}

/**
 * Secret model
 */
export interface Secret {
  /** Secret key */
  key: string;
  
  /** Secret owner */
  owner: string;
  
  /** Secret description */
  description?: string;
  
  /** When the secret was created */
  createdAt: string;
  
  /** When the secret was last updated */
  updatedAt: string;
  
  /** Secret tags */
  tags: string[];
}

/**
 * Trigger type
 */
export enum TriggerType {
  BLOCK = 'block',
  TRANSACTION = 'transaction',
  CONTRACT = 'contract',
  TIME = 'time',
  EVENT = 'event'
}

/**
 * Trigger status
 */
export enum TriggerStatus {
  ACTIVE = 'active',
  DISABLED = 'disabled',
  ERROR = 'error'
}

/**
 * Trigger model
 */
export interface Trigger {
  /** Unique identifier */
  id: string;
  
  /** Trigger name */
  name: string;
  
  /** Trigger description */
  description: string;
  
  /** Trigger owner address */
  owner: string;
  
  /** Trigger type */
  type: TriggerType;
  
  /** Trigger condition */
  condition: Record<string, any>;
  
  /** Associated function ID */
  functionId: string;
  
  /** Parameters to pass to the function */
  parameters: Record<string, any>;
  
  /** Trigger status */
  status: TriggerStatus;
  
  /** When the trigger was created */
  createdAt: string;
  
  /** When the trigger was last updated */
  updatedAt: string;
  
  /** When the trigger was last executed */
  lastExecuted?: string;
  
  /** Additional metadata */
  metadata: Record<string, any>;
}

/**
 * Transaction status
 */
export enum TransactionStatus {
  CREATED = 'created',
  SIGNED = 'signed',
  SENT = 'sent',
  PENDING = 'pending',
  CONFIRMED = 'confirmed',
  FAILED = 'failed',
  EXPIRED = 'expired'
}

/**
 * Transaction type
 */
export enum TransactionType {
  TRANSFER = 'transfer',
  CONTRACT_INVOKE = 'contract_invoke',
  CONTRACT_DEPLOY = 'contract_deploy',
  CUSTOM = 'custom'
}

/**
 * Transaction model
 */
export interface Transaction {
  /** Unique identifier */
  id: string;
  
  /** Transaction hash */
  hash?: string;
  
  /** Transaction type */
  type: TransactionType;
  
  /** Transaction status */
  status: TransactionStatus;
  
  /** Transaction sender */
  sender: string;
  
  /** Transaction recipient (if applicable) */
  recipient?: string;
  
  /** Transaction amount (if applicable) */
  amount?: number;
  
  /** Transaction asset (if applicable) */
  asset?: string;
  
  /** Contract script hash (if applicable) */
  contractHash?: string;
  
  /** Contract method (if applicable) */
  contractMethod?: string;
  
  /** Contract parameters (if applicable) */
  contractParams?: any[];
  
  /** Transaction fee */
  fee: number;
  
  /** Network fee */
  networkFee: number;
  
  /** System fee */
  systemFee: number;
  
  /** When the transaction was created */
  createdAt: string;
  
  /** When the transaction was signed */
  signedAt?: string;
  
  /** When the transaction was sent */
  sentAt?: string;
  
  /** When the transaction was confirmed */
  confirmedAt?: string;
  
  /** Block number where transaction was confirmed */
  blockNumber?: number;
  
  /** Block index where transaction was confirmed */
  blockIndex?: number;
  
  /** Additional metadata */
  metadata: Record<string, any>;
}
