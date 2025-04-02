// Example code snippet for the playground
export const exampleCode = `// Default example - Select a service and endpoint`;

/**
 * Example code snippets for specific endpoints.
 * Keys MUST match the 'value' property of endpoints in the 'services' array below.
 */
export const exampleSnippets: Record<string, string> = {
  // Price Feed
  'neo-usd': `// Get NEO/USD price
const priceFeed = new PriceFeed(client);
const price = await priceFeed.getPrice('NEO/USD');
console.log('Current NEO price:', price);`, 
  'gas-usd': `// Get GAS/USD price
const priceFeed = new PriceFeed(client);
const price = await priceFeed.getPrice('GAS/USD');
console.log('Current GAS price:', price);`, 
  'btc-usd': `// Get BTC/USD price
const priceFeed = new PriceFeed(client);
const price = await priceFeed.getPrice('BTC/USD');
console.log('Current BTC price:', price);`, 
  'eth-usd': `// Get ETH/USD price
const priceFeed = new PriceFeed(client);
const price = await priceFeed.getPrice('ETH/USD');
console.log('Current ETH price:', price);`, 
  'subscribe': `// Subscribe to price updates
const priceFeed = new PriceFeed(client);
priceFeed.subscribe('NEO/USD', (update) => {
  console.log('Price Update:', update);
});`, 
  'get-history': `// Get historical price data
const priceFeed = new PriceFeed(client);
const history = await priceFeed.getHistory('NEO/USD', { from: '2023-01-01', to: '2023-01-31' });
console.log('Historical Data:', history);`,
  // Contract Automation
  'create-task': `// Create automation task
const automation = new Automation(client);
await automation.createTask({
  name: 'My Task',
  contract: 'YOUR_CONTRACT_HASH',
  method: 'myMethod',
  schedule: '0 * * * *' /* Hourly */,
  params: [],
});
console.log('Task created!');`, 
  'list-tasks': `// List automation tasks
const automation = new Automation(client);
const tasks = await automation.listTasks();
console.log('Tasks:', tasks);`, 
  'delete-task': `// Delete an automation task
const automation = new Automation(client);
await automation.deleteTask('YOUR_TASK_ID');
console.log('Task deleted');`, 
  'update-task': `// Update an automation task schedule
const automation = new Automation(client);
await automation.updateTask('YOUR_TASK_ID', {
  schedule: '0 0 * * *' /* Daily */,
});
console.log('Task updated');`, 
  'get-task-status': `// Get task status
const automation = new Automation(client);
const status = await automation.getTaskStatus('YOUR_TASK_ID');
console.log('Task Status:', status);`, 
  'get-task-history': `// Get task execution history
const automation = new Automation(client);
const history = await automation.getTaskHistory('YOUR_TASK_ID', { limit: 10 });
console.log('Task History:', history);`,
  // Gas Bank
  'get-balance': `// Get Gas Bank balance
const gasBank = new GasBank(client);
const balance = await gasBank.getBalance();
console.log('Gas Bank Balance:', balance);`, 
  'top-up': `// Top up Gas Bank - Requires a separate transaction
console.log('Please send GAS to your Gas Bank address.');
// Example using Transaction service:
// const tx = new Transaction(client);
// await tx.createTransaction({ type: 'transfer', to: 'GAS_BANK_ADDRESS', amount: '100', asset: 'GAS' });`, 
  'withdraw': `// Withdraw from Gas Bank
const gasBank = new GasBank(client);
await gasBank.withdraw(100 /* Amount */, 'DESTINATION_ADDRESS');
console.log('Withdraw initiated.');`, 
  'set-auto-funding': `// Configure Gas Bank auto-funding
const gasBank = new GasBank(client);
await gasBank.configureAutoFunding({
  threshold: 100, // GAS
  targetBalance: 500,
  maxGasPerTransaction: 10,
});`, 
  'get-auto-funding-config': `// Get auto-funding config
const gasBank = new GasBank(client);
const config = await gasBank.getAutoFundingConfig();
console.log('Auto-funding Config:', config);`, 
  'get-transaction-history': `// Get Gas Bank transaction history
const gasBank = new GasBank(client);
const history = await gasBank.getTransactionHistory({ limit: 20 });
console.log('Gas Bank History:', history);`,
  // Transaction
  'create-transaction': `// Create transaction
const transaction = new Transaction(client);
const txConfig = {
  type: 'transfer',
  to: 'NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu',
  amount: '1.0',
  asset: 'GAS'
};
const result = await transaction.createTransaction(txConfig);
console.log('Created Tx ID:', result.id);`, 
  'sign-transaction': `// Sign transaction
const transaction = new Transaction(client);
const result = await transaction.signTransaction('YOUR_TRANSACTION_ID');
console.log('Signed Tx:', result);`, 
  'send-transaction': `// Send transaction
const transaction = new Transaction(client);
const result = await transaction.sendTransaction('YOUR_TRANSACTION_ID');
console.log('Sent Tx Hash:', result.hash);`, 
  'get-transaction-status': `// Get transaction status
const transaction = new Transaction(client);
const status = await transaction.getTransactionStatus('YOUR_TX_HASH');
console.log('Tx Status:', status);`, 
  'get-transaction': `// Get transaction details
const transaction = new Transaction(client);
const details = await transaction.getTransaction('YOUR_TRANSACTION_ID');
console.log('Transaction Details:', details);`, 
  'list-transactions': `// List transactions
const transaction = new Transaction(client);
const txList = await transaction.listTransactions({ page: 1, pageSize: 5 });
console.log('Transactions:', txList);`, 
  'estimate-fee': `// Estimate transaction fee
const transaction = new Transaction(client);
const feeEstimate = await transaction.estimateFee({
  type: 'transfer',
  to: 'NXV7ZhHxqYnP3E4PuG4aYqPKqhP7NQ3cYu',
  amount: '1.0',
  asset: 'GAS'
});
console.log('Estimated fee:', feeEstimate);`,
  // Functions
  'deploy-function': `// Deploy serverless function
const functions = new Functions(client);
await functions.deploy({
  name: 'price-alert',
  source: \`
    export async function checkPrice(params) { 
      // Your function logic here...
      console.log('Checking price for:', params.symbol);
      return { success: true }; 
    } 
  \`,
  runtime: 'node16',
});`, 
  'list-functions': `// List deployed functions
const functions = new Functions(client);
const funcList = await functions.listFunctions();
console.log('Functions:', funcList);`, 
  'delete-function': `// Delete function
const functions = new Functions(client);
await functions.deleteFunction('YOUR_FUNCTION_NAME');
console.log('Function deleted');`, 
  'update-function': `// Update function source code
const functions = new Functions(client);
await functions.updateFunction('YOUR_FUNCTION_NAME', {
  source: \` // New source code... \`
});`, 
  'get-logs': `// Get function execution logs
const functions = new Functions(client);
const logs = await functions.getLogs('YOUR_FUNCTION_NAME', { limit: 50 });
console.log('Function Logs:', logs);`, 
  'get-metrics': `// Get function metrics
const functions = new Functions(client);
const metrics = await functions.getMetrics('YOUR_FUNCTION_NAME');
console.log('Function Metrics:', metrics);`,
  // Secrets
  'set-secret': `// Store a secret
const secrets = new Secrets(client);
await secrets.setSecret('myApiKey', 'value123', { description: 'API key' });`, 
  'get-secret': `// Get secret (only metadata, not value)
const secrets = new Secrets(client);
const secretInfo = await secrets.getSecret('myApiKey');
console.log('Secret Info:', secretInfo);`, 
  'delete-secret': `// Delete secret
const secrets = new Secrets(client);
await secrets.deleteSecret('myApiKey');
console.log('Secret deleted');`, 
  'list-secrets': `// List secrets
const secrets = new Secrets(client);
const secretList = await secrets.listSecrets();
console.log('Secrets:', secretList);`, 
  'rotate-secret': `// Rotate secret value (use setSecret with the same key)
const secrets = new Secrets(client);
await secrets.setSecret('myApiKey', 'newSecretValue456'); 
console.log('Secret value updated/rotated');`, 
  'get-access-history': `// Get secret access history
const secrets = new Secrets(client);
const history = await secrets.getAccessHistory('myApiKey');
console.log('Access History:', history);`,
  // Trigger Service
  'create-trigger': `// Create blockchain event trigger
const triggers = new Triggers(client);
await triggers.create({
  name: 'NFT Transfer Trigger',
  contract: 'NFT_CONTRACT_HASH',
  event: 'Transfer',
  action: { type: 'function', name: 'processNftTransfer' }
});`, 
  'list-triggers': `// List triggers
const triggers = new Triggers(client);
const triggerList = await triggers.listTriggers();
console.log('Triggers:', triggerList);`, 
  'delete-trigger': `// Delete trigger
const triggers = new Triggers(client);
await triggers.deleteTrigger('YOUR_TRIGGER_ID');
console.log('Trigger deleted');`, 
  'update-trigger': `// Update trigger (e.g., change action function)
const triggers = new Triggers(client);
await triggers.updateTrigger('YOUR_TRIGGER_ID', {
  action: { type: 'function', name: 'newProcessFunction' }
});`, 
  'get-trigger-status': `// Get trigger status
const triggers = new Triggers(client);
const status = await triggers.getTriggerStatus('YOUR_TRIGGER_ID');
console.log('Trigger Status:', status);`, 
  'get-event-history': `// Get trigger event history
const triggers = new Triggers(client);
const history = await triggers.getEventHistory('YOUR_TRIGGER_ID');
console.log('Event History:', history);`,
  // Metrics
  'get-service-metrics': `// Get overall service metrics
const metrics = new Metrics(client);
const serviceMetrics = await metrics.getServiceMetrics();
console.log('Service Metrics:', serviceMetrics);`,
  'get-contract-metrics': `// Get metrics for a specific contract
const metrics = new Metrics(client);
const contractMetrics = await metrics.getContractMetrics('YOUR_CONTRACT_HASH');
console.log('Contract Metrics:', contractMetrics);`,
  'get-function-metrics': `// Get metrics for a specific function
const metrics = new Metrics(client);
const functionMetrics = await metrics.getFunctionMetrics('YOUR_FUNCTION_NAME');
console.log('Function Metrics:', functionMetrics);`,
  'get-usage-stats': `// Get API usage statistics
const metrics = new Metrics(client);
const usage = await metrics.getUsageStats();
console.log('Usage Stats:', usage);`,
  'get-alerts': `// Get configured alerts
const metrics = new Metrics(client);
const alerts = await metrics.getAlerts();
console.log('Alerts:', alerts);`,
  'configure-alerts': `// Configure a new alert
const metrics = new Metrics(client);
await metrics.configureAlerts({
  type: 'gas_balance_low',
  threshold: 50, 
  notificationChannel: 'webhook_url'
});`,
  // Logging
  'get-service-logs': `// Get general service logs
const logging = new Logging(client);
const logs = await logging.getServiceLogs({ limit: 100 });
console.log('Service Logs:', logs);`,
  'get-contract-logs': `// Get logs related to a contract
const logging = new Logging(client);
const logs = await logging.getContractLogs('YOUR_CONTRACT_HASH');
console.log('Contract Logs:', logs);`,
  'get-function-logs': `// Get logs for a specific function
const logging = new Logging(client);
const logs = await logging.getFunctionLogs('YOUR_FUNCTION_NAME');
console.log('Function Logs:', logs);`,
  'search-logs': `// Search logs with a query
const logging = new Logging(client);
const results = await logging.searchLogs({ query: 'error', timeRange: '1h' });
console.log('Search Results:', results);`,
  'export-logs': `// Export logs (API might return a link or job ID)
const logging = new Logging(client);
const exportJob = await logging.exportLogs({ format: 'json' });
console.log('Log Export Job:', exportJob);`,
  'configure-log-retention': `// Configure log retention period
const logging = new Logging(client);
await logging.configureLogRetention({ period: '30d' });`,
  // TEE
  'deploy-tee-function': `// Deploy function specifically to TEE
const tee = new TEE(client);
await tee.deployTEEFunction({
  name: 'confidential-calc',
  source: \` /* TEE specific code */ \`
});`,
  'list-tee-functions': `// List functions running in TEE
const tee = new TEE(client);
const teeFuncs = await tee.listTEEFunctions();
console.log('TEE Functions:', teeFuncs);`,
  'verify-tee': `// Verify TEE integrity (Attestation check)
const tee = new TEE(client);
const verificationResult = await tee.verifyTEE();
console.log('TEE Verification:', verificationResult);`,
  'get-tee-attestation': `// Get TEE attestation report
const tee = new TEE(client);
const attestation = await tee.getTEEAttestation();
console.log('TEE Attestation:', attestation);`,
  'execute-confidential': `// Execute computation within TEE
const tee = new TEE(client);
const result = await tee.executeConfidential('confidential-calc', { inputData: '...' });
console.log('Confidential Result:', result);`,
  'get-tee-status': `// Get TEE node status
const tee = new TEE(client);
const status = await tee.getTEEStatus();
console.log('TEE Status:', status);`,
};

/**
 * Detailed service information for the Neo Service Layer
 */
export const serviceDocumentation = {
  'Price Feed': `The Price Feed service provides real-time price data for various crypto assets sourced from multiple reliable oracles. 
  Data is verified through a network of trusted nodes and published on-chain for smart contracts to access. 
  Features include historical price data, WebSocket subscriptions for real-time updates, and customizable aggregation methods.`,
  
  'Contract Automation': `The Contract Automation service enables scheduled execution of smart contract methods without manual intervention. 
  Similar to Chainlink Keepers, it monitors conditions and triggers actions when criteria are met. 
  Tasks can be scheduled using CRON expressions or triggered by blockchain events. All executions are cryptographically verifiable.`,
  
  'Gas Bank': `The Gas Bank service manages GAS tokens for your smart contracts, ensuring they always have sufficient resources to execute. 
  It provides auto-funding capabilities with customizable thresholds, detailed transaction history, and withdrawal options. 
  Ideal for production dApps that need reliable resource management without manual intervention.`,
  
  'Transaction': `The Transaction service simplifies blockchain interactions by handling the creation, signing, and broadcast of transactions. 
  It supports various transaction types including transfers, contract invocations, and multi-signature operations. 
  Features include fee estimation, transaction status tracking, and batched operations for efficiency.`,
  
  'Functions': `The Functions service allows developers to deploy and execute serverless functions within a Trusted Execution Environment (TEE). 
  These functions can access off-chain data securely while maintaining confidentiality. 
  Ideal for complex off-chain computations that need to interact with on-chain contracts.`,
  
  'Secrets': `The Secrets service provides secure storage and management of sensitive information like API keys and private credentials. 
  All secrets are encrypted and stored in a TEE, inaccessible even to service operators. 
  Features include automatic secret rotation, access control, and audit logs for compliance.`,
  
  'Trigger Service': `The Trigger Service monitors blockchain events and executes predefined actions when specific conditions are met. 
  It can listen for smart contract events, transactions, or block production and respond with function calls, notifications, or contract invocations. 
  Supports complex event filters and conditional execution logic.`,
  
  'Metrics': `The Metrics service provides detailed insights into service usage, performance, and health. 
  It offers real-time monitoring of contract execution, gas consumption, and function reliability. 
  Features include customizable alerts, dashboard visualization, and historical data analysis.`,
  
  'Logging': `The Logging service centralizes log collection from all Neo Service Layer components. 
  It provides searchable, exportable logs with various retention options. 
  Features include structured logging, severity filtering, and integration with common log analysis tools.`,
  
  'TEE': `The Trusted Execution Environment (TEE) service provides a secure, isolated computing environment for confidential operations. 
  It enables secure off-chain computations with cryptographic guarantees that even service operators cannot access your data. 
  All operations include verifiable attestations to prove their integrity.`
};

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