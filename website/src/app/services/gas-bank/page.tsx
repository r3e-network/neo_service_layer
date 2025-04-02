// @ts-ignore
import * as React from 'react';
import { ChartBarIcon, ShieldCheckIcon } from '@heroicons/react/24/outline';

export default function GasBankPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <div className="inline-flex items-center justify-center p-3 rounded-full bg-blue-100 dark:bg-blue-900 mb-6">
            <ChartBarIcon className="h-8 w-8 text-blue-600 dark:text-blue-400" />
          </div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Gas Bank Service
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Efficient gas management system for automated contract operations.
          </p>
        </div>
        
        <div className="mt-16 flow-root">
          <div className="mt-10 space-y-8 border-t border-gray-200 dark:border-gray-700 pt-10 sm:mt-16 sm:pt-16">
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Key Features</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Automated Refills</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    Our Gas Bank automatically refills contract GAS balances when they fall below specified thresholds.
                    This ensures your contracts never run out of gas during critical operations.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Usage Tracking</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    We provide detailed analytics on gas usage across your contracts and transactions.
                    This allows you to identify optimization opportunities and predict future gas needs.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Cost Optimization</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    Our intelligent gas management algorithms optimize when to submit transactions based on network conditions.
                    This helps you save on gas costs while maintaining reliable contract operations.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Multi-wallet Support</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    Gas Bank can manage multiple funding wallets with different security policies.
                    This provides flexibility in how you fund and authorize gas payments for your contracts.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">How It Works</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Setup Process</h3>
                <ol className="list-decimal list-inside space-y-4 text-gray-600 dark:text-gray-400">
                  <li><strong>Register your contracts:</strong> Add the smart contracts you want to manage to the Gas Bank service.</li>
                  <li><strong>Configure parameters:</strong> Set minimum GAS thresholds, maximum top-up amounts, and funding sources.</li>
                  <li><strong>Choose monitoring frequency:</strong> Decide how often the Gas Bank should check your contract's GAS levels.</li>
                  <li><strong>Set up alerts:</strong> Configure notifications for low balances or when automatic top-ups occur.</li>
                </ol>
                
                <div className="mt-6 p-4 bg-blue-50 dark:bg-blue-900/30 rounded-lg">
                  <p className="text-sm text-blue-700 dark:text-blue-300">
                    <strong>Note:</strong> Gas Bank requires a deposit of GAS to fund your managed contracts. You can withdraw your
                    unused GAS at any time.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Integration Example</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                <p className="text-gray-600 dark:text-gray-400 mb-4">
                  Here's how to integrate the Gas Bank service with your application:
                </p>
                <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
                  <pre className="text-sm overflow-x-auto">
                    <code className="language-javascript">
{`// Register a contract with Gas Bank
const response = await neo.services.gasBank.registerContract({
  contractHash: '0x1234567890abcdef1234567890abcdef12345678',
  minGasThreshold: 10,
  maxTopUp: 50,
  monitoringInterval: '1h',
  alertEmail: 'alerts@example.com'
});

console.log(response);
// {
//   status: "success",
//   contractId: "gb-1234",
//   nextCheckTime: "2023-04-01T00:00:00Z"
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