/**
 * Neo blockchain utilities for the Neo Service Layer
 * 
 * This module provides functions for working with Neo blockchain addresses,
 * contract hashes, and script hashes.
 */

import * as crypto from 'crypto';
import bs58 from 'bs58';

/**
 * Validates a Neo contract hash format
 * 
 * @param hash - The contract hash to validate
 * @returns True if the hash is valid, false otherwise
 */
export function validateContractHash(hash: string): boolean {
  // Neo contract hashes are 40 characters (20 bytes) in hexadecimal
  if (!hash || typeof hash !== 'string' || hash.length !== 40) {
    return false;
  }
  
  // Check if it's a valid hex string
  return /^[0-9a-fA-F]{40}$/.test(hash);
}

/**
 * Formats a Neo address with proper spacing and checksum
 * 
 * @param address - The Neo address to format
 * @returns The formatted address or null if invalid
 */
export function formatNeoAddress(address: string): string | null {
  if (!address || typeof address !== 'string' || address.length !== 34) {
    return null;
  }
  
  // Format with spaces for readability
  return `${address.substring(0, 8)} ${address.substring(8, 16)} ${address.substring(16, 24)} ${address.substring(24)}`;
}

/**
 * Converts a Neo address to a script hash
 * 
 * @param address - The Neo address to convert
 * @returns The script hash in little-endian format
 */
export function getNeoScriptHash(address: string): string | null {
  try {
    // Decode from base58
    const decoded = bs58.decode(address);
    
    // Remove version byte and checksum (first and last 4 bytes)
    const scriptHash = decoded.slice(1, -4);
    
    // Convert to hex and reverse (Neo uses little-endian)
    return Buffer.from(scriptHash).reverse().toString('hex');
  } catch (error) {
    console.error(`Error converting address to script hash: ${error}`);
    return null;
  }
}

/**
 * Converts a script hash to a Neo address
 * 
 * @param scriptHash - The script hash in little-endian format
 * @returns The Neo address
 */
export function scriptHashToAddress(scriptHash: string): string | null {
  try {
    // Ensure proper format
    if (!validateContractHash(scriptHash)) {
      throw new Error('Invalid script hash format');
    }
    
    // Convert from hex and reverse (from little-endian to big-endian)
    const scriptHashBuffer = Buffer.from(scriptHash, 'hex').reverse();
    
    // Add version byte (0x17 for Neo mainnet addresses)
    const versionedBuffer = Buffer.concat([Buffer.from([0x17]), scriptHashBuffer]);
    
    // Calculate checksum (first 4 bytes of double SHA256)
    const firstSha = crypto.createHash('sha256').update(versionedBuffer).digest();
    const secondSha = crypto.createHash('sha256').update(firstSha).digest();
    const checksum = secondSha.slice(0, 4);
    
    // Combine versioned buffer and checksum
    const addressBuffer = Buffer.concat([versionedBuffer, checksum]);
    
    // Encode to base58
    return bs58.encode(addressBuffer);
  } catch (error) {
    console.error(`Error converting script hash to address: ${error}`);
    return null;
  }
}

/**
 * Calculates the gas cost for a Neo transaction
 * 
 * @param size - The size of the transaction in bytes
 * @param systemFee - The system fee for the transaction
 * @returns The total gas cost
 */
export function calculateGasCost(size: number, systemFee: number): number {
  // Network fee is calculated based on transaction size
  const networkFee = (size / 1024) * 0.001;
  
  // Total gas cost is the sum of system fee and network fee
  return systemFee + networkFee;
}

/**
 * Formats a GAS amount with appropriate precision
 * 
 * @param amount - The GAS amount to format
 * @param precision - The number of decimal places to show
 * @returns The formatted GAS amount
 */
export function formatGas(amount: number, precision: number = 8): string {
  return amount.toFixed(precision) + ' GAS';
}