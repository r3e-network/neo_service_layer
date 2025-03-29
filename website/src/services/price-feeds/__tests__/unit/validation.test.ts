import {
  validatePriceData,
  validateConfig,
  validateSymbol,
  validateSourceData
} from '../../utils/validation';
import { PRICE_FEED_CONSTANTS } from '../../constants';

describe('Price Feed Validation', () => {
  describe('validatePriceData', () => {
    const validSources = Array(PRICE_FEED_CONSTANTS.MIN_SOURCE_COUNT).fill(null).map(() => ({
      price: 100
    }));

    it('should pass validation for valid price data', () => {
      const errors = validatePriceData(100, Date.now(), validSources);
      expect(errors).toHaveLength(0);
    });

    it('should fail for negative price', () => {
      const errors = validatePriceData(-100, Date.now(), validSources);
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('price');
    });

    it('should fail for future timestamp', () => {
      const errors = validatePriceData(100, Date.now() + 1000000, validSources);
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('timestamp');
    });

    it('should fail for old timestamp', () => {
      const errors = validatePriceData(
        100,
        Date.now() - PRICE_FEED_CONSTANTS.MAX_PRICE_AGE - 1000,
        validSources
      );
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('timestamp');
    });

    it('should fail for insufficient sources', () => {
      const errors = validatePriceData(100, Date.now(), [{ price: 100 }]);
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('sources');
    });

    it('should fail for high price deviation', () => {
      const sources = [
        { price: 100 },
        { price: 100 },
        { price: 200 } // High deviation
      ];
      const errors = validatePriceData(100, Date.now(), sources);
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('price');
    });
  });

  describe('validateConfig', () => {
    it('should pass validation for valid config', () => {
      const config = {
        heartbeatInterval: 5000,
        deviationThreshold: 0.01,
        minSourceCount: 3,
        customSourceWeights: { source1: 0.5, source2: 0.5 },
        alertThresholds: {
          priceDeviation: 0.05,
          sourceLatency: 1000,
          minReliability: 0.8
        }
      };
      const errors = validateConfig(config);
      expect(errors).toHaveLength(0);
    });

    it('should fail for invalid heartbeat interval', () => {
      const errors = validateConfig({ heartbeatInterval: 0 });
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('heartbeatInterval');
    });

    it('should fail for invalid deviation threshold', () => {
      const errors = validateConfig({ deviationThreshold: 2 });
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('deviationThreshold');
    });

    it('should fail for invalid source count', () => {
      const errors = validateConfig({ minSourceCount: 0 });
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('minSourceCount');
    });

    it('should fail for invalid source weights', () => {
      const errors = validateConfig({
        customSourceWeights: { source1: 0.3, source2: 0.3 }
      });
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('customSourceWeights');
    });

    it('should fail for invalid alert thresholds', () => {
      const errors = validateConfig({
        alertThresholds: {
          priceDeviation: -1,
          sourceLatency: -1,
          minReliability: 2
        }
      });
      expect(errors).toHaveLength(3);
    });
  });

  describe('validateSymbol', () => {
    it('should pass for supported symbol', () => {
      const errors = validateSymbol(PRICE_FEED_CONSTANTS.SUPPORTED_PAIRS[0]);
      expect(errors).toHaveLength(0);
    });

    it('should fail for unsupported symbol', () => {
      const errors = validateSymbol('UNSUPPORTED/PAIR');
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('symbol');
    });
  });

  describe('validateSourceData', () => {
    const validSource = {
      id: 'source1',
      name: 'Source 1',
      currentPrice: 100,
      lastUpdate: Date.now(),
      latency: 100,
      reliability: 0.9
    };

    it('should pass validation for valid source data', () => {
      const errors = validateSourceData(validSource);
      expect(errors).toHaveLength(0);
    });

    it('should fail for missing id or name', () => {
      const errors = validateSourceData({ ...validSource, id: '', name: '' });
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('source');
    });

    it('should fail for invalid price', () => {
      const errors = validateSourceData({ ...validSource, currentPrice: -100 });
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('currentPrice');
    });

    it('should fail for future update time', () => {
      const errors = validateSourceData({
        ...validSource,
        lastUpdate: Date.now() + 1000000
      });
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('lastUpdate');
    });

    it('should fail for negative latency', () => {
      const errors = validateSourceData({ ...validSource, latency: -100 });
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('latency');
    });

    it('should fail for invalid reliability', () => {
      const errors = validateSourceData({ ...validSource, reliability: 2 });
      expect(errors).toHaveLength(1);
      expect(errors[0].field).toBe('reliability');
    });
  });
});