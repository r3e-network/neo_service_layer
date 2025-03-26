/**
 * Gas Optimization Function
 * 
 * This function automatically manages gas allocation for a contract
 * based on usage patterns and demonstrates gas bank integration.
 */

const { Neo, GasBank, Logger, Metrics } = require('@neo-sl/sdk');

// Configuration
const CONFIG = {
  CONTRACT_HASH: '0x668e0c1f9d7b70a99dd9e06eadd4c784d641385d',
  MIN_GAS_THRESHOLD: '1000000000',  // 10 GAS
  MAX_GAS_THRESHOLD: '5000000000',  // 50 GAS
  OPTIMAL_GAS_BUFFER: '2000000000', // 20 GAS
  CHECK_INTERVAL: 3600              // Check every hour
};

/**
 * Main function handler
 * @param {Object} context - Execution context
 * @param {Object} event - Trigger event data
 */
async function handler(context, event) {
  const logger = new Logger(context);
  const metrics = new Metrics(context);
  
  try {
    // Initialize services
    const neo = new Neo(context);
    const gasBank = new GasBank(context);
    
    // Get contract's current gas balance
    const contract = neo.getContract(CONFIG.CONTRACT_HASH);
    const gasBalance = await contract.getGasBalance();
    
    logger.info('Current contract gas balance', { gasBalance });
    
    // Get gas usage metrics for the last 24 hours
    const gasUsageMetrics = await metrics.query({
      metric: 'contract_gas_usage',
      contract: CONFIG.CONTRACT_HASH,
      period: '24h'
    });
    
    // Calculate optimal gas allocation
    const {
      averageUsage,
      peakUsage,
      recommendedAllocation
    } = calculateOptimalGasAllocation(gasUsageMetrics);
    
    logger.info('Gas usage analysis', {
      averageUsage,
      peakUsage,
      recommendedAllocation
    });
    
    // Check if reallocation is needed
    if (gasBalance < CONFIG.MIN_GAS_THRESHOLD) {
      // Allocate more gas
      const allocationAmount = recommendedAllocation - gasBalance;
      const allocation = await gasBank.allocate(allocationAmount.toString());
      
      // Transfer gas to contract
      await contract.transfer({
        asset: 'GAS',
        amount: allocationAmount.toString(),
        from: allocation.address,
        data: 'Gas optimization reallocation'
      });
      
      logger.info('Gas allocation increased', {
        amount: allocationAmount,
        newBalance: (gasBalance + allocationAmount).toString()
      });
      
    } else if (gasBalance > CONFIG.MAX_GAS_THRESHOLD) {
      // Return excess gas
      const excessAmount = gasBalance - recommendedAllocation;
      await contract.transfer({
        asset: 'GAS',
        amount: excessAmount.toString(),
        to: gasBank.getAddress(),
        data: 'Returning excess gas'
      });
      
      logger.info('Excess gas returned', {
        amount: excessAmount,
        newBalance: recommendedAllocation.toString()
      });
    }
    
    // Record metrics
    await metrics.record({
      metric: 'gas_optimization',
      contract: CONFIG.CONTRACT_HASH,
      values: {
        gasBalance: gasBalance.toString(),
        recommendedAllocation: recommendedAllocation.toString(),
        averageUsage: averageUsage.toString(),
        peakUsage: peakUsage.toString()
      }
    });
    
  } catch (error) {
    logger.error('Error in gas optimization', { error });
    throw error;
  }
}

/**
 * Calculate optimal gas allocation based on usage metrics
 * @param {Object} metrics - Gas usage metrics
 * @returns {Object} Optimization calculations
 */
function calculateOptimalGasAllocation(metrics) {
  const averageUsage = metrics.reduce((sum, m) => sum + BigInt(m.value), 0n) / BigInt(metrics.length);
  const peakUsage = metrics.reduce((max, m) => BigInt(m.value) > max ? BigInt(m.value) : max, 0n);
  
  // Recommended allocation: peak usage + buffer
  const recommendedAllocation = peakUsage + BigInt(CONFIG.OPTIMAL_GAS_BUFFER);
  
  return {
    averageUsage,
    peakUsage,
    recommendedAllocation
  };
}

module.exports = {
  handler,
  config: {
    name: 'gas-optimization',
    description: 'Automatically manages contract gas allocation',
    runtime: 'node16',
    trigger: {
      type: 'cron',
      schedule: `0 */${CONFIG.CHECK_INTERVAL} * * *` // Every hour
    },
    resources: {
      memory: 256,
      cpu: 0.2
    },
    permissions: [
      'neo:read',
      'neo:write',
      'gas-bank:allocate',
      'gas-bank:release',
      'metrics:read',
      'metrics:write'
    ]
  }
};