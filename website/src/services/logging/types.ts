/**
 * Types for the logging service
 */

export interface LogEntry {
  id: string;
  timestamp: number;
  level: LogLevel;
  message: string;
  source: ServiceName;
  context?: Record<string, any>;
  tags?: string[];
}

export interface LogFilter {
  startTime?: number;
  endTime?: number;
  levels?: LogLevel[];
  sources?: ServiceName[];
  tags?: string[];
  search?: string;
  limit?: number;
  offset?: number;
}

export interface LogStats {
  totalLogs: number;
  logsByLevel: Record<LogLevel, number>;
  logsBySource: Record<ServiceName, number>;
  topTags: { tag: string; count: number }[];
  timeDistribution: { timestamp: number; count: number }[];
}

export interface LogExportOptions {
  filter: LogFilter;
  format: 'json' | 'csv' | 'txt';
}

export interface LogStreamOptions {
  filter: LogFilter;
  realtime: boolean;
}

export type LogLevel = 'debug' | 'info' | 'warn' | 'error' | 'critical';

export type ServiceName = 
  | 'priceFeed'
  | 'gasBank'
  | 'trigger'
  | 'functions'
  | 'secrets'
  | 'api'
  | 'system';
