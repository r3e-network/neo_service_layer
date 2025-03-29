// Use default import for React
import React from 'react';
import { useWebSocket } from '../../../hooks/useWebSocket';
import { useAuth } from '../../../hooks/useAuth';
import { SECRETS_CONSTANTS } from '../constants';
import {
  Secret,
  SecretPermission,
  SecretFilter,
  SecretUpdatePayload,
  SecretPermissionUpdatePayload,
  SecretRotationResult,
  SecretMetrics,
  ValidationError
} from '../types/types';
import { validateSecret, validateSecretUpdate, validatePermissionUpdate } from '../utils/validation';

export function useSecrets(filter?: SecretFilter) {
  const [secrets, setSecrets] = React.useState<Secret[]>([]);
  const [permissions, setPermissions] = React.useState<SecretPermission[]>([]);
  const [metrics, setMetrics] = React.useState<SecretMetrics>({
    totalSecrets: 0,
    needsRotation: 0,
    expired: 0,
    upToDate: 0
  });
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  const { isAuthenticated, user, signMessage } = useAuth();
  const { socket } = useWebSocket({
    url: SECRETS_CONSTANTS.WEBSOCKET_URL,
    autoReconnect: true
  });

  // Fetch secrets with optional filtering
  const fetchSecrets = React.useCallback(async () => {
    if (!isAuthenticated) return;

    try {
      setLoading(true);
      const queryParams = new URLSearchParams();
      if (filter) {
        Object.entries(filter).forEach(([key, value]) => {
          if (Array.isArray(value)) {
            value.forEach(v => queryParams.append(key, v));
          } else if (value !== undefined) {
            queryParams.append(key, value.toString());
          }
        });
      }

      const response = await fetch(`/api/secrets?${queryParams}`);
      if (!response.ok) throw new Error('Failed to fetch secrets');

      const data = await response.json();
      setSecrets(data.secrets);
      setPermissions(data.permissions);
      setMetrics(data.metrics);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unknown error occurred');
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, filter]);

  // Create a new secret
  const createSecret = async (secretData: Partial<Secret>): Promise<Secret> => {
    if (!isAuthenticated) throw new Error('Authentication required');

    // Validate secret data
    const errors = validateSecret(secretData);
    if (errors.length > 0) {
      throw new Error(errors[0].message);
    }

    // Encrypt sensitive data if needed
    if (secretData.value && secretData.type === 'encrypted') {
      // Use a simple encryption for now, would be replaced with actual encryption in production
      secretData.value = btoa(secretData.value);
    }

    const timestamp = Date.now();
    const signature = await signMessage(`create-secret:${timestamp}`);

    const response = await fetch('/api/secrets', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Signature': signature,
        'X-Timestamp': timestamp.toString()
      },
      body: JSON.stringify(secretData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    const createdSecret = await response.json();
    setSecrets(prev => [...prev, createdSecret]);
    return createdSecret;
  };

  // Update an existing secret
  const updateSecret = async (secretId: string, secretData: SecretUpdatePayload): Promise<Secret> => {
    if (!isAuthenticated) throw new Error('Authentication required');

    // Validate update data
    const errors = validateSecretUpdate(secretData);
    if (errors.length > 0) {
      throw new Error(errors[0].message);
    }

    // Encrypt sensitive data if needed
    if (secretData.value && (secretData as any).type === 'encrypted') {
      // Use a simple encryption for now, would be replaced with actual encryption in production
      secretData.value = btoa(secretData.value);
    }

    const timestamp = Date.now();
    const signature = await signMessage(`update-secret:${secretId}:${timestamp}`);

    const response = await fetch(`/api/secrets/${secretId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'X-Signature': signature,
        'X-Timestamp': timestamp.toString()
      },
      body: JSON.stringify(secretData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    const updatedSecret = await response.json();
    setSecrets(prev => prev.map(s => s.id === secretId ? updatedSecret : s));
    return updatedSecret;
  };

  // Delete a secret
  const deleteSecret = async (secretId: string): Promise<void> => {
    if (!isAuthenticated) throw new Error('Authentication required');

    const timestamp = Date.now();
    const signature = await signMessage(`delete-secret:${secretId}:${timestamp}`);

    const response = await fetch(`/api/secrets/${secretId}`, {
      method: 'DELETE',
      headers: {
        'X-Signature': signature,
        'X-Timestamp': timestamp.toString()
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    setSecrets(prev => prev.filter(s => s.id !== secretId));
    setPermissions(prev => prev.filter(p => p.secretId !== secretId));
  };

  // Update permission for a secret
  const updatePermission = async (
    secretId: string,
    userId: string,
    permissionData: SecretPermissionUpdatePayload
  ): Promise<SecretPermission> => {
    if (!isAuthenticated) throw new Error('Authentication required');

    // Validate permission data
    const errors = validatePermissionUpdate(permissionData);
    if (errors.length > 0) {
      throw new Error(errors[0].message);
    }

    const timestamp = Date.now();
    const signature = await signMessage(`update-permission:${secretId}:${userId}:${timestamp}`);

    const response = await fetch(`/api/secrets/${secretId}/permissions/${userId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'X-Signature': signature,
        'X-Timestamp': timestamp.toString()
      },
      body: JSON.stringify(permissionData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    const updatedPermission = await response.json();
    setPermissions(prev => {
      const existingIndex = prev.findIndex(
        p => p.secretId === secretId && p.userId === userId
      );
      
      if (existingIndex >= 0) {
        return [
          ...prev.slice(0, existingIndex),
          updatedPermission,
          ...prev.slice(existingIndex + 1)
        ];
      } else {
        return [...prev, updatedPermission];
      }
    });
    
    return updatedPermission;
  };

  // Delete permission for a secret
  const deletePermission = async (secretId: string, userId: string): Promise<void> => {
    if (!isAuthenticated) throw new Error('Authentication required');

    const timestamp = Date.now();
    const signature = await signMessage(`delete-permission:${secretId}:${userId}:${timestamp}`);

    const response = await fetch(`/api/secrets/${secretId}/permissions/${userId}`, {
      method: 'DELETE',
      headers: {
        'X-Signature': signature,
        'X-Timestamp': timestamp.toString()
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    setPermissions(prev => prev.filter(
      p => !(p.secretId === secretId && p.userId === userId)
    ));
  };

  // Rotate secret
  const rotateSecret = async (secretId: string): Promise<SecretRotationResult> => {
    if (!isAuthenticated) throw new Error('Authentication required');

    const timestamp = Date.now();
    const signature = await signMessage(`rotate-secret:${secretId}:${timestamp}`);

    const response = await fetch(`/api/secrets/${secretId}/rotate`, {
      method: 'POST',
      headers: {
        'X-Signature': signature,
        'X-Timestamp': timestamp.toString()
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    const result = await response.json();
    setSecrets(prev => prev.map(s => s.id === secretId ? result.secret : s));
    return result;
  };

  // WebSocket event handlers
  React.useEffect(() => {
    if (!socket || !isAuthenticated) return;

    const handleSecretUpdate = (event: any) => {
      const data = event.data ? JSON.parse(event.data) : event;
      setSecrets(prev => prev.map(s => s.id === data.secret.id ? data.secret : s));
    };

    const handlePermissionUpdate = (event: any) => {
      const data = event.data ? JSON.parse(event.data) : event;
      setPermissions(prev => prev.map(p => 
        p.secretId === data.permission.secretId && p.userId === data.permission.userId
          ? data.permission
          : p
      ));
    };

    // Use addEventListener for WebSocket events (MUI v7 compatibility)
    socket.addEventListener(SECRETS_CONSTANTS.WEBSOCKET_EVENTS.SECRET_UPDATE, handleSecretUpdate);
    socket.addEventListener(SECRETS_CONSTANTS.WEBSOCKET_EVENTS.PERMISSION_UPDATE, handlePermissionUpdate);

    return () => {
      socket.removeEventListener(SECRETS_CONSTANTS.WEBSOCKET_EVENTS.SECRET_UPDATE, handleSecretUpdate);
      socket.removeEventListener(SECRETS_CONSTANTS.WEBSOCKET_EVENTS.PERMISSION_UPDATE, handlePermissionUpdate);
    };
  }, [socket, isAuthenticated]);

  // Initial fetch
  React.useEffect(() => {
    if (isAuthenticated) {
      fetchSecrets();
    }
  }, [isAuthenticated, fetchSecrets]);

  return {
    secrets,
    permissions,
    metrics,
    loading,
    error,
    createSecret,
    updateSecret,
    deleteSecret,
    updatePermission,
    deletePermission,
    rotateSecret,
    refresh: fetchSecrets
  };
}