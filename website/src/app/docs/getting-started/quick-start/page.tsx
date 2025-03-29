import React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const installCode = `npm install @neo-service-layer/core
# or
yarn add @neo-service-layer/core`;

const initCode = `import { Client } from '@neo-service-layer/core';

const client = new Client({
  signMessage: async (message) => {
    // Sign message using your Neo N3 wallet
    // Example using NeoLine wallet
    const { signature } = await window.NeoLineN3.signMessage({
      message: message,
    });
    return signature;
  },
});`;

const priceFeedCode = `import { PriceFeed } from '@neo-service-layer/core';

const priceFeed = new PriceFeed(client);

// Get real-time price data
const neoPrice = await priceFeed.getPrice('NEO/USD');
console.log('Current NEO/USD price:', neoPrice);

// Subscribe to price updates
priceFeed.subscribe('NEO/USD', (price) => {
  console.log('NEO/USD price updated:', price);
});`;

const automationCode = `import { Automation } from '@neo-service-layer/core';

const automation = new Automation(client);

// Create a new automation task
await automation.createTask({
  name: 'Price Update',
  contract: 'YOUR_CONTRACT_HASH',
  method: 'updatePrice',
  schedule: '*/30 * * * *', // Every 30 minutes
  params: ['NEO/USD'],
});

// List all tasks
const tasks = await automation.listTasks();
console.log('Active tasks:', tasks);`;

const secretsCode = `import { Secrets } from '@neo-service-layer/core';

const secrets = new Secrets(client);

// Store a secret
await secrets.setSecret('api_key', 'your-api-key');

// Use secret in a function
const function = new Function(client);
await function.create({
  name: 'process-data',
  code: \`
    const apiKey = await secrets.getSecret('api_key');
    // Use apiKey in your function
  \`,
});`;

export default function QuickStartPage() {
  return (
    <article className="prose prose-slate max-w-none">
      <h1>Quick Start Guide</h1>
      <p className="lead">
        This guide will help you get started with Neo Service Layer in just a few
        minutes. You'll learn how to set up the SDK, authenticate with your Neo N3
        wallet, and use core services.
      </p>

      <h2>Prerequisites</h2>
      <ul>
        <li>Node.js 16 or later</li>
        <li>A Neo N3 wallet (e.g., NeoLine)</li>
        <li>Basic knowledge of Neo N3 smart contracts</li>
      </ul>

      <h2>Installation</h2>
      <p>
        First, install the Neo Service Layer SDK using npm or yarn:
      </p>
      <CodeBlock code={installCode} language="bash" />

      <h2>Authentication</h2>
      <p>
        Neo Service Layer uses message signing for authentication. Initialize the client
        with your wallet's signing function:
      </p>
      <CodeBlock code={initCode} language="typescript" />

      <div className="my-8 rounded-lg bg-blue-50 p-6">
        <h3 className="mt-0 text-blue-900">Security Note</h3>
        <p className="mb-0 text-blue-800">
          Never share your private keys. The service only requires signed messages for
          authentication, not your actual keys.
        </p>
      </div>

      <h2>Using Price Feeds</h2>
      <p>
        Price feeds provide real-time price data from trusted sources. Here's how to
        get started:
      </p>
      <CodeBlock code={priceFeedCode} language="typescript" />

      <h2>Setting Up Contract Automation</h2>
      <p>
        Automate your smart contracts with customizable triggers and schedules:
      </p>
      <CodeBlock code={automationCode} language="typescript" />

      <h2>Managing Secrets</h2>
      <p>
        Securely store and use sensitive data in your functions:
      </p>
      <CodeBlock code={secretsCode} language="typescript" />

      <h2>Next Steps</h2>
      <div className="mt-8 grid gap-6 sm:grid-cols-2">
        <a
          href="/docs/guides/contract-integration"
          className="rounded-lg border border-gray-200 p-6 no-underline hover:border-blue-500"
        >
          <h3 className="mt-0 text-gray-900">Contract Integration →</h3>
          <p className="mb-0 text-gray-600">
            Learn how to integrate Neo Service Layer with your smart contracts
          </p>
        </a>
        <a
          href="/docs/services/functions"
          className="rounded-lg border border-gray-200 p-6 no-underline hover:border-blue-500"
        >
          <h3 className="mt-0 text-gray-900">Serverless Functions →</h3>
          <p className="mb-0 text-gray-600">
            Deploy and manage secure serverless functions
          </p>
        </a>
      </div>

      <div className="mt-12 rounded-xl bg-gray-50 p-6">
        <h3 className="mt-0">Need Help?</h3>
        <p>
          If you encounter any issues or have questions, you can:
        </p>
        <ul className="mb-0">
          <li>
            Join our{' '}
            <a href="https://discord.gg/neo" target="_blank" rel="noopener">
              Discord community
            </a>
          </li>
          <li>
            Check out our{' '}
            <a
              href="https://github.com/neo-project/neo-service-layer"
              target="_blank"
              rel="noopener"
            >
              GitHub repository
            </a>
          </li>
          <li>
            Read the{' '}
            <a href="/docs/api/sdk">SDK Reference</a>
          </li>
        </ul>
      </div>
    </article>
  );
} 