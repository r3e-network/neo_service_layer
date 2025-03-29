'use client';

// @ts-ignore
import * as React from 'react';
import { useWallet } from '../app/hooks/useWallet';

interface FunctionTemplate {
  name: string;
  description: string;
  code: string;
}

const templates: FunctionTemplate[] = [
  {
    name: 'Price Feed Consumer',
    description: 'Fetch and process price data from multiple sources',
    code: `import { PriceFeed } from '@neo-service-layer/core';

async function getPrices(symbols: string[]) {
  const feed = new PriceFeed();
  const prices = await Promise.all(
    symbols.map(symbol => feed.getPrice(symbol))
  );
  return prices;
}`,
  },
  {
    name: 'Contract Automation',
    description: 'Set up automated contract calls based on conditions',
    code: `import { Automation } from '@neo-service-layer/core';

async function setupAutomation(contract: string) {
  const automation = new Automation();
  await automation.createTask({
    contract,
    method: 'update',
    schedule: '*/30 * * * *'
  });
}`,
  },
];

export function Playground() {
  const { isConnected, connect } = useWallet();
  const [selectedTemplate, setSelectedTemplate] = React.useState<FunctionTemplate | null>(null);
  const [code, setCode] = React.useState('');
  const [output, setOutput] = React.useState('');
  const [isRunning, setIsRunning] = React.useState(false);

  const handleTemplateSelect = (template: FunctionTemplate) => {
    setSelectedTemplate(template);
    setCode(template.code);
  };

  const handleRun = async () => {
    if (!isConnected) {
      await connect();
      return;
    }

    setIsRunning(true);
    setOutput('Running function...');

    try {
      // In a real implementation, this would send the code to your
      // serverless function for execution in a secure environment
      await new Promise(resolve => setTimeout(resolve, 2000));
      setOutput('Function executed successfully!');
    } catch (error) {
      setOutput(`Error: ${error.message}`);
    } finally {
      setIsRunning(false);
    }
  };

  return (
    <div className="grid grid-cols-1 gap-8 lg:grid-cols-2">
      <div>
        <h2 className="text-lg font-semibold text-gray-900">Templates</h2>
        <div className="mt-4 grid grid-cols-1 gap-4">
          {templates.map((template) => (
            <button
              key={template.name}
              onClick={() => handleTemplateSelect(template)}
              className={`p-4 rounded-lg border ${
                selectedTemplate?.name === template.name
                  ? 'border-indigo-500 bg-indigo-50'
                  : 'border-gray-200 hover:border-indigo-500 hover:bg-gray-50'
              }`}
            >
              <h3 className="text-sm font-medium text-gray-900">{template.name}</h3>
              <p className="mt-1 text-sm text-gray-500">{template.description}</p>
            </button>
          ))}
        </div>
      </div>

      <div>
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold text-gray-900">Code Editor</h2>
          <button
            onClick={handleRun}
            disabled={!code || isRunning}
            className="inline-flex items-center rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 disabled:opacity-50"
          >
            {isRunning ? 'Running...' : isConnected ? 'Run' : 'Connect Wallet to Run'}
          </button>
        </div>
        <div className="mt-4">
          <textarea
            value={code}
            onChange={(e) => setCode(e.target.value)}
            className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 font-mono text-sm leading-6"
            rows={10}
            placeholder="Write or select a template..."
          />
        </div>
        {output && (
          <div className="mt-4">
            <h3 className="text-sm font-medium text-gray-900">Output</h3>
            <pre className="mt-2 rounded-md bg-gray-900 p-4 text-sm text-white overflow-auto">
              {output}
            </pre>
          </div>
        )}
      </div>
    </div>
  );
}