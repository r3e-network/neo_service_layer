'use client';

// @ts-ignore
import * as React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import {
  Bars3Icon,
  XMarkIcon,
} from '@heroicons/react/24/outline';
import { WalletConnect } from './WalletConnect';

const navigation = [
  { name: 'Home', href: '/' },
  { name: 'Documentation', href: '/docs' },
  { name: 'Services', href: '/services' },
  { name: 'Playground', href: '/playground' },
  { name: 'Status', href: '/status' },
];

export function Navigation() {
  const [mounted, setMounted] = React.useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = React.useState(false);
  const [scrolled, setScrolled] = React.useState(false);
  const pathname = usePathname();

  React.useEffect(() => {
    setMounted(true);

    const handleScroll = () => {
      const isScrolled = window.scrollY > 10;
      if (isScrolled !== scrolled) {
        setScrolled(isScrolled);
      }
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, [scrolled]);

  if (!mounted) {
    // Return a placeholder or skeleton during server rendering / mounting
    return (
        <header className="fixed w-full top-0 z-50 bg-white/90 h-[72px]">
           <nav className="mx-auto flex max-w-7xl items-center justify-between p-6 lg:px-8 h-full" aria-label="Global">
                {/* Basic skeleton layout */}
                <div className="flex lg:flex-1">
                    <div className="h-8 w-32 bg-gray-200 rounded"></div>
                </div>
                <div className="hidden lg:flex lg:gap-x-12">
                    <div className="h-4 w-20 bg-gray-200 rounded"></div>
                    <div className="h-4 w-24 bg-gray-200 rounded"></div>
                    <div className="h-4 w-16 bg-gray-200 rounded"></div>
                </div>
                <div className="hidden lg:flex lg:flex-1 lg:justify-end lg:gap-x-6 items-center">
                    <div className="h-8 w-8 bg-gray-200 rounded-full"></div>
                    <div className="h-8 w-24 bg-gray-200 rounded"></div>
                </div>
            </nav>
        </header>
    );
  }

  const navClasses = `fixed w-full top-0 z-50 backdrop-blur-lg transition-all duration-300 ${
    scrolled
      ? 'bg-white/90 shadow-md'
      : 'bg-transparent'
  }`;

  return (
    <header className={navClasses}>
      <nav className="mx-auto flex max-w-7xl items-center justify-between p-6 lg:px-8" aria-label="Global">
        <div className="flex lg:flex-1">
          <Link href="/" className="-m-1.5 p-1.5 group">
            <span className="sr-only">Neo Service Layer</span>
            <div className="flex items-center">
              <img
                className="h-8 w-auto transition-transform group-hover:scale-110 duration-300"
                src="/logo.svg"
                alt="Neo Service Layer"
              />
              <span className="ml-3 text-lg font-semibold text-gray-900">
                Neo Service Layer
              </span>
            </div>
          </Link>
        </div>
        
        <div className="flex lg:hidden">
          <button
            type="button"
            className="-m-2.5 inline-flex items-center justify-center rounded-md p-2.5 text-gray-700 hover:bg-gray-100/50 transition-colors duration-200 focus:outline-none"
            onClick={() => setMobileMenuOpen(true)}
          >
            <span className="sr-only">Open main menu</span>
            <Bars3Icon className="h-6 w-6" aria-hidden="true" />
          </button>
        </div>
        
        <div className="hidden lg:flex lg:gap-x-12">
          {navigation.map((item) => {
            const isActive = pathname === item.href || 
                             (item.href !== '/' && pathname?.startsWith(item.href));
            
            return (
              <Link
                key={item.name}
                href={item.href}
                className={`text-sm leading-6 transition-all duration-200 ease-in-out hover:-translate-y-0.5 ${ 
                  isActive 
                    ? 'font-semibold text-blue-600' 
                    : 'text-gray-700 hover:text-blue-600' 
                }`}
              >
                {item.name}
              </Link>
            );
          })}
        </div>
        
        <div className="hidden lg:flex lg:flex-1 lg:justify-end lg:gap-x-6 items-center">
          <WalletConnect />
        </div>
      </nav>

      {/* Mobile menu */}
      <div
        className={`fixed inset-y-0 right-0 z-50 w-full overflow-y-auto bg-white px-6 py-6 sm:max-w-sm transform transition-transform duration-300 ease-in-out shadow-lg ${
          mobileMenuOpen ? 'translate-x-0' : 'translate-x-full'
        }`}
      >
        <div className="flex items-center justify-between">
          <Link href="/" className="-m-1.5 p-1.5 flex items-center" onClick={() => setMobileMenuOpen(false)}>
            <span className="sr-only">Neo Service Layer</span>
            <img
              className="h-8 w-auto"
              src="/logo.svg"
              alt="Neo Service Layer"
            />
            <span className="ml-3 text-lg font-semibold text-gray-900">
              Neo Service Layer
            </span>
          </Link>
          <button
            type="button"
            className="-m-2.5 rounded-md p-2.5 text-gray-700 hover:bg-gray-100/50"
            onClick={() => setMobileMenuOpen(false)}
          >
            <span className="sr-only">Close menu</span>
            <XMarkIcon className="h-6 w-6" aria-hidden="true" />
          </button>
        </div>
        <div className="mt-6 flow-root">
          <div className="-my-6 divide-y divide-gray-500/10">
            <div className="space-y-2 py-6">
              {navigation.map((item) => {
                const isActive = pathname === item.href || 
                                (item.href !== '/' && pathname?.startsWith(item.href));
                
                return (
                  <Link
                    key={item.name}
                    href={item.href}
                    className={`-mx-3 block rounded-lg px-3 py-2.5 text-base font-semibold leading-7 ${ 
                      isActive 
                        ? 'bg-blue-50 text-blue-700' 
                        : 'text-gray-900 hover:bg-gray-50' 
                    } transition-colors duration-200`}
                    onClick={() => setMobileMenuOpen(false)}
                  >
                    {item.name}
                  </Link>
                );
              })}
            </div>
            <div className="py-6">
              <div className="flex items-center gap-x-4">
                <WalletConnect />
              </div>
            </div>
          </div>
        </div>
      </div>
    </header>
  );
}