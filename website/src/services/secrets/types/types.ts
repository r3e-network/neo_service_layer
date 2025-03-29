import { SECRETS_CONSTANTS } from '../constants';

export type SecretType = typeof SECRETS_CONSTANTS.SECRET_TYPES[keyof typeof SECRETS_CONSTANTS.SECRET_TYPES];
export type PermissionLevel = typeof SECRETS_CONSTANTS.PERMISSION_LEVELS[keyof typeof SECRETS_CONSTANTS.PERMISSION_LEVELS];
export type EncryptionAlgorithm = typeof SECRETS_CONSTANTS.ENCRYPTION_ALGORITHMS[keyof typeof SECRETS_CONSTANTS.ENCRYPTION_ALGORITHMS];

export interface Secret {
  id: string;
  name: string;
  description?: string;
  type: SecretType;
  value: string;
  encryptedValue: string;
  encryptionAlgorithm: EncryptionAlgorithm;
  createdAt: number;
  updatedAt: number;
  lastRotatedAt: number;
  rotationPeriod?: number;
  metadata?: Record<string, string>;
  tags?: string[];
  owner: string;
  version: number;
}

export interface SecretPermission {
  secretId: string;
  userId: string;
  level: PermissionLevel;
  grantedAt: number;
  grantedBy: string;
  expiresAt?: number;
}

export interface SecretRotationConfig {
  enabled: boolean;
  period: number;
  lastRotation: number;
  nextRotation: number;
  strategy: 'manual' | 'automatic';
  notificationThreshold: number;
}

export interface SecretMetrics {
  totalSecrets: number;
  secretsByType: Record<SecretType, number>;
  rotationStats: {
    upToDate: number;
    needsRotation: number;
    expired: number;
  };
  accessStats: {
    totalAccesses: number;
    uniqueUsers: number;
    averageAccessesPerDay: number;
  };
}

export interface SecretFilter {
  type?: SecretType[];
  tags?: string[];
  rotationStatus?: 'upToDate' | 'needsRotation' | 'expired';
  createdAfter?: number;
  createdBefore?: number;
  search?: string;
}

export interface SecretUpdatePayload {
  name?: string;
  description?: string;
  value?: string;
  metadata?: Record<string, string>;
  tags?: string[];
  rotationPeriod?: number;
}

export interface SecretPermissionUpdatePayload {
  level: PermissionLevel;
  expiresAt?: number;
}

export interface SecretRotationResult {
  success: boolean;
  newVersion: number;
  oldValue: string;
  newValue: string;
  rotatedAt: number;
  error?: string;
}

export interface SecretAuditLog {
  id: string;
  secretId: string;
  action: 'create' | 'read' | 'update' | 'delete' | 'rotate' | 'grant' | 'revoke';
  performedBy: string;
  performedAt: number;
  details: Record<string, any>;
  metadata: {
    ipAddress?: string;
    userAgent?: string;
    location?: string;
  };
}

export interface ValidationError {
  field: string;
  message: string;
}

export interface SecretEncryptionKey {
  id: string;
  publicKey: string;
  privateKey?: string;
  algorithm: EncryptionAlgorithm;
  createdAt: number;
  expiresAt?: number;
  status: 'active' | 'expired' | 'revoked';
}

export interface SecretAccessPolicy {
  id: string;
  name: string;
  description?: string;
  conditions: {
    timeRestrictions?: {
      startTime?: string;
      endTime?: string;
      daysOfWeek?: number[];
    };
    ipRestrictions?: string[];
    mfaRequired?: boolean;
    approvalRequired?: boolean;
    approvers?: string[];
  };
  createdAt: number;
  updatedAt: number;
  createdBy: string;
}