'use client';

// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const installCode = `npm install @neo-service-layer/core
# or
yarn add @neo-service-layer/core`;

const initCode = `import { Client, Trigger } from '@neo-service-layer/core';

// Initialize client with your Neo N3 wallet
const client = new Client({
  signMessage: async (message) => {
    return await window.neo3Wallet.signMessage(message);
  },
});

// Create Trigger instance
const trigger = new Trigger(client);`;

const eventTriggerCode = `// Create event trigger
const triggerId = await trigger.createEventTrigger({
  name: 'token-transfer',
  description: 'Monitor token transfers',
  contract: 'script hash of token contract',
  event: 'Transfer',
  filters: {
    from: 'specific address', // optional
    to: 'specific address',   // optional
    amount: {                 // optional
      min: 1000,
      max: 10000
    }
  },
  action: {
    type: 'function',
    functionId: 'process-transfer',
    params: {
      notifyUrl: 'https://api.example.com/webhook'
    }
  }
});

// List event triggers
const triggers = await trigger.listEventTriggers();

// Get trigger details
const details = await trigger.getEventTrigger(triggerId);

// Update trigger
await trigger.updateEventTrigger(triggerId, {
  filters: {
    amount: {
      min: 2000,
      max: 20000
    }
  }
});

// Delete trigger
await trigger.deleteEventTrigger(triggerId);`;

const conditionTriggerCode = `// Create condition trigger
const triggerId = await trigger.createConditionTrigger({
  name: 'price-threshold',
  description: 'Monitor price threshold',
  condition: {
    type: 'price',
    symbol: 'NEO/USD',
    operator: '>',
    value: 50
  },
  action: {
    type: 'contract',
    contract: 'script hash of contract',
    method: 'executeOrder',
    params: ['param1', 'param2']
  },
  checkInterval: 300 // Check every 5 minutes
});

// List condition triggers
const triggers = await trigger.listConditionTriggers();

// Get trigger details
const details = await trigger.getConditionTrigger(triggerId);

// Update trigger
await trigger.updateConditionTrigger(triggerId, {
  condition: {
    value: 60
  }
});

// Delete trigger
await trigger.deleteConditionTrigger(triggerId);`;

const monitoringCode = `// Get trigger execution history
const history = await trigger.getExecutionHistory(triggerId, {
  from: new Date('2024-01-01'),
  to: new Date(),
  limit: 100
});

// Get trigger metrics
const metrics = await trigger.getMetrics(triggerId, {
  from: new Date('2024-01-01'),
  to: new Date()
});

// Subscribe to trigger events
trigger.on('triggerExecuted', (event) => {
  console.log('Trigger executed:', event.triggerId);
  console.log('Status:', event.status);
  console.log('Result:', event.result);
});`;

export default function TriggerPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl lg:max-w-4xl">
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 dark:text-white">
            Trigger Service
          </h1>
          
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            The Trigger service enables you to monitor blockchain events and conditions, 
            automatically executing actions when specific criteria are met. Perfect for 
            building responsive and automated blockchain applications.
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
                Initialize the client and create a Trigger instance:
              </p>
              <div className="mt-4">
                <CodeBlock code={initCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Event Triggers
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Create and manage triggers based on blockchain events:
              </p>
              <div className="mt-4">
                <CodeBlock code={eventTriggerCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Condition Triggers
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Create and manage triggers based on custom conditions:
              </p>
              <div className="mt-4">
                <CodeBlock code={conditionTriggerCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Monitoring
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Monitor trigger executions and performance:
              </p>
              <div className="mt-4">
                <CodeBlock code={monitoringCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Trigger Types
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Event Triggers
                  </h3>
                  <dl className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <div>
                      <dt className="font-medium">Contract Events:</dt>
                      <dd>Monitor specific events from smart contracts</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Transaction Events:</dt>
                      <dd>Track transactions matching specific criteria</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Block Events:</dt>
                      <dd>React to new blocks and chain updates</dd>
                    </div>
                  </dl>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Condition Triggers
                  </h3>
                  <dl className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <div>
                      <dt className="font-medium">Price Conditions:</dt>
                      <dd>Monitor price thresholds and changes</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Balance Conditions:</dt>
                      <dd>Track account balance changes</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Custom Conditions:</dt>
                      <dd>Define custom logic for trigger conditions</dd>
                    </div>
                  </dl>
                </div>
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Action Types
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Function Actions
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Execute serverless functions in response to triggers. Perfect for 
                    complex processing, notifications, and integrations.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Contract Actions
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Call smart contract methods automatically when triggers fire. 
                    Ideal for on-chain automation and interactions.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Webhook Actions
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Send HTTP requests to external services. Great for integrating 
                    with existing systems and APIs.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Composite Actions
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Combine multiple actions into a single trigger response. 
                    Execute complex workflows automatically.
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
                    Trigger Design
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Keep triggers focused and specific. Use appropriate filters to reduce 
                    noise. Consider the frequency of events and potential impact on resources.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Error Handling
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Implement proper error handling in trigger actions. Set up monitoring 
                    and alerts for trigger failures. Use retry policies where appropriate.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Performance
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Monitor trigger execution times and resource usage. Optimize conditions 
                    and filters to reduce unnecessary processing. Use appropriate check intervals.
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