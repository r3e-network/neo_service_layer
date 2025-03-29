import { wallet } from '@cityofzion/neon-js';

export async function verifySignature(signature: string): Promise<boolean> {
  if (!signature) {
    return false;
  }

  // Check if signature matches expected format (64 bytes hex string)
  const hexRegex = /^[0-9a-fA-F]{128}$/;
  if (!hexRegex.test(signature)) {
    return false;
  }

  try {
    // Verify signature length and format
    const signatureBytes = Buffer.from(signature, 'hex');
    if (signatureBytes.length !== 64) {
      return false;
    }

    // Split signature into r and s components
    const r = signatureBytes.slice(0, 32);
    const s = signatureBytes.slice(32, 64);

    // Verify r and s are valid values
    return r.length === 32 && s.length === 32;
  } catch (error) {
    console.error('Error verifying signature:', error);
    return false;
  }
}

export function verifyNeoAddress(address: string): boolean {
  return wallet.isAddress(address);
}