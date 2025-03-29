'use client';

// @ts-ignore
import * as React from 'react';
import { usePathname } from 'next/navigation';
import Link from 'next/link';
import clsx from 'clsx';

interface NavItem {
  title: string;
  href: string;
  children?: NavItem[];
}

const navigation: NavItem[] = [
  {
    title: 'Getting Started',
    href: '/docs/getting-started',
    children: [
      { title: 'Introduction', href: '/docs/getting-started' },
      { title: 'Quick Start', href: '/docs/getting-started/quick-start' },
      { title: 'Installation', href: '/docs/getting-started/installation' },
    ],
  },
  {
    title: 'Core Concepts',
    href: '/docs/core-concepts',
    children: [
      { title: 'Architecture', href: '/docs/core-concepts/architecture' },
      { title: 'Security Model', href: '/docs/core-concepts/security' },
      { title: 'Network Design', href: '/docs/core-concepts/network' },
    ],
  },
  {
    title: 'Services',
    href: '/docs/services',
    children: [
      { title: 'Price Feeds', href: '/docs/services/price-feeds' },
      { title: 'Contract Automation', href: '/docs/services/automation' },
      { title: 'Gas Bank', href: '/docs/services/gas-bank' },
      { title: 'Functions', href: '/docs/services/functions' },
      { title: 'Secrets', href: '/docs/services/secrets' },
      { title: 'API', href: '/docs/services/api' },
    ],
  },
  {
    title: 'API Reference',
    href: '/docs/api',
    children: [
      { title: 'REST API', href: '/docs/api/rest' },
      { title: 'SDK Reference', href: '/docs/api/sdk' },
      { title: 'WebSocket API', href: '/docs/api/websocket' },
    ],
  },
  {
    title: 'Examples',
    href: '/docs/examples',
    children: [
      { title: 'Price Oracle', href: '/docs/examples/price-oracle' },
      { title: 'Automated Trading', href: '/docs/examples/automated-trading' },
      { title: 'NFT Integration', href: '/docs/examples/nft' },
    ],
  },
];

function NavLink({
  href,
  children,
  isActive,
}: {
  href: string;
  children: React.ReactNode;
  isActive?: boolean;
}) {
  return (
    <Link
      href={href}
      className={clsx(
        'block w-full pl-3.5 before:pointer-events-none before:absolute before:-left-1 before:top-1/2 before:h-1.5 before:w-1.5 before:-translate-y-1/2 before:rounded-full',
        isActive
          ? 'font-semibold text-indigo-600 before:bg-indigo-600'
          : 'text-slate-500 before:hidden before:bg-slate-300 hover:text-slate-600 hover:before:block'
      )}
    >
      {children}
    </Link>
  );
}

function NavGroup({ group, className }: { group: NavItem; className?: string }) {
  const pathname = usePathname();
  const isActiveGroup = pathname?.startsWith(group.href) ?? false;

  return (
    <li className={clsx('relative mt-6', className)}>
      <h2 className="text-xs font-semibold text-slate-900">
        {group.title}
      </h2>
      {group.children && (
        <ul className="mt-2 space-y-2 border-l-2 border-slate-100">
          {group.children.map((item) => (
            <li key={item.href} className="relative">
              <NavLink href={item.href} isActive={pathname === item.href}>
                {item.title}
              </NavLink>
            </li>
          ))}
        </ul>
      )}
    </li>
  );
}

export function DocSidebar() {
  return (
    <nav className="text-base lg:text-sm">
      <ul className="space-y-9">
        {navigation.map((group) => (
          <NavGroup key={group.href} group={group} />
        ))}
      </ul>
    </nav>
  );
}