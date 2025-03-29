'use client';

// @ts-ignore
import * as React from 'react';

interface Service {
  name: string;
  status: string;
  metrics: {
    uptime: string;
    [key: string]: string;
  };
}

interface MetricsDisplayProps {
  services: Service[];
}

export function MetricsDisplay({ services }: MetricsDisplayProps) {
  return (
    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
      {services.map((service, index) => (
        <div
          key={service.name}
          className="rounded-lg bg-white p-6 shadow-sm dark:bg-gray-800"
        >
          <h3 className="text-lg font-medium text-gray-900 dark:text-white">
            {service.name}
          </h3>
          <dl className="mt-4 space-y-4">
            {Object.entries(service.metrics).map(([key, value]) => (
              <div key={key} className="flex justify-between">
                <dt className="text-sm font-medium text-gray-500 dark:text-gray-400 capitalize">
                  {key.replace('_', ' ')}
                </dt>
                <dd className="text-sm font-semibold text-gray-900 dark:text-white">
                  {value}
                </dd>
              </div>
            ))}
          </dl>
        </div>
      ))}
    </div>
  );
}