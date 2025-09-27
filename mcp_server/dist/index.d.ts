#!/usr/bin/env node
/**
 * 전문적인 RougeShool MCP 서버
 * 엔터프라이즈급 아키텍처와 기능 제공
 */
export declare class ProfessionalRougeShoolMCPServer {
    private server;
    private codebaseAnalyzer;
    private fileAnalyzer;
    private codeReuseService;
    private conflictPredictor;
    constructor();
    /**
     * 서버 초기화
     */
    private initializeServer;
    /**
     * 서비스 초기화
     */
    private initializeServices;
    /**
     * 핸들러 설정
     */
    private setupHandlers;
    /**
     * 도구 호출 처리
     */
    private handleToolCall;
    /**
     * 에러 처리 설정
     */
    private setupErrorHandling;
    /**
     * 서버 시작
     */
    start(): Promise<void>;
    /**
     * 서버 종료
     */
    private shutdown;
}
//# sourceMappingURL=index.d.ts.map