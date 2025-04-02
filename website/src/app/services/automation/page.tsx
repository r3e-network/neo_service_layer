// @ts-ignore
import * as React from 'react';
import { CogIcon, ClockIcon, BoltIcon, ChartBarIcon } from '@heroicons/react/24/outline';

export default function AutomationPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <div className="inline-flex items-center justify-center p-3 rounded-full bg-purple-100 dark:bg-purple-900 mb-6">
            <CogIcon className="h-8 w-8 text-purple-600 dark:text-purple-400" />
          </div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Contract Automation Service
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Automated smart contract execution with comprehensive monitoring.
          </p>
        </div>
        
        <div className="mt-16 flow-root">
          <div className="mt-10 space-y-8 border-t border-gray-200 dark:border-gray-700 pt-10 sm:mt-16 sm:pt-16">
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Key Features</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <ClockIcon className="h-6 w-6 text-purple-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Scheduled Execution</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Set up recurring contract calls on customizable schedules - hourly, daily, weekly, or with cron expressions.
                    Perfect for regular operations like interest payments or token distributions.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <BoltIcon className="h-6 w-6 text-purple-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Event-based Triggers</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Configure automation to trigger based on blockchain events, price movements, or API calls.
                    React immediately to market conditions or user activities.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <CogIcon className="h-6 w-6 text-purple-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Error Handling</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Sophisticated retry mechanisms and fallback options ensure your automation is resilient.
                    Configurable alerting keeps you informed of any issues requiring attention.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <ChartBarIcon className="h-6 w-6 text-purple-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Performance Monitoring</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Detailed dashboards show execution history, gas usage, and success rates.
                    Track performance over time and optimize your automated processes.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Use Cases</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 space-y-6">
                <div>
                  <h3 className="text-xl font-semibold mb-2 text-gray-900 dark:text-white">DeFi Protocols</h3>
                  <ul className="list-disc list-inside space-y-2 text-gray-600 dark:text-gray-400">
                    <li>Automate interest calculations and distributions</li>
                    <li>Execute regular rebalancing of token portfolios</li>
                    <li>Trigger liquidations based on collateral ratios</li>
                    <li>Update price oracles at specified intervals</li>
                  </ul>
                </div>
                
                <div>
                  <h3 className="text-xl font-semibold mb-2 text-gray-900 dark:text-white">NFT Projects</h3>
                  <ul className="list-disc list-inside space-y-2 text-gray-600 dark:text-gray-400">
                    <li>Schedule drops and reveals</li>
                    <li>Automate royalty distributions to creators</li>
                    <li>Trigger state changes based on external events</li>
                  </ul>
                </div>
                
                <div>
                  <h3 className="text-xl font-semibold mb-2 text-gray-900 dark:text-white">DAOs & Governance</h3>
                  <ul className="list-disc list-inside space-y-2 text-gray-600 dark:text-gray-400">
                    <li>Execute approved proposals automatically</li>
                    <li>Schedule regular treasury operations</li>
                    <li>Automate voting cycles and governance events</li>
                  </ul>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Getting Started</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                <p className="text-gray-600 dark:text-gray-400 mb-4">
                  Configure an automated contract call:
                </p>
                <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
                  <pre className="text-sm overflow-x-auto">
                    <code className="language-javascript">
{`// Create a scheduled contract automation
const automation = await neo.services.automation.createTask({
  name: "Daily Reward Distribution",
  contractHash: "0xabcdef1234567890abcdef1234567890abcdef12",
  method: "distributeRewards",
  parameters: [],
  schedule: {
    type: "cron",
    expression: "0 0 * * *" // Daily at midnight
  },
  retryConfig: {
    maxRetries: 3,
    backoffMinutes: 5
  },
  notifications: {
    success: ["email@example.com"],
    failure: ["email@example.com", "sms:+1234567890"]
  }
});

console.log(automation);
// {
//   id: "auto-12345",
//   status: "active",
//   nextExecutionTime: "2023-04-01T00:00:00Z"
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