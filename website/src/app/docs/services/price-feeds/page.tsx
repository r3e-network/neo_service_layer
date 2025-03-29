// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const initCode = `import { PriceFeed } from '@neo-service-layer/core';

const priceFeed = new PriceFeed(client);

// Get current price
const neoPrice = await priceFeed.getPrice('NEO/USD');
console.log('Current NEO/USD price:', neoPrice);

// Subscribe to updates
priceFeed.subscribe('NEO/USD', (price) => {
  console.log('Price updated:', price);
});`;

const contractCode = `using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System;
using System.ComponentModel;

namespace NeoServiceLayer
{
    [DisplayName("PriceFeedAggregator")]
    [ManifestExtra("Author", "Neo Service Layer")]
    public class PriceFeedAggregator : SmartContract
    {
        // Events
        [DisplayName("PriceUpdated")]
        public static event Action<string, BigInteger, BigInteger> OnPriceUpdated;

        // Storage keys
        private static readonly byte[] PricePrefix = "price_".ToByteArray();
        private static readonly byte[] TimestampPrefix = "timestamp_".ToByteArray();
        private static readonly byte[] HeartbeatPrefix = "heartbeat_".ToByteArray();

        // Update price with timestamp and heartbeat
        public static void UpdatePrice(string symbol, BigInteger price, BigInteger timestamp)
        {
            // Verify caller
            if (!Runtime.CheckWitness(GetOwner()))
                throw new Exception("Unauthorized");

            // Get storage keys
            byte[] priceKey = ConcatKey(PricePrefix, symbol);
            byte[] timestampKey = ConcatKey(TimestampPrefix, symbol);
            byte[] heartbeatKey = ConcatKey(HeartbeatPrefix, symbol);

            // Get heartbeat interval
            BigInteger heartbeat = Storage.Get(heartbeatKey).ToBigInteger();
            if (heartbeat == 0) heartbeat = 3600; // Default 1 hour

            // Verify timestamp is recent enough
            BigInteger lastTimestamp = Storage.Get(timestampKey).ToBigInteger();
            if (timestamp <= lastTimestamp)
                throw new Exception("Stale timestamp");
            if (timestamp - lastTimestamp > heartbeat)
                throw new Exception("Heartbeat exceeded");

            // Store price and timestamp
            Storage.Put(priceKey, price);
            Storage.Put(timestampKey, timestamp);

            // Emit event
            OnPriceUpdated(symbol, price, timestamp);
        }

        // Helper to concatenate storage keys
        private static byte[] ConcatKey(byte[] prefix, string key)
        {
            return prefix.Concat(key.ToByteArray());
        }
    }
}`;

const configCode = `// Configure price feed settings
await priceFeed.configure({
  symbol: 'NEO/USD',
  sources: ['binance', 'huobi', 'okex'],
  aggregation: {
    type: 'median',
    minimumSources: 2,
    maximumDeviation: 0.01, // 1%
  },
  heartbeat: {
    interval: 300, // 5 minutes
    deviation: 0.005, // 0.5%
  },
  gasConfig: {
    maxGas: 10,
    priorityFee: 1,
  },
});`;

const automationCode = `// Set up automated price updates
await automation.createTask({
  name: 'NEO Price Update',
  contract: 'YOUR_CONTRACT_HASH',
  method: 'UpdatePrice',
  schedule: '*/5 * * * *', // Every 5 minutes
  params: ['NEO/USD'],
  conditions: [
    {
      type: 'deviation',
      threshold: 0.01, // 1%
    },
    {
      type: 'heartbeat',
      maxInterval: 3600, // 1 hour
    },
  ],
});`;

export default function PriceFeedsPage() {
  return (
    <article className="prose prose-slate max-w-none">
      <h1>Price Feeds Service</h1>
      <p className="lead">
        Neo Service Layer's Price Feeds service provides real-time price data from
        multiple trusted sources, secured by TEE and verified on-chain.
      </p>

      <h2>Features</h2>
      <div className="mt-6 grid gap-6 sm:grid-cols-2">
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Data Quality</h3>
          <ul className="mb-0">
            <li>Multiple trusted data sources</li>
            <li>Real-time price aggregation</li>
            <li>Outlier detection</li>
            <li>Price deviation monitoring</li>
          </ul>
        </div>
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Security</h3>
          <ul className="mb-0">
            <li>TEE-secured execution</li>
            <li>Signed price updates</li>
            <li>On-chain verification</li>
            <li>Tamper-proof delivery</li>
          </ul>
        </div>
      </div>

      <h2 className="mt-12">Getting Started</h2>
      <p>
        Initialize the Price Feeds service and start receiving price updates:
      </p>
      <CodeBlock code={initCode} language="typescript" />

      <h2>Smart Contract Integration</h2>
      <p>
        Example of a smart contract that receives and validates price updates:
      </p>
      <CodeBlock code={contractCode} language="csharp" />

      <div className="my-8 rounded-lg bg-blue-50 p-6">
        <h3 className="mt-0 text-blue-900">Price Feed Security</h3>
        <p className="text-blue-800">
          The contract implements several security measures:
        </p>
        <ul className="mb-0 text-blue-800">
          <li>Caller verification using Runtime.CheckWitness</li>
          <li>Timestamp validation to prevent stale data</li>
          <li>Heartbeat checks to ensure regular updates</li>
          <li>Storage key separation for different data types</li>
        </ul>
      </div>

      <h2>Configuration</h2>
      <p>
        Configure price feed settings including data sources, aggregation methods,
        and update conditions:
      </p>
      <CodeBlock code={configCode} language="typescript" />

      <h2>Automation</h2>
      <p>
        Set up automated price updates with custom conditions:
      </p>
      <CodeBlock code={automationCode} language="typescript" />

      <h2 className="mt-12">Advanced Features</h2>
      <div className="grid gap-6 sm:grid-cols-2">
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Price Aggregation</h3>
          <p className="mb-0">
            Customize how prices are aggregated from multiple sources:
          </p>
          <ul className="mb-0">
            <li>Median price calculation</li>
            <li>Weighted average based on volume</li>
            <li>Custom aggregation functions</li>
            <li>Source prioritization</li>
          </ul>
        </div>
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Update Conditions</h3>
          <p className="mb-0">
            Define when price updates should be triggered:
          </p>
          <ul className="mb-0">
            <li>Price deviation thresholds</li>
            <li>Time-based heartbeats</li>
            <li>Volume-based triggers</li>
            <li>Custom update conditions</li>
          </ul>
        </div>
      </div>

      <h2 className="mt-12">Monitoring</h2>
      <div className="rounded-lg border border-gray-200 p-6">
        <h3 className="mt-0">Health Checks</h3>
        <p>
          Monitor the health and performance of your price feeds:
        </p>
        <ul className="mb-0">
          <li>Source availability monitoring</li>
          <li>Update frequency tracking</li>
          <li>Price deviation alerts</li>
          <li>Gas usage optimization</li>
        </ul>
      </div>

      <div className="mt-12 rounded-xl bg-blue-50 p-6">
        <h3 className="mt-0 text-blue-900">Best Practices</h3>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <div className="rounded-lg bg-white p-4">
            <h4 className="mt-0 text-blue-900">Data Quality</h4>
            <ul className="mb-0 text-blue-700">
              <li>Use multiple data sources</li>
              <li>Set appropriate deviation thresholds</li>
              <li>Implement heartbeat checks</li>
            </ul>
          </div>
          <div className="rounded-lg bg-white p-4">
            <h4 className="mt-0 text-blue-900">Gas Optimization</h4>
            <ul className="mb-0 text-blue-700">
              <li>Configure update conditions</li>
              <li>Use gas estimation</li>
              <li>Monitor gas usage</li>
            </ul>
          </div>
        </div>
      </div>

      <div className="mt-12">
        <h2>Next Steps</h2>
        <div className="grid gap-4 sm:grid-cols-2">
          <a
            href="/docs/guides/contract-integration"
            className="rounded-lg border border-gray-200 p-6 no-underline hover:border-blue-500"
          >
            <h3 className="mt-0">Contract Integration →</h3>
            <p className="mb-0">
              Learn how to integrate price feeds with your smart contracts
            </p>
          </a>
          <a
            href="/docs/services/automation"
            className="rounded-lg border border-gray-200 p-6 no-underline hover:border-blue-500"
          >
            <h3 className="mt-0">Automation Service →</h3>
            <p className="mb-0">
              Explore automated price updates and triggers
            </p>
          </a>
        </div>
      </div>
    </article>
  );
} 