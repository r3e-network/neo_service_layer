# Service Integration Architecture

This document outlines how the service pages in `/src/app/services/*` connect to the actual service implementations in `/src/services/*`.

## Architecture Overview

Each service page in our application follows a consistent pattern for interacting with the underlying service implementations:

1. The UI components in `/src/app/services/*` provide the user interface
2. The service implementations in `/src/services/*` contain the business logic
3. A connection layer manages the communication between UI and service logic

## Implementation Pattern

For each service, we follow this structure:

```
/src/app/services/[service-name]/page.tsx  - UI component (Next.js page)
/src/services/[service-name]/              - Service implementation
/src/hooks/use[ServiceName].ts             - React hook for connecting UI to service
```

## Integration Approach

### 1. Service Implementation

The service folder contains the core business logic:
- API endpoints and methods
- Data processing
- Network requests
- Authentication handling

### 2. React Hooks

Each service has a corresponding hook that:
- Imports the service implementation
- Exposes methods for the UI to call
- Handles loading, error, and success states
- Manages data transformations
- Provides TypeScript interfaces for type safety

### 3. UI Integration

The service page uses the hook to:
- Display interactive components
- Call service methods on user actions
- Show appropriate loading and error states
- Update UI based on service responses

## Example Integration

For the Price Feed service:

```tsx
// In /src/hooks/usePriceFeed.ts
import { useState } from 'react';
import * as priceFeedService from '../services/price-feeds';

export function usePriceFeed() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [priceData, setPriceData] = useState<any>(null);

  const getPriceForAsset = async (assetPair) => {
    setLoading(true);
    setError(null);
    
    try {
      const data = await priceFeedService.getPrice(assetPair);
      setPriceData(data);
      return data;
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return {
    loading,
    error,
    priceData,
    getPriceForAsset
  };
}

// In /src/app/services/price-feed/page.tsx
import { usePriceFeed } from '../../../hooks/usePriceFeed';

export default function PriceFeedPage() {
  const { loading, error, priceData, getPriceForAsset } = usePriceFeed();
  
  // UI code that uses the hook
}
```

## Authentication and Security

Service interactions should:
- Utilize the authentication service for signed message verification
- Apply proper permission checks
- Handle sensitive data securely
- Use TEE for confidential operations

## Error Handling

Consistent error handling should include:
- User-friendly error messages
- Detailed logging (server-side)
- Appropriate fallbacks
- Retry mechanisms for transient failures

## Performance Considerations

To ensure optimal performance:
- Implement data caching
- Use React Suspense for loading states
- Optimize network requests
- Consider server components vs. client components based on use case

## Next Steps

To implement this integration for a new service:

1. Ensure the service implementation is complete
2. Create a custom hook for the service
3. Update the service page to use the hook
4. Test all interactive elements
5. Document any service-specific considerations 