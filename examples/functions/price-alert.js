/**
 * Price Alert Function
 * 
 * This function monitors NEO/GAS price and sends alerts when thresholds are crossed.
 * It demonstrates integration with the price feed service and secrets management.
 */

// Import required Neo N3 Service Layer SDK
const { PriceFeed, Secrets, Logger } = require('@neo-sl/sdk');

// Configuration
const CONFIG = {
  PRICE_PAIR: 'NEO/GAS',
  HIGH_THRESHOLD: 1.5,  // Alert when NEO/GAS > 1.5
  LOW_THRESHOLD: 0.5,   // Alert when NEO/GAS < 0.5
  CHECK_INTERVAL: 300   // Check every 5 minutes
};

/**
 * Main function handler
 * @param {Object} context - Execution context
 * @param {Object} event - Trigger event data
 */
async function handler(context, event) {
  const logger = new Logger(context);
  
  try {
    // Get current price from price feed
    const priceFeed = new PriceFeed(context);
    const price = await priceFeed.getPrice(CONFIG.PRICE_PAIR);
    
    logger.info(`Current ${CONFIG.PRICE_PAIR} price: ${price}`);
    
    // Check if price crosses thresholds
    if (price > CONFIG.HIGH_THRESHOLD || price < CONFIG.LOW_THRESHOLD) {
      // Get webhook URL from secrets
      const secrets = new Secrets(context);
      const webhookUrl = await secrets.get('WEBHOOK_URL');
      
      // Prepare alert message
      const alert = {
        timestamp: new Date().toISOString(),
        pair: CONFIG.PRICE_PAIR,
        price: price,
        threshold: price > CONFIG.HIGH_THRESHOLD ? 'HIGH' : 'LOW',
        message: `${CONFIG.PRICE_PAIR} price ${price} has crossed the ${price > CONFIG.HIGH_THRESHOLD ? 'high' : 'low'} threshold`
      };
      
      // Send alert
      await fetch(webhookUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(alert)
      });
      
      logger.info('Alert sent successfully', { alert });
    }
  } catch (error) {
    logger.error('Error in price alert function', { error });
    throw error;
  }
}

module.exports = {
  handler,
  config: {
    name: 'price-alert',
    description: 'Monitors NEO/GAS price and sends alerts when thresholds are crossed',
    runtime: 'node16',
    trigger: {
      type: 'cron',
      schedule: `*/${CONFIG.CHECK_INTERVAL} * * * *` // Every 5 minutes
    },
    resources: {
      memory: 128,
      cpu: 0.1
    },
    permissions: [
      'price-feed:read',
      'secrets:read'
    ]
  }
};