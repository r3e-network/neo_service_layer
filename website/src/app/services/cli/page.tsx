// @ts-ignore
import * as React from 'react';
import { CommandLineIcon, CubeIcon, CogIcon, ServerIcon } from '@heroicons/react/24/outline';

export default function CLIToolsPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <div className="inline-flex items-center justify-center p-3 rounded-full bg-sky-100 dark:bg-sky-900 mb-6">
            <CommandLineIcon className="h-8 w-8 text-sky-600 dark:text-sky-400" />
          </div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            CLI Tools
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Powerful command-line tools for developers to build, deploy, and manage smart contracts and services.
          </p>
        </div>
        
        <div className="mt-16 flow-root">
          <div className="mt-10 space-y-8 border-t border-gray-200 dark:border-gray-700 pt-10 sm:mt-16 sm:pt-16">
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Key Features</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <CubeIcon className="h-6 w-6 text-sky-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Contract Deployment</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Rapidly deploy smart contracts to testnets or production networks with a single command.
                    Includes verification, gas optimization, and deployment confirmation.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <ServerIcon className="h-6 w-6 text-sky-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Service Management</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Create, configure, and manage all Neo services from the command line.
                    Perfect for CI/CD pipelines and automated workflows.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <CogIcon className="h-6 w-6 text-sky-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Environment Configuration</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Easily switch between development, staging, and production environments.
                    Manage environment-specific configurations and secrets.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <CommandLineIcon className="h-6 w-6 text-sky-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Interactive Shell</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Access an interactive REPL environment for testing contracts and services.
                    Get immediate feedback and explore the API interactively.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Available Commands</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 mb-6">
                <h3 className="text-xl font-semibold mb-4 text-gray-900 dark:text-white">Core Commands</h3>
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                    <thead className="bg-gray-50 dark:bg-gray-900">
                      <tr>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Command</th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Description</th>
                      </tr>
                    </thead>
                    <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                      <tr>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-900 dark:text-gray-100">neo init</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">Initialize a new Neo project with scaffolding</td>
                      </tr>
                      <tr>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-900 dark:text-gray-100">neo login</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">Authenticate with Neo services</td>
                      </tr>
                      <tr>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-900 dark:text-gray-100">neo deploy</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">Deploy contracts or services</td>
                      </tr>
                      <tr>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-900 dark:text-gray-100">neo test</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">Run tests for your contracts and services</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                <h3 className="text-xl font-semibold mb-4 text-gray-900 dark:text-white">Service Commands</h3>
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                    <thead className="bg-gray-50 dark:bg-gray-900">
                      <tr>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Command</th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Description</th>
                      </tr>
                    </thead>
                    <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                      <tr>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-900 dark:text-gray-100">neo service:create</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">Create a new service configuration</td>
                      </tr>
                      <tr>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-900 dark:text-gray-100">neo function:deploy</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">Deploy a serverless function</td>
                      </tr>
                      <tr>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-900 dark:text-gray-100">neo trigger:create</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">Create a new trigger for automation</td>
                      </tr>
                      <tr>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-900 dark:text-gray-100">neo secret:set</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">Store encrypted secrets for your services</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Getting Started</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                <h3 className="text-xl font-semibold mb-4 text-gray-900 dark:text-white">Installation</h3>
                <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg mb-6">
                  <pre className="text-sm overflow-x-auto">
                    <code className="language-bash">
{`# Install Neo CLI globally
npm install -g @neo-project/cli

# Verify installation
neo --version`}
                    </code>
                  </pre>
                </div>
                
                <h3 className="text-xl font-semibold mb-4 text-gray-900 dark:text-white">Example Workflow</h3>
                <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
                  <pre className="text-sm overflow-x-auto">
                    <code className="language-bash">
{`# Initialize a new project
neo init my-dapp

# Change to project directory
cd my-dapp

# Login to Neo services
neo login

# Deploy a function
neo function:deploy my-first-function

# Create a trigger for the function
neo trigger:create --name "daily-execution" --schedule "0 0 * * *" --target my-first-function

# Monitor your deployments
neo logs --follow`}
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