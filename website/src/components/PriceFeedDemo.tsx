'use client';

import React from 'react';
import { ArrowUpIcon, ArrowDownIcon } from '@heroicons/react/24/solid';

interface PriceData {
  symbol: string;
  price: number;
  change: number;
  lastUpdate: string;
}

export function PriceFeedDemo() {
  const [prices, setPrices] = React.useState<PriceData[]>([]);
  const [loading, setLoading] = React.useState(true);

  React.useEffect(() => {
    const fetchPrices = async () => {
      try {
        const response = await fetch('/.netlify/functions/get-price-feeds');
        const data = await response.json();
        setPrices(
          data.map((item: any) => ({
            symbol: item.symbol,
            price: item.price,
            change: (Math.random() * 10 - 5).toFixed(2), // Mock data
            lastUpdate: new Date().toISOString(),
          }))
        );
      } catch (error) {
        console.error('Failed to fetch prices:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchPrices();
    const interval = setInterval(fetchPrices, 30000); // Update every 30 seconds

    return () => clearInterval(interval);
  }, []);

  return (
    <div className="mt-8">
      <div className="rounded-xl bg-white shadow-lg overflow-hidden">
        <div className="px-4 py-5 sm:p-6">
          <h3 className="text-lg font-medium leading-6 text-gray-900">
            Live Price Feed Demo
          </h3>
          <div className="mt-4">
            {loading ? (
              <div className="animate-pulse space-y-4">
                {[...Array(3)].map((_, i) => (
                  <div
                    key={i}
                    className="h-16 bg-gray-200 rounded-lg"
                  />
                ))}
              </div>
            ) : (
              <div className="space-y-4">
                {prices.map((price) => (
                  <div
                    key={price.symbol}
                    className="bg-gray-50 rounded-lg p-4"
                  >
                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="text-sm font-medium text-gray-900">
                          {price.symbol}
                        </h4>
                        <p className="mt-1 text-2xl font-semibold text-gray-900">
                          ${price.price.toFixed(2)}
                        </p>
                      </div>
                      <div className="text-right">
                        <div className={`flex items-center ${
                          Number(price.change) >= 0
                            ? 'text-green-600'
                            : 'text-red-600'
                        }`}>
                          {Number(price.change) >= 0 ? (
                            <ArrowUpIcon className="h-4 w-4" />
                          ) : (
                            <ArrowDownIcon className="h-4 w-4" />
                          )}
                          <span className="ml-1">{Math.abs(Number(price.change))}%</span>
                        </div>
                        <p className="mt-1 text-xs text-gray-500">
                          Last updated: {new Date(price.lastUpdate).toLocaleTimeString()}
                        </p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}