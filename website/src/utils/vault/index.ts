/**
 * Secure Vault utilities for the Neo Service Layer
 * 
 * This module provides a secure storage mechanism for sensitive data.
 */

import * as crypto from 'crypto';
import { encryptWithTEE, decryptWithTEE, getTEEAttestation, verifyTEEAttestation, TEEConfig } from '../tee';

/**
 * Secret structure
 */
export interface Secret {
  id: string;
  name: string;
  value: string;
  metadata: { [key: string]: any };
  tags?: string[];
  createdAt: string;
  updatedAt?: string;
  accessCount?: number;
  lastAccessed?: string;
  neoAddress?: string;
  permissions?: {
    functionIds: string[];
    roles: string[];
  };
  teeConfig?: {
    encryptionKeyId: string;
    attestationToken: string;
    mrEnclave: string;
  };
}

/**
 * Secret access metrics
 */
interface SecretAccessMetrics {
  accessCount: number;
  lastAccessed: number;
}

/**
 * Vault storage options
 */
export interface VaultOptions {
  storageKey?: string;
  encryptionKey?: string;
  useTEE?: boolean;
  enclaveId?: string;
  teeEnabled?: boolean;
  backupEnabled?: boolean;
  rotationPeriod?: number; // in milliseconds
}

/**
 * Default vault options
 */
const DEFAULT_OPTIONS: VaultOptions = {
  storageKey: 'neo-service-vault',
  useTEE: false,
  teeEnabled: false,
  backupEnabled: false,
  rotationPeriod: 86400000 // 24 hours
};

/**
 * Access record structure
 */
export interface AccessRecord {
  id: string;
  secretId: string;
  timestamp: string;
  functionId: string;
  neoAddress: string;
  status: 'granted' | 'denied';
  teeVerification?: {
    attestationValid: boolean;
    mrEnclaveMatch: boolean;
  };
}

/**
 * Rotation record structure
 */
export interface RotationRecord {
  id: string;
  secretId: string;
  timestamp: string;
  reason: string;
  previousValue: string;
}

/**
 * Secure vault for storing sensitive data
 */
export class SecretVault {
  private options: VaultOptions;
  private secrets: Map<string, Secret> = new Map();
  private metrics: Map<string, SecretAccessMetrics> = new Map();
  private accessLogs: Map<string, AccessRecord[]> = new Map();
  private rotationHistory: Map<string, RotationRecord[]> = new Map();
  private initialized: boolean = false;

  constructor(options: VaultOptions = {}) {
    this.options = { ...DEFAULT_OPTIONS, ...options };
  }

  /**
   * Initialize the vault
   */
  async initialize(): Promise<void> {
    if (this.initialized) return;

    // In a real implementation, this would load secrets from secure storage
    // For this mock implementation, we'll load from localStorage if available
    if (typeof window !== 'undefined' && window.localStorage) {
      const storedData = localStorage.getItem(this.options.storageKey || DEFAULT_OPTIONS.storageKey!);
      
      if (storedData) {
        try {
          const decrypted = await this.decryptData(storedData);
          const parsed = JSON.parse(decrypted.toString());
          
          if (parsed.secrets && Array.isArray(parsed.secrets)) {
            parsed.secrets.forEach((secret: Secret) => {
              this.secrets.set(secret.id, secret);
            });
          }
          
          if (parsed.metrics && typeof parsed.metrics === 'object') {
            Object.entries(parsed.metrics).forEach(([id, metrics]) => {
              this.metrics.set(id, metrics as SecretAccessMetrics);
            });
          }
          
          if (parsed.accessLogs && typeof parsed.accessLogs === 'object') {
            Object.entries(parsed.accessLogs).forEach(([id, logs]) => {
              this.accessLogs.set(id, logs as AccessRecord[]);
            });
          }
          
          if (parsed.rotationHistory && typeof parsed.rotationHistory === 'object') {
            Object.entries(parsed.rotationHistory).forEach(([id, history]) => {
              this.rotationHistory.set(id, history as RotationRecord[]);
            });
          }
        } catch (error) {
          console.error('Failed to initialize vault:', error);
        }
      }
    }
    
    this.initialized = true;
  }

  /**
   * Save the vault state
   */
  private async saveState(): Promise<void> {
    // In a real implementation, this would save to secure storage
    // For this mock implementation, we'll save to localStorage if available
    if (typeof window !== 'undefined' && window.localStorage) {
      const data = {
        secrets: Array.from(this.secrets.values()),
        metrics: Object.fromEntries(this.metrics.entries()),
        accessLogs: Object.fromEntries(this.accessLogs.entries()),
        rotationHistory: Object.fromEntries(this.rotationHistory.entries())
      };
      
      const encrypted = await this.encryptData(JSON.stringify(data));
      localStorage.setItem(this.options.storageKey || DEFAULT_OPTIONS.storageKey!, encrypted);
    }
  }

  /**
   * Encrypt data
   * @param data - Data to encrypt
   * @returns Encrypted data
   */
  private async encryptData(data: string): Promise<string> {
    if (!this.options.encryptionKey) {
      return data; // No encryption if no key
    }

    try {
      const key = crypto.createHash('sha256').update(this.options.encryptionKey).digest();
      const iv = crypto.randomBytes(16);
      const cipher = crypto.createCipheriv('aes-256-gcm', key, iv);
      
      let encrypted = cipher.update(data, 'utf8', 'hex');
      encrypted += cipher.final('hex');
      
      // In GCM mode, we need to get the auth tag
      const authTag = cipher.getAuthTag();
      
      // Return the encrypted data with IV and auth tag
      return JSON.stringify({
        iv: iv.toString('hex'),
        encryptedData: encrypted,
        authTag: authTag.toString('hex')
      });
    } catch (error) {
      console.error('Encryption failed:', error);
      return data; // Fallback to unencrypted data
    }
  }

  /**
   * Decrypt data
   * @param encryptedData - Encrypted data
   * @returns Decrypted data
   */
  private async decryptData(encryptedData: string): Promise<string> {
    if (!this.options.encryptionKey) {
      return encryptedData; // No decryption if no key
    }

    try {
      // Check if the data is actually encrypted (in JSON format)
      if (!encryptedData.startsWith('{')) {
        return encryptedData; // Not encrypted
      }
      
      const { iv, encryptedData: encrypted, authTag } = JSON.parse(encryptedData);
      
      const key = crypto.createHash('sha256').update(this.options.encryptionKey).digest();
      const decipher = crypto.createDecipheriv('aes-256-gcm', key, Buffer.from(iv, 'hex'));
      
      // Set auth tag for GCM mode
      decipher.setAuthTag(Buffer.from(authTag, 'hex'));
      
      let decrypted = decipher.update(encrypted, 'hex', 'utf8');
      decrypted += decipher.final('utf8');
      
      return decrypted;
    } catch (error) {
      console.error('Decryption failed:', error);
      return encryptedData; // Fallback to the original data
    }
  }

  /**
   * Create a new secret
   */
  async createSecret(secret: Secret): Promise<Secret> {
    await this.initialize();
    
    // Validate the secret
    if (!secret.id) {
      throw new Error('Secret ID is required');
    }
    
    if (this.secrets.has(secret.id)) {
      throw new Error(`Secret with ID ${secret.id} already exists`);
    }
    
    // Set created timestamp if not provided
    if (!secret.createdAt) {
      secret.createdAt = new Date().toISOString();
    }
    
    // Set updated timestamp if not provided
    if (!secret.updatedAt) {
      secret.updatedAt = new Date().toISOString();
    }
    
    // Initialize access count if not provided
    if (secret.accessCount === undefined) {
      secret.accessCount = 0;
    }
    
    // Store the secret
    this.secrets.set(secret.id, secret);
    await this.saveState();
    
    return secret;
  }

  /**
   * Get a secret by ID
   */
  async getSecret(id: string): Promise<Secret> {
    await this.initialize();
    
    const secret = this.secrets.get(id);
    if (!secret) {
      throw new Error(`Secret with ID ${id} not found`);
    }
    
    // Update access metrics
    await this.updateAccessMetrics(id);
    
    // Decrypt the secret value if TEE is enabled
    if (this.options.teeEnabled && secret.teeConfig) {
      try {
        const decryptedValue = await decryptWithTEE(secret.value, secret.teeConfig);
        return {
          ...secret,
          value: decryptedValue
        };
      } catch (error) {
        console.error('Failed to decrypt secret:', error);
        throw new Error('Failed to decrypt secret');
      }
    }
    
    return secret;
  }

  /**
   * Update a secret
   * @param id - Secret ID
   * @param secret - Updated secret
   * @returns Updated secret
   */
  async updateSecret(id: string, secret: Secret): Promise<Secret | null> {
    await this.initialize();
    
    if (!this.secrets.has(id)) {
      return null;
    }
    
    // Update the secret
    const updatedSecret = {
      ...this.secrets.get(id),
      ...secret,
      updatedAt: new Date().toISOString()
    };
    
    // Store the updated secret
    this.secrets.set(id, updatedSecret);
    await this.saveState();
    
    return updatedSecret;
  }

  /**
   * Delete a secret
   */
  async deleteSecret(id: string): Promise<void> {
    await this.initialize();
    
    if (!this.secrets.has(id)) {
      throw new Error(`Secret with ID ${id} not found`);
    }
    
    this.secrets.delete(id);
    this.metrics.delete(id);
    this.accessLogs.delete(id);
    this.rotationHistory.delete(id);
    
    await this.saveState();
  }

  /**
   * List secrets
   * @param neoAddress - Optional Neo address to filter by
   * @returns List of secrets
   */
  async listSecrets(neoAddress?: string): Promise<Secret[]> {
    await this.initialize();
    
    const secrets = Array.from(this.secrets.values());
    
    // Filter by Neo address if provided
    if (neoAddress) {
      return secrets.filter(secret => !secret.neoAddress || secret.neoAddress === neoAddress);
    }
    
    return secrets;
  }

  /**
   * Find secrets by tags
   */
  async findSecretsByTags(tags: string[]): Promise<Secret[]> {
    await this.initialize();
    
    return Array.from(this.secrets.values()).filter(secret => {
      if (!secret.tags || !secret.tags.length) return false;
      return tags.some(tag => secret.tags!.includes(tag));
    });
  }

  /**
   * Update access metrics for a secret
   * @param id - Secret ID
   */
  private async updateAccessMetrics(id: string): Promise<void> {
    const secret = this.secrets.get(id);
    if (!secret) return;
    
    // Update access count
    secret.accessCount = (secret.accessCount || 0) + 1;
    
    // Update last accessed timestamp
    secret.lastAccessed = new Date().toISOString();
    
    // Save the updated secret
    this.secrets.set(id, secret);
  }

  /**
   * Get access metrics for a secret
   */
  async getAccessMetrics(id: string): Promise<SecretAccessMetrics> {
    await this.initialize();
    
    const metrics = this.metrics.get(id);
    if (!metrics) {
      throw new Error(`Metrics for secret with ID ${id} not found`);
    }
    
    return metrics;
  }

  /**
   * Record access to a secret
   * @param secretId - Secret ID
   * @param functionId - Function ID requesting access
   * @param neoAddress - Neo address
   * @param allowed - Whether access was allowed
   * @returns Access record
   */
  public async recordAccess(
    secretId: string,
    functionId: string,
    neoAddress: string,
    allowed: boolean
  ): Promise<AccessRecord> {
    const secret = await this.getSecret(secretId);
    if (!secret) {
      throw new Error(`Secret ${secretId} not found`);
    }

    // Update the secret's access count and last accessed time
    secret.accessCount = (secret.accessCount || 0) + 1;
    secret.lastAccessed = new Date().toISOString();
    this.secrets.set(secretId, secret);

    // Create an access record
    const accessRecord: AccessRecord = {
      id: crypto.randomUUID(),
      secretId,
      functionId,
      neoAddress,
      timestamp: new Date().toISOString(),
      status: allowed ? 'granted' : 'denied',
    };

    // Add the access record to the access logs
    const accessLogs = this.accessLogs.get(secretId) || [];
    accessLogs.push(accessRecord);
    this.accessLogs.set(secretId, accessLogs);

    return accessRecord;
  }

  /**
   * Get access logs for a secret
   * @param secretId - Secret ID
   * @returns Access logs
   */
  public async getAccessLogs(secretId: string): Promise<AccessRecord[]> {
    return this.accessLogs.get(secretId) || [];
  }

  /**
   * Get rotation history for a secret
   * @param secretId - Secret ID
   * @returns Rotation history
   */
  public async getRotationHistory(secretId: string): Promise<RotationRecord[]> {
    return this.rotationHistory.get(secretId) || [];
  }

  /**
   * Rotate secrets that are due for rotation
   */
  async rotateSecrets(): Promise<void> {
    await this.initialize();
    
    const now = Date.now();
    
    for (const [id, secret] of this.secrets.entries()) {
      if (!secret.teeConfig) {
        continue;
      }
      
      // Parse attestation time
      const attestationTime = secret.teeConfig.attestationToken 
        ? new Date(JSON.parse(atob(secret.teeConfig.attestationToken.split('.')[1])).iat * 1000).getTime() 
        : 0;
      
      // Check if rotation period has passed
      if (this.options.rotationPeriod && now - attestationTime >= this.options.rotationPeriod) {
        await this.rotateSecret(id, crypto.randomBytes(32).toString('hex'), 'scheduled');
      }
    }
  }

  /**
   * Rotate a secret with a new value
   * @param secretId - Secret ID
   * @param newValue - New secret value
   * @param reason - Reason for rotation
   * @returns Updated secret
   */
  public async rotateSecret(secretId: string, newValue: string, reason: string): Promise<Secret> {
    const secret = await this.getSecret(secretId);
    if (!secret) {
      throw new Error(`Secret ${secretId} not found`);
    }

    // Create a rotation record
    const rotationRecord: RotationRecord = {
      id: crypto.randomUUID(),
      secretId,
      previousValue: secret.value,
      timestamp: new Date().toISOString(),
      reason,
    };

    // Update the secret with the new value
    const updatedSecret: Secret = {
      ...secret,
      value: newValue,
      updatedAt: new Date().toISOString(),
    };

    // Save the updated secret
    this.secrets.set(secretId, updatedSecret);

    // Add the rotation record to the rotation history
    const rotationHistory = this.rotationHistory.get(secretId) || [];
    rotationHistory.push(rotationRecord);
    this.rotationHistory.set(secretId, rotationHistory);

    return updatedSecret;
  }

  /**
   * Check if access to a secret is allowed
   * @param secretId - Secret ID
   * @param functionId - Function ID requesting access
   * @param neoAddress - Neo address
   * @returns Whether access is allowed
   */
  public async checkAccess(secretId: string, functionId: string, neoAddress: string): Promise<boolean> {
    const secret = await this.getSecret(secretId);
    if (!secret) {
      return false;
    }
    
    // Check permissions
    if (secret.permissions) {
      // Check function ID
      if (secret.permissions.functionIds && 
          secret.permissions.functionIds.length > 0 && 
          !secret.permissions.functionIds.includes(functionId)) {
        return false;
      }
      
      // Check Neo address
      if (secret.neoAddress && secret.neoAddress !== neoAddress) {
        return false;
      }
    }
    
    return true;
  }
  
  /**
   * Automatically rotate secrets that are due for rotation
   */
  private async autoRotateSecrets(): Promise<void> {
    const now = Date.now();
    
    for (const [id, secret] of this.secrets.entries()) {
      if (!secret.teeConfig) {
        continue;
      }
      
      // Parse attestation time
      const attestationTime = secret.teeConfig.attestationToken 
        ? new Date(JSON.parse(atob(secret.teeConfig.attestationToken.split('.')[1])).iat * 1000).getTime() 
        : 0;
      
      // Check if rotation period has passed
      if (this.options.rotationPeriod && now - attestationTime >= this.options.rotationPeriod) {
        await this.rotateSecret(id, crypto.randomBytes(32).toString('hex'), 'scheduled');
      }
    }
  }

  /**
   * Log access to a secret
   * @param secretId - Secret ID
   * @param accessRecord - Access record
   */
  public async logAccess(secretId: string, accessRecord: Omit<AccessRecord, 'id'>): Promise<void> {
    if (!this.accessLogs.has(secretId)) {
      this.accessLogs.set(secretId, []);
    }
    
    const logs = this.accessLogs.get(secretId);
    if (logs) {
      logs.push({
        id: crypto.randomUUID(),
        ...accessRecord,
        timestamp: new Date().toISOString()
      });
    }
    
    // Update metrics
    const secret = await this.getSecret(secretId);
    if (secret) {
      secret.accessCount = (secret.accessCount || 0) + 1;
      secret.lastAccessed = new Date().toISOString();
      await this.updateSecret(secretId, secret);
    }
  }

  /**
   * Clear all secrets (dangerous operation)
   */
  async clearAll(): Promise<void> {
    this.secrets.clear();
    this.metrics.clear();
    this.accessLogs.clear();
    this.rotationHistory.clear();
    
    await this.saveState();
  }
}