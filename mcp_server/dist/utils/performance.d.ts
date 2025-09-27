/**
 * 성능 메트릭 수집 및 모니터링
 */
export declare class PerformanceMonitor {
    private static instance;
    private metrics;
    private activeOperations;
    private constructor();
    static getInstance(): PerformanceMonitor;
    /**
     * 작업 시작 시간 기록
     */
    startOperation(operationId: string): void;
    /**
     * 작업 완료 시간 기록 및 메트릭 수집
     */
    endOperation(operationId: string): number;
    /**
     * 메트릭 기록
     */
    private recordMetric;
    /**
     * 작업별 평균 성능 조회
     */
    getAveragePerformance(operation: string): number | null;
    /**
     * 모든 성능 메트릭 조회
     */
    getAllMetrics(): Record<string, {
        average: number;
        count: number;
        latest: number;
    }>;
    /**
     * 메트릭 초기화
     */
    clearMetrics(): void;
    /**
     * 성능 리포트 생성
     */
    generateReport(): string;
}
export declare const performanceMonitor: PerformanceMonitor;
//# sourceMappingURL=performance.d.ts.map