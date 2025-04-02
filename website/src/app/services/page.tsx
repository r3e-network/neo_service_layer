// @ts-ignore
import * as React from 'react';
import Link from 'next/link';
import {
  BoltIcon,
  KeyIcon,
  CurrencyDollarIcon,
  CogIcon,
  ChartBarIcon,
  ShieldCheckIcon,
  CodeBracketIcon,
  CommandLineIcon,
} from '@heroicons/react/24/outline';

const services = [
  {
    name: 'Price Feed',
    description: 'Real-time price data from multiple sources with sophisticated filtering and validation.',
    icon: CurrencyDollarIcon,
    href: '/services/price-feed',
    features: [
      'Multi-source aggregation',
      'Kalman filtering',
      'Outlier detection',
      'Historical accuracy tracking',
    ],
  },
  {
    name: 'Gas Bank',
    description: 'Efficient gas management system for automated contract operations.',
    icon: ChartBarIcon,
    href: '/services/gas-bank',
    features: [
      'Automated refills',
      'Usage tracking',
      'Cost optimization',
      'Multi-wallet support',
    ],
  },
  {
    name: 'Contract Automation',
    description: 'Automated smart contract execution with comprehensive monitoring.',
    icon: CogIcon,
    href: '/services/automation',
    features: [
      'Scheduled execution',
      'Event-based triggers',
      'Error handling',
      'Performance monitoring',
    ],
  },
  {
    name: 'Secrets Management',
    description: 'Secure storage and management of sensitive data and credentials.',
    icon: KeyIcon,
    href: '/services/secrets',
    features: [
      'TEE protection',
      'Access control',
      'Audit logging',
      'Key rotation',
    ],
  },
  {
    name: 'Functions',
    description: 'Serverless functions that run in a secure trusted execution environment.',
    icon: CodeBracketIcon,
    href: '/services/functions',
    features: [
      'Secure runtime',
      'Custom logic',
      'Event triggers',
      'Scalable execution',
    ],
  },
  {
    name: 'Trigger Service',
    description: 'Event monitoring and automated response system.',
    icon: BoltIcon,
    href: '/services/triggers',
    features: [
      'Custom conditions',
      'Chain monitoring',
      'Webhook support',
      'Action automation',
    ],
  },
  {
    name: 'Metrics',
    description: 'Comprehensive system monitoring and performance tracking.',
    icon: ChartBarIcon,
    href: '/services/metrics',
    features: [
      'Real-time monitoring',
      'Custom metrics',
      'Alert system',
      'Performance analysis',
    ],
  },
  {
    name: 'CLI Tools',
    description: 'Command-line tools for interacting with the service layer.',
    icon: CommandLineIcon,
    href: '/services/cli',
    features: [
      'Service management',
      'Deployment tools',
      'Monitoring utilities',
      'Configuration tools',
    ],
  },
];

export default function ServicesPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Our Services
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Comprehensive blockchain infrastructure services powered by secure trusted execution environments.
          </p>
        </div>
        <div className="mx-auto mt-16 grid max-w-2xl grid-cols-1 gap-6 sm:mt-20 lg:mx-0 lg:max-w-none lg:grid-cols-3 lg:gap-8">
          {services.map((service) => (
            <Link 
              href={service.href} 
              key={service.name}
              className="flex flex-col rounded-3xl bg-white dark:bg-gray-800 shadow-sm ring-1 ring-gray-200 dark:ring-gray-700 hover:ring-blue-500 dark:hover:ring-blue-400 transition-all duration-200"
            >
              <div className="p-8">
                <service.icon
                  className="h-10 w-10 text-blue-600 dark:text-blue-400"
                  aria-hidden="true"
                />
                <h3 className="mt-6 text-2xl font-semibold leading-7 tracking-tight text-gray-900 dark:text-white">
                  {service.name}
                </h3>
                <p className="mt-2 text-base leading-7 text-gray-600 dark:text-gray-400">
                  {service.description}
                </p>
              </div>
              <div className="mt-auto border-t border-gray-200 dark:border-gray-700 p-8">
                <ul role="list" className="mt-2 space-y-2">
                  {service.features.map((feature) => (
                    <li key={feature} className="flex gap-x-3">
                      <ShieldCheckIcon
                        className="h-6 w-5 flex-none text-blue-600 dark:text-blue-400"
                        aria-hidden="true"
                      />
                      <span className="text-sm leading-6 text-gray-600 dark:text-gray-400">
                        {feature}
                      </span>
                    </li>
                  ))}
                </ul>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
} 