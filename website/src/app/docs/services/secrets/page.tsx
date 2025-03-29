'use client';

// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../../../components/CodeBlock';

const installCode = `npm install @neo-service-layer/core
# or
yarn add @neo-service-layer/core`;

const initCode = `import { Client, Secrets } from '@neo-service-layer/core';

// Initialize client with your Neo N3 wallet
const client = new Client({
  signMessage: async (message) => {
    return await window.neo3Wallet.signMessage(message);
  },
});

// Create Secrets instance
const secrets = new Secrets(client);`;

const secretsCode = `// Store a new secret
await secrets.setSecret('API_KEY', 'your-api-key-here');

// Get a secret value
const apiKey = await secrets.getSecret('API_KEY');

// List all secrets
const allSecrets = await secrets.listSecrets();

// Delete a secret
await secrets.deleteSecret('API_KEY');

// Update a secret
await secrets.updateSecret('API_KEY', 'new-api-key-value');

// Store multiple secrets
await secrets.setSecrets({
  'DATABASE_URL': 'postgresql://localhost:5432/mydb',
  'REDIS_URL': 'redis://localhost:6379',
  'JWT_SECRET': 'your-jwt-secret'
});`;

const functionIntegrationCode = `// Example function using secrets
const functionId = await functions.deploy({
  name: 'api-integration',
  source: \`
    import { Secrets } from '@neo-service-layer/core';
    
    export async function callExternalApi(params) {
      const secrets = new Secrets(client);
      const apiKey = await secrets.getSecret('API_KEY');
      
      const response = await fetch('https://api.example.com/data', {
        headers: {
          'Authorization': \`Bearer \${apiKey}\`
        }
      });
      
      return await response.json();
    }
  \`,
  secrets: ['API_KEY'], // Declare required secrets
});`;

const rotationCode = `// Rotate a secret
await secrets.rotateSecret('API_KEY', {
  newValue: 'new-api-key',
  gracePeriod: 3600, // 1 hour in seconds
});

// Check rotation status
const status = await secrets.getRotationStatus('API_KEY');

// Force complete rotation
await secrets.completeRotation('API_KEY');`;

export default function SecretsPage() {
  return (
    <div className="py-24 sm:py-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mx-auto max-w-2xl lg:max-w-4xl">
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 dark:text-white">
            Secrets Service
          </h1>
          
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            The Secrets service provides secure storage and management of sensitive configuration values 
            like API keys, credentials, and other secrets. All secrets are stored and accessed within 
            Trusted Execution Environments (TEE) for maximum security.
          </p>

          <div className="mt-16 space-y-20">
            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Installation
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Install the Neo Service Layer SDK using npm or yarn:
              </p>
              <div className="mt-4">
                <CodeBlock code={installCode} language="bash" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Initialization
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Initialize the client and create a Secrets instance:
              </p>
              <div className="mt-4">
                <CodeBlock code={initCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Managing Secrets
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Store, retrieve, update, and delete secrets:
              </p>
              <div className="mt-4">
                <CodeBlock code={secretsCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Functions Integration
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Use secrets in serverless functions:
              </p>
              <div className="mt-4">
                <CodeBlock code={functionIntegrationCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Secret Rotation
              </h2>
              <p className="mt-4 text-gray-600 dark:text-gray-400">
                Safely rotate secrets with zero downtime:
              </p>
              <div className="mt-4">
                <CodeBlock code={rotationCode} language="typescript" />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Security Features
              </h2>
              <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    TEE Protection
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    All secrets are stored and accessed within isolated Trusted Execution 
                    Environments, ensuring confidentiality and integrity.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Encryption
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Secrets are encrypted at rest and in transit using industry-standard 
                    encryption algorithms and key management.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Access Control
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Fine-grained access control ensures secrets are only accessible to 
                    authorized services and functions.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Audit Logging
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Comprehensive audit logs track all secret access and modifications 
                    for security monitoring.
                  </p>
                </div>
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                Best Practices
              </h2>
              <div className="mt-6 space-y-6">
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Secret Naming
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Use descriptive, consistent naming conventions for secrets. 
                    Prefix secrets by service or environment for better organization.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Regular Rotation
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Implement regular secret rotation schedules. Use the rotation API 
                    to ensure zero-downtime updates.
                  </p>
                </div>
                <div className="rounded-lg bg-white dark:bg-gray-800 p-6 shadow-sm ring-1 ring-gray-900/5">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                    Access Patterns
                  </h3>
                  <p className="mt-4 text-gray-600 dark:text-gray-400">
                    Cache secret values when appropriate to reduce API calls. 
                    Implement proper error handling for secret access failures.
                  </p>
                </div>
              </div>
            </section>
          </div>
        </div>
      </div>
    </div>
  );
} 