/**
 * Types for the Secrets service
 */

/**
 * Encryption algorithm options
 */
export enum EncryptionAlgorithm {
  AES256 = 'AES-256-GCM',
  AES192 = 'AES-192-GCM',
  AES128 = 'AES-128-GCM',
}

/**
 * Encryption options
 */
export interface EncryptionOptions {
  algorithm?: EncryptionAlgorithm;
  iv?: Buffer;
  authTagLength?: number;
}

/**
 * Encrypted data structure
 */
export interface EncryptedData {
  ciphertext: string;
  iv: string;
  authTag: string;
  algorithm: EncryptionAlgorithm;
}

/**
 * Secret key structure
 */
export interface SecretKey {
  id: string;
  key: string;
  createdAt: number;
  expiresAt?: number;
  metadata?: Record<string, any>;
}

/**
 * Secret management options
 */
export interface SecretOptions {
  expiresIn?: number; // milliseconds
  metadata?: Record<string, any>;
}