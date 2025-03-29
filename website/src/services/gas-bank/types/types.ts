/**
 * Types for the gas bank service
 */

export interface GasAccount {
  id: string;
  address: string;
  balance: number;
  reserved: number;
  available: number;
  lastUpdated: number;
  status: GasAccountStatus;
  transactions: GasTransaction[];
}

export type GasAccountStatus = 'active' | 'low' | 'depleted' | 'locked';

export interface GasTransaction {
  id: string;
  timestamp: number;
  type: GasTransactionType;
  amount: number;
  status: GasTransactionStatus;
  hash?: string;
  fee?: number;
  error?: string;
  metadata: {
    functionId?: string;
    triggerId?: string;
    requestId?: string;
    [key: string]: any;
  };
}

export type GasTransactionType = 
  | 'deposit'
  | 'withdraw'
  | 'reserve'
  | 'release'
  | 'consume';

export type GasTransactionStatus = 
  | 'pending'
  | 'confirmed'
  | 'failed'
  | 'cancelled';

export interface GasReservation {
  id: string;
  accountId: string;
  amount: number;
  expiresAt: number;
  purpose: string;
  status: GasReservationStatus;
  metadata: {
    functionId?: string;
    triggerId?: string;
    requestId?: string;
    [key: string]: any;
  };
}

export type GasReservationStatus = 
  | 'active'
  | 'consumed'
  | 'expired'
  | 'released';

export interface GasMetrics {
  totalAccounts: number;
  totalBalance: number;
  totalReserved: number;
  totalAvailable: number;
  averageGasPrice: number;
  transactionsPerMinute: number;
  successRate: number;
  accountBreakdown: {
    active: number;
    low: number;
    depleted: number;
    locked: number;
  };
}

export interface GasEstimate {
  estimatedGas: number;
  estimatedCost: number;
  confidence: number;
  baseFee: number;
  priorityFee: number;
}

export interface GasSettings {
  minBalance: number;
  maxBalance: number;
  reservationTimeout: number;
  autoReplenishThreshold: number;
  autoReplenishAmount: number;
  maxReservationAmount: number;
  maxTransactionsPerMinute: number;
}

export interface GasBalance {
  total: number;
  available: number;
  reserved: number;
  lastUpdated: number;
}