export interface CacheOptions {
  ttl?: number; // Time to live in seconds
}

export class CacheService {
  private cache: Map<string, { value: string; expires: number }> = new Map();

  async get(key: string): Promise<string | null> {
    const item = this.cache.get(key);
    if (!item) return null;
    
    if (Date.now() > item.expires) {
      this.cache.delete(key);
      return null;
    }

    return item.value;
  }

  async set(key: string, value: string, options: CacheOptions = {}): Promise<boolean> {
    const ttl = options.ttl || 300; // Default 5 minutes
    this.cache.set(key, {
      value,
      expires: Date.now() + (ttl * 1000)
    });
    return true;
  }
}