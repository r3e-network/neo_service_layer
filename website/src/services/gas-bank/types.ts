/**
 * Types for the Gas Bank service
 */

/**
 * Represents a gas reservation in the system
 */
export interface GasReservation {
  id: string;
  gasAmount: number;
  expiresAt: number;
  createdAt: number;
  status: 'active' | 'consumed' | 'expired';
  metadata?: Record<string, any>;
  accountId: string;
}

/**
 * Represents a gas account in the system
 */
export interface GasAccount {
  id: string;
  balance: number;
  owner: string;
  createdAt: number;
  lastUpdated: number;
  reservations?: GasReservation[];
}

/**
 * Options for creating a new gas reservation
 */
export interface CreateReservationOptions {
  gasAmount: number;
  duration: number; // in milliseconds
  metadata?: Record<string, any>;
  accountId: string;
}

/**
 * Response from the gas reservation API
 */
export interface GasReservationResponse {
  success: boolean;
  reservation?: GasReservation;
  error?: string;
}