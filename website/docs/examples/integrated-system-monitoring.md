# Integrated System Monitoring Example

## Overview
This example demonstrates how to integrate Functions, Metrics, and Logging services to create a comprehensive system monitoring solution that:
- Tracks system health across all services
- Provides real-time metrics and alerts
- Maintains detailed audit logs
- Offers performance analytics
- Enables system diagnostics

## Complete Integration Example

### System Health Function
```typescript
import { Handler } from '@netlify/functions';
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { SecretVault } from '../../utils/vault';
import { AlertService } from '../../services/alerts';
import { SystemHealth } from '../../services/system-health';

// Initialize services
const logger = Logger.getInstance().child({ service: 'system-health' });
const metrics = new MetricsService({
  namespace: 'neo_service_layer',
  subsystem: 'system_health'
});
const vault = new SecretVault({
  teeEnabled: true,
  backupEnabled: true,
  rotationPeriod: 24 * 60 * 60 * 1000
});

export const handler: Handler = async (event, context) => {
  const timer = metrics.startTimer('health_check_duration_seconds');
  
  const requestContext = {
    requestId: context.awsRequestId,
    path: event.path,
    method: event.httpMethod,
    timestamp: new Date().toISOString()
  };

  try {
    // Log incoming request
    logger.info('Processing system health request', {
      ...requestContext,
      queryParams: event.queryStringParameters
    });

    // Initialize system health service
    const systemHealth = new SystemHealth();

    // Parse request parameters
    const { service, component } = event.queryStringParameters || {};

    // Get system health metrics
    const healthMetrics = await getSystemHealth(
      systemHealth,
      service,
      component
    );

    // Record metrics
    Object.entries(healthMetrics.metrics).forEach(([key, value]) => {
      metrics.recordGauge(\`system_health_\${key}\`, value.current, {
        service: healthMetrics.service,
        component: healthMetrics.component
      });
    });

    // Log health check completion
    logger.info('System health check completed', {
      ...requestContext,
      service,
      component,
      status: healthMetrics.status
    });

    return {
      statusCode: 200,
      body: JSON.stringify(healthMetrics)
    };
  } catch (error) {
    // Record error metrics
    metrics.incrementCounter('health_check_errors_total', {
      error_type: error.name
    });

    // Log error details
    logger.error('System health check failed', {
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
        error: 'Failed to check system health',
        requestId: context.awsRequestId
      })
    };
  } finally {
    timer.end();
  }
};

async function getSystemHealth(
  systemHealth: SystemHealth,
  service?: string,
  component?: string
) {
  const metrics = new MetricsService({
    namespace: 'neo_service_layer',
    subsystem: 'health_check'
  });

  const timer = metrics.startTimer('health_check_operation_duration_seconds', {
    service: service || 'all',
    component: component || 'all'
  });

  try {
    // Get service health metrics
    const healthMetrics = await systemHealth.checkHealth(service, component);

    // Record service-specific metrics
    Object.entries(healthMetrics.metrics).forEach(([key, value]) => {
      metrics.recordGauge(\`service_\${key}\`, value.current, {
        service: healthMetrics.service,
        component: healthMetrics.component
      });

      // Record historical trends
      if (value.history) {
        metrics.recordHistogram(\`service_\${key}_history\`, value.history, {
          service: healthMetrics.service,
          component: healthMetrics.component
        });
      }
    });

    return healthMetrics;
  } finally {
    timer.end();
  }
}
```

### System Health Monitor Component
```typescript
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { AlertService } from '../../services/alerts';
import { SystemHealth } from '../../services/system-health';

class SystemHealthMonitor {
  private logger: Logger;
  private metrics: MetricsService;
  private alertService: AlertService;
  private systemHealth: SystemHealth;
  private monitoringInterval: NodeJS.Timeout | null = null;

  constructor() {
    this.logger = Logger.getInstance().child({
      component: 'SystemHealthMonitor'
    });
    
    this.metrics = new MetricsService({
      namespace: 'neo_service_layer',
      subsystem: 'health_monitor'
    });
    
    this.alertService = new AlertService();
    this.systemHealth = new SystemHealth();
  }

  async startMonitoring(interval: number) {
    this.logger.info('Starting system health monitoring', {
      interval
    });

    this.monitoringInterval = setInterval(async () => {
      await this.checkSystemHealth();
    }, interval);

    this.metrics.recordGauge('health_monitor_status', 1);
  }

  async checkSystemHealth() {
    const checkContext = {
      timestamp: new Date().toISOString()
    };

    this.logger.info('Checking system health', checkContext);

    try {
      // Get overall system health
      const healthMetrics = await this.systemHealth.checkHealth();

      // Process each service's health metrics
      for (const service of healthMetrics.services) {
        const serviceTimer = this.metrics.startTimer(
          'service_health_check_duration_seconds',
          { service: service.name }
        );

        try {
          // Record service status
          this.metrics.recordGauge('service_health_status', 
            service.status === 'healthy' ? 1 : 0,
            { service: service.name }
          );

          // Process component metrics
          for (const component of service.components) {
            // Record component status
            this.metrics.recordGauge('component_health_status',
              component.status === 'healthy' ? 1 : 0,
              {
                service: service.name,
                component: component.name
              }
            );

            // Record component metrics
            Object.entries(component.metrics).forEach(([key, value]) => {
              this.metrics.recordGauge(\`component_\${key}\`, value.current, {
                service: service.name,
                component: component.name
              });
            });

            // Check for component issues
            if (component.status !== 'healthy') {
              await this.alertService.sendAlert({
                type: 'COMPONENT_UNHEALTHY',
                severity: component.status === 'degraded' ? 'warning' : 'high',
                message: \`Component health issue detected: \${component.name}\`,
                data: {
                  service: service.name,
                  component: component.name,
                  status: component.status,
                  metrics: component.metrics
                }
              });
            }
          }

          // Check for service issues
          if (service.status !== 'healthy') {
            await this.alertService.sendAlert({
              type: 'SERVICE_UNHEALTHY',
              severity: service.status === 'degraded' ? 'warning' : 'high',
              message: \`Service health issue detected: \${service.name}\`,
              data: {
                service: service.name,
                status: service.status,
                components: service.components
                  .filter(c => c.status !== 'healthy')
                  .map(c => ({
                    name: c.name,
                    status: c.status
                  }))
              }
            });
          }
        } finally {
          serviceTimer.end();
        }
      }

      // Record overall system health
      this.metrics.recordGauge('system_health_status',
        healthMetrics.status === 'healthy' ? 1 : 0
      );

      this.logger.info('System health check completed', {
        ...checkContext,
        status: healthMetrics.status,
        services: healthMetrics.services.length
      });
    } catch (error) {
      this.metrics.incrementCounter('health_monitor_errors_total', {
        error_type: error.name
      });

      this.logger.error('System health check failed', {
        ...checkContext,
        error: {
          name: error.name,
          message: error.message,
          stack: error.stack
        }
      });

      await this.alertService.sendAlert({
        type: 'HEALTH_CHECK_ERROR',
        severity: 'high',
        message: 'Failed to perform system health check',
        data: {
          error: error.message,
          timestamp: new Date().toISOString()
        }
      });
    }
  }

  stop() {
    if (this.monitoringInterval) {
      clearInterval(this.monitoringInterval);
      this.monitoringInterval = null;
      
      this.metrics.recordGauge('health_monitor_status', 0);
      
      this.logger.info('System health monitoring stopped');
    }
  }
}
```

### Health Metrics Aggregator
```typescript
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';

class HealthMetricsAggregator {
  private logger: Logger;
  private metrics: MetricsService;

  constructor() {
    this.logger = Logger.getInstance().child({
      component: 'HealthMetricsAggregator'
    });
    
    this.metrics = new MetricsService({
      namespace: 'neo_service_layer',
      subsystem: 'health_metrics'
    });
  }

  async aggregateMetrics(timeRange: {
    start: Date;
    end: Date;
  }) {
    const aggregationContext = {
      timeRange,
      timestamp: new Date().toISOString()
    };

    this.logger.info('Starting health metrics aggregation', aggregationContext);

    try {
      // Get raw metrics
      const rawMetrics = await this.metrics.queryRange({
        metrics: [
          'system_health_status',
          'service_health_status',
          'component_health_status'
        ],
        start: timeRange.start,
        end: timeRange.end,
        step: '5m'
      });

      // Calculate system health score
      const systemHealthScore = calculateHealthScore(
        rawMetrics.system_health_status
      );

      // Calculate service health scores
      const serviceHealthScores = calculateServiceHealthScores(
        rawMetrics.service_health_status
      );

      // Calculate component health scores
      const componentHealthScores = calculateComponentHealthScores(
        rawMetrics.component_health_status
      );

      // Record aggregated metrics
      this.metrics.recordGauge('system_health_score', systemHealthScore);

      serviceHealthScores.forEach((score, service) => {
        this.metrics.recordGauge('service_health_score', score, {
          service
        });
      });

      componentHealthScores.forEach((score, key) => {
        const [service, component] = key.split(':');
        this.metrics.recordGauge('component_health_score', score, {
          service,
          component
        });
      });

      return {
        systemHealthScore,
        serviceHealthScores: Object.fromEntries(serviceHealthScores),
        componentHealthScores: Object.fromEntries(componentHealthScores),
        timeRange
      };
    } catch (error) {
      this.logger.error('Health metrics aggregation failed', {
        ...aggregationContext,
        error: {
          name: error.name,
          message: error.message,
          stack: error.stack
        }
      });

      throw error;
    }
  }
}

function calculateHealthScore(metrics: any[]): number {
  // Implementation would calculate a health score from metrics
  return 0;
}

function calculateServiceHealthScores(metrics: any[]): Map<string, number> {
  // Implementation would calculate health scores per service
  return new Map();
}

function calculateComponentHealthScores(metrics: any[]): Map<string, number> {
  // Implementation would calculate health scores per component
  return new Map();
}
```

### Testing the Integrated System

```typescript
import { handler } from './system-health';
import { Logger } from '../../utils/logger';
import { MetricsService } from '../../services/metrics';
import { SystemHealth } from '../../services/system-health';

jest.mock('../../utils/logger');
jest.mock('../../services/metrics');
jest.mock('../../services/system-health');

describe('System Health Integration', () => {
  let mockLogger: jest.Mocked<Logger>;
  let mockMetrics: jest.Mocked<MetricsService>;
  let mockSystemHealth: jest.Mocked<SystemHealth>;

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

    mockSystemHealth = {
      checkHealth: jest.fn().mockResolvedValue({
        status: 'healthy',
        services: [
          {
            name: 'test-service',
            status: 'healthy',
            components: [
              {
                name: 'test-component',
                status: 'healthy',
                metrics: {
                  cpu: { current: 50 },
                  memory: { current: 70 }
                }
              }
            ]
          }
        ]
      })
    } as any;
  });

  it('successfully checks system health', async () => {
    const event = {
      httpMethod: 'GET',
      path: '/api/system-health',
      queryStringParameters: null
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(200);
    const body = JSON.parse(response.body);
    expect(body.status).toBe('healthy');

    // Verify service calls
    expect(mockSystemHealth.checkHealth).toHaveBeenCalled();

    // Verify metrics recording
    expect(mockMetrics.recordGauge).toHaveBeenCalledWith(
      'system_health_cpu',
      50,
      expect.any(Object)
    );
    expect(mockMetrics.recordGauge).toHaveBeenCalledWith(
      'system_health_memory',
      70,
      expect.any(Object)
    );

    // Verify logging
    expect(mockLogger.info).toHaveBeenCalledWith(
      'System health check completed',
      expect.any(Object)
    );
  });

  it('handles service-specific health checks', async () => {
    const event = {
      httpMethod: 'GET',
      path: '/api/system-health',
      queryStringParameters: {
        service: 'test-service',
        component: 'test-component'
      }
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(200);
    expect(mockSystemHealth.checkHealth).toHaveBeenCalledWith(
      'test-service',
      'test-component'
    );
  });

  it('handles health check errors', async () => {
    mockSystemHealth.checkHealth.mockRejectedValue(
      new Error('Health check failed')
    );

    const event = {
      httpMethod: 'GET',
      path: '/api/system-health',
      queryStringParameters: null
    };

    const context = {
      awsRequestId: 'test-request-id'
    };

    const response = await handler(event as any, context as any);

    expect(response.statusCode).toBe(500);
    expect(mockMetrics.incrementCounter).toHaveBeenCalledWith(
      'health_check_errors_total',
      { error_type: 'Error' }
    );
    expect(mockLogger.error).toHaveBeenCalledWith(
      'System health check failed',
      expect.any(Object)
    );
  });
});
```

## Key Integration Points

1. **Metrics and Logging Integration**
   - Comprehensive system metrics
   - Structured logging
   - Performance tracking
   - Error monitoring

2. **Health Monitoring**
   - Service health checks
   - Component monitoring
   - Performance metrics
   - Status tracking

3. **Alerting Integration**
   - Health status alerts
   - Performance degradation alerts
   - Error notifications
   - Threshold monitoring

4. **Metrics Aggregation**
   - System health scores
   - Service performance metrics
   - Component health tracking
   - Historical analysis

## Best Practices Demonstrated

1. **Monitoring**
   - Comprehensive health checks
   - Multi-level monitoring
   - Performance tracking
   - Status aggregation

2. **Reliability**
   - Error handling
   - Service recovery
   - Alert management
   - Performance monitoring

3. **Observability**
   - Detailed metrics
   - Structured logging
   - Health tracking
   - Performance analytics

4. **Performance**
   - Efficient monitoring
   - Optimized checks
   - Resource tracking
   - Metric aggregation

5. **Maintainability**
   - Modular design
   - Clear separation of concerns
   - Comprehensive testing
   - Documentation