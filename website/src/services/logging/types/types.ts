/**
 * Types for the logging service
 */

export type LogLevel = 'debug' | 'info' | 'warn' | 'error' | 'critical';

export type ServiceName = 
  | 'priceFeed'
  | 'gasBank'
  | 'trigger'
  | 'functions'
  | 'secrets'
  | 'api'
  | 'system';

export interface LogEntry {
  id: string;
  timestamp: number;
  level: LogLevel;
  service: ServiceName;
  message: string;
  metadata: Record<string, any>;
  correlationId?: string;
  userId?: string;
  requestId?: string;
  stackTrace?: string;
}

export interface LogFilter {
  level?: LogLevel[];
  service?: ServiceName[];
  startTime?: number;
  endTime?: number;
  searchTerm?: string;
  correlationId?: string;
  userId?: string;
  requestId?: string;
  limit?: number;
  offset?: number;
}

export interface LogStats {
  totalLogs: number;
  errorCount: number;
  warningCount: number;
  serviceBreakdown: Record<ServiceName, number>;
  levelBreakdown: Record<LogLevel, number>;
  timeRangeStats: {
    startTime: number;
    endTime: number;
    logsPerMinute: number;
    errorRate: number;
  };
}

export interface LogStreamOptions {
  level?: LogLevel[];
  service?: ServiceName[];
  correlationId?: string;
  userId?: string;
  requestId?: string;
}

export interface LogExportOptions extends LogFilter {
  format: 'json' | 'csv';
  includeMetadata: boolean;
  includeStackTrace: boolean;
}