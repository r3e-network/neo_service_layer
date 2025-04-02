// @ts-ignore
import * as React from 'react';
import { KeyIcon, LockClosedIcon, DocumentTextIcon, ArrowPathIcon } from '@heroicons/react/24/outline';

export default function SecretsPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <div className="inline-flex items-center justify-center p-3 rounded-full bg-yellow-100 dark:bg-yellow-900 mb-6">
            <KeyIcon className="h-8 w-8 text-yellow-600 dark:text-yellow-400" />
          </div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Secrets Management Service
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Secure storage and management of sensitive data and credentials.
          </p>
        </div>
        
        <div className="mt-16 flow-root">
          <div className="mt-10 space-y-8 border-t border-gray-200 dark:border-gray-700 pt-10 sm:mt-16 sm:pt-16">
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Key Features</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <LockClosedIcon className="h-6 w-6 text-yellow-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">TEE Protection</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    All secrets are stored and processed within Trusted Execution Environments (TEEs), 
                    isolating your sensitive data from the underlying infrastructure and service operators.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <KeyIcon className="h-6 w-6 text-yellow-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Access Control</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Fine-grained permission system allows you to control which functions or services 
                    can access specific secrets, with support for temporary access grants.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <DocumentTextIcon className="h-6 w-6 text-yellow-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Audit Logging</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Comprehensive audit logs track all access to your secrets, including when and by which function,
                    providing transparency and security verification.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <ArrowPathIcon className="h-6 w-6 text-yellow-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Key Rotation</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Automated key rotation schedules and version management allow you to maintain 
                    secure credential practices without service interruption.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Common Use Cases</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 space-y-6">
                <div>
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">API Integration</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    Securely store API keys for external services like exchanges, data providers, or payment processors.
                    Your functions can access these credentials without exposing them in your code or config files.
                  </p>
                </div>
                
                <div>
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Private Transaction Data</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    Store sensitive transaction parameters that should not be visible on-chain, 
                    such as business logic thresholds, trade strategies, or personal user information.
                  </p>
                </div>
                
                <div>
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Encryption Keys</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    Manage encryption/decryption keys for secure data handling within your applications,
                    enabling end-to-end encrypted workflows across on-chain and off-chain components.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Getting Started</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                <p className="text-gray-600 dark:text-gray-400 mb-4">
                  Store and use a secret in your function:
                </p>
                <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
                  <pre className="text-sm overflow-x-auto">
                    <code className="language-javascript">
{`// First, store your secret
const secretId = await neo.services.secrets.store({
  name: "exchange-api-key",
  value: "your-super-secret-api-key",
  description: "API key for Binance trading account",
  accessPolicy: {
    functions: ["trade-executor", "market-data-collector"],
    expiresIn: "90d"
  }
});

// Later, in your function:
const myFunction = async () => {
  // The secret is only accessible within the TEE
  const apiKey = await neo.services.secrets.get("exchange-api-key");
  
  // Use the API key securely
  const tradingClient = new ExchangeClient(apiKey);
  const marketData = await tradingClient.fetchMarketData();
  
  // Process data and execute secure operations
  return { status: "success", data: marketData };
};`}
                    </code>
                  </pre>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
} 