import { z } from 'zod';
import dotenv from 'dotenv';
import path from 'path';
// 환경 변수 로드
dotenv.config();
/**
 * 설정 스키마 정의
 * Zod를 사용한 타입 안전한 설정 관리
 */
const ConfigSchema = z.object({
    // 서버 설정
    server: z.object({
        name: z.string().default('rougeshool-mcp-server'),
        version: z.string().default('2.0.0'),
        port: z.number().default(3000),
        host: z.string().default('localhost')
    }),
    // 프로젝트 설정
    project: z.object({
        rootPath: z.string(),
        scriptPath: z.string(),
        maxFileSize: z.number().default(10 * 1024 * 1024), // 10MB
        supportedExtensions: z.array(z.string()).default(['.cs', '.js', '.ts', '.json'])
    }),
    // 로깅 설정
    logging: z.object({
        level: z.enum(['error', 'warn', 'info', 'debug']).default('info'),
        enableFileLogging: z.boolean().default(true),
        enableConsoleLogging: z.boolean().default(true),
        logDirectory: z.string().default('logs')
    }),
    // 성능 설정
    performance: z.object({
        enableMetrics: z.boolean().default(true),
        slowOperationThreshold: z.number().default(1000), // 1초
        maxConcurrentOperations: z.number().default(10)
    }),
    // 캐시 설정
    cache: z.object({
        enableCaching: z.boolean().default(true),
        ttl: z.number().default(300000), // 5분
        maxSize: z.number().default(1000)
    })
});
/**
 * 설정 관리 클래스
 * 싱글톤 패턴으로 전역 설정 관리
 */
export class ConfigManager {
    static instance;
    config;
    constructor() {
        const projectRoot = path.resolve(process.cwd());
        this.config = ConfigSchema.parse({
            server: {
                name: process.env.SERVER_NAME || 'rougeshool-mcp-server',
                version: process.env.SERVER_VERSION || '2.0.0',
                port: parseInt(process.env.PORT || '3000'),
                host: process.env.HOST || 'localhost'
            },
            project: {
                rootPath: projectRoot,
                scriptPath: path.join(projectRoot, 'Assets', 'Script'),
                maxFileSize: parseInt(process.env.MAX_FILE_SIZE || '10485760'),
                supportedExtensions: (process.env.SUPPORTED_EXTENSIONS || '.cs,.js,.ts,.json').split(',')
            },
            logging: {
                level: process.env.LOG_LEVEL || 'info',
                enableFileLogging: process.env.ENABLE_FILE_LOGGING !== 'false',
                enableConsoleLogging: process.env.ENABLE_CONSOLE_LOGGING !== 'false',
                logDirectory: process.env.LOG_DIRECTORY || 'logs'
            },
            performance: {
                enableMetrics: process.env.ENABLE_METRICS !== 'false',
                slowOperationThreshold: parseInt(process.env.SLOW_OPERATION_THRESHOLD || '1000'),
                maxConcurrentOperations: parseInt(process.env.MAX_CONCURRENT_OPERATIONS || '10')
            },
            cache: {
                enableCaching: process.env.ENABLE_CACHING !== 'false',
                ttl: parseInt(process.env.CACHE_TTL || '300000'),
                maxSize: parseInt(process.env.CACHE_MAX_SIZE || '1000')
            }
        });
    }
    static getInstance() {
        if (!ConfigManager.instance) {
            ConfigManager.instance = new ConfigManager();
        }
        return ConfigManager.instance;
    }
    getConfig() {
        return this.config;
    }
    get(key) {
        return this.config[key];
    }
    updateConfig(updates) {
        this.config = { ...this.config, ...updates };
    }
}
export const config = ConfigManager.getInstance();
//# sourceMappingURL=config.js.map