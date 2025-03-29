'use client';

// @ts-ignore
import * as React from 'react';

// Custom SVG Icons
const PriceFeedIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" className="text-green-500">
    <path d="M3 12L7 8M7 8L11 12M7 8V16" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
    <path d="M21 12L17 16M17 16L13 12M17 16V8" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
    <path d="M12 3V21" stroke="currentColor" strokeWidth="2" strokeDasharray="2 2"/>
  </svg>
);

const GasBankIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" className="text-blue-500">
    <path d="M4 8H20" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <rect x="4" y="4" width="16" height="16" rx="2" stroke="currentColor" strokeWidth="2"/>
    <path d="M8 12H16" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <path d="M8 16H12" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
  </svg>
);

const ContractAutomationIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" className="text-purple-500">
    <path d="M12 6L12 18" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <path d="M6 12L18 12" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <circle cx="12" cy="12" r="9" stroke="currentColor" strokeWidth="2"/>
    <path d="M15 9L9 15" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
  </svg>
);

const SecretsIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" className="text-yellow-500">
    <rect x="4" y="6" width="16" height="12" rx="2" stroke="currentColor" strokeWidth="2"/>
    <circle cx="12" cy="12" r="3" stroke="currentColor" strokeWidth="2"/>
    <path d="M12 8V16" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <path d="M8 12H16" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
  </svg>
);

const FunctionIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" className="text-indigo-500">
    <path d="M8 6H16" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <path d="M8 12H16" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <path d="M8 18H16" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <path d="M4 6H4.01" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <path d="M4 12H4.01" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <path d="M4 18H4.01" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <path d="M20 6H20.01" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <path d="M20 12H20.01" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
    <path d="M20 18H20.01" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
  </svg>
);

const TEEIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" className="text-red-500">
    <path d="M12 3L20 7.5V16.5L12 21L4 16.5V7.5L12 3Z" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
    <path d="M12 12L12 21" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
    <path d="M12 12L20 7.5" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
    <path d="M12 12L4 7.5" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
    <circle cx="12" cy="12" r="2" stroke="currentColor" strokeWidth="2"/>
  </svg>
);

const features = [
  {
    name: 'Price Feeds',
    description: 'Real-time price data from multiple sources, aggregated and delivered on-chain with high reliability and accuracy.',
    icon: PriceFeedIcon,
    bgColor: 'bg-green-50',
    borderColor: 'border-green-200',
    link: '/services/price-feeds',
  },
  {
    name: 'Gas Bank',
    description: 'Efficient gas management system for seamless contract operations. Never worry about your contracts running out of fuel.',
    icon: GasBankIcon,
    bgColor: 'bg-blue-50',
    borderColor: 'border-blue-200',
    link: '/services/gas-bank',
  },
  {
    name: 'Contract Automation',
    description: 'Automated contract execution based on predefined triggers and conditions. Schedule and automate your blockchain operations.',
    icon: ContractAutomationIcon,
    bgColor: 'bg-purple-50',
    borderColor: 'border-purple-200',
    link: '/services/automation',
  },
  {
    name: 'Secrets Management',
    description: 'Secure storage and access control for sensitive data and API keys. Keep your credentials safe while using them in your dApps.',
    icon: SecretsIcon,
    bgColor: 'bg-yellow-50',
    borderColor: 'border-yellow-200',
    link: '/services/secrets',
  },
  {
    name: 'Function Management',
    description: 'Create, update, and manage serverless functions with ease. Deploy your code to our secure infrastructure with just a few clicks.',
    icon: FunctionIcon,
    bgColor: 'bg-indigo-50',
    borderColor: 'border-indigo-200',
    link: '/services/functions',
  },
  {
    name: 'Trusted Execution',
    description: 'Run your code in a secure, isolated environment with hardware-level protection against tampering and unauthorized access.',
    icon: TEEIcon,
    bgColor: 'bg-red-50',
    borderColor: 'border-red-200',
    link: '/services/tee',
  },
];

export function Features() {
  return (
    <div className="relative overflow-hidden bg-white py-24 sm:py-32">
      {/* Background decoration */}
      <div className="absolute top-0 left-0 w-full h-full overflow-hidden pointer-events-none">
        <div className="absolute -top-24 -right-24 w-96 h-96 rounded-full bg-green-100 opacity-20 blur-3xl"></div>
        <div className="absolute top-1/2 -left-24 w-80 h-80 rounded-full bg-blue-100 opacity-20 blur-3xl"></div>
        <div className="absolute -bottom-24 right-1/4 w-64 h-64 rounded-full bg-purple-100 opacity-20 blur-3xl"></div>
      </div>
      
      <div className="relative mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <h2 className="text-base font-semibold leading-7 text-green-600">
            Comprehensive Services
          </h2>
          <p className="mt-2 text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl">
            Everything you need to build on Neo N3
          </p>
          <p className="mt-6 text-lg leading-8 text-gray-600">
            Our enterprise-grade service layer provides all the essential tools and services you need to build
            powerful decentralized applications on the Neo N3 blockchain.
          </p>
        </div>
        <div className="mx-auto mt-16 max-w-2xl sm:mt-20 lg:mt-24 lg:max-w-none">
          <dl className="grid max-w-xl grid-cols-1 gap-x-8 gap-y-16 lg:max-w-none lg:grid-cols-3">
            {features.map((feature) => (
              <div
                key={feature.name}
                className={`flex flex-col rounded-2xl p-8 shadow-sm border ${feature.borderColor} ${feature.bgColor} transition-all duration-300 hover:shadow-lg hover:-translate-y-2`}
              >
                <dt className="flex items-center gap-x-3 text-lg font-semibold leading-7 text-gray-900">
                  <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-white shadow-sm border border-gray-200">
                    <feature.icon />
                  </div>
                  {feature.name}
                </dt>
                <dd className="mt-4 flex flex-auto flex-col text-base leading-7 text-gray-600">
                  <p className="flex-auto">{feature.description}</p>
                  <p className="mt-6">
                    <a 
                      href={feature.link} 
                      className="inline-flex items-center text-sm font-semibold leading-6 text-green-600 hover:text-green-500 transition-colors"
                    >
                      Learn more 
                      <svg className="ml-1 h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                        <path fillRule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clipRule="evenodd" />
                      </svg>
                    </a>
                  </p>
                </dd>
              </div>
            ))}
          </dl>
        </div>
      </div>
    </div>
  );
}