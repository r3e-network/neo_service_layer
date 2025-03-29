import { logger } from '../../src/utils/logger';

describe('Logger', () => {
  let consoleLogSpy: jest.SpyInstance;
  let consoleWarnSpy: jest.SpyInstance;
  let consoleErrorSpy: jest.SpyInstance;

  beforeEach(() => {
    // Mock console methods
    consoleLogSpy = jest.spyOn(console, 'log').mockImplementation();
    consoleWarnSpy = jest.spyOn(console, 'warn').mockImplementation();
    consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation();
  });

  afterEach(() => {
    // Restore console methods
    consoleLogSpy.mockRestore();
    consoleWarnSpy.mockRestore();
    consoleErrorSpy.mockRestore();
  });

  describe('Log Levels', () => {
    it('should log info messages', () => {
      const message = 'Test info message';
      const data = { key: 'value' };
      
      logger.info(message, data);
      
      expect(consoleLogSpy).toHaveBeenCalledWith(
        expect.stringMatching(/^\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z\] INFO: Test info message {"key":"value"}$/)
      );
    });

    it('should log warning messages', () => {
      const message = 'Test warning message';
      const data = { key: 'value' };
      
      logger.warn(message, data);
      
      expect(consoleWarnSpy).toHaveBeenCalledWith(
        expect.stringMatching(/^\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z\] WARN: Test warning message {"key":"value"}$/)
      );
    });

    it('should log error messages', () => {
      const message = 'Test error message';
      const error = new Error('Test error');
      
      logger.error(message, { error });
      
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        expect.stringMatching(/^\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z\] ERROR: Test error message {"error":.*}$/)
      );
    });
  });

  describe('Log Format', () => {
    it('should format logs with timestamp', () => {
      logger.info('Test message');
      
      expect(consoleLogSpy).toHaveBeenCalledWith(
        expect.stringMatching(/^\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z\]/)
      );
    });

    it('should handle undefined data', () => {
      logger.info('Test message');
      
      expect(consoleLogSpy).toHaveBeenCalledWith(
        expect.stringMatching(/^\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z\] INFO: Test message$/)
      );
    });

    it('should handle complex data objects', () => {
      const complexData = {
        nested: {
          array: [1, 2, 3],
          object: { key: 'value' },
        },
        date: new Date('2023-01-01'),
      };

      logger.info('Test message', complexData);
      
      expect(consoleLogSpy).toHaveBeenCalledWith(
        expect.stringMatching(/^\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z\] INFO: Test message {.*}$/)
      );
    });
  });

  describe('Singleton Pattern', () => {
    it('should maintain single instance', () => {
      const logMessage = 'Test singleton';
      
      // Log with current instance
      logger.info(logMessage);
      
      // Import logger again (should be same instance)
      const { logger: logger2 } = require('../../src/utils/logger');
      logger2.info(logMessage);
      
      expect(consoleLogSpy).toHaveBeenCalledTimes(2);
      expect(consoleLogSpy).toHaveBeenNthCalledWith(1,
        expect.stringMatching(/^\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z\] INFO: Test singleton$/)
      );
      expect(consoleLogSpy).toHaveBeenNthCalledWith(2,
        expect.stringMatching(/^\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z\] INFO: Test singleton$/)
      );
    });
  });
});