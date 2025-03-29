import React from 'react';
import Link from 'next/link';
import {
  BookOpenIcon,
  CodeBracketIcon,
  CommandLineIcon,
  CubeIcon,
  KeyIcon,
  RocketLaunchIcon,
} from '@heroicons/react/24/outline';

export const metadata = {
  title: 'Documentation - Neo Service Layer',
  description: 'Comprehensive documentation for Neo Service Layer services and APIs',
};

const sections = [
  {
    title: 'Getting Started',
    description: 'Quick start guide and basic concepts',
    icon: RocketLaunchIcon,
    links: [
      { title: 'Introduction', href: '/docs/getting-started/introduction' },
      { title: 'Installation', href: '/docs/getting-started/installation' },
      { title: 'Basic Concepts', href: '/docs/getting-started/concepts' },
      { title: 'Authentication', href: '/docs/getting-started/authentication' },
    ],
  },
  {
    title: 'Core Services',
    description: 'Detailed documentation for each service',
    icon: CubeIcon,
    links: [
      { title: 'Price Feed', href: '/docs/services/price-feed' },
      { title: 'Gas Bank', href: '/docs/services/gas-bank' },
      { title: 'Contract Automation', href: '/docs/services/automation' },
      { title: 'Functions', href: '/docs/services/functions' },
    ],
  },
  {
    title: 'Security',
    description: 'Security features and best practices',
    icon: KeyIcon,
    links: [
      { title: 'TEE Overview', href: '/docs/security/tee' },
      { title: 'Secrets Management', href: '/docs/security/secrets' },
      { title: 'Access Control', href: '/docs/security/access-control' },
      { title: 'Best Practices', href: '/docs/security/best-practices' },
    ],
  },
  {
    title: 'API Reference',
    description: 'Complete API documentation',
    icon: CodeBracketIcon,
    links: [
      { title: 'REST API', href: '/docs/api/rest' },
      { title: 'WebSocket API', href: '/docs/api/websocket' },
      { title: 'Smart Contracts', href: '/docs/api/contracts' },
      { title: 'SDKs', href: '/docs/api/sdks' },
    ],
  },
  {
    title: 'CLI Tools',
    description: 'Command-line interface documentation',
    icon: CommandLineIcon,
    links: [
      { title: 'Installation', href: '/docs/cli/installation' },
      { title: 'Commands', href: '/docs/cli/commands' },
      { title: 'Configuration', href: '/docs/cli/configuration' },
      { title: 'Plugins', href: '/docs/cli/plugins' },
    ],
  },
  {
    title: 'Guides',
    description: 'Tutorials and how-to guides',
    icon: BookOpenIcon,
    links: [
      { title: 'Quick Start', href: '/docs/guides/quick-start' },
      { title: 'Integration Guide', href: '/docs/guides/integration' },
      { title: 'Monitoring', href: '/docs/guides/monitoring' },
      { title: 'Troubleshooting', href: '/docs/guides/troubleshooting' },
    ],
  },
];

export default function DocsPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Documentation
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Everything you need to build with Neo Service Layer. From getting started guides to detailed API references.
          </p>
        </div>

        <div className="mx-auto mt-16 grid max-w-2xl auto-rows-fr grid-cols-1 gap-8 sm:mt-20 lg:mx-0 lg:max-w-none lg:grid-cols-3">
          {sections.map((section, index) => (
            <div
              key={section.title}
              className="relative isolate flex flex-col justify-between rounded-2xl bg-white dark:bg-gray-800 px-8 pb-8 pt-10 shadow-sm ring-1 ring-gray-200 dark:ring-gray-700 hover:ring-blue-500 dark:hover:ring-blue-400 transition-all duration-200"
            >
              <div>
                <div className="flex items-center gap-x-4">
                  <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-blue-600/10 dark:bg-blue-400/10">
                    <section.icon className="h-6 w-6 text-blue-600 dark:text-blue-400" aria-hidden="true" />
                  </span>
                  <h3 className="text-lg font-semibold leading-8 tracking-tight text-gray-900 dark:text-white">
                    {section.title}
                  </h3>
                </div>
                <p className="mt-4 text-base leading-7 text-gray-600 dark:text-gray-400">
                  {section.description}
                </p>
                <ul role="list" className="mt-8 space-y-3">
                  {section.links.map((link) => (
                    <li key={link.title}>
                      <Link
                        href={link.href}
                        className="text-sm leading-6 text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400"
                      >
                        {link.title}
                      </Link>
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}