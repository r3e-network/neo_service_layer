# Integrated Gas Bank Automation Example

## Overview
This example demonstrates how to integrate Gas Bank, Contract Automation, and Functions services to create an automated gas management system that:
- Monitors contract gas usage
- Automatically replenishes gas when needed
- Tracks gas consumption patterns
- Alerts on unusual activity
- Provides API endpoints for gas management

## Complete Integration Example

### Gas Management Function
```typescript
import { Handler } from '@netlify/functions';
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { SecretVault } from '../../utils/vault';
import { GasBank } from '../../services/gas-bank';
import { ContractAutomation } from '../../services/contract-automation';
import { verifyNeoSignature } from '../../utils/auth';

// Initialize services
const logger = Logger.getInstance().child({ service: 'gas-management' });
const metrics = new MetricsService({
  namespace: 'neo_service_layer',
  subsystem: 'gas_management'
});
const vault = new SecretVault({
  teeEnabled: true,
  backupEnabled: true,
  rotationPeriod: 24 * 60 * 60 * 1000
});

export const handler: Handler = async (event, context) => {
  const timer = metrics.startTimer('gas_management_request_duration_seconds');
  
  const requestContext = {
    requestId: context.awsRequestId,
    path: event.path,
    method: event.httpMethod,
    timestamp: new Date().toISOString()
  };

  try {
    // Log incoming request
    logger.info('Processing gas management request', {
      ...requestContext,
      queryParams: event.queryStringParameters
    });

    // Verify authentication
    const neoAddress = event.headers['x-neo-address'];
    const signature = event.headers['x-neo-signature'];
    const timestamp = event.headers['x-timestamp'];

    if (!neoAddress || !signature || !timestamp) {
      metrics.incrementCounter('gas_management_auth_errors_total', {
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
      metrics.incrementCounter('gas_management_auth_errors_total', {
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
    const credentials = await vault.getSecret('neo_network_credentials');
    const gasBank = new GasBank(JSON.parse(credentials.value));
    const automation = new ContractAutomation();

    // Parse request body
    const body = JSON.parse(event.body || '{}');
    const { contractAddress, action } = body;

    switch (action) {
      case 'setup_automation': {
        // Setup automated gas management for a contract
        const config = await setupGasAutomation(
          gasBank,
          automation,
          contractAddress,
          neoAddress
        );

        logger.info('Gas automation setup completed', {
          ...requestContext,
          contractAddress,
          config
        });

        return {
          statusCode: 200,
          body: JSON.stringify({
            message: 'Gas automation setup successful',
            config
          })
        };
      }

      case 'get_status': {
        // Get current gas management status
        const status = await getGasManagementStatus(
          gasBank,
          automation,
          contractAddress
        );

        logger.info('Gas status retrieved', {
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
    metrics.incrementCounter('gas_management_errors_total', {
      error_type: error.name
    });

    logger.error('Gas management request failed', {
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
        error: 'Failed to process gas management request',
        requestId: context.awsRequestId
      })
    };
  } finally {
    timer.end();
  }
};

async function setupGasAutomation(
  gasBank: GasBank,
  automation: ContractAutomation,
  contractAddress: string,
  ownerAddress: string
) {
  const logger = Logger.getInstance().child({
    component: 'gas-automation-setup'
  });
  
  const metrics = new MetricsService({
    namespace: 'neo_service_layer',
    subsystem: 'gas_automation'
  });

  try {
    // Configure gas monitoring
    const monitorConfig = await automation.createMonitor({
      contractAddress,
      checkInterval: 300, // 5 minutes
      conditions: [
        {
          type: 'gas_balance',
          threshold: '10', // GAS units
          operator: 'less_than'
        }
      ]
    });

    // Configure automated gas replenishment
    const automationConfig = await automation.createAutomation({
      trigger: monitorConfig.id,
      action: {
        type: 'replenish_gas',
        params: {
          amount: '50', // GAS units
          source: 'gas_bank'
        }
      },
      maxExecutions: -1, // Unlimited
      cooldownPeriod: 3600 // 1 hour
    });

    // Setup gas bank allowance
    await gasBank.setupAllowance(contractAddress, {
      maxAmount: '1000', // Maximum GAS that can be withdrawn
      periodSeconds: 86400, // Per day
      ownerAddress
    });

    // Record setup metrics
    metrics.incrementCounter('gas_automation_setups_total', {
      contract_address: contractAddress
    });

    logger.info('Gas automation setup completed', {
      contractAddress,
      monitorConfig,
      automationConfig
    });

    return {
      monitorId: monitorConfig.id,
      automationId: automationConfig.id,
      thresholds: monitorConfig.conditions,
      replenishmentConfig: automationConfig.action
    };
  } catch (error) {
    metrics.incrementCounter('gas_automation_setup_errors_total', {
      error_type: error.name
    });

    logger.error('Failed to setup gas automation', {
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

async function getGasManagementStatus(
  gasBank: GasBank,
  automation: ContractAutomation,
  contractAddress: string
) {
  const logger = Logger.getInstance().child({
    component: 'gas-management-status'
  });

  try {
    // Get current gas balance
    const balance = await gasBank.getBalance(contractAddress);

    // Get automation status
    const monitors = await automation.listMonitors({
      contractAddress,
      type: 'gas_balance'
    });

    const automations = await automation.listAutomations({
      contractAddress,
      actionType: 'replenish_gas'
    });

    // Get recent executions
    const recentExecutions = await automation.getExecutions({
      automationIds: automations.map(a => a.id),
      limit: 10
    });

    // Get gas usage history
    const usageHistory = await gasBank.getUsageHistory(contractAddress, {
      periodDays: 7
    });

    return {
      currentBalance: balance,
      monitors: monitors.map(m => ({
        id: m.id,
        conditions: m.conditions,
        lastCheck: m.lastCheck,
        status: m.status
      })),
      automations: automations.map(a => ({
        id: a.id,
        trigger: a.trigger,
        action: a.action,
        status: a.status
      })),
      recentExecutions: recentExecutions.map(e => ({
        timestamp: e.timestamp,
        status: e.status,
        gasAmount: e.params.amount
      })),
      usageHistory: usageHistory.map(h => ({
        date: h.date,
        amount: h.amount,
        transactions: h.transactionCount
      }))
    };
  } catch (error) {
    logger.error('Failed to get gas management status', {
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
```

### Gas Usage Monitor Component
```typescript
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { GasBank } from '../../services/gas-bank';
import { ContractAutomation } from '../../services/contract-automation';
import { AlertService } from '../../services/alerts';

class GasUsageMonitor {
  private logger: Logger;
  private metrics: MetricsService;
  private gasBank: GasBank;
  private automation: ContractAutomation;
  private alertService: AlertService;
  private monitoringInterval: NodeJS.Timeout | null = null;

  constructor(
    gasBank: GasBank,
    automation: ContractAutomation
  ) {
    this.logger = Logger.getInstance().child({
      component: 'GasUsageMonitor'
    });
    
    this.metrics = new MetricsService({
      namespace: 'neo_service_layer',
      subsystem: 'gas_monitor'
    });
    
    this.gasBank = gasBank;
    this.automation = automation;
    this.alertService = new AlertService();
  }

  async startMonitoring(contracts: string[], interval: number) {
    this.logger.info('Starting gas usage monitoring', {
      contracts,
      interval
    });

    this.monitoringInterval = setInterval(async () => {
      await this.checkGasUsage(contracts);
    }, interval);

    this.metrics.recordGauge('gas_monitor_status', 1);
  }

  async checkGasUsage(contracts: string[]) {
    const checkContext = {
      timestamp: new Date().toISOString(),
      contracts
    };

    this.logger.info('Checking gas usage', checkContext);

    for (const contract of contracts) {
      const timer = this.metrics.startTimer(
        'gas_usage_check_duration_seconds',
        { contract }
      );

      try {
        // Get current gas balance
        const balance = await this.gasBank.getBalance(contract);
        
        // Record balance metric
        this.metrics.recordGauge('contract_gas_balance', balance, {
          contract
        });

        // Get recent usage
        const usage = await this.gasBank.getUsageHistory(contract, {
          periodDays: 1
        });

        // Calculate usage rate
        const usageRate = usage.reduce(
          (total, entry) => total + entry.amount,
          0
        ) / usage.length;

        this.metrics.recordGauge('contract_gas_usage_rate', usageRate, {
          contract
        });

        // Check for unusual usage patterns
        const averageUsage = await this.getAverageUsage(contract);
        if (usageRate > averageUsage * 2) {
          await this.alertService.sendAlert({
            type: 'HIGH_GAS_USAGE',
            severity: 'warning',
            message: \`High gas usage detected for contract \${contract}\`,
            data: {
              contract,
              currentUsage: usageRate,
              averageUsage,
              timestamp: new Date().toISOString()
            }
          });
        }

        // Check automation status
        const automations = await this.automation.listAutomations({
          contractAddress: contract,
          actionType: 'replenish_gas'
        });

        for (const automation of automations) {
          this.metrics.recordGauge(
            'gas_automation_status',
            automation.status === 'active' ? 1 : 0,
            {
              contract,
              automation: automation.id
            }
          );

          // Check for failed executions
          const recentExecutions = await this.automation.getExecutions({
            automationIds: [automation.id],
            limit: 5
          });

          const failedExecutions = recentExecutions.filter(
            e => e.status === 'failed'
          );

          if (failedExecutions.length > 3) {
            await this.alertService.sendAlert({
              type: 'AUTOMATION_FAILURES',
              severity: 'high',
              message: \`Multiple gas automation failures for \${contract}\`,
              data: {
                contract,
                automationId: automation.id,
                failures: failedExecutions
              }
            });
          }
        }

        this.logger.info('Gas usage check completed', {
          ...checkContext,
          contract,
          balance,
          usageRate
        });
      } catch (error) {
        this.metrics.incrementCounter('gas_monitor_errors_total', {
          contract,
          error_type: error.name
        });

        this.logger.error('Gas usage check failed', {
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
          message: \`Failed to check gas usage for \${contract}\`,
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

  private async getAverageUsage(
    contract: string,
    days: number = 7
  ): Promise<number> {
    const history = await this.gasBank.getUsageHistory(contract, {
      periodDays: days
    });

    return history.reduce(
      (total, entry) => total + entry.amount,
      0
    ) / history.length;
  }

  stop() {
    if (this.monitoringInterval) {
      clearInterval(this.monitoringInterval);
      this.monitoringInterval = null;
      
      this.metrics.recordGauge('gas_monitor_status', 0);
      
      this.logger.info('Gas usage monitoring stopped');
    }
  }
}
```

### Testing the Integrated System

```typescript
import { handler } from './gas-management';
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { SecretVault } from '../../utils/vault';
import { GasBank } from '../../services/gas-bank';
import { ContractAutomation } from '../../services/contract-automation';
import { verifyNeoSignature } from '../../utils/auth';

jest.mock('../../utils/logger');
jest.mock('../../services/metrics');
jest.mock('../../utils/vault');
jest.mock('../../services/gas-bank');
jest.mock('../../services/contract-automation');
jest.mock('../../utils/auth');

describe('Integrated Gas Management', () => {
  let mockLogger: jest.Mocked<Logger>;
  let mockMetrics: jest.Mocked<MetricsService>;
  let mockVault: jest.Mocked<SecretVault>;
  let mockGasBank: jest.Mocked<GasBank>;
  let mockAutomation: jest.Mocked<ContractAutomation>;

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

    mockGasBank = {
      setupAllowance: jest.fn().mockResolvedValue(true),
      getBalance: jest.fn().mockResolvedValue('100'),
      getUsageHistory: jest.fn().mockResolvedValue([
        { date: '2024-01-01', amount: '10', transactionCount: 5 }
      ])
    } as any;

    mockAutomation = {
      createMonitor: jest.fn().mockResolvedValue({
        id: 'monitor-1',
        conditions: [{ type: 'gas_balance', threshold: '10' }]
      }),
      createAutomation: jest.fn().mockResolvedValue({
        id: 'automation-1',
        action: { type: 'replenish_gas', params: { amount: '50' } }
      }),
      listMonitors: jest.fn().mockResolvedValue([]),
      listAutomations: jest.fn().mockResolvedValue([]),
      getExecutions: jest.fn().mockResolvedValue([])
    } as any;

    (verifyNeoSignature as jest.Mock).mockResolvedValue(true);
  });

  it('successfully sets up gas automation', async () => {
    const event = {
      httpMethod: 'POST',
      path: '/api/gas-management',
      headers: {
        'x-neo-address': 'test-address',
        'x-neo-signature': 'test-signature',
        'x-timestamp': Date.now().toString()
      },
      body: JSON.stringify({
        action: 'setup_automation',
        contractAddress: 'test-contract'
      })
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(200);
    const body = JSON.parse(response.body);
    expect(body.message).toBe('Gas automation setup successful');
    expect(body.config).toHaveProperty('monitorId');
    expect(body.config).toHaveProperty('automationId');

    // Verify service calls
    expect(mockVault.getSecret).toHaveBeenCalledWith(
      'neo_network_credentials'
    );
    expect(mockAutomation.createMonitor).toHaveBeenCalled();
    expect(mockAutomation.createAutomation).toHaveBeenCalled();
    expect(mockGasBank.setupAllowance).toHaveBeenCalled();

    // Verify logging
    expect(mockLogger.info).toHaveBeenCalledWith(
      'Gas automation setup completed',
      expect.any(Object)
    );

    // Verify metrics
    expect(mockMetrics.startTimer).toHaveBeenCalledWith(
      'gas_management_request_duration_seconds'
    );
  });

  it('successfully retrieves gas management status', async () => {
    const event = {
      httpMethod: 'POST',
      path: '/api/gas-management',
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
    expect(body).toHaveProperty('currentBalance');
    expect(body).toHaveProperty('monitors');
    expect(body).toHaveProperty('automations');
    expect(body).toHaveProperty('recentExecutions');
    expect(body).toHaveProperty('usageHistory');

    // Verify service calls
    expect(mockGasBank.getBalance).toHaveBeenCalled();
    expect(mockAutomation.listMonitors).toHaveBeenCalled();
    expect(mockAutomation.listAutomations).toHaveBeenCalled();
    expect(mockGasBank.getUsageHistory).toHaveBeenCalled();
  });

  it('handles authentication errors', async () => {
    const event = {
      httpMethod: 'POST',
      path: '/api/gas-management',
      headers: {}, // Missing auth headers
      body: JSON.stringify({
        action: 'get_status',
        contractAddress: 'test-contract'
      })
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(401);
    expect(mockMetrics.incrementCounter).toHaveBeenCalledWith(
      'gas_management_auth_errors_total',
      { error_type: 'missing_headers' }
    );
    expect(mockLogger.warn).toHaveBeenCalledWith(
      'Missing authentication headers',
      expect.any(Object)
    );
  });

  it('handles service errors gracefully', async () => {
    mockGasBank.setupAllowance.mockRejectedValue(
      new Error('Gas bank error')
    );

    const event = {
      httpMethod: 'POST',
      path: '/api/gas-management',
      headers: {
        'x-neo-address': 'test-address',
        'x-neo-signature': 'test-signature',
        'x-timestamp': Date.now().toString()
      },
      body: JSON.stringify({
        action: 'setup_automation',
        contractAddress: 'test-contract'
      })
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(500);
    expect(mockMetrics.incrementCounter).toHaveBeenCalledWith(
      'gas_management_errors_total',
      { error_type: 'Error' }
    );
    expect(mockLogger.error).toHaveBeenCalledWith(
      'Gas management request failed',
      expect.any(Object)
    );
  });
});
```

## Key Integration Points

1. **Gas Bank and Contract Automation**
   - Automated gas replenishment based on balance thresholds
   - Configurable monitoring and automation rules
   - Gas usage tracking and analysis

2. **Authentication and Security**
   - Neo wallet signature verification
   - Secure credential management
   - Gas allowance controls

3. **Monitoring and Alerting**
   - Gas balance monitoring
   - Usage pattern analysis
   - Automation failure detection
   - Alert integration

4. **Metrics and Logging**
   - Gas usage metrics
   - Automation performance tracking
   - Structured logging
   - Error tracking

## Best Practices Demonstrated

1. **Security**
   - Signature verification
   - Secure credential storage
   - Gas allowance limits
   - Input validation

2. **Reliability**
   - Error handling
   - Service recovery
   - Monitoring and alerts
   - Usage pattern analysis

3. **Observability**
   - Comprehensive logging
   - Detailed metrics
   - Performance tracking
   - Usage analytics

4. **Performance**
   - Efficient gas management
   - Optimized monitoring
   - Resource cleanup
   - Caching considerations

5. **Maintainability**
   - Modular design
   - Clear separation of concerns
   - Comprehensive testing
   - Documentation