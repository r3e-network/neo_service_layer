'use client';

// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const installCode = `npm install @neo-service-layer/core
# or
yarn add @neo-service-layer/core`;

const initCode = `import { Client, Automation } from '@neo-service-layer/core';

// Initialize client with your Neo N3 wallet
const client = new Client({
  signMessage: async (message) => {
    return await window.neo3Wallet.signMessage(message);
  },
});

// Create Automation instance
const automation = new Automation(client);`;

const createTaskCode = `// Create a time-based automation task
const task = await automation.createTask({
  name: 'Daily Token Distribution',
  contract: 'YOUR_CONTRACT_HASH',
  method: 'distributeTokens',
  schedule: '0 0 * * *', // Daily at midnight
  params: ['1000'], // Amount to distribute
});

// Create an event-based automation task
const eventTask = await automation.createTask({
  name: 'Process Deposit',
  contract: 'YOUR_CONTRACT_HASH',
  method: 'processDeposit',
  trigger: {
    type: 'event',
    contract: 'TOKEN_CONTRACT_HASH',
    eventName: 'Transfer',
    filter: {
      to: 'YOUR_CONTRACT_HASH',
    },
  },
});`;

const manageTasksCode = `// List all tasks
const tasks = await automation.listTasks();

// Get task status
const status = await automation.getTaskStatus('task-id');

// Update task schedule
await automation.updateTask('task-id', {
  schedule: '*/30 * * * *', // Every 30 minutes
});

// Delete task
await automation.deleteTask('task-id');

// Get task execution history
const history = await automation.getTaskHistory('task-id', {
  from: new Date('2024-01-01'),
  to: new Date(),
});`;

const contractCode = `using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System;
using System.ComponentModel;

namespace AutomatedContract
{
    [DisplayName("AutomatedContract")]
    [ManifestExtra("Author", "Neo")]
    [ManifestExtra("Description", "Contract with automated functions")]
    public class AutomatedContract : SmartContract
    {
        // Event to notify when tokens are distributed
        [DisplayName("Distribution")]
        public static event Action<UInt160, BigInteger> Distribution;

        private static readonly StorageMap Store = new StorageMap(Storage.CurrentContext, "AutomatedContract");

        // Method called by automation service
        public static void DistributeTokens(BigInteger amount)
        {
            // Verify caller is authorized
            if (!Runtime.CheckWitness(GetAutomationAddress()))
                throw new Exception("Unauthorized");

            // Get recipients from storage
            var recipients = (UInt160[])Store.Get("recipients");
            var amountPerRecipient = amount / recipients.Length;

            foreach (var recipient in recipients)
            {
                // Transfer tokens to recipient
                var token = new NEP17Contract(GetTokenHash());
                token.Transfer(Runtime.ExecutingScriptHash, recipient, amountPerRecipient);
                
                // Notify about distribution
                Distribution(recipient, amountPerRecipient);
            }
        }

        // Helper methods
        private static UInt160 GetAutomationAddress()
        {
            return "NXV7J4vhN3Xth4wJEwK1Zb6YgbxboGP3gt".ToScriptHash();
        }

        private static UInt160 GetTokenHash()
        {
            return "NZNos2WqTbu5oCgyfss9kUJgBXJqhuYAaj".ToScriptHash();
        }
    }
}`;

export default function ContractAutomationPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl lg:max-w-4xl">
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 dark:text-white">
            Contract Automation Service
          </h1>
          
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            The Contract Automation service enables automated execution of smart contract methods based on time schedules or blockchain events. 
            Built with reliability and security in mind, it ensures your contracts run exactly when needed.
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
                Initialize the client and create an Automation instance:
              </p>
              <div className="mt-4">
                <CodeBlock code={initCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Creating Automation Tasks
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Create tasks that run on a schedule or in response to blockchain events:
              </p>
              <div className="mt-4">
                <CodeBlock code={createTaskCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Managing Tasks
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                List, update, and monitor your automation tasks:
              </p>
              <div className="mt-4">
                <CodeBlock code={manageTasksCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Smart Contract Example
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Example smart contract designed for automation:
              </p>
              <div className="mt-4">
                <CodeBlock code={contractCode} language="csharp" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Trigger Types
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Time-Based
                  </h3>
                  <p className="mt-2 text-gray-600 dark:text-gray-400">
                    Schedule tasks using cron expressions:
                  </p>
                  <ul className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <li>• Every N minutes: */N * * * *</li>
                    <li>• Daily at specific time: 0 H * * *</li>
                    <li>• Weekly on specific day: 0 0 * * D</li>
                    <li>• Monthly on specific date: 0 0 D * *</li>
                  </ul>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Event-Based
                  </h3>
                  <p className="mt-2 text-gray-600 dark:text-gray-400">
                    Trigger on blockchain events:
                  </p>
                  <ul className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <li>• Contract events (Transfer, Mint, etc.)</li>
                    <li>• Block events (new blocks)</li>
                    <li>• Transaction events</li>
                    <li>• Custom event filters</li>
                  </ul>
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
                    TEE Execution
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    All automation tasks run within a Trusted Execution Environment, 
                    ensuring secure and tamper-proof execution.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Signature Verification
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Each automation request is cryptographically signed and verified 
                    to ensure only authorized updates are processed.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Failure Handling
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Automatic retry mechanism with exponential backoff for failed 
                    executions, with detailed error reporting.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Gas Management
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Integrated with Gas Bank service for automated gas fee management 
                    and optimization.
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