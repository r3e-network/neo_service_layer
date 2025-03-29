// @ts-ignore
import * as crypto from 'crypto';
import { EncryptionAlgorithm, EncryptionOptions, EncryptedData } from '../types';
import { SECRETS_CONSTANTS } from '../constants';

const IV_LENGTH = 16;
const SALT_LENGTH = 32;
const KEY_LENGTH = 32;
const TAG_LENGTH = 16;

export class EncryptionService {
  private static instance: EncryptionService;
  private masterKey: Buffer;
  private algorithm: EncryptionAlgorithm;

  private constructor(masterKey: string, algorithm: EncryptionAlgorithm = EncryptionAlgorithm.AES256) {
    this.masterKey = Buffer.from(masterKey, 'hex');
    this.algorithm = algorithm;
  }

  public static getInstance(masterKey?: string, algorithm?: EncryptionAlgorithm): EncryptionService {
    if (!EncryptionService.instance) {
      if (!masterKey) {
        throw new Error('Master key is required for initialization');
      }
      EncryptionService.instance = new EncryptionService(masterKey, algorithm);
    }
    return EncryptionService.instance;
  }

  private deriveKey(salt: Buffer): Buffer {
    return crypto.createHash('sha256')
      .update(Buffer.concat([this.masterKey, salt]))
      .digest();
  }

  /**
   * Encrypts data using the configured algorithm
   */
  public async encrypt(
    data: string,
    options?: EncryptionOptions
  ): Promise<string> {
    try {
      const salt = crypto.randomBytes(SALT_LENGTH);
      const iv = crypto.randomBytes(IV_LENGTH);
      const key = this.deriveKey(salt);

      const cipher = crypto.createCipheriv(this.algorithm, key, iv, {
        authTagLength: TAG_LENGTH
      } as crypto.CipherGCMOptions);

      const encrypted = Buffer.concat([
        cipher.update(data, 'utf8'),
        cipher.final()
      ]);

      const authTag = (cipher as any).getAuthTag();

      // Format: salt:iv:authTag:encryptedData
      return Buffer.concat([
        salt,
        iv,
        authTag,
        encrypted
      ]).toString('base64');
    } catch (error) {
      throw new Error(`Encryption failed: ${error.message}`);
    }
  }

  public async decrypt(
    ciphertext: string,
    algorithm: EncryptionAlgorithm = EncryptionAlgorithm.AES256
  ): Promise<string> {
    try {
      const data = Buffer.from(ciphertext, 'base64');

      const salt = data.slice(0, SALT_LENGTH);
      const iv = data.slice(SALT_LENGTH, SALT_LENGTH + IV_LENGTH);
      const authTag = data.slice(SALT_LENGTH + IV_LENGTH, SALT_LENGTH + IV_LENGTH + TAG_LENGTH);
      const encrypted = data.slice(SALT_LENGTH + IV_LENGTH + TAG_LENGTH);

      const key = this.deriveKey(salt);

      const decipher = crypto.createDecipheriv(algorithm, key, iv, {
        authTagLength: TAG_LENGTH
      } as crypto.CipherGCMOptions);
      (decipher as any).setAuthTag(authTag);

      const decrypted = Buffer.concat([
        decipher.update(encrypted),
        decipher.final()
      ]);

      return decrypted.toString('utf8');
    } catch (error) {
      throw new Error(`Decryption failed: ${error.message}`);
    }
  }

  public async rotateKey(oldMasterKey: string, newMasterKey: string): Promise<void> {
    const oldInstance = new EncryptionService(oldMasterKey);
    const newInstance = new EncryptionService(newMasterKey);

    // In a real implementation, you would:
    // 1. Fetch all encrypted secrets
    // 2. Decrypt with old key
    // 3. Encrypt with new key
    // 4. Update storage atomically
    throw new Error('Key rotation must be implemented based on your storage solution');
  }

  public generateSecretKey(): string {
    return crypto.randomBytes(KEY_LENGTH).toString('hex');
  }

  public hashSecret(secret: string): string {
    return crypto.createHash('sha256')
      .update(secret)
      .digest('hex');
  }

  public async verifySecret(secret: string, hash: string): Promise<boolean> {
    const computedHash = this.hashSecret(secret);
    return computedHash === hash;
  }
}