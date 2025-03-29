'use client';

// @ts-ignore
import * as React from 'react';
import Link from 'next/link';
import {
  ChartBarIcon,
  BoltIcon,
  FireIcon,
  CommandLineIcon,
  KeyIcon,
  CodeBracketIcon,
} from '@heroicons/react/24/outline';

export interface ServiceStats {
  [key: string]: string;
}

export interface Service {
  title: string;
  description: string;
  icon: string;
  link: string;
  stats: ServiceStats;
}

const icons = {
  chart: ChartBarIcon,
  automation: BoltIcon,
  gas: FireIcon,
  function: CommandLineIcon,
  secrets: KeyIcon,
  api: CodeBracketIcon,
};

export function ServiceCard({ service }: { service: Service }) {
  const Icon = icons[service.icon as keyof typeof icons];

  return (
    <div
      className="flex flex-col bg-white rounded-2xl shadow-lg overflow-hidden hover:shadow-xl transition-shadow duration-300"
    >
      <div className="p-6">
        <div className="flex items-center justify-between">
          <div className="bg-indigo-500 rounded-lg p-2">
            <Icon className="h-6 w-6 text-white" />
          </div>
          <Link
            href={service.link}
            className="text-sm font-medium text-indigo-600 hover:text-indigo-500"
          >
            Learn more <span aria-hidden="true">&rarr;</span>
          </Link>
        </div>
        <div className="mt-4">
          <h3 className="text-xl font-semibold text-gray-900">{service.title}</h3>
          <p className="mt-2 text-base text-gray-500">{service.description}</p>
        </div>
        <div className="mt-6 grid grid-cols-3 gap-4 border-t border-gray-100 pt-4">
          {Object.entries(service.stats).map(([key, value]) => (
            <div key={key} className="text-center">
              <div className="text-lg font-semibold text-indigo-600">{value}</div>
              <div className="mt-1 text-xs text-gray-500 capitalize">{key}</div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}