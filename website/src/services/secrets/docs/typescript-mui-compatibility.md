# TypeScript and MUI v7 Compatibility Guide

This document outlines the compatibility changes required to make the Secrets service components work with the latest versions of TypeScript and Material UI (MUI) v7.

## React Import Strategy

### Previous Approach
```typescript
import React, { useState, useEffect } from 'react';
```

### New Approach
```typescript
import * as React from 'react';
```

### Usage
```typescript
// Old way
const [value, setValue] = useState(initialValue);

// New way
const [value, setValue] = React.useState(initialValue);
```

## MUI Icon Imports

### Previous Approach
```typescript
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon
} from '@mui/icons-material';
```

### New Approach
```typescript
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
```

## Type Imports

### Previous Approach
```typescript
import { Secret, SecretPermission } from '../types';
```

### New Approach
```typescript
import { Secret, SecretPermission } from '../types/types';
```

## Component Props

Ensure consistent prop naming across components:

```typescript
// Previous
interface SecretDialogProps {
  // ...
  onSubmit: (secret: Partial<Secret>) => void;
}

// New
interface SecretDialogProps {
  // ...
  onSave: (secret: Partial<Secret>) => void;
}
```

## TableCell Compatibility

The `colSpan` property is no longer directly available on TableCell in MUI v7. Instead, use the `sx` prop with Grid column span:

### Previous Approach
```typescript
<TableCell colSpan={7} align="center">
  {/* content */}
</TableCell>
```

### New Approach
```typescript
<TableCell 
  align="center"
  sx={{ gridColumn: 'span 7' }}
>
  {/* content */}
</TableCell>
```

## Component Structure

Each component should follow this general structure:

1. Imports (React, MUI components, MUI icons, local types and utilities)
2. Interface definitions for props
3. Component implementation
4. Export statement

Example:
```typescript
import * as React from 'react';
import { Button, TextField } from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import { MyType } from '../types/types';

interface MyComponentProps {
  value: string;
  onChange: (value: string) => void;
}

export default function MyComponent({ value, onChange }: MyComponentProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(e.target.value);
  };

  return (
    <div>
      <TextField value={value} onChange={handleChange} />
      <Button startIcon={<SaveIcon />}>Save</Button>
    </div>
  );
}
```

## TypeScript Configuration

To resolve the "allowSyntheticDefaultImports" warnings, the TypeScript configuration should include:

```json
{
  "compilerOptions": {
    "allowSyntheticDefaultImports": true,
    // other options...
  }
}
```

This allows using imports like `import * as React from 'react'` even when the module doesn't have a default export.

## Common Issues and Solutions

### 1. Missing React Hooks

**Issue**: TypeScript error: "Module 'react' has no exported member 'useState'"

**Solution**: Use namespace import and access hooks through the namespace:
```typescript
import * as React from 'react';
// ...
const [state, setState] = React.useState(initialValue);
```

### 2. MUI Icon Import Errors

**Issue**: TypeScript error: "Module '@mui/icons-material' has no exported member 'Add'"

**Solution**: Import icons directly from their individual modules:
```typescript
import AddIcon from '@mui/icons-material/Add';
```

### 3. TableCell Props

**Issue**: TypeScript error: "Property 'colSpan' does not exist on type 'IntrinsicAttributes & TableCellProps'"

**Solution**: Use the `sx` prop with Grid column span:
```typescript
<TableCell sx={{ gridColumn: 'span 7' }} align="center">
```

### 4. Type Import Paths

**Issue**: TypeScript error: "Module '../types' has no exported member 'Secret'"

**Solution**: Update import paths to point to the correct location:
```typescript
import { Secret } from '../types/types';
```

### 5. Component Prop Mismatches

**Issue**: TypeScript error: "Property 'onSubmit' does not exist on type 'IntrinsicAttributes & SecretDialogProps'"

**Solution**: Ensure consistent prop naming across components and update all references:
```typescript
// In SecretDialog.tsx
interface SecretDialogProps {
  // ...
  onSave: (secret: Partial<Secret>) => void;
}

// In SecretsList.tsx
<SecretDialog
  // ...
  onSave={handleUpdateSecret}
/>
```
