// @ts-ignore
import * as React from 'react';
import { CodeBracketIcon, LockClosedIcon, BoltIcon, ArrowsPointingOutIcon } from '@heroicons/react/24/outline';

export default function FunctionsPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <div className="inline-flex items-center justify-center p-3 rounded-full bg-indigo-100 dark:bg-indigo-900 mb-6">
            <CodeBracketIcon className="h-8 w-8 text-indigo-600 dark:text-indigo-400" />
          </div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Functions Service
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Serverless functions that run in a secure trusted execution environment.
          </p>
        </div>
        
        <div className="mt-16 flow-root">
          <div className="mt-10 space-y-8 border-t border-gray-200 dark:border-gray-700 pt-10 sm:mt-16 sm:pt-16">
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Key Features</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <LockClosedIcon className="h-6 w-6 text-indigo-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Secure Runtime</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    All functions execute within Trusted Execution Environments (TEEs) that provide 
                    hardware-level isolation and attestation, protecting your code and data from service operators.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <CodeBracketIcon className="h-6 w-6 text-indigo-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Custom Logic</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Write functions in JavaScript/TypeScript to implement custom business logic that interacts with 
                    blockchain data, external APIs, or other services in the Neo ecosystem.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <BoltIcon className="h-6 w-6 text-indigo-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Event Triggers</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Functions can be triggered by a variety of events: HTTP requests, blockchain transactions, 
                    scheduled intervals, or in response to other services' events.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <ArrowsPointingOutIcon className="h-6 w-6 text-indigo-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Scalable Execution</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Functions scale automatically to handle varying workloads, with no infrastructure management needed.
                    Pay only for the compute time your functions use.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Function Examples</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 space-y-8">
                <div>
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">HTTP-Triggered Function</h3>
                  <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
                    <pre className="text-sm overflow-x-auto">
                      <code className="language-javascript">
{`// An HTTP API endpoint that fetches on-chain data
export async function handleRequest(req, res) {
  // Validate the request
  const { address } = req.query;
  if (!address) {
    return res.status(400).json({ error: "Address parameter is required" });
  }
  
  // Use SDK to query blockchain
  const balances = await neo.blockchain.getTokenBalances(address);
  
  // Process the data
  const formattedBalances = balances.map(b => ({
    token: b.symbol,
    amount: neo.utils.formatTokenAmount(b.amount, b.decimals),
    usdValue: b.usdPrice ? neo.utils.formatUSD(b.amount * b.usdPrice / Math.pow(10, b.decimals)) : null
  }));
  
  // Return the response
  return res.json({
    address,
    balances: formattedBalances,
    timestamp: new Date().toISOString()
  });
}`}
                      </code>
                    </pre>
                  </div>
                </div>
                
                <div>
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Blockchain-Triggered Function</h3>
                  <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
                    <pre className="text-sm overflow-x-auto">
                      <code className="language-javascript">
{`// A function that reacts to specific blockchain events
export async function processTokenTransfer(event) {
  // Event contains transaction details
  const { txId, contract, from, to, amount, symbol } = event;
  
  // Access a secret (e.g., API key) securely within TEE
  const apiKey = await neo.services.secrets.get("notification-service-api-key");
  
  // Call external API securely
  if (amount > 10000) {
    await notifyLargeTransfer(apiKey, {
      txId,
      from,
      to,
      amount,
      symbol,
      timestamp: new Date().toISOString()
    });
  }
  
  // Log event for analytics
  await neo.services.logging.log({
    level: "info",
    message: \`Transfer of \${amount} \${symbol} from \${from} to \${to}\`,
    metadata: { txId, contract }
  });
  
  return { processed: true };
}`}
                      </code>
                    </pre>
                  </div>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Getting Started</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                <ol className="list-decimal list-inside space-y-4 text-gray-600 dark:text-gray-400">
                  <li>
                    <strong>Create a function</strong>: Write your function code and define its trigger type (HTTP, blockchain event, schedule, etc.)
                  </li>
                  <li>
                    <strong>Test locally</strong>: Use our emulator to test your function in a local environment
                  </li>
                  <li>
                    <strong>Deploy</strong>: Publish your function to our secure TEE infrastructure
                  </li>
                  <li>
                    <strong>Monitor</strong>: Track invocations, performance, and logs through our dashboard
                  </li>
                </ol>
                
                <div className="mt-6 p-4 bg-indigo-50 dark:bg-indigo-900/30 rounded-lg">
                  <p className="text-sm text-indigo-700 dark:text-indigo-300">
                    <strong>Tip:</strong> Functions can access all other Neo Service Layer services securely, 
                    allowing you to build complex workflows that leverage price feeds, gas bank, automation, and more.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
} 