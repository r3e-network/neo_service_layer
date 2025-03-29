/**
 * Blockchain Monitor Function
 * 
 * This function monitors Neo blockchain transactions and performs actions
 * based on specific transaction patterns.
 * 
 * It demonstrates how to use the Neo Service Layer SDK to interact with
 * blockchain data and other services.
 */

const { createFunction } = require('neo-service-layer-js/utils/function-context');

/**
 * Blockchain monitor function implementation
 */
module.exports = createFunction(async function(context) {
  // Log function execution
  context.log('Blockchain monitor function started');
  
  // Get parameters
  const { 
    contractHash = 'ef4073a0f2b305a38ec4050e4d3d28bc40ea63f5', 
    eventName = 'Transfer',
    minAmount = 1000
  } = context.parameters;
  
  try {
    // Access Neo blockchain data through the Neo Service Layer
    const neoClient = context.neoServiceLayer;
    
    // Get recent contract events
    const events = await neoClient.functions.invokeFunction({
      functionId: 'blockchain-query',
      parameters: {
        operation: 'getContractEvents',
        contractHash,
        eventName,
        limit: 10
      }
    });
    
    context.log(`Found ${events.result.length} ${eventName} events`);
    
    // Process events
    const largeTransfers = events.result.filter(event => {
      // Parse event parameters
      const params = event.state.map(p => p.value);
      // Assuming Transfer event has amount as the third parameter
      const amount = parseFloat(params[2]);
      return amount >= minAmount;
    });
    
    context.log(`Found ${largeTransfers.length} large transfers exceeding ${minAmount}`);
    
    if (largeTransfers.length > 0) {
      // Get current gas price for context
      const gasPrice = await context.getGasPrice();
      
      // Store analysis results
      await neoClient.functions.invokeFunction({
        functionId: 'data-storage',
        parameters: {
          operation: 'storeAnalysis',
          data: {
            timestamp: new Date().toISOString(),
            contractHash,
            eventName,
            largeTransfers: largeTransfers.map(event => ({
              txId: event.txid,
              from: event.state[0].value,
              to: event.state[1].value,
              amount: parseFloat(event.state[2].value)
            })),
            gasPrice
          }
        }
      });
      
      // Send alert if configured
      if (context.parameters.alertEnabled) {
        try {
          const alertSecret = await context.getSecret('alert-config');
          const alertConfig = JSON.parse(alertSecret);
          
          await context.invokeFunction('notification-sender', {
            message: `Alert: ${largeTransfers.length} large transfers detected for contract ${contractHash}`,
            config: alertConfig
          });
        } catch (error) {
          context.log(`Failed to send alert: ${error.message}`);
        }
      }
    }
    
    return {
      success: true,
      contractHash,
      eventName,
      totalEvents: events.result.length,
      largeTransfers: largeTransfers.length,
      minAmount
    };
  } catch (error) {
    context.log(`Error in blockchain monitor: ${error.message}`);
    throw new Error(`Blockchain monitoring failed: ${error.message}`);
  }
});
