# TypeScript and React Integration

This document describes the approach used for integrating TypeScript with React in the Neo Service Layer's trigger components.

## Import Strategy

To ensure compatibility between TypeScript and React, we use the following import pattern:

```typescript
// @ts-ignore
import * as React from 'react';
```

This approach:
1. Uses the namespace import syntax (`import * as React`)
2. Applies a `@ts-ignore` comment to suppress TypeScript errors related to module resolution
3. Allows access to all React features through the `React` namespace

## Hook Usage

When using React hooks, we access them through the React namespace:

```typescript
// Instead of:
const [state, setState] = useState(initialValue);

// We use:
const [state, setState] = React.useState(initialValue);
```

This approach ensures TypeScript correctly recognizes the hooks and their types.

## Constants and Type Safety

We use constants defined in `TRIGGER_CONSTANTS` for consistent values across components:

```typescript
// For status values:
TRIGGER_CONSTANTS.TRIGGER_STATUS.ACTIVE
TRIGGER_CONSTANTS.TRIGGER_STATUS.PAUSED
```

This provides:
1. Type safety through TypeScript's enum-like behavior
2. Centralized management of constant values
3. Consistency across the application

## Component Props

Component props are defined using TypeScript interfaces:

```typescript
interface TriggerListProps {
  triggers: Trigger[];
  executions?: Record<string, TriggerExecution[]>;
  loading: boolean;
  onEdit: (trigger: Trigger) => void;
  onDelete: (triggerId: string) => void;
  onToggleStatus: (triggerId: string, active: boolean) => void;
}
```

This ensures:
1. Type checking for props passed to components
2. Clear documentation of expected props
3. Better IDE support with autocompletion

## Function Parameters

Functions are defined with explicit parameter types and return types:

```typescript
const toggleTriggerStatus = async (triggerId: string, active?: boolean): Promise<Trigger> => {
  // Implementation
};
```

This provides:
1. Clear contract for function usage
2. Type checking for arguments
3. Documentation of expected behavior

## MUI Component Props

When working with MUI components that have TypeScript compatibility issues with certain props (like `colSpan` on TableCell), we use the following approaches:

### Using the `sx` Prop

The `sx` prop is fully typed in MUI and can be used to apply styles and handle layout:

```typescript
<TableCell sx={{ 
  padding: '16px', 
  textAlign: 'center',
  '&': { 
    // This is a workaround to set colSpan in a type-safe way
    gridColumn: 'span 7 / span 7' 
  }
}}>
  {/* Cell content */}
</TableCell>
```

### Calculating Values from Existing Data

For derived values like success rates, we calculate them from existing data rather than expecting additional properties:

```typescript
// Instead of using trigger.successCount which might not exist:
{trigger.executionCount > 0
  ? formatPercentage(
      ((trigger.executionCount - (trigger.failureCount || 0)) / trigger.executionCount)
    )
  : 'N/A'}
```

## Future Improvements

1. Configure TypeScript to allow synthetic default imports to eliminate the need for namespace imports
2. Add comprehensive JSDoc comments for better documentation
3. Implement stricter TypeScript settings for enhanced type safety
4. Create a shared types library for common types used across components
5. Extend MUI component types to include missing props when necessary
