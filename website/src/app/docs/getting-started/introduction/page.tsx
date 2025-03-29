import React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

export const metadata = {
  title: 'Introduction - Neo Service Layer Documentation',
  description: 'Introduction to Neo Service Layer and its core concepts',
};

const installCode = `npm install @neo-service-layer/core
# or
yarn add @neo-service-layer/core`;

const usageCode = `import { PriceFeed, Automation } from '@neo-service-layer/core';

// Initialize the client with your signature
const client = new Client({
  signMessage: async (message) => {
    // Sign message using your Neo N3 wallet
    return signature;
  }
});

// Get real-time price data
const priceFeed = new PriceFeed(client);
const neoPrice = await priceFeed.getPrice('NEO/USD');

// Set up contract automation
const automation = new Automation(client);
await automation.createTask({
  name: 'Price Update',
  contract: 'YOUR_CONTRACT_HASH',
  method: 'updatePrice',
  schedule: '*/30 * * * *',
  params: [neoPrice],
});`;

export default function IntroductionPage() {
  return (
    <div className="py-16 sm:py-24">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl lg:max-w-4xl">
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Introduction to Neo Service Layer
          </h1>
          
          <div className="mt-10 prose prose-lg prose-gray dark:prose-invert">
            <p>
              Neo Service Layer is a comprehensive infrastructure solution for the Neo N3 blockchain,
              providing essential services for building and maintaining decentralized applications.
              Our platform combines the security of Trusted Execution Environments (TEE) with the
              flexibility of modern cloud services.
            </p>

            <h2>Key Features</h2>
            <ul>
              <li>
                <strong>Secure Execution:</strong> All services run in TEE, ensuring the highest
                level of security for your sensitive operations.
              </li>
              <li>
                <strong>Real-time Data:</strong> Access accurate and timely price feeds from
                multiple trusted sources.
              </li>
              <li>
                <strong>Smart Contract Automation:</strong> Automate your contract operations
                with customizable triggers and schedules.
              </li>
              <li>
                <strong>Gas Management:</strong> Efficient handling of transaction fees with
                automated top-ups and monitoring.
              </li>
              <li>
                <strong>Secrets Management:</strong> Secure storage and access control for
                sensitive data and credentials.
              </li>
            </ul>

            <h2>Quick Start</h2>
            <p>
              Get started by installing the Neo Service Layer SDK:
            </p>
            <CodeBlock code={installCode} language="bash" />

            <p>
              Here's a simple example that demonstrates how to use price feeds and contract
              automation:
            </p>
            <CodeBlock code={usageCode} language="typescript" />

            <h2>Architecture Overview</h2>
            <p>
              Neo Service Layer is built on three core principles:
            </p>
            <ul>
              <li>
                <strong>Security First:</strong> TEE-based execution ensures that your code and
                data remain protected at all times.
              </li>
              <li>
                <strong>Decentralization:</strong> All critical operations are verified and
                recorded on the Neo N3 blockchain.
              </li>
              <li>
                <strong>Developer Experience:</strong> Comprehensive SDKs and documentation
                make integration seamless.
              </li>
            </ul>

            <h2>Authentication</h2>
            <p>
              Instead of traditional username/password authentication, Neo Service Layer uses
              cryptographic signatures. Users sign their requests using their Neo N3 wallet,
              ensuring secure and decentralized authentication.
            </p>

            <div className="mt-8 rounded-xl bg-blue-50 dark:bg-blue-900/20 p-6">
              <h3 className="mt-0 text-blue-900 dark:text-blue-100">Next Steps</h3>
              <p className="text-blue-800 dark:text-blue-200">
                Continue with our documentation to learn more about specific services and
                integration patterns:
              </p>
              <ul className="mt-4">
                <li>
                  <a href="/docs/getting-started/installation" className="text-blue-600 dark:text-blue-400 hover:underline">
                    Installation Guide
                  </a>
                </li>
                <li>
                  <a href="/docs/getting-started/concepts" className="text-blue-600 dark:text-blue-400 hover:underline">
                    Basic Concepts
                  </a>
                </li>
                <li>
                  <a href="/docs/getting-started/authentication" className="text-blue-600 dark:text-blue-400 hover:underline">
                    Authentication Guide
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
} 