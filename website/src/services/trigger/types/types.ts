import { TRIGGER_CONSTANTS } from '../constants';

export type EventType = typeof TRIGGER_CONSTANTS.EVENT_TYPES[keyof typeof TRIGGER_CONSTANTS.EVENT_TYPES];
export type TriggerStatus = typeof TRIGGER_CONSTANTS.TRIGGER_STATUS[keyof typeof TRIGGER_CONSTANTS.TRIGGER_STATUS];

export interface TriggerCondition {
  type: EventType;
  parameters: {
    // Block conditions
    blockHeight?: number;
    blockInterval?: number;
    
    // Transaction conditions
    sender?: string;
    recipient?: string;
    asset?: string;
    amount?: number;
    
    // Contract conditions
    contractHash?: string;
    eventName?: string;
    
    // Price conditions
    symbol?: string;
    threshold?: number;
    operator?: '>' | '<' | '==' | '>=' | '<=';
    
    // Time conditions
    cronExpression?: string;
    interval?: number;
    startTime?: number;
    endTime?: number;
  };
}

export interface TriggerAction {
  type: 'contract_call' | 'http_request' | 'function_execution';
  parameters: {
    // Contract call parameters
    contractHash?: string;
    method?: string;
    args?: any[];
    
    // HTTP request parameters
    url?: string;
    httpMethod?: 'GET' | 'POST' | 'PUT' | 'DELETE';
    headers?: Record<string, string>;
    body?: any;
    
    // Function execution parameters
    functionId?: string;
    input?: any;
  };
}

export interface Trigger {
  id: string;
  name: string;
  description?: string;
  owner: string;
  condition: TriggerCondition;
  action: TriggerAction;
  status: TriggerStatus;
  createdAt: number;
  updatedAt: number;
  lastExecutedAt?: number;
  nextExecutionAt?: number;
  executionCount: number;
  failureCount: number;
  retryCount: number;
  timeout: number;
  tags?: string[];
}

export interface TriggerExecution {
  id: string;
  triggerId: string;
  status: 'success' | 'failure' | 'pending';
  startTime: number;
  endTime?: number;
  result?: any;
  error?: string;
  retryCount: number;
}

export interface TriggerFilter {
  type?: EventType[];
  status?: TriggerStatus[];
  tags?: string[];
  search?: string;
  createdAfter?: number;
  createdBefore?: number;
}

export interface TriggerMetrics {
  total: number;
  active: number;
  failed: number;
  completed: number;
  executionsLast24h: number;
  failuresLast24h: number;
  averageExecutionTime: number;
  successRate: number;
}

export interface TriggerUpdatePayload {
  name?: string;
  description?: string;
  condition?: TriggerCondition;
  action?: TriggerAction;
  timeout?: number;
  tags?: string[];
}

export interface ValidationError {
  field: string;
  message: string;
}

export interface TriggerExecutionLog {
  id: string;
  executionId: string;
  timestamp: number;
  level: 'info' | 'warning' | 'error';
  message: string;
  metadata?: Record<string, any>;
}

export interface TriggerPermission {
  triggerId: string;
  userId: string;
  level: 'read' | 'write' | 'admin';
  grantedAt: number;
  grantedBy: string;
  expiresAt?: number;
}