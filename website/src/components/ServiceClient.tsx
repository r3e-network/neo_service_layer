'use client';

// @ts-ignore
import * as React from 'react';

interface ServiceClientState {
  isConnected: boolean;
  address: string | null;
  error: string | null;
}

export function useServiceClient() {
  const [state, setState] = React.useState<ServiceClientState>({
    isConnected: false,
    address: null,
    error: null,
  });

  React.useEffect(() => {
    // Initialize client on mount
    initializeClient();
  }, []);

  const initializeClient = async () => {
    try {
      // Check if Neo wallet is available
      if (!(window as any).NEOLineN3) {
        throw new Error('Please install NeoLine wallet extension');
      }

      // Initialize NEOLine
      const neolineN3 = new (window as any).NEOLineN3.Init();

      // Get connected account
      const { address } = await neolineN3.getAccount();

      setState({
        isConnected: true,
        address,
        error: null,
      });
    } catch (error) {
      setState({
        isConnected: false,
        address: null,
        error: (error as Error).message,
      });
    }
  };

  const signMessage = async (message: string): Promise<string> => {
    try {
      if (!state.isConnected) {
        throw new Error('Not connected to wallet');
      }

      const neolineN3 = new (window as any).NEOLineN3.Init();
      const { signature } = await neolineN3.signMessage({
        message,
        address: state.address!,
      });

      return signature;
    } catch (error) {
      throw new Error(`Failed to sign message: ${(error as Error).message}`);
    }
  };

  const disconnect = () => {
    setState({
      isConnected: false,
      address: null,
      error: null,
    });
  };

  return {
    ...state,
    signMessage,
    disconnect,
    connect: initializeClient,
  };
}

export function ServiceClientProvider({ children }: { children: React.ReactNode }) {
  const client = useServiceClient();

  if (client.error) {
    return (
      <div className="rounded-lg bg-red-50 p-4">
        <div className="flex">
          <div className="flex-shrink-0">
            <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
            </svg>
          </div>
          <div className="ml-3">
            <h3 className="text-sm font-medium text-red-800">Connection Error</h3>
            <div className="mt-2 text-sm text-red-700">
              <p>{client.error}</p>
            </div>
            <div className="mt-4">
              <button
                type="button"
                onClick={client.connect}
                className="rounded-md bg-red-50 px-2 py-1.5 text-sm font-medium text-red-800 hover:bg-red-100 focus:outline-none focus:ring-2 focus:ring-red-600 focus:ring-offset-2"
              >
                Try Again
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!client.isConnected) {
    return (
      <div className="rounded-lg bg-gray-50 p-4">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="text-sm font-medium text-gray-900">Connect Wallet</h3>
            <p className="mt-1 text-sm text-gray-500">
              Connect your Neo wallet to use the playground
            </p>
          </div>
          <button
            type="button"
            onClick={client.connect}
            className="rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50"
          >
            Connect
          </button>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between rounded-lg bg-gray-50 p-4">
        <div>
          <h3 className="text-sm font-medium text-gray-900">Connected Wallet</h3>
          <p className="mt-1 text-sm text-gray-500">
            {client.address}
          </p>
        </div>
        <button
          type="button"
          onClick={client.disconnect}
          className="rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50"
        >
          Disconnect
        </button>
      </div>
      {children}
    </div>
  );
} 