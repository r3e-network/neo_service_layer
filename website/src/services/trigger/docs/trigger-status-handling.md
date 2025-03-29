# Trigger Status Handling

This document describes how trigger status is handled in the Neo Service Layer trigger components.

## Status Values

Trigger status values are defined in the `TRIGGER_CONSTANTS.TRIGGER_STATUS` object:

- `ACTIVE`: The trigger is active and will execute when its conditions are met
- `PAUSED`: The trigger is temporarily paused and will not execute
- `FAILED`: The trigger has failed and requires attention
- `COMPLETED`: The trigger has completed its execution cycle

## Components Involved

### TriggerList Component

The TriggerList component displays triggers and allows users to toggle their status. It:

1. Uses the `TRIGGER_CONSTANTS.TRIGGER_STATUS` values to determine the current status
2. Displays appropriate status indicators (colors, icons) based on status
3. Provides a toggle button that calls the `onToggleStatus` prop function with:
   - The trigger ID
   - A boolean indicating whether the trigger should be active

### useTriggers Hook

The useTriggers hook provides the `toggleTriggerStatus` function that:

1. Accepts a trigger ID and an optional `active` boolean parameter
2. Determines the new status based on the `active` parameter or toggles the current status
3. Makes an API call to update the trigger status
4. Updates the local state with the updated trigger

### TriggerDashboard Component

The TriggerDashboard component:

1. Connects the TriggerList with the useTriggers hook
2. Handles user interactions for toggling trigger status
3. Provides feedback to the user about the status change

## Status Toggle Flow

1. User clicks the toggle button in TriggerList
2. TriggerList calls `onToggleStatus(triggerId, active)`
3. TriggerDashboard handles this by calling `toggleTriggerStatus(triggerId, active)`
4. useTriggers hook makes the API call and updates state
5. UI is updated to reflect the new status

## Error Handling

If the status toggle fails:
1. An error message is displayed to the user
2. The UI remains in its previous state
3. The error is logged for debugging purposes

## Future Improvements

- Add confirmation dialog for status changes that might have significant impacts
- Implement batch status updates for multiple triggers
- Add status change history tracking
