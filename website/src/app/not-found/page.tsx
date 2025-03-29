import React from 'react';
import Link from 'next/link';

export const metadata = {
  title: 'Page Not Found - Neo Service Layer',
  description: 'The page you are looking for does not exist.',
};

export default function NotFound() {
  return (
    <div className="min-h-screen bg-white dark:bg-gray-900 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
        <div className="text-center">
          <h1 className="text-6xl font-extrabold text-neo-green">404</h1>
          <h2 className="mt-4 text-3xl font-bold text-gray-900 dark:text-white">Page not found</h2>
          <p className="mt-2 text-base text-gray-600 dark:text-gray-400">
            Sorry, we couldn't find the page you're looking for.
          </p>
          <div className="mt-6">
            <Link
              href="/"
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-neo-green hover:bg-neo-green/90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-neo-green"
            >
              Go back home
            </Link>
          </div>
        </div>
        
        <div className="mt-10">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white">Popular pages</h3>
          <ul className="mt-4 divide-y divide-gray-200 dark:divide-gray-800">
            <li className="py-2">
              <Link href="/docs" className="text-neo-green hover:text-neo-green/80">
                Documentation
              </Link>
            </li>
            <li className="py-2">
              <Link href="/price-feed" className="text-neo-green hover:text-neo-green/80">
                Price Feed
              </Link>
            </li>
            <li className="py-2">
              <Link href="/api-reference" className="text-neo-green hover:text-neo-green/80">
                API Reference
              </Link>
            </li>
            <li className="py-2">
              <Link href="/examples" className="text-neo-green hover:text-neo-green/80">
                Examples
              </Link>
            </li>
          </ul>
        </div>
      </div>
    </div>
  );
}