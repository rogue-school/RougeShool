import { logger } from './logger.js';
import { config } from '../config/config.js';
/**
 * 성능 메트릭 수집 및 모니터링
 */
export class PerformanceMonitor {
    static instance;
    metrics = new Map();
    activeOperations = new Map();
    constructor() { }
    static getInstance() {
        if (!PerformanceMonitor.instance) {
            PerformanceMonitor.instance = new PerformanceMonitor();
        }
        return PerformanceMonitor.instance;
    }
    /**
     * 작업 시작 시간 기록
     */
    startOperation(operationId) {
        this.activeOperations.set(operationId, Date.now());
    }
    /**
     * 작업 완료 시간 기록 및 메트릭 수집
     */
    endOperation(operationId) {
        const startTime = this.activeOperations.get(operationId);
        if (!startTime) {
            logger.warn(`작업 시작 시간을 찾을 수 없습니다: ${operationId}`);
            return 0;
        }
        const duration = Date.now() - startTime;
        this.activeOperations.delete(operationId);
        // 메트릭 수집
        this.recordMetric(operationId, duration);
        // 성능 임계값 확인
        const threshold = config.get('performance').slowOperationThreshold;
        if (duration > threshold) {
            logger.warn(`느린 작업 감지: ${operationId} (${duration}ms)`, {
                operation: operationId,
                duration,
                threshold
            });
        }
        logger.performance(operationId, duration);
        return duration;
    }
    /**
     * 메트릭 기록
     */
    recordMetric(operation, duration) {
        if (!this.metrics.has(operation)) {
            this.metrics.set(operation, []);
        }
        const metrics = this.metrics.get(operation);
        metrics.push(duration);
        // 최대 100개까지만 유지
        if (metrics.length > 100) {
            metrics.shift();
        }
    }
    /**
     * 작업별 평균 성능 조회
     */
    getAveragePerformance(operation) {
        const metrics = this.metrics.get(operation);
        if (!metrics || metrics.length === 0) {
            return null;
        }
        return metrics.reduce((sum, duration) => sum + duration, 0) / metrics.length;
    }
    /**
     * 모든 성능 메트릭 조회
     */
    getAllMetrics() {
        const result = {};
        for (const [operation, durations] of this.metrics) {
            if (durations.length > 0) {
                result[operation] = {
                    average: durations.reduce((sum, duration) => sum + duration, 0) / durations.length,
                    count: durations.length,
                    latest: durations[durations.length - 1] || 0
                };
            }
        }
        return result;
    }
    /**
     * 메트릭 초기화
     */
    clearMetrics() {
        this.metrics.clear();
        this.activeOperations.clear();
    }
    /**
     * 성능 리포트 생성
     */
    generateReport() {
        const metrics = this.getAllMetrics();
        const report = ['=== 성능 리포트 ==='];
        for (const [operation, data] of Object.entries(metrics)) {
            report.push(`${operation}:`);
            report.push(`  평균: ${data.average.toFixed(2)}ms`);
            report.push(`  실행 횟수: ${data.count}`);
            report.push(`  최근: ${data.latest}ms`);
            report.push('');
        }
        return report.join('\n');
    }
}
export const performanceMonitor = PerformanceMonitor.getInstance();
//# sourceMappingURL=performance.js.map