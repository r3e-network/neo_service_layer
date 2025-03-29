import { wallet } from '@cityofzion/neon-js';

/**
 * Formats a Neo N3 address for display
 * @param address Full Neo N3 address
 * @returns Shortened address with ellipsis
 */
export function formatNeoAddress(address: string): string {
  if (!address) return '';
  if (address.length <= 13) return address;
  return `${address.slice(0, 6)}...${address.slice(-4)}`;
}

/**
 * Validates a Neo N3 contract hash
 * @param hash Contract hash to validate
 * @returns Promise<boolean> Whether the hash is valid
 */
export async function validateContractHash(hash: string): Promise<boolean> {
  try {
    // Basic format validation
    if (!hash || typeof hash !== 'string') return false;
    
    // Remove '0x' prefix if present
    const cleanHash = hash.startsWith('0x') ? hash.slice(2) : hash;
    
    // Check length (40 characters for Neo contract hash)
    if (cleanHash.length !== 40) return false;
    
    // Check if it's a valid hex string
    if (!/^[0-9a-fA-F]{40}$/.test(cleanHash)) return false;

    // In production, this would also verify the contract exists on chain
    // For now, we'll just return true if format is valid
    return true;
  } catch (error) {
    console.error('Error validating contract hash:', error);
    return false;
  }
}

export const getNeoScriptHash = (address: string): string => {
  try {
    // Convert Neo address to script hash
    return wallet.getScriptHashFromAddress(address);
  } catch (error) {
    console.error('Error getting script hash:', error);
    return '';
  }
};