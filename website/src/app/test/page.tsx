'use client';

// @ts-ignore
import * as React from 'react';

export default function TestPage() {
  const [mounted, setMounted] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    setMounted(true);
    try {
      // Check if basic browser APIs are available
      if (typeof window !== 'undefined') {
        console.log('Window is defined');
      }
      
      // Check if localStorage is available
      if (typeof localStorage !== 'undefined') {
        console.log('LocalStorage is available');
        localStorage.setItem('test', 'test');
        console.log('LocalStorage test successful');
      }
      
      // Check if document is available
      if (typeof document !== 'undefined') {
        console.log('Document is available');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
      console.error('Error in test page:', err);
    }
  }, []);

  if (!mounted) {
    return <div>Loading...</div>;
  }

  return (
    <div className="p-8">
      <h1 className="text-3xl font-bold mb-4">Test Page</h1>
      
      {error ? (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          <p><strong>Error:</strong> {error}</p>
        </div>
      ) : (
        <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4">
          <p>No errors detected in basic browser APIs.</p>
        </div>
      )}
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
        <div className="bg-white p-6 rounded-lg shadow-md">
          <h2 className="text-xl font-semibold mb-2">Tailwind Test</h2>
          <p className="text-gray-600 mb-4">This tests if Tailwind CSS is working properly.</p>
          <div className="flex space-x-2">
            <div className="w-8 h-8 bg-red-500 rounded"></div>
            <div className="w-8 h-8 bg-blue-500 rounded"></div>
            <div className="w-8 h-8 bg-green-500 rounded"></div>
            <div className="w-8 h-8 bg-yellow-500 rounded"></div>
          </div>
        </div>
        
        <div className="bg-white p-6 rounded-lg shadow-md">
          <h2 className="text-xl font-semibold mb-2">Font Test</h2>
          <p className="font-sans">This is sans font</p>
          <p className="font-serif">This is serif font</p>
          <p className="font-mono">This is mono font</p>
          <p className="font-bold">This is bold text</p>
          <p className="italic">This is italic text</p>
        </div>
      </div>
      
      <div className="bg-white p-6 rounded-lg shadow-md mb-4">
        <h2 className="text-xl font-semibold mb-2">Environment Info</h2>
        <pre className="bg-gray-100 p-4 rounded overflow-x-auto">
          {JSON.stringify({
            windowDefined: typeof window !== 'undefined',
            documentDefined: typeof document !== 'undefined',
            localStorageDefined: typeof localStorage !== 'undefined',
            navigatorDefined: typeof navigator !== 'undefined',
            userAgent: typeof navigator !== 'undefined' ? navigator.userAgent : 'undefined',
            viewport: typeof window !== 'undefined' ? {
              width: window.innerWidth,
              height: window.innerHeight
            } : 'undefined',
          }, null, 2)}
        </pre>
      </div>
      
      <div className="flex space-x-4">
        <a href="/" className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">
          Back to Home
        </a>
      </div>
    </div>
  );
}
