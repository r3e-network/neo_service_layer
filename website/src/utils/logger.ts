/**
 * Logger Utility
 * 
 * This module provides logging functionality for the Neo Service Layer website.
 * It configures Winston for structured logging with different log levels and
 * provides both a direct Winston logger instance and a class-based interface.
 */

import winston from 'winston';

// Define log levels
const levels = {
  error: 0,
  warn: 1,
  info: 2,
  http: 3,
  debug: 4,
};

// Define log level based on environment
const level = () => {
  const env = process.env.NODE_ENV || 'development';
  return env === 'development' ? 'debug' : 'info';
};

// Define colors for each log level
const colors = {
  error: 'red',
  warn: 'yellow',
  info: 'green',
  http: 'magenta',
  debug: 'blue',
};

// Add colors to winston
winston.addColors(colors);

// Define the format for logs
const format = winston.format.combine(
  winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss:ms' }),
  winston.format.colorize({ all: true }),
  winston.format.printf(
    (info) => `${info.timestamp} ${info.level}: ${info.message}`,
  ),
);

// Define which transports to use
const transports = [
  // Console transport for all logs
  new winston.transports.Console(),
  // File transport for error logs
  new winston.transports.File({
    filename: 'logs/error.log',
    level: 'error',
  }),
  // File transport for all logs
  new winston.transports.File({ filename: 'logs/all.log' }),
];

// Create the winston logger
const winstonLogger = winston.createLogger({
  level: level(),
  levels,
  format,
  transports,
});

/**
 * Logger class that provides a structured interface for logging
 * while using Winston under the hood for advanced logging capabilities.
 */
export class Logger {
  private static instance: Logger;
  private context: Record<string, any> = {};

  private constructor() {}

  public static getInstance(): Logger {
    if (!Logger.instance) {
      Logger.instance = new Logger();
    }
    return Logger.instance;
  }

  /**
   * Create a child logger with additional context
   * @param context Additional context to include in logs
   * @returns A new Logger instance with the combined context
   */
  child(context: Record<string, any>): Logger {
    const childLogger = new Logger();
    childLogger.context = { ...this.context, ...context };
    return childLogger;
  }

  /**
   * Format a log message with context
   * @param level Log level
   * @param message Log message
   * @param data Additional data to log
   * @returns Formatted message string
   */
  private formatMessage(level: string, message: string, data?: any): string {
    const contextStr = Object.keys(this.context).length > 0 
      ? ` ${JSON.stringify(this.context)}` 
      : '';
    const dataStr = data ? ` ${JSON.stringify(data)}` : '';
    return `${message}${contextStr}${dataStr}`;
  }

  /**
   * Log an info message
   * @param message Log message
   * @param data Additional data to log
   */
  public info(message: string, data?: any): void {
    winstonLogger.info(this.formatMessage('INFO', message, data));
  }

  /**
   * Log a warning message
   * @param message Log message
   * @param data Additional data to log
   */
  public warn(message: string, data?: any): void {
    winstonLogger.warn(this.formatMessage('WARN', message, data));
  }

  /**
   * Log an error message
   * @param message Log message
   * @param data Additional data to log
   */
  public error(message: string, data?: any): void {
    winstonLogger.error(this.formatMessage('ERROR', message, data));
  }

  /**
   * Log a debug message
   * @param message Log message
   * @param data Additional data to log
   */
  public debug(message: string, data?: any): void {
    winstonLogger.debug(this.formatMessage('DEBUG', message, data));
  }

  /**
   * Log an HTTP request
   * @param message Log message
   * @param data Additional data to log
   */
  public http(message: string, data?: any): void {
    winstonLogger.http(this.formatMessage('HTTP', message, data));
  }
}

// Export the singleton logger instance
export const logger = Logger.getInstance();

// Also export the winston logger for direct use if needed
export default winstonLogger;