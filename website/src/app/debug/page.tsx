'use client';

import React from 'react';

export default function DebugPage() {
  const [errors, setErrors] = React.useState<string[]>([]);
  const [cssLoaded, setCssLoaded] = React.useState(false);
  const [windowDimensions, setWindowDimensions] = React.useState({ width: 0, height: 0 });
  const [documentReady, setDocumentReady] = React.useState(false);

  React.useEffect(() => {
    // Capture console errors
    const originalError = console.error;
    console.error = (...args) => {
      setErrors(prev => [...prev, args.join(' ')]);
      originalError(...args);
    };

    // Check if CSS is loaded
    const allStyleSheets = document.styleSheets;
    setCssLoaded(allStyleSheets.length > 0);

    // Get window dimensions
    setWindowDimensions({
      width: window.innerWidth,
      height: window.innerHeight
    });

    // Check if document is ready
    setDocumentReady(document.readyState === 'complete');

    // Cleanup
    return () => {
      console.error = originalError;
    };
  }, []);

  return (
    <div style={{ padding: '20px', fontFamily: 'monospace' }}>
      <h1 style={{ fontSize: '24px', marginBottom: '20px' }}>Debug Information</h1>
      
      <div style={{ marginBottom: '20px' }}>
        <h2 style={{ fontSize: '18px', marginBottom: '10px' }}>Environment</h2>
        <ul style={{ listStyleType: 'none', padding: 0 }}>
          <li>Window Dimensions: {windowDimensions.width} x {windowDimensions.height}</li>
          <li>Document Ready: {documentReady ? 'Yes' : 'No'}</li>
          <li>CSS Loaded: {cssLoaded ? 'Yes' : 'No'}</li>
          <li>StyleSheets Count: {typeof document !== 'undefined' ? document.styleSheets.length : 0}</li>
        </ul>
      </div>
      
      <div style={{ marginBottom: '20px' }}>
        <h2 style={{ fontSize: '18px', marginBottom: '10px' }}>Console Errors</h2>
        {errors.length > 0 ? (
          <ul style={{ 
            listStyleType: 'none', 
            padding: '10px', 
            backgroundColor: '#ffebee', 
            border: '1px solid #ffcdd2', 
            borderRadius: '4px' 
          }}>
            {errors.map((error, index) => (
              <li key={index} style={{ marginBottom: '5px' }}>{error}</li>
            ))}
          </ul>
        ) : (
          <p>No errors captured</p>
        )}
      </div>
      
      <div style={{ marginBottom: '20px' }}>
        <h2 style={{ fontSize: '18px', marginBottom: '10px' }}>CSS Test</h2>
        <div style={{ display: 'flex', gap: '10px' }}>
          <div style={{ width: '50px', height: '50px', backgroundColor: 'red' }}></div>
          <div style={{ width: '50px', height: '50px', backgroundColor: 'green' }}></div>
          <div style={{ width: '50px', height: '50px', backgroundColor: 'blue' }}></div>
        </div>
      </div>
      
      <div>
        <h2 style={{ fontSize: '18px', marginBottom: '10px' }}>Actions</h2>
        <a 
          href="/" 
          style={{ 
            display: 'inline-block', 
            padding: '10px 15px', 
            backgroundColor: '#2196f3', 
            color: 'white', 
            textDecoration: 'none', 
            borderRadius: '4px' 
          }}
        >
          Back to Home
        </a>
      </div>
    </div>
  );
}
