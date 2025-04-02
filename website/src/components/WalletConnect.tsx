'use client';

import React from 'react';
import ConnectButton from './ConnectButton';
import { useWallet } from '@/hooks/useWallet';
import { ServiceClient } from '../lib/serviceClient';

interface WalletConnectProps {
  onConnect?: (client: ServiceClient | null) => void;
}

export function WalletConnect({ onConnect }: WalletConnectProps) {
  const walletState = useWallet();
  const { address, isConnected } = walletState;

  // Create and provide the ServiceClient when wallet is connected
  React.useEffect(() => {
    console.log('WalletConnect: Wallet state changed', { isConnected, address });
    
    if (isConnected && address) {
      console.log('WalletConnect: Creating ServiceClient with connected wallet');
      
      // Create a service client that uses the wallet to sign messages
      const client = new ServiceClient({
        signMessage: async (message: string) => {
          if (typeof window === 'undefined') {
            throw new Error('Window is not defined');
          }
          
          console.log('WalletConnect: Signing message with wallet');
          
          try {
            // Try neo3Dapi first (NEO N3)
            if (window.neo3Dapi) {
              console.log('WalletConnect: Using neo3Dapi for signing');
              const result = await window.neo3Dapi.signMessage({
                message,
                address
              });
              // Return the signature itself
              console.log('WalletConnect: Signature result from neo3Dapi', result);
              return result.signature || result.data;
            }
            
            // Fallback to legacy NeoLine
            if (window.NEOLine && window.NEOLine.instance) {
              console.log('WalletConnect: Using legacy NeoLine for signing');
              const result = await window.NEOLine.instance.signMessage({
                message,
                address
              });
              // Return the signature itself
              console.log('WalletConnect: Signature result from NEOLine', result);
              return result.signature || result.data;
            }
            
            console.error('WalletConnect: No compatible wallet found for signing');
            throw new Error('Neo wallet not available');
          } catch (error) {
            console.error('WalletConnect: Error signing message:', error);
            throw error;
          }
        },
      });
      
      // Pass the client to parent component
      console.log('WalletConnect: Providing ServiceClient to parent');
      onConnect?.(client);
    } else {
      console.log('WalletConnect: Clearing ServiceClient (wallet disconnected)');
      // Clear the client when disconnected
      onConnect?.(null);
    }
  }, [isConnected, address, onConnect]);

  return <ConnectButton />;
}