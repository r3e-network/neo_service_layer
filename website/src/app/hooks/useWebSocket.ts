// @ts-ignore
import * as React from 'react';

type EventCallback = (data: any) => void;

interface WebSocketHook {
  sendMessage: (message: string | object) => void;
  lastMessage: string | null;
  readyState: number;
  error: Error | null;
  subscribe: (event: string, callback: EventCallback) => void;
  unsubscribe: (event: string, callback: EventCallback) => void;
}

// Check if we're running in a browser environment
const isBrowser = typeof window !== 'undefined';

// Define WebSocket states for SSR compatibility
const WS_STATES = {
  CONNECTING: isBrowser ? WebSocket.CONNECTING : 0,
  OPEN: isBrowser ? WebSocket.OPEN : 1,
  CLOSING: isBrowser ? WebSocket.CLOSING : 2,
  CLOSED: isBrowser ? WebSocket.CLOSED : 3
};

export function useWebSocket(url?: string): WebSocketHook {
  const ws = React.useRef<WebSocket | null>(null);
  const [lastMessage, setLastMessage] = React.useState<string | null>(null);
  const [readyState, setReadyState] = React.useState<number>(WS_STATES.CONNECTING);
  const [error, setError] = React.useState<Error | null>(null);
  const eventListeners = React.useRef<Map<string, Set<EventCallback>>>(new Map());

  // Use default WebSocket URL if not provided
  const wsUrl = url || (isBrowser ? 
    `${window.location.protocol === 'https:' ? 'wss:' : 'ws:'}//${window.location.host}/api/ws` : '');

  React.useEffect(() => {
    // Only initialize WebSocket in browser environment
    if (!isBrowser || !wsUrl) return;
    
    try {
      ws.current = new WebSocket(wsUrl);

      ws.current.onopen = () => {
        setReadyState(WS_STATES.OPEN);
        setError(null);
      };

      ws.current.onclose = () => {
        setReadyState(WS_STATES.CLOSED);
      };

      ws.current.onerror = (event) => {
        setError(new Error('WebSocket error occurred'));
      };

      ws.current.onmessage = (event) => {
        setLastMessage(event.data);
        
        try {
          const message = JSON.parse(event.data);
          if (message && message.type && eventListeners.current.has(message.type)) {
            const callbacks = eventListeners.current.get(message.type);
            if (callbacks) {
              callbacks.forEach(callback => {
                try {
                  callback(message.data);
                } catch (err) {
                  console.error('Error in WebSocket event callback:', err);
                }
              });
            }
          }
        } catch (err) {
          console.error('Error parsing WebSocket message:', err);
        }
      };

      return () => {
        if (ws.current) {
          ws.current.close();
        }
      };
    } catch (error) {
      console.error('Error initializing WebSocket:', error);
      setError(error instanceof Error ? error : new Error('Failed to initialize WebSocket'));
      return () => {};
    }
  }, [wsUrl]);

  const sendMessage = React.useCallback((message: string | object) => {
    if (!isBrowser) {
      console.warn('Cannot send WebSocket message in server environment');
      return;
    }
    
    if (ws.current && ws.current.readyState === WS_STATES.OPEN) {
      const messageStr = typeof message === 'string' ? message : JSON.stringify(message);
      ws.current.send(messageStr);
    } else {
      setError(new Error('WebSocket is not connected'));
    }
  }, []);

  const subscribe = React.useCallback((event: string, callback: EventCallback) => {
    if (!eventListeners.current.has(event)) {
      eventListeners.current.set(event, new Set());
    }
    const callbacks = eventListeners.current.get(event);
    if (callbacks) {
      callbacks.add(callback);
    }
  }, []);

  const unsubscribe = React.useCallback((event: string, callback: EventCallback) => {
    const callbacks = eventListeners.current.get(event);
    if (callbacks) {
      callbacks.delete(callback);
      if (callbacks.size === 0) {
        eventListeners.current.delete(event);
      }
    }
  }, []);

  return {
    sendMessage,
    lastMessage,
    readyState,
    error,
    subscribe,
    unsubscribe
  };
}