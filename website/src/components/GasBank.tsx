'use client';

// @ts-ignore
import * as React from 'react';
import {
  ArrowTrendingUpIcon,
  BanknotesIcon,
  ClockIcon,
  CogIcon,
  ArrowPathIcon,
} from '@heroicons/react/24/outline';
import { useWallet } from '../app/hooks/useWallet';

interface GasBankStatus {
  balance: string;
  transactions: {
    daily: number;
    weekly: number;
    monthly: number;
  };
  efficiency: {
    savings: number;
    optimizationRate: number;
  };
  limits: {
    daily: string;
    transaction: string;
  };
}

interface GasTransaction {
  id: string;
  timestamp: string;
  amount: string;
  type: 'deposit' | 'withdrawal' | 'refund';
  status: 'completed' | 'pending' | 'failed';
  hash: string;
  contract?: string;
  optimized: boolean;
  savedAmount?: string;
}

export function GasBank() {
  const { isConnected, connect, signMessage } = useWallet();
  const [status, setStatus] = React.useState<GasBankStatus | null>(null);
  const [transactions, setTransactions] = React.useState<GasTransaction[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [showDepositModal, setShowDepositModal] = React.useState(false);
  const [depositAmount, setDepositAmount] = React.useState('');
  const [refreshKey, setRefreshKey] = React.useState(0);

  React.useEffect(() => {
    if (isConnected) {
      fetchGasBankStatus();
      fetchTransactions();
    }
  }, [isConnected, refreshKey]);

  const fetchGasBankStatus = async () => {
    try {
      const signature = await signMessage('fetch-gas-bank-status');
      const response = await fetch('/.netlify/functions/gas-bank-status', {
        headers: {
          'x-signature': signature,
        },
      });
      const data = await response.json();
      setStatus(data);
    } catch (error) {
      console.error('Failed to fetch gas bank status:', error);
    }
  };

  const fetchTransactions = async () => {
    try {
      const signature = await signMessage('fetch-transactions');
      // Mock transactions data for now
      const mockTransactions: GasTransaction[] = [
        {
          id: 'tx1',
          timestamp: new Date().toISOString(),
          amount: '10.0000 GAS',
          type: 'deposit',
          status: 'completed',
          hash: '0x1234...5678',
          optimized: true,
          savedAmount: '0.5000 GAS',
        },
        {
          id: 'tx2',
          timestamp: new Date(Date.now() - 3600000).toISOString(),
          amount: '5.0000 GAS',
          type: 'withdrawal',
          status: 'completed',
          hash: '0x8765...4321',
          contract: '0xabcd...ef12',
          optimized: true,
          savedAmount: '0.2500 GAS',
        },
      ];
      setTransactions(mockTransactions);
    } catch (error) {
      console.error('Failed to fetch transactions:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDeposit = async () => {
    try {
      const signature = await signMessage(`deposit-${depositAmount}`);
      // In production, this would call the actual deposit function
      await new Promise(resolve => setTimeout(resolve, 1000));
      setShowDepositModal(false);
      setDepositAmount('');
      setRefreshKey(prev => prev + 1);
    } catch (error) {
      console.error('Failed to deposit:', error);
    }
  };

  if (!isConnected) {
    return (
      <div className="text-center py-12">
        <h3 className="text-lg font-medium text-gray-900">Connect Wallet</h3>
        <p className="mt-2 text-sm text-gray-500">
          Please connect your wallet to access the Gas Bank.
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
        <h2 className="text-2xl font-bold text-gray-900">Gas Bank</h2>
        <button
          onClick={() => setShowDepositModal(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <BanknotesIcon className="h-5 w-5 mr-2" />
          Deposit GAS
        </button>
      </div>

      {/* Status Cards */}
      {status && (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <BanknotesIcon className="h-8 w-8 text-indigo-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Balance</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {status.balance}
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <ArrowTrendingUpIcon className="h-8 w-8 text-green-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Gas Savings</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {status.efficiency.savings}%
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <ClockIcon className="h-8 w-8 text-blue-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Daily Limit</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {status.limits.daily}
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <CogIcon className="h-8 w-8 text-purple-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Optimization Rate</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {status.efficiency.optimizationRate}%
                </p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Transaction History */}
      <div>
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-medium text-gray-900">Transaction History</h3>
          <button
            onClick={() => setRefreshKey(prev => prev + 1)}
            className="inline-flex items-center px-3 py-1 text-sm text-gray-600 hover:text-gray-900"
          >
            <ArrowPathIcon className="h-4 w-4 mr-1" />
            Refresh
          </button>
        </div>
        <div className="mt-4 bg-white shadow rounded-lg overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Amount
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Optimization
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Time
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {transactions.map((tx) => (
                <tr key={tx.id}>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        tx.type === 'deposit'
                          ? 'bg-green-100 text-green-800'
                          : tx.type === 'withdrawal'
                          ? 'bg-blue-100 text-blue-800'
                          : 'bg-yellow-100 text-yellow-800'
                      }`}
                    >
                      {tx.type}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {tx.amount}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        tx.status === 'completed'
                          ? 'bg-green-100 text-green-800'
                          : tx.status === 'pending'
                          ? 'bg-yellow-100 text-yellow-800'
                          : 'bg-red-100 text-red-800'
                      }`}
                    >
                      {tx.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {tx.optimized && (
                      <span className="text-green-600">
                        Saved {tx.savedAmount}
                      </span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {new Date(tx.timestamp).toLocaleString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Deposit Modal */}
      {showDepositModal && (
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-lg font-medium text-gray-900">Deposit GAS</h3>
            <div className="mt-4">
              <label className="block text-sm font-medium text-gray-700">
                Amount (GAS)
              </label>
              <div className="mt-1">
                <input
                  type="number"
                  min="0"
                  step="0.0001"
                  value={depositAmount}
                  onChange={(e) => setDepositAmount(e.target.value)}
                  className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                  placeholder="0.0000"
                />
              </div>
              <p className="mt-2 text-sm text-gray-500">
                Available balance: {status?.balance}
              </p>
            </div>
            <div className="mt-6 flex justify-end space-x-4">
              <button
                onClick={() => setShowDepositModal(false)}
                className="px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 rounded-md"
              >
                Cancel
              </button>
              <button
                onClick={handleDeposit}
                disabled={!depositAmount || parseFloat(depositAmount) <= 0}
                className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md disabled:opacity-50"
              >
                Confirm Deposit
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}