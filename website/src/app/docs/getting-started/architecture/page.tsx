import React from 'react';
import Image from 'next/image';

export default function ArchitecturePage() {
  return (
    <article className="prose prose-slate max-w-none">
      <h1>Architecture Overview</h1>
      <p className="lead">
        Neo Service Layer is designed as a secure, scalable, and decentralized
        architecture that bridges the gap between Neo N3 blockchain and external
        services while maintaining the highest security standards.
      </p>

      <h2>Core Components</h2>
      <div className="mt-6 grid gap-6">
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Trusted Execution Environment (TEE)</h3>
          <p>
            All services run within secure enclaves that provide:
          </p>
          <ul className="mb-0">
            <li>Code integrity verification</li>
            <li>Data encryption at rest and in transit</li>
            <li>Isolated execution environment</li>
            <li>Remote attestation capabilities</li>
          </ul>
        </div>

        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Service Modules</h3>
          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <h4 className="mt-0 text-gray-900">Price Feed Service</h4>
              <ul className="mb-0">
                <li>Real-time price aggregation</li>
                <li>Multi-source validation</li>
                <li>On-chain price updates</li>
                <li>Heartbeat monitoring</li>
              </ul>
            </div>
            <div>
              <h4 className="mt-0 text-gray-900">Contract Automation</h4>
              <ul className="mb-0">
                <li>Event monitoring</li>
                <li>Scheduled executions</li>
                <li>Conditional triggers</li>
                <li>Gas optimization</li>
              </ul>
            </div>
            <div>
              <h4 className="mt-0 text-gray-900">Gas Bank</h4>
              <ul className="mb-0">
                <li>Automated gas management</li>
                <li>Usage tracking</li>
                <li>Cost optimization</li>
                <li>Balance alerts</li>
              </ul>
            </div>
            <div>
              <h4 className="mt-0 text-gray-900">Functions Service</h4>
              <ul className="mb-0">
                <li>Secure function execution</li>
                <li>Version management</li>
                <li>Resource isolation</li>
                <li>Execution logs</li>
              </ul>
            </div>
          </div>
        </div>

        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Security Infrastructure</h3>
          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <h4 className="mt-0 text-gray-900">Authentication</h4>
              <ul className="mb-0">
                <li>Message signing</li>
                <li>Signature verification</li>
                <li>Access control</li>
                <li>Rate limiting</li>
              </ul>
            </div>
            <div>
              <h4 className="mt-0 text-gray-900">Secrets Management</h4>
              <ul className="mb-0">
                <li>Encrypted storage</li>
                <li>Access policies</li>
                <li>Key rotation</li>
                <li>Audit logging</li>
              </ul>
            </div>
          </div>
        </div>
      </div>

      <h2 className="mt-12">Data Flow</h2>
      <div className="rounded-lg border border-gray-200 p-6">
        <h3 className="mt-0">Request Processing</h3>
        <ol className="mb-0">
          <li>
            <strong>Authentication:</strong> All requests are authenticated using
            Neo N3 wallet signatures
          </li>
          <li>
            <strong>TEE Validation:</strong> Request is validated within the TEE
          </li>
          <li>
            <strong>Service Processing:</strong> Appropriate service module handles
            the request
          </li>
          <li>
            <strong>Blockchain Interaction:</strong> Results are written to Neo N3
            when required
          </li>
          <li>
            <strong>Response:</strong> Signed response returned to the client
          </li>
        </ol>
      </div>

      <h2 className="mt-12">Monitoring and Reliability</h2>
      <div className="grid gap-6 sm:grid-cols-2">
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Service Monitoring</h3>
          <ul className="mb-0">
            <li>Real-time health checks</li>
            <li>Performance metrics</li>
            <li>Error tracking</li>
            <li>Resource utilization</li>
          </ul>
        </div>
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Reliability Features</h3>
          <ul className="mb-0">
            <li>Automatic failover</li>
            <li>Load balancing</li>
            <li>Data replication</li>
            <li>Disaster recovery</li>
          </ul>
        </div>
      </div>

      <div className="mt-12 rounded-xl bg-blue-50 p-6">
        <h3 className="mt-0 text-blue-900">Integration Points</h3>
        <p className="text-blue-800">
          Neo Service Layer provides multiple integration points for your
          applications:
        </p>
        <div className="mt-4 grid gap-4 sm:grid-cols-3">
          <div className="rounded-lg bg-white p-4">
            <h4 className="mt-0 text-blue-900">SDK Integration</h4>
            <p className="mb-0 text-blue-700">
              Direct integration via our TypeScript/JavaScript SDK
            </p>
          </div>
          <div className="rounded-lg bg-white p-4">
            <h4 className="mt-0 text-blue-900">Smart Contracts</h4>
            <p className="mb-0 text-blue-700">
              Native Neo N3 smart contract integration
            </p>
          </div>
          <div className="rounded-lg bg-white p-4">
            <h4 className="mt-0 text-blue-900">REST API</h4>
            <p className="mb-0 text-blue-700">
              HTTP API for platform-agnostic integration
            </p>
          </div>
        </div>
      </div>

      <div className="mt-12">
        <h2>Next Steps</h2>
        <div className="grid gap-4 sm:grid-cols-2">
          <a
            href="/docs/getting-started/security"
            className="rounded-lg border border-gray-200 p-6 no-underline hover:border-blue-500"
          >
            <h3 className="mt-0">Security Model →</h3>
            <p className="mb-0">
              Learn more about our security architecture and guarantees
            </p>
          </a>
          <a
            href="/docs/api/sdk"
            className="rounded-lg border border-gray-200 p-6 no-underline hover:border-blue-500"
          >
            <h3 className="mt-0">SDK Reference →</h3>
            <p className="mb-0">
              Detailed documentation of our SDK and its capabilities
            </p>
          </a>
        </div>
      </div>
    </article>
  );
} 