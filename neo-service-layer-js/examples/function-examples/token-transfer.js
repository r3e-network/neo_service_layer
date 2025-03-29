/**
 * Token Transfer Function
 * 
 * This function demonstrates how to create, sign, and send a token transfer transaction
 * using the Neo Service Layer SDK transaction service.
 * 
 * It shows the complete transaction lifecycle from creation to confirmation.
 */

const { createFunction } = require('neo-service-layer-js/utils/function-context');
const { TransactionType, TransactionStatus } = require('neo-service-layer-js/types/models');

/**
 * Token transfer function implementation
 */
module.exports = createFunction(async function(context) {
  // Log function execution
  context.log('Token transfer function started');
  
  // Get parameters
  const { 
    recipient = 'NZV3gXfwUjHHvJyM5C7C4wgffVwtCrP3nY', 
    amount = 1.0,
    asset = 'NEO',
    waitForConfirmation = true
  } = context.parameters;
  
  try {
    // Create a new transaction
    context.log(`Creating ${asset} transfer transaction to ${recipient} for ${amount}`);
    const transaction = await context.transaction.createTransaction({
      type: TransactionType.TRANSFER,
      recipient,
      amount,
      asset
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
        return {
          success: true,
          transaction: sentTx,
          status: txStatus,
          message: `Successfully transferred ${amount} ${asset} to ${recipient}`
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
      message: `Successfully sent ${amount} ${asset} to ${recipient}`
    };
  } catch (error) {
    context.log(`Error in token transfer: ${error.message}`);
    throw new Error(`Token transfer failed: ${error.message}`);
  }
});
