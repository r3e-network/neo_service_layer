// @ts-ignore
import * as React from 'react';
import { BoltIcon, BellAlertIcon, EyeIcon, ArrowPathIcon } from '@heroicons/react/24/outline';

export default function TriggersPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <div className="inline-flex items-center justify-center p-3 rounded-full bg-amber-100 dark:bg-amber-900 mb-6">
            <BoltIcon className="h-8 w-8 text-amber-600 dark:text-amber-400" />
          </div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Trigger Service
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Event monitoring and automated response system.
          </p>
        </div>
        
        <div className="mt-16 flow-root">
          <div className="mt-10 space-y-8 border-t border-gray-200 dark:border-gray-700 pt-10 sm:mt-16 sm:pt-16">
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Key Features</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <EyeIcon className="h-6 w-6 text-amber-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Custom Conditions</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Define complex trigger conditions based on multiple factors including on-chain data, 
                    time intervals, external data sources, or API responses.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <BoltIcon className="h-6 w-6 text-amber-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Chain Monitoring</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Monitor blockchain events in real-time, including transaction confirmations, 
                    smart contract events, address activity, and token transfers.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <BellAlertIcon className="h-6 w-6 text-amber-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Webhook Support</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Integrate with external systems via webhooks to receive notifications or 
                    trigger additional actions when specific events occur.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <ArrowPathIcon className="h-6 w-6 text-amber-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Action Automation</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Automatically execute predefined actions when triggers fire, from calling smart 
                    contract functions to initiating serverless function execution.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Example Use Cases</h2>
              
              <div className="space-y-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Token Price Alerts</h3>
                  <p className="text-gray-600 dark:text-gray-400 mb-4">
                    Set up triggers to monitor token prices and execute predefined actions when prices 
                    cross certain thresholds, such as rebalancing portfolios or liquidating positions.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Transaction Monitoring</h3>
                  <p className="text-gray-600 dark:text-gray-400 mb-4">
                    Monitor specific addresses or contracts for transactions and receive immediate 
                    notifications when activity is detected, allowing for real-time responses.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Multi-step Workflows</h3>
                  <p className="text-gray-600 dark:text-gray-400 mb-4">
                    Create complex multi-step processes that trigger different actions based on 
                    intermediate results, enabling sophisticated automation sequences.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Getting Started</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                <p className="text-gray-600 dark:text-gray-400 mb-4">
                  Setting up a trigger is straightforward:
                </p>
                <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
                  <pre className="text-sm overflow-x-auto">
                    <code className="language-javascript">
{`// Create a price-based trigger
const trigger = await neo.services.triggers.create({
  name: "NEO Price Alert",
  condition: {
    type: "price",
    asset: "NEO/USD",
    operator: ">",
    threshold: 50.00
  },
  action: {
    type: "function",
    functionId: "execute-strategy-123",
    parameters: {
      strategy: "take-profit",
      amount: "50%"
    }
  },
  notifications: ["email", "slack"],
  cooldownMinutes: 60
});

console.log(trigger);
// {
//   id: "trig-12345",
//   status: "active",
//   nextEvaluationTime: "2023-04-01T00:05:00Z"
// }`}
                    </code>
                  </pre>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
} 