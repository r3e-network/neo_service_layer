/**
 * Token Balance Monitor Function
 *
 * This function monitors a NEP-17 token balance for a given address and
 * sends an alert when the balance falls below a defined threshold.
 *
 * Parameters:
 * - tokenHash: The NEP-17 token script hash
 * - address: The address to monitor
 * - thresholdAmount: The threshold amount for alerts
 * - alertEndpoint: The endpoint to send alerts to
 */

/**
 * Query the balance for the given NEP-17 token and address
 * @param {string} tokenHash - NEP-17 token script hash
 * @param {string} address - NEO address to check
 * @returns {Object} Balance information
 */
function queryTokenBalance(tokenHash, address) {
  console.info(`Querying balance for token ${tokenHash} and address ${address}`);
  
  // Use the provided SDK to query token balance on Neo blockchain
  // This is a simulated implementation
  try {
    // Track gas usage for blockchain interactions
    __trackGas(500);
    
    // In a real implementation, this would call the Neo blockchain
    const balance = neo.rpc.invokeFunction({
      scriptHash: tokenHash,
      operation: "balanceOf",
      params: [{ type: "Hash160", value: address }]
    });
    
    return {
      success: true,
      balance: balance.state.stack[0].value,
      decimals: 8, // Would come from token info
      symbol: "TOKEN" // Would come from token info
    };
  } catch (error) {
    console.error(`Error querying token balance: ${error.message}`);
    return {
      success: false,
      error: error.message
    };
  }
}

/**
 * Send an alert when balance is below threshold
 * @param {Object} data - Alert data
 * @param {string} endpoint - Endpoint to send alert to
 * @returns {Object} Alert result
 */
function sendAlert(data, endpoint) {
  console.info(`Sending alert to ${endpoint}`);
  
  try {
    // Track gas usage for external API calls
    __trackGas(1000);
    
    // In a real implementation, this would make an HTTP request
    const response = http.post(endpoint, {
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(data)
    });
    
    return {
      success: true,
      statusCode: response.statusCode,
      response: response.body
    };
  } catch (error) {
    console.error(`Error sending alert: ${error.message}`);
    return {
      success: false,
      error: error.message
    };
  }
}

/**
 * Format token amount with proper decimals
 * @param {number} amount - Raw token amount
 * @param {number} decimals - Token decimals
 * @returns {string} Formatted amount
 */
function formatTokenAmount(amount, decimals) {
  // Simple implementation for demonstration
  const divisor = Math.pow(10, decimals);
  return (amount / divisor).toFixed(decimals);
}

/**
 * Main function - entry point for execution
 * @param {Object} args - Function arguments
 * @returns {Object} Execution result
 */
function main(args) {
  console.info("Starting token balance monitor function");
  
  // Validate required parameters
  if (!args.tokenHash || !args.address || !args.thresholdAmount || !args.alertEndpoint) {
    return {
      success: false,
      error: "Missing required parameters: tokenHash, address, thresholdAmount, alertEndpoint"
    };
  }
  
  // Extract parameters
  const { tokenHash, address, thresholdAmount, alertEndpoint } = args;
  
  // Query token balance
  const balanceResult = queryTokenBalance(tokenHash, address);
  if (!balanceResult.success) {
    return {
      success: false,
      error: `Failed to query balance: ${balanceResult.error}`
    };
  }
  
  // Calculate formatted balance
  const rawBalance = balanceResult.balance;
  const formattedBalance = formatTokenAmount(rawBalance, balanceResult.decimals);
  
  console.info(`Current balance: ${formattedBalance} ${balanceResult.symbol}`);
  
  // Check if balance is below threshold
  if (rawBalance < thresholdAmount) {
    console.info(`Balance below threshold: ${formattedBalance} < ${formatTokenAmount(thresholdAmount, balanceResult.decimals)}`);
    
    // Send alert
    const alertData = {
      alertType: "LOW_BALANCE",
      address: address,
      token: {
        hash: tokenHash,
        symbol: balanceResult.symbol
      },
      balance: {
        raw: rawBalance,
        formatted: formattedBalance
      },
      threshold: {
        raw: thresholdAmount,
        formatted: formatTokenAmount(thresholdAmount, balanceResult.decimals)
      },
      timestamp: Date.now()
    };
    
    const alertResult = sendAlert(alertData, alertEndpoint);
    
    return {
      success: alertResult.success,
      balanceBelow: true,
      currentBalance: formattedBalance,
      threshold: formatTokenAmount(thresholdAmount, balanceResult.decimals),
      alertSent: alertResult.success,
      alertResponse: alertResult
    };
  }
  
  // Balance is above threshold, no action needed
  return {
    success: true,
    balanceBelow: false,
    currentBalance: formattedBalance,
    threshold: formatTokenAmount(thresholdAmount, balanceResult.decimals)
  };
} 