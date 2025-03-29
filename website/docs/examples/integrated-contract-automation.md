# Integrated Contract Automation Example

## Overview
This example demonstrates how to integrate Contract Automation, Functions, and Secrets services to create an automated trading system that:
- Monitors price feeds for trading opportunities
- Executes trades automatically based on conditions
- Manages contract interactions securely
- Tracks performance and positions
- Provides monitoring and alerts

## Complete Integration Example

### Automated Trading Function
```typescript
import { Handler } from '@netlify/functions';
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { SecretVault } from '../../utils/vault';
import { ContractAutomation } from '../../services/contract-automation';
import { PriceFeedService } from '../../services/price-feed';
import { verifyNeoSignature } from '../../utils/auth';

// Initialize services
const logger = Logger.getInstance().child({ service: 'trading-automation' });
const metrics = new MetricsService({
  namespace: 'neo_service_layer',
  subsystem: 'trading_automation'
});
const vault = new SecretVault({
  teeEnabled: true,
  backupEnabled: true,
  rotationPeriod: 24 * 60 * 60 * 1000
});

export const handler: Handler = async (event, context) => {
  const timer = metrics.startTimer('trading_automation_request_duration_seconds');
  
  const requestContext = {
    requestId: context.awsRequestId,
    path: event.path,
    method: event.httpMethod,
    timestamp: new Date().toISOString()
  };

  try {
    // Log incoming request
    logger.info('Processing trading automation request', {
      ...requestContext,
      queryParams: event.queryStringParameters
    });

    // Verify authentication
    const neoAddress = event.headers['x-neo-address'];
    const signature = event.headers['x-neo-signature'];
    const timestamp = event.headers['x-timestamp'];

    if (!neoAddress || !signature || !timestamp) {
      metrics.incrementCounter('trading_automation_auth_errors_total', {
        error_type: 'missing_headers'
      });
      
      logger.warn('Missing authentication headers', requestContext);
      
      return {
        statusCode: 401,
        body: JSON.stringify({ error: 'Missing authentication headers' })
      };
    }

    const isValid = await verifyNeoSignature(
      neoAddress,
      signature,
      \`\${event.path}:\${timestamp}\`
    );

    if (!isValid) {
      metrics.incrementCounter('trading_automation_auth_errors_total', {
        error_type: 'invalid_signature'
      });
      
      logger.warn('Invalid signature', {
        ...requestContext,
        neoAddress
      });
      
      return {
        statusCode: 401,
        body: JSON.stringify({ error: 'Invalid signature' })
      };
    }

    // Initialize services with credentials from vault
    const credentials = await vault.getSecret('trading_credentials');
    const automation = new ContractAutomation(JSON.parse(credentials.value));
    const priceFeed = new PriceFeedService();

    // Parse request body
    const body = JSON.parse(event.body || '{}');
    const { contractAddress, action } = body;

    switch (action) {
      case 'setup_strategy': {
        // Setup automated trading strategy
        const config = await setupTradingStrategy(
          automation,
          priceFeed,
          contractAddress,
          body.strategy,
          neoAddress
        );

        logger.info('Trading strategy setup completed', {
          ...requestContext,
          contractAddress,
          config
        });

        return {
          statusCode: 200,
          body: JSON.stringify({
            message: 'Trading strategy setup successful',
            config
          })
        };
      }

      case 'get_status': {
        // Get current trading status
        const status = await getTradingStatus(
          automation,
          priceFeed,
          contractAddress
        );

        logger.info('Trading status retrieved', {
          ...requestContext,
          contractAddress,
          status
        });

        return {
          statusCode: 200,
          body: JSON.stringify(status)
        };
      }

      default:
        return {
          statusCode: 400,
          body: JSON.stringify({ error: 'Invalid action' })
        };
    }
  } catch (error) {
    metrics.incrementCounter('trading_automation_errors_total', {
      error_type: error.name
    });

    logger.error('Trading automation request failed', {
      ...requestContext,
      error: {
        name: error.name,
        message: error.message,
        stack: error.stack
      }
    });

    return {
      statusCode: 500,
      body: JSON.stringify({
        error: 'Failed to process trading automation request',
        requestId: context.awsRequestId
      })
    };
  } finally {
    timer.end();
  }
};

async function setupTradingStrategy(
  automation: ContractAutomation,
  priceFeed: PriceFeedService,
  contractAddress: string,
  strategy: TradingStrategy,
  ownerAddress: string
) {
  const logger = Logger.getInstance().child({
    component: 'trading-strategy-setup'
  });
  
  const metrics = new MetricsService({
    namespace: 'neo_service_layer',
    subsystem: 'trading_strategy'
  });

  try {
    // Validate strategy parameters
    validateStrategy(strategy);

    // Configure price monitoring
    const priceMonitor = await automation.createMonitor({
      contractAddress,
      checkInterval: strategy.checkInterval || 60, // Default 1 minute
      conditions: buildPriceConditions(strategy)
    });

    // Configure trading automation
    const tradingAutomation = await automation.createAutomation({
      trigger: priceMonitor.id,
      action: {
        type: 'execute_trade',
        contract: contractAddress,
        method: 'executeTrade',
        params: buildTradeParameters(strategy)
      },
      maxExecutions: strategy.maxTrades || -1,
      cooldownPeriod: strategy.tradeCooldown || 300
    });

    // Setup position monitoring
    const positionMonitor = await automation.createMonitor({
      contractAddress,
      checkInterval: 60,
      conditions: [
        {
          type: 'position_size',
          threshold: strategy.maxPositionSize,
          operator: 'greater_than'
        },
        {
          type: 'unrealized_pnl',
          threshold: strategy.stopLoss,
          operator: 'less_than'
        }
      ]
    });

    // Configure position management automation
    const positionAutomation = await automation.createAutomation({
      trigger: positionMonitor.id,
      action: {
        type: 'manage_position',
        contract: contractAddress,
        method: 'closePosition',
        params: {
          full: true
        }
      },
      maxExecutions: -1,
      cooldownPeriod: 60
    });

    // Record setup metrics
    metrics.incrementCounter('trading_strategy_setups_total', {
      contract_address: contractAddress,
      strategy_type: strategy.type
    });

    logger.info('Trading strategy setup completed', {
      contractAddress,
      strategy,
      monitors: {
        price: priceMonitor.id,
        position: positionMonitor.id
      },
      automations: {
        trading: tradingAutomation.id,
        position: positionAutomation.id
      }
    });

    return {
      strategyId: \`\${tradingAutomation.id}_\${positionAutomation.id}\`,
      monitors: {
        price: priceMonitor.id,
        position: positionMonitor.id
      },
      automations: {
        trading: tradingAutomation.id,
        position: positionAutomation.id
      },
      config: {
        strategy,
        conditions: {
          price: priceMonitor.conditions,
          position: positionMonitor.conditions
        }
      }
    };
  } catch (error) {
    metrics.incrementCounter('trading_strategy_setup_errors_total', {
      error_type: error.name
    });

    logger.error('Failed to setup trading strategy', {
      contractAddress,
      strategy,
      error: {
        name: error.name,
        message: error.message,
        stack: error.stack
      }
    });

    throw error;
  }
}

async function getTradingStatus(
  automation: ContractAutomation,
  priceFeed: PriceFeedService,
  contractAddress: string
) {
  const logger = Logger.getInstance().child({
    component: 'trading-status'
  });

  try {
    // Get active monitors
    const monitors = await automation.listMonitors({
      contractAddress,
      types: ['price_feed', 'position_size', 'unrealized_pnl']
    });

    // Get active automations
    const automations = await automation.listAutomations({
      contractAddress,
      types: ['execute_trade', 'manage_position']
    });

    // Get recent executions
    const recentExecutions = await automation.getExecutions({
      automationIds: automations.map(a => a.id),
      limit: 10
    });

    // Get current positions
    const positions = await getContractPositions(contractAddress);

    // Get relevant price feeds
    const prices = await Promise.all(
      positions.map(async pos => ({
        pair: pos.pair,
        price: await priceFeed.getLatestPrice(...pos.pair.split('/'))
      }))
    );

    return {
      monitors: monitors.map(m => ({
        id: m.id,
        type: m.type,
        conditions: m.conditions,
        lastCheck: m.lastCheck,
        status: m.status
      })),
      automations: automations.map(a => ({
        id: a.id,
        type: a.type,
        trigger: a.trigger,
        status: a.status,
        lastExecution: a.lastExecution
      })),
      recentExecutions: recentExecutions.map(e => ({
        timestamp: e.timestamp,
        type: e.type,
        status: e.status,
        details: e.details
      })),
      positions: positions.map(pos => ({
        ...pos,
        currentPrice: prices.find(p => p.pair === pos.pair)?.price
      })),
      performance: await getStrategyPerformance(contractAddress)
    };
  } catch (error) {
    logger.error('Failed to get trading status', {
      contractAddress,
      error: {
        name: error.name,
        message: error.message,
        stack: error.stack
      }
    });

    throw error;
  }
}

interface TradingStrategy {
  type: 'momentum' | 'mean_reversion' | 'grid';
  pairs: string[];
  checkInterval?: number;
  maxTrades?: number;
  tradeCooldown?: number;
  maxPositionSize: number;
  stopLoss: number;
  parameters: {
    entryThreshold: number;
    exitThreshold: number;
    lookbackPeriod: number;
    [key: string]: any;
  };
}

function validateStrategy(strategy: TradingStrategy) {
  const requiredFields = [
    'type',
    'pairs',
    'maxPositionSize',
    'stopLoss',
    'parameters'
  ];

  for (const field of requiredFields) {
    if (!strategy[field]) {
      throw new Error(\`Missing required strategy field: \${field}\`);
    }
  }

  if (strategy.maxPositionSize <= 0) {
    throw new Error('maxPositionSize must be greater than 0');
  }

  if (strategy.stopLoss >= 0) {
    throw new Error('stopLoss must be less than 0');
  }

  // Validate strategy-specific parameters
  switch (strategy.type) {
    case 'momentum':
      validateMomentumStrategy(strategy);
      break;
    case 'mean_reversion':
      validateMeanReversionStrategy(strategy);
      break;
    case 'grid':
      validateGridStrategy(strategy);
      break;
    default:
      throw new Error(\`Unknown strategy type: \${strategy.type}\`);
  }
}

function buildPriceConditions(strategy: TradingStrategy) {
  switch (strategy.type) {
    case 'momentum':
      return buildMomentumConditions(strategy);
    case 'mean_reversion':
      return buildMeanReversionConditions(strategy);
    case 'grid':
      return buildGridConditions(strategy);
    default:
      throw new Error(\`Unknown strategy type: \${strategy.type}\`);
  }
}

function buildTradeParameters(strategy: TradingStrategy) {
  const baseParams = {
    maxPositionSize: strategy.maxPositionSize,
    stopLoss: strategy.stopLoss
  };

  switch (strategy.type) {
    case 'momentum':
      return {
        ...baseParams,
        ...buildMomentumParameters(strategy)
      };
    case 'mean_reversion':
      return {
        ...baseParams,
        ...buildMeanReversionParameters(strategy)
      };
    case 'grid':
      return {
        ...baseParams,
        ...buildGridParameters(strategy)
      };
    default:
      throw new Error(\`Unknown strategy type: \${strategy.type}\`);
  }
}

async function getContractPositions(contractAddress: string) {
  // Implementation would call contract to get current positions
  return [];
}

async function getStrategyPerformance(contractAddress: string) {
  // Implementation would calculate strategy performance metrics
  return {
    totalTrades: 0,
    winRate: 0,
    pnl: 0,
    sharpeRatio: 0
  };
}
```

### Trading Monitor Component
```typescript
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { ContractAutomation } from '../../services/contract-automation';
import { PriceFeedService } from '../../services/price-feed';
import { AlertService } from '../../services/alerts';

class TradingMonitor {
  private logger: Logger;
  private metrics: MetricsService;
  private automation: ContractAutomation;
  private priceFeed: PriceFeedService;
  private alertService: AlertService;
  private monitoringInterval: NodeJS.Timeout | null = null;

  constructor(
    automation: ContractAutomation,
    priceFeed: PriceFeedService
  ) {
    this.logger = Logger.getInstance().child({
      component: 'TradingMonitor'
    });
    
    this.metrics = new MetricsService({
      namespace: 'neo_service_layer',
      subsystem: 'trading_monitor'
    });
    
    this.automation = automation;
    this.priceFeed = priceFeed;
    this.alertService = new AlertService();
  }

  async startMonitoring(contracts: string[], interval: number) {
    this.logger.info('Starting trading monitoring', {
      contracts,
      interval
    });

    this.monitoringInterval = setInterval(async () => {
      await this.checkTradingStatus(contracts);
    }, interval);

    this.metrics.recordGauge('trading_monitor_status', 1);
  }

  async checkTradingStatus(contracts: string[]) {
    const checkContext = {
      timestamp: new Date().toISOString(),
      contracts
    };

    this.logger.info('Checking trading status', checkContext);

    for (const contract of contracts) {
      const timer = this.metrics.startTimer(
        'trading_status_check_duration_seconds',
        { contract }
      );

      try {
        // Get active automations
        const automations = await this.automation.listAutomations({
          contractAddress: contract,
          types: ['execute_trade', 'manage_position']
        });

        // Check automation health
        for (const automation of automations) {
          this.metrics.recordGauge(
            'trading_automation_status',
            automation.status === 'active' ? 1 : 0,
            {
              contract,
              automation: automation.id,
              type: automation.type
            }
          );

          // Check recent executions
          const recentExecutions = await this.automation.getExecutions({
            automationIds: [automation.id],
            limit: 5
          });

          const failedExecutions = recentExecutions.filter(
            e => e.status === 'failed'
          );

          if (failedExecutions.length > 2) {
            await this.alertService.sendAlert({
              type: 'AUTOMATION_FAILURES',
              severity: 'high',
              message: \`Multiple trading automation failures for \${contract}\`,
              data: {
                contract,
                automationId: automation.id,
                failures: failedExecutions
              }
            });
          }
        }

        // Get current positions
        const positions = await getContractPositions(contract);

        // Check position health
        for (const position of positions) {
          const currentPrice = await this.priceFeed.getLatestPrice(
            ...position.pair.split('/')
          );

          const unrealizedPnl = calculateUnrealizedPnl(
            position,
            currentPrice.value
          );

          this.metrics.recordGauge('position_unrealized_pnl', unrealizedPnl, {
            contract,
            pair: position.pair
          });

          // Alert on significant losses
          if (unrealizedPnl < position.stopLoss) {
            await this.alertService.sendAlert({
              type: 'POSITION_STOP_LOSS',
              severity: 'high',
              message: \`Position reached stop loss for \${contract}\`,
              data: {
                contract,
                pair: position.pair,
                unrealizedPnl,
                stopLoss: position.stopLoss
              }
            });
          }
        }

        // Check strategy performance
        const performance = await getStrategyPerformance(contract);
        
        this.metrics.recordGauge('strategy_win_rate', performance.winRate, {
          contract
        });
        
        this.metrics.recordGauge('strategy_pnl', performance.pnl, {
          contract
        });

        // Alert on poor performance
        if (performance.winRate < 0.4) {
          await this.alertService.sendAlert({
            type: 'POOR_STRATEGY_PERFORMANCE',
            severity: 'medium',
            message: \`Low win rate detected for \${contract}\`,
            data: {
              contract,
              winRate: performance.winRate,
              pnl: performance.pnl
            }
          });
        }

        this.logger.info('Trading status check completed', {
          ...checkContext,
          contract,
          automationCount: automations.length,
          positionCount: positions.length,
          performance
        });
      } catch (error) {
        this.metrics.incrementCounter('trading_monitor_errors_total', {
          contract,
          error_type: error.name
        });

        this.logger.error('Trading status check failed', {
          ...checkContext,
          contract,
          error: {
            name: error.name,
            message: error.message,
            stack: error.stack
          }
        });

        await this.alertService.sendAlert({
          type: 'MONITORING_ERROR',
          severity: 'high',
          message: \`Failed to check trading status for \${contract}\`,
          data: {
            contract,
            error: error.message
          }
        });
      } finally {
        timer.end();
      }
    }
  }

  stop() {
    if (this.monitoringInterval) {
      clearInterval(this.monitoringInterval);
      this.monitoringInterval = null;
      
      this.metrics.recordGauge('trading_monitor_status', 0);
      
      this.logger.info('Trading monitoring stopped');
    }
  }
}

function calculateUnrealizedPnl(position: any, currentPrice: number): number {
  // Implementation would calculate unrealized PnL based on position details
  return 0;
}
```

### Testing the Integrated System

```typescript
import { handler } from './trading-automation';
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { SecretVault } from '../../utils/vault';
import { ContractAutomation } from '../../services/contract-automation';
import { PriceFeedService } from '../../services/price-feed';
import { verifyNeoSignature } from '../../utils/auth';

jest.mock('../../utils/logger');
jest.mock('../../services/metrics');
jest.mock('../../utils/vault');
jest.mock('../../services/contract-automation');
jest.mock('../../services/price-feed');
jest.mock('../../utils/auth');

describe('Integrated Trading Automation', () => {
  let mockLogger: jest.Mocked<Logger>;
  let mockMetrics: jest.Mocked<MetricsService>;
  let mockVault: jest.Mocked<SecretVault>;
  let mockAutomation: jest.Mocked<ContractAutomation>;
  let mockPriceFeed: jest.Mocked<PriceFeedService>;

  beforeEach(() => {
    jest.clearAllMocks();

    mockLogger = {
      info: jest.fn(),
      warn: jest.fn(),
      error: jest.fn(),
      child: jest.fn().mockReturnThis()
    } as any;

    mockMetrics = {
      startTimer: jest.fn().mockReturnValue({
        end: jest.fn(),
        getDuration: jest.fn().mockReturnValue(100)
      }),
      incrementCounter: jest.fn(),
      recordGauge: jest.fn()
    } as any;

    mockVault = {
      getSecret: jest.fn().mockResolvedValue({
        value: JSON.stringify({
          nodeUrl: 'test-node',
          privateKey: 'test-key'
        })
      })
    } as any;

    mockAutomation = {
      createMonitor: jest.fn().mockResolvedValue({
        id: 'monitor-1',
        conditions: []
      }),
      createAutomation: jest.fn().mockResolvedValue({
        id: 'automation-1',
        action: { type: 'execute_trade' }
      }),
      listMonitors: jest.fn().mockResolvedValue([]),
      listAutomations: jest.fn().mockResolvedValue([]),
      getExecutions: jest.fn().mockResolvedValue([])
    } as any;

    mockPriceFeed = {
      getLatestPrice: jest.fn().mockResolvedValue({
        value: 50.0,
        timestamp: new Date().toISOString()
      })
    } as any;

    (verifyNeoSignature as jest.Mock).mockResolvedValue(true);
  });

  it('successfully sets up trading strategy', async () => {
    const event = {
      httpMethod: 'POST',
      path: '/api/trading-automation',
      headers: {
        'x-neo-address': 'test-address',
        'x-neo-signature': 'test-signature',
        'x-timestamp': Date.now().toString()
      },
      body: JSON.stringify({
        action: 'setup_strategy',
        contractAddress: 'test-contract',
        strategy: {
          type: 'momentum',
          pairs: ['NEO/USD'],
          maxPositionSize: 100,
          stopLoss: -10,
          parameters: {
            entryThreshold: 2,
            exitThreshold: 1,
            lookbackPeriod: 24
          }
        }
      })
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(200);
    const body = JSON.parse(response.body);
    expect(body.message).toBe('Trading strategy setup successful');
    expect(body.config).toHaveProperty('strategyId');
    expect(body.config).toHaveProperty('monitors');
    expect(body.config).toHaveProperty('automations');

    // Verify service calls
    expect(mockVault.getSecret).toHaveBeenCalledWith(
      'trading_credentials'
    );
    expect(mockAutomation.createMonitor).toHaveBeenCalled();
    expect(mockAutomation.createAutomation).toHaveBeenCalled();

    // Verify logging
    expect(mockLogger.info).toHaveBeenCalledWith(
      'Trading strategy setup completed',
      expect.any(Object)
    );

    // Verify metrics
    expect(mockMetrics.incrementCounter).toHaveBeenCalledWith(
      'trading_strategy_setups_total',
      expect.any(Object)
    );
  });

  it('successfully retrieves trading status', async () => {
    const event = {
      httpMethod: 'POST',
      path: '/api/trading-automation',
      headers: {
        'x-neo-address': 'test-address',
        'x-neo-signature': 'test-signature',
        'x-timestamp': Date.now().toString()
      },
      body: JSON.stringify({
        action: 'get_status',
        contractAddress: 'test-contract'
      })
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(200);
    const body = JSON.parse(response.body);
    expect(body).toHaveProperty('monitors');
    expect(body).toHaveProperty('automations');
    expect(body).toHaveProperty('recentExecutions');
    expect(body).toHaveProperty('positions');
    expect(body).toHaveProperty('performance');

    // Verify service calls
    expect(mockAutomation.listMonitors).toHaveBeenCalled();
    expect(mockAutomation.listAutomations).toHaveBeenCalled();
    expect(mockPriceFeed.getLatestPrice).toHaveBeenCalled();
  });

  it('handles invalid strategy parameters', async () => {
    const event = {
      httpMethod: 'POST',
      path: '/api/trading-automation',
      headers: {
        'x-neo-address': 'test-address',
        'x-neo-signature': 'test-signature',
        'x-timestamp': Date.now().toString()
      },
      body: JSON.stringify({
        action: 'setup_strategy',
        contractAddress: 'test-contract',
        strategy: {
          type: 'momentum',
          // Missing required parameters
          pairs: ['NEO/USD']
        }
      })
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(500);
    expect(mockMetrics.incrementCounter).toHaveBeenCalledWith(
      'trading_automation_errors_total',
      { error_type: 'Error' }
    );
    expect(mockLogger.error).toHaveBeenCalledWith(
      'Trading automation request failed',
      expect.any(Object)
    );
  });

  it('handles automation setup failures', async () => {
    mockAutomation.createMonitor.mockRejectedValue(
      new Error('Automation error')
    );

    const event = {
      httpMethod: 'POST',
      path: '/api/trading-automation',
      headers: {
        'x-neo-address': 'test-address',
        'x-neo-signature': 'test-signature',
        'x-timestamp': Date.now().toString()
      },
      body: JSON.stringify({
        action: 'setup_strategy',
        contractAddress: 'test-contract',
        strategy: {
          type: 'momentum',
          pairs: ['NEO/USD'],
          maxPositionSize: 100,
          stopLoss: -10,
          parameters: {
            entryThreshold: 2,
            exitThreshold: 1,
            lookbackPeriod: 24
          }
        }
      })
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(500);
    expect(mockMetrics.incrementCounter).toHaveBeenCalledWith(
      'trading_automation_errors_total',
      { error_type: 'Error' }
    );
    expect(mockLogger.error).toHaveBeenCalledWith(
      'Trading automation request failed',
      expect.any(Object)
    );
  });
});
```

## Key Integration Points

1. **Contract Automation and Price Feeds**
   - Price-based triggers for trading
   - Position monitoring and management
   - Automated trade execution
   - Performance tracking

2. **Security and Authentication**
   - Neo wallet signature verification
   - Secure credential management
   - Trading permissions
   - Position limits

3. **Monitoring and Alerts**
   - Strategy performance monitoring
   - Position risk tracking
   - Automation health checks
   - Alert integration

4. **Metrics and Logging**
   - Trading metrics
   - Performance analytics
   - Execution tracking
   - Error monitoring

## Best Practices Demonstrated

1. **Security**
   - Signature verification
   - Secure credential storage
   - Position limits
   - Risk management

2. **Reliability**
   - Error handling
   - Strategy validation
   - Monitoring and alerts
   - Performance tracking

3. **Observability**
   - Comprehensive logging
   - Performance metrics
   - Strategy analytics
   - Health monitoring

4. **Performance**
   - Efficient monitoring
   - Optimized triggers
   - Resource management
   - Execution tracking

5. **Maintainability**
   - Modular strategy design
   - Clear separation of concerns
   - Comprehensive testing
   - Strategy validation