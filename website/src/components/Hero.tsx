'use client';

// @ts-ignore
import * as React from 'react';
import { ArrowRightIcon } from '@heroicons/react/24/outline';
import Link from 'next/link';
import Image from 'next/image';

function DashboardSVG() {
  return (
    <svg
      viewBox="0 0 800 600"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className="w-full h-full"
    >
      <rect width="800" height="600" rx="8" fill="#F8FAFC" />
      
      {/* Dashboard Header */}
      <rect x="20" y="20" width="760" height="60" rx="4" fill="#E2E8F0" />
      <rect x="40" y="40" width="120" height="20" rx="2" fill="#94A3B8" />
      
      {/* Sidebar */}
      <rect x="20" y="100" width="200" height="480" rx="4" fill="#E2E8F0" />
      <rect x="40" y="120" width="160" height="20" rx="2" fill="#94A3B8" />
      <rect x="40" y="160" width="160" height="20" rx="2" fill="#94A3B8" />
      <rect x="40" y="200" width="160" height="20" rx="2" fill="#94A3B8" />
      
      {/* Main Content */}
      <rect x="240" y="100" width="540" height="220" rx="4" fill="#E2E8F0" />
      <rect x="260" y="120" width="200" height="20" rx="2" fill="#94A3B8" />
      <rect x="260" y="160" width="500" height="140" rx="2" fill="#94A3B8" />
      
      {/* Bottom Cards */}
      <rect x="240" y="340" width="260" height="240" rx="4" fill="#E2E8F0" />
      <rect x="520" y="340" width="260" height="240" rx="4" fill="#E2E8F0" />
      
      {/* Neo Logo */}
      <circle cx="100" cy="50" r="20" fill="#00E599" />
      <path d="M90 50 L110 40 L110 60 Z" fill="white" />
    </svg>
  );
}

export function Hero() {
  return (
    <div className="relative isolate overflow-hidden bg-gradient-to-b from-indigo-100/20 via-white to-white">
      <div className="absolute inset-x-0 -top-40 -z-10 transform-gpu overflow-hidden blur-3xl sm:-top-80"
        aria-hidden="true">
        <div className="relative left-[calc(50%-11rem)] aspect-[1155/678] w-[36.125rem] -translate-x-1/2 rotate-[30deg] bg-gradient-to-tr from-[#00E599] to-[#9089fc] opacity-30 sm:left-[calc(50%-30rem)] sm:w-[72.1875rem]"
          style={{
            clipPath:
              'polygon(74.1% 44.1%, 100% 61.6%, 97.5% 26.9%, 85.5% 0.1%, 80.7% 2%, 72.5% 32.5%, 60.2% 62.4%, 52.4% 68.1%, 47.5% 58.3%, 45.2% 34.5%, 27.5% 76.7%, 0.1% 64.9%, 17.9% 100%, 27.6% 76.8%, 76.1% 97.7%, 74.1% 44.1%)',
          }}
        />
      </div>
      <div className="mx-auto max-w-7xl px-6 pb-24 pt-10 sm:pb-32 lg:flex lg:px-8 lg:py-40">
        <div
          className="mx-auto max-w-2xl flex-shrink-0 lg:mx-0 lg:max-w-xl lg:pt-8 animate-fade-in-up"
        >
          <div className="mt-24 sm:mt-32 lg:mt-16">
            <Link
              href="/docs"
              className="inline-flex space-x-6"
            >
              <span className="rounded-full bg-green-600/10 px-3 py-1 text-sm font-semibold leading-6 text-green-600 ring-1 ring-inset ring-green-600/10">
                Latest updates
              </span>
              <span className="inline-flex items-center space-x-2 text-sm font-medium leading-6 text-gray-600">
                <span>Just released v1.0</span>
                <svg
                  className="h-5 w-5 text-gray-400"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                  aria-hidden="true"
                >
                  <path
                    fillRule="evenodd"
                    d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z"
                    clipRule="evenodd"
                  />
                </svg>
              </span>
            </Link>
          </div>
          <h1 
            className="mt-10 text-4xl font-bold tracking-tight text-gray-900 sm:text-6xl animate-fade-in delay-200"
          >
            <span className="block text-green-600">Neo</span> Service Layer
          </h1>
          <p 
            className="mt-6 text-lg leading-8 text-gray-600 animate-fade-in delay-400"
          >
            A comprehensive enterprise-grade service layer providing oracle services, automated functions, price feeds, 
            and secure infrastructure for the Neo N3 blockchain. Build powerful decentralized
            applications with confidence.
          </p>
          <div 
            className="mt-10 flex items-center gap-x-6 animate-fade-in delay-600"
          >
            <Link
              href="/docs/getting-started"
              className="rounded-md bg-green-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-green-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-green-600 transition-colors duration-200"
            >
              Get started
            </Link>
            <Link
              href="/playground"
              className="text-sm font-semibold leading-6 text-gray-900 flex items-center group"
            >
              Try Playground <ArrowRightIcon className="ml-1 h-4 w-4 transition-transform duration-200 group-hover:translate-x-1" />
            </Link>
          </div>
          
          <div 
            className="mt-10 flex flex-wrap gap-4 animate-fade-in delay-800"
          >
            <div className="flex items-center gap-x-2">
              <div className="h-4 w-4 rounded-full bg-green-400"></div>
              <p className="text-sm text-gray-500">99.9% Uptime</p>
            </div>
            <div className="flex items-center gap-x-2">
              <div className="h-4 w-4 rounded-full bg-blue-400"></div>
              <p className="text-sm text-gray-500">Enterprise Support</p>
            </div>
            <div className="flex items-center gap-x-2">
              <div className="h-4 w-4 rounded-full bg-purple-400"></div>
              <p className="text-sm text-gray-500">Trusted Execution</p>
            </div>
            <div className="flex items-center gap-x-2">
              <div className="h-4 w-4 rounded-full bg-yellow-400"></div>
              <p className="text-sm text-gray-500">Secure Infrastructure</p>
            </div>
          </div>
        </div>
        <div className="mx-auto mt-16 flex max-w-2xl sm:mt-24 lg:ml-10 lg:mr-0 lg:mt-0 lg:max-w-none xl:ml-32">
          <div className="max-w-3xl flex-none sm:max-w-5xl lg:max-w-none">
            <div
              className="rounded-xl bg-white/5 shadow-2xl ring-1 ring-gray-900/10 overflow-hidden animate-fade-in-scale delay-200"
            >
              <DashboardSVG />
            </div>
          </div>
        </div>
      </div>
      <div className="absolute inset-x-0 top-[calc(100%-13rem)] -z-10 transform-gpu overflow-hidden blur-3xl sm:top-[calc(100%-30rem)]"
        aria-hidden="true">
        <div className="relative left-[calc(50%+3rem)] aspect-[1155/678] w-[36.125rem] -translate-x-1/2 bg-gradient-to-tr from-[#00E599] to-[#9089fc] opacity-30 sm:left-[calc(50%+36rem)] sm:w-[72.1875rem]"
          style={{
            clipPath:
              'polygon(74.1% 44.1%, 100% 61.6%, 97.5% 26.9%, 85.5% 0.1%, 80.7% 2%, 72.5% 32.5%, 60.2% 62.4%, 52.4% 68.1%, 47.5% 58.3%, 45.2% 34.5%, 27.5% 76.7%, 0.1% 64.9%, 17.9% 100%, 27.6% 76.8%, 76.1% 97.7%, 74.1% 44.1%)',
          }}
        />
      </div>
    </div>
  );
}