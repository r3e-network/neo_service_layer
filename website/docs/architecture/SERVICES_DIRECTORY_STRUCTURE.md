# Services Directory Structure

## Overview

This document describes the organization of services in the Neo Service Layer application. The services are now organized in a structured directory approach to improve code organization, maintainability, and scalability.

## Directory Structure

All services are now located in the `/src/app/services` directory, with each service having its own subdirectory. This structure allows for better organization of related files and makes it easier to locate and maintain service code.

```
/src/app/services/
├── auth/
│   ├── index.ts        # Exports the AuthenticationService and related types
│   └── service.ts      # Implementation of the AuthenticationService
├── cache/
│   ├── index.ts        # Exports the CacheService and related types
│   └── service.ts      # Implementation of the CacheService
├── metrics/
│   ├── index.ts        # Exports the MetricsService and related types
│   └── service.ts      # Implementation of the MetricsService
├── neo-contract/
│   ├── index.ts        # Exports the NeoContractService and related types
│   └── service.ts      # Implementation of the NeoContractService
├── price-feeds/
│   ├── components/     # UI components related to price feeds
│   ├── hooks/          # React hooks for price feed data
│   ├── index.ts        # Exports the PriceFeedService and related types
│   └── service.ts      # Implementation of the PriceFeedService
└── websocket/
    ├── index.ts        # Exports the websocketService singleton and related types
    └── service.ts      # Implementation of the WebSocketService
```

## Import Path Configuration

The TypeScript configuration has been updated to support the new directory structure. The following path mappings have been added to `tsconfig.json`:

```json
"paths": {
  "@/*": ["./src/*"],
  "@/server/*": ["./server/*"],
  "@/services/*": ["./src/app/services/*"],
  "@/app/*": ["./src/app/*"]
}
```

This allows for consistent import paths across the application:

```typescript
// Import a service
import { PriceFeedService } from '@/services/price-feeds';

// Import a type
import type { PriceData } from '@/services/price-feeds';
```

## Service Structure

Each service follows a consistent structure:

1. **service.ts**: Contains the main implementation of the service
2. **index.ts**: Exports the service and related types
3. Additional files as needed (components, hooks, utilities)

### Index File Pattern

Each service's `index.ts` file follows a consistent pattern:

```typescript
/**
 * Service Name
 * 
 * Brief description of the service.
 */

// Export the service class
export { ServiceName } from './service';

// Export types using 'export type' for isolatedModules compatibility
export type { TypeName1, TypeName2 } from './service';
```

## Migration from Previous Structure

This directory structure replaces the previous flat structure in `/src/services`. All services have been moved to their respective subdirectories in `/src/app/services`, and import paths throughout the application have been updated to reflect this change.

## Benefits

1. **Better Organization**: Related files are grouped together
2. **Improved Discoverability**: Easier to find service code
3. **Enhanced Scalability**: New services can be added without cluttering the directory
4. **Consistent Structure**: All services follow the same pattern
5. **Clear Separation of Concerns**: Each service has its own directory

## Future Considerations

As the application grows, consider the following:

1. **Service Documentation**: Add README.md files to each service directory
2. **Testing**: Add test directories for each service
3. **Versioning**: Support multiple versions of services if needed
