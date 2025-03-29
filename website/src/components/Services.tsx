'use client';

// @ts-ignore
import * as React from 'react';
import Link from 'next/link';
import { 
  ChartBarIcon, 
  ClockIcon, 
  CodeBracketIcon, 
  CubeTransparentIcon, 
  KeyIcon, 
  ShieldCheckIcon
} from '@heroicons/react/24/outline';

const services = [
  {
    title: 'Price Feed Service',
    description: 'Get real-time price data from multiple sources, aggregated and delivered on-chain with high reliability.',
    link: '/services/price-feeds',
    icon: ChartBarIcon,
    color: 'bg-blue-500',
    stats: [
      { name: 'Data Sources', value: '10+' },
      { name: 'Update Frequency', value: '30s' },
      { name: 'Asset Pairs', value: '100+' },
    ],
  },
  {
    title: 'Gas Bank',
    description: 'Automated GAS distribution for your smart contracts, ensuring they always have the fuel they need to run.',
    link: '/services/gas-bank',
    icon: CubeTransparentIcon,
    color: 'bg-green-500',
    stats: [
      { name: 'Efficiency', value: '99.8%' },
      { name: 'Min Threshold', value: '0.1 GAS' },
      { name: 'Auto Refill', value: 'Yes' },
    ],
  },
  {
    title: 'Contract Automation',
    description: 'Automate your smart contracts with reliable, secure, and customizable triggers based on time or events.',
    link: '/services/automation',
    icon: ClockIcon,
    color: 'bg-purple-500',
    stats: [
      { name: 'Success Rate', value: '99.9%' },
      { name: 'Avg Response', value: '<2s' },
      { name: 'Active Jobs', value: '1000+' },
    ],
  },
  {
    title: 'Function Management',
    description: 'Deploy and manage serverless functions with built-in security and scalability for your dApps.',
    link: '/services/functions',
    icon: CodeBracketIcon,
    color: 'bg-indigo-500',
    stats: [
      { name: 'Runtime', value: 'TEE' },
      { name: 'Languages', value: '5+' },
      { name: 'Executions', value: '1M+' },
    ],
  },
  {
    title: 'Secrets Management',
    description: 'Securely store and manage sensitive information like API keys and private credentials for your smart contracts.',
    link: '/services/secrets',
    icon: KeyIcon,
    color: 'bg-yellow-500',
    stats: [
      { name: 'Encryption', value: 'AES-256' },
      { name: 'Access Control', value: 'Yes' },
      { name: 'Audit Logs', value: 'Real-time' },
    ],
  },
  {
    title: 'Trusted Execution',
    description: 'Run your code in a secure, isolated environment with hardware-level protection against tampering.',
    link: '/services/tee',
    icon: ShieldCheckIcon,
    color: 'bg-red-500',
    stats: [
      { name: 'Security Level', value: 'EAL5+' },
      { name: 'Verification', value: 'Remote' },
      { name: 'Compliance', value: 'GDPR' },
    ],
  },
];

export function Services() {
  return (
    <div className="bg-white py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl lg:max-w-none">
          <div className="text-center">
            <h2 className="text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl">
              Enterprise-Grade Blockchain Services
            </h2>
            <p className="mt-4 text-lg leading-8 text-gray-600">
              Comprehensive infrastructure services powering the most innovative projects on Neo N3
            </p>
          </div>
          <div className="mt-16 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {services.map((service, index) => (
              <div 
                key={service.title} 
                className="flex flex-col overflow-hidden rounded-lg shadow-lg transition-all hover:shadow-xl hover:-translate-y-1 duration-300"
              >
                <div className={`${service.color} p-6`}>
                  <service.icon className="h-8 w-8 text-white" />
                </div>
                <div className="flex flex-1 flex-col justify-between bg-white p-6">
                  <div className="flex-1">
                    <h3 className="text-xl font-semibold text-gray-900">{service.title}</h3>
                    <p className="mt-3 text-base text-gray-500">{service.description}</p>
                  </div>
                  <div className="mt-6">
                    <div className="grid grid-cols-3 gap-4 mb-4">
                      {service.stats.map((stat) => (
                        <div key={stat.name} className="mx-auto text-center">
                          <div className="text-lg font-semibold text-indigo-600">
                            {stat.value}
                          </div>
                          <div className="mt-1 text-xs text-gray-600">{stat.name}</div>
                        </div>
                      ))}
                    </div>
                    <Link
                      href={service.link}
                      className="text-sm font-semibold leading-6 text-indigo-600 hover:text-indigo-500 inline-flex items-center"
                    >
                      Learn more <span className="ml-1 text-lg" aria-hidden="true">→</span>
                    </Link>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}