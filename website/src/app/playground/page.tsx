'use client';

// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../components/CodeBlock';
import { WalletConnect } from '../../components/WalletConnect';
import { ServiceClient } from '../../lib/serviceClient';

const exampleCode = `// Example: Get NEO/USD price and update contract
const priceFeed = new PriceFeed(client);
const price = await priceFeed.getPrice('NEO/USD');

// Example: Create automation task
const automation = new Automation(client);
await automation.createTask({
  name: 'Price Update',
  contract: contractHash,
  method: 'updatePrice',
  schedule: '*/30 * * * *',
  params: [price],
});

// Example: Create and send a transaction
const transaction = new Transaction(client);
const txConfig = {
  type: 'transfer',
  to: 'NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu',
  amount: '1.0',
  asset: 'GAS'
};

// Create, sign, and send the transaction
const createResult = await transaction.createTransaction(txConfig);
const signResult = await transaction.signTransaction(createResult.id);
const sendResult = await transaction.sendTransaction(signResult.id);

// Check transaction status
const status = await transaction.getTransactionStatus(sendResult.hash);
console.log('Transaction status:', status);`;

const services = [
  {
    name: 'Price Feed',
    description: 'Get real-time price data from trusted sources',
    endpoints: [
      { name: 'NEO/USD', value: 'neo-usd' },
      { name: 'GAS/USD', value: 'gas-usd' },
      { name: 'BTC/USD', value: 'btc-usd' },
      { name: 'ETH/USD', value: 'eth-usd' },
      { name: 'Subscribe', value: 'subscribe' },
      { name: 'Get History', value: 'get-history' },
    ],
  },
  {
    name: 'Contract Automation',
    description: 'Automate smart contract execution with custom triggers',
    endpoints: [
      { name: 'Create Task', value: 'create-task' },
      { name: 'List Tasks', value: 'list-tasks' },
      { name: 'Delete Task', value: 'delete-task' },
      { name: 'Update Task', value: 'update-task' },
      { name: 'Get Task Status', value: 'get-task-status' },
      { name: 'Get Task History', value: 'get-task-history' },
    ],
  },
  {
    name: 'Gas Bank',
    description: 'Automated gas management for smart contracts',
    endpoints: [
      { name: 'Get Balance', value: 'get-balance' },
      { name: 'Top Up', value: 'top-up' },
      { name: 'Withdraw', value: 'withdraw' },
      { name: 'Set Auto-funding', value: 'set-auto-funding' },
      { name: 'Get Auto-funding Config', value: 'get-auto-funding-config' },
      { name: 'Get Transaction History', value: 'get-transaction-history' },
    ],
  },
  {
    name: 'Transaction',
    description: 'Create, sign, and manage blockchain transactions',
    endpoints: [
      { name: 'Create Transaction', value: 'create-transaction' },
      { name: 'Sign Transaction', value: 'sign-transaction' },
      { name: 'Send Transaction', value: 'send-transaction' },
      { name: 'Get Transaction Status', value: 'get-transaction-status' },
      { name: 'Get Transaction', value: 'get-transaction' },
      { name: 'List Transactions', value: 'list-transactions' },
      { name: 'Estimate Fee', value: 'estimate-fee' },
    ],
  },
  {
    name: 'Functions',
    description: 'Deploy and manage serverless functions in TEE',
    endpoints: [
      { name: 'Deploy Function', value: 'deploy-function' },
      { name: 'List Functions', value: 'list-functions' },
      { name: 'Delete Function', value: 'delete-function' },
      { name: 'Update Function', value: 'update-function' },
      { name: 'Get Logs', value: 'get-logs' },
      { name: 'Get Metrics', value: 'get-metrics' },
    ],
  },
  {
    name: 'Secrets',
    description: 'Secure secrets management with TEE protection',
    endpoints: [
      { name: 'Set Secret', value: 'set-secret' },
      { name: 'Get Secret', value: 'get-secret' },
      { name: 'Delete Secret', value: 'delete-secret' },
      { name: 'List Secrets', value: 'list-secrets' },
      { name: 'Rotate Secret', value: 'rotate-secret' },
      { name: 'Get Access History', value: 'get-access-history' },
    ],
  },
  {
    name: 'Trigger Service',
    description: 'Monitor and react to blockchain events',
    endpoints: [
      { name: 'Create Trigger', value: 'create-trigger' },
      { name: 'List Triggers', value: 'list-triggers' },
      { name: 'Delete Trigger', value: 'delete-trigger' },
      { name: 'Update Trigger', value: 'update-trigger' },
      { name: 'Get Trigger Status', value: 'get-trigger-status' },
      { name: 'Get Event History', value: 'get-event-history' },
    ],
  },
  {
    name: 'Metrics',
    description: 'Monitor service performance and usage',
    endpoints: [
      { name: 'Get Service Metrics', value: 'get-service-metrics' },
      { name: 'Get Contract Metrics', value: 'get-contract-metrics' },
      { name: 'Get Function Metrics', value: 'get-function-metrics' },
      { name: 'Get Usage Stats', value: 'get-usage-stats' },
      { name: 'Get Alerts', value: 'get-alerts' },
      { name: 'Configure Alerts', value: 'configure-alerts' },
    ],
  },
  {
    name: 'Logging',
    description: 'Centralized logging for all services',
    endpoints: [
      { name: 'Get Service Logs', value: 'get-service-logs' },
      { name: 'Get Contract Logs', value: 'get-contract-logs' },
      { name: 'Get Function Logs', value: 'get-function-logs' },
      { name: 'Search Logs', value: 'search-logs' },
      { name: 'Export Logs', value: 'export-logs' },
      { name: 'Configure Log Retention', value: 'configure-log-retention' },
    ],
  },
];

export default function PlaygroundPage() {
  const [selectedService, setSelectedService] = React.useState('');
  const [selectedEndpoint, setSelectedEndpoint] = React.useState('');
  const [response, setResponse] = React.useState('');
  const [loading, setLoading] = React.useState(false);
  const [client, setClient] = React.useState<ServiceClient | null>(null);
  const [requestParams, setRequestParams] = React.useState<Record<string, any>>({});

  // Initialize service client when wallet is connected
  React.useEffect(() => {
    if (typeof window !== 'undefined' && window.neo3Wallet) {
      const newClient = new ServiceClient({
        signMessage: async (message: string) => {
          if (!window.neo3Wallet) {
            throw new Error('Neo wallet not available');
          }
          return await window.neo3Wallet.signMessage(message);
        },
      });
      setClient(newClient);
    }
  }, []);

  // Update example code based on selected service and endpoint
  const getExampleCode = React.useCallback(() => {
    if (!selectedService || !selectedEndpoint) return exampleCode;

    const examples: Record<string, string> = {
      'neo-usd': `// Get NEO/USD price
const priceFeed = new PriceFeed(client);
const price = await priceFeed.getPrice('NEO/USD');
console.log('Current NEO price:', price);`,

      'create-task': `// Create automation task
const automation = new Automation(client);
await automation.createTask({
  name: 'Price Update',
  contract: 'YOUR_CONTRACT_HASH',
  method: 'updatePrice',
  schedule: '*/30 * * * *',
  params: ['NEO/USD'],
});`,

      'set-auto-funding': `// Configure Gas Bank auto-funding
const gasBank = new GasBank(client);
await gasBank.configureAutoFunding({
  threshold: 100, // GAS
  targetBalance: 500,
  maxGasPerTransaction: 10,
});`,

      'deploy-function': `// Deploy serverless function
const functions = new Functions(client);
await functions.deploy({
  name: 'price-alert',
  source: \`
    export async function checkPrice(params) {
      const price = await priceFeed.getPrice(params.symbol);
      if (price > params.threshold) {
        await sendNotification(params.webhook, {
          symbol: params.symbol,
          price: price,
        });
      }
    }
  \`,
  runtime: 'node16',
  memory: 256,
  timeout: 30,
});`,

      'set-secret': `// Store and rotate secret
const secrets = new Secrets(client);
await secrets.setSecret('api_key', 'your-api-key-here', {
  description: 'External API key',
  expiration: '30d',
  rotationSchedule: '7d',
});`,

      'create-trigger': `// Create blockchain event trigger
const triggers = new Triggers(client);
await triggers.create({
  name: 'token-transfer',
  contract: 'TOKEN_CONTRACT_HASH',
  event: 'Transfer',
  action: {
    type: 'function',
    name: 'process-transfer',
    params: ['from', 'to', 'amount'],
  },
});`,

      'create-transaction': `// Create transaction
const transaction = new Transaction(client);
const txConfig = {
  type: 'transfer',
  to: 'NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu',
  amount: '1.0',
  asset: 'GAS'
};
const createResult = await transaction.createTransaction(txConfig);
console.log('Transaction created:', createResult);`,

      'sign-transaction': `// Sign transaction
const transaction = new Transaction(client);
const txConfig = {
  type: 'transfer',
  to: 'NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu',
  amount: '1.0',
  asset: 'GAS'
};
const createResult = await transaction.createTransaction(txConfig);
const signResult = await transaction.signTransaction(createResult.id);
console.log('Transaction signed:', signResult);`,

      'send-transaction': `// Send transaction
const transaction = new Transaction(client);
const txConfig = {
  type: 'transfer',
  to: 'NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu',
  amount: '1.0',
  asset: 'GAS'
};
const createResult = await transaction.createTransaction(txConfig);
const signResult = await transaction.signTransaction(createResult.id);
const sendResult = await transaction.sendTransaction(signResult.id);
console.log('Transaction sent:', sendResult);`,

      'get-transaction-status': `// Get transaction status
const transaction = new Transaction(client);
const status = await transaction.getTransactionStatus('0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef');
console.log('Transaction status:', status);`,

      'get-transaction': `// Get transaction details
const transaction = new Transaction(client);
const txDetails = await transaction.getTransaction('tx-12345-67890');
console.log('Transaction details:', txDetails);`,

      'list-transactions': `// List transactions
const transaction = new Transaction(client);
const transactions = await transaction.listTransactions({
  page: 1,
  pageSize: 10,
  status: 'confirmed'
});
console.log('Transactions:', transactions);`,

      'estimate-fee': `// Estimate transaction fee
const transaction = new Transaction(client);
const feeEstimate = await transaction.estimateFee({
  type: 'transfer',
  to: 'NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu',
  amount: '1.0',
  asset: 'GAS'
});
console.log('Estimated fee:', feeEstimate);`,
    };

    return examples[selectedEndpoint] || exampleCode;
  }, [selectedService, selectedEndpoint]);

  // Get parameter template based on endpoint
  const getParamTemplate = React.useCallback(() => {
    const templates: Record<string, Record<string, any>> = {
      'neo-usd': {},
      'gas-usd': {},
      'btc-usd': {},
      'eth-usd': {},
      'subscribe': {
        symbol: 'NEO/USD',
        interval: '1h',
      },
      'get-history': {
        symbol: 'NEO/USD',
        from: '2023-01-01',
        to: '2023-01-31',
      },
      'create-task': {
        name: 'Price Update Task',
        contract: '',
        method: 'updatePrice',
        schedule: '*/30 * * * *',
        params: [],
      },
      'set-auto-funding': {
        threshold: 100,
        targetBalance: 500,
        maxGasPerTransaction: 10,
      },
      'deploy-function': {
        name: '',
        source: '',
        runtime: 'node16',
        memory: 256,
        timeout: 30,
      },
      'set-secret': {
        key: '',
        value: '',
        description: '',
        expiration: '30d',
        rotationSchedule: '7d',
      },
      'create-trigger': {
        name: '',
        contract: '',
        event: '',
        action: {
          type: 'function',
          name: '',
          params: [],
        },
      },
      'create-transaction': {
        type: 'transfer',
        to: '',
        amount: '',
        asset: 'GAS',
      },
      'sign-transaction': {
        id: '',
      },
      'send-transaction': {
        id: '',
      },
      'get-transaction-status': {
        hash: '',
      },
      'get-transaction': {
        id: '',
      },
      'list-transactions': {
        page: 1,
        pageSize: 10,
        status: '',
      },
      'estimate-fee': {
        type: 'transfer',
        to: '',
        amount: '',
        asset: 'GAS',
      },
    };

    return templates[selectedEndpoint] || {};
  }, [selectedEndpoint]);

  React.useEffect(() => {
    if (selectedEndpoint) {
      setRequestParams(getParamTemplate());
    }
  }, [selectedEndpoint, getParamTemplate]);

  const handleParamChange = (key: string, value: any) => {
    setRequestParams((prev) => ({
      ...prev,
      [key]: value,
    }));
  };

  const handleExecute = async () => {
    if (!selectedService || !selectedEndpoint || !client) {
      setResponse('Please connect wallet and select a service and endpoint');
      return;
    }

    setLoading(true);
    try {
      const result = await client.execute(
        selectedService.toLowerCase().replace(' ', '-'),
        selectedEndpoint,
        requestParams
      );
      setResponse(JSON.stringify(result, null, 2));
    } catch (error) {
      setResponse(JSON.stringify({
        status: 'error',
        message: error instanceof Error ? error.message : 'Unknown error',
      }, null, 2));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Service Layer Playground
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Test our services interactively. Connect your wallet to get started.
          </p>
          <div className="mt-6">
            <WalletConnect onConnect={setClient} />
          </div>
        </div>

        <div className="mt-16">
          <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
            {services.map((service) => (
              <div
                key={service.name}
                className={`rounded-2xl p-8 ${
                  selectedService === service.name
                    ? 'bg-blue-50 dark:bg-blue-900/20 ring-2 ring-blue-500'
                    : 'bg-white dark:bg-gray-800'
                } cursor-pointer transition-all duration-200 hover:ring-2 hover:ring-blue-500/50`}
                onClick={() => setSelectedService(service.name)}
              >
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                  {service.name}
                </h3>
                <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
                  {service.description}
                </p>
                <div className="mt-4 space-y-2">
                  {service.endpoints.map((endpoint) => (
                    <button
                      key={endpoint.value}
                      className={`block w-full rounded-lg px-4 py-2 text-left text-sm ${
                        selectedEndpoint === endpoint.value
                          ? 'bg-blue-500 text-white'
                          : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                      }`}
                      onClick={(e) => {
                        e.stopPropagation();
                        setSelectedEndpoint(endpoint.value);
                      }}
                    >
                      {endpoint.name}
                    </button>
                  ))}
                </div>
              </div>
            ))}
          </div>

          {selectedEndpoint && Object.keys(requestParams).length > 0 && (
            <div className="mt-8">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                Request Parameters
              </h3>
              <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
                {Object.entries(requestParams).map(([key, value]) => (
                  <div key={key} className="rounded-lg bg-white dark:bg-gray-800 p-4">
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                      {key}
                    </label>
                    <input
                      type="text"
                      value={typeof value === 'object' ? JSON.stringify(value) : String(value || '')}
                      onChange={(e) => handleParamChange(key, e.target.value)}
                      className="mt-1 block w-full rounded-md border-gray-300 dark:border-gray-600 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm dark:bg-gray-700 dark:text-white"
                    />
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="mt-12">
            <div className="rounded-xl bg-gray-50 dark:bg-gray-800/50 p-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                Example Code
              </h3>
              <div className="mt-4">
                <CodeBlock code={getExampleCode()} language="typescript" />
              </div>
            </div>
          </div>

          <div className="mt-8 flex justify-center">
            <button
              onClick={handleExecute}
              disabled={loading || !selectedService || !selectedEndpoint || !client}
              className={`rounded-full px-8 py-3 text-base font-semibold text-white shadow-sm transition-all duration-200 ${
                loading || !selectedService || !selectedEndpoint || !client
                  ? 'bg-gray-400 cursor-not-allowed'
                  : 'bg-blue-600 hover:bg-blue-500'
              }`}
            >
              {loading ? 'Executing...' : 'Execute Request'}
            </button>
          </div>

          {response && (
            <div className="mt-8">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                Response
              </h3>
              <div className="mt-4">
                <CodeBlock code={response} language="json" />
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}