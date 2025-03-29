# React Hooks Documentation

## Overview
The React hooks provide essential functionality for the Neo Service Layer frontend. They handle wallet integration, user feedback, and other UI-related features.

## Available Hooks

### [Wallet Integration](./useWallet.md)
Manages Neo N3 wallet connection:
- Connect/disconnect wallet
- Sign messages
- Track connection state
- Handle wallet events

### [Toast Notifications](./useToast.md)
Provides toast notification system:
- Show notifications
- Multiple types (success, error, etc.)
- Auto-dismiss
- Queue management

## Best Practices
1. Use hooks consistently across components
2. Handle errors appropriately
3. Implement proper cleanup
4. Follow React hooks rules
5. Test all hook functionality

## Error Handling
All hooks follow consistent error handling:
- Clear error messages
- Error state management
- Recovery mechanisms
- User feedback

## Testing
Each hook has comprehensive tests:
- Unit tests
- Integration tests
- Error case testing
- Cleanup verification

## Performance
Hooks are optimized for:
- Minimal re-renders
- Efficient state updates
- Proper cleanup
- Memory management

## Contributing
When adding or modifying hooks:
1. Follow existing patterns
2. Add comprehensive tests
3. Update documentation
4. Consider performance
5. Handle errors properly