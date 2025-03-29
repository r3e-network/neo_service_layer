'use client';

// @ts-ignore
import * as React from 'react';
import {
  CloudArrowUpIcon,
  DocumentCheckIcon,
  ExclamationCircleIcon,
  ArrowPathIcon,
} from '@heroicons/react/24/outline';
import { useWallet } from '../app/hooks/useWallet';

interface ContractDeployment {
  id: string;
  name: string;
  version: string;
  status: 'pending' | 'deployed' | 'failed' | 'upgrading';
  address?: string;
  deploymentDate?: string;
  network: string;
  bytecodeHash: string;
  constructor: {
    name: string;
    params: {
      name: string;
      type: string;
      value: string;
    }[];
  };
  verification: {
    status: 'pending' | 'verified' | 'failed';
    explorerUrl?: string;
    date?: string;
  };
  upgrades: {
    version: string;
    date: string;
    status: 'successful' | 'failed';
    reason?: string;
  }[];
  metrics: {
    gasUsed: number;
    transactionHash: string;
    blockNumber: number;
    deploymentCost: string;
  };
}

export function ContractDeployment() {
  const { isConnected, connect, signMessage } = useWallet();
  const [deployments, setDeployments] = React.useState<ContractDeployment[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [showNewDeploymentModal, setShowNewDeploymentModal] = React.useState(false);
  const [newDeployment, setNewDeployment] = React.useState({
    name: '',
    version: '',
    network: 'testnet',
    constructor: {
      name: '',
      params: [],
    },
  });

  React.useEffect(() => {
    if (isConnected) {
      fetchDeployments();
    }
  }, [isConnected]);

  const fetchDeployments = async () => {
    try {
      const signature = await signMessage('fetch-deployments');
      const response = await fetch('/.netlify/functions/contract-deployment', {
        headers: {
          'x-signature': signature,
        },
      });
      const data = await response.json();
      setDeployments(data);
    } catch (error) {
      console.error('Failed to fetch deployments:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateDeployment = async () => {
    try {
      const signature = await signMessage(JSON.stringify(newDeployment));
      const response = await fetch('/.netlify/functions/contract-deployment', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'x-signature': signature,
        },
        body: JSON.stringify(newDeployment),
      });
      const data = await response.json();
      setDeployments([...deployments, data]);
      setShowNewDeploymentModal(false);
      setNewDeployment({
        name: '',
        version: '',
        network: 'testnet',
        constructor: {
          name: '',
          params: [],
        },
      });
    } catch (error) {
      console.error('Failed to create deployment:', error);
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'deployed':
        return <DocumentCheckIcon className="h-5 w-5 text-green-500" />;
      case 'failed':
        return <ExclamationCircleIcon className="h-5 w-5 text-red-500" />;
      case 'upgrading':
        return <ArrowPathIcon className="h-5 w-5 text-yellow-500" />;
      default:
        return <CloudArrowUpIcon className="h-5 w-5 text-blue-500" />;
    }
  };

  if (!isConnected) {
    return (
      <div className="text-center py-12">
        <h3 className="text-lg font-medium text-gray-900">Connect Wallet</h3>
        <p className="mt-2 text-sm text-gray-500">
          Please connect your wallet to manage contract deployments.
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
        <h2 className="text-2xl font-bold text-gray-900">Contract Deployments</h2>
        <button
          onClick={() => setShowNewDeploymentModal(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <CloudArrowUpIcon className="h-5 w-5 mr-2" />
          New Deployment
        </button>
      </div>

      {loading ? (
        <div className="space-y-4">
          {[...Array(3)].map((_, i) => (
            <div
              key={i}
              className="animate-pulse bg-gray-100 rounded-lg h-32"
            />
          ))}
        </div>
      ) : (
        <div className="space-y-6">
          {deployments.map((deployment) => (
            <div
              key={deployment.id}
              className="bg-white rounded-lg shadow p-6"
            >
              <div className="flex items-center justify-between">
                <div className="flex items-center">
                  {getStatusIcon(deployment.status)}
                  <div className="ml-4">
                    <h3 className="text-lg font-medium text-gray-900">
                      {deployment.name}
                    </h3>
                    <p className="text-sm text-gray-500">
                      Version {deployment.version} | {deployment.network}
                    </p>
                  </div>
                </div>
                <div className="text-right">
                  <p className="text-sm text-gray-500">
                    {deployment.deploymentDate
                      ? new Date(deployment.deploymentDate).toLocaleDateString()
                      : 'Pending'}
                  </p>
                  {deployment.address && (
                    <p className="mt-1 text-sm font-mono text-gray-600">
                      {deployment.address}
                    </p>
                  )}
                </div>
              </div>

              <div className="mt-6 grid grid-cols-2 gap-6">
                <div>
                  <h4 className="text-sm font-medium text-gray-900">
                    Verification Status
                  </h4>
                  <div className="mt-2 flex items-center">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        deployment.verification.status === 'verified'
                          ? 'bg-green-100 text-green-800'
                          : deployment.verification.status === 'pending'
                          ? 'bg-yellow-100 text-yellow-800'
                          : 'bg-red-100 text-red-800'
                      }`}
                    >
                      {deployment.verification.status}
                    </span>
                    {deployment.verification.explorerUrl && (
                      <a
                        href={deployment.verification.explorerUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="ml-2 text-sm text-indigo-600 hover:text-indigo-500"
                      >
                        View on Explorer
                      </a>
                    )}
                  </div>
                </div>

                <div>
                  <h4 className="text-sm font-medium text-gray-900">Metrics</h4>
                  <div className="mt-2 text-sm text-gray-500">
                    <p>Gas Used: {deployment.metrics.gasUsed}</p>
                    <p>Cost: {deployment.metrics.deploymentCost}</p>
                  </div>
                </div>
              </div>

              {deployment.upgrades.length > 0 && (
                <div className="mt-6">
                  <h4 className="text-sm font-medium text-gray-900">
                    Upgrade History
                  </h4>
                  <div className="mt-2 space-y-2">
                    {deployment.upgrades.map((upgrade, index) => (
                      <div
                        key={index}
                        className="flex items-center justify-between text-sm"
                      >
                        <span className="text-gray-500">
                          Version {upgrade.version}
                        </span>
                        <span
                          className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                            upgrade.status === 'successful'
                              ? 'bg-green-100 text-green-800'
                              : 'bg-red-100 text-red-800'
                          }`}
                        >
                          {upgrade.status}
                        </span>
                        <span className="text-gray-500">
                          {new Date(upgrade.date).toLocaleDateString()}
                        </span>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* New Deployment Modal */}
      {showNewDeploymentModal && (
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center">
          <div className="bg-white rounded-lg p-6 max-w-lg w-full">
            <h3 className="text-lg font-medium text-gray-900">
              Deploy New Contract
            </h3>
            <div className="mt-4 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Contract Name
                </label>
                <input
                  type="text"
                  value={newDeployment.name}
                  onChange={(e) =>
                    setNewDeployment({ ...newDeployment, name: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Version
                </label>
                <input
                  type="text"
                  value={newDeployment.version}
                  onChange={(e) =>
                    setNewDeployment({ ...newDeployment, version: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Network
                </label>
                <select
                  value={newDeployment.network}
                  onChange={(e) =>
                    setNewDeployment({ ...newDeployment, network: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                >
                  <option value="testnet">Testnet</option>
                  <option value="mainnet">Mainnet</option>
                </select>
              </div>
              <div className="flex justify-end space-x-4">
                <button
                  onClick={() => setShowNewDeploymentModal(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 rounded-md"
                >
                  Cancel
                </button>
                <button
                  onClick={handleCreateDeployment}
                  className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md"
                >
                  Deploy Contract
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}