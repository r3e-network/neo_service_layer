import {
  calculateWeightedAverage,
  calculateVolatility,
  calculateReliabilityScore,
  calculateSourceStats,
  calculatePriceChange,
  calculateMovingAverage,
  detectPriceAnomaly
} from '../../utils/calculations';
import { PriceSource, HistoricalPriceData } from '../../types';

describe('Price Feed Calculations', () => {
  describe('calculateWeightedAverage', () => {
    const mockSources: PriceSource[] = [
      { id: '1', currentPrice: 100, weight: 0.5, status: 'active' },
      { id: '2', currentPrice: 200, weight: 0.3, status: 'active' },
      { id: '3', currentPrice: 300, weight: 0.2, status: 'active' }
    ] as PriceSource[];

    it('should calculate correct weighted average', () => {
      const result = calculateWeightedAverage(mockSources);
      // (100 * 0.5 + 200 * 0.3 + 300 * 0.2) = 170
      expect(result).toBe(170);
    });

    it('should handle custom weights', () => {
      const customWeights = { '1': 0.6, '2': 0.3, '3': 0.1 };
      const result = calculateWeightedAverage(mockSources, customWeights);
      // (100 * 0.6 + 200 * 0.3 + 300 * 0.1) = 150
      expect(result).toBe(150);
    });

    it('should return 0 for empty sources', () => {
      expect(calculateWeightedAverage([])).toBe(0);
    });
  });

  describe('calculateVolatility', () => {
    const mockPrices: HistoricalPriceData[] = [
      { price: 100, timestamp: 1000 },
      { price: 110, timestamp: 2000 },
      { price: 90, timestamp: 3000 },
      { price: 105, timestamp: 4000 }
    ];

    it('should calculate volatility correctly', () => {
      const result = calculateVolatility(mockPrices);
      expect(result).toBeGreaterThan(0);
      expect(result).toBeLessThan(1);
    });

    it('should return 0 for insufficient data points', () => {
      expect(calculateVolatility([{ price: 100, timestamp: 1000 }])).toBe(0);
    });
  });

  describe('calculateReliabilityScore', () => {
    const mockSource = {
      currentPrice: 100,
      latency: 500
    } as PriceSource;

    it('should calculate reliability score correctly', () => {
      const result = calculateReliabilityScore(mockSource, 105, 1000);
      expect(result).toBeGreaterThan(0);
      expect(result).toBeLessThan(1);
    });

    it('should handle extreme deviations', () => {
      const result = calculateReliabilityScore(mockSource, 200, 1000);
      expect(result).toBeLessThan(0.5);
    });
  });

  describe('calculateSourceStats', () => {
    const mockSources: PriceSource[] = [
      { id: '1', currentPrice: 100, latency: 500, weight: 0.5, status: 'active' },
      { id: '2', currentPrice: 110, latency: 600, weight: 0.5, status: 'active' }
    ] as PriceSource[];

    it('should calculate all stats correctly', () => {
      const stats = calculateSourceStats(mockSources);
      expect(stats.avgPrice).toBe(105);
      expect(stats.avgLatency).toBe(550);
      expect(stats.avgDeviation).toBeGreaterThan(0);
      expect(stats.reliabilityScore).toBeGreaterThan(0);
    });

    it('should handle empty sources', () => {
      const stats = calculateSourceStats([]);
      expect(stats).toEqual({
        avgPrice: 0,
        avgLatency: 0,
        avgDeviation: 0,
        reliabilityScore: 0
      });
    });
  });

  describe('calculatePriceChange', () => {
    const mockHistorical: HistoricalPriceData[] = [
      { price: 100, timestamp: Date.now() - 3600000 }, // 1 hour ago
      { price: 110, timestamp: Date.now() - 1800000 }, // 30 mins ago
    ];

    it('should calculate price changes correctly', () => {
      const result = calculatePriceChange(120, mockHistorical, 3600000);
      expect(result.absoluteChange).toBe(20);
      expect(result.percentageChange).toBe(0.2);
    });

    it('should handle missing historical data', () => {
      const result = calculatePriceChange(120, [], 3600000);
      expect(result.absoluteChange).toBe(0);
      expect(result.percentageChange).toBe(0);
    });
  });

  describe('calculateMovingAverage', () => {
    const mockPrices: HistoricalPriceData[] = [
      { price: 100, timestamp: 1000 },
      { price: 110, timestamp: 2000 },
      { price: 120, timestamp: 3000 },
      { price: 130, timestamp: 4000 }
    ];

    it('should calculate moving average correctly', () => {
      const result = calculateMovingAverage(mockPrices, 2);
      expect(result).toEqual([105, 115, 125]);
    });

    it('should return empty array for insufficient data', () => {
      expect(calculateMovingAverage(mockPrices, 5)).toEqual([]);
    });
  });

  describe('detectPriceAnomaly', () => {
    const mockPrices: HistoricalPriceData[] = Array(20).fill(0).map((_, i) => ({
      price: 100 + i,
      timestamp: i * 1000
    }));

    it('should detect anomalies correctly', () => {
      expect(detectPriceAnomaly(200, mockPrices, 2)).toBe(true);
      expect(detectPriceAnomaly(110, mockPrices, 2)).toBe(false);
    });

    it('should handle insufficient data points', () => {
      expect(detectPriceAnomaly(200, mockPrices.slice(0, 5), 2)).toBe(false);
    });
  });
});