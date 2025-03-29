/**
 * Types for the functions service
 */

export interface UserFunction {
  id: string;
  name: string;
  description: string;
  owner: string;
  code: string;
  language: ProgrammingLanguage;
  runtime: RuntimeEnvironment;
  version: string;
  status: FunctionStatus;
  createdAt: number;
  updatedAt: number;
  lastDeployed?: number;
  lastExecuted?: number;
  metrics: FunctionMetrics;
  config: FunctionConfig;
  permissions: FunctionPermissions;
  triggers: FunctionTrigger[];
}

export type ProgrammingLanguage = 
  | 'javascript'
  | 'typescript'
  | 'python'
  | 'go'
  | 'rust';

export type RuntimeEnvironment = {
  name: string;
  version: string;
  features: string[];
  memoryLimit: number;
  timeoutSeconds: number;
  concurrency: number;
};

export type FunctionStatus = 
  | 'draft'
  | 'deployed'
  | 'failed'
  | 'disabled'
  | 'deprecated';

export interface FunctionMetrics {
  totalExecutions: number;
  successRate: number;
  averageExecutionTime: number;
  lastExecutionTime?: number;
  gasUsed: number;
  errors: {
    count: number;
    lastError?: string;
    lastErrorTime?: number;
  };
  resourceUsage: {
    cpu: number;
    memory: number;
    storage: number;
  };
}

export interface FunctionConfig {
  environment: Record<string, string>;
  secrets: string[];
  dependencies: {
    name: string;
    version: string;
  }[];
  memoryLimit: number;
  timeoutSeconds: number;
  maxConcurrency: number;
  retryPolicy: {
    maxAttempts: number;
    initialDelay: number;
    maxDelay: number;
    multiplier: number;
  };
}

export interface FunctionPermissions {
  allowedContracts: string[];
  allowedAPIs: string[];
  allowedSecrets: string[];
  maxGasPerExecution: number;
  roles: string[];
}

export interface FunctionTrigger {
  id: string;
  type: TriggerType;
  name: string;
  enabled: boolean;
  config: TriggerConfig;
  lastTriggered?: number;
  nextTrigger?: number;
}

export type TriggerType = 
  | 'schedule'
  | 'event'
  | 'http'
  | 'contract'
  | 'oracle';

export type TriggerConfig = {
  schedule?: {
    cron: string;
    timezone: string;
  };
  event?: {
    contractHash: string;
    eventName: string;
    filters: Record<string, any>;
  };
  http?: {
    method: string;
    path: string;
    auth: boolean;
  };
  contract?: {
    contractHash: string;
    method: string;
    parameters: any[];
    frequency: number;
  };
  oracle?: {
    dataSource: string;
    query: string;
    frequency: number;
  };
};

export interface FunctionExecution {
  id: string;
  functionId: string;
  triggerId?: string;
  status: ExecutionStatus;
  startTime: number;
  endTime?: number;
  duration?: number;
  result?: any;
  error?: string;
  logs: string[];
  gasUsed: number;
  metrics: {
    cpu: number;
    memory: number;
    storage: number;
  };
}

export type ExecutionStatus = 
  | 'pending'
  | 'running'
  | 'completed'
  | 'failed'
  | 'timeout'
  | 'cancelled';

export interface FunctionTemplate {
  id: string;
  name: string;
  description: string;
  language: ProgrammingLanguage;
  code: string;
  config: Partial<FunctionConfig>;
  category: string;
  tags: string[];
}