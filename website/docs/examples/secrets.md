# Secrets Management Examples

## Overview
These examples demonstrate how to use the Secrets Management service to securely store and access sensitive information within TEE.

## Basic Secrets Management

### Creating and Accessing Secrets
```typescript
import { useState } from 'react';
import { SecretVault } from '../utils/vault';
import { useWallet } from '../src/app/hooks/useWallet';

function SecretManager() {
  const { neoAddress, signMessage } = useWallet();
  const [secretName, setSecretName] = useState('');
  const [secretValue, setSecretValue] = useState('');
  const vault = new SecretVault({
    teeEnabled: true,
    backupEnabled: true,
    rotationPeriod: 24 * 60 * 60 * 1000 // 24 hours
  });

  const createSecret = async () => {
    try {
      // Sign the creation request
      const message = `Create secret: ${secretName}`;
      const signature = await signMessage(message);

      await vault.createSecret({
        id: \`secret_\${Date.now()}\`,
        name: secretName,
        value: secretValue,
        neoAddress,
        createdAt: new Date().toISOString(),
        lastAccessed: new Date().toISOString(),
        accessCount: 0,
        permissions: {
          functionIds: [],
          roles: ['owner']
        },
        teeConfig: await vault.generateTEEConfig()
      });

      setSecretName('');
      setSecretValue('');
    } catch (error) {
      console.error('Failed to create secret:', error);
    }
  };

  return (
    <div>
      <input
        value={secretName}
        onChange={(e) => setSecretName(e.target.value)}
        placeholder="Secret Name"
      />
      <input
        type="password"
        value={secretValue}
        onChange={(e) => setSecretValue(e.target.value)}
        placeholder="Secret Value"
      />
      <button onClick={createSecret}>Create Secret</button>
    </div>
  );
}
```

### Listing and Managing Secrets
```typescript
import { useEffect, useState } from 'react';
import { SecretVault } from '../utils/vault';
import { useWallet } from '../src/app/hooks/useWallet';

interface Secret {
  id: string;
  name: string;
  lastAccessed: string;
  accessCount: number;
}

function SecretsList() {
  const { neoAddress } = useWallet();
  const [secrets, setSecrets] = useState<Secret[]>([]);
  const vault = new SecretVault({
    teeEnabled: true,
    backupEnabled: true,
    rotationPeriod: 24 * 60 * 60 * 1000
  });

  useEffect(() => {
    const fetchSecrets = async () => {
      if (!neoAddress) return;
      try {
        const userSecrets = await vault.listSecrets(neoAddress);
        setSecrets(userSecrets);
      } catch (error) {
        console.error('Failed to fetch secrets:', error);
      }
    };

    fetchSecrets();
  }, [neoAddress]);

  const deleteSecret = async (id: string) => {
    try {
      await vault.deleteSecret(id);
      setSecrets(prev => prev.filter(s => s.id !== id));
    } catch (error) {
      console.error('Failed to delete secret:', error);
    }
  };

  return (
    <div>
      <h2>Your Secrets</h2>
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Last Accessed</th>
            <th>Access Count</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {secrets.map(secret => (
            <tr key={secret.id}>
              <td>{secret.name}</td>
              <td>{new Date(secret.lastAccessed).toLocaleString()}</td>
              <td>{secret.accessCount}</td>
              <td>
                <button onClick={() => deleteSecret(secret.id)}>
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

## Advanced Usage

### Function-Specific Secret Access
```typescript
import { useState, useEffect } from 'react';
import { SecretVault } from '../utils/vault';

interface FunctionSecret {
  functionId: string;
  secretId: string;
  permissions: string[];
}

function FunctionSecretManager() {
  const [functionSecrets, setFunctionSecrets] = useState<FunctionSecret[]>([]);
  const vault = new SecretVault({
    teeEnabled: true,
    backupEnabled: true,
    rotationPeriod: 24 * 60 * 60 * 1000
  });

  const grantAccess = async (functionId: string, secretId: string) => {
    try {
      const secret = await vault.getSecret(secretId);
      await vault.updateSecret({
        ...secret,
        permissions: {
          ...secret.permissions,
          functionIds: [...secret.permissions.functionIds, functionId]
        }
      });
    } catch (error) {
      console.error('Failed to grant access:', error);
    }
  };

  const revokeAccess = async (functionId: string, secretId: string) => {
    try {
      const secret = await vault.getSecret(secretId);
      await vault.updateSecret({
        ...secret,
        permissions: {
          ...secret.permissions,
          functionIds: secret.permissions.functionIds.filter(
            id => id !== functionId
          )
        }
      });
    } catch (error) {
      console.error('Failed to revoke access:', error);
    }
  };

  return (
    <div>
      <h2>Function Secret Access</h2>
      {/* Access management UI */}
    </div>
  );
}
```

### Secret Rotation Management
```typescript
import { useState, useEffect } from 'react';
import { SecretVault } from '../utils/vault';

function SecretRotationManager() {
  const [rotationStatus, setRotationStatus] = useState<{
    lastRotation: string;
    nextRotation: string;
    inProgress: boolean;
  }>({
    lastRotation: '',
    nextRotation: '',
    inProgress: false
  });

  const vault = new SecretVault({
    teeEnabled: true,
    backupEnabled: true,
    rotationPeriod: 24 * 60 * 60 * 1000
  });

  const rotateSecrets = async () => {
    try {
      setRotationStatus(prev => ({ ...prev, inProgress: true }));
      await vault.rotateSecrets();
      setRotationStatus(prev => ({
        ...prev,
        inProgress: false,
        lastRotation: new Date().toISOString(),
        nextRotation: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()
      }));
    } catch (error) {
      console.error('Failed to rotate secrets:', error);
      setRotationStatus(prev => ({ ...prev, inProgress: false }));
    }
  };

  return (
    <div>
      <h2>Secret Rotation</h2>
      <div>
        Last Rotation: {new Date(rotationStatus.lastRotation).toLocaleString()}
      </div>
      <div>
        Next Rotation: {new Date(rotationStatus.nextRotation).toLocaleString()}
      </div>
      <button
        onClick={rotateSecrets}
        disabled={rotationStatus.inProgress}
      >
        {rotationStatus.inProgress ? 'Rotating...' : 'Rotate Now'}
      </button>
    </div>
  );
}
```

### Secret Access Logging
```typescript
import { useState, useEffect } from 'react';
import { SecretVault } from '../utils/vault';

interface AccessLog {
  id: string;
  secretId: string;
  timestamp: string;
  functionId: string;
  status: 'granted' | 'denied';
}

function SecretAccessLogs() {
  const [logs, setLogs] = useState<AccessLog[]>([]);
  const [filter, setFilter] = useState<{
    secretId?: string;
    status?: 'granted' | 'denied';
  }>({});

  const vault = new SecretVault({
    teeEnabled: true,
    backupEnabled: true,
    rotationPeriod: 24 * 60 * 60 * 1000
  });

  useEffect(() => {
    const fetchLogs = async () => {
      try {
        // In a real implementation, this would fetch from a secure log store
        const accessLogs = await vault.getAccessLogs(filter);
        setLogs(accessLogs);
      } catch (error) {
        console.error('Failed to fetch access logs:', error);
      }
    };

    fetchLogs();
  }, [filter]);

  return (
    <div>
      <h2>Secret Access Logs</h2>
      <div>
        <select
          value={filter.status || ''}
          onChange={(e) => setFilter(prev => ({
            ...prev,
            status: e.target.value as 'granted' | 'denied' | undefined
          }))}
        >
          <option value="">All Status</option>
          <option value="granted">Granted</option>
          <option value="denied">Denied</option>
        </select>
      </div>
      <table>
        <thead>
          <tr>
            <th>Timestamp</th>
            <th>Secret ID</th>
            <th>Function ID</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {logs.map(log => (
            <tr key={log.id}>
              <td>{new Date(log.timestamp).toLocaleString()}</td>
              <td>{log.secretId}</td>
              <td>{log.functionId}</td>
              <td>{log.status}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

## Testing Examples

### Unit Testing Secret Management
```typescript
import { render, screen, fireEvent } from '@testing-library/react';
import { SecretVault } from '../utils/vault';
import SecretManager from './SecretManager';

jest.mock('../utils/vault');
jest.mock('../src/app/hooks/useWallet', () => ({
  useWallet: () => ({
    neoAddress: 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd',
    signMessage: async (msg: string) => 'signed_' + msg
  })
}));

describe('SecretManager', () => {
  let vault: jest.Mocked<SecretVault>;

  beforeEach(() => {
    vault = new SecretVault({
      teeEnabled: true,
      backupEnabled: true,
      rotationPeriod: 24 * 60 * 60 * 1000
    }) as jest.Mocked<SecretVault>;
    
    vault.createSecret = jest.fn();
    vault.generateTEEConfig = jest.fn().mockResolvedValue({
      encryptionKeyId: 'test-key',
      attestationToken: 'test-token',
      mrEnclave: 'test-mrenclave'
    });
  });

  it('creates a new secret', async () => {
    render(<SecretManager />);

    fireEvent.change(
      screen.getByPlaceholderText('Secret Name'),
      { target: { value: 'API Key' } }
    );

    fireEvent.change(
      screen.getByPlaceholderText('Secret Value'),
      { target: { value: 'secret123' } }
    );

    fireEvent.click(screen.getByText('Create Secret'));

    expect(vault.createSecret).toHaveBeenCalledWith(
      expect.objectContaining({
        name: 'API Key',
        value: 'secret123',
        neoAddress: 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd'
      })
    );
  });
});
```