'use client';

// @ts-ignore
import * as React from 'react';
import Link from 'next/link';
import { ArrowRightIcon } from '@heroicons/react/24/outline';
import { motion } from 'framer-motion';

export function CTA() {
  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }} 
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true, amount: 0.2 }}
      transition={{ duration: 0.6, ease: "easeOut" }}
      className="bg-white py-24 sm:py-32"
    >
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="relative isolate overflow-hidden bg-gradient-to-r from-gray-100 to-gray-200 px-6 py-16 sm:py-24 text-center shadow-xl sm:rounded-3xl sm:px-16">
          <h2 className="mx-auto max-w-2xl text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl">
            Start Building on <span className="text-green-600">Neo N3</span> Today
          </h2>
          <p className="mx-auto mt-6 max-w-xl text-lg leading-8 text-gray-600">
            Join the growing ecosystem of developers building innovative applications with our
            comprehensive enterprise-grade service layer.
          </p>
          <div className="mt-10 flex flex-col sm:flex-row items-center justify-center gap-x-6 gap-y-4">
            <Link
              href="/docs/getting-started/introduction"
              className="rounded-md bg-blue-600 px-5 py-3 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600 transition-all duration-200 ease-in-out w-full sm:w-auto hover:scale-[1.03] active:scale-[0.98]"
            >
              Get started
            </Link>
            <Link
              href="/playground"
              className="rounded-md border border-blue-600 px-5 py-3 text-sm font-semibold text-blue-600 hover:bg-blue-50 transition-all duration-200 ease-in-out w-full sm:w-auto hover:scale-[1.03] active:scale-[0.98]"
            >
              Try Playground
            </Link>
            <Link
              href="/docs"
              className="text-sm font-semibold leading-6 text-blue-600 flex items-center group hover:text-blue-500 transition-all duration-200 ease-in-out hover:scale-[1.03] active:scale-[0.98]"
            >
              Learn more <ArrowRightIcon className="ml-1 h-4 w-4 transition-transform duration-200 group-hover:translate-x-1" />
            </Link>
          </div>
        </div>
      </div>
    </motion.div>
  );
}