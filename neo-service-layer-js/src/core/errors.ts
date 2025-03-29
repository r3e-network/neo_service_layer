/**
 * Error handling for Neo Service Layer SDK
 */

/**
 * Base error class for Neo Service Layer SDK
 */
export class NeoServiceLayerError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'NeoServiceLayerError';
    Object.setPrototypeOf(this, NeoServiceLayerError.prototype);
  }
}

/**
 * Error thrown when API requests fail
 */
export class ApiError extends NeoServiceLayerError {
  public statusCode: number;
  public data: any;

  constructor(message: string, statusCode: number, data?: any) {
    super(message);
    this.name = 'ApiError';
    this.statusCode = statusCode;
    this.data = data;
    Object.setPrototypeOf(this, ApiError.prototype);
  }
}

/**
 * Error thrown when authentication fails
 */
export class AuthenticationError extends NeoServiceLayerError {
  constructor(message: string) {
    super(message);
    this.name = 'AuthenticationError';
    Object.setPrototypeOf(this, AuthenticationError.prototype);
  }
}

/**
 * Error thrown when a function execution fails
 */
export class FunctionExecutionError extends NeoServiceLayerError {
  public functionId: string;
  public executionId?: string;
  public logs?: string[];

  constructor(message: string, functionId: string, executionId?: string, logs?: string[]) {
    super(message);
    this.name = 'FunctionExecutionError';
    this.functionId = functionId;
    this.executionId = executionId;
    this.logs = logs;
    Object.setPrototypeOf(this, FunctionExecutionError.prototype);
  }
}

/**
 * Function execution error
 */
export class FunctionError extends NeoServiceLayerError {
  constructor(message: string, public functionId?: string) {
    super(`Function error: ${message}`);
    this.name = 'FunctionError';
  }
}

/**
 * Error thrown when gas bank operations fail
 */
export class GasBankError extends NeoServiceLayerError {
  constructor(message: string) {
    super(`Gas bank error: ${message}`);
    this.name = 'GasBankError';
  }
}

/**
 * Error thrown when validation fails
 */
export class ValidationError extends NeoServiceLayerError {
  public field?: string;
  public details?: any;

  constructor(message: string, field?: string, details?: any) {
    super(message);
    this.name = 'ValidationError';
    this.field = field;
    this.details = details;
    Object.setPrototypeOf(this, ValidationError.prototype);
  }
}

/**
 * Error thrown when a resource is not found
 */
export class NotFoundError extends NeoServiceLayerError {
  public resourceType: string;
  public resourceId: string;

  constructor(resourceType: string, resourceId: string) {
    super(`${resourceType} with ID ${resourceId} not found`);
    this.name = 'NotFoundError';
    this.resourceType = resourceType;
    this.resourceId = resourceId;
    Object.setPrototypeOf(this, NotFoundError.prototype);
  }
}
