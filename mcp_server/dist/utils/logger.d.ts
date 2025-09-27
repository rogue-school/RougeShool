/**
 * 전문적인 로깅 시스템
 * 엔터프라이즈급 로깅 기능 제공
 */
export declare class Logger {
    private static instance;
    private logger;
    private constructor();
    static getInstance(): Logger;
    info(message: string, meta?: any): void;
    warn(message: string, meta?: any): void;
    error(message: string, error?: Error, meta?: any): void;
    debug(message: string, meta?: any): void;
    performance(operation: string, duration: number, meta?: any): void;
}
export declare const logger: Logger;
//# sourceMappingURL=logger.d.ts.map