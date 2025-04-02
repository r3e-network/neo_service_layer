'use client';

// @ts-ignore
import * as React from 'react';
import { ArrowRightIcon, CheckCircleIcon } from '@heroicons/react/24/solid';
import Link from 'next/link';
import { motion } from 'framer-motion';

// New Detailed Bridge Graphic for Light Theme
function DetailedBridgeGraphic() {
  const id = "bridgeGradLight";
  const bgColor = '#F9FAFB'; // Light gray background (gray-50)
  const web2Color = '#6B7280'; // gray-500
  const web3Color = '#10B981'; // green-500
  const bridgeColor = `url(#${id})`;
  const lineColor = '#9CA3AF'; // gray-400
  const iconColor = '#4B5563'; // gray-600

  return (
    <svg
      viewBox="0 0 800 400" // Adjusted viewBox height
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={`w-full h-full overflow-hidden`}
      style={{ backgroundColor: bgColor }}
    >
       <style>{/* Animation for connection lines */ `
          @keyframes subtle-dash-flow {
            to { stroke-dashoffset: -16; }
          }
          .subtle-flow-line {
             stroke-dasharray: 4 4;
             stroke-dashoffset: 0;
             animation: subtle-dash-flow 1.5s linear infinite;
             opacity: 0.8;
          }
       `}</style>
      <defs>
         <linearGradient id={id} x1="0%" y1="0%" x2="100%" y2="100%">
           <stop offset="0%" stopColor="#60A5FA" /> {/* blue-400 */}
           <stop offset="100%" stopColor="#34D399" /> {/* green-400 */}
         </linearGradient>
         {/* Optional faint background grid */}
          <pattern id="lightGrid" width="40" height="40" patternUnits="userSpaceOnUse">
            <path d="M 40 0 L 0 0 0 40" fill="none" stroke="#E5E7EB" strokeWidth="0.5" /> {/* gray-200 */}
          </pattern>
      </defs>

      {/* Background Grid */}
      <rect width="100%" height="100%" fill="url(#lightGrid)" opacity="0.5"/>

      {/* Web2 Icons (Left) */}
      <g transform="translate(120 200)">
         <circle cx="0" cy="0" r="90" fill="#FFFFFF" stroke="#E5E7EB" strokeWidth="1"/>
         {/* Database Icon */}
         <g transform="translate(-30 -30)" stroke={iconColor} strokeWidth="1.5" opacity="0.8">
            <ellipse cx="0" cy="0" rx="15" ry="6"/>
            <path d="M-15 0 V20 A15 6 0 0 0 15 20 V0"/>
            <path d="M-15 8 A15 6 0 0 0 15 8"/>
            <path d="M-15 14 A15 6 0 0 0 15 14"/>
         </g>
          {/* API Icon */}
          <g transform="translate(30 -30)" stroke={iconColor} strokeWidth="2" strokeLinecap="round" opacity="0.8">
             <path d="M-8 -8 L -14 0 L -8 8"/>
             <path d="M8 -8 L 14 0 L 8 8"/>
          </g>
           {/* Cloud Icon */}
          <g transform="translate(0 30)">
             <path d="M-15 -4 C -25 -4 -25 7 -15 7 L 12 7 C 22 7 22 -8 10 -8 C 5 -15 -10 -15 -15 -4 Z" fill={iconColor} opacity="0.1"/>
             <path d="M-15 -4 C -25 -4 -25 7 -15 7 L 12 7 C 22 7 22 -8 10 -8 C 5 -15 -10 -15 -15 -4 Z" stroke={iconColor} strokeWidth="1.5" opacity="0.7"/>
          </g>
           <text x="0" y="115" textAnchor="middle" fontSize="14" fontWeight="medium" fill={web2Color}>Web2 World</text>
      </g>

      {/* Web3/Neo Icons (Right) */}
      <g transform="translate(680 200)">
         <circle cx="0" cy="0" r="90" fill="#FFFFFF" stroke="#E5E7EB" strokeWidth="1"/>
          {/* Neo Logo Icon */}
         <g transform="translate(0 -30)">
            <path d="M-10,-12 L0,-25 L10,-12 L0,1 Z" fill={web3Color} transform="scale(1.5)" opacity="0.9"/>
         </g>
         {/* Blocks Icon */}
          <g transform="translate(-25 25)" fill="none" stroke={web3Color} strokeWidth="1.5" opacity="0.7">
            <rect x="0" y="0" width="12" height="12" rx="1"/>
            <rect x="15" y="-5" width="12" height="12" rx="1"/>
            <rect x="8" y="15" width="12" height="12" rx="1"/>
          </g>
           {/* Contract Icon */}
          <g transform="translate(25 25)" fill="none" stroke={iconColor} strokeWidth="1.5" opacity="0.7">
            <path d="M-8,-12 Q -8 -15 0 -15 Q 8 -15 8 -12 V12 Q 8 15 0 15 Q -8 15 -8 12 Z"/>
            <line x1="-5" y1="-8" x2="5" y2="-8"/>
            <line x1="-5" y1="-4" x2="5" y2="-4"/>
            <line x1="-5" y1="0" x2="3" y2="0"/>
          </g>
          <text x="0" y="115" textAnchor="middle" fontSize="14" fontWeight="medium" fill={web3Color}>Neo N3 World</text>
      </g>

      {/* Service Layer Bridge (Center) */}
      <g transform="translate(400 200)">
          {/* Platform */}
          <rect x="-100" y="-60" width="200" height="120" rx="10" fill="#FFFFFF" stroke="#D1D5DB" strokeWidth="1" />
          {/* Logo Bars */}
          <g transform="translate(-24 -15) scale(2.5)">
            <path d="M16 22 H28 L32 30 H20 Z" fill={bridgeColor}/>
            <path d="M8 11 H20 L24 19 H12 Z" fill={bridgeColor} />
            <path d="M0 0 H12 L16 8 H4 Z" fill={bridgeColor} />
          </g>
         <text x="0" y="90" textAnchor="middle" fontSize="16" fontWeight="semibold" fill="#1D4ED8">Service Layer</text>
      </g>

       {/* Connection Lines */} 
       <g strokeWidth="2" strokeLinecap="round">
         {/* Web2 -> Bridge */}
         <line x1="215" y1="200" x2="295" y2="200" stroke={lineColor} className="subtle-flow-line"/>
         <path d="M290 196 L 300 200 L 290 204" fill={lineColor}/>
          {/* Bridge -> Web3 */}
         <line x1="505" y1="200" x2="585" y2="200" stroke={lineColor} className="subtle-flow-line" style={{animationDirection: 'reverse'}}/>
         <path d="M580 196 L 590 200 L 580 204" fill={lineColor}/>
       </g>

    </svg>
  );
}

export function Hero() {
  return (
    <motion.div 
      initial={{ opacity: 0 }}
      whileInView={{ opacity: 1 }}
      viewport={{ once: true, amount: 0.2 }}
      transition={{ duration: 0.8 }}
      className="relative isolate overflow-hidden bg-white py-24 sm:py-32"
    >
       {/* Optional: Add light-themed decorative elements like faint gradients or shapes */}
       <div className="absolute inset-x-0 -top-40 -z-10 transform-gpu overflow-hidden blur-3xl sm:-top-80" aria-hidden="true">
        <div className="relative left-[calc(50%-11rem)] aspect-[1155/678] w-[36.125rem] -translate-x-1/2 rotate-[30deg] bg-gradient-to-tr from-[#A5B4FC]/50 to-[#34D399]/50 opacity-30 sm:left-[calc(50%-30rem)] sm:w-[72.1875rem]" style={{ clipPath: 'polygon(74.1% 44.1%, 100% 61.6%, 97.5% 26.9%, 85.5% 0.1%, 80.7% 2%, 72.5% 32.5%, 60.2% 62.4%, 52.4% 68.1%, 47.5% 58.3%, 45.2% 34.5%, 27.5% 76.7%, 0.1% 64.9%, 17.9% 100%, 27.6% 76.8%, 76.1% 97.7%, 74.1% 44.1%)' }} />
      </div>

      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="flex flex-col lg:flex-row lg:items-center">
          <div
            className="w-full lg:w-1/2 lg:pr-8 xl:pr-16"
          >
            {/* Text colors for light theme */}
            <h1 
              className="mt-10 text-4xl font-bold tracking-tight text-gray-900 sm:text-6xl"
            >
              {/* Keep green accent */}
              <span className="block text-green-600">Neo</span> Service Layer
            </h1>
            <p 
              className="mt-6 text-lg leading-8 text-gray-600"
            >
              A comprehensive enterprise-grade service layer providing oracle services, automated functions, price feeds, 
              and secure infrastructure for the Neo N3 blockchain.
            </p>
            <div 
              className="mt-10 flex items-center gap-x-6"
            >
              {/* Buttons for light theme - Blue primary, Outline secondary */}
              <Link
                href="/docs/getting-started/introduction"
                className="rounded-md bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600 transition-all duration-200 ease-in-out hover:scale-[1.03] active:scale-[0.98]"
              >
                Get started
              </Link>
              <Link
                href="/playground"
                className="rounded-md border border-blue-600 px-4 py-2.5 text-sm font-semibold text-blue-600 hover:bg-blue-50 transition-all duration-200 ease-in-out hover:scale-[1.03] active:scale-[0.98] group"
              >
                Try Playground <ArrowRightIcon className="ml-1 h-4 w-4 transition-transform duration-200 group-hover:translate-x-1" />
              </Link>
            </div>
            
            {/* Trust Signals: Blue icon, darker gray text */}
            <div 
              className="mt-10 grid grid-cols-2 gap-x-6 gap-y-4 sm:grid-cols-4"
            >
              <div className="flex items-center gap-x-2">
                <CheckCircleIcon className="h-5 w-5 text-blue-600" />
                <p className="text-sm text-gray-600">High Uptime</p>
              </div>
              <div className="flex items-center gap-x-2">
                <CheckCircleIcon className="h-5 w-5 text-blue-600" />
                <p className="text-sm text-gray-600">Enterprise Support</p>
              </div>
              <div className="flex items-center gap-x-2">
                <CheckCircleIcon className="h-5 w-5 text-blue-600" />
                <p className="text-sm text-gray-600">Trusted Execution</p>
              </div>
              <div className="flex items-center gap-x-2">
                <CheckCircleIcon className="h-5 w-5 text-blue-600" />
                <p className="text-sm text-gray-600">Secure Infrastructure</p>
              </div>
            </div>
          </div>
          <div className="w-full lg:w-1/2 mt-16 lg:mt-0">
            <div className="max-w-3xl flex-none sm:max-w-5xl lg:max-w-none">
              {/* SVG container styling for light theme */}
              <div
                className="rounded-xl bg-white shadow-xl ring-1 ring-gray-900/10 overflow-hidden"
              >
                 {/* Use the NEW DetailedBridgeGraphic */}
                 <DetailedBridgeGraphic />
              </div>
            </div>
          </div>
        </div>
      </div>
       {/* Add bottom decorative element */}
       <div className="absolute inset-x-0 top-[calc(100%-13rem)] -z-10 transform-gpu overflow-hidden blur-3xl sm:top-[calc(100%-30rem)]" aria-hidden="true">
        <div className="relative left-[calc(50%+3rem)] aspect-[1155/678] w-[36.125rem] -translate-x-1/2 bg-gradient-to-tr from-[#A5B4FC]/50 to-[#34D399]/50 opacity-30 sm:left-[calc(50%+36rem)] sm:w-[72.1875rem]" style={{ clipPath: 'polygon(74.1% 44.1%, 100% 61.6%, 97.5% 26.9%, 85.5% 0.1%, 80.7% 2%, 72.5% 32.5%, 60.2% 62.4%, 52.4% 68.1%, 47.5% 58.3%, 45.2% 34.5%, 27.5% 76.7%, 0.1% 64.9%, 17.9% 100%, 27.6% 76.8%, 76.1% 97.7%, 74.1% 44.1%)' }} />
      </div>
    </motion.div>
  );
}