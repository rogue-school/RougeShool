import { logger } from './logger.js';
import { config } from '../config/config.js';
import { CacheError } from './errors.js';
/**
 * 고성능 메모리 캐시 시스템
 */
export class CacheManager {
    static instance;
    cache = new Map();
    maxSize;
    defaultTtl;
    constructor() {
        const cacheConfig = config.get('cache');
        this.maxSize = cacheConfig.maxSize;
        this.defaultTtl = cacheConfig.ttl;
    }
    static getInstance() {
        if (!CacheManager.instance) {
            CacheManager.instance = new CacheManager();
        }
        return CacheManager.instance;
    }
    /**
     * 캐시에 값 저장
     */
    set(key, value, ttl) {
        try {
            // 캐시 크기 제한 확인
            if (this.cache.size >= this.maxSize) {
                this.evictOldest();
            }
            const item = {
                value,
                timestamp: Date.now(),
                ttl: ttl || this.defaultTtl
            };
            this.cache.set(key, item);
            logger.debug(`캐시 저장: ${key}`, { ttl: item.ttl });
        }
        catch (error) {
            throw new CacheError('set', `Failed to set cache item: ${error}`);
        }
    }
    /**
     * 캐시에서 값 조회
     */
    get(key) {
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
            return item.value;
        }
        catch (error) {
            throw new CacheError('get', `Failed to get cache item: ${error}`);
        }
    }
    /**
     * 캐시에서 값 삭제
     */
    delete(key) {
        try {
            const deleted = this.cache.delete(key);
            logger.debug(`캐시 삭제: ${key}`, { deleted });
            return deleted;
        }
        catch (error) {
            throw new CacheError('delete', `Failed to delete cache item: ${error}`);
        }
    }
    /**
     * 캐시 전체 초기화
     */
    clear() {
        try {
            this.cache.clear();
            logger.info('캐시 전체 초기화 완료');
        }
        catch (error) {
            throw new CacheError('clear', `Failed to clear cache: ${error}`);
        }
    }
    /**
     * 캐시 크기 조회
     */
    size() {
        return this.cache.size;
    }
    /**
     * 캐시 통계 조회
     */
    getStats() {
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
    evictOldest() {
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
    cleanup() {
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
    isEnabled() {
        return config.get('cache').enableCaching;
    }
}
export const cacheManager = CacheManager.getInstance();
//# sourceMappingURL=cache.js.map