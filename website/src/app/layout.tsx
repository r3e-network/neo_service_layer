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
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <link rel="icon" href="/logo.svg" type="image/svg+xml" />
        <link rel="apple-touch-icon" href="/logo.svg" />
        <meta name="theme-color" content="#60A5FA" />
        {/* @ts-ignore */}
        <Script src="https://cdn.jsdelivr.net/npm/@neoline/neo-line@latest/dist/neoline.min.js" strategy="beforeInteractive" />
      </head>
      <body className="bg-gray-50 text-gray-900">
        <Providers>
          <div className="flex flex-col min-h-screen">
            <Navigation />
            <main className="flex-grow w-full z-10">
              {children}
            </main>
            <Footer />
          </div>
        </Providers>
      </body>
    </html>
  );
}