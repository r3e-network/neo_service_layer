'use client';

import React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const installCode = `npm install @neo-service-layer/core
# or
yarn add @neo-service-layer/core`;

const initCode = `import { Client, Logging } from '@neo-service-layer/core';

// Initialize client with your Neo N3 wallet
const client = new Client({
  signMessage: async (message) => {
    return await window.neo3Wallet.signMessage(message);
  },
});

// Create Logging instance
const logging = new Logging(client);`;

const loggingCode = `// Log a message
await logging.log('info', 'User action completed', {
  userId: '12345',
  action: 'transfer',
  amount: 100
});

// Log with different levels
await logging.debug('Debug message');
await logging.info('Info message');
await logging.warn('Warning message');
await logging.error('Error message', new Error('Something went wrong'));

// Structured logging
await logging.log('info', 'Transaction processed', {
  transactionId: 'tx123',
  type: 'token_transfer',
  from: 'address1',
  to: 'address2',
  amount: 1000,
  token: 'NEO',
  timestamp: new Date().toISOString(),
  metadata: {
    network: 'mainnet',
    gas_used: 0.5,
    status: 'success'
  }
});`;

const queryCode = `// Query logs
const logs = await logging.query({
  level: 'error',
  from: new Date('2024-01-01'),
  to: new Date(),
  filters: {
    userId: '12345',
    action: 'transfer'
  },
  limit: 100,
  offset: 0
});

// Get log statistics
const stats = await logging.getStats({
  from: new Date('2024-01-01'),
  to: new Date(),
  groupBy: ['level', 'action']
});

// Stream real-time logs
const subscription = logging.subscribe({
  level: 'error',
  filters: {
    action: 'transfer'
  }
});

subscription.on('log', (log) => {
  console.log('New log:', log);
});

// Unsubscribe when done
subscription.unsubscribe();`;

const retentionCode = `// Configure log retention
await logging.setRetentionPolicy({
  default: '30d',
  rules: [
    {
      level: 'error',
      retention: '90d'
    },
    {
      type: 'security_audit',
      retention: '365d'
    }
  ]
});

// Export logs
const exportId = await logging.exportLogs({
  from: new Date('2024-01-01'),
  to: new Date(),
  format: 'json',
  compression: 'gzip',
  destination: {
    type: 's3',
    bucket: 'my-logs',
    prefix: 'exports/'
  }
});

// Check export status
const status = await logging.getExportStatus(exportId);`;

const integrationCode = `// Integration with Functions Service
export async function myFunction(context) {
  const { logging } = context.services;
  
  try {
    // Log function execution start
    await logging.info('Function execution started', {
      functionId: context.functionId,
      trigger: context.trigger
    });

    // Your function logic here
    
    // Log function execution success
    await logging.info('Function execution completed', {
      functionId: context.functionId,
      duration: context.executionTime
    });
  } catch (error) {
    // Log function execution error
    await logging.error('Function execution failed', {
      functionId: context.functionId,
      error: error.message,
      stack: error.stack
    });
    throw error;
  }
}

// Integration with Price Feed Service
const priceFeed = new PriceFeed(client);
priceFeed.onPriceUpdate('NEO/USD', async (price) => {
  await logging.info('Price update received', {
    pair: 'NEO/USD',
    price: price.value,
    timestamp: price.timestamp,
    source: price.source
  });
});

// Integration with Contract Automation
const automation = new Automation(client);
automation.onTaskExecution('myTask', async (execution) => {
  await logging.info('Contract automation task executed', {
    taskId: execution.taskId,
    contract: execution.contract,
    method: execution.method,
    result: execution.result,
    gasUsed: execution.gasUsed
  });
});

// Integration with Gas Bank
const gasBank = new GasBank(client);
gasBank.onLowBalance(async (event) => {
  await logging.warn('Contract low on GAS', {
    contract: event.contract,
    balance: event.balance,
    threshold: event.threshold
  });
});

// Integration with Trigger Service
const trigger = new Trigger(client);
trigger.onTriggerFired('myTrigger', async (event) => {
  await logging.info('Trigger fired', {
    triggerId: event.triggerId,
    condition: event.condition,
    actions: event.actions
  });
});

// Integration with Secrets Service
const secrets = new Secrets(client);
secrets.onSecretRotation('mySecret', async (event) => {
  await logging.info('Secret rotated', {
    secretId: event.secretId,
    version: event.version,
    rotatedAt: event.timestamp
  });
});

// Integration with Metrics Service
const metrics = new Metrics(client);
metrics.onAlertTriggered('highLatency', async (alert) => {
  await logging.warn('High latency detected', {
    metric: alert.metric,
    value: alert.value,
    threshold: alert.threshold,
    service: alert.service
  });
});`;

export default function LoggingPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl lg:max-w-4xl">
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 dark:text-white">
            Logging Service
          </h1>
          
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            The Logging service provides centralized log management for your Neo Service Layer 
            applications. Collect, store, and analyze logs from all your services in a secure 
            and scalable way.
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
                Initialize the client and create a Logging instance:
              </p>
              <div className="mt-4">
                <CodeBlock code={initCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Logging
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Record logs with different levels and structured data:
              </p>
              <div className="mt-4">
                <CodeBlock code={loggingCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Querying
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Search and analyze logs:
              </p>
              <div className="mt-4">
                <CodeBlock code={queryCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Management
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Manage log retention and exports:
              </p>
              <div className="mt-4">
                <CodeBlock code={retentionCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Log Types
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Application Logs
                  </h3>
                  <dl className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <div>
                      <dt className="font-medium">Debug:</dt>
                      <dd>Detailed debugging information</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Info:</dt>
                      <dd>General information about system operation</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Warning:</dt>
                      <dd>Warning messages for potential issues</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Error:</dt>
                      <dd>Error messages for actual problems</dd>
                    </div>
                  </dl>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    System Logs
                  </h3>
                  <dl className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <div>
                      <dt className="font-medium">Security:</dt>
                      <dd>Authentication and authorization events</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Performance:</dt>
                      <dd>System performance and resource usage</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Audit:</dt>
                      <dd>System and data access audit trail</dd>
                    </div>
                  </dl>
                </div>
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Features
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Structured Logging
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Rich structured data support with automatic indexing. 
                    Custom fields and metadata for better organization and search.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Real-time Processing
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Stream logs in real-time for monitoring and alerts. 
                    Automatic parsing and enrichment of log data.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Security
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Secure log storage with encryption. Access control and 
                    audit trails for log access and management.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Analytics
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Advanced search and analysis capabilities. Custom dashboards 
                    and visualizations for log data.
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
                    Log Structure
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Use consistent log formats and levels. Include relevant context 
                    in structured data. Avoid logging sensitive information.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Performance
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Use appropriate log levels in production. Batch log submissions 
                    when possible. Configure proper retention policies.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Security
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Implement proper access controls. Regularly audit log access 
                    and usage. Follow data protection regulations.
                  </p>
                </div>
              </div>
            </section>

            <section className="mt-16">
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Service Integrations
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                The Logging service seamlessly integrates with other Neo Service Layer services to provide comprehensive logging coverage across your entire application:
              </p>
              <div className="mt-4">
                <CodeBlock code={integrationCode} language="typescript" />
              </div>
              
              <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Functions Integration
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Automatically log function executions, errors, and performance metrics within your serverless functions.
                  </p>
                </div>
                
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Price Feed Integration
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Track price updates, data sources, and feed health with structured logging.
                  </p>
                </div>
                
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Contract Automation
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Monitor automated task executions, gas usage, and contract interactions.
                  </p>
                </div>
                
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Gas Bank Integration
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Track GAS balances, auto-funding events, and usage patterns across contracts.
                  </p>
                </div>
                
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Trigger Integration
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Log trigger conditions, executions, and resulting actions with full context.
                  </p>
                </div>
                
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Secrets Integration
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Audit secret access, rotations, and usage while maintaining security.
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