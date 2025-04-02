// @ts-ignore
import * as React from 'react';
import { ChartBarIcon, BellAlertIcon, ClockIcon, RocketLaunchIcon } from '@heroicons/react/24/outline';

export default function MetricsPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <div className="inline-flex items-center justify-center p-3 rounded-full bg-sky-100 dark:bg-sky-900 mb-6">
            <ChartBarIcon className="h-8 w-8 text-sky-600 dark:text-sky-400" />
          </div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Metrics Service
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Comprehensive system monitoring and performance tracking.
          </p>
        </div>
        
        <div className="mt-16 flow-root">
          <div className="mt-10 space-y-8 border-t border-gray-200 dark:border-gray-700 pt-10 sm:mt-16 sm:pt-16">
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Key Features</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <ChartBarIcon className="h-6 w-6 text-sky-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Real-time Monitoring</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Track your system's performance with real-time dashboards that update continuously.
                    Get immediate visibility into your application's health and behavior.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <RocketLaunchIcon className="h-6 w-6 text-sky-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Custom Metrics</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Define and track metrics that matter to your business, from transaction throughput 
                    to user engagement patterns or custom business KPIs.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <BellAlertIcon className="h-6 w-6 text-sky-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Alert System</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Configure intelligent alerts based on thresholds, anomaly detection, or complex 
                    conditions. Receive notifications via email, SMS, or your preferred collaboration tools.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <ClockIcon className="h-6 w-6 text-sky-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Performance Analysis</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Analyze trends over time with historical data retention and powerful visualization tools.
                    Identify bottlenecks and opportunities for optimization.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Available Metrics</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">System Metrics</h3>
                  <ul className="list-disc list-inside space-y-2 text-gray-600 dark:text-gray-400">
                    <li>CPU and memory usage</li>
                    <li>Network throughput and latency</li>
                    <li>Disk I/O and storage utilization</li>
                    <li>Error rates and exceptions</li>
                    <li>Request/response times</li>
                  </ul>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Blockchain Metrics</h3>
                  <ul className="list-disc list-inside space-y-2 text-gray-600 dark:text-gray-400">
                    <li>Transaction throughput and confirmation times</li>
                    <li>Gas usage and optimization</li>
                    <li>Contract invocation statistics</li>
                    <li>Event emission frequency</li>
                    <li>Node connectivity and health</li>
                  </ul>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Service Metrics</h3>
                  <ul className="list-disc list-inside space-y-2 text-gray-600 dark:text-gray-400">
                    <li>Function execution performance</li>
                    <li>Price feed accuracy and latency</li>
                    <li>Gas bank utilization and refill metrics</li>
                    <li>Trigger activation statistics</li>
                    <li>API usage and response times</li>
                  </ul>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Business Metrics</h3>
                  <ul className="list-disc list-inside space-y-2 text-gray-600 dark:text-gray-400">
                    <li>User activity and engagement</li>
                    <li>Resource consumption by project</li>
                    <li>API usage by endpoint</li>
                    <li>Custom business KPIs</li>
                    <li>Cost and usage forecasting</li>
                  </ul>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Integration</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                <p className="text-gray-600 dark:text-gray-400 mb-4">
                  Accessing and analyzing metrics in your application:
                </p>
                <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
                  <pre className="text-sm overflow-x-auto">
                    <code className="language-javascript">
{`// Query metrics data for analysis
const metrics = await neo.services.metrics.query({
  metrics: ["function.execution.time", "function.execution.count"],
  filters: {
    functionId: "my-function-123",
    status: "success"
  },
  timeRange: {
    start: "2023-04-01T00:00:00Z",
    end: "2023-04-07T23:59:59Z"
  },
  aggregation: "daily"
});

console.log(metrics);
// [
//   {
//     date: "2023-04-01",
//     function.execution.time: { avg: 245, min: 120, max: 560, p95: 320 },
//     function.execution.count: 143
//   },
//   ...
// ]`}
                    </code>
                  </pre>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
} 