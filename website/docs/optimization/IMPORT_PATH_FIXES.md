# Import Path Fixes for Neo Service Layer Website

This document outlines the import path issues that were causing build failures in the Neo Service Layer website and the solutions implemented to fix them.

## Issue Overview

The build process was failing due to several import path errors:

1. **Missing or incorrect import paths in App Router components:**
   - `./src/app/hooks/useAuth.ts` - Can't resolve '../services/auth'
   - `./src/app/services/secrets/components/SecretList.tsx` - Can't resolve '../utils/formatters'

2. **Missing or incorrect import paths in Pages Router API routes:**
   - `./pages/api/auth/challenge.ts` - Can't resolve '../../../utils/logger'
   - `./pages/api/auth/challenge.ts` - Can't resolve '../../../services/metrics'

## Root Causes

1. **Mixed Routing Systems:**
   - The project uses both the older Pages Router (`/pages`) and the newer App Router (`/src/app`)
   - Import paths were not properly adjusted when migrating components between routing systems

2. **Inconsistent Directory Structure:**
   - Utility files and services are located in different places:
     - `/src/utils` - Core utilities
     - `/src/services` - Service modules
     - `/src/app/services` - App-specific service components

## Solutions Implemented

### 1. Fix Import Paths in App Router Components

For components in the App Router (`/src/app`), we need to update import paths to correctly reference services and utilities:

```typescript
// Before (incorrect)
import { AuthenticationService } from '../services/auth';

// After (correct)
import { AuthenticationService } from '@/services/auth';
```

### 2. Fix Import Paths in Pages Router API Routes

For API routes in the Pages Router (`/pages/api`), we need to update import paths to correctly reference utilities and services:

```typescript
// Before (incorrect)
import logger from '../../../utils/logger';
import { recordApiRequest } from '../../../services/metrics';

// After (correct)
import logger from '@/utils/logger';
import { recordApiRequest } from '@/services/metrics';
```

### 3. Create Missing Utility Functions

For any missing utility functions, we need to create them in the correct locations:

- Authentication service in `/src/services/auth.ts`
- Formatters utility in `/src/app/services/secrets/utils/formatters.ts`

## Implementation Strategy

1. Update import paths in `useAuth.ts` to use absolute imports with the `@/` prefix
2. Update import paths in `SecretList.tsx` to correctly reference the formatters utility
3. Update import paths in API routes to use absolute imports with the `@/` prefix
4. Ensure all referenced files exist in the correct locations

## Benefits

1. **Consistent Import Paths:** Using absolute imports with the `@/` prefix makes it easier to move files without breaking imports
2. **Better Maintainability:** Clear separation between app-specific and shared services/utilities
3. **Improved Build Performance:** Resolving import errors reduces build time and prevents unnecessary rebuilds

## Future Recommendations

1. **Complete App Router Migration:** Finish migrating all pages from Pages Router to App Router
2. **Standardize Directory Structure:** Maintain a consistent directory structure for services and utilities
3. **Use Path Aliases:** Configure additional path aliases in `tsconfig.json` for commonly used directories
