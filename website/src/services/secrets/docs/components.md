# Secrets Service Components Documentation

This document describes the React components used in the Secrets service, including their purpose, props, and interactions.

## Components Overview

### SecretsList

A component that displays a table of secrets with filtering, pagination, and action buttons.

```typescript
interface SecretsListProps {
  secrets: Secret[];
  permissions: SecretPermission[];
  onRefresh: () => void;
}
```

**Features:**
- Displays secrets in a paginated table
- Shows secret metadata (name, type, last updated, etc.)
- Provides action buttons for each secret (edit, permissions, rotate, delete)
- Handles pagination and filtering

**Implementation Notes:**
- Uses MUI Table components for layout
- Uses React hooks for state management
- Imports MUI icons as named imports (e.g., `import EditIcon from '@mui/icons-material/Edit'`)
- Uses the `useSecrets` hook for CRUD operations

### SecretDialog

A dialog component for creating, editing, and viewing secrets.

```typescript
interface SecretDialogProps {
  open: boolean;
  mode: 'create' | 'edit' | 'view';
  secret?: Secret;
  loading?: boolean;
  error?: string;
  onClose: () => void;
  onSave: (secret: Partial<Secret>) => void;
}
```

**Features:**
- Supports create, edit, and view modes
- Provides form validation
- Handles secret value visibility toggling
- Supports all secret metadata fields

**Implementation Notes:**
- Uses MUI Dialog components
- Uses form validation with error messages
- Supports showing/hiding secret values

### PermissionDialog

A dialog component for managing permissions on a secret.

```typescript
interface PermissionDialogProps {
  open: boolean;
  onClose: () => void;
  secretId: string;
  permissions: SecretPermission[];
}
```

**Features:**
- Displays current permissions for a secret
- Allows adding new permissions
- Allows removing existing permissions
- Shows permission metadata (granted by, expiration, etc.)

**Implementation Notes:**
- Uses MUI Table components for permissions list
- Uses form controls for adding new permissions
- Uses the `useSecrets` hook for permission operations

### ConfirmDialog

A reusable confirmation dialog component.

```typescript
interface ConfirmDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  content: string;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';
}
```

**Features:**
- Customizable title and content
- Customizable button text and colors
- Handles confirmation and cancellation actions

**Implementation Notes:**
- Uses MUI Dialog components
- Provides customization options for different use cases

## TypeScript and MUI Compatibility

These components have been updated to work with the latest versions of TypeScript and MUI v7:

1. **React Import Strategy**: 
   - Using namespace imports with `import * as React from 'react'`
   - Using React hooks via the namespace (e.g., `React.useState`)

2. **MUI Icon Imports**:
   - Using direct imports for each icon (e.g., `import EditIcon from '@mui/icons-material/Edit'`)
   - This replaces the previous approach of importing from the index (e.g., `import { Edit as EditIcon } from '@mui/icons-material'`)

3. **Type Imports**:
   - Importing types from the correct path (`../types/types`)
   - Ensuring all props interfaces are properly defined

4. **Component Props**:
   - Ensuring all components have properly typed props
   - Using consistent naming conventions for props (e.g., `onSave` instead of `onSubmit`)

5. **TableCell Compatibility**:
   - Replaced `colSpan` property with `sx` prop using Grid column span:
   - `<TableCell sx={{ gridColumn: 'span 7' }} align="center">` instead of `<TableCell colSpan={7} align="center">`

6. **Component Prop Consistency**:
   - Using consistent prop naming across components (e.g., `onSave` instead of `onSubmit`)

## Component Interactions

The components interact in the following ways:

1. **SecretsList** displays the list of secrets and provides action buttons
2. **SecretDialog** is opened when creating or editing a secret
3. **PermissionDialog** is opened when managing permissions for a secret
4. **ConfirmDialog** is opened when confirming destructive actions (e.g., delete)

The parent component (e.g., SecretsDashboard) is responsible for:
- Fetching the list of secrets and permissions
- Passing them to the SecretsList component
- Handling refresh operations when data changes
