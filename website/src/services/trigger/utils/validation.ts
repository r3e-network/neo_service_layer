import { TRIGGER_CONSTANTS } from '../constants';
import {
  Trigger,
  TriggerCondition,
  TriggerAction,
  ValidationError,
  TriggerUpdatePayload
} from '../types/types';
import { isValidCronExpression } from './cronValidation';

export function validateTriggerCondition(condition: TriggerCondition): ValidationError[] {
  const errors: ValidationError[] = [];

  if (!Object.values(TRIGGER_CONSTANTS.EVENT_TYPES).includes(condition.type)) {
    errors.push({
      field: 'condition.type',
      message: 'Invalid event type'
    });
  }

  switch (condition.type) {
    case 'block':
      if (condition.parameters.blockInterval && 
          (condition.parameters.blockInterval < 1 || !Number.isInteger(condition.parameters.blockInterval))) {
        errors.push({
          field: 'condition.parameters.blockInterval',
          message: 'Block interval must be a positive integer'
        });
      }
      break;

    case 'transaction':
      if (condition.parameters.sender && !/^[a-zA-Z0-9]{34}$/.test(condition.parameters.sender)) {
        errors.push({
          field: 'condition.parameters.sender',
          message: 'Invalid sender address'
        });
      }
      if (condition.parameters.recipient && !/^[a-zA-Z0-9]{34}$/.test(condition.parameters.recipient)) {
        errors.push({
          field: 'condition.parameters.recipient',
          message: 'Invalid recipient address'
        });
      }
      if (condition.parameters.amount && condition.parameters.amount <= 0) {
        errors.push({
          field: 'condition.parameters.amount',
          message: 'Amount must be greater than 0'
        });
      }
      break;

    case 'contract':
      if (!condition.parameters.contractHash || !/^0x[a-fA-F0-9]{40}$/.test(condition.parameters.contractHash)) {
        errors.push({
          field: 'condition.parameters.contractHash',
          message: 'Invalid contract hash'
        });
      }
      if (!condition.parameters.eventName) {
        errors.push({
          field: 'condition.parameters.eventName',
          message: 'Event name is required'
        });
      }
      break;

    case 'price':
      if (!condition.parameters.symbol) {
        errors.push({
          field: 'condition.parameters.symbol',
          message: 'Symbol is required'
        });
      }
      if (condition.parameters.threshold === undefined || condition.parameters.threshold <= 0) {
        errors.push({
          field: 'condition.parameters.threshold',
          message: 'Threshold must be greater than 0'
        });
      }
      if (!condition.parameters.operator || !['>', '<', '==', '>=', '<='].includes(condition.parameters.operator)) {
        errors.push({
          field: 'condition.parameters.operator',
          message: 'Invalid operator'
        });
      }
      break;

    case 'time':
      if (condition.parameters.cronExpression && !isValidCronExpression(condition.parameters.cronExpression)) {
        errors.push({
          field: 'condition.parameters.cronExpression',
          message: 'Invalid cron expression'
        });
      }
      if (condition.parameters.interval) {
        if (condition.parameters.interval < TRIGGER_CONSTANTS.MIN_INTERVAL) {
          errors.push({
            field: 'condition.parameters.interval',
            message: `Interval must be at least ${TRIGGER_CONSTANTS.MIN_INTERVAL} seconds`
          });
        }
        if (condition.parameters.interval > TRIGGER_CONSTANTS.MAX_INTERVAL) {
          errors.push({
            field: 'condition.parameters.interval',
            message: `Interval must not exceed ${TRIGGER_CONSTANTS.MAX_INTERVAL} seconds`
          });
        }
      }
      break;
  }

  return errors;
}

export function validateTriggerAction(action: TriggerAction): ValidationError[] {
  const errors: ValidationError[] = [];

  if (!['contract_call', 'http_request', 'function_execution'].includes(action.type)) {
    errors.push({
      field: 'action.type',
      message: 'Invalid action type'
    });
  }

  switch (action.type) {
    case 'contract_call':
      if (!action.parameters.contractHash || !/^0x[a-fA-F0-9]{40}$/.test(action.parameters.contractHash)) {
        errors.push({
          field: 'action.parameters.contractHash',
          message: 'Invalid contract hash'
        });
      }
      if (!action.parameters.method) {
        errors.push({
          field: 'action.parameters.method',
          message: 'Method name is required'
        });
      }
      break;

    case 'http_request':
      if (!action.parameters.url || !isValidUrl(action.parameters.url)) {
        errors.push({
          field: 'action.parameters.url',
          message: 'Invalid URL'
        });
      }
      if (!['GET', 'POST', 'PUT', 'DELETE'].includes(action.parameters.httpMethod as string)) {
        errors.push({
          field: 'action.parameters.httpMethod',
          message: 'Invalid HTTP method'
        });
      }
      break;

    case 'function_execution':
      if (!action.parameters.functionId) {
        errors.push({
          field: 'action.parameters.functionId',
          message: 'Function ID is required'
        });
      }
      break;
  }

  return errors;
}

export function validateTrigger(trigger: Partial<Trigger>): ValidationError[] {
  const errors: ValidationError[] = [];

  if (!trigger.name?.trim()) {
    errors.push({
      field: 'name',
      message: 'Name is required'
    });
  }

  if (!trigger.condition) {
    errors.push({
      field: 'condition',
      message: 'Condition is required'
    });
  } else {
    errors.push(...validateTriggerCondition(trigger.condition));
  }

  if (!trigger.action) {
    errors.push({
      field: 'action',
      message: 'Action is required'
    });
  } else {
    errors.push(...validateTriggerAction(trigger.action));
  }

  if (trigger.timeout && (trigger.timeout < 0 || trigger.timeout > TRIGGER_CONSTANTS.DEFAULT_TIMEOUT * 2)) {
    errors.push({
      field: 'timeout',
      message: `Timeout must be between 0 and ${TRIGGER_CONSTANTS.DEFAULT_TIMEOUT * 2} milliseconds`
    });
  }

  if (trigger.tags && (!Array.isArray(trigger.tags) || trigger.tags.some(tag => typeof tag !== 'string'))) {
    errors.push({
      field: 'tags',
      message: 'Tags must be an array of strings'
    });
  }

  return errors;
}

export function validateTriggerUpdate(update: TriggerUpdatePayload): ValidationError[] {
  const errors: ValidationError[] = [];

  if (update.name !== undefined && !update.name.trim()) {
    errors.push({
      field: 'name',
      message: 'Name cannot be empty'
    });
  }

  if (update.condition) {
    errors.push(...validateTriggerCondition(update.condition));
  }

  if (update.action) {
    errors.push(...validateTriggerAction(update.action));
  }

  if (update.timeout !== undefined && (update.timeout < 0 || update.timeout > TRIGGER_CONSTANTS.DEFAULT_TIMEOUT * 2)) {
    errors.push({
      field: 'timeout',
      message: `Timeout must be between 0 and ${TRIGGER_CONSTANTS.DEFAULT_TIMEOUT * 2} milliseconds`
    });
  }

  if (update.tags !== undefined && (!Array.isArray(update.tags) || update.tags.some(tag => typeof tag !== 'string'))) {
    errors.push({
      field: 'tags',
      message: 'Tags must be an array of strings'
    });
  }

  return errors;
}

function isValidUrl(url: string): boolean {
  try {
    new URL(url);
    return true;
  } catch {
    return false;
  }
}