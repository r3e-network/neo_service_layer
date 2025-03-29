'use client';

// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const installCode = `npm install @neo-service-layer/core
# or
yarn add @neo-service-layer/core`;

const initCode = `import { Client, GasBank } from '@neo-service-layer/core';

// Initialize client with your Neo N3 wallet
const client = new Client({
  signMessage: async (message) => {
    return await window.neo3Wallet.signMessage(message);
  },
});

// Create GasBank instance
const gasBank = new GasBank(client);`;

const autoFundingCode = `// Configure auto-funding for your contract
await gasBank.configureAutoFunding({
  contract: 'YOUR_CONTRACT_HASH',
  threshold: 100, // GAS amount to trigger refill
  targetBalance: 500, // Target GAS balance after refill
  maxGasPerTransaction: 10, // Maximum GAS per transaction
  alertThreshold: 50, // Alert when balance below this
});

// Get auto-funding configuration
const config = await gasBank.getAutoFundingConfig('YOUR_CONTRACT_HASH');

// Update configuration
await gasBank.updateAutoFundingConfig('YOUR_CONTRACT_HASH', {
  threshold: 200,
  targetBalance: 1000,
});`;

const balanceCode = `// Get contract GAS balance
const balance = await gasBank.getBalance('YOUR_CONTRACT_HASH');

// Top up contract GAS balance
await gasBank.topUp('YOUR_CONTRACT_HASH', '100');

// Withdraw GAS from contract
await gasBank.withdraw('YOUR_CONTRACT_HASH', '50');

// Get transaction history
const history = await gasBank.getTransactionHistory('YOUR_CONTRACT_HASH', {
  from: new Date('2024-01-01'),
  to: new Date(),
  type: 'all', // 'topup', 'withdraw', or 'all'
});`;

const contractCode = `using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System;
using System.ComponentModel;

namespace GasBankConsumer
{
    [DisplayName("GasBankConsumer")]
    [ManifestExtra("Author", "Neo")]
    [ManifestExtra("Description", "Contract using Gas Bank")]
    public class GasBankConsumer : SmartContract
    {
        // Event to notify when gas is received
        [DisplayName("GasReceived")]
        public static event Action<UInt160, BigInteger> GasReceived;

        private static readonly StorageMap Store = new StorageMap(Storage.CurrentContext, "GasBankConsumer");

        // Method to receive GAS from Gas Bank
        public static void OnGasReceived(UInt160 from, BigInteger amount)
        {
            // Verify caller is Gas Bank
            if (!Runtime.CheckWitness(GetGasBankAddress()))
                throw new Exception("Unauthorized");

            // Update balance
            var currentBalance = (BigInteger)Store.Get("gasBalance");
            Store.Put("gasBalance", currentBalance + amount);

            // Notify about received gas
            GasReceived(from, amount);
        }

        // Method to get current GAS balance
        public static BigInteger GetGasBalance()
        {
            return (BigInteger)Store.Get("gasBalance");
        }

        // Helper to get Gas Bank address
        private static UInt160 GetGasBankAddress()
        {
            return "NXV7J4vhN3Xth4wJEwK1Zb6YgbxboGP3gt".ToScriptHash();
        }
    }
}`;

export default function GasBankPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl lg:max-w-4xl">
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 dark:text-white">
            Gas Bank Service
          </h1>
          
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            The Gas Bank service provides automated gas management for your smart contracts. 
            It ensures your contracts always have sufficient GAS for operations while optimizing costs and preventing out-of-gas errors.
          </p>

          <div className="mt-16 space-y-20">
            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Installation
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Install the Neo Service Layer SDK using npm or yarn:
              </p>
              <div className="mt-4">
                <CodeBlock code={installCode} language="bash" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Initialization
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Initialize the client and create a GasBank instance:
              </p>
              <div className="mt-4">
                <CodeBlock code={initCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Auto-Funding Configuration
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Set up and manage automatic gas funding for your contracts:
              </p>
              <div className="mt-4">
                <CodeBlock code={autoFundingCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Balance Management
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Monitor and manage your contract's GAS balance:
              </p>
              <div className="mt-4">
                <CodeBlock code={balanceCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Smart Contract Example
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Example smart contract that works with Gas Bank:
              </p>
              <div className="mt-4">
                <CodeBlock code={contractCode} language="csharp" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Features
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Auto-Funding
                  </h3>
                  <ul className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <li>• Configurable balance thresholds</li>
                    <li>• Automatic refills when low</li>
                    <li>• Custom target balance</li>
                    <li>• Transaction amount limits</li>
                  </ul>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Cost Optimization
                  </h3>
                  <ul className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <li>• Dynamic gas fee calculation</li>
                    <li>• Batch transactions</li>
                    <li>• Fee estimation</li>
                    <li>• Usage analytics</li>
                  </ul>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Monitoring
                  </h3>
                  <ul className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <li>• Real-time balance tracking</li>
                    <li>• Usage patterns analysis</li>
                    <li>• Alert notifications</li>
                    <li>• Transaction history</li>
                  </ul>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Security
                  </h3>
                  <ul className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <li>• TEE-protected operations</li>
                    <li>• Signature verification</li>
                    <li>• Transaction limits</li>
                    <li>• Audit logging</li>
                  </ul>
                </div>
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Best Practices
              </h2>
              <div className="mt-6 space-y-6">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Setting Thresholds
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Set your threshold to at least 2-3x your average transaction gas cost to ensure 
                    sufficient buffer for multiple transactions. Consider peak usage patterns when 
                    setting target balance.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Monitoring and Alerts
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Configure alerts at 2x your threshold to have time to react to potential issues. 
                    Regularly review transaction history and usage patterns to optimize your configuration.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Contract Integration
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Always implement proper authorization checks in your contract when receiving gas. 
                    Keep track of received gas and implement safety measures for unexpected scenarios.
                  </p>
                </div>
              </div>
            </section>
          </div>
        </div>
      </div>
    </div>
  );
} 