import { ValidationError, PriceFeedConfig } from '../types/types';
import { PRICE_FEED_CONSTANTS } from '../constants';

export function validatePriceData(
  price: number,
  timestamp: number,
  sources: { price: number }[]
): ValidationError[] {
  const errors: ValidationError[] = [];

  // Check price validity
  if (price <= 0) {
    errors.push({
      field: 'price',
      message: 'Price must be greater than 0'
    });
  }

  // Check timestamp validity
  const now = Date.now();
  if (timestamp > now || timestamp < now - PRICE_FEED_CONSTANTS.MAX_PRICE_AGE) {
    errors.push({
      field: 'timestamp',
      message: 'Price data is too old or from the future'
    });
  }

  // Check source count
  if (sources.length < PRICE_FEED_CONSTANTS.MIN_SOURCE_COUNT) {
    errors.push({
      field: 'sources',
      message: `At least ${PRICE_FEED_CONSTANTS.MIN_SOURCE_COUNT} sources are required`
    });
  }

  // Check price deviation
  if (sources.length > 0) {
    const avgPrice = sources.reduce((sum, s) => sum + s.price, 0) / sources.length;
    const maxDeviation = Math.max(...sources.map(s => Math.abs(s.price - avgPrice) / avgPrice));
    
    if (maxDeviation > PRICE_FEED_CONSTANTS.DEFAULT_DEVIATION_THRESHOLD) {
      errors.push({
        field: 'price',
        message: 'Price deviation between sources is too high'
      });
    }
  }

  return errors;
}

export function validateConfig(config: Partial<PriceFeedConfig>): ValidationError[] {
  const errors: ValidationError[] = [];

  if ('heartbeatInterval' in config) {
    const interval = config.heartbeatInterval ?? 0; 
    if (interval < 1000 || interval > 3600000) {
      errors.push({
        field: 'heartbeatInterval',
        message: 'Heartbeat interval must be between 1 second and 1 hour'
      });
    }
  }

  if ('deviationThreshold' in config) {
    const threshold = config.deviationThreshold ?? 0; 
    if (threshold < 0.001 || threshold > 1) {
      errors.push({
        field: 'deviationThreshold',
        message: 'Deviation threshold must be between 0.1% and 100%'
      });
    }
  }

  if ('minSourceCount' in config) {
    const count = config.minSourceCount ?? 0; 
    if (count < 1 || count > 20) {
      errors.push({
        field: 'minSourceCount',
        message: 'Minimum source count must be between 1 and 20'
      });
    }
  }

  if ('customSourceWeights' in config && config.customSourceWeights) {
    const weights = Object.values(config.customSourceWeights);
    const totalWeight = weights.reduce((acc, weight) => acc + weight, 0);
    if (Math.abs(totalWeight - 1) > 0.001) {
      errors.push({
        field: 'customSourceWeights',
        message: 'Source weights must sum to 1'
      });
    }
  }

  if ('alertThresholds' in config && config.alertThresholds) {
    const { priceDeviation, sourceLatency, minReliability } = config.alertThresholds;
    if (priceDeviation < 0 || priceDeviation > 1) {
      errors.push({
        field: 'alertThresholds',
        message: 'Price deviation threshold must be between 0 and 1'
      });
    }
    if (sourceLatency < 0) {
      errors.push({
        field: 'alertThresholds',
        message: 'Source latency threshold must be positive'
      });
    }
    if (minReliability < 0 || minReliability > 1) {
      errors.push({
        field: 'alertThresholds',
        message: 'Minimum reliability must be between 0 and 1'
      });
    }
  }

  return errors;
}

export function validateSymbol(symbol: string): ValidationError[] {
  const errors: ValidationError[] = [];

  if (!PRICE_FEED_CONSTANTS.SUPPORTED_PAIRS.includes(symbol as any)) {
    errors.push({
      field: 'symbol',
      message: 'Unsupported trading pair'
    });
  }

  return errors;
}

export function validateSourceData(source: {
  id: string;
  name: string;
  currentPrice: number;
  lastUpdate: number;
  latency: number;
  reliability: number;
}): ValidationError[] {
  const errors: ValidationError[] = [];

  if (!source.id || !source.name) {
    errors.push({
      field: 'source',
      message: 'Source ID and name are required'
    });
  }

  if (source.currentPrice <= 0) {
    errors.push({
      field: 'currentPrice',
      message: 'Price must be greater than 0'
    });
  }

  const now = Date.now();
  if (source.lastUpdate > now || source.lastUpdate < now - PRICE_FEED_CONSTANTS.MAX_PRICE_AGE) {
    errors.push({
      field: 'lastUpdate',
      message: 'Source data is too old or from the future'
    });
  }

  if (source.latency < 0) {
    errors.push({
      field: 'latency',
      message: 'Latency must be non-negative'
    });
  }

  if (source.reliability < 0 || source.reliability > 1) {
    errors.push({
      field: 'reliability',
      message: 'Reliability must be between 0 and 1'
    });
  }

  return errors;
}