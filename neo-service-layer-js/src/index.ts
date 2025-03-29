/**
 * Neo Service Layer JavaScript SDK
 * 
 * Main entry point for the SDK, exporting the client and services.
 */

// Export core client
export { NeoServiceLayer } from './core/client';

// Export error classes
export {
  NeoServiceLayerError,
  ApiError,
  AuthenticationError,
  ValidationError,
  NotFoundError,
  FunctionError,
  GasBankError
} from './core/errors';

// Export services
export { FunctionsService } from './services/functions';
export { GasBankService } from './services/gasbank';
export { PriceFeedService } from './services/pricefeed';
export { SecretsService } from './services/secrets';
export { TriggerService } from './services/trigger';
export { TransactionService } from './services/transaction';

// Export function context utilities
export {
  createFunction,
  createFunctionContext,
  wrapFunctionHandler
} from './utils/function-context';

// Export types
export * from './types/models';
export * from './types/requests';
export * from './types/responses';
export * from './types/config';
