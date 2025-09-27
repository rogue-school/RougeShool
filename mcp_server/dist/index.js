#!/usr/bin/env node
import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { CallToolRequestSchema, ListToolsRequestSchema, } from '@modelcontextprotocol/sdk/types.js';
// 유틸리티 및 설정 임포트
import { logger } from './utils/logger.js';
import { config } from './config/config.js';
import { performanceMonitor } from './utils/performance.js';
import { cacheManager } from './utils/cache.js';
import { ErrorHandler, MCPServerError } from './utils/errors.js';
// 서비스 임포트
import { CodebaseAnalyzer } from './services/codebaseAnalyzer.js';
import { FileAnalyzer } from './services/fileAnalyzer.js';
import { CodeReuseService } from './services/codeReuseService.js';
import { ConflictPredictor } from './services/conflictPredictor.js';
/**
 * 전문적인 RougeShool MCP 서버
 * 엔터프라이즈급 아키텍처와 기능 제공
 */
export class ProfessionalRougeShoolMCPServer {
    server;
    codebaseAnalyzer;
    fileAnalyzer;
    codeReuseService;
    conflictPredictor;
    constructor() {
        this.initializeServer();
        this.initializeServices();
        this.setupHandlers();
        this.setupErrorHandling();
    }
    /**
     * 서버 초기화
     */
    initializeServer() {
        const serverConfig = config.get('server');
        this.server = new Server({
            name: serverConfig.name,
            version: serverConfig.version,
        }, {
            capabilities: {
                tools: {},
            },
        });
        logger.info('MCP 서버 초기화 완료', {
            name: serverConfig.name,
            version: serverConfig.version
        });
    }
    /**
     * 서비스 초기화
     */
    initializeServices() {
        this.codebaseAnalyzer = new CodebaseAnalyzer();
        this.fileAnalyzer = new FileAnalyzer();
        this.codeReuseService = new CodeReuseService();
        this.conflictPredictor = new ConflictPredictor();
        logger.info('서비스 초기화 완료');
    }
    /**
     * 핸들러 설정
     */
    setupHandlers() {
        // 도구 목록 제공
        this.server.setRequestHandler(ListToolsRequestSchema, async () => {
            return {
                tools: [
                    {
                        name: 'get_codebase_state',
                        description: '현재 코드베이스의 전체 상태를 분석하고 반환합니다.',
                        inputSchema: {
                            type: 'object',
                            properties: {
                                random_string: {
                                    type: 'string',
                                    description: 'Dummy parameter for no-parameter tools'
                                }
                            },
                            required: ['random_string']
                        }
                    },
                    {
                        name: 'analyze_file',
                        description: '특정 파일의 의존성, 패턴, 인터페이스를 분석합니다.',
                        inputSchema: {
                            type: 'object',
                            properties: {
                                filePath: {
                                    type: 'string',
                                    description: '분석할 파일의 경로'
                                }
                            },
                            required: ['filePath']
                        }
                    },
                    {
                        name: 'suggest_code_reuse',
                        description: '요구사항에 맞는 기존 코드 재사용 방안을 제안합니다.',
                        inputSchema: {
                            type: 'object',
                            properties: {
                                requirement: {
                                    type: 'string',
                                    description: '구현하려는 기능의 요구사항'
                                },
                                targetSystem: {
                                    type: 'string',
                                    enum: ['CoreSystem', 'CombatSystem', 'CharacterSystem', 'SkillCardSystem', 'SaveSystem', 'StageSystem', 'UISystem', 'UtilitySystem'],
                                    description: '대상 시스템'
                                }
                            },
                            required: ['requirement']
                        }
                    },
                    {
                        name: 'predict_conflicts',
                        description: '새로운 코드가 기존 코드와 충돌할 가능성을 예측합니다.',
                        inputSchema: {
                            type: 'object',
                            properties: {
                                newCode: {
                                    type: 'string',
                                    description: '새로 추가할 코드'
                                },
                                targetFile: {
                                    type: 'string',
                                    description: '대상 파일 경로'
                                }
                            },
                            required: ['newCode', 'targetFile']
                        }
                    }
                ]
            };
        });
        // 도구 호출 처리
        this.server.setRequestHandler(CallToolRequestSchema, async (request) => {
            const operationId = `tool_${request.params.name}_${Date.now()}`;
            try {
                performanceMonitor.startOperation(operationId);
                const result = await this.handleToolCall(request.params.name, request.params.arguments);
                performanceMonitor.endOperation(operationId);
                return {
                    content: [
                        {
                            type: 'text',
                            text: JSON.stringify(result, null, 2)
                        }
                    ]
                };
            }
            catch (error) {
                performanceMonitor.endOperation(operationId);
                ErrorHandler.logError(error, { tool: request.params.name });
                const normalizedError = ErrorHandler.normalizeError(error);
                return {
                    content: [
                        {
                            type: 'text',
                            text: JSON.stringify({
                                error: true,
                                message: normalizedError.message,
                                code: normalizedError.code
                            }, null, 2)
                        }
                    ],
                    isError: true
                };
            }
        });
    }
    /**
     * 도구 호출 처리
     */
    async handleToolCall(toolName, args) {
        switch (toolName) {
            case 'get_codebase_state':
                return await this.codebaseAnalyzer.getCodebaseState();
            case 'analyze_file':
                if (!args.filePath) {
                    throw new MCPServerError('filePath 매개변수가 필요합니다', 'MISSING_PARAMETER', 400);
                }
                return await this.fileAnalyzer.analyzeFile(args.filePath);
            case 'suggest_code_reuse':
                if (!args.requirement) {
                    throw new MCPServerError('requirement 매개변수가 필요합니다', 'MISSING_PARAMETER', 400);
                }
                return await this.codeReuseService.suggestCodeReuse(args.requirement, args.targetSystem);
            case 'predict_conflicts':
                if (!args.newCode || !args.targetFile) {
                    throw new MCPServerError('newCode와 targetFile 매개변수가 필요합니다', 'MISSING_PARAMETER', 400);
                }
                return await this.conflictPredictor.predictConflicts(args.newCode, args.targetFile);
            default:
                throw new MCPServerError(`알 수 없는 도구: ${toolName}`, 'UNKNOWN_TOOL', 400);
        }
    }
    /**
     * 에러 처리 설정
     */
    setupErrorHandling() {
        process.on('uncaughtException', (error) => {
            logger.error('처리되지 않은 예외 발생', error);
            ErrorHandler.logError(error);
            process.exit(1);
        });
        process.on('unhandledRejection', (reason, promise) => {
            logger.error('처리되지 않은 Promise 거부', reason);
            ErrorHandler.logError(reason);
        });
        process.on('SIGINT', () => {
            logger.info('서버 종료 중...');
            this.shutdown();
        });
        process.on('SIGTERM', () => {
            logger.info('서버 종료 중...');
            this.shutdown();
        });
    }
    /**
     * 서버 시작
     */
    async start() {
        try {
            const transport = new StdioServerTransport();
            await this.server.connect(transport);
            logger.info('MCP 서버 시작 완료');
            // 주기적 캐시 정리
            setInterval(() => {
                cacheManager.cleanup();
            }, 60000); // 1분마다
        }
        catch (error) {
            logger.error('서버 시작 실패', error);
            throw error;
        }
    }
    /**
     * 서버 종료
     */
    async shutdown() {
        try {
            logger.info('서버 종료 중...');
            // 성능 리포트 출력
            const report = performanceMonitor.generateReport();
            logger.info('최종 성능 리포트:\n' + report);
            // 캐시 통계 출력
            const cacheStats = cacheManager.getStats();
            logger.info('캐시 통계:', cacheStats);
            process.exit(0);
        }
        catch (error) {
            logger.error('서버 종료 중 오류 발생', error);
            process.exit(1);
        }
    }
}
// 서버 시작
if (import.meta.url === `file://${process.argv[1]}`) {
    const server = new ProfessionalRougeShoolMCPServer();
    server.start().catch((error) => {
        logger.error('서버 시작 실패', error);
        process.exit(1);
    });
}
//# sourceMappingURL=index.js.map