'use client';

// @ts-ignore
import * as React from 'react';
import { CheckCircleIcon, ExclamationCircleIcon, XCircleIcon } from '@heroicons/react/24/outline';

interface Service {
  name: string;
  status: string;
  metrics: {
    uptime: string;
    [key: string]: string;
  };
}

interface StatusGridProps {
  services: Service[];
}

const getStatusIcon = (status: string) => {
  switch (status.toLowerCase()) {
    case 'operational':
      return <CheckCircleIcon className="h-6 w-6 text-green-500" />;
    case 'degraded':
      return <ExclamationCircleIcon className="h-6 w-6 text-yellow-500" />;
    case 'outage':
      return <XCircleIcon className="h-6 w-6 text-red-500" />;
    default:
      return <ExclamationCircleIcon className="h-6 w-6 text-gray-400" />;
  }
};

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'operational':
      return 'bg-green-50 dark:bg-green-900/20';
    case 'degraded':
      return 'bg-yellow-50 dark:bg-yellow-900/20';
    case 'down':
      return 'bg-red-50 dark:bg-red-900/20';
    default:
      return 'bg-green-50 dark:bg-green-900/20';
  }
};

export function StatusGrid({ services }: StatusGridProps) {
  const [activeService, setActiveService] = React.useState<Service | null>(null);
  const [isLoading, setIsLoading] = React.useState(false);

  React.useEffect(() => {
    // Simulate loading state
    setIsLoading(true);
    setTimeout(() => {
      setIsLoading(false);
    }, 1000);
  }, []);

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {services.map((service, index) => (
        <div
          key={service.name}
          className={`relative rounded-lg p-6 ${getStatusColor(service.status)}`}
        >
          <div className="flex items-center">
            <div className="flex-shrink-0">
              {getStatusIcon(service.status)}
            </div>
            <div className="ml-4">
              <h3 className="text-sm font-medium text-gray-900 dark:text-white">
                {service.name}
              </h3>
              <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                Uptime: {service.metrics.uptime}
              </p>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}