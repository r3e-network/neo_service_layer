// @ts-ignore
import * as React from 'react';
import { LogEntry, LogFilter, LogStats, LogStreamOptions } from '../types/types';
import { loggingApi } from '../api/loggingApi';
import { DEFAULT_LOG_LIMIT } from '../constants';

export function useLogs() {
  const [logs, setLogs] = React.useState<LogEntry[]>([]);
  const [totalLogs, setTotalLogs] = React.useState(0);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<Error | null>(null);
  const [stats, setStats] = React.useState<LogStats | null>(null);
  const [filter, setFilter] = React.useState<LogFilter>({
    limit: DEFAULT_LOG_LIMIT,
    offset: 0
  });
  const streamCleanupRef = React.useRef<(() => void) | null>(null);

  const fetchLogs = React.useCallback(async (newFilter?: LogFilter) => {
    try {
      setLoading(true);
      const currentFilter = newFilter || filter;
      const { logs: newLogs, total } = await loggingApi.getLogs(currentFilter);
      setLogs(newLogs);
      setTotalLogs(total);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch logs'));
    } finally {
      setLoading(false);
    }
  }, [filter]);

  const fetchStats = React.useCallback(async (timeRange?: { startTime: number; endTime: number }) => {
    try {
      const newStats = await loggingApi.getLogStats(timeRange);
      setStats(newStats);
    } catch (err) {
      console.error('Failed to fetch log statistics:', err);
    }
  }, []);

  React.useEffect(() => {
    fetchLogs();
    fetchStats();
  }, [fetchLogs, fetchStats]);

  const updateFilter = React.useCallback((newFilter: Partial<LogFilter>) => {
    setFilter(prev => ({ ...prev, ...newFilter, offset: 0 }));
  }, []);

  const exportLogs = React.useCallback(async (format: 'json' | 'csv') => {
    try {
      const blob = await loggingApi.exportLogs({
        ...filter,
        format,
        includeMetadata: true,
        includeStackTrace: true
      });
      
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `logs_export_${new Date().toISOString()}.${format}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to export logs'));
    }
  }, [filter]);

  const getRelatedLogs = React.useCallback(async (correlationId: string): Promise<LogEntry[]> => {
    try {
      return await loggingApi.getRelatedLogs(correlationId);
    } catch (err) {
      console.error('Failed to fetch related logs:', err);
      return [];
    }
  }, []);

  const startLogStream = React.useCallback((options: LogStreamOptions) => {
    // Clean up existing stream if any
    if (streamCleanupRef.current) {
      streamCleanupRef.current();
    }

    const cleanup = loggingApi.streamLogs(options, (newLog) => {
      setLogs(prev => [newLog, ...prev].slice(0, filter.limit || DEFAULT_LOG_LIMIT));
      setTotalLogs(prev => prev + 1);
    });

    streamCleanupRef.current = cleanup;
  }, [filter.limit]);

  const stopLogStream = React.useCallback(() => {
    if (streamCleanupRef.current) {
      streamCleanupRef.current();
      streamCleanupRef.current = null;
    }
  }, []);

  React.useEffect(() => {
    return () => {
      if (streamCleanupRef.current) {
        streamCleanupRef.current();
      }
    };
  }, []);

  return {
    logs,
    totalLogs,
    loading,
    error,
    stats,
    filter,
    updateFilter,
    fetchLogs,
    fetchStats,
    exportLogs,
    getRelatedLogs,
    startLogStream,
    stopLogStream
  };
}