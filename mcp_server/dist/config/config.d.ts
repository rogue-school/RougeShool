import { z } from 'zod';
/**
 * 설정 스키마 정의
 * Zod를 사용한 타입 안전한 설정 관리
 */
declare const ConfigSchema: z.ZodObject<{
    server: z.ZodObject<{
        name: z.ZodDefault<z.ZodString>;
        version: z.ZodDefault<z.ZodString>;
        port: z.ZodDefault<z.ZodNumber>;
        host: z.ZodDefault<z.ZodString>;
    }, "strip", z.ZodTypeAny, {
        name: string;
        version: string;
        port: number;
        host: string;
    }, {
        name?: string | undefined;
        version?: string | undefined;
        port?: number | undefined;
        host?: string | undefined;
    }>;
    project: z.ZodObject<{
        rootPath: z.ZodString;
        scriptPath: z.ZodString;
        maxFileSize: z.ZodDefault<z.ZodNumber>;
        supportedExtensions: z.ZodDefault<z.ZodArray<z.ZodString, "many">>;
    }, "strip", z.ZodTypeAny, {
        rootPath: string;
        scriptPath: string;
        maxFileSize: number;
        supportedExtensions: string[];
    }, {
        rootPath: string;
        scriptPath: string;
        maxFileSize?: number | undefined;
        supportedExtensions?: string[] | undefined;
    }>;
    logging: z.ZodObject<{
        level: z.ZodDefault<z.ZodEnum<["error", "warn", "info", "debug"]>>;
        enableFileLogging: z.ZodDefault<z.ZodBoolean>;
        enableConsoleLogging: z.ZodDefault<z.ZodBoolean>;
        logDirectory: z.ZodDefault<z.ZodString>;
    }, "strip", z.ZodTypeAny, {
        level: "info" | "error" | "warn" | "debug";
        enableFileLogging: boolean;
        enableConsoleLogging: boolean;
        logDirectory: string;
    }, {
        level?: "info" | "error" | "warn" | "debug" | undefined;
        enableFileLogging?: boolean | undefined;
        enableConsoleLogging?: boolean | undefined;
        logDirectory?: string | undefined;
    }>;
    performance: z.ZodObject<{
        enableMetrics: z.ZodDefault<z.ZodBoolean>;
        slowOperationThreshold: z.ZodDefault<z.ZodNumber>;
        maxConcurrentOperations: z.ZodDefault<z.ZodNumber>;
    }, "strip", z.ZodTypeAny, {
        enableMetrics: boolean;
        slowOperationThreshold: number;
        maxConcurrentOperations: number;
    }, {
        enableMetrics?: boolean | undefined;
        slowOperationThreshold?: number | undefined;
        maxConcurrentOperations?: number | undefined;
    }>;
    cache: z.ZodObject<{
        enableCaching: z.ZodDefault<z.ZodBoolean>;
        ttl: z.ZodDefault<z.ZodNumber>;
        maxSize: z.ZodDefault<z.ZodNumber>;
    }, "strip", z.ZodTypeAny, {
        enableCaching: boolean;
        ttl: number;
        maxSize: number;
    }, {
        enableCaching?: boolean | undefined;
        ttl?: number | undefined;
        maxSize?: number | undefined;
    }>;
}, "strip", z.ZodTypeAny, {
    server: {
        name: string;
        version: string;
        port: number;
        host: string;
    };
    project: {
        rootPath: string;
        scriptPath: string;
        maxFileSize: number;
        supportedExtensions: string[];
    };
    logging: {
        level: "info" | "error" | "warn" | "debug";
        enableFileLogging: boolean;
        enableConsoleLogging: boolean;
        logDirectory: string;
    };
    performance: {
        enableMetrics: boolean;
        slowOperationThreshold: number;
        maxConcurrentOperations: number;
    };
    cache: {
        enableCaching: boolean;
        ttl: number;
        maxSize: number;
    };
}, {
    server: {
        name?: string | undefined;
        version?: string | undefined;
        port?: number | undefined;
        host?: string | undefined;
    };
    project: {
        rootPath: string;
        scriptPath: string;
        maxFileSize?: number | undefined;
        supportedExtensions?: string[] | undefined;
    };
    logging: {
        level?: "info" | "error" | "warn" | "debug" | undefined;
        enableFileLogging?: boolean | undefined;
        enableConsoleLogging?: boolean | undefined;
        logDirectory?: string | undefined;
    };
    performance: {
        enableMetrics?: boolean | undefined;
        slowOperationThreshold?: number | undefined;
        maxConcurrentOperations?: number | undefined;
    };
    cache: {
        enableCaching?: boolean | undefined;
        ttl?: number | undefined;
        maxSize?: number | undefined;
    };
}>;
export type Config = z.infer<typeof ConfigSchema>;
/**
 * 설정 관리 클래스
 * 싱글톤 패턴으로 전역 설정 관리
 */
export declare class ConfigManager {
    private static instance;
    private config;
    private constructor();
    static getInstance(): ConfigManager;
    getConfig(): Config;
    get<K extends keyof Config>(key: K): Config[K];
    updateConfig(updates: Partial<Config>): void;
}
export declare const config: ConfigManager;
export {};
//# sourceMappingURL=config.d.ts.map