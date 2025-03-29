'use client';

// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const installCode = `npm install @neo-service-layer/core
# or
yarn add @neo-service-layer/core`;

const initCode = `import { Client, Metrics } from '@neo-service-layer/core';

// Initialize client with your Neo N3 wallet
const client = new Client({
  signMessage: async (message) => {
    return await window.neo3Wallet.signMessage(message);
  },
});

// Create Metrics instance
const metrics = new Metrics(client);`;

const customMetricsCode = `// Record custom metrics
await metrics.recordMetric('order_value', 150.75, {
  orderId: '12345',
  currency: 'USD',
  customer: 'enterprise',
});

// Record multiple metrics
await metrics.recordMetrics([
  {
    name: 'transaction_time',
    value: 2.5,
    tags: { type: 'transfer', network: 'mainnet' }
  },
  {
    name: 'gas_used',
    value: 0.5,
    tags: { contract: 'token_sale', operation: 'mint' }
  }
]);

// Get custom metrics
const orderMetrics = await metrics.getMetrics('order_value', {
  from: new Date('2024-01-01'),
  to: new Date(),
  aggregation: 'avg',
  groupBy: ['currency', 'customer'],
  interval: '1h'
});`;

const systemMetricsCode = `// Get system metrics
const systemMetrics = await metrics.getSystemMetrics({
  services: ['price-feed', 'automation'],
  metrics: ['cpu_usage', 'memory_usage', 'request_count'],
  from: new Date('2024-01-01'),
  to: new Date(),
  interval: '5m'
});

// Get service health metrics
const healthMetrics = await metrics.getHealthMetrics({
  services: ['price-feed'],
  from: new Date('2024-01-01'),
  to: new Date()
});

// Get performance metrics
const perfMetrics = await metrics.getPerformanceMetrics({
  services: ['automation'],
  metrics: ['execution_time', 'success_rate'],
  from: new Date('2024-01-01'),
  to: new Date(),
  interval: '1h'
});`;

const alertsCode = `// Create metric alert
const alertId = await metrics.createAlert({
  name: 'high-gas-usage',
  description: 'Alert when gas usage exceeds threshold',
  metric: 'gas_used',
  condition: {
    operator: '>',
    value: 10,
    window: '5m'
  },
  actions: [
    {
      type: 'webhook',
      url: 'https://api.example.com/alerts',
      headers: {
        'Authorization': 'Bearer \${secret:ALERT_TOKEN}'
      }
    },
    {
      type: 'email',
      recipients: ['admin@example.com']
    }
  ]
});

// List alerts
const alerts = await metrics.listAlerts();

// Update alert
await metrics.updateAlert(alertId, {
  condition: {
    value: 15
  }
});

// Delete alert
await metrics.deleteAlert(alertId);

// Get alert history
const history = await metrics.getAlertHistory(alertId, {
  from: new Date('2024-01-01'),
  to: new Date(),
  limit: 100
});`;

export default function MetricsPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl lg:max-w-4xl">
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 dark:text-white">
            Metrics Service
          </h1>
          
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            The Metrics service provides comprehensive monitoring and analytics for your 
            Neo Service Layer applications. Track system performance, custom metrics, 
            and set up alerts for proactive monitoring.
          </p>

          <div className="mt-16 space-y-20">
            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Installation
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Install the Neo Service Layer SDK using npm or yarn:
              </p>
              <div className="mt-4">
                <CodeBlock code={installCode} language="bash" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Initialization
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Initialize the client and create a Metrics instance:
              </p>
              <div className="mt-4">
                <CodeBlock code={initCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Custom Metrics
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Record and retrieve custom metrics for your application:
              </p>
              <div className="mt-4">
                <CodeBlock code={customMetricsCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                System Metrics
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Monitor system and service performance:
              </p>
              <div className="mt-4">
                <CodeBlock code={systemMetricsCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Alerts
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Create and manage metric-based alerts:
              </p>
              <div className="mt-4">
                <CodeBlock code={alertsCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Metric Types
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    System Metrics
                  </h3>
                  <dl className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <div>
                      <dt className="font-medium">Resource Usage:</dt>
                      <dd>CPU, memory, disk, and network utilization</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Service Health:</dt>
                      <dd>Uptime, response times, error rates</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Performance:</dt>
                      <dd>Throughput, latency, queue lengths</dd>
                    </div>
                  </dl>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Custom Metrics
                  </h3>
                  <dl className="mt-4 space-y-2 text-sm text-gray-600 dark:text-gray-400">
                    <div>
                      <dt className="font-medium">Business Metrics:</dt>
                      <dd>Transaction values, user activity, custom events</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Application Metrics:</dt>
                      <dd>Cache hits, queue sizes, processing times</dd>
                    </div>
                    <div>
                      <dt className="font-medium">Blockchain Metrics:</dt>
                      <dd>Gas usage, contract calls, event counts</dd>
                    </div>
                  </dl>
                </div>
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Features
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Data Collection
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    High-performance metric collection with automatic aggregation 
                    and efficient storage. Support for various data types and 
                    custom tagging.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Aggregation
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Flexible metric aggregation with support for various time windows 
                    and statistical functions. Custom grouping and filtering options.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Visualization
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Built-in support for metric visualization through the dashboard. 
                    Export capabilities for external analysis and reporting.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Alerting
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Sophisticated alerting system with support for multiple notification 
                    channels and complex alert conditions.
                  </p>
                </div>
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Best Practices
              </h2>
              <div className="mt-6 space-y-6">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Metric Design
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Use clear naming conventions for metrics. Choose appropriate data types 
                    and aggregations. Consider cardinality when designing tags.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Alert Configuration
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Set appropriate thresholds and windows for alerts. Use multiple conditions 
                    for complex scenarios. Configure proper notification channels.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Performance
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Batch metric submissions when possible. Use appropriate sampling rates. 
                    Consider storage and query performance implications.
                  </p>
                </div>
              </div>
            </section>
          </div>
        </div>
      </div>
    </div>
  );
} 