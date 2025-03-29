export const mockWebSocket = {
  connect: jest.fn(),
  disconnect: jest.fn(),
  send: jest.fn(),
  onMessage: jest.fn(),
  onClose: jest.fn(),
  onError: jest.fn()
};

export class MockWebSocket {
  private listeners: { [key: string]: Function[] } = {};

  constructor(url: string) {
    mockWebSocket.connect(url);
  }

  addEventListener(event: string, callback: Function) {
    if (!this.listeners[event]) {
      this.listeners[event] = [];
    }
    this.listeners[event].push(callback);
  }

  removeEventListener(event: string, callback: Function) {
    if (this.listeners[event]) {
      this.listeners[event] = this.listeners[event].filter(cb => cb !== callback);
    }
  }

  send(data: string) {
    mockWebSocket.send(data);
  }

  close() {
    mockWebSocket.disconnect();
    this.emit('close');
  }

  // Helper method to simulate incoming messages
  emit(event: string, data?: any) {
    if (this.listeners[event]) {
      this.listeners[event].forEach(callback => {
        if (event === 'message') {
          callback({ data: JSON.stringify(data) });
        } else {
          callback(data);
        }
      });
    }
  }
}

// Replace global WebSocket with mock
(global as any).WebSocket = MockWebSocket;