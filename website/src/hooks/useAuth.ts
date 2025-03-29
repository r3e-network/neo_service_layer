// @ts-ignore
import * as React from 'react';

interface User {
  id: string;
  username: string;
  email: string;
  role: string;
}

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: Error | null;
  token: string | null;
}

export function useAuth() {
  const [authState, setAuthState] = React.useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
    error: null,
    token: null
  });

  const getStoredToken = React.useCallback(() => {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('auth_token');
    }
    return null;
  }, []);

  const setStoredToken = React.useCallback((token: string | null) => {
    if (typeof window !== 'undefined') {
      if (token) {
        localStorage.setItem('auth_token', token);
      } else {
        localStorage.removeItem('auth_token');
      }
    }
  }, []);

  const login = React.useCallback(async (username: string, password: string) => {
    setAuthState(prev => ({ ...prev, isLoading: true, error: null }));
    
    try {
      // Mock API call - replace with actual API call
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ username, password })
      });
      
      if (!response.ok) {
        throw new Error('Login failed');
      }
      
      const data = await response.json();
      
      setStoredToken(data.token);
      
      setAuthState({
        user: data.user,
        isAuthenticated: true,
        isLoading: false,
        error: null,
        token: data.token
      });
      
      return data.user;
    } catch (error) {
      setAuthState(prev => ({
        ...prev,
        isAuthenticated: false,
        isLoading: false,
        error: error as Error,
        token: null
      }));
      
      throw error;
    }
  }, [setStoredToken]);

  const logout = React.useCallback(() => {
    setStoredToken(null);
    setAuthState({
      user: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,
      token: null
    });
  }, [setStoredToken]);

  const checkAuth = React.useCallback(async () => {
    const token = getStoredToken();
    
    if (!token) {
      setAuthState({
        user: null,
        isAuthenticated: false,
        isLoading: false,
        error: null,
        token: null
      });
      return;
    }
    
    setAuthState(prev => ({ ...prev, isLoading: true }));
    
    try {
      // Mock API call - replace with actual API call
      const response = await fetch('/api/auth/me', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      
      if (!response.ok) {
        throw new Error('Authentication failed');
      }
      
      const user = await response.json();
      
      setAuthState({
        user,
        isAuthenticated: true,
        isLoading: false,
        error: null,
        token
      });
    } catch (error) {
      setStoredToken(null);
      setAuthState({
        user: null,
        isAuthenticated: false,
        isLoading: false,
        error: error as Error,
        token: null
      });
    }
  }, [getStoredToken, setStoredToken]);

  const signMessage = React.useCallback(async (message: string): Promise<string> => {
    if (!authState.isAuthenticated || !authState.token) {
      throw new Error('User not authenticated');
    }
    
    try {
      // Mock API call - replace with actual API call
      const response = await fetch('/api/auth/sign', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authState.token}`
        },
        body: JSON.stringify({ message })
      });
      
      if (!response.ok) {
        throw new Error('Signing failed');
      }
      
      const data = await response.json();
      return data.signature;
    } catch (error) {
      throw error;
    }
  }, [authState.isAuthenticated, authState.token]);

  React.useEffect(() => {
    checkAuth();
  }, [checkAuth]);

  return {
    user: authState.user,
    isAuthenticated: authState.isAuthenticated,
    isLoading: authState.isLoading,
    error: authState.error,
    token: authState.token,
    login,
    logout,
    checkAuth,
    signMessage
  };
}
