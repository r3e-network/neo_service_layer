'use client';

import React from 'react';
import Link from 'next/link';
import '../styles/minimal.css';
import { Hero } from '../components/Hero';
import { Features } from '../components/Features';
import { Services } from '../components/Services';
import { CTA } from '../components/CTA';

export default function Home() {
  return (
    <main className="flex-grow">
      <div className="bg-gradient -to-b from-white via-gray-50 to-white">
        <section className="relative z-10">
          <Hero />
        </section>
        
        <section className="relative z-20 py-16 sm:py-24">
          <Features />
        </section>
        
        <section className="relative z-20 py-16 sm:py-24 bg-white/50">
          <Services />
        </section>
        
        <section className="relative z-20 py-16 sm:py-24">
          <CTA />
        </section>
      </div>
    </main>
  );
}