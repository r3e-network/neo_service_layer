import React from 'react';
import { useWebSocket } from './useWebSocket';

interface WebSocketEventHandlers {
  [eventType: string]: Array<(data: any) => void>;
}

interface UseWebSocketEventsOptions {
  url: string;
}

export function useWebSocketEvents({ url }: UseWebSocketEventsOptions) {
  const eventHandlers = React.useRef<WebSocketEventHandlers>({});
  const { send, disconnect, socket, isConnected } = useWebSocket({ url });

  const subscribe = React.useCallback((eventType: string, handler: (data: any) => void) => {
    if (!eventHandlers.current[eventType]) {
      eventHandlers.current[eventType] = [];
    }
    eventHandlers.current[eventType].push(handler);
    
    // If we have a socket, register the handler
    if (socket) {
      socket.on(eventType, handler);
    }
  }, [socket]);

  const unsubscribe = React.useCallback((eventType: string, handler: (data: any) => void) => {
    if (eventHandlers.current[eventType]) {
      eventHandlers.current[eventType] = eventHandlers.current[eventType].filter(h => h !== handler);
    }
    
    // If we have a socket, unregister the handler
    if (socket) {
      socket.off(eventType, handler);
    }
  }, [socket]);

  // When the socket changes, re-register all handlers
  React.useEffect(() => {
    if (socket) {
      // Register all existing handlers
      Object.entries(eventHandlers.current).forEach(([eventType, handlers]) => {
        // Explicitly cast handlers to the correct type
        const typedHandlers = handlers as Array<(data: any) => void>;
        typedHandlers.forEach(handler => {
          socket.on(eventType, handler);
        });
      });
    }
  }, [socket]);

  return {
    isConnected,
    send,
    disconnect,
    subscribe,
    unsubscribe
  };
}
