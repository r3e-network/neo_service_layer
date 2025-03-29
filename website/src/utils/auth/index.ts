/**
 * Authentication utilities for the Neo Service Layer
 * 
 * This module provides functions for signature verification and Neo address validation.
 */

import * as crypto from 'crypto';

/**
 * Verifies a digital signature against a message and public key
 * 
 * @param message - The original message that was signed
 * @param signature - The signature to verify
 * @param publicKey - The public key to verify against
 * @returns True if the signature is valid, false otherwise
 */
export function verifySignature(
  message: string,
  signature: string,
  publicKey: string
): boolean {
  try {
    const verify = crypto.createVerify('SHA256');
    verify.update(message);
    return verify.verify(
      {
        key: Buffer.from(publicKey, 'hex'),
        padding: crypto.constants.RSA_PKCS1_PSS_PADDING,
      },
      Buffer.from(signature, 'hex')
    );
  } catch (error) {
    console.error(`Error verifying signature: ${error}`);
    return false;
  }
}

/**
 * Validates a Neo blockchain address format
 * 
 * @param address - The Neo address to validate
 * @returns True if the address is valid, false otherwise
 */
export function verifyNeoAddress(address: string): boolean {
  // Neo addresses are base58 encoded and 34 characters long
  if (!address || typeof address !== 'string' || address.length !== 34) {
    return false;
  }

  // Neo addresses start with 'A' or 'N'
  if (!address.startsWith('A') && !address.startsWith('N')) {
    return false;
  }

  // Check for valid base58 characters
  const base58Chars = '123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz';
  return address.split('').every(char => base58Chars.includes(char));
}

/**
 * Generates a JWT token for authentication
 * 
 * @param payload - The data to include in the token
 * @param secret - The secret key for signing
 * @param expiresIn - Token expiration time in seconds
 * @returns The signed JWT token
 */
export function generateToken(
  payload: Record<string, any>,
  secret: string,
  expiresIn: number = 3600
): string {
  const header = {
    alg: 'HS256',
    typ: 'JWT'
  };

  const now = Math.floor(Date.now() / 1000);
  const tokenPayload = {
    ...payload,
    iat: now,
    exp: now + expiresIn
  };

  const base64Header = Buffer.from(JSON.stringify(header)).toString('base64').replace(/=/g, '');
  const base64Payload = Buffer.from(JSON.stringify(tokenPayload)).toString('base64').replace(/=/g, '');
  
  const signature = crypto
    .createHmac('sha256', secret)
    .update(`${base64Header}.${base64Payload}`)
    .digest('base64')
    .replace(/=/g, '');

  return `${base64Header}.${base64Payload}.${signature}`;
}

/**
 * Verifies a JWT token
 * 
 * @param token - The JWT token to verify
 * @param secret - The secret key used for signing
 * @returns The decoded payload if valid, null otherwise
 */
export function verifyToken(token: string, secret: string): Record<string, any> | null {
  try {
    const [header, payload, signature] = token.split('.');
    
    const expectedSignature = crypto
      .createHmac('sha256', secret)
      .update(`${header}.${payload}`)
      .digest('base64')
      .replace(/=/g, '');
    
    if (signature !== expectedSignature) {
      return null;
    }
    
    const decodedPayload = JSON.parse(Buffer.from(payload, 'base64').toString());
    const now = Math.floor(Date.now() / 1000);
    
    if (decodedPayload.exp && decodedPayload.exp < now) {
      return null; // Token expired
    }
    
    return decodedPayload;
  } catch (error) {
    console.error(`Error verifying token: ${error}`);
    return null;
  }
}