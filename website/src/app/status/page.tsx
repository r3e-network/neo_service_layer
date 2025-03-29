import React from 'react';
import { StatusGrid } from '../../components/StatusGrid';
import { MetricsDisplay } from '../../components/MetricsDisplay';
import { IncidentHistory } from '../../components/IncidentHistory';

export const metadata = {
  title: 'System Status - Neo Service Layer',
  description: 'Real-time status and health monitoring for Neo Service Layer components',
};

interface Service {
  name: string;
  status: string;
  metrics: {
    uptime: string;
    [key: string]: string;
  };
}

interface Incident {
  id: number;
  date: string;
  title: string;
  description: string;
  status: string;
  duration: string;
}

const services: Service[] = [
  {
    name: 'Price Feed Service',
    status: 'operational',
    metrics: {
      uptime: '99.99%',
      latency: '45ms',
      accuracy: '99.95%',
    },
  },
  {
    name: 'Gas Bank Service',
    status: 'operational',
    metrics: {
      uptime: '99.98%',
      transactions: '1.2M/day',
      efficiency: '98.5%',
    },
  },
  {
    name: 'Contract Automation',
    status: 'operational',
    metrics: {
      uptime: '99.95%',
      executions: '500K/day',
      success_rate: '99.9%',
    },
  },
  {
    name: 'Secrets Management',
    status: 'operational',
    metrics: {
      uptime: '100%',
      requests: '2M/day',
      latency: '35ms',
    },
  },
  {
    name: 'Functions Service',
    status: 'operational',
    metrics: {
      uptime: '99.97%',
      executions: '1M/day',
      avg_duration: '120ms',
    },
  },
  {
    name: 'Trigger Service',
    status: 'operational',
    metrics: {
      uptime: '99.99%',
      events: '800K/day',
      latency: '50ms',
    },
  },
  {
    name: 'Metrics Service',
    status: 'operational',
    metrics: {
      uptime: '100%',
      data_points: '10M/day',
      retention: '99.99%',
    },
  },
  {
    name: 'API Gateway',
    status: 'operational',
    metrics: {
      uptime: '99.99%',
      requests: '5M/day',
      latency: '65ms',
    },
  },
];

const incidents: Incident[] = [
  {
    id: 1,
    date: '2024-03-26',
    title: 'Price Feed Service Latency',
    description: 'Increased latency in price feed updates due to upstream provider issues. Resolved through fallback mechanisms.',
    status: 'resolved',
    duration: '15 minutes',
  },
  {
    id: 2,
    date: '2024-03-25',
    title: 'API Rate Limiting',
    description: 'Temporary increase in API response times due to unexpected traffic spike. Resolved by scaling infrastructure.',
    status: 'resolved',
    duration: '10 minutes',
  },
  {
    id: 3,
    date: '2024-03-24',
    title: 'Contract Automation Delay',
    description: 'Brief delay in automated contract executions due to network congestion. Resolved through gas price adjustment.',
    status: 'resolved',
    duration: '8 minutes',
  },
];

export default function StatusPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            System Status
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Real-time monitoring and status information for all Neo Service Layer components.
          </p>
        </div>

        <div className="mt-16">
          <StatusGrid services={services} />
        </div>

        <div className="mt-16">
          <h2 className="text-2xl font-bold tracking-tight text-gray-900 dark:text-white">
            System Metrics
          </h2>
          <div className="mt-6">
            <MetricsDisplay services={services} />
          </div>
        </div>

        <div className="mt-16">
          <h2 className="text-2xl font-bold tracking-tight text-gray-900 dark:text-white">
            Incident History
          </h2>
          <div className="mt-6">
            <IncidentHistory incidents={incidents} />
          </div>
        </div>
      </div>
    </div>
  );
}