// @ts-ignore
import * as React from 'react';
import { wallet } from '@cityofzion/neon-js';

interface WalletProvider {
  address: string;
  network: string;
}

export function useWallet() {
  const [isConnected, setIsConnected] = React.useState(false);
  const [neoAddress, setNeoAddress] = React.useState('');
  const [provider, setProvider] = React.useState<WalletProvider | null>(null);

  const connect = React.useCallback(async () => {
    try {
      const mockProvider = { address: 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd', network: 'N3MainNet' };
      setProvider(mockProvider);
      setNeoAddress(mockProvider.address);
      setIsConnected(true);
    } catch (error) {
      console.error('Error connecting wallet:', error);
      setProvider(null);
      setNeoAddress('');
      setIsConnected(false);
      throw error;
    }
  }, []);

  const disconnect = React.useCallback(async () => {
    try {
      setProvider(null);
      setNeoAddress('');
      setIsConnected(false);
    } catch (error) {
      console.error('Error disconnecting wallet:', error);
      throw error;
    }
  }, []);

  const signMessage = React.useCallback(async (message: string) => {
    try {
      if (!isConnected) {
        const error = new Error('Wallet not connected');
        console.error('Error signing message:', error);
        throw error;
      }
      return '1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef';
    } catch (error) {
      console.error('Error signing message:', error);
      throw error;
    }
  }, [isConnected]);

  return {
    isConnected,
    neoAddress,
    provider,
    connect,
    disconnect,
    signMessage,
  };
}