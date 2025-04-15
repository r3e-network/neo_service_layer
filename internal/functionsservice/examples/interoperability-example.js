/**
 * Neo Service Layer JavaScript Interoperability Example
 * 
 * This example demonstrates how to use the JavaScript interoperability features
 * of the Neo Service Layer Function Service.
 */

/**
 * Main function entry point
 * @param {Object} args - Function arguments
 * @returns {Object} - Function result
 */
function main(args) {
  // Log function start
  context.log('Function started');
  
  // Access function context properties
  const functionId = context.functionId;
  const executionId = context.executionId;
  const owner = context.owner;
  const caller = context.caller || 'No caller specified';
  
  context.log(`Function ID: ${functionId}`);
  context.log(`Execution ID: ${executionId}`);
  context.log(`Owner: ${owner}`);
  context.log(`Caller: ${caller}`);
  
  // Access function parameters
  const { symbol = 'NEO', threshold = 50 } = args;
  context.log(`Symbol: ${symbol}`);
  context.log(`Threshold: ${threshold}`);
  
  // Access environment variables
  const environment = context.env.ENVIRONMENT || 'development';
  context.log(`Environment: ${environment}`);
  
  // Get current price (async operation)
  const price = context.getPrice(symbol);
  context.log(`Current ${symbol} price: $${price}`);
  
  // Get gas price (async operation)
  const gasPrice = context.getGasPrice();
  context.log(`Current gas price: ${gasPrice}`);
  
  // Get a secret (async operation)
  const apiKey = context.getSecret('api-key');
  context.log(`API Key retrieved (masked): ${maskSecret(apiKey)}`);
  
  // Invoke another function (async operation)
  const result = context.invokeFunction('data-processor', {
    price,
    symbol,
    threshold
  });
  context.log(`Result from data-processor: ${JSON.stringify(result)}`);
  
  // Perform analysis based on price and threshold
  const analysis = analyzePrice(price, threshold);
  
  // Return result
  return {
    success: true,
    functionId,
    executionId,
    symbol,
    price,
    threshold,
    analysis,
    processingResult: result,
    timestamp: new Date().toISOString()
  };
}

/**
 * Analyze price based on threshold
 * @param {number} price - Current price
 * @param {number} threshold - Price threshold
 * @returns {Object} - Analysis result
 */
function analyzePrice(price, threshold) {
  const percentDifference = ((price - threshold) / threshold) * 100;
  
  let sentiment;
  let action;
  
  if (price > threshold * 1.1) {
    sentiment = 'very bullish';
    action = 'STRONG SELL';
  } else if (price > threshold) {
    sentiment = 'bullish';
    action = 'SELL';
  } else if (price > threshold * 0.9) {
    sentiment = 'neutral';
    action = 'HOLD';
  } else if (price > threshold * 0.8) {
    sentiment = 'bearish';
    action = 'BUY';
  } else {
    sentiment = 'very bearish';
    action = 'STRONG BUY';
  }
  
  return {
    sentiment,
    action,
    percentDifference: percentDifference.toFixed(2) + '%',
    timestamp: new Date().toISOString()
  };
}

/**
 * Mask a secret for logging
 * @param {string} secret - Secret to mask
 * @returns {string} - Masked secret
 */
function maskSecret(secret) {
  if (!secret || secret.length < 8) {
    return '********';
  }
  
  return secret.substring(0, 4) + '****' + secret.substring(secret.length - 4);
}
