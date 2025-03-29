/**
 * Transaction Monitor Function
 * 
 * This function demonstrates how to monitor transaction status and handle
 * transaction events using the Neo Service Layer SDK transaction service.
 * 
 * It can be used to trigger actions when transactions reach specific states.
 */

const { createFunction } = require('neo-service-layer-js/utils/function-context');
const { TransactionStatus } = require('neo-service-layer-js/types/models');

/**
 * Transaction monitor function implementation
 */
module.exports = createFunction(async function(context) {
  // Log function execution
  context.log('Transaction monitor function started');
  
  // Get parameters
  const { 
    transactionId,
    requiredConfirmations = 1,
    maxAttempts = 30,
    waitInterval = 5000, // milliseconds
    notifyOnConfirmation = true
  } = context.parameters;
  
  if (!transactionId) {
    throw new Error('Transaction ID is required');
  }
  
  try {
    context.log(`Monitoring transaction ${transactionId}`);
    
    // Initial status check
    let txStatus = await context.transaction.getTransactionStatus(transactionId);
    context.log(`Initial transaction status: ${txStatus.status}`);
    
    // If already confirmed with enough confirmations, return immediately
    if (txStatus.status === TransactionStatus.CONFIRMED && 
        txStatus.confirmations >= requiredConfirmations) {
      context.log(`Transaction already confirmed with ${txStatus.confirmations} confirmations`);
      
      if (notifyOnConfirmation) {
        await sendConfirmationNotification(context, transactionId, txStatus);
      }
      
      return {
        success: true,
        transactionId,
        status: txStatus,
        message: `Transaction is already confirmed with ${txStatus.confirmations} confirmations`
      };
    }
    
    // If failed or expired, return immediately
    if (txStatus.status === TransactionStatus.FAILED || 
        txStatus.status === TransactionStatus.EXPIRED) {
      context.log(`Transaction ${txStatus.status.toLowerCase()}`);
      return {
        success: false,
        transactionId,
        status: txStatus,
        message: `Transaction ${txStatus.status.toLowerCase()}`
      };
    }
    
    // Monitor transaction until confirmed or max attempts reached
    let attempts = 0;
    let confirmed = false;
    
    while (!confirmed && attempts < maxAttempts) {
      // Wait before checking again
      await new Promise(resolve => setTimeout(resolve, waitInterval));
      
      // Check transaction status
      txStatus = await context.transaction.getTransactionStatus(transactionId);
      context.log(`Transaction status: ${txStatus.status}, Confirmations: ${txStatus.confirmations || 0}`);
      
      // Check if failed or expired
      if (txStatus.status === TransactionStatus.FAILED || 
          txStatus.status === TransactionStatus.EXPIRED) {
        context.log(`Transaction ${txStatus.status.toLowerCase()}`);
        return {
          success: false,
          transactionId,
          status: txStatus,
          message: `Transaction ${txStatus.status.toLowerCase()}`
        };
      }
      
      // Check if confirmed with enough confirmations
      if (txStatus.status === TransactionStatus.CONFIRMED && 
          txStatus.confirmations >= requiredConfirmations) {
        confirmed = true;
      }
      
      attempts++;
    }
    
    if (confirmed) {
      context.log(`Transaction confirmed with ${txStatus.confirmations} confirmations`);
      
      // Send notification if configured
      if (notifyOnConfirmation) {
        await sendConfirmationNotification(context, transactionId, txStatus);
      }
      
      // Get transaction details
      const transaction = await context.transaction.getTransaction(transactionId);
      
      // Process transaction based on type
      await processConfirmedTransaction(context, transaction, txStatus);
      
      return {
        success: true,
        transactionId,
        status: txStatus,
        message: `Transaction confirmed with ${txStatus.confirmations} confirmations`
      };
    } else {
      context.log('Transaction not confirmed within the timeout period');
      return {
        success: false,
        transactionId,
        status: txStatus,
        message: `Transaction not confirmed after ${attempts} attempts`
      };
    }
  } catch (error) {
    context.log(`Error in transaction monitor: ${error.message}`);
    throw new Error(`Transaction monitoring failed: ${error.message}`);
  }
});

/**
 * Send a notification about transaction confirmation
 */
async function sendConfirmationNotification(context, transactionId, status) {
  try {
    // Get notification config from secrets
    const notificationConfig = await context.getSecret('notification-config');
    const config = JSON.parse(notificationConfig);
    
    // Invoke notification function
    await context.invokeFunction('notification-sender', {
      message: `Transaction ${transactionId} confirmed with ${status.confirmations} confirmations`,
      blockHeight: status.blockHeight,
      timestamp: status.timestamp,
      config
    });
    
    context.log('Confirmation notification sent');
  } catch (error) {
    context.log(`Failed to send confirmation notification: ${error.message}`);
  }
}

/**
 * Process a confirmed transaction based on its type
 */
async function processConfirmedTransaction(context, transaction, status) {
  // Example processing logic based on transaction type
  switch (transaction.type) {
    case 'transfer':
      context.log(`Processing confirmed transfer of ${transaction.amount} ${transaction.asset}`);
      
      // Update balances or perform other actions
      await context.invokeFunction('balance-updater', {
        sender: transaction.sender,
        recipient: transaction.recipient,
        amount: transaction.amount,
        asset: transaction.asset
      });
      break;
      
    case 'contract_invoke':
      context.log(`Processing confirmed contract invocation`);
      
      // Check application logs for events
      if (status.applicationLog && status.applicationLog.executions) {
        const execution = status.applicationLog.executions[0];
        if (execution.notifications && execution.notifications.length > 0) {
          // Process contract notifications/events
          for (const notification of execution.notifications) {
            context.log(`Contract event: ${notification.eventName}`);
            
            // Process specific events
            if (notification.eventName === 'Transfer') {
              await context.invokeFunction('transfer-event-handler', {
                contractHash: notification.contract,
                event: notification
              });
            }
          }
        }
      }
      break;
      
    default:
      context.log(`No specific processing for transaction type: ${transaction.type}`);
  }
}
