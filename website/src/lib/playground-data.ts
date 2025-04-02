// Example code snippet for the playground
export const exampleCode = `// Example: Get NEO/USD price and update contract
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

// Service definitions for the playground
export const services = [
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
  {
    name: 'TEE',
    description: 'Trusted Execution Environment for secure computation',
    endpoints: [
      { name: 'Deploy TEE Function', value: 'deploy-tee-function' },
      { name: 'List TEE Functions', value: 'list-tee-functions' },
      { name: 'Verify TEE', value: 'verify-tee' },
      { name: 'Get TEE Attestation', value: 'get-tee-attestation' },
      { name: 'Execute Confidential Computation', value: 'execute-confidential' },
      { name: 'Get TEE Status', value: 'get-tee-status' },
    ],
  },
]; 