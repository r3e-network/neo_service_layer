# Hooks Directory

This directory contains all the React hooks used throughout the application. The hooks were previously split between `/website/hooks` and `/website/src/app/hooks` and have been consolidated here for better organization and maintainability.

## Available Hooks

- `useAuth` - Authentication hook for wallet connection and user authentication
- `useWebSocket` - WebSocket connection management hook
- `usePriceFeed` - Price feed data and updates hook
- `useWallet` - Wallet connection and management hook
- `useToast` - Toast notification management hook

## Migration Notes

The hooks were consolidated on [DATE] from:
- `/website/hooks`
- `/website/src/app/hooks`

All imports should now reference hooks from this directory (`/website/src/app/hooks`).