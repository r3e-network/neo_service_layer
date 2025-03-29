import { LogEntry, LogFilter, LogStats, LogExportOptions, LogStreamOptions } from '../types/types';
import { API_BASE_URL } from '../constants';

/**
 * Logging API client for interacting with the logging service
 */
class LoggingApi {
  private baseUrl: string;

  constructor() {
    this.baseUrl = `${API_BASE_URL}/logging`;
  }

  /**
   * Fetch logs based on filter criteria
   */
  async getLogs(filter: LogFilter): Promise<{
    logs: LogEntry[];
    total: number;
  }> {
    const params = new URLSearchParams();
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== undefined) {
        if (Array.isArray(value)) {
          value.forEach(v => params.append(key, v));
        } else {
          params.append(key, value.toString());
        }
      }
    });

    const response = await fetch(`${this.baseUrl}/logs?${params.toString()}`);
    if (!response.ok) {
      throw new Error('Failed to fetch logs');
    }
    return response.json();
  }

  /**
   * Get log statistics
   */
  async getLogStats(timeRange?: { startTime: number; endTime: number }): Promise<LogStats> {
    const params = new URLSearchParams();
    if (timeRange) {
      params.append('startTime', timeRange.startTime.toString());
      params.append('endTime', timeRange.endTime.toString());
    }

    const response = await fetch(`${this.baseUrl}/stats?${params.toString()}`);
    if (!response.ok) {
      throw new Error('Failed to fetch log statistics');
    }
    return response.json();
  }

  /**
   * Export logs based on filter criteria
   */
  async exportLogs(options: LogExportOptions): Promise<Blob> {
    const params = new URLSearchParams();
    Object.entries(options).forEach(([key, value]) => {
      if (value !== undefined) {
        if (Array.isArray(value)) {
          value.forEach(v => params.append(key, v));
        } else {
          params.append(key, value.toString());
        }
      }
    });

    const response = await fetch(`${this.baseUrl}/export?${params.toString()}`);
    if (!response.ok) {
      throw new Error('Failed to export logs');
    }
    return response.blob();
  }

  /**
   * Get a single log entry by ID
   */
  async getLogById(id: string): Promise<LogEntry> {
    const response = await fetch(`${this.baseUrl}/logs/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch log entry');
    }
    return response.json();
  }

  /**
   * Get related logs by correlation ID
   */
  async getRelatedLogs(correlationId: string): Promise<LogEntry[]> {
    const response = await fetch(`${this.baseUrl}/logs/correlated/${correlationId}`);
    if (!response.ok) {
      throw new Error('Failed to fetch related logs');
    }
    return response.json();
  }

  /**
   * Stream logs in real-time
   */
  streamLogs(options: LogStreamOptions, onMessage: (log: LogEntry) => void): () => void {
    const params = new URLSearchParams();
    Object.entries(options).forEach(([key, value]) => {
      if (value !== undefined) {
        if (Array.isArray(value)) {
          value.forEach(v => params.append(key, v));
        } else {
          params.append(key, value.toString());
        }
      }
    });

    const eventSource = new EventSource(
      `${this.baseUrl}/stream?${params.toString()}`
    );

    eventSource.onmessage = (event) => {
      const log = JSON.parse(event.data);
      onMessage(log);
    };

    eventSource.onerror = (error) => {
      console.error('Log stream error:', error);
      eventSource.close();
    };

    // Return cleanup function
    return () => eventSource.close();
  }
}

export const loggingApi = new LoggingApi();