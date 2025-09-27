/**
 * 커스텀 에러 클래스들
 * 체계적인 에러 처리 시스템
 */
export declare class MCPServerError extends Error {
    readonly code: string;
    readonly statusCode: number;
    readonly isOperational: boolean;
    constructor(message: string, code: string, statusCode?: number, isOperational?: boolean);
}
export declare class FileNotFoundError extends MCPServerError {
    constructor(filePath: string);
}
export declare class InvalidFileTypeError extends MCPServerError {
    constructor(filePath: string, expectedTypes: string[]);
}
export declare class FileSizeExceededError extends MCPServerError {
    constructor(filePath: string, size: number, maxSize: number);
}
export declare class AnalysisError extends MCPServerError {
    constructor(operation: string, reason: string);
}
export declare class CacheError extends MCPServerError {
    constructor(operation: string, reason: string);
}
export declare class ValidationError extends MCPServerError {
    constructor(field: string, value: any, reason: string);
}
export declare class PerformanceError extends MCPServerError {
    constructor(operation: string, duration: number, threshold: number);
}
/**
 * 에러 처리 유틸리티 함수들
 */
export declare class ErrorHandler {
    /**
     * 에러를 적절한 형태로 변환
     */
    static normalizeError(error: unknown): MCPServerError;
    /**
     * 에러가 운영상 에러인지 확인
     */
    static isOperationalError(error: Error): boolean;
    /**
     * 에러를 안전하게 로깅
     */
    static logError(error: unknown, context?: any): void;
}
//# sourceMappingURL=errors.d.ts.map