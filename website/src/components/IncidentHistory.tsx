'use client';

// @ts-ignore
import * as React from 'react';
import { CheckCircleIcon } from '@heroicons/react/24/outline';

interface Incident {
  id: number;
  date: string;
  title: string;
  description: string;
  status: string;
  duration: string;
}

interface IncidentHistoryProps {
  incidents: Incident[];
}

export function IncidentHistory({ incidents }: IncidentHistoryProps) {
  return (
    <div className="flow-root">
      <ul role="list" className="-mb-8">
        {incidents.map((incident, index) => (
          <li
            key={incident.id}
          >
            <div className="relative pb-8">
              {index !== incidents.length - 1 ? (
                <span
                  className="absolute left-4 top-4 -ml-px h-full w-0.5 bg-gray-200 dark:bg-gray-700"
                  aria-hidden="true"
                />
              ) : null}
              <div className="relative flex space-x-3">
                <div>
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-green-100 dark:bg-green-900/20">
                    <CheckCircleIcon className="h-5 w-5 text-green-500" aria-hidden="true" />
                  </span>
                </div>
                <div className="flex min-w-0 flex-1 justify-between space-x-4">
                  <div>
                    <p className="text-sm font-medium text-gray-900 dark:text-white">
                      {incident.title}
                    </p>
                    <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                      {incident.description}
                    </p>
                  </div>
                  <div className="whitespace-nowrap text-right text-sm">
                    <time className="text-gray-500 dark:text-gray-400">
                      {incident.date}
                    </time>
                    <p className="mt-1 text-gray-500 dark:text-gray-400">
                      Duration: {incident.duration}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}