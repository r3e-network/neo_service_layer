/**
 * Logging utilities for the Neo Service Layer
 * 
 * This module provides a standardized logging interface for the application.
 */

/**
 * Log levels in order of severity
 */
export enum LogLevel {
  DEBUG = 'debug',
  INFO = 'info',
  WARN = 'warn',
  ERROR = 'error',
  FATAL = 'fatal'
}

/**
 * Log entry structure
 */
export interface LogEntry {
  timestamp: number;
  level: LogLevel;
  message: string;
  context?: Record<string, any>;
  source?: string;
}

/**
 * Logger configuration options
 */
export interface LoggerOptions {
  minLevel?: LogLevel;
  enableConsole?: boolean;
  enableRemote?: boolean;
  remoteEndpoint?: string;
  applicationName?: string;
  environment?: string;
}

/**
 * Default logger configuration
 */
const DEFAULT_OPTIONS: LoggerOptions = {
  minLevel: LogLevel.INFO,
  enableConsole: true,
  enableRemote: false,
  applicationName: 'neo-service-layer',
  environment: process.env.NODE_ENV || 'development'
};

/**
 * Logger class for consistent logging across the application
 */
class Logger {
  private options: LoggerOptions;
  private queue: LogEntry[] = [];
  private flushInterval: NodeJS.Timeout | null = null;

  constructor(options: LoggerOptions = {}) {
    this.options = { ...DEFAULT_OPTIONS, ...options };
    
    if (this.options.enableRemote) {
      this.startFlushInterval();
    }
  }

  /**
   * Log a message at the specified level
   */
  private log(level: LogLevel, message: string, context?: Record<string, any>, source?: string): void {
    // Skip if below minimum level
    if (this.getLevelValue(level) < this.getLevelValue(this.options.minLevel!)) {
      return;
    }

    const entry: LogEntry = {
      timestamp: Date.now(),
      level,
      message,
      context,
      source
    };

    // Log to console if enabled
    if (this.options.enableConsole) {
      this.logToConsole(entry);
    }

    // Queue for remote logging if enabled
    if (this.options.enableRemote) {
      this.queue.push(entry);
    }
  }

  /**
   * Log to console with appropriate formatting
   */
  private logToConsole(entry: LogEntry): void {
    const timestamp = new Date(entry.timestamp).toISOString();
    const prefix = `[${timestamp}] [${entry.level.toUpperCase()}]`;
    const source = entry.source ? ` [${entry.source}]` : '';
    const message = `${prefix}${source}: ${entry.message}`;
    
    switch (entry.level) {
      case LogLevel.DEBUG:
        console.debug(message, entry.context || '');
        break;
      case LogLevel.INFO:
        console.info(message, entry.context || '');
        break;
      case LogLevel.WARN:
        console.warn(message, entry.context || '');
        break;
      case LogLevel.ERROR:
      case LogLevel.FATAL:
        console.error(message, entry.context || '');
        break;
    }
  }

  /**
   * Get numeric value for log level for comparison
   */
  private getLevelValue(level: LogLevel): number {
    const levels = {
      [LogLevel.DEBUG]: 0,
      [LogLevel.INFO]: 1,
      [LogLevel.WARN]: 2,
      [LogLevel.ERROR]: 3,
      [LogLevel.FATAL]: 4
    };
    return levels[level] || 0;
  }

  /**
   * Start interval to flush logs to remote endpoint
   */
  private startFlushInterval(): void {
    if (this.flushInterval) {
      clearInterval(this.flushInterval);
    }
    
    this.flushInterval = setInterval(() => this.flushLogs(), 5000);
  }

  /**
   * Flush queued logs to remote endpoint
   */
  private async flushLogs(): Promise<void> {
    if (!this.queue.length) return;
    
    const logs = [...this.queue];
    this.queue = [];
    
    if (!this.options.remoteEndpoint) return;
    
    try {
      const response = await fetch(this.options.remoteEndpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          application: this.options.applicationName,
          environment: this.options.environment,
          logs
        })
      });
      
      if (!response.ok) {
        console.error(`Failed to send logs to remote endpoint: ${response.statusText}`);
        // Put logs back in queue
        this.queue = [...logs, ...this.queue];
      }
    } catch (error) {
      console.error(`Error sending logs to remote endpoint: ${error}`);
      // Put logs back in queue
      this.queue = [...logs, ...this.queue];
    }
  }

  /**
   * Log a debug message
   */
  debug(message: string, context?: Record<string, any>, source?: string): void {
    this.log(LogLevel.DEBUG, message, context, source);
  }

  /**
   * Log an info message
   */
  info(message: string, context?: Record<string, any>, source?: string): void {
    this.log(LogLevel.INFO, message, context, source);
  }

  /**
   * Log a warning message
   */
  warn(message: string, context?: Record<string, any>, source?: string): void {
    this.log(LogLevel.WARN, message, context, source);
  }

  /**
   * Log an error message
   */
  error(message: string, context?: Record<string, any>, source?: string): void {
    this.log(LogLevel.ERROR, message, context, source);
  }

  /**
   * Log a fatal error message
   */
  fatal(message: string, context?: Record<string, any>, source?: string): void {
    this.log(LogLevel.FATAL, message, context, source);
  }

  /**
   * Update logger configuration
   */
  configure(options: LoggerOptions): void {
    this.options = { ...this.options, ...options };
    
    if (this.options.enableRemote && !this.flushInterval) {
      this.startFlushInterval();
    } else if (!this.options.enableRemote && this.flushInterval) {
      clearInterval(this.flushInterval);
      this.flushInterval = null;
    }
  }

  /**
   * Manually flush logs to remote endpoint
   */
  async flush(): Promise<void> {
    return this.flushLogs();
  }
}

/**
 * Default logger instance
 */
export const logger = new Logger();

/**
 * Create a new logger with custom configuration
 */
export function createLogger(options: LoggerOptions): Logger {
  return new Logger(options);
}