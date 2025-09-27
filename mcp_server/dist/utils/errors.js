/**
 * 커스텀 에러 클래스들
 * 체계적인 에러 처리 시스템
 */
export class MCPServerError extends Error {
    code;
    statusCode;
    isOperational;
    constructor(message, code, statusCode = 500, isOperational = true) {
        super(message);
        this.name = this.constructor.name;
        this.code = code;
        this.statusCode = statusCode;
        this.isOperational = isOperational;
        Error.captureStackTrace(this, this.constructor);
    }
}
export class FileNotFoundError extends MCPServerError {
    constructor(filePath) {
        super(`파일을 찾을 수 없습니다: ${filePath}`, 'FILE_NOT_FOUND', 404);
    }
}
export class InvalidFileTypeError extends MCPServerError {
    constructor(filePath, expectedTypes) {
        super(`지원하지 않는 파일 타입입니다: ${filePath}. 지원 타입: ${expectedTypes.join(', ')}`, 'INVALID_FILE_TYPE', 400);
    }
}
export class FileSizeExceededError extends MCPServerError {
    constructor(filePath, size, maxSize) {
        super(`파일 크기가 초과되었습니다: ${filePath} (${size} bytes > ${maxSize} bytes)`, 'FILE_SIZE_EXCEEDED', 413);
    }
}
export class AnalysisError extends MCPServerError {
    constructor(operation, reason) {
        super(`분석 실패: ${operation} - ${reason}`, 'ANALYSIS_ERROR', 500);
    }
}
export class CacheError extends MCPServerError {
    constructor(operation, reason) {
        super(`캐시 오류: ${operation} - ${reason}`, 'CACHE_ERROR', 500);
    }
}
export class ValidationError extends MCPServerError {
    constructor(field, value, reason) {
        super(`유효성 검사 실패: ${field} = ${value} - ${reason}`, 'VALIDATION_ERROR', 400);
    }
}
export class PerformanceError extends MCPServerError {
    constructor(operation, duration, threshold) {
        super(`성능 임계값 초과: ${operation} (${duration}ms > ${threshold}ms)`, 'PERFORMANCE_ERROR', 500);
    }
}
/**
 * 에러 처리 유틸리티 함수들
 */
export class ErrorHandler {
    /**
     * 에러를 적절한 형태로 변환
     */
    static normalizeError(error) {
        if (error instanceof MCPServerError) {
            return error;
        }
        if (error instanceof Error) {
            return new MCPServerError(error.message, 'UNKNOWN_ERROR', 500, false);
        }
        return new MCPServerError('알 수 없는 오류가 발생했습니다', 'UNKNOWN_ERROR', 500, false);
    }
    /**
     * 에러가 운영상 에러인지 확인
     */
    static isOperationalError(error) {
        if (error instanceof MCPServerError) {
            return error.isOperational;
        }
        return false;
    }
    /**
     * 에러를 안전하게 로깅
     */
    static logError(error, context) {
        const normalizedError = this.normalizeError(error);
        // 로깅 시스템에 전달 (나중에 logger와 연동)
        console.error('MCP Server Error:', {
            message: normalizedError.message,
            code: normalizedError.code,
            statusCode: normalizedError.statusCode,
            stack: normalizedError.stack,
            context
        });
    }
}
//# sourceMappingURL=errors.js.map