'use client';

import React from 'react';
import {
  KeyIcon,
  EyeIcon,
  EyeSlashIcon,
  TrashIcon,
  PlusIcon,
  ShieldCheckIcon,
  ClockIcon,
  DocumentDuplicateIcon,
} from '@heroicons/react/24/outline';
import { useWallet } from '../app/hooks/useWallet';
import { useToast } from '../app/hooks/useToast';
import { getTEEAttestation } from '../utils/tee';
import { formatNeoAddress, validateContractHash } from '../utils/neo';

interface Secret {
  id: string;
  name: string;
  description: string;
  createdAt: string;
  lastAccessed: string;
  accessCount: number;
  neoAddress: string;
  contractHash?: string;
  permissions: {
    functionIds: string[];
    roles: string[];
    contractMethods?: string[];
  };
}

interface SecretAccess {
  id: string;
  secretId: string;
  timestamp: string;
  functionId: string;
  neoAddress: string;
  contractHash?: string;
  method?: string;
  status: 'granted' | 'denied';
  reason?: string;
  teeVerification: {
    attestationValid: boolean;
    mrEnclaveMatch: boolean;
  };
}

export function SecretsManagement() {
  const { isConnected, connect, signMessage, neoAddress } = useWallet();
  const { showToast } = useToast();
  const [secrets, setSecrets] = React.useState<Secret[]>([]);
  const [accessLogs, setAccessLogs] = React.useState<SecretAccess[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [showNewSecretModal, setShowNewSecretModal] = React.useState(false);
  const [showPermissionsModal, setShowPermissionsModal] = React.useState(false);
  const [selectedSecret, setSelectedSecret] = React.useState<Secret | null>(null);
  const [newSecret, setNewSecret] = React.useState({
    name: '',
    description: '',
    value: '',
    contractHash: '',
    permissions: {
      functionIds: [] as string[],
      roles: [] as string[],
      contractMethods: [] as string[],
    },
  });

  React.useEffect(() => {
    if (isConnected && neoAddress) {
      fetchSecrets();
    }
  }, [isConnected, neoAddress]);

  const fetchSecrets = async () => {
    try {
      const signature = await signMessage('fetch-secrets');
      const response = await fetch('/.netlify/functions/secrets-management', {
        headers: {
          'x-signature': signature,
          'x-neo-address': neoAddress,
        },
      });
      
      if (!response.ok) {
        throw new Error('Failed to fetch secrets');
      }
      
      const data = await response.json();
      setSecrets(data);
    } catch (error) {
      showToast('Error fetching secrets', 'error');
      console.error('Failed to fetch secrets:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateSecret = async () => {
    try {
      if (newSecret.contractHash && !await validateContractHash(newSecret.contractHash)) {
        showToast('Invalid contract hash', 'error');
        return;
      }

      // Get TEE attestation
      const attestation = await getTEEAttestation('default-mrenclave');
      
      const secretData = {
        ...newSecret,
        attestationToken: attestation.attestationToken,
      };

      const signature = await signMessage(JSON.stringify(secretData));
      const response = await fetch('/.netlify/functions/secrets-management', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'x-signature': signature,
          'x-neo-address': neoAddress,
        },
        body: JSON.stringify(secretData),
      });

      if (!response.ok) {
        throw new Error('Failed to create secret');
      }

      const data = await response.json();
      setSecrets([...secrets, data]);
      setShowNewSecretModal(false);
      setNewSecret({
        name: '',
        description: '',
        value: '',
        contractHash: '',
        permissions: {
          functionIds: [],
          roles: [],
          contractMethods: [],
        },
      });
      showToast('Secret created successfully', 'success');
    } catch (error) {
      showToast('Error creating secret', 'error');
      console.error('Failed to create secret:', error);
    }
  };

  const handleDeleteSecret = async (secretId: string) => {
    try {
      const signature = await signMessage(`delete-secret-${secretId}`);
      const response = await fetch(`/.netlify/functions/secrets-management/${secretId}`, {
        method: 'DELETE',
        headers: {
          'x-signature': signature,
          'x-neo-address': neoAddress,
        },
      });

      if (!response.ok) {
        throw new Error('Failed to delete secret');
      }

      setSecrets(secrets.filter((s) => s.id !== secretId));
      showToast('Secret deleted successfully', 'success');
    } catch (error) {
      showToast('Error deleting secret', 'error');
      console.error('Failed to delete secret:', error);
    }
  };

  const handleUpdatePermissions = async () => {
    if (!selectedSecret) return;

    try {
      const signature = await signMessage(`update-permissions-${selectedSecret.id}`);
      const response = await fetch(
        `/.netlify/functions/secrets-management/${selectedSecret.id}`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'x-signature': signature,
            'x-neo-address': neoAddress,
          },
          body: JSON.stringify({
            permissions: selectedSecret.permissions,
          }),
        }
      );

      if (!response.ok) {
        throw new Error('Failed to update permissions');
      }

      const data = await response.json();
      setSecrets(
        secrets.map((s) => (s.id === selectedSecret.id ? data : s))
      );
      setShowPermissionsModal(false);
      setSelectedSecret(null);
      showToast('Permissions updated successfully', 'success');
    } catch (error) {
      showToast('Error updating permissions', 'error');
      console.error('Failed to update permissions:', error);
    }
  };

  const copyToClipboard = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text);
      showToast('Copied to clipboard', 'success');
    } catch (error) {
      showToast('Failed to copy', 'error');
    }
  };

  if (!isConnected) {
    return (
      <div className="text-center py-12">
        <h3 className="text-lg font-medium text-gray-900">Connect Neo N3 Wallet</h3>
        <p className="mt-2 text-sm text-gray-500">
          Please connect your Neo N3 wallet to manage secrets.
        </p>
        <button
          onClick={connect}
          className="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          Connect Wallet
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Secrets Management</h2>
          <p className="mt-1 text-sm text-gray-500">
            Connected as: {formatNeoAddress(neoAddress)}
          </p>
        </div>
        <button
          onClick={() => setShowNewSecretModal(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="h-5 w-5 mr-2" />
          New Secret
        </button>
      </div>

      {/* Secrets List */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {secrets.map((secret) => (
          <div
            key={secret.id}
            className="bg-white rounded-lg shadow p-6"
          >
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <KeyIcon className="h-8 w-8 text-indigo-600" />
                <div className="ml-4">
                  <h3 className="text-lg font-medium text-gray-900">
                    {secret.name}
                  </h3>
                  <p className="text-sm text-gray-500">{secret.description}</p>
                </div>
              </div>
              <div className="flex space-x-2">
                <button
                  onClick={() => copyToClipboard(secret.id)}
                  className="p-2 text-gray-400 hover:text-indigo-600"
                  title="Copy Secret ID"
                >
                  <DocumentDuplicateIcon className="h-5 w-5" />
                </button>
                <button
                  onClick={() => handleDeleteSecret(secret.id)}
                  className="p-2 text-gray-400 hover:text-red-600"
                  title="Delete Secret"
                >
                  <TrashIcon className="h-5 w-5" />
                </button>
              </div>
            </div>

            <div className="mt-4 grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="text-gray-500">Last Accessed</p>
                <p className="font-medium">
                  {new Date(secret.lastAccessed).toLocaleString()}
                </p>
              </div>
              <div>
                <p className="text-gray-500">Access Count</p>
                <p className="font-medium">{secret.accessCount}</p>
              </div>
            </div>

            {secret.contractHash && (
              <div className="mt-4 text-sm">
                <p className="text-gray-500">Contract Hash</p>
                <p className="font-medium truncate">{secret.contractHash}</p>
              </div>
            )}

            <div className="mt-4">
              <button
                onClick={() => {
                  setSelectedSecret(secret);
                  setShowPermissionsModal(true);
                }}
                className="inline-flex items-center px-3 py-1 text-sm text-indigo-600 hover:text-indigo-500"
              >
                <ShieldCheckIcon className="h-4 w-4 mr-1" />
                Manage Permissions
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* New Secret Modal */}
      {showNewSecretModal && (
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center">
          <div className="bg-white rounded-lg p-6 max-w-lg w-full">
            <h3 className="text-lg font-medium text-gray-900">Create New Secret</h3>
            <div className="mt-4 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Name
                </label>
                <input
                  type="text"
                  value={newSecret.name}
                  onChange={(e) =>
                    setNewSecret({ ...newSecret, name: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Description
                </label>
                <input
                  type="text"
                  value={newSecret.description}
                  onChange={(e) =>
                    setNewSecret({ ...newSecret, description: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Value
                </label>
                <input
                  type="password"
                  value={newSecret.value}
                  onChange={(e) =>
                    setNewSecret({ ...newSecret, value: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Contract Hash (Optional)
                </label>
                <input
                  type="text"
                  value={newSecret.contractHash}
                  onChange={(e) =>
                    setNewSecret({ ...newSecret, contractHash: e.target.value })
                  }
                  placeholder="Neo N3 contract hash"
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Function IDs (comma-separated)
                </label>
                <input
                  type="text"
                  value={newSecret.permissions.functionIds.join(', ')}
                  onChange={(e) =>
                    setNewSecret({
                      ...newSecret,
                      permissions: {
                        ...newSecret.permissions,
                        functionIds: e.target.value.split(',').map((id) => id.trim()),
                      },
                    })
                  }
                  placeholder="func_1, func_2, ..."
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Contract Methods (comma-separated, optional)
                </label>
                <input
                  type="text"
                  value={newSecret.permissions.contractMethods?.join(', ') || ''}
                  onChange={(e) =>
                    setNewSecret({
                      ...newSecret,
                      permissions: {
                        ...newSecret.permissions,
                        contractMethods: e.target.value.split(',').map((method) => method.trim()),
                      },
                    })
                  }
                  placeholder="transfer, mint, ..."
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div className="flex justify-end space-x-4">
                <button
                  onClick={() => setShowNewSecretModal(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 rounded-md"
                >
                  Cancel
                </button>
                <button
                  onClick={handleCreateSecret}
                  className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md"
                >
                  Create Secret
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Permissions Modal */}
      {showPermissionsModal && selectedSecret && (
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center">
          <div className="bg-white rounded-lg p-6 max-w-lg w-full">
            <h3 className="text-lg font-medium text-gray-900">
              Manage Permissions - {selectedSecret.name}
            </h3>
            <div className="mt-4 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Function IDs
                </label>
                <input
                  type="text"
                  value={selectedSecret.permissions.functionIds.join(', ')}
                  onChange={(e) =>
                    setSelectedSecret({
                      ...selectedSecret,
                      permissions: {
                        ...selectedSecret.permissions,
                        functionIds: e.target.value.split(',').map((id) => id.trim()),
                      },
                    })
                  }
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                  placeholder="func_1, func_2, ..."
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Contract Methods
                </label>
                <input
                  type="text"
                  value={selectedSecret.permissions.contractMethods?.join(', ') || ''}
                  onChange={(e) =>
                    setSelectedSecret({
                      ...selectedSecret,
                      permissions: {
                        ...selectedSecret.permissions,
                        contractMethods: e.target.value.split(',').map((method) => method.trim()),
                      },
                    })
                  }
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                  placeholder="transfer, mint, ..."
                />
              </div>
              <div className="flex justify-end space-x-4">
                <button
                  onClick={() => {
                    setShowPermissionsModal(false);
                    setSelectedSecret(null);
                  }}
                  className="px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 rounded-md"
                >
                  Cancel
                </button>
                <button
                  onClick={handleUpdatePermissions}
                  className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md"
                >
                  Update Permissions
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}