'use client';

// @ts-ignore
import * as React from 'react';
import { BellIcon, BellSlashIcon, PlusIcon } from '@heroicons/react/24/outline';
import { useWallet } from '../app/hooks/useWallet';

interface ContractEvent {
  id: string;
  contract: string;
  eventName: string;
  timestamp: string;
  blockNumber: number;
  transactionHash: string;
  parameters: {
    name: string;
    type: string;
    value: string;
  }[];
  status: 'processed' | 'pending' | 'failed';
}

interface EventSubscription {
  id: string;
  contract: string;
  eventName: string;
  status: 'active' | 'paused';
  filters: {
    parameter: string;
    operator: 'eq' | 'gt' | 'lt' | 'contains';
    value: string;
  }[];
  notifications: {
    type: 'webhook' | 'email';
    destination: string;
  }[];
}

export function ContractEvents() {
  const { isConnected, connect, signMessage } = useWallet();
  const [events, setEvents] = React.useState<ContractEvent[]>([]);
  const [subscriptions, setSubscriptions] = React.useState<EventSubscription[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [showNewSubscriptionModal, setShowNewSubscriptionModal] = React.useState(false);
  const [newSubscription, setNewSubscription] = React.useState({
    contract: '',
    eventName: '',
    filters: [],
    notifications: [],
  });

  React.useEffect(() => {
    if (isConnected) {
      fetchEvents();
      fetchSubscriptions();
    }
  }, [isConnected]);

  const fetchEvents = async () => {
    try {
      const signature = await signMessage('fetch-events');
      const response = await fetch('/.netlify/functions/contract-events/events', {
        headers: {
          'x-signature': signature,
        },
      });
      const data = await response.json();
      setEvents(data);
    } catch (error) {
      console.error('Failed to fetch events:', error);
    }
  };

  const fetchSubscriptions = async () => {
    try {
      const signature = await signMessage('fetch-subscriptions');
      const response = await fetch('/.netlify/functions/contract-events/subscriptions', {
        headers: {
          'x-signature': signature,
        },
      });
      const data = await response.json();
      setSubscriptions(data);
    } catch (error) {
      console.error('Failed to fetch subscriptions:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateSubscription = async () => {
    try {
      const signature = await signMessage(JSON.stringify(newSubscription));
      const response = await fetch('/.netlify/functions/contract-events/subscriptions', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'x-signature': signature,
        },
        body: JSON.stringify(newSubscription),
      });
      const data = await response.json();
      setSubscriptions([...subscriptions, data]);
      setShowNewSubscriptionModal(false);
      setNewSubscription({
        contract: '',
        eventName: '',
        filters: [],
        notifications: [],
      });
    } catch (error) {
      console.error('Failed to create subscription:', error);
    }
  };

  const handleToggleSubscription = async (subId: string, currentStatus: string) => {
    try {
      const signature = await signMessage('toggle-subscription-' + subId);
      const response = await fetch('/.netlify/functions/contract-events/subscriptions/' + subId, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'X-Signature': signature,
        },
        body: JSON.stringify({
          status: currentStatus === 'active' ? 'paused' : 'active',
        }),
      });

      if (!response.ok) {
        throw new Error('Failed to toggle subscription status: ' + response.statusText);
      }

      // Update subscription status in UI
      setSubscriptions(subscriptions.map(sub => 
        sub.id === subId
          ? { ...sub, status: sub.status === 'active' ? 'paused' : 'active' }
          : sub
      ));
    } catch (error) {
      console.error('Error toggling subscription status:', error);
      // Show error notification
    }
  };

  if (!isConnected) {
    return (
      <div className="text-center py-12">
        <h3 className="text-lg font-medium text-gray-900">Connect Wallet</h3>
        <p className="mt-2 text-sm text-gray-500">
          Please connect your wallet to view contract events.
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
        <h2 className="text-2xl font-bold text-gray-900">Contract Events</h2>
        <button
          onClick={() => setShowNewSubscriptionModal(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="h-5 w-5 mr-2" />
          New Subscription
        </button>
      </div>

      {/* Event Subscriptions */}
      <div>
        <h3 className="text-lg font-medium text-gray-900">Active Subscriptions</h3>
        <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
          {subscriptions.map((sub) => (
            <div
              key={sub.id}
              className="bg-white rounded-lg shadow p-6"
            >
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium text-gray-900">{sub.eventName}</h4>
                  <p className="mt-1 text-sm text-gray-500">{sub.contract}</p>
                </div>
                <button
                  onClick={() => handleToggleSubscription(sub.id, sub.status)}
                  className={`p-2 rounded-md ${
                    sub.status === 'active'
                      ? 'text-green-600 hover:bg-green-50'
                      : 'text-gray-400 hover:bg-gray-50'
                  }`}
                >
                  {sub.status === 'active' ? (
                    <BellIcon className="h-5 w-5" />
                  ) : (
                    <BellSlashIcon className="h-5 w-5" />
                  )}
                </button>
              </div>
              {sub.filters.length > 0 && (
                <div className="mt-4">
                  <p className="text-sm font-medium text-gray-700">Filters:</p>
                  <div className="mt-2 space-y-2">
                    {sub.filters.map((filter, index) => (
                      <p key={index} className="text-sm text-gray-500">
                        {filter.parameter} {filter.operator} {filter.value}
                      </p>
                    ))}
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>

      {/* Recent Events */}
      <div>
        <h3 className="text-lg font-medium text-gray-900">Recent Events</h3>
        <div className="mt-4 space-y-4">
          {events.map((event) => (
            <div
              key={event.id}
              className="bg-white rounded-lg shadow p-6"
            >
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium text-gray-900">{event.eventName}</h4>
                  <p className="mt-1 text-sm text-gray-500">
                    Block: {event.blockNumber} | Tx: {event.transactionHash}
                  </p>
                </div>
                <span
                  className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                    event.status === 'processed'
                      ? 'bg-green-100 text-green-800'
                      : event.status === 'pending'
                      ? 'bg-yellow-100 text-yellow-800'
                      : 'bg-red-100 text-red-800'
                  }`}
                >
                  {event.status}
                </span>
              </div>
              <div className="mt-4">
                <p className="text-sm font-medium text-gray-700">Parameters:</p>
                <div className="mt-2 grid grid-cols-2 gap-4">
                  {event.parameters.map((param, index) => (
                    <div key={index} className="text-sm">
                      <p className="text-gray-500">{param.name}</p>
                      <p className="font-medium text-gray-900">{param.value}</p>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* New Subscription Modal */}
      {showNewSubscriptionModal && (
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center">
          <div className="bg-white rounded-lg p-6 max-w-lg w-full">
            <h3 className="text-lg font-medium text-gray-900">
              Create Event Subscription
            </h3>
            <div className="mt-4 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Contract Address
                </label>
                <input
                  type="text"
                  value={newSubscription.contract}
                  onChange={(e) =>
                    setNewSubscription({ ...newSubscription, contract: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Event Name
                </label>
                <input
                  type="text"
                  value={newSubscription.eventName}
                  onChange={(e) =>
                    setNewSubscription({ ...newSubscription, eventName: e.target.value })
                  }
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div className="flex justify-end space-x-4">
                <button
                  onClick={() => setShowNewSubscriptionModal(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 rounded-md"
                >
                  Cancel
                </button>
                <button
                  onClick={handleCreateSubscription}
                  className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md"
                >
                  Create Subscription
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}