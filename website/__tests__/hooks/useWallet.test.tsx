import { renderHook, act } from '@testing-library/react';
import { useWallet } from '../../src/app/hooks/useWallet';

jest.mock('@cityofzion/neon-js', () => ({
  wallet: {
    isAddress: (address: string) => 
      address === 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd',
  },
}));

describe('useWallet Hook', () => {
  let consoleErrorSpy: jest.SpyInstance;

  beforeEach(() => {
    jest.clearAllMocks();
    consoleErrorSpy = jest.spyOn(console, 'error');
  });

  afterEach(() => {
    consoleErrorSpy.mockRestore();
  });

  it('should initialize with disconnected state', () => {
    const { result } = renderHook(() => useWallet());
    
    expect(result.current.isConnected).toBe(false);
    expect(result.current.neoAddress).toBe('');
    expect(result.current.provider).toBeNull();
  });

  describe('connect', () => {
    it('should connect successfully', async () => {
      const { result } = renderHook(() => useWallet());
      
      await act(async () => {
        await result.current.connect();
      });

      expect(result.current.isConnected).toBe(true);
      expect(result.current.neoAddress).toBe('NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd');
      expect(result.current.provider).toEqual({
        address: 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd',
        network: 'N3MainNet',
      });
    });

    it('should handle connection errors', async () => {
      const { result } = renderHook(() => useWallet());
      
      // Mock implementation to simulate error
      const error = new Error('Connection failed');
      const originalConnect = result.current.connect;
      result.current.connect = jest.fn().mockRejectedValue(error);

      await act(async () => {
        await expect(result.current.connect()).rejects.toThrow('Connection failed');
      });

      expect(result.current.isConnected).toBe(false);
      expect(result.current.neoAddress).toBe('');
      expect(result.current.provider).toBeNull();
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error connecting wallet:',
        error
      );

      result.current.connect = originalConnect;
    });
  });

  describe('disconnect', () => {
    it('should disconnect successfully', async () => {
      const { result } = renderHook(() => useWallet());
      
      // First connect
      await act(async () => {
        await result.current.connect();
      });

      expect(result.current.isConnected).toBe(true);

      // Then disconnect
      await act(async () => {
        await result.current.disconnect();
      });

      expect(result.current.isConnected).toBe(false);
      expect(result.current.neoAddress).toBe('');
      expect(result.current.provider).toBeNull();
    });

    it('should handle disconnection errors', async () => {
      const { result } = renderHook(() => useWallet());
      
      // First connect
      await act(async () => {
        await result.current.connect();
      });

      // Mock implementation to simulate error
      const error = new Error('Disconnection failed');
      const originalDisconnect = result.current.disconnect;
      result.current.disconnect = jest.fn().mockRejectedValue(error);

      await act(async () => {
        await expect(result.current.disconnect()).rejects.toThrow('Disconnection failed');
      });

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error disconnecting wallet:',
        error
      );

      result.current.disconnect = originalDisconnect;
    });
  });

  describe('signMessage', () => {
    it('should sign message successfully', async () => {
      const { result } = renderHook(() => useWallet());
      const message = 'Test message';
      const expectedSignature = '1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef';
      
      await act(async () => {
        await result.current.connect();
      });

      const signature = await result.current.signMessage(message);
      expect(signature).toBe(expectedSignature);
    });

    it('should handle signing errors', async () => {
      const { result } = renderHook(() => useWallet());
      
      await act(async () => {
        await result.current.connect();
      });

      // Mock implementation to simulate error
      const error = new Error('Signing failed');
      const originalSignMessage = result.current.signMessage;
      result.current.signMessage = jest.fn().mockRejectedValue(error);

      await expect(result.current.signMessage('Test')).rejects.toThrow('Signing failed');
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error signing message:',
        error
      );

      result.current.signMessage = originalSignMessage;
    });
  });
});