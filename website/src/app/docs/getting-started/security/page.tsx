// @ts-ignore
import * as React from 'react';

export default function SecurityPage() {
  return (
    <article className="prose prose-slate max-w-none">
      <h1>Security Model</h1>
      <p className="lead">
        Security is a fundamental principle of Neo Service Layer. Our security model
        is built on multiple layers of protection, with Trusted Execution
        Environments (TEE) at its core.
      </p>

      <h2>Core Security Principles</h2>
      <div className="mt-6 grid gap-6 sm:grid-cols-2">
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Zero Trust Architecture</h3>
          <ul className="mb-0">
            <li>Every request is authenticated and authorized</li>
            <li>No implicit trust between services</li>
            <li>Continuous verification of identity and permissions</li>
            <li>End-to-end encryption for all communications</li>
          </ul>
        </div>
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Defense in Depth</h3>
          <ul className="mb-0">
            <li>Multiple layers of security controls</li>
            <li>Redundant security mechanisms</li>
            <li>Fail-safe default configurations</li>
            <li>Regular security audits and updates</li>
          </ul>
        </div>
      </div>

      <h2 className="mt-12">Trusted Execution Environment</h2>
      <div className="rounded-lg border border-gray-200 p-6">
        <h3 className="mt-0">TEE Guarantees</h3>
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <h4 className="mt-0 text-gray-900">Code Integrity</h4>
            <ul className="mb-0">
              <li>Verified code execution</li>
              <li>Protected runtime environment</li>
              <li>Tamper-proof execution</li>
              <li>Secure boot process</li>
            </ul>
          </div>
          <div>
            <h4 className="mt-0 text-gray-900">Data Protection</h4>
            <ul className="mb-0">
              <li>Memory encryption</li>
              <li>Secure storage</li>
              <li>Data sealing</li>
              <li>Anti-tampering measures</li>
            </ul>
          </div>
        </div>
      </div>

      <h2 className="mt-12">Authentication & Authorization</h2>
      <div className="rounded-lg border border-gray-200 p-6">
        <h3 className="mt-0">Message Signing</h3>
        <p>
          All interactions with Neo Service Layer require signed messages using Neo
          N3 wallets, ensuring:
        </p>
        <ul>
          <li>Non-repudiation of requests</li>
          <li>Identity verification</li>
          <li>Transaction integrity</li>
          <li>Secure communication channel establishment</li>
        </ul>

        <h4 className="text-gray-900">Authorization Flow</h4>
        <ol className="mb-0">
          <li>Client generates a message for the intended operation</li>
          <li>Message is signed using the Neo N3 wallet</li>
          <li>Signature is verified by the service</li>
          <li>Permissions are checked against the wallet address</li>
          <li>Operation is executed if all checks pass</li>
        </ol>
      </div>

      <h2 className="mt-12">Secrets Management</h2>
      <div className="rounded-lg border border-gray-200 p-6">
        <h3 className="mt-0">Secure Storage</h3>
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <h4 className="mt-0 text-gray-900">Encryption</h4>
            <ul className="mb-0">
              <li>AES-256 encryption</li>
              <li>Key rotation policies</li>
              <li>Secure key storage</li>
              <li>Hardware security module integration</li>
            </ul>
          </div>
          <div>
            <h4 className="mt-0 text-gray-900">Access Control</h4>
            <ul className="mb-0">
              <li>Fine-grained permissions</li>
              <li>Role-based access control</li>
              <li>Audit logging</li>
              <li>Access revocation</li>
            </ul>
          </div>
        </div>
      </div>

      <h2 className="mt-12">Network Security</h2>
      <div className="grid gap-6 sm:grid-cols-2">
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Communication Security</h3>
          <ul className="mb-0">
            <li>TLS 1.3 encryption</li>
            <li>Certificate pinning</li>
            <li>Secure RPC channels</li>
            <li>DDoS protection</li>
          </ul>
        </div>
        <div className="rounded-lg border border-gray-200 p-6">
          <h3 className="mt-0">Monitoring & Detection</h3>
          <ul className="mb-0">
            <li>Real-time threat detection</li>
            <li>Anomaly detection</li>
            <li>Intrusion prevention</li>
            <li>Security event logging</li>
          </ul>
        </div>
      </div>

      <h2 className="mt-12">Smart Contract Security</h2>
      <div className="rounded-lg border border-gray-200 p-6">
        <h3 className="mt-0">Contract Interaction Security</h3>
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <h4 className="mt-0 text-gray-900">Validation</h4>
            <ul className="mb-0">
              <li>Input validation</li>
              <li>Output verification</li>
              <li>Gas limit checks</li>
              <li>Reentry protection</li>
            </ul>
          </div>
          <div>
            <h4 className="mt-0 text-gray-900">Monitoring</h4>
            <ul className="mb-0">
              <li>Transaction monitoring</li>
              <li>Event tracking</li>
              <li>Error detection</li>
              <li>Performance analysis</li>
            </ul>
          </div>
        </div>
      </div>

      <div className="mt-12 rounded-xl bg-blue-50 p-6">
        <h3 className="mt-0 text-blue-900">Security Best Practices</h3>
        <p className="text-blue-800">
          When integrating with Neo Service Layer, follow these security best
          practices:
        </p>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <div className="rounded-lg bg-white p-4">
            <h4 className="mt-0 text-blue-900">Key Management</h4>
            <ul className="mb-0 text-blue-700">
              <li>Secure private key storage</li>
              <li>Regular key rotation</li>
              <li>Access control implementation</li>
            </ul>
          </div>
          <div className="rounded-lg bg-white p-4">
            <h4 className="mt-0 text-blue-900">Contract Integration</h4>
            <ul className="mb-0 text-blue-700">
              <li>Input validation</li>
              <li>Error handling</li>
              <li>Gas optimization</li>
            </ul>
          </div>
        </div>
      </div>

      <div className="mt-12">
        <h2>Next Steps</h2>
        <div className="grid gap-4 sm:grid-cols-2">
          <a
            href="/docs/guides/contract-integration"
            className="rounded-lg border border-gray-200 p-6 no-underline hover:border-blue-500"
          >
            <h3 className="mt-0">Contract Integration →</h3>
            <p className="mb-0">
              Learn how to securely integrate with smart contracts
            </p>
          </a>
          <a
            href="/docs/services/secrets"
            className="rounded-lg border border-gray-200 p-6 no-underline hover:border-blue-500"
          >
            <h3 className="mt-0">Secrets Management →</h3>
            <p className="mb-0">
              Understand how to manage sensitive data securely
            </p>
          </a>
        </div>
      </div>
    </article>
  );
} 