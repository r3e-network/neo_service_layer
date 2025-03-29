/**
 * Response types for Neo Service Layer SDK
 */

/**
 * Authentication response
 */
export interface SignatureVerificationResponse {
  /** Whether the signature is valid */
  valid: boolean;
  
  /** NEO address */
  address: string;
  
  /** NEO script hash */
  scriptHash: string;
  
  /** Authentication token (if valid) */
  token?: string;
}

/**
 * System health status
 */
export interface HealthStatus {
  /** Service name */
  name: string;
  
  /** Service status */
  status: 'healthy' | 'unhealthy' | 'warning';
  
  /** Status message */
  message?: string;
  
  /** Additional details */
  details?: Record<string, any>;
}

/**
 * System health response
 */
export interface SystemHealthResponse {
  /** Whether the system is healthy */
  healthy: boolean;
  
  /** Service health statuses */
  services: HealthStatus[];
  
  /** System uptime in seconds */
  uptime: number;
  
  /** Deployment region */
  region: string;
  
  /** Response timestamp */
  timestamp: string;
  
  /** System version */
  version: string;
}

/**
 * System statistics response
 */
export interface StatsResponse {
  /** Total number of functions */
  totalFunctions: number;
  
  /** Total number of function executions */
  totalExecutions: number;
  
  /** Total number of triggers */
  totalTriggers: number;
  
  /** Total number of trigger executions */
  totalTriggerExecutions: number;
  
  /** Total number of users */
  totalUsers: number;
  
  /** Total gas used */
  totalGasUsed: number;
  
  /** Average execution time in milliseconds */
  avgExecutionTime: number;
  
  /** Last updated timestamp */
  lastUpdated: string;
}

/**
 * Pagination metadata
 */
export interface PaginationMeta {
  /** Current page */
  page: number;
  
  /** Page size */
  pageSize: number;
  
  /** Total number of items */
  totalItems: number;
  
  /** Total number of pages */
  totalPages: number;
  
  /** Whether there is a next page */
  hasNext: boolean;
  
  /** Whether there is a previous page */
  hasPrev: boolean;
}

/**
 * Paginated response
 */
export interface PaginatedResponse<T> {
  /** Response items */
  items: T[];
  
  /** Pagination metadata */
  meta: PaginationMeta;
}

/**
 * Error response
 */
export interface ErrorResponse {
  /** Error code */
  code: string;
  
  /** Error message */
  message: string;
  
  /** Error details */
  details?: Record<string, any>;
}
