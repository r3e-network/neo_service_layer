'use client';

// @ts-ignore
import * as React from 'react';
import { WalletOutlined } from '@ant-design/icons';
import { ServiceClient } from '../lib/serviceClient';

interface WalletInfo {
  address: string;
  network: string;
  balance: {
    NEO: string;
    GAS: string;
  };
}

interface WalletConnectProps {
  onConnect?: (client: ServiceClient | null) => void;
}

export function WalletConnect({ onConnect }: WalletConnectProps) {
  const [isConnected, setIsConnected] = React.useState(false);
  const [isConnecting, setIsConnecting] = React.useState(false);
  const [walletInfo, setWalletInfo] = React.useState<WalletInfo | null>(null);
  const [isClient, setIsClient] = React.useState(false);

  // Set isClient to true when component mounts in the browser
  React.useEffect(() => {
    setIsClient(true);
    
    // Check if wallet is already connected
    const checkWalletConnection = async () => {
      if (typeof window !== 'undefined' && window.neo3Wallet) {
        try {
          const account = await window.neo3Wallet.getAccount();
          if (account && account.address) {
            handleConnect();
          }
        } catch (error) {
          console.log('No wallet connected yet');
        }
      }
    };
    
    checkWalletConnection();
  }, []);

  const handleConnect = async () => {
    if (!isClient) return;
    
    setIsConnecting(true);
    try {
      if (!window.neo3Wallet) {
        throw new Error('Neo N3 wallet not found. Please install a compatible wallet.');
      }

      // Get wallet info
      const [account, balance, network] = await Promise.all([
        window.neo3Wallet.getAccount(),
        window.neo3Wallet.getBalance(),
        window.neo3Wallet.getNetwork(),
      ]);

      setWalletInfo({
        address: account.address,
        network,
        balance,
      });

      // Create and pass service client to parent
      const client = new ServiceClient({
        signMessage: async (message: string) => {
          if (!window.neo3Wallet) {
            throw new Error('Neo wallet not available');
          }
          return await window.neo3Wallet.signMessage(message);
        },
      });

      onConnect?.(client);
      setIsConnected(true);
    } catch (error) {
      console.error('Failed to connect wallet:', error);
      alert(error instanceof Error ? error.message : 'Failed to connect wallet');
    } finally {
      setIsConnecting(false);
    }
  };

  const handleDisconnect = () => {
    setWalletInfo(null);
    setIsConnected(false);
    onConnect?.(null);
  };

  // Don't render anything during SSR
  if (!isClient) {
    return (
      <button className="inline-flex items-center gap-2 rounded-full px-6 py-3 text-base font-semibold text-white shadow-sm bg-blue-600">
        <WalletOutlined />
        Connect Wallet
      </button>
    );
  }

  if (isConnected && walletInfo) {
    return (
      <div className="inline-flex items-center gap-4 rounded-full bg-white dark:bg-gray-800 px-6 py-3 shadow-sm ring-1 ring-gray-900/5">
        <div className="flex items-center gap-2">
          <WalletOutlined className="text-green-500" />
          <span className="text-sm font-medium text-gray-900 dark:text-white">
            Connected
          </span>
        </div>
        <div className="h-5 w-px bg-gray-900/5 dark:bg-gray-700" />
        <div className="flex flex-col">
          <span className="text-xs text-gray-500 dark:text-gray-400">
            {walletInfo.network}
          </span>
          <span className="text-sm font-medium text-gray-900 dark:text-white">
            {`${walletInfo.address.slice(0, 6)}...${walletInfo.address.slice(-4)}`}
          </span>
        </div>
        <div className="h-5 w-px bg-gray-900/5 dark:bg-gray-700" />
        <div className="flex flex-col">
          <div className="flex items-center gap-1">
            <span className="text-xs text-gray-500 dark:text-gray-400">NEO:</span>
            <span className="text-sm font-medium text-gray-900 dark:text-white">
              {walletInfo.balance.NEO}
            </span>
          </div>
          <div className="flex items-center gap-1">
            <span className="text-xs text-gray-500 dark:text-gray-400">GAS:</span>
            <span className="text-sm font-medium text-gray-900 dark:text-white">
              {walletInfo.balance.GAS}
            </span>
          </div>
        </div>
        <button
          onClick={handleDisconnect}
          className="rounded-full bg-red-50 dark:bg-red-900/20 px-3 py-1 text-sm font-medium text-red-600 dark:text-red-400 hover:bg-red-100 dark:hover:bg-red-900/40 transition-colors"
        >
          Disconnect
        </button>
      </div>
    );
  }

  return (
    <button
      onClick={handleConnect}
      disabled={isConnecting}
      className={`inline-flex items-center gap-2 rounded-full px-6 py-3 text-base font-semibold text-white shadow-sm transition-all duration-200 ${
        isConnecting
          ? 'bg-blue-400 cursor-not-allowed'
          : 'bg-blue-600 hover:bg-blue-500'
      }`}
    >
      <WalletOutlined />
      {isConnecting ? 'Connecting...' : 'Connect Wallet'}
    </button>
  );
}