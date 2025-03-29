import React from 'react';
import Script from 'next/script';
import { Providers } from './providers';
import { Navigation } from '../components/Navigation';
import { Footer } from '../components/Footer';
import '../styles/globals.css';

export const metadata = {
  title: 'Neo Service Layer - Enterprise Blockchain Infrastructure',
  description: 'Professional blockchain infrastructure for Neo N3 with real-time price feeds, automated contract execution, and comprehensive system monitoring.',
  keywords: 'Neo, blockchain, N3, price feed, smart contract, automation, TEE, monitoring',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <head>
        {/* @ts-ignore */}
        <Script src="https://cdn.jsdelivr.net/npm/@neoline/neo-line@latest/dist/neoline.min.js" strategy="beforeInteractive" />
      </head>
      <body className="font-sans antialiased">
        <Providers>
          <div className="min-h-screen bg-gradient-to-b from-gray-50 to-white dark:from-gray-900 dark:to-gray-800">
            <Navigation />
            <main className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-8 pt-24">
              {children}
            </main>
            <Footer />
          </div>
        </Providers>
      </body>
    </html>
  );
}