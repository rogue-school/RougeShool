import { logger } from './logger.js';
import { config } from '../config/config.js';
import { CacheError } from './errors.js';

/**
 * 캐시 항목 인터페이스
 */
interface CacheItem<T> {
  value: T;
  timestamp: number;
  ttl: number;
}

/**
 * 고성능 메모리 캐시 시스템
 */
export class CacheManager {
  private static instance: CacheManager;
  private cache: Map<string, CacheItem<any>> = new Map();
  private maxSize: number;
  private defaultTtl: number;

  private constructor() {
    const cacheConfig = config.get('cache');
    this.maxSize = cacheConfig.maxSize;
    this.defaultTtl = cacheConfig.ttl;
  }

  public static getInstance(): CacheManager {
    if (!CacheManager.instance) {
      CacheManager.instance = new CacheManager();
    }
    return CacheManager.instance;
  }

  /**
   * 캐시에 값 저장
   */
  public set<T>(key: string, value: T, ttl?: number): void {
    try {
      // 캐시 크기 제한 확인
      if (this.cache.size >= this.maxSize) {
        this.evictOldest();
      }

      const item: CacheItem<T> = {
        value,
        timestamp: Date.now(),
        ttl: ttl || this.defaultTtl
      };

      this.cache.set(key, item);
      logger.debug(`캐시 저장: ${key}`, { ttl: item.ttl });
    } catch (error) {
      throw new CacheError('set', `Failed to set cache item: ${error}`);
    }
  }

  /**
   * 캐시에서 값 조회
   */
  public get<T>(key: string): T | null {
    try {
      const item = this.cache.get(key);
      
      if (!item) {
        logger.debug(`캐시 미스: ${key}`);
        return null;
      }

      // TTL 확인
      if (Date.now() - item.timestamp > item.ttl) {
        this.cache.delete(key);
        logger.debug(`캐시 만료: ${key}`);
        return null;
      }

      logger.debug(`캐시 히트: ${key}`);
      return item.value as T;
    } catch (error) {
      throw new CacheError('get', `Failed to get cache item: ${error}`);
    }
  }

  /**
   * 캐시에서 값 삭제
   */
  public delete(key: string): boolean {
    try {
      const deleted = this.cache.delete(key);
      logger.debug(`캐시 삭제: ${key}`, { deleted });
      return deleted;
    } catch (error) {
      throw new CacheError('delete', `Failed to delete cache item: ${error}`);
    }
  }

  /**
   * 캐시 전체 초기화
   */
  public clear(): void {
    try {
      this.cache.clear();
      logger.info('캐시 전체 초기화 완료');
    } catch (error) {
      throw new CacheError('clear', `Failed to clear cache: ${error}`);
    }
  }

  /**
   * 캐시 크기 조회
   */
  public size(): number {
    return this.cache.size;
  }

  /**
   * 캐시 통계 조회
   */
  public getStats(): {
    size: number;
    maxSize: number;
    hitRate: number;
    keys: string[];
  } {
    return {
      size: this.cache.size,
      maxSize: this.maxSize,
      hitRate: 0, // TODO: 히트율 계산 구현
      keys: Array.from(this.cache.keys())
    };
  }

  /**
   * 가장 오래된 항목 제거
   */
  private evictOldest(): void {
    let oldestKey = '';
    let oldestTime = Date.now();

    for (const [key, item] of this.cache) {
      if (item.timestamp < oldestTime) {
        oldestTime = item.timestamp;
        oldestKey = key;
      }
    }

    if (oldestKey) {
      this.cache.delete(oldestKey);
      logger.debug(`캐시 제거: ${oldestKey} (LRU)`);
    }
  }

  /**
   * 만료된 항목들 정리
   */
  public cleanup(): void {
    const now = Date.now();
    let cleanedCount = 0;

    for (const [key, item] of this.cache) {
      if (now - item.timestamp > item.ttl) {
        this.cache.delete(key);
        cleanedCount++;
      }
    }

    if (cleanedCount > 0) {
      logger.info(`캐시 정리 완료: ${cleanedCount}개 항목 제거`);
    }
  }

  /**
   * 캐시가 활성화되어 있는지 확인
   */
  public isEnabled(): boolean {
    return config.get('cache').enableCaching;
  }
}

export const cacheManager = CacheManager.getInstance();
