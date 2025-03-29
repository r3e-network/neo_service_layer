# Contract Automation Examples

## Overview
These examples demonstrate how to use the contract automation service, allowing automatic execution of contract functions based on predefined conditions.

## Basic Contract Automation

### Setting Up Contract Monitoring
```typescript
import { useState } from 'react';
import { setupContractMonitor } from '../api/automation';
import { useWallet } from '../src/app/hooks/useWallet';

function ContractMonitorSetup() {
  const { neoAddress, signMessage } = useWallet();
  const [contractHash, setContractHash] = useState('');
  const [interval, setInterval] = useState('300'); // 5 minutes in seconds
  const [status, setStatus] = useState<'idle' | 'pending' | 'success' | 'error'>('idle');

  const handleSetup = async () => {
    try {
      setStatus('pending');

      // Sign the setup request
      const message = `Setup monitor: ${contractHash}`;
      const signature = await signMessage(message);

      await setupContractMonitor({
        contractHash,
        ownerAddress: neoAddress,
        checkInterval: parseInt(interval),
        signature,
        message
      });

      setStatus('success');
    } catch (error) {
      console.error('Setup failed:', error);
      setStatus('error');
    }
  };

  return (
    <div>
      <input
        value={contractHash}
        onChange={(e) => setContractHash(e.target.value)}
        placeholder="Contract Hash"
      />
      <input
        type="number"
        value={interval}
        onChange={(e) => setInterval(e.target.value)}
        placeholder="Check Interval (seconds)"
      />
      <button
        onClick={handleSetup}
        disabled={!contractHash || status === 'pending'}
      >
        Setup Monitoring
      </button>
      {status === 'success' && (
        <div className="success">Monitoring setup successful!</div>
      )}
    </div>
  );
}
```

### Defining Trigger Conditions
```typescript
import { useState } from 'react';
import { setTriggerCondition } from '../api/automation';

interface TriggerCondition {
  type: 'time' | 'event' | 'threshold';
  params: Record<string, any>;
}

function TriggerSetup() {
  const [condition, setCondition] = useState<TriggerCondition>({
    type: 'time',
    params: { interval: 300 }
  });

  const handleSubmit = async () => {
    try {
      await setTriggerCondition({
        contractHash: '0x1234567890abcdef1234567890abcdef12345678',
        condition,
        functionName: 'checkUpkeep'
      });
    } catch (error) {
      console.error('Failed to set trigger:', error);
    }
  };

  return (
    <div>
      <select
        value={condition.type}
        onChange={(e) => setCondition({
          type: e.target.value as TriggerCondition['type'],
          params: {}
        })}
      >
        <option value="time">Time-based</option>
        <option value="event">Event-based</option>
        <option value="threshold">Threshold-based</option>
      </select>

      {condition.type === 'time' && (
        <input
          type="number"
          value={condition.params.interval || ''}
          onChange={(e) => setCondition({
            ...condition,
            params: { interval: parseInt(e.target.value) }
          })}
          placeholder="Interval (seconds)"
        />
      )}

      {condition.type === 'event' && (
        <input
          value={condition.params.eventName || ''}
          onChange={(e) => setCondition({
            ...condition,
            params: { eventName: e.target.value }
          })}
          placeholder="Event Name"
        />
      )}

      {condition.type === 'threshold' && (
        <>
          <input
            value={condition.params.variable || ''}
            onChange={(e) => setCondition({
              ...condition,
              params: { 
                ...condition.params,
                variable: e.target.value
              }
            })}
            placeholder="Variable Name"
          />
          <input
            type="number"
            value={condition.params.threshold || ''}
            onChange={(e) => setCondition({
              ...condition,
              params: {
                ...condition.params,
                threshold: parseFloat(e.target.value)
              }
            })}
            placeholder="Threshold Value"
          />
        </>
      )}

      <button onClick={handleSubmit}>Set Trigger</button>
    </div>
  );
}
```

## Advanced Usage

### Custom Upkeep Logic
```typescript
import { useState, useEffect } from 'react';
import { registerUpkeepLogic } from '../api/automation';

function CustomUpkeepRegistration() {
  const [logic, setLogic] = useState(`
// Custom upkeep logic
function checkCondition(params) {
  const { balance, threshold } = params;
  return balance < threshold;
}

// Perform upkeep if condition is met
function performUpkeep(params) {
  const { contractHash, method } = params;
  return {
    contractHash,
    method,
    args: []
  };
}
  `);

  const handleRegister = async () => {
    try {
      await registerUpkeepLogic({
        contractHash: '0x1234567890abcdef1234567890abcdef12345678',
        functionName: 'customUpkeep',
        logic,
        params: {
          threshold: 100,
          method: 'refill'
        }
      });
    } catch (error) {
      console.error('Failed to register upkeep logic:', error);
    }
  };

  return (
    <div>
      <textarea
        value={logic}
        onChange={(e) => setLogic(e.target.value)}
        rows={10}
        style={{ width: '100%' }}
      />
      <button onClick={handleRegister}>
        Register Upkeep Logic
      </button>
    </div>
  );
}
```

### Monitoring Automation Status
```typescript
import { useEffect, useState } from 'react';
import { getAutomationStatus } from '../api/automation';

interface AutomationStatus {
  contractHash: string;
  lastCheck: string;
  nextCheck: string;
  status: 'active' | 'paused' | 'error';
  lastError?: string;
}

function AutomationMonitor() {
  const [statuses, setStatuses] = useState<AutomationStatus[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStatus = async () => {
      try {
        const result = await getAutomationStatus();
        setStatuses(result);
        setLoading(false);
      } catch (error) {
        console.error('Failed to fetch status:', error);
        setLoading(false);
      }
    };

    fetchStatus();
    const interval = setInterval(fetchStatus, 60000); // Update every minute
    return () => clearInterval(interval);
  }, []);

  if (loading) return <div>Loading automation status...</div>;

  return (
    <div>
      <h2>Automation Status</h2>
      <table>
        <thead>
          <tr>
            <th>Contract</th>
            <th>Status</th>
            <th>Last Check</th>
            <th>Next Check</th>
            <th>Error</th>
          </tr>
        </thead>
        <tbody>
          {statuses.map((status) => (
            <tr key={status.contractHash}>
              <td>{status.contractHash}</td>
              <td>
                <span className={`status-${status.status}`}>
                  {status.status}
                </span>
              </td>
              <td>{new Date(status.lastCheck).toLocaleString()}</td>
              <td>{new Date(status.nextCheck).toLocaleString()}</td>
              <td>{status.lastError || '-'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

### Event-Based Automation
```typescript
import { useState } from 'react';
import { setupEventTrigger } from '../api/automation';

function EventTriggerSetup() {
  const [config, setConfig] = useState({
    contractHash: '',
    eventName: '',
    filterCondition: '',
    actionMethod: '',
    actionArgs: ''
  });

  const handleSubmit = async () => {
    try {
      await setupEventTrigger({
        contractHash: config.contractHash,
        event: {
          name: config.eventName,
          filter: config.filterCondition
            ? JSON.parse(config.filterCondition)
            : undefined
        },
        action: {
          method: config.actionMethod,
          args: config.actionArgs
            ? JSON.parse(config.actionArgs)
            : []
        }
      });
    } catch (error) {
      console.error('Failed to setup event trigger:', error);
    }
  };

  return (
    <div>
      <input
        value={config.contractHash}
        onChange={(e) => setConfig({
          ...config,
          contractHash: e.target.value
        })}
        placeholder="Contract Hash"
      />
      <input
        value={config.eventName}
        onChange={(e) => setConfig({
          ...config,
          eventName: e.target.value
        })}
        placeholder="Event Name"
      />
      <textarea
        value={config.filterCondition}
        onChange={(e) => setConfig({
          ...config,
          filterCondition: e.target.value
        })}
        placeholder="Filter Condition (JSON)"
      />
      <input
        value={config.actionMethod}
        onChange={(e) => setConfig({
          ...config,
          actionMethod: e.target.value
        })}
        placeholder="Action Method"
      />
      <textarea
        value={config.actionArgs}
        onChange={(e) => setConfig({
          ...config,
          actionArgs: e.target.value
        })}
        placeholder="Action Arguments (JSON)"
      />
      <button onClick={handleSubmit}>
        Setup Event Trigger
      </button>
    </div>
  );
}
```

## Testing Examples

### Unit Testing Automation Components
```typescript
import { render, screen, fireEvent } from '@testing-library/react';
import { setupContractMonitor } from '../api/automation';
import ContractMonitorSetup from './ContractMonitorSetup';

jest.mock('../api/automation');
jest.mock('../src/app/hooks/useWallet', () => ({
  useWallet: () => ({
    neoAddress: 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd',
    signMessage: async (msg: string) => 'signed_' + msg
  })
}));

describe('ContractMonitorSetup', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('sets up contract monitoring', async () => {
    (setupContractMonitor as jest.Mock).mockResolvedValue({ success: true });

    render(<ContractMonitorSetup />);

    // Fill in form
    fireEvent.change(
      screen.getByPlaceholderText('Contract Hash'),
      { target: { value: '0x1234567890abcdef1234567890abcdef12345678' } }
    );

    fireEvent.change(
      screen.getByPlaceholderText('Check Interval (seconds)'),
      { target: { value: '300' } }
    );

    // Submit form
    fireEvent.click(screen.getByText('Setup Monitoring'));

    expect(setupContractMonitor).toHaveBeenCalledWith({
      contractHash: '0x1234567890abcdef1234567890abcdef12345678',
      ownerAddress: 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd',
      checkInterval: 300,
      signature: expect.stringContaining('signed_'),
      message: expect.stringContaining('Setup monitor:')
    });

    // Check success message
    expect(await screen.findByText('Monitoring setup successful!')).toBeInTheDocument();
  });
});
```