// @ts-ignore
import * as React from 'react';
import { authService } from '../../services/auth/authService';
import type { WalletInfo, AuthenticationResult } from '../../services/auth/service';

interface UseAuthResult {
  isAuthenticated: boolean;
  wallet: WalletInfo | null;
  connectWallet: () => Promise<WalletInfo>;
  authenticate: () => Promise<AuthenticationResult>;
  disconnect: () => void;
  loading: boolean;
  error: string | null;
}

export function useAuth(): UseAuthResult {
  const [isAuthenticated, setIsAuthenticated] = React.useState<boolean>(authService.isAuthenticated());
  const [wallet, setWallet] = React.useState<WalletInfo | null>(authService.getWallet());
  const [loading, setLoading] = React.useState<boolean>(false);
  const [error, setError] = React.useState<string | null>(null);

  // Listen for auth events
  React.useEffect(() => {
    const handleAuthenticated = (result: AuthenticationResult) => {
      setIsAuthenticated(true);
      setWallet(result.walletInfo);
    };

    const handleDisconnected = () => {
      setIsAuthenticated(false);
      setWallet(null);
    };

    const handleWalletConnected = (walletInfo: WalletInfo) => {
      setWallet(walletInfo);
    };

    authService.on('authenticated', handleAuthenticated);
    authService.on('disconnected', handleDisconnected);
    authService.on('wallet_connected', handleWalletConnected);

    return () => {
      authService.off('authenticated', handleAuthenticated);
      authService.off('disconnected', handleDisconnected);
      authService.off('wallet_connected', handleWalletConnected);
    };
  }, []);

  const connectWallet = React.useCallback(async (): Promise<WalletInfo> => {
    setLoading(true);
    setError(null);
    
    try {
      const result = await authService.connectWallet();
      return result;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to connect wallet';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const authenticate = React.useCallback(async (): Promise<AuthenticationResult> => {
    setLoading(true);
    setError(null);
    
    try {
      const result = await authService.authenticate();
      return result;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Authentication failed';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const disconnect = React.useCallback((): void => {
    authService.disconnect();
  }, []);

  return {
    isAuthenticated,
    wallet,
    connectWallet,
    authenticate,
    disconnect,
    loading,
    error
  };
}