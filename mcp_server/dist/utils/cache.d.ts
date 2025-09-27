/**
 * 고성능 메모리 캐시 시스템
 */
export declare class CacheManager {
    private static instance;
    private cache;
    private maxSize;
    private defaultTtl;
    private constructor();
    static getInstance(): CacheManager;
    /**
     * 캐시에 값 저장
     */
    set<T>(key: string, value: T, ttl?: number): void;
    /**
     * 캐시에서 값 조회
     */
    get<T>(key: string): T | null;
    /**
     * 캐시에서 값 삭제
     */
    delete(key: string): boolean;
    /**
     * 캐시 전체 초기화
     */
    clear(): void;
    /**
     * 캐시 크기 조회
     */
    size(): number;
    /**
     * 캐시 통계 조회
     */
    getStats(): {
        size: number;
        maxSize: number;
        hitRate: number;
        keys: string[];
    };
    /**
     * 가장 오래된 항목 제거
     */
    private evictOldest;
    /**
     * 만료된 항목들 정리
     */
    cleanup(): void;
    /**
     * 캐시가 활성화되어 있는지 확인
     */
    isEnabled(): boolean;
}
export declare const cacheManager: CacheManager;
//# sourceMappingURL=cache.d.ts.map