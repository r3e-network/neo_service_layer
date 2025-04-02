'use client';

import React from 'react';

export interface WalletState {
  isConnected: boolean;
  address: string | null;
  balance: {
    NEO: string;
    GAS: string;
  } | null;
  provider: 'neoline' | 'o3' | null;
  network: 'MainNet' | 'TestNet' | null;
  isLoading: boolean;
  error: string | null;
}

interface NeoLineInterface {
  getAccount: () => Promise<{
    address: string;
    label: string;
  }>;
  getBalance: () => Promise<{
    NEO: string;
    GAS: string;
  }>;
  getNetworks: () => Promise<{
    chainId: number;
    networks: string[];
    defaultNetwork: string;
  }>;
  signMessage: (params: { message: string; address: string }) => Promise<{
    data: string;
    publicKey: string;
    signature: string;
  }>;
}

declare global {
  interface Window {
    NEOLine?: {
      instance: NeoLineInterface;
    };
    neo3Dapi?: NeoLineInterface;
  }
}

export function useWallet() {
  const [walletState, setWalletState] = React.useState<WalletState>({
    isConnected: false,
    address: null,
    balance: null,
    provider: null,
    network: null,
    isLoading: false,
    error: null,
  });

  // Add a state to track if we've checked for the wallet
  const [hasCheckedWallet, setHasCheckedWallet] = React.useState(false);

  const checkNeoLineInstalled = React.useCallback((): NeoLineInterface | null => {
    if (typeof window === 'undefined') return null;
    
    // First try to access the objects directly
    // Check for NEO3 NeoLine extension
    if (window.neo3Dapi) {
      console.log('Detected Neo N3 wallet via neo3Dapi');
      return window.neo3Dapi;
    }
    
    // Check for legacy NeoLine extension
    if (window.NEOLine && window.NEOLine.instance) {
      console.log('Detected legacy NeoLine wallet');
      return window.NEOLine.instance;
    }
    
    // Check if NeoLine was detected in a more lenient way previously - for Chrome extensions
    // that might be slower to inject their objects
    try {
      // Use localStorage to communicate between detection attempts
      const neolineDetected = localStorage.getItem('neolineDetected');
      if (neolineDetected === 'true') {
        console.log('NeoLine previously detected based on localStorage flag');
        return {
          getAccount: async () => {
            throw new Error('NeoLine not fully initialized. Please try again.');
          },
          getBalance: async () => {
            throw new Error('NeoLine not fully initialized. Please try again.');
          },
          getNetworks: async () => {
            throw new Error('NeoLine not fully initialized. Please try again.');
          },
          signMessage: async () => {
            throw new Error('NeoLine not fully initialized. Please try again.');
          }
        };
      }
      
      // Check for any properties that might indicate NeoLine is installed but not initialized
      if ('neo3Dapi' in window || 'NEOLine' in window) {
        console.log('NeoLine properties exist but not initialized');
        // Mark that we've seen NeoLine
        localStorage.setItem('neolineDetected', 'true');
        return {
          getAccount: async () => {
            throw new Error('NeoLine not fully initialized. Please refresh or try again later.');
          },
          getBalance: async () => {
            throw new Error('NeoLine not fully initialized. Please refresh or try again later.');
          },
          getNetworks: async () => {
            throw new Error('NeoLine not fully initialized. Please refresh or try again later.');
          },
          signMessage: async () => {
            throw new Error('NeoLine not fully initialized. Please refresh or try again later.');
          }
        };
      }
    } catch (e) {
      // Silently fail if localStorage is not available
      console.warn('Error accessing localStorage for NeoLine detection:', e);
    }
    
    // Get all the keys in the window object that might indicate NeoLine
    const neoRelatedKeys = Object.keys(window).filter(key => 
      key.toLowerCase().includes('neo') || 
      key.toLowerCase().includes('dapi') ||
      key.toLowerCase().includes('wallet')
    );
    
    if (neoRelatedKeys.length > 0) {
      console.log('Found Neo-related keys in window:', neoRelatedKeys);
      // Mark that we've seen something that might be NeoLine
      try {
        localStorage.setItem('neolineDetected', 'true');
      } catch (e) {
        // Silently fail if localStorage is not available
      }
      return {
        getAccount: async () => {
          throw new Error('Neo-related properties found but wallet not fully initialized. Please try again.');
        },
        getBalance: async () => {
          throw new Error('Neo-related properties found but wallet not fully initialized. Please try again.');
        },
        getNetworks: async () => {
          throw new Error('Neo-related properties found but wallet not fully initialized. Please try again.');
        },
        signMessage: async () => {
          throw new Error('Neo-related properties found but wallet not fully initialized. Please try again.');
        }
      };
    }
    
    // If hasCheckedWallet is true and we still don't see the extension, log it
    if (hasCheckedWallet) {
      console.log('NeoLine wallet not detected after checking');
    } else {
      console.log('Still waiting for NeoLine wallet check');
    }
    
    return null;
  }, [hasCheckedWallet]);

  // Wait for neo3Dapi to be initialized by the extension
  React.useEffect(() => {
    if (typeof window === 'undefined') return;
    
    // Track if wallet was found in any check
    let walletFound = false;
    
    // The NeoLine extension might not be immediately available when the page loads
    // It often injects the neo3Dapi object after a delay
    const checkForNeoLine = () => {
      console.log('Checking for NeoLine...', {
        neo3Dapi: !!window.neo3Dapi,
        neoline: !!(window.NEOLine && window.NEOLine.instance)
      });
      
      // Consider the wallet installed even if only the object is present but not fully initialized
      // This handles cases where the extension is installed but not yet ready
      if ('neo3Dapi' in window || 'NEOLine' in window) {
        console.log('NeoLine wallet detected by property check');
        walletFound = true;
        setHasCheckedWallet(true);
        return true;
      }
      
      // Traditional check for fully initialized extensions
      if (window.neo3Dapi || (window.NEOLine && window.NEOLine.instance)) {
        console.log('NeoLine wallet detected by initialized object');
        walletFound = true;
        setHasCheckedWallet(true);
        return true;
      }
      
      return false;
    };

    // Check immediately
    const immediateResult = checkForNeoLine();
    
    // If not found immediately, try multiple checks with increasing delays
    const checkAttempts = [300, 600, 1000, 2000, 3000];
    const timeoutIds: NodeJS.Timeout[] = [];

    if (!immediateResult) {
      console.log('NeoLine not detected immediately, scheduling checks...');
      
      checkAttempts.forEach((delay, index) => {
        const timeoutId = setTimeout(() => {
          console.log(`NeoLine check attempt ${index + 1} (after ${delay}ms)`);
          const found = checkForNeoLine();
          
          // If this is the last attempt and we still haven't found it, mark as checked
          if (!found && index === checkAttempts.length - 1) {
            console.log('Final NeoLine check completed - not found');
            setHasCheckedWallet(true);
          }
        }, delay);
        
        timeoutIds.push(timeoutId);
      });
    }
    
    // Listen for the NEOLine.NEO.EVENT.READY event (for legacy NeoLine)
    const handleNeoLineReady = () => {
      console.log('NeoLine ready event detected');
      walletFound = true;
      setHasCheckedWallet(true);
    };
    
    window.addEventListener('NEOLine.NEO.EVENT.READY', handleNeoLineReady);
    
    // Clean up
    return () => {
      timeoutIds.forEach(id => clearTimeout(id));
      window.removeEventListener('NEOLine.NEO.EVENT.READY', handleNeoLineReady);
    };
  }, []);

  const connect = React.useCallback(async () => {
    // Reset the state
    setWalletState((prevState) => ({
      ...prevState,
      isLoading: true,
      error: null,
    }));

    try {
      console.log('Direct window object check:', { 
        windowKeys: Object.keys(window).filter(key => key.includes('neo') || key.includes('NEO')),
        hasNeo3Dapi: 'neo3Dapi' in window,
        hasNEOLine: 'NEOLine' in window
      });
      
      // Try different ways to access NeoLine - some versions inject the API differently
      let neoLineAPI: NeoLineInterface | null = null;
      
      // First try direct access to neo3Dapi
      if (window.neo3Dapi) {
        console.log('Found neo3Dapi directly on window');
        neoLineAPI = window.neo3Dapi;
      } 
      // Then try NEOLine.instance
      else if (window.NEOLine && window.NEOLine.instance) {
        console.log('Found NEOLine.instance');
        neoLineAPI = window.NEOLine.instance;
      }
      // If neither object is available but we see the properties, wait longer
      else if ('neo3Dapi' in window || 'NEOLine' in window) {
        console.log('NeoLine properties found but not initialized, waiting longer...');
        
        // Increase wait time here
        await new Promise(resolve => setTimeout(resolve, 2500)); // Increased to 2.5 seconds
        
        // Check again after waiting
        if (window.neo3Dapi) {
          console.log('Found neo3Dapi after waiting');
          neoLineAPI = window.neo3Dapi;
        } else if (window.NEOLine && window.NEOLine.instance) {
          console.log('Found NEOLine.instance after waiting');
          neoLineAPI = window.NEOLine.instance;
        }
      }
      
      // If we still don't have a valid API object, try a different approach
      if (!neoLineAPI) {
        console.log('No NeoLine API found through direct access, trying indirect detection');
        
        // Create an event to detect when NeoLine might be ready
        const readyPromise = new Promise<void>((resolve) => {
          const checkForNeoLine = () => {
            if (window.neo3Dapi || (window.NEOLine && window.NEOLine.instance)) {
              resolve();
              return true;
            }
            return false;
          };
          
          // Check immediately
          if (checkForNeoLine()) return;
          
          // Listen for the ready event
          const handleEvent = () => {
            console.log('NeoLine ready event detected');
            resolve();
          };
          
          window.addEventListener('NEOLine.NEO.EVENT.READY', handleEvent);
          window.addEventListener('NEO3_READY', handleEvent);
          
          // Also set a timeout just in case
          setTimeout(() => {
            window.removeEventListener('NEOLine.NEO.EVENT.READY', handleEvent);
            window.removeEventListener('NEO3_READY', handleEvent);
            resolve(); // Resolve anyway after timeout
          }, 3000);
        });
        
        // Wait for the ready event or timeout
        await readyPromise;
        
        // Try again to get the API
        if (window.neo3Dapi) {
          neoLineAPI = window.neo3Dapi;
        } else if (window.NEOLine && window.NEOLine.instance) {
          neoLineAPI = window.NEOLine.instance;
        }
      }
      
      if (!neoLineAPI) {
        console.error('NeoLine wallet not detected after multiple attempts');
        throw new Error('NeoLine wallet not detected. Please make sure it is installed and enabled, then refresh the page.'); // Adjusted error message
      }

      console.log('Requesting account access...');
      // Get the user's account
      const account = await neoLineAPI.getAccount();
      console.log('Account response:', account);
      
      if (!account || !account.address) {
        throw new Error('Failed to get account details from NeoLine.');
      }

      // Get the balance
      console.log('Requesting balance...');
      const balance = await neoLineAPI.getBalance();
      console.log('Balance response:', balance);
      
      // Get the network
      console.log('Requesting network...');
      const networkData = await neoLineAPI.getNetworks();
      console.log('Network response:', networkData);
      
      const network = (networkData.defaultNetwork === 'N3MainNet' || networkData.defaultNetwork === '1') 
        ? 'MainNet' as const
        : 'TestNet' as const;

      // Update the state
      setWalletState({
        isConnected: true,
        address: account.address,
        balance,
        provider: 'neoline',
        network,
        isLoading: false,
        error: null,
      });

      // Store connection in local storage
      localStorage.setItem('walletConnected', 'true');
      localStorage.setItem('walletProvider', 'neoline');

      return {
        address: account.address,
        balance,
        network,
      };
    } catch (error) {
      console.error('Error connecting to wallet:', error);
      setWalletState({
        isConnected: false,
        address: null,
        balance: null,
        provider: null,
        network: null,
        isLoading: false,
        error: error instanceof Error ? error.message : 'Failed to connect to wallet',
      });
      throw error;
    }
  }, []);

  const disconnect = React.useCallback(() => {
    setWalletState({
      isConnected: false,
      address: null,
      balance: null,
      provider: null,
      network: null,
      isLoading: false,
      error: null,
    });
    
    // Remove connection from local storage
    localStorage.removeItem('walletConnected');
    localStorage.removeItem('walletProvider');
  }, []);

  const refreshBalance = React.useCallback(async () => {
    if (!walletState.isConnected) return;

    try {
      const neoLineAPI = checkNeoLineInstalled();
      if (!neoLineAPI) {
        throw new Error('NeoLine extension not installed.');
      }

      const balance = await neoLineAPI.getBalance();
      
      setWalletState(prev => ({
        ...prev,
        balance
      }));

      return balance;
    } catch (error) {
      console.error('Error refreshing balance:', error);
      // Don't update the state on error to maintain existing balance
    }
  }, [walletState.isConnected, checkNeoLineInstalled]);

  // Check for previous connection on mount
  React.useEffect(() => {
    // Only attempt auto-connect if we've detected the wallet and it's not already connected
    if (!hasCheckedWallet || walletState.isConnected) return;
    
    const attemptReconnect = async () => {
      const wasConnected = localStorage.getItem('walletConnected') === 'true';
      const provider = localStorage.getItem('walletProvider');
      
      if (wasConnected && provider === 'neoline') {
        console.log('Attempting to reconnect to previously connected wallet');
        try {
          await connect();
          console.log('Auto-reconnect successful');
        } catch (error) {
          // Silent failure for auto-connect
          console.warn('Failed to auto-connect to wallet:', error);
        }
      }
    };
    
    // If NeoLine is installed, try to reconnect
    if (checkNeoLineInstalled()) {
      attemptReconnect();
    }
  }, [connect, hasCheckedWallet, walletState.isConnected, checkNeoLineInstalled]);

  return {
    ...walletState,
    connect,
    disconnect,
    refreshBalance,
    // Consider both fully initialized objects, properties in the window, and localStorage flag
    isNeoLineInstalled: hasCheckedWallet ? 
      (!!checkNeoLineInstalled() || 
       ('neo3Dapi' in window) || 
       ('NEOLine' in window) || 
       localStorage.getItem('neolineDetected') === 'true') : 
      true,
    hasCheckedWallet
  };
} 