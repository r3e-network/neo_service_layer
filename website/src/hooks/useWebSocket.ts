// @ts-ignore
import * as React from 'react';

// Custom WebSocket wrapper that adds on/off methods similar to EventEmitter
class WebSocketWrapper {
  private socket: WebSocket;
  private eventHandlers: Record<string, Array<(data: any) => void>> = {};

  constructor(url: string) {
    this.socket = new WebSocket(url);
    
    this.socket.onmessage = (event) => {
      let data;
      try {
        data = JSON.parse(event.data);
        if (data.type && this.eventHandlers[data.type]) {
          this.eventHandlers[data.type].forEach(handler => handler(data));
        }
      } catch (e) {
        console.error('Error parsing WebSocket message:', e);
      }
    };
  }

  get readyState(): number {
    return this.socket.readyState;
  }

  on(event: string, handler: (data: any) => void): void {
    if (!this.eventHandlers[event]) {
      this.eventHandlers[event] = [];
    }
    this.eventHandlers[event].push(handler);
  }

  off(event: string, handler: (data: any) => void): void {
    if (this.eventHandlers[event]) {
      this.eventHandlers[event] = this.eventHandlers[event].filter(h => h !== handler);
    }
  }

  send(data: any): void {
    if (this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(typeof data === 'string' ? data : JSON.stringify(data));
    }
  }

  close(): void {
    this.socket.close();
  }

  // Forward native WebSocket event handlers
  set onopen(handler: (event: Event) => void) {
    this.socket.onopen = handler;
  }

  set onclose(handler: (event: CloseEvent) => void) {
    this.socket.onclose = handler;
  }

  set onerror(handler: (event: Event) => void) {
    this.socket.onerror = handler;
  }
  
  set onmessage(handler: (event: MessageEvent) => void) {
    this.socket.onmessage = handler;
  }
}

interface WebSocketOptions {
  url: string;
  onMessage?: (data: any) => void;
  onOpen?: () => void;
  onClose?: () => void;
  onError?: (error: Event) => void;
  autoReconnect?: boolean;
  reconnectInterval?: number;
  maxReconnectAttempts?: number;
}

interface WebSocketState {
  socket: WebSocketWrapper | null;
  isConnected: boolean;
  error: Event | null;
}

export function useWebSocket({
  url,
  onMessage,
  onOpen,
  onClose,
  onError,
  autoReconnect = true,
  reconnectInterval = 5000,
  maxReconnectAttempts = 5
}: WebSocketOptions) {
  const [state, setState] = React.useState<WebSocketState>({
    socket: null,
    isConnected: false,
    error: null
  });
  const [reconnectAttempts, setReconnectAttempts] = React.useState(0);

  const connect = React.useCallback(() => {
    try {
      const socketWrapper = new WebSocketWrapper(url);

      socketWrapper.onopen = () => {
        setState(prev => ({ ...prev, socket: socketWrapper, isConnected: true, error: null }));
        setReconnectAttempts(0);
        if (onOpen) onOpen();
      };

      socketWrapper.onmessage = (event) => {
        let data;
        try {
          data = JSON.parse(event.data);
        } catch (e) {
          data = event.data;
        }
        if (onMessage) onMessage(data);
      };

      socketWrapper.onclose = (event) => {
        setState(prev => ({ ...prev, socket: null, isConnected: false }));
        if (onClose) onClose();

        if (autoReconnect && reconnectAttempts < maxReconnectAttempts) {
          setTimeout(() => {
            setReconnectAttempts(prev => prev + 1);
            connect();
          }, reconnectInterval);
        }
      };

      socketWrapper.onerror = (error) => {
        setState(prev => ({ ...prev, error }));
        if (onError) onError(error);
      };

      setState(prev => ({ ...prev, socket: socketWrapper }));
    } catch (error) {
      setState(prev => ({ ...prev, error: error as Event }));
    }
  }, [url, onMessage, onOpen, onClose, onError, autoReconnect, reconnectInterval, maxReconnectAttempts, reconnectAttempts]);

  const disconnect = React.useCallback(() => {
    if (state.socket) {
      state.socket.close();
    }
  }, [state.socket]);

  const send = React.useCallback((data: any) => {
    if (state.socket && state.isConnected) {
      state.socket.send(data);
      return true;
    }
    return false;
  }, [state.socket, state.isConnected]);

  React.useEffect(() => {
    connect();
    return () => {
      disconnect();
    };
  }, [connect, disconnect]);

  return {
    isConnected: state.isConnected,
    error: state.error,
    socket: state.socket,
    send,
    disconnect,
    reconnect: connect
  };
}
