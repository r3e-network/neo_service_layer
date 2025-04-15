/**
 * Neo Service Layer Advanced JavaScript Interoperability Example
 * 
 * This example demonstrates advanced interoperability features including:
 * - Trigger registration
 * - Event handling
 * - Transaction management
 */

/**
 * Main function entry point
 * @param {Object} args - Function arguments
 * @returns {Object} - Function result
 */
function main(args) {
  // Log function start
  context.log('Advanced interoperability example started');
  
  // Extract parameters
  const { action = 'setup', symbol = 'NEO', threshold = 50, recipient } = args;
  
  // Different actions based on input
  switch (action) {
    case 'setup':
      return setupTriggers(symbol, threshold);
    case 'manage':
      return manageTriggers();
    case 'transaction':
      return createTransaction(recipient, 1.0, symbol);
    case 'event':
      return setupEventHandlers();
    default:
      return { error: `Unknown action: ${action}` };
  }
}

/**
 * Set up various triggers for monitoring
 * @param {string} symbol - Asset symbol to monitor
 * @param {number} threshold - Price threshold
 * @returns {Object} - Created trigger IDs
 */
function setupTriggers(symbol, threshold) {
  context.log(`Setting up triggers for ${symbol} with threshold $${threshold}`);
  
  // Create a blockchain event trigger for token transfers
  const transferTrigger = context.trigger.create({
    name: `${symbol} Transfer Monitor`,
    description: `Monitor ${symbol} transfers on the blockchain`,
    type: 'blockchain',
    eventName: 'Transfer',
    contractHash: getContractHashForSymbol(symbol),
    minAmount: 100,
    functionId: 'process-transfer',
    retryCount: 3
  });
  
  context.log(`Created blockchain trigger: ${transferTrigger.triggerId}`);
  
  // Create a time-based trigger for daily price checks
  const timeTrigger = context.trigger.create({
    name: `Daily ${symbol} Price Check`,
    description: `Check ${symbol} price daily at 8:00 AM UTC`,
    type: 'schedule',
    cronExpression: '0 0 8 * * *',
    timezone: 'UTC',
    functionId: 'daily-price-check',
    parameters: {
      symbol: symbol
    }
  });
  
  context.log(`Created time trigger: ${timeTrigger.triggerId}`);
  
  // Create a price condition trigger
  const priceTrigger = context.trigger.create({
    name: `${symbol} Price Alert`,
    description: `Trigger when ${symbol} price crosses $${threshold}`,
    type: 'condition',
    condition: {
      symbol: symbol,
      threshold: threshold,
      operator: 'above'
    },
    functionId: 'price-alert-handler',
    parameters: {
      notificationEmail: 'user@example.com'
    }
  });
  
  context.log(`Created price trigger: ${priceTrigger.triggerId}`);
  
  return {
    success: true,
    triggers: {
      transfer: transferTrigger.triggerId,
      time: timeTrigger.triggerId,
      price: priceTrigger.triggerId
    }
  };
}

/**
 * Manage existing triggers
 * @returns {Object} - Trigger management results
 */
function manageTriggers() {
  context.log('Managing existing triggers');
  
  // List all triggers
  const triggers = context.trigger.list();
  context.log(`Found ${triggers.length} triggers`);
  
  let updated = 0;
  let deleted = 0;
  
  // Update and delete some triggers based on conditions
  triggers.forEach(trigger => {
    if (trigger.type === 'blockchain') {
      // Update blockchain triggers to add a new parameter
      const updateResult = context.trigger.update(trigger.triggerId, {
        description: `Updated: ${trigger.description}`,
        retryCount: 5  // Increase retry count
      });
      
      if (updateResult.success) {
        updated++;
        context.log(`Updated trigger: ${trigger.triggerId}`);
      }
    } else if (trigger.name.includes('Deprecated')) {
      // Delete deprecated triggers
      const deleteResult = context.trigger.delete(trigger.triggerId);
      
      if (deleteResult.success) {
        deleted++;
        context.log(`Deleted trigger: ${trigger.triggerId}`);
      }
    }
  });
  
  return {
    success: true,
    stats: {
      total: triggers.length,
      updated,
      deleted,
      remaining: triggers.length - deleted
    }
  };
}

/**
 * Create and send a transaction
 * @param {string} recipient - Transaction recipient
 * @param {number} amount - Transaction amount
 * @param {string} symbol - Asset symbol
 * @returns {Object} - Transaction result
 */
function createTransaction(recipient, amount, symbol) {
  if (!recipient) {
    return { error: 'Recipient address is required' };
  }
  
  context.log(`Creating transaction: ${amount} ${symbol} to ${recipient}`);
  
  // Get current gas price
  const gasPrice = context.getGasPrice();
  context.log(`Current gas price: ${gasPrice}`);
  
  // Get current token price
  const tokenPrice = context.getPrice(symbol);
  context.log(`Current ${symbol} price: $${tokenPrice}`);
  
  // Create transaction
  const tx = context.transaction.create({
    type: 'transfer',
    asset: symbol,
    from: context.owner,  // Use function owner as sender
    to: recipient,
    amount: amount,
    gasPrice: gasPrice,
    memo: `${symbol} transfer from Neo Service Layer function`
  });
  
  context.log(`Transaction created with ID: ${tx.txId}`);
  
  // Sign transaction
  const signedTx = context.transaction.sign(tx.txId);
  context.log(`Transaction signed: ${signedTx.status}`);
  
  // Send transaction
  const sentTx = context.transaction.send(tx.txId);
  context.log(`Transaction sent with hash: ${sentTx.hash}`);
  
  // Check transaction status
  const txStatus = context.transaction.status(tx.txId);
  
  return {
    success: true,
    transaction: {
      id: tx.txId,
      hash: sentTx.hash,
      status: txStatus.status,
      details: {
        asset: symbol,
        amount: amount,
        recipient: recipient,
        gasPrice: gasPrice,
        value: amount * tokenPrice
      }
    }
  };
}

/**
 * Set up event handlers for various events
 * @returns {Object} - Event handler registration result
 */
function setupEventHandlers() {
  context.log('Setting up event handlers');
  
  // Register blockchain event handler
  const blockchainEvent = context.event.onBlockchain(
    {
      contractHash: getContractHashForSymbol('NEO'),
      eventName: 'Transfer',
      minAmount: 1000
    },
    'large-transfer-handler'
  );
  
  context.log(`Registered blockchain event handler: ${blockchainEvent.eventId}`);
  
  // Register time-based event handler
  const scheduleEvent = context.event.onSchedule(
    '0 0 12 * * MON-FRI',  // Noon on weekdays
    'market-summary-handler'
  );
  
  context.log(`Registered schedule event handler: ${scheduleEvent.eventId}`);
  
  // Register API event handler
  const apiEvent = context.event.onAPI(
    '/api/price-alert',
    'price-alert-handler'
  );
  
  context.log(`Registered API event handler: ${apiEvent.eventId}`);
  
  // Invoke another function to register additional handlers
  const additionalResult = context.invokeFunction('register-additional-handlers', {
    owner: context.owner,
    baseHandlers: [
      blockchainEvent.eventId,
      scheduleEvent.eventId,
      apiEvent.eventId
    ]
  });
  
  return {
    success: true,
    handlers: {
      blockchain: blockchainEvent.eventId,
      schedule: scheduleEvent.eventId,
      api: apiEvent.eventId,
      additional: additionalResult.success ? additionalResult.handlers : []
    }
  };
}

/**
 * Helper function to get contract hash for a symbol
 * @param {string} symbol - Asset symbol
 * @returns {string} - Contract hash
 */
function getContractHashForSymbol(symbol) {
  // In a real implementation, this would be a lookup or API call
  const contractHashes = {
    'NEO': '0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5',
    'GAS': '0xd2a4cff31913016155e38e474a2c06d08be276cf',
    'FLM': '0x4d9eab13620fe3569ba3b0e56e2877739e4145e3'
  };
  
  return contractHashes[symbol] || contractHashes['NEO'];
}

/**
 * Record a transfer (placeholder)
 * @param {string} from - Sender address
 * @param {string} to - Recipient address
 * @param {number} amount - Transfer amount
 * @param {string} asset - Asset symbol
 */
function recordTransfer(from, to, amount, asset) {
  context.log(`Recording transfer: ${amount} ${asset} from ${from} to ${to}`);
  // In a real implementation, this would store the transfer in a database
  return { success: true, recorded: true };
}

/**
 * Store prices (placeholder)
 * @param {Object} prices - Price data
 */
function storePrices(prices) {
  context.log(`Storing prices: ${JSON.stringify(prices)}`);
  // In a real implementation, this would store prices in a database
  return { success: true, stored: Object.keys(prices).length };
}

/**
 * Notify about sell opportunity (placeholder)
 * @param {string} symbol - Asset symbol
 * @param {number} price - Current price
 * @param {number} threshold - Price threshold
 */
function notifySellOpportunity(symbol, price, threshold) {
  context.log(`Sell opportunity: ${symbol} at $${price} (threshold: $${threshold})`);
  // In a real implementation, this would send a notification
  return { success: true, notification: 'sent' };
}

/**
 * Notify about buy opportunity (placeholder)
 * @param {string} symbol - Asset symbol
 * @param {number} price - Current price
 * @param {number} threshold - Price threshold
 */
function notifyBuyOpportunity(symbol, price, threshold) {
  context.log(`Buy opportunity: ${symbol} at $${price} (threshold: $${threshold})`);
  // In a real implementation, this would send a notification
  return { success: true, notification: 'sent' };
}
