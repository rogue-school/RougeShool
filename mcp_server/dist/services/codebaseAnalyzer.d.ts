/**
 * 코드베이스 분석 서비스
 * 전체 프로젝트 구조와 의존성을 분석
 */
export declare class CodebaseAnalyzer {
    private projectConfig;
    /**
     * 코드베이스 전체 상태 분석
     */
    getCodebaseState(): Promise<any>;
    /**
     * 파일 구조 분석
     */
    private analyzeFileStructure;
    /**
     * 시스템 아키텍처 분석
     */
    private analyzeSystemArchitecture;
    /**
     * 개별 시스템 분석
     */
    private analyzeSystem;
    /**
     * 의존성 분석
     */
    private analyzeDependencies;
    /**
     * 내부 의존성 찾기
     */
    private findInternalDependencies;
    /**
     * 패턴 분석
     */
    private analyzePatterns;
    /**
     * 통계 생성
     */
    private generateStatistics;
    /**
     * 핵심 파일인지 확인
     */
    private isCoreFile;
}
//# sourceMappingURL=codebaseAnalyzer.d.ts.map