// @ts-ignore
import * as React from 'react';
import {
  GasAccount,
  GasTransaction,
  GasReservation,
  GasMetrics,
  GasEstimate,
  GasSettings
} from '../types/types';
import { gasBankApi } from '../api/gasBankApi';
import { REFRESH_INTERVAL } from '../constants';

export function useGasBank(address?: string) {
  const [accounts, setAccounts] = React.useState<GasAccount[]>([]);
  const [selectedAccount, setSelectedAccount] = React.useState<GasAccount | null>(null);
  const [transactions, setTransactions] = React.useState<GasTransaction[]>([]);
  const [totalTransactions, setTotalTransactions] = React.useState(0);
  const [reservations, setReservations] = React.useState<GasReservation[]>([]);
  const [metrics, setMetrics] = React.useState<GasMetrics | null>(null);
  const [settings, setSettings] = React.useState<GasSettings | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<Error | null>(null);

  const fetchAccounts = React.useCallback(async () => {
    try {
      const accounts = await gasBankApi.getAccounts();
      setAccounts(accounts);
      if (address) {
        const account = accounts.find(a => a.address === address);
        setSelectedAccount(account || null);
      }
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch accounts'));
    }
  }, [address]);

  const fetchTransactions = React.useCallback(async (
    accountAddress: string,
    options?: {
      limit?: number;
      offset?: number;
      type?: string;
      status?: string;
    }
  ) => {
    try {
      const { transactions, total } = await gasBankApi.getTransactions(
        accountAddress,
        options
      );
      setTransactions(transactions);
      setTotalTransactions(total);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch transactions'));
    }
  }, []);

  const fetchReservations = React.useCallback(async (accountAddress: string) => {
    try {
      const reservations = await gasBankApi.getReservations(accountAddress);
      setReservations(reservations);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch reservations'));
    }
  }, []);

  const fetchMetrics = React.useCallback(async () => {
    try {
      const metrics = await gasBankApi.getMetrics();
      setMetrics(metrics);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch metrics:', err);
    }
  }, []);

  const fetchSettings = React.useCallback(async () => {
    try {
      const settings = await gasBankApi.getSettings();
      setSettings(settings);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch settings:', err);
    }
  }, []);

  React.useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      await Promise.all([
        fetchAccounts(),
        fetchMetrics(),
        fetchSettings()
      ]);
      if (address) {
        await Promise.all([
          fetchTransactions(address),
          fetchReservations(address)
        ]);
      }
      setLoading(false);
    };

    fetchData();
    const interval = setInterval(fetchData, REFRESH_INTERVAL);
    return () => clearInterval(interval);
  }, [
    address,
    fetchAccounts,
    fetchTransactions,
    fetchReservations,
    fetchMetrics,
    fetchSettings
  ]);

  const createAccount = React.useCallback(async (address: string) => {
    try {
      const account = await gasBankApi.createAccount(address);
      setAccounts(prev => [...prev, account]);
      return account;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to create account');
    }
  }, []);

  const createReservation = React.useCallback(async (
    address: string,
    amount: number,
    purpose: string,
    metadata: Record<string, any>
  ) => {
    try {
      const reservation = await gasBankApi.createReservation(
        address,
        amount,
        purpose,
        metadata
      );
      setReservations(prev => [...prev, reservation]);
      return reservation;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to create reservation');
    }
  }, []);

  const releaseReservation = React.useCallback(async (
    address: string,
    reservationId: string
  ) => {
    try {
      const reservation = await gasBankApi.releaseReservation(address, reservationId);
      setReservations(prev =>
        prev.map(r => (r.id === reservationId ? reservation : r))
      );
      return reservation;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to release reservation');
    }
  }, []);

  const consumeGas = React.useCallback(async (
    address: string,
    reservationId: string,
    amount: number,
    metadata: Record<string, any>
  ) => {
    try {
      const transaction = await gasBankApi.consumeGas(
        address,
        reservationId,
        amount,
        metadata
      );
      setTransactions(prev => [transaction, ...prev]);
      return transaction;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to consume gas');
    }
  }, []);

  const estimateGas = React.useCallback(async (
    address: string,
    operation: string,
    metadata: Record<string, any>
  ): Promise<GasEstimate> => {
    try {
      return await gasBankApi.estimateGas(address, operation, metadata);
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to estimate gas');
    }
  }, []);

  const updateSettings = React.useCallback(async (newSettings: Partial<GasSettings>) => {
    try {
      const updated = await gasBankApi.updateSettings(newSettings);
      setSettings(updated);
      return updated;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to update settings');
    }
  }, []);

  return {
    accounts,
    selectedAccount,
    transactions,
    totalTransactions,
    reservations,
    metrics,
    settings,
    loading,
    error,
    createAccount,
    createReservation,
    releaseReservation,
    consumeGas,
    estimateGas,
    updateSettings,
    fetchTransactions
  };
}