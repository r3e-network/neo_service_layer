import {
  GasAccount,
  GasTransaction,
  GasTransactionStatus,
  GasEstimate,
  GasSettings,
  GasReservation,
  GasMetrics
} from '../types/types';
import { API_BASE_URL } from '../constants';

/**
 * Gas Bank API client for managing gas on Neo N3
 */
class GasBankApi {
  private baseUrl: string;

  constructor() {
    this.baseUrl = `${API_BASE_URL}/gas-bank`;
  }

  /**
   * Get gas account by address
   */
  async getAccount(address: string): Promise<GasAccount> {
    const response = await fetch(`${this.baseUrl}/accounts/${address}`);
    if (!response.ok) {
      throw new Error('Failed to fetch gas account');
    }
    return response.json();
  }

  /**
   * Get all gas accounts
   */
  async getAccounts(): Promise<GasAccount[]> {
    const response = await fetch(`${this.baseUrl}/accounts`);
    if (!response.ok) {
      throw new Error('Failed to fetch gas accounts');
    }
    return response.json();
  }

  /**
   * Create a new gas account
   */
  async createAccount(address: string): Promise<GasAccount> {
    const response = await fetch(`${this.baseUrl}/accounts`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ address }),
    });
    if (!response.ok) {
      throw new Error('Failed to create gas account');
    }
    return response.json();
  }

  /**
   * Get gas transactions for an account
   */
  async getTransactions(
    address: string,
    options?: {
      limit?: number;
      offset?: number;
      type?: string;
      status?: string;
    }
  ): Promise<{ transactions: GasTransaction[]; total: number }> {
    const params = new URLSearchParams();
    if (options) {
      Object.entries(options).forEach(([key, value]) => {
        if (value !== undefined) {
          params.append(key, value.toString());
        }
      });
    }

    const response = await fetch(
      `${this.baseUrl}/accounts/${address}/transactions?${params.toString()}`
    );
    if (!response.ok) {
      throw new Error('Failed to fetch gas transactions');
    }
    return response.json();
  }

  /**
   * Reserve gas for a specific purpose
   */
  async createReservation(
    address: string,
    amount: number,
    purpose: string,
    metadata: Record<string, any>
  ): Promise<GasReservation> {
    const response = await fetch(`${this.baseUrl}/accounts/${address}/reservations`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ amount, purpose, metadata }),
    });
    if (!response.ok) {
      throw new Error('Failed to create gas reservation');
    }
    return response.json();
  }

  /**
   * Get active reservations for an account
   */
  async getReservations(address: string): Promise<GasReservation[]> {
    const response = await fetch(`${this.baseUrl}/accounts/${address}/reservations`);
    if (!response.ok) {
      throw new Error('Failed to fetch gas reservations');
    }
    return response.json();
  }

  /**
   * Release a gas reservation
   */
  async releaseReservation(
    address: string,
    reservationId: string
  ): Promise<GasReservation> {
    const response = await fetch(
      `${this.baseUrl}/accounts/${address}/reservations/${reservationId}`,
      {
        method: 'DELETE',
      }
    );
    if (!response.ok) {
      throw new Error('Failed to release gas reservation');
    }
    return response.json();
  }

  /**
   * Consume gas from a reservation
   */
  async consumeGas(
    address: string,
    reservationId: string,
    amount: number,
    metadata: Record<string, any>
  ): Promise<GasTransaction> {
    const response = await fetch(
      `${this.baseUrl}/accounts/${address}/reservations/${reservationId}/consume`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ amount, metadata }),
      }
    );
    if (!response.ok) {
      throw new Error('Failed to consume gas');
    }
    return response.json();
  }

  /**
   * Get gas metrics
   */
  async getMetrics(): Promise<GasMetrics> {
    const response = await fetch(`${this.baseUrl}/metrics`);
    if (!response.ok) {
      throw new Error('Failed to fetch gas metrics');
    }
    return response.json();
  }

  /**
   * Estimate gas for a transaction
   */
  async estimateGas(
    address: string,
    operation: string,
    metadata: Record<string, any>
  ): Promise<GasEstimate> {
    const response = await fetch(`${this.baseUrl}/estimate`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ address, operation, metadata }),
    });
    if (!response.ok) {
      throw new Error('Failed to estimate gas');
    }
    return response.json();
  }

  /**
   * Get gas settings
   */
  async getSettings(): Promise<GasSettings> {
    const response = await fetch(`${this.baseUrl}/settings`);
    if (!response.ok) {
      throw new Error('Failed to fetch gas settings');
    }
    return response.json();
  }

  /**
   * Update gas settings
   */
  async updateSettings(settings: Partial<GasSettings>): Promise<GasSettings> {
    const response = await fetch(`${this.baseUrl}/settings`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(settings),
    });
    if (!response.ok) {
      throw new Error('Failed to update gas settings');
    }
    return response.json();
  }
}

export const gasBankApi = new GasBankApi();