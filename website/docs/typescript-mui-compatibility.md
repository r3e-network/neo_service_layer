# TypeScript and MUI Compatibility Guide

This document outlines the approach to resolving TypeScript and MUI compatibility issues in the Neo Service Layer web application.

## React Import Strategy

To fix issues with missing hook exports and React JSX errors, use namespace imports for React:

```tsx
import * as React from 'react';
```

This ensures that all React hooks and JSX elements are properly recognized by TypeScript.

## Common TypeScript Errors and Solutions

### 1. "React refers to a UMD global, but the current file is a module"

This error occurs when JSX is used in a file without importing React. To fix:

```tsx
// Add this import at the top of the file
import * as React from 'react';
```

### 2. "Module 'react' has no exported member 'useEffect'"

This error occurs when hooks are imported directly from React. To fix:

```tsx
// Instead of this:
import { useEffect, useState } from 'react';

// Use this:
import * as React from 'react';
// Then use hooks as:
React.useEffect(() => {
  // ...
});
React.useState(initialValue);
```

### 3. MUI TableCell Compatibility Issues

For issues with the `colSpan` property on TableCell components:

```tsx
// Instead of this:
<TableCell colSpan={3}>Content</TableCell>

// Use this:
<TableCell sx={{ gridColumn: 'span 3' }}>Content</TableCell>
```

### 4. Recharts Component TypeScript Errors

For Recharts components that cause TypeScript errors:

```tsx
// Add @ts-ignore comment before the problematic component
// @ts-ignore
<LineChart data={data}>
  {/* ... */}
</LineChart>
```

## Implementation Approach

When fixing TypeScript errors:

1. Start with the most critical components first
2. Ensure consistent import patterns across all files
3. Test each component after making changes
4. Document any special cases or workarounds

## Fixed Components

The following components have been updated with the correct React import strategy:

- Vault implementation and tests
- Debug page component
- Architecture and Introduction page components
- Trigger components (TriggerList, TriggerDashboard)
- API components (ApiDashboard, ApiKeysList, ApiUsageMetrics)
- Function components (FunctionsDashboard, FunctionTriggers)

## Remaining Work

There are still TypeScript errors in other parts of the codebase that need to be addressed using the same approach.
