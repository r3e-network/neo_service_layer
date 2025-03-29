'use client';

// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const installCode = `npm install @neo-service-layer/core
# or
yarn add @neo-service-layer/core`;

const initCode = `import { Client, PriceFeed } from '@neo-service-layer/core';

// Initialize client with your Neo N3 wallet
const client = new Client({
  signMessage: async (message) => {
    return await window.neo3Wallet.signMessage(message);
  },
});

// Create PriceFeed instance
const priceFeed = new PriceFeed(client);`;

const getDataCode = `// Get current price
const neoPrice = await priceFeed.getPrice('NEO/USD');
console.log('Current NEO price:', neoPrice);

// Get historical data
const history = await priceFeed.getHistory({
  symbol: 'NEO/USD',
  from: new Date('2024-01-01'),
  to: new Date(),
  interval: '1h',
});

// Subscribe to price updates
const unsubscribe = await priceFeed.subscribe({
  symbol: 'NEO/USD',
  interval: 60, // Update every 60 seconds
  onUpdate: (price) => {
    console.log('New price update:', price);
  },
});

// Later, unsubscribe when needed
unsubscribe();`;

const contractCode = `// Example smart contract that uses price feed
const contractCode = \`
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System;
using System.ComponentModel;

namespace PriceFeedConsumer
{
    [DisplayName("PriceFeedConsumer")]
    [ManifestExtra("Author", "Neo")]
    [ManifestExtra("Email", "dev@neo.org")]
    [ManifestExtra("Description", "Price Feed Consumer Contract")]
    public class PriceFeedConsumer : SmartContract
    {
        // Event to notify when price is updated
        [DisplayName("PriceUpdated")]
        public static event Action<string, BigInteger> PriceUpdated;

        private static readonly StorageMap Store = new StorageMap(Storage.CurrentContext, "PriceFeedConsumer");

        // Method to update price, called by automation service
        public static void UpdatePrice(string symbol, BigInteger price)
        {
            // Verify caller is authorized (e.g., automation service)
            if (!Runtime.CheckWitness(GetAutomationAddress()))
                throw new Exception("Unauthorized");

            // Store the new price
            Store.Put(symbol, price);

            // Notify about the update
            PriceUpdated(symbol, price);
        }

        // Method to get the latest stored price
        public static BigInteger GetPrice(string symbol)
        {
            return (BigInteger)Store.Get(symbol);
        }

        // Helper to get automation service address
        private static UInt160 GetAutomationAddress()
        {
            return "NXV7J4vhN3Xth4wJEwK1Zb6YgbxboGP3gt".ToScriptHash();
        }
    }
}\``;

const automationCode = `// Set up automation to update contract with latest price
const automation = new Automation(client);

await automation.createTask({
  name: 'NEO Price Update',
  contract: 'YOUR_CONTRACT_HASH',
  method: 'UpdatePrice',
  schedule: '*/5 * * * *', // Every 5 minutes
  params: ['NEO/USD'],
});`;

export default function PriceFeedPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl lg:max-w-4xl">
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 dark:text-white">
            Price Feed Service
          </h1>
          
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            The Price Feed service provides real-time and historical price data for various assets on the Neo N3 blockchain. 
            Built with security and reliability in mind, it sources data from multiple trusted providers and validates it in a TEE before publishing.
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
                Initialize the client and create a PriceFeed instance:
              </p>
              <div className="mt-4">
                <CodeBlock code={initCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Getting Price Data
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                You can fetch current prices, historical data, and subscribe to real-time updates:
              </p>
              <div className="mt-4">
                <CodeBlock code={getDataCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Smart Contract Integration
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Example smart contract that consumes price feed data:
              </p>
              <div className="mt-4">
                <CodeBlock code={contractCode} language="csharp" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Automation Integration
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Automate price updates in your smart contract using the Automation service:
              </p>
              <div className="mt-4">
                <CodeBlock code={automationCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Available Price Pairs
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
                {[
                  { pair: 'NEO/USD', updateInterval: '30 seconds', source: 'Aggregated' },
                  { pair: 'GAS/USD', updateInterval: '30 seconds', source: 'Aggregated' },
                  { pair: 'BTC/USD', updateInterval: '30 seconds', source: 'Aggregated' },
                  { pair: 'ETH/USD', updateInterval: '30 seconds', source: 'Aggregated' },
                  { pair: 'NEO/BTC', updateInterval: '30 seconds', source: 'Aggregated' },
                  { pair: 'GAS/BTC', updateInterval: '30 seconds', source: 'Aggregated' },
                ].map((pair) => (
                  <div
                    key={pair.pair}
                    className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5"
                  >
                    <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                      {pair.pair}
                    </h3>
                    <dl className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                      <div className="flex justify-between">
                        <dt>Update Interval:</dt>
                        <dd>{pair.updateInterval}</dd>
                      </div>
                      <div className="flex justify-between">
                        <dt>Source:</dt>
                        <dd>{pair.source}</dd>
                      </div>
                    </dl>
                  </div>
                ))}
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Security & Reliability
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    TEE Protection
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    All price data is processed and validated within a Trusted Execution Environment, 
                    ensuring data integrity and preventing manipulation.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Data Aggregation
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Prices are aggregated from multiple trusted sources with outlier detection 
                    and validation to ensure accuracy and reliability.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Heartbeat Verification
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Regular heartbeat checks ensure data freshness. Stale prices are automatically 
                    detected and updated.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Deviation Thresholds
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Price updates are triggered when deviation thresholds are met, ensuring 
                    timely updates during high volatility.
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