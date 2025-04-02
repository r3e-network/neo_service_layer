'use client';

// @ts-ignore
import * as React from 'react';
import Link from 'next/link';
import { motion } from 'framer-motion';

// --- Simplified Logo-based SVG Figures (v2) ---

const LogoGradient = () => (
  <linearGradient id="logoGradSimplified" x1="0%" y1="0%" x2="100%" y2="100%">
    <stop offset="0%" stopColor="#60A5FA" /> {/* blue-400 */}
    <stop offset="100%" stopColor="#34D399" /> {/* green-400 */}
  </linearGradient>
);

// Price Feed: Simple line graph
const LogoStylePriceFeedFigure = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs><LogoGradient /></defs>
    <path d="M4 18 L8 10 L12 14 L16 6 L20 12" stroke="url(#logoGradSimplified)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
  </svg>
);

// Gas Bank: Simple container/coin shape
const LogoStyleGasBankFigure = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs><LogoGradient /></defs>
    <rect x="4" y="4" width="16" height="16" rx="3" stroke="url(#logoGradSimplified)" strokeWidth="1.5"/>
    <circle cx="12" cy="12" r="4" fill="url(#logoGradSimplified)" opacity="0.5"/>
  </svg>
);

// Automation: Simple clock hands
const LogoStyleAutomationFigure = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs><LogoGradient /></defs>
    <circle cx="12" cy="12" r="9" stroke="url(#logoGradSimplified)" strokeWidth="1.5" opacity="0.5"/>
    <path d="M12 7 V12 L 16 14" stroke="url(#logoGradSimplified)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
  </svg>
);

// Secrets: Simple lock icon
const LogoStyleSecretsFigure = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs><LogoGradient /></defs>
    <rect x="5" y="11" width="14" height="10" rx="2" stroke="url(#logoGradSimplified)" strokeWidth="1.5"/>
    <path d="M8 11 V7 C8 4.79 9.79 3 12 3 C14.21 3 16 4.79 16 7 V11" stroke="url(#logoGradSimplified)" strokeWidth="1.5" strokeLinecap="round"/>
    <circle cx="12" cy="16" r="1.5" fill="url(#logoGradSimplified)" opacity="0.7"/>
  </svg>
);

// Functions: Simple code brackets
const LogoStyleFunctionFigure = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs><LogoGradient /></defs>
    <path d="M9 4 L4 9 L9 14" stroke="url(#logoGradSimplified)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
    <path d="M15 4 L20 9 L15 14" stroke="url(#logoGradSimplified)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
    <line x1="12" y1="5" x2="12" y2="19" stroke="url(#logoGradSimplified)" strokeWidth="1.5" strokeDasharray="2 2" opacity="0.5"/>
  </svg>
);

// TEE: Simple shield icon
const LogoStyleTEEFigure = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <defs><LogoGradient /></defs>
    <path d="M12 3 L4 6 V12 C4 16.418 7.582 21 12 21 C16.418 21 20 16.418 20 12 V6 L12 3 Z" 
          stroke="url(#logoGradSimplified)" strokeWidth="1.5"/>
    <path d="M10 13 L12 15 L14 11" stroke="url(#logoGradSimplified)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" opacity="0.8"/>
  </svg>
);

// features array remains the same, referencing the LogoStyle...Figure components
const features = [
  { 
    name: 'Price Feeds',
    description: 'Reliable, aggregated price data delivered securely on-chain.',
    icon: LogoStylePriceFeedFigure, 
    link: '/services/price-feed',
  },
  { 
    name: 'Gas Bank',
    description: 'Automated GAS management ensures your contracts never run dry.',
    icon: LogoStyleGasBankFigure, 
    link: '/services/gas-bank',
  },
  { 
    name: 'Contract Automation',
    description: 'Execute smart contracts based on time or custom events reliably.',
    icon: LogoStyleAutomationFigure, 
    link: '/services/automation',
  },
  { 
    name: 'Secrets Management',
    description: 'Securely manage API keys and credentials for your dApps.',
    icon: LogoStyleSecretsFigure, 
    link: '/services/secrets',
  },
  { 
    name: 'Function Management',
    description: 'Deploy and manage serverless functions within secure TEEs.',
    icon: LogoStyleFunctionFigure, 
    link: '/services/functions',
  },
  { 
    name: 'Trusted Execution',
    description: 'Hardware-level security for computations via TEE technology.',
    icon: LogoStyleTEEFigure, 
    link: '/services/tee',
  },
];

export function Features() {
  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }} 
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true, amount: 0.1 }}
      transition={{ duration: 0.6, ease: "easeOut" }}
      className="relative isolate overflow-hidden bg-white py-24 sm:py-32"
    >
      <div className="relative mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <h2 className="text-base font-semibold leading-7 text-blue-600">
            Core Services
          </h2>
          <p className="mt-2 text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl">
            Infrastructure for Verifiable Computation
          </p>
          <p className="mt-6 text-lg leading-8 text-gray-600">
             Leverage TEEs and blockchain for building robust, decentralized applications with verifiable trust and security guarantees.
          </p>
        </div>
        <motion.dl 
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true, amount: 0.1 }}
          transition={{ staggerChildren: 0.1 }}
          className="mx-auto mt-16 grid max-w-2xl grid-cols-1 gap-8 sm:mt-20 lg:mt-24 lg:max-w-none lg:grid-cols-3"
        >
          {features.map((feature) => (
            <motion.div
              key={feature.name}
              variants={{
                hidden: { opacity: 0, y: 20 },
                visible: { opacity: 1, y: 0 }
              }}
              transition={{ duration: 0.5 }}
              className={`relative flex flex-col rounded-xl p-6 bg-white border border-gray-200 transition-all duration-300 hover:shadow-lg hover:border-gray-300 hover:-translate-y-1`}
            >
              <dt className="flex items-center gap-x-3 text-base font-semibold leading-7 text-gray-900">
                 {/* Icon container (no background needed) */}
                <div className="flex h-10 w-10 flex-none items-center justify-center rounded-lg">
                  <feature.icon /> 
                </div>
                {feature.name}
              </dt>
              <dd className="mt-4 flex flex-auto flex-col text-sm leading-6 text-gray-600">
                <p className="flex-auto">{feature.description}</p>
                <p className="mt-4">
                  <Link 
                    href={feature.link} 
                    className="inline-flex items-center font-medium text-blue-600 hover:text-blue-500 transition-colors"
                  >
                    Learn more 
                    <svg className="ml-1 h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clipRule="evenodd" />
                    </svg>
                  </Link>
                </p>
              </dd>
            </motion.div>
          ))}
        </motion.dl>
      </div>
    </motion.div>
  );
}