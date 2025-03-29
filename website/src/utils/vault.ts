import { Logger } from './logger';
import { encryptWithTEE, decryptWithTEE, generateTEEConfig } from './tee';

export interface Secret {
  id: string;
  name: string;
  value: string;
  neoAddress: string;
  createdAt: string;
  lastAccessed: string;
  accessCount: number;
  permissions: {
    functionIds: string[];
    roles: string[];
  };
  teeConfig: {
    encryptionKeyId: string;
    attestationToken: string;
    mrEnclave: string;
  };
}

export interface VaultConfig {
  teeEnabled?: boolean;
  backupEnabled?: boolean;
  rotationPeriod?: number;
}

export class SecretVault {
  private config: VaultConfig;
  private secrets: Map<string, Secret>;
  private logger: Logger;

  constructor(config: VaultConfig = {}) {
    this.config = {
      teeEnabled: config.teeEnabled ?? false,
      backupEnabled: config.backupEnabled ?? false,
      rotationPeriod: config.rotationPeriod ?? 30 * 24 * 60 * 60 * 1000 // 30 days default
    };
    this.secrets = new Map();
    this.logger = Logger.getInstance();
  }

  async createSecret(secret: Secret): Promise<void> {
    const now = new Date().toISOString();
    const newSecret = {
      ...secret,
      createdAt: now,
      lastAccessed: now,
      accessCount: 0
    };
    this.secrets.set(secret.id, newSecret);
  }

  async getSecret(id: string): Promise<Secret | undefined> {
    const secret = this.secrets.get(id);
    if (!secret) {
      return undefined;
    }
    await this.logAccess(id);
    return secret;
  }

  async listSecrets(neoAddress: string): Promise<Secret[]> {
    return Array.from(this.secrets.values())
      .filter(secret => secret.neoAddress === neoAddress);
  }

  async updateSecret(secret: Secret): Promise<void> {
    if (!this.secrets.has(secret.id)) {
      throw new Error(`Secret not found: ${secret.id}`);
    }
    const existingSecret = this.secrets.get(secret.id)!;
    this.secrets.set(secret.id, {
      ...existingSecret,
      ...secret,
      lastAccessed: existingSecret.lastAccessed,
      accessCount: existingSecret.accessCount
    });
  }

  async deleteSecret(id: string): Promise<void> {
    this.secrets.delete(id);
    await this.deleteAccessLogs(id);
  }

  async logAccess(id: string): Promise<void> {
    const secret = this.secrets.get(id);
    if (secret) {
      const now = new Date().toISOString();
      const updatedSecret = {
        ...secret,
        accessCount: (secret.accessCount || 0) + 1,
        lastAccessed: now
      };
      this.secrets.set(secret.id, updatedSecret);
      this.logger.info('Secret accessed', { id: secret.id, accessCount: updatedSecret.accessCount });
    }
  }

  async updateAccessMetrics(id: string): Promise<void> {
    // This method is called after logAccess to ensure metrics are updated
    // The actual update is already done in logAccess
    return;
  }

  async deleteAccessLogs(id: string): Promise<void> {
    // In this implementation, we don't maintain separate access logs
    // They are deleted when the secret is deleted
    return;
  }

  async rotateSecrets(): Promise<void> {
    const now = Date.now();
    const rotationPeriod = this.config.rotationPeriod || 30 * 24 * 60 * 60 * 1000; // Default to 30 days
    
    for (const [id, secret] of this.secrets.entries()) {
      const attestationTime = parseInt(secret.teeConfig.attestationToken.split('_')[1] || '0');
      if (now - attestationTime > rotationPeriod) {
        const newConfig = await generateTEEConfig();
        const rotatedSecret = {
          ...secret,
          teeConfig: newConfig
        };
        await this.createSecret(rotatedSecret);
        this.logger.info('Secret rotated', { id });
      }
    }
  }
}