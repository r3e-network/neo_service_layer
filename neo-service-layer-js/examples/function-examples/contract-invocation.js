/**
 * Smart Contract Invocation Function
 * 
 * This function demonstrates how to create, sign, and send a contract invocation transaction
 * using the Neo Service Layer SDK transaction service.
 * 
 * It shows how to interact with smart contracts on the Neo blockchain.
 */

const { createFunction } = require('neo-service-layer-js/utils/function-context');
const { TransactionType, TransactionStatus } = require('neo-service-layer-js/types/models');

/**
 * Contract invocation function implementation
 */
module.exports = createFunction(async function(context) {
  // Log function execution
  context.log('Contract invocation function started');
  
  // Get parameters
  const { 
    contractHash = 'ef4073a0f2b305a38ec4050e4d3d28bc40ea63f5',
    operation = 'transfer',
    from = null, // Will use the function's wallet if null
    to = 'NZV3gXfwUjHHvJyM5C7C4wgffVwtCrP3nY',
    amount = 1.0,
    waitForConfirmation = true
  } = context.parameters;
  
  try {
    // Convert amount to the smallest unit (assuming NEP-17 token with 8 decimals)
    const amountInSmallestUnit = Math.floor(amount * 100000000).toString();
    
    // Create contract invocation transaction
    context.log(`Creating contract invocation transaction for ${contractHash}.${operation}`);
    
    const transaction = await context.transaction.createTransaction({
      type: TransactionType.CONTRACT_INVOKE,
      contractHash,
      operation,
      args: [
        { type: 'Hash160', value: from || 'sender' }, // 'sender' is a special value that will be replaced with the function's wallet
        { type: 'Hash160', value: to },
        { type: 'Integer', value: amountInSmallestUnit }
      ]
    });
    
    context.log(`Transaction created with ID: ${transaction.id}`);
    
    // Sign the transaction
    context.log('Signing transaction...');
    const signedTx = await context.transaction.signTransaction(transaction.id);
    
    context.log(`Transaction signed. Status: ${signedTx.status}`);
    
    // Send the transaction to the blockchain
    context.log('Sending transaction to the blockchain...');
    const sentTx = await context.transaction.sendTransaction(signedTx.id);
    
    context.log(`Transaction sent. Hash: ${sentTx.hash}`);
    
    // If configured to wait for confirmation, poll the transaction status
    if (waitForConfirmation) {
      context.log('Waiting for transaction confirmation...');
      
      let confirmed = false;
      let attempts = 0;
      let txStatus;
      
      while (!confirmed && attempts < 30) {
        // Wait 5 seconds between checks
        await new Promise(resolve => setTimeout(resolve, 5000));
        
        // Check transaction status
        txStatus = await context.transaction.getTransactionStatus(sentTx.id);
        context.log(`Transaction status: ${txStatus.status}, Confirmations: ${txStatus.confirmations || 0}`);
        
        // Check if confirmed (usually requires 1 or more confirmations)
        if (txStatus.status === TransactionStatus.CONFIRMED && 
            txStatus.confirmations >= 1) {
          confirmed = true;
        }
        
        attempts++;
      }
      
      if (confirmed) {
        context.log(`Transaction confirmed with ${txStatus.confirmations} confirmations`);
        
        // Check for application logs to determine success
        const appLog = txStatus.applicationLog;
        if (appLog && appLog.executions && appLog.executions.length > 0) {
          const execution = appLog.executions[0];
          if (execution.state === 'HALT') {
            context.log('Contract execution successful');
            
            // Parse the result if available
            let result = null;
            if (execution.stack && execution.stack.length > 0) {
              result = execution.stack[0].value;
            }
            
            return {
              success: true,
              transaction: sentTx,
              status: txStatus,
              contractResult: result,
              message: `Successfully invoked ${operation} on contract ${contractHash}`
            };
          } else {
            context.log(`Contract execution failed: ${execution.exception || 'Unknown error'}`);
            return {
              success: false,
              transaction: sentTx,
              status: txStatus,
              error: execution.exception || 'Contract execution failed',
              message: `Failed to invoke ${operation} on contract ${contractHash}`
            };
          }
        }
        
        return {
          success: true,
          transaction: sentTx,
          status: txStatus,
          message: `Successfully invoked ${operation} on contract ${contractHash}`
        };
      } else {
        context.log('Transaction not confirmed within the timeout period');
        return {
          success: true,
          transaction: sentTx,
          status: txStatus,
          message: `Transaction sent but not yet confirmed. Check status later.`
        };
      }
    }
    
    // Return transaction details
    return {
      success: true,
      transaction: sentTx,
      message: `Successfully sent contract invocation transaction for ${contractHash}.${operation}`
    };
  } catch (error) {
    context.log(`Error in contract invocation: ${error.message}`);
    throw new Error(`Contract invocation failed: ${error.message}`);
  }
});
