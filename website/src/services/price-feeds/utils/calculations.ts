import { PriceSource, HistoricalPriceData } from '../types/types';

export function calculateWeightedAverage(
  sources: PriceSource[],
  customWeights?: Record<string, number>
): number {
  const activeSources = sources.filter(s => s.status === 'active');
  if (activeSources.length === 0) return 0;

  let totalWeight = 0;
  let weightedSum = 0;

  activeSources.forEach(source => {
    const weight = customWeights?.[source.id] ?? source.weight ?? 1; // Default to 1 if weight is undefined
    weightedSum += source.currentPrice * weight;
    totalWeight += weight;
  });

  return totalWeight > 0 ? weightedSum / totalWeight : 0;
}

export function calculateVolatility(prices: HistoricalPriceData[]): number {
  if (prices.length < 2) return 0;

  const returns = prices.slice(1).map((p, i) => {
    const prev = prices[i];
    return (p.price - prev.price) / prev.price;
  });

  const mean = returns.reduce((a, b) => a + b, 0) / returns.length;
  const variance = returns.reduce((a, b) => a + Math.pow(b - mean, 2), 0) / returns.length;

  return Math.sqrt(variance);
}

export function calculateReliabilityScore(
  source: PriceSource,
  avgPrice: number,
  avgLatency: number
): number {
  const priceDeviation = Math.abs(source.currentPrice - avgPrice) / avgPrice;
  const latencyScore = Math.max(0, 1 - source.latency / (2 * avgLatency));
  const deviationScore = Math.max(0, 1 - priceDeviation / 0.1); // 10% max deviation

  return (latencyScore + deviationScore) / 2;
}

export function calculateSourceStats(sources: PriceSource[]): {
  avgPrice: number;
  avgLatency: number;
  avgDeviation: number;
  reliabilityScore: number;
} {
  if (sources.length === 0) {
    return {
      avgPrice: 0,
      avgLatency: 0,
      avgDeviation: 0,
      reliabilityScore: 0
    };
  }

  const avgPrice = calculateWeightedAverage(sources);
  const avgLatency = sources.reduce((acc, s) => acc + s.latency, 0) / sources.length;
  
  const deviations = sources.map(s => Math.abs(s.currentPrice - avgPrice) / avgPrice);
  const avgDeviation = deviations.reduce((acc, d) => acc + d, 0) / deviations.length;
  
  const reliabilityScores = sources.map(s => calculateReliabilityScore(s, avgPrice, avgLatency));
  const avgReliability = reliabilityScores.reduce((acc, r) => acc + r, 0) / reliabilityScores.length;

  return {
    avgPrice,
    avgLatency,
    avgDeviation,
    reliabilityScore: avgReliability
  };
}

export function calculatePriceChange(
  currentPrice: number,
  historicalPrices: HistoricalPriceData[],
  timeframeMs: number
): {
  absoluteChange: number;
  percentageChange: number;
} {
  const now = Date.now();
  const targetTime = now - timeframeMs;
  
  const oldPrice = historicalPrices
    .filter(p => p.timestamp <= targetTime)
    .sort((a, b) => b.timestamp - a.timestamp)[0]?.price ?? currentPrice;

  const absoluteChange = currentPrice - oldPrice;
  const percentageChange = oldPrice !== 0 ? (absoluteChange / oldPrice) : 0;

  return {
    absoluteChange,
    percentageChange
  };
}

export function calculateMovingAverage(
  prices: HistoricalPriceData[],
  period: number
): number[] {
  if (prices.length < period) return [];

  const movingAverages: number[] = [];
  for (let i = period - 1; i < prices.length; i++) {
    const sum = prices.slice(i - period + 1, i + 1).reduce((acc, p) => acc + p.price, 0);
    movingAverages.push(sum / period);
  }

  return movingAverages;
}

export function detectPriceAnomaly(
  price: number,
  recentPrices: HistoricalPriceData[],
  deviationThreshold: number
): boolean {
  if (recentPrices.length < 10) return false;

  const mean = recentPrices.reduce((acc, p) => acc + p.price, 0) / recentPrices.length;
  const stdDev = Math.sqrt(
    recentPrices.reduce((acc, p) => acc + Math.pow(p.price - mean, 2), 0) / recentPrices.length
  );

  const zScore = Math.abs(price - mean) / stdDev;
  return zScore > deviationThreshold;
}