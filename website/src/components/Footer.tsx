'use client';

// @ts-ignore
import * as React from 'react';
import Link from 'next/link';
import Image from 'next/image';

const navigation = {
  main: [
    { name: 'Documentation', href: '/docs' },
    { name: 'API Reference', href: '/docs/api' },
    { name: 'Services', href: '/services' },
    { name: 'Status', href: '/status' },
    { name: 'Playground', href: '/playground' },
  ],
  services: [
    { name: 'Price Feed', href: '/services/price-feed' },
    { name: 'Gas Bank', href: '/services/gas-bank' },
    { name: 'Contract Automation', href: '/services/automation' },
    { name: 'Secrets Management', href: '/services/secrets' },
    { name: 'Functions', href: '/services/functions' },
    { name: 'Trusted Execution', href: '/services/tee' },
  ],
  resources: [
    { name: 'Blog', href: '/blog' },
    { name: 'Tutorials', href: '/tutorials' },
    { name: 'Case Studies', href: '/case-studies' },
    { name: 'FAQ', href: '/faq' },
    { name: 'Support', href: '/support' },
  ],
  social: [
    {
      name: 'GitHub',
      href: 'https://github.com/neo-project/neo-service-layer',
      icon: (props: any) => (
        <svg fill="currentColor" viewBox="0 0 24 24" {...props}>
          <path
            fillRule="evenodd"
            d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z"
            clipRule="evenodd"
          />
        </svg>
      ),
    },
    {
      name: 'Discord',
      href: 'https://discord.gg/neo',
      icon: (props: any) => (
        <svg fill="currentColor" viewBox="0 0 24 24" {...props}>
          <path d="M20.317 4.37a19.791 19.791 0 00-4.885-1.515.074.074 0 00-.079.037c-.21.375-.444.864-.608 1.25a18.27 18.27 0 00-5.487 0 12.64 12.64 0 00-.617-1.25.077.077 0 00-.079-.037A19.736 19.736 0 003.677 4.37a.07.07 0 00-.032.027C.533 9.046-.32 13.58.099 18.057a.082.082 0 00.031.057 19.9 19.9 0 005.993 3.03.078.078 0 00.084-.028c.462-.63.874-1.295 1.226-1.994a.076.076 0 00-.041-.106 13.107 13.107 0 01-1.872-.892.077.077 0 01-.008-.128 10.2 10.2 0 00.372-.292.074.074 0 01.077-.01c3.928 1.793 8.18 1.793 12.062 0a.074.074 0 01.078.01c.12.098.246.198.373.292a.077.077 0 01-.006.127 12.299 12.299 0 01-1.873.892.077.077 0 00-.041.107c.36.698.772 1.362 1.225 1.993a.076.076 0 00.084.028 19.839 19.839 0 006.002-3.03.077.077 0 00.032-.054c.5-5.177-.838-9.674-3.549-13.66a.061.061 0 00-.031-.03zM8.02 15.33c-1.183 0-2.157-1.085-2.157-2.419 0-1.333.956-2.419 2.157-2.419 1.21 0 2.176 1.096 2.157 2.42 0 1.333-.956 2.418-2.157 2.418zm7.975 0c-1.183 0-2.157-1.085-2.157-2.419 0-1.333.955-2.419 2.157-2.419 1.21 0 2.176 1.096 2.157 2.42 0 1.333-.946 2.418-2.157 2.418z" />
        </svg>
      ),
    },
    {
      name: 'Twitter',
      href: 'https://twitter.com/neo_blockchain',
      icon: (props: any) => (
        <svg fill="currentColor" viewBox="0 0 24 24" {...props}>
          <path d="M8.29 20.251c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0022 5.92a8.19 8.19 0 01-2.357.646 4.118 4.118 0 001.804-2.27 8.224 8.224 0 01-2.605.996 4.107 4.107 0 00-6.993 3.743 11.65 11.65 0 01-8.457-4.287 4.106 4.106 0 001.27 5.477A4.072 4.072 0 012.8 9.713v.052a4.105 4.105 0 003.292 4.022 4.095 4.095 0 01-1.853.07 4.108 4.108 0 003.834 2.85A8.233 8.233 0 012 18.407a11.616 11.616 0 006.29 1.84" />
        </svg>
      ),
    },
  ],
};

export function Footer() {
  return (
    <footer className="relative bg-gray-50 overflow-hidden border-t border-gray-200">
      {/* Remove background decoration elements */}
      {/* <div className="absolute bottom-0 right-0 ..." /> */}
      {/* <div className="absolute top-0 left-0 ..." /> */}
      
      <div className="mx-auto max-w-7xl overflow-hidden px-6 py-16 sm:py-20 lg:px-8">
        {/* Newsletter Subscription */}
        <div className="mb-16 p-6 lg:p-8 rounded-2xl bg-gradient-to-br from-white to-gray-100 shadow-lg ring-1 ring-gray-900/5">
          <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between">
            <div className="mb-6 lg:mb-0 lg:max-w-xl">
              <h2 className="text-xl font-bold text-gray-900 mb-2">Subscribe to our newsletter</h2>
              <p className="text-sm text-gray-600">Stay updated with the latest features, tutorials, and resources for Neo Service Layer.</p>
            </div>
            <div className="flex flex-col sm:flex-row gap-3 sm:max-w-md w-full">
              <input
                type="email"
                placeholder="Enter your email"
                className="form-input flex-grow text-sm rounded-md border-gray-300 bg-white text-gray-900 placeholder-gray-400 focus:ring-blue-500 focus:border-blue-500"
                aria-label="Email address"
              />
              <button className="rounded-md bg-blue-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600 transition-all duration-200 ease-in-out whitespace-nowrap hover:scale-[1.03] active:scale-[0.98]">
                Subscribe
              </button>
            </div>
          </div>
        </div>
        
        <div className="flex flex-col md:flex-row justify-between items-start mb-12 gap-8">
          <div className="mb-8 md:mb-0">
            <div className="flex items-center group">
              <div className="h-10 w-10 mr-3 relative group-hover:scale-110 transition-transform duration-300">
                 <img src="/logo.svg" alt="Neo Service Layer Logo" className="w-full h-full"/>
              </div>
              <div>
                <h2 className="text-xl font-bold text-gray-900">Neo Service Layer</h2>
                <p className="text-sm text-gray-500">Enterprise-grade blockchain services</p>
              </div>
            </div>
            
            <div className="mt-6 text-sm text-gray-500 max-w-xs">
              <p>Providing reliable infrastructure for Neo N3 with real-time price feeds, automated contract execution, and secure TEE environments.</p>
            </div>
            
            <div className="mt-6 flex space-x-4">
              {navigation.social.map((item) => (
                <Link 
                  key={item.name} 
                  href={item.href} 
                  className="text-gray-500 hover:text-blue-600 transition-colors duration-300 hover:scale-110 transform p-1"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <span className="sr-only">{item.name}</span>
                  <item.icon className="h-6 w-6" aria-hidden="true" />
                </Link>
              ))}
            </div>
          </div>

          <nav className="grid grid-cols-1 sm:grid-cols-3 gap-8 md:gap-10 lg:gap-16 w-full md:max-w-3xl" aria-label="Footer">
            <div>
              <h3 className="text-sm font-semibold leading-6 text-gray-900 uppercase tracking-wider">Main</h3>
              <ul role="list" className="mt-6 space-y-4">
                {navigation.main.map((item) => (
                  <li key={item.name}>
                    <Link 
                      href={item.href} 
                      className="text-sm leading-6 text-gray-600 hover:text-blue-600 transition-colors duration-200"
                    >
                      {item.name}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
            <div>
              <h3 className="text-sm font-semibold leading-6 text-gray-900 uppercase tracking-wider">Services</h3>
              <ul role="list" className="mt-6 space-y-4">
                {navigation.services.map((item) => (
                  <li key={item.name}>
                    <Link 
                      href={item.href} 
                      className="text-sm leading-6 text-gray-600 hover:text-blue-600 transition-colors duration-200"
                    >
                      {item.name}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
            <div>
              <h3 className="text-sm font-semibold leading-6 text-gray-900 uppercase tracking-wider">Resources</h3>
              <ul role="list" className="mt-6 space-y-4">
                {navigation.resources.map((item) => (
                  <li key={item.name}>
                    <Link 
                      href={item.href} 
                      className="text-sm leading-6 text-gray-600 hover:text-blue-600 transition-colors duration-200"
                    >
                      {item.name}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          </nav>
        </div>

        <div className="mt-10 pt-8 border-t border-gray-200">
          <div className="flex flex-col sm:flex-row justify-between items-center">
            <p className="text-sm text-gray-500 mb-4 sm:mb-0">
              &copy; {new Date().getFullYear()} Neo Service Layer. All rights reserved.
            </p>
            <div className="flex flex-wrap justify-center gap-x-6 gap-y-2">
              <Link href="/privacy" className="text-sm text-gray-500 hover:text-blue-600 transition-colors duration-200">
                Privacy Policy
              </Link>
              <Link href="/terms" className="text-sm text-gray-500 hover:text-blue-600 transition-colors duration-200">
                Terms of Service
              </Link>
              <Link href="/contact" className="text-sm text-gray-500 hover:text-blue-600 transition-colors duration-200">
                Contact Us
              </Link>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}