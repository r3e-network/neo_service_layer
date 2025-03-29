/**
 * Price Alert Function
 * 
 * This function checks if a cryptocurrency price has crossed a threshold
 * and sends a notification if it has.
 * 
 * It demonstrates how to use the Neo Service Layer SDK within a function.
 */

const { createFunction } = require('neo-service-layer-js/utils/function-context');

/**
 * Price alert function implementation
 */
module.exports = createFunction(async function(context) {
  // Log function execution
  context.log('Price alert function started');
  
  // Get parameters
  const { symbol = 'NEO', threshold = 50, operator = 'above' } = context.parameters;
  
  // Get current price from price feed service
  const currentPrice = await context.getPrice(symbol);
  context.log(`Current ${symbol} price: $${currentPrice}`);
  
  // Check if price crossed threshold
  let alertTriggered = false;
  if (operator === 'above' && currentPrice > threshold) {
    alertTriggered = true;
  } else if (operator === 'below' && currentPrice < threshold) {
    alertTriggered = true;
  }
  
  // If alert is triggered, send notification
  if (alertTriggered) {
    context.log(`Alert triggered! ${symbol} price is ${operator} $${threshold}`);
    
    // Get notification webhook URL from secrets
    try {
      const webhookUrl = await context.getSecret('notification-webhook');
      
      // Call notification function
      await context.invokeFunction('notification-sender', {
        message: `${symbol} price alert: $${currentPrice} is ${operator} threshold of $${threshold}`,
        webhookUrl
      });
      
      return {
        success: true,
        message: `${symbol} price alert triggered`,
        price: currentPrice,
        threshold,
        operator
      };
    } catch (error) {
      context.log(`Failed to send notification: ${error.message}`);
      throw new Error(`Notification failed: ${error.message}`);
    }
  }
  
  // No alert triggered
  return {
    success: true,
    message: `${symbol} price is within expected range`,
    price: currentPrice,
    threshold,
    operator,
    alertTriggered: false
  };
});
