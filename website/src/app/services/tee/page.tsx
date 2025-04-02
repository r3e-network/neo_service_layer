// @ts-ignore
import * as React from 'react';
import { ShieldCheckIcon, LockClosedIcon, ServerIcon, DocumentTextIcon } from '@heroicons/react/24/outline';

export default function TEEPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <div className="inline-flex items-center justify-center p-3 rounded-full bg-red-100 dark:bg-red-900 mb-6">
            <ShieldCheckIcon className="h-8 w-8 text-red-600 dark:text-red-400" />
          </div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-4xl">
            Trusted Execution Environment
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Run your code in a secure, isolated environment with hardware-level protection against tampering and unauthorized access.
          </p>
        </div>
        
        <div className="mt-16 flow-root">
          <div className="mt-10 space-y-8 border-t border-gray-200 dark:border-gray-700 pt-10 sm:mt-16 sm:pt-16">
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Key Features</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <LockClosedIcon className="h-6 w-6 text-red-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Hardware Isolation</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Our TEE solution leverages secure hardware enclaves to create a physically isolated execution environment.
                    This provides the strongest possible protection against both software and hardware-based attacks.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <ServerIcon className="h-6 w-6 text-red-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Secure Enclaves</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Operations within the enclave are protected from the host system, including from system administrators 
                    or cloud providers. Even the underlying operating system cannot access the protected memory space.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <ShieldCheckIcon className="h-6 w-6 text-red-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Data Protection</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    All data handled within the TEE is encrypted in memory and remains inaccessible to external processes.
                    This includes sensitive business logic, API keys, private keys, and user data.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center mb-4">
                    <DocumentTextIcon className="h-6 w-6 text-red-500 mr-2" />
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Verified Execution</h3>
                  </div>
                  <p className="text-gray-600 dark:text-gray-400">
                    Remote attestation allows users to verify that their code is running in a genuine TEE with the 
                    expected configuration, providing cryptographic proof of the environment's integrity.
                  </p>
                </div>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">How It Works</h2>
              
              <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                <ol className="list-decimal list-inside space-y-6 text-gray-600 dark:text-gray-400">
                  <li className="flex flex-col">
                    <strong className="text-gray-900 dark:text-white text-lg mb-2">Code Deployment</strong>
                    <p>
                      Your code is encrypted before transmission and only decrypted within the secure enclave. 
                      This ensures that no unauthorized parties can access your business logic during deployment.
                    </p>
                  </li>
                  
                  <li className="flex flex-col">
                    <strong className="text-gray-900 dark:text-white text-lg mb-2">Secure Initialization</strong>
                    <p>
                      The TEE environment establishes its identity through remote attestation, allowing you to 
                      verify it's a genuine enclave before sending any sensitive data or cryptographic keys.
                    </p>
                  </li>
                  
                  <li className="flex flex-col">
                    <strong className="text-gray-900 dark:text-white text-lg mb-2">Protected Execution</strong>
                    <p>
                      Your code executes in the isolated environment, with all memory and processing protected from 
                      external observation. Data remains encrypted in memory and is only accessible within the TEE.
                    </p>
                  </li>
                  
                  <li className="flex flex-col">
                    <strong className="text-gray-900 dark:text-white text-lg mb-2">Secure Communication</strong>
                    <p>
                      Communication between your application and the TEE is encrypted end-to-end, with the TEE's 
                      attestation providing cryptographic proof of the environment's integrity.
                    </p>
                  </li>
                </ol>
              </div>
            </div>
            
            <div className="mx-auto max-w-4xl">
              <h2 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Use Cases</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Confidential Computing</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    Process sensitive data while keeping it encrypted and protected, even during computation.
                    Perfect for handling PII, financial data, or intellectual property.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Secure Oracles</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    Create trustworthy data feeds for smart contracts with verifiable integrity guarantees.
                    Ensures data hasn't been tampered with before being provided to blockchain applications.
                  </p>
                </div>
                
                <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
                  <h3 className="text-xl font-semibold mb-3 text-gray-900 dark:text-white">Key Management</h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    Store and use cryptographic keys within the secure environment, protecting them from 
                    extraction or unauthorized use, even in the event of server compromise.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
} 