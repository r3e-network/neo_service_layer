'use client';

// @ts-ignore
import * as React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';

const navigation = {
  'Getting Started': [
    { name: 'Introduction', href: '/docs' },
    { name: 'Quick Start', href: '/docs/getting-started/quick-start' },
    { name: 'Architecture', href: '/docs/getting-started/architecture' },
    { name: 'Security Model', href: '/docs/getting-started/security' },
  ],
  'Core Services': [
    { name: 'Price Feeds', href: '/docs/services/price-feeds' },
    { name: 'Contract Automation', href: '/docs/services/automation' },
    { name: 'Gas Bank', href: '/docs/services/gas-bank' },
    { name: 'Functions', href: '/docs/services/functions' },
    { name: 'Secrets Management', href: '/docs/services/secrets' },
    { name: 'API Service', href: '/docs/services/api' },
  ],
  'Guides': [
    { name: 'Authentication', href: '/docs/guides/authentication' },
    { name: 'Contract Integration', href: '/docs/guides/contract-integration' },
    { name: 'Function Development', href: '/docs/guides/function-development' },
    { name: 'Monitoring & Metrics', href: '/docs/guides/monitoring' },
  ],
  'API Reference': [
    { name: 'REST API', href: '/docs/api/rest' },
    { name: 'SDK Reference', href: '/docs/api/sdk' },
    { name: 'Smart Contracts', href: '/docs/api/contracts' },
  ],
};

export default function DocsLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const [mobileMenuOpen, setMobileMenuOpen] = React.useState(false);

  return (
    <div className="relative flex min-h-screen">
      {/* Sidebar */}
      <aside className="fixed inset-y-0 left-0 hidden w-64 overflow-y-auto border-r border-gray-200 bg-white px-4 py-6 sm:px-6 lg:px-8 xl:block">
        <nav className="space-y-8">
          {Object.entries(navigation).map(([category, items]) => (
            <div key={category}>
              <h3 className="font-semibold text-gray-900">{category}</h3>
              <ul className="mt-2 space-y-2">
                {items.map((item) => (
                  <li key={item.name}>
                    <Link
                      href={item.href}
                      className={`block rounded-md px-3 py-2 text-sm ${
                        pathname === item.href
                          ? 'bg-gray-50 text-blue-600'
                          : 'text-gray-700 hover:bg-gray-50'
                      }`}
                    >
                      {item.name}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </nav>
      </aside>

      {/* Mobile menu */}
      <div className="fixed inset-0 z-40 xl:hidden" role="dialog" aria-modal="true">
        <div
          className={`fixed inset-0 bg-black bg-opacity-25 ${
            mobileMenuOpen ? 'block' : 'hidden'
          }`}
          aria-hidden="true"
        />

        <div
          className={`fixed inset-0 z-40 flex transform transition-transform duration-300 ease-in-out ${
            mobileMenuOpen ? 'translate-x-0' : '-translate-x-full'
          }`}
        >
          <div className="relative flex w-full max-w-xs flex-1 flex-col bg-white pb-4 pt-5">
            <div className="absolute right-0 top-0 -mr-12 pt-2">
              <button
                type="button"
                className="ml-1 flex h-10 w-10 items-center justify-center rounded-full focus:outline-none focus:ring-2 focus:ring-inset focus:ring-white"
                onClick={() => setMobileMenuOpen(false)}
              >
                <span className="sr-only">Close sidebar</span>
                <svg
                  className="h-6 w-6 text-white"
                  fill="none"
                  viewBox="0 0 24 24"
                  strokeWidth="1.5"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M6 18L18 6M6 6l12 12"
                  />
                </svg>
              </button>
            </div>

            <div className="flex-1 px-4 sm:px-6">
              <nav className="space-y-8">
                {Object.entries(navigation).map(([category, items]) => (
                  <div key={category}>
                    <h3 className="font-semibold text-gray-900">{category}</h3>
                    <ul className="mt-2 space-y-2">
                      {items.map((item) => (
                        <li key={item.name}>
                          <Link
                            href={item.href}
                            className={`block rounded-md px-3 py-2 text-sm ${
                              pathname === item.href
                                ? 'bg-gray-50 text-blue-600'
                                : 'text-gray-700 hover:bg-gray-50'
                            }`}
                            onClick={() => setMobileMenuOpen(false)}
                          >
                            {item.name}
                          </Link>
                        </li>
                      ))}
                    </ul>
                  </div>
                ))}
              </nav>
            </div>
          </div>
        </div>
      </div>

      {/* Main content */}
      <main className="flex-1 xl:pl-64">
        <div className="px-4 py-10 sm:px-6 lg:px-8 xl:px-12">
          <button
            type="button"
            className="mb-4 inline-flex items-center xl:hidden"
            onClick={() => setMobileMenuOpen(true)}
          >
            <span className="sr-only">Open sidebar</span>
            <svg
              className="h-6 w-6"
              fill="none"
              viewBox="0 0 24 24"
              strokeWidth="1.5"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5"
              />
            </svg>
          </button>
          {children}
        </div>
      </main>
    </div>
  );
}