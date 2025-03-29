/**
 * Transaction Fee Estimator Function
 * 
 * This function demonstrates how to estimate transaction fees for different
 * transaction types using the Neo Service Layer SDK.
 * 
 * It helps users understand the cost of various blockchain operations.
 */

const { createFunction } = require('neo-service-layer-js/utils/function-context');
const { TransactionType } = require('neo-service-layer-js/types/models');

/**
 * Transaction fee estimator function implementation
 */
module.exports = createFunction(async function(context) {
  // Log function execution
  context.log('Transaction fee estimator function started');
  
  // Get parameters
  const { 
    transactionTypes = ['transfer', 'contract_invoke', 'contract_deploy'],
    asset = 'NEO',
    amount = 1.0,
    recipient = 'NZV3gXfwUjHHvJyM5C7C4wgffVwtCrP3nY'
  } = context.parameters;
  
  try {
    // Get current gas price for context
    const gasPrice = await context.getGasPrice();
    context.log(`Current gas price: ${gasPrice} GAS`);
    
    // Estimate fees for each transaction type
    const estimates = {};
    
    for (const type of transactionTypes) {
      let txRequest;
      
      // Prepare transaction request based on type
      switch (type) {
        case 'transfer':
          txRequest = {
            type: TransactionType.TRANSFER,
            recipient,
            amount,
            asset
          };
          break;
          
        case 'contract_invoke':
          txRequest = {
            type: TransactionType.CONTRACT_INVOKE,
            contractHash: 'ef4073a0f2b305a38ec4050e4d3d28bc40ea63f5',
            operation: 'transfer',
            args: [
              { type: 'Hash160', value: 'NZV3gXfwUjHHvJyM5C7C4wgffVwtCrP3nY' },
              { type: 'Hash160', value: 'NiNmXL8FjEUEs1nfX9uHFBNaenxDHJtmuB' },
              { type: 'Integer', value: '100000000' }
            ]
          };
          break;
          
        case 'contract_deploy':
          txRequest = {
            type: TransactionType.CONTRACT_DEPLOY,
            name: 'ExampleContract',
            script: '0c0d747261636b5472616e73666572670d747261636b5472616e73666572',
            manifest: {
              name: 'ExampleContract',
              supportedStandards: ['NEP-17'],
              abi: {
                methods: [
                  {
                    name: 'transfer',
                    parameters: [
                      { name: 'from', type: 'Hash160' },
                      { name: 'to', type: 'Hash160' },
                      { name: 'amount', type: 'Integer' }
                    ],
                    returnType: 'Boolean'
                  }
                ]
              }
            }
          };
          break;
          
        default:
          context.log(`Unsupported transaction type: ${type}`);
          continue;
      }
      
      // Estimate fee
      context.log(`Estimating fee for ${type} transaction...`);
      const fee = await context.transaction.estimateFee(txRequest);
      
      // Store estimate
      estimates[type] = {
        fee,
        feeInUSD: await context.getPrice('GAS') * fee
      };
      
      context.log(`Estimated fee for ${type}: ${fee} GAS (approx. $${estimates[type].feeInUSD.toFixed(2)})`);
    }
    
    // Return fee estimates
    return {
      success: true,
      gasPrice,
      estimates,
      message: 'Fee estimation completed successfully'
    };
  } catch (error) {
    context.log(`Error in fee estimation: ${error.message}`);
    throw new Error(`Fee estimation failed: ${error.message}`);
  }
});
