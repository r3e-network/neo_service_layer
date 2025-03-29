/**
 * Authentication Service
 * 
 * This service provides authentication functionality for the Neo Service Layer.
 * It handles user authentication, session management, and access control.
 */

import React from 'react';

// Authentication types
export interface User {
  id: string;
  address: string;
  username?: string;
  isAuthenticated: boolean;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface AuthError {
  message: string;
  code?: string;
}

/**
 * Generate a challenge for wallet authentication
 * @returns Challenge string for the user to sign
 */
export async function generateChallenge(address: string): Promise<string> {
  try {
    const response = await fetch(`/api/auth/challenge?address=${address}`);
    if (!response.ok) {
      throw new Error('Failed to generate challenge');
    }
    const data = await response.json();
    return data.challenge;
  } catch (error) {
    console.error('Error generating challenge:', error);
    throw error;
  }
}

/**
 * Verify a signed challenge for authentication
 * @param address Wallet address
 * @param signature Signed challenge
 * @returns Authentication response with token and user information
 */
export async function verifySignature(address: string, signature: string): Promise<AuthResponse> {
  try {
    const response = await fetch('/api/auth/verify', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ address, signature }),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Authentication failed');
    }

    return await response.json();
  } catch (error) {
    console.error('Error verifying signature:', error);
    throw error;
  }
}

/**
 * Get the current authenticated user
 * @returns User information if authenticated, null otherwise
 */
export async function getCurrentUser(): Promise<User | null> {
  try {
    // Check for token in localStorage
    const token = localStorage.getItem('neo_auth_token');
    if (!token) {
      return null;
    }

    const response = await fetch('/api/auth/me', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      localStorage.removeItem('neo_auth_token');
      return null;
    }

    const data = await response.json();
    return data.user;
  } catch (error) {
    console.error('Error getting current user:', error);
    return null;
  }
}

/**
 * Log out the current user
 */
export function logout(): void {
  localStorage.removeItem('neo_auth_token');
  // You might want to redirect to login page or home page
  window.location.href = '/';
}

/**
 * Get authentication token
 * @returns The authentication token if available
 */
export function getAuthToken(): string | null {
  if (typeof window === 'undefined') {
    return null;
  }
  return localStorage.getItem('neo_auth_token');
}

/**
 * Set authentication token
 * @param token The authentication token to store
 */
export function setAuthToken(token: string): void {
  if (typeof window === 'undefined') {
    return;
  }
  localStorage.setItem('neo_auth_token', token);
}
