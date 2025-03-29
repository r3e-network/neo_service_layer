// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const initCode = `import { Automation } from '@neo-service-layer/core';

const automation = new Automation(client);

// Create a new automation task
const task = await automation.createTask({
  name: 'Daily Balance Check',
  contract: 'YOUR_CONTRACT_HASH',
  method: 'checkAndUpdateBalance',
  schedule: '0 0 * * *', // Daily at midnight
  params: ['NEO/USD'],
});

// Get task status
const status = await automation.getTaskStatus(task.id);
console.log('Task status:', status);`;

const contractCode = `using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System;
using System.ComponentModel;

namespace NeoServiceLayer
{
    [DisplayName("AutomatedContract")]
    [ManifestExtra("Author", "Neo Service Layer")]
    public class AutomatedContract : SmartContract
    {
        // Events
        [DisplayName("TaskExecuted")]
        public static event Action<string, bool> OnTaskExecuted;

        // Storage keys
        private static readonly byte[] LastExecutionPrefix = "last_execution_".ToByteArray();
        private static readonly byte[] OwnerKey = "owner".ToByteArray();

        // Constructor
        public static void _deploy(object data, bool update)
        {
            if (!update)
            {
                Storage.Put(OwnerKey, Transaction.Sender);
            }
        }

        // Method to be automated
        public static void CheckAndUpdateBalance(string symbol)
        {
            // Verify caller
            if (!Runtime.CheckWitness(GetOwner()))
                throw new Exception("Unauthorized");

            // Get current timestamp
            var currentTime = Runtime.Time;

            // Get last execution time
            byte[] lastExecutionKey = ConcatKey(LastExecutionPrefix, symbol);
            BigInteger lastExecution = Storage.Get(lastExecutionKey).ToBigInteger();

            try
            {
                // Perform balance check and update
                // ... your business logic here ...

                // Update last execution time
                Storage.Put(lastExecutionKey, currentTime);

                // Emit success event
                OnTaskExecuted(symbol, true);
            }
            catch (Exception e)
            {
                // Emit failure event
                OnTaskExecuted(symbol, false);
                throw;
            }
        }

        // Helper to concatenate storage keys
        private static byte[] ConcatKey(byte[] prefix, string key)
        {
            return prefix.Concat(key.ToByteArray());
        }

        // Get the contract owner
        public static byte[] GetOwner()
        {
            return Storage.Get(OwnerKey);
        }
    }
}`;

const configCode = `// Configure automation task with conditions
await automation.createTask({
  name: 'Balance Monitor',
  contract: 'YOUR_CONTRACT_HASH',
  method: 'checkAndUpdateBalance',
  schedule: '*/15 * * * *', // Every 15 minutes
  params: ['NEO/USD'],
  conditions: [
    {
      type: 'threshold',
      field: 'balance',
      operator: 'lt',
      value: '1000',
    },
    {
      type: 'time',
      minInterval: 3600, // Minimum 1 hour between executions
    },
  ],
  retryConfig: {
    maxAttempts: 3,
    backoffMultiplier: 1.5,
    initialDelay: 1000,
  },
  gasConfig: {
    maxGas: 10,
    priorityFee: 1,
  },
});`;

const monitoringCode = `// Set up task monitoring
automation.on('taskExecuted', (event) => {
  console.log('Task executed:', event.taskId);
  console.log('Success:', event.success);
  console.log('Gas used:', event.gasUsed);
});

// Get task execution history
const history = await automation.getTaskHistory(taskId, {
  limit: 10,
  status: 'completed',
});

// Get task metrics
const metrics = await automation.getTaskMetrics(taskId);
console.log('Average gas used:', metrics.averageGasUsed);
console.log('Success rate:', metrics.successRate);`;

export default function AutomationPage() {
  return (
    <article className="prose prose-slate max-w-none">
      <h1>Contract Automation Service</h1>
      <p className="lead">
        Neo Service Layer's Contract Automation service enables reliable, secure, and
        customizable automation of your smart contracts, similar to Chainlink Keeper
        functionality.
      </p>

      <h2>Features</h2>
      <div className="mt-6 grid gap-6 sm:grid-cols-2">
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Task Management</h3>
          <ul className="mb-0">
            <li>Scheduled executions</li>
            <li>Conditional triggers</li>
            <li>Custom parameters</li>
            <li>Retry mechanisms</li>
          </ul>
        </div>
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Security</h3>
          <ul className="mb-0">
            <li>TEE-secured execution</li>
            <li>Signature verification</li>
            <li>Gas management</li>
            <li>Error handling</li>
          </ul>
        </div>
      </div>

      <h2 className="mt-12">Getting Started</h2>
      <p>
        Initialize the Automation service and create your first task:
      </p>
      <CodeBlock code={initCode} language="typescript" />

      <h2>Smart Contract Integration</h2>
      <p>
        Example of a smart contract designed for automation:
      </p>
      <CodeBlock code={contractCode} language="csharp" />

      <div className="my-8 rounded-lg bg-blue-50 p-6">
        <h3 className="mt-0 text-blue-900">Contract Security</h3>
        <p className="text-blue-800">
          The contract implements several security measures:
        </p>
        <ul className="mb-0 text-blue-800">
          <li>Caller verification using Runtime.CheckWitness</li>
          <li>Execution time tracking</li>
          <li>Error handling and event emission</li>
          <li>Storage key separation</li>
        </ul>
      </div>

      <h2>Task Configuration</h2>
      <p>
        Configure automation tasks with conditions, retry logic, and gas settings:
      </p>
      <CodeBlock code={configCode} language="typescript" />

      <h2>Monitoring</h2>
      <p>
        Monitor task executions and performance:
      </p>
      <CodeBlock code={monitoringCode} language="typescript" />

      <h2 className="mt-12">Advanced Features</h2>
      <div className="grid gap-6 sm:grid-cols-2">
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Trigger Conditions</h3>
          <p className="mb-0">
            Customize when your tasks should execute:
          </p>
          <ul className="mb-0">
            <li>Time-based schedules</li>
            <li>Value thresholds</li>
            <li>Event triggers</li>
            <li>Custom conditions</li>
          </ul>
        </div>
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Execution Control</h3>
          <p className="mb-0">
            Fine-tune task execution behavior:
          </p>
          <ul className="mb-0">
            <li>Retry strategies</li>
            <li>Gas optimization</li>
            <li>Priority settings</li>
            <li>Dependency management</li>
          </ul>
        </div>
      </div>

      <h2 className="mt-12">Performance Optimization</h2>
      <div className="rounded-lg border border-gray-200 p-6">
        <h3 className="mt-0">Best Practices</h3>
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <h4 className="mt-0 text-gray-900">Gas Efficiency</h4>
            <ul className="mb-0">
              <li>Set appropriate gas limits</li>
              <li>Use gas estimation</li>
              <li>Optimize contract code</li>
              <li>Monitor gas trends</li>
            </ul>
          </div>
          <div>
            <h4 className="mt-0 text-gray-900">Reliability</h4>
            <ul className="mb-0">
              <li>Implement proper error handling</li>
              <li>Use retry mechanisms</li>
              <li>Set up monitoring</li>
              <li>Regular maintenance</li>
            </ul>
          </div>
        </div>
      </div>

      <div className="mt-12 rounded-xl bg-blue-50 p-6">
        <h3 className="mt-0 text-blue-900">Task Management Best Practices</h3>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <div className="rounded-lg bg-white p-4">
            <h4 className="mt-0 text-blue-900">Task Design</h4>
            <ul className="mb-0 text-blue-700">
              <li>Keep tasks atomic and focused</li>
              <li>Implement proper validation</li>
              <li>Handle edge cases</li>
            </ul>
          </div>
          <div className="rounded-lg bg-white p-4">
            <h4 className="mt-0 text-blue-900">Monitoring</h4>
            <ul className="mb-0 text-blue-700">
              <li>Set up alerts for failures</li>
              <li>Track performance metrics</li>
              <li>Regular auditing</li>
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
              Learn how to integrate automation with your smart contracts
            </p>
          </a>
          <a
            href="/docs/services/gas-bank"
            className="rounded-lg border border-gray-200 p-6 no-underline hover:border-blue-500"
          >
            <h3 className="mt-0">Gas Bank Service →</h3>
            <p className="mb-0">
              Explore gas management for automated tasks
            </p>
          </a>
        </div>
      </div>
    </article>
  );
} 