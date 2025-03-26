/**
 * Token Transfer Monitor Function
 * 
 * This function monitors NEP-17 token transfers and demonstrates
 * contract event monitoring and automation capabilities.
 */

const { Neo, GasBank, Secrets, Logger } = require('@neo-sl/sdk');

// Configuration
const CONFIG = {
  TOKEN_SCRIPT_HASH: '0x43cf4863eb7d1148a0b7d833a4acd2e432748334', // Example NEP-17 token
  MIN_TRANSFER_AMOUNT: '1000000000', // 10 tokens (assuming 8 decimals)
  GAS_AMOUNT: '1000000' // 0.01 GAS for contract calls
};

/**
 * Main function handler
 * @param {Object} context - Execution context
 * @param {Object} event - Contract event data
 */
async function handler(context, event) {
  const logger = new Logger(context);
  
  try {
    // Initialize Neo client
    const neo = new Neo(context);
    
    // Get gas allocation for contract calls
    const gasBank = new GasBank(context);
    const gasAllocation = await gasBank.allocate(CONFIG.GAS_AMOUNT);
    
    logger.info('Received transfer event', { event });
    
    // Parse transfer event
    const { from, to, amount } = event.params;
    
    // Check if transfer amount exceeds threshold
    if (BigInt(amount) >= BigInt(CONFIG.MIN_TRANSFER_AMOUNT)) {
      // Get notification settings from secrets
      const secrets = new Secrets(context);
      const alertEmail = await secrets.get('ALERT_EMAIL');
      
      // Get token details
      const tokenContract = neo.getContract(CONFIG.TOKEN_SCRIPT_HASH);
      const symbol = await tokenContract.call('symbol');
      const decimals = await tokenContract.call('decimals');
      
      // Format amount with proper decimals
      const formattedAmount = BigInt(amount) / BigInt(10 ** decimals);
      
      // Prepare notification
      const notification = {
        type: 'large_transfer',
        token: {
          symbol,
          scriptHash: CONFIG.TOKEN_SCRIPT_HASH
        },
        transfer: {
          from,
          to,
          amount: formattedAmount.toString()
        },
        timestamp: new Date().toISOString()
      };
      
      // Send notification
      await sendEmailNotification(alertEmail, notification);
      
      logger.info('Large transfer notification sent', { notification });
    }
    
    // Release gas allocation
    await gasBank.release(gasAllocation.id);
    
  } catch (error) {
    logger.error('Error in token transfer monitor', { error });
    throw error;
  }
}

/**
 * Send email notification
 * @param {string} email - Recipient email
 * @param {Object} data - Notification data
 */
async function sendEmailNotification(email, data) {
  // Implementation would depend on your email service integration
  // This is a placeholder for demonstration
  console.log(`[Email to ${email}]`, JSON.stringify(data, null, 2));
}

module.exports = {
  handler,
  config: {
    name: 'token-transfer-monitor',
    description: 'Monitors large NEP-17 token transfers',
    runtime: 'node16',
    trigger: {
      type: 'contract_event',
      contract: CONFIG.TOKEN_SCRIPT_HASH,
      event: 'Transfer'
    },
    resources: {
      memory: 256,
      cpu: 0.2
    },
    permissions: [
      'neo:read',
      'gas-bank:allocate',
      'gas-bank:release',
      'secrets:read'
    ]
  }
};