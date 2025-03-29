import { SECRETS_CONSTANTS } from '../constants';
import {
  Secret,
  SecretPermission,
  SecretRotationConfig,
  ValidationError,
  SecretUpdatePayload,
  SecretPermissionUpdatePayload,
  SecretAccessPolicy
} from '../types/types';

export function validateSecret(secret: Partial<Secret>): ValidationError[] {
  const errors: ValidationError[] = [];

  // Validate name
  if (!secret.name || secret.name.trim() === '') {
    errors.push({
      field: 'name',
      message: 'Secret name is required'
    });
  } else if (secret.name && secret.name.length > 100) {
    errors.push({
      field: 'name',
      message: 'Secret name cannot exceed 100 characters'
    });
  }

  if (!Object.values(SECRETS_CONSTANTS.SECRET_TYPES).includes(String(secret.type || ''))) {
    errors.push({
      field: 'type',
      message: 'Invalid secret type'
    });
  }

  // Validate value
  if (secret.type === 'encrypted' && (!secret.value || secret.value.trim() === '')) {
    errors.push({
      field: 'value',
      message: 'Secret value is required for encrypted secrets'
    });
  } else if (secret.value && secret.value.length > SECRETS_CONSTANTS.MAX_SECRET_SIZE) {
    errors.push({
      field: 'value',
      message: `Secret value cannot exceed ${SECRETS_CONSTANTS.MAX_SECRET_SIZE} bytes`
    });
  }

  if (!Object.values(SECRETS_CONSTANTS.ENCRYPTION_ALGORITHMS).includes(String(secret.encryptionAlgorithm || ''))) {
    errors.push({
      field: 'encryptionAlgorithm',
      message: 'Invalid encryption algorithm'
    });
  }

  if (secret.rotationPeriod && !Object.keys(SECRETS_CONSTANTS.ROTATION_PERIODS).includes(String(secret.rotationPeriod || ''))) {
    errors.push({
      field: 'rotationPeriod',
      message: 'Invalid rotation period'
    });
  }

  if (secret.metadata && typeof secret.metadata !== 'object') {
    errors.push({
      field: 'metadata',
      message: 'Metadata must be an object'
    });
  }

  if (secret.tags && (!Array.isArray(secret.tags) || secret.tags.some(tag => typeof tag !== 'string'))) {
    errors.push({
      field: 'tags',
      message: 'Tags must be an array of strings'
    });
  }

  return errors;
}

export function validateSecretPermission(permission: Partial<SecretPermission>): ValidationError[] {
  const errors: ValidationError[] = [];

  if (!permission.secretId?.trim()) {
    errors.push({
      field: 'secretId',
      message: 'Secret ID is required'
    });
  }

  if (!permission.userId?.trim()) {
    errors.push({
      field: 'userId',
      message: 'User ID is required'
    });
  }

  if (permission.level && !Object.values(SECRETS_CONSTANTS.PERMISSION_LEVELS).includes(String(permission.level || ''))) {
    errors.push({
      field: 'level',
      message: 'Invalid permission level'
    });
  }

  if (permission.expiresAt && permission.expiresAt <= Date.now()) {
    errors.push({
      field: 'expiresAt',
      message: 'Expiration date must be in the future'
    });
  }

  return errors;
}

export function validateRotationConfig(config: Partial<SecretRotationConfig>): ValidationError[] {
  const errors: ValidationError[] = [];

  if (typeof config.enabled !== 'boolean') {
    errors.push({
      field: 'enabled',
      message: 'Enabled flag must be a boolean'
    });
  }

  if (config.period && !Object.keys(SECRETS_CONSTANTS.ROTATION_PERIODS).includes(String(config.period || ''))) {
    errors.push({
      field: 'period',
      message: 'Invalid rotation period'
    });
  }

  if (config.strategy && !['manual', 'automatic'].includes(String(config.strategy || ''))) {
    errors.push({
      field: 'strategy',
      message: 'Invalid rotation strategy'
    });
  }

  if (config.notificationThreshold && (config.notificationThreshold <= 0 || config.notificationThreshold > 100)) {
    errors.push({
      field: 'notificationThreshold',
      message: 'Notification threshold must be between 1 and 100'
    });
  }

  return errors;
}

export function validateSecretUpdate(update: SecretUpdatePayload): ValidationError[] {
  const errors: ValidationError[] = [];

  if (update.name !== undefined && (!update.name || update.name.trim() === '')) {
    errors.push({
      field: 'name',
      message: 'Secret name cannot be empty'
    });
  } else if (update.name !== undefined && update.name.length > 100) {
    errors.push({
      field: 'name',
      message: 'Secret name cannot exceed 100 characters'
    });
  }

  // Remove type check for SecretUpdatePayload since it doesn't have a type property
  // if (update.type !== undefined && !Object.values(SECRETS_CONSTANTS.SECRET_TYPES).includes(update.type as string)) {
  //   errors.push({
  //     field: 'type',
  //     message: 'Invalid secret type'
  //   });
  // }

  if (update.value !== undefined && (!update.value || update.value.trim() === '')) {
    errors.push({
      field: 'value',
      message: 'Secret value is required'
    });
  } else if (update.value !== undefined && update.value.length > SECRETS_CONSTANTS.MAX_SECRET_SIZE) {
    errors.push({
      field: 'value',
      message: `Secret value cannot exceed ${SECRETS_CONSTANTS.MAX_SECRET_SIZE} bytes`
    });
  }

  if (update.metadata !== undefined && typeof update.metadata !== 'object') {
    errors.push({
      field: 'metadata',
      message: 'Metadata must be an object'
    });
  }

  if (update.tags !== undefined && (!Array.isArray(update.tags) || update.tags.some(tag => typeof tag !== 'string'))) {
    errors.push({
      field: 'tags',
      message: 'Tags must be an array of strings'
    });
  }

  if (update.rotationPeriod !== undefined && !Object.keys(SECRETS_CONSTANTS.ROTATION_PERIODS).includes(String(update.rotationPeriod || ''))) {
    errors.push({
      field: 'rotationPeriod',
      message: 'Invalid rotation period'
    });
  }

  return errors;
}

export function validatePermissionUpdate(update: SecretPermissionUpdatePayload): ValidationError[] {
  const errors: ValidationError[] = [];

  if (!Object.values(SECRETS_CONSTANTS.PERMISSION_LEVELS).includes(String(update.level || ''))) {
    errors.push({
      field: 'level',
      message: 'Invalid permission level'
    });
  }

  if (update.expiresAt !== undefined && update.expiresAt <= Date.now()) {
    errors.push({
      field: 'expiresAt',
      message: 'Expiration date must be in the future'
    });
  }

  return errors;
}

export function validateAccessPolicy(policy: Partial<SecretAccessPolicy>): ValidationError[] {
  const errors: ValidationError[] = [];

  if (!policy.name?.trim()) {
    errors.push({
      field: 'name',
      message: 'Policy name is required'
    });
  }

  if (policy.conditions?.timeRestrictions) {
    const { timeRestrictions } = policy.conditions;
    
    if (timeRestrictions.startTime && !/^([01]\d|2[0-3]):([0-5]\d)$/.test(timeRestrictions.startTime)) {
      errors.push({
        field: 'conditions.timeRestrictions.startTime',
        message: 'Invalid start time format (HH:mm)'
      });
    }

    if (timeRestrictions.endTime && !/^([01]\d|2[0-3]):([0-5]\d)$/.test(timeRestrictions.endTime)) {
      errors.push({
        field: 'conditions.timeRestrictions.endTime',
        message: 'Invalid end time format (HH:mm)'
      });
    }

    if (timeRestrictions.daysOfWeek && (!Array.isArray(timeRestrictions.daysOfWeek) || 
        timeRestrictions.daysOfWeek.some(day => day < 0 || day > 6))) {
      errors.push({
        field: 'conditions.timeRestrictions.daysOfWeek',
        message: 'Days of week must be an array of numbers 0-6'
      });
    }
  }

  if (policy.conditions?.ipRestrictions) {
    const ipRegex = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(?:\/(?:3[0-2]|[1-2][0-9]|[0-9]))?$/;
    if (!policy.conditions.ipRestrictions.every(ip => ipRegex.test(ip))) {
      errors.push({
        field: 'conditions.ipRestrictions',
        message: 'Invalid IP address or CIDR format'
      });
    }
  }

  if (policy.conditions?.approvers && (!Array.isArray(policy.conditions.approvers) || 
      policy.conditions.approvers.some(approver => !approver.trim()))) {
    errors.push({
      field: 'conditions.approvers',
      message: 'Approvers must be an array of non-empty strings'
    });
  }

  return errors;
}