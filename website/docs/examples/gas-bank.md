# Gas Bank Usage Examples

## Overview
These examples demonstrate how to interact with the Gas Bank service for managing gas on the Neo N3 network.

## Basic Gas Bank Usage

### Checking Gas Balance
```typescript
import { useEffect, useState } from 'react';
import { getGasBalance } from '../api/gas-bank';

function GasBalanceDisplay() {
  const [balance, setBalance] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchBalance = async () => {
      try {
        const result = await getGasBalance();
        setBalance(result.balance);
        setLoading(false);
      } catch (error) {
        console.error('Failed to fetch gas balance:', error);
        setLoading(false);
      }
    };

    fetchBalance();
    const interval = setInterval(fetchBalance, 60000); // Update every minute
    return () => clearInterval(interval);
  }, []);

  if (loading) return <div>Loading gas balance...</div>;
  return <div>Available Gas: {balance?.toFixed(8)}</div>;
}
```

### Requesting Gas
```typescript
import { useState } from 'react';
import { requestGas } from '../api/gas-bank';
import { useWallet } from '../src/app/hooks/useWallet';

function GasRequest() {
  const { neoAddress, signMessage } = useWallet();
  const [amount, setAmount] = useState('');
  const [status, setStatus] = useState<'idle' | 'pending' | 'success' | 'error'>('idle');
  const [error, setError] = useState<string | null>(null);

  const handleRequest = async () => {
    if (!amount || !neoAddress) return;

    try {
      setStatus('pending');
      setError(null);

      // Sign the request
      const message = `Request gas: ${amount}`;
      const signature = await signMessage(message);

      // Submit request
      await requestGas({
        amount: parseFloat(amount),
        address: neoAddress,
        signature,
        message
      });

      setStatus('success');
      setAmount('');
    } catch (error) {
      setError(error.message);
      setStatus('error');
    }
  };

  return (
    <div>
      <input
        type="number"
        value={amount}
        onChange={(e) => setAmount(e.target.value)}
        placeholder="Enter GAS amount"
        disabled={status === 'pending'}
      />
      <button
        onClick={handleRequest}
        disabled={!amount || status === 'pending'}
      >
        Request GAS
      </button>
      {status === 'success' && (
        <div className="success">Gas request successful!</div>
      )}
      {error && (
        <div className="error">{error}</div>
      )}
    </div>
  );
}
```

## Advanced Usage

### Automated Gas Management
```typescript
import { useEffect, useState } from 'react';
import { getGasBalance, requestGas } from '../api/gas-bank';
import { useWallet } from '../src/app/hooks/useWallet';

function AutoGasManager() {
  const { neoAddress, signMessage } = useWallet();
  const [isManaging, setIsManaging] = useState(false);
  const threshold = 1; // Request more gas when balance falls below this
  const requestAmount = 5; // Amount of gas to request

  useEffect(() => {
    if (!isManaging || !neoAddress) return;

    const checkAndRequestGas = async () => {
      try {
        const { balance } = await getGasBalance();
        
        if (balance < threshold) {
          const message = `Auto request gas: ${requestAmount}`;
          const signature = await signMessage(message);
          
          await requestGas({
            amount: requestAmount,
            address: neoAddress,
            signature,
            message
          });
          
          console.log('Auto gas request successful');
        }
      } catch (error) {
        console.error('Auto gas request failed:', error);
      }
    };

    // Check every 5 minutes
    const interval = setInterval(checkAndRequestGas, 5 * 60 * 1000);
    return () => clearInterval(interval);
  }, [isManaging, neoAddress]);

  return (
    <div>
      <label>
        <input
          type="checkbox"
          checked={isManaging}
          onChange={(e) => setIsManaging(e.target.checked)}
        />
        Enable Auto Gas Management
      </label>
    </div>
  );
}
```

### Gas Usage Tracking
```typescript
import { useEffect, useState } from 'react';
import { getGasUsage } from '../api/gas-bank';

interface GasUsage {
  date: string;
  amount: number;
  transaction: string;
}

function GasUsageTracker() {
  const [usage, setUsage] = useState<GasUsage[]>([]);
  const [timeframe, setTimeframe] = useState<'day' | 'week' | 'month'>('day');

  useEffect(() => {
    const fetchUsage = async () => {
      try {
        const data = await getGasUsage(timeframe);
        setUsage(data);
      } catch (error) {
        console.error('Failed to fetch gas usage:', error);
      }
    };

    fetchUsage();
  }, [timeframe]);

  const totalUsage = usage.reduce((sum, entry) => sum + entry.amount, 0);

  return (
    <div>
      <div>
        <select
          value={timeframe}
          onChange={(e) => setTimeframe(e.target.value as any)}
        >
          <option value="day">Last 24 Hours</option>
          <option value="week">Last Week</option>
          <option value="month">Last Month</option>
        </select>
      </div>
      
      <div>Total Gas Used: {totalUsage.toFixed(8)}</div>
      
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Amount</th>
            <th>Transaction</th>
          </tr>
        </thead>
        <tbody>
          {usage.map((entry) => (
            <tr key={entry.transaction}>
              <td>{new Date(entry.date).toLocaleString()}</td>
              <td>{entry.amount.toFixed(8)}</td>
              <td>{entry.transaction}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

### Gas Bank Notifications
```typescript
import { useEffect } from 'react';
import { subscribeToGasEvents } from '../api/gas-bank';
import { useToast } from '../src/app/hooks/useToast';

function GasNotifications() {
  const { showToast } = useToast();

  useEffect(() => {
    const unsubscribe = subscribeToGasEvents((event) => {
      switch (event.type) {
        case 'low_balance':
          showToast(
            `Low gas balance: ${event.balance.toFixed(8)} GAS`,
            'warning'
          );
          break;
        case 'request_approved':
          showToast(
            `Gas request approved: ${event.amount.toFixed(8)} GAS`,
            'success'
          );
          break;
        case 'request_rejected':
          showToast(
            `Gas request rejected: ${event.reason}`,
            'error'
          );
          break;
      }
    });

    return () => unsubscribe();
  }, []);

  return null; // This component just handles notifications
}
```

## Testing Examples

### Unit Testing Gas Bank Components
```typescript
import { render, screen, fireEvent, act } from '@testing-library/react';
import { requestGas } from '../api/gas-bank';
import GasRequest from './GasRequest';

jest.mock('../api/gas-bank');
jest.mock('../src/app/hooks/useWallet', () => ({
  useWallet: () => ({
    neoAddress: 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd',
    signMessage: async (msg: string) => 'signed_' + msg
  })
}));

describe('GasRequest', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('submits gas request successfully', async () => {
    (requestGas as jest.Mock).mockResolvedValue({ success: true });

    render(<GasRequest />);
    
    const input = screen.getByPlaceholderText('Enter GAS amount');
    fireEvent.change(input, { target: { value: '5' } });
    
    const button = screen.getByText('Request GAS');
    await act(async () => {
      fireEvent.click(button);
    });

    expect(requestGas).toHaveBeenCalledWith({
      amount: 5,
      address: 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd',
      signature: expect.stringContaining('signed_'),
      message: expect.stringContaining('Request gas: 5')
    });

    expect(screen.getByText('Gas request successful!')).toBeInTheDocument();
  });

  it('handles request errors', async () => {
    (requestGas as jest.Mock).mockRejectedValue(
      new Error('Insufficient funds')
    );

    render(<GasRequest />);
    
    const input = screen.getByPlaceholderText('Enter GAS amount');
    fireEvent.change(input, { target: { value: '5' } });
    
    const button = screen.getByText('Request GAS');
    await act(async () => {
      fireEvent.click(button);
    });

    expect(screen.getByText('Insufficient funds')).toBeInTheDocument();
  });
});
```