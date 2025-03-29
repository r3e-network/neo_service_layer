// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const contractCode = `using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System;
using System.ComponentModel;

namespace NeoServiceLayer
{
    [DisplayName("PriceFeedConsumer")]
    [ManifestExtra("Author", "Neo Service Layer")]
    public class PriceFeedConsumer : SmartContract
    {
        // Events
        [DisplayName("PriceUpdated")]
        public static event Action<string, BigInteger> OnPriceUpdated;

        // Storage keys
        private static readonly byte[] PriceKey = "price".ToByteArray();
        private static readonly byte[] OwnerKey = "owner".ToByteArray();

        // Constructor
        public static void _deploy(object data, bool update)
        {
            if (!update)
            {
                Storage.Put(OwnerKey, Runtime.CheckWitness(Transaction.Sender));
            }
        }

        // Update price (can only be called by Neo Service Layer)
        public static void UpdatePrice(string symbol, BigInteger price)
        {
            // Verify the caller is authorized
            if (!Runtime.CheckWitness(GetOwner()))
                throw new Exception("Unauthorized");

            // Store the price
            Storage.Put(PriceKey, price);

            // Emit event
            OnPriceUpdated(symbol, price);
        }

        // Get the current price
        public static BigInteger GetPrice()
        {
            return Storage.Get(PriceKey).ToBigInteger();
        }

        // Get the contract owner
        public static byte[] GetOwner()
        {
            return Storage.Get(OwnerKey);
        }
    }
}`;

const sdkCode = `import { PriceFeed, Automation } from '@neo-service-layer/core';

// Initialize services
const client = new Client({
  signMessage: async (message) => {
    const { signature } = await window.NeoLineN3.signMessage({
      message: message,
    });
    return signature;
  },
});

const priceFeed = new PriceFeed(client);
const automation = new Automation(client);

// Set up price feed automation
await automation.createTask({
  name: 'NEO Price Update',
  contract: 'YOUR_CONTRACT_HASH',
  method: 'UpdatePrice',
  schedule: '*/5 * * * *', // Every 5 minutes
  params: ['NEO/USD'],
  gasConfig: {
    maxGas: 10,
    priorityFee: 1,
  },
});`;

const eventCode = `// Subscribe to price updates from the contract
const { DefaultNep17API } = require('@cityofzion/neon-core');
const nep17 = new DefaultNep17API();

nep17.addEventListener('PriceUpdated', (event) => {
  const [symbol, price] = event.parameters;
  console.log(\`Price updated for \${symbol}: \${price}\`);
});`;

export default function ContractIntegrationPage() {
  return (
    <article className="prose prose-slate max-w-none">
      <h1>Contract Integration Guide</h1>
      <p className="lead">
        Learn how to integrate Neo Service Layer with your smart contracts to
        receive real-time price feeds, automate contract functions, and more.
      </p>

      <h2>Smart Contract Setup</h2>
      <p>
        First, let's create a smart contract that can receive price updates from
        Neo Service Layer. This contract will:
      </p>
      <ul>
        <li>Store the latest price data</li>
        <li>Verify the caller is authorized</li>
        <li>Emit events when prices are updated</li>
      </ul>

      <CodeBlock code={contractCode} language="csharp" />

      <div className="my-8 rounded-lg bg-blue-50 p-6">
        <h3 className="mt-0 text-blue-900">Security Considerations</h3>
        <ul className="mb-0 text-blue-800">
          <li>Always verify the caller using Runtime.CheckWitness</li>
          <li>Implement proper access control for sensitive operations</li>
          <li>Use safe math operations to prevent overflows</li>
          <li>Validate input parameters before processing</li>
        </ul>
      </div>

      <h2>SDK Integration</h2>
      <p>
        After deploying your contract, use the Neo Service Layer SDK to set up
        automated price updates:
      </p>
      <CodeBlock code={sdkCode} language="typescript" />

      <h2>Event Handling</h2>
      <p>
        Listen for price update events from your contract:
      </p>
      <CodeBlock code={eventCode} language="typescript" />

      <h2>Best Practices</h2>
      <div className="mt-6 grid gap-6 sm:grid-cols-2">
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Gas Management</h3>
          <ul className="mb-0">
            <li>Set appropriate gas limits</li>
            <li>Use gas estimation for dynamic pricing</li>
            <li>Implement gas optimization patterns</li>
            <li>Monitor gas usage trends</li>
          </ul>
        </div>
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Error Handling</h3>
          <ul className="mb-0">
            <li>Implement proper error messages</li>
            <li>Handle network failures gracefully</li>
            <li>Add retry mechanisms</li>
            <li>Log errors for debugging</li>
          </ul>
        </div>
      </div>

      <h2 className="mt-12">Advanced Features</h2>
      <div className="grid gap-6 sm:grid-cols-2">
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Multi-Source Aggregation</h3>
          <p className="mb-0">
            Configure your contract to receive price data from multiple sources and
            implement custom aggregation logic:
          </p>
          <ul className="mb-0">
            <li>Median price calculation</li>
            <li>Weighted averages</li>
            <li>Outlier detection</li>
            <li>Confidence scoring</li>
          </ul>
        </div>
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Heartbeat Monitoring</h3>
          <p className="mb-0">
            Implement heartbeat checks to ensure price data freshness:
          </p>
          <ul className="mb-0">
            <li>Maximum update interval</li>
            <li>Staleness checks</li>
            <li>Alert mechanisms</li>
            <li>Fallback procedures</li>
          </ul>
        </div>
      </div>

      <h2 className="mt-12">Testing</h2>
      <div className="rounded-lg border border-gray-200 p-6">
        <h3 className="mt-0">Test Environment</h3>
        <p>
          Neo Service Layer provides a testnet environment for development and
          testing. To use it:
        </p>
        <ol className="mb-0">
          <li>Deploy your contract to Neo N3 TestNet</li>
          <li>Configure the SDK to use testnet endpoints</li>
          <li>Use test tokens for gas payments</li>
          <li>Monitor test transactions and events</li>
        </ol>
      </div>

      <div className="mt-12 rounded-xl bg-blue-50 p-6">
        <h3 className="mt-0 text-blue-900">Ready to Deploy?</h3>
        <p className="text-blue-800">
          Before deploying to mainnet, ensure you have:
        </p>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <div className="rounded-lg bg-white p-4">
            <h4 className="mt-0 text-blue-900">Contract Audit</h4>
            <ul className="mb-0 text-blue-700">
              <li>Security review completed</li>
              <li>Gas optimization verified</li>
              <li>Error handling tested</li>
            </ul>
          </div>
          <div className="rounded-lg bg-white p-4">
            <h4 className="mt-0 text-blue-900">Monitoring Setup</h4>
            <ul className="mb-0 text-blue-700">
              <li>Event listeners configured</li>
              <li>Alert systems in place</li>
              <li>Backup procedures ready</li>
            </ul>
          </div>
        </div>
      </div>

      <div className="mt-12">
        <h2>Next Steps</h2>
        <div className="grid gap-4 sm:grid-cols-2">
          <a
            href="/docs/services/price-feeds"
            className="rounded-lg border border-gray-200 p-6 no-underline hover:border-blue-500"
          >
            <h3 className="mt-0">Price Feeds →</h3>
            <p className="mb-0">
              Learn more about price feed configuration and customization
            </p>
          </a>
          <a
            href="/docs/services/automation"
            className="rounded-lg border border-gray-200 p-6 no-underline hover:border-blue-500"
          >
            <h3 className="mt-0">Contract Automation →</h3>
            <p className="mb-0">
              Explore advanced automation features and patterns
            </p>
          </a>
        </div>
      </div>
    </article>
  );
}