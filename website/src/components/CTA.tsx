'use client';

// @ts-ignore
import * as React from 'react';
import Link from 'next/link';
import { ArrowRightIcon } from '@heroicons/react/24/outline';

export function CTA() {
  return (
    <div className="bg-white">
      <div className="mx-auto max-w-7xl px-6 py-24 sm:py-32 lg:px-8">
        <div className="relative isolate overflow-hidden bg-gradient-to-br from-gray-900 to-gray-800 px-6 py-24 text-center shadow-2xl sm:rounded-3xl sm:px-16">
          <h2 className="mx-auto max-w-2xl text-3xl font-bold tracking-tight text-white sm:text-4xl">
            Start Building on <span className="text-green-400">Neo N3</span> Today
          </h2>
          <p className="mx-auto mt-6 max-w-xl text-lg leading-8 text-gray-300">
            Join the growing ecosystem of developers building innovative applications with our
            comprehensive enterprise-grade service layer.
          </p>
          <div className="mt-10 flex flex-col sm:flex-row items-center justify-center gap-x-6 gap-y-4">
            <Link
              href="/docs/getting-started"
              className="rounded-md bg-green-500 px-5 py-3 text-sm font-semibold text-white shadow-sm hover:bg-green-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-green-500 transition-colors duration-200 w-full sm:w-auto"
            >
              Get started
            </Link>
            <Link
              href="/playground"
              className="rounded-md border border-white/20 bg-white/10 backdrop-blur-sm px-5 py-3 text-sm font-semibold text-white hover:bg-white/20 transition-colors duration-200 w-full sm:w-auto"
            >
              Try Playground
            </Link>
            <Link
              href="/docs"
              className="text-sm font-semibold leading-6 text-white flex items-center group"
            >
              Learn more <ArrowRightIcon className="ml-1 h-4 w-4 transition-transform duration-200 group-hover:translate-x-1" />
            </Link>
          </div>
          
          {/* Features highlight */}
          <div className="mt-12 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 max-w-4xl mx-auto">
            <div className="bg-white/10 backdrop-blur-sm rounded-lg p-4 text-left hover:bg-white/15 transition-colors duration-200">
              <div className="text-green-400 font-semibold mb-1">Price Feeds</div>
              <div className="text-sm text-gray-300">Real-time price data for your dApps</div>
            </div>
            <div className="bg-white/10 backdrop-blur-sm rounded-lg p-4 text-left hover:bg-white/15 transition-colors duration-200">
              <div className="text-blue-400 font-semibold mb-1">Gas Bank</div>
              <div className="text-sm text-gray-300">Automated GAS distribution</div>
            </div>
            <div className="bg-white/10 backdrop-blur-sm rounded-lg p-4 text-left hover:bg-white/15 transition-colors duration-200">
              <div className="text-purple-400 font-semibold mb-1">Automation</div>
              <div className="text-sm text-gray-300">Schedule contract executions</div>
            </div>
            <div className="bg-white/10 backdrop-blur-sm rounded-lg p-4 text-left hover:bg-white/15 transition-colors duration-200">
              <div className="text-yellow-400 font-semibold mb-1">Trusted Execution</div>
              <div className="text-sm text-gray-300">Secure, isolated environment</div>
            </div>
          </div>
          
          <svg
            viewBox="0 0 1024 1024"
            className="absolute left-1/2 top-1/2 -z-10 h-[64rem] w-[64rem] -translate-x-1/2 -translate-y-1/2 [mask-image:radial-gradient(closest-side,white,transparent)]"
            aria-hidden="true"
          >
            <circle
              cx="512"
              cy="512"
              r="512"
              fill="url(#827591b1-ce8c-4110-b064-7cb85a0b1217)"
              fillOpacity="0.7"
            />
            <defs>
              <radialGradient id="827591b1-ce8c-4110-b064-7cb85a0b1217">
                <stop stopColor="#00E599" />
                <stop offset="1" stopColor="#1E2B34" />
              </radialGradient>
            </defs>
          </svg>
        </div>
      </div>
    </div>
  );
}