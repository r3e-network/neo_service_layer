// @ts-ignore
import * as React from 'react';
import { CurrencyDollarIcon, ShieldCheckIcon } from '@heroicons/react/24/outline';
import PriceFeedDisplay from './components/PriceFeedDisplay';

export default function PriceFeedPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <div className="inline-flex items-center justify-center p-3 rounded-full bg-green-100 dark:bg-green-900 mb-6">
            <CurrencyDollarIcon className="h-8 w-8 text-green-600 dark:text-green-400" />
          </div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Price Feed Service
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Real-time price data from multiple sources with sophisticated filtering and validation.
          </p>
        </div>
        
        <div className="mt-16 flow-root">
          {/* Interactive Price Feed Display */}
          <div className="mx-auto max-w-4xl mb-12">
            <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Real-time Price Data</h2>
            <PriceFeedDisplay />
          </div>
          
          <div className="mt-10 space-y-8 border-t border-gray-200 dark:border-gray-700 pt-10 sm:mt-16 sm:pt-16">
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Key Features</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Multi-source Aggregation</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    Our price feed service collects data from multiple trusted providers to ensure accuracy and reliability.
                    By aggregating data from various sources, we can identify and filter out anomalies.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Kalman Filtering</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    We employ advanced Kalman filtering techniques to process noisy price data and estimate true underlying values.
                    This provides more stable price feeds with reduced volatility from market noise.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Outlier Detection</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    Our service automatically identifies and excludes outlier data points that could skew price reporting.
                    This ensures that manipulated or erroneous data doesn't affect our price feeds.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Historical Accuracy Tracking</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    We maintain detailed metrics on the historical accuracy of our price feeds compared to market standards.
                    This allows us to continually refine our models and improve data quality.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Usage</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Integration</h3>
                <p className="text-gray-600 dark:text-gray-400 mb-4">
                  Price feed data can be accessed directly on-chain through our oracle contracts or via our API.
                </p>
                <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
                  <pre className="text-sm overflow-x-auto">
                    <code className="language-javascript">
{`// Sample API usage
const priceData = await neo.services.priceFeeds.getLatestPrice({
  asset: 'NEO/USD',
  sources: ['binance', 'coinbase', 'huobi'],
  aggregationMethod: 'median'
});

console.log(priceData);
// {
//   price: "27.45",
//   timestamp: 1648218739,
//   confidence: 0.98,
//   sources: ['binance', 'coinbase', 'huobi']
// }`}
                    </code>
                  </pre>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Supported Assets</h2>
              
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                {['NEO/USD', 'GAS/USD', 'BTC/USD', 'ETH/USD', 'FLM/USD', 'USDT/USD'].map((asset) => (
                  <div key={asset} className="bg-white dark:bg-gray-800 p-4 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 flex items-center">
                    <ShieldCheckIcon className="h-5 w-5 text-green-500 mr-2" />
                    <span className="text-gray-900 dark:text-white font-medium">{asset}</span>
                  </div>
                ))}
              </div>
              <p className="mt-4 text-gray-600 dark:text-gray-400 text-sm">
                Additional assets can be added upon request.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
} 