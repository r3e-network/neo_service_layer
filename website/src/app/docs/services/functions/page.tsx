'use client';

// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const installCode = `npm install @neo-service-layer/core
# or
yarn add @neo-service-layer/core`;

const initCode = `import { Client, Functions } from '@neo-service-layer/core';

// Initialize client with your Neo N3 wallet
const client = new Client({
  signMessage: async (message) => {
    return await window.neo3Wallet.signMessage(message);
  },
});

// Create Functions instance
const functions = new Functions(client);`;

const deployCode = `// Deploy a new function
const functionId = await functions.deploy({
  name: 'price-alert',
  description: 'Monitor price and send alerts',
  runtime: 'node16',
  source: \`
    import { PriceFeed } from '@neo-service-layer/core';
    
    export async function checkPrice(params) {
      const priceFeed = new PriceFeed(client);
      const price = await priceFeed.getPrice(params.symbol);
      
      if (price > params.threshold) {
        await sendNotification(params.webhook, {
          symbol: params.symbol,
          price: price,
          threshold: params.threshold,
        });
      }
      
      return { price, threshold: params.threshold };
    }
  \`,
  memory: 256,
  timeout: 30,
  environment: {
    NODE_ENV: 'production',
  },
  secrets: ['WEBHOOK_URL'],
});

// List deployed functions
const functions = await functions.list();

// Get function details
const details = await functions.get(functionId);

// Update function
await functions.update(functionId, {
  source: updatedSource,
  memory: 512,
});

// Delete function
await functions.delete(functionId);`;

const invokeCode = `// Invoke function synchronously
const result = await functions.invoke(functionId, {
  symbol: 'NEO/USD',
  threshold: 50,
  webhook: process.env.WEBHOOK_URL,
});

// Invoke function asynchronously
const jobId = await functions.invokeAsync(functionId, {
  symbol: 'NEO/USD',
  threshold: 50,
  webhook: process.env.WEBHOOK_URL,
});

// Check job status
const status = await functions.getJobStatus(jobId);

// Get function logs
const logs = await functions.getLogs(functionId, {
  from: new Date('2024-01-01'),
  to: new Date(),
  limit: 100,
});

// Get function metrics
const metrics = await functions.getMetrics(functionId, {
  from: new Date('2024-01-01'),
  to: new Date(),
});`;

const automationCode = `// Create automation task to run function
const automation = new Automation(client);

await automation.createTask({
  name: 'Price Alert Check',
  type: 'function',
  functionId: functionId,
  schedule: '*/5 * * * *', // Every 5 minutes
  params: {
    symbol: 'NEO/USD',
    threshold: 50,
    webhook: process.env.WEBHOOK_URL,
  },
});`;

export default function FunctionsPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl lg:max-w-4xl">
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 dark:text-white">
            Functions Service
          </h1>
          
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            The Functions service enables you to deploy and run serverless functions in a secure Trusted Execution Environment. 
            Perfect for off-chain computation, API integrations, and complex automation tasks.
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
                Initialize the client and create a Functions instance:
              </p>
              <div className="mt-4">
                <CodeBlock code={initCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Deploying Functions
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Deploy, manage, and update your serverless functions:
              </p>
              <div className="mt-4">
                <CodeBlock code={deployCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Invoking Functions
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Execute functions and monitor their performance:
              </p>
              <div className="mt-4">
                <CodeBlock code={invokeCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Automation Integration
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Schedule function execution using the Automation service:
              </p>
              <div className="mt-4">
                <CodeBlock code={automationCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Runtime Environments
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Node.js
                  </h3>
                  <dl className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <div>
                      <dt className="font-medium">Versions:</dt>
                      <dd>16.x, 18.x</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Memory:</dt>
                      <dd>128MB - 1GB</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Timeout:</dt>
                      <dd>1-300 seconds</dd>
                    </div>
                  </dl>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Python
                  </h3>
                  <dl className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <div>
                      <dt className="font-medium">Versions:</dt>
                      <dd>3.9, 3.10, 3.11</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Memory:</dt>
                      <dd>128MB - 1GB</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Timeout:</dt>
                      <dd>1-300 seconds</dd>
                    </div>
                  </dl>
                </div>
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Security Features
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    TEE Protection
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    All functions run in isolated Trusted Execution Environments, ensuring 
                    code and data confidentiality and integrity.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Secrets Management
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Secure storage and access to sensitive configuration values and API keys, 
                    integrated with the Secrets service.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Network Isolation
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Functions run in isolated network environments with configurable access 
                    to external services and APIs.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Resource Limits
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Strict memory, CPU, and execution time limits to prevent resource abuse 
                    and ensure fair usage.
                  </p>
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
                    Function Design
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Keep functions focused and modular. Break complex operations into smaller, 
                    reusable functions. Use async/await for better performance and resource utilization.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Error Handling
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Implement comprehensive error handling and logging. Use try-catch blocks 
                    and provide detailed error messages. Set up alerts for critical failures.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Resource Management
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Monitor function performance and adjust memory allocation as needed. 
                    Use caching when appropriate. Clean up resources in finally blocks.
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